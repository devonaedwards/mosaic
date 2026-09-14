// KILL ZONE - a real-time strategy video game.
// Shooting, ramming, and the one place the game rolls dice.
//
// Ground and direct fire are deterministic: a shot that connects does exactly
// the damage the table says, because a strategy player needs to be able to count
// how many drones a tank takes and be right every time. Chance is confined to
// two places where uncertainty is the point - whether an interceptor catches its
// target, and what an autonomous munition decides to hit.

namespace KZ.Sim
{
    public static class CombatSystem
    {
        public static void Step(World w)
        {
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (!w.Entities.Has(i, ComponentMask.Weapon)) continue;
                StepOne(w, i);
            }
        }

        static void StepOne(World w, int i)
        {
            WeaponState weapon = w.Entities.Weapon[i];
            if (w.Tick < weapon.NextFireTick) return;

            // Reloading. A mount that has just emptied itself is, for the next few
            // seconds, scenery - which is the point of making it spend its magazine.
            if (weapon.ReloadingUntilTick > 0)
            {
                if (w.Tick < weapon.ReloadingUntilTick) return;
                weapon.ReloadingUntilTick = 0;
                weapon.EngagementsRemaining = weapon.EngagementsPerBelt;
                w.Entities.Weapon[i] = weapon;
            }

            EntityHandle target = FindTarget(w, i, weapon);
            if (!w.Entities.IsAlive(target)) return;

            Fix2 myPos = w.Entities.Position[i];
            Fix2 targetPos = w.Entities.Position[target.Index];
            Fix range = EffectiveReach(w, i, target, weapon);
            if (Fix2.SqrDistance(myPos, targetPos) > range * range) return;

            // Acquisition takes time, and a degraded link doubles it. That delay is
            // most of what an amber pip actually costs you in a fight.
            //
            // Switching to a new target also costs whatever it takes to physically
            // point the mount at it. That is why a mount with one barrel cannot
            // simply service every target in its envelope in turn, and why an
            // attack arriving from two altitudes at once is worse for it than the
            // same number of aircraft arriving from one.
            if (weapon.Acquiring != target)
            {
                Fix acqMul = w.Entities.Has(i, ComponentMask.Mover)
                    ? w.Entities.Mover[i].AcquisitionMultiplier
                    : Fix.One;
                int acqTicks = (Fix.FromInt(weapon.AcquisitionTicks) * acqMul).RoundToInt();
                acqTicks += SlewTicks(w, i, ref weapon, target);

                weapon.Acquiring = target;
                weapon.AcquiringUntilTick = w.Tick + acqTicks;
                w.Entities.Weapon[i] = weapon;
                return;
            }
            if (w.Tick < weapon.AcquiringUntilTick) return;

            if (weapon.IsInterceptor) ResolveInterception(w, i, target, ref weapon);
            else ResolveDirectFire(w, i, target, ref weapon);

            // A miss costs a round exactly as a hit does, which is most of why a
            // defence can be made to run itself dry.
            if (weapon.EngagementsPerBelt > 0)
            {
                weapon.EngagementsRemaining--;
                if (weapon.EngagementsRemaining <= 0)
                {
                    weapon.ReloadingUntilTick = w.Tick + weapon.ReloadTicks;
                    w.Events.Push(SimEventKind.WeaponReloading, w.Tick, w.Entities.HandleAt(i));
                }
            }

            weapon.NextFireTick = w.Tick + weapon.CooldownTicks;
            weapon.Acquiring = EntityHandle.None;
            w.Entities.Weapon[i] = weapon;
        }

