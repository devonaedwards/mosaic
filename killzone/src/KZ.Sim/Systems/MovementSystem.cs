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

            // A target that has died stops being a target. Without this an attack
            // order can never complete: the destination falls back to OrderPoint -
            // where the target was when the order was given - the airframe arrives,
            // and the arrival test below refuses to clear an order that still names
            // a target. So it hangs over that spot for the rest of the match, and
            // because a crew comes back only by landing or by dying, and a drone
            // frozen in mid-air does neither, it takes a crew with it.
            //
            // That is not a cosmetic leak. The playable scenario's opposition
            // throws a one-way FPV at whatever it can see, and most of what it can
            // see is the player's own one-way drones, which kill themselves on
            // impact. Every airframe it launched at one stalled over a dead handle
            // holding a crew, and a side with six crews and no way to get them back
            // stops launching anything after six sorties.
            if (!mover.OrderTarget.IsNone && !w.Entities.IsAlive(mover.OrderTarget))
            {
                mover.OrderTarget = EntityHandle.None;
                w.Entities.Mover[i] = mover;
            }

            Fix2 destination = mover.OrderPoint;
            if (w.Entities.IsAlive(mover.OrderTarget))
            {
                destination = w.Entities.Position[mover.OrderTarget.Index];

                // An interceptor does not chase. It is vectored.
                if (w.Entities.Has(i, ComponentMask.Weapon) && w.Entities.Weapon[i].IsInterceptor)
                    destination = InterceptPoint(w, i, mover.OrderTarget, mover);
            }

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

                    // A one-way airframe is the munition (CombatSystem says so in
                    // as many words when it kills one that has just struck). It has
                    // no undercarriage and no way home, so an order that ends with
                    // nothing there ends the airframe: it goes into the ground at
                    // the aimpoint. The alternative, which is what happened before,
                    // is a warhead hovering over an empty field forever holding one
                    // of the six or fourteen crews the whole game is rationed by.
                    //
                    // Gated on HasLeftHome so that a munition ordered at a point
                    // inside its own pad's landing ring is not destroyed on the
                    // tick its egress hold expires, before it has flown anywhere.
                    //
                    // And gated on the seeker being laid on nothing, which is the
                    // one that took measuring. A munition arrives with its warhead
                    // pointed at something or it does not, and CombatSystem has
                    // already answered that: a target inside the 96 m weapon
                    // envelope is in Acquiring or CommittedTarget several ticks
                    // before the 24 m arrival ring. The first version of this
                    // expended on arrival flat, which destroyed every raiding
                    // airframe one tick into a fifteen-tick acquisition and turned a
                    // strike that should have taken a relay mast off the map into a
                    // warhead in an empty field - the exact failure the log already
                    // had a line for.
                    if (w.Entities.Has(i, ComponentMask.Sortie)
                        && w.Entities.Sortie[i].OneWay
                        && w.Entities.Sortie[i].HasLeftHome
                        && !StillHunting(w, i))
                        w.Kill(w.Entities.HandleAt(i), EntityHandle.None);
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
        /// Whether this airframe's own seeker currently has something. Read off the
        /// weapon rather than re-scanned here, because CombatSystem's answer is the
        /// one that decides whether a shot happens and two answers that could
        /// disagree would be worse than one.
        /// </summary>
        static bool StillHunting(World w, int i)
        {
            if (!w.Entities.Has(i, ComponentMask.Weapon)) return false;
            WeaponState wp = w.Entities.Weapon[i];
            return w.Entities.IsAlive(wp.Acquiring) || w.Entities.IsAlive(wp.CommittedTarget);
        }

        /// <summary>
        /// Where to send an interceptor, which is not where its target is.
        ///
        /// This is the mechanism FINDINGS 35 asked for and the one thing in the
        /// interception path that was never built. IsInterceptor, InterceptBase
        /// Chance, ResolveInterception, CueMultiplier and SpeedRatio all existed and
        /// all describe the *terminal* moment - what happens once the interceptor is
        /// already there. Nothing ever got it there. An interceptor flew the same
        /// order every other airframe flies: at the target's current position, every
        /// tick, which is a curve of pursuit. Against something slower that is merely
        /// wasteful; against anything faster - and point-defence.md §5's whole
        /// argument is that the threat has moved to 500-600 km/h against a 300 km/h
        /// propeller interceptor - it is a stern chase that never closes, and the
        /// airframe follows its target off the far side of the map until it is shot
        /// down or the match ends.
        ///
        /// Real interception is a cue, a solution, and a vector: hold the track,
        /// measure the velocity, compute where the two will meet, fly there, and let
        /// the seeker take the last two hundred metres. So the question a slower
        /// interceptor faces is not "can it catch up" - it never catches up - but
        /// "is the track good enough to compute the meeting point", and that is a
        /// far better mechanic because it makes detection quality pay.
        ///
        /// The solution is iterated a fixed three times rather than solved in closed
        /// form. The quadratic is exact but squares a term of order (range x speed),
        /// which at 20 km and 500 play-metres a second overflows a Q31.32; three
        /// passes of "where will it be when I could get to where I last thought it
        /// would be" converge to within a metre or two over any geometry this game
        /// produces, and a fixed iteration count is what the determinism rules allow.
        ///
        /// How much of that lead is actually flown is decided by World.TrackQuality
        /// Of - see SimConstants.InterceptLeadRadarTrack - and what is left over is
        /// the part a second interceptor is worth buying.
        /// </summary>
        static Fix2 InterceptPoint(World w, int i, EntityHandle target, MoverState mover)
        {
            int ti = target.Index;
            Fix2 targetPos = w.Entities.Position[ti];
            Fix2 targetVel = w.Entities.Velocity[ti];
            if (targetVel.SqrMagnitude().Raw == 0) return targetPos;

            Fix mySpeed = mover.SpeedMetresPerSecond * mover.SpeedMultiplier;
            if (mySpeed.Raw <= 0) return targetPos;

            Fix2 pos = w.Entities.Position[i];
            Fix t = Fix2.Distance(pos, targetPos) / mySpeed;
            for (int pass = 0; pass < 2; pass++)
            {
                if (t > SimConstants.InterceptMaxLeadSeconds) t = SimConstants.InterceptMaxLeadSeconds;
                t = Fix2.Distance(pos, targetPos + targetVel * t) / mySpeed;
            }
            if (t > SimConstants.InterceptMaxLeadSeconds) t = SimConstants.InterceptMaxLeadSeconds;

            Fix2 fullLead = targetVel * t;

            Fix flown;
            switch (w.TrackQualityOf(w.Entities.Team[i], target))
            {
                case TrackQuality.Radar: flown = SimConstants.InterceptLeadRadarTrack; break;
                case TrackQuality.Optical: flown = SimConstants.InterceptLeadOpticalTrack; break;
                // Nobody is holding it. There is no solution to fly, so the
                // interceptor is pointed at the contact and does what it did before
                // any of this existed.
                default: return targetPos;
            }

            // Deliberately no bracketing term, and this is worth stating because it
            // is the obvious next thing and the obvious version of it would be
            // wrong. A second interceptor is not a second sample of a noisy
            // estimate, to be spread over a variance radius; the meeting point is
            // uncertain because the target is somebody making choices - it dives, it
            // breaks, it flies a line chosen in anticipation of exactly this - and a
            // pair of interceptors is worth buying because it covers two of those
            // branches, not because it covers a blob.
            //
            // This game has no evasion, so its targets have no branches, so a second
            // interceptor here is honestly worth one more terminal roll and nothing
            // else. Building a spread and calling it bracketing would have measured
            // as an improvement and meant nothing. The pieces for the real version
            // are all present and are recorded rather than guessed at: Autonomy
            // Classifier.IsPilotedOnLiveFeed already separates the airframes that
            // could break - a person is watching the feed and can see the
            // interceptor coming - from the autonomous ones that cannot, and the
            // catalogue already prices how many branches an airframe has, with the
            // Jet Strike Drone turning at 22 degrees a second against an FPV's 180.
            // Fast means few options, each displacing the meeting point a long way;
            // nimble means many, each displacing it a little. When that exists,
            // interceptors should be spread across branches and the second crew
            // should buy a cut-off, not a re-roll.
            return targetPos + fullLead * flown;
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
                // "Has this airframe actually departed" is now asked of every
                // sortie, not only the ones that can come back: StepOne above
                // expends a one-way munition that arrives at an empty aimpoint and
                // needs the same guard against doing it on the pad. Landing is
                // still only for something that can land and still has a crew to
                // hand back.
                else if (s.HasLeftHome && !s.OneWay && s.CrewId >= 0)
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
