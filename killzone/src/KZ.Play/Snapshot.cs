// KILL ZONE - a real-time strategy video game.
// One team's view of the world, as JSON.
//
// This file is where fog of war lives, and it is the only place it can honestly
// live: the simulation knows everything, the browser must be told only what one
// side's sensors reach, and the gap between those two is the subject of the
// whole research corpus. So nothing here reads the entity table and filters
// afterwards - every enemy entity is offered to World.IsDetectedBy first and
// simply does not appear in the JSON if the answer is no. A contact the player
// cannot see is not a hidden field in the payload. It is absent.
//
// It writes positions in real metres. There is no scale factor, no pixel, and
// no viewport here: docs/SCALE.md's correction is that compression is a view
// parameter, so the transform from metres to pixels belongs entirely to the
// browser and this file must not know it exists.

using KZ.Sim;

namespace KZ.Play
{
    public static class Snapshot
    {
        /// <summary>
        /// Everything that never changes: terrain, map size, where the border
        /// runs, and the catalogue rows behind the hangar bar. Fetched once.
        /// </summary>
        public static string Static(World w)
        {
            JsonWriter j = new JsonWriter();
            j.BeginObject();
            j.Field("widthMetres", w.Terrain.WidthMetres);
            j.Field("heightMetres", w.Terrain.HeightMetres);
            j.Field("tileMetres", SimConstants.BuildTileMetres);
            j.Field("tilesX", w.Terrain.WidthTiles);
            j.Field("tilesY", w.Terrain.HeightTiles);
            j.Field("borderMetres", Scenario.BorderMetres);
            j.Field("ticksPerSecond", SimConstants.TicksPerSecond);
            j.Field("timeMultiplier", SimConstants.TimeMultiplier);
            j.Field("team", Scenario.PlayerTeam);

            // The scenario's opening camera, in metres. The renderer fits this
            // rectangle to whatever viewport it has and derives its zoom from
            // that, so a phone and a desktop open on the same ground at
            // different scales rather than on different amounts of the map.
            // Stated by the scenario because framing is a property of the
            // situation, not of the device - docs/SCALE.md's correction.
            j.Object("view");
            j.Field("cx", 15000);
            j.Field("cy", 9600);
            j.Field("widthMetres", 26000);
            j.EndObject();

            // Where sorties leave from unless the player moves it. Forward of the
            // relay mast, which is where a fiber thread is short enough to
            // survive the flight - "where you launch from is a real decision".
            j.Object("pad");
            j.Field("x", 10320);
            // In the clean lane between the northern treeline and the road. A
            // fiber thread dragged over either of those parts, so where the pad
            // sits is the first decision the scenario makes for the player and
            // the first one they should take back.
            j.Field("y", 9000);
            j.EndObject();

            // One digit per build tile, one string per row. 300 x 200 of them is
            // sixty kilobytes fetched once, which is cheaper than any encoding
            // clever enough to need a decoder on the other side.
            j.Array("tiles");
            for (int ty = 0; ty < w.Terrain.HeightTiles; ty++)
            {
                System.Text.StringBuilder row = new System.Text.StringBuilder(w.Terrain.WidthTiles);
                for (int tx = 0; tx < w.Terrain.WidthTiles; tx++)
                    row.Append((char)('0' + (int)w.Terrain.At(tx, ty)));
                j.Value(row.ToString());
            }
            j.EndArray();

            j.Array("hangar");
            for (int i = 0; i < Scenario.Hangar.Length; i++)
            {
                UnitDef d = Catalog.Get(Catalog.IdOf(Scenario.Hangar[i]));
                j.BeginObject();
                j.Field("defId", d.Id);
                j.Field("name", d.Name);
                j.Field("cost", d.CostMateriel);
                j.Field("link", d.Link.ToString());
                j.Field("crew", d.ConsumesCrew);
                j.Field("oneWay", d.OneWay);
                j.Field("speed", d.SpeedMetresPerSecond.RoundToInt());
                j.Field("nightOnly", d.NightOnly);
                j.EndObject();
            }
            j.EndArray();
            j.EndObject();
            return j.ToString();
        }

