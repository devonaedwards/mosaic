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
        EntityCapacityReached = 5,
        DaylightRefused = 6,
        WeatherRefused = 7
    }

    public static class SortieSystem
    {
        /// <summary>
        /// How close a reusable airframe must get to its own pad before
        /// MovementSystem.CheckLandings calls Recover below. No source gives
        /// a figure for this - a designer estimate, set a little looser than
        /// MovementSystem's own arrival radius so a drone ordered back onto
        /// its exact home point reliably lands once it gets there rather than
        /// sitting one metre outside a stricter ring. AUDIT-UNWIRED.md F13.
        /// </summary>
        public const int LandingRadiusMetres = 30;


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

            // A heavy multirotor in daylight is not a weapon, it is a target: slow,
            // loud, and the size of a small car. They fly at night or not at all,
            // which is what gives the clock its weight.
            if (def.NightOnly && !w.IsNight)
            {
                w.Events.Push(SimEventKind.SortieRefusedDaylight, w.Tick, EntityHandle.None,
                              EntityHandle.None, team, defId);
                return LaunchResult.DaylightRefused;
            }

            // Weather grounds by what an airframe burns, not by what it costs.
            // Wind takes the small electrics - a quadcopter cannot hold station in
            // eighteen metres a second and an interceptor cannot catch anything.
            // Rain, and in winter icing, takes every electric: a quarter of the
            // thrust is gone inside the first minute of accretion, and no de-icing
            // is reaching expendable airframes this decade.
            //
            // Nothing grounds a two-stroke engine above the cloud deck. That is
            // the asymmetry, and it is the one place this game is deliberately
            // unfair: the side flying cheap quadcopters loses half its year and
            // the side flying combustion strike drones does not.
            if (WeatherGrounds(w.Weather, def.Propulsion))
            {
                w.Events.Push(SimEventKind.SortieRefusedWeather, w.Tick, EntityHandle.None,
                              EntityHandle.None, team, defId);
                return LaunchResult.WeatherRefused;
            }

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
            // AUDIT-UNWIRED.md F13: explicit, even though World.Spawn already
            // set HomePosition to this same padPosition - a launch is the one
            // place that actually knows "this is a pad", so it is the right
            // place to say so, rather than leaning on Spawn's default.
            w.Entities.Sortie[i].HomePosition = padPosition;
            w.Entities.Sortie[i].HasLeftHome = false;

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
        /// Lay a stick of mines along a line, from a heavy drone.
        ///
        /// This is the answer to ground you cannot hold by standing on it, and it
        /// is the least glamorous thing in the game: no pilot skill, no timing, no
        /// counter-play in the moment. You put it there at night and it is still
        /// there in the morning, and it does not ask whose vehicle arrives first.
        /// </summary>
        public static int LayMines(World w, EntityHandle bomber, Fix2 from, Fix2 to)
        {
            if (!w.Entities.IsAlive(bomber)) return 0;
            int i = bomber.Index;
            int defId = w.Entities.DefId[i];
            if (defId < 0) return 0;

            UnitDef def = Catalog.Get(defId);
            if (def.MinesCarried <= 0) return 0;

            byte team = w.Entities.Team[i];
            Fix2 axis = to - from;
            Fix length = axis.Magnitude();
            Fix2 step = length.Raw > 0
                ? axis / Fix.FromInt(def.MinesCarried)
                : new Fix2(Fix.FromInt(SimConstants.MineSpacingMetres), Fix.Zero);

            for (int m = 0; m < def.MinesCarried; m++)
                w.SpawnMine(team, from + step * Fix.FromInt(m), def.MineDamage);

            w.Events.Push(SimEventKind.MinesLaid, w.Tick, bomber, EntityHandle.None,
                          team, def.MinesCarried);
            return def.MinesCarried;
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

        /// <summary>
        /// Whether this weather stops this kind of airframe leaving the ground.
        /// Murk is absent on purpose: fog grounds nothing at all, it only blinds -
        /// which is exactly what makes it the window to attack through.
        /// </summary>
        public static bool WeatherGrounds(WeatherState weather, Propulsion propulsion)
        {
            switch (weather)
            {
                case WeatherState.Wind:
                    return propulsion == Propulsion.SmallElectric;
                case WeatherState.Wet:
                    return propulsion == Propulsion.SmallElectric
                        || propulsion == Propulsion.HeavyElectric;
                default:
                    return false;
            }
        }
    }
}
