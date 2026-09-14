// KILL ZONE - a real-time strategy video game.
// The per-tick heart of the game: does this drone still have a pilot?
//
// Four steps, every tick, for every drone. Is there a path back to a human at
// all? How hard is this spot being jammed? What does that make the link - green,
// amber, or black? And what happens to the drone as a result?
//
// Note what is deliberately absent. There is no signal-to-noise ratio, no
// separate control and video channels, no frequency band. One jamming number
// against one robustness number, producing one of three states. The simulation
// must not be able to represent a situation the interface cannot draw, because a
// player who cannot see why their drone died learns nothing from it.

namespace KZ.Sim
{
    public static class LinkResolver
    {
        public static void ResolveAll(World w)
        {
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (!w.Entities.Has(i, ComponentMask.Link)) continue;
                Resolve(w, i);
            }
        }

        static void Resolve(World w, int i)
        {
            LinkState link = w.Entities.Link[i];

            // Autonomy has no link to sever. It pays for that elsewhere, in the
            // classifier, by occasionally choosing the wrong thing to destroy.
            if (link.Kind == LinkKind.Autonomy || link.Kind == LinkKind.None)
            {
                link.Pip = LinkPip.Green;
                link.AmberTicks = 0;
                link.BlackTicks = 0;
                link.LastEvalTick = w.Tick;
                w.Entities.Link[i] = link;
                ApplyGreenEffects(w, i);
                return;
            }

            link.RobustnessEffective = ComputeRobustness(w, i, link);

            // A geofence is not a fade. Every other way of losing a link in this
            // game degrades - jamming climbs, a mesh parent dies and a child
            // spends a moment looking for another, a tether frays. Crossing out of
            // satellite coverage does none of that: one tick the drone is on a
            // satellite and the next it is on its own, with no warning and nothing
            // the pilot can do about it. So it skips the amber grace period
            // entirely and goes straight to black.
            if (link.Kind == LinkKind.Satellite && !InSatelliteCoverage(w, i))
            {
                if (!link.OutsideCoverage)
                {
                    link.OutsideCoverage = true;
                    w.Events.Push(SimEventKind.SatelliteCoverageLost, w.Tick,
                                  w.Entities.HandleAt(i), EntityHandle.None,
                                  w.Entities.Team[i], 0);
                }
                if (link.Pip != LinkPip.Black)
                {
                    link.Pip = LinkPip.Black;
                    link.BlackTicks = 0;
                    w.Events.Push(SimEventKind.LinkBlack, w.Tick, w.Entities.HandleAt(i));
                }
                link.AmberTicks = 0;
                link.LastEvalTick = w.Tick;
                w.Entities.Link[i] = link;
                ApplyBlackEffects(w, i);
                return;
            }
            if (link.OutsideCoverage)
            {
                link.OutsideCoverage = false;
                w.Events.Push(SimEventKind.SatelliteCoverageRegained, w.Tick,
                              w.Entities.HandleAt(i), EntityHandle.None,
                              w.Entities.Team[i], 0);
            }

            bool connected = HasControlPath(w, i, ref link);
            int jam = SampleJam(w, i, link);
            link.JamSampled = (byte)jam;

            bool jammed = !link.IsUnjammable && jam > link.RobustnessEffective;

            if (!connected || jammed)
            {
                link.AmberTicks++;
                if (link.Pip == LinkPip.Green)
                {
                    link.Pip = LinkPip.Amber;
                    w.Events.Push(SimEventKind.LinkAmber, w.Tick, w.Entities.HandleAt(i));
                }
                if (link.AmberTicks >= SimConstants.AmberToBlackTicks && link.Pip != LinkPip.Black)
                {
                    link.Pip = LinkPip.Black;
                    link.BlackTicks = 0;
                    w.Events.Push(SimEventKind.LinkBlack, w.Tick, w.Entities.HandleAt(i));
                }
            }
            else
            {
                if (link.Pip != LinkPip.Green)
                    w.Events.Push(SimEventKind.LinkRestored, w.Tick, w.Entities.HandleAt(i));
                link.Pip = LinkPip.Green;
                link.AmberTicks = 0;
                link.BlackTicks = 0;
            }

            link.LastEvalTick = w.Tick;
            w.Entities.Link[i] = link;

            switch (link.Pip)
            {
                case LinkPip.Green: ApplyGreenEffects(w, i); break;
                case LinkPip.Amber: ApplyAmberEffects(w, i); break;
                case LinkPip.Black: ApplyBlackEffects(w, i); break;
            }
        }

