// KILL ZONE - a real-time strategy video game.
// Unit and structure definitions - the stat blocks.
//
// These are balance numbers for a game. They are written here in code for now so
// the simulation can be tested end to end; they will move out to data files that
// designers and modders can edit without a compiler, and that get hashed into
// the multiplayer handshake so two players cannot unknowingly play with
// different stat blocks.

using System.Collections.Generic;

namespace KZ.Sim
{
    public sealed class UnitDef
    {
        public int Id;
        public string Name;
        public FactionId Faction = FactionId.Neutral; // Neutral means both sides can build it
        public int Tier = 1;

        public int CostMateriel;
        public int BuildTicks;

        public Fix Hp;
        public ArmourClass Armour;
        public Layer Layer = Layer.Ground;

        public Fix SpeedMetresPerSecond;
        public int TurnRateDegreesPerSecond = 180;

        public LinkKind Link = LinkKind.None;
        public LinkKind AltLink = LinkKind.None;
        public byte LinkRobustness;
        public BlackPolicy BlackPolicy = BlackPolicy.Abort;
        public Fix SpoolLengthMetres;         // fiber only

        /// <summary>Whether flying this consumes one of the player's scarce crews.</summary>
        public bool ConsumesCrew;

        /// <summary>The airframe is the munition: it does not come home.</summary>
        public bool OneWay;

        public Fix WeaponDamage;
        public DamageType WeaponType = DamageType.Fragmentation;
        public Fix WeaponRangeMetres;
        public int WeaponCooldownTicks = 32;
        public int WeaponAcquisitionTicks = 16;
        public bool IsInterceptor;
        public Fix InterceptBaseChance;

        /// <summary>
        /// Whether this can shoot at something in the air at all.
        ///
        /// False for almost everything, deliberately. The setting's whole argument
        /// is that a cheap drone is a credible threat to an expensive vehicle, and
        /// that collapses the moment a tank can swat drones with its main gun. Air
        /// defence is interceptor drones, interceptor batteries and gun mounts.
        /// Nothing else.
        /// </summary>
        public bool CanEngageAir;

        public Fix SensorFootprintMetres;
        public bool SensorOptical = true;
        public bool SensorThermal;
        public bool SensorRadioFrequency;

        public byte JamStrength;
        public Fix JamRadiusMetres;

        public byte AutonomyQuality;

        public bool IsStructure;
        public int FootprintTiles = 1;

        /// <summary>How much a jammer or radar gives away by being switched on.</summary>
        public byte SignatureWhileEmitting = 85;

        public bool IsMeshRepeater;

        /// <summary>
        /// Cannot fly in daylight at all. Heavy multirotors are slow, loud and
        /// enormous; in daylight they are simply targets. Restricting them to
        /// darkness is what gives the day and night cycle teeth, and what makes the
        /// player who has bought thermal imaging feel it.
        /// </summary>
        public bool NightOnly;

        /// <summary>How many mines one sortie can lay, if any.</summary>
        public int MinesCarried;
        public Fix MineDamage;

        public override string ToString() { return Name + "#" + Id; }
    }

    public static class Catalog
    {
        static readonly List<UnitDef> defs = new List<UnitDef>();
        static readonly Dictionary<string, int> byName = new Dictionary<string, int>();

        public static int Count { get { return defs.Count; } }
        public static UnitDef Get(int id) { return defs[id]; }

        public static UnitDef ByName(string name)
        {
            int id;
            if (!byName.TryGetValue(name, out id)) return null;
            return defs[id];
        }

        public static int IdOf(string name)
        {
            int id;
            return byName.TryGetValue(name, out id) ? id : -1;
        }

        static UnitDef Add(UnitDef d)
        {
            d.Id = defs.Count;
            defs.Add(d);
            byName[d.Name] = d.Id;
            return d;
        }

        static Fix M(double v) { return Fix.FromDoubleContentOnly(v); }

        static Catalog()
        {
            BuildStructures();
            BuildGroundUnits();
            BuildAirUnits();
        }

