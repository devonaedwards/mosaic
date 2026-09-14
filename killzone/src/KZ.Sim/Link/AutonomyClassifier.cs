// KILL ZONE - a real-time strategy video game.
// What an autonomous munition decides to hit, and how a pile of cheap decoys
// makes it decide wrong.
//
// This closes the loop that the rest of the game opens. Jamming beats radio.
// Fiber beats jamming but is leashed. Autonomy beats jamming with no leash at
// all - because there is no link to cut, there is nothing to jam. So what beats
// autonomy?
//
// Not more electronic warfare. A machine picking its own target from a camera is
// beaten by giving the camera something convincing to look at. A hundred-and-
// fifty-Materiel inflatable with a heat source is a better answer to a
// six-hundred-Materiel autonomous munition than any jammer, and the more your
// opponent leans on machines to escape their crew limit, the more those
// inflatables cost them.
//
// One rule keeps this honest and is enforced here rather than left to tuning:
// deception fools machines, never people. An attack flown by a human with a live
// picture ignores decoys entirely.

namespace KZ.Sim
{
    public struct ClassifierCandidate
    {
        public EntityHandle Handle;
        public TargetKind Kind;
        public Fix Plausibility;
    }

    public static class AutonomyClassifier
    {
        /// <summary>
        /// Choose what an autonomous munition attacks. Returns EntityHandle.None if
        /// it finds nothing worth diving on.
        /// </summary>
        public static EntityHandle SelectTarget(World w, int munitionIndex, out bool misidentified)
        {
            misidentified = false;

            byte team = w.Entities.Team[munitionIndex];
            Fix2 pos = w.Entities.Position[munitionIndex];
            Fix cone = Fix.FromInt(SimConstants.AutonomySeekerConeMetres);
            Fix coneSq = cone * cone;

            ClassifierCandidate[] candidates = new ClassifierCandidate[SimConstants.AutonomyMaxCandidates];
            int n = 0;
            int decoysInCone = 0;

            for (int i = 1; i < w.Entities.HighWater && n < SimConstants.AutonomyMaxCandidates; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (i == munitionIndex) continue;
                if (!w.Entities.Has(i, ComponentMask.Transform)) continue;
                if (w.Entities.Has(i, ComponentMask.Salvage)) continue;
                if (Fix2.SqrDistance(pos, w.Entities.Position[i]) > coneSq) continue;

                TargetKind kind = ClassifyCandidate(w, i, team);
                if (kind == TargetKind.Decoy) decoysInCone++;

                candidates[n].Handle = w.Entities.HandleAt(i);
                candidates[n].Kind = kind;
                candidates[n].Plausibility = PlausibilityOf(w, i, kind);
                n++;
            }

            if (n == 0) return EntityHandle.None;

            int quality = EffectiveQuality(w, munitionIndex, decoysInCone);

            // The roll: does the machine see through the clutter this time?
            DetRandom rng = w.Random.Get(RandomStream.AutonomyClassify);
            bool correct = rng.NextInt(100) < quality;

            EntityHandle chosen;
            if (correct)
            {
                chosen = PickMostPlausibleGenuine(candidates, n);
                if (chosen.IsNone) chosen = PickWeighted(candidates, n, rng);
            }
            else
            {
                chosen = PickWeighted(candidates, n, rng);
            }

            if (!chosen.IsNone)
            {
                TargetKind k = KindOf(candidates, n, chosen);
                if (k == TargetKind.Decoy || k == TargetKind.Friendly || k == TargetKind.Neutral)
                {
                    misidentified = true;
                    // Never swallowed quietly. A machine that kills one of your own
                    // vehicles says so, loudly, with its own marker and its own
                    // sound, because that is the cost the player accepted when they
                    // stopped putting a human in the loop.
                    w.Events.Push(SimEventKind.AutonomyMisidentified, w.Tick,
                                  w.Entities.HandleAt(munitionIndex), chosen, team, (int)k);
                }
            }
            return chosen;
        }

        /// <summary>
        /// Confidence, after the world has had its say. Each decoy in the cone
        /// costs five points, so a dense screen does not merely add one wrong
        /// answer to the list - it degrades the machine's judgement outright.
        /// </summary>
        public static int EffectiveQuality(World w, int munitionIndex, int decoysInCone)
        {
            int q = w.Entities.Has(munitionIndex, ComponentMask.Autonomy)
                ? w.Entities.Autonomy[munitionIndex].Quality
                : 55;

            q -= decoysInCone * SimConstants.AutonomyDecoyQualityPenalty;

            if (w.IsNight)
            {
                bool thermal = w.Entities.Has(munitionIndex, ComponentMask.Sensor)
                               && w.Entities.Sensor[munitionIndex].Thermal;
                byte team = w.Entities.Team[munitionIndex];
                if (!thermal && !(team < w.Players.Length && w.Players[team].HasThermalOptics))
                    q -= SimConstants.AutonomyNightNoThermalPenalty;
            }

            if (q < 0) q = 0;
            if (q > 100) q = 100;
            return q;
        }