        /// <summary>
        /// The chance a gun connects with something in the air.
        ///
        /// A mount's stated reach is the distance its rounds carry, not the
        /// distance at which it reliably hits a two-kilogram object crossing its
        /// front. Accuracy falls off sharply with range, falls further against
        /// something small, and falls further still against something fast - so the
        /// envelope inside which a turret is genuinely dangerous is much smaller
        /// than the circle drawn on the map, and a fast target can cross the outer
        /// part of that circle almost with impunity.
        ///
        /// This is also the honest answer to why a turbojet strike drone is such a
        /// problem: it is not that guns cannot reach it, it is that they cannot hit
        /// it.
        /// </summary>
        static Fix AirHitChance(World w, int attackerIndex, EntityHandle target, WeaponState weapon)
        {
            Fix distance = Fix2.Distance(w.Entities.Position[attackerIndex],
                                         w.Entities.Position[target.Index]);
            Fix reach = weapon.RangeMetres;
            if (reach.Raw <= 0) return Fix.One;

            Fix fraction = Fix.Clamp(distance / reach, Fix.Zero, Fix.One);
            Fix closeness = Fix.One - fraction;

            // What the mount is loaded with decides the *shape* of the falloff, not
            // just its height, and that is the whole of the ammunition decision.
            Fix rangeTerm, ceiling, aimForgiveness;
            switch (weapon.Ammo)
            {
                case AmmoType.Buckshot:
                    // Murderous inside a hundred metres and nothing beyond it.
                    rangeTerm = closeness * closeness * closeness * closeness;
                    ceiling = Fix.FromDoubleContentOnly(0.97);
                    aimForgiveness = Fix.FromDoubleContentOnly(0.85);
                    break;

                case AmmoType.Proximity:
                    // Does not need to connect, only to pass close.
                    rangeTerm = closeness;
                    ceiling = Fix.FromDoubleContentOnly(0.80);
                    aimForgiveness = Fix.FromDoubleContentOnly(0.45);
                    break;

                case AmmoType.Airburst:
                    // Fills a volume rather than threading a needle. The flattest
                    // curve of the four, and the reason a gun can trouble something
                    // fast at all.
                    rangeTerm = Fix.One - (fraction * Fix.FromDoubleContentOnly(0.55));
                    ceiling = Fix.FromDoubleContentOnly(0.72);
                    aimForgiveness = Fix.FromDoubleContentOnly(0.25);
                    break;

                default:
                    // A saturating falloff with a thin tail rather than a parabola
                    // that reaches exactly zero at the stated range. A burst at the
                    // edge of the envelope is a poor bet, not an impossible one,
                    // and modelling it as impossible produced a hard wall where
                    // reality has a gradient.
                    rangeTerm = closeness * closeness * Fix.FromDoubleContentOnly(0.94)
                              + Fix.FromDoubleContentOnly(0.06);
                    ceiling = Fix.FromDoubleContentOnly(0.92);
                    aimForgiveness = Fix.One;
                    break;
            }

            Fix chance = ceiling * rangeTerm;

            // Size. A heavy multirotor is a far easier thing to hit than a racing
            // quadcopter, and the visual signature is already the right measure of
            // how big a thing looks.
            int defId = w.Entities.DefId[target.Index];
            if (defId >= 0)
            {
                UnitDef def = Catalog.Get(defId);
                Fix size = Fix.FromInt(def.SigVisual < 10 ? 10 : def.SigVisual) / Fix.FromInt(45);
                chance = chance * Fix.Clamp(size, Fix.FromDoubleContentOnly(0.45),
                                                  Fix.FromDoubleContentOnly(1.30));

                // Speed. A target crossing at fifty metres a second gives a mount
                // very little time in which its solution is still good - unless the
                // round does not need a good solution, which is what the fuze is
                // for. Aim forgiveness pulls the speed penalty back toward one.
                // Speed, and how much it hurts depends on how far away it is. A
                // fast crosser at two hundred metres is a modest problem; the same
                // target at a kilometre is a much worse one, because the time of
                // flight during which the solution decays is longer. A flat speed
                // penalty gets that backwards at one end or the other.
                Fix speed = def.SpeedMetresPerSecond;
                if (speed.Raw > 0)
                {
                    Fix speedTerm = Fix.FromInt(20) / speed;
                    speedTerm = Fix.Clamp(speedTerm, Fix.FromDoubleContentOnly(0.20),
                                                     Fix.FromDoubleContentOnly(1.15));

                    // Scale the penalty up with fractional range: mild up close,
                    // severe at the rim.
                    Fix severity = Fix.FromDoubleContentOnly(0.55)
                                 + fraction * Fix.FromDoubleContentOnly(1.45);
                    speedTerm = Fix.One - (Fix.One - speedTerm) * severity;

                    speedTerm = Fix.One - (Fix.One - speedTerm) * aimForgiveness;
                    chance = chance * Fix.Clamp(speedTerm, Fix.FromDoubleContentOnly(0.05), Fix.One);
                }
            }

            // Shooting upward is harder again.
            if (w.Entities.EntityLayer[target.Index] == Layer.High)
                chance = chance * Fix.FromDoubleContentOnly(0.70);

            return Fix.Clamp(chance, Fix.FromDoubleContentOnly(0.02), Fix.FromDoubleContentOnly(0.95));
        }

