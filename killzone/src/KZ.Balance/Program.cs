// KILL ZONE - a real-time strategy video game.
// Balance experiments: run the same engagement many times with one variable
// changed, and print what actually happened.
//
// This exists because arguing about balance from a stat table is guesswork. If
// the question is "can five drones take a gun position", the honest answer comes
// from running it two hundred times, not from multiplying numbers on a page.
//
// AUDIT-UNWIRED.md F33/F34 (previous pass, kept because the reasoning is the
// reason the file is shaped this way): three experiments used to overwrite the
// Gun Mount's range back to 550 m after FINDINGS 2 corrected it to 85, and
// every experiment ran on Fill(Open) with clear weather, firm ground, an
// ownerless map and no imagery. Both are fixed: a mount that needs different
// numbers is a "Test ..." catalogue entry rather than a shipped unit written
// behind the reader's back, and every experiment states the world it ran on in
// its own output.
//
// FINDINGS 32 and the experiment-drift guard (docs/EXPERIMENT-DRIFT.md): seven
// of the ten experiments here could not detect a change to the simulation.
// Stacking, Vertical, Decoy Escort, Mines, Sensors, Aperture and Range have
// been gone through one at a time; the reasoning for each is on the experiment
// itself rather than summarised here, because a summary at the top of a file is
// the first thing to go stale. In outline:
//
//   SENSORS and APERTURE were not experiments. They spawned a mount, overwrote
//   its SensorSuite, stepped once and printed a detection range - the detection
//   formula evaluated at a few points, which moves when a signature constant
//   moves and never otherwise. Both now fight, using test-only catalogue
//   mounts that differ only in what they can see with.
//
//   STACKING, VERTICAL and DECOY ESCORT were pinned to ceilings: a threshold
//   ladder whose bottom rung already won, an attack the defence was
//   structurally incapable of engaging, and a target no package in the budget
//   could have killed. All three now run against forces and defences that can
//   go either way.
//
//   RANGE was asking a question FINDINGS 31 had already answered. It now asks
//   which of the mount's two numbers is binding rather than assuming it is the
//   barrel - and the answer turns out to depend on the arrival pattern.
//
//   MINES was correctly flat and is still flat. What it needed was not
//   resolution but honesty: its damage column was computed by re-deriving
//   Catalog.DamageMultiplier in this harness rather than observed, and the
//   indiscriminate rule FINDINGS 10 rests its argument on had no row at all.
//
// The single mechanism behind most of it: every experiment launched its whole
// flight on one tick, so a mount got one engagement window per trial and any
// quantity that only matters across engagements - a magazine, a second turret,
// a band change, a longer barrel - could not be measured at all. See the
// "Arrival scheduling" section below.

using System;
using System.Collections.Generic;
using KZ.Sim;

namespace KZ.Balance
{
    public static class Program
    {
        static Fix F(double v) { return Fix.FromDoubleContentOnly(v); }
        static Fix2 P(double x, double y) { return new Fix2(F(x), F(y)); }

        // ------------------------------------------------------------------
        // World construction.
        //
        // AUDIT-UNWIRED.md F34: every experiment used to run on Fill(Open), clear
        // weather, firm ground, an ownerless map and no imagery anywhere - flat,
        // empty, weatherless, and with nobody owning any ground. Five whole
        // systems (weather, ground state, territory, reference imagery, the
        // satellite link) are provably inert under those conditions, so nothing
        // measured there can speak to how they interact with anything else.
        //
        // Weather and ground state below are Clear and Firm - themselves ordinary,
        // common conditions, not a fog of war for the harness to hide behind. They
        // are picked deliberately (rather than left as whatever the default
        // happens to be) because Wet and Wind both ground every small-electric
        // airframe these experiments fly (SortieSystem.WeatherGrounds) - which
        // would not make the experiments more realistic, it would make them
        // report zero every time and answer nothing. NightExperiment already
        // exercises the day/night axis on purpose; a dedicated weather experiment
        // that varies Weather itself is future work, not this task's.
        const WeatherState DefaultWeather = WeatherState.Clear;
        const GroundState DefaultGround = GroundState.Firm;

        // Every attacker-vs-gun-mount experiment below places the attacker's
        // infrastructure west of this line and the defended position east of it,
        // just in front of the gun at x=1650 - so the last stretch of any
        // approach is contested ground, not the attacker's own rear.
        const int StandardBorderMetres = 1550;

        /// <summary>
        /// Lays open ground with a road, a treeline and a stretch of rubble across
        /// it, so a match on this map is never the featureless plain F34
        /// described. Positions are fractions of the map's own size so the same
        /// layout works on every map an experiment below happens to use, and the
        /// road band is centred where every experiment already stages its
        /// engagement (roughly y = 0.49 * height, matching KZ.Headless's own
        /// road and every drone pad below).
        /// </summary>
        static void PaintMixedTerrain(Terrain t)
        {
            t.Fill(TileClass.Open);
            int w = t.WidthTiles, h = t.HeightTiles;

            t.FillRect(0, h * 47 / 100, w - 1, h * 51 / 100, TileClass.Road);
            t.FillRect(w * 28 / 100, h * 12 / 100, w * 46 / 100, h * 40 / 100, TileClass.Forest);
            t.FillRect(w * 58 / 100, h * 58 / 100, w * 74 / 100, h * 78 / 100, TileClass.Rubble);
        }

        /// <summary>Grants a team imagery over its own side of the border, cell by cell.</summary>
        static void GrantHomeImagery(World w, byte team, bool west, int borderMetres)
        {
            int borderCell = borderMetres / ReferenceImagery.CellMetres;
            for (int cy = 0; cy < w.Imagery.CellsY; cy++)
                for (int cx = 0; cx < w.Imagery.CellsX; cx++)
                    if (west ? cx <= borderCell : cx >= borderCell)
                        w.Imagery.Grant(team, cx, cy);
        }

        /// <summary>
        /// The default world for a balance experiment: mixed terrain, a stated
        /// weather and ground state, a territory border with an owner on each
        /// side, and each side's reconnaissance of its own rear. This is what
        /// every FINDINGS.md number should have been measured against, and per
        /// F34 none of it was.
        /// </summary>
        static World MakeRealisticWorld(int widthMetres, int heightMetres, int entityCapacity,
            int tetherCapacity, ulong seed, int playerCount, int startTick,
            int borderMetres, byte westTeam, byte eastTeam)
        {
            Terrain t = new Terrain(widthMetres, heightMetres);
            PaintMixedTerrain(t);

            World w = new World(t, entityCapacity, tetherCapacity, seed, playerCount, startTick);
            w.Weather = DefaultWeather;
            w.Ground = DefaultGround;

            const int neutralMetres = 64; // two Territory cells (CellMetres=32) either side of the line
            w.Territory.SetVerticalBorder(borderMetres, westTeam, eastTeam, neutralMetres);
            GrantHomeImagery(w, westTeam, true, borderMetres - neutralMetres);
            GrantHomeImagery(w, eastTeam, false, borderMetres + neutralMetres);

            return w;
        }

        /// <summary>
        /// Flat, clear, firm, ownerless and uncovered - exactly the world every
        /// experiment ran on by accident before this fix (AUDIT-UNWIRED.md F34).
        /// Kept, and named plainly, only so a result can still be checked against
        /// the old FINDINGS.md conclusions that were measured this way. Nothing
        /// below should reach for this because it is simpler; the one caller that
        /// does (GunRangeExperiment) explains why in its own comment.
        /// </summary>
        static World MakeFlatControlWorld(int widthMetres, int heightMetres, int entityCapacity,
            int tetherCapacity, ulong seed, int playerCount, int startTick)
        {
            Terrain t = new Terrain(widthMetres, heightMetres);
            t.Fill(TileClass.Open);
            return new World(t, entityCapacity, tetherCapacity, seed, playerCount, startTick);
        }

        /// <summary>
        /// Prints the conditions an experiment ran under. F34's point generalised:
        /// an experiment whose world is invisible in its own output is exactly how
        /// three balance findings went unnoticed for as long as they did.
        /// </summary>
        static void PrintWorldConfig(int borderMetres, byte westTeam, byte eastTeam)
        {
            Console.WriteLine(string.Format(
                "  world: mixed terrain (road, forest, rubble over open ground), weather {0}, "
              + "ground {1}, territory split at {2} m (team {3} west / team {4} east), "
              + "imagery granted over each side's own ground",
                DefaultWeather, DefaultGround, borderMetres, westTeam, eastTeam));
        }

