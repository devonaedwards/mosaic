// KILL ZONE - a real-time strategy video game.
// The world: all simulation state, and the single Step() that advances it by one
// tick.
//
// Everything here is deterministic. Given the same starting state and the same
// sequence of player commands, Step() produces bit-identical results on any
// machine - which is what lets an iPad and a PC play the same multiplayer match,
// and what lets a replay be stored as a few kilobytes of commands rather than a
// recording.

using System.Collections.Generic;

namespace KZ.Sim
{
    public sealed class PlayerState
    {
        public byte Team;
        public FactionId Faction;
        public Fix Materiel;
        public Fix TaskingPoints;
        public int UplinkCapacity;      // scarce: how many satellite drones can be up at once
        public int UplinkInUse;
        public CrewPool Crews;
        public bool HasThermalOptics;

        /// <summary>
        /// This team's autonomy no-go bubble, and the authority for it.
        /// AutonomyState carries a copy on each munition because that is what
        /// the classifier reads, but the team record is what a unit launched
        /// after the order was given inherits from - and a munition launched
        /// after the bubble was drawn is the common case, not the exception, so
        /// without this the feature would protect only whatever happened to
        /// already be in the air. AUDIT-UNWIRED.md F17.
        /// </summary>
        public Fix2 NoGoBoxMin;
        public Fix2 NoGoBoxMax;
        public int NoGoBoxExpiryTick;
        public bool HasNoGoBox;
    }

    public sealed class World
    {
        public readonly Terrain Terrain;
        public readonly EntityTable Entities;
        public readonly SignalGrid Signal;
        public readonly Territory Territory;
        public readonly ReferenceImagery Imagery;
        public readonly MeshGraph Mesh;
        public readonly TetherSystem Tethers;
        public readonly RandomStreams Random;
        public readonly EventRing Events;
        public readonly PlayerState[] Players;
        public readonly CommandBuffer Commands = new CommandBuffer();

        public int Tick { get; private set; }

        /// <summary>
        /// What time of day this match is fought at. Fixed when the match is set
        /// up and constant for its whole length: at the global time multiplier a
        /// 24-hour cycle would take six hours of play, so a match cannot contain
        /// one and a turret that slews in 71 ticks at the same time. The cycle
        /// belongs to the campaign layer, whose clock runs in months.
        /// docs/SCALE.md, "What this costs, and the one thing it cannot buy".
        /// </summary>
        public DayPhase Phase { get; private set; }

        /// <summary>
        /// The weather, and the state of the ground. Two dials rather than one,
        /// because they run on completely different clocks - the sky changes in an
        /// hour and the ground changes over six weeks - and because collapsing
        /// them would hide that mud and fog ask opposite questions.
        /// </summary>
        public WeatherState Weather = WeatherState.Clear;
        public GroundState Ground = GroundState.Firm;

        readonly List<EntityHandle> pendingDeaths = new List<EntityHandle>();
        readonly int[] meshNodeSlotByEntity;

        // ---- Phase 3 / 4.5 wiring constants --------------------------------
        //
        // These belong conceptually in SimConstants.cs, alongside
        // UplinkCapacity's sibling CrewsPerQuarters. They live here instead
        // because another agent is rescaling every distance and speed in that
        // file this session (WIRING-SPEC.md, "Files"), and a concurrent edit
        // to the same lines would be the exact git-add-A hazard the spec's
        // process note warns about. Move them once that pass lands.

        /// <summary>
        /// How much satellite capacity one Uplink Terminal grants its side.
        /// No figure in economics.md or elsewhere - a designer estimate, kept
        /// below Crew Quarters' four crews per building because the satellite
        /// rung is meant to stay the scarce, expensive top of the link ladder
        /// rather than a second population cap. AUDIT-UNWIRED.md F15.
        /// </summary>
        const int UplinkCapacityPerTerminal = 2;

        /// <summary>
        /// How close a friendly Recovery UGV must get to a salvage pile to
        /// claim it. No figure in economics.md - a designer estimate, picked
        /// on the same scale as the other short proximity checks in this file
        /// (a mine's trigger radius, a sortie's landing radius) rather than
        /// derived from any source. AUDIT-UNWIRED.md F3.
        /// </summary>
        const int SalvageCollectionRadiusMetres = 20;

        /// <summary>
        /// ground-force.md §2.1/§8.2: a cage that disrupts the jet is not a
        /// full save - "crushing the cone" still lets some of the charge
        /// through - and a cage that fails to disrupt detonates the warhead
        /// closer to its own optimum standoff, which the research says can
        /// raise penetration above the no-cage case, not just fail to lower
        /// it. Neither multiplier is given a figure by the research (the only
        /// number it gives is the 0.30-0.80 spread on the *save chance*
        /// itself, passed in by the caller of FitCage) - both are designer
        /// estimates, chosen so a save is clearly worth having and a failure
        /// is clearly worse than an uncaged hit rather than merely equal to it.
        /// </summary>
        static readonly Fix CageDisruptedScale = Fix.FromDoubleContentOnly(0.15);
        static readonly Fix CageFailedScale = Fix.FromDoubleContentOnly(1.15);

        public World(Terrain terrain, int entityCapacity, int tetherCapacity, ulong matchSeed, int playerCount)
            : this(terrain, entityCapacity, tetherCapacity, matchSeed, playerCount, 0) { }

        /// <summary>
        /// startTick is the clock reading the match opens on. It sets what time of
        /// day the match is fought at - missions open at dawn or in darkness,
        /// experiments run the same engagement under both - and, unlike before,
        /// that is all it sets: the phase never changes again once the match is
        /// running. SimConstants.TimeOfDayAt is the mapping.
        /// </summary>
        public World(Terrain terrain, int entityCapacity, int tetherCapacity, ulong matchSeed,
                     int playerCount, int startTick)
        {
            Terrain = terrain;
            Entities = new EntityTable(entityCapacity);
            Signal = new SignalGrid(terrain.WidthMetres, terrain.HeightMetres);

            // Default: the whole map belongs to nobody, so a satellite link works
            // nowhere until a mission draws a border. That is deliberately the
            // inconvenient default - a mission that forgets to say where the
            // border runs should notice immediately, rather than quietly granting
            // global coverage the way the old unconditional rule did.
            Territory = new Territory(terrain.WidthMetres, terrain.HeightMetres);
            Imagery = new ReferenceImagery(terrain.WidthMetres, terrain.HeightMetres, playerCount + 1);
            Mesh = new MeshGraph();
            Random = new RandomStreams(matchSeed);
            Tethers = new TetherSystem(tetherCapacity, terrain, Random.Get(RandomStream.TetherSnag));
            Events = new EventRing(4096);
            meshNodeSlotByEntity = new int[entityCapacity];

            Players = new PlayerState[playerCount + 1];
            for (int i = 0; i <= playerCount; i++)
            {
                Players[i] = new PlayerState
                {
                    Team = (byte)i,
                    Faction = FactionId.Neutral,
                    Materiel = Fix.FromInt(4000),
                    TaskingPoints = Fix.Zero,
                    UplinkCapacity = 0,
                    UplinkInUse = 0,
                    Crews = new CrewPool((byte)i, SimConstants.StartingCrews, false)
                };
            }
            Tick = startTick;
            Phase = SimConstants.TimeOfDayAt(startTick);
        }

        public PlayerState Player(byte team) { return Players[team]; }

        public bool IsNight { get { return Phase == DayPhase.Night; } }

        // ---- entity construction -------------------------------------------