        /// <summary>
        /// How long it takes to swing the mount onto a new target, and to change
        /// elevation band if the new one is at a different height.
        /// </summary>
        static int SlewTicks(World w, int i, ref WeaponState weapon, EntityHandle target)
        {
            if (weapon.TraverseBamPerTick <= 0) return 0;

            Fix2 delta = w.Entities.Position[target.Index] - w.Entities.Position[i];
            ushort wanted = Trig.Atan2(delta.Y, delta.X);
            int swing = Trig.Delta(weapon.Bearing, wanted);
            if (swing < 0) swing = -swing;

            int ticks = swing / weapon.TraverseBamPerTick;

            // Re-laying from one height band to another costs more than traversing
            // within one, because the mount has to find the target again as well as
            // point at it.
            Layer targetLayer = w.Entities.EntityLayer[target.Index];
            if (targetLayer != weapon.TrackingLayer) ticks += 20;

            weapon.Bearing = wanted;
            weapon.TrackingLayer = targetLayer;
            return ticks;
        }

        /// <summary>
        /// How far a weapon actually reaches against this target. Shooting upward
        /// costs range for anything that is not a purpose-built interceptor: a
        /// ground mount firing at something a kilometre up is at the edge of what
        /// its rounds will do.
        /// </summary>
        static Fix EffectiveReach(World w, int attackerIndex, EntityHandle target, WeaponState weapon)
        {
            if (w.Entities.EntityLayer[target.Index] != Layer.High) return weapon.RangeMetres;

            // A hard ceiling, not a penalty. A machine gun does not engage
            // something at two and a half kilometres badly; it does not engage it.
            // Moving the cruise altitude up is how one-way attack drones walked
            // away from gun defence, and it is a cleaner mechanic than a multiplier.
            if (!weapon.CanReachHigh) return Fix.Zero;
            if (weapon.IsInterceptor) return weapon.RangeMetres;
            return weapon.RangeMetres * Fix.FromDoubleContentOnly(0.60);
        }

        /// <summary>
        /// Whether this attacker is allowed to engage that target at all. The rule
        /// that matters: only interceptors, interceptor batteries and gun mounts
        /// can touch anything airborne. A tank cannot shoot a drone down with its
        /// main gun, however well it would go if it could - the entire premise of
        /// the setting is that it cannot.
        /// </summary>
        static bool CanEngage(World w, int attackerIndex, EntityHandle target)
        {
            if (!w.Entities.IsAlive(target)) return false;

            // Nothing is shootable until somebody has eyes on it. This is the rule
            // that makes reconnaissance the gate on every shot fired, and the rule
            // that gives night its meaning: an optical sensor loses most of its
            // reach after dark, so a gun that dominates an approach in daylight can
            // only see a few hundred metres of it at night.
            if (!w.IsDetectedBy(w.Entities.Team[attackerIndex], target)) return false;

            if (w.Entities.EntityLayer[target.Index] == Layer.Ground) return true;

            int defId = w.Entities.DefId[attackerIndex];
            if (defId < 0) return false;
            return Catalog.Get(defId).CanEngageAir;
        }

        static EntityHandle FindTarget(World w, int i, WeaponState weapon)
        {
            // An explicit order wins. An autonomous munition with no order asks its
            // classifier instead, and that is where decoys get their chance.
            if (w.Entities.Has(i, ComponentMask.Mover))
            {
                EntityHandle ordered = w.Entities.Mover[i].OrderTarget;
                if (w.Entities.IsAlive(ordered) && CanEngage(w, i, ordered)) return ordered;
            }

            bool autonomous = w.Entities.Has(i, ComponentMask.Link)
                              && w.Entities.Link[i].Kind == LinkKind.Autonomy;
            if (autonomous)
            {
                bool misidentified;
                return AutonomyClassifier.SelectTarget(w, i, out misidentified);
            }

            return BestTargetInRange(w, i, weapon);
        }