        public static int Main(string[] args)
        {
            string which = args.Length > 0 ? args[0] : "all";

            if (which == "all" || which == "saturation") SaturationExperiment();
            if (which == "all" || which == "range") GunRangeExperiment();
            if (which == "all" || which == "approach") ApproachExperiment();
            if (which == "all" || which == "night") NightExperiment();
            if (which == "all" || which == "mines") MineExperiment();
            if (which == "all" || which == "sensors") SensorMixExperiment();
            if (which == "all" || which == "stacking") StackingExperiment();
            if (which == "all" || which == "vertical") VerticalExperiment();
            if (which == "all" || which == "decoys") DecoyEscortExperiment();
            if (which == "all" || which == "aperture") ApertureExperiment();

            return 0;
        }

        // ------------------------------------------------------------------

        /// <summary>
        /// Send N drones at a gun position, all at once, and see how many arrive.
        ///
        /// The question this answers is whether a gun that kills one drone at a
        /// time can be beaten by sending more drones at a time. It is the first
        /// thing any player will try.
        /// </summary>
        static void SaturationExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("SATURATION - simultaneous drones against one gun mount");
            Console.WriteLine("launched together from 1,200 m, gun's real range is 85 m (FINDINGS 2)");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  drones   arrived   gun killed   materiel spent   per kill");
            Console.WriteLine("  " + new string('-', 62));

            int[] counts = { 1, 2, 3, 5, 8, 12, 16, 24 };
            for (int c = 0; c < counts.Length; c++)
            {
                int n = counts[c];
                int arrivedTotal = 0, gunKilled = 0;
                const int trials = 60;

                for (int trial = 0; trial < trials; trial++)
                {
                    int arrived;
                    bool killed = RunAssault(n, F(1200), (ulong)(trial + 1), out arrived);
                    arrivedTotal += arrived;
                    if (killed) gunKilled++;
                }

                int cost = n * Catalog.Get(Catalog.IdOf("FPV Team")).CostMateriel;
                double killRate = gunKilled / (double)trials;
                string perKill = killRate > 0 ? ((int)(cost / killRate)).ToString() : "never";

                Console.WriteLine(string.Format("  {0,6}   {1,7}   {2,9}   {3,14}   {4,8}",
                    n,
                    (arrivedTotal / (double)trials).ToString("0.0"),
                    (killRate * 100).ToString("0") + "%",
                    cost,
                    perKill));
            }
            Console.WriteLine();
            Console.WriteLine("  A gun mount takes three hits to destroy. 'arrived' is the average");
            Console.WriteLine("  number of drones per attempt that lived long enough to strike it.");
        }

        /// <summary>
        /// What actually limits a mount's output: the barrel, or the seeing.
        ///
        /// <para><b>What it used to measure, and why that question was already
        /// answered.</b> Eight drones launched together against "Test Long Mount"
        /// with its barrel swept from 550 m down to 85 m. It read 7.6 arrived at
        /// 550 m and 7.8 at 85 m, and 100% gun killed at every rung - twenty-five
        /// seconds of nominal exposure and three point nine producing the same
        /// result. FINDINGS 31 explains why, and the explanation makes the sweep
        /// a question with a known and boring answer: above about 70 m the barrel
        /// is not what stops the mount shooting, because 70 m is where it first
        /// holds a track and the whole engagement is over 3.2 seconds later. A
        /// sweep of a variable that is not binding is a flat line by
        /// construction.</para>
        ///
        /// <para>It was also worse than flat. Sweeping the barrel against a
        /// simultaneous wave gives the mount one engagement window whatever the
        /// barrel is, so even a barrel that *was* binding could not have shown
        /// it: a longer reach buys time, and time is only worth something if
        /// there is a second target to spend it on.</para>
        ///
        /// <para><b>What it measures now.</b> Two sweeps side by side against a
        /// stream of arrivals - the barrel with the seeing held fixed, and the
        /// seeing with the barrel held fixed. The comparison is the point: if
        /// FINDINGS 31 is right, the first should be nearly flat and the second
        /// should not, and the experiment becomes a test of that claim rather
        /// than a restatement of it.</para>
        ///
        /// <para><b>And one thing that had to change.</b> This sweep used to run
        /// on MakeFlatControlWorld, so that terrain and weather could not confound
        /// the variable. That world has no territory owner, and navigation error
        /// is now consumed by World.ApplyDamage - over an ownerless map every
        /// square metre is denied ground, every FPV Team's dead reckoning crosses
        /// SimConstants.MunitionMissRadiusMetres on the way in, and every warhead
        /// misses. The sweep read 0% gun killed at all seven rungs. The flat
        /// control world stopped being a control the moment something started
        /// reading the territory layer, which is worth recording against FINDINGS
        /// 33's "identical, every cell" table rather than working around
        /// silently. This runs on the same realistic world as everything else.</para>
        /// </summary>
        static void GunRangeExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("REACH - what limits a mount's output, the barrel or the seeing");
            Console.WriteLine("'Test Long Mount' (Defs.cs, test-only), four FPV Teams one every 3 s");
            Console.WriteLine("from 1,200 m. Each sweep varies one of the mount's own numbers and");
            Console.WriteLine("holds the other fixed - see the comment on this method for why this is");
            Console.WriteLine("no longer run on the flat control world.");
            PrintWorldConfig(StandardBorderMetres, 1, 2);

            const int trials = 100;

            Console.WriteLine();
            Console.WriteLine("  (a) the barrel, with the mount's 600 m optics held fixed");
            Console.WriteLine();
            Console.WriteLine("  barrel   seconds under fire   arrived   mount survives");
            Console.WriteLine("  " + new string('-', 60));

            int[] ranges = { 550, 450, 350, 250, 180, 120, 85 };
            for (int r = 0; r < ranges.Length; r++)
            {
                int arrivedTotal = 0, survived = 0;
                for (int trial = 0; trial < trials; trial++)
                {
                    int arrived;
                    if (!RunRangeSweep(4, (ulong)(trial + 1), out arrived,
                                       MountTweak.Range(F(ranges[r])))) survived++;
                    arrivedTotal += arrived;
                }
                // A drone crosses the gun's reach at 22 m/s.
                double exposure = ranges[r] / 22.0;
                Console.WriteLine(string.Format("  {0,6}   {1,18}   {2,7}   {3,14}{4}",
                    ranges[r] + " m",
                    exposure.ToString("0.0") + " s",
                    (arrivedTotal / (double)trials).ToString("0.0"),
                    (survived * 100 / trials) + "%",
                    ranges[r] == 85 ? "   <- the deployed barrel" : ""));
            }

            Console.WriteLine();
            Console.WriteLine("  (b) the optics, with the barrel held at 550 m");
            Console.WriteLine();
            Console.WriteLine("  optics   finds a quad at      arrived   mount survives");
            Console.WriteLine("  " + new string('-', 60));

            int[] optics = { 600, 450, 300, 200, 120, 60 };
            for (int o = 0; o < optics.Length; o++)
            {
                int arrivedTotal = 0, survived = 0;
                for (int trial = 0; trial < trials; trial++)
                {
                    int arrived;
                    MountTweak t = MountTweak.Range(F(550));
                    t.Optical = F(optics[o]);
                    if (!RunRangeSweep(4, (ulong)(trial + 1), out arrived, t)) survived++;
                    arrivedTotal += arrived;
                }
                Console.WriteLine(string.Format("  {0,6}   {1,15}   {2,10}   {3,14}{4}",
                    optics[o] + " m",
                    OpticalReachVsFPV(optics[o]).RoundToInt() + " m",
                    (arrivedTotal / (double)trials).ToString("0.0"),
                    (survived * 100 / trials) + "%",
                    optics[o] == 600 ? "   <- the deployed optics" : ""));
            }