        public static string View(World w, byte team, MatchLoop loop, int sinceSeq)
        {
            JsonWriter j = new JsonWriter();
            j.BeginObject();

            j.Field("tick", w.Tick);
            // Play seconds, not world seconds. The world's clock runs
            // TimeMultiplier times faster and is not what anyone is watching.
            j.Field("playSeconds", w.Tick / SimConstants.TicksPerSecond);
            j.Field("paused", loop.Paused);
            j.Field("speed", loop.Speed);
            j.Field("phase", w.Phase.ToString());
            j.Field("weather", w.Weather.ToString());
            j.Field("ground", w.Ground.ToString());
            j.Field("stateHash", "0x" + w.StateHash().ToString("X16"));

            PlayerState p = w.Player(team);
            j.Object("you");
            j.Field("team", team);
            j.Field("faction", p.Faction.ToString());
            j.Field("materiel", p.Materiel.RoundToInt());
            j.Field("taskingPoints", p.TaskingPoints.RoundToInt());
            j.Field("crewsReady", p.Crews.ReadyCount);
            j.Field("crewsFlying", p.Crews.FlyingCount);
            j.Field("crewsTotal", p.Crews.Count);
            j.Field("uplinkInUse", p.UplinkInUse);
            j.Field("uplinkCapacity", p.UplinkCapacity);
            j.EndObject();

            // ---- what you own -------------------------------------------------
            j.Array("units");
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (w.Entities.Team[i] != team) continue;
                WriteOwn(j, w, i);
            }
            j.EndArray();

            // ---- what your sensors report -------------------------------------
            j.Array("contacts");
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (w.Entities.Team[i] == team) continue;
                EntityHandle h = w.Entities.HandleAt(i);
                if (!w.IsDetectedBy(team, h)) continue;
                WriteContact(j, w, team, i);
            }
            j.EndArray();

            // ---- fiber threads you are flying ---------------------------------
            //
            // Your own only. A thread is a physical object an enemy can stumble
            // onto (SimEventKind.TetherFound) but not something your map draws for
            // them, and drawing the enemy's would give away an operator's position
            // for free.
            j.Array("tethers");
            for (int id = 0; id < w.Tethers.Capacity; id++)
            {
                TetherSystem.Tether t = w.Tethers.Get(id);
                if (t.State != TetherState.Live && t.State != TetherState.Taut) continue;
                if (t.Team != team) continue;
                j.BeginObject();
                j.Field("spooled", t.Spooled.RoundToInt());
                j.Field("spoolMax", t.SpoolMax.RoundToInt());
                j.Array("pts");
                for (int n = 0; n < t.NodeCount; n++)
                {
                    j.Value(t.Nodes[n].Position.X.RoundToInt());
                    j.Value(t.Nodes[n].Position.Y.RoundToInt());
                }
                j.EndArray();
                j.EndObject();
            }
            j.EndArray();

            // ---- jamming you can see -------------------------------------------
            //
            // A jammer's bubble is drawn only when the jammer itself is a contact.
            // "A jammer is the easiest thing on the map to find while it is
            // switched on" is a lesson the player has to be allowed to learn, and
            // it is not a lesson if the dome is on the map before the ESM is.
            j.Array("jamming");
            for (int e = 0; e < w.Signal.EmitterCount; e++)
            {
                JamEmitter em = w.Signal.GetEmitter(e);
                if (em.Team != team && !w.IsDetectedBy(team, em.Source)) continue;
                j.BeginObject();
                j.Field("x", em.Position.X.RoundToInt());
                j.Field("y", em.Position.Y.RoundToInt());
                j.Field("radius", em.RadiusMetres.RoundToInt());
                j.Field("strength", em.Strength);
                j.Field("mine", em.Team == team);
                j.EndObject();
            }
            j.EndArray();

            j.Array("events");
            loop.WriteEventsSince(j, sinceSeq);
            j.EndArray();
            j.Field("eventSeq", loop.EventSeq);

            j.Array("objectives");
            for (int i = 0; i < loop.Objectives.Length; i++)
            {
                MatchLoop.Objective o = loop.Objectives[i];
                j.BeginObject();
                j.Field("name", o.Name);
                j.Field("alive", w.Entities.IsAlive(o.Handle));
                j.EndObject();
            }
            j.EndArray();

