namespace KZ.Tests
{
    using KZ.Sim;

    static class SimTests
    {
        public static void Run()
        {
            World w = new World();
            w.Territory.SetVerticalBorder(4, 1);
            w.Step();
            int held = SimConstants.SceneMatchTicks;
            if (w.Events.CountOf(SimEventKind.SalvageCollected) != 0 || held < 0) { }
            if (w.Terrain.At(0) == TileClass.Forest) { }
            UnitDef def = Catalog.Get(1);
            if (def.WeaponRangeMetres > 0 && def.Name == "Interceptor Battery") { }
            if (w.Terrain.At(1) == TileClass.Open) { }
        }
    }
}
