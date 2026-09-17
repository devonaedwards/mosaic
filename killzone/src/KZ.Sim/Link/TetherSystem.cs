// KILL ZONE - a real-time strategy video game.
// Fiber-optic tethers: the unjammable link, and what it costs you.
//
// A fiber drone trails a filament back to the operator. No radio means nothing
// to jam - the one link in the game that a jammer cannot touch. In exchange the
// thread is three separate liabilities at once. It is a hard leash, so the drone
// has a maximum reach that no upgrade to its engine can extend. It snags on
// terrain, so the route matters as much as the target. And it is a physical
// object lying across the map that an enemy can find and follow home.
//
// The thread is deliberately simulated as a real polyline rather than a distance
// check, because all three of those liabilities are geometric.

namespace KZ.Sim
{
    public enum TetherState : byte
    {
        Free = 0,
        Live = 1,
        Taut = 2,
        Cut = 3,
        Lingering = 4
    }

    public struct TetherNode
    {
        public Fix2 Position;
        public TileClass Tile;
        public int LastEvalTick;
    }

    public sealed class TetherSystem
    {
        public sealed class Tether
        {
            public TetherState State;
            public EntityHandle Drone;
            public Fix2 AnchorPosition;
            public Fix Spooled;
            public Fix SpoolMax;
            public int NodeCount;
            public readonly TetherNode[] Nodes = new TetherNode[SimConstants.TetherMaxNodes];
            public int TautSinceTick;
            public int CutTick;
            public int RoundRobinCursor;
            public byte Team;

            /// <summary>
            /// Which teams have already walked over this thread, one bit each.
            /// A filament is found once and stays found: without this the same
            /// vehicle sitting on the same line raises the same event four times
            /// a second for as long as it is parked there, which is a log full of
            /// one discovery rather than a discovery.
            /// </summary>
            public byte FoundByTeams;
        }

        readonly Tether[] tethers;
        readonly Terrain terrain;
        readonly DetRandom rng;

        public TetherSystem(int capacity, Terrain terrain, DetRandom snagRng)
        {
            tethers = new Tether[capacity];
            for (int i = 0; i < capacity; i++) tethers[i] = new Tether();
            this.terrain = terrain;
            this.rng = snagRng;
        }

        /// <summary>
        /// Every part of a thread that the simulation reads later, folded into the
        /// world hash.
        ///
        /// This existed as FoundByTeams alone, hashed from World.StateHash, and the
        /// comment there said out loud that the polyline was not covered. That was a
        /// real hole rather than a tidy-up: node positions decide whether a drone is
        /// held back by its own cable (ConstrainVelocity), whether a thread snags,
        /// and - since AUDIT F14 - whether an enemy driving over one finds it. Two
        /// peers that disagreed about where a filament lay would have agreed about
        /// the world hash right up until the drone on the end of it went somewhere
        /// else, which is the worst shape a determinism bug can have: silent, then
        /// far from its cause.
        ///
        /// Free slots are skipped rather than hashed as zero, so capacity can change
        /// between builds without moving the hash of a world that is using less of
        /// it. Nodes are hashed to NodeCount for the same reason.
        /// </summary>
        public ulong StateHash()
        {
            unchecked
            {
                const ulong Prime = 1099511628211UL;
                ulong h = 1469598103934665603UL;
                for (int id = 0; id < tethers.Length; id++)
                {
                    Tether t = tethers[id];
                    h = (h ^ (ulong)t.State) * Prime;
                    if (t.State == TetherState.Free) continue;

                    h = (h ^ (ulong)t.Drone.Value) * Prime;
                    h = (h ^ (ulong)t.AnchorPosition.X.Raw) * Prime;
                    h = (h ^ (ulong)t.AnchorPosition.Y.Raw) * Prime;
                    h = (h ^ (ulong)t.Spooled.Raw) * Prime;
                    h = (h ^ (ulong)t.SpoolMax.Raw) * Prime;
                    h = (h ^ (ulong)t.TautSinceTick) * Prime;
                    h = (h ^ (ulong)t.CutTick) * Prime;
                    h = (h ^ (ulong)t.RoundRobinCursor) * Prime;
                    h = (h ^ t.Team) * Prime;
                    h = (h ^ t.FoundByTeams) * Prime;

                    h = (h ^ (ulong)t.NodeCount) * Prime;
                    for (int n = 0; n < t.NodeCount; n++)
                    {
                        h = (h ^ (ulong)t.Nodes[n].Position.X.Raw) * Prime;
                        h = (h ^ (ulong)t.Nodes[n].Position.Y.Raw) * Prime;
                        h = (h ^ (ulong)t.Nodes[n].Tile) * Prime;
                    }
                }
                return h;
            }
        }