        static TargetKind ClassifyCandidate(World w, int i, byte seekerTeam)
        {
            if (w.Entities.Has(i, ComponentMask.Decoy)) return TargetKind.Decoy;

            byte team = w.Entities.Team[i];
            if (team == seekerTeam) return TargetKind.Friendly;
            if (team == 0) return TargetKind.Neutral;

            int defId = w.Entities.DefId[i];
            if (defId < 0) return TargetKind.Neutral;

            UnitDef def = Catalog.Get(defId);
            bool highValue = def.Armour == ArmourClass.Heavy
                             || def.JamStrength > 0
                             || def.SensorFootprintMetres > Fix.FromInt(500)
                             || def.IsInterceptor
                             || (def.IsStructure && def.CostMateriel >= 700);
            return highValue ? TargetKind.HighValue : TargetKind.LowValue;
        }

        static Fix PlausibilityOf(World w, int i, TargetKind kind)
        {
            Fix p = kind == TargetKind.Decoy && w.Entities.Has(i, ComponentMask.Decoy)
                ? w.Entities.Decoy[i].Plausibility
                : Catalog.BasePlausibility(kind);

            // A thermal blanket does not hide a vehicle from a person. It makes it
            // a less convincing answer to a machine, which is a different and
            // cheaper kind of protection.
            if (w.Entities.Has(i, ComponentMask.Sensor) && w.Entities.Sensor[i].Thermal)
            {
                Fix reduction = w.IsNight
                    ? Fix.FromDoubleContentOnly(0.40)
                    : Fix.FromDoubleContentOnly(0.20);
                p -= reduction;
            }

            return p.Raw < 0 ? Fix.Zero : p;
        }

        static EntityHandle PickMostPlausibleGenuine(ClassifierCandidate[] c, int n)
        {
            EntityHandle best = EntityHandle.None;
            Fix bestP = Fix.Zero;
            for (int i = 0; i < n; i++)
            {
                if (c[i].Kind == TargetKind.Decoy || c[i].Kind == TargetKind.Friendly
                    || c[i].Kind == TargetKind.Neutral) continue;
                if (best.IsNone || c[i].Plausibility > bestP)
                {
                    best = c[i].Handle;
                    bestP = c[i].Plausibility;
                }
            }
            return best;
        }

        static EntityHandle PickWeighted(ClassifierCandidate[] c, int n, DetRandom rng)
        {
            Fix total = Fix.Zero;
            for (int i = 0; i < n; i++) total += c[i].Plausibility;
            if (total.Raw <= 0) return EntityHandle.None;

            Fix roll = new Fix(Fix.MulRaw(rng.NextFix().Raw, total.Raw));
            Fix acc = Fix.Zero;
            for (int i = 0; i < n; i++)
            {
                acc += c[i].Plausibility;
                if (roll <= acc) return c[i].Handle;
            }
            return c[n - 1].Handle;
        }

        static TargetKind KindOf(ClassifierCandidate[] c, int n, EntityHandle h)
        {
            for (int i = 0; i < n; i++) if (c[i].Handle == h) return c[i].Kind;
            return TargetKind.Neutral;
        }

        /// <summary>
        /// Whether this attack is being flown by a person who can see what they are
        /// attacking. If so the classifier is not consulted at all - the player's
        /// order is the answer, and every decoy on the field is just scenery.
        /// </summary>
        public static bool IsPilotedWithClearFeed(World w, int attackerIndex, Fix2 targetPos)
        {
            if (!w.Entities.Has(attackerIndex, ComponentMask.Sortie)) return false;
            if (w.Entities.Sortie[attackerIndex].CrewId < 0) return false;
            if (!w.Entities.Has(attackerIndex, ComponentMask.Link)) return false;
            if (w.Entities.Link[attackerIndex].Pip != LinkPip.Green) return false;

            Fix range = Fix.FromInt(SimConstants.PilotedDecoyImmunityRangeMetres);
            return Fix2.SqrDistance(w.Entities.Position[attackerIndex], targetPos) <= range * range;
        }
    }
}
