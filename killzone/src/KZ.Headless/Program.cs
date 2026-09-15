// KILL ZONE - a real-time strategy video game.
// Runs a scripted match with no renderer and narrates what happens.
//
// This exists for three reasons. It is how the simulation is exercised before
// there is anything to look at. It is how balance changes get measured - a
// thousand of these run headless in a few minutes and tell you whether four
// drones still kill a tank. And it prints the state hash, which is the number
// two machines in a real match compare to prove they are still playing the same
// game.
//
// AUDIT-UNWIRED.md F6: this was the gap KZ.Balance's own harness fix (FINDINGS
// 33/34) did not close. That fix gave the balance experiments a populated
// world; it never touched the one place that builds the world a match is
// actually played on. Before this pass BuildMap below drew terrain and spawned
// units and left Territory at its constructed default - every cell owned by
// nobody - so every satellite link was permanently black and scene matching
// could never lock in the one scenario meant to stand in for a real game. It
// now places a border and grants each side reconnaissance imagery over its own
// ground, the same shape MakeRealisticWorld uses in src/KZ.Balance/Program.cs
// (read, not edited, per WIRING-SPEC ownership).

using System;
using KZ.Sim;

namespace KZ.Headless
{
    public static class Program
    {
        static Fix F(double v) { return Fix.FromDoubleContentOnly(v); }
        static Fix2 P(double x, double y) { return new Fix2(F(x), F(y)); }

        // The attacker's infrastructure (Relay Mast at x=780) sits well west of
        // this line; the defender's forward tank (x=1450) sits shortly east of
        // it and the supply truck behind it (x=1900) a good deal further -
        // exactly as FINDINGS 26 describes: push past your own border and your
        // drones are on their own over ground you are attacking into, the
        // shallow shot more than the deep one. Mirrors
        // src/KZ.Balance/Program.cs's StandardBorderMetres.
        const int BorderMetres = 15600;   // real metres; 1,300 at the old 12:1
        const int NeutralMetres = 768; // two Territory cells (384 m) either side of the line

        static EntityHandle designatorHandle;

        public static int Main(string[] args)
        {
            ulong seed = 20260914UL;
            // 75 rather than 60: long enough for the early deep strike Script()
            // launches at the supply truck (18 m/s, ~1 km, most of it denied) to
            // actually arrive and either connect or miss on navigation error,
            // instead of the match ending mid-flight.
            int seconds = 75;
            bool verbose = true;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--seed" && i + 1 < args.Length) seed = ulong.Parse(args[++i]);
                else if (args[i] == "--seconds" && i + 1 < args.Length) seconds = int.Parse(args[++i]);
                else if (args[i] == "--quiet") verbose = false;
            }

            Console.WriteLine("KILL ZONE - headless match");
            Console.WriteLine("seed " + seed + ", " + seconds + " seconds of play ("
                              + (seconds * SimConstants.TimeMultiplier) + " s of world at "
                              + SimConstants.TimeMultiplier + "x)");
            Console.WriteLine(new string('-', 68));

            World w = BuildMap(seed);
            Narrator narrator = new Narrator(w, verbose);

            int totalTicks = seconds * SimConstants.TicksPerSecond;
            DateTime start = DateTime.UtcNow;

            for (int tick = 0; tick < totalTicks; tick++)
            {
                Script(w, tick);
                w.Step();
                narrator.Observe();
            }

            double elapsed = (DateTime.UtcNow - start).TotalSeconds;

            Console.WriteLine(new string('-', 68));
            Report(w, narrator, totalTicks, elapsed);
            return 0;
        }

