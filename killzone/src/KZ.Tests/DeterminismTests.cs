// KILL ZONE - a real-time strategy video game.
// The tests that matter most.
//
// This game is meant to let somebody on an iPad play a multiplayer match against
// somebody on a PC. Neither machine sends the other the state of the world -
// that would be far too much data over a phone connection. Instead both machines
// run the same simulation and exchange only the players' orders, trusting that
// identical inputs produce identical worlds.
//
// That trust is the entire architecture, and it is fragile: one differing bit,
// anywhere, compounds until the two players are watching different battles. So
// the simulation is tested for it directly, and the state hash is checked
// continuously during real matches too.

using KZ.Sim;

namespace KZ.Tests
{
    public static class DeterminismTests
    {
        static Fix F(double v) { return Fix.FromDoubleContentOnly(v); }
        static Fix2 P(double x, double y) { return new Fix2(F(x), F(y)); }

        public static void Register(TestRunner r)
        {
            r.Group("determinism");

            r.Run("two worlds given the same orders stay bit-identical", delegate
            {
                World a = BuildScenario(777);
                World b = BuildScenario(777);

                for (int tick = 0; tick < 600; tick++)
                {
                    ApplyScriptedOrders(a, tick);
                    ApplyScriptedOrders(b, tick);
                    a.Step();
                    b.Step();

                    if (tick % 25 == 0)
                        Assert.Equal((long)a.StateHash(), (long)b.StateHash(),
                                     "state hash at tick " + tick);
                }
                Assert.Equal((long)a.StateHash(), (long)b.StateHash(), "final state hash");
            });

            r.Run("a different seed produces a different match", delegate
            {
                // The corollary: if two different seeds produced identical results,
                // the hash would not actually be measuring anything.
                World a = BuildScenario(1);
                World b = BuildScenario(2);
                for (int tick = 0; tick < 400; tick++)
                {
                    ApplyScriptedOrders(a, tick);
                    ApplyScriptedOrders(b, tick);
                    a.Step();
                    b.Step();
                }
                Assert.True(a.StateHash() != b.StateHash(), "different seeds diverge");
            });

            r.Run("a match replays identically from its command log", delegate
            {
                // What a replay file actually is: a seed and a list of orders. If
                // this holds, a twenty-five minute match is a few kilobytes.
                World original = BuildScenario(4242);
                ulong[] checkpoints = new ulong[12];
                for (int tick = 0; tick < 600; tick++)
                {
                    ApplyScriptedOrders(original, tick);
                    original.Step();
                    if (tick % 50 == 0) checkpoints[tick / 50] = original.StateHash();
                }

                World replay = BuildScenario(4242);
                for (int tick = 0; tick < 600; tick++)
                {
                    ApplyScriptedOrders(replay, tick);
                    replay.Step();
                    if (tick % 50 == 0)
                        Assert.Equal((long)checkpoints[tick / 50], (long)replay.StateHash(),
                                     "replay checkpoint at tick " + tick);
                }
            });

            r.Run("entity handles are reused safely", delegate
            {
                // When a drone dies its slot is reused. An order still holding the
                // old handle must not silently retarget onto whatever took its
                // place - which would be a bug that only shows up in long matches.
                EntityTable table = new EntityTable(64);
                EntityHandle first = table.Create();
                table.Destroy(first);
                EntityHandle second = table.Create();

                Assert.Equal(first.Index, second.Index, "the slot was reused");
                Assert.True(first.Value != second.Value, "but the handle is different");
                Assert.False(table.IsAlive(first), "the stale handle no longer resolves");
                Assert.True(table.IsAlive(second), "the new one does");
            });

            r.Run("allocation order depends only on the sequence of events", delegate
            {
                EntityTable a = new EntityTable(64);
                EntityTable b = new EntityTable(64);

                EntityHandle[] ha = new EntityHandle[8];
                EntityHandle[] hb = new EntityHandle[8];
                for (int i = 0; i < 8; i++) { ha[i] = a.Create(); hb[i] = b.Create(); }

                // Kill the same ones in the same order on both machines.
                int[] toKill = { 3, 1, 6 };
                for (int i = 0; i < toKill.Length; i++)
                {
                    a.Destroy(ha[toKill[i]]);
                    b.Destroy(hb[toKill[i]]);
                }

                for (int i = 0; i < 3; i++)
                    Assert.Equal(a.Create().Value, b.Create().Value,
                                 "reallocation " + i + " lands on the same slot");
            });

            r.Run("the hash notices a change anywhere in the world", delegate
            {
                World w = BuildScenario(99);
                for (int i = 0; i < 40; i++) w.Step();
                ulong before = w.StateHash();

                // Nudge one unit by a single fixed-point step - the smallest change
                // the simulation can represent.
                for (int i = 1; i < w.Entities.HighWater; i++)
                {
                    if (!w.Entities.IsSlotAlive(i)) continue;
                    w.Entities.Position[i] = new Fix2(new Fix(w.Entities.Position[i].X.Raw + 1),
                                                      w.Entities.Position[i].Y);
                    break;
                }
                Assert.True(before != w.StateHash(), "one bit of movement changes the hash");
            });
        }

