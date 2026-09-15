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

        /// <summary>
        /// Currently outside its own side's satellite coverage. Held rather than
        /// recomputed so the crossing fires once, in each direction, instead of
        /// every tick the drone spends over the wrong ground.
        /// </summary>
        public bool OutsideCoverage;

        public bool IsUnjammable
        {
            get { return Kind == LinkKind.Fiber || Kind == LinkKind.Autonomy; }
        }
    }

    /// <summary>
    /// Where a drone thinks it is, which stops being the same as where it is the
    /// moment nobody can tell it.
    ///
    /// The research this is built on overturned the obvious design twice, so both
    /// corrections are recorded here rather than in a commit nobody will read.
    ///
    /// <para><b>Celestial navigation is not a position fix.</b> It is tempting to
    /// treat a star tracker as the answer to satellite denial, and every piece of
    /// press coverage reads that way. A star tracker measures orientation against
    /// the star field. Getting a *position* out of that needs the star's angle
    /// against local vertical, and an aircraft has no horizon, so local vertical
    /// comes from the inertial unit - which is exactly the thing that was wrong in
    /// the first place. The conversion is brutal and published: one arc-second of
    /// vertical deflection is thirty metres of position error. So celestial bounds
    /// *heading* drift, which kills the fastest-growing term in inertial error
    /// without ever fixing position. It is a drift-rate modifier, and modelling it
    /// as a periodic reset to zero would be wrong.</para>
    ///
    /// <para><b>And it is not reaching cheap airframes.</b> A fielded daylight-
    /// capable star tracker is a quarter of a million; the cheap demonstrator is
    /// night-only and accurate to four kilometres. It belongs on expensive
    /// platforms and nowhere else.</para>
    /// </summary>
    public struct NavState
    {
        public NavAid Aid;

        /// <summary>
        /// Bounds heading drift. Expensive, and deliberately not a position fix -
        /// see the note above.
        /// </summary>
        public bool CelestialHeading;

        /// <summary>
        /// Current aimpoint error, in real metres. What the drone is wrong by.
        /// </summary>
        public Fix ErrorMetres;

        /// <summary>
        /// Distance flown since the last absolute fix. Error is driven by distance
        /// rather than by time because for anything cruising, the dominant unknown
        /// is the wind it has been flying through, not the clock.
        /// </summary>
        public Fix MetresSinceFix;

        /// <summary>
        /// Whether scene matching currently has a lock. Losing it is a cliff, not
        /// a slope: a recursive estimator degrades quietly and then latches onto
        /// the wrong answer entirely.
        /// </summary>
        public bool HasLock;

        /// <summary>
        /// Distance flown over ground it can match while trying to get a lock
        /// back. Re-acquisition is not instant and not free.
        /// </summary>
        public Fix ReacquireProgress;
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

        /// <summary>
        /// The pad this sortie launched from. A reusable airframe hands its
        /// crew back (MovementSystem.CheckLandings) once it is close enough to
        /// this again - AUDIT-UNWIRED.md F13.
        /// </summary>
        public Fix2 HomePosition;

        /// <summary>
        /// True once this sortie has actually flown outside landing range of
        /// its own pad. Guards the landing check above: without it, a drone
        /// would "land" on the very tick it launched, since it starts at
        /// HomePosition, and hand its just-assigned crew straight back.
        /// </summary>
        public bool HasLeftHome;
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
        /// <summary>
        /// How far a microphone gets this, as a fraction of that microphone's
        /// maximum reach - not loudness. The distinction matters: a quadcopter is
        /// piercing at ten metres and gone at three hundred, because what it
        /// radiates is high-frequency and the air eats it. A two-stroke engine is
        /// no louder up close and is heard for kilometres. Acoustic detection range
        /// across airframes spans something like twelve to one, and putting
        /// loudness in this field flattened that to about two to one.
        /// </summary>
        public byte Acoustic;
        /// <summary>Size and contrast against the ground. This is the one darkness ruins.</summary>
        public byte Visual;
        /// <summary>
        /// Radar cross-section, in decibels rather than as a linear index:
        /// <c>S = 2 x RCS_dBsm + 80</c>, so 80 is one square metre and two points
        /// is one decibel. Reach goes as the fourth root of cross-section, which
        /// works out to <c>10^((S-80)/80)</c>.
        ///
        /// A decibel scale rather than a linear one because the real spread is
        /// enormous - a plastic quadcopter and a corner reflector are three orders
        /// of magnitude apart - and a linear 0-100 index cannot hold that. Read
        /// linearly, a decoy at 80 escorting a drone at 60 pulled fire seven
        /// percent further out, which is not a decoy.
        /// </summary>
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

        /// <summary>
        /// How wide an arc the pointed sensors cover, in degrees. 360 means the
        /// head sees all round.
        ///
        /// This is the trade nobody escapes. A camera or an imager has a fixed
        /// number of pixels to spend, and it can spend them on a narrow slice of
        /// the world seen in detail a long way off, or a wide slice seen poorly
        /// close in. It cannot have both. So a mount either buys a narrow head and
        /// accepts blind sides, buys several heads and pays several times, or
        /// sweeps one head and accepts that it is looking somewhere else most of
        /// the time.
        ///
        /// Microphones and radio antennas are exempt - they are omnidirectional by
        /// nature, which is exactly why they are the cheap way to know something is
        /// out there and the useless way to know precisely where.
        /// </summary>
        public int DirectionalArcDegrees;

        /// <summary>Where the pointed sensors are looking.</summary>
        public ushort Facing;

        /// <summary>
        /// How fast the head sweeps, in degrees per second. Zero means it stares.
        /// A sweeping head eventually covers everything and is looking at any
        /// particular thing only a fraction of the time, so a fast target can cross
        /// a covered sector between one pass and the next.
        /// </summary>
        public int ScanDegreesPerSecond;

        /// <summary>
        /// What spreading the pixels over a wider arc costs in reach. Ninety
        /// degrees is taken as the reference, so a narrow head sees further and a
        /// panoramic one sees a good deal less.
        /// </summary>
        public Fix ApertureRangeScale
        {
            get
            {
                int arc = DirectionalArcDegrees <= 0 ? 360 : DirectionalArcDegrees;
                Fix ratio = Fix.FromInt(90) / Fix.FromInt(arc);
                Fix scale = Fix.Sqrt(ratio);
                return Fix.Clamp(scale, Fix.FromDoubleContentOnly(0.40),
                                        Fix.FromDoubleContentOnly(2.20));
            }
        }

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

        /// <summary>
        /// The one target this mount is currently prosecuting, held across
        /// ticks rather than re-chosen every time it comes off cooldown.
        ///
        /// point-defence.md §"Q3. How many simultaneous targets, and what does
        /// saturation look like?": every serial effector in the table - MG
        /// turret, autocannon, laser, guided rocket - shows "Simultaneous
        /// engagements: 1", and the design consequence is explicit: "a
        /// point-defence unit in the game should have an explicit engagement
        /// channel count (almost always 1)". Before this field existed the
        /// channel count actually was one, tick to tick, because only one
        /// StepOne call happens per mount per tick - but nothing stopped the
        /// mount re-running BestTargetInRange every time it came off cooldown
        /// and hopping onto whichever target scored highest that instant.
        /// That is not one engagement channel with a cooldown, it is free
        /// re-targeting with a cooldown attached, and it is a direct cause of
        /// FINDINGS #25: the reflector decoy measured as having zero effect
        /// because nothing ever made the mount spend a whole engagement on it
        /// instead of splitting attention with whatever arrived alongside it.
        ///
        /// Held until the target dies, leaves the envelope (out of range, or
        /// no longer engageable), or the magazine runs dry - see
        /// CombatSystem.CommittedOrBestTarget. Breaking it early for a target
        /// worth more is allowed in exactly one case there, and it costs
        /// exactly the SlewTicks/AcquisitionTicks that re-laying onto any new
        /// target already costs - commitment does not add a second charge on
        /// top of the traverse cost that already prices switching targets.
        /// </summary>
        public EntityHandle CommittedTarget;

        /// <summary>What the mount is loaded with, which changes the shape of its
        /// hit curve far more than its damage.</summary>
        public AmmoType Ammo;

        /// <summary>Whether it can engage the high band at all.</summary>
        public bool CanReachHigh;

        /// <summary>
        /// How many engagements are left before it has to reload, and how long that
        /// takes.
        ///
        /// This is what makes saturating a defence mean something. A mount that
        /// never runs dry can only be beaten by out-shooting it; a mount with a
        /// magazine can be beaten by making it spend one. It is also the real cost
        /// of the clever ammunition - the rounds that are forgiving of a bad aiming
        /// solution are bulky and expensive, so you carry far fewer of them.
        /// </summary>
        /// <summary>
        /// How many more targets this mount can prosecute before it has to reload,
        /// and how many it gets from a full belt.
        ///
        /// The unit is <b>engagements, not rounds</b>, and the distinction is the
        /// whole reason these are not called ammunition. A burst against a drone is
        /// twenty to fifty rounds fired in under a second at eight hundred and
        /// fifty rounds a minute; what takes the time is re-laying and re-acquiring
        /// afterwards, three to eight seconds of it. A hundred-round belt is
        /// therefore three to six *targets*, not a hundred of anything.
        ///
        /// Counting rounds would model the wrong scarce thing. The mount does not
        /// run out mid-burst; it runs out of targets it can take before somebody
        /// has to stand up in the open and feed it.
        /// </summary>
        public int EngagementsRemaining;
        public int EngagementsPerBelt;
        public int ReloadTicks;
        public int ReloadingUntilTick;
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
        /// <summary>
        /// Which of the two autonomies this is. The distinction decides whether a
        /// crew is consumed, whether decoys work, and whether accuracy goes up or
        /// down - and those three answers differ between the tiers, which is why
        /// one field could never carry it.
        /// </summary>
        public AutonomyTier Tier;

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

        /// <summary>
        /// How far this thing actually moved on the last tick, after terrain and
        /// bounds had their say. Navigation error is driven by distance flown
        /// rather than by time elapsed, so it needs the distance that happened
        /// rather than the one the speed implied.
        /// </summary>
        public Fix LastStepDistance;
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