        public int Capacity { get { return tethers.Length; } }
        public Tether Get(int id) { return tethers[id]; }

        public int LiveCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < tethers.Length; i++)
                    if (tethers[i].State == TetherState.Live || tethers[i].State == TetherState.Taut) n++;
                return n;
            }
        }

        /// <summary>Spool out a new thread from an anchor. Returns -1 if none are free.</summary>
        public int Create(EntityHandle drone, Fix2 anchorPos, Fix spoolMax, byte team, int tick)
        {
            for (int i = 0; i < tethers.Length; i++)
            {
                Tether t = tethers[i];
                if (t.State != TetherState.Free) continue;

                t.State = TetherState.Live;
                t.Drone = drone;
                t.AnchorPosition = anchorPos;
                t.Spooled = Fix.Zero;
                t.SpoolMax = spoolMax;
                t.NodeCount = 1;
                t.Nodes[0].Position = anchorPos;
                t.Nodes[0].Tile = terrain.AtPosition(anchorPos);
                t.Nodes[0].LastEvalTick = tick;
                t.TautSinceTick = 0;
                t.CutTick = 0;
                t.RoundRobinCursor = 0;
                t.Team = team;
                t.FoundByTeams = 0;
                return i;
            }
            return -1;
        }

        public void Release(int id)
        {
            if (id < 0 || id >= tethers.Length) return;
            tethers[id].State = TetherState.Free;
            tethers[id].NodeCount = 0;
        }

        /// <summary>Cut the thread. The drone loses its link; the line stays on the map.</summary>
        public void Cut(int id, int tick)
        {
            if (id < 0 || id >= tethers.Length) return;
            Tether t = tethers[id];
            if (t.State == TetherState.Free || t.State == TetherState.Cut || t.State == TetherState.Lingering) return;
            t.State = TetherState.Cut;
            t.CutTick = tick;
        }

        public bool IsConnected(int id)
        {
            if (id < 0 || id >= tethers.Length) return false;
            TetherState s = tethers[id].State;
            return s == TetherState.Live || s == TetherState.Taut;
        }

        /// <summary>
        /// Advance one thread. Lays new line behind the drone, keeps the spool
        /// accounting, enforces the leash, and rolls for snags.
        /// </summary>
        public void Update(int id, Fix2 dronePos, int tick, out bool cutThisTick)
        {
            cutThisTick = false;
            Tether t = tethers[id];

            if (t.State == TetherState.Cut)
            {
                t.State = TetherState.Lingering;
                return;
            }
            if (t.State == TetherState.Lingering)
            {
                // A dead man's thread still points home for a while, then fades.
                if (tick - t.CutTick >= SimConstants.TetherLingerTicks) Release(id);
                return;
            }
            if (t.State == TetherState.Free) return;

            AppendNodeIfNeeded(t, dronePos, tick);
            RecomputeSpooled(t, dronePos);

            // The leash. Past it the drone can still fly sideways or back toward
            // the anchor, which reads on screen as exactly what it is.
            if (t.Spooled >= t.SpoolMax)
            {
                if (t.State != TetherState.Taut)
                {
                    t.State = TetherState.Taut;
                    t.TautSinceTick = tick;
                }
                else if (tick - t.TautSinceTick >= SimConstants.TetherTautGraceTicks)
                {
                    // Over-extension eventually parts the line. Preventing the
                    // drone from going further would be safer and far less
                    // interesting; this way pushing your luck has a price.
                    Cut(id, tick);
                    cutThisTick = true;
                    return;
                }
            }
            else if (t.State == TetherState.Taut)
            {
                t.State = TetherState.Live;
                t.TautSinceTick = 0;
            }

            if (RollSnag(t, tick))
            {
                Cut(id, tick);
                cutThisTick = true;
            }
        }

        /// <summary>
        /// A new node goes down every twelve metres of travel. Twelve because it is
        /// one and a half build tiles, so a segment never spans more than two
        /// terrain classes and its snag lookup is a single read.
        /// </summary>
        static void AppendNodeIfNeeded(Tether t, Fix2 dronePos, int tick)
        {
            Fix2 last = t.Nodes[t.NodeCount - 1].Position;
            Fix d = Fix2.Distance(last, dronePos);
            if (d < Fix.FromInt(SimConstants.TetherNodeSpacingMetres)) return;

            if (t.NodeCount >= SimConstants.TetherMaxNodes)
            {
                Decimate(t);
                if (t.NodeCount >= SimConstants.TetherMaxNodes) return;
            }

            t.Nodes[t.NodeCount].Position = dronePos;
            t.Nodes[t.NodeCount].Tile = TileClass.Open;
            t.Nodes[t.NodeCount].LastEvalTick = tick;
            t.NodeCount++;
        }

        /// <summary>
        /// Drop nodes that sit almost on the straight line between their
        /// neighbours. A drone that flies in circles can otherwise exhaust the
        /// node budget without covering any ground.
        /// </summary>
        static void Decimate(Tether t)
        {
            int write = 1;
            for (int i = 1; i < t.NodeCount - 1; i++)
            {
                Fix2 a = t.Nodes[write - 1].Position;
                Fix2 b = t.Nodes[i].Position;
                Fix2 c = t.Nodes[i + 1].Position;
                Fix direct = Fix2.Distance(a, c);
                Fix viaB = Fix2.Distance(a, b) + Fix2.Distance(b, c);
                // If going through b is barely longer than going straight, b is noise.
                if (viaB - direct < Fix.FromDoubleContentOnly(1.5)) continue;
                t.Nodes[write++] = t.Nodes[i];
            }
            t.Nodes[write++] = t.Nodes[t.NodeCount - 1];
            t.NodeCount = write;
        }

        static void RecomputeSpooled(Tether t, Fix2 dronePos)
        {
            Fix total = Fix.Zero;
            for (int i = 1; i < t.NodeCount; i++)
                total += Fix2.Distance(t.Nodes[i - 1].Position, t.Nodes[i].Position);
            total += Fix2.Distance(t.Nodes[t.NodeCount - 1].Position, dronePos);
            t.Spooled = total;
        }

        /// <summary>
        /// Roll once per tick for whether the thread parts.
        ///
        /// Two segments are inspected: the newest, which is where the drone
        /// actually is, so flying into a treeline bites immediately; and one older
        /// segment on a rotating cursor, so the trailing line matters too. The
        /// worst ground of the two sets the hazard.
        ///
        /// The roll is made once against a per-second rate, not once per segment.
        /// Rolling per segment sounds more detailed and is badly wrong: a long
        /// thread has dozens of segments, so it would multiply the stated hazard by
        /// its own length and a drone flying a kilometre over a quiet road would be
        /// certain to lose its line. "Nine percent per second under power lines"
        /// has to mean what it says, because that is the number a player learns.
        /// </summary>
        bool RollSnag(Tether t, int tick)
        {
            if (t.NodeCount < 2) return false;

            Fix worst = SegmentRate(t, t.NodeCount - 1);

            if (t.NodeCount > 2)
            {
                t.RoundRobinCursor++;
                if (t.RoundRobinCursor >= t.NodeCount - 1) t.RoundRobinCursor = 1;
                Fix older = SegmentRate(t, t.RoundRobinCursor);
                if (older > worst) worst = older;
            }

            if (worst.Raw <= 0) return false;
            // Terrain.SnagRatePerSecond is per real second, so it divides by the
            // ticks in a real second, not by the tick rate. At 4x that is eight,
            // and the probability per second of *world* is unchanged - which is
            // what keeps a forest crossing as dangerous per kilometre flown as it
            // was before the rescale.
            return rng.Chance(worst / Fix.FromInt(SimConstants.TicksPerRealSecond));
        }

        /// <summary>The per-second hazard carried by the ground one segment lies across.</summary>
        Fix SegmentRate(Tether t, int nodeIndex)
        {
            if (nodeIndex < 1 || nodeIndex >= t.NodeCount) return Fix.Zero;

            Fix2 a = t.Nodes[nodeIndex - 1].Position;
            Fix2 b = t.Nodes[nodeIndex].Position;
            Fix2 mid = new Fix2((a.X + b.X) / 2, (a.Y + b.Y) / 2);

            TileClass tile = terrain.AtPosition(mid);
            t.Nodes[nodeIndex].Tile = tile;
            return Terrain.SnagRatePerSecond(tile);
        }

        /// <summary>
        /// Constrain a drone at full stretch. It keeps whatever part of its
        /// intended velocity points along or back toward the anchor and loses the
        /// part that would pull further away.
        /// </summary>
        public Fix2 ConstrainVelocity(int id, Fix2 dronePos, Fix2 desiredVelocity)
        {
            Tether t = tethers[id];
            if (t.State != TetherState.Taut) return desiredVelocity;

            // Normally the line pulls back along the last laid segment. When the
            // drone is sitting almost exactly on its newest node that direction is
            // numerically meaningless, so fall back to the bearing home.
            Fix2 toLastNode = dronePos - t.Nodes[t.NodeCount - 1].Position;
            Fix2 outward = toLastNode.SqrMagnitude() > Fix.One
                ? toLastNode.Normalized()
                : (dronePos - t.AnchorPosition).Normalized();
            if (outward == Fix2.Zero) return desiredVelocity;

            Fix radial = Fix2.Dot(desiredVelocity, outward);
            if (radial <= Fix.Zero) return desiredVelocity;
            return desiredVelocity - outward * radial;
        }

        /// <summary>
        /// Whether any live segment of this thread passes within range of a point.
        /// Used when an enemy stumbles across a filament: finding one gives a
        /// bearing back to the launch site, which is the price fiber pays for
        /// being unjammable.
        /// </summary>
        public bool AnySegmentNear(int id, Fix2 point, Fix range)
        {
            Tether t = tethers[id];
            if (t.State != TetherState.Live && t.State != TetherState.Taut
                && t.State != TetherState.Lingering) return false;

            Fix rangeSq = range * range;
            for (int i = 1; i < t.NodeCount; i++)
            {
                if (PointSegmentDistanceSq(point, t.Nodes[i - 1].Position, t.Nodes[i].Position) <= rangeSq)
                    return true;
            }
            return false;
        }

        public static Fix PointSegmentDistanceSq(Fix2 p, Fix2 a, Fix2 b)
        {
            Fix2 ab = b - a;
            Fix lenSq = ab.SqrMagnitude();
            if (lenSq.Raw == 0) return Fix2.SqrDistance(p, a);

            Fix t = Fix2.Dot(p - a, ab) / lenSq;
            t = Fix.Clamp(t, Fix.Zero, Fix.One);
            Fix2 proj = a + ab * t;
            return Fix2.SqrDistance(p, proj);
        }
    }
}