            Console.WriteLine();
            Console.WriteLine("  (c) the flat control world, kept so the old conclusions stay checkable");
            Console.WriteLine();
            Console.WriteLine("  world                              arrived   mount survives");
            Console.WriteLine("  " + new string('-', 60));
            {
                int arrivedTotal = 0, survived = 0;
                for (int trial = 0; trial < trials; trial++)
                {
                    int arrived;
                    World w = MakeFlatControlWorld(2400, 1600, 256, 32, (ulong)(trial + 1), 2, 0);
                    if (!RunAssaultScenario(w, "Test Long Mount", MountTweak.Range(F(550)),
                                            4, F(1200), Sec(3), out arrived)) survived++;
                    arrivedTotal += arrived;
                }
                Console.WriteLine(string.Format("  {0,-33}  {1,7}   {2,14}",
                    "flat, clear, firm, ownerless",
                    (arrivedTotal / (double)trials).ToString("0.0"),
                    (survived * 100 / trials) + "%"));
            }
            Console.WriteLine();
            Console.WriteLine("  That row is the same 550 m mount as the top of (a), on the world every");
            Console.WriteLine("  experiment ran on before AUDIT F34 was fixed. FINDINGS 33 recorded the");
            Console.WriteLine("  two worlds as identical in every cell. They are not any more: the flat");
            Console.WriteLine("  world has no territory owner, so every square metre of it is denied");
            Console.WriteLine("  ground, every FPV Team's dead reckoning crosses the miss radius on the");
            Console.WriteLine("  way in, and the warheads land on empty grass. Nothing about the mount");
            Console.WriteLine("  changed. The world did, and something finally reads it.");
            Console.WriteLine();
            Console.WriteLine("  Read the two 'mount survives' columns against each other. They are the");
            Console.WriteLine("  same mount, the same attack and the same seeds; the only difference is");
            Console.WriteLine("  which of its two numbers is being spent. Whichever column moves is the");
            Console.WriteLine("  one that is binding, and that is the number a player's upgrade should");
            Console.WriteLine("  be buying.");
            Console.WriteLine();
            Console.WriteLine("  Both of them move, which is not what the old flat sweep implied and is");
            Console.WriteLine("  not quite what FINDINGS 31 says either. The barrel matters a great");
            Console.WriteLine("  deal here and mattered not at all before, and the difference is only");
            Console.WriteLine("  the spacing: reach buys time, and time is worth nothing against a");
            Console.WriteLine("  flight that arrives in one instant and everything against one that");
            Console.WriteLine("  keeps arriving. 'Barrel length barely matters' is true of a wave and");
            Console.WriteLine("  false of a stream.");
            Console.WriteLine();
            Console.WriteLine("  The optics column is not monotonic and that is the more interesting");
            Console.WriteLine("  half. A mount that finds the drone at 64 m does better than one that");
            Console.WriteLine("  finds it at 127, because a five-round belt spent at the rim of the");
            Console.WriteLine("  envelope is a belt spent at the worst hit chance the falloff offers.");
            Console.WriteLine("  Seeing further is only an advantage to a mount that can afford to");
            Console.WriteLine("  wait, and this one cannot.");
        }

        /// <summary>
        /// What a 120-degree sweeping head of the given nominal reach actually
        /// finds a quadcopter at, after the aperture law and the signature table
        /// have had their say. Printed beside the sweep so the optics column is
        /// readable as a distance rather than as a catalogue figure.
        /// </summary>
        static Fix OpticalReachVsFPV(int nominalOptical)
        {
            World w = MakeRealisticWorld(2048, 2048, 64, 4, 1, 2, 0, 1100, 1, 2);
            EntityHandle gun = w.Spawn(Catalog.IdOf("Test Long Mount"), 1, P(1000, 1000));
            w.Entities.Sensor[gun.Index].Optical = F(nominalOptical);
            EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(1200, 1000));
            w.Step();
            return w.DetectionRangeFor(gun.Index, drone.Index, SensorChannel.Optical);
        }

        /// <summary>
        /// How much launching from closer helps. This is what a forward position
        /// buys you, and it is the lever a player actually has.
        /// </summary>
        static void ApproachExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("APPROACH - eight drones, varying where they launch from");
            Console.WriteLine("gun at 1,650 m reaching its real 85 m (FINDINGS 2), so its edge is at 1,565 m");
            Console.WriteLine("(the launch pads below were chosen to bracket the old, wrong 550 m edge at");
            Console.WriteLine(" 1,100 m; they do not resolve the much closer real edge - see notes below)");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  launch at   arrived   gun killed");
            Console.WriteLine("  " + new string('-', 38));

            int[] pads = { 400, 800, 1100, 1300, 1500, 1600 };
            for (int p = 0; p < pads.Length; p++)
            {
                int arrivedTotal = 0, gunKilled = 0;
                const int trials = 60;

                for (int trial = 0; trial < trials; trial++)
                {
                    int arrived;
                    bool killed = RunAssault(8, F(pads[p]), (ulong)(trial + 1), out arrived);
                    arrivedTotal += arrived;
                    if (killed) gunKilled++;
                }

                Console.WriteLine(string.Format("  {0,9}   {1,7}   {2,10}",
                    pads[p] + " m",
                    (arrivedTotal / (double)trials).ToString("0.0"),
                    (gunKilled * 100 / trials) + "%"));
            }
            Console.WriteLine();
            Console.WriteLine("  Launching from inside the gun's reach means the drones are under");
            Console.WriteLine("  fire from the moment they exist, but for far less time. Only the last");
            Console.WriteLine("  row or two above actually land inside the real 85 m envelope - a");
            Console.WriteLine("  pad set re-tuned around that edge would resolve this transition better.");
        }

        /// <summary>
        /// The same assault, in daylight and in darkness.
        ///
        /// A gun mount finds its targets optically. After dark that reach collapses
        /// (SimConstants.NightOpticalDetectionScale), so it cannot begin shooting
        /// until the drones are far closer - and the seconds it does not get are
        /// the seconds the drones needed. This is the counter the subject matter
        /// actually uses, and it costs the attacker nothing but patience.
        /// </summary>
        static void NightExperiment()
        {
            Fix dayReach = GunOpticalReachVsFPV(false);
            Fix nightReach = GunOpticalReachVsFPV(true);

            Console.WriteLine();
            Console.WriteLine("DARKNESS - the same assault by day and by night");
            Console.WriteLine(string.Format("gun's own best sensor reaches {0} m by day, {1} m after dark",
                dayReach.RoundToInt(), nightReach.RoundToInt()));
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  drones   by day             at night");
            Console.WriteLine("           arrived  killed    arrived  killed");
            Console.WriteLine("  " + new string('-', 52));

            int[] counts = { 3, 5, 8, 12 };
            for (int c = 0; c < counts.Length; c++)
            {
                int n = counts[c];
                int dayArrived = 0, dayKilled = 0, nightArrived = 0, nightKilled = 0;
                const int trials = 60;

                for (int trial = 0; trial < trials; trial++)
                {
                    int a;
                    if (RunAssault(n, F(1200), (ulong)(trial + 1), out a, 0)) dayKilled++;
                    dayArrived += a;
                    // Well into the night phase of the cycle.
                    if (RunAssault(n, F(1200), (ulong)(trial + 1), out a, 8000)) nightKilled++;
                    nightArrived += a;
                }

                Console.WriteLine(string.Format("  {0,6}   {1,7}  {2,6}    {3,7}  {4,6}",
                    n,
                    (dayArrived / (double)trials).ToString("0.0"),
                    (dayKilled * 100 / trials) + "%",
                    (nightArrived / (double)trials).ToString("0.0"),
                    (nightKilled * 100 / trials) + "%"));
            }
            Console.WriteLine();
            Console.WriteLine("  Nothing about the gun changed. Only whether it could see.");
        }

        /// <summary>The gun's best detection channel against an FPV Team, day or night.</summary>
        static Fix GunOpticalReachVsFPV(bool night)
        {
            World w = MakeRealisticWorld(2400, 1600, 8, 1, 1, 2, night ? 8000 : 0,
                StandardBorderMetres, 1, 2);
            EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(1650, 780));
            EntityHandle target = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(1200, 780));
            w.Step();
            return w.BestDetectionRange(gun.Index, target.Index);
        }

        /// <summary>
        /// A minefield laid across a supply road, against the traffic that has to
        /// use it.
        ///
        /// Mines are the least interesting weapon in the game to operate and the
        /// hardest to argue with. They need no crew, no link and no pilot, cannot
        /// be jammed or shot down, and are still there an hour later. They are also
        /// indiscriminate: the field below is armed against whoever drives into it
        /// first, and the experiment reports both.
        /// </summary>
        static void MineExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("MINES - a field laid across a supply road");
            Console.WriteLine("four mines from one heavy drone, 600 damage each, into the underside");
            PrintWorldConfig(900, 2, 1); // the convoy's own rear (west) vs. the ambush ground it drives into (east)
            Console.WriteLine();
            Console.WriteLine("  vehicle             per mine   survives a mine   field stops");
            Console.WriteLine("  " + new string('-', 60));

            string[] vehicles = { "Supply Truck", "Logistics UGV", "IFV", "Main Tank" };
            for (int v = 0; v < vehicles.Length; v++)
            {
                UnitDef def = Catalog.ByName(vehicles[v]);
                Fix mult = Catalog.DamageMultiplier(DamageType.Shaped, def.Armour, true);
                Fix perMine = F(600) * mult;
                int survives = 0;
                Fix hp = def.Hp;
                while (hp.Raw > 0) { hp -= perMine; survives++; }

                int killed = RunMineField(vehicles[v]);

                Console.WriteLine(string.Format("  {0,-18}  {1,8}   {2,15}   {3}",
                    vehicles[v],
                    perMine.RoundToInt(),
                    (survives - 1) + " of 4",
                    killed > 0 ? "yes, after " + killed + " mine(s)" : "no"));
            }
            Console.WriteLine();
            Console.WriteLine("  A mine costs nothing to keep there. The heavy drone that laid it");
            Console.WriteLine("  flew home and can do it again tomorrow night.");
        }

        /// <summary>Drive one vehicle down a road through a four-mine field.</summary>
        static int RunMineField(string vehicleName)
        {
            World w = MakeRealisticWorld(2400, 1600, 128, 8, 4242, 2, 8000, 900, 2, 1);

            // The robot in this list is radio-controlled, so without something to
            // talk to it stops of its own accord and the experiment measures the
            // wrong thing entirely.
            w.Spawn(Catalog.IdOf("Command Post"), 2, P(700, 900));

            EntityHandle vehicle = w.Spawn(Catalog.IdOf(vehicleName), 2, P(600, 780));
            for (int m = 0; m < 4; m++)
                w.SpawnMine(1, P(1000 + m * SimConstants.MineSpacingMetres, 780), F(600));

            w.Enqueue(Command.MoveTo(2, vehicle, P(1800, 780)));

            int minesTriggered = 0;
            for (int tick = 0; tick < 300 * SimConstants.TicksPerSecond; tick++)
            {
                w.Step();
                for (int e = 0; e < w.Events.Count; e++)
                    if (w.Events[e].Kind == SimEventKind.MineDetonated) minesTriggered++;
                if (!w.Entities.IsAlive(vehicle)) return minesTriggered;
                if (w.Entities.Position[vehicle.Index].X > F(1700)) return 0;
            }
            return 0;
        }

        /// <summary>
        /// The same turret with different sensors fitted, by day and by night.
        ///
        /// A gun's reach is set by its gun. Its *envelope* is set by whichever
        /// sensor finds the target first, and against a small drone that is a much
        /// shorter distance than the barrel can throw a round. Which sensors are
        /// fitted therefore matters more than the gun does.
        /// </summary>
        static void SensorMixExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("SENSORS - what a turret can find, and from how far");
            Console.WriteLine("the gun's own kill range is 85 m (FINDINGS 2); these are how far its");
            Console.WriteLine("sensors could find a target if the barrel could reach that far too -");
            Console.WriteLine("the sensor suites below are hypothetical fits, not the shipped Gun Mount's");
            PrintWorldConfig(1100, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  fitted with              vs quad (day)  vs quad (night)  vs tank (day)");
            Console.WriteLine("  " + new string('-', 74));

            string[] names = { "optics only", "acoustic only", "optics + acoustic",
                               "optics + thermal", "optics + acoustic + thermal" };
            Fix[][] suites = {
                new Fix[] { F(600), Fix.Zero, Fix.Zero },
                new Fix[] { Fix.Zero, Fix.Zero, F(400) },
                new Fix[] { F(600), Fix.Zero, F(400) },
                new Fix[] { F(600), F(450), Fix.Zero },
                new Fix[] { F(600), F(450), F(400) },
            };

            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine(string.Format("  {0,-24}  {1,12}  {2,15}  {3,13}",
                    names[i],
                    Reach(suites[i], "FPV Team", false).RoundToInt() + " m",
                    Reach(suites[i], "FPV Team", true).RoundToInt() + " m",
                    Reach(suites[i], "Main Tank", false).RoundToInt() + " m"));
            }
            Console.WriteLine();
            Console.WriteLine("  Cameras alone leave a turret nearly blind after dark. Microphones");
            Console.WriteLine("  do not care about the time, and against a quadcopter they are the");
            Console.WriteLine("  best sensor on the list at any hour. Thermal buys back the night");
            Console.WriteLine("  against vehicles, and rather little against a small drone, because");
            Console.WriteLine("  a small drone is not very hot.");
        }

        static Fix Reach(Fix[] suite, string targetName, bool night)
        {
            World w = MakeRealisticWorld(2048, 2048, 64, 4, 1, 2, night ? 8000 : 0, 1100, 1, 2);

            EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
            w.Entities.Sensor[gun.Index] = new SensorSuite
            {
                Optical = suite[0], Thermal = suite[1], Acoustic = suite[2],
                Radar = Fix.Zero, Esm = Fix.Zero, Quality = 60
            };
            EntityHandle target = w.Spawn(Catalog.IdOf(targetName), 2, P(1200, 1000));
            w.Step();
            return w.BestDetectionRange(gun.Index, target.Index);
        }

        /// <summary>
        /// More turrets covering the same ground, against flights that arrive
        /// together and flights that arrive in a trickle.
        ///
        /// <para><b>What it used to measure, and why it could not move.</b> For
        /// each turret count it walked a ladder {8, 12, 16, 24, 32, 48, 64} of
        /// simultaneous drones and printed the first rung that won 18 trials in
        /// 20. Eight drones beat four turrets, so every row printed the ladder's
        /// bottom rung - 8, 8, 8, 8 - on every build in this project's history
        /// (FINDINGS 32). A threshold search whose floor already wins reports the
        /// floor, and the floor is a property of the ladder, not of the game.</para>
        ///
        /// <para><b>What it measures now.</b> The share of trials the attack
        /// wins, at four forces small enough that the answer is not settled
        /// before the run starts, crossed with two arrival patterns - and, beside
        /// it, how many rounds each mount in the stack actually fired. A win rate
        /// is a continuous quantity that moves when anything about the engagement
        /// moves; the rounds column is what makes the win rate explicable rather
        /// than just readable, and it is the column that shows *why* a stack does
        /// or does not pay.</para>
        ///
        /// <para><b>Why the arrival axis is the point.</b> A second mount can only
        /// pay for itself if there is a second engagement to service, and a
        /// simultaneous wave offers exactly one (FINDINGS 31). If stacking matters
        /// anywhere it has to matter here; if it does not matter here either then
        /// FINDINGS 13's structural argument is about something this game does not
        /// contain.</para>
        /// </summary>
        static void StackingExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("STACKING - more turrets covering the same approach");
            Console.WriteLine("a cluster of mounts all covering one defended point, 450 Materiel each");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  each cell: share of 40 trials in which the attack destroyed the lead");
            Console.WriteLine("  mount. FPV Teams, 200 Materiel each, from a pad at 1,200 m. The last");
            Console.WriteLine("  column is rounds fired per trial by each mount, lead first, averaged");
            Console.WriteLine("  over every trial in the row.");

            int[] forces = { 3, 4, 5, 6 };
            int[] spacings = { 0, Sec(3) };
            string[] patterns = { "launched together", "one drone every 3 s" };

            for (int sp = 0; sp < spacings.Length; sp++)
            {
                Console.WriteLine();
                Console.WriteLine("  " + patterns[sp] + ":");
                Console.WriteLine();
                Console.Write("  turrets ");
                for (int f = 0; f < forces.Length; f++)
                    Console.Write(string.Format("{0,9}", forces[f] + " drones"));
                Console.WriteLine("   needed   rounds fired per trial");
                Console.WriteLine("  " + new string('-', 88));

                for (int guns = 1; guns <= 4; guns++)
                {
                    Console.Write(string.Format("  {0,7} ", guns));
                    int needed = -1;
                    int[] shots = new int[4];
                    int trialsInRow = 0;
                    for (int f = 0; f < forces.Length; f++)
                    {
                        int wins = 0;
                        const int trials = 40;
                        for (int trial = 0; trial < trials; trial++)
                            if (RunStacked(guns, forces[f], (ulong)(trial + 1), spacings[sp], shots)) wins++;
                        trialsInRow += trials;
                        int pct = wins * 100 / trials;
                        if (needed < 0 && pct >= 90) needed = forces[f];
                        Console.Write(string.Format("{0,9}", pct + "%"));
                    }

                    string rounds = "";
                    for (int g = 0; g < guns; g++)
                        rounds += (g > 0 ? " | " : "")
                                + (shots[g] / (double)trialsInRow).ToString("0.00");
                    Console.WriteLine(string.Format("   {0,6}   {1}",
                        needed > 0 ? needed.ToString() : "over 6", rounds));
                }
            }

            Console.WriteLine();
            Console.WriteLine("  A mount is bought to service an engagement. A flight that arrives in");
            Console.WriteLine("  one instant offers exactly one, and the rounds column says what that");
            Console.WriteLine("  costs: against a wave the third and fourth mounts in a cluster fire");
            Console.WriteLine("  nothing at all, because by the time a drone is 70 m from them it is");
            Console.WriteLine("  already on top of the mount it was sent at. Against a stream the same");
            Console.WriteLine("  four mounts all shoot, the lead one empties its five-round belt in");
            Console.WriteLine("  nearly every trial, and the stack starts to be worth its money.");
        }

        /// <summary>
        /// One attempt against a stack of mounts. Wins if the lead turret dies.
        /// <paramref name="shotsByMount"/> accumulates rounds fired, read from
        /// each mount's own EngagementsRemaining rather than inferred from the
        /// outcome - the belt is the thing FINDINGS 31 found unreachable, so an
        /// experiment about stacking should say out loud whether it is reached.
        /// </summary>
        static bool RunStacked(int gunCount, int droneCount, ulong seed, int spacingTicks,
                               int[] shotsByMount)
        {
            World w = MakeRealisticWorld(2400, 1600, 512, 32, seed, 2, 0, StandardBorderMetres, 1, 2);
            BuildAttackerRear(w);

            // A fixed cluster, so the lead mount - the one the attack is aimed at
            // and the one a win is measured on - sits at the same place whatever
            // the stack size, and every mount added to it has the defended point
            // inside its own 85 m envelope. The old layout spread mounts along a
            // line centred on the stack, which moved the lead as the count changed
            // and put the third mount 120 m from the engagement, out of reach - so
            // two turrets and three turrets measured identically to the digit for
            // a reason that had nothing to do with stacking.
            //
            // Every supporting mount is beside or behind the lead, never in front
            // of it. A mount 40 m forward of the one being defended measured as
            // worth more than two mounts beside it, because it opens fire first.
            // That is a real and interesting fact about siting, and a confound in
            // a table whose one variable is supposed to be how many mounts there
            // are.
            Fix2[] positions = { P(1650, 780), P(1695, 735), P(1695, 825), P(1720, 780) };

            EntityHandle first = EntityHandle.None;
            int[] gunIdx = new int[4];
            int[] lastBelt = new int[4];
            for (int g = 0; g < gunCount; g++)
            {
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 2, positions[g]);
                gunIdx[g] = gun.Index;
                lastBelt[g] = -1;
                if (g == 0) first = gun;
            }

            List<Arrival> plan = PlanFlight(1, Catalog.IdOf("FPV Team"), F(1200), F(780), 12,
                                            first, droneCount, spacingTicks, 0);
            int next = 0;
            int lastLaunchTick = plan[plan.Count - 1].Tick;

            for (int tick = 0; tick < 200 * SimConstants.TicksPerSecond; tick++)
            {
                IssueDue(w, plan, ref next, tick);
                w.Step();

                for (int g = 0; g < gunCount; g++)
                {
                    if (!w.Entities.IsSlotAlive(gunIdx[g])) continue;
                    int belt = w.Entities.Weapon[gunIdx[g]].EngagementsRemaining;
                    // A reload refills the belt, so only a fall counts as a round.
                    if (lastBelt[g] >= 0 && belt < lastBelt[g]) shotsByMount[g] += lastBelt[g] - belt;
                    lastBelt[g] = belt;
                }

                if (!w.Entities.IsAlive(first)) return true;
                if (next >= plan.Count && tick > lastLaunchTick + 8
                    && CountFriendlyDronesAirborne(w) == 0) return false;
            }
            return false;
        }

        /// <summary>
        /// The same flight, split between altitudes, against two different mounts.
        ///
        /// <para><b>What it used to measure, and why it could not move.</b> Six
        /// Multirole Quads, launched together, against a Gun Mount - with and
        /// without a Radar Mast parked beside it. Every one of the ten rows read
        /// 6.0 arrived and 100% killed on every build in this project's history
        /// (FINDINGS 32), and two separate ceilings held it there.</para>
        ///
        /// <para>The first is that <c>CanReachHigh</c> is false on the Gun Mount,
        /// and <c>CombatSystem.EffectiveReach</c> returns zero range against a
        /// High target for a weapon that cannot reach high. Against this mount,
        /// height is not cover that has to be paid for - it is immunity. Any mix
        /// containing a high drone therefore contains drones the defence is
        /// incapable of engaging, and the experiment was asking which of five
        /// winning hands wins. The second is that the radar arm changed nothing
        /// because a Radar Mast carries no weapon: it could improve a firing
        /// solution the Gun Mount is not allowed to take.</para>
        ///
        /// <para><b>What it measures now.</b> The same splits against the mount
        /// that cannot reach high and against the Autocannon Mount, which can -
        /// and which until now no balance experiment had ever spawned (FINDINGS
        /// 31). Against the first, the table is a statement about the roster:
        /// there is no answer to the high band in this game, and that is a design
        /// fact worth printing rather than a measurement. Against the second the
        /// question FINDINGS 18 was actually asking becomes askable, because a
        /// mount that can engage both bands is a mount that can be made to choose
        /// between them.</para>
        ///
        /// <para>Arrivals are spaced. The mechanism that is supposed to make
        /// splitting pay is the band-change penalty in
        /// <c>CombatSystem.SlewTicks</c>, which is only ever charged when a mount
        /// re-lays from one engagement to the next - so an experiment that gives
        /// it one engagement cannot see it at all, whatever the constant is set
        /// to. This is the same error FINDINGS 18 records making twice already.</para>
        /// </summary>
        static void VerticalExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("VERTICAL - three Multirole Quads, split between altitudes");
            Console.WriteLine("arriving one every 3 s, because a band change is only charged between");
            Console.WriteLine("engagements and a simultaneous wave only ever offers one");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("                          Gun Mount (85 m,           Autocannon Mount");
            Console.WriteLine("                          cannot reach high)         (280 m, reaches high)");
            Console.WriteLine("  attack                  quads lost   mount killed  quads lost   mount killed");
            Console.WriteLine("  " + new string('-', 78));

            int[][] splits = {
                new int[] {3, 0}, new int[] {2, 1}, new int[] {1, 2}, new int[] {0, 3}
            };
            string[] labels = {
                "all low", "two low, one high", "one low, two high", "all high"
            };
            string[] mounts = { "Gun Mount", "Autocannon Mount" };

            for (int i = 0; i < splits.Length; i++)
            {
                string row = string.Format("  {0,-22}", labels[i]);
                for (int m = 0; m < mounts.Length; m++)
                {
                    int lostTotal = 0, killed = 0;
                    const int trials = 40;
                    for (int trial = 0; trial < trials; trial++)
                    {
                        int lost;
                        if (RunSplitAssault(splits[i][0], splits[i][1], (ulong)(trial + 1),
                                            mounts[m], Sec(3), out lost)) killed++;
                        lostTotal += lost;
                    }
                    row += string.Format("  {0,10}   {1,12}",
                        (lostTotal / (double)trials).ToString("0.00"),
                        (killed * 100 / trials) + "%");
                }
                Console.WriteLine(row);
            }

            Console.WriteLine();
            Console.WriteLine("  The left pair is not a tactic and should not be read as one. A Gun");
            Console.WriteLine("  Mount cannot engage the high band at all, so a drone sent there is");
            Console.WriteLine("  not evading the defence, it is outside it - which is why one high");
            Console.WriteLine("  drone in the flight takes the mount's score to exactly zero and");
            Console.WriteLine("  keeps it there. Its 100% column is a ceiling and carries no");
            Console.WriteLine("  information; the quads-lost column beside it is the live one.");
            Console.WriteLine();
            Console.WriteLine("  The right pair is the question FINDINGS 18 was asking. The Autocannon");
            Console.WriteLine("  reaches both bands, pays 20 ticks every time it re-lays across them,");
            Console.WriteLine("  and loses 40% of its range and 30% of its hit chance shooting");
            Console.WriteLine("  upward. Against it, splitting is worth something and committing");
            Console.WriteLine("  everything high is worth slightly less than splitting - which is the");
            Console.WriteLine("  shape FINDINGS 18 guessed at and could not measure, arrived at");
            Console.WriteLine("  against a defence that is allowed to answer.");
        }

        /// <summary>
        /// One attempt. The flight is launched on a spacing and each airframe is
        /// sent to its band as it appears, rather than the whole flight being
        /// spawned and then sorted: with staggered launches there is no single
        /// tick at which every drone exists to be sorted.
        /// </summary>
        static bool RunSplitAssault(int low, int high, ulong seed, string mountDefName,
                                    int spacingTicks, out int quadsLost)
        {
            World w = MakeRealisticWorld(2400, 1600, 512, 32, seed, 2, 0, StandardBorderMetres, 1, 2);
            BuildAttackerRear(w);

            EntityHandle gun = w.Spawn(Catalog.IdOf(mountDefName), 2, P(1650, 780));

            int total = low + high;
            // Spread the high drones through the order rather than sending the
            // low half first: a flight that goes low then high is a sequential
            // attack, which is a different tactic from a split one.
            bool[] goesHigh = new bool[total];
            for (int k = 0; k < high; k++) goesHigh[(k * total) / high] = true;

            List<Arrival> plan = PlanFlight(1, Catalog.IdOf("Multirole Quad"), F(1200), F(780), 12,
                                            gun, total, spacingTicks, 0);
            int next = 0;
            int lastLaunchTick = plan[plan.Count - 1].Tick;

            bool[] sorted = new bool[w.Entities.Capacity];
            int sortedCount = 0;

            bool[] airborneOnce = new bool[w.Entities.Capacity];
            bool[] counted = new bool[w.Entities.Capacity];
            int lost = 0;

            for (int tick = 0; tick < 200 * SimConstants.TicksPerSecond; tick++)
            {
                IssueDue(w, plan, ref next, tick);
                w.Step();

                // Newly airborne airframes, in the order they were launched -
                // CommandBuffer.Execute applies launches in enqueue order and
                // World.Spawn hands out ascending slots, so index order is launch
                // order for the length of one of these trials.
                for (int i = 1; i < w.Entities.HighWater && sortedCount < total; i++)
                {
                    if (sorted[i] || !w.Entities.IsSlotAlive(i)) continue;
                    if (w.Entities.Team[i] != 1) continue;
                    if (!w.Entities.Has(i, ComponentMask.Sortie)) continue;
                    sorted[i] = true;
                    if (goesHigh[sortedCount])
                        w.Enqueue(Command.SetAltitude(1, w.Entities.HandleAt(i), Layer.High));
                    sortedCount++;
                }

                // What the defence actually achieved, counted as airframes it
                // removed. "Arrived" was the old measure and it is unusable once
                // the mount can die mid-flight: the trial stops at that instant,
                // so a defence that dies early scores well on drones-not-arrived
                // for the same reason it lost.
                for (int i = 1; i < w.Entities.HighWater; i++)
                {
                    if (!w.Entities.IsSlotAlive(i)) continue;
                    if (w.Entities.Team[i] != 1) continue;
                    if (w.Entities.Has(i, ComponentMask.Sortie)) airborneOnce[i] = true;
                }
                for (int i = 1; i < w.Entities.HighWater; i++)
                    if (airborneOnce[i] && !counted[i] && !w.Entities.IsSlotAlive(i))
                    { counted[i] = true; lost++; }

                if (!w.Entities.IsAlive(gun)) { quadsLost = lost; return true; }

                if (next >= plan.Count && tick > lastLaunchTick + 8
                    && CountFriendlyDronesAirborne(w) == 0) break;
            }
            quadsLost = lost;
            return false;
        }

        /// <summary>
        /// Cheap decoys flown alongside a real strike, against a defence that can
        /// actually engage them.
        ///
        /// <para><b>What it used to measure, and why it could not move.</b> Three
        /// budget-equal packages of Heavy Strike Drones and Decoy Drones against
        /// one Gun Mount and one Radar Mast, with a right-hand control column that
        /// re-ran each package with the decoy's radar cross-section cut from 92 to
        /// 52. Every cell of both columns read every real drone through, on every
        /// build in this project's history, unchanged to the hundredth (FINDINGS
        /// 25, 32).</para>
        ///
        /// <para>Both halves were dead, for two different reasons, and neither is
        /// "the numbers happened to agree".</para>
        ///
        /// <para>The <i>package</i> column was dead because a Heavy Strike Drone
        /// and a Decoy Drone both have <c>Layer = High</c> and a Gun Mount has
        /// <c>CanReachHigh = false</c>, so <c>CombatSystem.EffectiveReach</c>
        /// returns zero range against every airframe in the experiment. The
        /// defence in this experiment has never been able to fire a single round
        /// at anything in it. "Every real drone gets through" was not a
        /// measurement of decoy escort; it was a restatement of the roster.</para>
        ///
        /// <para>The <i>reflector</i> column was dead for a reason that survives
        /// fixing the first, which is why it is not fixed below but retired.
        /// Radar cross-section enters the simulation at exactly one place:
        /// <c>World.ComputeDetection</c>, where it sets how far a radar reaches.
        /// Detection then gates exactly two things - whether a weapon may engage
        /// (<c>CombatSystem.CanEngage</c>) and how well an interceptor is cued
        /// (<c>CueMultiplier</c>) - and both of those are asked only about a
        /// target already inside a weapon envelope, which in this game is at most
        /// 320 m. Every sensor in the roster finds every airframe in the roster at
        /// 320 m. So a decoy that is detected at 2,373 m instead of 750 m cannot
        /// change any outcome, at any package mix, against any defence that can be
        /// assembled from the current catalogue - not as an empirical result but
        /// as a property of where the number is read. FINDINGS 25 reached that
        /// conclusion from arithmetic and was right; keeping a control column that
        /// can only ever confirm it is keeping an instrument that reads zero
        /// because it is not plugged in.</para>
        ///
        /// <para><b>What it measures now.</b> The same budget-equal packages
        /// against an Interceptor Battery - the unit the research says a decoy
        /// exists to make somebody spend - defending a Command Post behind it. The
        /// battery reaches the high band, carries twelve engagements and a
        /// twenty-second reload, and cycles every three seconds, so it is a
        /// defence that can be saturated at a realistic rate. The control arm is
        /// the same money spent entirely on warheads. That is a control that
        /// varies the thing under test: whether buying decoys instead of warheads
        /// puts more warheads on the target.</para>
        /// </summary>
        static void DecoyEscortExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("DECOY ESCORT - about 2,500 Materiel of strike package against one battery");
            Console.WriteLine("heavy strike drone 800, decoy drone 130; the battery defends the radar");
            Console.WriteLine("mast 100 m behind it, and the package is aimed at the mast (two warheads");
            Console.WriteLine("on target destroy it)");
            Console.WriteLine("arrivals spaced 2 s apart, so the battery's belt and its cooldown both bind");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  The first row is the control: the same money, no decoys. The question");
            Console.WriteLine("  is whether any row below it puts more warheads on the target than it");
            Console.WriteLine("  does. There is no reflector column any more - see the comment on this");
            Console.WriteLine("  method for why that control could not move and was not worth keeping.");
            Console.WriteLine();
            Console.WriteLine("  package                cost   warheads on target   mast killed   decoys shot   rounds");
            Console.WriteLine("  " + new string('-', 96));

            int strikeCost = Catalog.ByName("Heavy Strike Drone").CostMateriel;
            int decoyCost = Catalog.ByName("Decoy Drone").CostMateriel;

            int[][] mixes = {
                new int[] {3, 0}, new int[] {3, 1}, new int[] {2, 6}, new int[] {1, 13}
            };
            for (int i = 0; i < mixes.Length; i++)
            {
                int real = mixes[i][0], decoys = mixes[i][1];
                const int trials = 40;
                int through = 0, postKilled = 0, decoysShot = 0, rounds = 0;
                for (int trial = 0; trial < trials; trial++)
                {
                    int t, ds, rd;
                    if (RunDecoyStrike(real, decoys, (ulong)(trial + 1), out t, out ds, out rd))
                        postKilled++;
                    through += t; decoysShot += ds; rounds += rd;
                }

                Console.WriteLine(string.Format("  {0,-20}  {1,5}   {2,18}   {3,11}   {4,11}   {5,6}",
                    real + " real + " + decoys + " decoy",
                    real * strikeCost + decoys * decoyCost,
                    (through / (double)trials).ToString("0.00") + " of " + real,
                    (postKilled * 100 / trials) + "%",
                    (decoysShot / (double)trials).ToString("0.0") + " of " + decoys,
                    (rounds / (double)trials).ToString("0.0")));
            }

            Console.WriteLine();
            Console.WriteLine("  Nothing special-cases a decoy here. A defence picks targets by how");
            Console.WriteLine("  much of one it can remove per shot, and a decoy dies to one round");
            Console.WriteLine("  just as a strike drone does - so it is an equally good thing to shoot");
            Console.WriteLine("  at, which is exactly the product being sold. The rounds column is");
            Console.WriteLine("  whether the defence was made to spend it.");
            Console.WriteLine();
            Console.WriteLine("  The bottom row is the other end of the trade and it is not a failure");
            Console.WriteLine("  of the mechanic: two warheads are needed to take the mast and that");
            Console.WriteLine("  package only buys one, so it can saturate the battery completely and");
            Console.WriteLine("  still not finish the job. Spending everything on escorts is the same");
            Console.WriteLine("  mistake as spending nothing on them.");
        }

        /// <summary>
        /// One strike. Returns whether the command post died, and reports how many
        /// warheads reached it, how many decoys the defence removed, and how many
        /// rounds the battery spent.
        /// </summary>
        static bool RunDecoyStrike(int real, int decoys, ulong seed,
                                   out int through, out int decoysShot, out int rounds)
        {
            World w = MakeRealisticWorld(2400, 1600, 512, 32, seed, 2, 0, StandardBorderMetres, 1, 2);
            BuildAttackerRear(w);

            // A Radar Mast, not a Command Post. The post has 5,000 hit points and
            // a Heavy Strike Drone removes 468 of them, so no package this budget
            // can buy could ever kill one and the "killed" column was a constant
            // zero - the same ceiling error, one column over. The mast is 900, so
            // two warheads on target take it, which makes the column a result.
            // The attacker has flown reconnaissance over the objective, so this
            // experiment measures the escort and not navigation denial. Without
            // it the answer is a different one entirely: a Heavy Strike Drone is
            // one-way and scene-matching, its NavState error crosses
            // SimConstants.MunitionMissRadiusMetres about 270 m past the border,
            // and World.ApplyDamage then puts the warhead on empty ground - so
            // every package delivers nothing whatever the escort does, and the
            // table returns to being a constant for a reason that has nothing to
            // do with decoys. That interaction is real and worth its own
            // experiment; it is a confound in this one.
            GrantHomeImagery(w, 1, true, 2400);

            EntityHandle post = w.Spawn(Catalog.IdOf("Radar Mast"), 2, P(1750, 780));
            EntityHandle battery = w.Spawn(Catalog.IdOf("Interceptor Battery"), 2, P(1650, 780));

            List<Arrival> plan = PlanFlight(1, Catalog.IdOf("Heavy Strike Drone"), F(900), F(780), 30,
                                            post, real, Sec(2), 0);
            List<Arrival> escort = PlanFlight(1, Catalog.IdOf("Decoy Drone"), F(900), F(760), 22,
                                              post, decoys, Sec(2), real);
            // Interleave by tick so the escort flies with the package rather than
            // behind it; a decoy that arrives after the warheads is not an escort.
            // Merged by hand rather than with List.Sort: both inputs are already
            // in tick order, a merge that takes the warhead first on a tie is
            // stable where List.Sort is not, and this harness's whole value rests
            // on two runs of the same seed producing the same launch order.
            plan = MergeByTick(plan, escort);

            int next = 0;
            int lastLaunchTick = plan[plan.Count - 1].Tick;

            int decoyDefId = Catalog.IdOf("Decoy Drone");

            bool[] sawDecoy = new bool[w.Entities.Capacity];
            bool[] countedDecoy = new bool[w.Entities.Capacity];
            int reached = 0, shot = 0;
            int lastBelt = -1, fired = 0;
            // Warheads *delivered*, counted as distinct falls in the mast's
            // health. Proximity was the old measure and it counts a drone that
            // arrived and missed as a warhead on target.
            Fix lastHp = w.Entities.Hp[post.Index];

            for (int tick = 0; tick < 300 * SimConstants.TicksPerSecond; tick++)
            {
                IssueDue(w, plan, ref next, tick);
                w.Step();

                if (w.Entities.IsSlotAlive(battery.Index))
                {
                    int belt = w.Entities.Weapon[battery.Index].EngagementsRemaining;
                    if (lastBelt >= 0 && belt < lastBelt) fired += lastBelt - belt;
                    lastBelt = belt;
                }

                for (int i = 1; i < w.Entities.HighWater; i++)
                {
                    if (w.Entities.IsSlotAlive(i) && w.Entities.DefId[i] == decoyDefId) sawDecoy[i] = true;
                    if (sawDecoy[i] && !countedDecoy[i] && !w.Entities.IsSlotAlive(i))
                    { countedDecoy[i] = true; shot++; }
                }

                if (w.Entities.IsSlotAlive(post.Index))
                {
                    Fix hp = w.Entities.Hp[post.Index];
                    if (hp < lastHp) reached++;
                    lastHp = hp;
                }

                if (!w.Entities.IsAlive(post))
                { through = reached; decoysShot = shot; rounds = fired; return true; }


                if (next >= plan.Count && tick > lastLaunchTick + 8
                    && CountFriendlyDronesAirborne(w) == 0) break;
            }
            through = reached; decoysShot = shot; rounds = fired;
            return false;
        }

        /// <summary>
        /// The aperture trade, and what it costs to see all the way round.
        ///
        /// A camera has a fixed number of pixels. Spend them on a narrow slice and
        /// you see a long way into very little; spread them over everything and you
        /// see a short way into all of it. A mount therefore chooses between a
        /// blind side, a short reach, a sweep that is looking elsewhere most of the
        /// time, or paying several times over for several heads.
        /// </summary>
        static void ApertureExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("APERTURE - the same 600 m camera, spread over different arcs");
            PrintWorldConfig(1100, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  arc      reach vs a quad   covered at once   heads for 360   total cost");
            Console.WriteLine("  " + new string('-', 74));

            int[] arcs = { 30, 45, 90, 120, 180, 360 };
            foreach (int arc in arcs)
            {
                Fix reach = ApertureReach(arc);
                int heads = (360 + arc - 1) / arc;
                Console.WriteLine(string.Format("  {0,4} deg  {1,15}   {2,15}   {3,13}   {4,10}",
                    arc,
                    reach.RoundToInt() + " m",
                    (arc * 100 / 360) + "%",
                    heads,
                    (heads * 450) + " MAT"));
            }

            Console.WriteLine();
            Console.WriteLine("  Buying 360 degrees at long reach costs twelve heads. Buying it with");
            Console.WriteLine("  one head costs more than half the reach. Both are worse deals than");
            Console.WriteLine("  a cheap short-range sensor line, which is the next table.");
            Console.WriteLine();
            Console.WriteLine("  COVERAGE - one good head against several poor ones, same money");
            Console.WriteLine();
            Console.WriteLine("  arrangement                          drone seen at   gaps");
            Console.WriteLine("  " + new string('-', 62));

            Console.WriteLine(string.Format("  {0,-36}  {1,13}   {2}",
                "one 30-deg staring head", ApertureReach(30).RoundToInt() + " m",
                "blind over 11/12 of the sky"));
            Console.WriteLine(string.Format("  {0,-36}  {1,13}   {2}",
                "one 120-deg head sweeping at 70 deg/s", ApertureReach(120).RoundToInt() + " m",
                "covered, but looking away 2/3 of the time"));
            Console.WriteLine(string.Format("  {0,-36}  {1,13}   {2}",
                "one 360-deg head", ApertureReach(360).RoundToInt() + " m",
                "none, and half the reach"));
            Console.WriteLine(string.Format("  {0,-36}  {1,13}   {2}",
                "three 120-deg heads, no sweep", ApertureReach(120).RoundToInt() + " m",
                "none, at three times the price"));
            Console.WriteLine();
            Console.WriteLine("  Microphones and radio listening are exempt from all of this, because");
            Console.WriteLine("  they are omnidirectional by nature - which is exactly why they are");
            Console.WriteLine("  the cheap way to know something is out there and the useless way to");
            Console.WriteLine("  know where it is.");
        }

        static Fix ApertureReach(int arcDegrees)
        {
            World w = MakeRealisticWorld(2048, 2048, 64, 4, 1, 2, 0, 1100, 1, 2);

            EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
            SensorSuite s = w.Entities.Sensor[gun.Index];
            s.DirectionalArcDegrees = arcDegrees;
            s.ScanDegreesPerSecond = 0;
            s.Facing = 0;
            w.Entities.Sensor[gun.Index] = s;

            EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(1200, 1000));
            w.Step();
            return w.DetectionRangeFor(gun.Index, drone.Index, SensorChannel.Optical);
        }

        // ------------------------------------------------------------------

        // ------------------------------------------------------------------
        // Arrival scheduling, and why half the experiments below needed it.
        //
        // Every experiment in this file used to launch its whole flight on tick
        // zero. A mount therefore got exactly one engagement window per trial:
        // it holds a track at about 70 m, lays on one drone, fires once, and the
        // rest of the flight arrives together (FINDINGS 31). In one window a
        // five-round belt cannot bind, a second turret adds nothing it would not
        // have added in the same instant, and an attack split between two
        // altitudes is one simultaneous problem rather than two problems in
        // sequence. That is the mechanism behind FINDINGS 32's three dead
        // experiments and most of the four the drift guard added to them: vary
        // only the launch spacing and the same mount goes from 0 reloads in 60
        // trials to 60 (FINDINGS 31, amended).
        //
        // A simultaneous wave is not wrong. It is one real tactic - the one a
        // player gets by pre-staging a flight and tapping once - and it is the
        // one the recorded findings were measured under, so it stays as a column
        // rather than being replaced. What was wrong is that it was the only
        // tactic any experiment could express.
        //
        // The simulation has its own staggered-departure field
        // (SortieState.EgressUntilTick, written by SortieSystem.Launch from the
        // launchIndex every call below already passes) and nothing anywhere
        // reads it - AUDIT-UNWIRED F19, still open, and not this harness's to
        // fix. So spacing is done the one honest way available from outside the
        // simulation: each launch command is enqueued on the tick it is meant to
        // be issued on, which is also exactly what a player tapping a card eight
        // times over eight seconds produces.
        struct Arrival { public int Tick; public Command Cmd; }

        /// <summary>
        /// A flight of <paramref name="count"/> airframes leaving the same pad
        /// area, one every <paramref name="spacingTicks"/> ticks. Spacing zero is
        /// the old simultaneous wave.
        /// </summary>
        static List<Arrival> PlanFlight(byte team, int defId, Fix padX, Fix padY, int spreadY,
                                        EntityHandle target, int count, int spacingTicks,
                                        int firstIndex)
        {
            List<Arrival> plan = new List<Arrival>();
            for (int i = 0; i < count; i++)
            {
                Arrival a;
                a.Tick = i * spacingTicks;
                Fix2 spot = new Fix2(padX, padY + F((i - count / 2) * spreadY));
                a.Cmd = Command.LaunchSortie(team, defId, spot, target, firstIndex + i);
                plan.Add(a);
            }
            return plan;
        }

        /// <summary>
        /// Merge two already-ordered flights into one schedule, taking from the
        /// first on a tie. A stable merge, because an unstable sort would let two
        /// runs of the same seed launch in different orders and every guarantee
        /// this harness rests on would be gone.
        /// </summary>
        static List<Arrival> MergeByTick(List<Arrival> a, List<Arrival> b)
        {
            List<Arrival> merged = new List<Arrival>();
            int i = 0, j = 0;
            while (i < a.Count || j < b.Count)
            {
                if (j >= b.Count || (i < a.Count && a[i].Tick <= b[j].Tick)) merged.Add(a[i++]);
                else merged.Add(b[j++]);
            }
            return merged;
        }

        /// <summary>Issue every launch whose tick has come.</summary>
        static void IssueDue(World w, List<Arrival> plan, ref int next, int tick)
        {
            while (next < plan.Count && plan[next].Tick <= tick) { w.Enqueue(plan[next].Cmd); next++; }
        }

        /// <summary>Seconds expressed in ticks, for readability at the call sites.</summary>
        static int Sec(int seconds) { return seconds * SimConstants.TicksPerSecond; }

        /// <summary>
        /// The attacker's rear: money, a command post, crews and two relays. Every
        /// assault experiment needs the same one, and the second relay is far
        /// enough back that the experiment measures the mount against drones and
        /// not the mount against a relay mast.
        /// </summary>
        static void BuildAttackerRear(World w)
        {
            w.Player(1).Materiel = Fix.FromInt(200000);
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(400, 780));
            for (int q = 0; q < 6; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(300 + q * 40, 900));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(1000, 780));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(1450, 1150));
        }

        static bool RunAssault(int droneCount, Fix padX, ulong seed, out int arrived)
        {
            return RunAssault(droneCount, padX, seed, out arrived, 0);
        }

        /// <summary>
        /// One attempt: launch the drones together and let it play out until either
        /// the gun is destroyed or every drone is gone. Runs the real, shipped Gun
        /// Mount on the realistic default world - see AUDIT-UNWIRED.md F33/F34.
        /// This used to overwrite the gun's range back to 550 m on every call;
        /// it no longer touches the gun at all.
        /// </summary>
        static bool RunAssault(int droneCount, Fix padX, ulong seed, out int arrived, int startTick)
        {
            World w = MakeRealisticWorld(2400, 1600, 256, 32, seed, 2, startTick,
                StandardBorderMetres, 1, 2);
            return RunAssaultScenario(w, "Gun Mount", null, droneCount, padX, out arrived);
        }

        /// <summary>
        /// GunRangeExperiment only. Sweeps one of "Test Long Mount"'s own numbers
        /// - a catalogue entry that exists solely for this purpose (Defs.cs) - and
        /// holds everything else fixed. See AUDIT-UNWIRED.md F33 and the comment
        /// on GunRangeExperiment for why it no longer runs on the flat control
        /// world.
        /// </summary>
        static bool RunRangeSweep(int droneCount, ulong seed, out int arrived, MountTweak tweak)
        {
            World w = MakeRealisticWorld(2400, 1600, 256, 32, seed, 2, 0,
                StandardBorderMetres, 1, 2);
            return RunAssaultScenario(w, "Test Long Mount", tweak, droneCount, F(1200),
                                      Sec(3), out arrived);
        }

        /// <summary>
        /// The shared engagement: build the attacker's rear, spawn one mount of
        /// the given kind for the defender, launch the drones, and play it out.
        /// </summary>
        /// <summary>
        /// A change made to the defending mount after it is spawned. Two of the
        /// experiments below sweep one of the mount's own numbers as their
        /// independent variable, which is the one legitimate reason to write a
        /// spawned unit's state from here: the unit being written is "Test Long
        /// Mount", a catalogue entry that exists for exactly this and is never
        /// shown to a player. Writing a *shipped* unit's stats behind the
        /// reader's back is AUDIT-UNWIRED F33 and is what this harness was fixed
        /// for.
        /// </summary>
        struct MountTweak
        {
            public Fix? WeaponRange;
            public Fix? Optical;
            public bool AimSensorWest;
            public static MountTweak None { get { return new MountTweak(); } }
            public static MountTweak Range(Fix r)
            { MountTweak t = new MountTweak(); t.WeaponRange = r; return t; }
            public static MountTweak Optics(Fix o)
            { MountTweak t = new MountTweak(); t.Optical = o; return t; }
            public static MountTweak Aimed()
            { MountTweak t = new MountTweak(); t.AimSensorWest = true; return t; }
        }

        static bool RunAssaultScenario(World w, string gunDefName, Fix? gunRangeOverride,
                                       int droneCount, Fix padX, out int arrived)
        {
            MountTweak t = gunRangeOverride.HasValue
                ? MountTweak.Range(gunRangeOverride.Value) : MountTweak.None;
            return RunAssaultScenario(w, gunDefName, t, droneCount, padX, 0, out arrived);
        }

        /// <summary>
        /// The shared engagement: build the attacker's rear, spawn one mount of
        /// the given kind for the defender, launch the drones on the given
        /// spacing, and play it out. Spacing zero is the simultaneous wave the
        /// recorded findings were measured under.
        /// </summary>
        static bool RunAssaultScenario(World w, string gunDefName, MountTweak tweak,
                                       int droneCount, Fix padX, int spacingTicks, out int arrived)
        {
            w.Player(1).Materiel = Fix.FromInt(100000);
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(400, 780));
            // Enough crews that the experiment measures the gun, not the crew cap.
            for (int q = 0; q < 6; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(300 + q * 40, 900));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(1000, 780));
            // Far enough back that the experiment measures the gun against
            // drones, and not the gun against a relay mast.
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(1450, 1150));

            EntityHandle gun = w.Spawn(Catalog.IdOf(gunDefName), 2, P(1650, 780));
            if (tweak.WeaponRange.HasValue)
                w.Entities.Weapon[gun.Index].RangeMetres = tweak.WeaponRange.Value;
            if (tweak.Optical.HasValue)
                w.Entities.Sensor[gun.Index].Optical = tweak.Optical.Value;
            if (tweak.AimSensorWest) AimWest(w, gun);

            List<Arrival> plan = PlanFlight(1, Catalog.IdOf("FPV Team"), padX, F(780), 14,
                                            gun, droneCount, spacingTicks, 0);
            int next = 0;
            int lastLaunchTick = plan[plan.Count - 1].Tick;

            bool[] struck = new bool[w.Entities.Capacity];
            int reached = 0;
            Fix strikeRange = Catalog.Get(Catalog.IdOf("FPV Team")).WeaponRangeMetres + F(4);

            for (int tick = 0; tick < 200 * SimConstants.TicksPerSecond; tick++)
            {
                IssueDue(w, plan, ref next, tick);
                w.Step();
                if (!w.Entities.IsAlive(gun)) break;

                Fix2 gunPos = w.Entities.Position[gun.Index];
                for (int i = 1; i < w.Entities.HighWater; i++)
                {
                    if (struck[i]) continue;
                    if (!w.Entities.IsSlotAlive(i)) continue;
                    if (w.Entities.Team[i] != 1) continue;
                    if (w.Entities.EntityLayer[i] == Layer.Ground) continue;
                    if (Fix2.Distance(w.Entities.Position[i], gunPos) <= strikeRange)
                    {
                        struck[i] = true;
                        reached++;
                    }
                }

                if (next >= plan.Count && tick > lastLaunchTick + 8
                    && CountFriendlyDronesAirborne(w) == 0) break;
            }

            arrived = reached;
            return !w.Entities.IsAlive(gun);
        }

        /// <summary>
        /// Point a staring head at the threat axis. Half a turn of bearing, done
        /// from outside the simulation because the simulation offers no way to do
        /// it from inside - see the note printed by ApertureExperiment.
        /// </summary>
        static void AimWest(World w, EntityHandle mount)
        {
            SensorSuite s = w.Entities.Sensor[mount.Index];
            s.Facing = 32768;   // 180 degrees in the 16-bit bearing units Trig uses
            w.Entities.Sensor[mount.Index] = s;
        }

        static int CountFriendlyDronesAirborne(World w)
        {
            int n = 0;
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (w.Entities.Team[i] != 1) continue;
                if (w.Entities.EntityLayer[i] == Layer.Ground) continue;
                if (!w.Entities.Has(i, ComponentMask.Sortie)) continue;
                n++;
            }
            return n;
        }
    }
}
