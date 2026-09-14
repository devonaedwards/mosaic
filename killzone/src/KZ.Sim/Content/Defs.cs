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

        /// <summary>
        /// What this airframe can work out about its own position when nothing is
        /// telling it. Dead reckoning by default, because that is what cheap
        /// airframes have, and it is the default that makes a border matter.
        /// </summary>
        public NavAid NavAid;

        /// <summary>
        /// A star tracker. Bounds heading drift rather than fixing position, and
        /// costs a quarter of a million, so it belongs on almost nothing.
        /// </summary>
        public bool HasCelestialHeading;
        public Fix InterceptBaseChance;

        /// <summary>Degrees per second the mount can traverse. Zero means instant.</summary>
        public int TraverseDegreesPerSecond;

        public AmmoType Ammo = AmmoType.Solid;

        /// <summary>
        /// How high it can reach, in the game's three bands. A machine gun cannot
        /// touch something cruising at two and a half kilometres, and that - not
        /// speed - is what took the gun trucks out of the Shahed business.
        /// </summary>
        public bool CanReachHigh = true;

        /// <summary>Engagements before reloading. Zero means it never runs dry.</summary>
        public int AmmoCapacity;
        public int ReloadSeconds = 8;

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

        // What it can find things with. Zero means the sensor is not fitted, and
        // which ones are missing is usually more interesting than which are there.
        public Fix SensorOptical;
        public Fix SensorThermal;
        public Fix SensorAcoustic;
        public Fix SensorRadar;
        public Fix SensorEsm;

        // What it gives away, on each channel, 0-100.
        public byte SigRadio;
        public byte SigThermal;
        public byte SigAcoustic;
        public byte SigVisual;
        public byte SigRadar;

        /// <summary>
        /// Arc the pointed sensors cover, in degrees, and how fast the head sweeps.
        /// A narrow staring head is the long-range choice with blind sides; a wide
        /// one covers everything badly; a sweeping one covers everything
        /// intermittently. Zero arc means omnidirectional.
        /// </summary>
        public int SensorArcDegrees;
        public int SensorScanDegreesPerSecond;

        public byte JamStrength;
        public Fix JamRadiusMetres;

        public byte AutonomyQuality;

        public bool IsStructure;
        public int FootprintTiles = 1;

        // A note on thermal signatures, because the numbers look wrong at a glance.
        // An electric quadcopter runs its motors at forty to eighty degrees and its
        // battery at sixty; a combustion engine runs at four hundred to eight
        // hundred, and a turbojet hotter still. That is not a small difference in
        // degree, it is the difference between a thing a heat sensor can find and a
        // thing it mostly cannot - which is why the cheap electric drones are the
        // hard ones to see and the expensive strike drones are the easy ones.

        /// <summary>How much a jammer or radar gives away by being switched on.</summary>
        public byte SignatureWhileEmitting = 85;

        public bool IsMeshRepeater;

        /// <summary>
        /// Carries no warhead and exists to be shot at. Unlike the inflatable on
        /// the ground, this one is a valid target for everything - looking like the
        /// real thing on radar is precisely the product.
        /// </summary>
        public bool IsFlyingDecoy;

        /// <summary>Can trade altitude for cover, which is a real decision.</summary>
        public bool CanChangeAltitude;

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
                SensorOptical = M(400), SensorEsm = M(500),
                SigRadio = 80, SigThermal = 60, SigAcoustic = 25, SigVisual = 95,
                SensorArcDegrees = 360});

            Add(new UnitDef
            {
                Name = "Crew Quarters", IsStructure = true, Tier = 1,
                CostMateriel = 700, BuildTicks = SimConstants.Seconds(22),
                Hp = M(1400), Armour = ArmourClass.Structure, FootprintTiles = 4,
                SigRadio = 55, SigThermal = 40, SigAcoustic = 20, SigVisual = 75
            });

            Add(new UnitDef
            {
                Name = "Drone Workshop", IsStructure = true, Tier = 1,
                CostMateriel = 900, BuildTicks = SimConstants.Seconds(28),
                Hp = M(1600), Armour = ArmourClass.Structure, FootprintTiles = 5,
                SigRadio = 40, SigThermal = 45, SigAcoustic = 35, SigVisual = 80
            });

            Add(new UnitDef
            {
                Name = "Radar Mast", IsStructure = true, Tier = 2,
                CostMateriel = 1000, BuildTicks = SimConstants.Seconds(30),
                Hp = M(900), Armour = ArmourClass.Structure, FootprintTiles = 3,
                SensorRadar = M(1400), SensorEsm = M(900),
                SigRadio = 25, SigThermal = 25, SigAcoustic = 15, SigVisual = 70
            });

            Add(new UnitDef
            {
                Name = "Spool Plant", IsStructure = true, Tier = 2,
                CostMateriel = 800, BuildTicks = SimConstants.Seconds(26),
                Hp = M(1200), Armour = ArmourClass.Structure, FootprintTiles = 4,
                SigRadio = 20, SigThermal = 35, SigAcoustic = 25, SigVisual = 75
            });

            // A jammer is loud. Switching it on paints a permanent mark on the
            // enemy's map, so it is a posture you must defend, not a wall.
            Add(new UnitDef
            {
                Name = "EW Post", IsStructure = true, Tier = 2,
                CostMateriel = 750, BuildTicks = SimConstants.Seconds(24),
                Hp = M(1100), Armour = ArmourClass.Structure, FootprintTiles = 3,
                JamStrength = 70, JamRadiusMetres = M(450),
                SigRadio = 25, SigThermal = 40, SigAcoustic = 20, SigVisual = 70
            });

            Add(new UnitDef
            {
                Name = "Relay Mast", IsStructure = true, Tier = 2,
                CostMateriel = 500, BuildTicks = SimConstants.Seconds(18),
                Hp = M(600), Armour = ArmourClass.Structure, FootprintTiles = 2,
                SigRadio = 70, SigThermal = 15, SigAcoustic = 10, SigVisual = 55
            });

            Add(new UnitDef
            {
                Name = "Gun Mount", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.Seconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                // Eighty-five metres, not the five hundred and fifty this had.
                // The old figure was the mount's *detection* reach being used as
                // its kill ring, and the two are an order of magnitude apart: a
                // heavy machine gun's useful engagement envelope against a small
                // drone is a few hundred real metres, which is tens of metres on
                // this map. Getting that wrong was most of why a single turret
                // looked unbeatable.
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(85), WeaponCooldownTicks = 24,
                CanReachHigh = false,
                SensorOptical = M(600), SensorAcoustic = M(200),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 120, SensorScanDegreesPerSecond = 70});

            Add(new UnitDef
            {
                Name = "Uplink Terminal", IsStructure = true, Faction = FactionId.KestrelPact, Tier = 3,
                CostMateriel = 2200, BuildTicks = SimConstants.Seconds(50),
                Hp = M(1800), Armour = ArmourClass.Structure, FootprintTiles = 5,
                SigRadio = 85, SigThermal = 40, SigAcoustic = 20, SigVisual = 80
            });

            Add(new UnitDef
            {
                Name = "Autonomy Lab", IsStructure = true, Faction = FactionId.ObsidianDirectorate, Tier = 3,
                CostMateriel = 2000, BuildTicks = SimConstants.Seconds(48),
                Hp = M(1700), Armour = ArmourClass.Structure, FootprintTiles = 5,
                SigRadio = 30, SigThermal = 45, SigAcoustic = 30, SigVisual = 80
            });
        }

        static void BuildGroundUnits()
        {
            Add(new UnitDef
            {
                Name = "Recovery UGV", Tier = 1,
                CostMateriel = 500, BuildTicks = SimConstants.Seconds(18),
                Hp = M(420), Armour = ArmourClass.Light, SpeedMetresPerSecond = M(5.5),
                SensorOptical = M(180),
                SigRadio = 0, SigThermal = 40, SigAcoustic = 45, SigVisual = 55
            });

            Add(new UnitDef
            {
                Name = "Net Engineer", Tier = 1,
                CostMateriel = 250, BuildTicks = SimConstants.Seconds(12),
                Hp = M(220), Armour = ArmourClass.Soft, SpeedMetresPerSecond = M(4.5),
                SensorOptical = M(200),
                SigRadio = 0, SigThermal = 22, SigAcoustic = 15, SigVisual = 20
            });

            Add(new UnitDef
            {
                Name = "Motorcycle Squad", Tier = 1,
                CostMateriel = 300, BuildTicks = SimConstants.Seconds(12),
                Hp = M(260), Armour = ArmourClass.Soft, SpeedMetresPerSecond = M(17.0),
                WeaponDamage = M(45), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(160),
                SensorOptical = M(220),
                SigRadio = 0, SigThermal = 35, SigAcoustic = 60, SigVisual = 30
            });

            Add(new UnitDef
            {
                Name = "Supply Truck", Tier = 1,
                CostMateriel = 350, BuildTicks = SimConstants.Seconds(14),
                Hp = M(520), Armour = ArmourClass.Light, SpeedMetresPerSecond = M(14.0),
                SensorOptical = M(160),
                SigRadio = 0, SigThermal = 55, SigAcoustic = 60, SigVisual = 70
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
                SensorOptical = M(160),
                SigRadio = 45, SigThermal = 35, SigAcoustic = 40, SigVisual = 50
            });

            Add(new UnitDef
            {
                Name = "EW Truck", Tier = 2,
                CostMateriel = 700, BuildTicks = SimConstants.Seconds(24),
                Hp = M(380), Armour = ArmourClass.Light, SpeedMetresPerSecond = M(8.0),
                JamStrength = 55, JamRadiusMetres = M(350),
                SensorOptical = M(200), SensorEsm = M(400),
                SigRadio = 30, SigThermal = 45, SigAcoustic = 45, SigVisual = 55
            });

            Add(new UnitDef
            {
                Name = "IFV", Tier = 2,
                CostMateriel = 900, BuildTicks = SimConstants.Seconds(26),
                Hp = M(1250), Armour = ArmourClass.Heavy, SpeedMetresPerSecond = M(9.0),
                WeaponDamage = M(130), WeaponType = DamageType.Kinetic,
                WeaponRangeMetres = M(420),
                SensorOptical = M(320), SensorThermal = M(250),
                SigRadio = 0, SigThermal = 75, SigAcoustic = 75, SigVisual = 80,
                SensorArcDegrees = 90, SensorScanDegreesPerSecond = 30});

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
                WeaponRangeMetres = M(620),
                SensorOptical = M(360), SensorThermal = M(300),
                SigRadio = 0, SigThermal = 90, SigAcoustic = 80, SigVisual = 90,
                SensorArcDegrees = 90, SensorScanDegreesPerSecond = 25});

            Add(new UnitDef
            {
                Name = "Designator Team", Faction = FactionId.KestrelPact, Tier = 2,
                CostMateriel = 280, BuildTicks = SimConstants.Seconds(12),
                Hp = M(180), Armour = ArmourClass.Soft, SpeedMetresPerSecond = M(4.0),
                Link = LinkKind.Satellite, LinkRobustness = 95,
                SensorOptical = M(600), SensorThermal = M(300),
                SigRadio = 40, SigThermal = 22, SigAcoustic = 15, SigVisual = 20,
                SensorArcDegrees = 30});

            // Crew-free air defence. It costs no sortie capacity, which is the
            // whole reason its faction can hold a rear area without grounding its
            // strike wing.
            Add(new UnitDef
            {
                Name = "Interceptor Battery", Faction = FactionId.KestrelPact, Tier = 3,
                CanEngageAir = true, TraverseDegreesPerSecond = 45,
                Ammo = AmmoType.Proximity, AmmoCapacity = 12, ReloadSeconds = 20,
                CostMateriel = 1200, BuildTicks = SimConstants.Seconds(32),
                Hp = M(600), Armour = ArmourClass.Light, SpeedMetresPerSecond = M(6.0),
                WeaponDamage = M(220), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(320), WeaponCooldownTicks = 96,
                IsInterceptor = true, InterceptBaseChance = M(0.75),
                SensorThermal = M(500), SensorRadar = M(800), SensorEsm = M(600),
                SigRadio = 70, SigThermal = 50, SigAcoustic = 40, SigVisual = 60,
                SensorArcDegrees = 360});
        }

        static void BuildAirUnits()
        {
            Add(new UnitDef
            {
                Name = "Scout Quad", CanChangeAltitude = true, Tier = 1,
                CostMateriel = 120, BuildTicks = SimConstants.Seconds(6),
                Hp = M(40), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(16.0),
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true, IsMeshRepeater = true,
                SensorOptical = M(250),
                SigRadio = 60, SigThermal = 8, SigAcoustic = 12, SigVisual = 12, SigRadar = 22,
                SensorArcDegrees = 180});

            // The workhorse: ammunition with a pilot. It does not come home.
            Add(new UnitDef
            {
                Name = "FPV Team", CanChangeAltitude = true, Tier = 1,
                CostMateriel = 200, BuildTicks = SimConstants.Seconds(8),
                Hp = M(55), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(22.0),
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(260), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(8), WeaponAcquisitionTicks = 12, IsMeshRepeater = true,
                SensorOptical = M(140),
                SigRadio = 70, SigThermal = 8, SigAcoustic = 12, SigVisual = 15, SigRadar = 22
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
                SensorOptical = M(220),
                SigRadio = 0, SigThermal = 8, SigAcoustic = 12, SigVisual = 15, SigRadar = 22
            });

            Add(new UnitDef
            {
                Name = "Multirole Quad", CanChangeAltitude = true, Tier = 2,
                CostMateriel = 380, BuildTicks = SimConstants.Seconds(14),
                Hp = M(110), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(19.0),
                Link = LinkKind.Radio, AltLink = LinkKind.Mesh, LinkRobustness = 45,
                ConsumesCrew = true,
                WeaponDamage = M(180), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(40),
                IsMeshRepeater = true,
                SensorOptical = M(260),
                SigRadio = 65, SigThermal = 10, SigAcoustic = 14, SigVisual = 20, SigRadar = 26,
                SensorArcDegrees = 120});

            // Fast and cheap, but nearly useless without a radar telling it where
            // to look. Killing the radar is how you open the sky.
            Add(new UnitDef
            {
                Name = "Interceptor FPV", CanChangeAltitude = true, Tier = 2, CanEngageAir = true,
                CostMateriel = 300, BuildTicks = SimConstants.Seconds(10),
                Hp = M(60), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(34.0),
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(0), WeaponType = DamageType.Ram,
                WeaponRangeMetres = M(12), IsInterceptor = true, InterceptBaseChance = M(0.55), IsMeshRepeater = true,
                SensorOptical = M(180),
                SigRadio = 70, SigThermal = 9, SigAcoustic = 15, SigVisual = 15, SigRadar = 22
            });

            // The eyes. Nothing on the map is worth shooting at until one of these
            // has seen it, so losing them does not slow an army down - it turns its
            // lights off.
            Add(new UnitDef
            {
                Name = "Recon Wing", Tier = 2, NavAid = NavAid.SceneMatching,
                CostMateriel = 900, BuildTicks = SimConstants.Seconds(30),
                Hp = M(200), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(12.0), TurnRateDegreesPerSecond = 60,
                Link = LinkKind.Radio, AltLink = LinkKind.Mesh, LinkRobustness = 45,
                ConsumesCrew = true, IsMeshRepeater = true,
                SensorOptical = M(900),
                SigRadio = 55, SigThermal = 25, SigAcoustic = 18, SigVisual = 30, SigRadar = 40,
                SensorArcDegrees = 45});

            Add(new UnitDef
            {
                Name = "Night Bomber", CanChangeAltitude = true, Tier = 2, NightOnly = true,
                MinesCarried = 4, MineDamage = M(600),
                CostMateriel = 1100, BuildTicks = SimConstants.Seconds(34),
                Hp = M(480), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                SpeedMetresPerSecond = M(8.0), TurnRateDegreesPerSecond = 90,
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true,
                WeaponDamage = M(300), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(30), WeaponCooldownTicks = 96,
                SensorOptical = M(300), SensorThermal = M(400),
                SigRadio = 60, SigThermal = 22, SigAcoustic = 35, SigVisual = 55, SigRadar = 58,
                SensorArcDegrees = 60});

            Add(new UnitDef
            {
                Name = "Loitering Munition", Tier = 2,
                CostMateriel = 550, BuildTicks = SimConstants.Seconds(16),
                Hp = M(90), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(24.0), TurnRateDegreesPerSecond = 70,
                Link = LinkKind.Mesh, LinkRobustness = 65, ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(300), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(10),
                SensorOptical = M(240),
                SigRadio = 50, SigThermal = 45, SigAcoustic = 55, SigVisual = 25, SigRadar = 38
            });

            // Navigates by looking at the ground rather than by listening to a
            // radio, so no jammer touches it. It picks its own target at the end of
            // the run, which is exactly what a field of decoys exploits.
            Add(new UnitDef
            {
                Name = "Mid-Range Striker", Faction = FactionId.KestrelPact, Tier = 3,
                NavAid = NavAid.SceneMatching,
                CostMateriel = 700, BuildTicks = SimConstants.Seconds(20),
                Hp = M(120), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(28.0), TurnRateDegreesPerSecond = 60,
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true, AutonomyQuality = 72,
                WeaponDamage = M(420), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(10),
                SensorOptical = M(200),
                SigRadio = 0, SigThermal = 40, SigAcoustic = 50, SigVisual = 25, SigRadar = 44
            });

            // A relay in the sky. Extends reach, not capacity - every drone it
            // carries still needs its own crew. Kill it and everything hanging off
            // it drops at once.
            // The slow heavy one. Big warhead, long reach, and cheap enough to send
            // in numbers. It is not especially tough - a heavy machine gun does
            // real damage to it - so its protection is the altitude it cruises at,
            // and the decision of whether to come down is the player's.
            Add(new UnitDef
            {
                Name = "Heavy Strike Drone", Tier = 3, NavAid = NavAid.SceneMatching,
                CostMateriel = 800, BuildTicks = SimConstants.Seconds(22),
                Hp = M(210), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(18.0), TurnRateDegreesPerSecond = 35,
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true, AutonomyQuality = 60,
                CanChangeAltitude = true,
                WeaponDamage = M(520), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(10),
                SensorOptical = M(200),
                SigRadio = 0, SigThermal = 60, SigAcoustic = 90, SigVisual = 45, SigRadar = 52
            });

            // The fast one. Same job, three times the speed, and the reason a gun
            // mount stops being an answer: it crosses the gun's envelope faster
            // than the gun can find it, aim and fire. Expensive, and it carries
            // less for the money.
            Add(new UnitDef
            {
                Name = "Jet Strike Drone", Tier = 3,
                NavAid = NavAid.SceneMatching, HasCelestialHeading = true,
                CostMateriel = 1900, BuildTicks = SimConstants.Seconds(34),
                Hp = M(180), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(55.0), TurnRateDegreesPerSecond = 22,
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true, AutonomyQuality = 58,
                WeaponDamage = M(380), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(10),
                SensorOptical = M(180),
                SigRadio = 0, SigThermal = 85, SigAcoustic = 95, SigVisual = 40, SigRadar = 50
            });

            // Plywood, foam and a corner reflector. It carries nothing and hurts
            // nobody. Its entire purpose is to look like the expensive thing on
            // somebody else's radar, so that the shot which should have stopped a
            // real strike is spent on it instead.
            Add(new UnitDef
            {
                Name = "Decoy Drone", Tier = 3,
                CostMateriel = 130, BuildTicks = SimConstants.Seconds(7),
                Hp = M(90), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(20.0), TurnRateDegreesPerSecond = 40,
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true,
                IsFlyingDecoy = true, CanChangeAltitude = true,
                SigRadio = 0, SigThermal = 25, SigAcoustic = 65, SigVisual = 30,
                // Deliberately louder on radar than the thing it is imitating. That
                // is the whole product.
                SigRadar = 92
            });

            Add(new UnitDef
            {
                Name = "Mothership", Faction = FactionId.ObsidianDirectorate, Tier = 3,
                CostMateriel = 1400, BuildTicks = SimConstants.Seconds(38),
                Hp = M(520), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(13.0), TurnRateDegreesPerSecond = 50,
                Link = LinkKind.Mesh, LinkRobustness = 65, ConsumesCrew = true, IsMeshRepeater = true,
                SensorOptical = M(400), SensorEsm = M(300),
                SigRadio = 75, SigThermal = 45, SigAcoustic = 40, SigVisual = 45, SigRadar = 66
            });

            Add(new UnitDef
            {
                Name = "Autonomous Munition", Faction = FactionId.ObsidianDirectorate, Tier = 3,
                NavAid = NavAid.SceneMatching,
                CostMateriel = 620, BuildTicks = SimConstants.Seconds(18),
                Hp = M(110), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                SpeedMetresPerSecond = M(22.0), TurnRateDegreesPerSecond = 70,
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true, AutonomyQuality = 55,
                WeaponDamage = M(320), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(10),
                SensorOptical = M(220),
                SigRadio = 0, SigThermal = 42, SigAcoustic = 52, SigVisual = 25, SigRadar = 44
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