        static void BuildStructures()
        {
            Add(new UnitDef
            {
                Name = "Command Post", IsStructure = true, Tier = 1,
                CostMateriel = 2000, BuildTicks = SimConstants.Seconds(60),
                Hp = M(5000), Armour = ArmourClass.Structure, FootprintTiles = 8,
                SensorFootprintMetres = M(300)
            });

            Add(new UnitDef
            {
                Name = "Crew Quarters", IsStructure = true, Tier = 1,
                CostMateriel = 700, BuildTicks = SimConstants.Seconds(22),
                Hp = M(1400), Armour = ArmourClass.Structure, FootprintTiles = 4
            });

            Add(new UnitDef
            {
                Name = "Drone Workshop", IsStructure = true, Tier = 1,
                CostMateriel = 900, BuildTicks = SimConstants.Seconds(28),
                Hp = M(1600), Armour = ArmourClass.Structure, FootprintTiles = 5
            });

            Add(new UnitDef
            {
                Name = "Radar Mast", IsStructure = true, Tier = 2,
                CostMateriel = 1000, BuildTicks = SimConstants.Seconds(30),
                Hp = M(900), Armour = ArmourClass.Structure, FootprintTiles = 3,
                SensorFootprintMetres = M(1400), SensorRadioFrequency = true
            });

            Add(new UnitDef
            {
                Name = "Spool Plant", IsStructure = true, Tier = 2,
                CostMateriel = 800, BuildTicks = SimConstants.Seconds(26),
                Hp = M(1200), Armour = ArmourClass.Structure, FootprintTiles = 4
            });

            // A jammer is loud. Switching it on paints a permanent mark on the
            // enemy's map, so it is a posture you must defend, not a wall.
            Add(new UnitDef
            {
                Name = "EW Post", IsStructure = true, Tier = 2,
                CostMateriel = 750, BuildTicks = SimConstants.Seconds(24),
                Hp = M(1100), Armour = ArmourClass.Structure, FootprintTiles = 3,
                JamStrength = 70, JamRadiusMetres = M(450)
            });

            Add(new UnitDef
            {
                Name = "Relay Mast", IsStructure = true, Tier = 2,
                CostMateriel = 500, BuildTicks = SimConstants.Seconds(18),
                Hp = M(600), Armour = ArmourClass.Structure, FootprintTiles = 2
            });

            Add(new UnitDef
            {
                Name = "Gun Mount", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.Seconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(550), WeaponCooldownTicks = 24,
                SensorFootprintMetres = M(600)
            });

            Add(new UnitDef
            {
                Name = "Uplink Terminal", IsStructure = true, Faction = FactionId.KestrelPact, Tier = 3,
                CostMateriel = 2200, BuildTicks = SimConstants.Seconds(50),
                Hp = M(1800), Armour = ArmourClass.Structure, FootprintTiles = 5
            });

            Add(new UnitDef
            {
                Name = "Autonomy Lab", IsStructure = true, Faction = FactionId.ObsidianDirectorate, Tier = 3,
                CostMateriel = 2000, BuildTicks = SimConstants.Seconds(48),
                Hp = M(1700), Armour = ArmourClass.Structure, FootprintTiles = 5
            });
        }

