// KILL ZONE - a real-time strategy video game.
// Balance experiments: run the same engagement many times with one variable
// changed, and print what actually happened.
//
// This exists because arguing about balance from a stat table is guesswork. If
// the question is "can five drones take a gun position", the honest answer comes
// from running it two hundred times, not from multiplying numbers on a page.

using System;
using KZ.Sim;

namespace KZ.Balance
{
    public static class Program
    {
        static Fix F(double v) { return Fix.FromDoubleContentOnly(v); }
        static Fix2 P(double x, double y) { return new Fix2(F(x), F(y)); }

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
            Console.WriteLine("launched together from 1,200 m, gun reaches 550 m");
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
        /// The same assault, with the gun's reach varied. If saturation cannot beat
        /// it at any number, the reach is the variable that matters.
        /// </summary>
        static void GunRangeExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("REACH - eight drones against one gun mount, varying its range");
            Console.WriteLine();
            Console.WriteLine("  gun range   seconds under fire   arrived   gun killed");
            Console.WriteLine("  " + new string('-', 58));

            int[] ranges = { 550, 450, 350, 250, 180, 120 };
            for (int r = 0; r < ranges.Length; r++)
            {
                Fix range = F(ranges[r]);
                int arrivedTotal = 0, gunKilled = 0;
                const int trials = 60;

                for (int trial = 0; trial < trials; trial++)
                {
                    int arrived;
                    bool killed = RunAssault(8, F(1200), (ulong)(trial + 1), out arrived, range);
                    arrivedTotal += arrived;
                    if (killed) gunKilled++;
                }

                // A drone crosses the gun's reach at 22 m/s.
                double exposure = ranges[r] / 22.0;

                Console.WriteLine(string.Format("  {0,9}   {1,18}   {2,7}   {3,10}",
                    ranges[r] + " m",
                    exposure.ToString("0.0") + " s",
                    (arrivedTotal / (double)trials).ToString("0.0"),
                    (gunKilled * 100 / trials) + "%"));
            }
            Console.WriteLine();
            Console.WriteLine("  The gun fires roughly once every 1.25 seconds including acquisition,");
            Console.WriteLine("  and one hit kills any rotary drone, so 'seconds under fire' divided");
            Console.WriteLine("  by 1.25 is how many drones it gets to kill before the rest arrive.");
        }

