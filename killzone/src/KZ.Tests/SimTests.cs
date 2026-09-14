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

            r.Run("darkness shortens what an optical sensor can see", delegate
            {
                Terrain t = new Terrain(2048, 2048);
                t.Fill(TileClass.Open);

                World day = new World(t, 256, 16, 65, 2, 0);
                EntityHandle gunDay = day.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
                EntityHandle droneDay = day.Spawn(Catalog.IdOf("Scout Quad"), 2, P(1400, 1000));
                day.Step();
                Assert.True(day.IsDetectedBy(1, droneDay), "seen at 400 m in daylight");

                World night = new World(t, 256, 16, 65, 2, 8000);
                night.Spawn(Catalog.IdOf("Gun Mount"), 1, P(1000, 1000));
                EntityHandle droneNight = night.Spawn(Catalog.IdOf("Scout Quad"), 2, P(1400, 1000));
                night.Step();
                Assert.False(night.IsDetectedBy(1, droneNight), "not at the same range after dark");

                EntityHandle close = night.Spawn(Catalog.IdOf("Scout Quad"), 2, P(1100, 1000));
                night.Step();
                Assert.True(night.IsDetectedBy(1, close), "but seen at 100 m");
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
