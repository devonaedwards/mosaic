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
        public readonly MeshGraph Mesh;
        public readonly TetherSystem Tethers;
        public readonly RandomStreams Random;
        public readonly EventRing Events;
        public readonly PlayerState[] Players;
        public readonly CommandBuffer Commands = new CommandBuffer();

        public int Tick { get; private set; }
        public DayPhase Phase { get; private set; }

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

            if (def.SensorFootprintMetres.Raw > 0)
            {
                Entities.AddComponent(i, ComponentMask.Sensor);
                Entities.Sensor[i] = new SensorState
                {
                    FootprintMetres = def.SensorFootprintMetres,
                    Quality = 60,
                    Optical = def.SensorOptical,
                    Thermal = def.SensorThermal,
                    RadioFrequency = def.SensorRadioFrequency
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
                    InterceptBaseChance = def.InterceptBaseChance
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

            if (def.AutonomyQuality > 0)
            {
                Entities.AddComponent(i, ComponentMask.Autonomy);
                Entities.Autonomy[i] = new AutonomyState
                {
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

            LinkResolver.ResolveAll(this);
            UpdateTethers();
            MovementSystem.Step(this);
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
                    Fix footprint = EffectiveSensorRange(i);
                    if (Fix2.SqrDistance(Entities.Position[i], vp) <= footprint * footprint) return true;
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
        /// Radar sees through darkness and terrain but only detects things in the
        /// air. Everything else is an optical or thermal footprint, and an optical
        /// one collapses to about a third of its reach at night unless the player
        /// has bought thermal imaging. That single fact is why night is when
        /// armour moves and when assaults go in.
        /// </summary>
        public bool IsDetectedBy(byte team, EntityHandle target)
        {
            if (!Entities.IsAlive(target)) return false;
            int ti = target.Index;
            if (Entities.Team[ti] == team) return true;

            Fix2 tp = Entities.Position[ti];
            bool airborne = Entities.EntityLayer[ti] != Layer.Ground;

            for (int i = 1; i < Entities.HighWater; i++)
            {
                if (!Entities.IsSlotAlive(i)) continue;
                if (Entities.Team[i] != team) continue;
                if (!Entities.Has(i, ComponentMask.Sensor)) continue;

                SensorState s = Entities.Sensor[i];

                if (s.RadioFrequency && airborne)
                {
                    Fix r = s.FootprintMetres;
                    if (Fix2.SqrDistance(Entities.Position[i], tp) <= r * r) return true;
                    continue;
                }

                Fix range = EffectiveSensorRange(i);
                if (Fix2.SqrDistance(Entities.Position[i], tp) <= range * range) return true;
            }
            return false;
        }

        /// <summary>
        /// How far a sensor actually sees right now. Optical sensors collapse after
        /// dark unless the player has paid for thermal imaging, which is what makes
        /// night the time to move.
        /// </summary>
        public Fix EffectiveSensorRange(int entityIndex)
        {
            SensorState s = Entities.Sensor[entityIndex];
            Fix range = s.FootprintMetres;
            if (!IsNight) return range;

            byte team = Entities.Team[entityIndex];
            bool thermal = s.Thermal || (team < Players.Length && Players[team].HasThermalOptics);
            if (thermal || !s.Optical) return range;
            return range * SimConstants.NightOpticalDetectionScale;
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
            }

            h = (h ^ Random.StateHash()) * Prime;
            return h;
        }
    }
}
