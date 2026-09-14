// KILL ZONE - a real-time strategy video game.
// Launching a sortie: the game's atomic action.
//
// In most strategy games you build a unit and it appears on the map. Here,
// finished airframes go into a hangar and stay there. What puts one in the air
// is a sortie, and a sortie needs four things at once: the airframe, a crew free
// to fly it, a control link back to that crew, and - for anything that shoots
// indirectly - somebody who can see the target.
//
// Take any one away and the launch is refused. A player whose crews are all
// recovering watches every card in the sidebar grey out at the same moment, and
// learns more from those four seconds than from any tutorial.

namespace KZ.Sim
{
    public enum LaunchResult : byte
    {
        Launched = 0,
        NoCrew = 1,
        NoUplinkCapacity = 2,
        NoTetherAvailable = 3,
        InsufficientMateriel = 4,
        EntityCapacityReached = 5
    }

    public static class SortieSystem
    {
        /// <summary>
        /// Put one airframe in the air from a pad, optionally against a target.
        /// launchIndex staggers departures so a flight of six does not spawn on top
        /// of itself.
        /// </summary>
        public static LaunchResult Launch(World w, byte team, int defId, Fix2 padPosition,
                                          EntityHandle target, int launchIndex, out EntityHandle spawned)
        {
            spawned = EntityHandle.None;
            UnitDef def = Catalog.Get(defId);
            PlayerState player = w.Player(team);

            if (player.Materiel < Fix.FromInt(def.CostMateriel))
            {
                w.Events.Push(SimEventKind.SortieRejected, w.Tick, EntityHandle.None,
                              EntityHandle.None, team, (int)LaunchResult.InsufficientMateriel);
                return LaunchResult.InsufficientMateriel;
            }

            // Hands are the scarce resource, not aircraft.
            int crewId = -1;
            if (def.ConsumesCrew)
            {
                crewId = player.Crews.SelectForLaunch();
                if (crewId < 0)
                {
                    w.Events.Push(SimEventKind.SortieRejected, w.Tick, EntityHandle.None,
                                  EntityHandle.None, team, (int)LaunchResult.NoCrew);
                    return LaunchResult.NoCrew;
                }
            }

            // Satellite channels are scarce in a different way, and the interface
            // greys the card out with the same language, because it is the same
            // kind of failure.
            if (def.Link == LinkKind.Satellite)
            {
                if (player.UplinkInUse >= player.UplinkCapacity)
                {
                    w.Events.Push(SimEventKind.SortieRejected, w.Tick, EntityHandle.None,
                                  EntityHandle.None, team, (int)LaunchResult.NoUplinkCapacity);
                    return LaunchResult.NoUplinkCapacity;
                }
            }

            EntityHandle h;
            try { h = w.Spawn(defId, team, padPosition); }
            catch (System.InvalidOperationException)
            {
                w.Events.Push(SimEventKind.SortieRejected, w.Tick, EntityHandle.None,
                              EntityHandle.None, team, (int)LaunchResult.EntityCapacityReached);
                return LaunchResult.EntityCapacityReached;
            }

            int i = h.Index;

            // A fiber drone needs a physical spool. If none is free the airframe
            // never leaves the pad.
            if (def.Link == LinkKind.Fiber)
            {
                int tetherId = w.Tethers.Create(h, EntityHandle.None, padPosition,
                                                def.SpoolLengthMetres, team, w.Tick);
                if (tetherId < 0)
                {
                    w.QueueDeath(h);
                    w.Events.Push(SimEventKind.SortieRejected, w.Tick, EntityHandle.None,
                                  EntityHandle.None, team, (int)LaunchResult.NoTetherAvailable);
                    return LaunchResult.NoTetherAvailable;
                }
                w.Entities.TetherId[i] = tetherId;
            }

            if (def.Link == LinkKind.Satellite) player.UplinkInUse++;

            player.Materiel -= Fix.FromInt(def.CostMateriel);

            if (crewId >= 0)
            {
                player.Crews.Assign(crewId, h, w.Tick);
                w.Entities.Sortie[i].CrewId = crewId;
            }

            w.Entities.Sortie[i].Phase = SortiePhase.Spawning;
            w.Entities.Sortie[i].SpawnTick = w.Tick;
            w.Entities.Sortie[i].EgressUntilTick = w.Tick
                + SimConstants.SortiePadEgressBaseTicks
                + launchIndex * SimConstants.SortiePadEgressPerIndexTicks;

            if (w.Entities.IsAlive(target)) MovementSystem.OrderAttack(w, h, target);

            w.Events.Push(SimEventKind.SortieLaunched, w.Tick, h, target, team, defId);
            spawned = h;
            return LaunchResult.Launched;
        }

        /// <summary>
        /// How many of this airframe could actually get airborne right now. This is
        /// the number the sidebar card shows, and it is the honest one: it counts
        /// crews and satellite channels, not just money.
        /// </summary>
        public static int LaunchableCount(World w, byte team, int defId, int airframesInStock)
        {
            UnitDef def = Catalog.Get(defId);
            PlayerState player = w.Player(team);

            int limit = airframesInStock;
            if (def.ConsumesCrew) limit = System.Math.Min(limit, player.Crews.ReadyCount);
            if (def.Link == LinkKind.Satellite)
                limit = System.Math.Min(limit, player.UplinkCapacity - player.UplinkInUse);
            if (limit < 0) limit = 0;
            return limit;
        }

        /// <summary>
        /// Hand a crew back when a reusable airframe lands. One-way airframes never
        /// reach here; their crew is released when the drone is destroyed.
        /// </summary>
        public static void Recover(World w, EntityHandle drone)
        {
            if (!w.Entities.IsAlive(drone)) return;
            int i = drone.Index;
            if (!w.Entities.Has(i, ComponentMask.Sortie)) return;

            int crewId = w.Entities.Sortie[i].CrewId;
            byte team = w.Entities.Team[i];
            if (crewId >= 0 && team < w.Players.Length)
            {
                w.Players[team].Crews.Release(crewId, w.Tick);
                w.Entities.Sortie[i].CrewId = -1;
            }
            w.Entities.Sortie[i].Phase = SortiePhase.None;
        }
    }
}
