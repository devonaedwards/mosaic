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

        public World(Terrain terrain, int entityCapacity, int tetherCapacity, ulong matchSeed, int playerCount)
            : this(terrain, entityCapacity, tetherCapacity, matchSeed, playerCount, 0) { }

        /// <summary>
        /// startTick offsets where in the day and night cycle the match begins.
        /// Missions use it to open at dawn or in darkness; experiments use it to
        /// test the same engagement under both.
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
            Phase = DayPhase.Day;
            UpdateDayPhase();
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
            Entities.CageHp[i] = Fix.Zero;
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
                    OneWay = def.OneWay
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
                    Ammo = def.Ammo,
                    CanReachHigh = def.CanReachHigh,
                    AmmoCapacity = def.AmmoCapacity,
                    AmmoRemaining = def.AmmoCapacity,
                    ReloadTicks = SimConstants.Seconds(def.ReloadSeconds),
                    ReloadingUntilTick = 0
                };
            }

            if (def.JamStrength > 0)
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
                TriggerRadiusMetres = Fix.FromInt(12),
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

            UpdateDayPhase();

            if (Tick % SimConstants.SignalRebuildInterval == 0) RebuildSignalField();
            if (Tick % SimConstants.MeshRebuildInterval == 0) RebuildMeshGraphs();

            SweepSensors();
            RebuildDetection();
            LinkResolver.ResolveAll(this);
            UpdateTethers();
            MovementSystem.Step(this);

            // After movement, because navigation error is driven by the distance
            // actually flown this tick, and before combat, because what a drone
            // is wrong by is what it is wrong by when it arrives.
            NavigationSystem.Step(this);

            CombatSystem.Step(this);
            UpdateSalvage();
            UpdateDecoys();
            UpdateMines();

            for (int t = 1; t < Players.Length; t++) Players[t].Crews.Tick(Tick, Events);

            FlushDeaths();
        }

        void UpdateDayPhase()
        {
            int t = Tick % SimConstants.DayNightCycleTicks;
            DayPhase p;
            if (t < SimConstants.DayTicks) p = DayPhase.Day;
            else if (t < SimConstants.DayTicks + SimConstants.DuskTicks) p = DayPhase.Dusk;
            else if (t < SimConstants.DayTicks + SimConstants.DuskTicks + SimConstants.NightTicks) p = DayPhase.Night;
            else p = DayPhase.Dawn;

            if (p != Phase)
            {
                Phase = p;
                Events.Push(SimEventKind.DayPhaseChanged, Tick, EntityHandle.None,
                            EntityHandle.None, 0, (int)p);
            }
        }

        void RebuildSignalField()
        {
            Signal.ClearEmitters();
            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (!Entities.Has(i, ComponentMask.Emitter)) continue;
                if (!Entities.Emitter[i].Active) continue;

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
        /// down from above. A cage soaks shaped-charge damage first, which is the
        /// only thing that lets a tank survive a drone swarm long enough to matter.
        /// </summary>
        public void ApplyDamage(EntityHandle target, Fix baseDamage, DamageType type,
                                bool topAttack, EntityHandle attacker)
        {
            if (!Entities.IsAlive(target)) return;
            int i = target.Index;

            Fix mult = Catalog.DamageMultiplier(type, Entities.Armour[i], topAttack);
            if (mult.Raw == 0) return;

            Fix damage = baseDamage * mult;

            if (type == DamageType.Shaped && Entities.CageHp[i].Raw > 0)
            {
                Fix absorbed = Fix.Min(damage, Entities.CageHp[i]);
                Entities.CageHp[i] -= absorbed;
                damage -= absorbed;
            }

            if (damage.Raw <= 0) return;

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

            return ComputeDetection(team, ti);
        }

        bool[][] detectionCache;
        int detectionCacheTick = -1;

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
                lastSeenTick = new int[Players.Length][];
                for (int t = 0; t < Players.Length; t++)
                {
                    detectionCache[t] = new bool[Entities.Capacity];
                    lastSeenTick[t] = new int[Entities.Capacity];
                    for (int i = 0; i < Entities.Capacity; i++) lastSeenTick[t][i] = -1;
                }
            }

            for (byte team = 1; team < Players.Length; team++)
            {
                bool[] seen = detectionCache[team];
                int[] last = lastSeenTick[team];
                for (int i = 1; i < Entities.HighWater; i++) seen[i] = false;

                for (int i = 1; i < Entities.HighWater; i++)
                {
                    if (!Entities.IsSlotAlive(i)) continue;
                    if (Entities.Team[i] == team) { seen[i] = true; continue; }

                    if (ComputeDetection(team, i))
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
                }
            }
            detectionCacheTick = Tick;
        }

        bool ComputeDetection(byte team, int ti)
        {
            Fix2 tp = Entities.Position[ti];
            Layer layer = Entities.EntityLayer[ti];
            SignatureProfile sig = EffectiveSignature(ti);

            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (Entities.Team[i] != team) continue;
                if (!Entities.Has(i, ComponentMask.Sensor)) continue;

                SensorSuite s = Entities.Sensor[i];
                if (!s.HasAny) continue;

                Fix distSq = Fix2.SqrDistance(Entities.Position[i], tp);

                // Passive listening. Unaffected by darkness or altitude - if it is
                // transmitting, it is transmitting.
                if (Reaches(s.Esm, sig.Radio, Fix.One, distSq, SensorChannel.Esm, ti)) return true;

                // Active radar. Air only, and better against altitude, where there
                // is no ground clutter to pick a small return out of.
                if (layer != Layer.Ground)
                {
                    Fix radarMod = layer == Layer.High
                        ? Fix.FromDoubleContentOnly(1.20)
                        : Fix.FromDoubleContentOnly(0.80);
                    if (Reaches(s.Radar, sig.Radar, radarMod, distSq, SensorChannel.Radar, ti)) return true;
                }

                // Thermal, which is a night sensor and a poor day one. Sunlight
                // heats the background until there is little contrast left to work
                // with, so the imager that owns the small hours is mediocre at noon.
                bool pointedAtIt = WithinArc(s, Entities.Position[i], tp);

                Fix thermalMod = ThermalTimeScale() * s.ApertureRangeScale;
                if (layer == Layer.High) thermalMod = thermalMod * SimConstants.ThermalHighScale;
                if (pointedAtIt
                    && Reaches(s.Thermal, sig.Thermal, thermalMod, distSq, SensorChannel.Thermal, ti))
                    return true;

                // Microphones. Not the universal answer the model used to make
                // them - against a small electric quad a camera still beats them,
                // even after dark. What they are is the one sensor that gets a
                // piston-engined thing at range, and the one that gets better at
                // night rather than worse.
                if (Reaches(s.Acoustic, sig.Acoustic, AcousticTimeScale(layer), distSq,
                            SensorChannel.Acoustic, ti)) return true;

                // Cameras. Long reach in daylight, and after dark this is the line
                // that stops being true.
                Fix opticalMod = s.ApertureRangeScale;
                if (layer == Layer.High) opticalMod = opticalMod * SimConstants.OpticalHighScale;
                if (IsNight && !TeamHasThermalOptics(team))
                    opticalMod = opticalMod * SimConstants.NightOpticalDetectionScale;
                if (pointedAtIt
                    && Reaches(s.Optical, sig.Visual, opticalMod, distSq, SensorChannel.Optical, ti))
                    return true;
            }
            return false;
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
        bool Reaches(Fix sensorRange, byte signature, Fix modifier, Fix distSq,
                     SensorChannel channel, int targetIndex)
        {
            if (sensorRange.Raw <= 0 || signature == 0) return false;

            Fix effective = sensorRange * modifier * WeatherScale(channel)
                            * SignatureScale(signature, channel);
            if (effective.Raw <= 0) return false;
            if (distSq > effective * effective) return false;

            Fix solid = effective * SimConstants.DetectionSolidFraction;
            if (distSq <= solid * solid) return true;

            // Out at the edge. Roll, but keyed to the tick and the pair rather than
            // drawn fresh each call, so one target does not resolve differently for
            // two sensors in the same instant.
            int reliability = ChannelReliability(channel);
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
                    if (layer == Layer.Ground) return Fix.Zero;
                    nominal = s.Radar; strength = sig.Radar;
                    mod = layer == Layer.High ? Fix.FromDoubleContentOnly(1.20)
                                              : Fix.FromDoubleContentOnly(0.80);
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
            return nominal * mod * WeatherScale(channel) * SignatureScale(strength, channel);
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

                if (Entities.Has(i, ComponentMask.Link))
                {
                    h = (h ^ (ulong)Entities.Link[i].Pip) * Prime;
                    h = (h ^ (ulong)Entities.Link[i].JamSampled) * Prime;
                    h = (h ^ (ulong)Entities.Link[i].AmberTicks) * Prime;
                    h = (h ^ (ulong)Entities.Link[i].Hops) * Prime;
                }
            }

            for (int t = 1; t < Players.Length; t++)
            {
                h = (h ^ (ulong)Players[t].Materiel.Raw) * Prime;
                h = (h ^ (ulong)Players[t].TaskingPoints.Raw) * Prime;
                h = (h ^ Players[t].Crews.StateHash()) * Prime;

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
            }

            h = (h ^ Territory.StateHash()) * Prime;
            h = (h ^ Imagery.StateHash()) * Prime;
            h = (h ^ (ulong)Weather) * Prime;
            h = (h ^ (ulong)Ground) * Prime;
            h = (h ^ Random.StateHash()) * Prime;
            return h;
        }
    }
}