        /// <summary>
        /// Pick what to shoot at, by what the shot is actually worth rather than by
        /// what is closest.
        ///
        /// Nearest-target selection looks reasonable and behaves stupidly. A gun
        /// mount with a relay mast parked beside it will spend an entire engagement
        /// chipping twenty-one damage a time off a six-hundred hit-point structure
        /// while the drones that are about to destroy it fly past unengaged - which
        /// is exactly what it did the first time this was run.
        ///
        /// So each candidate is scored by the fraction of its remaining health one
        /// shot removes. A shot that kills outright scores full marks; a shot that
        /// scratches paint scores almost nothing. Distance only breaks ties. The
        /// result is a weapon that shoots at whatever it can actually hurt, which
        /// is what a human would do and what the player will expect.
        /// </summary>
        static EntityHandle BestTargetInRange(World w, int i, WeaponState weapon)
        {
            byte team = w.Entities.Team[i];
            Fix2 pos = w.Entities.Position[i];
            Fix rangeSq = weapon.RangeMetres * weapon.RangeMetres;

            EntityHandle best = EntityHandle.None;
            Fix bestScore = Fix.Zero;
            Fix bestDistSq = Fix.MaxValue;

            for (int j = 1; j < w.Entities.HighWater; j++)
            {
                if (!w.Entities.IsSlotAlive(j)) continue;
                if (w.Entities.Team[j] == team || w.Entities.Team[j] == 0) continue;
                if (!w.Entities.Has(j, ComponentMask.Health)) continue;
                if (w.Entities.Has(j, ComponentMask.Decoy)) continue;

                // An interceptor only engages things in the air, and almost
                // nothing else may engage air at all.
                if (weapon.IsInterceptor && w.Entities.EntityLayer[j] == Layer.Ground) continue;
                if (!CanEngage(w, i, w.Entities.HandleAt(j))) continue;

                Fix dSq = Fix2.SqrDistance(pos, w.Entities.Position[j]);
                if (dSq > rangeSq) continue;

                Fix score = ShotValue(w, i, j, weapon);
                if (score.Raw <= 0) continue;

                if (score > bestScore || (score == bestScore && dSq < bestDistSq))
                {
                    bestScore = score;
                    bestDistSq = dSq;
                    best = w.Entities.HandleAt(j);
                }
            }
            return best;
        }

        /// <summary>
        /// What fraction of a target one shot removes, capped at one - overkill
        /// buys nothing, so a weapon does not prefer a nearly-dead target over a
        /// live threat it can also kill.
        /// </summary>
        static Fix ShotValue(World w, int attackerIndex, int targetIndex, WeaponState weapon)
        {
            // A ram either kills what it hits or does nothing at all.
            if (weapon.Type == DamageType.Ram)
            {
                ArmourClass a = w.Entities.Armour[targetIndex];
                return (a == ArmourClass.AirRotary || a == ArmourClass.AirFixed) ? Fix.One : Fix.Zero;
            }

            bool topAttack = w.Entities.EntityLayer[attackerIndex] == Layer.Low
                             && w.Entities.EntityLayer[targetIndex] == Layer.Ground;

            Fix mult = Catalog.DamageMultiplier(weapon.Type, w.Entities.Armour[targetIndex], topAttack);
            if (mult.Raw <= 0) return Fix.Zero;

            Fix perShot = weapon.Damage * mult;
            Fix remaining = w.Entities.Hp[targetIndex] + w.Entities.CageHp[targetIndex];
            if (remaining.Raw <= 0) return Fix.Zero;

            Fix fraction = perShot / remaining;
            return fraction > Fix.One ? Fix.One : fraction;
        }

        static void ResolveDirectFire(World w, int i, EntityHandle target, ref WeaponState weapon)
        {
            // A rotary drone diving on a vehicle hits the thin plate on top. It is
            // the reason a two-hundred-Materiel drone is a real threat to a tank.
            bool topAttack = w.Entities.EntityLayer[i] == Layer.Low
                             && w.Entities.EntityLayer[target.Index] == Layer.Ground;

            // Shooting at something in the air is a different proposition from
            // shooting at something on the ground, and the simulation treats it
            // that way: ground fire connects, air defence rolls.
            if (w.Entities.EntityLayer[target.Index] != Layer.Ground)
            {
                Fix chance = AirHitChance(w, i, target, weapon);
                if (!w.Random.Get(RandomStream.Interception).Chance(chance))
                {
                    // A miss still costs the mount its cycle, which is the whole
                    // reason a defended approach can be saturated at all.
                    return;
                }
            }

            w.ApplyDamage(target, weapon.Damage, weapon.Type, topAttack, w.Entities.HandleAt(i));

            // A one-way airframe is the munition. It does not come home.
            if (w.Entities.Has(i, ComponentMask.Sortie) && w.Entities.Sortie[i].OneWay)
                w.Kill(w.Entities.HandleAt(i), EntityHandle.None);
        }

