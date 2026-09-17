// KILL ZONE - a real-time strategy video game.
// Unit and structure definitions - the stat blocks.
//
// Units: distances are REAL metres and speeds are REAL metres per real second,
// not the compressed "map metres" this catalogue used to hold. The simulation
// runs the world at SimConstants.TimeMultiplier - four seconds of world per
// second of play - and that is the only compression left anywhere. Before this,
// twelve-to-one distance compression had been baked separately into every speed
// by hand, which is how the catalogue ended up implying 10.2x time compression
// for an interceptor, 3.4x for a heavy strike drone and 240x for the clock, and
// an FPV flying at 1,469 km/h. See docs/SCALE.md, "The scale is currently
// incoherent, which is the real answer to accuracy".
//
// Every distance and speed below cites the research document it came from, or
// says in as many words that it is a designer estimate. That is the point of
// real units: 180 km/h can be checked against reporting and 18 map metres per
// second cannot.
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
        /// <summary>
        /// Time between engagements. A tick is an eighth of a real second at the
        /// global time multiplier, so the default is four real seconds - the cycle
        /// point-defence.md's "Suggested replacement units" gives a gun turret.
        /// </summary>
        public int WeaponCooldownTicks = SimConstants.Seconds(4);

        /// <summary>
        /// What it costs to lay onto a target the mount was not already laid on:
        /// two real seconds of settling, re-ranging and re-classification.
        /// point-defence.md §"Where the existing numbers break" 7 names this as
        /// the real cost rather than the slew, and says a passive EO turret "pays
        /// that in full every time".
        /// </summary>
        public int WeaponAcquisitionTicks = SimConstants.Seconds(2);
        public bool IsInterceptor;

        /// <summary>
        /// What this airframe can work out about its own position when nothing is
        /// telling it. Dead reckoning by default, because that is what cheap
        /// airframes have, and it is the default that makes a border matter.
        /// </summary>
        public NavAid NavAid;

        /// <summary>
        /// What pushes this along. Weather sorts on propulsion rather than on size
        /// or cost, and it is the reason weather is the one system in this game
        /// that is deliberately asymmetric: the side flying cheap electric
        /// quadcopters loses half its year, and the side flying two-stroke engines
        /// above the cloud deck does not.
        /// </summary>
        public Propulsion Propulsion;

        /// <summary>
        /// Which kind of autonomy this airframe carries, if any. Terminal guidance
        /// is the routine one and keeps its crew; target selection is the
        /// speculative one and is what frees a crew and pays for it.
        /// </summary>
        public AutonomyTier AutonomyTier;

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
        public int EngagementsPerBelt;
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

        /// <summary>
        /// This thing radiates to do its job, so it can be told to stop.
        ///
        /// Deliberately not derived from JamStrength, which is how the emitter
        /// component used to be granted. A radar mast jams nobody and is the
        /// loudest building a player owns - SensorSuite.Radar says so in as many
        /// words - so keying "does it transmit" off "does it deny" left the one
        /// unit the dilemma was written for with nothing to switch off. AUDIT
        /// F8. A jammer sets this too: denying and transmitting are the same
        /// act for it, but they are not the same field.
        /// </summary>
        public bool EmitsWhileActive;

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
                CostMateriel = 2000, BuildTicks = SimConstants.PlaySeconds(60),
                Hp = M(5000), Armour = ArmourClass.Structure, FootprintTiles = 8,
                SensorOptical = M(4800), SensorEsm = M(6000),
                SigRadio = 80, SigThermal = 60, SigAcoustic = 25, SigVisual = 95,
                SensorArcDegrees = 360});

            Add(new UnitDef
            {
                Name = "Crew Quarters", IsStructure = true, Tier = 1,
                CostMateriel = 700, BuildTicks = SimConstants.PlaySeconds(22),
                Hp = M(1400), Armour = ArmourClass.Structure, FootprintTiles = 4,
                SigRadio = 55, SigThermal = 40, SigAcoustic = 20, SigVisual = 75
            });

            Add(new UnitDef
            {
                Name = "Drone Workshop", IsStructure = true, Tier = 1,
                CostMateriel = 900, BuildTicks = SimConstants.PlaySeconds(28),
                Hp = M(1600), Armour = ArmourClass.Structure, FootprintTiles = 5,
                SigRadio = 40, SigThermal = 45, SigAcoustic = 35, SigVisual = 80
            });

            Add(new UnitDef
            {
                Name = "Radar Mast", IsStructure = true, Tier = 2,
                CostMateriel = 1000, BuildTicks = SimConstants.PlaySeconds(30),
                Hp = M(900), Armour = ArmourClass.Structure, FootprintTiles = 3,
                SensorRadar = M(16800), SensorEsm = M(10800),
                // An active radar is an emitter. No number here is new: the
                // boost it now takes is UnitDef.SignatureWhileEmitting's
                // existing default, the same one an EW Post has always had.
                // What changes is that the mast can be told to stop, and a
                // mast that has stopped cannot see on the channel it stopped
                // using - World.DetectionRangeFor.
                EmitsWhileActive = true,
                SigRadio = 25, SigThermal = 25, SigAcoustic = 15, SigVisual = 70
            });

            Add(new UnitDef
            {
                Name = "Spool Plant", IsStructure = true, Tier = 2,
                CostMateriel = 800, BuildTicks = SimConstants.PlaySeconds(26),
                Hp = M(1200), Armour = ArmourClass.Structure, FootprintTiles = 4,
                SigRadio = 20, SigThermal = 35, SigAcoustic = 25, SigVisual = 75
            });

            // A jammer is loud. Switching it on paints a permanent mark on the
            // enemy's map, so it is a posture you must defend, not a wall.
            Add(new UnitDef
            {
                Name = "EW Post", IsStructure = true, Tier = 2,
                CostMateriel = 750, BuildTicks = SimConstants.PlaySeconds(24),
                Hp = M(1100), Armour = ArmourClass.Structure, FootprintTiles = 3,
                JamStrength = 70, JamRadiusMetres = M(5400), EmitsWhileActive = true,
                // thermal-optical.md §11 "Jammer, transmitting": thermal 48 (was
                // 40), visual 75 (was 70). Kilowatts into an amplifier and a
                // cooling loop is a genuine hot spot on a vehicle-sized target.
                SigRadio = 25, SigThermal = 48, SigAcoustic = 20, SigVisual = 75
            });

            Add(new UnitDef
            {
                Name = "Relay Mast", IsStructure = true, Tier = 2,
                CostMateriel = 500, BuildTicks = SimConstants.PlaySeconds(18),
                Hp = M(600), Armour = ArmourClass.Structure, FootprintTiles = 2,
                SigRadio = 70, SigThermal = 15, SigAcoustic = 10, SigVisual = 55
            });

            Add(new UnitDef
            {
                Name = "Gun Mount", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                // A thousand metres, which is point-defence.md §Q2's ceiling read
                // straight: "Above ~1,000-1,200 m: machine-gun-class point defence
                // is finished." The M2's quoted effective range is 1,830 m, but
                // that is slant range against an aircraft-sized target and hit
                // probability against a drone collapses long before it.
                //
                // In the old compressed units this was 85 map metres, itself a
                // correction of a 550 that was the mount's *detection* reach being
                // used as its kill ring - the two are an order of magnitude apart,
                // and getting it wrong was most of why a single turret looked
                // unbeatable. In real metres the mistake is much harder to make
                // again: 550 map metres would have to be written as 6,600 m, and
                // nobody types six kilometres for a machine gun.
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(1000), WeaponCooldownTicks = SimConstants.Seconds(4),
                CanReachHigh = false,
                // This mount shipped with neither a traverse rate nor a magazine,
                // and both mechanics quietly did nothing for the one unit every
                // turret finding was measured against: SlewTicks returns early on
                // a zero traverse rate, so the band-change penalty FINDINGS §18
                // called "badly understated" was exactly zero, and with no
                // magazine the saturation tables in FINDINGS §13-16 were run
                // against infinite ammunition. 150°/s is the light AI EO turret
                // figure (point-defence.md §"Where the existing numbers break" 5,
                // §"Azimuth and elevation rates by class"); five shots then a
                // twenty-second reload is the AI Gun Turret row of §"Suggested
                // replacement units", inside §Q3's three to six engagements per
                // belt. Saturation is meant to be the barrel running dry, not the
                // sensors failing.
                TraverseDegreesPerSecond = 150,       // point-defence.md §"Where the existing numbers break" 5 (was 0: instant)
                EngagementsPerBelt = 5, ReloadSeconds = 20, // point-defence.md §"Suggested replacement units" (was 0: never ran dry)
                // Optical 7,200 m: thermal-optical.md §8.3's resolution-limited
                // table gives 7.00 km wide-search detection of a 7 m vehicle,
                // which is what this reach is quoted against (the signature curve
                // then cuts it down for smaller targets - a 0.3 m quad comes out
                // an order of magnitude closer, as §8.3's own 0.30 km row says it
                // should).
                //
                // Acoustic 2,400 m is one microphone, not a network, and the gap
                // is deliberate: point-defence.md §"Where the existing numbers
                // break" 3 notes that Sky Fortress detects Shaheds at ~5 km and
                // concludes the answer is "a cheap networked acoustic unit whose
                // value is coverage rather than accuracy" - a unit this catalogue
                // does not have yet. Against a Shahed-class signature this mount
                // reaches 2.2 km of that 5 km, which is the right shape for one
                // node standing alone.
                SensorOptical = M(7200), SensorAcoustic = M(2400),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 120, SensorScanDegreesPerSecond = 70});

            // The real step up from the Gun Mount, and the research is explicit
            // that the step is not "a bigger magazine" - it is the sensor.
            // point-defence.md §"3. Autocannon with programmable airburst":
            // "an organic AESA search-and-track radar plus EO/IR. This is the
            // first family in the list where the system *measures* target
            // velocity rather than inferring it" - against the Gun Mount's
            // §"Sensor fit": "Passive EO/IR only... it means no direct velocity
            // measurement. Range and closing rate have to be inferred from
            // image scale, which is exactly the measurement that a firing
            // solution is most sensitive to" (§Q1's whole arithmetic argument).
            // That is why this unit carries SensorRadar and the Gun Mount does
            // not: a player choosing between them is choosing a sensor, not a
            // bigger number.
            //
            // CostMateriel, BuildTicks, Hp, FootprintTiles, SensorOptical,
            // SensorRadar and the Sig* signature values have no figure in
            // point-defence.md or thermal-optical.md (the closest it comes is
            // "expensive to build" and "organic radar" as bare qualitative
            // notes) - these seven are designer estimates, set above the Gun
            // Mount and in line with the Interceptor Battery's existing
            // AESA-class radar figure (SensorRadar 800), not a cited fact.
            Add(new UnitDef
            {
                Name = "Autocannon Mount", IsStructure = true, Tier = 3,
                CanEngageAir = true,
                CostMateriel = 1600, BuildTicks = SimConstants.PlaySeconds(36),
                Hp = M(950), Armour = ArmourClass.Structure, FootprintTiles = 3,
                WeaponDamage = M(130), WeaponType = DamageType.Fragmentation,
                // point-defence.md §"Suggested replacement units", row "Airburst
                // Autocannon (Skyranger class)": 280, in a table whose own header
                // says "Range in map metres (12:1)", so 3,360 real metres. That
                // is the same figure to within 12% as §Q2's "Above ~3,000 m:
                // 30 mm is finished" and as ground-force.md §3.1's "3,000 m with
                // AHEAD airburst", which is the corroboration worth having: two
                // documents and a conversion agree.
                WeaponRangeMetres = M(3360),
                // ~2 s per target, revolver cannon - point-defence.md §Q3
                // comparison table and §"Suggested replacement units" "Cycle: 2 s".
                WeaponCooldownTicks = SimConstants.Seconds(2),
                // The Gun Mount cannot touch the High band at all (Q2: "Above
                // ~1,000-1,200 m: machine-gun-class point defence is
                // finished"). This mount reaches into it - §"Suggested
                // replacement units" gives its ceiling as "mid band", and Q2
                // separately gives 30 mm's own limit as "Above ~3,000 m: 30 mm
                // is finished", which is the WeaponRangeMetres above, not a
                // second ceiling. Explicit rather than relying on the
                // CanReachHigh default so the Gun Mount's false and this
                // true read as the same decision made twice, not as one unit
                // configured and one left alone.
                CanReachHigh = true,
                // AHEAD-class airburst: point-defence.md §"3. Autocannon with
                // programmable airburst" - "releases ~152 tungsten
                // sub-projectiles near the target, producing a cloud rather
                // than requiring a direct hit". AmmoType.Airburst already had
                // the flattest range falloff and the most forgiving speed
                // term of the four ammo curves in AirHitChance; nothing had
                // ever loaded it before this unit.
                Ammo = AmmoType.Airburst,
                // 20 engagements before a 40 s reload - point-defence.md
                // §"Suggested replacement units", same row: "20 / 40 s". The
                // Gun Mount's own 5/20 s comes from the row above it in the
                // same table.
                EngagementsPerBelt = 20, ReloadSeconds = 40,
                // ~90°/s - point-defence.md §"Where the existing numbers
                // break" item 5: "~150°/s for a light AI EO turret, ~90°/s
                // for an autocannon turret, ~45°/s for a crewed gun truck",
                // corroborated by §"Azimuth and elevation rates by class":
                // "Self-propelled AAA turret (Gepard, Skyranger): ~90-120°/s"
                // `[GEN]`. Slower than the Gun Mount's 150°/s: mass sets the
                // rate, and a revolver cannon with an ammunition feed and a
                // radar is heavier than a machine gun on a light robotic mount.
                TraverseDegreesPerSecond = 90,
                SensorOptical = M(7200),
                SensorRadar = M(9600),
                SigRadio = 20, SigThermal = 35, SigAcoustic = 25, SigVisual = 60,
                SensorArcDegrees = 120, SensorScanDegreesPerSecond = 70});

            // TEST-ONLY. Not a shipping unit and never presented to a player -
            // it exists so KZ.Balance's GunRangeExperiment can sweep the mount's
            // reach as its one independent variable without silently overwriting
            // the real Gun Mount's 1,000 m (AUDIT-UNWIRED.md F33: three
            // experiments used to do exactly that, and reported the result as if
            // it were the shipped gun). 6,600 m is not a design value or a
            // citation - it is the old, wrong "detection reach used as kill ring"
            // figure (550 map metres) the Gun Mount comment above corrects, kept
            // here as the sweep's upper bound so the experiment still shows how
            // much that mistake was worth. Every other
            // field is copied from Gun Mount so the sweep isolates range alone.
            Add(new UnitDef
            {
                Name = "Test Long Mount", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(6600), WeaponCooldownTicks = SimConstants.Seconds(4),
                CanReachHigh = false,
                TraverseDegreesPerSecond = 150,
                EngagementsPerBelt = 5, ReloadSeconds = 20,
                SensorOptical = M(7200), SensorAcoustic = M(2400),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 120, SensorScanDegreesPerSecond = 70});

            // TEST-ONLY, and the same argument as "Test Long Mount" above applied
            // to the other half of a mount. KZ.Balance's SensorMixExperiment used
            // to answer "which sensors matter" by spawning a real Gun Mount and
            // then overwriting EntityTable.Sensor with a hypothetical suite -
            // which is the hand-assignment WIRING-SPEC rules out as evidence, and
            // which could only ever produce a detection *range*, never an
            // outcome. These four are ordinary catalogue entries that go through
            // World.Spawn like anything else, so the experiment can fight them.
            //
            // Every field except the sensor fit is copied from Gun Mount,
            // including the 1,000 m barrel, so the one variable is what the mount
            // can find things with. The sensor reaches are the Gun Mount's own
            // optical 7,200 m, a 4,800 m microphone array (acoustic.md's
            // man-portable array figure, already used as the hypothetical fit in
            // this experiment before it had units to carry it) and a 5,400 m
            // uncooled imager, between the tank's 3,000 and the Interceptor
            // Battery's 6,240.
            // None of the four is a shipping unit or a proposal for one.
            Add(new UnitDef
            {
                Name = "Test Mount Optics", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(1000), WeaponCooldownTicks = SimConstants.Seconds(4),
                CanReachHigh = false,
                TraverseDegreesPerSecond = 150,
                EngagementsPerBelt = 5, ReloadSeconds = 20,
                SensorOptical = M(7200),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 120, SensorScanDegreesPerSecond = 70});

            Add(new UnitDef
            {
                Name = "Test Mount Acoustic", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(1000), WeaponCooldownTicks = SimConstants.Seconds(4),
                CanReachHigh = false,
                TraverseDegreesPerSecond = 150,
                EngagementsPerBelt = 5, ReloadSeconds = 20,
                SensorAcoustic = M(4800),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 120, SensorScanDegreesPerSecond = 70});

            Add(new UnitDef
            {
                Name = "Test Mount Optics Acoustic", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(1000), WeaponCooldownTicks = SimConstants.Seconds(4),
                CanReachHigh = false,
                TraverseDegreesPerSecond = 150,
                EngagementsPerBelt = 5, ReloadSeconds = 20,
                SensorOptical = M(7200), SensorAcoustic = M(4800),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 120, SensorScanDegreesPerSecond = 70});

            Add(new UnitDef
            {
                Name = "Test Mount Optics Acoustic Thermal", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(1000), WeaponCooldownTicks = SimConstants.Seconds(4),
                CanReachHigh = false,
                TraverseDegreesPerSecond = 150,
                EngagementsPerBelt = 5, ReloadSeconds = 20,
                SensorOptical = M(7200), SensorThermal = M(5400), SensorAcoustic = M(4800),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 120, SensorScanDegreesPerSecond = 70});

            // TEST-ONLY, the same argument again applied to the aperture trade.
            // FINDINGS 22 records a slider - narrow and far-seeing with a blind
            // side, or wide and short - and every number recorded against it is
            // a detection range computed by KZ.Balance from a hand-edited
            // SensorSuite. Nothing has ever fought one. These three, plus the
            // shipped Gun Mount's own 120-degree head sweeping at 70 deg/s, are
            // the four positions on that slider as spawnable units, identical to
            // the Gun Mount in every other field.
            Add(new UnitDef
            {
                Name = "Test Mount Arc 30", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(1000), WeaponCooldownTicks = SimConstants.Seconds(4),
                CanReachHigh = false,
                TraverseDegreesPerSecond = 150,
                EngagementsPerBelt = 5, ReloadSeconds = 20,
                SensorOptical = M(7200), SensorAcoustic = M(2400),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 30, SensorScanDegreesPerSecond = 0});

            Add(new UnitDef
            {
                Name = "Test Mount Arc 120 Staring", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(1000), WeaponCooldownTicks = SimConstants.Seconds(4),
                CanReachHigh = false,
                TraverseDegreesPerSecond = 150,
                EngagementsPerBelt = 5, ReloadSeconds = 20,
                SensorOptical = M(7200), SensorAcoustic = M(2400),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 120, SensorScanDegreesPerSecond = 0});

            Add(new UnitDef
            {
                Name = "Test Mount Arc 360", IsStructure = true, Tier = 2,
                CanEngageAir = true,
                CostMateriel = 450, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(700), Armour = ArmourClass.Structure, FootprintTiles = 2,
                WeaponDamage = M(70), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(1000), WeaponCooldownTicks = SimConstants.Seconds(4),
                CanReachHigh = false,
                TraverseDegreesPerSecond = 150,
                EngagementsPerBelt = 5, ReloadSeconds = 20,
                SensorOptical = M(7200), SensorAcoustic = M(2400),
                SigRadio = 15, SigThermal = 30, SigAcoustic = 20, SigVisual = 55,
                SensorArcDegrees = 360, SensorScanDegreesPerSecond = 0});

            Add(new UnitDef
            {
                Name = "Uplink Terminal", IsStructure = true, Faction = FactionId.KestrelPact, Tier = 3,
                CostMateriel = 2200, BuildTicks = SimConstants.PlaySeconds(50),
                Hp = M(1800), Armour = ArmourClass.Structure, FootprintTiles = 5,
                SigRadio = 85, SigThermal = 40, SigAcoustic = 20, SigVisual = 80
            });

            Add(new UnitDef
            {
                Name = "Autonomy Lab", IsStructure = true, Faction = FactionId.ObsidianDirectorate, Tier = 3,
                CostMateriel = 2000, BuildTicks = SimConstants.PlaySeconds(48),
                Hp = M(1700), Armour = ArmourClass.Structure, FootprintTiles = 5,
                SigRadio = 30, SigThermal = 45, SigAcoustic = 30, SigVisual = 80
            });
        }

        // Radar signatures on the ground, which until now were all zero.
        //
        // radar-rf.md finding 9: "radar can see ground targets, and the game says
        // it cannot... the tank's — in the radar column is wrong. The correct
        // constraint is Doppler: a stationary vehicle is invisible, a moving one
        // is loud." §2.7 backs it with vendor figures - 12 km on vehicles against
        // 5 km on a Phantom 4 - and with the three constraints that make it a
        // rule rather than a number: the Doppler gate, line of sight, and mode
        // exclusivity. The first is SimConstants' notch; the second is AUDIT F1
        // and still unbuilt; the third is not modelled and is worth a brief.
        //
        // Only the tank has a published figure. §5's table gives a moving main
        // battle tank at radar signature 94 and a stationary one at 0, and the
        // zero is the Doppler gate rather than a second number - so the tank
        // carries 94 and the notch takes it away when it stops, which is one
        // mechanic instead of two.
        //
        // Everything else below it is a designer estimate stepped off that 94 by
        // size, on the same scale the air roster already uses (94 down to 64 is
        // 15 dB of cross-section, a tank to a motorbike). The scale is
        // logarithmic - two points per decibel - so these are not "about three
        // quarters of a tank", they are an order of magnitude below it.
        //
        // The two dismounted teams keep a zero, and that is a decision rather
        // than an omission. §2.7 says C-UAS radars classify "human" alongside
        // "vehicle", so a figure would be defensible - but both walk at 1.5 m/s,
        // which is the floor of the notch, so their radial component is under it
        // in almost every geometry. A signature nothing can ever read is the
        // disease WIRING-SPEC exists to cure, so they do not get one until
        // somebody gives infantry a vehicle to ride in.
        static void BuildGroundUnits()
        {
            Add(new UnitDef
            {
                Name = "Recovery UGV", Tier = 1,
                CostMateriel = 500, BuildTicks = SimConstants.PlaySeconds(18),
                Hp = M(420), Armour = ArmourClass.Light,
                // 20 km/h. ground-logistics.md §12 correction 1: logistics robots
                // "run 20-60 km/h flat out but far slower in practice", with a
                // documented medevac averaging 16 km/h over 36.5 km.
                SpeedMetresPerSecond = M(5.5),
                SensorOptical = M(2160),
                SigRadio = 0, SigThermal = 40, SigAcoustic = 45, SigVisual = 55,
                // A small tracked robot. Designer estimate, an order of magnitude
                // of cross-section below the tank's 94.
                SigRadar = 76
            });

            Add(new UnitDef
            {
                Name = "Net Engineer", Tier = 1,
                CostMateriel = 250, BuildTicks = SimConstants.PlaySeconds(12),
                Hp = M(220), Armour = ArmourClass.Soft,
                // 5.4 km/h, a section on foot carrying netting. Designer estimate -
                // the corpus describes net-laying as a construction rate (5-12 km a
                // day, ground-logistics.md §12 correction 3) and never as a walking
                // pace.
                SpeedMetresPerSecond = M(1.5),
                SensorOptical = M(2400),
                SigRadio = 0, SigThermal = 22, SigAcoustic = 15, SigVisual = 20
            });

            Add(new UnitDef
            {
                Name = "Motorcycle Squad", Tier = 1,
                CostMateriel = 300, BuildTicks = SimConstants.PlaySeconds(12),
                Hp = M(260), Armour = ArmourClass.Soft,
                // 61 km/h. Designer estimate: ground-logistics.md documents the
                // motorbike assault as a tactic without giving a speed.
                SpeedMetresPerSecond = M(17.0),
                WeaponDamage = M(45), WeaponType = DamageType.Fragmentation,
                // 400 m of small arms from a moving bike. Designer estimate; the
                // corpus documents the motorbike assault without giving one. The
                // old 160 map metres would read as 1,920 m, which is a rifle
                // squad's *maximum* range on a range card, not an engagement.
                WeaponRangeMetres = M(400),
                SensorOptical = M(2640),
                SigRadio = 0, SigThermal = 35, SigAcoustic = 60, SigVisual = 30,
                // Motorbikes. Designer estimate, and the smallest thing on the
                // ground that gets a radar return at all - 15 dB under the tank.
                SigRadar = 64
            });

            Add(new UnitDef
            {
                Name = "Supply Truck", Tier = 1,
                CostMateriel = 350, BuildTicks = SimConstants.PlaySeconds(14),
                Hp = M(520), Armour = ArmourClass.Light,
                // 50 km/h. Designer estimate - ground-logistics.md §12 correction 2
                // is about where trucks die (30-50 km depth), not how fast they go.
                SpeedMetresPerSecond = M(14.0),
                SensorOptical = M(1920),
                SigRadio = 0, SigThermal = 55, SigAcoustic = 60, SigVisual = 70,
                // A large boxy soft-skinned vehicle. Designer estimate, a little
                // under the tank because the shape is worse, not the size.
                SigRadar = 88
            });

            // A ground robot keeps flying its link, so an electronic-warfare bubble
            // does not kill it - it simply stops dead in the open, which is often
            // worse.
            Add(new UnitDef
            {
                Name = "Logistics UGV", Tier = 2,
                CostMateriel = 400, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(500), Armour = ArmourClass.Light,
                // 18 km/h, just under the 20 km/h bottom of ground-logistics.md §12's
                // flat-out band and near its 16 km/h medevac anchor, because a robot
                // under tele-operation does not drive flat out. The Kuryer in §3 is
                // quoted at 35 km/h.
                SpeedMetresPerSecond = M(5.0),
                Link = LinkKind.Radio, LinkRobustness = 40,
                SensorOptical = M(1920),
                SigRadio = 45, SigThermal = 35, SigAcoustic = 40, SigVisual = 50,
                SigRadar = 76   // as Recovery UGV. Designer estimate.
            });

            Add(new UnitDef
            {
                Name = "EW Truck", Tier = 2,
                CostMateriel = 700, BuildTicks = SimConstants.PlaySeconds(24),
                Hp = M(380), Armour = ArmourClass.Light,
                // 29 km/h cross-country. Designer estimate.
                SpeedMetresPerSecond = M(8.0),
                JamStrength = 55, JamRadiusMetres = M(4200), EmitsWhileActive = true,
                SensorOptical = M(2400), SensorEsm = M(4800),
                SigRadio = 30, SigThermal = 45, SigAcoustic = 45, SigVisual = 55,
                SigRadar = 88   // as Supply Truck, same chassis class. Designer estimate.
            });

            Add(new UnitDef
            {
                Name = "IFV", Tier = 2,
                CostMateriel = 900, BuildTicks = SimConstants.PlaySeconds(26),
                Hp = M(1250), Armour = ArmourClass.Heavy,
                // 32 km/h cross-country. Designer estimate.
                SpeedMetresPerSecond = M(9.0),
                WeaponDamage = M(130), WeaponType = DamageType.Kinetic,
                // 2,000 m: ground-force.md §3.1 sizes a 30 mm remote weapon
                // station ("Dune") at "30 mm, out to about 2 km", which is this
                // vehicle's gun. A straight reading of the old 420 map metres
                // would have been 5,040 m, which no 30 mm autocannon does.
                WeaponRangeMetres = M(2000),
                SensorOptical = M(3840), SensorThermal = M(3000),
                SigRadio = 0, SigThermal = 75, SigAcoustic = 75, SigVisual = 80,
                // Designer estimate, a couple of decibels under the tank.
                SigRadar = 90,
                SensorArcDegrees = 90, SensorScanDegreesPerSecond = 30});

            // Expensive, powerful, and only survivable with jamming cover, cages,
            // and the dark. A tank on open ground in daylight is a salvage pile.
            Add(new UnitDef
            {
                Name = "Main Tank", Tier = 2,
                CostMateriel = 1600, BuildTicks = SimConstants.PlaySeconds(40),
                // 2250 rather than the 2300 in the design document, which does not
                // quite deliver its own stated intent: a shaped charge into the top
                // plate does 572, and four of those come to 2288, so at 2300 the
                // fourth drone leaves the tank alive on twelve hit points. The
                // intent - four drones kill a tank and three do not - is the thing
                // worth preserving; the arithmetic slip is not.
                Hp = M(2250), Armour = ArmourClass.Heavy,
                // 27 km/h cross-country. Designer estimate; front-2026.md describes
                // armour being used as standoff fire rather than for breakthrough
                // speed, and gives no figure.
                SpeedMetresPerSecond = M(7.5),
                WeaponDamage = M(340), WeaponType = DamageType.Kinetic,
                WeaponRangeMetres = M(3000),
                // Thermal 3,000 m: thermal-optical.md §10 "Cooled/uncooled
                // implied by the sensor table" - the battery's cooled imager
                // should out-reach a tank's uncooled one by about two to one, and
                // it does (6,240 against 3,000). §2's own table puts a good
                // uncooled sensor on a running tank at 3.53 km by night and
                // 1.43 km by day, which brackets this.
                //
                // The main gun reaches 3,000 m, and that is a change of substance
                // rather than of units: a straight reading of the old 620 map
                // metres would be 7,440 m, which is not a figure any tank gun
                // has. The corpus gives no tank main-gun engagement range, so
                // 3,000 is a designer estimate anchored on the two direct-fire
                // envelopes it does give - ground-force.md §3.1's 30 mm RWS "out
                // to about 2 km" and the same section's 3,000 m airburst turret.
                SensorOptical = M(4320), SensorThermal = M(3000),
                // thermal-optical.md §11: visual 100 (was 90) anchors the top of
                // the optical scale. Thermal 90 stays as the night value.
                SigRadio = 0, SigThermal = 90, SigAcoustic = 80, SigVisual = 100,
                // radar-rf.md §5: a main battle tank moving reads 94, stationary
                // 0. The zero is the Doppler notch doing it, not a second field.
                SigRadar = 94,
                SensorArcDegrees = 90, SensorScanDegreesPerSecond = 25});

            Add(new UnitDef
            {
                Name = "Designator Team", Faction = FactionId.KestrelPact, Tier = 2,
                CostMateriel = 280, BuildTicks = SimConstants.PlaySeconds(12),
                Hp = M(180), Armour = ArmourClass.Soft,
                // 5.4 km/h on foot. Designer estimate, as Net Engineer.
                SpeedMetresPerSecond = M(1.5),
                Link = LinkKind.Satellite, LinkRobustness = 95,
                SensorOptical = M(7200), SensorThermal = M(3600),
                SigRadio = 40, SigThermal = 22, SigAcoustic = 15, SigVisual = 20,
                SensorArcDegrees = 30});

            // Crew-free air defence. It costs no sortie capacity, which is the
            // whole reason its faction can hold a rear area without grounding its
            // strike wing.
            Add(new UnitDef
            {
                Name = "Interceptor Battery", Faction = FactionId.KestrelPact, Tier = 3,
                CanEngageAir = true, TraverseDegreesPerSecond = 45,
                Ammo = AmmoType.Proximity, EngagementsPerBelt = 12, ReloadSeconds = 20,
                CostMateriel = 1200, BuildTicks = SimConstants.PlaySeconds(32),
                Hp = M(600), Armour = ArmourClass.Light,
                // 21 km/h repositioning. Designer estimate.
                SpeedMetresPerSecond = M(6.0),
                WeaponDamage = M(220), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(3840), WeaponCooldownTicks = SimConstants.Seconds(15),
                IsInterceptor = true, InterceptBaseChance = M(0.75),
                // Thermal 6,240 m, a cooled imager: thermal-optical.md §10
                // "Cooled/uncooled implied by the sensor table", paired with the
                // tank's uncooled 3,000. §2's table puts a cooled sensor on a
                // sky-backed Shahed past 20 km and on a running tank at 18.8 km by
                // night, so this is conservative rather than generous.
                SensorThermal = M(6240), SensorRadar = M(9600), SensorEsm = M(7200),
                SigRadio = 70, SigThermal = 50, SigAcoustic = 40, SigVisual = 60,
                SensorArcDegrees = 360});
        }

        static void BuildAirUnits()
        {
            Add(new UnitDef
            {
                Name = "Scout Quad", CanChangeAltitude = true, Tier = 1, Propulsion = Propulsion.SmallElectric,
                CostMateriel = 120, BuildTicks = SimConstants.PlaySeconds(6),
                Hp = M(40), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                // 60 km/h, the bottom of point-defence.md §"The target set these
                // systems have to beat": "Small FPV / multirotor | 60-150 km/h". A
                // scout loiters rather than races, so it sits at the bottom.
                SpeedMetresPerSecond = M(17.0),
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true, IsMeshRepeater = true,
                SensorOptical = M(3000),
                // thermal-optical.md §11 "Small electric quad": visual 6 (was 12).
                // A 0.3 m airframe; §8.3 puts wide-search detection of one at
                // 300 m, and 6 is still generous.
                SigRadio = 60, SigThermal = 8, SigAcoustic = 12, SigVisual = 6, SigRadar = 22,
                SensorArcDegrees = 180});

            // The workhorse: ammunition with a pilot. It does not come home.
            Add(new UnitDef
            {
                Name = "FPV Team", CanChangeAltitude = true, Tier = 1, Propulsion = Propulsion.SmallElectric,
                AutonomyTier = AutonomyTier.TerminalGuidance,
                CostMateriel = 200, BuildTicks = SimConstants.PlaySeconds(8),
                Hp = M(55), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                // 120 km/h. ground-force.md §3.3(a) gives the attack run directly -
                // "an FPV covers 100 m in 2-3 seconds", which is 33-50 m/s - and
                // acoustic.md §"sound is late" works its example against "an FPV doing
                // 120 km/h". Inside point-defence.md's 60-150 km/h band.
                SpeedMetresPerSecond = M(33.0),
                BlackPolicy = BlackPolicy.LastMile,  // one-way and terminally guided: it finishes on the last point it was given
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(260), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(96), WeaponAcquisitionTicks = SimConstants.Millis(1500), IsMeshRepeater = true,
                SensorOptical = M(1680),
                // thermal-optical.md §11 "Small electric quad": visual 6 (was 15).
                // This number is also the target's size in the gun's hit roll, so
                // 15 was giving the mount nearly 2.8 km against a 0.3 m airframe;
                // §8.3 puts wide-search detection of one at 300 m.
                SigRadio = 70, SigThermal = 8, SigAcoustic = 12, SigVisual = 6, SigRadar = 22
            });

            // Unjammable, and leashed for it. Slower, less agile, and trailing a
            // thread an enemy can find and follow back to the operator.
            Add(new UnitDef
            {
                Name = "Fiber FPV Team", Tier = 2, Propulsion = Propulsion.SmallElectric,
                AutonomyTier = AutonomyTier.TerminalGuidance,
                CostMateriel = 420, BuildTicks = SimConstants.PlaySeconds(12),
                Hp = M(70), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                // 79 km/h. Designer estimate inside point-defence.md's 60-150 km/h
                // band, at the slow end: the spool is drag, and the research is
                // consistent that fiber buys immunity at the cost of agility.
                SpeedMetresPerSecond = M(22.0), TurnRateDegreesPerSecond = 140,
                BlackPolicy = BlackPolicy.LastMile,  // one-way and terminally guided: it finishes on the last point it was given
                Link = LinkKind.Fiber, LinkRobustness = SimConstants.UnjammableRobustness,
                // 16.8 km of fiber. ground-logistics.md §12 puts the fielded
                // spool limit at "5-20 km", and this sits at the top of it -
                // which is the point of the unit, since the leash is what it
                // trades agility for.
                SpoolLengthMetres = M(16800), ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(340), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(96), WeaponAcquisitionTicks = SimConstants.Millis(1500),
                SensorOptical = M(2640),
                // thermal-optical.md §11 "Fiber-optic quad": thermal 9 (was 8) for
                // the spool drag on the motors, visual 6 (was 15) as the FPV Team.
                SigRadio = 0, SigThermal = 9, SigAcoustic = 12, SigVisual = 6, SigRadar = 22
            });

            Add(new UnitDef
            {
                Name = "Multirole Quad", CanChangeAltitude = true, Tier = 2, Propulsion = Propulsion.SmallElectric,
                AutonomyTier = AutonomyTier.TerminalGuidance,
                CostMateriel = 380, BuildTicks = SimConstants.PlaySeconds(14),
                Hp = M(110), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                // 101 km/h, mid-band of point-defence.md §"The target set".
                SpeedMetresPerSecond = M(28.0),
                BlackPolicy = BlackPolicy.DualLink,  // reusable and dual-linked: it tries the other link rather than committing
                Link = LinkKind.Radio, AltLink = LinkKind.Mesh, LinkRobustness = 45,
                ConsumesCrew = true,
                WeaponDamage = M(180), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(480),
                IsMeshRepeater = true,
                SensorOptical = M(3120),
                SigRadio = 65, SigThermal = 10, SigAcoustic = 14, SigVisual = 20, SigRadar = 26,
                SensorArcDegrees = 120});

            // Fast and cheap, but nearly useless without a radar telling it where
            // to look. Killing the radar is how you open the sky.
            Add(new UnitDef
            {
                Name = "Interceptor FPV", CanChangeAltitude = true, Tier = 2, CanEngageAir = true,
                Propulsion = Propulsion.SmallElectric,
                CostMateriel = 300, BuildTicks = SimConstants.PlaySeconds(10),
                Hp = M(60), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                // 306 km/h. point-defence.md §5: "Propeller interceptors top out
                // around 300-315 km/h" - and the same paragraph is why this unit is
                // in trouble, because the jet drones it is meant to catch do 500-600.
                SpeedMetresPerSecond = M(85.0),
                BlackPolicy = BlackPolicy.LastMile,  // one-way and terminally guided: it finishes on the last point it was given
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(0), WeaponType = DamageType.Ram,
                WeaponRangeMetres = M(144), IsInterceptor = true, InterceptBaseChance = M(0.55), IsMeshRepeater = true,
                SensorOptical = M(2160),
                SigRadio = 70, SigThermal = 9, SigAcoustic = 15, SigVisual = 15, SigRadar = 22
            });

            // The eyes. Nothing on the map is worth shooting at until one of these
            // has seen it, so losing them does not slow an army down - it turns its
            // lights off.
            Add(new UnitDef
            {
                Name = "Recon Wing", Tier = 2, NavAid = NavAid.SceneMatching,
                Propulsion = Propulsion.SmallElectric,
                CostMateriel = 900, BuildTicks = SimConstants.PlaySeconds(30),
                Hp = M(200), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                // 90 km/h. Designer estimate: the corpus sizes recon by endurance and
                // altitude rather than speed.
                SpeedMetresPerSecond = M(25.0), TurnRateDegreesPerSecond = 60,
                BlackPolicy = BlackPolicy.DualLink,  // reusable and dual-linked: it tries the other link rather than committing
                Link = LinkKind.Radio, AltLink = LinkKind.Mesh, LinkRobustness = 45,
                ConsumesCrew = true, IsMeshRepeater = true,
                SensorOptical = M(10800),
                // thermal-optical.md §11 "Fixed-wing recon": the research gives 14
                // for an electric airframe and 38 for a small two-stroke, and calls
                // the old 25 the average of two different aircraft. This one is
                // SmallElectric, so 14. Visual 40 (was 30): a three-metre span is a
                // far bigger optical target than a quad.
                SigRadio = 55, SigThermal = 14, SigAcoustic = 18, SigVisual = 40, SigRadar = 40,
                SensorArcDegrees = 45});

            Add(new UnitDef
            {
                Name = "Night Bomber", CanChangeAltitude = true, Tier = 2, NightOnly = true,
                Propulsion = Propulsion.HeavyElectric,
                MinesCarried = 4, MineDamage = M(600),
                CostMateriel = 1100, BuildTicks = SimConstants.PlaySeconds(34),
                Hp = M(480), Armour = ArmourClass.AirRotary, Layer = Layer.Low,
                // 50 km/h. Designer estimate - the heavy multirotor is characterised
                // everywhere in the corpus by payload and noise, never by speed.
                SpeedMetresPerSecond = M(14.0), TurnRateDegreesPerSecond = 90,
                Link = LinkKind.Radio, LinkRobustness = 40, ConsumesCrew = true,
                WeaponDamage = M(300), WeaponType = DamageType.Fragmentation,
                WeaponRangeMetres = M(360), WeaponCooldownTicks = SimConstants.Seconds(6),
                SensorOptical = M(3600), SensorThermal = M(4800),
                // thermal-optical.md §11 "Electric heavy multirotor": thermal 32
                // (was 22) - six to eight motors at 60-90 °C plus large packs is
                // about ten times the FPV's radiating area, and these are
                // intercepted at night by infrared; visual 30 (was 55), which was
                // too high for a 1.5 m airframe against a tank at 100.
                // Radar stays at 58, and the reason is a trap worth marking. The
                // two radar documents use different offsets for identical physics:
                // decoys-masking.md reads S = 2*dBsm + 80, which is what this
                // column implements, and radar-rf.md reads 2*dBsm + 60. A figure
                // from the second converts to this scale by adding twenty.
                //
                // radar-rf.md §5 puts a heavy multirotor at 0.08 m², which is
                // -11 dBsm, which is 2(-11) + 80 = 58 here. Reading its 38 across
                // unconverted would have made this airframe a tenth of its true
                // cross-section and, worse, would have looked like a fix, because
                // 38 sits below the strike drones where intuition says a
                // multirotor belongs.
                //
                // Intuition is wrong on that. A Baba Yaga is physically larger
                // than a Shahed, and cross-section follows geometry rather than
                // menace. The two documents genuinely disagree about the Shahed
                // itself - 0.25 m² against 0.016 - and that disagreement is real
                // and unresolved, but it is not this row's problem.
                SigRadio = 60, SigThermal = 32, SigAcoustic = 35, SigVisual = 30, SigRadar = 58,
                SensorArcDegrees = 60});

            Add(new UnitDef
            {
                Name = "Loitering Munition", Tier = 2, Propulsion = Propulsion.Combustion,
                AutonomyTier = AutonomyTier.TerminalGuidance,
                CostMateriel = 550, BuildTicks = SimConstants.PlaySeconds(16),
                Hp = M(90), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                // 130 km/h. Designer estimate for a piston loitering munition,
                // under the 160-220 km/h point-defence.md §"The target set" gives the
                // Shahed-136 because this airframe is smaller and loiters.
                SpeedMetresPerSecond = M(36.0), TurnRateDegreesPerSecond = 70,
                BlackPolicy = BlackPolicy.LastMile,  // one-way and terminally guided: it finishes on the last point it was given
                Link = LinkKind.Mesh, LinkRobustness = 65, ConsumesCrew = true, OneWay = true,
                WeaponDamage = M(300), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(120),
                SensorOptical = M(2880),
                // thermal-optical.md §11 "Combustion loitering munition": thermal
                // 52 (was 45), a two-stroke at 325-345 °C; visual 20 (was 25), a
                // one-to-two-metre airframe usually seen frontally in a dive.
                SigRadio = 50, SigThermal = 52, SigAcoustic = 55, SigVisual = 20, SigRadar = 38
            });

            // Navigates by looking at the ground rather than by listening to a
            // radio, so no jammer touches it. It picks its own target at the end of
            // the run, which is exactly what a field of decoys exploits.
            Add(new UnitDef
            {
                Name = "Mid-Range Striker", Faction = FactionId.KestrelPact, Tier = 3,
                Propulsion = Propulsion.Combustion,
                AutonomyTier = AutonomyTier.TerminalGuidance,
                NavAid = NavAid.SceneMatching,
                CostMateriel = 700, BuildTicks = SimConstants.PlaySeconds(20),
                Hp = M(120), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                // 162 km/h, the bottom of point-defence.md §"The target set":
                // "Shahed-136 / Geran-2 | 160-220 km/h cruise".
                SpeedMetresPerSecond = M(45.0), TurnRateDegreesPerSecond = 60,
                BlackPolicy = BlackPolicy.LastMile,  // one-way and terminally guided: it finishes on the last point it was given
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true,
                WeaponDamage = M(420), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(120),
                SensorOptical = M(2400),
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
                Propulsion = Propulsion.Combustion,
                AutonomyTier = AutonomyTier.TerminalGuidance,
                CostMateriel = 800, BuildTicks = SimConstants.PlaySeconds(22),
                Hp = M(210), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                // 180 km/h. point-defence.md §"The target set these systems have to
                // beat": "Shahed-136 / Geran-2 | 160-220 km/h cruise `[H]`", middle of
                // the band. This is the airframe that row is about.
                SpeedMetresPerSecond = M(50.0), TurnRateDegreesPerSecond = 35,
                BlackPolicy = BlackPolicy.LastMile,  // one-way and terminally guided: it finishes on the last point it was given
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true,
                CanChangeAltitude = true,
                WeaponDamage = M(520), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(120),
                SensorOptical = M(2400),
                // thermal-optical.md §11 "Combustion heavy strike drone": thermal
                // 72 (was 60) - a fifty-horsepower two-stroke with an exposed
                // exhaust, credibly found beyond 3-5 km by mid-wave infrared;
                // visual 38 (was 45).
                SigRadio = 0, SigThermal = 72, SigAcoustic = 90, SigVisual = 38, SigRadar = 52
            });

            // The fast one. Same job, three times the speed, and the reason a gun
            // mount stops being an answer: it crosses the gun's envelope faster
            // than the gun can find it, aim and fire. Expensive, and it carries
            // less for the money.
            Add(new UnitDef
            {
                Name = "Jet Strike Drone", Tier = 3, Propulsion = Propulsion.Turbojet,
                AutonomyTier = AutonomyTier.TerminalGuidance,
                NavAid = NavAid.SceneMatching, HasCelestialHeading = true,
                CostMateriel = 1900, BuildTicks = SimConstants.PlaySeconds(34),
                Hp = M(180), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                // 504 km/h. point-defence.md §5: jet Geran variants "are designed for
                // 500-600 km/h"; economics.md §1 makes the same figure the headline
                // trend - "interceptors were designed against a 185 km/h target and
                // Russia has moved to ~500 km/h jet airframes". The observed cruise
                // in §5 is lower (300-350 with a terminal sprint); the design figure
                // is used because this unit exists to be the thing the gun cannot
                // track.
                SpeedMetresPerSecond = M(140.0), TurnRateDegreesPerSecond = 22,
                BlackPolicy = BlackPolicy.LastMile,  // one-way and terminally guided: it finishes on the last point it was given
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true,
                WeaponDamage = M(380), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(120),
                SensorOptical = M(2160),
                // thermal-optical.md §11 "Turbojet strike drone": thermal 92 (was
                // 85), the hottest thing in the sky; visual 34 (was 40) - smaller
                // and faster than the piston version, not bigger.
                SigRadio = 0, SigThermal = 92, SigAcoustic = 95, SigVisual = 34, SigRadar = 50
            });

            // Plywood, foam and a corner reflector. It carries nothing and hurts
            // nobody. Its entire purpose is to look like the expensive thing on
            // somebody else's radar, so that the shot which should have stopped a
            // real strike is spent on it instead.
            Add(new UnitDef
            {
                Name = "Decoy Drone", Tier = 3, Propulsion = Propulsion.Combustion,
                CostMateriel = 130, BuildTicks = SimConstants.PlaySeconds(7),
                Hp = M(90), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                // The speed of the thing it is imitating, which is the entire product:
                // point-defence.md §"The target set" gives the Gerbera decoy's speed
                // as "as above", meaning the Shahed's 160-220 km/h. Anything slower
                // and radar discrimination has it on velocity alone - which
                // decoys-masking.md §"the most important change" says is exactly how
                // a 120 km/h reflector glider gets rejected.
                SpeedMetresPerSecond = M(50.0), TurnRateDegreesPerSecond = 40,
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true,
                IsFlyingDecoy = true, CanChangeAltitude = true,
                // thermal-optical.md §11 "Decoy drone (reflectors)": the research
                // gives 40 for a piston-powered decoy and 12 for a cheap electric
                // one. This one is Combustion, so 40 (was 25) - a Gerbera-type
                // decoy is thermally convincing, which is the point of it. Visual
                // 22 (was 30).
                SigRadio = 0, SigThermal = 40, SigAcoustic = 65, SigVisual = 22,
                // Deliberately louder on radar than the thing it is imitating. That
                // is the whole product.
                SigRadar = 92
            });

            Add(new UnitDef
            {
                Name = "Mothership", Faction = FactionId.ObsidianDirectorate, Tier = 3,
                Propulsion = Propulsion.HeavyElectric,
                CostMateriel = 1400, BuildTicks = SimConstants.PlaySeconds(38),
                Hp = M(520), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                // 60 km/h. Designer estimate; a heavy multirotor carrying FPVs.
                SpeedMetresPerSecond = M(17.0), TurnRateDegreesPerSecond = 50,
                Link = LinkKind.Mesh, LinkRobustness = 65, ConsumesCrew = true, IsMeshRepeater = true,
                SensorOptical = M(4800), SensorEsm = M(3600),
                SigRadio = 75, SigThermal = 45, SigAcoustic = 40, SigVisual = 45, SigRadar = 66
            });

            Add(new UnitDef
            {
                Name = "Autonomous Munition", Faction = FactionId.ObsidianDirectorate, Tier = 3,
                Propulsion = Propulsion.Combustion, AutonomyTier = AutonomyTier.TargetSelection,
                NavAid = NavAid.SceneMatching,
                CostMateriel = 620, BuildTicks = SimConstants.PlaySeconds(18),
                Hp = M(110), Armour = ArmourClass.AirFixed, Layer = Layer.High,
                // 130 km/h. Designer estimate, as Loitering Munition.
                SpeedMetresPerSecond = M(36.0), TurnRateDegreesPerSecond = 70,
                BlackPolicy = BlackPolicy.LastMile,  // one-way and terminally guided: it finishes on the last point it was given
                Link = LinkKind.Autonomy, LinkRobustness = SimConstants.UnjammableRobustness,
                ConsumesCrew = false, OneWay = true, AutonomyQuality = 55,
                WeaponDamage = M(320), WeaponType = DamageType.Shaped,
                WeaponRangeMetres = M(120),
                SensorOptical = M(2640),
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