        static byte ComputeRobustness(World w, int i, LinkState link)
        {
            if (link.Kind == LinkKind.Fiber) return SimConstants.UnjammableRobustness;

            int r = link.RobustnessBase;

            // A mesh with somewhere else to route is genuinely harder to break than
            // a single thin chain.
            if (link.Kind == LinkKind.Mesh) r += MeshGraph.RedundancyBonus(link.AltHops);

            // An experienced crew pushes through interference that would lose a
            // rookie the aircraft.
            if (w.Entities.Has(i, ComponentMask.Sortie))
            {
                int crewId = w.Entities.Sortie[i].CrewId;
                byte team = w.Entities.Team[i];
                if (crewId >= 0 && team < w.Players.Length)
                    r += w.Players[team].Crews.LinkBonusFor(crewId);
            }

            if (r > 254) r = 254;
            if (r < 0) r = 0;
            return (byte)r;
        }

        /// <summary>
        /// Whether this drone is over ground its own side's satellite service is
        /// licensed for. Neutral ground counts as outside - a constellation does
        /// not light up a strip nobody has claimed.
        /// </summary>
        public static bool InSatelliteCoverage(World w, int i)
        {
            return w.Territory.OwnerAt(w.Entities.Position[i]) == w.Entities.Team[i];
        }

        static bool HasControlPath(World w, int i, ref LinkState link)
        {
            switch (link.Kind)
            {
                case LinkKind.Fiber:
                    return w.Tethers.IsConnected(w.Entities.TetherId[i]);

                case LinkKind.Satellite:
                    // Coverage is not everywhere. A constellation is licensed by
                    // country, so the link ends at a border - a fixed line drawn
                    // before any of this started, and emphatically not the front.
                    //
                    // This is the constraint that stops satellite being a strictly
                    // better radio. Everything the rung offers - unlimited range,
                    // immunity to all but the heaviest jamming - applies only over
                    // your own ground. The moment a drone crosses out, there is
                    // nothing to degrade: the link does not weaken with distance,
                    // it stops.
                    //
                    // And it cuts the wrong way on purpose. Push an offensive past
                    // your own border and your drones are unsupported over ground
                    // you have taken and hold, because the constellation is not
                    // watching the war, it is reading a map.
                    return w.Territory.OwnerAt(w.Entities.Position[i]) == w.Entities.Team[i];

                case LinkKind.Mesh:
                    if (!link.Parent.IsNone && w.Entities.IsAlive(link.Parent)
                        && link.Hops <= SimConstants.MeshMaxHops)
                        return true;
                    // Briefly orphaned between graph rebuilds. Hold the link for one
                    // rebuild period rather than strobing the whole formation every
                    // time an edge node dies.
                    return link.Reparenting
                        && (w.Tick - link.LastEvalTick) <= SimConstants.MeshRebuildInterval;

                case LinkKind.Radio:
                    return HasRadioAnchorInRange(w, i);

                default:
                    return true;
            }
        }

        /// <summary>
        /// Radio needs a friendly crew quarters, relay mast or command post within
        /// range. Relay masts are cheap, which is what makes pushing radio control
        /// forward an ordinary, affordable thing to do - and makes the masts worth
        /// hunting.
        /// </summary>
        static bool HasRadioAnchorInRange(World w, int i)
        {
            byte team = w.Entities.Team[i];
            Fix2 pos = w.Entities.Position[i];
            Fix range = Fix.FromInt(SimConstants.RadioRangeMetres);
            Fix rangeSq = range * range;

            for (int j = 1; j < w.Entities.HighWater; j++)
            {
                if (!w.Entities.IsSlotAlive(j)) continue;
                if (w.Entities.Team[j] != team) continue;
                if (!w.Entities.Has(j, ComponentMask.Structure)) continue;

                int defId = w.Entities.DefId[j];
                if (defId < 0) continue;
                string name = Catalog.Get(defId).Name;
                if (name != "Crew Quarters" && name != "Relay Mast" && name != "Command Post") continue;

                if (Fix2.SqrDistance(pos, w.Entities.Position[j]) <= rangeSq) return true;
            }
            return false;
        }

        static int SampleJam(World w, int i, LinkState link)
        {
            if (link.Kind == LinkKind.Fiber) return 0;
            return w.Signal.SampleFor(w.Entities.Position[i], link.RobustnessEffective);
        }

        static void ApplyGreenEffects(World w, int i)
        {
            if (!w.Entities.Has(i, ComponentMask.Mover)) return;
            w.Entities.Mover[i].SpeedMultiplier = Fix.One;
            w.Entities.Mover[i].AcquisitionMultiplier = Fix.One;
            if (w.Entities.Has(i, ComponentMask.Sortie))
                w.Entities.Sortie[i].AcceptsNewOrders = true;
        }

