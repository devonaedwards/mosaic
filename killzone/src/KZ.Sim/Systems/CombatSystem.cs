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

            EntityHandle target = FindTarget(w, i, weapon);
            if (!w.Entities.IsAlive(target)) return;

            Fix2 myPos = w.Entities.Position[i];
            Fix2 targetPos = w.Entities.Position[target.Index];
            Fix range = weapon.RangeMetres;
            if (Fix2.SqrDistance(myPos, targetPos) > range * range) return;

            // Acquisition takes time, and a degraded link doubles it. That delay is
            // most of what an amber pip actually costs you in a fight.
            if (weapon.Acquiring != target)
            {
                Fix acqMul = w.Entities.Has(i, ComponentMask.Mover)
                    ? w.Entities.Mover[i].AcquisitionMultiplier
                    : Fix.One;
                int acqTicks = (Fix.FromInt(weapon.AcquisitionTicks) * acqMul).RoundToInt();
                weapon.Acquiring = target;
                weapon.AcquiringUntilTick = w.Tick + acqTicks;
                w.Entities.Weapon[i] = weapon;
                return;
            }
            if (w.Tick < weapon.AcquiringUntilTick) return;

            if (weapon.IsInterceptor) ResolveInterception(w, i, target, ref weapon);
            else ResolveDirectFire(w, i, target, ref weapon);

            weapon.NextFireTick = w.Tick + weapon.CooldownTicks;
            weapon.Acquiring = EntityHandle.None;
            w.Entities.Weapon[i] = weapon;
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

            return NearestEnemyInRange(w, i, weapon);
        }

        static EntityHandle NearestEnemyInRange(World w, int i, WeaponState weapon)
        {
            byte team = w.Entities.Team[i];
            Fix2 pos = w.Entities.Position[i];
            Fix rangeSq = weapon.RangeMetres * weapon.RangeMetres;

            EntityHandle best = EntityHandle.None;
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
                if (dSq < bestDistSq) { bestDistSq = dSq; best = w.Entities.HandleAt(j); }
            }
            return best;
        }

        static void ResolveDirectFire(World w, int i, EntityHandle target, ref WeaponState weapon)
        {
            // A rotary drone diving on a vehicle hits the thin plate on top. It is
            // the reason a two-hundred-Materiel drone is a real threat to a tank.
            bool topAttack = w.Entities.EntityLayer[i] == Layer.Low
                             && w.Entities.EntityLayer[target.Index] == Layer.Ground;

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
                if (!w.Entities.Sensor[j].RadioFrequency) continue;

                Fix r = w.Entities.Sensor[j].FootprintMetres;
                if (Fix2.SqrDistance(w.Entities.Position[j], targetPos) <= r * r)
                    return Fix.One;  // radar-cued
            }

            for (int j = 1; j < w.Entities.HighWater; j++)
            {
                if (!w.Entities.IsSlotAlive(j)) continue;
                if (w.Entities.Team[j] != team) continue;
                if (!w.Entities.Has(j, ComponentMask.Sensor)) continue;

                Fix r = w.EffectiveSensorRange(j);
                if (Fix2.SqrDistance(w.Entities.Position[j], targetPos) <= r * r)
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