        /// <summary>
        /// A small scripted battle: two bases, a jammer, and drones flying into it.
        /// Deliberately exercises the parts most likely to diverge - link state
        /// changes, interception rolls, tether snags and crew assignment.
        /// </summary>
        static World BuildScenario(ulong seed)
        {
            Terrain t = new Terrain(2048, 2048);
            t.Fill(TileClass.Open);
            // A treeline and a power-line corridor for tethers to catch on.
            t.FillRect(60, 40, 90, 70, TileClass.Forest);
            t.FillRect(100, 0, 102, 255, TileClass.PowerLine);

            World w = new World(t, 512, 64, seed, 2);

            w.Spawn(Catalog.IdOf("Command Post"), 1, P(300, 1000));
            w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(360, 1060));
            w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(420, 1000));

            w.Spawn(Catalog.IdOf("Command Post"), 2, P(1700, 1000));
            w.Spawn(Catalog.IdOf("EW Post"), 2, P(1200, 1000));
            w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1300, 1000));
            w.Spawn(Catalog.IdOf("Supply Truck"), 2, P(1400, 1100));

            w.Player(1).Materiel = Fix.FromInt(20000);
            w.Player(2).Materiel = Fix.FromInt(20000);
            return w;
        }

        static void ApplyScriptedOrders(World w, int tick)
        {
            // Launch a drone every second, alternating radio and fiber, and send it
            // east toward the enemy - through the jammer for the radio ones.
            if (tick % 32 != 0) return;

            int wave = tick / 32;
            string airframe = (wave % 2 == 0) ? "FPV Team" : "Fiber FPV Team";

            EntityHandle spawned;
            LaunchResult res = SortieSystem.Launch(w, 1, Catalog.IdOf(airframe),
                                                   P(400, 1000), EntityHandle.None, wave, out spawned);
            if (res == LaunchResult.Launched)
                MovementSystem.OrderMoveTo(w, spawned, P(1300, 1000));

            // The defender sends a truck back and forth, so something is moving on
            // both sides.
            if (wave % 4 == 0)
            {
                for (int i = 1; i < w.Entities.HighWater; i++)
                {
                    if (!w.Entities.IsSlotAlive(i)) continue;
                    if (w.Entities.Team[i] != 2) continue;
                    if (w.Entities.DefId[i] != Catalog.IdOf("Supply Truck")) continue;
                    Fix2 dest = (wave % 8 == 0) ? P(1600, 1100) : P(1250, 1100);
                    MovementSystem.OrderMoveTo(w, w.Entities.HandleAt(i), dest);
                }
            }
        }
    }
}