        public EntityHandle Spawn(int defId, byte team, Fix2 position)
        {
            UnitDef def = Catalog.Get(defId);
            EntityHandle h = Entities.Create();
            int i = h.Index;

            Entities.AddComponent(i, ComponentMask.Transform | ComponentMask.Health);
            Entities.Position[i] = position;
            Entities.Velocity[i] = Fix2.Zero;
            Entities.Yaw[i] = 0;
            Entities.EntityLayer[i] = def.Layer;
            Entities.Hp[i] = def.Hp;
            Entities.HpMax[i] = def.Hp;
            Entities.CageDisruptionChance[i] = Fix.Zero;
            Entities.Armour[i] = def.Armour;
            Entities.Team[i] = team;
            Entities.DefId[i] = defId;
            Entities.Rank[i] = 1;

            if (def.IsStructure) Entities.AddComponent(i, ComponentMask.Structure);

            if (def.SpeedMetresPerSecond.Raw > 0)
            {
                Entities.AddComponent(i, ComponentMask.Mover);
                Entities.Mover[i] = new MoverState
                {
                    SpeedMetresPerSecond = def.SpeedMetresPerSecond,
                    TurnRateBamPerTick = Trig.DegreesPerSecondToBamPerTick(def.TurnRateDegreesPerSecond),
                    SpeedMultiplier = Fix.One,
                    AcquisitionMultiplier = Fix.One,
                    HasOrder = false,
                    OrderPoint = position,
                    OrderTarget = EntityHandle.None,
                    RadiusClass = 0
                };
            }

            if (def.Link != LinkKind.None)
            {
                Entities.AddComponent(i, ComponentMask.Nav);
                Entities.Nav[i] = new NavState
                {
                    Aid = def.NavAid,
                    CelestialHeading = def.HasCelestialHeading,
                    ErrorMetres = Fix.Zero,
                    MetresSinceFix = Fix.Zero,
                    HasLock = def.NavAid == NavAid.SceneMatching,
                    ReacquireProgress = Fix.Zero
                };

                Entities.AddComponent(i, ComponentMask.Link);
                Entities.Link[i] = new LinkState
                {
                    Kind = def.Link,
                    AltKind = def.AltLink,
                    RobustnessBase = def.LinkRobustness,
                    RobustnessEffective = def.LinkRobustness,
                    JamSampled = 0,
                    Pip = LinkPip.Green,
                    Hops = 0,
                    AltHops = 0,
                    Policy = def.BlackPolicy,
                    AmberTicks = 0,
                    BlackTicks = 0,
                    Parent = EntityHandle.None,
                    LastEvalTick = Tick,
                    Reparenting = false
                };
            }

            if (def.ConsumesCrew || def.Layer != Layer.Ground)
            {
                Entities.AddComponent(i, ComponentMask.Sortie);
                Entities.Sortie[i] = new SortieState
                {
                    CrewId = -1,
                    Phase = SortiePhase.None,
                    Target = EntityHandle.None,
                    DesignatedPoint = position,
                    HasDesignatedPoint = false,
                    SpawnTick = Tick,
                    EgressUntilTick = 0,
                    AcceptsNewOrders = true,
                    OneWay = def.OneWay,
                    // AUDIT-UNWIRED.md F13: where a reusable airframe reports
                    // back to once it is home. Set here (rather than left at
                    // the zero default) so a unit that never sorties through
                    // SortieSystem.Launch - none currently exist, but nothing
                    // stops one being spawned directly in a test or a mission
                    // script - still has a sane home rather than a landing
                    // check comparing against (0,0).
                    HomePosition = position,
                    HasLeftHome = false
                };
            }

            Entities.Signature[i] = SignatureProfile.Make(
                def.SigRadio, def.SigThermal, def.SigAcoustic, def.SigVisual, def.SigRadar);

            if (def.SensorOptical.Raw > 0 || def.SensorThermal.Raw > 0 || def.SensorAcoustic.Raw > 0
                || def.SensorRadar.Raw > 0 || def.SensorEsm.Raw > 0)
            {
                Entities.AddComponent(i, ComponentMask.Sensor);
                Entities.Sensor[i] = new SensorSuite
                {
                    Optical = def.SensorOptical,
                    Thermal = def.SensorThermal,
                    Acoustic = def.SensorAcoustic,
                    Radar = def.SensorRadar,
                    Esm = def.SensorEsm,
                    Quality = 60,
                    DirectionalArcDegrees = def.SensorArcDegrees > 0 ? def.SensorArcDegrees : 360,
                    Facing = 0,
                    ScanDegreesPerSecond = def.SensorScanDegreesPerSecond
                };
            }

            if (def.WeaponDamage.Raw > 0 || def.WeaponType == DamageType.Ram)
            {
                Entities.AddComponent(i, ComponentMask.Weapon);
                Entities.Weapon[i] = new WeaponState
                {
                    Damage = def.WeaponDamage,
                    Type = def.WeaponType,
                    RangeMetres = def.WeaponRangeMetres,
                    CooldownTicks = def.WeaponCooldownTicks,
                    NextFireTick = 0,
                    AcquisitionTicks = def.WeaponAcquisitionTicks,
                    AcquiringUntilTick = 0,
                    Acquiring = EntityHandle.None,
                    IsInterceptor = def.IsInterceptor,
                    InterceptBaseChance = def.InterceptBaseChance,
                    Bearing = 0,
                    TraverseBamPerTick = def.TraverseDegreesPerSecond > 0
                        ? Trig.DegreesPerSecondToBamPerTick(def.TraverseDegreesPerSecond)
                        : 0,
                    TrackingLayer = Layer.Ground,
                    CommittedTarget = EntityHandle.None,
                    Ammo = def.Ammo,
                    CanReachHigh = def.CanReachHigh,
                    EngagementsPerBelt = def.EngagementsPerBelt,
                    EngagementsRemaining = def.EngagementsPerBelt,
                    ReloadTicks = SimConstants.Seconds(def.ReloadSeconds),
                    ReloadingUntilTick = 0
                };
            }

            // AUDIT F8: this used to read `def.JamStrength > 0`, which is why
            // the Radar Mast - the loudest building a player owns, by its own
            // stat block - had no emitter component, took no radio boost, and
            // could not be switched off. Whether a thing transmits and whether
            // it denies are different questions and now read different fields.
            if (def.EmitsWhileActive)
            {
                Entities.AddComponent(i, ComponentMask.Emitter);
                Entities.Emitter[i] = new EmitterState
                {
                    JamStrength = def.JamStrength,
                    RadiusMetres = def.JamRadiusMetres,
                    Active = true,
                    SignatureWhileEmitting = def.SignatureWhileEmitting
                };
            }

            if (def.AutonomyQuality > 0 || def.AutonomyTier != AutonomyTier.None)
            {
                Entities.AddComponent(i, ComponentMask.Autonomy);
                Entities.Autonomy[i] = new AutonomyState
                {
                    Tier = def.AutonomyTier,
                    Quality = def.AutonomyQuality,
                    BoxMin = Fix2.Zero,
                    BoxMax = Fix2.Zero,
                    BoxExpiryTick = 0,
                    HasBox = false,
                    ConsumesCrew = def.ConsumesCrew
                };

                // AUDIT-UNWIRED.md F17: a bubble the player drew ten seconds ago
                // has to cover the munition they launch now, or it covers almost
                // nothing - these are one-way airframes that spend their whole
                // life inside a single order.
                if (team < Players.Length && Players[team].HasNoGoBox) CopyNoGoBoxTo(i, team);
            }

            if (def.IsMeshRepeater) Entities.AddComponent(i, ComponentMask.MeshRepeater);

            // Crew quarters are where the people are. Building one is how you raise
            // the ceiling on how many sorties you can have in the air at once, and
            // losing one is how an opponent lowers it again.
            if (def.IsStructure && def.Name == "Crew Quarters" && team < Players.Length)
            {
                for (int c = 0; c < SimConstants.CrewsPerQuarters; c++)
                    Players[team].Crews.AddCrew(h, 1);
            }

            // AUDIT-UNWIRED.md F15: PlayerState.UplinkCapacity was initialised
            // to zero and never incremented anywhere, so the satellite rung of
            // the link ladder was unlaunchable in any real match - Designator
            // Team is the only carrier of LinkKind.Satellite, and
            // SortieSystem.Launch refuses every one of its launches with
            // NoUplinkCapacity. Same pattern as Crew Quarters above.
            // UplinkCapacityPerTerminal has no figure in economics.md or
            // anywhere else; it is a designer estimate, kept low because
            // satellite is the top, scarcest rung of the ladder rather than a
            // second Crew Quarters.
            if (def.IsStructure && def.Name == "Uplink Terminal" && team < Players.Length)
                Players[team].UplinkCapacity += UplinkCapacityPerTerminal;

            Events.Push(SimEventKind.UnitSpawned, Tick, h, EntityHandle.None, team, defId);
            return h;
        }

        public EntityHandle SpawnDecoy(byte team, Fix2 position, TargetKind mimics, int lifetimeTicks)
        {
            EntityHandle h = Entities.Create();
            int i = h.Index;
            Entities.AddComponent(i, ComponentMask.Transform | ComponentMask.Health | ComponentMask.Decoy);
            Entities.Position[i] = position;
            Entities.Hp[i] = Fix.FromInt(20);
            Entities.HpMax[i] = Fix.FromInt(20);
            Entities.Armour[i] = ArmourClass.Soft;
            Entities.Team[i] = team;
            Entities.DefId[i] = -1;

            // decoys-masking.md §2.1: "the inflation engine emits heat that
            // presents an infrared signature" - no published figure, so 25 is
            // a designer estimate, set well under a real vehicle's (Main Tank
            // 90) since a blower is a far smaller heat source than a running
            // engine. Visual 20 is likewise a designer estimate for a physical
            // object of roughly vehicle size. Everything else stays at zero:
            // no source gives this bare inflatable a radio, acoustic or radar
            // return - a reflector is the flying Decoy Drone catalogue unit,
            // not this one - and giving it one it does not have would make it
            // as loud as the real thing for a fraction of the price. Without
            // any signature at all this was undetectable by IsDetectedBy on
            // every channel, which meant CombatSystem could never engage one
            // regardless of AUDIT-UNWIRED.md F18's targeting fix below.
            Entities.Signature[i] = SignatureProfile.Make(0, 25, 0, 20, 0);

            Entities.Decoy[i] = new DecoyState
            {
                Mimics = mimics,
                Plausibility = Catalog.BasePlausibility(TargetKind.Decoy),
                ExpiryTick = Tick + lifetimeTicks
            };
            return h;
        }

        /// <summary>
        /// Lay one mine. It arms after a short delay so a bomber cannot mine the
        /// ground directly beneath a vehicle it is already over.
        /// </summary>
        public EntityHandle SpawnMine(byte laidByTeam, Fix2 position, Fix damage)
        {
            EntityHandle h = Entities.Create();
            int i = h.Index;
            Entities.AddComponent(i, ComponentMask.Transform | ComponentMask.Mine);
            Entities.Position[i] = position;
            Entities.Team[i] = 0;          // a mine belongs to nobody once it is down
            Entities.DefId[i] = -1;
            Entities.Mine[i] = new MineState
            {
                Damage = damage,
                Type = DamageType.Shaped,
                // 144 real metres of trigger. Designer estimate - deep-strike.md
                // describes air-laid road mining without giving an influence
                // radius - and it is the old 12 at the catalogue's 12:1, which
                // reads as "the vehicle drove over the stretch of road it is on".
                TriggerRadiusMetres = Fix.FromInt(144),
                ArmedAtTick = Tick + SimConstants.MineArmingTicks,
                LaidByTeam = laidByTeam
            };
            return h;
        }

        public EntityHandle SpawnSalvage(Fix2 position, Fix amount)
        {
            EntityHandle h = Entities.Create();
            int i = h.Index;
            Entities.AddComponent(i, ComponentMask.Transform | ComponentMask.Salvage);
            Entities.Position[i] = position;
            Entities.Team[i] = 0;
            Entities.DefId[i] = -1;
            Entities.SalvagePile[i] = new SalvageState
            {
                Amount = amount,
                DecayPerTick = amount / Fix.FromInt(SimConstants.SalvageDecayTicks),
                CreatedTick = Tick
            };
            Events.Push(SimEventKind.SalvageDropped, Tick, h, EntityHandle.None, 0, amount.RoundToInt());
            return h;
        }

        // ---- fittable upgrades ------------------------------------------------
        //
        // AUDIT-UNWIRED.md F16: both HasThermalBlanket and CageDisruptionChance
        // had a real, working consumer and no production writer at all - only
        // KZ.Tests ever set either one. These two methods are that writer, and
        // CommandKind.FitCage/FitThermalBlanket are what a player gets to call
        // them with.

        /// <summary>
        /// Fit a thermal blanket. Cheapest masking in the game and the only
        /// kind a vehicle can wear - see EffectiveSignature for what it buys.
        /// </summary>
        public void FitThermalBlanket(EntityHandle h)
        {
            if (!Entities.IsAlive(h)) return;
            Entities.HasThermalBlanket[h.Index] = true;
        }

        /// <summary>
        /// Fit (or replace) a cage/slat screen. disruptionChance is the save
        /// roll ApplyDamage makes against a topAttack shaped-charge hit -
        /// ground-force.md §2.1 gives a spread of 0.30 (coarse, poorly placed)
        /// to 0.80 (fine, disruption-dominated) from a single source of
        /// unclear provenance, so this is left to the caller rather than
        /// hard-coded. Zero removes the cage.
        /// </summary>
        public void FitCage(EntityHandle h, Fix disruptionChance)
        {
            if (!Entities.IsAlive(h)) return;
            Entities.CageDisruptionChance[h.Index] = Fix.Clamp(disruptionChance, Fix.Zero, Fix.One);
        }

        /// <summary>
        /// Switch an emitter on or off. The one thing that makes a jammer a
        /// posture rather than a wall, and the only mitigation this game offers
        /// for a bubble that lands on its owner's own launch corridor -
        /// see Command.SetEmitting for why there is no own-side exemption.
        ///
        /// Silent on a subject with no emitter, so a player fat-fingering the
        /// order onto a truck costs them nothing, and silent on a redundant
        /// order, so a standing order can restate its posture every tick
        /// without filling the log.
        /// </summary>
        public void SetEmitting(EntityHandle h, bool on)
        {
            if (!Entities.IsAlive(h)) return;
            int i = h.Index;
            if (!Entities.Has(i, ComponentMask.Emitter)) return;
            if (Entities.Emitter[i].Active == on) return;
            Entities.Emitter[i].Active = on;
            Events.Push(SimEventKind.EmissionsChanged, Tick, h, EntityHandle.None,
                        Entities.Team[i], on ? 1 : 0);
        }

        // ---- the autonomy no-go bubble ---------------------------------------

        /// <summary>
        /// Draw (or redraw) this team's no-go bubble: the rectangle its own
        /// autonomous munitions will not consider a target inside, whoever is
        /// standing in it. autonomy.md §9.3 asks for it by name as the counter
        /// to the 1-3% fratricide rate the autonomous tier otherwise carries.
        ///
        /// The two corners arrive in whatever order the player dragged them and
        /// are normalised here rather than at the factory, so a replay of the
        /// raw command produces the same rectangle on every machine.
        ///
        /// durationTicks of zero or less erases the bubble instead of drawing
        /// one - the eraser is the same order with no time on it rather than a
        /// second command kind, the way a decoy with no lifetime is not a decoy.
        /// See AutonomyState.BoxExpiryTick for why a bubble lapses at all.
        /// </summary>
        public void SetAutonomyNoGoBox(byte team, Fix2 cornerA, Fix2 cornerB, int durationTicks)
        {
            if (team >= Players.Length) return;

            PlayerState p = Players[team];
            p.NoGoBoxMin = new Fix2(Fix.Min(cornerA.X, cornerB.X), Fix.Min(cornerA.Y, cornerB.Y));
            p.NoGoBoxMax = new Fix2(Fix.Max(cornerA.X, cornerB.X), Fix.Max(cornerA.Y, cornerB.Y));
            p.NoGoBoxExpiryTick = Tick + (durationTicks > 0 ? durationTicks : 0);
            p.HasNoGoBox = durationTicks > 0;

            // Everything of this team's that is already flying, too. One order,
            // every machine - the player drew a line around their own position,
            // not around one airframe's opinion of it.
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (Entities.Team[i] != team) continue;
                if (!Entities.Has(i, ComponentMask.Autonomy)) continue;
                CopyNoGoBoxTo(i, team);
            }
        }

