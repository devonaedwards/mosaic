// KILL ZONE - a real-time strategy video game.
// Moving things toward where they were told to go.

namespace KZ.Sim
{
    public static class MovementSystem
    {
        public static void Step(World w)
        {
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (!w.Entities.Has(i, ComponentMask.Mover)) continue;
                if (w.Entities.Has(i, ComponentMask.Structure)) continue;
                StepOne(w, i);
            }

            CheckLandings(w);
        }

        static void StepOne(World w, int i)
        {
            MoverState mover = w.Entities.Mover[i];
            Fix2 pos = w.Entities.Position[i];

            // AUDIT-UNWIRED.md F19: SortieState.EgressUntilTick was written by
            // SortieSystem.Launch from the launch index and read nowhere, so
            // the balance harness had to fake departure spacing by hand,
            // enqueueing each launch command on a different tick, because a
            // flight ordered to attack the same target spawned on top of
            // itself and flew it as one simultaneous arrival. A drone held
            // here until its own egress tick elapses gets the same spacing
            // from the launch it already carries, with no harness workaround
            // needed.
            if (w.Entities.Has(i, ComponentMask.Sortie) && w.Tick < w.Entities.Sortie[i].EgressUntilTick)
            {
                w.Entities.Velocity[i] = Fix2.Zero;
                return;
            }

            // A drone flying an order it can no longer receive updates to still
            // flies the last order it got. A ground robot that loses its link just
            // stops, in the open, which is frequently worse.
            if (!mover.HasOrder)
            {
                w.Entities.Velocity[i] = Fix2.Zero;
                return;
            }

            Fix2 destination = mover.OrderPoint;
            if (w.Entities.IsAlive(mover.OrderTarget))
                destination = w.Entities.Position[mover.OrderTarget.Index];

            Fix2 toTarget = destination - pos;
            Fix distance = toTarget.Magnitude();

            // Twenty-four metres, not two. This has to stay larger than one
            // tick of travel or nothing ever arrives: at real speeds and a tick
            // of an eighth of a real second, a jet strike drone covers 17.5 m
            // between ticks and would step straight over a 2 m circle, turn
            // round, and orbit its own destination forever. The old 2 was safe
            // only because the fastest thing in the catalogue moved 1.7 map
            // metres a tick.
            Fix arriveRadius = Fix.FromInt(24);
            if (distance <= arriveRadius)
            {
                w.Entities.Velocity[i] = Fix2.Zero;
                if (mover.OrderTarget.IsNone)
                {
                    mover.HasOrder = false;
                    w.Entities.Mover[i] = mover;
                }
                return;
            }

            Fix2 direction = toTarget / distance;

            // Turn toward the heading rather than snapping to it, so a fixed-wing
            // drone arcs and a quadcopter pivots.
            ushort desiredYaw = Trig.Atan2(direction.Y, direction.X);
            w.Entities.Yaw[i] = Trig.RotateToward(w.Entities.Yaw[i], desiredYaw, mover.TurnRateBamPerTick);

            Fix speed = mover.SpeedMetresPerSecond * mover.SpeedMultiplier
                        * GroundScale(w, i);
            Fix2 velocity = direction * speed;

            // A fiber drone at full stretch keeps whatever part of its intended
            // motion does not pull further from the anchor. On screen it reads as
            // exactly what it is: a drone on the end of a line.
            int tetherId = w.Entities.TetherId[i];
            if (tetherId >= 0) velocity = w.Tethers.ConstrainVelocity(tetherId, pos, velocity);

            Fix2 step = velocity * SimConstants.Dt;
            Fix2 next = pos + step;

            // Ground units respect terrain; anything airborne ignores it.
            if (w.Entities.EntityLayer[i] == Layer.Ground && !w.Terrain.IsPassableForGround(next))
            {
                Fix2 slideX = new Fix2(next.X, pos.Y);
                Fix2 slideY = new Fix2(pos.X, next.Y);
                if (w.Terrain.IsPassableForGround(slideX)) next = slideX;
                else if (w.Terrain.IsPassableForGround(slideY)) next = slideY;
                else next = pos;
            }

            if (!w.Terrain.InBounds(next)) next = pos;

            mover.LastStepDistance = Fix2.Distance(pos, next);

            w.Entities.Position[i] = next;
            w.Entities.Velocity[i] = velocity;
            w.Entities.Mover[i] = mover;
        }

