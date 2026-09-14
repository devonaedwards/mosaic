// KILL ZONE - a real-time strategy video game.
// Mesh relay chains: control range that grows with your army.
//
// One faction's whole identity is that every drone it puts in the air is also a
// repeater. Push six drones forward and they become their own relay chain, so
// the deeper you are the deeper you can go. The cost is fragility: a chain is
// only as good as its weakest hop, and killing one node drops everything hanging
// off it. That trade - range you build out of your own units, against a
// single-point failure an opponent can hunt - is the faction.
//
// The graph is rebuilt from scratch a few times a second rather than patched
// incrementally. Rebuilding is cheap at these sizes and, more importantly, a
// full rebuild has no history, so it cannot drift between two machines playing
// the same match.

namespace KZ.Sim
{
    public struct MeshNode
    {
        public EntityHandle Handle;
        public Fix2 Position;
        public byte Team;
        public bool IsAnchor;   // crew quarters, relay masts, motherships
        public bool IsRepeater; // any friendly air unit, for the mesh faction
    }

    public sealed class MeshGraph
    {
        public const int MaxNodes = 512;
        public const int MaxNeighbourCandidates = 24;

        readonly MeshNode[] nodes = new MeshNode[MaxNodes];
        int nodeCount;

        readonly int[] hops = new int[MaxNodes];
        readonly int[] parent = new int[MaxNodes];
        readonly int[] altParents = new int[MaxNodes];
        readonly int[] queue = new int[MaxNodes];

        public int NodeCount { get { return nodeCount; } }
        public MeshNode NodeAt(int i) { return nodes[i]; }
        public int HopsAt(int i) { return hops[i]; }
        public int AltParentsAt(int i) { return altParents[i]; }
        public EntityHandle ParentOf(int i)
        {
            return parent[i] >= 0 ? nodes[parent[i]].Handle : EntityHandle.None;
        }

        public void Clear() { nodeCount = 0; }

        public int Add(MeshNode n)
        {
            if (nodeCount >= MaxNodes) return -1;
            nodes[nodeCount] = n;
            return nodeCount++;
        }

        /// <summary>
        /// Breadth-first search outward from every anchor at once.
        ///
        /// Ties are broken by hop count, then by distance to the candidate parent,
        /// then by raw entity handle. That last tiebreak looks arbitrary and is the
        /// most important line in the method: without a total ordering, two
        /// machines simulating the same match could pick different parents for a
        /// drone equidistant from two relays, and the match would silently diverge.
        /// </summary>
        public void Rebuild(byte team, Fix hopRangeMetres, int maxHops)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                hops[i] = int.MaxValue;
                parent[i] = -1;
                altParents[i] = 0;
            }

            int head = 0, tail = 0;

            for (int i = 0; i < nodeCount; i++)
            {
                if (nodes[i].Team != team || !nodes[i].IsAnchor) continue;
                hops[i] = 0;
                queue[tail++] = i;
            }

            Fix hopRangeSq = hopRangeMetres * hopRangeMetres;

            while (head < tail)
            {
                int current = queue[head++];
                int nextHop = hops[current] + 1;
                if (nextHop > maxHops) continue;

                for (int j = 0; j < nodeCount; j++)
                {
                    if (j == current) continue;
                    if (nodes[j].Team != team) continue;
                    if (nodes[j].IsAnchor) continue;

                    Fix dSq = Fix2.SqrDistance(nodes[current].Position, nodes[j].Position);
                    if (dSq > hopRangeSq) continue;

                    // A node that can already be reached at this depth by another
                    // parent gains redundancy rather than a new parent. Redundancy
                    // is what makes a dense mesh harder to jam than a thin one.
                    if (hops[j] == nextHop && parent[j] != current)
                    {
                        if (altParents[j] < 3) altParents[j]++;
                        continue;
                    }

                    if (nextHop < hops[j])
                    {
                        hops[j] = nextHop;
                        parent[j] = current;
                        altParents[j] = 0;
                        queue[tail++] = j;
                    }
                    else if (nextHop == hops[j] && parent[j] >= 0)
                    {
                        // Same depth: prefer the nearer parent, then the lower handle.
                        Fix existing = Fix2.SqrDistance(nodes[parent[j]].Position, nodes[j].Position);
                        bool better = dSq < existing
                            || (dSq == existing && nodes[current].Handle.Value < nodes[parent[j]].Handle.Value);
                        if (better) parent[j] = current;
                        if (altParents[j] < 3) altParents[j]++;
                    }
                }
            }
        }

        public bool IsConnected(int i) { return hops[i] != int.MaxValue; }

        /// <summary>
        /// Extra robustness earned by having somewhere else to route. Capped, so a
        /// hundred-drone cloud is not simply immune to electronic warfare.
        /// </summary>
        public static int RedundancyBonus(int altParentCount)
        {
            int bonus = altParentCount * SimConstants.MeshRobustnessPerAltHop;
            return bonus > SimConstants.MeshRobustnessAltCap ? SimConstants.MeshRobustnessAltCap : bonus;
        }
    }
}