        void CopyNoGoBoxTo(int i, byte team)
        {
            PlayerState p = Players[team];
            Entities.Autonomy[i].BoxMin = p.NoGoBoxMin;
            Entities.Autonomy[i].BoxMax = p.NoGoBoxMax;
            Entities.Autonomy[i].BoxExpiryTick = p.NoGoBoxExpiryTick;
            Entities.Autonomy[i].HasBox = p.HasNoGoBox;
        }

        // ---- the tick -------------------------------------------------------

        /// <summary>Queue an order for the next tick.</summary>
        public void Enqueue(Command c) { Commands.Enqueue(c); }

        public void Step()
        {
            Tick++;
            Events.Clear();

            // Orders are applied inside the tick rather than before it, so the
            // events they produce belong to the tick that executed them and reach
            // the renderer, the interface and the replay together.
            Commands.Execute(this);

            if (Tick % SimConstants.SignalRebuildInterval == 0) RebuildSignalField();
            if (Tick % SimConstants.MeshRebuildInterval == 0) RebuildMeshGraphs();

            SweepSensors();
            RebuildDetection();
            LinkResolver.ResolveAll(this);
            UpdateTethers();
            MovementSystem.Step(this);

            // After movement, so a reconnaissance airframe grants coverage of
            // where it actually is this tick, and before NavigationSystem, so a
            // scene-matching drone flying the same ground this tick can use
            // what recon just bought it.
            if (Tick % SimConstants.ReconImageryGrantInterval == 0) UpdateReconImagery();

            // After movement, because navigation error is driven by the distance
            // actually flown this tick, and before combat, because what a drone
            // is wrong by is what it is wrong by when it arrives.
            NavigationSystem.Step(this);

            CombatSystem.Step(this);
            CollectSalvage();
            UpdateSalvage();
            UpdateDecoys();
            UpdateMines();

            for (int t = 1; t < Players.Length; t++) Players[t].Crews.Tick(Tick, Events);

            FlushDeaths();
        }