        /// <summary>
        /// AUDIT-UNWIRED.md F13: SortieSystem.Recover had zero call sites, so
        /// the five reusable airframes (Scout Quad, Multirole Quad, Recon
        /// Wing, Night Bomber, Mothership) held their crew forever - the only
        /// way to free one was to lose the aircraft, which inverts "airframes
        /// are cheap, crews are the cap" into "flying anything costs a crew
        /// permanently" for exactly the units that were supposed to give
        /// theirs back. A reusable, still-crewed airframe now lands - and
        /// hands its crew back - the moment it is within LandingRadiusMetres
        /// of the pad it launched from, provided it actually left first.
        ///
        /// That guard is not optional: HomePosition is set to the launch
        /// position, so without it every fresh launch would be "within
        /// landing range of home" on its very first tick and undo the crew
        /// assignment SortieSystem.Launch just made in the same breath.
        ///
        /// Deliberately a passive check rather than an autopilot - it does not
        /// invent a return order. Getting a drone home again is still the
        /// player's (or a future AI's) job; this only wires up what happens
        /// once it gets there, which is what AUDIT-UNWIRED.md's own sizing for
        /// this item describes.
        /// </summary>
        static void CheckLandings(World w)
        {
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (!w.Entities.Has(i, ComponentMask.Sortie)) continue;

                SortieState s = w.Entities.Sortie[i];
                if (s.OneWay || s.CrewId < 0) continue;

                Fix radius = Fix.FromInt(SortieSystem.LandingRadiusMetres);
                Fix distSq = Fix2.SqrDistance(w.Entities.Position[i], s.HomePosition);

                if (distSq > radius * radius)
                {
                    if (!s.HasLeftHome)
                    {
                        s.HasLeftHome = true;
                        w.Entities.Sortie[i] = s;
                    }
                }
                else if (s.HasLeftHome)
                {
                    SortieSystem.Recover(w, w.Entities.HandleAt(i));
                }
            }
        }

        public static void OrderMoveTo(World w, EntityHandle h, Fix2 point)
        {
            if (!w.Entities.IsAlive(h)) return;
            int i = h.Index;
            if (!w.Entities.Has(i, ComponentMask.Mover)) return;

            // An amber link means you have lost fine control: the drone finishes
            // what it is doing and will not take a new instruction.
            if (w.Entities.Has(i, ComponentMask.Sortie) && !w.Entities.Sortie[i].AcceptsNewOrders) return;

            w.Entities.Mover[i].HasOrder = true;
            w.Entities.Mover[i].OrderPoint = point;
            w.Entities.Mover[i].OrderTarget = EntityHandle.None;

            if (w.Entities.Has(i, ComponentMask.Sortie))
            {
                w.Entities.Sortie[i].DesignatedPoint = point;
                w.Entities.Sortie[i].HasDesignatedPoint = true;
            }
        }

        public static void OrderAttack(World w, EntityHandle h, EntityHandle target)
        {
            if (!w.Entities.IsAlive(h) || !w.Entities.IsAlive(target)) return;
            int i = h.Index;
            if (!w.Entities.Has(i, ComponentMask.Mover)) return;
            if (w.Entities.Has(i, ComponentMask.Sortie) && !w.Entities.Sortie[i].AcceptsNewOrders) return;

            w.Entities.Mover[i].HasOrder = true;
            w.Entities.Mover[i].OrderTarget = target;
            w.Entities.Mover[i].OrderPoint = w.Entities.Position[target.Index];

            if (w.Entities.Has(i, ComponentMask.Sortie))
            {
                w.Entities.Sortie[i].Target = target;
                w.Entities.Sortie[i].Phase = SortiePhase.Transit;
                // Remember where the target was. If the link dies and the airframe
                // has last-mile guidance, this is the point it carries on to.
                w.Entities.Sortie[i].DesignatedPoint = w.Entities.Position[target.Index];
                w.Entities.Sortie[i].HasDesignatedPoint = true;
            }
        }

        /// <summary>
        /// What the state of the ground does to a vehicle on it.
        ///
        /// Mud does not slow a road down - a road in the rain is still a road. What
        /// it does is delete everything either side of the road, and the effect of
        /// that is not that vehicles go slower, it is that they all end up in the
        /// same place. Every wheel on the map gets funnelled onto the handful of
        /// hard surfaces, which are already the most watched ground there is.
        ///
        /// Frozen ground is the opposite and is genuinely better than firm: the
        /// whole landscape becomes driveable at once. It costs somewhere else, in
        /// what the cold does to every battery in the air.
        ///
        /// Aircraft are unaffected by all of it, which is the point of aircraft.
        /// </summary>
        public static Fix GroundScale(World w, int i)
        {
            if (w.Entities.EntityLayer[i] != Layer.Ground) return Fix.One;
            if (w.Ground == GroundState.Firm) return Fix.One;

            TileClass tile = w.Terrain.AtPosition(w.Entities.Position[i]);
            bool onRoad = tile == TileClass.Road;

            if (w.Ground == GroundState.Mud)
                return onRoad ? Fix.One : SimConstants.MudOffRoadScale;

            return onRoad ? Fix.One : SimConstants.FrozenOffRoadScale;
        }
    }
}
