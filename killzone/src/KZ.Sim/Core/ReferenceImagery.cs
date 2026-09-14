// KILL ZONE - a real-time strategy video game. Fictional factions, hit points,
// build timers. Not operational guidance.
//
// Which ground a side has stored imagery of, and can therefore navigate over
// without anyone telling its drones where they are.
//
// The design this replaces was "reference imagery expires after N days", and the
// research says that half of the idea is real and the other half is invented.
//
// Real: imagery is a scarce, suppliable, politically deniable resource. It has a
// source, a pipeline, a latency and an off-switch, which is every property a game
// resource needs. When one supplier suspended access in 2025 the reporting said
// drone pilots were effectively blinded and the deep-strike campaign obstructed.
// That is a mechanic.
//
// Invented: the decay curve. Nothing measures one, and what evidence exists
// points the other way - imagery spanning nineteen years has been matched
// successfully across seasons, and fielded systems deliberately key on the things
// that do not change: roads, rivers, coastlines, the shape of the ground. A
// "fourteen days old, minus twenty percent" rule would be a number nobody has
// ever measured.
//
// So coverage is binary and permanent until something visible breaks it. You have
// the ground or you do not. It is invalidated by events a player can watch happen
// - a sector bombarded into unrecognisability - rather than by a clock ticking
// somewhere off-screen.

namespace KZ.Sim
{
    public sealed class ReferenceImagery
    {
        public const int CellMetres = 64;

        public readonly int CellsX;
        public readonly int CellsY;
        readonly int teams;

        // One bit per team per cell, flattened. Teams are few and cells are many,
        // so a byte per cell with one bit per team costs less than a bool array
        // per team and keeps the whole thing in one contiguous block.
        readonly byte[] covered;

        public ReferenceImagery(int widthMetres, int heightMetres, int teamCount)
        {
            CellsX = (widthMetres + CellMetres - 1) / CellMetres;
            CellsY = (heightMetres + CellMetres - 1) / CellMetres;
            teams = teamCount;
            covered = new byte[CellsX * CellsY];
        }

        static int Bit(byte team) { return 1 << (team & 7); }

        public bool HasCoverage(byte team, Fix2 p)
        {
            int cx = p.X.FloorToInt() / CellMetres;
            int cy = p.Y.FloorToInt() / CellMetres;
            if (cx < 0 || cy < 0 || cx >= CellsX || cy >= CellsY) return false;
            return (covered[cy * CellsX + cx] & Bit(team)) != 0;
        }

        public void Grant(byte team, int cx, int cy)
        {
            if (cx < 0 || cy < 0 || cx >= CellsX || cy >= CellsY) return;
            covered[cy * CellsX + cx] |= (byte)Bit(team);
        }

        /// <summary>
        /// What a reconnaissance sortie buys: coverage of the ground it flew over.
        /// This is the supply end of the resource, and the reason deep strike
        /// needs reconnaissance of ground you do not hold before it needs
        /// anything else.
        /// </summary>
        public void GrantAround(byte team, Fix2 centre, Fix radiusMetres)
        {
            int r = radiusMetres.FloorToInt() / CellMetres + 1;
            int cx0 = centre.X.FloorToInt() / CellMetres;
            int cy0 = centre.Y.FloorToInt() / CellMetres;
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                    if (dx * dx + dy * dy <= r * r)
                        Grant(team, cx0 + dx, cy0 + dy);
        }

        /// <summary>
        /// What heavy bombardment takes away. Not a timer - an event, and one the
        /// player watched happen, so when a drone gets lost over that sector
        /// afterwards the reason is something they saw rather than something the
        /// simulation did quietly.
        ///
        /// It invalidates everyone's imagery, including the imagery of whoever
        /// did the bombarding. Churned ground does not read differently depending
        /// on who churned it.
        /// </summary>
        public void Invalidate(Fix2 centre, Fix radiusMetres)
        {
            int r = radiusMetres.FloorToInt() / CellMetres + 1;
            int cx0 = centre.X.FloorToInt() / CellMetres;
            int cy0 = centre.Y.FloorToInt() / CellMetres;
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (dx * dx + dy * dy > r * r) continue;
                    int cx = cx0 + dx, cy = cy0 + dy;
                    if (cx < 0 || cy < 0 || cx >= CellsX || cy >= CellsY) continue;
                    covered[cy * CellsX + cx] = 0;
                }
            }
        }

        public ulong StateHash()
        {
            unchecked
            {
                ulong h = 1469598103934665603UL;
                for (int i = 0; i < covered.Length; i++)
                    h = (h ^ covered[i]) * 1099511628211UL;
                return h;
            }
        }
    }
}
