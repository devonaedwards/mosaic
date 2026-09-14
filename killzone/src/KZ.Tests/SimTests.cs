// KILL ZONE - a real-time strategy video game.
// Tests for the game systems: jamming, links, tethers, crews, damage, and the
// autonomy classifier. Several of these assert the headline balance cases from
// the design document, so if somebody retunes a number the test says which
// design promise they just broke.

using KZ.Sim;

namespace KZ.Tests
{
    public static class SimTests
    {
        static Fix F(double v) { return Fix.FromDoubleContentOnly(v); }
        static Fix2 P(double x, double y) { return new Fix2(F(x), F(y)); }

        /// <summary>A small empty map with two teams, for tests that need a world.</summary>
        static World MakeWorld(ulong seed)
        {
            Terrain t = new Terrain(2048, 2048);
            t.Fill(TileClass.Open);
            return new World(t, 512, 64, seed, 2);
        }

        public static void Register(TestRunner r)
        {
            RegisterJamming(r);
            RegisterLinks(r);
            RegisterMesh(r);
            RegisterTethers(r);
            RegisterAutonomy(r);
            RegisterCrews(r);
            RegisterDamage(r);
            RegisterMinesAndNight(r);
        }

        // ------------------------------------------------------------------

        static void RegisterJamming(TestRunner r)
        {
            r.Group("jamming field");

            r.Run("a bubble bites hardest at the centre and frays at the rim", delegate
            {
                // An electronic warfare post: strength 70, reach 450 metres.
                Assert.Equal(91, SignalGrid.EffectiveJam(70, F(450), F(0)), "at the emitter");
                Assert.Equal(46, SignalGrid.EffectiveJam(70, F(450), F(225)), "at half reach");
                Assert.Equal(0, SignalGrid.EffectiveJam(70, F(450), F(450)), "at the rim");
                Assert.Equal(0, SignalGrid.EffectiveJam(70, F(450), F(600)), "outside");
            });

            r.Run("skirting the edge of a bubble is a real play", delegate
            {
                // A basic radio drone has robustness 40. It should survive the
                // outer third of an electronic warfare post's reach, so flying
                // around the rim rather than through the middle is worth doing.
                int atRim = SignalGrid.EffectiveJam(70, F(450), F(340));
                Assert.True(atRim < 40, "a radio drone survives at 340 metres out");
                int deeper = SignalGrid.EffectiveJam(70, F(450), F(200));
                Assert.True(deeper > 40, "and does not at 200 metres out");
            });

            r.Run("overlapping jammers take the strongest, never the sum", delegate
            {
                // Two weak jammers must not quietly add up to a strong one. Nothing
                // on screen would show it and the player would be learning a rule
                // the game never told them.
                SignalGrid g = new SignalGrid(1024, 1024);
                g.AddEmitter(new JamEmitter { Position = P(500, 500), Strength = 55, RadiusMetres = F(350), Team = 1 });
                g.AddEmitter(new JamEmitter { Position = P(520, 500), Strength = 55, RadiusMetres = F(350), Team = 1 });
                g.Rebuild();

                int sampled = g.SampleFor(P(510, 500), 40);
                int oneAlone = SignalGrid.EffectiveJam(55, F(350), F(10));
                Assert.True(sampled <= oneAlone + 2, "overlap does not stack (got " + sampled + ")");
            });

            r.Run("the edge is evaluated exactly, so a drone does not flicker", delegate
            {
                // Near a drone's own threshold the coarse grid is not trusted,
                // because across one cell the field changes by several points and a
                // drone on the boundary would blink in and out of having a pilot.
                SignalGrid g = new SignalGrid(1024, 1024);
                g.AddEmitter(new JamEmitter { Position = P(512, 512), Strength = 70, RadiusMetres = F(450), Team = 1 });
                g.Rebuild();

                int prev = -1;
                bool monotonic = true;
                for (int d = 0; d < 440; d += 8)
                {
                    int v = g.SampleFor(P(512 + d, 512), 40);
                    if (prev >= 0 && v > prev + 1) monotonic = false;
                    prev = v;
                }
                Assert.True(monotonic, "jamming falls off smoothly as you fly outward");
            });
        }

        // ------------------------------------------------------------------

        static void RegisterLinks(TestRunner r)
        {
            r.Group("the control-link ladder");

            r.Run("a radio drone flown into a jammer loses its pilot", delegate
            {
                World w = MakeWorld(1);
                // An anchor so the drone starts with a control path at all.
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(400, 500));
                EntityHandle jammer = w.Spawn(Catalog.IdOf("EW Post"), 2, P(1000, 500));
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(500, 500));

                for (int i = 0; i < 8; i++) w.Step();
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Green,
                            "green outside the bubble");

                // Move it well inside the bubble, but not on top of the jammer -
                // a one-way drone parked on an enemy structure quite correctly
                // dives on it, which would end the test early.
                w.Entities.Position[drone.Index] = P(1150, 500);
                w.Step(); w.Step(); w.Step(); w.Step();
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Amber,
                            "amber inside the bubble");

