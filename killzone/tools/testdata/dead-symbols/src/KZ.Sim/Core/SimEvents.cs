namespace KZ.Sim
{
    public enum SimEventKind : byte
    {
        None = 0,
        UnitDied,
        SalvageCollected   // DEAD: counted by a reader, pushed by nobody
    }

    public sealed class EventRing
    {
        readonly SimEventKind[] kinds = new SimEventKind[64];
        int count;

        public int Count { get { return count; } }

        public void Push(SimEventKind kind, int tick)
        {
            if (count < kinds.Length) { kinds[count] = kind; count++; }
        }

        public int CountOf(SimEventKind kind)
        {
            int n = 0;
            for (int i = 0; i < count; i++) if (kinds[i] == kind) n++;
            return n;
        }
    }
}
