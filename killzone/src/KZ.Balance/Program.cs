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

        // Where the line is, and why it is no longer in front of the objective.
        //
        // Real metres, like everything else in this file since docs/SCALE.md's
        // "content in real metres and real seconds" landed in the catalogue.
        // Every distance below was 12:1 compressed and the harness did not move
        // with the catalogue, so for one session every experiment here was fought
        // inside a 2.4 km box with a mount whose kill ring is 1 km and a mast
        // that reaches 16.8 km. Nine of the ten drifted and none of the drift
        // meant anything.
        //
        // This one is not a straight twelve times, and the reason is the most
        // consequential thing the rescale did to this harness. The old line sat
        // 100 compressed metres in front of the gun so that the last stretch of
        // any approach was contested ground. Twelve times over that is 1,200 m,
        // and with the neutral band it puts 1,968 m of denied ground between the
        // attacker's own territory and the objective. An FPV Team is
        // NavAid.DeadReckoning and OneWay, NavigationSystem adds
        // SimConstants.NavDriftRateInertial - three percent of distance flown -
        // for every metre of that, and World.ApplyDamage throws the warhead away
        // past SimConstants.MunitionMissRadiusMetres. Measured through Spawn and
        // Step: the drone arrives with 66 m of error against a 40 m radius, so
        // every single FPV strike in every gun-mount experiment missed and the
        // mount was immortal at every drone count. The budget is 1,333 m of
        // denied flight and the old geometry spends 1,968.
        //
        // That is a real interaction and a large one - it says a cheap
        // dead-reckoning airframe cannot strike two kilometres past the line,
        // which is exactly what navigation-denied.md argues should be true - but
        // it is not what any experiment below is measuring, and left in place it
        // pins six of them to a fresh ceiling in the FINDINGS 32 shape. Nothing
        // in the catalogue lets an attacker buy his way out of it either: scene
        // matching needs NavAid.SceneMatching, which an FPV Team does not have,
        // so granting imagery does nothing for it.
        //
        // So the line moves behind the objective instead of in front of it, and
        // the scenario changes with it: the defended mount is a forward
        // strongpoint inside ground the attacker holds, being reduced, rather
        // than a rear-area battery being raided. That is one of docs/SCALE.md's
        // own three border relations rather than an invented setting, it removes
        // the confound by construction rather than by leaving a thin margin
        // against a constant, and it keeps every territory and imagery wire live
        // and printed. The interaction itself is not lost: REACH (c) still
        // measures it, because the flat control world has no owner at all and
        // every warhead on it lands on empty grass for precisely this reason.
        //
        // 21,600 m: 1,800 m past the mount at 19,800, so the attacker's rear, his
        // pads, the whole approach and the objective are all on his own ground
        // and the line is still on the map with an owner and imagery either side.
        const int StandardBorderMetres = 21600;

        /// <summary>
        /// Every world this file builds, checked against the one hard limit the
        /// move to real metres introduced.
        ///
        /// Detection and weapon tests compare *squared* distances, so the
        /// simulation cannot compare anything beyond
        /// <c>SimConstants.MaxComparableRangeMetres</c> - past it Fix.MulRaw
        /// wraps and a sensor silently sees nothing at all (docs/SCALE.md,
        /// "What robust looks like"). A map whose own diagonal is past that
        /// limit is therefore a map on which a long-ranged sensor can fail in
        /// silence, which is the worst failure mode available: no exception, no
        /// zero, just a mast that reports nothing and an experiment that reads
        /// it as a balance result.
        ///
        /// Cheap enough to run on every world construction, and it is a
        /// construction-time check rather than a comment because a comment does
        /// not fire when somebody types a bigger number. The two map sizes this
        /// file uses - 28,800 x 19,200 and 24,576 x 24,576 - have diagonals of
        /// 34.6 km and 34.8 km against a 45 km limit, so both clear it with
        /// about a quarter to spare.
        /// </summary>
        static void CheckMapFitsTheComparableRange(int widthMetres, int heightMetres)
        {
            long limit = SimConstants.MaxComparableRangeMetres.RoundToInt();
            long diagSq = (long)widthMetres * widthMetres + (long)heightMetres * heightMetres;
            if (diagSq > limit * limit)
                throw new InvalidOperationException(string.Format(
                    "map {0} x {1} m has a diagonal past SimConstants.MaxComparableRangeMetres "
                  + "({2} m) - squared-distance comparisons would wrap and sensors would go "
                  + "silently blind. See docs/SCALE.md's ceiling correction.",
                    widthMetres, heightMetres, limit));
        }

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
            CheckMapFitsTheComparableRange(widthMetres, heightMetres);
            Terrain t = new Terrain(widthMetres, heightMetres);
            PaintMixedTerrain(t);

            World w = new World(t, entityCapacity, tetherCapacity, seed, playerCount, startTick);
            w.Weather = DefaultWeather;
            w.Ground = DefaultGround;

            const int neutralMetres = 768; // two Territory cells (CellMetres=384) either side of the line
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
            CheckMapFitsTheComparableRange(widthMetres, heightMetres);
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
            Console.WriteLine("SATURATION - one tap's worth of drones against one gun mount");
            Console.WriteLine("launched on one tap from a pad 5,400 m out; the gun's kill ring is");
            Console.WriteLine("1,000 m (point-defence.md Q2) and its camera finds a quad at 1,527 m.");
            Console.WriteLine("'together' is now one tap: pad egress walks them off 4 ticks apart, so");
            Console.WriteLine("the 24-drone row is spread over nearly three of the mount's cooldowns");
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
                    bool killed = RunAssault(n, F(14400), (ulong)(trial + 1), out arrived);
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
        /// with its barrel swept from 550 to 85 of the old compressed metres
        /// (6,600 m to 1,020 m real). It read 7.6 arrived at the top and 7.8 at
        /// the bottom, and 100% gun killed at every rung - twenty-five seconds
        /// of nominal exposure and three point nine producing the same
        /// result. FINDINGS 31 explains why, and the explanation makes the sweep
        /// a question with a known and boring answer: above about 840 m the
        /// barrel is not what stops the mount shooting, because 840 m is where
        /// it first holds a track. A
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
            Console.WriteLine("'Test Long Mount' (Defs.cs, test-only), four FPV Teams one tap every");
            Console.WriteLine("3 s of play from a pad 5,400 m out. Each sweep varies one of the mount's");
            Console.WriteLine("own numbers and");
            Console.WriteLine("holds the other fixed - see the comment on this method for why this is");
            Console.WriteLine("no longer run on the flat control world.");
            PrintWorldConfig(StandardBorderMetres, 1, 2);

            const int trials = 100;

            Console.WriteLine();
            Console.WriteLine("  (a) the barrel, with the mount's 7,200 m optics held fixed");
            Console.WriteLine();
            Console.WriteLine("  barrel   real s under fire   arrived   mount survives");
            Console.WriteLine("  " + new string('-', 62));

            // Retuned, not just multiplied. Twelve times the old rungs is 6,600 m
            // down to 1,020 m, and six of those seven sit above anything this
            // mount can see: its 7,200 m camera finds an FPV Team at 1,527 m
            // after the aperture law and the signature table are applied, and the
            // head only latches a track around 840 m of that. A rung at 4 km and
            // a rung at 6 km are the same mount with the same engagement and
            // differ only in a number nothing reads - the flat line FINDINGS 31
            // predicts, printed five times. The band that can bind is either side
            // of the seeing, so the sweep runs from well inside it to just above
            // it. 6,600 m is kept as the top rung, unchanged, so the old
            // conclusion stays checkable against its own figure; 1,000 m is the
            // Gun Mount's deployed kill ring (point-defence.md §Q2).
            int[] ranges = { 6600, 3000, 2000, 1500, 1200, 1000, 700, 400 };
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
                // 33 m/s, the FPV Team's catalogue speed in real metres per
                // real second (ground-force.md §3.3(a)). Real seconds, not
                // seconds of play, because this column is a physical exposure
                // and the mount's cooldown it wants reading against - four
                // seconds - is a real one too. Divide by TimeMultiplier for what
                // the player watches.
                double exposure = ranges[r] / 33.0;
                Console.WriteLine(string.Format("  {0,6}   {1,18}   {2,7}   {3,14}{4}",
                    ranges[r] + " m",
                    exposure.ToString("0.0") + " s",
                    (arrivedTotal / (double)trials).ToString("0.0"),
                    (survived * 100 / trials) + "%",
                    ranges[r] == 1000 ? "   <- the deployed barrel" : ""));
            }

            Console.WriteLine();
            Console.WriteLine("  (b) the optics, with the barrel held at 6,600 m");
            Console.WriteLine();
            Console.WriteLine("  optics   finds a quad at      arrived   mount survives");
            Console.WriteLine("  " + new string('-', 60));

            // A straight twelve times the old rungs, and unlike (a) this one
            // needed nothing else: 7,200 m of nominal camera comes out at
            // 1,527 m against a quadcopter and 720 m comes out at 153, so the
            // sweep already brackets both the 1,000 m kill ring and the 840 m
            // the head latches at. The "finds a quad at" column is the one to
            // read - the nominal figure is quoted against a 7 m vehicle.
            int[] optics = { 7200, 5400, 3600, 2400, 1440, 720 };
            for (int o = 0; o < optics.Length; o++)
            {
                int arrivedTotal = 0, survived = 0;
                for (int trial = 0; trial < trials; trial++)
                {
                    int arrived;
                    MountTweak t = MountTweak.Range(F(6600));
                    t.Optical = F(optics[o]);
                    if (!RunRangeSweep(4, (ulong)(trial + 1), out arrived, t)) survived++;
                    arrivedTotal += arrived;
                }
                Console.WriteLine(string.Format("  {0,6}   {1,15}   {2,10}   {3,14}{4}",
                    optics[o] + " m",
                    OpticalReachVsFPV(optics[o]).RoundToInt() + " m",
                    (arrivedTotal / (double)trials).ToString("0.0"),
                    (survived * 100 / trials) + "%",
                    optics[o] == 7200 ? "   <- the deployed optics" : ""));
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
                    World w = MakeFlatControlWorld(28800, 19200, 256, 32, (ulong)(trial + 1), 2, 0);
                    if (!RunAssaultScenario(w, "Test Long Mount", MountTweak.Range(F(6600)),
                                            4, F(14400), Sec(3), out arrived)) survived++;
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
            World w = MakeRealisticWorld(24576, 24576, 64, 4, 1, 2, 0, 13200, 1, 2);
            EntityHandle gun = w.Spawn(Catalog.IdOf("Test Long Mount"), 1, P(12000, 12000));
            w.Entities.Sensor[gun.Index].Optical = F(nominalOptical);
            EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(14400, 12000));
            w.Step();
            return w.DetectionRangeFor(gun.Index, drone.Index, SensorChannel.Optical);
        }

        /// <summary>
        /// How much launching from closer helps. This is what a forward position
        /// buys you, and it is the lever a player actually has.
        ///
        /// <para><b>Retuned, and it was already known to need it.</b> FINDINGS 31
        /// recorded that this experiment's pads were chosen to bracket the Gun
        /// Mount's old, wrong 550-metre reach and did not resolve the real edge -
        /// "only the last row or two above actually land inside the real
        /// envelope", printed by the experiment itself. The rescale made that
        /// worse rather than better: twelve times the old pads is 4,800 m to
        /// 19,200 m against a mount at 19,800 m whose kill ring is 1,000 m, so
        /// five of the six rows launch from entirely outside the envelope and
        /// differ only in how much empty sky the flight crosses first. A sweep
        /// with one informative row is the ceiling problem of FINDINGS 32 wearing
        /// a distance label.</para>
        ///
        /// <para>The pads below are therefore set by distance from the mount
        /// rather than by map coordinate, and they bracket the two edges that
        /// actually exist: the 1,000 m kill ring, and the ~840 m at which a
        /// 120-degree head sweeping at 70 deg/s first latches a track on a
        /// quadcopter (FINDINGS 31's 70 m, in real metres). The old 5,400 m pad
        /// is kept as the first row so every other experiment in this file, all
        /// of which launch from it, has a row here to be read against.</para>
        ///
        /// <para>The force is four drones and not eight. SATURATION puts eight
        /// at 100% against this mount at every pad distance worth testing, so an
        /// eight-drone sweep prints a column of hundreds and measures nothing -
        /// FINDINGS 32's ceiling, arrived at from the other direction now that
        /// the mount is much stronger than it was in compressed units. Four is
        /// the force SATURATION reads at 67%, which leaves room either way.</para>
        /// </summary>
        static void ApproachExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("APPROACH - four drones, varying where they launch from");
            Console.WriteLine("gun at 19,800 m with a 1,000 m kill ring, so its edge is at 18,800 m and");
            Console.WriteLine("it latches a track on a quad at about 840 m (FINDINGS 31 in real metres)");
            Console.WriteLine("pads retuned this session to bracket that edge - the old set bracketed");
            Console.WriteLine("the wrong one and FINDINGS 31 said so; the 5,400 m row is the pad every");
            Console.WriteLine("other experiment in this file launches from");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  launch at   out from gun   arrived   gun killed");
            Console.WriteLine("  " + new string('-', 54));

            // Distance from the mount at x = 19,800, converted to a pad
            // coordinate below. 5,400 is the standard pad; the rest step across
            // the kill ring and the track-latch distance.
            int[] standoffs = { 5400, 2400, 1600, 1200, 900, 600, 300 };
            int[] pads = new int[standoffs.Length];
            for (int i = 0; i < standoffs.Length; i++) pads[i] = 19800 - standoffs[i];
            for (int p = 0; p < pads.Length; p++)
            {
                int arrivedTotal = 0, gunKilled = 0;
                const int trials = 60;

                for (int trial = 0; trial < trials; trial++)
                {
                    int arrived;
                    bool killed = RunAssault(4, F(pads[p]), (ulong)(trial + 1), out arrived);
                    arrivedTotal += arrived;
                    if (killed) gunKilled++;
                }

                Console.WriteLine(string.Format("  {0,9}   {1,12}   {2,7}   {3,10}",
                    pads[p] + " m",
                    standoffs[p] + " m" + (standoffs[p] == 1200 ? " *" : ""),
                    (arrivedTotal / (double)trials).ToString("0.0"),
                    (gunKilled * 100 / trials) + "%"));
            }
            Console.WriteLine();
            Console.WriteLine("  * the row that straddles the 1,000 m kill ring. Above it the flight");
            Console.WriteLine("  crosses the whole envelope; below it the drones are inside the ring");
            Console.WriteLine("  from the tick they exist, under fire from the start but for far less");
            Console.WriteLine("  of it. That transition is what a forward pad buys, and it is the");
            Console.WriteLine("  thing the old pad set - chosen around the mount's long-since-corrected");
            Console.WriteLine("  550 m reach - could not show at all.");
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
                    if (RunAssault(n, F(14400), (ulong)(trial + 1), out a, 0)) dayKilled++;
                    dayArrived += a;
                    // Well into the night phase of the cycle.
                    if (RunAssault(n, F(14400), (ulong)(trial + 1), out a, 8000)) nightKilled++;
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
            World w = MakeRealisticWorld(28800, 19200, 8, 1, 1, 2, night ? 8000 : 0,
                StandardBorderMetres, 1, 2);
            EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(19800, 9360));
            EntityHandle target = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(14400, 9360));
            w.Step();
            return w.BestDetectionRange(gun.Index, target.Index);
        }

        /// <summary>
        /// A minefield laid across a supply road, against the traffic that has to
        /// use it.
        ///
        /// <para>Mines are the least interesting weapon in the game to operate and
        /// the hardest to argue with. They need no crew, no link and no pilot,
        /// cannot be jammed or shot down, and are still there an hour later. They
        /// are also indiscriminate, and the last row below is that claim measured
        /// rather than asserted.</para>
        ///
        /// <para><b>On this experiment being flat.</b> The drift guard lists this
        /// among seven suspected inert, and it is the one of the seven that is
        /// simply correct. A mine involves no sensor, no hit roll, no magazine,
        /// no traverse and no arrival order, so none of the faults that killed
        /// the other six can reach it, and there is no stochastic term for a
        /// trial count to average over: one run is the answer. FINDINGS 10
        /// already records it as the only table that came through the honest
        /// re-run untouched.</para>
        ///
        /// <para>Two things did need fixing, and neither was inertia. The
        /// "per mine" column was computed in this harness by calling
        /// <c>Catalog.DamageMultiplier</c> and multiplying - so it re-derived the
        /// simulation's arithmetic instead of observing it, and would have agreed
        /// with a broken <c>World.ApplyDamage</c> exactly as readily as with a
        /// working one. It is now read off the vehicle's health in a running
        /// world. And the indiscriminate rule, which is the entry's whole moral
        /// argument, had no row at all.</para>
        /// </summary>
        static void MineExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("MINES - a field laid across a supply road");
            Console.WriteLine("four mines from one heavy drone, 600 damage each, into the underside");
            Console.WriteLine("every number below is read off a vehicle driving through a live field,");
            Console.WriteLine("not computed from the damage table; a + means the first mine killed it");
            Console.WriteLine("outright, so the observed bite is a lower bound on the warhead");
            PrintWorldConfig(10800, 2, 1); // the convoy's own rear (west) vs. the ambush ground it drives into (east)
            Console.WriteLine();
            Console.WriteLine("  vehicle             health off   survives a mine   field stops");
            Console.WriteLine("  " + new string('-', 60));

            string[] vehicles = { "Supply Truck", "Logistics UGV", "IFV", "Main Tank" };
            for (int v = 0; v < vehicles.Length; v++)
            {
                int perMine, triggered; bool atLeast;
                bool died = RunMineField(vehicles[v], 2, out perMine, out triggered, out atLeast);

                UnitDef def = Catalog.ByName(vehicles[v]);
                int eats = atLeast ? 1 : (def.Hp.RoundToInt() + perMine - 1) / perMine;

                Console.WriteLine(string.Format("  {0,-18}  {1,8}   {2,15}   {3}",
                    vehicles[v],
                    perMine + (atLeast ? "+" : " "),
                    (eats > 1 ? (eats - 1).ToString() : "0") + " of 4",
                    died ? "yes, after " + triggered + " mine(s)" : "no"));
            }

            Console.WriteLine();
            Console.WriteLine("  And the same field, driven into by the side that laid it:");
            Console.WriteLine();
            {
                int perMine, triggered; bool atLeast;
                bool died = RunMineField("Supply Truck", 1, out perMine, out triggered, out atLeast);
                Console.WriteLine(string.Format("  {0,-18}  {1,8}   {2,15}   {3}",
                    "Supply Truck (own)",
                    perMine + (atLeast ? "+" : " "),
                    "0 of 4",
                    died ? "yes, after " + triggered + " mine(s)" : "no"));
            }

            Console.WriteLine();
            Console.WriteLine("  A mine costs nothing to keep there. The heavy drone that laid it");
            Console.WriteLine("  flew home and can do it again tomorrow night. It also does not ask");
            Console.WriteLine("  whose vehicle arrives first, which is not a gameplay penalty - it is");
            Console.WriteLine("  what a mine is, and a game about this subject should not pretend");
            Console.WriteLine("  otherwise.");
        }

        /// <summary>
        /// Drive one vehicle of the given team down a road through a four-mine
        /// field laid by team 1, and report what the field did to it: how much
        /// one mine actually took off, how many went off, and whether it stopped.
        /// </summary>
        static bool RunMineField(string vehicleName, byte vehicleTeam,
                                 out int damagePerMine, out int minesTriggered,
                                 out bool damageWasClamped)
        {
            damageWasClamped = false;
            World w = MakeRealisticWorld(28800, 19200, 128, 8, 4242, 2, 8000, 10800, 2, 1);

            // The robot in this list is radio-controlled, so without something to
            // talk to it stops of its own accord and the experiment measures the
            // wrong thing entirely. One post per side, so a team-1 vehicle is no
            // worse connected than a team-2 one and the two rows differ only in
            // whose field it is.
            w.Spawn(Catalog.IdOf("Command Post"), 2, P(8400, 10800));
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(8400, 7920));

            EntityHandle vehicle = w.Spawn(Catalog.IdOf(vehicleName), vehicleTeam, P(7200, 9360));
            for (int m = 0; m < 4; m++)
                w.SpawnMine(1, P(12000 + m * SimConstants.MineSpacingMetres, 9360), F(600));

            w.Enqueue(Command.MoveTo(vehicleTeam, vehicle, P(21600, 9360)));

            Fix lastHp = w.Entities.Hp[vehicle.Index];
            Fix biggestBite = Fix.Zero;
            minesTriggered = 0;

            for (int tick = 0; tick < 300 * SimConstants.TicksPerSecond; tick++)
            {
                w.Step();
                for (int e = 0; e < w.Events.Count; e++)
                    if (w.Events[e].Kind == SimEventKind.MineDetonated) minesTriggered++;

                if (w.Entities.IsSlotAlive(vehicle.Index))
                {
                    Fix hp = w.Entities.Hp[vehicle.Index];
                    if (hp < lastHp && lastHp - hp > biggestBite) biggestBite = lastHp - hp;
                    lastHp = hp;
                }

                if (!w.Entities.IsAlive(vehicle))
                {
                    // A killing bite is clamped by whatever health was left, so it
                    // understates the warhead rather than reporting it. When the
                    // first mine kills outright there is no unclamped observation
                    // to use, and the honest thing to print is the vehicle's own
                    // starting health marked as a lower bound - not a figure
                    // recomputed from the damage table, which is what this
                    // experiment was doing for every row.
                    if (biggestBite.Raw == 0 || minesTriggered <= 1)
                    {
                        biggestBite = Catalog.ByName(vehicleName).Hp;
                        damageWasClamped = true;
                    }
                    damagePerMine = biggestBite.RoundToInt();
                    return true;
                }
                if (w.Entities.Position[vehicle.Index].X > F(20400)) break;
            }

            damagePerMine = biggestBite.RoundToInt();
            return false;
        }

        /// <summary>
        /// The same turret with different sensors fitted, by day and by night -
        /// and what the difference is worth when something attacks it.
        ///
        /// <para><b>What it used to measure, and why it could not move.</b> It
        /// spawned a real Gun Mount, overwrote <c>EntityTable.Sensor</c> with a
        /// hypothetical suite, stepped the world once and printed
        /// <c>BestDetectionRange</c>. That is a lookup of the detection formula
        /// printed as a table: it moves when a signature number or a channel
        /// constant moves and at no other time, it is the hand-assignment
        /// WIRING-SPEC rules out as evidence, and it can never answer the
        /// question the experiment's own summary asks - whether which sensors
        /// are fitted matters more than the gun does. Nothing in it was a
        /// measurement of the game.</para>
        ///
        /// <para>Its commentary had also gone quietly wrong underneath it, which
        /// is the cheapest possible demonstration of the cost: it asserted that
        /// against a quadcopter microphones are "the best sensor on the list at
        /// any hour" while the numbers printed directly above it read 73 m for
        /// optics and 48 m for acoustic by day. The corrected signature table
        /// moved the numbers and nobody re-read the paragraph.</para>
        ///
        /// <para><b>What it measures now.</b> Four test-only catalogue mounts
        /// (Defs.cs, "Test Mount ...") identical to the shipped Gun Mount apart
        /// from what they can see with, spawned through <c>World.Spawn</c> and
        /// made to fight the same assault by day and after dark. The reach
        /// columns are kept, because a reach is a useful thing to be able to read
        /// off - but they are now read off a spawned unit rather than an
        /// assembled struct, and they are no longer the whole experiment.</para>
        /// </summary>
        static void SensorMixExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("SENSORS - what a turret can find, and what finding it is worth");
            Console.WriteLine("four test-only mounts, identical to the Gun Mount except for the sensor");
            Console.WriteLine("fit; all four carry the Gun Mount's real 1,000 m barrel");
            Console.WriteLine("the assault is three FPV Teams, one tap every 3 s of play, from a pad");
            Console.WriteLine("5,400 m out");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("                             finds a quad at     vs tank    mount survives");
            Console.WriteLine("  fitted with                 day     night        day      day     night");
            Console.WriteLine("  " + new string('-', 76));

            string[] names = { "optics only", "acoustic only", "optics + acoustic",
                               "optics + acoustic + thermal" };
            string[] defs = { "Test Mount Optics", "Test Mount Acoustic",
                              "Test Mount Optics Acoustic", "Test Mount Optics Acoustic Thermal" };

            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine(string.Format("  {0,-27} {1,6}  {2,8}  {3,9}  {4,7}  {5,8}",
                    names[i],
                    MountReach(defs[i], "FPV Team", false).RoundToInt() + " m",
                    MountReach(defs[i], "FPV Team", true).RoundToInt() + " m",
                    MountReach(defs[i], "Main Tank", false).RoundToInt() + " m",
                    MountSurvival(defs[i], false) + "%",
                    MountSurvival(defs[i], true) + "%"));
            }

            Console.WriteLine();
            Console.WriteLine("  A mount's reach is set by its gun and its envelope by whichever sensor");
            Console.WriteLine("  finds the target first, and against a small electric quadcopter that");
            Console.WriteLine("  is far shorter than the barrel can throw a round - which is why the");
            Console.WriteLine("  survival columns move at all when the barrel never changes, and why");
            Console.WriteLine("  they move so much less by day than after dark: in daylight every fit");
            Console.WriteLine("  on this list finds the drone before the barrel can use the fact.");
            Console.WriteLine();
            Console.WriteLine("  Read the day and night reach columns together rather than separately.");
            Console.WriteLine("  A camera beats a microphone against a quadcopter in daylight and");
            Console.WriteLine("  collapses after dark; the microphone does not care what time it is");
            Console.WriteLine("  and becomes the better of the two. Thermal buys back the night");
            Console.WriteLine("  against a vehicle and very little against a small drone, because a");
            Console.WriteLine("  small electric drone is not very hot.");
        }

        /// <summary>The best channel one spawned mount has against one target.</summary>
        static Fix MountReach(string mountDefName, string targetName, bool night)
        {
            World w = MakeRealisticWorld(24576, 24576, 64, 4, 1, 2, night ? 8000 : 0, 13200, 1, 2);
            EntityHandle gun = w.Spawn(Catalog.IdOf(mountDefName), 1, P(12000, 12000));
            EntityHandle target = w.Spawn(Catalog.IdOf(targetName), 2, P(14400, 12000));
            w.Step();
            return w.BestDetectionRange(gun.Index, target.Index);
        }

        /// <summary>
        /// How often that same mount is still standing after three drones have
        /// come at it three seconds apart.
        /// </summary>
        static int MountSurvival(string mountDefName, bool night)
        {
            // 100 trials rather than the 40 the other experiments use: the
            // differences this table is reporting are ten or fifteen points and
            // at 40 trials that is inside the noise, which would make it a table
            // that moves without meaning anything - the opposite failure to the
            // one being fixed, and just as useless.
            const int trials = 100;
            int survived = 0;
            for (int trial = 0; trial < trials; trial++)
            {
                World w = MakeRealisticWorld(28800, 19200, 256, 32, (ulong)(trial + 1), 2,
                    night ? 8000 : 0, StandardBorderMetres, 1, 2);
                int arrived;
                if (!RunAssaultScenario(w, mountDefName, MountTweak.None, 3, F(14400), Sec(3), out arrived))
                    survived++;
            }
            return survived * 100 / trials;
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
            Console.WriteLine("  mount. FPV Teams, 200 Materiel each, from a pad 5,400 m out. The last");
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
            World w = MakeRealisticWorld(28800, 19200, 512, 32, seed, 2, 0, StandardBorderMetres, 1, 2);
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
            Fix2[] positions = { P(19800, 9360), P(20340, 8820), P(20340, 9900), P(20640, 9360) };

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

            List<Arrival> plan = PlanFlight(1, Catalog.IdOf("FPV Team"), F(14400), F(9360), 144,
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
            Console.WriteLine("one tap every 3 s of play, because a band change is only charged between");
            Console.WriteLine("engagements and a simultaneous wave only ever offers one");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("                          Gun Mount (1,000 m,        Autocannon Mount");
            Console.WriteLine("                          cannot reach high)         (3,360 m, reaches high)");
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
            World w = MakeRealisticWorld(28800, 19200, 512, 32, seed, 2, 0, StandardBorderMetres, 1, 2);
            BuildAttackerRear(w);

            EntityHandle gun = w.Spawn(Catalog.IdOf(mountDefName), 2, P(19800, 9360));

            int total = low + high;
            // Spread the high drones through the order rather than sending the
            // low half first: a flight that goes low then high is a sequential
            // attack, which is a different tactic from a split one.
            bool[] goesHigh = new bool[total];
            for (int k = 0; k < high; k++) goesHigh[(k * total) / high] = true;

            List<Arrival> plan = PlanFlight(1, Catalog.IdOf("Multirole Quad"), F(14400), F(9360), 144,
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
            Console.WriteLine("mast 1,200 m behind it, and the package is aimed at the mast (two warheads");
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
            World w = MakeRealisticWorld(28800, 19200, 512, 32, seed, 2, 0, StandardBorderMetres, 1, 2);
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
            GrantHomeImagery(w, 1, true, 28800);

            EntityHandle post = w.Spawn(Catalog.IdOf("Radar Mast"), 2, P(21000, 9360));
            EntityHandle battery = w.Spawn(Catalog.IdOf("Interceptor Battery"), 2, P(19800, 9360));

            List<Arrival> plan = PlanFlight(1, Catalog.IdOf("Heavy Strike Drone"), F(10800), F(9360), 360,
                                            post, real, Sec(2), 0);
            List<Arrival> escort = PlanFlight(1, Catalog.IdOf("Decoy Drone"), F(10800), F(9120), 264,
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
        /// The aperture trade, and what each position on it is worth in a fight.
        ///
        /// <para><b>What it used to measure, and why it could not move.</b> A
        /// spawned Gun Mount with <c>DirectionalArcDegrees</c> overwritten to
        /// each of six values in turn, one <c>World.Step</c>, and
        /// <c>DetectionRangeFor</c> printed. That is the square-root-of-arc law
        /// evaluated at six points - correct, useful to have written down, and
        /// not an experiment: it can only move when a signature number or an
        /// optical constant moves, and it says nothing about whether the choice
        /// it describes is worth making. The second table under it was worse
        /// still: "blind over 11/12 of the sky" and "looking away 2/3 of the
        /// time" are assertions with a reach number typed beside them, not
        /// measurements of anything.</para>
        ///
        /// <para><b>What it measures now.</b> The law is kept, labelled as a law.
        /// Under it, four spawnable mounts - a 30-degree staring head, a
        /// 120-degree staring head, the shipped Gun Mount's 120-degree head
        /// sweeping at 70 deg/s, and a 360-degree head - fight the same assault,
        /// and the table reports how often each survives it.</para>
        ///
        /// <para><b>And one thing the rebuild found rather than measured.</b> A
        /// staring head has to be pointed at something, and nothing in this game
        /// can point one. <c>SensorSuite.Facing</c> is initialised to zero (due
        /// east) by <c>World.Spawn</c> and the only thing that ever writes it
        /// again is <c>World.SweepSensors</c>, which does nothing for a head with
        /// no scan rate. There is no command, no order and no structure
        /// orientation. So the narrow end of FINDINGS 22's slider is not a
        /// purchase a player can make: a 30-degree mount faces east for the
        /// whole match. The row below that sets the facing by hand is marked as
        /// doing so, and it is measuring a unit the game cannot currently
        /// produce.</para>
        /// </summary>
        static void ApertureExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("APERTURE - the same 7,200 m camera, spread over different arcs");
            PrintWorldConfig(13200, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  The law. Reach scales as the square root of how narrow the arc is;");
            Console.WriteLine("  this is the formula evaluated, not a fight, and it moves only when a");
            Console.WriteLine("  signature or an optical constant moves.");
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
            Console.WriteLine("  What it is worth. Four mounts differing only in their head, against");
            Console.WriteLine("  three FPV Teams, one tap every 3 s of play, from the west.");
            Console.WriteLine();
            Console.WriteLine("  head                                         reach on axis   mount survives");
            Console.WriteLine("  " + new string('-', 76));

            string[] labels = {
                "30 deg staring, facing east (default)",
                "30 deg staring, turned to face the threat",
                "120 deg staring, facing east (default)",
                "120 deg sweeping at 70 deg/s (shipped)",
                "360 deg, no blind side"
            };
            string[] defs = {
                "Test Mount Arc 30", "Test Mount Arc 30", "Test Mount Arc 120 Staring",
                "Gun Mount", "Test Mount Arc 360"
            };
            bool[] aimed = { false, true, false, false, false };

            for (int i = 0; i < labels.Length; i++)
            {
                Console.WriteLine(string.Format("  {0,-42}  {1,13}   {2,14}",
                    labels[i],
                    ApertureMountReach(defs[i], aimed[i]).RoundToInt() + " m",
                    ApertureSurvival(defs[i], aimed[i]) + "%"));
            }

            Console.WriteLine();
            Console.WriteLine("  \"Reach on axis\" is what the head finds when it happens to be pointed");
            Console.WriteLine("  the right way - DetectionRangeFor answers the range question and not");
            Console.WriteLine("  the bearing one - so rows one and two share a number and not an");
            Console.WriteLine("  outcome. That is the whole content of an arc.");
            Console.WriteLine();
            Console.WriteLine("  The second row is the only one that turns its head, and it does so by");
            Console.WriteLine("  a direct write to SensorSuite.Facing from this harness, because the");
            Console.WriteLine("  simulation has no way to aim a staring sensor: World.Spawn sets the");
            Console.WriteLine("  facing due east and only SweepSensors ever moves it. Every other row");
            Console.WriteLine("  is a unit a player could actually buy. The gap between rows one and");
            Console.WriteLine("  two is therefore not a balance figure, it is the size of a missing");
            Console.WriteLine("  mechanic.");
            Console.WriteLine();
            Console.WriteLine("  Microphones and radio listening are exempt from all of this, because");
            Console.WriteLine("  they are omnidirectional by nature - which is exactly why they are");
            Console.WriteLine("  the cheap way to know something is out there and the useless way to");
            Console.WriteLine("  know where it is. It is also why the narrow heads below are not as");
            Console.WriteLine("  blind as their arc suggests: these mounts carry the Gun Mount's");
            Console.WriteLine("  2,400 m array as well, and it does not care which way the camera is");
            Console.WriteLine("  pointed.");
            Console.WriteLine();
            Console.WriteLine("  Worth reading against the law above it: the 30-degree head sees three");
            Console.WriteLine("  and a half times as far as the 360-degree one, costs twelve times as");
            Console.WriteLine("  much to cover the same sky, and - when it is pointed the right way,");
            Console.WriteLine("  which nothing can currently arrange - survives exactly as often. Above");
            Console.WriteLine("  about 70 m the extra reach buys a mount nothing, because 70 m is where");
            Console.WriteLine("  it first holds a track and 3.2 s is all it has after that (FINDINGS");
            Console.WriteLine("  31). The panoramic head is not the compromise position on this slider;");
            Console.WriteLine("  on these numbers it is the only one worth buying.");
        }

        /// <summary>
        /// The optical law at one arc. This is the one place in this file that
        /// still writes a sensor field on a spawned unit, and it is deliberate:
        /// the table above is the formula, and spawning six near-identical
        /// catalogue entries to evaluate a formula would be six dead units in
        /// Defs.cs to avoid a comment.
        /// </summary>
        static Fix ApertureReach(int arcDegrees)
        {
            World w = MakeRealisticWorld(24576, 24576, 64, 4, 1, 2, 0, 13200, 1, 2);

            EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
            SensorSuite s = w.Entities.Sensor[gun.Index];
            s.DirectionalArcDegrees = arcDegrees;
            s.ScanDegreesPerSecond = 0;
            s.Facing = 0;
            w.Entities.Sensor[gun.Index] = s;

            EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(14400, 12000));
            w.Step();
            return w.DetectionRangeFor(gun.Index, drone.Index, SensorChannel.Optical);
        }

        /// <summary>Optical reach of one spawned mount against a quadcopter to its west.</summary>
        static Fix ApertureMountReach(string mountDefName, bool aimWest)
        {
            World w = MakeRealisticWorld(24576, 24576, 64, 4, 1, 2, 0, 13200, 1, 2);
            EntityHandle gun = w.Spawn(Catalog.IdOf(mountDefName), 1, P(12000, 12000));
            if (aimWest) AimWest(w, gun);
            EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(9600, 12000));
            w.Step();
            return w.DetectionRangeFor(gun.Index, drone.Index, SensorChannel.Optical);
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

        /// <summary>How often that head is still standing after three drones.</summary>
        static int ApertureSurvival(string mountDefName, bool aimWest)
        {
            const int trials = 100;
            int survived = 0;
            for (int trial = 0; trial < trials; trial++)
            {
                World w = MakeRealisticWorld(28800, 19200, 256, 32, (ulong)(trial + 1), 2, 0,
                    StandardBorderMetres, 1, 2);
                int arrived;
                if (!RunAssaultScenario(w, mountDefName,
                        aimWest ? MountTweak.Aimed() : MountTweak.None, 3, F(14400), Sec(3), out arrived))
                    survived++;
            }
            return survived * 100 / trials;
        }

        // ------------------------------------------------------------------
        // Arrival scheduling, and why half the experiments below needed it.
        //
        // Every experiment in this file used to launch its whole flight on tick
        // zero. A mount therefore got exactly one engagement window per trial:
        // it holds a track at about 840 m, lays on one drone, fires once, and the
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
        // AUDIT-UNWIRED F19 is now closed and the two mechanisms compound, so
        // what follows is what they add up to rather than a note that one of
        // them is missing.
        //
        // SortieState.EgressUntilTick used to be written by SortieSystem.Launch
        // from the launchIndex every call below already passes, and read by
        // nothing. MovementSystem now holds an airframe on its pad until that
        // tick elapses, so a launch carries a departure delay of
        // SortiePadEgressBaseTicks + launchIndex * SortiePadEgressPerIndexTicks
        // - 8 + 4i ticks, measured on a flight of eight: departures at ticks
        // 8, 12, 16 ... 36 for a flight enqueued all on tick zero, and at
        // 8, 108, 208 ... for one enqueued a hundred ticks - sorry, ninety-six
        // ticks - apart.
        //
        // Two consequences, and they pull in opposite directions.
        //
        // The engine's spacing is NOT a substitute for the scheduling below, so
        // the scheduling stays. Four ticks per airframe is half a real second.
        // The Gun Mount's cooldown is 32 ticks (four real seconds) and its
        // reload is twenty, so an eight-drone flight leaves the pad inside a
        // single cooldown and the mount still gets one engagement window - which
        // is the exact condition FINDINGS 31's amendment says makes the belt
        // unreachable. It is pad de-confliction, which is what its own doc
        // comment claims for it ("so a flight does not spawn stacked"), and it
        // is about a twenty-fourth of the stagger the belt needs.
        //
        // But it is not nothing either, and it is no longer possible to express
        // a genuinely simultaneous wave from out here. A flight of 24 - the top
        // of SATURATION's ladder - is now spread over 92 ticks, which is nearly
        // three of the mount's cooldowns. So "launched together" means "tapped
        // together" from this session on, and the rows where it matters say so.
        // It also inflates every deliberate spacing by four ticks per airframe:
        // "one every 3 s" is one tap every 3 s of play and one departure every
        // 3.1. That is left uncorrected on purpose - subtracting an engine
        // constant from a player's tap schedule would model a player who taps
        // faster to compensate for his own pad, and nobody does that.
        //
        // So spacing is still done the one honest way available from outside the
        // simulation: each launch command is enqueued on the tick it is meant to
        // be issued on, which is also exactly what a player tapping a card eight
        // times over eight seconds produces.
        struct Arrival { public int Tick; public Command Cmd; }

        /// <summary>
        /// A flight of <paramref name="count"/> airframes leaving the same pad
        /// area, one tap every <paramref name="spacingTicks"/> ticks - the pad
        /// itself then adds four ticks per airframe on top (see above), so this
        /// is the interval between orders and not quite the interval between
        /// departures. Spacing zero is the simultaneous wave, which since pad
        /// egress was wired is a wave only in the sense that one tap produces
        /// it.
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
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(4800, 9360));
            for (int q = 0; q < 6; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(3600 + q * 480, 10800));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(12000, 9360));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(17400, 13800));
        }

        // ------------------------------------------------------------------

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
            World w = MakeRealisticWorld(28800, 19200, 256, 32, seed, 2, startTick,
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
            World w = MakeRealisticWorld(28800, 19200, 256, 32, seed, 2, 0,
                StandardBorderMetres, 1, 2);
            return RunAssaultScenario(w, "Test Long Mount", tweak, droneCount, F(14400),
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
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(4800, 9360));
            // Enough crews that the experiment measures the gun, not the crew cap.
            for (int q = 0; q < 6; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(3600 + q * 480, 10800));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(12000, 9360));
            // Far enough back that the experiment measures the gun against
            // drones, and not the gun against a relay mast.
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(17400, 13800));

            EntityHandle gun = w.Spawn(Catalog.IdOf(gunDefName), 2, P(19800, 9360));
            if (tweak.WeaponRange.HasValue)
                w.Entities.Weapon[gun.Index].RangeMetres = tweak.WeaponRange.Value;
            if (tweak.Optical.HasValue)
                w.Entities.Sensor[gun.Index].Optical = tweak.Optical.Value;
            if (tweak.AimSensorWest) AimWest(w, gun);

            List<Arrival> plan = PlanFlight(1, Catalog.IdOf("FPV Team"), padX, F(9360), 168,
                                            gun, droneCount, spacingTicks, 0);
            int next = 0;
            int lastLaunchTick = plan[plan.Count - 1].Tick;

            bool[] struck = new bool[w.Entities.Capacity];
            int reached = 0;
            Fix strikeRange = Catalog.Get(Catalog.IdOf("FPV Team")).WeaponRangeMetres + F(48);

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
