// KILL ZONE - a real-time strategy video game.
// The entity table: a dense struct-of-arrays store with generation-stamped
// handles.
//
// This is deliberately not a general entity-component system. A general ECS
// earns its complexity when you do not know your component composition ahead of
// time. We do - this game has about thirty component kinds and fourteen stable
// archetypes. A flat table with a free list is a few hundred lines, serialises
// trivially for snapshots, hashes trivially for desync detection, and has no
// iteration-order subtleties to police.

using System;

namespace KZ.Sim
{
    /// <summary>
    /// A reference to an entity: twenty bits of index and twelve bits of
    /// generation. The generation is what makes a stale handle safe - when a
    /// drone dies and its slot is reused, an old handle pointing at that slot no
    /// longer resolves, so a queued order cannot silently retarget onto whatever
    /// took its place.
    /// </summary>
    public readonly struct EntityHandle : IEquatable<EntityHandle>
    {
        public readonly uint Value;

        public const int IndexBits = 20;
        public const uint IndexMask = (1u << IndexBits) - 1;
        public const uint GenerationMask = 0xFFFu;

        public EntityHandle(uint value) { Value = value; }

        public EntityHandle(int index, int generation)
        {
            Value = ((uint)index & IndexMask) | (((uint)generation & GenerationMask) << IndexBits);
        }

        public static readonly EntityHandle None = new EntityHandle(0u);

        public int Index { get { return (int)(Value & IndexMask); } }
        public int Generation { get { return (int)((Value >> IndexBits) & GenerationMask); } }
        public bool IsNone { get { return Value == 0u; } }

        public bool Equals(EntityHandle o) { return Value == o.Value; }
        public override bool Equals(object o) { return o is EntityHandle && ((EntityHandle)o).Value == Value; }
        public override int GetHashCode() { return (int)Value; }
        public override string ToString() { return "E" + Index + ":" + Generation; }

        public static bool operator ==(EntityHandle a, EntityHandle b) { return a.Value == b.Value; }
        public static bool operator !=(EntityHandle a, EntityHandle b) { return a.Value != b.Value; }
    }

    [Flags]
    public enum ComponentMask : uint
    {
        None = 0,
        Transform = 1u << 0,
        Health = 1u << 1,
        Mover = 1u << 2,
        Link = 1u << 3,
        Sortie = 1u << 4,
        Sensor = 1u << 5,
        Weapon = 1u << 6,
        Emitter = 1u << 7,
        Tethered = 1u << 8,
        Autonomy = 1u << 9,
        Producer = 1u << 10,
        Salvage = 1u << 11,
        Decoy = 1u << 12,
        Structure = 1u << 13,
        MeshRepeater = 1u << 14,
        Mine = 1u << 15,
        Nav = 1u << 16
    }

    public sealed class EntityTable
    {
        public readonly int Capacity;

        // Slot bookkeeping.
        readonly bool[] alive;
        readonly int[] generation;
        readonly int[] freeList;
        int freeCount;
        int highWater;

        // Component presence and the dense arrays themselves.
        public readonly ComponentMask[] Mask;

        public readonly Fix2[] Position;
        public readonly Fix2[] Velocity;
        public readonly ushort[] Yaw;
        public readonly Layer[] EntityLayer;

        public readonly Fix[] Hp;
        public readonly Fix[] HpMax;
        public readonly Fix[] CageHp;
        public readonly ArmourClass[] Armour;

        public readonly byte[] Team;
        public readonly int[] DefId;
        public readonly byte[] Rank;

        public readonly LinkState[] Link;
        public readonly SortieState[] Sortie;
        public readonly SensorSuite[] Sensor;
        public readonly SignatureProfile[] Signature;
        public readonly WeaponState[] Weapon;
        public readonly EmitterState[] Emitter;
        public readonly AutonomyState[] Autonomy;
        public readonly MoverState[] Mover;
        public readonly SalvageState[] SalvagePile;
        public readonly DecoyState[] Decoy;
        public readonly MineState[] Mine;
        public readonly NavState[] Nav;

        public readonly int[] TetherId;

        /// <summary>Cheapest masking in the game, and the only kind a vehicle can wear.</summary>
        public readonly bool[] HasThermalBlanket;

        public EntityTable(int capacity)
        {
            Capacity = capacity;
            alive = new bool[capacity];
            generation = new int[capacity];
            freeList = new int[capacity];
            freeCount = 0;
            // Index zero is reserved so that a zero handle always means "nothing".
            highWater = 1;

            Mask = new ComponentMask[capacity];
            Position = new Fix2[capacity];
            Velocity = new Fix2[capacity];
            Yaw = new ushort[capacity];
            EntityLayer = new Layer[capacity];
            Hp = new Fix[capacity];
            HpMax = new Fix[capacity];
            CageHp = new Fix[capacity];
            Armour = new ArmourClass[capacity];
            Team = new byte[capacity];
            DefId = new int[capacity];
            Rank = new byte[capacity];
            Link = new LinkState[capacity];
            Sortie = new SortieState[capacity];
            Sensor = new SensorSuite[capacity];
            Signature = new SignatureProfile[capacity];
            Weapon = new WeaponState[capacity];
            Emitter = new EmitterState[capacity];
            Autonomy = new AutonomyState[capacity];
            Mover = new MoverState[capacity];
            SalvagePile = new SalvageState[capacity];
            Decoy = new DecoyState[capacity];
            Mine = new MineState[capacity];
            Nav = new NavState[capacity];
            TetherId = new int[capacity];
            HasThermalBlanket = new bool[capacity];

            for (int i = 0; i < capacity; i++) generation[i] = 1;
        }

        public int AliveCount { get; private set; }

        /// <summary>
        /// Allocate a slot. Reuse comes off the tail of the free list, which keeps
        /// allocation order a pure function of the sequence of creates and
        /// destroys - the property the whole lockstep design rests on.
        /// </summary>
        public EntityHandle Create()
        {
            int index;
            if (freeCount > 0)
            {
                index = freeList[--freeCount];
            }
            else
            {
                if (highWater >= Capacity)
                    throw new InvalidOperationException("Entity capacity exhausted (" + Capacity + ")");
                index = highWater++;
            }

            alive[index] = true;
            Mask[index] = ComponentMask.None;
            HasThermalBlanket[index] = false;
            TetherId[index] = -1;
            Rank[index] = 1;
            AliveCount++;
            return new EntityHandle(index, generation[index]);
        }

        public void Destroy(EntityHandle h)
        {
            if (!IsAlive(h)) return;
            int i = h.Index;
            alive[i] = false;
            Mask[i] = ComponentMask.None;
            generation[i] = (generation[i] + 1) & (int)EntityHandle.GenerationMask;
            if (generation[i] == 0) generation[i] = 1;
            freeList[freeCount++] = i;
            AliveCount--;
        }

        public bool IsAlive(EntityHandle h)
        {
            if (h.IsNone) return false;
            int i = h.Index;
            return i > 0 && i < Capacity && alive[i] && generation[i] == h.Generation;
        }

        public bool IsSlotAlive(int index) { return index > 0 && index < Capacity && alive[index]; }

        public EntityHandle HandleAt(int index) { return new EntityHandle(index, generation[index]); }

        /// <summary>One past the highest slot ever used. Iterate [1, HighWater).</summary>
        public int HighWater { get { return highWater; } }

        public void AddComponent(int index, ComponentMask c) { Mask[index] |= c; }
        public bool Has(int index, ComponentMask c) { return (Mask[index] & c) == c; }
        public bool HasAny(int index, ComponentMask c) { return (Mask[index] & c) != 0; }
    }
}
