namespace KZ.Sim
{
    public static class SimConstants
    {
        public const int TicksPerSecond = 32;          // live: read by World

        /// <summary>DEAD: the two-second track hold, with zero call sites.</summary>
        public const int TrackHoldTicks = 64;

        /// <summary>TEST-ONLY: read by a test and by nothing in the simulation.</summary>
        public const int SceneMatchTicks = 96;
    }
}
