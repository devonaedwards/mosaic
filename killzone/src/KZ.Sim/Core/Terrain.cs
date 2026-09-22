// KILL ZONE - a real-time strategy video game.
// The ground: one tile class per 8 m build tile.
//
// Terrain does three jobs here. It decides where things can drive, it blocks
// line of sight for ground units, and - the interesting one - it decides how
// likely a fiber-optic drone's thread is to part. A drone dragging a hair-thin
// filament through a forest or under a power line is living dangerously in a way
// the same drone over open steppe is not.

namespace KZ.Sim
{
    public sealed class Terrain
    {
        public readonly int WidthTiles;
        public readonly int HeightTiles;
        public readonly int WidthMetres;
        public readonly int HeightMetres;

        readonly TileClass[] tiles;

        public Terrain(int widthMetres, int heightMetres)
        {
            WidthMetres = widthMetres;
            HeightMetres = heightMetres;
            WidthTiles = widthMetres / SimConstants.BuildTileMetres;
            HeightTiles = heightMetres / SimConstants.BuildTileMetres;
            tiles = new TileClass[WidthTiles * HeightTiles];
        }

        public TileClass At(int tx, int ty)
        {
            if (tx < 0 || ty < 0 || tx >= WidthTiles || ty >= HeightTiles) return TileClass.Impassable;
            return tiles[ty * WidthTiles + tx];
        }

        public void Set(int tx, int ty, TileClass c)
        {
            if (tx < 0 || ty < 0 || tx >= WidthTiles || ty >= HeightTiles) return;
            tiles[ty * WidthTiles + tx] = c;
        }

        public TileClass AtPosition(Fix2 p)
        {
            return At(p.X.FloorToInt() / SimConstants.BuildTileMetres,
                      p.Y.FloorToInt() / SimConstants.BuildTileMetres);
        }

        public void Fill(TileClass c)
        {
            for (int i = 0; i < tiles.Length; i++) tiles[i] = c;
        }

        public void FillRect(int tx0, int ty0, int tx1, int ty1, TileClass c)
        {
            for (int y = ty0; y <= ty1; y++)
                for (int x = tx0; x <= tx1; x++)
                    Set(x, y, c);
        }

        public bool IsPassableForGround(Fix2 p)
        {
            TileClass c = AtPosition(p);
            return c != TileClass.Impassable && c != TileClass.Water;
        }

        public bool InBounds(Fix2 p)
        {
            return p.X.Raw >= 0 && p.Y.Raw >= 0
                && p.X < Fix.FromInt(WidthMetres) && p.Y < Fix.FromInt(HeightMetres);
        }

        /// <summary>
        /// How likely a fiber thread lying across this ground is to part, as a
        /// probability per second per node-length of thread. Open ground is free;
        /// a power-line corridor will eat a drone in about ten seconds.
        ///
        /// **All five are designer estimates and none of them is sourced.** That
        /// has to be said here because it is not obvious anywhere else: terrain.md
        /// §7 and its consequence-four passage both recite these figures, which
        /// makes them look researched, but the document is quoting this table
        /// rather than sourcing it - it says so itself two hundred lines further
        /// down ("that is the game's SnagRatePerSecond(Forest) = 0.040 rendered in
        /// prose"). The citation runs code to document, so a reader checking the
        /// number finds a mirror.
        ///
        /// What the research does support is the *direction*: snagging is real and
        /// terrain-coupled ("to maximise range, operators need to fly low and in a
        /// straight line to prevent the cable from sagging or snagging on
        /// obstacles", terrain.md §7). Not one rate, and no figure anywhere in the
        /// corpus for how often a thread parts per sortie.
        ///
        /// This matters more than a normal unsourced constant: FINDINGS 43's fiber
        /// sweep concluded that a filament dragged down a road parts after about
        /// 1,500 m, and where a player launches from is a decision because of it.
        /// That conclusion rests on the 0.015 below. Research queue brief N asks
        /// for the replacements.
        /// </summary>
        public static Fix SnagRatePerSecond(TileClass c)
        {
            switch (c)
            {
                case TileClass.Open: return Fix.Zero;
                case TileClass.Road: return Fix.FromDoubleContentOnly(0.015);
                case TileClass.Forest: return Fix.FromDoubleContentOnly(0.040);
                case TileClass.PowerLine: return Fix.FromDoubleContentOnly(0.090);
                case TileClass.Rubble: return Fix.FromDoubleContentOnly(0.030);
                default: return Fix.Zero;
            }
        }

        /// <summary>Ground units cannot see through forest, rubble or buildings.</summary>
        public static bool BlocksGroundSight(TileClass c)
        {
            return c == TileClass.Forest || c == TileClass.Rubble || c == TileClass.Impassable;
        }
    }
}