        /// <summary>
        /// Amber is the interesting state. The drone is still yours, but it is
        /// slower, slower to find a target, and - the part players feel most - it
        /// will finish the order it already has and refuse a new one. You have lost
        /// fine control without losing the aircraft.
        /// </summary>
        static void ApplyAmberEffects(World w, int i)
        {
            if (w.Entities.Has(i, ComponentMask.Mover))
            {
                w.Entities.Mover[i].SpeedMultiplier = Fix.FromDoubleContentOnly(0.70);
                w.Entities.Mover[i].AcquisitionMultiplier = Fix.FromInt(2);
            }
            if (w.Entities.Has(i, ComponentMask.Sortie))
                w.Entities.Sortie[i].AcceptsNewOrders = false;
        }

        static void ApplyBlackEffects(World w, int i)
        {
            LinkState link = w.Entities.Link[i];
            link.BlackTicks++;
            w.Entities.Link[i] = link;

            if (w.Entities.Has(i, ComponentMask.Sortie))
                w.Entities.Sortie[i].AcceptsNewOrders = false;

            switch (link.Policy)
            {
                case BlackPolicy.Abort:
                    // Orbit with nobody flying it, then fall out of the sky. The
                    // airframe is lost; the crew, being in a dugout a few kilometres
                    // away, is not.
                    if (w.Entities.Has(i, ComponentMask.Mover))
                    {
                        w.Entities.Mover[i].HasOrder = false;
                        w.Entities.Mover[i].SpeedMultiplier = Fix.FromDoubleContentOnly(0.30);
                    }
                    if (link.BlackTicks >= SimConstants.BlackToLostTicks)
                    {
                        w.Events.Push(SimEventKind.DroneLostToLinkLoss, w.Tick, w.Entities.HandleAt(i));
                        w.Kill(w.Entities.HandleAt(i), EntityHandle.None);
                    }
                    break;

                case BlackPolicy.LastMile:
                    // The upgrade that makes a jammer a partial defence rather than
                    // a total one: the drone carries on to the last point it was
                    // told about and finishes the job under its own guidance.
                    if (w.Entities.Has(i, ComponentMask.Sortie) && w.Entities.Sortie[i].HasDesignatedPoint
                        && w.Entities.Has(i, ComponentMask.Mover))
                    {
                        w.Entities.Mover[i].HasOrder = true;
                        w.Entities.Mover[i].OrderPoint = w.Entities.Sortie[i].DesignatedPoint;
                        w.Entities.Mover[i].OrderTarget = EntityHandle.None;
                        w.Entities.Mover[i].SpeedMultiplier = Fix.One;
                    }
                    ReleaseCrew(w, i);
                    break;

                case BlackPolicy.DualLink:
                    if (link.BlackTicks >= SimConstants.DualLinkSwitchTicks && link.AltKind != LinkKind.None)
                    {
                        link.Kind = link.AltKind;
                        link.AltKind = LinkKind.None;
                        link.Pip = LinkPip.Amber;
                        link.AmberTicks = 0;
                        link.BlackTicks = 0;
                        link.RobustnessBase = LinkResolverDefaults.RobustnessFor(link.Kind);
                        w.Entities.Link[i] = link;
                        if (w.Entities.Has(i, ComponentMask.Mover))
                            w.Entities.Mover[i].SpeedMultiplier = Fix.FromDoubleContentOnly(0.80);
                        w.Events.Push(SimEventKind.LinkFellBack, w.Tick, w.Entities.HandleAt(i));
                    }
                    break;
            }
        }

        static void ReleaseCrew(World w, int i)
        {
            if (!w.Entities.Has(i, ComponentMask.Sortie)) return;
            int crewId = w.Entities.Sortie[i].CrewId;
            if (crewId < 0) return;
            byte team = w.Entities.Team[i];
            if (team >= w.Players.Length) return;

            w.Players[team].Crews.Release(crewId, w.Tick);
            w.Entities.Sortie[i].CrewId = -1;
        }
    }

    public static class LinkResolverDefaults
    {
        public static byte RobustnessFor(LinkKind kind)
        {
            switch (kind)
            {
                case LinkKind.Radio: return 40;
                case LinkKind.Mesh: return 65;
                case LinkKind.Satellite: return 95;
                case LinkKind.Fiber: return SimConstants.UnjammableRobustness;
                default: return 0;
            }
        }
    }
}
