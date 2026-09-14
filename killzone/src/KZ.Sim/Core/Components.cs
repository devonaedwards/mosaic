// KILL ZONE - a real-time strategy video game.
// The mutable per-entity component structs held in the entity table's arrays.

namespace KZ.Sim
{
    /// <summary>
    /// Everything the control-link layer knows about one drone. This is the
    /// signature system of the game: a drone is only as good as the path back to
    /// the human flying it.
    /// </summary>
    public struct LinkState
    {
        public LinkKind Kind;
        public LinkKind AltKind;      // what a dual-link airframe falls back to
        public byte RobustnessBase;
        public byte RobustnessEffective; // base, plus veterancy, upgrades, mesh redundancy
        public byte JamSampled;          // the jamming strength measured here last evaluation
        public LinkPip Pip;
        public byte Hops;                // mesh depth from an anchor
        public byte AltHops;             // how many other viable parents, capped at three
        public BlackPolicy Policy;
        public int AmberTicks;
        public int BlackTicks;
        public EntityHandle Parent;      // the node controlling this drone
        public int LastEvalTick;
        public bool Reparenting;         // briefly orphaned; the next graph rebuild may save it

        public bool IsUnjammable
        {
            get { return Kind == LinkKind.Fiber || Kind == LinkKind.Autonomy; }
        }
    }

    public struct SortieState
    {
        public int CrewId;              // -1 when no crew is flying this
        public SortiePhase Phase;
        public EntityHandle Target;
        public Fix2 DesignatedPoint;    // where a last-mile drone carries on to if the link dies
        public bool HasDesignatedPoint;
        public int SpawnTick;
        public int EgressUntilTick;     // staggered pad departure so a flight does not spawn stacked
        public bool AcceptsNewOrders;
        public bool OneWay;             // the airframe is the munition; it does not come home
    }

    public struct SensorState
    {
        public Fix FootprintMetres;
        public byte Quality;
        public bool Optical;            // optical sensors collapse at night without thermal
        public bool Thermal;
        public bool RadioFrequency;     // sees emitters through the shroud
    }

    public struct WeaponState
    {
        public Fix Damage;
        public DamageType Type;
        public Fix RangeMetres;
        public int CooldownTicks;
        public int NextFireTick;
        public int AcquisitionTicks;
        public int AcquiringUntilTick;
        public EntityHandle Acquiring;
        /// <summary>Interception is the one place a direct attack rolls dice.</summary>
        public bool IsInterceptor;
        public Fix InterceptBaseChance;
    }

    /// <summary>
    /// A jammer. Emitting is loud: an active jammer raises its own signature and
    /// is picked up by any enemy radar through the fog, which is why a jammer is
    /// a posture you have to defend rather than a wall you build once.
    /// </summary>
    public struct EmitterState
    {
        public byte JamStrength;
        public Fix RadiusMetres;
        public bool Active;
        public byte SignatureWhileEmitting;
    }

    public struct AutonomyState
    {
        public byte Quality;            // confidence; decoys drag this down
        public Fix2 BoxMin;
        public Fix2 BoxMax;
        public int BoxExpiryTick;
        public bool HasBox;
        public bool ConsumesCrew;
    }

    public struct MoverState
    {
        public Fix SpeedMetresPerSecond;
        public int TurnRateBamPerTick;
        public Fix SpeedMultiplier;     // link state and terrain scale this
        public Fix AcquisitionMultiplier;
        public bool HasOrder;
        public Fix2 OrderPoint;
        public EntityHandle OrderTarget;
        public byte RadiusClass;
    }

    /// <summary>
    /// A pile of wreckage worth salvaging. Everything that dies leaves one, worth
    /// a third of what it cost, and it rots away in under a minute - so the
    /// richest ground on the map is wherever the last fight happened, and it is
    /// only rich for a moment.
    /// </summary>
    public struct SalvageState
    {
        public Fix Amount;
        public Fix DecayPerTick;
        public int CreatedTick;
    }

    public struct DecoyState
    {
        public TargetKind Mimics;
        public Fix Plausibility;        // how convincing this looks to a classifier
        public int ExpiryTick;
    }

    /// <summary>
    /// One human crew. Crews are the real population cap in this game: airframes
    /// are cheap stock in a hangar, but only a crew can fly one, and only one at
    /// a time. Your army is not how many drones you own, it is how many sorties
    /// you can have in the air at once.
    /// </summary>
    public struct Crew
    {
        public CrewState State;
        public byte Rank;
        public int VerifiedKills;
        public int StateUntilTick;
        public EntityHandle Flying;
        public int SortiesInWindow;
        public int WindowStartTick;
        public EntityHandle HomeQuarters;
        public int LastIdleTick;

        public bool IsAvailable
        {
            get { return State == CrewState.Ready || State == CrewState.Fatigued; }
        }
    }
}
