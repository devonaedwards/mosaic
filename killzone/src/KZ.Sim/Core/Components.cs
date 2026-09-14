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

    /// <summary>
    /// What a thing gives away about itself, on five separate channels.
    ///
    /// This is the other half of the detection problem and the half that was
    /// missing. A sensor has a reach; what it actually reaches depends entirely on
    /// what the target is emitting, and different things emit on different
    /// channels. The consequences fall out on their own:
    ///
    /// A fiber drone transmits nothing at all - that is the point of the fiber -
    /// so no amount of passive radio listening will ever find one. But it is a
    /// quadcopter, and quadcopters are loud, so a microphone hears it exactly as
    /// well as it hears a radio-controlled one. A jammer, conversely, is the
    /// loudest thing on the map on the radio channel and is trivially located the
    /// moment it switches on.
    ///
    /// Values are 0-100 and are not interchangeable between channels. A tank is 90
    /// thermal and 0 radio; a relay mast is 70 radio and 15 thermal.
    /// </summary>
    public struct SignatureProfile
    {
        /// <summary>How loudly it transmits. Zero for fiber and for autonomy.</summary>
        public byte Radio;
        /// <summary>Engine and exhaust heat. A blanket cuts this; darkness does not.</summary>
        public byte Thermal;
        /// <summary>Rotor and engine noise. Multirotors are extremely loud for their size.</summary>
        public byte Acoustic;
        /// <summary>Size and contrast against the ground. This is the one darkness ruins.</summary>
        public byte Visual;
        /// <summary>Radar cross-section. Small airframes are genuinely hard to see.</summary>
        public byte Radar;

        public static SignatureProfile Make(byte radio, byte thermal, byte acoustic, byte visual, byte radar)
        {
            SignatureProfile s;
            s.Radio = radio; s.Thermal = thermal; s.Acoustic = acoustic;
            s.Visual = visual; s.Radar = radar;
            return s;
        }
    }

    /// <summary>
    /// What a thing can detect with. Each figure is the reach against a target
    /// emitting at full strength on that channel; a quieter target is found
    /// closer. Zero means the sensor is not fitted, which is usually the
    /// interesting part - a turret with optics and no microphone is a different
    /// weapon after dark than one with both.
    /// </summary>
    public struct SensorSuite
    {
        /// <summary>Cameras. Cheap, long-ranged, and nearly useless at night.</summary>
        public Fix Optical;

        /// <summary>Thermal imaging. Costs money, and does not care what time it is.</summary>
        public Fix Thermal;

        /// <summary>
        /// Microphones. Short-ranged, unaffected by darkness, and the only thing
        /// that reliably finds a small drone - because a drone that has gone quiet
        /// on every other channel is still a quadcopter.
        /// </summary>
        public Fix Acoustic;

        /// <summary>
        /// Active radar. Long-ranged, sees through weather and darkness and
        /// terrain, and only finds things in the air. Announces itself while doing
        /// it, which is why a radar mast is the loudest building a player owns.
        /// </summary>
        public Fix Radar;

        /// <summary>
        /// Passive listening for transmissions. Finds anything that is talking,
        /// however far away and however dark, and finds nothing that is not. This
        /// is what locates a jammer and what a fiber drone defeats completely.
        /// </summary>
        public Fix Esm;

        public byte Quality;

        public bool HasAny
        {
            get
            {
                return Optical.Raw > 0 || Thermal.Raw > 0 || Acoustic.Raw > 0
                    || Radar.Raw > 0 || Esm.Raw > 0;
            }
        }
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

        /// <summary>
        /// Where the barrel is pointing, and how fast it can be pointed somewhere
        /// else. A mount with one barrel cannot engage two targets at once, and
        /// swinging between a low target close by and a high one further off costs
        /// real time - which is what makes attacking from two altitudes at once a
        /// tactic rather than a preference.
        /// </summary>
        public ushort Bearing;
        public int TraverseBamPerTick;
        public Layer TrackingLayer;
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
        /// <summary>Radio signature while switched on. Switching off is a real option.</summary>
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

    /// <summary>
    /// A mine, laid remotely by a heavy drone.
    ///
    /// This is the grim, cheap, effective answer to ground that cannot be held by
    /// standing on it. You cannot shoot down a minefield, it does not need a
    /// crew, a link or a pilot, and it does not stop being there when you stop
    /// paying attention to it. It is the only thing in the roster that denies
    /// ground rather than destroying something on it.
    ///
    /// It also does not care whose vehicle drives over it. Mines here are armed
    /// against everyone, including the side that laid them - not as a gameplay
    /// punishment but because that is what a mine is, and a game about this
    /// subject should not pretend otherwise.
    /// </summary>
    public struct MineState
    {
        public Fix Damage;
        public DamageType Type;
        public Fix TriggerRadiusMetres;
        public int ArmedAtTick;
        public byte LaidByTeam;
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
