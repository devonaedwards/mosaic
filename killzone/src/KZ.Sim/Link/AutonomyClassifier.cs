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

            // Terminal guidance does not choose anything. A person already did,
            // while they could still see, and the machine is only flying the last
            // couple of seconds after the link died. So there is no classification
            // to get wrong and no decoy to fall for - the deception happened after
            // the decision, which is too late to matter.
            //
            // This is the distinction the game collapsed for a long time, and
            // collapsing it made every autonomous munition a crewless gamble when
            // the fielded reality is the opposite: the common tier keeps its crew
            // and is *more* accurate, not less.
            //
            // It is also the reason the no-go bubble below is a narrow tool and
            // must stay one. Everything returning here never reaches it, so the
            // bubble governs AutonomyTier.TargetSelection and nothing else - one
            // unit in the catalogue, which FINDINGS 30 records as the correct
            // rarity. It is not a no-fly zone, it does not stop a gun mount, and
            // it cannot call back a munition a human already aimed.
            if (w.Entities.Has(munitionIndex, ComponentMask.Autonomy)
                && w.Entities.Autonomy[munitionIndex].Tier != AutonomyTier.TargetSelection)
                return EntityHandle.None;

            byte team = w.Entities.Team[munitionIndex];
            Fix2 pos = w.Entities.Position[munitionIndex];
            Fix cone = Fix.FromInt(SimConstants.AutonomySeekerConeMetres);
            Fix coneSq = cone * cone;

            // The player's no-go bubble (AUDIT-UNWIRED.md F17, autonomy.md §9.3).
            // Until it existed the seeker cone was the only control anybody had
            // over what a machine may consider, and the cone is a fixed radius
            // around wherever the munition happens to be - which is to say, no
            // control at all. This is the other half of §1's definition of
            // autonomous selection: a class filter *and an area*.
            //
            // Read once, outside the loop, because the candidate loop is the
            // expensive part and this must cost two comparisons per candidate
            // and nothing else. An expired box is simply not consulted; nothing
            // sweeps it, because a lazy check is free and a sweep is a pass over
            // every entity every tick to delete a rectangle nobody is reading.
            bool hasBox = false;
            Fix2 boxMin = Fix2.Zero, boxMax = Fix2.Zero;
            if (w.Entities.Has(munitionIndex, ComponentMask.Autonomy))
            {
                AutonomyState au = w.Entities.Autonomy[munitionIndex];
                hasBox = au.HasBox && w.Tick < au.BoxExpiryTick;
                boxMin = au.BoxMin;
                boxMax = au.BoxMax;
            }

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

                // Inside the bubble, so the machine never learns it is there.
                // Deliberately ahead of the decoy count as well: a decoy the
                // player parked inside their own no-go area is not clutter this
                // seeker has to see through, because it is not offered to it.
                // And deliberately blind to whose it is - an enemy who drives
                // into your bubble is as safe as your own vehicles, which is the
                // price of the order and the reason to draw it tight.
                if (hasBox && InsideBox(w.Entities.Position[i], boxMin, boxMax)) continue;

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
                               && w.Entities.Sensor[munitionIndex].Thermal.Raw > 0;
                byte team = w.Entities.Team[munitionIndex];
                if (!thermal && !(team < w.Players.Length && w.Players[team].HasThermalOptics))
                    q -= SimConstants.AutonomyNightNoThermalPenalty;
            }

            if (q < 0) q = 0;
            if (q > 100) q = 100;
            return q;
        }

        /// <summary>
        /// Axis-aligned, inclusive of the edge: a vehicle sitting exactly on the
        /// line the player drew is inside it. The player drew the line around
        /// something they wanted spared, and the tie goes to them.
        /// </summary>
        static bool InsideBox(Fix2 p, Fix2 min, Fix2 max)
        {
            return p.X >= min.X && p.X <= max.X && p.Y >= min.Y && p.Y <= max.Y;
        }

        static TargetKind ClassifyCandidate(World w, int i, byte seekerTeam)
        {
            if (w.Entities.Has(i, ComponentMask.Decoy)) return TargetKind.Decoy;

            byte team = w.Entities.Team[i];
            if (team == seekerTeam) return TargetKind.Friendly;
            if (team == 0) return TargetKind.Neutral;

            int defId = w.Entities.DefId[i];
            if (defId < 0) return TargetKind.Neutral;

            // What a machine considers worth its warhead: armour, anything that
            // jams or radiates, anything that shoots back at aircraft, and any
            // structure that cost real money.
            UnitDef def = Catalog.Get(defId);
            bool highValue = def.Armour == ArmourClass.Heavy
                             || def.JamStrength > 0
                             || def.SensorRadar.Raw > 0
                             || def.SensorEsm.Raw > 0
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
            // cheaper kind of protection - and the same blanket is already cutting
            // what any heat sensor on the map has to work with.
            if (w.Entities.HasThermalBlanket[i])
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
        /// Whether a person is flying this airframe right now, on a picture that
        /// is actually arriving. Crew first, link second, and in that order on
        /// purpose: <see cref="LinkKind.Autonomy"/> is pinned Green by
        /// LinkResolver because there is nothing there to jam, which is not the
        /// same statement as "somebody is watching". Every deep-strike airframe
        /// in the catalogue is on that rung with no crew at all, so a pip-only
        /// test would hand the whole family an exemption it has not earned.
        ///
        /// Amber is deliberately not enough. A stuttering picture is the state
        /// the pilot is already half blind in, and the decoy rule below has
        /// always drawn the line at Green - two different definitions of "the
        /// pilot can see" would be worse than one imperfect one.
        ///
        /// This is the question World.ApplyDamage asks before displacing a
        /// munition's aimpoint, and it is why that gate is not about the
        /// autonomy tier: a plain radio FPV with no terminal guidance at all,
        /// flown into a vehicle on camera, does not need to know its own
        /// coordinates either. What defeats navigational error is the eye, not
        /// the tier.
        /// </summary>
        public static bool IsPilotedOnLiveFeed(World w, int attackerIndex)
        {
            if (!w.Entities.Has(attackerIndex, ComponentMask.Sortie)) return false;
            if (w.Entities.Sortie[attackerIndex].CrewId < 0) return false;
            if (!w.Entities.Has(attackerIndex, ComponentMask.Link)) return false;
            return w.Entities.Link[attackerIndex].Pip == LinkPip.Green;
        }

        /// <summary>
        /// Whether this attack is being flown by a person who can see what they are
        /// attacking <em>well enough to tell it from an inflatable</em>. If so the
        /// classifier is not consulted at all - the player's order is the answer,
        /// and every decoy on the field is just scenery.
        ///
        /// The range test is what separates this from IsPilotedOnLiveFeed above,
        /// and it belongs to this question alone: telling a real vehicle from a
        /// decoy is a resolution problem, sized by optics
        /// (SimConstants.PilotedDecoyImmunityRangeMetres), while steering a
        /// warhead into something already filling the frame is not. Sharing one
        /// constant between the two would mean a future decoy-balance change
        /// silently moved strike accuracy.
        /// </summary>
        public static bool IsPilotedWithClearFeed(World w, int attackerIndex, Fix2 targetPos)
        {
            if (!IsPilotedOnLiveFeed(w, attackerIndex)) return false;

            Fix range = Fix.FromInt(SimConstants.PilotedDecoyImmunityRangeMetres);
            return Fix2.SqrDistance(w.Entities.Position[attackerIndex], targetPos) <= range * range;
        }
    }
}
