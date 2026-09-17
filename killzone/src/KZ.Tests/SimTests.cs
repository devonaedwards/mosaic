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
        /// <summary>A point on the map, in real metres - as everything is now.</summary>
        static Fix2 P(double x, double y) { return new Fix2(F(x), F(y)); }

        /// <summary>A small empty map with two teams, for tests that need a world.</summary>
        static World MakeWorld(ulong seed)
        {
            Terrain t = new Terrain(24576, 24576);
            t.Fill(TileClass.Open);
            return new World(t, 512, 64, seed, 2);
        }

        public static void Register(TestRunner r)
        {
            RegisterJamming(r);
            RegisterEmissionControl(r);
            RegisterLinks(r);
            RegisterMesh(r);
            RegisterTethers(r);
            RegisterAutonomy(r);
            RegisterCrews(r);
            RegisterDamage(r);
            RegisterMinesAndNight(r);
            RegisterPointDefence(r);
            RegisterEconomy(r);
        }

        // ------------------------------------------------------------------

        static void RegisterJamming(TestRunner r)
        {
            r.Group("jamming field");

            r.Run("a bubble bites hardest at the centre and frays at the rim", delegate
            {
                // An electronic warfare post: strength 70, reach 5,400 metres.
                Assert.Equal(91, SignalGrid.EffectiveJam(70, F(5400), F(0)), "at the emitter");
                Assert.Equal(46, SignalGrid.EffectiveJam(70, F(5400), F(2700)), "at half reach");
                Assert.Equal(0, SignalGrid.EffectiveJam(70, F(5400), F(5400)), "at the rim");
                Assert.Equal(0, SignalGrid.EffectiveJam(70, F(5400), F(7200)), "outside");
            });

            r.Run("skirting the edge of a bubble is a real play", delegate
            {
                // A basic radio drone has robustness 40. It should survive the
                // outer third of an electronic warfare post's reach, so flying
                // around the rim rather than through the middle is worth doing.
                int atRim = SignalGrid.EffectiveJam(70, F(5400), F(4080));
                Assert.True(atRim < 40, "a radio drone survives four kilometres out");
                int deeper = SignalGrid.EffectiveJam(70, F(5400), F(2400));
                Assert.True(deeper > 40, "and does not at two and a half");
            });

            r.Run("overlapping jammers take the strongest, never the sum", delegate
            {
                // Two weak jammers must not quietly add up to a strong one. Nothing
                // on screen would show it and the player would be learning a rule
                // the game never told them.
                SignalGrid g = new SignalGrid(12288, 12288);
                g.AddEmitter(new JamEmitter { Position = P(6000, 6000), Strength = 55, RadiusMetres = F(4200), Team = 1 });
                g.AddEmitter(new JamEmitter { Position = P(6240, 6000), Strength = 55, RadiusMetres = F(4200), Team = 1 });
                g.Rebuild();

                int sampled = g.SampleFor(P(6120, 6000), 40);
                int oneAlone = SignalGrid.EffectiveJam(55, F(4200), F(120));
                Assert.True(sampled <= oneAlone + 2, "overlap does not stack (got " + sampled + ")");
            });

            r.Run("the edge is evaluated exactly, so a drone does not flicker", delegate
            {
                // Near a drone's own threshold the coarse grid is not trusted,
                // because across one cell the field changes by several points and a
                // drone on the boundary would blink in and out of having a pilot.
                SignalGrid g = new SignalGrid(12288, 12288);
                g.AddEmitter(new JamEmitter { Position = P(6144, 6144), Strength = 70, RadiusMetres = F(5400), Team = 1 });
                g.Rebuild();

                int prev = -1;
                bool monotonic = true;
                for (int d = 0; d < 5280; d += 96)
                {
                    int v = g.SampleFor(P(6144 + d, 6144), 40);
                    if (prev >= 0 && v > prev + 1) monotonic = false;
                    prev = v;
                }
                Assert.True(monotonic, "jamming falls off smoothly as you fly outward");
            });
        }

        // ------------------------------------------------------------------

        /// <summary>
        /// Emission control: the off switch, and what it costs the side that
        /// throws it. AUDIT-UNWIRED.md F8.
        ///
        /// Everything here goes through Spawn, Enqueue and Step. That matters
        /// more than usual for this feature, because EmitterState.Active was
        /// written into every spawned emitter, read by three systems, and set
        /// false by exactly one line in this file - the FINDINGS 30 shape
        /// precisely, a flag that only its own test ever moved.
        /// </summary>
        static void RegisterEmissionControl(TestRunner r)
        {
            r.Group("emission control");

            r.Run("a jammer told to stop transmitting stops jamming", delegate
            {
                // The measured defect this exists for: the defence's EW Post sits
                // across its own launch corridor, jamming is team-blind, and so
                // every raid it flew went black off the pad. Here the jammer and
                // the drone are on the same side, which is the case the game had
                // no answer to at all.
                World w = MakeWorld(9101);
                w.Spawn(Catalog.IdOf("Command Post"), 2, P(14400, 6000));
                EntityHandle post = w.Spawn(Catalog.IdOf("EW Post"), 2, P(12000, 6000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(13800, 6000));

                for (int i = 0; i < SimConstants.AmberToBlackTicks + 8; i++) w.Step();
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Black,
                            "its own side's bubble has taken its pilot away");

                w.Enqueue(Command.SetEmitting(2, post, false));
                for (int i = 0; i < 8; i++) w.Step();

                Assert.True(w.Entities.IsAlive(post), "the post is still standing");
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Green,
                            "and with the post quiet the pilot is back");
                Assert.Equal(0, w.Entities.Link[drone.Index].JamSampled,
                             "there is no field left to sample");

                // And back on again, because a posture you cannot resume is not a
                // posture.
                w.Enqueue(Command.SetEmitting(2, post, true));
                for (int i = 0; i < SimConstants.AmberToBlackTicks + 8; i++) w.Step();
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Black,
                            "switched back on, it denies its own side again");
            });

            r.Run("you cannot switch off somebody else's jammer", delegate
            {
                World w = MakeWorld(9102);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(4800, 6000));
                EntityHandle post = w.Spawn(Catalog.IdOf("EW Post"), 2, P(12000, 6000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(13800, 6000));

                for (int i = 0; i < SimConstants.AmberToBlackTicks + 8; i++) w.Step();
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Black, "jammed");

                // The order every player would send first if it worked.
                w.Enqueue(Command.SetEmitting(1, post, false));
                for (int i = 0; i < 8; i++) w.Step();

                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Black,
                            "an enemy's order over the radio does not turn their transmitter off");
            });

            r.Run("a radar mast can be switched off, and jams nobody when it is on", delegate
            {
                // The decoupling, stated as a pair. The mast now carries an
                // emitter component - it did not before, because the component
                // was granted off JamStrength and a radar's is zero - and the
                // thing that must not have come with it is a jamming bubble.
                World w = MakeWorld(9103);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(4800, 6000));
                EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(6000, 6000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(6600, 6000));

                for (int i = 0; i < SimConstants.AmberToBlackTicks + 8; i++) w.Step();
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Green,
                            "standing next to your own radar mast costs you nothing");
                Assert.Equal(0, w.Entities.Link[drone.Index].JamSampled,
                             "a radar is not a jammer, however loud it is");

                // And the order reaches it, which is the whole point of the flag
                // being separate from JamStrength.
                //
                // The component test is not decoration. Without it this reads
                // the Active field of a struct the mast may not carry at all,
                // and an unfitted component's default is false - so the
                // assertion below would pass by construction against exactly
                // the code this change replaces. That is the failure WIRING-SPEC
                // opens with, and it is one line to close.
                Assert.True(w.Entities.Has(mast.Index, ComponentMask.Emitter),
                            "a radar mast is an emitter, which is what it was not before");
                Assert.True(w.Entities.Emitter[mast.Index].Active, "and it starts switched on");

                w.Enqueue(Command.SetEmitting(1, mast, false));
                w.Step();
                Assert.False(w.Entities.Emitter[mast.Index].Active,
                             "the loudest building a player owns can be told to be quiet");
            });

            r.Run("a radar that has been switched off cannot see, and still hears", delegate
            {
                // The half that gives the off switch weight on both sides of the
                // map. A mast under emission control keeps every passive channel
                // it owns - it has ESM out to 10,800 m - and loses the one it was
                // shouting on.
                World w = MakeWorld(9104);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(2400, 6000));
                EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(6000, 6000));

                // Something only the radar reaches: an airframe 6,600 m out,
                // inside the mast's 8,501 m radar reach against it and outside
                // every other channel on the map - the mast's own ESM reads
                // zero against a jet that transmits nothing, and the Command
                // Post is 10,200 m back with 6,000 m of ESM and 4,800 m of
                // optics.
                EntityHandle spawned;
                LaunchResult res = SortieSystem.Launch(w, 2, Catalog.IdOf("Jet Strike Drone"),
                                                       P(12600, 6000), EntityHandle.None, 0,
                                                       out spawned);
                Assert.Equal((long)LaunchResult.Launched, (long)res, "launched");

                // And something loud enough for the mast's own ESM to hold: an
                // enemy jammer, transmitting, 7,200 m away against 9,957 m of
                // passive reach.
                EntityHandle theirs = w.Spawn(Catalog.IdOf("EW Post"), 2, P(13200, 6000));

                for (int i = 0; i < 16; i++) w.Step();
                Assert.True(w.IsDetectedBy(1, spawned), "the radar holds the airframe");
                Assert.True(w.TrackQualityOf(1, spawned) == TrackQuality.Radar,
                            "and it is a radar track, which is what a lead solution is flown off");

                w.Enqueue(Command.SetEmitting(1, mast, false));
                // Past the track hold: a track once acquired is held for a couple
                // of seconds after the sensor stops reaching it, and this test is
                // about the sensor rather than about the memory of it.
                for (int i = 0; i < SimConstants.TrackHoldTicks + 16; i++) w.Step();

                Assert.False(w.IsDetectedBy(1, spawned),
                             "a radar that is not radiating does not find an airframe");
                Assert.True(w.TrackQualityOf(1, spawned) == TrackQuality.None,
                            "so no interceptor flies a full lead off it either");
                Assert.True(w.IsDetectedBy(1, theirs),
                            "but the passive channels are untouched: the mast still hears a transmitter");

                w.Enqueue(Command.SetEmitting(1, mast, true));
                for (int i = 0; i < 16; i++) w.Step();
                Assert.True(w.TrackQualityOf(1, spawned) == TrackQuality.Radar,
                            "and it is a radar again the moment it is told to be");
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
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(4800, 6000));
                EntityHandle jammer = w.Spawn(Catalog.IdOf("EW Post"), 2, P(12000, 6000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(6000, 6000));

                for (int i = 0; i < 8; i++) w.Step();
                Assert.True(w.Entities.Link[drone.Index].Pip == LinkPip.Green,
                            "green outside the bubble");

                // Move it well inside the bubble, but not on top of the jammer -
                // a one-way drone parked on an enemy structure quite correctly
                // dives on it, which would end the test early.
                w.Entities.Position[drone.Index] = P(13800, 6000);
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
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(4800, 6000));
                w.Spawn(Catalog.IdOf("EW Post"), 2, P(12000, 6000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(13800, 6000));

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
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(4800, 6000));
                w.Spawn(Catalog.IdOf("EW Post"), 2, P(12000, 6000));

                EntityHandle spawned;
                LaunchResult res = SortieSystem.Launch(w, 1, Catalog.IdOf("Fiber FPV Team"),
                                                       P(10800, 6000), EntityHandle.None, 0, out spawned);
                Assert.Equal((long)LaunchResult.Launched, (long)res, "launched");

                w.Entities.Position[spawned.Index] = P(13800, 6000);
                for (int i = 0; i < 20; i++) w.Step();

                Assert.True(w.Entities.IsAlive(spawned), "still flying");
                Assert.True(w.Entities.Link[spawned.Index].Pip == LinkPip.Green,
                            "green in the middle of a jammer");
                Assert.Equal(0, w.Entities.Link[spawned.Index].JamSampled,
                             "fiber samples no jamming at all");
            });

            r.Run("last-mile guidance is what a jammer cannot take away", delegate
            {
                // BlackPolicy was declared, read by LinkResolver, and set by not
                // one of the thirty-five units - so every airframe in the game
                // aborted, and LastMile and DualLink were unreachable code. Which
                // also silently contradicted this session's own autonomy work:
                // seven units were given terminal guidance while still being
                // configured to orbit and fall out of the sky the moment they lost
                // their link, which is the exact opposite of what terminal
                // guidance is for.
                World w = MakeWorld(801);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(6000, 6000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(7200, 6000));

                // Designate a point while the link is still good - a human making
                // the decision in time is precisely what the upgrade buys.
                w.Entities.Sortie[drone.Index].DesignatedPoint = P(10800, 6000);
                w.Entities.Sortie[drone.Index].HasDesignatedPoint = true;

                // Now take the link away for longer than it takes to go black.
                // Beside the lane rather than on it: a one-way drone that flies
                // straight over an enemy structure quite correctly dives on it,
                // which would end the test early and prove nothing about the
                // link. 600 m off the track is still deep inside the post's
                // 5,400 m bubble and well outside the drone's own 96 m reach.
                EntityHandle jammer = w.Spawn(Catalog.IdOf("EW Post"), 2, P(7440, 6600));
                for (int i = 0; i < SimConstants.AmberToBlackTicks
                                  + SimConstants.BlackToLostTicks + 32; i++)
                    w.Step();

                Assert.True(w.Entities.IsAlive(drone),
                            "it carries on rather than falling out of the sky");
                Assert.True(w.Entities.Mover[drone.Index].HasOrder,
                            "still flying, under its own guidance");
                Assert.True(w.Entities.IsAlive(jammer), "the jammer is still there");
            });

            r.Run("a drone with no link at all falls out of the sky", delegate
            {
                World w = MakeWorld(4);
                // No command post, no relay: nothing to talk to.
                EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(6000, 6000));

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
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(6000, 6000));
                PlayerState p = w.Player(1);
                int before = p.Crews.ReadyCount;

                EntityHandle spawned;
                SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(6000, 6000),
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
                g.Add(new MeshNode { Handle = new EntityHandle(2, 1), Position = P(7200, 0), Team = 1, IsRepeater = true });
                g.Add(new MeshNode { Handle = new EntityHandle(3, 1), Position = P(14400, 0), Team = 1, IsRepeater = true });
                g.Add(new MeshNode { Handle = new EntityHandle(4, 1), Position = P(21600, 0), Team = 1, IsRepeater = true });

                g.Rebuild(1, Fix.FromInt(SimConstants.MeshRangePerHopMetres), SimConstants.MeshMaxHops);

                Assert.Equal(0, g.HopsAt(0), "the anchor is at depth zero");
                Assert.Equal(1, g.HopsAt(1), "one hop out");
                Assert.Equal(2, g.HopsAt(2), "two hops out");
                Assert.Equal(3, g.HopsAt(3), "three hops out, 21.6 km from home");
            });

            r.Run("a node beyond the last hop is not reached", delegate
            {
                MeshGraph g = new MeshGraph();
                g.Add(new MeshNode { Handle = new EntityHandle(1, 1), Position = P(0, 0), Team = 1, IsAnchor = true });
                g.Add(new MeshNode { Handle = new EntityHandle(2, 1), Position = P(18000, 0), Team = 1, IsRepeater = true });
                g.Rebuild(1, Fix.FromInt(SimConstants.MeshRangePerHopMetres), SimConstants.MeshMaxHops);
                Assert.False(g.IsConnected(1), "18 km is too far for one 8.4 km hop");
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
                    g.Add(new MeshNode { Handle = new EntityHandle(11, 1), Position = P(0, 7200), Team = 1, IsAnchor = true });
                    g.Add(new MeshNode { Handle = new EntityHandle(12, 1), Position = P(3600, 3600), Team = 1, IsRepeater = true });
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                TetherSystem ts = new TetherSystem(8, t, new DetRandom(1));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(1200, 1200), F(16800), 1, 0);

                // Fly out, then turn the corner and fly across.
                bool cut;
                for (int x = 1200; x <= 4800; x += 120) ts.Update(id, P(x, 1200), 0, out cut);
                for (int y = 1200; y <= 4800; y += 120) ts.Update(id, P(4800, y), 0, out cut);

                TetherSystem.Tether te = ts.Get(id);
                // Roughly 7.2 km of line paid out to reach a point 5.1 km away.
                Assert.InRange(6720, 7680, te.Spooled.ToDoubleForDisplay(), "line paid out");
                Assert.True(te.NodeCount > 20, "the thread has real geometry, not two endpoints");
            });

            r.Run("the leash is hard, and over-extending eventually parts the line", delegate
            {
                Terrain t = new Terrain(49152, 49152);
                t.Fill(TileClass.Open);
                TetherSystem ts = new TetherSystem(8, t, new DetRandom(1));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(1200, 1200), F(3600), 1, 0);

                bool cut = false;
                int tick = 0;
                for (int x = 1200; x <= 9600 && !cut; x += 120)
                    ts.Update(id, P(x, 1200), tick++, out cut);

                Assert.True(ts.Get(id).State == TetherState.Taut
                            || ts.Get(id).State == TetherState.Cut, "at full stretch");

                // Holding at full stretch parts it after the grace period. Simply
                // preventing the drone from going further would be safer and far
                // less interesting.
                for (int i = 0; i < SimConstants.TetherTautGraceTicks + 4 && !cut; i++)
                    ts.Update(id, P(9600, 1200), tick++, out cut);
                Assert.True(cut, "the line parts after three seconds at the leash");
            });

            r.Run("at full stretch a drone can fly back but not further out", delegate
            {
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                TetherSystem ts = new TetherSystem(8, t, new DetRandom(1));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(1200, 1200), F(2400), 1, 0);

                bool cut;
                int tick = 0;
                for (int x = 1200; x <= 4080; x += 120) ts.Update(id, P(x, 1200), tick++, out cut);
                if (ts.Get(id).State != TetherState.Taut) return; // nothing to assert

                Fix2 outward = ts.ConstrainVelocity(id, P(4080, 1200), new Fix2(F(10), Fix.Zero));
                Assert.True(outward.X <= F(0.01), "cannot pull further away");

                Fix2 inward = ts.ConstrainVelocity(id, P(4080, 1200), new Fix2(F(-10), Fix.Zero));
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                TetherSystem ts = new TetherSystem(8, t, new DetRandom(1));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(1200, 1200), F(16800), 1, 0);

                bool cut;
                int tick = 0;
                for (int x = 1200; x <= 4800; x += 120) ts.Update(id, P(x, 1200), tick++, out cut);

                ts.Cut(id, tick);
                ts.Update(id, P(4800, 1200), ++tick, out cut);
                Assert.True(ts.Get(id).State == TetherState.Lingering, "the line remains");
                Assert.True(ts.AnySegmentNear(id, P(3000, 1200), F(240)),
                            "an enemy walking over it would find it");

                // And it fades after half a minute rather than cluttering the map.
                for (int i = 0; i < SimConstants.TetherLingerTicks + 4; i++)
                    ts.Update(id, P(4800, 1200), ++tick, out cut);
                Assert.True(ts.Get(id).State == TetherState.Free, "gone after thirty seconds");
            });
        }

        static int CountCuts(TileClass tile, int trials)
        {
            int cuts = 0;
            for (int trial = 0; trial < trials; trial++)
            {
                Terrain t = new Terrain(24576, 24576);
                t.Fill(tile);
                TetherSystem ts = new TetherSystem(4, t, new DetRandom((ulong)(trial + 1)));
                int id = ts.Create(new EntityHandle(1, 1), EntityHandle.None, P(1200, 1200), F(24000), 1, 0);

                bool cut = false;
                int tick = 0;
                for (int x = 1200; x <= 8400 && !cut; x += 60)
                    ts.Update(id, P(x, 1200), tick++, out cut);
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
                EntityHandle m = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(12000, 12000));

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
                    EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
                    EntityHandle m = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(12360, 12000));

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
                    EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
                    for (int d = 0; d < 9; d++)
                        w.SpawnDecoy(2, P(12000 + (d % 3) * 240 - 240, 12000 + (d / 3) * 240 - 240),
                                     TargetKind.HighValue, 10000);

                    EntityHandle m = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(12480, 12000));
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
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(11400, 12000));

                EntityHandle drone;
                SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(12000, 12000),
                                    EntityHandle.None, 0, out drone);
                w.Step();

                Assert.True(AutonomyClassifier.IsPilotedWithClearFeed(w, drone.Index, P(12600, 12000)),
                            "a piloted drone with a live link close to the target");

                // The same drone once its link has gone.
                w.Entities.Link[drone.Index].Pip = LinkPip.Black;
                Assert.False(AutonomyClassifier.IsPilotedWithClearFeed(w, drone.Index, P(12600, 12000)),
                             "not once the pilot has lost the picture");
            });

            r.Run("an unmanned mount is willing to shoot a ground decoy (AUDIT-UNWIRED F18)", delegate
            {
                // CombatSystem.BestTargetInRange used to skip every
                // ComponentMask.Decoy entity for every attacker, piloted or
                // not - so a Gun Mount, which is never piloted at all, could
                // never spend a shot on one either. That directly contradicts
                // FINDINGS #17/#25: a decoy is supposed to be "one more thing
                // worth shooting at" to ordinary shot-value targeting.
                World w = MakeWorld(720);
                EntityHandle decoy = w.SpawnDecoy(1, P(12050, 12000), TargetKind.HighValue, 100000);
                w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(12000, 12000));

                bool everHit = false;
                // A full sweep of the mount's scanning head is ~5.1 s at
                // 70 deg/s; run several to be well clear of wherever the
                // sweep happens to start.
                for (int t = 0; t < SimConstants.Seconds(20) && !everHit; t++)
                {
                    w.Step();
                    if (!w.Entities.IsAlive(decoy) || w.Entities.Hp[decoy.Index] < Fix.FromInt(20))
                        everHit = true;
                }
                Assert.True(everHit, "an unmanned mount takes the shot the old blanket skip refused it");
            });

            r.Run("a piloted drone with a live link still ignores the same decoy", delegate
            {
                // The other half of F18: the fix narrows the immunity to the
                // piloted case, it does not remove it. Same decoy, an FPV Team
                // with nothing else to shoot at instead of a Gun Mount.
                World w = MakeWorld(721);
                EntityHandle decoy = w.SpawnDecoy(2, P(12010, 12000), TargetKind.HighValue, 100000);
                // A radio link needs a Command Post/Relay Mast/Crew Quarters
                // in range to hold Green - without one it sits Amber, which
                // IsPilotedWithClearFeed correctly refuses to call "a live
                // link". Same fixture the existing "deception fools machines"
                // test above already uses for the same reason.
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(11400, 12000));

                EntityHandle drone;
                SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(12000, 12000),
                                    EntityHandle.None, 0, out drone);

                for (int t = 0; t < SimConstants.Seconds(15); t++) w.Step();

                Assert.True(w.Entities.IsAlive(decoy), "still standing");
                Assert.Equal(20, w.Entities.Hp[decoy.Index].RoundToInt(),
                             "untouched - a live-linked pilot was never fooled by it");
            });

            r.Run("a machine that picks wrong says so out loud", delegate
            {
                bool sawReport = false;
                for (int i = 0; i < 200 && !sawReport; i++)
                {
                    World w = MakeWorld((ulong)(3000 + i));
                    w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
                    for (int d = 0; d < 9; d++)
                        w.SpawnDecoy(2, P(12000 + d * 96 - 384, 12120), TargetKind.HighValue, 10000);
                    EntityHandle m = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(12480, 12000));

                    bool mis;
                    AutonomyClassifier.SelectTarget(w, m.Index, out mis);
                    if (mis && w.Events.CountOf(SimEventKind.AutonomyMisidentified) > 0) sawReport = true;
                }
                Assert.True(sawReport, "a misidentification is reported, never swallowed");
            });

            r.Run("a no-go bubble stops a munition diving on its own side (AUDIT-UNWIRED F17)", delegate
            {
                // autonomy.md §9.3: fratricide is "1-3% of autonomous
                // engagements, dropping to near zero if the player has
                // designated a no-go bubble".
                //
                // Everything here goes through the production path on purpose -
                // World.Enqueue, World.Step, and the munition's own weapon loop
                // reaching AutonomyClassifier by itself. Nothing hand-assigns
                // AutonomyState, because a test that sets the state it then
                // asserts on is exactly the failure FINDINGS 30 is about, and
                // the box's whole problem was that only a test had ever written
                // one.
                Fix lostWithout, lostWith;
                int without = NoGoBubbleTrial(4100, false, out lostWithout);
                int with = NoGoBubbleTrial(4100, true, out lostWith);

                // The control has to be loud, or the box proves nothing: with a
                // friendly as the only thing in the seeker cone the classifier
                // has no genuine target to prefer and picks it every time.
                Assert.True(without > 0, "control: unprotected, it reports killing its own (got "
                                         + without + " reports)");
                Assert.True(lostWithout.Raw > 0, "control: and the friendly actually took the hits");

                Assert.Equal(0, with, "inside the bubble it is never considered at all");
                Assert.Equal(0, lostWith.RoundToInt(), "so the friendly is untouched");
            });

            r.Run("the bubble lapses, and stops protecting anything when it does", delegate
            {
                // The expiry is not decoration - see AutonomyState.BoxExpiryTick
                // for why a bubble a player never refreshes is worse than none.
                // Same fixture, a bubble with four play-seconds on it, run long
                // past that: the friendly is safe at first and not afterwards.
                World w = MakeWorld(4200);
                EntityHandle friendly = w.Spawn(Catalog.IdOf("Main Tank"), 1, P(12000, 12000));
                w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(12080, 12000));
                w.Enqueue(Command.SetAutonomyBox(1, P(11950, 11950), P(12050, 12050),
                                                 SimConstants.PlaySeconds(4)));

                int before = 0, after = 0;
                for (int t = 0; t < SimConstants.PlaySeconds(12); t++)
                {
                    w.Step();
                    int n = w.Events.CountOf(SimEventKind.AutonomyMisidentified);
                    if (w.Tick <= SimConstants.PlaySeconds(4)) before += n; else after += n;
                }

                Assert.Equal(0, before, "protected while the designation stands");
                Assert.True(after > 0, "and on its own again once it lapses (got " + after + ")");
                Assert.True(w.Entities.Hp[friendly.Index] < Catalog.Get(Catalog.IdOf("Main Tank")).Hp,
                            "which is a real cost, not a flag flipping");
            });

            r.Run("the bubble belongs to the team, so a munition launched later inherits it", delegate
            {
                // Per-team rather than per-airframe, and this is the case that
                // decides it: these are one-way munitions that spend their whole
                // life inside a single order, so a bubble that only reached what
                // was already in the air would reach almost nothing. The order
                // is given first and the munition built afterwards, which is the
                // ordinary way round.
                World w = MakeWorld(4300);
                EntityHandle friendly = w.Spawn(Catalog.IdOf("Main Tank"), 1, P(12000, 12000));
                w.Enqueue(Command.SetAutonomyBox(1, P(11950, 11950), P(12050, 12050),
                                                 SimConstants.PlaySeconds(60)));
                w.Step();

                w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(12080, 12000));

                int reports = 0;
                for (int t = 0; t < SimConstants.Seconds(20); t++)
                {
                    w.Step();
                    reports += w.Events.CountOf(SimEventKind.AutonomyMisidentified);
                }
                Assert.Equal(0, reports, "an airframe that did not exist when the line was drawn obeys it");
                Assert.Equal(Catalog.Get(Catalog.IdOf("Main Tank")).Hp.RoundToInt(),
                             w.Entities.Hp[friendly.Index].RoundToInt(), "untouched");
            });

            r.Run("the bubble spares whoever is standing in it, enemy included", delegate
            {
                // Not a friend filter: the machine is not offered anything
                // inside the rectangle, so an opponent who parks in one is as
                // safe as the vehicles it was drawn for. That is the price of
                // the order, and the reason to draw it tightly - stated here as
                // a test rather than a comment because it is the half of the
                // design a player will discover the expensive way.
                World w = MakeWorld(4400);
                EntityHandle enemy = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
                w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(12080, 12000));
                w.Enqueue(Command.SetAutonomyBox(1, P(11950, 11950), P(12050, 12050),
                                                 SimConstants.PlaySeconds(60)));

                for (int t = 0; t < SimConstants.Seconds(20); t++) w.Step();

                Assert.Equal(Catalog.Get(Catalog.IdOf("Main Tank")).Hp.RoundToInt(),
                             w.Entities.Hp[enemy.Index].RoundToInt(),
                             "a tank inside your own no-go area is a tank you told your munitions to ignore");
            });
        }

        /// <summary>
        /// One run of the fratricide fixture: a friendly vehicle and an
        /// autonomous munition 80 m away, which is inside the seeker cone. With
        /// the friendly as the only candidate the classifier has no genuine
        /// target to prefer, so it picks the friendly on both branches of its
        /// roll - which makes this the sharpest possible control for the box.
        /// Returns how many times the munition reported killing its own, and
        /// hands back the health the friendly lost doing it.
        /// </summary>
        static int NoGoBubbleTrial(ulong seed, bool drawBubble, out Fix hpLost)
        {
            World w = MakeWorld(seed);
            EntityHandle friendly = w.Spawn(Catalog.IdOf("Main Tank"), 1, P(12000, 12000));
            w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(12080, 12000));
            Fix hp0 = w.Entities.Hp[friendly.Index];

            // The real command path: queued, executed inside Step, applied by
            // CommandBuffer.Apply. The munition sits outside the rectangle and
            // the friendly inside it.
            if (drawBubble)
                w.Enqueue(Command.SetAutonomyBox(1, P(11950, 11950), P(12050, 12050),
                                                 SimConstants.PlaySeconds(60)));

            int reports = 0;
            for (int t = 0; t < SimConstants.Seconds(20); t++)
            {
                w.Step();
                reports += w.Events.CountOf(SimEventKind.AutonomyMisidentified);
            }
            hpLost = hp0 - w.Entities.Hp[friendly.Index];
            return reports;
        }

        // ------------------------------------------------------------------

        static void RegisterCrews(TestRunner r)
        {
            r.Group("crews");

            r.Run("crews cap how many sorties are up at once, not how many drones you own", delegate
            {
                World w = MakeWorld(20);
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(6000, 6000));
                w.Player(1).Materiel = Fix.FromInt(100000);

                int launched = 0;
                for (int i = 0; i < 20; i++)
                {
                    EntityHandle h;
                    if (SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(6000, 6000),
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
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(6000, 6000));
                w.Player(1).Materiel = Fix.FromInt(100000);

                for (int i = 0; i < SimConstants.StartingCrews; i++)
                {
                    EntityHandle h;
                    SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(6000, 6000),
                                        EntityHandle.None, i, out h);
                }
                EntityHandle extra;
                LaunchResult res = SortieSystem.Launch(w, 1, Catalog.IdOf("FPV Team"), P(6000, 6000),
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

            r.Run("a reusable airframe lands and hands its crew back (AUDIT-UNWIRED F13)", delegate
            {
                // Before this, SortieSystem.Recover had zero call sites: the
                // only way any of the five reusable airframes ever freed a
                // crew was to be destroyed. A Scout Quad here should behave
                // like what it is - a scout that comes home - not like a
                // one-way munition that happens to survive.
                World w = MakeWorld(22);
                Fix2 pad = P(6000, 6000);
                Fix2 outbound = P(6300, 6000); // 300 m out: well past both the
                                               // 24 m arrival radius and the
                                               // 30 m landing radius, so the
                                               // return leg is unambiguous.

                // A radio link out of range of any anchor sits Amber, and an
                // amber link refuses new orders - the second OrderMoveTo below
                // needs a live link to actually turn the drone around, which
                // is a fact about MovementSystem.OrderMoveTo rather than
                // anything this test is trying to prove, so give it a
                // Command Post the way the existing "deception fools
                // machines" test already does for the same reason.
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(5900, 6000));

                EntityHandle drone;
                LaunchResult res = SortieSystem.Launch(w, 1, Catalog.IdOf("Scout Quad"), pad,
                                                       EntityHandle.None, 0, out drone);
                Assert.Equal((long)LaunchResult.Launched, (long)res, "the sortie gets airborne");
                Assert.Equal(SimConstants.StartingCrews - 1, w.Player(1).Crews.ReadyCount,
                             "one crew is up");

                MovementSystem.OrderMoveTo(w, drone, outbound);
                int ticksOut = 0;
                while (Fix2.Distance(w.Entities.Position[drone.Index], pad).ToDoubleForDisplay() < 100
                       && ticksOut < 2000)
                {
                    w.Step();
                    ticksOut++;
                }
                Assert.True(w.Entities.Sortie[drone.Index].HasLeftHome,
                            "it actually left before anything can land it");
                Assert.Equal(SimConstants.StartingCrews - 1, w.Player(1).Crews.ReadyCount,
                             "still out - flying away is not landing");

                MovementSystem.OrderMoveTo(w, drone, pad);
                int ticksBack = 0;
                while (w.Entities.Sortie[drone.Index].CrewId >= 0 && ticksBack < 2000)
                {
                    w.Step();
                    ticksBack++;
                }

                Assert.Equal(-1, w.Entities.Sortie[drone.Index].CrewId, "the crew has been handed back");

                // Release() puts the crew into its ordinary post-flight
                // Recovering state rather than straight back to Ready - the
                // same as any other landing - so wait that out too before
                // asking whether the crew is actually flyable again.
                for (int i = 0; i < SimConstants.CrewRecoveryTicks + 8; i++) w.Step();

                Assert.Equal(SimConstants.StartingCrews, w.Player(1).Crews.ReadyCount,
                             "and is available to fly again - not lost, not stuck recovering forever");
                Assert.Equal(SimConstants.StartingCrews, w.Player(1).Crews.Count,
                             "no crew was created or destroyed in the process");
            });

            r.Run("a flight leaves the pad staggered, not stacked (AUDIT-UNWIRED F19)", delegate
            {
                // SortieState.EgressUntilTick used to be write-only: computed
                // from the launch index and read nowhere, so the balance
                // harness had to fake the spacing itself by enqueueing launch
                // commands on different ticks (KZ.Balance/Program.cs's own
                // "Arrival scheduling" note). Two drones launched from the
                // same pad in the same tick, indices 0 and 1, should now
                // separate on their own.
                World w = MakeWorld(23);
                Fix2 pad = P(6000, 6000);
                Fix2 far = P(9000, 6000);

                EntityHandle first, second;
                SortieSystem.Launch(w, 1, Catalog.IdOf("Scout Quad"), pad, EntityHandle.None, 0, out first);
                SortieSystem.Launch(w, 1, Catalog.IdOf("Scout Quad"), pad, EntityHandle.None, 1, out second);
                MovementSystem.OrderMoveTo(w, first, far);
                MovementSystem.OrderMoveTo(w, second, far);

                int firstEgress = w.Entities.Sortie[first.Index].EgressUntilTick;
                int secondEgress = w.Entities.Sortie[second.Index].EgressUntilTick;
                Assert.True(secondEgress > firstEgress, "the second index egresses later than the first");

                // Step to a tick after the first drone's own egress window and
                // before the second's - only reachable at all because
                // SortiePadEgressPerIndexTicks is nonzero, i.e. the two
                // windows actually differ.
                while (w.Tick < firstEgress) w.Step();
                w.Step();

                Assert.True(w.Tick < secondEgress,
                            "test still inside the second drone's egress window");
                Assert.True(w.Entities.Position[first.Index].X != pad.X,
                            "the first drone has started moving");
                Assert.True(w.Entities.Position[second.Index].X == pad.X,
                            "the second drone is still held at the pad");
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
                w.Spawn(Catalog.IdOf("Command Post"), 2, P(10800, 10800));
                EntityHandle truck = w.Spawn(Catalog.IdOf("Supply Truck"), 2, P(10800, 12000));
                w.SpawnMine(1, P(13200, 12000), F(600));
                w.Enqueue(Command.MoveTo(2, truck, P(15600, 12000)));

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
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(10800, 10800));
                EntityHandle friendly = w.Spawn(Catalog.IdOf("Supply Truck"), 1, P(10800, 12000));
                w.SpawnMine(1, P(13200, 12000), F(600));
                w.Enqueue(Command.MoveTo(1, friendly, P(15600, 12000)));

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
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(12000, 12000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("Scout Quad"), 1, P(13200, 12000));
                w.SpawnMine(2, P(13200, 12000), F(600));
                for (int i = 0; i < 200; i++) w.Step();
                Assert.True(w.Entities.IsAlive(drone), "a drone flies over a minefield");
            });

            r.Run("a mine needs a moment to arm", delegate
            {
                World w = MakeWorld(63);
                w.Spawn(Catalog.IdOf("Command Post"), 2, P(10800, 10800));
                EntityHandle truck = w.Spawn(Catalog.IdOf("Supply Truck"), 2, P(13200, 12000));
                w.SpawnMine(1, P(13200, 12000), F(600));

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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);

                World day = new World(t, 256, 16, 64, 2, 0);
                day.Spawn(Catalog.IdOf("Command Post"), 1, P(12000, 12000));
                day.Player(1).Materiel = Fix.FromInt(20000);
                EntityHandle h;
                Assert.Equal((long)LaunchResult.DaylightRefused,
                             (long)SortieSystem.Launch(day, 1, Catalog.IdOf("Night Bomber"),
                                                       P(12000, 12000), EntityHandle.None, 0, out h),
                             "refused by day");

                World night = new World(t, 256, 16, 64, 2, 8000);
                night.Spawn(Catalog.IdOf("Command Post"), 1, P(12000, 12000));
                night.Player(1).Materiel = Fix.FromInt(20000);
                Assert.Equal((long)LaunchResult.Launched,
                             (long)SortieSystem.Launch(night, 1, Catalog.IdOf("Night Bomber"),
                                                       P(12000, 12000), EntityHandle.None, 0, out h),
                             "flies after dark");
            });

            r.Run("a small drone is found far closer than a tank", delegate
            {
                // Detection reach scales with what the target is giving off, so the
                // same camera that picks a tank out at seven kilometres struggles
                // to find a quadcopter inside two. This is the fact the
                // whole subject rests on and the model had no way to express.
                World w = MakeWorld(65);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("Scout Quad"), 2, P(14400, 12000));
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(16800, 12000));
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
                // decibels or so once everything stops moving.
                //
                // This test used to go on to assert that against a small quad the
                // camera still won after dark. That was true only of two numbers
                // the research corrected: optical night was 0.35 where it should
                // be 0.20, and a quad's visual signature was 15 where it should be
                // 6 (thermal-optical.md §10, §11 - an unlit matte drone is close to
                // invisible to a camera at night). With the corrected values the
                // microphone is the defence's best sensor against a quad once the
                // sun goes down, and even that reaches far less than the daytime
                // camera did - so darkness is a real advantage, and against
                // anything with an engine the microphone wins outright.
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);

                World day = new World(t, 256, 16, 65, 2, 0);
                EntityHandle gunDay = day.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle droneDay = day.Spawn(Catalog.IdOf("FPV Team"), 2, P(14400, 12000));
                day.Step();

                World night = new World(t, 256, 16, 65, 2, 8000);
                EntityHandle gunNight = night.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle droneNight = night.Spawn(Catalog.IdOf("FPV Team"), 2, P(14400, 12000));
                night.Step();

                Assert.True(night.IsNight, "the second world is actually after dark");

                Fix opticalDay = day.DetectionRangeFor(gunDay.Index, droneDay.Index, SensorChannel.Optical);
                Fix opticalNight = night.DetectionRangeFor(gunNight.Index, droneNight.Index, SensorChannel.Optical);
                Assert.True(opticalNight < opticalDay, "cameras lose most of their reach after dark");

                Fix acousticDay = day.DetectionRangeFor(gunDay.Index, droneDay.Index, SensorChannel.Acoustic);
                Fix acousticNight = night.DetectionRangeFor(gunNight.Index, droneNight.Index, SensorChannel.Acoustic);
                Assert.True(acousticNight > acousticDay,
                            "quiet air after dark buys the microphone real reach");

                Assert.True(acousticNight > opticalNight,
                            "after dark the microphone, not the camera, is the best sensor "
                            + "against a small quad");
                Assert.True(acousticNight < opticalDay,
                            "and even that is well short of the daytime camera, "
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
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle quad = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(14400, 12000));
                EntityHandle engine = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 2, P(14400, 12000));
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                World w = new World(t, 256, 16, 301, 2);

                // Team 1 owns the west, team 2 the east, with a neutral strip.
                w.Territory.SetVerticalBorder(12288, 1, 2, 768);

                EntityHandle home = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(4800, 12000));
                EntityHandle across = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(19200, 12000));
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                World w = new World(t, 256, 16, 302, 2);
                w.Territory.SetVerticalBorder(12288, 1, 2, 0);

                EntityHandle d = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(10800, 12000));
                w.Step();
                Assert.Equal((int)LinkPip.Green, (int)w.Entities.Link[d.Index].Pip,
                             "green to begin with");

                // One step across. Not a long flight - one tick.
                w.Entities.Position[d.Index] = P(13200, 12000);
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                World w = new World(t, 256, 16, 303, 2);
                w.Territory.SetVerticalBorder(12288, 1, 2, 0);

                // Team 1 has pushed its front well east of its own border and is
                // sitting on ground it controls by every military measure.
                EntityHandle held = w.Spawn(Catalog.IdOf("Designator Team"), 1, P(16800, 12000));
                w.Spawn(Catalog.IdOf("Command Post"), 1, P(15600, 12000));
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Forest);      // matchable ground
                World w = new World(t, 256, 16, 401, 2);
                w.Territory.SetVerticalBorder(7200, 1, 2, 0);
                w.Imagery.GrantAround(1, P(14400, 12000), Fix.FromInt(7200));

                EntityHandle cheap = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(12000, 12000));
                EntityHandle withMap = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 1, P(12000, 12000));

                // A long run, because drift is a fraction of distance flown and a
                // few seconds of it proves nothing either way.
                for (int k = 0; k < 1500; k++)
                {
                    MovementSystem.OrderMoveTo(w, cheap, P(23400, 12000));
                    MovementSystem.OrderMoveTo(w, withMap, P(23400, 12000));
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
                Terrain flat = new Terrain(24576, 24576);
                flat.Fill(TileClass.Open);
                World w1 = new World(flat, 256, 16, 402, 2);
                w1.Territory.SetVerticalBorder(7200, 1, 2, 0);
                w1.Imagery.GrantAround(1, P(14400, 12000), Fix.FromInt(7200));

                Terrain broken = new Terrain(24576, 24576);
                broken.Fill(TileClass.Forest);
                World w2 = new World(broken, 256, 16, 402, 2);
                w2.Territory.SetVerticalBorder(7200, 1, 2, 0);
                w2.Imagery.GrantAround(1, P(14400, 12000), Fix.FromInt(7200));

                EntityHandle a = w1.Spawn(Catalog.IdOf("Heavy Strike Drone"), 1, P(12000, 12000));
                EntityHandle b = w2.Spawn(Catalog.IdOf("Heavy Strike Drone"), 1, P(12000, 12000));
                for (int k = 0; k < 200; k++)
                {
                    MovementSystem.OrderMoveTo(w1, a, P(16800, 12000));
                    MovementSystem.OrderMoveTo(w2, b, P(16800, 12000));
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Forest);
                World w = new World(t, 256, 16, 403, 2);
                w.Territory.SetVerticalBorder(7200, 1, 2, 0);
                w.Imagery.GrantAround(1, P(14400, 12000), Fix.FromInt(4800));

                Assert.True(NavigationSystem.CanMatchHere(w, 1, P(14400, 12000)),
                            "imagery of this sector, so it can be matched");

                w.Imagery.Invalidate(P(14400, 12000), Fix.FromInt(2400));
                Assert.True(!NavigationSystem.CanMatchHere(w, 1, P(14400, 12000)),
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);        // nothing to match, so both dead reckon
                World w = new World(t, 256, 16, 404, 2);
                w.Territory.SetVerticalBorder(7200, 1, 2, 0);

                EntityHandle plain = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 1, P(12000, 12000));
                EntityHandle starry = w.Spawn(Catalog.IdOf("Jet Strike Drone"), 1, P(12000, 12000));
                for (int k = 0; k < 200; k++)
                {
                    MovementSystem.OrderMoveTo(w, plain, P(18000, 12000));
                    MovementSystem.OrderMoveTo(w, starry, P(18000, 12000));
                    w.Step();
                }

                Fix plainError = w.Entities.Nav[plain.Index].ErrorMetres;
                Fix starryError = w.Entities.Nav[starry.Index].ErrorMetres;

                Assert.True(starryError < plainError, "celestial slows the drift");
                Assert.True(starryError > Fix.Zero,
                            "but it is still drifting - it never got a position fix");
            });

            r.Run("navigation error displaces a one-way munition's aimpoint", delegate
            {
                // AUDIT-UNWIRED.md F5: NavState.ErrorMetres was computed by the
                // whole navigation system and read by nothing but tests. Two
                // dead-reckoning FPV Teams cross the same border on the same
                // heading; one's target sits just past it, the other's target
                // sits deep behind it. navigation-denied.md §5's own worked
                // example - 3% of distance flown, 600 m over a 20 km run -
                // says the shallow shot should still land and the deep
                // one should not, and only World.Step producing that difference
                // (not a hand-assigned NavState) is evidence it is wired.
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                World w = new World(t, 64, 8, 405, 2);
                w.Territory.SetVerticalBorder(7200, 1, 2, 0);

                // Two parallel lanes, 4.8 km apart, so the far drone's straight
                // line to its own target never passes within weapon range of
                // the near tank.
                Fix2 nearTarget = P(8400, 12000);   // ~1.1 km of denied ground to cross
                Fix2 farTarget = P(19200, 16800);   // ~11.9 km of denied ground to cross

                EntityHandle nearTank = w.Spawn(Catalog.IdOf("Main Tank"), 2, nearTarget);
                EntityHandle farTank = w.Spawn(Catalog.IdOf("Main Tank"), 2, farTarget);
                Fix fullHp = Catalog.Get(Catalog.IdOf("Main Tank")).Hp;

                EntityHandle nearDrone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(7080, 12000));
                EntityHandle farDrone = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(7080, 16800));

                // Long enough for the far drone to actually arrive: 12.1 km of
                // denied ground at 33 m/s is 2,940 ticks of world, where the same
                // geometry in compressed units took 2,200.
                int misses = 0;
                for (int k = 0; k < 3400; k++)
                {
                    if (w.Entities.IsAlive(nearDrone)) MovementSystem.OrderMoveTo(w, nearDrone, nearTarget);
                    if (w.Entities.IsAlive(farDrone)) MovementSystem.OrderMoveTo(w, farDrone, farTarget);
                    w.Step();
                    misses += w.Events.CountOf(SimEventKind.NavMissedAimpoint);
                }

                Assert.True(w.Entities.Hp[nearTank.Index] < fullHp,
                            "1.1 km of dead reckoning rounds to nothing - the near shot lands");
                Assert.True(w.Entities.Hp[farTank.Index] == fullHp,
                            "11.9 km of it does not - the far shot goes off on empty ground");
                Assert.True(misses > 0,
                            "and the simulation says so, rather than silently doing no damage");
            });

            r.Run("a pilot watching the target does not miss for navigational reasons", delegate
            {
                // FINDINGS 34's open defect. The F5 gate above asked only
                // "did this airframe drift", so an FPV Team - dead reckoning,
                // one-way, and flown the whole way in on a camera by a person -
                // missed every time it crossed enough denied ground, and a Gun
                // Mount was immortal at every drone count in every experiment.
                //
                // Nothing in navigation-denied.md §5 says that should happen:
                // its separability argument is about position versus operator,
                // and FINDINGS 28 is explicit that terminal guidance keeps its
                // crew and makes the shot better rather than worse. The gate
                // now asks whether the munition still needs to know where it
                // is when it arrives, and a live feed is the answer that says
                // no.
                //
                // Both lanes fly the identical run and accrue the identical
                // error. The only difference is whether there is a radio
                // anchor behind them, which is to say whether anybody is
                // still flying it - so this is also the black-link half of the
                // decision, on the record: cutting the link is what makes a
                // drone's position start mattering, and the two denials
                // compound.
                DeniedStrike linked = FlyDeniedStrike("FPV Team", true, 4200, 2600);
                DeniedStrike black = FlyDeniedStrike("FPV Team", false, 4200, 2600);

                Assert.True(linked.PeakError > SimConstants.MunitionMissRadiusMetres,
                            "the drift really happened - 4.7 km of denied ground at 3% is "
                            + linked.PeakError.RoundToInt() + " m against a 40 m radius");
                Assert.True(black.PeakError > SimConstants.MunitionMissRadiusMetres,
                            "and identically in the unanchored lane");

                Assert.True(linked.TargetDamaged,
                            "a person on a live picture flew it into the vehicle anyway");
                Assert.Equal(0, linked.Misses,
                             "and the simulation never claimed otherwise");

                Assert.True(!black.TargetDamaged,
                            "the same airframe with nobody flying it is on a remembered "
                            + "coordinate in its own drifted frame, and detonates on empty ground");
                Assert.True(black.Misses > 0, "which it says out loud");
            });

            r.Run("a fiber drone's picture is the one thing nothing can take away", delegate
            {
                // The rule stated at the rung where it is least escapable.
                // A Fiber FPV Team is dead reckoning like every other quad, so
                // it accrues exactly the same error over exactly the same
                // ground - but LinkKind.Fiber cannot be jammed and needs no
                // anchor behind it, so there is no state of the world in which
                // its crew loses the feed short of the thread parting. FINDINGS
                // 28: what fiber buys is narrow and specific, and this is one
                // of the two things on that list being worth something.
                DeniedStrike fiber = FlyDeniedStrike("Fiber FPV Team", false, 4201, 3600);

                Assert.True(fiber.PeakError > SimConstants.MunitionMissRadiusMetres,
                            "same drift as any other quad ("
                            + fiber.PeakError.RoundToInt() + " m)");
                Assert.True(fiber.TargetDamaged, "and it lands the shot regardless");
                Assert.Equal(0, fiber.Misses, "no navigational miss anywhere in the run");
            });

            r.Run("a reconnaissance sortie buys the imagery scene matching needs", delegate
            {
                // AUDIT-UNWIRED.md F6 / navigation-denied.md §6, the supply end.
                // GrantAround had zero callers outside KZ.Tests. A Recon Wing
                // spawned and flown - nothing more - should light up the ground
                // it passes over on its own, through World.Step, not because the
                // test told the imagery resource to appear.
                Terrain t = new Terrain(30720, 24576);
                t.Fill(TileClass.Open);
                World w = new World(t, 64, 8, 406, 2);

                Fix2 farGround = P(25200, 12000);
                Assert.False(w.Imagery.HasCoverage(1, farGround),
                             "nothing has flown near this ground yet");

                EntityHandle scout = w.Spawn(Catalog.IdOf("Recon Wing"), 1, P(12000, 12000));
                for (int k = 0; k < 700; k++)
                {
                    MovementSystem.OrderMoveTo(w, scout, farGround);
                    w.Step();
                }

                Assert.True(w.Imagery.HasCoverage(1, farGround),
                            "the recon wing's own camera bought coverage of the ground it overflew");
                Assert.True(!w.Imagery.HasCoverage(2, farGround),
                            "and only for the side that flew the sortie");
            });

            r.Run("heavy bombardment invalidates imagery; a routine kamikaze hit does not", delegate
            {
                // navigation-denied.md §6: a coverage resource invalidated by
                // events a player watched happen, not a timer - and Invalidate
                // had zero callers outside KZ.Tests. Two attacks land on the
                // same covered ground: an FPV Team's kamikaze warhead (260,
                // routine) and a Heavy Strike Drone's (520, the kind of hit the
                // research calls "bombardment"). Only the second should churn
                // the sector.
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                World w = new World(t, 64, 8, 407, 2);
                w.Territory.Fill(1);   // both attackers are on their own ground throughout - isolates the damage threshold from F5's aimpoint error

                Fix2 spot = P(12000, 12000);
                w.Imagery.GrantAround(1, spot, Fix.FromInt(600));
                Assert.True(w.Imagery.HasCoverage(1, spot), "test setup: covered to start");

                EntityHandle target1 = w.Spawn(Catalog.IdOf("Main Tank"), 2, spot);
                EntityHandle fpv = w.Spawn(Catalog.IdOf("FPV Team"), 1, P(11880, 12000));
                for (int k = 0; k < 60 && w.Entities.IsAlive(fpv); k++)
                {
                    MovementSystem.OrderAttack(w, fpv, target1);
                    w.Step();
                }

                Assert.True(w.Entities.Hp[target1.Index] < Catalog.Get(Catalog.IdOf("Main Tank")).Hp,
                            "test setup: the kamikaze hit actually landed");
                Assert.True(w.Imagery.HasCoverage(1, spot),
                            "a 260-warhead kamikaze hit is not what the research calls bombardment");

                // An explicit attack order, not a move order: Heavy Strike Drone
                // carries Link.Autonomy, so with no order at all FindTarget would
                // route it through AutonomyClassifier instead, which returns
                // nothing for a terminal-guidance airframe by design (FINDINGS
                // 28) - a human already picked the target, which is what this
                // order represents.
                EntityHandle heavy = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 1, P(11880, 12000));
                for (int k = 0; k < 60 && w.Entities.IsAlive(heavy); k++)
                {
                    MovementSystem.OrderAttack(w, heavy, target1);
                    w.Step();
                }

                Assert.True(!w.Imagery.HasCoverage(1, spot),
                            "a 520-warhead strike churns the ground it lands on");
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
                EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(12000, 12000));
                EntityHandle bomber = w.Spawn(Catalog.IdOf("Night Bomber"), 1, P(12000, 12000));
                EntityHandle target = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 2, P(15600, 12000));
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                for (int tx = 0; tx < t.WidthTiles; tx++) t.Set(tx, 60, TileClass.Road);
                World w = new World(t, 256, 16, 502, 2);

                EntityHandle onRoad = w.Spawn(Catalog.IdOf("Supply Truck"), 1,
                                              P(6000, 60 * SimConstants.BuildTileMetres + 48));
                EntityHandle offRoad = w.Spawn(Catalog.IdOf("Supply Truck"), 1, P(6000, 10800));
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
                w.SpawnDecoy(2, P(14400, 12000), TargetKind.HighValue, 6000);
                w.SpawnDecoy(2, P(14520, 12120), TargetKind.HighValue, 6000);
                w.Spawn(Catalog.IdOf("Main Tank"), 2, P(14640, 12240));

                EntityHandle guided = w.Spawn(Catalog.IdOf("Loitering Munition"), 1, P(13800, 12000));
                EntityHandle choosing = w.Spawn(Catalog.IdOf("Autonomous Munition"), 1, P(13800, 12000));
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

            r.Run("a drone it cannot reach does not stop it shooting the ones it can", delegate
            {
                // A gun mount cannot engage the High band at all - that is the
                // design, and it is how one-way attack drones walked away from gun
                // defence. But the target scorer did not know it. It gated on
                // weapon range and on CanEngage, neither of which consults the
                // altitude ceiling, so it would happily select a high drone and the
                // firing code would then compute a reach of zero and return having
                // fired at nothing.
                //
                // A ceiling meant as immunity for one attacker was therefore also a
                // jammer protecting every other attacker in the sky.
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                World w = new World(t, 256, 16, 901, 2);
                w.Territory.SetVerticalBorder(12288, 1, 2, 0);

                // The high one must be the *better* shot, or the scorer picks the
                // reachable target anyway and the bug hides. A 55-point FPV is a
                // larger fraction of a kill per shot than a 110-point quad, so put
                // the FPV out of reach and the quad within it - which is also the
                // tactically sensible way to fly the pair.
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle low = w.Spawn(Catalog.IdOf("Multirole Quad"), 2, P(12600, 12000));
                EntityHandle high = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(12576, 12000));
                w.Entities.EntityLayer[high.Index] = Layer.High;

                Fix startHp = w.Entities.Hp[low.Index];
                for (int k = 0; k < 400; k++)
                {
                    MovementSystem.OrderMoveTo(w, low, P(12000, 12000));
                    MovementSystem.OrderMoveTo(w, high, P(12000, 12000));
                    w.Step();
                    if (!w.Entities.IsAlive(low)) break;
                }

                Assert.True(!w.Entities.IsAlive(low) || w.Entities.Hp[low.Index] < startHp,
                            "the mount engages the drone it can reach, "
                            + "regardless of what else is overhead");
                Assert.True(w.Entities.IsAlive(high),
                            "and still cannot touch the one above its ceiling");
            });

            r.Run("a fiber drone is beaten by everything except a jammer", delegate
            {
                // Guarding a claim I got wrong once. A fiber drone's radio
                // signature is zero, and it is tempting to read that as immunity.
                // It is not. The fiber buys exactly two things - it cannot be
                // jammed, and passive radio listening cannot find it - and every
                // other way of killing a drone works normally.
                //
                // Four of the five channels see it. A gun shoots it. An
                // interceptor rams it. That is what "the counter to any rung sits
                // one rung back" was always supposed to mean.
                World w = MakeWorld(701);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(12000, 12000));
                EntityHandle bomber = w.Spawn(Catalog.IdOf("Night Bomber"), 1, P(12000, 12000));
                EntityHandle fiber = w.Spawn(Catalog.IdOf("Fiber FPV Team"), 2, P(12480, 12000));
                w.Step();

                Assert.Equal(0, w.DetectionRangeFor(mast.Index, fiber.Index, SensorChannel.Esm).Raw,
                             "passive listening finds nothing, which is the point of the fiber");

                Assert.True(w.DetectionRangeFor(gun.Index, fiber.Index, SensorChannel.Optical) > Fix.Zero,
                            "but a camera sees it");
                Assert.True(w.DetectionRangeFor(gun.Index, fiber.Index, SensorChannel.Acoustic) > Fix.Zero,
                            "a microphone hears it - it is still a quadcopter");
                Assert.True(w.DetectionRangeFor(mast.Index, fiber.Index, SensorChannel.Radar) > Fix.Zero,
                            "radar returns off it like anything else");
                Assert.True(w.DetectionRangeFor(bomber.Index, fiber.Index, SensorChannel.Thermal) > Fix.Zero,
                            "and its motors are warm");

                Assert.True(w.IsDetectedBy(1, fiber), "so the defence has it");

                // And a rotary interceptor with a seeker is a real counter to it,
                // which is the other half of the same correction.
                UnitDef interceptor = Catalog.Get(Catalog.IdOf("Interceptor FPV"));
                Assert.True(interceptor.CanEngageAir, "interceptors engage air");
                Assert.True(interceptor.SensorOptical > Fix.Zero, "using an optical seeker");
            });

            r.Run("a fiber drone defeats radio listening completely", delegate
            {
                // The property that makes fiber worth its leash, and one the old
                // model could not represent: there is no transmission to find.
                World w = MakeWorld(67);
                EntityHandle post = w.Spawn(Catalog.IdOf("Command Post"), 1, P(12000, 12000));
                EntityHandle radio = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(15600, 12000));
                EntityHandle fiber = w.Spawn(Catalog.IdOf("Fiber FPV Team"), 2, P(15600, 12000));
                w.Step();

                Assert.True(w.DetectionRangeFor(post.Index, radio.Index, SensorChannel.Esm) > Fix.Zero,
                            "a radio drone is transmitting and can be heard");
                Assert.Equal(0, w.DetectionRangeFor(post.Index, fiber.Index, SensorChannel.Esm).Raw,
                             "a fiber drone is not transmitting at all");
            });

            r.Run("a jammer that is switched on is the easiest thing on the map to find", delegate
            {
                World w = MakeWorld(68);
                EntityHandle post = w.Spawn(Catalog.IdOf("Command Post"), 1, P(12000, 12000));
                EntityHandle jammer = w.Spawn(Catalog.IdOf("EW Post"), 2, P(16800, 12000));
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
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle radar = w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(12000, 12000));
                EntityHandle low = w.Spawn(Catalog.IdOf("Multirole Quad"), 2, P(14400, 12000));
                EntityHandle high = w.Spawn(Catalog.IdOf("Multirole Quad"), 2, P(14400, 12000));
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
                EntityHandle mast = w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(12000, 12000));
                EntityHandle decoy = w.Spawn(Catalog.IdOf("Decoy Drone"), 2, P(16800, 12000));
                EntityHandle strike = w.Spawn(Catalog.IdOf("Heavy Strike Drone"), 2, P(16800, 12000));
                w.Step();

                Fix vsDecoy = w.DetectionRangeFor(mast.Index, decoy.Index, SensorChannel.Radar);
                Fix vsStrike = w.DetectionRangeFor(mast.Index, strike.Index, SensorChannel.Radar);
                Assert.True(vsDecoy > vsStrike * Fix.FromInt(2),
                            "a decoy has to be seen far enough ahead of what it escorts "
                            + "to draw the engagement");

                // And the other end of the scale still works: a plastic quadcopter
                // is most of an order of magnitude below the decoy.
                EntityHandle quad = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(16800, 12000));
                w.Step();
                Fix vsQuad = w.DetectionRangeFor(mast.Index, quad.Index, SensorChannel.Radar);
                Assert.True(vsDecoy > vsQuad * Fix.FromInt(5),
                            "and a small quad is a genuinely hard radar target");
            });

            r.Run("a thermal blanket is real masking, not just a trick on machines", delegate
            {
                World w = MakeWorld(70);
                EntityHandle bomber = w.Spawn(Catalog.IdOf("Night Bomber"), 1, P(12000, 12000));
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(15600, 12000));
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
                Terrain t = new Terrain(24576, 24576);
                t.Fill(TileClass.Open);
                World w = new World(t, 256, 16, 66, 2, 8000);

                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle drone = w.Spawn(Catalog.IdOf("Scout Quad"), 2, P(16800, 12000));

                // Inside the gun's optical reach by day, well outside it after dark.
                for (int i = 0; i < 200; i++) w.Step();
                Assert.True(w.Entities.IsAlive(drone), "unseen and therefore unshot");
            });

            r.Run("a lost track is held for two seconds, not dropped instantly", delegate
            {
                // FINDINGS.md #21 describes a two-second track hold; until this was
                // wired, SimConstants.TrackHoldTicks had zero call sites and
                // Reaches() rerolled every fringe contact from scratch every tick
                // (AUDIT-UNWIRED F2). This exercises the production path end to
                // end: spawn, Step(), and watch IsDetectedBy - the same call
                // CombatSystem.CanEngage makes - through the loss and the hold.
                World w = MakeWorld(2100);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12600, 12000));
                w.Step();
                Assert.True(w.IsDetectedBy(1, tank), "close and large - solidly detected");

                // Move the target well beyond every channel's reach in one jump,
                // rather than flying it out over many ticks - the point is to give
                // RebuildDetection() a target with zero raw reach on every channel,
                // starting on a known tick, not to hand-write the track state
                // itself. Squared, this stays well inside Q31.32 range.
                w.Entities.Position[tank.Index] = P(12000, 252000);

                for (int i = 0; i < SimConstants.TrackHoldTicks; i++)
                {
                    w.Step();
                    Assert.True(w.IsDetectedBy(1, tank),
                                "tick " + i + " into the hold - the track should still be live");
                }

                w.Step();
                Assert.True(!w.IsDetectedBy(1, tank),
                            "one tick past the hold - the track should finally drop");
            });

            r.Run("the hold covers losing a track, not gaining one", delegate
            {
                // The asymmetry the design calls for: a target that has never been
                // solidly seen gets no grace from the hold. Getting this backwards
                // (granting the hold on entry too) would make fringe detection
                // stickier in both directions, which is not what the research asks
                // for - a marginal contact should still flicker while it is being
                // acquired.
                World w = MakeWorld(2101);
                w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 252000));

                for (int i = 0; i < SimConstants.TrackHoldTicks * 2; i++) w.Step();
                Assert.True(!w.IsDetectedBy(1, tank),
                            "never in range, so never acquired, so no hold applies");
            });
        }

        // ------------------------------------------------------------------

        /// <summary>What one denied strike run did, as observed from outside.</summary>
        struct DeniedStrike
        {
            public bool TargetDamaged;
            public Fix PeakError;     // the most NavigationSystem ever had it wrong by
            public int Misses;        // SimEventKind.NavMissedAimpoint over the run
        }

        /// <summary>
        /// Launch one one-way airframe from friendly ground, across a border, at a
        /// vehicle 4.7 km inside denied ground, and watch what arrives.
        ///
        /// Everything here is the production path on purpose. The airframe comes
        /// from SortieSystem.Launch, so it has a real crew out of the real pool;
        /// the order is the one Launch issues; the flight, the border crossing, the
        /// drift, the link resolution and the strike are all World.Step. Nothing
        /// hand-assigns NavState or LinkState - which is the whole point, since
        /// F5's original tests passed by constructing the state they asserted on.
        ///
        /// <paramref name="radioAnchor"/> is the only lever: with one, a radio link
        /// holds Green the whole way and somebody is flying the drone; without one
        /// it falls to Amber and then Black, LinkResolver releases the crew, and
        /// the airframe finishes on BlackPolicy.LastMile. Same geometry, same
        /// drift, different answer to "does it still need to know where it is".
        /// Open terrain throughout, so there is nothing to scene-match against and
        /// nothing for a fiber tether to snag on.
        /// </summary>
        static DeniedStrike FlyDeniedStrike(string airframe, bool radioAnchor, ulong seed, int ticks)
        {
            Terrain t = new Terrain(24576, 24576);
            t.Fill(TileClass.Open);
            World w = new World(t, 512, 64, seed, 2);
            w.Territory.SetVerticalBorder(7200, 1, 2, 0);

            // Within SimConstants.RadioRangeMetres of the whole run when it is
            // there at all, and simply absent when it is not - rather than parked
            // just out of range, which would make the test depend on the exact
            // radio range instead of on whether a link exists.
            if (radioAnchor) w.Spawn(Catalog.IdOf("Command Post"), 1, P(6000, 12000));

            EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
            Fix fullHp = w.Entities.Hp[tank.Index];

            EntityHandle drone;
            SortieSystem.Launch(w, 1, Catalog.IdOf(airframe), P(6000, 12000), tank, 0, out drone);

            DeniedStrike s = new DeniedStrike();
            s.PeakError = Fix.Zero;
            for (int k = 0; k < ticks; k++)
            {
                if (w.Entities.IsAlive(drone))
                {
                    Fix e = w.Entities.Nav[drone.Index].ErrorMetres;
                    if (e > s.PeakError) s.PeakError = e;
                }
                w.Step();
                s.Misses += w.Events.CountOf(SimEventKind.NavMissedAimpoint);
            }

            s.TargetDamaged = !w.Entities.IsAlive(tank) || w.Entities.Hp[tank.Index] < fullHp;
            return s;
        }

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

            r.Run("a fitted cage rolls a save, it does not bank hit points (AUDIT-UNWIRED F16)", delegate
            {
                // ground-force.md §2.1/§8.2: "the mechanism is not 'more
                // armour'... the effect is therefore probabilistic and
                // geometry-dependent, not a hit-point buffer", and "a cage
                // that merely adds standoff without disrupting the warhead
                // can raise penetration rather than lower it." CageHp used to
                // be exactly the hit-point buffer the research says a cage is
                // not, and nothing but a test could ever fit one (World had no
                // production writer at all). This drives enough hits through
                // World.FitCage's real consumer, World.ApplyDamage, to show
                // both halves of that: some hits do far less than an uncaged
                // one would, and at least one does *more* - which a hit-point
                // pool can never produce, because a pool only ever subtracts.
                World w = MakeWorld(42);
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
                w.FitCage(tank, F(0.55)); // ground-force.md §2.1's 0.30-0.80 spread, midpoint
                Fix hit = Catalog.Get(Catalog.IdOf("FPV Team")).WeaponDamage;
                Fix uncaged = hit * Catalog.DamageMultiplier(DamageType.Shaped, ArmourClass.Heavy, true);

                bool sawASave = false, sawAFailureWorseThanUncaged = false;
                Fix lastHp = w.Entities.Hp[tank.Index];
                for (int i = 0; i < 40 && w.Entities.Hp[tank.Index].Raw > 0; i++)
                {
                    // A different attacker handle each hit, so the
                    // deterministic per-hit roll (keyed on tick, target and
                    // attacker - see ApplyDamage) actually varies instead of
                    // landing on the same result forty times running.
                    w.ApplyDamage(tank, hit, DamageType.Shaped, true, new EntityHandle(i + 1, 1));
                    Fix taken = lastHp - w.Entities.Hp[tank.Index];
                    if (taken < uncaged) sawASave = true;
                    if (taken > uncaged) sawAFailureWorseThanUncaged = true;
                    lastHp = w.Entities.Hp[tank.Index];
                }

                Assert.True(sawASave, "at least one hit was disrupted well below the uncaged figure");
                Assert.True(sawAFailureWorseThanUncaged,
                            "and at least one failed save did more damage than no cage at all");
            });

            r.Run("a fitted cage still nets the tank a survivability win", delegate
            {
                // The point of building one: the same assault the tank could
                // not survive uncaged, it usually can with a 0.55 save chance
                // in play, because 0.55 saves at *0.15 plus 0.45 failures at
                // *1.15 nets well under 1.0 in expectation.
                World w = MakeWorld(43);
                EntityHandle uncaged = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
                EntityHandle caged = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(20000, 12000));
                w.FitCage(caged, F(0.55));

                Fix hit = Catalog.Get(Catalog.IdOf("FPV Team")).WeaponDamage;
                for (int i = 0; i < 5; i++)
                {
                    EntityHandle attacker = new EntityHandle(i + 1, 1);
                    w.ApplyDamage(uncaged, hit, DamageType.Shaped, true, attacker);
                    w.ApplyDamage(caged, hit, DamageType.Shaped, true, attacker);
                }

                // Neither tank is actually destroyed here - ApplyDamage can
                // drive Hp negative on its own, and destruction only happens
                // when a Step() flushes it - so both Hp values compare
                // meaningfully even past zero.
                Assert.True(w.Entities.Hp[caged.Index] > w.Entities.Hp[uncaged.Index],
                             "the cage is ahead over five hits despite its own failure branch");
            });

            r.Run("fitting a thermal blanket shrinks a target's thermal reach (AUDIT-UNWIRED F16)", delegate
            {
                World w = MakeWorld(44);
                EntityHandle battery = w.Spawn(Catalog.IdOf("Interceptor Battery"), 1, P(12000, 12000));
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12500, 12000));

                Fix before = w.DetectionRangeFor(battery.Index, tank.Index, SensorChannel.Thermal);
                w.FitThermalBlanket(tank);
                Fix after = w.DetectionRangeFor(battery.Index, tank.Index, SensorChannel.Thermal);

                Assert.True(before.Raw > 0, "sanity: there was something to cut");
                Assert.True(after.Raw > 0, "masking, not invisibility");
                Assert.True(after < before, "a fitted blanket cuts the reach a heat sensor gets");
            });

            r.Run("an order fits the cage, and the same six drones stop working (AUDIT-UNWIRED F16)", delegate
            {
                // The two tests above prove the cage works and prove nothing
                // about whether anybody can have one: until CommandKind.FitCage
                // existed, World.FitCage's only caller was this file. This runs
                // the same tank through a real attack twice, identical but for
                // one queued order, and never touches CageDisruptionChance.
                //
                // 55%: ground-force.md §2.1 gives a 0.30-0.80 spread and no
                // single figure, so the midpoint is a designer estimate.
                //
                // Six drones against one tank, and the order is worth exactly
                // the difference between the rush working and not working -
                // which is the shape World.cs already claims for it: "the only
                // thing that lets a tank survive a drone swarm long enough to
                // matter."
                Assert.False(TankSurvivesADroneRush(4500, 0), "control: six drones kill a bare tank");
                Assert.True(TankSurvivesADroneRush(4500, 55), "one queued order and the same six do not");
            });

            r.Run("an order fits the blanket, and the wrong team's order does not", delegate
            {
                World w = MakeWorld(4600);
                EntityHandle battery = w.Spawn(Catalog.IdOf("Interceptor Battery"), 1, P(12000, 12000));
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12500, 12000));
                Fix before = w.DetectionRangeFor(battery.Index, tank.Index, SensorChannel.Thermal);

                // Team 1 would love to blanket team 2's tank into being a less
                // convincing target for team 1's own munitions. An upgrade is
                // fitted to something you own, and CommandBuffer.Apply checks.
                w.Enqueue(Command.FitThermalBlanket(1, tank));
                w.Step();
                Assert.Equal(before.RoundToInt(),
                             w.DetectionRangeFor(battery.Index, tank.Index, SensorChannel.Thermal).RoundToInt(),
                             "an order from the wrong team does nothing at all");

                w.Enqueue(Command.FitThermalBlanket(2, tank));
                w.Step();
                Assert.True(w.DetectionRangeFor(battery.Index, tank.Index, SensorChannel.Thermal) < before,
                            "its owner's order fits it");
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
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
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
                EntityHandle pile = w.SpawnSalvage(P(12000, 12000), Fix.FromInt(1000));
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
            EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
            Fix damage = Catalog.Get(Catalog.IdOf("FPV Team")).WeaponDamage;
            for (int i = 0; i < droneHits; i++)
                w.ApplyDamage(tank, damage, DamageType.Shaped, true, EntityHandle.None);
            return w.Entities.Hp[tank.Index].Raw > 0;
        }

        /// <summary>
        /// Six one-way drones against one tank, fought out through World.Step,
        /// and whether the tank is still standing at the end. cagePercent of
        /// zero queues no order at all, so the two runs differ by exactly one
        /// command and nothing else.
        /// </summary>
        static bool TankSurvivesADroneRush(ulong seed, int cagePercent)
        {
            World w = MakeWorld(seed);
            EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12000, 12000));
            // Low-flying rotary drones, because a cage covers the roof and
            // CombatSystem only calls a hit top-attack when the shooter is in
            // the low layer - which is the same reason the research says a cage
            // is built for that arc and no other.
            for (int d = 0; d < 6; d++)
                w.Spawn(Catalog.IdOf("FPV Team"), 1, P(12060 + d * 4, 12000));
            if (cagePercent > 0) w.Enqueue(Command.FitCage(2, tank, cagePercent));

            for (int t = 0; t < SimConstants.Seconds(60); t++)
            {
                w.Step();
                if (!w.Entities.IsAlive(tank) || w.Entities.Hp[tank.Index].Raw <= 0) return false;
            }
            return true;
        }

        static void RegisterPointDefence(TestRunner r)
        {
            r.Group("point defence");

            r.Run("a mount facing two targets at once only ever damages one of them", delegate
            {
                // point-defence.md §Q3: "Guns: no. Strictly one at a time. One
                // barrel, one line of sight, one firing solution" - and the
                // "Suggested replacement units" table gives the Gun Mount
                // exactly one engagement channel. Before WeaponState.
                // CommittedTarget existed, the mount re-ran BestTargetInRange
                // every time it came off cooldown, so a much-better-scoring
                // arrival could pull its very next shot away from whatever it
                // was already shooting at - which is not a serial weapon, and
                // it is why the reflector decoy measured as having zero
                // effect (FINDINGS #25).
                //
                // Both targets sit on the ground, which makes the test fully
                // deterministic: ResolveDirectFire only rolls dice against an
                // airborne target, so a ground target's damage is exact.
                World w = MakeWorld(900);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));

                // Heavy armour: fragmentation does 0.20x, so this is a poor
                // shot (a low fraction of its health) and stays that way for
                // a long time - the mount has every incentive, on a fresh
                // rescore, to abandon it for something better.
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12360, 12000));
                Fix tankStart = w.Entities.Hp[tank.Index];

                int firstHitTick = -1;
                for (int t = 0; t < 200 && firstHitTick < 0; t++)
                {
                    w.Step();
                    if (w.Entities.Hp[tank.Index] < tankStart) firstHitTick = t;
                }
                Assert.True(firstHitTick >= 0, "the mount should have committed to the tank and hit it");

                // Soft armour: fragmentation does 1.60x. Arriving now, this is
                // by far the better shot - a much higher-value target by
                // exactly the measure BestTargetInRange uses - but it is not
                // a one-shot kill, so it is not the one case that is allowed
                // to break a live commitment.
                EntityHandle soft = w.Spawn(Catalog.IdOf("Motorcycle Squad"), 2, P(12360, 12000));
                Fix softStart = w.Entities.Hp[soft.Index];
                Fix tankAfterFirstHit = w.Entities.Hp[tank.Index];

                for (int t = 0; t < 250; t++) w.Step();

                Assert.Equal(softStart.Raw, w.Entities.Hp[soft.Index].Raw,
                             "a higher-scoring arrival that is not a kill shot should not pull the mount off its target");
                Assert.True(w.Entities.Hp[tank.Index] < tankAfterFirstHit,
                            "the mount keeps spending its magazine on the tank it already committed to");
            });

            r.Run("a target the mount would kill outright is worth breaking commitment for", delegate
            {
                // The one exception CombatSystem.CommittedOrBestTarget allows,
                // and the cost it pays for it: re-laying through the normal
                // Acquiring/SlewTicks path, not a second penalty on top.
                World w = MakeWorld(901);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle tank = w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12360, 12000));
                Fix tankStart = w.Entities.Hp[tank.Index];

                int firstHitTick = -1;
                for (int t = 0; t < 200 && firstHitTick < 0; t++)
                {
                    w.Step();
                    if (w.Entities.Hp[tank.Index] < tankStart) firstHitTick = t;
                }
                Assert.True(firstHitTick >= 0, "the mount should have committed to the tank and hit it");

                // 90 hit points of soft armour: this mount's 70-damage,
                // fragmentation round at 1.60x is 112, so one connecting shot
                // removes all of it. ShotValue reports that as a fraction
                // capped at one - a shot the tank, at 0.20x, can never match.
                EntityHandle weak = w.Spawn(Catalog.IdOf("Net Engineer"), 2, P(12360, 12000));
                w.Entities.Hp[weak.Index] = Fix.FromInt(90);

                for (int t = 0; t < 100; t++) w.Step();

                Assert.True(!w.Entities.IsAlive(weak),
                            "a guaranteed kill is the one thing allowed to pull the mount off its current target");
            });

            r.Run("the autocannon reaches the high band the gun mount cannot", delegate
            {
                // point-defence.md §Q2: "Above ~1,000-1,200 m: machine-gun-
                // class point defence is finished" (Gun Mount: CanReachHigh =
                // false) against the Autocannon Mount's own §"Suggested
                // replacement units" ceiling of "mid band" (CanReachHigh =
                // true). Both mounts can see this target fine - it is 720
                // metres out and the Gun Mount's own optical reach is 7,200 -
                // the difference the sensor buys is whether the mount is ever
                // mechanically allowed to fire, which EngagementsRemaining
                // reports without needing a hit to land (that part still
                // rolls dice - see AirHitChance - so it is not what this test
                // is about).
                World w = MakeWorld(902);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                EntityHandle auto = w.Spawn(Catalog.IdOf("Autocannon Mount"), 1, P(24000, 12000));

                EntityHandle gunTarget = w.Spawn(Catalog.IdOf("Multirole Quad"), 2, P(12720, 12000));
                EntityHandle autoTarget = w.Spawn(Catalog.IdOf("Multirole Quad"), 2, P(24720, 12000));
                w.Entities.EntityLayer[gunTarget.Index] = Layer.High;
                w.Entities.EntityLayer[autoTarget.Index] = Layer.High;

                int gunBelt = Catalog.Get(Catalog.IdOf("Gun Mount")).EngagementsPerBelt;
                int autoBelt = Catalog.Get(Catalog.IdOf("Autocannon Mount")).EngagementsPerBelt;

                for (int t = 0; t < 150; t++) w.Step();

                Assert.Equal(gunBelt, w.Entities.Weapon[gun.Index].EngagementsRemaining,
                             "CanReachHigh = false means the gun mount never gets a shot at a High target");

                bool autoFired = w.Entities.Weapon[auto.Index].EngagementsRemaining < autoBelt
                                  || w.Entities.Weapon[auto.Index].ReloadingUntilTick > 0;
                Assert.True(autoFired, "CanReachHigh = true means the autocannon actually takes the shot");
            });

            r.Run("consecutive bursts at one target are a cooldown apart, not a fresh acquisition", delegate
            {
                // FINDINGS #31 traced a Gun Mount getting exactly one shot at an
                // incoming FPV and never a second. The trace's last two lines are
                // the reason: the mount came off its cooldown with the drone at
                // five metres, still detected, still committed and still dead
                // ahead, and started a sixteen-tick acquisition clock over
                // instead of firing. CombatSystem cleared WeaponState.Acquiring
                // on every shot, so the acquisition-and-slew the code charges for
                // "switching to a new target" was charged again for every burst
                // at a target the barrel had never left - while
                // EngagementsRemaining's own doc comment prices a belt in targets
                // prosecuted and calls the re-lay part of the cycle, not an extra
                // on top of it.
                //
                // A stationary ground target makes the arithmetic exact:
                // ResolveDirectFire only rolls dice against something airborne,
                // and a target that does not move adds no slew, so the gap
                // between two bursts is the cooldown and nothing else. Before the
                // fix it was the cooldown plus WeaponAcquisitionTicks.
                World w = MakeWorld(903);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                w.Spawn(Catalog.IdOf("Main Tank"), 2, P(12360, 12000));

                UnitDef def = Catalog.Get(Catalog.IdOf("Gun Mount"));
                int belt = def.EngagementsPerBelt;
                int firstBurst = -1, secondBurst = -1;
                for (int t = 0; t < 400 && secondBurst < 0; t++)
                {
                    w.Step();
                    int left = w.Entities.Weapon[gun.Index].EngagementsRemaining;
                    if (left >= belt) continue;
                    belt = left;
                    if (firstBurst < 0) firstBurst = w.Tick; else secondBurst = w.Tick;
                }

                Assert.True(secondBurst > 0, "the mount should get a second burst at a target it never left");
                Assert.Equal(def.WeaponCooldownTicks, secondBurst - firstBurst,
                             "a mount already laid on its target pays the cooldown and nothing else");
            });

            r.Run("a gun mount gets a second burst at a closing drone", delegate
            {
                // The FINDINGS #31 engagement itself, end to end on the
                // production path rather than as tick arithmetic: spawn the
                // mount, fly real FPVs at it, and count what the belt spends.
                //
                // The window is genuinely short and most of it is not the
                // weapon's fault - the mount first holds the track at about
                // seventy metres because a 120-degree head sweeping at seventy
                // degrees a second is pointed elsewhere two thirds of the time
                // (FINDINGS #22), and the lay costs two seconds more. What is
                // the weapon's fault is spending the remainder re-acquiring. On
                // sixty seeds of the balance harness's own assault, the mount
                // took exactly 1.00 shots per attempt at every drone count
                // before this fix and never once got a second.
                World w = MakeWorld(904);
                EntityHandle gun = w.Spawn(Catalog.IdOf("Gun Mount"), 1, P(12000, 12000));
                for (int d = 0; d < 3; d++)
                {
                    EntityHandle drone = w.Spawn(Catalog.IdOf("FPV Team"), 2, P(13800, 11832 + d * 168));
                    w.Enqueue(Command.Attack(2, drone, gun));
                }

                UnitDef def = Catalog.Get(Catalog.IdOf("Gun Mount"));
                int belt = def.EngagementsPerBelt;
                int lastTick = -1;
                EntityHandle lastTarget = EntityHandle.None;
                bool sawSecondBurst = false;

                for (int t = 0; t < 600 && w.Entities.IsAlive(gun) && !sawSecondBurst; t++)
                {
                    w.Step();
                    WeaponState ws = w.Entities.Weapon[gun.Index];
                    if (ws.EngagementsRemaining >= belt) continue;
                    belt = ws.EngagementsRemaining;

                    // Two bursts at the same drone, a cooldown apart. Against a
                    // target that is closing at twenty-two metres a second this
                    // is only possible if the mount held its lay: re-acquiring
                    // would put the second burst at least an acquisition later,
                    // and by then the drone has arrived. Bursts at a *different*
                    // drone are not evidence either way - re-laying onto one of
                    // those is what the acquisition cost is for.
                    if (lastTick >= 0 && ws.CommittedTarget == lastTarget)
                    {
                        Assert.Equal(def.WeaponCooldownTicks, w.Tick - lastTick,
                                     "a second burst at the same closing drone is a cooldown after the first");
                        sawSecondBurst = true;
                    }
                    lastTick = w.Tick;
                    lastTarget = ws.CommittedTarget;
                }

                Assert.True(sawSecondBurst,
                            "the mount should spend more than one engagement on a drone it has already laid on");
            });

            r.Run("an interceptor is vectored ahead of a crossing target, not at it", delegate
            {
                // The whole of the interception path existed - IsInterceptor,
                // InterceptBaseChance, ResolveInterception, CueMultiplier,
                // SpeedRatio - and every bit of it describes the moment the
                // interceptor is already there. Nothing ever got it there: an
                // interceptor flew the same order every other airframe flies, at
                // the target's position, this tick, every tick.
                //
                // Everything here goes through the production path. The launch is a
                // queued LaunchSortie, the target is a queued LaunchSortie, and the
                // only thing asserted on is where the interceptor's nose ends up
                // pointing after the world has stepped.
                World w = MakeWorld(7710);
                w.Player(1).Materiel = Fix.FromInt(100000);
                w.Player(2).Materiel = Fix.FromInt(100000);
                w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(12000, 12000));

                // A fast target crossing left to right well clear of the pad, so
                // that pointing at it and meeting it are different directions.
                EntityHandle waypoint = w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(4000, 8000));
                w.Enqueue(Command.LaunchSortie(2, Catalog.IdOf("Jet Strike Drone"),
                                               P(16000, 8000), waypoint, 0));
                for (int n = 0; n < 40; n++) w.Step();

                EntityHandle threat = FirstOfTeam(w, 2, "Jet Strike Drone");
                Assert.True(w.Entities.IsAlive(threat), "sanity: the target is airborne");

                w.Enqueue(Command.LaunchSortie(1, Catalog.IdOf("Interceptor FPV"),
                                               P(12000, 12000), threat, 0));
                for (int n = 0; n < 40; n++) w.Step();

                EntityHandle chaser = FirstOfTeam(w, 1, "Interceptor FPV");
                Assert.True(w.Entities.IsAlive(chaser), "sanity: the interceptor is airborne");
                Assert.True(w.Entities.IsAlive(threat), "sanity: it has not caught it yet");

                // Where it is flying against where the target currently is. A pure
                // pursuer has these two identical to within its turn rate; a vector
                // onto a meeting point is measurably ahead of the target.
                Fix2 toNow = w.Entities.Position[threat.Index] - w.Entities.Position[chaser.Index];
                ushort bearingToTarget = Trig.Atan2(toNow.Y, toNow.X);
                int lead = Trig.Delta(bearingToTarget, w.Entities.Yaw[chaser.Index]);
                if (lead < 0) lead = -lead;
                int leadDegrees = (int)((long)lead * 360 / 65536);

                Assert.True(leadDegrees > 5,
                            "the interceptor's nose is off the target's current bearing: it is leading");

                // And leading the right way - toward where the target is going,
                // rather than merely mis-pointed.
                Fix2 ahead = (w.Entities.Position[threat.Index] + w.Entities.Velocity[threat.Index])
                           - w.Entities.Position[chaser.Index];
                int toAhead = Trig.Delta(Trig.Atan2(ahead.Y, ahead.X), w.Entities.Yaw[chaser.Index]);
                if (toAhead < 0) toAhead = -toAhead;
                Assert.True(toAhead < lead, "and the lead is on the side the target is travelling toward");
            });

            r.Run("a radar track buys more lead than an eyeball does", delegate
            {
                // The half of this that the research is sharpest about.
                // point-defence.md's effector table splits its sensors on exactly
                // this line - the machine-gun turret is "passive EO/IR only... no
                // velocity measurement" and reads closing rate off image scale, the
                // autocannon has "organic AESA search/track + EO/IR; measured
                // velocity" - and the same document says a firing solution is most
                // sensitive to that measurement. So the radar does not buy a bigger
                // number on the roll. It buys a solution.
                //
                // Two identical worlds, identical orders, one difference: what is
                // holding the target. Everything goes through Spawn, Enqueue and
                // Step, and what is read back is where the interceptor's nose ends
                // up pointing.
                int radarLead = LeadHeldBy(true);
                int opticalLead = LeadHeldBy(false);

                Assert.True(opticalLead > 0, "an eyeball track still produces some lead");
                Assert.True(radarLead > opticalLead,
                            "and a radar track produces more of it: the solution is measured, not inferred");
            });

            r.Run("a one-way munition that arrives at an empty aimpoint is expended", delegate
            {
                // An attack order whose target dies used to be unfinishable. The
                // destination fell back to where the target had been, the airframe
                // flew there, and the arrival test refused to clear an order that
                // still named a target - so it hung over the spot for the rest of
                // the match holding one of the crews the whole game is rationed by.
                //
                // Production path throughout: a queued launch, a queued attack, and
                // World.Step. Nothing here touches a component by hand.
                World w = MakeWorld(7712);
                w.Player(1).Materiel = Fix.FromInt(100000);
                int readyBefore = w.Player(1).Crews.ReadyCount;

                EntityHandle prey = w.Spawn(Catalog.IdOf("Relay Mast"), 2, P(9000, 12000));
                w.Enqueue(Command.LaunchSortie(1, Catalog.IdOf("FPV Team"), P(12000, 12000), prey, 0));
                for (int n = 0; n < 40; n++) w.Step();

                EntityHandle munition = FirstOfTeam(w, 1, "FPV Team");
                Assert.True(w.Entities.IsAlive(munition), "sanity: the munition is airborne");
                Assert.True(w.Player(1).Crews.ReadyCount < readyBefore, "sanity: it took a crew with it");

                // The target is removed from under it - exactly what happens when
                // somebody else's drone gets there first.
                w.Kill(prey, EntityHandle.None);
                for (int n = 0; n < 32 * 40; n++)
                {
                    w.Step();
                    if (!w.Entities.IsAlive(munition)) break;
                }

                Assert.False(w.Entities.IsAlive(munition),
                             "it goes into the ground at the aimpoint rather than hovering over it forever");
                Assert.Equal(readyBefore, w.Player(1).Crews.ReadyCount,
                             "and the crew comes back");
            });
        }

        /// <summary>
        /// How far ahead of a crossing target an interceptor is aiming, in degrees
        /// off the bearing to where the target actually is, with the target held on
        /// radar or only optically. One world, one order set, one difference.
        /// </summary>
        static int LeadHeldBy(bool radar)
        {
            World w = MakeWorld(7713);
            w.Player(1).Materiel = Fix.FromInt(100000);
            w.Player(2).Materiel = Fix.FromInt(100000);
            // A Radar Mast measures velocity; a Designator Team's optics reach the
            // same target and cannot, so the contact exists either way and the only
            // difference between the two worlds is the kind of track behind it.
            w.Spawn(Catalog.IdOf(radar ? "Radar Mast" : "Designator Team"), 1, P(11000, 11000));

            EntityHandle waypoint = w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(4000, 8000));
            w.Enqueue(Command.LaunchSortie(2, Catalog.IdOf("Jet Strike Drone"),
                                           P(16000, 8000), waypoint, 0));
            for (int n = 0; n < 40; n++) w.Step();

            EntityHandle threat = FirstOfTeam(w, 2, "Jet Strike Drone");
            w.Enqueue(Command.LaunchSortie(1, Catalog.IdOf("Interceptor FPV"),
                                           P(11000, 11000), threat, 0));
            for (int n = 0; n < 40; n++) w.Step();

            EntityHandle chaser = FirstOfTeam(w, 1, "Interceptor FPV");
            if (!w.Entities.IsAlive(chaser) || !w.Entities.IsAlive(threat)) return 0;

            Fix2 toNow = w.Entities.Position[threat.Index] - w.Entities.Position[chaser.Index];
            int lead = Trig.Delta(Trig.Atan2(toNow.Y, toNow.X), w.Entities.Yaw[chaser.Index]);
            if (lead < 0) lead = -lead;
            return (int)((long)lead * 360 / 65536);
        }

        /// <summary>The first live entity of one team and one catalogue name.</summary>
        static EntityHandle FirstOfTeam(World w, byte team, string defName)
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

        // ------------------------------------------------------------------

        static void RegisterEconomy(TestRunner r)
        {
            r.Group("economy");

            r.Run("a Recovery UGV collects what a fight leaves behind (AUDIT-UNWIRED F3)", delegate
            {
                // Before this, SalvageState decayed to nothing and
                // SimEventKind.SalvageCollected was never pushed - Materiel
                // only ever went down from its starting balance. Salvage is
                // already spawned by World.Kill and already decaying every
                // tick; this only has to prove the other end exists.
                World w = MakeWorld(90);
                Fix2 spot = P(12000, 12000);
                EntityHandle victim = w.Spawn(Catalog.IdOf("Main Tank"), 2, spot);
                w.Kill(victim, EntityHandle.None);
                w.Spawn(Catalog.IdOf("Recovery UGV"), 1, spot); // parked right on the wreck

                Fix before = w.Player(1).Materiel;
                w.Step();

                Assert.True(w.Player(1).Materiel > before,
                            "collecting a pile raises the collector's own side's Materiel");
                Assert.True(w.Events.CountOf(SimEventKind.SalvageCollected) > 0,
                            "and the event nothing used to push actually fires");

                bool pileRemains = false;
                for (int i = 1; i < w.Entities.HighWater; i++)
                    if (w.Entities.IsSlotAlive(i) && w.Entities.Has(i, ComponentMask.Salvage))
                        pileRemains = true;
                Assert.False(pileRemains, "the claimed pile is gone outright, not left to keep decaying");
            });

            r.Run("a pile too far from any Recovery UGV just decays, as before", delegate
            {
                // The negative case: collection is a proximity check, not an
                // automatic payout the instant something dies. Regresses to
                // the pre-existing decay behaviour when nothing is close
                // enough to claim it - see SalvageCollectionRadiusMetres.
                World w = MakeWorld(91);
                Fix2 spot = P(12000, 12000);
                EntityHandle victim = w.Spawn(Catalog.IdOf("Main Tank"), 2, spot);
                w.Kill(victim, EntityHandle.None);
                w.Spawn(Catalog.IdOf("Recovery UGV"), 1, P(20000, 12000)); // far away

                Fix before = w.Player(1).Materiel;
                for (int i = 0; i < SimConstants.SalvageDecayTicks + 8; i++) w.Step();

                Assert.Equal(before.RoundToInt(), w.Player(1).Materiel.RoundToInt(),
                             "nobody was close enough to collect it, so it just rotted away");
            });

            r.Run("money gates production, crews gate employment - two valves, not one (FINDINGS 24)", delegate
            {
                // "Airframes are cheap, crews are the cap" is most of the
                // picture and misses the first valve: a bank with nothing in
                // it should stop a launch exactly as completely as an empty
                // crew roster does, and for a different reason - the sidebar
                // card should grey out for two different reasons, not one.
                // Salvage collection above is what makes Materiel a real,
                // two-way resource instead of a countdown from 4000; this is
                // that resource actually gating something, and recovering
                // from a gate closing the moment income arrives.
                World w = MakeWorld(92);
                w.Player(1).Materiel = Fix.FromInt(50); // less than any airframe in the catalogue

                EntityHandle refused;
                LaunchResult moneyGate = SortieSystem.Launch(w, 1, Catalog.IdOf("Scout Quad"), P(6000, 6000),
                                                             EntityHandle.None, 0, out refused);
                Assert.Equal((long)LaunchResult.InsufficientMateriel, (long)moneyGate,
                             "refused for want of money");
                Assert.Equal(SimConstants.StartingCrews, w.Player(1).Crews.ReadyCount,
                             "the crew gate was never even reached - money closed first");

                Fix2 spot = P(6000, 6000);
                EntityHandle victim = w.Spawn(Catalog.IdOf("Main Tank"), 2, spot);
                w.Kill(victim, EntityHandle.None);
                w.Spawn(Catalog.IdOf("Recovery UGV"), 1, spot);
                w.Step();
                Assert.True(w.Player(1).Materiel > Fix.FromInt(120), "collection actually paid out");

                EntityHandle launched;
                LaunchResult afterIncome = SortieSystem.Launch(w, 1, Catalog.IdOf("Scout Quad"), P(6000, 6000),
                                                               EntityHandle.None, 1, out launched);
                Assert.Equal((long)LaunchResult.Launched, (long)afterIncome,
                             "the same crew, idle a moment ago, can fly the instant the money gate opens");
            });

            r.Run("an Uplink Terminal grants the satellite rung its capacity (AUDIT-UNWIRED F15)", delegate
            {
                // PlayerState.UplinkCapacity started at zero and nothing ever
                // incremented it, so Designator Team - the only carrier of
                // LinkKind.Satellite - could not launch in any real match.
                // The satellite rung of the link ladder was unreachable code,
                // not merely untested.
                World w = MakeWorld(93);
                w.Player(1).Materiel = Fix.FromInt(100000);

                EntityHandle refused;
                LaunchResult before = SortieSystem.Launch(w, 1, Catalog.IdOf("Designator Team"), P(6000, 6000),
                                                          EntityHandle.None, 0, out refused);
                Assert.Equal((long)LaunchResult.NoUplinkCapacity, (long)before,
                             "unlaunchable with no terminal built");

                w.Spawn(Catalog.IdOf("Uplink Terminal"), 1, P(6100, 6000));

                EntityHandle launched;
                LaunchResult after = SortieSystem.Launch(w, 1, Catalog.IdOf("Designator Team"), P(6000, 6000),
                                                         EntityHandle.None, 1, out launched);
                Assert.Equal((long)LaunchResult.Launched, (long)after,
                             "and launchable the instant one is built");
            });
        }
    }
}
