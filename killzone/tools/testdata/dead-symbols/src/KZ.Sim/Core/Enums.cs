namespace KZ.Sim
{
    public enum TileClass : byte
    {
        Open = 0,
        Forest = 1
    }

    public enum CrewState : byte
    {
        Ready = 0,
        Flying = 1,
        Reserved = 2   // DEAD: no code path ever puts a crew here
    }

    public enum BlackPolicy : byte
    {
        Abort = 0,
        DualLink = 1
    }
}
