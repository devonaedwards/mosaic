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
        const int BorderMetres = 1300;
        const int NeutralMetres = 64; // two Territory cells either side of the line

        static EntityHandle designatorHandle;

        public static int Main(string[] args)
        {
            ulong seed = 20260914UL;
            int seconds = 60;
            bool verbose = true;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--seed" && i + 1 < args.Length) seed = ulong.Parse(args[++i]);
                else if (args[i] == "--seconds" && i + 1 < args.Length) seconds = int.Parse(args[++i]);
                else if (args[i] == "--quiet") verbose = false;
            }

            Console.WriteLine("KILL ZONE - headless match");
            Console.WriteLine("seed " + seed + ", " + seconds + " seconds of simulated time");
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
            Terrain t = new Terrain(2400, 1600);
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
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(300, 780));
            w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(380, 860));
            w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(300, 900));
            w.Spawn(Catalog.IdOf("Drone Workshop"), 1, P(380, 700));
            w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(450, 780));
            // Far enough forward to extend radio control, far enough back that
            // the defending tank cannot simply shell it.
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(780, 780));
            // On the attacker's own ground when the match starts - link green,
            // in the constellation's licensed coverage. Script() below pushes
            // it across the border later, which is the only way to see FINDINGS
            // 26's mechanic happen in a played match rather than a unit test.
            designatorHandle = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(1200, 700));

            // Defender.
            w.Spawn(Catalog.IdOf("Command Post"), 2, P(2100, 780));
            w.Spawn(Catalog.IdOf("EW Post"), 2, P(1500, 780));
            // The gun mount sits back on the base rather than covering the whole
            // approach. As currently tuned one of them kills a drone every 1.25
            // seconds across 550 metres, which nothing in the roster can cross -
            // see docs/FINDINGS.md, item 2 - so where it is placed decides whether
            // there is a game in front of it.
            w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(2050, 780));
            w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1450, 780));
            w.Spawn(Catalog.IdOf("Supply Truck"), 2, P(1900, 860));

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
            // AUDIT-UNWIRED.md F6 / FINDINGS 26: with the border now placed,
            // push the forward observer across it partway through the match -
            // ground the attacker is not shooting for, just standing on. This
            // is the only line in the mission that makes the satellite link go
            // black in a played match rather than a unit test.
            if (tick == SimConstants.Seconds(10))
                MovementSystem.OrderMoveTo(w, designatorHandle, P(1450, 700));

            // One sortie a second, alternating between the cheap radio airframe -
            // which the jammer will eat before it arrives - and the fiber airframe,
            // which it cannot touch.
            //
            // The lesson the run should show: the radio airframes lose their
            // pilots inside the jamming bubble and fall out of the sky short of the
            // target, while the fiber airframes fly through it untouched and only
            // have to survive the thread itself.
            if (tick % (SimConstants.TicksPerSecond * 2) != 0) return;
            int wave = tick / (SimConstants.TicksPerSecond * 2);
            if (wave == 0) return;

            string airframe = (wave % 2 == 0) ? "FPV Team" : "Fiber FPV Team";

            // Launched forward, beside the relay mast. A fiber thread is exposed
            // for every second it is in the air, so a drone launched a kilometre
            // further back usually loses its line before it arrives. Where you
            // launch from is a real decision, not a detail.
            Fix2 pad = P(860, 780);

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
            Console.WriteLine("AUDIT-UNWIRED.md F6, wired this pass:");
            Console.WriteLine("  satellite lost/regained crossing the border  "
                              + n.SatelliteLost + " / " + n.SatelliteRegained);
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
        public int SatelliteLost, SatelliteRegained;

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
                    case SimEventKind.DayPhaseChanged:
                        Say(e.Tick, "it is now " + ((DayPhase)e.Param).ToString().ToLowerInvariant());
                        break;
                    // AUDIT-UNWIRED.md F6: this fired only inside KZ.Tests
                    // before this pass. Narrating it here is how a played
                    // match, not just a unit test, shows the border actually
                    // doing something.
                    case SimEventKind.SatelliteCoverageLost:
                        SatelliteLost++;
                        Say(e.Tick, "the designator team crossed the border and lost satellite coverage");
                        break;
                    case SimEventKind.SatelliteCoverageRegained:
                        SatelliteRegained++;
                        Say(e.Tick, "the designator team is back over its own ground");
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
            int seconds = tick / SimConstants.TicksPerSecond;
            Console.WriteLine(string.Format("  {0,2}:{1:00}  {2}", seconds / 60, seconds % 60, what));
        }
    }
}