        /// <summary>
        /// Whether an interceptor connects. The dominant term is not speed or skill
        /// but whether anybody told it where to look: a radar-cued intercept is
        /// three times as likely to land as an uncued one. That is why the radar
        /// mast is the real air-defence building, and why killing it is the opening
        /// move of every deep strike.
        /// </summary>
        static void ResolveInterception(World w, int i, EntityHandle target, ref WeaponState weapon)
        {
            Fix chance = weapon.InterceptBaseChance;
            chance = chance * CueMultiplier(w, i, target);
            chance = chance * SpeedRatio(w, i, target);
            chance = chance * VeterancyMultiplier(w, i);
            chance = Fix.Clamp(chance, Fix.Zero, Fix.One);

            DetRandom rng = w.Random.Get(RandomStream.Interception);
            bool hit = rng.Chance(chance);

            EntityHandle me = w.Entities.HandleAt(i);

            if (hit)
            {
                if (weapon.Type == DamageType.Ram) w.Kill(target, me);
                else w.ApplyDamage(target, weapon.Damage, weapon.Type, false, me);
            }

            // A ramming interceptor is expended either way; it flew into the thing.
            if (weapon.Type == DamageType.Ram) w.Kill(me, EntityHandle.None);
        }

        static Fix CueMultiplier(World w, int i, EntityHandle target)
        {
            byte team = w.Entities.Team[i];
            Fix2 targetPos = w.Entities.Position[target.Index];

            for (int j = 1; j < w.Entities.HighWater; j++)
            {
                if (!w.Entities.IsSlotAlive(j)) continue;
                if (w.Entities.Team[j] != team) continue;
                if (!w.Entities.Has(j, ComponentMask.Sensor)) continue;
                if (w.Entities.Sensor[j].Radar.Raw <= 0) continue;

                Fix r = w.DetectionRangeFor(j, target.Index, SensorChannel.Radar);
                if (r.Raw > 0 && Fix2.SqrDistance(w.Entities.Position[j], targetPos) <= r * r)
                    return Fix.One;  // radar-cued
            }

            for (int j = 1; j < w.Entities.HighWater; j++)
            {
                if (!w.Entities.IsSlotAlive(j)) continue;
                if (w.Entities.Team[j] != team) continue;
                if (!w.Entities.Has(j, ComponentMask.Sensor)) continue;

                Fix r = w.BestDetectionRange(j, target.Index);
                if (r.Raw > 0 && Fix2.SqrDistance(w.Entities.Position[j], targetPos) <= r * r)
                    return Fix.FromDoubleContentOnly(0.60);  // somebody has eyes on it
            }

            return Fix.FromDoubleContentOnly(0.30);  // firing blind
        }

        static Fix SpeedRatio(World w, int i, EntityHandle target)
        {
            Fix mySpeed = w.Entities.Has(i, ComponentMask.Mover)
                ? w.Entities.Mover[i].SpeedMetresPerSecond : Fix.FromInt(20);

            int ti = target.Index;
            Fix targetSpeed = w.Entities.Has(ti, ComponentMask.Mover)
                ? w.Entities.Mover[ti].SpeedMetresPerSecond : Fix.FromInt(1);

            Fix denom = targetSpeed * Fix.FromDoubleContentOnly(1.6);
            if (denom.Raw <= 0) return Fix.FromDoubleContentOnly(1.15);

            Fix ratio = mySpeed / denom;
            return Fix.Clamp(ratio, Fix.FromDoubleContentOnly(0.45), Fix.FromDoubleContentOnly(1.15));
        }

        static Fix VeterancyMultiplier(World w, int i)
        {
            if (!w.Entities.Has(i, ComponentMask.Sortie)) return Fix.One;
            int crewId = w.Entities.Sortie[i].CrewId;
            byte team = w.Entities.Team[i];
            if (crewId < 0 || team >= w.Players.Length) return Fix.One;

            switch (w.Players[team].Crews.Get(crewId).Rank)
            {
                case 2: return Fix.FromDoubleContentOnly(1.05);
                case 3: return Fix.FromDoubleContentOnly(1.12);
                case 4: return Fix.FromDoubleContentOnly(1.20);
                default: return Fix.One;
            }
        }
    }
}