        static void BuildGroundUnits()
        {
            Add(new UnitDef
            {
                Name = "Recovery UGV", Tier = 1,
                CostMateriel = 500, BuildTicks = SimConstants.Seconds(18),
                Hp = M(420), Armour = ArmourClass.Light, SpeedMetresPerSecond = M(5.5),
                SensorFootprintMetres = M(180)
            });

            Add(new UnitDef
            {
                Name = "Net Engineer", Tier = 1,
                CostMateriel = 250, BuildTicks = SimConstants.Seconds(12),
                Hp = M(220), Armour = ArmourClass.Soft, SpeedMetresPerSecond = M(4.5),
                SensorFootprintMetres = M(200)
            });

            Add(new UnitDef
            {
                Name = "Motorcycle Squad", Tier = 1,
                CostMateriel = 300, BuildTicks = SimConstants.Seconds(12),
                Hp = M(260), Armour = ArmourClass.Soft, SpeedMetresPerSecond = M(17.0),
                WeaponDamage = M(45), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(160), SensorFootprintMetres = M(220)
            });

            Add(new UnitDef
            {
                Name = "Supply Truck", Tier = 1,
                CostMateriel = 350, BuildTicks = SimConstants.Seconds(14),
                Hp = M(520), Armour = ArmourClass.Light, SpeedMetresPerSecond = M(14.0),
                SensorFootprintMetres = M(160)
            });

            // A ground robot keeps flying its link, so an electronic-warfare bubble
            // does not kill it - it simply stops dead in the open, which is often
            // worse.
            Add(new UnitDef
            {
                Name = "Logistics UGV", Tier = 2,
                CostMateriel = 400, BuildTicks = SimConstants.Seconds(16),
                Hp = M(500), Armour = ArmourClass.Light, SpeedMetresPerSecond = M(5.0),
                Link = LinkKind.Radio, LinkRobustness = 40,
                SensorFootprintMetres = M(160)
            });

            Add(new UnitDef
            {
                Name = "EW Truck", Tier = 2,
                CostMateriel = 700, BuildTicks = SimConstants.Seconds(24),
                Hp = M(380), Armour = ArmourClass.Light, SpeedMetresPerSecond = M(8.0),
                JamStrength = 55, JamRadiusMetres = M(350),
                SensorFootprintMetres = M(200)
            });

            Add(new UnitDef
            {
                Name = "IFV", Tier = 2,
                CostMateriel = 900, BuildTicks = SimConstants.Seconds(26),
                Hp = M(1250), Armour = ArmourClass.Heavy, SpeedMetresPerSecond = M(9.0),
                WeaponDamage = M(130), WeaponType = DamageType.Kinetic,
                WeaponRangeMetres = M(420), SensorFootprintMetres = M(320)
            });

            // Expensive, powerful, and only survivable with jamming cover, cages,
            // and the dark. A tank on open ground in daylight is a salvage pile.
            Add(new UnitDef
            {
                Name = "Main Tank", Tier = 2,
                CostMateriel = 1600, BuildTicks = SimConstants.Seconds(40),
                // 2250 rather than the 2300 in the design document, which does not
                // quite deliver its own stated intent: a shaped charge into the top
                // plate does 572, and four of those come to 2288, so at 2300 the
                // fourth drone leaves the tank alive on twelve hit points. The
                // intent - four drones kill a tank and three do not - is the thing
                // worth preserving; the arithmetic slip is not.
                Hp = M(2250), Armour = ArmourClass.Heavy, SpeedMetresPerSecond = M(7.5),
                WeaponDamage = M(340), WeaponType = DamageType.Kinetic,
                WeaponRangeMetres = M(620), SensorFootprintMetres = M(360)
            });

            Add(new UnitDef
            {
                Name = "Designator Team", Faction = FactionId.KestrelPact, Tier = 2,
                CostMateriel = 280, BuildTicks = SimConstants.Seconds(12),
                Hp = M(180), Armour = ArmourClass.Soft, SpeedMetresPerSecond = M(4.0),
                Link = LinkKind.Satellite, LinkRobustness = 95,
                SensorFootprintMetres = M(600)
            });

            // Crew-free air defence. It costs no sortie capacity, which is the
            // whole reason its faction can hold a rear area without grounding its
            // strike wing.
            Add(new UnitDef
            {
                Name = "Interceptor Battery", Faction = FactionId.KestrelPact, Tier = 3,
                CanEngageAir = true,
                CostMateriel = 1200, BuildTicks = SimConstants.Seconds(32),
                Hp = M(600), Armour = ArmourClass.Light, SpeedMetresPerSecond = M(6.0),
                WeaponDamage = M(220), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(2000), WeaponCooldownTicks = 96,
                IsInterceptor = true, InterceptBaseChance = M(0.75),
                SensorFootprintMetres = M(400)
            });
        }

