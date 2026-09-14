namespace KZ.Sim
{
    public sealed class Terrain
    {
        public TileClass At(int x) { return x == 0 ? TileClass.Open : TileClass.Forest; }

        /// <summary>DEAD: the whole reason detection has no line of sight.</summary>
        public bool BlocksGroundSight(int ax, int bx) { return At(ax) == TileClass.Forest; }
    }
}