        /// <summary>
        /// A small scripted engagement. The attacker on the left pushes drones east
        /// into a defended position; the defender has a jammer covering the
        /// approach, a tank, and a gun mount.
        /// </summary>
        static World BuildMap(ulong seed)
        {
            // 28.8 x 19.2 real kilometres - the same ground as the old
            // 2400 x 1600 at the twelve-to-one the catalogue used to be written
            // in, now stated in the units everything else is in. The tile
            // rectangles below are tile indices and are unchanged, because the
            // build tile grew with the map.
            Terrain t = new Terrain(28800, 19200);
            t.Fill(TileClass.Open);
            // A treeline across the middle, and power lines beside the road, so
            // there is a fast route and a safe route and they are not the same.
            t.FillRect(120, 60, 190, 90, TileClass.Forest);
            t.FillRect(140, 100, 145, 199, TileClass.PowerLine);
            t.FillRect(0, 95, 299, 98, TileClass.Road);

            World w = new World(t, 1024, 128, seed, 2);

            w.Player(1).Faction = FactionId.KestrelPact;
            w.Player(2).Faction = FactionId.ObsidianDirectorate;
            w.Player(1).Materiel = Fix.FromInt(12000);
            w.Player(2).Materiel = Fix.FromInt(12000);

            // AUDIT-UNWIRED.md F6: the border a satellite link and scene
            // matching both read. Team 1 owns the west, team 2 the east.
            w.Territory.SetVerticalBorder(BorderMetres, 1, 2, NeutralMetres);
            GrantHomeImagery(w, 1, true, BorderMetres - NeutralMetres);
            GrantHomeImagery(w, 2, false, BorderMetres + NeutralMetres);

            // Attacker.
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(3600, 9360));
            w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(4560, 10320));
            w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(3600, 10800));
            w.Spawn(Catalog.IdOf("Drone Workshop"), 1, P(4560, 8400));
            w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(5400, 9360));
            // Far enough forward to extend radio control, far enough back that
            // the defending tank cannot simply shell it.
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(9360, 9360));
            // On the attacker's own ground when the match starts - link green,
            // in the constellation's licensed coverage. Script() below pushes
            // it across the border later, which is the only way to see FINDINGS
            // 26's mechanic happen in a played match rather than a unit test.
            designatorHandle = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(14400, 8400));

            // Defender.
            w.Spawn(Catalog.IdOf("Command Post"), 2, P(25200, 9360));
            w.Spawn(Catalog.IdOf("EW Post"), 2, P(18000, 9360));
            // The gun mount sits back on the base rather than covering the whole
            // approach. As currently tuned one of them kills a drone every four
            // seconds across a kilometre, which nothing in the roster crosses
            // for free -
            // see docs/FINDINGS.md, item 2 - so where it is placed decides whether
            // there is a game in front of it.
            w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(24600, 9360));
            w.Spawn(Catalog.IdOf("Main Tank"), 2, P(17400, 9360));
            w.Spawn(Catalog.IdOf("Supply Truck"), 2, P(22800, 10320));

            return w;
        }

        /// <summary>
        /// Grants a team imagery over its own side of the border, cell by cell -
        /// the same shape as src/KZ.Balance/Program.cs's GrantHomeImagery,
        /// reproduced here rather than shared because the two console apps do
        /// not reference each other's Program class.
        /// </summary>
        static void GrantHomeImagery(World w, byte team, bool west, int borderMetres)
        {
            int borderCell = borderMetres / ReferenceImagery.CellMetres;
            for (int cy = 0; cy < w.Imagery.CellsY; cy++)
                for (int cx = 0; cx < w.Imagery.CellsX; cx++)
                    if (west ? cx <= borderCell : cx >= borderCell)
                        w.Imagery.Grant(team, cx, cy);
        }

        static void Script(World w, int tick)
        {
            // AUDIT-UNWIRED.md F16/F17: the defender's opening housekeeping, and
            // the only place in the repository outside a test where a cage, a
            // blanket or a no-go bubble is brought to a match. All three go
            // through w.Enqueue rather than calling World directly, because the
            // point of the exercise was that they had no order behind them.
            if (tick == 0)
            {
                EntityHandle tank = FindFirst(w, 2, "Main Tank");
                if (w.Entities.IsAlive(tank))
                    // 55%: ground-force.md §2.1 gives a 0.30-0.80 spread for a
                    // cage's disruption chance and no single figure, so the
                    // midpoint is a designer estimate, not a sourced number.
                    w.Enqueue(Command.FitCage(2, tank, 55));

                EntityHandle truck = FindFirst(w, 2, "Supply Truck");
                if (w.Entities.IsAlive(truck))
                    w.Enqueue(Command.FitThermalBlanket(2, truck));

                // A bubble over the defender's own rear - command post, gun
                // mount and the truck behind them - which is the shape
                // autonomy.md §5 describes brigades improvising. Nothing in
                // this scenario currently flies an autonomous munition, so it
                // changes no outcome here; it is in the mission because a
                // mission is where the order has to be reachable from, and
                // FINDINGS 30 is about exactly the gap between those two.
                //
                // 120 play-seconds is a designer estimate. autonomy.md gives no
                // figure for how long a designation should stand, and the
                // duration is pacing rather than physics - it is how often the
                // player is made to look at their own rear area.
                w.Enqueue(Command.SetAutonomyBox(2, P(22000, 8000), P(26000, 11000),
                                                 SimConstants.PlaySeconds(120)));
            }

            // AUDIT-UNWIRED.md F6 / FINDINGS 26: with the border now placed,
            // push the forward observer across it partway through the match -
            // ground the attacker is not shooting for, just standing on. This
            // is the only line in the mission that makes the satellite link go
            // black in a played match rather than a unit test.
            if (tick == SimConstants.Seconds(10))
                MovementSystem.OrderMoveTo(w, designatorHandle, P(17400, 8400));

            // One extra, early strike straight at the supply truck - deep
            // enough behind the border that a scene-matching drone with no
            // imagery of that ground dead-reckons the whole way. Scripted
            // separately from the rotation below so it fires while there is
            // still Materiel for it, rather than waiting on the tank to fall
            // and the treasury to be nearly spent.
            if (tick == SimConstants.Seconds(6))
            {
                EntityHandle truck = FindFirst(w, 2, "Supply Truck");
                if (w.Entities.IsAlive(truck))
                    w.Enqueue(Command.LaunchSortie(1, Catalog.IdOf("Heavy Strike Drone"),
                                                    P(10320, 9360), truck, 1000));
            }

            // One sortie a second, rotating the cheap radio airframe - which the
            // jammer will eat before it arrives - the fiber airframe, which it
            // cannot touch, and every third wave a scene-matching strike drone
            // whose navigation, not its link, is what the border now taxes.
            //
            // The lesson the run should show: the radio airframes lose their
            // pilots inside the jamming bubble and fall out of the sky short of
            // the target; the fiber airframes fly through it untouched and only
            // have to survive the thread itself; and the strike drone keeps
            // both, but AUDIT-UNWIRED.md F5's aimpoint error grows the moment it
            // crosses the border with no imagery of the far side to match
            // against, and a deep enough shot goes off on empty ground instead
            // of the target - see World.ApplyDamage and SimEventKind.
            // NavMissedAimpoint below.
            if (tick % (SimConstants.TicksPerSecond * 2) != 0) return;
            int wave = tick / (SimConstants.TicksPerSecond * 2);
            if (wave == 0) return;

            string airframe;
            switch (wave % 3)
            {
                case 1: airframe = "FPV Team"; break;
                case 2: airframe = "Fiber FPV Team"; break;
                default: airframe = "Heavy Strike Drone"; break;
            }

            // Launched forward, beside the relay mast. A fiber thread is exposed
            // for every second it is in the air, so a drone launched a kilometre
            // further back usually loses its line before it arrives. Where you
            // launch from is a real decision, not a detail.
            Fix2 pad = P(10320, 9360);

            EntityHandle target = FindFirst(w, 2, "Main Tank");
            if (!w.Entities.IsAlive(target)) target = FindFirst(w, 2, "Supply Truck");
            if (!w.Entities.IsAlive(target)) return;

            w.Enqueue(Command.LaunchSortie(1, Catalog.IdOf(airframe), pad, target, wave));
        }

        static EntityHandle FindFirst(World w, byte team, string defName)
        {
            int defId = Catalog.IdOf(defName);
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (w.Entities.Team[i] != team) continue;
                if (w.Entities.DefId[i] != defId) continue;
                return w.Entities.HandleAt(i);
            }
            return EntityHandle.None;
        }

        static void Report(World w, Narrator n, int ticks, double elapsedSeconds)
        {
            Console.WriteLine("simulated  " + ticks + " ticks in " + elapsedSeconds.ToString("0.00")
                              + " s real time  (" + (ticks / elapsedSeconds).ToString("0")
                              + " ticks/s, " + (ticks / elapsedSeconds / SimConstants.TicksPerSecond)
                                                 .ToString("0.0") + "x faster than play)");
            Console.WriteLine();
            Console.WriteLine("sorties launched      " + n.SortiesLaunched);
            Console.WriteLine("  refused, no crew    " + n.RefusedNoCrew);
            Console.WriteLine("links gone amber      " + n.LinksAmber);
            Console.WriteLine("links gone black      " + n.LinksBlack);
            Console.WriteLine("drones lost to jamming" + Pad(n.LostToJamming));
            Console.WriteLine("tethers cut           " + n.TethersCut);
            Console.WriteLine("kills, verified       " + n.VerifiedKills);
            Console.WriteLine("kills, unverified     " + n.UnverifiedKills);
            Console.WriteLine();
            Console.WriteLine("AUDIT-UNWIRED.md F5/F6, wired this pass:");
            Console.WriteLine("  satellite lost/regained crossing the border  "
                              + n.SatelliteLost + " / " + n.SatelliteRegained);
            Console.WriteLine("  scene-matching lock lost/regained            "
                              + n.NavLocksLost + " / " + n.NavLocksRegained);
            Console.WriteLine("  strike drones that missed on navigation error"
                              + Pad(n.MissedByNavError));
            Console.WriteLine();

            for (byte team = 1; team <= 2; team++)
            {
                PlayerState p = w.Player(team);
                Console.WriteLine("team " + team + " (" + p.Faction + ")");
                Console.WriteLine("  materiel        " + p.Materiel.RoundToInt());
                Console.WriteLine("  tasking points  " + p.TaskingPoints.RoundToInt());
                Console.WriteLine("  crews ready     " + p.Crews.ReadyCount + " of " + p.Crews.Count
                                  + "   flying " + p.Crews.FlyingCount);
            }

            EntityHandle tank = FindFirst(w, 2, "Main Tank");
            Console.WriteLine();
            if (w.Entities.IsAlive(tank))
            {
                Fix full = Catalog.Get(Catalog.IdOf("Main Tank")).Hp;
                Fix lost = full - w.Entities.Hp[tank.Index];
                int hits = (lost / Fix.FromDoubleContentOnly(748.0)).FloorToInt();
                Console.WriteLine("the tank survived on " + w.Entities.Hp[tank.Index].RoundToInt()
                                  + " of " + full.RoundToInt() + " hit points ("
                                  + hits + " drones got through; it takes four)");
            }
            else
            {
                Console.WriteLine("the tank was destroyed");
            }

            Console.WriteLine();
            Console.WriteLine("entities alive  " + w.Entities.AliveCount);
            Console.WriteLine("state hash      0x" + w.StateHash().ToString("X16"));
        }

        static string Pad(int v) { return "  " + v; }
    }

    /// <summary>
    /// Watches the event stream and counts what happened. This is exactly the
    /// channel the renderer and the interface will read from, so anything the
    /// narrator can describe, the game can show.
    /// </summary>
    sealed class Narrator
    {
        readonly World world;
        readonly bool verbose;

        public int SortiesLaunched, RefusedNoCrew, LinksAmber, LinksBlack;
        public int LostToJamming, TethersCut, VerifiedKills, UnverifiedKills;
        public int NavLocksLost, NavLocksRegained, SatelliteLost, SatelliteRegained;
        public int MissedByNavError;

        public Narrator(World w, bool verbose) { world = w; this.verbose = verbose; }

        public void Observe()
        {
            for (int i = 0; i < world.Events.Count; i++)
            {
                SimEvent e = world.Events[i];
                switch (e.Kind)
                {
                    case SimEventKind.SortieLaunched: SortiesLaunched++; break;
                    case SimEventKind.SortieRejected:
                        if (e.Param == (int)LaunchResult.NoCrew) RefusedNoCrew++;
                        break;
                    case SimEventKind.LinkAmber: LinksAmber++; break;
                    case SimEventKind.LinkBlack:
                        LinksBlack++;
                        Say(e.Tick, "a drone's link went black");
                        break;
                    case SimEventKind.DroneLostToLinkLoss:
                        LostToJamming++;
                        Say(e.Tick, "a drone fell out of the sky with nobody flying it");
                        break;
                    case SimEventKind.TetherCut:
                        TethersCut++;
                        Say(e.Tick, "a fiber thread parted");
                        break;
                    case SimEventKind.KillVerified:
                        VerifiedKills++;
                        Say(e.Tick, "team " + e.Team + " confirmed a kill for " + e.Param + " tasking points");
                        break;
                    case SimEventKind.KillUnverified:
                        UnverifiedKills++;
                        break;
                    case SimEventKind.StructureDestroyed:
                        Say(e.Tick, "a " + NameOf(e.Param) + " was destroyed");
                        break;
                    case SimEventKind.CrewKilled:
                        Say(e.Tick, "a crew was lost with its quarters");
                        break;
                    case SimEventKind.AutonomyMisidentified:
                        Say(e.Tick, "an autonomous munition picked the wrong target");
                        break;
                    // AUDIT-UNWIRED.md F5/F6: these four fired only inside
                    // KZ.Tests before this pass. Narrating them here is how a
                    // played match, not just a unit test, shows the border
                    // actually doing something.
                    case SimEventKind.NavLockLost:
                        NavLocksLost++;
                        Say(e.Tick, "a strike drone lost its scene-matching lock");
                        break;
                    case SimEventKind.NavLockRegained:
                        NavLocksRegained++;
                        Say(e.Tick, "a strike drone got its lock back");
                        break;
                    case SimEventKind.SatelliteCoverageLost:
                        SatelliteLost++;
                        Say(e.Tick, "the designator team crossed the border and lost satellite coverage");
                        break;
                    case SimEventKind.SatelliteCoverageRegained:
                        SatelliteRegained++;
                        Say(e.Tick, "the designator team is back over its own ground");
                        break;
                    case SimEventKind.NavMissedAimpoint:
                        MissedByNavError++;
                        Say(e.Tick, "a strike drone's warhead went off on empty ground - it did not know where it was");
                        break;
                }
            }
        }

        static string NameOf(int defId)
        {
            return defId >= 0 && defId < Catalog.Count ? Catalog.Get(defId).Name : "structure";
        }

        void Say(int tick, string what)
        {
            if (!verbose) return;
            // The match clock a reader wants is play time, which is what the
            // tick rate measures; the world's own clock runs TimeMultiplier
            // times faster and is not what anyone is watching.
            int seconds = tick / SimConstants.TicksPerSecond;
            Console.WriteLine(string.Format("  {0,2}:{1:00}  {2}", seconds / 60, seconds % 60, what));
        }
    }
}
