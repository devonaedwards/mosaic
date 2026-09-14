using System;

namespace KZ.Sim
{
    [Flags]
    public enum ComponentMask : uint
    {
        None = 0,
        Sensor = 1u << 0,
        Producer = 1u << 1   // DEAD: set at spawn and tested by nothing
    }

    public sealed class EntityTable
    {
        public readonly ComponentMask[] Mask = new ComponentMask[256];

        public void AddComponent(int index, ComponentMask c) { Mask[index] |= c; }
        public bool Has(int index, ComponentMask c) { return (Mask[index] & c) == c; }
    }
}
