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
// Gun Mount's range back to 550 compressed metres after FINDINGS 2 corrected
// it to 85 (the ring is 1,000 real metres now, and every figure in this file
// is real metres since the catalogue rescale - docs/SCALE.md), and
// every experiment ran on Fill(Open) with clear weather, firm ground, an
// ownerless map and no imagery. Both are fixed: a mount that needs different
// numbers is a "Test ..." catalogue entry rather than a shipped unit written
// behind the reader's back, and every experiment states the world it ran on in
// its own output.
//
// FINDINGS 32 and the experiment-drift guard (docs/EXPERIMENT-DRIFT.md): seven
// of the ten experiments here were reported as unable to detect a change to the
// simulation. Stacking, Vertical, Decoy Escort, Mines, Sensors, Aperture and
// Range were gone through one at a time; the reasoning for each is on the
// experiment itself rather than summarised here, because a summary at the top of
// a file is the first thing to go stale. In outline:
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
//
// docs/SCALE.md's rescale: every distance in this file is now a real metre.
// The catalogue moved to real metres and one global 4x time multiplier and this
// harness did not move with it, so for one session every experiment here was
// fought on a 2,400 x 1,600 *metre* map by a mount whose kill ring is 1,000 m
// against a mast that reaches 16.8 km - a box in which every weapon and every
// sensor was effectively unlimited. Nine of the ten drifted and none of the
// drift meant anything. Three things came out of putting it right, none of them
// a multiplication:
//
//   THE BORDER MOVED BEHIND THE OBJECTIVE, because twelve times the old
//   geometry puts 1,968 m of denied ground in front of the gun and an FPV
//   Team's dead reckoning throws the warhead away after 1,333. See
//   StandardBorderMetres.
//
//   TWO SWEEPS WERE RETUNED rather than converted, because twelve times their
//   old rungs no longer spans anything: APPROACH's pads (FINDINGS 31 already
//   said they bracketed the wrong edge) and REACH's barrel ladder, five of
//   whose rungs are now above what the mount can see.
//
//   PAD EGRESS IS WIRED NOW and compounds with the launch spacing below, so
//   "launched together" means "tapped together". See "Arrival scheduling".
//
// FINDINGS 40 revisited all of that and corrected two halves of it. Four of the
// seven were never inert - the guard was inferring deadness from what its
// siblings did, and SENSORS in particular carries no radar and was supposed to
// sit still through a Doppler notch. The two that genuinely were pinned are not
// on that list at all: SATURATION and DARKNESS were sweeping past the Gun
// Mount's transition rather than across it, and both are rebuilt around it, with
// a second arm on a different mount where one mount could not be made to
// resolve. VERTICAL's Gun Mount column is a ceiling, is measured to be one that
// no ladder can move, and is excused in writing rather than smoothed.
//
// FINDINGS 40's larger finding is about this file as a whole and is not fixed
// here: the ten are ten variations on one engagement - a mount, a pad and FPV
// Teams - and interception vectoring, emission control, ground radar and fiber
// can each be broken outright without any of them printing a different byte.
// Three experiments are specified there and none of them is written yet.
//
// Every map size this file builds is checked against the ~32 km ceiling the
// squared-distance comparison imposes (CheckMapFitsTheComparableRange). The two
// in use, 28,800 x 19,200 and 24,576 x 24,576, have diagonals of 34.6 and
// 34.8 km against a 45 km limit.

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
            if (which == "all" || which == "radar") RadarGroundExperiment();
            if (which == "all" || which == "intercept") InterceptionExperiment();
            if (which == "all" || which == "fiber") FiberExperiment();

            return 0;
        }

        // ------------------------------------------------------------------

        /// <summary>
        /// Send N drones at a gun position, all at once, and see how many arrive.
        ///
        /// The question this answers is whether a gun that kills one drone at a
        /// time can be beaten by sending more drones at a time. It is the first
        /// thing any player will try.
        ///
        /// <para><b>Why there are two arms now.</b> FINDINGS 40. Against the Gun
        /// Mount alone this experiment went from 0% to 100% across a two-drone
        /// span, and the ladder it swept - 1, 2, 3, 5, 8, 12, 16, 24 - put five of
        /// its eight rungs past the top of that span. Six of the eight cells in
        /// the "gun killed" column read 0% or 100%, which is FINDINGS 32's
        /// diagnosis exactly: a measurement pinned to a ceiling is a constant, not
        /// a result. Four rows said "yes, obviously" four times.</para>
        ///
        /// <para>The fix is FINDINGS 32's own prescription - a defence that starts
        /// with an advantage - taken by adding the second point-defence unit the
        /// catalogue already ships rather than by stacking more of the first. More
        /// Gun Mounts would not have worked: the STACKING table below already
        /// measures four mounts as worth two points over one against a tap,
        /// because a supporting mount commits to a target that is already being
        /// engaged. The Autocannon Mount is a different proposition - twenty
        /// engagements a belt against five, a two-second cycle, airburst rounds
        /// and 3,360 m of reach - and its own transition sits between 6 and 16
        /// drones, so each arm now brackets its own band: one rung the mount
        /// always survives, three that move, one it never survives.</para>
        ///
        /// <para>The per-kill column is what the second arm buys as a question
        /// rather than as a row. It is the only place in this harness where two
        /// pieces of the same kind of kit are priced against the attack that
        /// removes them.</para>
        /// </summary>
        // MEASURES saturation: track-hold
        static void SaturationExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("SATURATION - one tap's worth of drones against one gun position");
            Console.WriteLine("launched on one tap from a pad 5,400 m out; pad egress walks them");
            Console.WriteLine("off 4 ticks apart, so the longer rows are spread over several of");
            Console.WriteLine("the mount's cooldowns");
            Console.WriteLine("two arms, each swept across its own mount's band - see the comment");
            Console.WriteLine("on this method for why one arm could not be measured");
            PrintWorldConfig(StandardBorderMetres, 1, 2);

            // Ladders chosen to bracket each mount's own transition, measured
            // before they were written down: the Gun Mount goes 0% at two drones
            // to 100% at six, the Autocannon 0% at six to 100% at sixteen. A rung
            // below the band and a rung above it, and nothing repeated.
            SaturationArm("Gun Mount", new int[] { 2, 3, 4, 5, 6 });
            SaturationArm("Autocannon Mount", new int[] { 6, 8, 10, 12, 16 });

            Console.WriteLine();
            Console.WriteLine("  A gun mount takes three hits to destroy. 'arrived' is the average");
            Console.WriteLine("  number of drones per attempt that lived long enough to strike it.");
            Console.WriteLine();
            Console.WriteLine("  Read the two per-kill columns against the two mounts' own prices,");
            Console.WriteLine("  because surviving longer and being worth the money are different");
            Console.WriteLine("  questions and this is the only table in the harness that separates");
            Console.WriteLine("  them. The Autocannon absorbs three times the tap - six drones where");
            Console.WriteLine("  the Gun Mount starts losing at three - and costs three and a half");
            Console.WriteLine("  times as much. At its cheapest it makes an attacker spend about one");
            Console.WriteLine("  and a half times its own price to remove it; the Gun Mount makes him");
            Console.WriteLine("  spend more than twice. So the cheap mount is the better buy per");
            Console.WriteLine("  Materiel and the expensive one is the better buy per position, and");
            Console.WriteLine("  which of those a player wants is a map question rather than a");
            Console.WriteLine("  balance one.");
        }

        /// <summary>One mount type's saturation ladder.</summary>
        static void SaturationArm(string mountDef, int[] counts)
        {
            UnitDef mount = Catalog.Get(Catalog.IdOf(mountDef));
            Console.WriteLine();
            Console.WriteLine(string.Format("  against the {0} - {1} MAT, {2} m of reach, {3} engagements a belt",
                mountDef, mount.CostMateriel, mount.WeaponRangeMetres.RoundToInt(),
                mount.EngagementsPerBelt));
            Console.WriteLine();
            Console.WriteLine("  drones   arrived   gun killed   materiel spent   per kill");
            Console.WriteLine("  " + new string('-', 62));

            for (int c = 0; c < counts.Length; c++)
            {
                int n = counts[c];
                int arrivedTotal = 0, gunKilled = 0;
                const int trials = 60;

                for (int trial = 0; trial < trials; trial++)
                {
                    World w = MakeRealisticWorld(28800, 19200, 256, 32, (ulong)(trial + 1), 2, 0,
                        StandardBorderMetres, 1, 2);
                    int arrived;
                    bool killed = RunAssaultScenario(w, mountDef, MountTweak.None, n,
                                                     F(14400), 0, out arrived);
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
        // MEASURES range: track-hold
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
            Console.WriteLine("  That row is the same 6,600 m mount as the top of (a), on the world every");
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
            Console.WriteLine("  Both columns now run the same way, which they did not in compressed");
            Console.WriteLine("  units: more barrel is better and more camera is better, and the two");
            Console.WriteLine("  meet in the middle of the table. The old sweep read a *shorter*");
            Console.WriteLine("  camera as the better buy - a belt spent at the rim of the envelope");
            Console.WriteLine("  being a belt spent at the worst hit chance the falloff offers - and");
            Console.WriteLine("  that inversion is gone. At real speeds an FPV Team covers the");
            Console.WriteLine("  mount's kill ring in thirty real seconds rather than four, which is");
            Console.WriteLine("  enough time for a second and a third engagement, so a mount that");
            Console.WriteLine("  sees early can now afford to open early. The inversion was a");
            Console.WriteLine("  property of a three-second engagement, not of the falloff.");
            Console.WriteLine();
            Console.WriteLine("  Read (a) and (b) against each other and the binding number is the");
            Console.WriteLine("  seeing, not the barrel: the whole of (b) is fought with a 6,600 m");
            Console.WriteLine("  barrel and still falls by thirty points, while the top half of (a)");
            Console.WriteLine("  is fought with 7,200 m of camera and barely moves until the barrel");
            Console.WriteLine("  drops below what the camera finds. That is FINDINGS 31 general");
            Console.WriteLine("  claim surviving the rescale even though its arithmetic did not.");
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
        // MEASURES approach: track-hold
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
                // 200. This sweep is reporting a transition rather than a level,
                // and at 60 trials the standard error is six points against a
                // transition that turns out to be about twenty-five wide - enough
                // for the middle of the curve to read as non-monotonic noise.
                const int trials = 200;

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
            Console.WriteLine("  * the row that straddles the 1,000 m kill ring.");
            Console.WriteLine();
            Console.WriteLine("  A forward pad is worth something, and it is worth much less than");
            Console.WriteLine("  the map makes it look. Moving the pad from 5,400 m to 300 m - most");
            Console.WriteLine("  of a map width - takes the mount from surviving one attack in five");
            Console.WriteLine("  to surviving none, and nearly all of that is spent in the last");
            Console.WriteLine("  900 m. Above the kill ring the column barely moves, because what");
            Console.WriteLine("  the mount spends is not the crossing time - it has thirty real");
            Console.WriteLine("  seconds of that at every row but the last - it is the seconds");
            Console.WriteLine("  between latching a track and the drone arriving, and those are set");
            Console.WriteLine("  by the ring and not by the pad.");
            Console.WriteLine();
            Console.WriteLine("  The two rows either side of the ring are the defence's best after");
            Console.WriteLine("  the longest standoff, by about two standard errors at 200 trials.");
            Console.WriteLine("  A pad at the rim gives the mount its engagement at maximum range");
            Console.WriteLine("  and its full belt to spend on the approach; a pad inside the rim");
            Console.WriteLine("  gives it neither. Worth a probe before it is worth a conclusion.");
            Console.WriteLine();
            Console.WriteLine("  The arrived column is flat across the whole sweep and that is the");
            Console.WriteLine("  more useful half: a forward pad does not get more drones through, it");
            Console.WriteLine("  gets the same drones through against a mount that has had less");
            Console.WriteLine("  warning. The old pad set - chosen around the mount's long-since-");
            Console.WriteLine("  corrected 550-compressed-metre reach - could show neither.");
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
        // MEASURES night: track-hold, night-optics
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

            // FINDINGS 40. The old ladder was 3, 5, 8, 12 and five of its eight
            // outcome cells read 100%: at five drones and above the mount dies
            // whatever the light is, so three of the four rows were asking a
            // question the first row had already answered. The Gun Mount's
            // transition against a tap runs from two drones to six, so the ladder
            // is the transition now - one rung it always survives, two that move,
            // one it never survives - and the day/night contrast lives in the two
            // in the middle, where it can be read, instead of in one row out of
            // four.
            int[] counts = { 2, 3, 4, 5 };
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
            Console.WriteLine("  The two middle rows are the whole of the measurement: outside");
            Console.WriteLine("  them the tap is either too small to trouble the mount in any");
            Console.WriteLine("  light or large enough to take it in any light, and a row that");
            Console.WriteLine("  reads the same in both columns is not evidence that darkness");
            Console.WriteLine("  costs nothing - it is evidence that the row was chosen badly.");
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
        /// <para><b>On this experiment being flat.</b> The drift guard used to list
        /// this among seven suspected inert; it no longer does, and FINDINGS 40
        /// says why the accusation was wrong about this one and about four others
        /// with it. A mine involves no sensor, no hit roll, no magazine, no
        /// traverse and no arrival order, so none of the faults that killed the
        /// genuinely pinned experiments can reach it, and there is no stochastic
        /// term for a trial count to average over: one run is the answer.
        /// FINDINGS 10 already records it as the only table that came through the
        /// honest re-run untouched, and FINDINGS 40 measured it again from the
        /// other direction - lengthening the track hold, which moves eight of the
        /// ten, correctly moves nothing here.</para>
        ///
        /// <para>Two things did need fixing, and neither was resolution. The
        /// "per mine" column was computed in this harness by calling
        /// <c>Catalog.DamageMultiplier</c> and multiplying - so it re-derived the
        /// simulation's arithmetic instead of observing it, and would have agreed
        /// with a broken <c>World.ApplyDamage</c> exactly as readily as with a
        /// working one. It is now read off the vehicle's health in a running
        /// world. And the indiscriminate rule, which is the entry's whole moral
        /// argument, had no row at all.</para>
        /// </summary>
        // MEASURES mines: mine-arming
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
                // perMine is observed rather than computed, so it can legitimately
                // be zero - a field that never went off took nothing off anything.
                // On the shipped tree it never is, which is why this divided
                // straight into it until the mutation guard broke a mine's fuze and
                // got a DivideByZeroException instead of a table. A harness that
                // crashes rather than reporting "the mechanic did nothing" cannot
                // be asked whether the mechanic matters, which is the one question
                // this experiment exists to answer.
                int eats = atLeast ? 1
                         : perMine <= 0 ? 0
                         : (def.Hp.RoundToInt() + perMine - 1) / perMine;

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
        // MEASURES sensors: track-hold, night-optics
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
        // MEASURES stacking: track-hold
        static void StackingExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("STACKING - more turrets covering the same approach");
            Console.WriteLine("a cluster of mounts all covering one defended point, 450 Materiel each");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  each cell: share of 100 trials in which the attack destroyed the lead");
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
                        // 100, not the 40 this table used to run. At 40 trials a
                        // cell's standard error is about eight points and every
                        // difference this table reports between one mount and
                        // four is smaller than that, so the answer it printed was
                        // noise shaped like a result - the same failure as a
                        // ceiling, one floor down. 100 halves it. It costs about
                        // a minute of build time and buys the only column anyone
                        // reads this experiment for.
                        const int trials = 100;
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
            Console.WriteLine("  A mount is bought to service an engagement, and the rounds column");
            Console.WriteLine("  is where the money goes. Two things changed with the rescale and");
            Console.WriteLine("  they are worth separating.");
            Console.WriteLine();
            Console.WriteLine("  The supporting mounts now shoot in both arms. In compressed units");
            Console.WriteLine("  the third and fourth mounts of a cluster fired 0.00 rounds against a");
            Console.WriteLine("  wave; here they fire about one each. Two causes, and only one of");
            Console.WriteLine("  them is the scale: a 1,000 m kill ring covers the whole cluster");
            Console.WriteLine("  where an 85 m one covered only the mount being attacked, and pad");
            Console.WriteLine("  egress - now wired, four ticks per airframe - means a tap is no");
            Console.WriteLine("  longer one instant. 'Launched together' is a tap, not a wave.");
            Console.WriteLine();
            Console.WriteLine("  Against a tap the stack still does not pay. Four mounts read within");
            Console.WriteLine("  a couple of points of one mount in every column, for 450 Materiel");
            Console.WriteLine("  each, and the rounds column says why: the lead fires its whole belt");
            Console.WriteLine("  either way and each supporting mount gets about one round, because");
            Console.WriteLine("  it commits to a target that is already being engaged and the");
            Console.WriteLine("  engagement is over before it can commit to another.");
            Console.WriteLine();
            Console.WriteLine("  Against a stream it does. Four mounts take six points off the");
            Console.WriteLine("  attack at three drones and twelve at six, and every supporting");
            Console.WriteLine("  mount fires four and a half rounds rather than one. That is the");
            Console.WriteLine("  whole content of FINDINGS 13's structural argument and it is the");
            Console.WriteLine("  first table in this project to show it, because it is the first one");
            Console.WriteLine("  to offer a second engagement for a second mount to service.");
            Console.WriteLine();
            Console.WriteLine("  One row does not fit and is left standing rather than smoothed. In");
            Console.WriteLine("  the stream arm TWO mounts are worse for the defence than one, in");
            Console.WriteLine("  all four columns - 22 against 17, 48 against 37, 71 against 67, 90");
            Console.WriteLine("  against 96 - while three and four are better than either. The cells");
            Console.WriteLine("  share seeds, so the comparison is paired and the standard error is");
            Console.WriteLine("  smaller than the five points 100 trials would suggest, but four");
            Console.WriteLine("  cells one way is suggestive rather than settled. If it is real it is");
            Console.WriteLine("  about engagement commitment - a supporting mount that kills the");
            Console.WriteLine("  drone the lead had committed to costs the lead an acquisition - and");
            Console.WriteLine("  it is worth a probe rather than a paragraph.");
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
            // inside its own 1,000 m envelope. The old layout spread mounts along
            // a line centred on the stack, which moved the lead as the count
            // changed and put the third mount 1,440 m from the engagement, out of
            // reach - so two turrets and three turrets measured identically to
            // the digit for a reason that had nothing to do with stacking.
            //
            // Every supporting mount is beside or behind the lead, never in front
            // of it. A mount 480 m forward of the one being defended measured as
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
        // MEASURES vertical: track-hold, radar-notch, radar-dark
        static void VerticalExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("VERTICAL - five Multirole Quads, split between altitudes");
            Console.WriteLine("one tap every 3 s of play, because a band change is only charged between");
            Console.WriteLine("engagements and a simultaneous wave only ever offers one");
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("                          Gun Mount (1,000 m,        Autocannon Mount");
            Console.WriteLine("                          cannot reach high)         (3,360 m, reaches high)");
            Console.WriteLine("  attack                  quads lost   mount killed  quads lost   mount killed");
            Console.WriteLine("  " + new string('-', 78));

            // Five, not three. Against the Autocannon Mount three quads lose 2.90
            // of themselves on average - a loss column pinned within a tenth of
            // its own ceiling, which is the FINDINGS 32 shape however live the
            // cell beside it looks. Five leaves the defence something to fail at.
            int[][] splits = {
                new int[] {5, 0}, new int[] {3, 2}, new int[] {2, 3}, new int[] {0, 5}
            };
            string[] labels = {
                "all low", "three low, two high", "two low, three high", "all high"
            };
            string[] mounts = { "Gun Mount", "Autocannon Mount" };

            for (int i = 0; i < splits.Length; i++)
            {
                string row = string.Format("  {0,-22}", labels[i]);
                for (int m = 0; m < mounts.Length; m++)
                {
                    int lostTotal = 0, killed = 0;
                    const int trials = 100;
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
            Console.WriteLine("  drone in the flight takes the mount's score to almost exactly zero");
            Console.WriteLine("  and keeps it there. Its 100% column is a ceiling and carries no");
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
        /// 3,840 m - the Interceptor Battery's, the longest in the roster. Every
        /// sensor in the roster finds every airframe in the roster at 3,840 m. So
        /// a decoy detected at 28 km instead of 9 cannot
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
        // MEASURES decoys: radar-notch, radar-dark
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
            // SimConstants.MunitionMissRadiusMetres after 1,333 m of denied
            // flight - three percent of distance flown against a 40 m radius -
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
        // MEASURES aperture: track-hold
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
            Console.WriteLine("  which nothing can currently arrange - survives no more often; on");
            Console.WriteLine("  these numbers it survives slightly less. Above");
            Console.WriteLine("  about 840 m the extra reach buys a mount nothing, because 840 m is");
            Console.WriteLine("  where it first holds a track and about 25 real seconds is all it has");
            Console.WriteLine("  after that (FINDINGS 31, in real metres). The panoramic head is not the compromise position on this slider;");
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
        // The three experiments FINDINGS 40 specified and did not write.
        //
        // All three exist because of one measurement: FINDINGS 40 broke the
        // constants behind ground radar, emission control, interception
        // vectoring and fiber, one at a time, and every one of the ten
        // experiments above printed identical bytes. Not thin coverage - none.
        // The three below are the coverage, and each is written to the standard
        // that finding implies: it has to move when the thing it measures is
        // broken, and tools/check_experiment_mutations.py is what says whether
        // it does. The `// MEASURES` line above each one is the claim; the guard
        // is what makes the claim cost something.

        /// <summary>
        /// A radar mast against something on the ground, swept by what the
        /// vehicle is doing rather than by how far away it is.
        ///
        /// <para><b>Why the sweep is a velocity and not a range.</b> FINDINGS 38
        /// wired two mechanics and measured both on one geometry: radar can see
        /// the ground at all (<c>SimConstants.RadarGroundScale</c>), and the
        /// Doppler notch gates a return by its <i>radial</i> velocity rather
        /// than its size. The second is not a stat, it is a geometry: the same
        /// tank at the same range is a loud return driving at the mast, a weak
        /// one driving obliquely, and no return at all driving across its face
        /// or sitting still. A range sweep cannot see any of that, which is why
        /// this one sweeps the vehicle's closest approach and its heading and
        /// holds its speed at the one the catalogue gives it.</para>
        ///
        /// <para><b>No trial loop, and it is not an oversight.</b> Nothing here
        /// draws from a random stream. The edge-of-envelope roll in
        /// <c>World.Reaches</c> is keyed to the tick and the sensor-target pair
        /// rather than drawn from <c>DetRandom</c> - deliberately, so that one
        /// target does not resolve differently for two sensors in the same
        /// instant - and movement and detection are otherwise arithmetic. So a
        /// second trial of the same drive is the same drive, and averaging sixty
        /// of them would be averaging one number with itself. MINES is flat for
        /// the same kind of reason and says so in the same place.</para>
        ///
        /// <para><b>What the parked column is.</b> A constant zero, in every
        /// row, and it is the comparison rather than a defect: a vehicle at rest
        /// has no radial component, so the notch is a hard gate on it whatever
        /// its cross-section and wherever it is standing. That is the mechanic's
        /// whole claim, and the two live columns beside it are read against it.
        /// It is excused in writing in tools/experiment-ceiling-allow.txt rather
        /// than smoothed away, by value, so the day it moves the warning comes
        /// back.</para>
        /// </summary>
        // MEASURES radar: radar-notch, radar-slow-band, radar-ground-clutter, radar-dark, emission-order
        static void RadarGroundExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("RADAR - a mast against a vehicle on the ground");
            Console.WriteLine("Radar Mast at 14,400 m; a Main Tank at its catalogue 7.5 m/s driving a");
            Console.WriteLine("12,000 m leg past it. The sweep is the vehicle's closest approach and its");
            Console.WriteLine("heading, because what gates this channel is radial velocity and not range");
            Console.WriteLine(string.Format("the mast holds a tank closing head-on out to {0} m, and a tank "
                                          + "crossing its face at 0 m", RadarClosingReach().RoundToInt()));
            Console.WriteLine("one run per cell, not sixty: nothing in this engagement draws from a");
            Console.WriteLine("random stream, so a second trial is the same drive (see the comment)");
            PrintWorldConfig(StandardBorderMetres, 1, 2);

            Console.WriteLine();
            Console.WriteLine("  (a) the notch. Each cell is the share of the drive's ticks the mast");
            Console.WriteLine("  holds the tank at all.");
            Console.WriteLine();
            Console.WriteLine("  closest approach   driving in   driving across   parked");
            Console.WriteLine("  " + new string('-', 62));

            // Rungs picked to bracket the mast's own reach against a closing tank
            // rather than to spread evenly: 300 m is inside everything and 10,800 m
            // is outside the medium band's reach, so the sweep has one row that is
            // held whatever the heading and one that is held on no heading at all,
            // which is what bracketing a transition means (FINDINGS 40).
            int[] offsets = { 300, 1200, 2400, 3600, 4800, 6000, 7200 };
            for (int o = 0; o < offsets.Length; o++)
            {
                Console.WriteLine(string.Format("  {0,16}   {1,10}   {2,14}   {3,6}",
                    offsets[o] + " m",
                    RadarPass(RadarDrive.In, offsets[o], true) + "%",
                    RadarPass(RadarDrive.Across, offsets[o], true) + "%",
                    RadarPass(RadarDrive.Parked, offsets[o], true) + "%"));
            }

            Console.WriteLine();
            Console.WriteLine("  The parked column is the notch in its pure form: zero at every range,");
            Console.WriteLine("  because a vehicle at rest has no radial component and no cross-section");
            Console.WriteLine("  and no proximity can buy its way out of a hard gate. That is the one");
            Console.WriteLine("  cell-for-cell comparison in the table - same position, same tank, same");
            Console.WriteLine("  mast, nothing different but whether the engine is running.");
            Console.WriteLine();
            Console.WriteLine("  The two moving columns are NOT a controlled comparison of heading and");
            Console.WriteLine("  should not be read as one. Both legs are 12,000 m, but a leg driven at");
            Console.WriteLine("  the mast spans every range from 13 km down to the stated one while a");
            Console.WriteLine("  leg driven across its face never leaves the stated one by more than");
            Console.WriteLine("  half its length, so the crossing column is fought much closer in. That");
            Console.WriteLine("  is why it overtakes the closing column past about 3 km rather than");
            Console.WriteLine("  being quieter everywhere: inside the envelope the notch costs the");
            Console.WriteLine("  crosser most of its contact, and outside it the closer has no contact");
            Console.WriteLine("  to lose. Both facts are about a real manoeuvre and neither is about");
            Console.WriteLine("  the other.");
            Console.WriteLine();
            Console.WriteLine("  The crossing column peaking at 3,600 m is the sharpest thing here and");
            Console.WriteLine("  is left standing rather than smoothed. Nearer than that, a crossing");
            Console.WriteLine("  tank spends most of its leg inside the angle where the radial falls");
            Console.WriteLine("  into the 1.5-4 m/s band - short reach and 30% reliability, so the");
            Console.WriteLine("  contact is both closer and intermittent. Further out, the band is");
            Console.WriteLine("  irrelevant because the whole leg is past what the mast reaches. 3,600 m");
            Console.WriteLine("  is where the geometry puts the whole crossing inside the medium band");
            Console.WriteLine("  and inside the envelope at once, and it is the best place on this map");
            Console.WriteLine("  to be a tank the mast can see.");

            Console.WriteLine();
            Console.WriteLine("  (b) emission control - FINDINGS 37's order, issued through");
            Console.WriteLine("  Command.SetEmitting and never before in a balance table. Two rows and");
            Console.WriteLine("  not a sweep, and the comment on this method says why.");
            Console.WriteLine();
            Console.WriteLine("  the mast is    heard at   a listener at 3,000 m holds it   its own pass");
            Console.WriteLine("  " + new string('-', 76));
            Console.WriteLine(string.Format("  {0,-13}  {1,8}   {2,29}   {3,12}",
                "radiating", RadarHeardAt(true).RoundToInt() + " m",
                RadarHeard(3000, true) + "% of ticks",
                RadarPass(RadarDrive.In, 1200, true) + "%"));
            Console.WriteLine(string.Format("  {0,-13}  {1,8}   {2,29}   {3,12}",
                "dark", RadarHeardAt(false).RoundToInt() + " m",
                RadarHeard(3000, false) + "% of ticks",
                RadarPass(RadarDrive.In, 1200, false) + "%"));
            Console.WriteLine();
            Console.WriteLine("  'heard at' is how far an EW Truck's passive listening reaches the mast,");
            Console.WriteLine("  read off a spawned truck in a stepped world. 'its own pass' is the");
            Console.WriteLine("  1,200 m closing drive from (a), so the last column is what the order");
            Console.WriteLine("  costs and the first two are what it buys.");
            Console.WriteLine();
            Console.WriteLine("  Switching off does not hide the mast. It moves the range at which the");
            Console.WriteLine("  other side hears it from a little over four kilometres to a little");
            Console.WriteLine("  under two and a half - a factor of about 1.8 - because a quiet mast is");
            Console.WriteLine("  still a structure with a radio signature of its own. What it pays for");
            Console.WriteLine("  that is everything: against a ground target a dark mast holds nothing");
            Console.WriteLine("  at any range on any heading, because its only other sensor is passive");
            Console.WriteLine("  RF and a Main Tank's radio signature is zero. So the order is not a");
            Console.WriteLine("  posture a player can hold while still working - it is a decision to");
            Console.WriteLine("  stop seeing in exchange for halving the radius inside which somebody");
            Console.WriteLine("  can find you.");
        }

        enum RadarDrive { In, Across, Parked }

        /// <summary>
        /// The mast's own position in the radar experiment. Far enough west of
        /// StandardBorderMetres that the whole leg is on one side of the line, so
        /// territory is not quietly a second variable.
        /// </summary>
        const int RadarMastX = 14400;
        const int RadarMastY = 9360;

        /// <summary>
        /// The leg, in metres and in ticks. 12,000 m at the Main Tank's 7.5 m/s is
        /// 1,600 real seconds; the loop is given a quarter more than that so a
        /// drive that is slowed by terrain still finishes inside it, and the share
        /// is taken over the ticks actually driven rather than over the budget.
        /// </summary>
        const int RadarLegMetres = 12000;

        /// <summary>
        /// One drive past the mast, and the share of its ticks the mast held the
        /// tank. Closest approach is the same quantity in all three modes: the
        /// drive-in leg ends abeam at that distance, the crossing leg passes abeam
        /// at it at its midpoint, and the parked vehicle simply stands at it.
        /// </summary>
        static int RadarPass(RadarDrive mode, int closestApproach, bool radiating)
        {
            World w = MakeRealisticWorld(28800, 19200, 64, 4, 1, 2, 0,
                StandardBorderMetres, 1, 2);

            EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 2, P(RadarMastX, RadarMastY));
            if (!radiating) w.Enqueue(Command.SetEmitting(2, mast, false));

            Fix2 start, destination;
            switch (mode)
            {
                case RadarDrive.In:
                    // From 12,000 m out on the mast's own axis, ending abeam.
                    start = P(RadarMastX - RadarLegMetres, RadarMastY + closestApproach);
                    destination = P(RadarMastX, RadarMastY + closestApproach);
                    break;
                case RadarDrive.Across:
                    // Perpendicular to the bearing, abeam at the midpoint.
                    start = P(RadarMastX + closestApproach, RadarMastY - RadarLegMetres / 2);
                    destination = P(RadarMastX + closestApproach, RadarMastY + RadarLegMetres / 2);
                    break;
                default:
                    start = P(RadarMastX + closestApproach, RadarMastY);
                    destination = start;
                    break;
            }

            EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 1, start);
            if (mode != RadarDrive.Parked)
                w.Enqueue(Command.MoveTo(1, tank, destination));

            // The leg's own duration, plus a quarter. A Main Tank is LinkKind.None
            // - a crewed vehicle, not a robot - so nothing stops it for want of a
            // command post, and the drive is the whole of the trial.
            int budget = (RadarLegMetres * SimConstants.TicksPerRealSecond * 5) / (4 * 7)
                       + SimConstants.TicksPerSecond;
            int held = 0, ticks = 0;
            for (int t = 0; t < budget; t++)
            {
                w.Step();
                ticks++;
                if (w.IsDetectedBy(2, tank)) held++;
                if (mode != RadarDrive.Parked
                    && Fix2.Distance(w.Entities.Position[tank.Index], destination) < F(48)) break;
            }
            return ticks > 0 ? (held * 100) / ticks : 0;
        }

        /// <summary>
        /// How far the mast holds a tank that is closing on it head-on, read off a
        /// driving vehicle rather than off the formula: the tank is given a moment
        /// to be moving before the range is asked for, because a stationary tank
        /// returns zero and that is the mechanic rather than a measurement error.
        /// </summary>
        static Fix RadarClosingReach()
        {
            World w = MakeRealisticWorld(28800, 19200, 64, 4, 1, 2, 0,
                StandardBorderMetres, 1, 2);
            EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 2, P(RadarMastX, RadarMastY));
            EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 1, P(RadarMastX - 6000, RadarMastY));
            w.Enqueue(Command.MoveTo(1, tank, P(RadarMastX, RadarMastY)));
            for (int t = 0; t < SimConstants.TicksPerSecond * 4; t++) w.Step();
            return w.DetectionRangeFor(mast.Index, tank.Index, SensorChannel.Radar);
        }

        /// <summary>
        /// The other half of the emission-control trade: how much of a run the
        /// listening side holds the mast, with the mast transmitting and with it
        /// told to stop. Nothing drives here - this is about the mast's own
        /// signature, which is what the order changes.
        /// </summary>
        /// <remarks>
        /// Swept across the listener's range first, and it would not resolve:
        /// passive listening is the most reliable channel in the model, so the
        /// share of ticks is 100% out to the reach and 0% past it with almost
        /// nothing in between, and a column of 100s and 0s is FINDINGS 32's
        /// ceiling however many rungs it has. The quantity that actually moves is
        /// the reach itself, so that is what is printed, at one range where the
        /// two answers differ.
        /// </remarks>
        static int RadarHeard(int listenerRange, bool radiating)
        {
            World w = MakeRealisticWorld(28800, 19200, 64, 4, 1, 2, 0,
                StandardBorderMetres, 1, 2);
            EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 2, P(RadarMastX, RadarMastY));
            if (!radiating) w.Enqueue(Command.SetEmitting(2, mast, false));
            w.Spawn(Catalog.IdOf("EW Truck"), 1, P(RadarMastX - listenerRange, RadarMastY));

            // Long enough for the order to be executed, the emitter signature to
            // be rebuilt and the edge roll in World.Reaches to have been asked a
            // few hundred times - the share is what that roll produces, so a run
            // of one tick would report a coin toss as a balance figure.
            int held = 0, ticks = SimConstants.TicksPerSecond * 20;
            for (int t = 0; t < ticks; t++)
            {
                w.Step();
                if (w.IsDetectedBy(1, mast)) held++;
            }
            return (held * 100) / ticks;
        }

        /// <summary>
        /// How far an EW Truck's passive listening reaches the mast, with the
        /// mast transmitting and with it dark. Read off a spawned truck after the
        /// order has been executed and the emitter signature rebuilt, not
        /// computed: the boost a radiating emitter takes is
        /// UnitDef.SignatureWhileEmitting and this harness should not be
        /// re-deriving it (MINES made that mistake with the damage table).
        /// </summary>
        static Fix RadarHeardAt(bool radiating)
        {
            World w = MakeRealisticWorld(28800, 19200, 64, 4, 1, 2, 0,
                StandardBorderMetres, 1, 2);
            EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 2, P(RadarMastX, RadarMastY));
            if (!radiating) w.Enqueue(Command.SetEmitting(2, mast, false));
            EntityHandle truck = w.Spawn(Catalog.IdOf("EW Truck"), 1, P(RadarMastX - 3000, RadarMastY));
            for (int t = 0; t < 4; t++) w.Step();
            return w.DetectionRangeFor(truck.Index, mast.Index, SensorChannel.Esm);
        }

        // ------------------------------------------------------------------

        /// <summary>
        /// One interceptor against one incoming airframe, swept by how fast the
        /// airframe is and how far off the interceptor's pad sits from its track.
        ///
        /// <para><b>What it is for.</b> FINDINGS 36 built the vectoring - cue,
        /// solution, fly to the meeting point - and FINDINGS 40 found that
        /// nothing in this harness had ever launched an airframe that flies one.
        /// <c>decoys</c> spawns an Interceptor Battery, which is a structure: it
        /// rolls <c>ResolveInterception</c> at the merge and never flies to one.
        /// So <c>InterceptLeadRadarTrack</c> and <c>InterceptLeadOpticalTrack</c>
        /// could both be set to 0.05 - the mechanic switched off - with no
        /// experiment printing a different byte. This is the sweep that makes the
        /// lead table cost something.</para>
        ///
        /// <para><b>Why the two arms are a radar and no radar.</b> How much of
        /// the computed lead an interceptor actually flies is decided by
        /// <c>World.TrackQualityOf</c>: a radar track buys the whole solution, an
        /// optical one buys 0.55 of it, and a bearing buys none. That is the
        /// entire content of the lead table, so the honest sweep of it is the
        /// same geometry with the mast there and with it absent, and the gap
        /// between the two arms is what the mast is worth.</para>
        ///
        /// <para><b>And one correction to FINDINGS 40's specification, found in
        /// the code rather than argued.</b> That entry says this experiment would
        /// be "the first experiment in which <c>AirHitChance</c>'s speed term is
        /// off its clamp". It would not have been: <c>CombatSystem</c> branches
        /// to <c>ResolveInterception</c> before <c>AirHitChance</c> is reached,
        /// so an interceptor never scores an air hit roll at all - which FINDINGS
        /// 40 itself says two sections earlier. An interceptor experiment alone
        /// leaves the speed term exactly where it found it. Arm (c) is the
        /// repair: the same four airframes against an Autocannon Mount, which is
        /// not an interceptor and therefore does score that roll, and which at
        /// 140 m/s is the first thing in this harness to take the term off the
        /// 1.15 ceiling it has sat on since it was written.</para>
        /// </summary>
        // MEASURES intercept: intercept-lead-radar, intercept-lead-optical, firing-solution-speed
        static void InterceptionExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("INTERCEPTION - one Interceptor FPV against one incoming airframe");
            Console.WriteLine("the target flies west to east at a structure at 21,600 m; the defence");
            Console.WriteLine("scrambles one interceptor from a pad at 19,800 m, offset from the track");
            Console.WriteLine("by the column heading. 85 m/s against targets from 28 to 140");
            PrintWorldConfig(StandardBorderMetres, 1, 2);

            // The speed ladder is the catalogue's, not an invented one, and the
            // two rungs in the middle are chosen for where they sit relative to
            // AirHitChance's clamp: the speed term is reference/speed clamped at
            // 1.15, so it is pinned for anything under 39 m/s and live above it.
            // The Loitering Munition at 36 and the Mid-Range Striker at 45 sit
            // either side of that line. The gap from 50 to 140 is the roster's
            // own - there is nothing in the catalogue between them - and it is
            // why the jet reads as a different kind of problem rather than a
            // harder version of the same one.
            string[] targets = { "Multirole Quad", "Loitering Munition", "Mid-Range Striker",
                                 "Heavy Strike Drone", "Jet Strike Drone" };
            int[] offsets = { 0, 900, 1800, 3000 };

            for (int arm = 0; arm < 2; arm++)
            {
                bool radar = arm == 0;
                Console.WriteLine();
                Console.WriteLine(radar
                    ? "  (a) with a Radar Mast radiating behind the defence"
                    : "  (b) the same, with no radar on the defence at all");
                Console.WriteLine();
                Console.Write("  target                     ");
                for (int o = 0; o < offsets.Length; o++)
                    Console.Write(string.Format("{0,12}",
                        offsets[o] == 0 ? "on track" : offsets[o] + " m off"));
                Console.WriteLine();
                Console.WriteLine("  " + new string('-', 76));

                for (int t = 0; t < targets.Length; t++)
                {
                    UnitDef def = Catalog.ByName(targets[t]);
                    Console.Write(string.Format("  {0,-18} {1,3} m/s ",
                        targets[t], def.SpeedMetresPerSecond.RoundToInt()));
                    for (int o = 0; o < offsets.Length; o++)
                        Console.Write(string.Format("{0,12}",
                            InterceptRate(targets[t], offsets[o], radar) + "%"));
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("  (c) the same five airframes against an Autocannon Mount, which is not");
            Console.WriteLine("  an interceptor - it shoots, so it scores AirHitChance rather than");
            Console.WriteLine("  ResolveInterception. This is the only column in the harness where");
            Console.WriteLine("  that roll's speed term is off its clamp (FINDINGS 40's fifth finding:");
            Console.WriteLine("  every airframe the harness had ever flown at a gun was slow enough to");
            Console.WriteLine("  land on the 1.15 ceiling, so the term could have been deleted).");
            Console.WriteLine();
            Console.WriteLine("  target                      mount kills it   speed term");
            Console.WriteLine("  " + new string('-', 62));
            for (int t = 0; t < targets.Length; t++)
            {
                UnitDef def = Catalog.ByName(targets[t]);
                Fix raw = SimConstants.FiringSolutionReferenceSpeed / def.SpeedMetresPerSecond;
                bool clamped = raw > Fix.FromDoubleContentOnly(1.15);
                Console.WriteLine(string.Format("  {0,-18} {1,3} m/s   {2,12}   {3}",
                    targets[t], def.SpeedMetresPerSecond.RoundToInt(),
                    GunKillRate(targets[t]) + "%",
                    clamped ? "on the clamp" : "live"));
            }

            Console.WriteLine();
            Console.WriteLine("  The first four rows do not move across the offset columns at all,");
            Console.WriteLine("  and that is the result rather than a broken table: against anything");
            Console.WriteLine("  under about 50 m/s an 85 m/s interceptor closes by pursuit whatever");
            Console.WriteLine("  it was told and from wherever it started, so where the pad sits is");
            Console.WriteLine("  worth nothing. They do move between the arms - 53% with a mast and");
            Console.WriteLine("  40% without, in every cell - and that 13 points is the cue");
            Console.WriteLine("  multiplier and not the lead: the interceptor was going to arrive");
            Console.WriteLine("  either way, and what the mast changes is the roll it makes when it");
            Console.WriteLine("  gets there.");
            Console.WriteLine();
            Console.WriteLine("  The Multirole Quad is the exception and it is a signature and not a");
            Console.WriteLine("  speed. Its radar cross-section is 26 against the Heavy Strike Drone's");
            Console.WriteLine("  52, and the mast does not hold it at the merge at all, so its two");
            Console.WriteLine("  arms are the same number. A radar mast is not air defence for small");
            Console.WriteLine("  drones. It is air defence for big ones, which is the roster saying");
            Console.WriteLine("  out loud what it has always implied.");
            Console.WriteLine();
            Console.WriteLine("  The jet row is the experiment. Read it two ways. Across, the offset");
            Console.WriteLine("  columns fall away to nothing: a pad off the raid's track has to make");
            Console.WriteLine("  the lateral distance up out of a closing budget it does not have, and");
            Console.WriteLine("  at 3,000 m off it never arrives. Down, the two arms differ by about");
            Console.WriteLine("  a third at every offset the interceptor can reach at all, and that");
            Console.WriteLine("  gap is the mast: a radar track flies the whole computed lead and");
            Console.WriteLine("  scores the merge at 1.00, an optical one flies 0.55 of it and scores");
            Console.WriteLine("  at 0.60. Against a 140 m/s target the missing 45% of the lead is most");
            Console.WriteLine("  of a kilometre of aimpoint.");
            Console.WriteLine();
            Console.WriteLine("  So the sentence the roster has always implied is now measured: the");
            Console.WriteLine("  interceptor is a good buy against everything it can catch and a");
            Console.WriteLine("  coin-toss against the one thing it was bought for, and the coin is");
            Console.WriteLine("  weighted by whether a mast is up.");
        }

        /// <summary>
        /// One scramble, repeated. The share of trials in which the interceptor
        /// removes the incoming airframe before it reaches the thing it was sent
        /// at. <c>ResolveInterception</c> draws from RandomStream.Interception, so
        /// unlike the radar sweep above this one genuinely needs trials.
        /// </summary>
        static int InterceptRate(string targetDefName, int padOffsetMetres, bool radar)
        {
            // 60. The differences this table reports between its two arms are
            // tens of points, so the six-point standard error 60 trials carries is
            // comfortably inside them; the differences between adjacent offset
            // columns are smaller and are read as a shape rather than cell by cell.
            const int trials = 60;
            int killed = 0;
            for (int trial = 0; trial < trials; trial++)
                if (RunIntercept(targetDefName, padOffsetMetres, radar, (ulong)(trial + 1)))
                    killed++;
            return killed * 100 / trials;
        }

        static bool RunIntercept(string targetDefName, int padOffsetMetres, bool radar, ulong seed)
        {
            World w = MakeRealisticWorld(28800, 19200, 128, 8, seed, 2, 0,
                StandardBorderMetres, 1, 2);

            // The attacker: enough rear to launch one airframe and keep it linked.
            w.Player(1).Materiel = Fix.FromInt(100000);
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(2400, 9360));
            for (int q = 0; q < 3; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(1800 + q * 480, 10800));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(9600, 9360));

            // The defence: the thing being flown at, a rear to launch from, and -
            // in arm (a) only - the mast that turns a contact into a solution.
            w.Player(2).Materiel = Fix.FromInt(100000);
            EntityHandle objective = w.Spawn(Catalog.IdOf("Command Post"), 2, P(21600, 9360));
            for (int q = 0; q < 3; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 2, P(22800 + q * 480, 10800));
            w.Spawn(Catalog.IdOf("Relay Mast"), 2, P(20400, 9360));
            if (radar) w.Spawn(Catalog.IdOf("Radar Mast"), 2, P(21000, 9360));

            int targetDef = Catalog.IdOf(targetDefName);
            int interceptorDef = Catalog.IdOf("Interceptor FPV");
            w.Enqueue(Command.LaunchSortie(1, targetDef, P(12000, 9360), objective, 0));

            EntityHandle target = EntityHandle.None;
            bool targetSeen = false;
            bool scrambled = false;
            Fix2 pad = P(19800, 9360 + padOffsetMetres);

            for (int tick = 0; tick < 600 * SimConstants.TicksPerSecond; tick++)
            {
                w.Step();

                // Whether the interceptor got it, asked of the simulation rather
                // than inferred from the airframe having disappeared. That
                // distinction is the whole measurement here: every target in this
                // table is OneWay, so it kills itself the instant it strikes, and
                // a first version of this counted "the target is no longer alive"
                // and reported 100% for every armed airframe in every cell - an
                // experiment measuring the roster's suicide rule and calling it
                // interception. UnitDied carries the killer in B, so the question
                // has a real answer: A is the raid and B is who removed it.
                for (int e = 0; e < w.Events.Count; e++)
                {
                    SimEvent ev = w.Events[e];
                    if (ev.Kind != SimEventKind.UnitDied) continue;
                    if (targetSeen && ev.A == target)
                        return ev.B != EntityHandle.None
                            && ev.B.Index < w.Entities.Capacity
                            && w.Entities.DefId[ev.B.Index] == interceptorDef;
                }

                if (!targetSeen)
                {
                    // The airframe, once it has left the pad. Index order is
                    // launch order for the length of a trial (World.Spawn hands
                    // out ascending slots) and there is exactly one team-1 sortie.
                    for (int i = 1; i < w.Entities.HighWater; i++)
                    {
                        if (!w.Entities.IsSlotAlive(i)) continue;
                        if (w.Entities.Team[i] != 1) continue;
                        if (!w.Entities.Has(i, ComponentMask.Sortie)) continue;
                        if (w.Entities.DefId[i] != targetDef) continue;
                        target = w.Entities.HandleAt(i);
                        targetSeen = true;
                        break;
                    }
                    continue;
                }

                if (!w.Entities.IsAlive(target)) return false;

                // Scramble when the raid is 6,000 m out, which is a scramble on
                // warning rather than a standing patrol: the same trigger for
                // every target, so the column that varies is the geometry and not
                // how much notice the defence was given.
                if (!scrambled
                    && Fix2.Distance(w.Entities.Position[target.Index],
                                     w.Entities.Position[objective.Index]) < F(6000))
                {
                    w.Enqueue(Command.LaunchSortie(2, interceptorDef, pad, target, 0));
                    scrambled = true;
                }

                if (!w.Entities.IsAlive(objective)) return false;
            }
            return false;
        }

        /// <summary>
        /// The same four airframes flown past an Autocannon Mount instead. The
        /// mount is static and does not chase, so there is no offset axis: what is
        /// being read is the hit roll, which is the half of the air-defence model
        /// no interceptor ever reaches.
        /// </summary>
        static int GunKillRate(string targetDefName)
        {
            const int trials = 60;
            int killed = 0;
            int targetDef = Catalog.IdOf(targetDefName);
            for (int trial = 0; trial < trials; trial++)
            {
                World w = MakeRealisticWorld(28800, 19200, 128, 8, (ulong)(trial + 1), 2, 0,
                    StandardBorderMetres, 1, 2);
                w.Player(1).Materiel = Fix.FromInt(100000);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(2400, 9360));
                for (int q = 0; q < 3; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(1800 + q * 480, 10800));
                w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(9600, 9360));

                EntityHandle mount = w.Spawn(Catalog.IdOf("Autocannon Mount"), 2, P(19800, 9360));
                w.Enqueue(Command.LaunchSortie(1, targetDef, P(12000, 9360), mount, 0));

                EntityHandle target = EntityHandle.None;
                bool seen = false;
                for (int tick = 0; tick < 600 * SimConstants.TicksPerSecond; tick++)
                {
                    w.Step();

                    // The mount's kill, not the airframe's disappearance - same
                    // reason as RunIntercept, and here it matters even more: the
                    // airframes this arm is about are one-way, so "it is gone"
                    // and "the mount got it" are different questions with very
                    // different answers.
                    bool done = false;
                    for (int e = 0; e < w.Events.Count; e++)
                    {
                        SimEvent ev = w.Events[e];
                        if (ev.Kind != SimEventKind.UnitDied) continue;
                        if (seen && ev.A == target)
                        {
                            if (ev.B == mount) killed++;
                            done = true;
                        }
                    }
                    if (done) break;

                    if (!seen)
                    {
                        for (int i = 1; i < w.Entities.HighWater; i++)
                        {
                            if (!w.Entities.IsSlotAlive(i)) continue;
                            if (w.Entities.Team[i] != 1) continue;
                            if (!w.Entities.Has(i, ComponentMask.Sortie)) continue;
                            if (w.Entities.DefId[i] != targetDef) continue;
                            target = w.Entities.HandleAt(i);
                            seen = true;
                            break;
                        }
                        continue;
                    }
                    if (!w.Entities.IsAlive(target)) break;
                    if (!w.Entities.IsAlive(mount)) break;
                }
            }
            return killed * 100 / trials;
        }

        // ------------------------------------------------------------------

        /// <summary>
        /// What the thread costs, swept by how far back the launch pad is.
        ///
        /// <para><b>What it is for.</b> FINDINGS 39 built fiber's third
        /// liability - a filament lying across the map that an enemy vehicle can
        /// drive onto, giving that enemy a bearing home - and found by hand, on
        /// the shipped scenario's one geometry, that it makes fiber "a decision
        /// about where you launch": from the forward pad the thread costs the
        /// player a relay mast, from the rear pad it costs a log line. FINDINGS
        /// 40 then found that no experiment in this harness had ever spawned a
        /// Fiber FPV Team at all, and that the discovery range could be
        /// multiplied by ten without anything printing a different byte. This is
        /// that decision turned into a sweep.</para>
        ///
        /// <para><b>The control column is the point of the table.</b> A launch
        /// site is not secret: from a forward pad the defence's own sensors are
        /// standing on it anyway, and the thread tells it nothing it did not
        /// know. So the same sortie is flown twice from each pad - once on fiber
        /// and once on radio, which drags no thread - and the difference between
        /// the two "site held" columns is the price of the filament, isolated
        /// from what the defence would have seen regardless. Without that column
        /// the fiber figure is a statement about how far the defence's cameras
        /// reach.</para>
        ///
        /// <para><b>Where the trial variance comes from.</b> The defence's tank
        /// patrols a lane, and each trial starts it at a different point on that
        /// lane. That is the honest variable: a thread is found because somebody
        /// happened to drive over it, so whether one is found is a question about
        /// where the traffic was when you launched. Seeding alone would not do
        /// it - discovery is pure geometry, with no roll anywhere in it, which is
        /// deliberate (FINDINGS 39: no stream gains a draw and no recorded result
        /// reorders).</para>
        /// </summary>
        // MEASURES fiber: tether-discovery-range, tether-reveal-radius, spool-length
        static void FiberExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("FIBER - the same sortie flown from further and further back");
            Console.WriteLine("one Fiber FPV Team at a structure at 21,600 m, launched from a pad the");
            Console.WriteLine("column below names, with a Relay Mast 600 m behind that pad as the");
            Console.WriteLine("launch site. The defence's Main Tank patrols a lane across the approach");
            Console.WriteLine("at 18,600 m; each trial starts it at a different point on that lane");
            Console.WriteLine(string.Format("the airframe carries {0} m of spool and flies at 22 m/s",
                Catalog.ByName("Fiber FPV Team").SpoolLengthMetres.RoundToInt()));
            PrintWorldConfig(StandardBorderMetres, 1, 2);
            Console.WriteLine();
            Console.WriteLine("  Each cell is a share of 40 trials. 'site held' is whether the defence");
            Console.WriteLine("  ever holds a contact on the relay mast standing at the launch pad -");
            Console.WriteLine("  asked of World.TrackQualityOf, so it is the simulation's answer and");
            Console.WriteLine("  not this harness's arithmetic. 'thread parts after' is how much line");
            Console.WriteLine("  was out when it was cut, read off the tether.");

            for (int arm = 0; arm < 2; arm++)
            {
                bool road = arm == 0;
                Console.WriteLine();
                Console.WriteLine(road
                    ? "  (a) flown down the road, which is the route every other experiment in"
                    : "  (b) the same sortie flown over open ground, 1,700 m north of the road");
                Console.WriteLine(road
                    ? "      this file stages on"
                    : "");
                Console.WriteLine();
                Console.WriteLine("  pad at    out from target   parts after   thread found   site held   site held");
                Console.WriteLine("                                                           on fiber    on radio");
                Console.WriteLine("  " + new string('-', 88));

                int[] standoffs = { 3000, 6000, 9000, 12000, 15000, 18000 };
                for (int i = 0; i < standoffs.Length; i++)
                {
                    int found, heldFiber, heldRadio, spool;
                    RunFiberSweep(standoffs[i], road, out found, out heldFiber,
                                  out heldRadio, out spool);
                    Console.WriteLine(string.Format(
                        "  {0,7}   {1,14}   {2,11}   {3,12}   {4,9}   {5,9}",
                        (FiberObjectiveX - standoffs[i]) + " m",
                        standoffs[i] + " m",
                        spool + " m",
                        found + "%", heldFiber + "%", heldRadio + "%"));
                }
            }

            Console.WriteLine();
            Console.WriteLine("  The two arms are the result, and they are not what this experiment");
            Console.WriteLine("  was written to measure. A filament dragged down a road parts after");
            Console.WriteLine("  about 1,500 m whatever the airframe is carrying: Terrain's snag rate");
            Console.WriteLine("  is 1.5% per real second on a road and zero on open ground, and at");
            Console.WriteLine("  22 m/s that is a thread with an expected life of a kilometre and a");
            Console.WriteLine("  half. Over open ground it does not part at all until the spool runs");
            Console.WriteLine("  out. So the 16,800 m in the catalogue is not the leash on the route");
            Console.WriteLine("  every other experiment in this file flies - there the thread is gone");
            Console.WriteLine("  at a tenth of it, and the number on the unit's card is a number the");
            Console.WriteLine("  player will never see spent.");
            Console.WriteLine();
            Console.WriteLine("  That changes what the third liability is. A thread that parts 1,500 m");
            Console.WriteLine("  from the pad can only be found by traffic within 1,500 m of the pad,");
            Console.WriteLine("  so on the road arm the found column collapses as soon as the pad is");
            Console.WriteLine("  further back than that: the cable is a liability only from a pad that");
            Console.WriteLine("  was already standing in the enemy's lap. On the open arm the thread");
            Console.WriteLine("  survives the whole flight and the found column reads 95-100% across a");
            Console.WriteLine("  twelve-kilometre sweep of standoff. That is FINDINGS 39's hand");
            Console.WriteLine("  measurement - seven threads found from the forward pad and seven from");
            Console.WriteLine("  the rear one - reproduced as a sweep rather than as two runs, and it");
            Console.WriteLine("  holds: how often a cable is found is not a function of where you");
            Console.WriteLine("  launched. What you launched over decides that.");
            Console.WriteLine();
            Console.WriteLine("  The bottom row of the open arm is the spool, and it is the only place");
            Console.WriteLine("  in this table where the catalogue figure binds: 18,000 m of flight on");
            Console.WriteLine("  16,800 m of thread parts the line 1,200 m short, four fifths of the");
            Console.WriteLine("  trials never get a thread across the lane at all, and the found column");
            Console.WriteLine("  falls to 20%. A leash long enough to be irrelevant everywhere else is");
            Console.WriteLine("  an asset exactly once.");
            Console.WriteLine();
            Console.WriteLine("  The 'site held on radio' column is the control and it is what makes");
            Console.WriteLine("  the fiber column mean anything. The same sortie flown by an FPV Team");
            Console.WriteLine("  drags no thread, so where the two agree the filament disclosed nothing");
            Console.WriteLine("  the defence's own sensors had not - a launch site inside the Radar");
            Console.WriteLine("  Mast's passive listening is held whatever took off from it, and on");
            Console.WriteLine("  these rungs that boundary sits between 6,600 m and 9,600 m out. Past");
            Console.WriteLine("  it the control is zero and the fiber column is not, and those three");
            Console.WriteLine("  rows are the price of the thread with everything else divided out:");
            Console.WriteLine("  from a pad the enemy could not otherwise find, one vehicle driving");
            Console.WriteLine("  over a cable hands him the whole launch site.");
        }

        /// <summary>
        /// One standoff's worth of trials, fiber and radio, sharing patrol phases
        /// so that the two 'site held' columns are paired rather than independent.
        /// </summary>
        static void RunFiberSweep(int standoff, bool road, out int found, out int heldFiber,
                                  out int heldRadio, out int spoolSpent)
        {
            const int trials = 40;
            int f = 0, hf = 0, hr = 0;
            long spoolTotal = 0;
            for (int trial = 0; trial < trials; trial++)
            {
                // The tank's starting point on its lane, stepped evenly across it.
                int phase = FiberLaneBottom
                          + (trial * (FiberLaneTop - FiberLaneBottom)) / trials;
                bool foundOne, heldOne;
                int spooledOne;
                RunFiberSortie("Fiber FPV Team", standoff, road, phase, (ulong)(trial + 1),
                               out foundOne, out heldOne, out spooledOne);
                if (foundOne) f++;
                if (heldOne) hf++;
                spoolTotal += spooledOne;

                bool ignoredFound, heldRadioOne;
                int ignoredSpool;
                RunFiberSortie("FPV Team", standoff, road, phase, (ulong)(trial + 1),
                               out ignoredFound, out heldRadioOne, out ignoredSpool);
                if (heldRadioOne) hr++;
            }
            found = f * 100 / trials;
            heldFiber = hf * 100 / trials;
            heldRadio = hr * 100 / trials;
            spoolSpent = (int)(spoolTotal / trials);
        }

        const int FiberObjectiveX = 21600;
        const int FiberLaneX = 18600;
        // A short lane, and the length is the one number in this experiment that
        // had to be chosen rather than read off something. Too long and the
        // vehicle is almost never on the cable when the sortie flies, so every
        // standoff reads zero and the table measures nothing; too short and it is
        // always on it, so every standoff reads 100% and the table measures
        // nothing in the other direction. 1,920 m puts the 288 m discovery band
        // across about a third of the lap, which leaves the found-thread column
        // free to move. It is a condition of the experiment and not a result of
        // it, and the prose under the table says so.
        const int FiberLaneBottom = 8400;
        const int FiberLaneTop = 12240;

        /// <summary>
        /// The two routes the sortie is flown on. 9,360 is the road band every
        /// other experiment in this file stages on (PaintMixedTerrain puts a road
        /// across y = 0.47 to 0.51 of the map); 11,040 is open ground 1,700 m
        /// north of it. They are the experiment's second variable and they turned
        /// out to matter more than the one it was written to sweep.
        /// </summary>
        const int FiberRoadY = 9360;
        const int FiberOpenY = 11040;

        /// <summary>
        /// One sortie from one pad, with the defence's vehicle starting at
        /// <paramref name="patrolPhase"/> on its lane.
        /// </summary>
        static void RunFiberSortie(string droneDefName, int standoff, bool road, int patrolPhase,
                                   ulong seed, out bool threadFound, out bool siteHeld,
                                   out int spoolSpent)
        {
            World w = MakeRealisticWorld(28800, 19200, 128, 8, seed, 2, 0,
                StandardBorderMetres, 1, 2);

            int padX = FiberObjectiveX - standoff;
            int routeY = road ? FiberRoadY : FiberOpenY;

            // The attacker: the pad, the structure standing on it that a found
            // thread would reveal, and the rear that pays for the sortie. The
            // relays sit behind the pad rather than in front of it so the radio
            // arm is flown on the link the pad actually has, not on one strung
            // out along a route the fiber arm does not need.
            w.Player(1).Materiel = Fix.FromInt(100000);
            // 600 m behind the pad rather than on top of it, and the offset is
            // the point. A found thread reveals a *disc* around the launch point
            // (SimConstants.TetherFoundRevealRadiusMetres, 1,080 m), so a
            // structure standing exactly on the anchor is inside any disc at all
            // and the radius is untested by construction - which is what the
            // mutation guard said when the first version of this claimed to
            // measure it and printed identical bytes with the radius cut to 24 m.
            // Putting the mast where a mast would actually be, beside the pad and
            // not on it, makes the question "how much of your rear does the cable
            // give away" a question with an answer.
            EntityHandle site = w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(padX - 600, routeY));
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(1800, 9360));
            for (int q = 0; q < 3; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(1200 + q * 480, 10800));

            // The defence: the objective, and the traffic that finds cables.
            EntityHandle objective = w.Spawn(Catalog.IdOf("Radar Mast"), 2, P(FiberObjectiveX, routeY));
            EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(FiberLaneX, patrolPhase));
            bool northbound = true;
            w.Enqueue(Command.MoveTo(2, tank, P(FiberLaneX, FiberLaneTop)));

            int droneDef = Catalog.IdOf(droneDefName);
            w.Enqueue(Command.LaunchSortie(1, droneDef, P(padX, routeY), objective, 0));

            threadFound = false;
            siteHeld = false;
            spoolSpent = 0;

            bool airborne = false;
            int goneAtTick = -1;

            for (int tick = 0; tick < 3000 * SimConstants.TicksPerSecond; tick++)
            {
                w.Step();

                // Turn the vehicle round at the ends of its lane, so it patrols
                // rather than parking at the far end - a parked vehicle finds no
                // cables, and half the trials would be measuring a tank standing
                // still at one end of a lane.
                if (w.Entities.IsSlotAlive(tank.Index))
                {
                    Fix y = w.Entities.Position[tank.Index].Y;
                    if (northbound && y > F(FiberLaneTop - 32))
                    { northbound = false; w.Enqueue(Command.MoveTo(2, tank, P(FiberLaneX, FiberLaneBottom))); }
                    else if (!northbound && y < F(FiberLaneBottom + 32))
                    { northbound = true; w.Enqueue(Command.MoveTo(2, tank, P(FiberLaneX, FiberLaneTop))); }
                }

                for (int e = 0; e < w.Events.Count; e++)
                    if (w.Events[e].Kind == SimEventKind.TetherFound) threadFound = true;

                if (w.TrackQualityOf(2, site) != TrackQuality.None) siteHeld = true;

                // The most thread this sortie ever had out, read off the tether
                // rather than computed from the distance flown: a thread follows
                // the airframe's path and is cut when it runs out, so the two are
                // not the same number and only one of them is the mechanic.
                for (int id = 0; id < w.Tethers.Capacity; id++)
                {
                    TetherSystem.Tether t = w.Tethers.Get(id);
                    if (t.State == TetherState.Free || t.Team != 1) continue;
                    int spooled = t.Spooled.RoundToInt();
                    if (spooled > spoolSpent) spoolSpent = spooled;
                }

                // Stop once the sortie is over and the reveal window with it. The
                // thread lingers after the airframe dies (TetherLingerTicks) and
                // what it can still disclose in that time is part of what the
                // thread costs, so the run does not end the instant the drone does.
                bool nowAirborne = CountTeamDronesAirborne(w, 1) > 0;
                if (nowAirborne) airborne = true;
                if (airborne && !nowAirborne)
                {
                    if (goneAtTick < 0) goneAtTick = tick;
                    if (tick - goneAtTick > SimConstants.TetherLingerTicks
                                          + SimConstants.TetherFoundRevealTicks) break;
                }
                if (!w.Entities.IsAlive(objective)) break;
            }
        }

        static int CountTeamDronesAirborne(World w, byte team)
        {
            int n = 0;
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (w.Entities.Team[i] != team) continue;
                if (w.Entities.EntityLayer[i] == Layer.Ground) continue;
                if (!w.Entities.Has(i, ComponentMask.Sortie)) continue;
                n++;
            }
            return n;
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

        /// <summary>
        /// Seconds of play expressed in ticks, for readability at the call sites.
        /// Seconds of play and not real seconds, deliberately: what this converts
        /// is a launch schedule, and a launch schedule is the player's hands on
        /// the cards rather than a physical duration - the same split
        /// SimConstants makes between PlaySeconds() and Seconds(). At the 4x
        /// multiplier Sec(3) is twelve real seconds, which is three of the Gun
        /// Mount's four-second cooldowns.
        /// </summary>
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
        /// This used to overwrite the gun's range back to 550 compressed metres on every call;
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