        void RebuildSignalField()
        {
            Signal.ClearEmitters();
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (!Entities.Has(i, ComponentMask.Emitter)) continue;
                if (!Entities.Emitter[i].Active) continue;
                // An emitter that denies nothing has no place in the jamming
                // field. Since the component stopped being granted off
                // JamStrength a radar mast has one, and without this it would
                // take one of the sixty-four emitter slots and - worse - hand
                // Snapshot a zero-radius jamming dome to draw over its owner.
                if (Entities.Emitter[i].JamStrength == 0) continue;

                Signal.AddEmitter(new JamEmitter
                {
                    Position = Entities.Position[i],
                    Strength = Entities.Emitter[i].JamStrength,
                    RadiusMetres = Entities.Emitter[i].RadiusMetres,
                    Team = Entities.Team[i],
                    Source = Entities.HandleAt(i)
                });
            }
            Signal.Rebuild();
        }

        void RebuildMeshGraphs()
        {
            for (byte team = 1; team < Players.Length; team++)
            {
                Mesh.Clear();
                for (int i = 1; i < Entities.HighWater; i++)
                {
                    if (!Entities.IsSlotAlive(i)) continue;
                    if (Entities.Team[i] != team) continue;

                    bool anchor = Entities.Has(i, ComponentMask.Structure)
                                  && IsMeshAnchorStructure(Entities.DefId[i]);
                    bool repeater = Entities.Has(i, ComponentMask.MeshRepeater);
                    bool usesMesh = Entities.Has(i, ComponentMask.Link)
                                    && Entities.Link[i].Kind == LinkKind.Mesh;

                    if (!anchor && !repeater && !usesMesh) continue;

                    int slot = Mesh.Add(new MeshNode
                    {
                        Handle = Entities.HandleAt(i),
                        Position = Entities.Position[i],
                        Team = team,
                        IsAnchor = anchor,
                        IsRepeater = repeater
                    });
                    meshNodeSlotByEntity[i] = slot;
                }

                Mesh.Rebuild(team, Fix.FromInt(SimConstants.MeshRangePerHopMetres), SimConstants.MeshMaxHops);

                // Write the graph result back onto the drones that use it.
                for (int n = 0; n < Mesh.NodeCount; n++)
                {
                    EntityHandle h = Mesh.NodeAt(n).Handle;
                    if (!Entities.IsAlive(h)) continue;
                    int i = h.Index;
                    if (!Entities.Has(i, ComponentMask.Link)) continue;
                    if (Entities.Link[i].Kind != LinkKind.Mesh) continue;

                    if (Mesh.IsConnected(n))
                    {
                        Entities.Link[i].Parent = Mesh.ParentOf(n);
                        Entities.Link[i].Hops = (byte)Mesh.HopsAt(n);
                        Entities.Link[i].AltHops = (byte)Mesh.AltParentsAt(n);
                        Entities.Link[i].Reparenting = false;
                    }
                    else if (!Entities.Link[i].Parent.IsNone)
                    {
                        // Briefly orphaned. The next rebuild may find a new route;
                        // without this grace a whole mesh army strobes every time a
                        // relay dies at the edge of the chain.
                        Entities.Link[i].Reparenting = true;
                        Entities.Link[i].Parent = EntityHandle.None;
                    }
                }
            }
        }

        static bool IsMeshAnchorStructure(int defId)
        {
            if (defId < 0) return false;
            string n = Catalog.Get(defId).Name;
            return n == "Crew Quarters" || n == "Relay Mast" || n == "Command Post";
        }

        void UpdateTethers()
        {
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                int id = Entities.TetherId[i];
                if (id < 0) continue;

                bool cut;
                Tethers.Update(id, Entities.Position[i], Tick, out cut);
                if (cut) Events.Push(SimEventKind.TetherCut, Tick, Entities.HandleAt(i));
            }

            // Threads whose drone is gone keep running down their linger timer.
            for (int id = 0; id < Tethers.Capacity; id++)
            {
                TetherSystem.Tether t = Tethers.Get(id);
                if (t.State != TetherState.Cut && t.State != TetherState.Lingering) continue;
                if (Entities.IsAlive(t.Drone)) continue;
                bool cut;
                Tethers.Update(id, t.Nodes[t.NodeCount > 0 ? t.NodeCount - 1 : 0].Position, Tick, out cut);
            }
        }

        /// <summary>
        /// What a reconnaissance sortie buys: coverage of the ground its camera
        /// passes over. navigation-denied.md §6's supply end of the imagery
        /// resource - "acquired by reconnaissance sorties over ground you do
        /// not control" - and AUDIT-UNWIRED.md F6 found neither end connected.
        ///
        /// Only the two unarmed sensor platforms count as reconnaissance here
        /// (Recon Wing and Scout Quad - both introduced in Defs.cs as the
        /// army's "eyes" and neither carries a weapon), the same name-based
        /// test IsMeshAnchorStructure above already uses for "which structures
        /// anchor a mesh". An FPV team's camera exists to find its own target
        /// in the last second of a one-way flight, not to build a stockpile the
        /// rest of the army can draw on.
        ///
        /// The coverage radius is the airframe's own optical reach - what the
        /// camera can actually see - rather than an invented figure.
        /// </summary>
        void UpdateReconImagery()
        {
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (!Entities.Has(i, ComponentMask.Sensor)) continue;
                int defId = Entities.DefId[i];
                if (defId < 0) continue;

                string n = Catalog.Get(defId).Name;
                if (n != "Recon Wing" && n != "Scout Quad") continue;

                Fix radius = Entities.Sensor[i].Optical;
                if (radius.Raw <= 0) continue;
                Imagery.GrantAround(Entities.Team[i], Entities.Position[i], radius);
            }
        }

        void UpdateSalvage()
        {
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (!Entities.Has(i, ComponentMask.Salvage)) continue;

                Entities.SalvagePile[i].Amount -= Entities.SalvagePile[i].DecayPerTick;
                if (Entities.SalvagePile[i].Amount.Raw <= 0)
                    pendingDeaths.Add(Entities.HandleAt(i));
            }
        }

        /// <summary>
        /// economics.md "Concrete recommendations" 1 / FINDINGS.md §24:
        /// AUDIT-UNWIRED.md F3 found salvage spawned, decayed, and never
        /// collected - SalvageCollected was never pushed and Materiel only
        /// ever went down from its starting balance, so no economic
        /// conclusion drawn from this simulation was actually about an
        /// economy. This is the collection end: a live Recovery UGV within
        /// SalvageCollectionRadiusMetres of a pile claims whatever is left of
        /// it - not a fraction, because the pile is already decaying on its
        /// own every tick and collection only decides whether *this* tick's
        /// remaining value goes to a player or to nothing.
        ///
        /// Called before UpdateSalvage so a pile created by a kill earlier
        /// this same tick can still be claimed at its full just-spawned
        /// value, rather than losing one tick's decay to ordering.
        ///
        /// Cost is bounded the same way UpdateMines already is: outer loop
        /// over live piles (rare and short-lived - fifty seconds per pile),
        /// inner loop over live entities to find a collector. Detection
        /// remains the expensive thing per tick, not this.
        /// </summary>
        void CollectSalvage()
        {
            for (int p = 1; p < Entities.HighWater; p++)
            {
                if (!Entities.IsSlotAlive(p)) continue;
                if (!Entities.Has(p, ComponentMask.Salvage)) continue;

                Fix2 pilePos = Entities.Position[p];
                Fix radius = Fix.FromInt(SalvageCollectionRadiusMetres);
                Fix radiusSq = radius * radius;

                for (int i = 1; i < Entities.HighWater; i++)
                {
                    if (!Entities.IsSlotAlive(i)) continue;
                    byte team = Entities.Team[i];
                    if (team == 0 || team >= Players.Length) continue;
                    int defId = Entities.DefId[i];
                    if (defId < 0) continue;
                    if (Catalog.Get(defId).Name != "Recovery UGV") continue;
                    if (Fix2.SqrDistance(Entities.Position[i], pilePos) > radiusSq) continue;

                    Fix amount = Entities.SalvagePile[p].Amount;
                    Players[team].Materiel += amount;
                    Events.Push(SimEventKind.SalvageCollected, Tick, Entities.HandleAt(p),
                                Entities.HandleAt(i), team, amount.RoundToInt());
                    pendingDeaths.Add(Entities.HandleAt(p));
                    break;
                }
            }
        }

        /// <summary>
        /// Mines wait. That is the whole of their behaviour and the whole of their
        /// point: they cost nothing to maintain, cannot be jammed, cannot be shot
        /// down, and are still there twenty minutes later.
        ///
        /// A mine triggers on any ground vehicle, on either side. Laying a
        /// minefield across an approach denies it to your opponent and to you.
        /// </summary>
        void UpdateMines()
        {
            for (int m = 1; m < Entities.HighWater; m++)
            {
                if (!Entities.IsSlotAlive(m)) continue;
                if (!Entities.Has(m, ComponentMask.Mine)) continue;
                if (Tick < Entities.Mine[m].ArmedAtTick) continue;

                Fix2 minePos = Entities.Position[m];
                Fix trigger = Entities.Mine[m].TriggerRadiusMetres;
                Fix triggerSq = trigger * trigger;

                for (int v = 1; v < Entities.HighWater; v++)
                {
                    if (!Entities.IsSlotAlive(v)) continue;
                    if (v == m) continue;
                    if (Entities.EntityLayer[v] != Layer.Ground) continue;
                    if (!Entities.Has(v, ComponentMask.Health)) continue;
                    if (Entities.Has(v, ComponentMask.Structure)) continue;
                    if (Entities.Team[v] == 0) continue;
                    if (Fix2.SqrDistance(minePos, Entities.Position[v]) > triggerSq) continue;

                    // A mine goes off underneath, which is where armour is thinnest.
                    ApplyDamage(Entities.HandleAt(v), Entities.Mine[m].Damage,
                                Entities.Mine[m].Type, true, EntityHandle.None);
                    Events.Push(SimEventKind.MineDetonated, Tick, Entities.HandleAt(m),
                                Entities.HandleAt(v), Entities.Team[v], 0);
                    pendingDeaths.Add(Entities.HandleAt(m));
                    break;
                }
            }
        }

        void UpdateDecoys()
        {
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (!Entities.Has(i, ComponentMask.Decoy)) continue;
                if (Tick >= Entities.Decoy[i].ExpiryTick)
                    pendingDeaths.Add(Entities.HandleAt(i));
            }
        }

        // ---- damage and death ------------------------------------------------

        /// <summary>
        /// Apply damage, accounting for armour class and whether the attack came
        /// down from above. A fitted cage gets one disruption roll against a
        /// shaped-charge hit to the arc it covers - see the roll below for why
        /// that is not the same thing as soaking damage.
        /// </summary>
        public void ApplyDamage(EntityHandle target, Fix baseDamage, DamageType type,
                                bool topAttack, EntityHandle attacker)
        {
            if (!Entities.IsAlive(target)) return;
            int i = target.Index;

            Fix mult = Catalog.DamageMultiplier(type, Entities.Armour[i], topAttack);
            if (mult.Raw == 0) return;

            // AUDIT-UNWIRED.md F5 / navigation-denied.md §5: a one-way airframe
            // that does not know where it is detonates where it thinks the
            // target is, not where the target actually is. ErrorMetres is the
            // aimpoint error NavigationSystem accumulated over the flight, and
            // "at the moment of arrival" is exactly this call for a munition
            // that does not come home. A drone holding a scene-matching lock
            // never reaches MunitionMissRadiusMetres (its error rounds to
            // nothing); unescorted dead reckoning over a deep denied
            // penetration does.
            //
            // But the question this gate has to ask is not "did this thing
            // drift". It is **does it still need to know where it is at the
            // moment it arrives** - and a munition being flown into a vehicle
            // on a live camera by a person does not. Its coordinates are
            // irrelevant to it; the pilot is steering onto an image. Gating on
            // the airframe alone made every FPV strike in every gun-mount
            // experiment miss (FINDINGS 34), because an FPV Team is
            // dead-reckoning and one-way and has to cross about two kilometres
            // of denied ground to reach its own 96 m weapon range - FINDINGS 34
            // measured 66 m of error against a 40 m radius, for an attack a
            // human was watching all the way in.
            //
            // So the exemption is the eye, not the autonomy tier. It is
            // deliberately not conditioned on AutonomyTier.TerminalGuidance:
            // a plain radio FPV with no terminal guidance at all is flown the
            // same way and is equally indifferent to its own position, and a
            // terminal seeker's contribution is already priced into
            // MunitionMissRadiusMetres, whose own derivation is "about as far
            // off as a terminal seeker can be and still have the target
            // somewhere in frame when it looks".
            //
            // And the exemption ends with the link, which is the decision
            // worth stating rather than leaving to be read off the code. A
            // terminal-guidance airframe under BlackPolicy.LastMile is not
            // flying on a live feed - LinkResolver clears its OrderTarget and
            // sends it to Sortie.DesignatedPoint, a remembered *coordinate*,
            // in the same drifted frame this error describes. So jamming and
            // navigation denial compound: cutting the link is what makes a
            // drone's position start mattering, and 40 m is how much slack the
            // seeker then has. That is §5's rule read in both directions -
            // crossing the border costs you the operator, and losing the
            // operator is what makes losing your position cost you the shot.
            //
            // None of which un-separates the two penalties, which is what the
            // old comment here claimed this gate was preserving. §5's
            // separability is that a drone which brought a map keeps its
            // position while still losing its operator; it says nothing about
            // charging a navigation penalty to a drone that never lost one,
            // because its whole scenario is a drone past the geofence that
            // already has.
            if (Entities.IsAlive(attacker) && Entities.Has(attacker.Index, ComponentMask.Sortie)
                && Entities.Sortie[attacker.Index].OneWay
                && Entities.Has(attacker.Index, ComponentMask.Nav)
                && Entities.Nav[attacker.Index].ErrorMetres > SimConstants.MunitionMissRadiusMetres
                && !AutonomyClassifier.IsPilotedOnLiveFeed(this, attacker.Index))
            {
                Events.Push(SimEventKind.NavMissedAimpoint, Tick, target, attacker,
                            Entities.Team[attacker.Index], 0);
                return;
            }

            Fix damage = baseDamage * mult;

            // AUDIT-UNWIRED.md F16 / ground-force.md §2.1: "the mechanism is
            // not 'more armour'... the effect is therefore probabilistic and
            // geometry-dependent, not a hit-point buffer." So this is a roll,
            // not a subtraction, and it only runs for the arc a cage actually
            // covers - topAttack, the roof and turret a diving drone hits,
            // which is also the arc every fielded cage in the research is
            // built for. A save crushes the cone before it forms; a failure
            // is not merely "no save" - §2.1 again: "a cage that merely adds
            // standoff without disrupting the warhead can raise penetration
            // rather than lower it" - so the failed roll scales damage up, not
            // just through. Keyed to the tick, the target and the attacker
            // rather than a DetRandom draw: WIRING-SPEC.md's hard constraint is
            // that a new draw on an existing stream reorders every stream after
            // it, and Math/DetRandom.cs is outside the files this pass owns, so
            // this follows Reaches()'s own precedent for a deterministic
            // per-event roll that needs no stream of its own.
            if (type == DamageType.Shaped && topAttack && Entities.CageDisruptionChance[i].Raw > 0)
            {
                ulong key = (ulong)(uint)i * 0x9E3779B97F4A7C15UL
                          + (ulong)(uint)Tick * 0xBF58476D1CE4E5B9UL
                          + (ulong)attacker.Value * 0x2545F4914F6CDD1DUL;
                key ^= key >> 31;
                int roll = (int)(key % 100UL);
                int chancePercent = (Entities.CageDisruptionChance[i] * Fix.FromInt(100)).RoundToInt();
                damage = damage * (roll < chancePercent ? CageDisruptedScale : CageFailedScale);
            }

            if (damage.Raw <= 0) return;

            // navigation-denied.md §6: imagery is invalidated by events a
            // player watched happen, never by a clock. Heavy bombardment is
            // the example the research names for a sector losing its coverage;
            // see SimConstants.HeavyBombardmentDamageThreshold for why this
            // reads the raw warhead rather than the post-armour damage. It
            // invalidates everyone's imagery, including the attacker's own -
            // churned ground does not read differently depending on who
            // churned it, which is also true of the enemy's mines.
            if (baseDamage >= SimConstants.HeavyBombardmentDamageThreshold)
                Imagery.Invalidate(Entities.Position[i], SimConstants.HeavyBombardmentInvalidateRadiusMetres);

            Entities.Hp[i] -= damage;
            if (Entities.Hp[i].Raw <= 0) Kill(target, attacker);
        }

        public void Kill(EntityHandle h, EntityHandle killer)
        {
            if (!Entities.IsAlive(h)) return;
            if (pendingDeaths.Contains(h)) return;
            pendingDeaths.Add(h);

            int i = h.Index;
            byte victimTeam = Entities.Team[i];
            int defId = Entities.DefId[i];

            // Everything that dies leaves wreckage worth about a third of its cost.
            // It decays in under a minute, so the richest ground on the map is
            // always the site of the most recent fight.
            if (defId >= 0)
            {
                UnitDef def = Catalog.Get(defId);
                Fix salvage = Fix.FromInt(def.CostMateriel) * SimConstants.SalvageFraction;
                if (salvage.Raw > 0) SpawnSalvage(Entities.Position[i], salvage);

                if (Entities.IsAlive(killer))
                {
                    byte killerTeam = Entities.Team[killer.Index];
                    if (killerTeam != victimTeam && killerTeam < Players.Length)
                        AwardKill(killerTeam, h, killer, def);
                }

                if (def.IsStructure)
                {
                    Events.Push(SimEventKind.StructureDestroyed, Tick, h, killer, victimTeam, defId);
                    if (def.Name == "Crew Quarters" && victimTeam < Players.Length)
                        Players[victimTeam].Crews.OnQuartersDestroyed(h, Tick, Events);
                }
            }

            // Whoever was flying this gets their hands back.
            if (Entities.Has(i, ComponentMask.Sortie))
            {
                int crewId = Entities.Sortie[i].CrewId;
                if (crewId >= 0 && victimTeam < Players.Length)
                    Players[victimTeam].Crews.Release(crewId, Tick);
            }

            if (Entities.TetherId[i] >= 0)
            {
                Tethers.Cut(Entities.TetherId[i], Tick);
                Entities.TetherId[i] = -1;
            }

            Events.Push(SimEventKind.UnitDied, Tick, h, killer, victimTeam, defId);
        }

        /// <summary>
        /// Pay out Tasking Points, but only in full if somebody actually watched it
        /// happen. A kill scored in the dark by a machine with no observer pays
        /// half. That makes the reconnaissance layer earn its keep twice over:
        /// once as the thing that lets you shoot, and once as the accountant.
        /// </summary>
        void AwardKill(byte killerTeam, EntityHandle victim, EntityHandle killer, UnitDef victimDef)
        {
            bool verified = IsKillVerified(killerTeam, victim, killer);

            Fix fraction = victimDef.IsStructure
                ? SimConstants.TaskingPointsStructureFraction
                : SimConstants.TaskingPointsUnitFraction;

            Fix points = Fix.FromInt(victimDef.CostMateriel) * fraction;
            if (!verified) points = points * SimConstants.TaskingPointsUnverifiedScale;

            Players[killerTeam].TaskingPoints += points;

            if (verified)
            {
                int ki = killer.Index;
                if (Entities.Has(ki, ComponentMask.Sortie))
                {
                    int crewId = Entities.Sortie[ki].CrewId;
                    if (crewId >= 0) Players[killerTeam].Crews.CreditKill(crewId, Tick, Events);
                }
                Events.Push(SimEventKind.KillVerified, Tick, victim, killer, killerTeam, points.RoundToInt());
            }
            else
            {
                Events.Push(SimEventKind.KillUnverified, Tick, victim, killer, killerTeam, points.RoundToInt());
            }
        }

        public bool IsKillVerified(byte observerTeam, EntityHandle victim, EntityHandle killer)
        {
            if (!Entities.IsAlive(victim)) return false;
            Fix2 vp = Entities.Position[victim.Index];

            // A piloted drone with a live link counts: the crew saw the feed.
            if (Entities.IsAlive(killer))
            {
                int ki = killer.Index;
                if (Entities.Has(ki, ComponentMask.Link) && Entities.Has(ki, ComponentMask.Sortie)
                    && Entities.Sortie[ki].CrewId >= 0 && Entities.Link[ki].Pip == LinkPip.Green)
                    return true;
            }

            Fix witnessRange = Fix.FromInt(SimConstants.VerificationWitnessRangeMetres);
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (Entities.Team[i] != observerTeam) continue;

                if (Entities.Has(i, ComponentMask.Sensor))
                {
                    Fix reach = BestDetectionRange(i, victim.Index);
                    if (Fix2.SqrDistance(Entities.Position[i], vp) <= reach * reach) return true;
                }
                else if (Fix2.SqrDistance(Entities.Position[i], vp) <= witnessRange * witnessRange)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether a team can currently see an entity.
        ///
        /// Five channels, each matching one kind of sensor against one kind of
        /// signature. Detection succeeds if any single pairing reaches. The
        /// interesting consequences all come from the fact that nothing detects on
        /// every channel and nothing is silent on every channel:
        ///
        /// A fiber drone has defeated passive radio listening completely and has
        /// done nothing at all about the microphone. A jammer is the easiest thing
        /// on the map to find while it is switched on. A turret with cameras and no
        /// thermal imager is most of the way to blind after dark - unless it also
        /// has microphones, in which case darkness barely troubles it.
        ///
        /// Reach scales with the square root of the target's signature rather than
        /// linearly, because that is roughly how detection actually falls off with
        /// emitted strength, and because linear scaling made small drones
        /// effectively invisible to everything.
        /// </summary>
        public bool IsDetectedBy(byte team, EntityHandle target)
        {
            if (!Entities.IsAlive(target)) return false;
            int ti = target.Index;
            if (Entities.Team[ti] == team) return true;

            if (detectionCacheTick == Tick && team < detectionCache.Length)
                return detectionCache[team][ti];

            bool ignored;
            return ComputeDetection(team, ti, out ignored);
        }

        /// <summary>
        /// Whether this side's track on that target is good enough to shoot at, as
        /// opposed to merely good enough to know something is out there.
        ///
        /// AUDIT-UNWIRED.md F9. The only thing that separates the two is passive
        /// RF: a bearing with no range is a cue for another sensor and never a
        /// weapon (radar-rf.md finding 8, §3.4), unless a second listener crosses
        /// it into a fix (§3A.3). Everything else that detects anything in this
        /// game produces a position.
        ///
        /// Cached alongside the detection answer rather than recomputed, because
        /// CanEngage asks it once per candidate per weapon per tick and a walk of
        /// the entity table in there would make the whole thing cubic. Held for
        /// the same TrackHoldTicks as detection itself and for the same reason:
        /// a mount that may fire this tick and not the next, as the edge roll
        /// flickers, is not a tracker, it is a strobe.
        /// </summary>
        public bool HasFiringSolution(byte team, EntityHandle target)
        {
            if (!Entities.IsAlive(target)) return false;
            int ti = target.Index;
            if (Entities.Team[ti] == team) return true;

            if (detectionCacheTick == Tick && team < solutionCache.Length)
                return solutionCache[team][ti];

            bool solution;
            ComputeDetection(team, ti, out solution);
            return solution;
        }

        bool[][] detectionCache;
        bool[][] solutionCache;
        int detectionCacheTick = -1;

        /// <summary>
        /// How many listeners each side owns, counted once per tick. A side with
        /// one cannot cross-fix anything, and this is what lets EsmCrossFix say so
        /// without walking the entity table to find out.
        /// </summary>
        int[] esmSensorCount = new int[0];

        /// <summary>
        /// The tick each team last actually reached each entity on any channel, or
        /// -1 if never. This is the track hold's only state.
        ///
        /// Grain: per team, not per sensor. A track is a side's belief that
        /// something is there, and the belief does not lapse because the one
        /// mount that first raised it happened to blink - RebuildDetection already
        /// collapses detection to "does this team see it at all" before anything
        /// downstream (CanEngage, the renderer) asks, so remembering per sensor
        /// would track state nothing reads and cost O(entities^2) doing it. Per
        /// target, per team is O(entities x teams), the same shape as
        /// detectionCache above.
        /// </summary>
        int[][] lastSeenTick;

        /// <summary>
        /// The same memory for the firing solution, kept apart from it because the
        /// two lapse independently: a contact that drops from a camera to a bare
        /// ESM bearing is still detected and has stopped being shootable, and one
        /// number cannot say that.
        /// </summary>
        int[][] lastSolutionTick;

        /// <summary>
        /// Work out what each side can see, once per tick.
        ///
        /// Detection is asked about constantly - every weapon, against every
        /// candidate, every tick - so computing it on demand made the cost grow
        /// with the cube of the unit count. Once per tick per pair is quadratic,
        /// which at these unit counts is affordable and, more to the point, is a
        /// single place to put a spatial index when it stops being.
        /// </summary>
        /// <summary>
        /// Turn every sweeping sensor head a little further round. A staring head
        /// keeps whatever bearing it was given.
        /// </summary>
        void SweepSensors()
        {
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (!Entities.Has(i, ComponentMask.Sensor)) continue;
                int rate = Entities.Sensor[i].ScanDegreesPerSecond;
                if (rate <= 0) continue;
                Entities.Sensor[i].Facing = (ushort)(
                    (Entities.Sensor[i].Facing + Trig.DegreesPerSecondToBamPerTick(rate)) & 0xFFFF);
            }
        }

        /// <summary>
        /// Whether a pointed sensor is looking at something right now. Microphones
        /// and radio antennas always are; cameras and imagers only if the head
        /// happens to be round the right way.
        /// </summary>
        static bool WithinArc(SensorSuite s, Fix2 from, Fix2 to)
        {
            if (s.DirectionalArcDegrees >= 360) return true;
            Fix2 delta = to - from;
            if (delta.SqrMagnitude().Raw == 0) return true;
            ushort bearing = Trig.Atan2(delta.Y, delta.X);
            int off = Trig.Delta(s.Facing, bearing);
            if (off < 0) off = -off;
            int halfArc = (s.DirectionalArcDegrees * 65536) / (360 * 2);
            return off <= halfArc;
        }

        void RebuildDetection()
        {
            if (detectionCache == null)
            {
                detectionCache = new bool[Players.Length][];
                solutionCache = new bool[Players.Length][];
                lastSeenTick = new int[Players.Length][];
                lastSolutionTick = new int[Players.Length][];
                esmSensorCount = new int[Players.Length];
                for (int t = 0; t < Players.Length; t++)
                {
                    detectionCache[t] = new bool[Entities.Capacity];
                    solutionCache[t] = new bool[Entities.Capacity];
                    lastSeenTick[t] = new int[Entities.Capacity];
                    lastSolutionTick[t] = new int[Entities.Capacity];
                    for (int i = 0; i < Entities.Capacity; i++)
                    {
                        lastSeenTick[t][i] = -1;
                        lastSolutionTick[t][i] = -1;
                    }
                }
            }

            // Who owns enough listeners to cross-fix anything. One pass over the
            // table rather than one per bearing-only contact.
            for (int t = 0; t < esmSensorCount.Length; t++) esmSensorCount[t] = 0;
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (!Entities.Has(i, ComponentMask.Sensor)) continue;
                if (Entities.Sensor[i].Esm.Raw <= 0) continue;
                byte t = Entities.Team[i];
                if (t < esmSensorCount.Length) esmSensorCount[t]++;
            }

            for (byte team = 1; team < Players.Length; team++)
            {
                bool[] seen = detectionCache[team];
                bool[] shootable = solutionCache[team];
                int[] last = lastSeenTick[team];
                int[] lastSolution = lastSolutionTick[team];
                for (int i = 1; i < Entities.HighWater; i++) { seen[i] = false; shootable[i] = false; }

                for (int i = 1; i < Entities.HighWater; i++)
                {
                    if (!Entities.IsSlotAlive(i)) continue;
                    if (Entities.Team[i] == team) { seen[i] = true; shootable[i] = true; continue; }

                    bool solution;
                    if (ComputeDetection(team, i, out solution))
                    {
                        seen[i] = true;
                        last[i] = Tick;
                    }
                    else
                    {
                        // Lost this tick, not gained: a track once acquired is held
                        // for TrackHoldTicks after the sensor stops reaching it
                        // (acoustic.md Recommendation, FINDINGS.md #21), so a
                        // marginal contact does not strobe in and out with the
                        // per-tick edge roll in Reaches(). A target that has never
                        // been solidly seen (last[i] == -1) gets no such grace -
                        // the hold protects a track from being dropped, it does not
                        // make one easier to acquire.
                        seen[i] = last[i] >= 0 && Tick - last[i] <= SimConstants.TrackHoldTicks;
                    }

                    if (solution) lastSolution[i] = Tick;
                    shootable[i] = solution
                        || (lastSolution[i] >= 0 && Tick - lastSolution[i] <= SimConstants.TrackHoldTicks);
                }
            }
            detectionCacheTick = Tick;
        }

        /// <summary>
        /// Whether this side reaches that target on any channel, and - separately -
        /// whether what it has is good enough to shoot at.
        ///
        /// The second answer is F9. radar-rf.md finding 8: "passive RF gives you a
        /// bearing, not a firing solution", and §3.4 is explicit that a single
        /// listener produces a line of bearing with the range "unbounded along the
        /// bearing". Every other channel here produces a position. So a contact
        /// held only on ESM is detected and is not engageable, unless a second
        /// listener crosses it (§3A.3, EsmCrossFix below).
        ///
        /// Cost note. The channels are now tried position-first rather than
        /// ESM-first, so the ordinary case - something a camera or a radar or a
        /// microphone has - returns on the first channel that reaches, exactly as
        /// it did. What changed is that an *ESM-only* contact no longer short-
        /// circuits: it costs a full sweep of the side's sensors, because the
        /// question "is there anything better than a bearing" cannot be answered
        /// by the first bearing. The reordering itself cannot change any result:
        /// this is a disjunction, and Reaches' edge roll is keyed to the target,
        /// the tick and the channel rather than drawn from a stream, so the
        /// answers do not depend on the order they are asked in.
        /// </summary>
        bool ComputeDetection(byte team, int ti, out bool firingSolution)
        {
            Fix2 tp = Entities.Position[ti];
            Layer layer = Entities.EntityLayer[ti];
            SignatureProfile sig = EffectiveSignature(ti);
            bool heardOnEsm = false;

            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (Entities.Team[i] != team) continue;
                if (!Entities.Has(i, ComponentMask.Sensor)) continue;

                SensorSuite s = Entities.Sensor[i];
                if (!s.HasAny) continue;

                Fix distSq = Fix2.SqrDistance(Entities.Position[i], tp);

                // Active radar. It sees the ground too - radar-rf.md finding 9,
                // "radar can see ground targets, and the game says it cannot" -
                // and the constraint that replaces the layer gate is Doppler,
                // which RadarDetectionScale applies. Better against altitude,
                // where there is no clutter to pick a small return out of, and
                // worst against the ground, which is the clutter.
                //
                // And only while it is actually transmitting: a radar that has
                // been told to go quiet is not a passive sensor, it is a
                // switched-off one. The mask test is per sensor rather than per
                // pair and only reached by something that has a radar at all, so
                // the cost in the inner loop is one flag read on a handful of
                // entities - and so is the square root RadarDetectionScale needs
                // for the radial component.
                if (s.Radar.Raw > 0 && IsRadiating(i))
                {
                    int radarReliability;
                    Fix radarMod = RadarDetectionScale(i, ti, layer, out radarReliability);
                    if (radarMod.Raw > 0
                        && Reaches(s.Radar, sig.Radar, radarMod, distSq, SensorChannel.Radar, ti,
                                   radarReliability))
                    { firingSolution = true; return true; }
                }

                // Thermal, which is a night sensor and a poor day one. Sunlight
                // heats the background until there is little contrast left to work
                // with, so the imager that owns the small hours is mediocre at noon.
                bool pointedAtIt = WithinArc(s, Entities.Position[i], tp);

                Fix thermalMod = ThermalTimeScale() * s.ApertureRangeScale;
                if (layer == Layer.High) thermalMod = thermalMod * SimConstants.ThermalHighScale;
                if (pointedAtIt
                    && Reaches(s.Thermal, sig.Thermal, thermalMod, distSq, SensorChannel.Thermal, ti))
                { firingSolution = true; return true; }

                // Microphones. Not the universal answer the model used to make
                // them - against a small electric quad a camera still beats them,
                // even after dark. What they are is the one sensor that gets a
                // piston-engined thing at range, and the one that gets better at
                // night rather than worse.
                if (Reaches(s.Acoustic, sig.Acoustic, AcousticTimeScale(layer), distSq,
                            SensorChannel.Acoustic, ti))
                { firingSolution = true; return true; }

                // Cameras. Long reach in daylight, and after dark this is the line
                // that stops being true.
                Fix opticalMod = s.ApertureRangeScale;
                if (layer == Layer.High) opticalMod = opticalMod * SimConstants.OpticalHighScale;
                if (IsNight && !TeamHasThermalOptics(team))
                    opticalMod = opticalMod * SimConstants.NightOpticalDetectionScale;
                if (pointedAtIt
                    && Reaches(s.Optical, sig.Visual, opticalMod, distSq, SensorChannel.Optical, ti))
                { firingSolution = true; return true; }

                // Passive listening, last. Unaffected by darkness or altitude - if
                // it is transmitting, it is transmitting - and on its own it is a
                // direction and nothing else.
                if (Reaches(s.Esm, sig.Radio, Fix.One, distSq, SensorChannel.Esm, ti))
                    heardOnEsm = true;
            }

            firingSolution = heardOnEsm && EsmCrossFix(team, ti, tp);
            return heardOnEsm;
        }

        /// <summary>
        /// Whether two of this side's listeners hold the same emitter from far
        /// enough apart to have a position rather than two directions.
        ///
        /// radar-rf.md §3A.3's implementable rule: promote a bearing-only contact
        /// to a fix when two or more RF sensors hold it and the bearings cross by
        /// more than about 20 degrees. The spread is measured against whichever
        /// listener reached it first, which is exact for the pair that matters -
        /// if every other listener sits within a narrow cone of the first then no
        /// pair among them crosses any wider than the widest one does with it.
        ///
        /// Geometry only: it asks which listeners are in range, not which of them
        /// got a return this particular tick. The edge roll in Reaches is about
        /// whether a marginal contact is held at all, and its caller has already
        /// answered that - a crossing that flickered with the roll would make a
        /// contact shootable and unshootable thirty-two times a second, and would
        /// also put TrackQualityOf and ComputeDetection at odds, because the first
        /// tests range and the second rolls.
        ///
        /// Cost: it is reached only for a contact that is on ESM and on nothing
        /// else, and only by a side that owns two listeners at all - the count is
        /// taken once per tick in RebuildDetection rather than per target.
        /// </summary>
        bool EsmCrossFix(byte team, int ti, Fix2 tp)
        {
            if (team >= esmSensorCount.Length || esmSensorCount[team] < 2) return false;

            bool haveFirst = false;
            ushort first = 0;
            int lowest = 0, highest = 0;

            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (Entities.Team[i] != team) continue;
                if (!Entities.Has(i, ComponentMask.Sensor)) continue;

                if (Entities.Sensor[i].Esm.Raw <= 0) continue;

                Fix2 sp = Entities.Position[i];
                Fix reach = DetectionRangeFor(i, ti, SensorChannel.Esm);
                if (reach.Raw <= 0 || Fix2.SqrDistance(sp, tp) > reach * reach) continue;

                ushort bearing = Trig.Atan2(tp.Y - sp.Y, tp.X - sp.X);
                if (!haveFirst) { haveFirst = true; first = bearing; continue; }

                int off = Trig.Delta(first, bearing);
                if (off < lowest) lowest = off;
                if (off > highest) highest = off;
                if (highest - lowest >= SimConstants.EsmCrossFixBam) return true;
            }
            return false;
        }

        /// <summary>
        /// What the radar channel's reach is multiplied by against this target,
        /// and how much of its edge reliability survives.
        ///
        /// Two separate things, and the second is why this returns the reliability
        /// through a parameter rather than folding it into the reach. A target at
        /// 2 m/s radial is not merely detected closer in; it is detected
        /// intermittently, which is what SimConstants' 0.30 says. Zero reach means
        /// the notch: not a weak contact, no contact.
        /// </summary>
        Fix RadarDetectionScale(int sensorIndex, int targetIndex, Layer layer,
                                out int reliabilityPercent)
        {
            reliabilityPercent = 100;

            Fix2 delta = Entities.Position[targetIndex] - Entities.Position[sensorIndex];
            Fix distSq = delta.SqrMagnitude();

            // The radial component, which needs the one square root in this
            // function. Squaring the dot product to avoid it does not work: the
            // dot of a 45 km baseline with a 140 m/s closure is seven figures, and
            // its square leaves Q31.32 an order of magnitude behind - the same
            // 46,340 m ceiling SCALE.md records, arriving from the other side.
            Fix radial;
            if (distSq.Raw <= 0) radial = Fix.Zero;
            else
            {
                Fix2 relative = Entities.Velocity[targetIndex] - Entities.Velocity[sensorIndex];
                radial = Fix.Abs(Fix2.Dot(relative, delta) / Fix.Sqrt(distSq));
            }

            // Velocity is written by MovementSystem, which runs after
            // RebuildDetection, so this reads last tick's motion: a thirty-second
            // of a second of lag on a mechanic whose thresholds are whole metres
            // per second.
            if (radial < SimConstants.RadarNotchMetresPerSecond) return Fix.Zero;

            Fix band = Fix.One;
            if (radial < SimConstants.RadarSlowMetresPerSecond)
            {
                band = SimConstants.RadarSlowReachScale;
                reliabilityPercent = SimConstants.RadarSlowReliabilityPercent;
            }
            else if (radial < SimConstants.RadarMediumMetresPerSecond)
            {
                band = SimConstants.RadarMediumReachScale;
                reliabilityPercent = SimConstants.RadarMediumReliabilityPercent;
            }

            return RadarLayerScale(layer) * band;
        }

        /// <summary>What band the target is flying in does to a radar's reach.</summary>
        static Fix RadarLayerScale(Layer layer)
        {
            if (layer == Layer.High) return SimConstants.RadarHighScale;
            if (layer == Layer.Ground) return SimConstants.RadarGroundScale;
            return SimConstants.RadarLowScale;
        }

        /// <summary>
        /// Whether one channel reaches, and how certainly.
        ///
        /// Nothing here is a switch. Inside about two thirds of a sensor's reach a
        /// contact is solid; beyond that the sensor is working at its limit and
        /// produces an intermittent track. How intermittent depends on the channel:
        /// a transmission is a transmission and passive listening rarely loses one,
        /// while a microphone in any wind is doing well to hold a drone at all, and
        /// a radar has to decide whether the small fast return it just got was a
        /// drone or a bird.
        /// </summary>
        /// <param name="reliabilityPercent">
        /// What survives of the channel's edge reliability. Only the radar channel
        /// passes anything but 100: a return that is barely clear of the clutter
        /// notch is intermittent as well as short (SimConstants' Doppler bands).
        /// </param>
        bool Reaches(Fix sensorRange, byte signature, Fix modifier, Fix distSq,
                     SensorChannel channel, int targetIndex, int reliabilityPercent = 100)
        {
            if (sensorRange.Raw <= 0 || signature == 0) return false;

            Fix effective = sensorRange * modifier * WeatherScale(channel)
                            * SignatureScale(signature, channel);
            if (effective.Raw <= 0) return false;
            if (effective > SimConstants.MaxComparableRangeMetres)
                effective = SimConstants.MaxComparableRangeMetres;
            if (distSq > effective * effective) return false;

            Fix solid = effective * SimConstants.DetectionSolidFraction;
            if (distSq <= solid * solid) return true;

            // Out at the edge. Roll, but keyed to the tick and the pair rather than
            // drawn fresh each call, so one target does not resolve differently for
            // two sensors in the same instant.
            int reliability = (ChannelReliability(channel) * reliabilityPercent) / 100;
            ulong key = (ulong)(uint)targetIndex * 0x9E3779B97F4A7C15UL
                      + (ulong)(uint)Tick * 0xBF58476D1CE4E5B9UL
                      + (ulong)channel * 0x94D049BB133111EBUL;
            key ^= key >> 31;
            return (int)(key % 100UL) < reliability;
        }

        /// <summary>
        /// How target strength converts to reach, which is a different law per
        /// channel because the underlying physics is different per channel.
        ///
        /// <para><b>Radar</b> reads its 0-100 signature as decibels of cross-section
        /// rather than as a linear index, and that is the important correction. The
        /// fourth-root rule was right; feeding it a linear index was not. Real
        /// decoys beat the drones they escort on cross-section by a factor of
        /// hundreds, and a linear 80-against-60 is a factor of 1.33, which the
        /// fourth root flattens to a seven percent range advantage. A decoy that
        /// draws fire seven percent further out is not a decoy. On the decibel
        /// reading the same pair is 92 against 52, which is 3.2x, and the unit does
        /// what the unit is for.</para>
        ///
        /// <para><b>Acoustic</b> is linear, and the signature is read as "fraction
        /// of maximum detection range" rather than as loudness. The square root
        /// could not express the real spread: across airframes, acoustic detection
        /// range varies by something like twelve to one, and a square root over a
        /// 0-100 scale cannot reach that without signature values down near one.</para>
        ///
        /// <para>Everything else keeps the square root.</para>
        /// </summary>
        static Fix SignatureScale(byte signature, SensorChannel channel)
        {
            if (channel == SensorChannel.Radar)
            {
                int s = signature > 100 ? 100 : signature;
                return new Fix(RadarReachTable[s]);
            }

            Fix fraction = Fix.FromInt(signature) / Fix.FromInt(100);
            if (channel == SensorChannel.Acoustic) return fraction;
            return Fix.Sqrt(fraction);
        }

        /// <summary>
        /// Reach multiplier per radar signature point: 10^((S-80)/80), which is the
        /// fourth root of cross-section once the signature is read as decibels.
        /// Signature 80 is one square metre and is the figure every radar's nominal
        /// reach is quoted against.
        ///
        /// Baked rather than computed because the simulation has to produce the same
        /// bits on an iPad and on a PC for tens of thousands of ticks, and powers of
        /// ten in fixed point need a table regardless - so the table may as well be
        /// built once, offline, where it can be read. Regenerate with
        /// tools/propagation/radar_scale.py.
        /// </summary>
        static readonly long[] RadarReachTable = {
            429496730, 442038261, 454946011, 468230674, 481903257,
            495975086, 510457820, 525363457, 540704347, 556493199,
            572743094, 589467494, 606680256, 624395639, 642628321,
            661393407, 680706443, 700583430, 721040835, 742095608,
            763765191, 786067537, 809021124, 832644967, 856958639,
            881982283, 907736631, 934243019, 961523408, 989600398,
            1018497251, 1048237908, 1078847007, 1110349909, 1142772712,
            1176142277, 1210486252, 1245833088, 1282212071, 1319653340,
            1358187913, 1397847716, 1438665606, 1480675401, 1523911903,
            1568410934, 1614209360, 1661345124, 1709857278, 1759786012,
            1811172691, 1864059888, 1918491420, 1974512381, 2032169183,
            2091509595, 2152582778, 2215439330, 2280131326, 2346712363,
            2415237601, 2485763812, 2558349426, 2633054578, 2709941160,
            2789072870, 2870515268, 2954335828, 3040603991, 3129391231,
            3220771105, 3314819319, 3411613790, 3511234712, 3613764616,
            3719288448, 3827893632, 3939670144, 4054710589, 4173110276,
            4294967296, 4420382605, 4549460108, 4682306741, 4819032567,
            4959750858, 5104578198, 5253634573, 5407043472, 5564931992,
            5727430940, 5894674943, 6066802559, 6243956392, 6426283210,
            6613934068, 6807064429, 7005834299, 7210408354, 7420956079,
            7637651909
        };

        /// <summary>
        /// How often a channel produces a usable track at the edge of its envelope.
        /// None of them is certain, and the microphone is the least certain of all.
        /// </summary>
        public static int ChannelReliability(SensorChannel channel)
        {
            switch (channel)
            {
                case SensorChannel.Esm: return 95;      // a transmission is hard to miss
                // Thermal above optical, not below it. Against sky the heat sensor
                // is the steadier discriminator; the camera is the one that dies
                // in haze, glare and darkness. The old 88 / 84 had them reversed.
                // thermal-optical.md §10 "Channel reliability".
                case SensorChannel.Optical: return 82;  // was 88
                case SensorChannel.Thermal: return 88;  // was 84
                case SensorChannel.Radar: return 78;    // birds, clutter, small returns
                default: return 52;                     // microphones, in any wind at all
            }
        }

        /// <summary>
        /// What the weather is doing to one sensor channel.
        ///
        /// Note which way fog cuts. It takes almost everything from the cameras and
        /// the heat sensors and nothing whatsoever from radar and passive listening,
        /// so the side that paid for radar is briefly the only side that can see.
        /// That makes fog an opportunity to be waited for rather than a misfortune
        /// to be suffered, which is the opposite of how a "bad weather" state
        /// usually reads.
        /// </summary>
        public Fix WeatherScale(SensorChannel channel)
        {
            switch (Weather)
            {
                case WeatherState.Wet:
                    if (channel == SensorChannel.Optical) return SimConstants.WetOpticalScale;
                    if (channel == SensorChannel.Thermal) return SimConstants.WetThermalScale;
                    if (channel == SensorChannel.Radar) return SimConstants.WetRadarScale;
                    return Fix.One;

                case WeatherState.Murk:
                    if (channel == SensorChannel.Optical) return SimConstants.MurkOpticalScale;
                    if (channel == SensorChannel.Thermal) return SimConstants.MurkThermalScale;
                    return Fix.One;

                case WeatherState.Wind:
                    // Wind does nothing to anything that looks and ends anything
                    // that listens. What defeats a microphone is not turbulence,
                    // it is the wind roaring across the element itself.
                    if (channel == SensorChannel.Acoustic) return Fix.Zero;
                    return Fix.One;

                default:
                    return Fix.One;
            }
        }

        /// <summary>
        /// How well a microphone is working at this hour, against a target in this
        /// band. Quieter air after dark, and below about the height of the trees
        /// the dawn inversion bends sound back down as well.
        /// </summary>
        public Fix AcousticTimeScale(Layer layer)
        {
            if (layer == Layer.High)
            {
                Fix quiet = Phase == DayPhase.Day
                    ? SimConstants.AcousticDayScale
                    : SimConstants.AcousticHighNightScale;
                return SimConstants.AcousticHighScale * quiet;
            }

            switch (Phase)
            {
                case DayPhase.Night: return SimConstants.AcousticNightScale;
                case DayPhase.Dawn: return SimConstants.AcousticDawnScale;
                case DayPhase.Dusk: return SimConstants.AcousticDuskScale;
                default: return SimConstants.AcousticDayScale;
            }
        }

        /// <summary>How well thermal imaging is working at this hour.</summary>
        public Fix ThermalTimeScale()
        {
            switch (Phase)
            {
                case DayPhase.Night: return SimConstants.ThermalNightScale;
                case DayPhase.Day: return SimConstants.ThermalDayScale;
                default: return SimConstants.ThermalTwilightScale;
            }
        }

        bool TeamHasThermalOptics(byte team)
        {
            return team < Players.Length && Players[team].HasThermalOptics;
        }

        /// <summary>
        /// What a unit is giving away right now, as opposed to on paper.
        ///
        /// Two things change it. A jammer or radar that is switched on becomes the
        /// loudest object on the map; switching it off is a real tactical choice
        /// between denying the enemy their radios and not being found. And a
        /// thermal blanket cuts what a heat sensor has to work with, which is the
        /// cheapest masking in the game and the only one available to a vehicle.
        /// </summary>
        public SignatureProfile EffectiveSignature(int i)
        {
            SignatureProfile sig = Entities.Signature[i];

            if (Entities.Has(i, ComponentMask.Emitter) && Entities.Emitter[i].Active)
            {
                byte emitting = Entities.Emitter[i].SignatureWhileEmitting;
                if (emitting > sig.Radio) sig.Radio = emitting;
            }

            if (Entities.HasThermalBlanket[i])
            {
                int reduced = (sig.Thermal * 40) / 100;
                sig.Thermal = (byte)reduced;
            }

            return sig;
        }

        /// <summary>
        /// Whether this entity's transmitters are on.
        ///
        /// Something with no emitter component is always radiating, because it
        /// has nothing to switch: a tank's optics are not a transmission and a
        /// microphone is not either. The distinction only bites on the radar
        /// channel, which is the one channel that finds things by shouting at
        /// them, and it bites in exactly one direction - a mast that has gone
        /// quiet keeps every passive sensor it owns. A Radar Mast under emission
        /// control still hears on its ESM, which is the trade the whole order is
        /// for: stop being the loudest thing on the map, keep listening, lose
        /// the thing that measures velocity.
        /// </summary>
        bool IsRadiating(int i)
        {
            return !Entities.Has(i, ComponentMask.Emitter) || Entities.Emitter[i].Active;
        }

        /// <summary>
        /// The reach of one channel against one target, for the interface to draw
        /// and for tests to assert against.
        /// </summary>
        public Fix DetectionRangeFor(int sensorIndex, int targetIndex, SensorChannel channel)
        {
            SensorSuite s = Entities.Sensor[sensorIndex];
            SignatureProfile sig = EffectiveSignature(targetIndex);
            Layer layer = Entities.EntityLayer[targetIndex];

            Fix nominal, mod = Fix.One;
            byte strength;

            switch (channel)
            {
                case SensorChannel.Esm: nominal = s.Esm; strength = sig.Radio; break;
                case SensorChannel.Radar:
                    if (!IsRadiating(sensorIndex)) return Fix.Zero;
                    nominal = s.Radar; strength = sig.Radar;
                    // No layer gate any more (radar-rf.md finding 9) - what gates
                    // this channel is the Doppler notch, and a zero here is a
                    // target sitting in the clutter rather than one flying in the
                    // wrong band. ComputeDetection asks the same question of the
                    // same function, so what the interface draws and what a
                    // weapon gets are one answer.
                    int reliabilityIgnored;
                    mod = RadarDetectionScale(sensorIndex, targetIndex, layer, out reliabilityIgnored);
                    if (mod.Raw <= 0) return Fix.Zero;
                    break;
                case SensorChannel.Thermal:
                    nominal = s.Thermal; strength = sig.Thermal;
                    mod = ThermalTimeScale() * s.ApertureRangeScale;
                    if (layer == Layer.High) mod = mod * SimConstants.ThermalHighScale;
                    break;
                case SensorChannel.Acoustic:
                    nominal = s.Acoustic; strength = sig.Acoustic;
                    mod = AcousticTimeScale(layer);
                    break;
                default:
                    nominal = s.Optical; strength = sig.Visual;
                    mod = s.ApertureRangeScale;
                    if (layer == Layer.High) mod = mod * SimConstants.OpticalHighScale;
                    if (IsNight && !TeamHasThermalOptics(Entities.Team[sensorIndex]))
                        mod = mod * SimConstants.NightOpticalDetectionScale;
                    break;
            }

            if (nominal.Raw <= 0 || strength == 0) return Fix.Zero;

            // Capped, because every caller squares this to avoid a square root
            // per pair per tick and a Q31.32 square runs out at 46,340 m - see
            // SimConstants.MaxComparableRangeMetres. A radar mast against a decoy
            // built to return three times its own cross-section clears that, and
            // uncapped it would wrap and the mast would see nothing at all.
            Fix reach = nominal * mod * WeatherScale(channel) * SignatureScale(strength, channel);
            return reach > SimConstants.MaxComparableRangeMetres
                 ? SimConstants.MaxComparableRangeMetres : reach;
        }

        /// <summary>The best reach any channel of one sensor has against one target.</summary>
        public Fix BestDetectionRange(int sensorIndex, int targetIndex)
        {
            Fix best = Fix.Zero;
            for (int ch = 0; ch < 5; ch++)
            {
                Fix r = DetectionRangeFor(sensorIndex, targetIndex, (SensorChannel)ch);
                if (r > best) best = r;
            }
            return best;
        }

        /// <summary>
        /// What kind of track this side is holding on that target right now.
        ///
        /// Lifted out of CombatSystem's CueMultiplier, which has always computed
        /// exactly this and thrown away everything but a number. Two things need
        /// the answer and they must agree: the terminal roll an interceptor makes
        /// when it arrives, and - the reason this exists - the vector it is given to
        /// get there. An interceptor guided by a radar track flies to a computed
        /// meeting point; one guided by an eyeball flies at the target, and against
        /// anything faster than itself that is a chase it cannot win.
        ///
        /// Cost: up to three passes over the entity table per asking, the same
        /// shape the multiplier already had. Interceptors are a handful of
        /// entities and both callers ask once each per airframe per tick, so this
        /// does not touch the detection inner loop. The third pass is only
        /// reached by a contact nothing but a listener holds.
        ///
        /// AUDIT-UNWIRED.md F9 added the bottom rung. The second pass used to ask
        /// BestDetectionRange, which includes the ESM channel, so a drone held on
        /// nothing but its own transmissions read as <c>Optical</c> - and an
        /// interceptor sent at it flew 0.55 of a lead computed from a velocity
        /// nobody had measured, off a contact whose range was never known at all.
        /// It reads as <c>Bearing</c> now, which flies no lead.
        /// </summary>
        public TrackQuality TrackQualityOf(byte team, EntityHandle target)
        {
            if (!Entities.IsAlive(target)) return TrackQuality.None;
            Fix2 targetPos = Entities.Position[target.Index];

            for (int j = 1; j < Entities.HighWater; j++)
            {
                if (!Entities.IsSlotAlive(j)) continue;
                if (Entities.Team[j] != team) continue;
                if (!Entities.Has(j, ComponentMask.Sensor)) continue;
                if (Entities.Sensor[j].Radar.Raw <= 0) continue;

                // A mast that has been told to stop transmitting returns zero
                // here (DetectionRangeFor gates the radar channel on
                // IsRadiating), so it cannot hand an interceptor a full lead
                // solution off a sensor that is not radiating. The passes below
                // reach the same answer the same way: DetectionRangeFor asks per
                // channel, and the radar channel is the one that is dark.
                Fix r = DetectionRangeFor(j, target.Index, SensorChannel.Radar);
                if (r.Raw > 0 && Fix2.SqrDistance(Entities.Position[j], targetPos) <= r * r)
                    return TrackQuality.Radar;
            }

            bool heardOnEsm = false;
            for (int j = 1; j < Entities.HighWater; j++)
            {
                if (!Entities.IsSlotAlive(j)) continue;
                if (Entities.Team[j] != team) continue;
                if (!Entities.Has(j, ComponentMask.Sensor)) continue;

                Fix2 sensorPos = Entities.Position[j];
                Fix distSq = Fix2.SqrDistance(sensorPos, targetPos);

                for (int ch = 0; ch < 5; ch++)
                {
                    SensorChannel channel = (SensorChannel)ch;
                    Fix r = DetectionRangeFor(j, target.Index, channel);
                    if (r.Raw <= 0 || distSq > r * r) continue;
                    if (channel == SensorChannel.Esm) { heardOnEsm = true; continue; }
                    return TrackQuality.Optical;
                }
            }

            // Two listeners with a baseline between them have a position rather
            // than two directions, and a position with no measured velocity is
            // what the rung above means - radar-rf.md §3A.3.
            if (heardOnEsm)
            {
                if (EsmCrossFix(team, target.Index, targetPos)) return TrackQuality.Optical;
                return TrackQuality.Bearing;
            }

            return TrackQuality.None;
        }

        void FlushDeaths()
        {
            for (int i = 0; i < pendingDeaths.Count; i++)
            {
                EntityHandle h = pendingDeaths[i];
                if (!Entities.IsAlive(h)) continue;
                int idx = h.Index;

                if (Entities.Has(idx, ComponentMask.Link) && Entities.Link[idx].Kind == LinkKind.Satellite)
                {
                    byte team = Entities.Team[idx];
                    if (team < Players.Length && Players[team].UplinkInUse > 0) Players[team].UplinkInUse--;
                }
                Entities.Destroy(h);

                // A dead slot's index gets handed to whatever spawns next
                // (EntityTable.Create reuses off the free list). Without this, a
                // brand-new, unrelated entity could inherit a track hold that
                // belonged to whatever used to occupy its index.
                if (lastSeenTick != null)
                    for (int t = 0; t < lastSeenTick.Length; t++) lastSeenTick[t][idx] = -1;
                if (lastSolutionTick != null)
                    for (int t = 0; t < lastSolutionTick.Length; t++) lastSolutionTick[t][idx] = -1;
            }
            pendingDeaths.Clear();
        }

        public void QueueDeath(EntityHandle h) { if (!pendingDeaths.Contains(h)) pendingDeaths.Add(h); }

        // ---- hashing ---------------------------------------------------------

        /// <summary>
        /// A fingerprint of the entire simulation state.
        ///
        /// Two machines playing the same networked match compare this number every
        /// few seconds. If it ever differs they have diverged, and the match is
        /// stopped and reported rather than allowed to drift into two different
        /// games that each player believes they are winning.
        /// </summary>
        public ulong StateHash()
        {
            const ulong Prime = 1099511628211UL;
            ulong h = 1469598103934665603UL;

            h = (h ^ (ulong)Tick) * Prime;
            h = (h ^ (ulong)Entities.AliveCount) * Prime;

            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                h = (h ^ (ulong)Entities.Position[i].X.Raw) * Prime;
                h = (h ^ (ulong)Entities.Position[i].Y.Raw) * Prime;
                h = (h ^ (ulong)Entities.Hp[i].Raw) * Prime;
                h = (h ^ (ulong)Entities.Yaw[i]) * Prime;
                h = (h ^ (ulong)Entities.Team[i]) * Prime;
                h = (h ^ (ulong)(uint)Entities.DefId[i]) * Prime;
                h = (h ^ (ulong)Entities.Mask[i]) * Prime;

                // AUDIT-UNWIRED.md F16: both had a working consumer and no
                // writer until FitThermalBlanket/FitCage, so a mismatch here
                // could only ever have come from the tests that hand-set them.
                // Now that a real command path can set them mid-match, a
                // divergence in either is a divergence in what a shaped-charge
                // hit or a detection roll actually does, and neither was
                // previously reachable through here.
                h = (h ^ (Entities.HasThermalBlanket[i] ? 1UL : 0UL)) * Prime;
                h = (h ^ (ulong)Entities.CageDisruptionChance[i].Raw) * Prime;

                if (Entities.Has(i, ComponentMask.Sortie))
                {
                    // AUDIT-UNWIRED.md F13/F19: EgressUntilTick now gates
                    // MovementSystem (it did not before, so a miss here would
                    // not have shown up as anything but a stale position was
                    // already covered), and HomePosition/HasLeftHome decide
                    // whether SortieSystem.Recover fires this tick - a crew
                    // returning on one client and not the other is exactly
                    // the kind of divergence the state hash exists to catch
                    // before Players[t].Crews.StateHash() below reflects it.
                    SortieState s = Entities.Sortie[i];
                    h = (h ^ (ulong)(uint)s.CrewId) * Prime;
                    h = (h ^ (ulong)s.Phase) * Prime;
                    h = (h ^ (ulong)(uint)s.EgressUntilTick) * Prime;
                    h = (h ^ (ulong)s.HomePosition.X.Raw) * Prime;
                    h = (h ^ (ulong)s.HomePosition.Y.Raw) * Prime;
                    h = (h ^ (s.HasLeftHome ? 1UL : 0UL)) * Prime;
                }

                // AUDIT-UNWIRED.md F17: the no-go bubble is persistent state
                // that decides what a munition is allowed to consider, and it
                // is set by an order rather than derived from anything else
                // hashed here - two peers that disagreed about where the line
                // was drawn would disagree about which of the player's own
                // vehicles their munitions are willing to dive on, which is
                // about as divergent as a match gets.
                if (Entities.Has(i, ComponentMask.Autonomy))
                {
                    AutonomyState a = Entities.Autonomy[i];
                    h = (h ^ (a.HasBox ? 1UL : 0UL)) * Prime;
                    h = (h ^ (ulong)a.BoxMin.X.Raw) * Prime;
                    h = (h ^ (ulong)a.BoxMin.Y.Raw) * Prime;
                    h = (h ^ (ulong)a.BoxMax.X.Raw) * Prime;
                    h = (h ^ (ulong)a.BoxMax.Y.Raw) * Prime;
                    h = (h ^ (ulong)(uint)a.BoxExpiryTick) * Prime;
                }

                // AUDIT F8: EmitterState.Active was persistent state that no
                // command could reach, so the only thing that could ever have
                // desynchronised it was a test. Now a standing order toggles it
                // mid-match, and it decides three separate things a tick later -
                // whose links die, who is the loudest thing on the map, and
                // whether a radar mast can see at all. Two peers that disagreed
                // about whether a mast was transmitting would disagree about all
                // three and about nothing else hashed here.
                if (Entities.Has(i, ComponentMask.Emitter))
                    h = (h ^ (Entities.Emitter[i].Active ? 1UL : 0UL)) * Prime;

                if (Entities.Has(i, ComponentMask.Link))
                {
                    h = (h ^ (ulong)Entities.Link[i].Pip) * Prime;
                    h = (h ^ (ulong)Entities.Link[i].JamSampled) * Prime;
                    h = (h ^ (ulong)Entities.Link[i].AmberTicks) * Prime;
                    h = (h ^ (ulong)Entities.Link[i].Hops) * Prime;
                }

                // Which target a mount has committed to (CombatSystem task
                // 1/point-defence.md §Q3) is persistent state that changes
                // future ticks - a mount holds it rather than re-picking - so
                // a divergence here has to be caught here, not inferred later
                // from whichever target ends up dead.
                // Which target a mount is laid on (Acquiring) and where the barrel
                // is pointing (Bearing) are the same kind of state and are hashed
                // for the same reason: a mount now holds its lay across bursts
                // rather than dropping it after every shot, so both persist beyond
                // the tick that set them and both decide when the next shot lands.
                // The magazine goes in for the third time around the same point -
                // until the lay was held, no mount in a real engagement ever
                // reached its second shot, let alone its fifth, so a divergence in
                // EngagementsRemaining or in a reload timer could not previously
                // show up here at all.
                if (Entities.Has(i, ComponentMask.Weapon))
                {
                    h = (h ^ (ulong)Entities.Weapon[i].CommittedTarget.Value) * Prime;
                    h = (h ^ (ulong)Entities.Weapon[i].Acquiring.Value) * Prime;
                    h = (h ^ (ulong)Entities.Weapon[i].Bearing) * Prime;
                    h = (h ^ (ulong)(uint)Entities.Weapon[i].EngagementsRemaining) * Prime;
                    h = (h ^ (ulong)(uint)Entities.Weapon[i].ReloadingUntilTick) * Prime;
                }
            }

            for (int t = 1; t < Players.Length; t++)
            {
                h = (h ^ (ulong)Players[t].Materiel.Raw) * Prime;
                h = (h ^ (ulong)Players[t].TaskingPoints.Raw) * Prime;
                h = (h ^ Players[t].Crews.StateHash()) * Prime;

                // The team's copy of the bubble as well as each munition's. It
                // is what every munition launched from here on will inherit, so
                // a divergence in it is a divergence in the future rather than
                // in anything currently alive - exactly the kind that would
                // otherwise surface minutes later as an unexplained one.
                h = (h ^ (Players[t].HasNoGoBox ? 1UL : 0UL)) * Prime;
                h = (h ^ (ulong)Players[t].NoGoBoxMin.X.Raw) * Prime;
                h = (h ^ (ulong)Players[t].NoGoBoxMin.Y.Raw) * Prime;
                h = (h ^ (ulong)Players[t].NoGoBoxMax.X.Raw) * Prime;
                h = (h ^ (ulong)Players[t].NoGoBoxMax.Y.Raw) * Prime;
                h = (h ^ (ulong)(uint)Players[t].NoGoBoxExpiryTick) * Prime;

                // The track hold's memory (RebuildDetection). It is a function of
                // tick history rather than of this instant's positions, so unlike
                // most of what is hashed above it would not be caught by anything
                // else here if it drifted between platforms.
                if (lastSeenTick != null)
                {
                    int[] last = lastSeenTick[t];
                    for (int i = 1; i < Entities.HighWater; i++)
                        h = (h ^ (ulong)(uint)last[i]) * Prime;
                }

                // And the firing solution's memory, which is a second clock on the
                // same shape and lapses on its own schedule (AUDIT-UNWIRED.md F9).
                // Two peers that disagreed about it would disagree about which
                // contacts their mounts are allowed to shoot at, and about nothing
                // else hashed here.
                if (lastSolutionTick != null)
                {
                    int[] lastSolution = lastSolutionTick[t];
                    for (int i = 1; i < Entities.HighWater; i++)
                        h = (h ^ (ulong)(uint)lastSolution[i]) * Prime;
                }
            }

            // The match's time of day. It used to be a pure function of Tick
            // (which is hashed above) and is now a setting chosen when the world
            // is built, so it is persistent state of its own: two peers that
            // disagreed about whether it is night would disagree about every
            // optical detection from the first tick.
            h = (h ^ (ulong)Phase) * Prime;

            h = (h ^ Territory.StateHash()) * Prime;
            h = (h ^ Imagery.StateHash()) * Prime;
            h = (h ^ (ulong)Weather) * Prime;
            h = (h ^ (ulong)Ground) * Prime;
            h = (h ^ Random.StateHash()) * Prime;
            return h;
        }
    }
}
