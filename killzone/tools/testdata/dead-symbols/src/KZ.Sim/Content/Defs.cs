using System.Collections.Generic;

namespace KZ.Sim
{
    public sealed class UnitDef
    {
        public string Name;

        /// <summary>DEAD: written by every def, read nowhere outside this file.</summary>
        public int Tier = 1;

        /// <summary>DEFAULTED: read by the link code, set by no unit at all.</summary>
        public BlackPolicy BlackPolicy = BlackPolicy.Abort;

        /// <summary>SINGLE CARRIER: the Gun Mount shape from audit F4.</summary>
        public int TraverseDegreesPerSecond;

        /// <summary>SINGLE CARRIER: the magazine, likewise.</summary>
        public int AmmoCapacity;

        /// <summary>HARNESS ONLY: set by a Test unit and by no unit the game fields.</summary>
        public int ReloadSeconds;

        public int WeaponRangeMetres;
    }

    public static class Catalog
    {
        static readonly List<UnitDef> defs = new List<UnitDef>();

        public static UnitDef Get(int id) { return defs[id]; }

        static void Add(UnitDef d) { defs.Add(d); }

        static Catalog()
        {
            Add(new UnitDef
            {
                Name = "Gun Mount", Tier = 2,
                WeaponRangeMetres = 85
            });

            Add(new UnitDef
            {
                Name = "Interceptor Battery", Tier = 3,
                TraverseDegreesPerSecond = 45, AmmoCapacity = 12,
                WeaponRangeMetres = 320
            });

            // A harness fixture, not a unit the game fields. It must not count as
            // a carrier: that is the whole rule, applied to the catalogue.
            Add(new UnitDef
            {
                Name = "Test Long Mount", Tier = 2,
                ReloadSeconds = 20,
                WeaponRangeMetres = 550
            });
        }
    }
}
