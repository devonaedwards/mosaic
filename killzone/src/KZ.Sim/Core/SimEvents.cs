// KILL ZONE - a real-time strategy video game.
// The one-way channel from the simulation to everything that watches it.
//
// The renderer, the audio system, the interface and the telemetry all read this
// single append-only stream rather than querying the simulation. That keeps the
// data flowing in exactly one direction: the view can never ask the simulation a
// question whose answer might cost a calculation, and can never accidentally
// mutate it. It is also why a replay can reproduce not just the outcome of a
// match but every explosion and every line of radio chatter in it.

namespace KZ.Sim
{
    public enum SimEventKind : byte
    {
        None = 0,
        UnitSpawned,
        UnitDied,
        LinkAmber,
        LinkBlack,
        LinkRestored,
        LinkFellBack,        // a dual-link airframe switched to its alternate
        DroneLostToLinkLoss, // orbited with no pilot until it fell out of the sky
        TetherCut,
        TetherFound,         // an enemy stumbled onto a filament and has a bearing home
        CrewStateChanged,
        CrewPromoted,
        CrewKilled,
        SortieLaunched,
        SortieRejected,      // no crew, no airframe, or no uplink capacity
        KillVerified,
        KillUnverified,
        SalvageDropped,
        SalvageCollected,
        AutonomyMisidentified, // an autonomous munition picked a decoy or a friendly
        StructureDestroyed,
        DayPhaseChanged
    }

    public struct SimEvent
    {
        public SimEventKind Kind;
        public int Tick;
        public EntityHandle A;
        public EntityHandle B;
        public byte Team;
        public int Param;
    }

    /// <summary>
    /// A fixed-size ring. Overflow is a bug rather than something to absorb
    /// quietly: if a tick ever produces four thousand events, something is wrong
    /// and silently dropping the tail would hide it.
    /// </summary>
    public sealed class EventRing
    {
        readonly SimEvent[] buffer;
        int count;
        public bool Overflowed { get; private set; }

        public EventRing(int capacity) { buffer = new SimEvent[capacity]; }

        public int Count { get { return count; } }
        public SimEvent this[int i] { get { return buffer[i]; } }

        public void Clear() { count = 0; Overflowed = false; }

        public void Push(SimEventKind kind, int tick, EntityHandle a, EntityHandle b, byte team, int param)
        {
            if (count >= buffer.Length) { Overflowed = true; return; }
            buffer[count].Kind = kind;
            buffer[count].Tick = tick;
            buffer[count].A = a;
            buffer[count].B = b;
            buffer[count].Team = team;
            buffer[count].Param = param;
            count++;
        }

        public void Push(SimEventKind kind, int tick, EntityHandle a)
        {
            Push(kind, tick, a, EntityHandle.None, 0, 0);
        }

        public int CountOf(SimEventKind kind)
        {
            int n = 0;
            for (int i = 0; i < count; i++) if (buffer[i].Kind == kind) n++;
            return n;
        }
    }
}