        static void BuildAirUnits()
        {
            Add(new UnitDef
            {
                Name = "Scout Quad", Tier = 1,
                CostMateriel = 120, BuildTicks = SimConstants.Seconds(6),
                Hp = M(40), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(16.0),
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true,
                SensorFootprintMetres = M(250), IsMeshRepeater = true
            });

            // The workhorse: ammunition with a pilot. It does not come home.
            Add(new UnitDef
            {
                Name = "FPV Team", Tier = 1,
                CostMateriel = 200, BuildTicks = SimConstants.Seconds(8),
                Hp = M(55), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(22.0),
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(260), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(8), WeaponAcquisitionTicks = 12,
                SensorFootprintMetres = M(140), IsMeshRepeater = true
            });

            // Unjammable, and leashed for it. Slower, less agile, and trailing a
            // thread an enemy can find and follow back to the operator.
            Add(new UnitDef
            {
                Name = "Fiber FPV Team", Tier = 2,
                CostMateriel = 420, BuildTicks = SimConstants.Seconds(12),
                Hp = M(70), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(15.0), TurnRateDegreesPerSecond = 140,
                Link = LinkKind.Fiber, LinkRobustness = SimConstants.UnjammableRobustness,
                SpoolLengthMetres = M(1400), ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(340), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(8), WeaponAcquisitionTicks = 12,
                SensorFootprintMetres = M(220)
            });

            Add(new UnitDef
            {
                Name = "Multirole Quad", Tier = 2,
                CostMateriel = 380, BuildTicks = SimConstants.Seconds(14),
                Hp = M(110), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(19.0),
                Link = LinkKind.Radio, AltLink = LinkKind.Mesh, LinkRobustness = 45,
                ConsumesCrew = true,
                WeaponDamage = M(180), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(40), SensorFootprintMetres = M(260),
                IsMeshRepeater = true
            });

            // Fast and cheap, but nearly useless without a radar telling it where
            // to look. Killing the radar is how you open the sky.
            Add(new UnitDef
            {
                Name = "Interceptor FPV", Tier = 2, CanEngageAir = true,
                CostMateriel = 300, BuildTicks = SimConstants.Seconds(10),
                Hp = M(60), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(34.0),
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(0), WeaponType = DamageType.Ram,
                WeaponRangeMetres = M(12), IsInterceptor = true, InterceptBaseChance = M(0.55),
                SensorFootprintMetres = M(180), IsMeshRepeater = true
            });

            // The eyes. Nothing on the map is worth shooting at until one of these
            // has seen it, so losing them does not slow an army down - it turns its
            // lights off.
            Add(new UnitDef
            {
                Name = "Recon Wing", Tier = 2,
                CostMateriel = 900, BuildTicks = SimConstants.Seconds(30),
                Hp = M(200), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(12.0), TurnRateDegreesPerSecond = 60,
                Link = LinkKind.Radio, AltLink = LinkKind.Mesh, LinkRobustness = 45,
                ConsumesCrew = true,
                SensorFootprintMetres = M(900), IsMeshRepeater = true
            });

            Add(new UnitDef
            {
                Name = "Night Bomber", Tier = 2, NightOnly = true,
                MinesCarried = 4, MineDamage = M(600),
                CostMateriel = 1100, BuildTicks = SimConstants.Seconds(34),
                Hp = M(480), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(8.0), TurnRateDegreesPerSecond = 90,
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true,
                WeaponDamage = M(300), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(30), WeaponCooldownTicks = 96,
                SensorFootprintMetres = M(300), SensorThermal = true
            });

            Add(new UnitDef
            {
                Name = "Loitering Munition", Tier = 2,
                CostMateriel = 550, BuildTicks = SimConstants.Seconds(16),
                Hp = M(90), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(24.0), TurnRateDegreesPerSecond = 70,
                Link = LinkKind.Mesh, LinkRobustness = 65, ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(300), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(10), SensorFootprintMetres = M(240)
            });

            // Navigates by looking at the ground rather than by listening to a
            // radio, so no jammer touches it. It picks its own target at the end of
            // the run, which is exactly what a field of decoys exploits.
            Add(new UnitDef
            {
                Name = "Mid-Range Striker", Faction = FactionId.KestrelPact, Tier = 3,
                CostMateriel = 700, BuildTicks = SimConstants.Seconds(20),
                Hp = M(120), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(28.0), TurnRateDegreesPerSecond = 60,
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true, AutonomyQuality = 72,
                WeaponDamage = M(420), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(10), SensorFootprintMetres = M(200)
            });

            // A relay in the sky. Extends reach, not capacity - every drone it
            // carries still needs its own crew. Kill it and everything hanging off
            // it drops at once.
            Add(new UnitDef
            {
                Name = "Mothership", Faction = FactionId.ObsidianDirectorate, Tier = 3,
                CostMateriel = 1400, BuildTicks = SimConstants.Seconds(38),
                Hp = M(520), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(13.0), TurnRateDegreesPerSecond = 50,
                Link = LinkKind.Mesh, LinkRobustness = 65, ConsumesCrew = true,
                SensorFootprintMetres = M(400), IsMeshRepeater = true
            });

            Add(new UnitDef
            {
                Name = "Autonomous Munition", Faction = FactionId.ObsidianDirectorate, Tier = 3,
                CostMateriel = 620, BuildTicks = SimConstants.Seconds(18),
                Hp = M(110), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(22.0), TurnRateDegreesPerSecond = 70,
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true, AutonomyQuality = 55,
                WeaponDamage = M(320), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(10), SensorFootprintMetres = M(220)
            });
        }