            // What is driving at the command post. The losing condition in this
            // scenario is a ground advance, and FINDINGS 35's complaint about the
            // defence was that it "never threatens" rather than that it never
            // damages - so the one thing the interface owes the player is a line
            // that says which vehicle, how far, and whether the thing that stops
            // it is still standing. MatchLoop.ThreatReport only ever reports a
            // contact, so this leaks nothing the map is not already drawing.
            MatchLoop.ThreatReport threat = loop.Threat;
            j.Object("threat");
            j.Field("any", threat.Any);
            if (threat.Any)
            {
                j.Field("name", threat.Name);
                j.Field("metres", threat.Metres);
                j.Field("jammed", threat.Jammed);
            }
            j.EndObject();

            j.Field("outcome", loop.Outcome);

            j.EndObject();
            return j.ToString();
        }

        static void WriteOwn(JsonWriter j, World w, int i)
        {
            EntityTable e = w.Entities;
            UnitDef def = Catalog.Get(e.DefId[i]);

            j.BeginObject();
            j.Field("h", (long)e.HandleAt(i).Value);
            j.Field("name", def.Name);
            j.Field("x", e.Position[i].X.RoundToInt());
            j.Field("y", e.Position[i].Y.RoundToInt());
            j.Field("hp", e.Hp[i].RoundToInt());
            j.Field("hpMax", e.HpMax[i].RoundToInt());
            j.Field("layer", (int)e.EntityLayer[i]);
            j.Field("structure", e.Has(i, ComponentMask.Structure));
            j.Field("yaw", Degrees(e.Yaw[i]));

            // Whether this thing is transmitting, for the two structures that
            // can choose. The player needs to see the state to decide, and the
            // interface needs to know the order exists at all - a switch nothing
            // reports the position of is a switch nobody uses.
            if (e.Has(i, ComponentMask.Emitter))
                j.Field("emitting", e.Emitter[i].Active);

            if (e.Has(i, ComponentMask.Link))
            {
                LinkState l = e.Link[i];
                j.Object("link");
                j.Field("kind", l.Kind.ToString());
                j.Field("pip", l.Pip.ToString());
                j.Field("hops", l.Hops);
                j.Field("jam", l.JamSampled);
                // The control line the interface spec calls Link View: where the
                // orders are physically coming from. A drone whose parent is a
                // relay mast two kilometres behind it is a drone that dies when
                // the mast does, and that is not readable from a pip alone.
                if (e.IsAlive(l.Parent))
                {
                    j.Field("px", e.Position[l.Parent.Index].X.RoundToInt());
                    j.Field("py", e.Position[l.Parent.Index].Y.RoundToInt());
                }
                j.EndObject();
            }

            if (e.Has(i, ComponentMask.Sortie))
            {
                SortieState s = e.Sortie[i];
                j.Object("sortie");
                j.Field("phase", s.Phase.ToString());
                j.Field("crew", s.CrewId);
                j.Field("oneWay", s.OneWay);
                if (e.IsAlive(s.Target))
                {
                    j.Field("tx", e.Position[s.Target.Index].X.RoundToInt());
                    j.Field("ty", e.Position[s.Target.Index].Y.RoundToInt());
                }
                j.EndObject();
            }

            if (e.Has(i, ComponentMask.Sensor))
            {
                SensorSuite s = e.Sensor[i];
                j.Object("sensor");
                j.Field("optical", s.Optical.RoundToInt());
                j.Field("thermal", s.Thermal.RoundToInt());
                j.Field("acoustic", s.Acoustic.RoundToInt());
                j.Field("radar", s.Radar.RoundToInt());
                j.Field("esm", s.Esm.RoundToInt());
                // The arc and where it is currently pointed. A turret was mute for
                // fifty-five ticks in this project's history because its head was
                // facing the wrong way and nothing drew that anywhere.
                j.Field("arc", s.DirectionalArcDegrees);
                j.Field("facing", Degrees(s.Facing));
                j.EndObject();
            }

            if (e.Has(i, ComponentMask.Weapon))
            {
                WeaponState wp = e.Weapon[i];
                j.Object("weapon");
                j.Field("range", wp.RangeMetres.RoundToInt());
                j.Field("bearing", Degrees(wp.Bearing));
                j.Field("reloading", wp.ReloadingUntilTick > w.Tick);
                j.Field("rounds", wp.EngagementsRemaining);
                j.EndObject();
            }

            if (e.Has(i, ComponentMask.Mover))
            {
                MoverState m = e.Mover[i];
                j.Object("mover");
                j.Field("speed", def.SpeedMetresPerSecond.RoundToInt());
                j.Field("hasOrder", m.HasOrder);
                j.Field("ox", m.OrderPoint.X.RoundToInt());
                j.Field("oy", m.OrderPoint.Y.RoundToInt());
                j.EndObject();
            }

            if (e.Has(i, ComponentMask.Nav))
            {
                NavState n = e.Nav[i];
                j.Object("nav");
                j.Field("aid", n.Aid.ToString());
                j.Field("lock", n.HasLock);
                j.Field("error", n.ErrorMetres.RoundToInt());
                j.EndObject();
            }
            j.EndObject();
        }

        /// <summary>
        /// A contact, plus the single most useful thing the interface can say
        /// about one: which channel is holding it. That is read back out of
        /// World.DetectionRangeFor, which exists "for the interface to draw".
        ///
        /// One honest caveat, stated here rather than hidden: the optical and
        /// thermal channels also require the sensor head to be pointed at the
        /// target, and that test is inside the simulation where it belongs. So
        /// the channel named here is the best-reaching channel, not necessarily
        /// the one that actually resolved the track. Whether the contact exists
        /// at all is never a guess - that comes from IsDetectedBy.
        /// </summary>
        static void WriteContact(JsonWriter j, World w, byte team, int ti)
        {
            EntityTable e = w.Entities;
            UnitDef def = e.DefId[ti] >= 0 && e.DefId[ti] < Catalog.Count
                        ? Catalog.Get(e.DefId[ti]) : null;

            SensorChannel bestChannel = SensorChannel.Optical;
            int bestSensor = -1;
            Fix bestMargin = Fix.MinValue;

            for (int i = 1; i < e.HighWater; i++)
            {
                if (!e.IsSlotAlive(i)) continue;
                if (e.Team[i] != team) continue;
                if (!e.Has(i, ComponentMask.Sensor)) continue;
                Fix dist = Fix2.Distance(e.Position[i], e.Position[ti]);
                for (int ch = 0; ch < 5; ch++)
                {
                    Fix reach = w.DetectionRangeFor(i, ti, (SensorChannel)ch);
                    if (reach.Raw <= 0) continue;
                    Fix margin = reach - dist;
                    if (margin.Raw < 0) continue;
                    if (margin > bestMargin)
                    {
                        bestMargin = margin;
                        bestChannel = (SensorChannel)ch;
                        bestSensor = i;
                    }
                }
            }

            j.BeginObject();
            j.Field("h", (long)e.HandleAt(ti).Value);
            j.Field("name", def != null ? def.Name
                                       : (e.Has(ti, ComponentMask.Salvage) ? "salvage" : "contact"));
            j.Field("x", e.Position[ti].X.RoundToInt());
            j.Field("y", e.Position[ti].Y.RoundToInt());
            j.Field("hp", e.Hp[ti].RoundToInt());
            j.Field("hpMax", e.HpMax[ti].RoundToInt());
            j.Field("layer", (int)e.EntityLayer[ti]);
            j.Field("team", e.Team[ti]);
            j.Field("structure", e.Has(ti, ComponentMask.Structure));
            j.Field("decoy", e.Has(ti, ComponentMask.Decoy));
            j.Field("salvage", e.Has(ti, ComponentMask.Salvage));
            j.Field("channel", bestSensor >= 0 ? bestChannel.ToString() : "held");
            // What kind of track is behind the contact, which is a different
            // question from whether there is one and is the one that decides
            // whether an interceptor sent at this thing can be vectored onto a
            // meeting point or only pointed at it. The player has no other way to
            // find that out, and it is the difference between spending 300 on a
            // defence that works and 300 on a stern chase - so it goes on the
            // contact, beside the channel that found it.
            j.Field("track", w.TrackQualityOf(team, e.HandleAt(ti)).ToString());
            if (bestSensor >= 0)
            {
                j.Field("sx", e.Position[bestSensor].X.RoundToInt());
                j.Field("sy", e.Position[bestSensor].Y.RoundToInt());
            }
            j.EndObject();
        }

        /// <summary>BAM (0-65535 over a full turn) to whole degrees.</summary>
        static int Degrees(ushort bam) { return (int)((long)bam * 360 / 65536); }
    }
}