                // Amber for four seconds, then the link is gone.
                for (int i = 0; i < SimConstants.AmberToBlackTicks + 8; i++) w.Step();
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Black,
                            "black after four seconds of amber");
                Assert.True(w.Entities.IsAlive(jammer), "the jammer is still standing");
            });

            r.Run("amber costs you fine control before it costs you the aircraft", delegate
            {
                World w = MakeWorld(2);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(400, 500));
                w.Spawn(Catalog.IdOf("EW Post"), 2, P(1000, 500));
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(1150, 500));

                for (int i = 0; i < 6; i++) w.Step();
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Amber, "amber");
                Assert.False(w.Entities.Sortie[drone.Index].AcceptsNewOrders,
                             "it will not take a new order");
                Assert.True(w.Entities.Mover[drone.Index].SpeedMultiplier < Fix.One,
                            "and it is slower");
            });

            r.Run("a fiber drone flies through the same bubble untouched", delegate
            {
                // This is the whole point of the rung: there is no radio to jam.
                World w = MakeWorld(3);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(400, 500));
                w.Spawn(Catalog.IdOf("EW Post"), 2, P(1000, 500));

                EntityHandle spawned;
                LaunchResult res = SortieSystem.Launch(w, 1, Catalog.IdOf("Fiber FPV Team"),
                                                       P(900, 500), EntityHandle.None, 0, out spawned);
                Assert.Equal((long)LaunchResult.Launched, (long)res, "launched");

                w.Entities.Position[spawned.Index] = P(1150, 500);
                for (int i = 0; i < 20; i++) w.Step();

                Assert.True(w.Entities.IsAlive(spawned), "still flying");
                Assert.True(w.Entities.Link[spawned.Index].Pip == LinkPip.Green,
                            "green in the middle of a jammer");
                Assert.Equal(0, w.Entities.Link[spawned.Index].JamSampled,
                             "fiber samples no jamming at all");
            });

            r.Run("a drone with no link at all falls out of the sky", delegate
            {
                World w = MakeWorld(4);
                // No command post, no relay: nothing to talk to.
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(500, 500));

                for (int i = 0; i < SimConstants.AmberToBlackTicks
                                  + SimConstants.BlackToLostTicks + 16; i++)
                {
                    w.Step();
                    if (!w.Entities.IsAlive(drone)) break;
                }
                Assert.False(w.Entities.IsAlive(drone), "lost after orbiting with nobody flying it");
            });

            r.Run("losing a drone costs materiel and a crew's time, never the crew", delegate
            {
                World w = MakeWorld(5);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(500, 500));
                PlayerState p = w.Player(1);
                int before = p.Crews.ReadyCount;

                EntityHandle spawned;
                SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(500, 500),
                                    EntityHandle.None, 0, out spawned);
                w.Step();
                Assert.Equal(before - 1, p.Crews.ReadyCount, "a crew is now flying");

                w.Kill(spawned, EntityHandle.None);
                w.Step();
                Assert.Equal(before - 1, p.Crews.ReadyCount, "the crew is recovering, not gone");

                for (int i = 0; i < SimConstants.CrewRecoveryTicks + 4; i++) w.Step();
                Assert.Equal(before, p.Crews.ReadyCount, "and back on the roster eight seconds later");
            });
        }

        // ------------------------------------------------------------------

        static void RegisterMesh(TestRunner r)
        {
            r.Group("mesh relay chains");

            r.Run("control range grows with the chain", delegate
            {
                MeshGraph g = new MeshGraph();
                g.Add(new MeshNode { Handle = new EntityHandle(1, 1), Position = P(0, 0), Team = 1, IsAnchor = true });
                g.Add(new MeshNode { Handle = new EntityHandle(2, 1), Position = P(600, 0), Team = 1, IsRepeater = true });
                g.Add(new MeshNode { Handle = new EntityHandle(3, 1), Position = P(1200, 0), Team = 1, IsRepeater = true });
                g.Add(new MeshNode { Handle = new EntityHandle(4, 1), Position = P(1800, 0), Team = 1, IsRepeater = true });

                g.Rebuild(1, Fix.FromInt(SimConstants.MeshRangePerHopMetres), SimConstants.MeshMaxHops);

                Assert.Equal(0, g.HopsAt(0), "the anchor is at depth zero");
                Assert.Equal(1, g.HopsAt(1), "one hop out");
                Assert.Equal(2, g.HopsAt(2), "two hops out");
                Assert.Equal(3, g.HopsAt(3), "three hops out, 1800 metres from home");
            });

            r.Run("a node beyond the last hop is not reached", delegate
            {
                MeshGraph g = new MeshGraph();
                g.Add(new MeshNode { Handle = new EntityHandle(1, 1), Position = P(0, 0), Team = 1, IsAnchor = true });
                g.Add(new MeshNode { Handle = new EntityHandle(2, 1), Position = P(1500, 0), Team = 1, IsRepeater = true });
                g.Rebuild(1, Fix.FromInt(SimConstants.MeshRangePerHopMetres), SimConstants.MeshMaxHops);
                Assert.False(g.IsConnected(1), "1500 metres is too far for one 700 metre hop");
            });

            r.Run("parent choice is fully ordered, so two machines agree", delegate
            {
                // Two relays exactly equidistant from a drone. Without a total
                // ordering the two players' machines could pick differently and the
                // match would quietly diverge.
                MeshGraph a = new MeshGraph();
                MeshGraph b = new MeshGraph();
                for (int pass = 0; pass < 2; pass++)
                {
                    MeshGraph g = pass == 0 ? a : b;
                    g.Add(new MeshNode { Handle = new EntityHandle(10, 1), Position = P(0, 0), Team = 1, IsAnchor = true });
                    g.Add(new MeshNode { Handle = new EntityHandle(11, 1), Position = P(0, 600), Team = 1, IsAnchor = true });
                    g.Add(new MeshNode { Handle = new EntityHandle(12, 1), Position = P(300, 300), Team = 1, IsRepeater = true });
                    g.Rebuild(1, Fix.FromInt(SimConstants.MeshRangePerHopMetres), SimConstants.MeshMaxHops);
                }
                Assert.Equal((long)a.ParentOf(2).Value, (long)b.ParentOf(2).Value,
                             "the same parent is chosen every time");
            });

            r.Run("redundancy hardens a mesh, but only so far", delegate
            {
                Assert.Equal(0, MeshGraph.RedundancyBonus(0), "a lone chain gets nothing");
                Assert.Equal(5, MeshGraph.RedundancyBonus(1), "one alternate route");
                Assert.Equal(15, MeshGraph.RedundancyBonus(3), "three routes is the cap");
                Assert.Equal(15, MeshGraph.RedundancyBonus(10), "a hundred drones is still the cap");
            });
        }

        // ------------------------------------------------------------------

        static void RegisterTethers(TestRunner r)
        {
            r.Group("fiber tethers");

            r.Run("the spool tracks the path flown, not the straight line home", delegate
            {
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);
                TetherSystem ts = new TetherSystem(8, t, new DetRandom(1));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(100, 100), F(1400), 1, 0);

                // Fly out, then turn the corner and fly across.
                bool cut;
                for (int x = 100; x <= 400; x += 10) ts.Update(id, P(x, 100), 0, out cut);
                for (int y = 100; y <= 400; y += 10) ts.Update(id, P(400, y), 0, out cut);

                TetherSystem.Tether te = ts.Get(id);
                // Roughly 600 metres of line paid out to reach a point 424 away.
                Assert.InRange(560, 640, te.Spooled.ToDoubleForDisplay(), "line paid out");
                Assert.True(te.NodeCount > 20, "the thread has real geometry, not two endpoints");
            });

            r.Run("the leash is hard, and over-extending eventually parts the line", delegate
            {
                Terrain t = new Terrain(4096, 4096);
                t.Fill(TileClass.Open);
                TetherSystem ts = new TetherSystem(8, t, new DetRandom(1));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(100, 100), F(300), 1, 0);

                bool cut = false;
                int tick = 0;
                for (int x = 100; x <= 800 && !cut; x += 10)
                    ts.Update(id, P(x, 100), tick++, out cut);

                Assert.True(ts.Get(id).State == TetherState.Taut
                            || ts.Get(id).State == TetherState.Cut, "at full stretch");

                // Holding at full stretch parts it after the grace period. Simply
                // preventing the drone from going further would be safer and far
                // less interesting.
                for (int i = 0; i < SimConstants.TetherTautGraceTicks + 4 && !cut; i++)
                    ts.Update(id, P(800, 100), tick++, out cut);
                Assert.True(cut, "the line parts after three seconds at the leash");
            });

            r.Run("at full stretch a drone can fly back but not further out", delegate
            {
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);
                TetherSystem ts = new TetherSystem(8, t, new DetRandom(1));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(100, 100), F(200), 1, 0);

                bool cut;
                int tick = 0;
                for (int x = 100; x <= 340; x += 10) ts.Update(id, P(x, 100), tick++, out cut);
                if (ts.Get(id).State != TetherState.Taut) return; // nothing to assert

                Fix2 outward = ts.ConstrainVelocity(id, P(340, 100), new Fix2(F(10), Fix.Zero));
                Assert.True(outward.X <= F(0.01), "cannot pull further away");

                Fix2 inward = ts.ConstrainVelocity(id, P(340, 100), new Fix2(F(-10), Fix.Zero));
                Assert.Near(-10.0, inward.X.ToDoubleForDisplay(), 0.01, "can fly home freely");
            });

            r.Run("terrain decides how dangerous a thread is", delegate
            {
                Assert.Near(0.0, Terrain.SnagRatePerSecond(TileClass.Open).ToDoubleForDisplay(),
                            1e-9, "open ground is free");
                Assert.Near(0.04, Terrain.SnagRatePerSecond(TileClass.Forest).ToDoubleForDisplay(),
                            1e-6, "forest");
                Assert.Near(0.09, Terrain.SnagRatePerSecond(TileClass.PowerLine).ToDoubleForDisplay(),
                            1e-6, "a power line corridor is the worst ground on the map");
            });

            r.Run("dragging a thread through a forest costs you drones", delegate
            {
                // Over many runs a forest crossing should part the line often, and
                // an identical run over open ground should never do so. That
                // difference is the whole reason route choice matters for fiber.
                int cutsInForest = CountCuts(TileClass.Forest, 200);
                int cutsInOpen = CountCuts(TileClass.Open, 200);
                Assert.Equal(0, cutsInOpen, "open ground never snags");
                Assert.InRange(20, 200, cutsInForest, "forest snags often (got " + cutsInForest + " of 200)");
            });

            r.Run("a cut thread still lies on the map and still leads home", delegate
            {
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);
                TetherSystem ts = new TetherSystem(8, t, new DetRandom(1));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(100, 100), F(1400), 1, 0);

                bool cut;
                int tick = 0;
                for (int x = 100; x <= 400; x += 10) ts.Update(id, P(x, 100), tick++, out cut);

                ts.Cut(id, tick);
                ts.Update(id, P(400, 100), ++tick, out cut);
                Assert.True(ts.Get(id).State == TetherState.Lingering, "the line remains");
                Assert.True(ts.AnySegmentNear(id, P(250, 100), F(20)),
                            "an enemy walking over it would find it");

                // And it fades after half a minute rather than cluttering the map.
                for (int i = 0; i < SimConstants.TetherLingerTicks + 4; i++)
                    ts.Update(id, P(400, 100), ++tick, out cut);
                Assert.True(ts.Get(id).State == TetherState.Free, "gone after thirty seconds");
            });
        }

        static int CountCuts(TileClass tile, int trials)
        {
            int cuts = 0;
            for (int trial = 0; trial < trials; trial++)
            {
                Terrain t = new Terrain(2048, 2048);
                t.Fill(tile);
                TetherSystem ts = new TetherSystem(4, t, new DetRandom((ulong)(trial + 1)));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(100, 100), F(2000), 1, 0);

                bool cut = false;
                int tick = 0;
                for (int x = 100; x <= 700 && !cut; x += 5)
                    ts.Update(id, P(x, 100), tick++, out cut);
                if (cut) cuts++;
            }
            return cuts;
        }

        // ------------------------------------------------------------------

        static void RegisterAutonomy(TestRunner r)
        {
            r.Group("autonomy and deception");

            r.Run("decoys drag a classifier's confidence down", delegate
            {
                World w = MakeWorld(10);
                EntityHandle m = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(1000, 1000));

                Assert.Equal(55, AutonomyClassifier.EffectiveQuality(w, m.Index, 0),
                             "clear field");
                Assert.Equal(30, AutonomyClassifier.EffectiveQuality(w, m.Index, 5),
                             "five decoys");
                Assert.Equal(10, AutonomyClassifier.EffectiveQuality(w, m.Index, 9),
                             "nine decoys");
                Assert.Equal(0, AutonomyClassifier.EffectiveQuality(w, m.Index, 20),
                             "confidence floors at zero");
            });

            r.Run("a lone tank in the open dies to an autonomous munition", delegate
            {
                int hits = 0;
                const int trials = 300;
                for (int i = 0; i < trials; i++)
                {
                    World w = MakeWorld((ulong)(1000 + i));
                    EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1000, 1000));
                    EntityHandle m = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(1030, 1000));

                    bool mis;
                    EntityHandle chosen = AutonomyClassifier.SelectTarget(w, m.Index, out mis);
                    if (chosen == tank) hits++;
                }
                Assert.InRange(0.85, 1.0, hits / (double)trials,
                               "a tank with nothing to hide behind is hit almost every time");
            });

            r.Run("the same tank behind nine decoys survives four attacks in five", delegate
            {
                // The headline case from the design document, and the reason the
                // answer to autonomy is a cheap inflatable rather than a jammer.
                int hits = 0;
                const int trials = 600;
                for (int i = 0; i < trials; i++)
                {
                    World w = MakeWorld((ulong)(2000 + i));
                    EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1000, 1000));
                    for (int d = 0; d < 9; d++)
                        w.SpawnDecoy(2, P(1000 + (d % 3) * 20 - 20, 1000 + (d / 3) * 20 - 20),
                                     TargetKind.HighValue, 10000);

                    EntityHandle m = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(1040, 1000));
                    bool mis;
                    EntityHandle chosen = AutonomyClassifier.SelectTarget(w, m.Index, out mis);
                    if (chosen == tank) hits++;
                }
                double rate = hits / (double)trials;
                Assert.InRange(0.12, 0.30, rate,
                               "the real tank is picked about one time in five (got "
                               + rate.ToString("0.###") + ")");
            });

            r.Run("deception fools machines and never people", delegate
            {
                // A human with a clear picture ignores every decoy on the field.
                // This is a hard branch in the code rather than a very high
                // confidence value, because it is a rule and not a tuning number.
                World w = MakeWorld(11);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(950, 1000));

                EntityHandle drone;
                SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(1000, 1000),
                                    EntityHandle.None, 0, out drone);
                w.Step();

                Assert.True(AutonomyClassifier.IsPilotedWithClearFeed(w, drone.Index, P(1050, 1000)),
                            "a piloted drone with a live link close to the target");

                // The same drone once its link has gone.
                w.Entities.Link[drone.Index].Pip = LinkPip.Black;
                Assert.False(AutonomyClassifier.IsPilotedWithClearFeed(w, drone.Index, P(1050, 1000)),
                             "not once the pilot has lost the picture");
            });

            r.Run("a machine that picks wrong says so out loud", delegate
            {
                bool sawReport = false;
                for (int i = 0; i < 200 && !sawReport; i++)
                {
                    World w = MakeWorld((ulong)(3000 + i));
                    w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1000, 1000));
                    for (int d = 0; d < 9; d++)
                        w.SpawnDecoy(2, P(1000 + d * 8 - 32, 1010), TargetKind.HighValue, 10000);
                    EntityHandle m = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(1040, 1000));

                    bool mis;
                    AutonomyClassifier.SelectTarget(w, m.Index, out mis);
                    if (mis && w.Events.CountOf(SimEventKind.AutonomyMisidentified) > 0) sawReport = true;
                }
                Assert.True(sawReport, "a misidentification is reported, never swallowed");
            });
        }

        // ------------------------------------------------------------------

        static void RegisterCrews(TestRunner r)
        {
            r.Group("crews");

            r.Run("crews cap how many sorties are up at once, not how many drones you own", delegate
            {
                World w = MakeWorld(20);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(500, 500));
                w.Player(1).Materiel = Fix.FromInt(100000);

                int launched = 0;
                for (int i = 0; i < 20; i++)
                {
                    EntityHandle h;
                    if (SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(500, 500),
                                            EntityHandle.None, i, out h) == LaunchResult.Launched)
                        launched++;
                }
                Assert.Equal(SimConstants.StartingCrews, launched,
                             "six crews means six drones in the air, however much money you have");
                Assert.True(w.Player(1).Materiel > Fix.FromInt(90000),
                            "and the money is still sitting there unspent");
            });

            r.Run("a launch with no crew free is refused and says why", delegate
            {
                World w = MakeWorld(21);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(500, 500));
                w.Player(1).Materiel = Fix.FromInt(100000);

                for (int i = 0; i < SimConstants.StartingCrews; i++)
                {
                    EntityHandle h;
                    SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(500, 500),
                                        EntityHandle.None, i, out h);
                }
                EntityHandle extra;
                LaunchResult res = SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(500, 500),
                                                       EntityHandle.None, 0, out extra);
                Assert.Equal((long)LaunchResult.NoCrew, (long)res, "refused for want of a crew");
            });

            r.Run("crew selection is deterministic", delegate
            {
                CrewPool a = new CrewPool(1, 6, false);
                CrewPool b = new CrewPool(1, 6, false);
                for (int i = 0; i < 6; i++)
                {
                    int pa = a.SelectForLaunch();
                    int pb = b.SelectForLaunch();
                    Assert.Equal(pa, pb, "same crew chosen on both machines, launch " + i);
                    a.Assign(pa, new EntityHandle(i + 1, 1), 0);
                    b.Assign(pb, new EntityHandle(i + 1, 1), 0);
                }
            });

            r.Run("flying five sorties in two minutes tires a crew out", delegate
            {
                CrewPool pool = new CrewPool(1, 1, false);
                EventRing events = new EventRing(256);
                int tick = 0;

                for (int s = 0; s < SimConstants.CrewFatigueSortieThreshold; s++)
                {
                    pool.Assign(0, new EntityHandle(1, 1), tick);
                    tick += 16;
                    pool.Release(0, tick);
                    // A crew already flagged as tired takes the longer recovery, so
                    // wait out the worst case rather than the nominal one.
                    for (int i = 0; i < SimConstants.CrewFatiguedRecoveryTicks + 4; i++)
                        pool.Tick(++tick, events);
                }
                Assert.Equal((long)CrewState.Fatigued, (long)pool.Get(0).State,
                             "the crew is tired");

                for (int i = 0; i < SimConstants.CrewFatigueClearIdleTicks + 4; i++) pool.Tick(++tick, events);
                Assert.Equal((long)CrewState.Ready, (long)pool.Get(0).State,
                             "and recovers after a proper break");
            });

            r.Run("verified kills promote a crew", delegate
            {
                CrewPool pool = new CrewPool(1, 1, false);
                EventRing events = new EventRing(256);
                Assert.Equal(1, pool.Get(0).Rank, "starts a rookie");

                for (int i = 0; i < SimConstants.CrewRankUpKills2; i++) pool.CreditKill(0, 0, events);
                Assert.Equal(2, pool.Get(0).Rank, "promoted after three kills");

                for (int i = SimConstants.CrewRankUpKills2; i < SimConstants.CrewRankUpKills3; i++)
                    pool.CreditKill(0, 0, events);
                Assert.Equal(3, pool.Get(0).Rank, "and again at nine");
                Assert.Equal(SimConstants.VeteranLinkRobustnessBonus, pool.LinkBonusFor(0),
                             "a veteran pushes through interference a rookie would lose to");
            });

            r.Run("the two factions pay very differently for a lost rear area", delegate
            {
                EventRing events = new EventRing(256);
                EntityHandle quarters = new EntityHandle(5, 1);

                // Forward dugouts: the people are in the building.
                CrewPool dugouts = new CrewPool(2, 4, true);
                for (int i = 0; i < 4; i++) SetHome(dugouts, i, quarters);
                dugouts.OnQuartersDestroyed(quarters, 0, events);
                Assert.Equal(0, dugouts.ReadyCount, "four crews gone for the rest of the match");

                // Remote bays: they lose their consoles for forty-five seconds.
                CrewPool bays = new CrewPool(1, 4, false);
                for (int i = 0; i < 4; i++) SetHome(bays, i, quarters);
                bays.OnQuartersDestroyed(quarters, 0, events);
                Assert.Equal(0, bays.ReadyCount, "grounded for now");
                int tick = 0;
                for (int i = 0; i < SimConstants.CrewBenchedTicks + 4; i++) bays.Tick(++tick, events);
                Assert.Equal(4, bays.ReadyCount, "but back afterwards");
            });
        }

        static void SetHome(CrewPool pool, int crewId, EntityHandle quarters)
        {
            pool.SetHomeQuarters(crewId, quarters);
        }

        // ------------------------------------------------------------------

        static void RegisterMinesAndNight(TestRunner r)
        {
            r.Group("mines and darkness");

            r.Run("a mine waits, then goes off under whatever drives over it", delegate
            {
                World w = MakeWorld(60);
                w.Spawn(Catalog.IdOf("Command Post"), 2, P(900, 900));
                EntityHandle truck = w.Spawn(Catalog.IdOf("Supply Truck"), 2, P(900, 1000));
                w.SpawnMine(1, P(1100, 1000), F(600));
                w.Enqueue(Command.MoveTo(2, truck, P(1300, 1000)));

                for (int i = 0; i < 60 * SimConstants.TicksPerSecond; i++)
                {
                    w.Step();
                    if (!w.Entities.IsAlive(truck)) break;
                }
                Assert.False(w.Entities.IsAlive(truck), "the truck drove into it");
            });

            r.Run("a mine does not care whose vehicle arrives first", delegate
            {
                // Laid by team one, triggered by a team one vehicle. This is not a
                // gameplay punishment, it is what a mine is.
                World w = MakeWorld(61);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(900, 900));
                EntityHandle friendly = w.Spawn(Catalog.IdOf("Supply Truck"), 1, P(900, 1000));
                w.SpawnMine(1, P(1100, 1000), F(600));
                w.Enqueue(Command.MoveTo(1, friendly, P(1300, 1000)));

                for (int i = 0; i < 60 * SimConstants.TicksPerSecond; i++)
                {
                    w.Step();
                    if (!w.Entities.IsAlive(friendly)) break;
                }
                Assert.False(w.Entities.IsAlive(friendly), "it went off under its own side");
            });

            r.Run("a mine ignores aircraft", delegate
            {
                World w = MakeWorld(62);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(1000, 1000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("Scout Quad"), 1, P(1100, 1000));
                w.SpawnMine(2, P(1100, 1000), F(600));
                for (int i = 0; i < 200; i++) w.Step();
                Assert.True(w.Entities.IsAlive(drone), "a drone flies over a minefield");
            });

            r.Run("a mine needs a moment to arm", delegate
            {
                World w = MakeWorld(63);
                w.Spawn(Catalog.IdOf("Command Post"), 2, P(900, 900));
                EntityHandle truck = w.Spawn(Catalog.IdOf("Supply Truck"), 2, P(1100, 1000));
                w.SpawnMine(1, P(1100, 1000), F(600));

                w.Step();
                Assert.True(w.Entities.IsAlive(truck), "not instantly");

                for (int i = 0; i < SimConstants.MineArmingTicks + 4; i++)
                {
                    w.Step();
                    if (!w.Entities.IsAlive(truck)) break;
                }
                Assert.False(w.Entities.IsAlive(truck), "but a second and a half later, yes");
            });

            r.Run("a heavy drone will not fly in daylight", delegate
            {
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);

                World day = new World(t, 256, 16, 64, 2, 0);
                day.Spawn(Catalog.IdOf("Command Post"), 1, P(1000, 1000));
                day.Player(1).Materiel = Fix.FromInt(20000);
                EntityHandle h;
                Assert.Equal((long)LaunchResult.DaylightRefused,
                             (long)SortieSystem.Launch(day, 1, Catalog.IdOf("Night Bomber"),
                                                       P(1000, 1000), EntityHandle.None, 0, out h),
                             "refused by day");

                World night = new World(t, 256, 16, 64, 2, 8000);
                night.Spawn(Catalog.IdOf("Command Post"), 1, P(1000, 1000));
                night.Player(1).Materiel = Fix.FromInt(20000);
                Assert.Equal((long)LaunchResult.Launched,
                             (long)SortieSystem.Launch(night, 1, Catalog.IdOf("Night Bomber"),
                                                       P(1000, 1000), EntityHandle.None, 0, out h),
                             "flies after dark");
            });

            r.Run("a small drone is found far closer than a tank", delegate
            {
                // Detection reach scales with what the target is giving off, so the
                // same camera that picks a tank out at half a kilometre struggles
                // to find a quadcopter at two hundred metres. This is the fact the
                // whole subject rests on and the model had no way to express.
                World w = MakeWorld(65);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("Scout Quad"), 2, P(1200, 1000));
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1400, 1000));
                w.Step();

                Fix vsDrone = w.DetectionRangeFor(gun.Index, drone.Index, SensorChannel.Optical);
                Fix vsTank = w.DetectionRangeFor(gun.Index, tank.Index, SensorChannel.Optical);
                Assert.True(vsTank > vsDrone * Fix.FromInt(2),
                            "a tank is seen more than twice as far as a drone");
            });

            r.Run("darkness helps the attacker, but not for free", delegate
            {
                // This reverses an earlier conclusion, and the reversal is the
                // point. The model used to hold that a microphone was the best
                // anti-drone sensor at any hour, which made darkness nearly
                // worthless to an attacker. That came out of a single wrong number:
                // a quadcopter was rated as loud as a turbojet. It is not. What it
                // radiates is high-frequency and the air absorbs it, so a quad is
                // heard for a few hundred metres and a two-stroke engine for
                // kilometres.
                //
                // With honest figures the night is a trade. Cameras lose most of
                // their reach; microphones gain, because the ambient floor falls ten
                // decibels or so once everything stops moving. Against a small quad
                // the camera still wins even after dark - so darkness is a real
                // advantage - and against anything with an engine the microphone
                // wins outright.
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);

                World day = new World(t, 256, 16, 65, 2, 0);
                EntityHandle gunDay = day.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
                EntityHandle droneDay = day.Spawn(Catalog.IdOf("FPV Team"), 2, P(1200, 1000));
                day.Step();

                World night = new World(t, 256, 16, 65, 2, 8000);
                EntityHandle gunNight = night.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
                EntityHandle droneNight = night.Spawn(Catalog.IdOf("FPV Team"), 2, P(1200, 1000));
                night.Step();

                Assert.True(night.IsNight, "the second world is actually after dark");

                Fix opticalDay = day.DetectionRangeFor(gunDay.Index, droneDay.Index, SensorChannel.Optical);
                Fix opticalNight = night.DetectionRangeFor(gunNight.Index, droneNight.Index, SensorChannel.Optical);
                Assert.True(opticalNight < opticalDay, "cameras lose most of their reach after dark");

                Fix acousticDay = day.DetectionRangeFor(gunDay.Index, droneDay.Index, SensorChannel.Acoustic);
                Fix acousticNight = night.DetectionRangeFor(gunNight.Index, droneNight.Index, SensorChannel.Acoustic);
                Assert.True(acousticNight > acousticDay,
                            "quiet air after dark buys the microphone real reach");

                Assert.True(opticalNight > acousticNight,
                            "but against a small quad the camera still wins at night, "
                            + "which is why darkness is worth flying in");
            });

            r.Run("an engine is heard far further than a quadcopter", delegate
            {
                // The other half of the same correction. Loudness at the source is
                // not what sets detection range - frequency is. A quad is piercing
                // at ten metres and gone at three hundred; a two-stroke is no louder
                // up close and is heard across kilometres, which is why acoustic
                // nets are built against engines and not against quads.
                World w = MakeWorld(165);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
                EntityHandle quad = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(1200, 1000));
                EntityHandle engine = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 2, P(1200, 1000));
                w.Step();

                Fix vsQuad = w.DetectionRangeFor(gun.Index, quad.Index, SensorChannel.Acoustic);
                Fix vsEngine = w.DetectionRangeFor(gun.Index, engine.Index, SensorChannel.Acoustic);
                Assert.True(vsEngine > vsQuad * Fix.FromInt(3),
                            "the spread across airframes is large, not the two-to-one "
                            + "the old table allowed");
            });

            r.Run("a satellite link ends at a border, not at the front", delegate
            {
                // The rung was previously unconditional - "coverage is everywhere"
                // was a literal return true - and nothing in the suite noticed when
                // that changed, because nothing in the suite flew a satellite drone
                // at all. It does now.
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);
                World w = new World(t, 256, 16, 301, 2);

                // Team 1 owns the west, team 2 the east, with a neutral strip.
                w.Territory.SetVerticalBorder(1024, 1, 2, 64);

                EntityHandle home = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(400, 1000));
                EntityHandle across = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(1600, 1000));
                w.Step();

                Assert.True(LinkResolver.InSatelliteCoverage(w, home.Index),
                            "over its own ground the constellation is there");
                Assert.True(!LinkResolver.InSatelliteCoverage(w, across.Index),
                            "over the other side's ground it is not");

                Assert.Equal((int)LinkPip.Green, (int)w.Entities.Link[home.Index].Pip,
                             "a link at home is green");
                Assert.Equal((int)LinkPip.Black, (int)w.Entities.Link[across.Index].Pip,
                             "and across the border it is black");
            });

            r.Run("crossing a geofence is instant, not a fade", delegate
            {
                // Every other way of losing a link in this game degrades through
                // amber first. This one does not, and the difference is the whole
                // character of the rung: there is no warning and nothing the pilot
                // can do, because the drone did not fly out of range of anything -
                // it flew across a line on a map.
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);
                World w = new World(t, 256, 16, 302, 2);
                w.Territory.SetVerticalBorder(1024, 1, 2, 0);

                EntityHandle d = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(900, 1000));
                w.Step();
                Assert.Equal((int)LinkPip.Green, (int)w.Entities.Link[d.Index].Pip,
                             "green to begin with");

                // One step across. Not a long flight - one tick.
                w.Entities.Position[d.Index] = P(1100, 1000);
                w.Step();
                Assert.Equal((int)LinkPip.Black, (int)w.Entities.Link[d.Index].Pip,
                             "black on the very next evaluation, with no amber in between");
            });

            r.Run("advancing past your own border costs you the satellite", delegate
            {
                // The consequence nobody designed and the model produced anyway.
                // A constellation is licensed by country, so its line is political
                // and fixed. Take ground beyond it and you hold that ground with no
                // satellite link over it, because the service is not watching the
                // war - it is reading a map. The top rung of the ladder is the one
                // that punishes success.
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);
                World w = new World(t, 256, 16, 303, 2);
                w.Territory.SetVerticalBorder(1024, 1, 2, 0);

                // Team 1 has pushed its front well east of its own border and is
                // sitting on ground it controls by every military measure.
                EntityHandle held = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(1400, 1000));
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(1300, 1000));
                w.Step();

                Assert.True(!LinkResolver.InSatelliteCoverage(w, held.Index),
                            "holding the ground does not move the border");
                Assert.Equal((int)LinkPip.Black, (int)w.Entities.Link[held.Index].Pip,
                             "so the deeper the advance, the longer drones are on their own");
            });

            r.Run("crossing the border costs you the operator, not your position", delegate
            {
                // The one-line rule the navigation research produced, and the
                // reason the geofence is a procurement decision rather than a flat
                // tax. Two drones cross the same line. The cheap one is lost; the
                // one carrying a map knows exactly where it is and has only lost
                // the human who was going to pick the target.
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Forest);      // matchable ground
                World w = new World(t, 256, 16, 401, 2);
                w.Territory.SetVerticalBorder(600, 1, 2, 0);
                w.Imagery.GrantAround(1, P(1200, 1000), Fix.FromInt(600));

                EntityHandle cheap = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(1000, 1000));
                EntityHandle withMap = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 1, P(1000, 1000));

                // A long run, because drift is a fraction of distance flown and a
                // few seconds of it proves nothing either way.
                for (int k = 0; k < 1500; k++)
                {
                    MovementSystem.OrderMoveTo(w, cheap, P(1950, 1000));
                    MovementSystem.OrderMoveTo(w, withMap, P(1950, 1000));
                    w.Step();
                }

                Fix cheapError = w.Entities.Nav[cheap.Index].ErrorMetres;
                Fix mapError = w.Entities.Nav[withMap.Index].ErrorMetres;

                Assert.True(cheapError > mapError * Fix.FromInt(3),
                            "dead reckoning drifts and scene matching does not");
                Assert.True(w.Entities.Nav[withMap.Index].HasLock,
                            "the one with imagery of this ground holds its lock");
            });

            r.Run("open ground is where a drone gets lost", delegate
            {
                // The inversion the research turned up, and the one worth keeping.
                // Matching the *shape* of the ground beats matching its
                // appearance - shape survives snow, ploughing and craters. But
                // elevation matching degenerates where the ground is flat, and
                // this game is set on the East European plain. So the robust
                // technique is the weaker one across most of the map, and open
                // steppe is the hazard rather than the safe crossing.
                Terrain flat = new Terrain(2048, 2048);
                flat.Fill(TileClass.Open);
                World w1 = new World(flat, 256, 16, 402, 2);
                w1.Territory.SetVerticalBorder(600, 1, 2, 0);
                w1.Imagery.GrantAround(1, P(1200, 1000), Fix.FromInt(600));

                Terrain broken = new Terrain(2048, 2048);
                broken.Fill(TileClass.Forest);
                World w2 = new World(broken, 256, 16, 402, 2);
                w2.Territory.SetVerticalBorder(600, 1, 2, 0);
                w2.Imagery.GrantAround(1, P(1200, 1000), Fix.FromInt(600));

                EntityHandle a = w1.Spawn(Catalog.IdOf("Heavy Strike Drone"), 1, P(1000, 1000));
                EntityHandle b = w2.Spawn(Catalog.IdOf("Heavy Strike Drone"), 1, P(1000, 1000));
                for (int k = 0; k < 200; k++)
                {
                    MovementSystem.OrderMoveTo(w1, a, P(1400, 1000));
                    MovementSystem.OrderMoveTo(w2, b, P(1400, 1000));
                    w1.Step();
                    w2.Step();
                }

                Assert.True(!w1.Entities.Nav[a.Index].HasLock,
                            "over open steppe there is nothing to match against");
                Assert.True(w2.Entities.Nav[b.Index].HasLock,
                            "over broken ground there is");
                Assert.True(w1.Entities.Nav[a.Index].ErrorMetres
                            > w2.Entities.Nav[b.Index].ErrorMetres,
                            "so the same airframe is lost on one map and exact on the other");
            });

            r.Run("bombardment takes away the map, and a clock does not", delegate
            {
                // Reference imagery is a coverage resource, not a freshness one.
                // Imagery spanning nineteen years has been matched successfully
                // across seasons, and fielded systems deliberately key on the
                // things that do not change - so an expiry timer would be a number
                // nobody has ever measured. What does invalidate it is the ground
                // being churned into something else, which is an event a player
                // can watch happen.
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Forest);
                World w = new World(t, 256, 16, 403, 2);
                w.Territory.SetVerticalBorder(600, 1, 2, 0);
                w.Imagery.GrantAround(1, P(1200, 1000), Fix.FromInt(400));

                Assert.True(NavigationSystem.CanMatchHere(w, 1, P(1200, 1000)),
                            "imagery of this sector, so it can be matched");

                w.Imagery.Invalidate(P(1200, 1000), Fix.FromInt(200));
                Assert.True(!NavigationSystem.CanMatchHere(w, 1, P(1200, 1000)),
                            "and after the sector is churned, it cannot");
            });

            r.Run("a star tracker slows the drift and never resets it", delegate
            {
                // The correction that mattered most in the navigation research.
                // A star tracker measures orientation, not position; getting a
                // position out of it needs local vertical, which comes from the
                // inertial unit that was already wrong. One arc-second of vertical
                // deflection is thirty metres. So it bounds heading drift - which
                // removes the fastest-growing term - and never fixes position.
                // Modelling it as a periodic reset to zero would be wrong.
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);        // nothing to match, so both dead reckon
                World w = new World(t, 256, 16, 404, 2);
                w.Territory.SetVerticalBorder(600, 1, 2, 0);

                EntityHandle plain = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 1, P(1000, 1000));
                EntityHandle starry = w.Spawn(Catalog.IdOf("Jet Strike Drone"), 1, P(1000, 1000));
                for (int k = 0; k < 200; k++)
                {
                    MovementSystem.OrderMoveTo(w, plain, P(1500, 1000));
                    MovementSystem.OrderMoveTo(w, starry, P(1500, 1000));
                    w.Step();
                }

                Fix plainError = w.Entities.Nav[plain.Index].ErrorMetres;
                Fix starryError = w.Entities.Nav[starry.Index].ErrorMetres;

                Assert.True(starryError < plainError, "celestial slows the drift");
                Assert.True(starryError > Fix.Zero,
                            "but it is still drifting - it never got a position fix");
            });

            r.Run("fog grounds nothing and blinds everything", delegate
            {
                // The weather state a designer is most likely to get wrong, by
                // filing it under "bad weather" along with the rain. It is the
                // opposite of the rain: nothing stops flying and almost nothing can
                // see - except radar and passive listening, which do not care at
                // all. So the side that bought radar is briefly the only side with
                // eyes, and fog becomes a window to attack through rather than a
                // misfortune to sit out.
                World w = MakeWorld(501);
                EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(1000, 1000));
                EntityHandle bomber = w.Spawn(Catalog.IdOf("Night Bomber"), 1, P(1000, 1000));
                EntityHandle target = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 2, P(1300, 1000));
                w.Step();

                Fix clearOptical = w.DetectionRangeFor(bomber.Index, target.Index, SensorChannel.Optical);
                Fix clearThermal = w.DetectionRangeFor(bomber.Index, target.Index, SensorChannel.Thermal);
                Fix clearRadar = w.DetectionRangeFor(mast.Index, target.Index, SensorChannel.Radar);

                w.Weather = WeatherState.Murk;
                w.Step();

                Fix fogOptical = w.DetectionRangeFor(bomber.Index, target.Index, SensorChannel.Optical);
                Fix fogThermal = w.DetectionRangeFor(bomber.Index, target.Index, SensorChannel.Thermal);
                Fix fogRadar = w.DetectionRangeFor(mast.Index, target.Index, SensorChannel.Radar);

                Assert.True(fogOptical < clearOptical / Fix.FromInt(4), "cameras lose nearly everything");
                Assert.True(fogThermal < clearThermal / Fix.FromInt(2),
                            "and thermal is not the answer to fog that it is to darkness");
                Assert.Equal(clearRadar.Raw, fogRadar.Raw, "radar does not care about fog at all");

                Assert.True(!SortieSystem.WeatherGrounds(WeatherState.Murk, Propulsion.SmallElectric),
                            "and nothing at all is grounded by it");
            });

            r.Run("weather sorts on what an airframe burns", delegate
            {
                // The one place this game is deliberately unfair, and it is unfair
                // in the direction the reporting describes. Wind takes the small
                // electrics; rain and icing take every electric; a two-stroke
                // engine above the cloud deck flies through all of it. So winter
                // degrades the side flying cheap quadcopters and interceptors, and
                // does not degrade the side flying combustion strike drones.
                Assert.True(SortieSystem.WeatherGrounds(WeatherState.Wind, Propulsion.SmallElectric),
                            "a quadcopter cannot hold station in eighteen metres a second");
                Assert.True(!SortieSystem.WeatherGrounds(WeatherState.Wind, Propulsion.HeavyElectric),
                            "a heavy multirotor has the mass to stay put");
                Assert.True(SortieSystem.WeatherGrounds(WeatherState.Wet, Propulsion.HeavyElectric),
                            "but icing takes a quarter of its thrust in the first minute");
                Assert.True(!SortieSystem.WeatherGrounds(WeatherState.Wet, Propulsion.Combustion),
                            "and the engine flies on");
                Assert.True(!SortieSystem.WeatherGrounds(WeatherState.Wind, Propulsion.Combustion),
                            "in either kind of weather");
            });

            r.Run("mud does not slow the road, it deletes everything else", delegate
            {
                // Which is a different mechanic and a much more interesting one. A
                // road in the rain is still a road; what changes is that it becomes
                // the only road, and every vehicle on the map ends up on the same
                // few hard surfaces - the ones already under the most observation.
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);
                for (int tx = 0; tx < t.WidthTiles; tx++) t.Set(tx, 60, TileClass.Road);
                World w = new World(t, 256, 16, 502, 2);

                EntityHandle onRoad = w.Spawn(Catalog.IdOf("Supply Truck"), 1, P(500, 60 * 8 + 4));
                EntityHandle offRoad = w.Spawn(Catalog.IdOf("Supply Truck"), 1, P(500, 900));
                w.Step();

                w.Ground = GroundState.Mud;
                Assert.Equal(Fix.One.Raw, MovementSystem.GroundScale(w, onRoad.Index).Raw,
                             "the road is unaffected");
                Assert.True(MovementSystem.GroundScale(w, offRoad.Index) < Fix.One / Fix.FromInt(2),
                            "and leaving it stops being worth doing");

                // Frozen ground is the opposite, and is better than firm.
                w.Ground = GroundState.Frozen;
                Assert.True(MovementSystem.GroundScale(w, offRoad.Index) > Fix.One,
                            "deep winter opens the whole landscape up again");
            });

            r.Run("the two autonomies are not the same autonomy", delegate
            {
                // The correction that had been pending longest. The game treated
                // autonomy as one thing that saved a crew and paid an error rate.
                // It is two things, and the common one does neither.
                //
                // Terminal guidance - the machine flying the last two seconds onto
                // a target a person already chose - became routine on production
                // airframes during 2026 and costs about a hundred dollars as an
                // add-on. It keeps its crew and it makes the shot better, because
                // the hard part of the attack is the final approach. Autonomous
                // target selection, the one that actually frees a crew, was still
                // in initial testing at the same date.
                //
                // So a decoy screen does nothing to the common tier and everything
                // to the rare one, and the classifier must not even run for the
                // first. The decision was made by someone who could see; deceiving
                // the camera afterwards is too late.
                World w = MakeWorld(601);
                w.SpawnDecoy(2, P(1200, 1000), TargetKind.HighValue, 6000);
                w.SpawnDecoy(2, P(1210, 1010), TargetKind.HighValue, 6000);
                w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1220, 1020));

                EntityHandle guided = w.Spawn(Catalog.IdOf("Loitering Munition"), 1, P(1150, 1000));
                EntityHandle choosing = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(1150, 1000));
                w.Step();

                Assert.Equal((int)AutonomyTier.TerminalGuidance,
                             (int)w.Entities.Autonomy[guided.Index].Tier,
                             "the routine tier");
                Assert.Equal((int)AutonomyTier.TargetSelection,
                             (int)w.Entities.Autonomy[choosing.Index].Tier,
                             "and the speculative one");

                bool misidentified;
                EntityHandle guidedPick =
                    AutonomyClassifier.SelectTarget(w, guided.Index, out misidentified);
                Assert.True(guidedPick.IsNone,
                            "terminal guidance chooses nothing - a person already did");
                Assert.True(!misidentified, "so there is nothing for it to get wrong");

                EntityHandle chosenPick =
                    AutonomyClassifier.SelectTarget(w, choosing.Index, out misidentified);
                Assert.True(!chosenPick.IsNone,
                            "target selection does choose, and can be fooled");
            });

            r.Run("a fiber drone defeats radio listening completely", delegate
            {
                // The property that makes fiber worth its leash, and one the old
                // model could not represent: there is no transmission to find.
                World w = MakeWorld(67);
                EntityHandle post = w.Spawn(Catalog.IdOf("Command Post"), 1, P(1000, 1000));
                EntityHandle radio = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(1300, 1000));
                EntityHandle fiber = w.Spawn(Catalog.IdOf("Fiber FPV Team"), 2, P(1300, 1000));
                w.Step();

                Assert.True(w.DetectionRangeFor(post.Index, radio.Index, SensorChannel.Esm) > Fix.Zero,
                            "a radio drone is transmitting and can be heard");
                Assert.Equal(0, w.DetectionRangeFor(post.Index, fiber.Index, SensorChannel.Esm).Raw,
                             "a fiber drone is not transmitting at all");
            });

            r.Run("a jammer that is switched on is the easiest thing on the map to find", delegate
            {
                World w = MakeWorld(68);
                EntityHandle post = w.Spawn(Catalog.IdOf("Command Post"), 1, P(1000, 1000));
                EntityHandle jammer = w.Spawn(Catalog.IdOf("EW Post"), 2, P(1400, 1000));
                w.Step();

                Fix emitting = w.DetectionRangeFor(post.Index, jammer.Index, SensorChannel.Esm);
                Assert.True(w.IsDetectedBy(1, jammer), "found while it is jamming");

                // Switching it off stops it being a beacon, and stops it jamming.
                w.Entities.Emitter[jammer.Index].Active = false;
                w.Step();
                Fix silent = w.DetectionRangeFor(post.Index, jammer.Index, SensorChannel.Esm);
                Assert.True(silent < emitting, "much harder to find once it goes quiet");
            });

            r.Run("altitude beats microphones and helps radar", delegate
            {
                // Flying high is not a free escape. It puts a drone outside what a
                // microphone can localise and squarely inside what a radar is for.
                //
                // Same airframe in both bands, deliberately. The previous version of
                // this test compared a quad down low against a fixed-wing up high
                // and so measured two things at once; it passed for a reason that
                // had nothing to do with altitude.
                World w = MakeWorld(69);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
                EntityHandle radar = w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(1000, 1000));
                EntityHandle low = w.Spawn(Catalog.IdOf("Multirole Quad"), 2, P(1200, 1000));
                EntityHandle high = w.Spawn(Catalog.IdOf("Multirole Quad"), 2, P(1200, 1000));
                w.Entities.EntityLayer[high.Index] = Layer.High;
                w.Step();

                Fix acousticLow = w.DetectionRangeFor(gun.Index, low.Index, SensorChannel.Acoustic);
                Fix acousticHigh = w.DetectionRangeFor(gun.Index, high.Index, SensorChannel.Acoustic);
                Assert.True(acousticHigh < acousticLow,
                            "sound from altitude arrives faint and from nowhere in particular");

                Fix radarHigh = w.DetectionRangeFor(radar.Index, high.Index, SensorChannel.Radar);
                Assert.True(radarHigh > acousticHigh * Fix.FromInt(3),
                            "which is what the radar is there for");
            });

            r.Run("a decoy drone is what the radar sees first", delegate
            {
                // The reason the radar signature had to become a decibel scale. Read
                // as a linear index, a decoy at 80 escorting a strike drone at 60
                // drew fire seven percent further out - which is to say the decoy
                // unit cost money and did nothing. Read as decibels of cross-section
                // the same pair is roughly three to one, which is the figure the
                // reporting supports for a reflector-equipped airframe.
                World w = MakeWorld(166);
                EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(1000, 1000));
                EntityHandle decoy = w.Spawn(Catalog.IdOf("Decoy Drone"), 2, P(1400, 1000));
                EntityHandle strike = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 2, P(1400, 1000));
                w.Step();

                Fix vsDecoy = w.DetectionRangeFor(mast.Index, decoy.Index, SensorChannel.Radar);
                Fix vsStrike = w.DetectionRangeFor(mast.Index, strike.Index, SensorChannel.Radar);
                Assert.True(vsDecoy > vsStrike * Fix.FromInt(2),
                            "a decoy has to be seen far enough ahead of what it escorts "
                            + "to draw the engagement");

                // And the other end of the scale still works: a plastic quadcopter
                // is most of an order of magnitude below the decoy.
                EntityHandle quad = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(1400, 1000));
                w.Step();
                Fix vsQuad = w.DetectionRangeFor(mast.Index, quad.Index, SensorChannel.Radar);
                Assert.True(vsDecoy > vsQuad * Fix.FromInt(5),
                            "and a small quad is a genuinely hard radar target");
            });

            r.Run("a thermal blanket is real masking, not just a trick on machines", delegate
            {
                World w = MakeWorld(70);
                EntityHandle bomber = w.Spawn(Catalog.IdOf("Night Bomber"), 1, P(1000, 1000));
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1300, 1000));
                w.Step();

                Fix bare = w.DetectionRangeFor(bomber.Index, tank.Index, SensorChannel.Thermal);
                w.Entities.HasThermalBlanket[tank.Index] = true;
                Fix covered = w.DetectionRangeFor(bomber.Index, tank.Index, SensorChannel.Thermal);
                Assert.True(covered < bare, "a covered tank is found closer");
                Assert.True(covered > Fix.Zero, "but not invisible");
            });

            r.Run("a weapon cannot shoot what its side cannot see", delegate
            {
                // The third pillar, enforced rather than assumed. Without it the
                // reconnaissance layer is decoration and night means nothing.
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);
                World w = new World(t, 256, 16, 66, 2, 8000);

                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("Scout Quad"), 2, P(1400, 1000));

                // Inside the gun's 550 m reach, outside its 210 m night vision.
                for (int i = 0; i < 200; i++) w.Step();
                Assert.True(w.Entities.IsAlive(drone), "unseen and therefore unshot");
            });
        }

        // ------------------------------------------------------------------

        static void RegisterDamage(TestRunner r)
        {
            r.Group("damage");

            r.Run("a drone diving on a vehicle hits the thin plate on top", delegate
            {
                Fix top = Catalog.DamageMultiplier(DamageType.Shaped, ArmourClass.Heavy, true);
                Fix side = Catalog.DamageMultiplier(DamageType.Shaped, ArmourClass.Heavy, false);
                Assert.Near(2.20, top.ToDoubleForDisplay(), 1e-6, "into the roof");
                Assert.Near(1.10, side.ToDoubleForDisplay(), 1e-6, "into the flank");
                Assert.True(top > side, "the roof is the weak spot");
            });

            r.Run("four drones kill a tank and three do not", delegate
            {
                // The single most important balance number in the game: a
                // two-hundred-Materiel drone is a credible threat to a
                // sixteen-hundred-Materiel tank, but it takes four of them.
                Assert.True(TankSurvives(3), "three drones leave it alive");
                Assert.False(TankSurvives(4), "four kill it");
            });

            r.Run("a cage buys the tank one more drone", delegate
            {
                Assert.True(TankSurvivesWithCage(4), "four is no longer enough");
                Assert.False(TankSurvivesWithCage(5), "five still does it");
            });

            r.Run("fragmentation is useless against armour and lethal against people", delegate
            {
                Fix vsHeavy = Catalog.DamageMultiplier(DamageType.Fragmentation, ArmourClass.Heavy, false);
                Fix vsSoft = Catalog.DamageMultiplier(DamageType.Fragmentation, ArmourClass.Soft, false);
                Assert.Near(0.20, vsHeavy.ToDoubleForDisplay(), 1e-6, "against a tank");
                Assert.Near(1.60, vsSoft.ToDoubleForDisplay(), 1e-6, "against infantry");
            });

            r.Run("an interceptor ramming something kills it outright, and nothing else", delegate
            {
                Assert.Near(1.0, Catalog.DamageMultiplier(DamageType.Ram, ArmourClass.AirRotary, false)
                                        .ToDoubleForDisplay(), 1e-9, "against a drone");
                Assert.Near(0.0, Catalog.DamageMultiplier(DamageType.Ram, ArmourClass.Heavy, false)
                                        .ToDoubleForDisplay(), 1e-9, "a ram does nothing to a tank");
            });

            r.Run("everything that dies leaves wreckage worth about a third of it", delegate
            {
                World w = MakeWorld(30);
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1000, 1000));
                w.Kill(tank, EntityHandle.None);
                w.Step();

                Fix found = Fix.Zero;
                for (int i = 1; i < w.Entities.HighWater; i++)
                {
                    if (!w.Entities.IsSlotAlive(i)) continue;
                    if (!w.Entities.Has(i, ComponentMask.Salvage)) continue;
                    found += w.Entities.SalvagePile[i].Amount;
                }
                Assert.InRange(520, 570, found.ToDoubleForDisplay(),
                               "a 1600 Materiel tank leaves about 560 of salvage");
            });

            r.Run("wreckage rots, so the richest ground is only rich for a moment", delegate
            {
                World w = MakeWorld(31);
                EntityHandle pile = w.SpawnSalvage(P(1000, 1000), Fix.FromInt(1000));
                for (int i = 0; i < SimConstants.SalvageDecayTicks + 8; i++)
                {
                    w.Step();
                    if (!w.Entities.IsAlive(pile)) break;
                }
                Assert.False(w.Entities.IsAlive(pile), "gone within fifty seconds");
            });
        }

        static bool TankSurvives(int droneHits)
        {
            World w = MakeWorld(40);
            EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1000, 1000));
            Fix damage = Catalog.Get(Catalog.IdOf("FPV Team")).WeaponDamage;
            for (int i = 0; i < droneHits; i++)
                w.ApplyDamage(tank, damage, DamageType.Shaped, true, EntityHandle.None);
            return w.Entities.Hp[tank.Index].Raw > 0;
        }

        static bool TankSurvivesWithCage(int droneHits)
        {
            World w = MakeWorld(41);
            EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(1000, 1000));
            w.Entities.CageHp[tank.Index] = Fix.FromInt(600);
            Fix damage = Catalog.Get(Catalog.IdOf("FPV Team")).WeaponDamage;
            for (int i = 0; i < droneHits; i++)
                w.ApplyDamage(tank, damage, DamageType.Shaped, true, EntityHandle.None);
            return w.Entities.Hp[tank.Index].Raw > 0;
        }
    }
}