        /// <summary>
        /// How much launching from closer helps. This is what a forward position
        /// buys you, and it is the lever a player actually has.
        /// </summary>
        static void ApproachExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("APPROACH - eight drones, varying where they launch from");
            Console.WriteLine("gun at 1,650 m reaching 550 m, so its edge is at 1,100 m");
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
            Console.WriteLine("  fire from the moment they exist, but for far less time.");
        }

        /// <summary>
        /// The same assault, in daylight and in darkness.
        ///
        /// A gun mount finds its targets optically. After dark that reach collapses
        /// to about a third, so it cannot begin shooting until the drones are far
        /// closer - and the seconds it does not get are the seconds the drones
        /// needed. This is the counter the subject matter actually uses, and it
        /// costs the attacker nothing but patience.
        /// </summary>
        static void NightExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("DARKNESS - the same assault by day and by night");
            Console.WriteLine("gun sees 600 m optically by day, about 210 m after dark");
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
                    if (RunAssault(n, F(1200), (ulong)(trial + 1), out a, F(550), 0)) dayKilled++;
                    dayArrived += a;
                    // Well into the night phase of the cycle.
                    if (RunAssault(n, F(1200), (ulong)(trial + 1), out a, F(550), 8000)) nightKilled++;
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
            Terrain t = new Terrain(2400, 1600);
            t.Fill(TileClass.Open);
            t.FillRect(0, 95, 299, 98, TileClass.Road);
            World w = new World(t, 128, 8, 4242, 2, 8000);

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
            Console.WriteLine("gun reaches 550 m; these are the distances it can see that far");
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
            Terrain t = new Terrain(2048, 2048);
            t.Fill(TileClass.Open);
            World w = new World(t, 64, 4, 1, 2, night ? 8000 : 0);

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
        /// More turrets covering the same ground. The question is whether defence
        /// stacks cleanly or whether two guns are worth more than twice one.
        /// </summary>
        static void StackingExperiment()
        {
            Console.WriteLine();
            Console.WriteLine("STACKING - more turrets covering the same approach");
            Console.WriteLine("twelve drones launched together, 450 Materiel per turret");
            Console.WriteLine();
            Console.WriteLine("  turrets   defence cost   drones needed to take one   attacker cost");
            Console.WriteLine("  " + new string('-', 68));

            for (int guns = 1; guns <= 4; guns++)
            {
                int needed = -1;
                int[] tries = { 8, 12, 16, 24, 32, 48, 64 };
                for (int t = 0; t < tries.Length && needed < 0; t++)
                {
                    int wins = 0;
                    for (int trial = 0; trial < 20; trial++)
                        if (RunStacked(guns, tries[t], (ulong)(trial + 1))) wins++;
                    if (wins >= 18) needed = tries[t];
                }

                int defenceCost = guns * 450;
                string attackerCost = needed > 0 ? (needed * 200).ToString() : "more than 12,800";
                Console.WriteLine(string.Format("  {0,7}   {1,12}   {2,25}   {3,13}",
                    guns, defenceCost,
                    needed > 0 ? needed.ToString() : "more than 64",
                    attackerCost));
            }
            Console.WriteLine();
            Console.WriteLine("  Each turret adds its own rate of fire over the same crossing time,");
            Console.WriteLine("  so the cost of attacking rises roughly in step with the number of");
            Console.WriteLine("  guns - but the attacker is also paying for drones that die to guns");
            Console.WriteLine("  they were never sent at.");
        }

        static bool RunStacked(int gunCount, int droneCount, ulong seed)
        {
            Terrain t = new Terrain(2400, 1600);
            t.Fill(TileClass.Open);
            World w = new World(t, 512, 32, seed, 2, 0);

            w.Player(1).Materiel = Fix.FromInt(200000);
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(400, 780));
            for (int q = 0; q < 6; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(300 + q * 40, 900));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(1000, 780));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(1450, 1150));

            EntityHandle first = EntityHandle.None;
            for (int g = 0; g < gunCount; g++)
            {
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(1650, 780 + (g - gunCount / 2) * 60));
                if (g == 0) first = gun;
            }

            for (int i = 0; i < droneCount; i++)
                w.Enqueue(Command.LaunchSortie(1, Catalog.IdOf("FPV Team"),
                    new Fix2(F(1200), F(780 + (i - droneCount / 2) * 12)), first, i));

            for (int tick = 0; tick < 200 * SimConstants.TicksPerSecond; tick++)
            {
                w.Step();
                if (!w.Entities.IsAlive(first)) return true;
                if (CountFriendlyDronesAirborne(w) == 0 && tick > 8) return false;
            }
            return false;
        }

        // ------------------------------------------------------------------

        static bool RunAssault(int droneCount, Fix padX, ulong seed, out int arrived)
        {
            return RunAssault(droneCount, padX, seed, out arrived, F(550));
        }

        /// <summary>
        /// One attempt: launch the drones together and let it play out until either
        /// the gun is destroyed or every drone is gone.
        /// </summary>
        static bool RunAssault(int droneCount, Fix padX, ulong seed, out int arrived, Fix gunRange)
        {
            return RunAssault(droneCount, padX, seed, out arrived, gunRange, 0);
        }

        static bool RunAssault(int droneCount, Fix padX, ulong seed, out int arrived,
                               Fix gunRange, int startTick)
        {
            Terrain t = new Terrain(2400, 1600);
            t.Fill(TileClass.Open);
            World w = new World(t, 256, 32, seed, 2, startTick);

            w.Player(1).Materiel = Fix.FromInt(100000);
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(400, 780));
            // Enough crews that the experiment measures the gun, not the crew cap.
            for (int q = 0; q < 6; q++) w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(300 + q * 40, 900));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(1000, 780));
            // Far enough back that the experiment measures the gun against
            // drones, and not the gun against a relay mast.
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(1450, 1150));

            EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(1650, 780));
            w.Entities.Weapon[gun.Index].RangeMetres = gunRange;

            for (int i = 0; i < droneCount; i++)
            {
                // A small spread, so they are not literally stacked in one point.
                Fix2 spot = new Fix2(padX, F(780 + (i - droneCount / 2) * 14));
                w.Enqueue(Command.LaunchSortie(1, Catalog.IdOf("FPV Team"), spot, gun, i));
            }

            bool[] struck = new bool[w.Entities.Capacity];
            int reached = 0;
            Fix strikeRange = Catalog.Get(Catalog.IdOf("FPV Team")).WeaponRangeMetres + F(4);

            for (int tick = 0; tick < 200 * SimConstants.TicksPerSecond; tick++)
            {
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

                if (CountFriendlyDronesAirborne(w) == 0 && tick > 8) break;
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
