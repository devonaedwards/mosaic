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
        }

        static void StepOne(World w, int i)
        {
            MoverState mover = w.Entities.Mover[i];
            Fix2 pos = w.Entities.Position[i];

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

            Fix arriveRadius = Fix.FromInt(2);
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