        /// <summary>
        /// How much damage of one type does to one armour class.
        ///
        /// The headline number is shaped charge against the top of a vehicle: a
        /// rotary drone diving on a tank hits the thinnest plate it has, which is
        /// why a two-hundred-Materiel drone is a credible threat to a
        /// sixteen-hundred-Materiel tank. Four hits kill it and three do not, and
        /// that margin is the single most important balance number in the game.
        /// </summary>
        public static Fix DamageMultiplier(DamageType type, ArmourClass armour, bool topAttack)
        {
            if (type == DamageType.Ram)
                return (armour == ArmourClass.AirRotary || armour == ArmourClass.AirFixed) ? Fix.One : Fix.Zero;

            if (topAttack && armour == ArmourClass.Heavy)
            {
                switch (type)
                {
                    case DamageType.Shaped: return M(2.20);
                    case DamageType.Fragmentation: return M(0.35);
                    case DamageType.Kinetic: return M(1.25);
                    case DamageType.Incendiary: return M(0.40);
                }
            }

            switch (type)
            {
                case DamageType.Shaped:
                    switch (armour)
                    {
                        case ArmourClass.Soft: return M(0.45);
                        case ArmourClass.Light: return M(1.30);
                        case ArmourClass.Heavy: return M(1.10);
                        case ArmourClass.Structure: return M(0.90);
                        default: return M(0.30);
                    }
                case DamageType.Fragmentation:
                    switch (armour)
                    {
                        case ArmourClass.Soft: return M(1.60);
                        case ArmourClass.Light: return M(0.75);
                        case ArmourClass.Heavy: return M(0.20);
                        case ArmourClass.Structure: return M(0.30);
                        case ArmourClass.AirRotary: return M(1.40);
                        default: return M(1.20);
                    }
                case DamageType.Kinetic:
                    switch (armour)
                    {
                        case ArmourClass.Soft: return M(0.90);
                        case ArmourClass.Light: return M(1.45);
                        case ArmourClass.Heavy: return M(1.25);
                        case ArmourClass.Structure: return M(0.65);
                        case ArmourClass.AirRotary: return M(0.25);
                        default: return M(0.20);
                    }
                case DamageType.Incendiary:
                    switch (armour)
                    {
                        case ArmourClass.Soft: return M(1.30);
                        case ArmourClass.Light: return M(0.60);
                        case ArmourClass.Heavy: return M(0.25);
                        case ArmourClass.Structure: return M(1.70);
                        case ArmourClass.AirRotary: return M(0.15);
                        default: return M(0.10);
                    }
            }
            return Fix.One;
        }

        /// <summary>How convincing a candidate looks to a machine deciding what to hit.</summary>
        public static Fix BasePlausibility(TargetKind kind)
        {
            switch (kind)
            {
                case TargetKind.HighValue: return Fix.One;
                case TargetKind.LowValue: return M(0.60);
                case TargetKind.Decoy: return M(0.90);
                case TargetKind.Friendly: return M(0.25);
                default: return M(0.15);
            }
        }
    }
}
