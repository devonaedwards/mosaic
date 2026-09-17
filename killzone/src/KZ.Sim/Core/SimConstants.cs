// KILL ZONE - a real-time strategy video game.
// Every tuning number the simulation depends on, in one place, in ticks and
// REAL metres. These are balance values for a game: they exist to be argued
// about in playtests and changed.
//
// Units, since this file is where they are decided (docs/SCALE.md, "What robust
// looks like"):
//
//   Distance   real metres. Not "map metres" at some compression - the kill
//              zone is 22 km deep whoever is looking at it, and a number in
//              real units can be checked against the reporting in
//              docs/research, which is the whole method here. A number in
//              compressed units cannot.
//   Time       real seconds for anything with a physical rate, converted to
//              ticks by Seconds(). PlaySeconds() exists for the few durations
//              that are pacing rather than physics, and says so.
//   Speed      real metres per real second.
//
// The game still plays faster than life: TimeMultiplier below advances the
// world four seconds for every second at the controls. It is one number,
// applied once, in Dt - not a compression baked separately into every speed,
// which is the state this file was in before (an FPV at 1,469 km/h, four
// airframes disagreeing about the scale by a factor of three, and a day/night
// cycle disagreeing with all of them by a factor of fifty).

namespace KZ.Sim
{
    public static class SimConstants
    {
        // ---- time ---------------------------------------------------------

        /// <summary>
        /// Ticks per second of play. 32 rather than 30 because 1/32 is exact in
        /// binary, so dt introduces no rounding drift over a 25-minute match. It
        /// also makes every derived duration a clean power-of-two count of ticks.
        /// </summary>
        public const int TicksPerSecond = 32;

        /// <summary>
        /// How many seconds of world the simulation advances per second of play.
        ///
        /// The one explicit knob docs/SCALE.md asks for, replacing a distance
        /// compression that had been baked separately into every speed and never
        /// reconciled. At 4x an FPV crosses the 22 km kill zone in 2.3 minutes of
        /// play and a supply truck in 6.1 - the two-minute sortie the map-size
        /// argument reasoned toward, now derived from a real speed rather than
        /// reverse-engineered from a feel.
        ///
        /// It is global and it is a match setting, never a camera setting:
        /// lockstep requires every participant to simulate the same ticks, so a
        /// player who could speed up their own clock would simply be playing a
        /// different match. Zoom is the per-player dial; this is not.
        ///
        /// 4 rather than any other number because it divides TicksPerSecond
        /// exactly, so Dt stays a power of two and a real second stays a whole
        /// number of ticks. A multiplier that did not divide 32 would reintroduce
        /// rounding into the one place this codebase has been careful to keep it
        /// out of.
        /// </summary>
        public const int TimeMultiplier = 4;

        /// <summary>Ticks in one real-world second: 8, at 32 Hz and 4x.</summary>
        public const int TicksPerRealSecond = TicksPerSecond / TimeMultiplier;

        /// <summary>Seconds of world per tick. Speeds are real metres per real second.</summary>
        public static readonly Fix Dt = new Fix(Fix.OneRaw / TicksPerRealSecond);

        /// <summary>Real-world seconds to ticks. Everything with a physical rate uses this.</summary>
        public static int Seconds(int s) { return s * TicksPerRealSecond; }

        /// <summary>
        /// Real-world milliseconds to ticks. Granularity is 125 ms at 4x, so this
        /// is only honest for durations of a second or more.
        /// </summary>
        public static int Millis(int ms) { return (ms * TicksPerRealSecond) / 1000; }

        /// <summary>
        /// Seconds of *play* to ticks, for the handful of durations that are
        /// pacing rather than physics - a build timer is not a thing anything in
        /// the world does at a rate, it is how long the player waits. Keeping
        /// those in play seconds is what stops the rescale quietly making every
        /// production queue four times faster. Anything with a source in
        /// docs/research uses Seconds() instead.
        /// </summary>
        public static int PlaySeconds(int s) { return s * TicksPerSecond; }

        /// <summary>Commands are gathered into 4-tick turns (125 ms) for network play.</summary>
        public const int TicksPerCommandTurn = 4;

        // ---- the map ------------------------------------------------------

        // The three grids below are engine resolution, not measurements of
        // anything. They were 8 / 32 / 16 against a 2,048-map-metre map and are
        // twelve times that against the same ground in real metres, so every grid
        // is the same number of cells across as it was: the signal field still
        // rebuilds 64x64 cells rather than 768x768, and a terrain array still
        // costs 65 kB rather than 9 MB. A finer grid is a cost decision to argue
        // separately, not something the change of units should have made for us.

        /// <summary>One build tile is 96 m. Structure footprints are given in tiles.</summary>
        public const int BuildTileMetres = 96;

        /// <summary>Jamming and threat are tracked on a coarser 384 m grid.</summary>
        public const int SignalCellMetres = 384;

        /// <summary>Vision sits between the two, at 192 m.</summary>
        public const int VisionCellMetres = 192;

        /// <summary>
        /// The longest reach the simulation will compare against a squared
        /// distance, in real metres.
        ///
        /// Q31.32 stores two billion, so a 1.2 million metre front is nothing to
        /// it - but detection and weapon tests compare *squared* distances to
        /// avoid a square root per pair per tick, and a square is what runs out
        /// first: 46,340 m is the largest range whose square still fits, and
        /// beyond it Fix.MulRaw wraps and a sensor silently sees nothing at all.
        /// That is the real headroom limit of the move to real metres, and it is
        /// reached in practice: a radar mast at 16.8 km against a decoy built to
        /// return 3.2x its own cross-section works out to 53 km of nominal reach.
        ///
        /// Capping is the right answer rather than a trap to avoid, because a
        /// sensor that reaches beyond the map already sees all of it. It also
        /// caps the map: the same arithmetic puts the largest safe square map at
        /// about 32 km a side (its diagonal squared is what overflows), so the
        /// standard 24.6 km map is fine and a doubled one is not.
        /// </summary>
        public static readonly Fix MaxComparableRangeMetres = Fix.FromInt(45000);

        /// <summary>The signal grid is rebuilt at 8 Hz; jam fields do not move fast.</summary>
        public const int SignalRebuildInterval = 4;

        /// <summary>The mesh relay graph is rebuilt at 8 Hz.</summary>
        public const int MeshRebuildInterval = 4;

        // ---- the control-link layer ---------------------------------------

        /// <summary>
        /// A jammed drone flies amber for four seconds of play before the link is
        /// considered lost. Long enough to react, short enough to hurt - which is
        /// a statement about the player's hands, not about a radio, so it is
        /// PlaySeconds and stays at the 128 ticks it has always been.
        /// </summary>
        public static readonly int AmberToBlackTicks = PlaySeconds(4);

        /// <summary>
        /// A drone with a dead link orbits for twelve seconds of play, then is
        /// lost. The same argument as above: it is the window in which a player
        /// can do something about it.
        /// </summary>
        public static readonly int BlackToLostTicks = PlaySeconds(12);

        /// <summary>
        /// Dual-link airframes fall back to their alternate link after two real
        /// seconds. This one is the radio and not the player - re-acquiring a mesh
        /// or satellite path takes as long as it takes - so it converts, and at 4x
        /// it costs a quarter of the play time it used to.
        /// </summary>
        public static readonly int DualLinkSwitchTicks = Seconds(2);

        /// <summary>
        /// Jamming falls off linearly from the emitter and is scaled by 1.3, so a
        /// bubble bites hardest at its centre and frays at the edge. Skirting the
        /// rim of a jammer is a real and learnable play.
        /// </summary>
        public static readonly Fix JamFalloffScale = Fix.FromDoubleContentOnly(1.3);

        /// <summary>
        /// Within this margin of a drone's robustness, the coarse grid value is not
        /// trusted and the exact distance to each contributing emitter is evaluated.
        /// Without it a drone flickers green/amber as it crosses a cell border.
        /// </summary>
        public const int JamBoundaryRefineMargin = 12;

        /// <summary>A robustness of 255 means "nothing to jam" - fiber and autonomy.</summary>
        public const int UnjammableRobustness = 255;

        /// <summary>
        /// How far one airborne relay hop carries, in real metres. Designer
        /// estimate: the corpus gives control ranges for ground robots (a Kuryer
        /// at 3-10 km, ground-logistics.md §3) and nothing for a drone-to-drone
        /// relay leg, so this is the old 700 map metres read at the 12:1 the rest
        /// of the catalogue was written against, which lands inside that band.
        /// </summary>
        public const int MeshRangePerHopMetres = 8400;
        public const int MeshMaxHops = 4;
        public const int MeshRobustnessPerAltHop = 5;
        public const int MeshRobustnessAltCap = 15;
        public const int MeshAcquisitionPenaltyTicksPerHop = 13;

        /// <summary>
        /// Direct radio control range, real metres. Designer estimate on the same
        /// footing as the hop above - ground-logistics.md §3's 3-10 km control
        /// range for a tele-operated ground robot is the nearest sourced figure,
        /// and an airborne link with line of sight reaches further than one
        /// talking to a robot in a ditch.
        /// </summary>
        public const int RadioRangeMetres = 12000;

        /// <summary>A veteran crew pushes a drone through interference a rookie would lose.</summary>
        public const int VeteranLinkRobustnessBonus = 8;

        // ---- fiber tethers ------------------------------------------------

        public const int TetherMaxNodes = 64;

        /// <summary>
        /// How far apart the thread's geometry is sampled, real metres. Engine
        /// resolution rather than a measurement, scaled with everything else so
        /// the 64-node cap still describes the same shape of flight path.
        /// </summary>
        public const int TetherNodeSpacingMetres = 144;

        /// <summary>Three seconds of play at full stretch before the line parts.</summary>
        public static readonly int TetherTautGraceTicks = PlaySeconds(3);

        /// <summary>
        /// A cut thread stays on the map for thirty seconds of play. It still
        /// leads home. Map clutter is a pacing question, not a physical one.
        /// </summary>
        public static readonly int TetherLingerTicks = PlaySeconds(30);

        /// <summary>Each tether checks its newest segment plus one older one per tick.</summary>
        public const int TetherSegmentsPerTick = 2;

        // ---- crews and sorties ---------------------------------------------

        // Crew timers are pacing, and deliberately so. A crew turning a sortie
        // round in eight seconds is not a claim about people; it is how long the
        // player waits before the roster refills, and the real thing takes an
        // afternoon. So these stay in seconds of play at the values they have
        // always had rather than becoming four times faster for free.
        public static readonly int CrewRecoveryTicks = PlaySeconds(8);
        public static readonly int CrewRecoveryPriorityTicks = 160;  // 5 s of play with Priority Recovery
        public static readonly int CrewFatiguedRecoveryTicks = 448;  // 14 s of play
        public const int CrewFatigueSortieThreshold = 5;
        public static readonly int CrewFatigueWindowTicks = PlaySeconds(120);
        public static readonly int CrewFatigueClearIdleTicks = PlaySeconds(30);
        public static readonly int CrewBenchedTicks = PlaySeconds(45);  // remote piloting bays

        public const int CrewRankUpKills2 = 3;
        public const int CrewRankUpKills3 = 9;
        public const int CrewRankUpKills4 = 20;

        public const int StartingCrews = 6;
        public const int CrewsPerQuarters = 4;
        public const int MaxCrews = 30;

        // ---- interception ---------------------------------------------------
        //
        // How much of a computed lead an interceptor is actually flown to, by what
        // the side vectoring it is holding the target on. This is the whole of the
        // cue-and-vector chain: a radar track measures closing rate and the
        // interceptor is sent to the meeting point; an optical track infers it from
        // image scale and sends the interceptor somewhere short of it; no track at
        // all leaves it pointed at where the target is, which against anything
        // faster than the interceptor is a chase it cannot win.

        /// <summary>
        /// point-defence.md's effector table gives the autocannon "organic AESA
        /// search/track + EO/IR; measured velocity". A measured velocity is a
        /// solution, so a radar-cued interceptor flies the whole computed lead.
        /// </summary>
        public static readonly Fix InterceptLeadRadarTrack = Fix.FromDoubleContentOnly(1.00);

        /// <summary>
        /// Designer estimate. The same table gives the AI machine-gun turret
        /// "passive EO/IR only... no velocity measurement" - it reads closing rate
        /// off image scale - and the document says a firing solution is most
        /// sensitive to exactly that measurement, but gives no figure for how far
        /// short the estimate falls. 0.55 is chosen so that an optically-cued pair
        /// bracketing the residual is worth about what one radar-cued interceptor
        /// is, which is the trade the mechanic exists to offer.
        /// </summary>
        public static readonly Fix InterceptLeadOpticalTrack = Fix.FromDoubleContentOnly(0.55);

        /// <summary>
        /// The longest flight time an intercept solution is computed over. Beyond
        /// this the extrapolation is fantasy - a target twenty seconds out will have
        /// manoeuvred - and the solution is recomputed every tick anyway, so the cap
        /// costs nothing and stops an aimpoint being thrown off the map by a
        /// near-parallel geometry.
        /// </summary>
        public static readonly Fix InterceptMaxLeadSeconds = Fix.FromInt(20);

        /// <summary>Drones leave the pad a few ticks apart so a flight does not spawn stacked.</summary>
        public const int SortiePadEgressBaseTicks = 8;
        public const int SortiePadEgressPerIndexTicks = 4;

        // ---- economy -------------------------------------------------------

        /// <summary>
        /// Every destroyed unit drops salvage worth 35% of its cost, right where it
        /// died, and it decays away in fifty seconds. This is the heart of the
        /// economy: the richest ground on the map is always the corpse of the last
        /// fight, in the middle, in daylight.
        /// </summary>
        public static readonly Fix SalvageFraction = Fix.FromDoubleContentOnly(0.35);

        /// <summary>Fifty seconds of play to nothing. A window to react in, so PlaySeconds.</summary>
        public static readonly int SalvageDecayTicks = PlaySeconds(50);

        /// <summary>Verified kills pay 12% of the victim's cost as Tasking Points.</summary>
        public static readonly Fix TaskingPointsUnitFraction = Fix.FromDoubleContentOnly(0.12);
        public static readonly Fix TaskingPointsStructureFraction = Fix.FromDoubleContentOnly(0.08);
        public static readonly Fix TaskingPointsUnverifiedScale = Fix.FromDoubleContentOnly(0.5);

        /// <summary>
        /// A kill counts as verified if a friendly saw it happen within this range,
        /// in real metres. Designer estimate: the corpus is emphatic that strike
        /// confirmation is a real problem and gives no radius for it, so this is
        /// the old 400 map metres at 12:1.
        /// </summary>
        public const int VerificationWitnessRangeMetres = 4800;

        // ---- the time of day a match is fought at ----------------------------

        /// <summary>
        /// A match happens *at* a time of day. It does not contain one.
        ///
        /// This used to be a cycle the match ran through: a full day every six
        /// minutes of play, which is 240x real time against a simulation whose
        /// airframes were at 3-10x and whose turret slews over 71 ticks. At the 4x
        /// this file now runs at, a real 24-hour cycle is six hours of play, so
        /// the two cannot share a match at all - docs/SCALE.md, "What this costs,
        /// and the one thing it cannot buy". The honest resolution is the one that
        /// document reaches: the phase is fixed when the match is set up and never
        /// changes, and the cycle belongs to the campaign layer, whose clock
        /// already runs in months.
        ///
        /// What survives here is a mapping, not a duration. A match is opened with
        /// a clock reading - the campaign hands one over, an experiment picks one -
        /// and these four spans carve that reading into the four phases. The
        /// numbers are deliberately unchanged from the cycle they replace, because
        /// nothing lives through them any more and changing them would silently
        /// move every match that was set up as "night" into daylight.
        /// </summary>
        /// <remarks>
        /// Private, and deliberately: they are the internals of TimeOfDayAt, not
        /// dials anything else should read. Nothing outside can usefully do
        /// arithmetic on a phase boundary now that no match crosses one, and the
        /// dead-symbol guard is right that a constant which only feeds another
        /// constant is not wired to anything. The public surface is the function.
        /// </remarks>
        const int TimeOfDayDialTicks = 11520;
        const int DayTicks = 5760;
        const int DuskTicks = 1440;
        const int NightTicks = 3360;
        const int DawnTicks = 960;

        /// <summary>
        /// Which phase a match opened on this clock reading is fought in, for its
        /// whole length. Integer-only and total: the four spans above tile the
        /// dial, so every reading lands somewhere and no match is phaseless.
        /// </summary>
        public static DayPhase TimeOfDayAt(int clockTicks)
        {
            int t = clockTicks % TimeOfDayDialTicks;
            if (t < 0) t += TimeOfDayDialTicks;

            if (t < DayTicks) return DayPhase.Day;
            t -= DayTicks;
            if (t < DuskTicks) return DayPhase.Dusk;
            t -= DuskTicks;
            if (t < NightTicks) return DayPhase.Night;
            t -= NightTicks;
            if (t < DawnTicks) return DayPhase.Dawn;
            return DayPhase.Day;
        }

        /// <summary>
        /// Optical sensors collapse at night unless the player has bought thermal.
        ///
        /// 0.20 rather than the 0.35 FINDINGS §12 measured the turret's night
        /// reach with. Keeping 65% implied a low-light sensor and a target with
        /// some brightness of its own; for a small matte unlit drone the honest
        /// figure is close to zero, and an entire night-bombing doctrine exists
        /// because of it. The one paired measurement that supported 0.35 was for
        /// illuminated road vehicles - the best case, not the typical one.
        /// thermal-optical.md §10 "Optical night 0.35 - too generous".
        /// </summary>
        public static readonly Fix NightOpticalDetectionScale = Fix.FromDoubleContentOnly(0.20);

        /// <summary>
        /// Thermal imaging is worse in daylight, not better.
        ///
        /// It works on temperature contrast, and sunlight heats the background
        /// until there is very little contrast left - the same imager that picks a
        /// warm engine out of a cold sky at four in the morning struggles past a
        /// few hundred metres on a hot afternoon. This is the opposite of the
        /// intuition that "thermal is the night sensor and neutral by day", and the
        /// model had it wrong in exactly that way.
        ///
        /// 0.62 and 1.15 rather than the 0.55 and 1.25 of FINDINGS §19. The
        /// direction was right; the ratio (2.27) sat at the top of the 1.80-2.23
        /// band the radiometric model gives for sky-backed targets, and 1.85
        /// centres it. The research calls this a minor tuning change.
        /// thermal-optical.md §10 "Thermal day 0.55 / night 1.25".
        /// </summary>
        public static readonly Fix ThermalDayScale = Fix.FromDoubleContentOnly(0.62);
        public static readonly Fix ThermalNightScale = Fix.FromDoubleContentOnly(1.15);
        /// <summary>
        /// Dawn and dusk are the worst hour for thermal, not a midpoint between the
        /// other two. Everything the sun warmed all day passes back down through
        /// the temperature of the things hiding in it, and for a while a vehicle and
        /// the field it is parked in read the same. The old 0.90 treated twilight as
        /// nearly-night; it is closer to worse-than-noon.
        /// </summary>
        public static readonly Fix ThermalTwilightScale = Fix.FromDoubleContentOnly(0.65);

        /// <summary>
        /// Microphones get better after dark, and by more than anything else in the
        /// model moves. The ambient floor drops ten to fifteen decibels between
        /// midday and a quiet night, and the dawn inversion bends sound back down to
        /// the ground on top of that.
        ///
        /// This is the point of modelling it: night is no longer a straight
        /// attacker advantage. It buys a great deal against cameras and costs
        /// something real against microphones, which is a decision rather than a
        /// default.
        ///
        /// Refraction only helps against things flying low - a high target sits on
        /// a steep ray path that bending barely touches - so high targets get the
        /// quieter air and not the bent path.
        /// </summary>
        public static readonly Fix AcousticDayScale = Fix.One;
        public static readonly Fix AcousticNightScale = Fix.FromDoubleContentOnly(2.00);
        public static readonly Fix AcousticDawnScale = Fix.FromDoubleContentOnly(2.50);
        public static readonly Fix AcousticDuskScale = Fix.FromDoubleContentOnly(1.60);
        /// <summary>What a high target gets instead, on any of the above.</summary>
        public static readonly Fix AcousticHighNightScale = Fix.FromDoubleContentOnly(1.50);

        /// <summary>
        /// What altitude costs a microphone. The old 0.35 was doing two jobs at
        /// once - standing in for both the distance and the fact that small
        /// electric aircraft are inaudible up there - and with honest per-airframe
        /// signatures the second job is done by the signature. A quad at altitude
        /// now works out to a handful of metres of reach on its own.
        /// </summary>
        public static readonly Fix AcousticHighScale = Fix.FromDoubleContentOnly(0.50);

        /// <summary>
        /// What altitude does to a camera and a heat sensor - and the answer is
        /// that it helps them. The model used to take 30% off optical and 20% off
        /// thermal for a high target, which made height a hiding place from
        /// exactly the two sensors it exposes a target to: a sky-silhouetted
        /// airframe is the best optical background there is, and against a cold
        /// sky the thermal contrast gate never binds. The penalty those numbers
        /// were groping for is what the target is seen *against*, which is the
        /// ground-clutter term (audit F11), not altitude.
        /// thermal-optical.md §10 "High-altitude modifiers - optical sign is wrong":
        /// optical ×1.00 (was 0.70), thermal ×1.30 (was 0.80).
        /// </summary>
        public static readonly Fix OpticalHighScale = Fix.One;
        public static readonly Fix ThermalHighScale = Fix.FromDoubleContentOnly(1.30);

        // ---- radar: clutter, and the Doppler notch ---------------------------

        /// <summary>
        /// What band the target is in does to a radar's reach. High is clean sky;
        /// low is the clutter the whole sensor struggles with; the ground is the
        /// clutter, by definition.
        ///
        /// The first two are the 1.20 / 0.80 that were written inline in
        /// World.cs twice over. The third is new, and it is the one that needed
        /// a number: radar-rf.md §4 recommends a three-band altitude modifier of
        /// x1.25 / x1.00 / x0.40 and calls the bottom band nap-of-the-earth, so a
        /// target actually on the earth gets that band. It is an extension of a
        /// sourced figure rather than the figure itself, and it lands where the
        /// research says it should: §2.7 quotes the O.W.L. 3D radar at 12 km on
        /// vehicles, and a Radar Mast's 16,800 m against a moving tank at
        /// signature 94 works out to 9.5 km - the right order, inside the vendor
        /// figure rather than beyond it.
        /// </summary>
        public static readonly Fix RadarHighScale = Fix.FromDoubleContentOnly(1.20);
        public static readonly Fix RadarLowScale = Fix.FromDoubleContentOnly(0.80);
        public static readonly Fix RadarGroundScale = Fix.FromDoubleContentOnly(0.40);

        /// <summary>
        /// The Doppler notch, which radar-rf.md finding 12 calls "the missing
        /// mechanic" and §2B.3 "the single most important mechanic in the radar
        /// model".
        ///
        /// Ground clutter sits at zero Doppler and is spread by wind in the
        /// vegetation. A return whose radial velocity falls inside that spread is
        /// not a weak return, it is buried - so the notch is a hard gate and not
        /// a multiplier, however large the cross-section. §2B.3's table, read
        /// straight:
        ///
        ///   under 1.5 m/s   not detected
        ///   1.5 - 4 m/s     reach x0.40, reliability x0.30
        ///   4 - 10 m/s      reach x0.80, reliability x0.70
        ///   over 10 m/s     full
        ///
        /// Radial velocity, not speed: the component along the sensor-to-target
        /// line. That is what makes this a mechanic rather than a stat. A tank
        /// that stops is invisible; a drone that hovers is invisible; a drone
        /// crossing a radar's face at any speed at all is invisible, because a
        /// tangential track has no radial component. The same airframe running in
        /// at the mast is the loudest thing on the channel.
        /// </summary>
        public static readonly Fix RadarNotchMetresPerSecond = Fix.FromDoubleContentOnly(1.5);
        public static readonly Fix RadarSlowMetresPerSecond = Fix.FromInt(4);
        public static readonly Fix RadarMediumMetresPerSecond = Fix.FromInt(10);
        public static readonly Fix RadarSlowReachScale = Fix.FromDoubleContentOnly(0.40);
        public static readonly Fix RadarMediumReachScale = Fix.FromDoubleContentOnly(0.80);
        public const int RadarSlowReliabilityPercent = 30;
        public const int RadarMediumReliabilityPercent = 70;

        // ---- weather ---------------------------------------------------------

        /// <summary>
        /// What each weather state does to each sensor channel, as a multiplier on
        /// reach. The ordering is the point rather than the exact figures.
        ///
        /// Rain takes a bite out of the cameras and a smaller one out of the
        /// heat sensors. Fog takes almost everything from both and nothing at all
        /// from radar and passive listening - which is the whole reason a player
        /// buys radar, and the reason fog is an opportunity rather than a
        /// misfortune. Wind touches none of them and kills microphones outright,
        /// because what stops a microphone is not the air being turbulent, it is
        /// the wind roaring across the element.
        /// </summary>
        public static readonly Fix WetOpticalScale = Fix.FromDoubleContentOnly(0.55);
        public static readonly Fix WetThermalScale = Fix.FromDoubleContentOnly(0.60);
        public static readonly Fix WetRadarScale = Fix.FromDoubleContentOnly(0.90);

        /// <summary>
        /// Fog, with the corrected coefficients. The first set had long-wave at
        /// about a tenth of visible, which would have made thermal imaging a
        /// near-complete answer to fog. It is not: measured work puts visible,
        /// near-infrared and mid-wave comparable to one another, with long-wave
        /// somewhere between half and equal. Front-line reporting says the same
        /// thing more bluntly - in fog at fifty yards it makes no difference
        /// whether the drone carries a thermal camera or a night one.
        /// </summary>
        public static readonly Fix MurkOpticalScale = Fix.FromDoubleContentOnly(0.15);
        public static readonly Fix MurkThermalScale = Fix.FromDoubleContentOnly(0.30);

        /// <summary>Cold halves an electric airframe. Small ones suffer more.</summary>
        public static readonly Fix FrozenEnduranceScale = Fix.FromDoubleContentOnly(0.50);

        /// <summary>
        /// What mud does to a vehicle that leaves the road. Not a slowdown - a
        /// reason not to try, which is the point: it funnels everything onto the
        /// corridors that are already the most watched ground on the map.
        /// </summary>
        public static readonly Fix MudOffRoadScale = Fix.FromDoubleContentOnly(0.25);

        /// <summary>Frozen ground is better than firm for going across country.</summary>
        public static readonly Fix FrozenOffRoadScale = Fix.FromDoubleContentOnly(1.15);

        // ---- navigation, once nobody is telling it where it is ---------------

        /// <summary>
        /// What scene matching is wrong by, in real metres, when it has a lock.
        ///
        /// Four unrelated fielded systems agree on ten to thirty real metres of
        /// bounded error (navigation-denied.md §4), and now that the catalogue is
        /// in real metres that figure can be written down as itself instead of
        /// being divided by twelve first. Twenty metres is the middle of the band.
        /// This tier is still not "more accurate" but "knows where it is": twenty
        /// metres is under the miss radius below, so the interesting question stays
        /// whether it has a lock at all.
        /// </summary>
        public static readonly Fix SceneMatchErrorMetres = Fix.FromDoubleContentOnly(20.0);

        /// <summary>
        /// Error added per metre flown on inertial alone. The literature's growth
        /// is cubic in time, but for anything cruising the dominant unknown is the
        /// wind, which scales with distance - and percent-of-distance is the law a
        /// player can actually reason about. Three percent is the pessimistic end
        /// of the published one-to-three band, which is where an expendable
        /// airframe's inertial unit belongs.
        /// </summary>
        public static readonly Fix NavDriftRateInertial = Fix.FromDoubleContentOnly(0.03);

        /// <summary>
        /// The same, with a star tracker aboard.
        ///
        /// Celestial navigation does not fix position - it bounds heading, which
        /// removes the fastest-growing term in the error. So it is a smaller slope
        /// and never a reset. The exact figure is an estimate: the research gives
        /// the mechanism firmly and no number for this class of airframe, because
        /// this class of airframe does not carry one.
        /// </summary>
        public static readonly Fix NavDriftRateCelestial = Fix.FromDoubleContentOnly(0.008);

        /// <summary>
        /// How far a drone must fly over matchable ground to get a lost lock back.
        /// navigation-denied.md §4 puts re-acquisition at 700-1,300 real metres
        /// ("at 30 m/s that is 700-1,300 m of flight to re-converge"); 1,000 is
        /// the middle of it, and is now stored as the metres the research says
        /// rather than as an eightieth of a compressed map.
        /// </summary>
        public static readonly Fix NavReacquireMetres = Fix.FromDoubleContentOnly(1000.0);

        /// <summary>
        /// How far NavState.ErrorMetres can displace a one-way munition's
        /// aimpoint before the warhead goes off on empty ground instead of the
        /// target - see World.ApplyDamage. AUDIT-UNWIRED.md F5: the whole
        /// navigation system computed this number and nothing consumed it.
        ///
        /// navigation-denied.md §5 sources the error itself (scene matching is
        /// good to tens of metres and "rounds to nothing"; dead reckoning over a
        /// deep denied penetration is 3% of distance flown, which its worked
        /// example puts at 600 m over a 20 km run). It does not say how much
        /// displacement a given warhead can tolerate and still land on something
        /// the size of a vehicle, so this radius is a designer estimate, and in
        /// real metres it is one that can now be argued with: forty metres is
        /// twice the scene-matching error, so a drone that brought a map never
        /// misses on navigation grounds, and it is about as far off as a terminal
        /// seeker can be and still have the target somewhere in frame when it
        /// looks. It is reached after 1,300 m of unescorted denied flight
        /// (40 / 0.03), so the geofence still costs an airframe something inside a
        /// realistic run rather than only on a deep raid.
        ///
        /// It is NOT the old 8 map metres read at 12:1, which would be 96 m - far
        /// enough off that the warhead detonating there is not a near miss, it is
        /// a different field.
        ///
        /// Note what the "somewhere in frame when it looks" half of that argument
        /// commits this number to, now that World.ApplyDamage exempts a munition
        /// a person is flying on a live feed: forty metres is the slack a
        /// *terminal seeker* has, and the munitions this radius is still read
        /// against are exactly the ones with no operator watching. A one-way
        /// airframe with neither an operator nor a seeker - AutonomyTier.None,
        /// link black - is being held to a seeker's tolerance it does not have,
        /// which makes it a fraction too accurate rather than too fragile. That
        /// is a known conservatism and not a tuned one: the research gives no
        /// figure for either case, and inventing a second, tighter radius to
        /// separate them would be adding a number nobody has measured to a model
        /// whose one measured input is the 3%/metre drift rate.
        /// </summary>
        public static readonly Fix MunitionMissRadiusMetres = Fix.FromDoubleContentOnly(40.0);

        /// <summary>
        /// How much warhead a hit needs to carry before it counts as the "heavy
        /// bombardment" that navigation-denied.md §6 names as one of the events
        /// that invalidates reference imagery - "a sector bombarded into
        /// unrecognisability" - rather than the routine kamikaze hit an FPV or
        /// Fiber FPV Team carries (260 / 340). Read against the raw warhead
        /// (WeaponState.Damage / MineState.Damage as passed to ApplyDamage,
        /// before the armour multiplier - a shot that happens to land on thin
        /// plate is not thereby a bigger bomb), this sits above every
        /// FPV-class munition and at or below every Tier-3 strike drone and
        /// the Night Bomber's mines. The research names the mechanism, not a
        /// joules figure, so both this and the radius below are designer
        /// estimates.
        /// </summary>
        public static readonly Fix HeavyBombardmentDamageThreshold = Fix.FromDoubleContentOnly(350.0);

        /// <summary>
        /// How much of a sector one heavy hit churns past matching, in real
        /// metres. Designer estimate - see HeavyBombardmentDamageThreshold - and
        /// the word the research uses is "sector", which is why this is 1.8 km of
        /// ground rather than a crater radius: what stops a scene matcher is not
        /// the hole, it is that the landmarks around it have been rearranged.
        /// </summary>
        public static readonly Fix HeavyBombardmentInvalidateRadiusMetres = Fix.FromDoubleContentOnly(1800.0);

        /// <summary>
        /// How often a reconnaissance airframe's camera pays into the imagery
        /// resource - navigation-denied.md §6's supply end, AUDIT-UNWIRED.md F6.
        /// Coverage is a persistent bit per cell, so granting it every tick buys
        /// nothing once a cell is already covered and only spends the tick
        /// budget detection already needs. Once a second of world time is enough
        /// that a recon airframe cruising at Recon Wing's 25 m/s covers 25 m
        /// between grants, far under one 768 m imagery cell, so it never skips a
        /// cell it flew over - an engineering cadence, not a content number, so it
        /// is not cited to a section.
        /// </summary>
        public const int ReconImageryGrantInterval = 8;

        // ---- how reliable any of this is ------------------------------------

        /// <summary>
        /// Inside this fraction of a sensor's reach a contact is solid. Beyond it,
        /// out to full reach, the sensor is working at the edge of what it can do
        /// and produces an intermittent track rather than a certain one.
        /// </summary>
        public static readonly Fix DetectionSolidFraction = Fix.FromDoubleContentOnly(0.60);

        /// <summary>
        /// How far apart two passive listeners' bearings to the same emitter have
        /// to cross before the pair is a position rather than two directions.
        ///
        /// radar-rf.md §3A.3: one sensor gives a line of bearing and "never a
        /// weapon"; two give a fix whose error is sigma_theta x R / sin(delta),
        /// which is 131 m at a 90 degree crossing, 383 m at 20 degrees and 1,500 m
        /// at 5. Its implementable rule promotes a bearing to a fix when
        /// sin(delta) > 0.35 - about 20 degrees - and that is this number, in the
        /// simulation's own angle units (65,536 to the turn).
        ///
        /// Which turns "I have ESM everywhere" into a question about where the
        /// ESM is, and that is the point of it. Two listeners on one line are a
        /// worse sensor than the same two spread across a front.
        /// </summary>
        public const int EsmCrossFixBam = (20 * 65536) / 360;

        /// <summary>
        /// The target speed a gun's firing solution is quoted against, in real
        /// metres per second - CombatSystem.AirHitChance scores every target
        /// relative to this, faster ones harder and slower ones easier.
        ///
        /// 45 m/s is 160 km/h, the bottom of point-defence.md §"The target set
        /// these systems have to beat" for the Shahed-136/Geran-2 row (160-220
        /// km/h cruise) - the threat every system in that document was sized
        /// against, which is exactly what a reference speed should be.
        ///
        /// It was a bare 20 in the middle of AirHitChance, and 20 was a *map*
        /// metre per second at the old compression, so once the catalogue moved
        /// to real speeds every airframe in the game read as a harder target than
        /// it had been - an FPV at 33 m/s scoring worse than it used to at 22.
        /// A reference speed in real units is a figure the reporting can be held
        /// against; a reference speed in compressed units is a number that moves
        /// whenever the compression does.
        /// </summary>
        public static readonly Fix FiringSolutionReferenceSpeed = Fix.FromInt(45);

        /// <summary>
        /// Once something has been seen, the track is held for two real seconds
        /// even if the sensor loses it. Real systems coast a track rather than
        /// dropping it the instant a return is missed, and without this a marginal
        /// contact strobes. This is the tracker and not the player, so it is two
        /// seconds of world - a quarter of the play time the flat 64 ticks used to
        /// buy, and the same two seconds FINDINGS #21 describes.
        /// </summary>
        public static readonly int TrackHoldTicks = Seconds(2);

        // ---- mines -----------------------------------------------------------

        /// <summary>
        /// A short arming delay, so a bomber cannot drop a mine directly onto a
        /// vehicle and have it go off in the same instant. A fuze arming is
        /// physical, so it is real seconds.
        /// </summary>
        public static readonly int MineArmingTicks = Millis(1500);

        /// <summary>
        /// How far apart a stick of mines is spaced when laid from the air, real
        /// metres. Designer estimate: deep-strike.md describes road mining from
        /// the air without giving a stick spacing, so this is the old 26 map
        /// metres at 12:1, which is about the length of ground a low pass covers
        /// between releases.
        /// </summary>
        public const int MineSpacingMetres = 312;

        // ---- autonomy --------------------------------------------------------

        /// <summary>
        /// Each decoy inside a seeker's cone drags classifier confidence down by
        /// five points. This is the term that makes deception a real strategy and
        /// the reason the counter to autonomy is a cheap inflatable, not a jammer.
        /// </summary>
        public const int AutonomyDecoyQualityPenalty = 5;
        public const int AutonomyNightNoThermalPenalty = 20;
        public const int AutonomySmokePenalty = 15;
        /// <summary>
        /// How far ahead a terminal seeker looks for candidates, real metres.
        /// Designer estimate: autonomy.md describes the terminal decision without
        /// sizing the search volume. 1.4 km is roughly where a seeker's camera
        /// stops resolving a vehicle well enough to argue about it, and it is the
        /// old 120 map metres at 12:1.
        /// </summary>
        public const int AutonomySeekerConeMetres = 1440;
        public const int AutonomyMaxCandidates = 24;

        /// <summary>
        /// Deception fools machines, never people. An attack flown by a crew with a
        /// live link from inside this range ignores decoys completely.
        ///
        /// One kilometre, not the 4.8 km a straight 12:1 reading of the old 400
        /// would give. thermal-optical.md §8.3's resolution table is the check the
        /// real units make possible: a wide-search camera resolves a 7 m vehicle
        /// at 7 km but a narrow tracker is what gives a pilot enough pixels to
        /// tell a real vehicle from an inflatable, and that argument runs out
        /// somewhere around a kilometre on the cheap optics an FPV carries.
        /// Designer estimate, but a checkable one.
        /// </summary>
        public const int PilotedDecoyImmunityRangeMetres = 1000;

        // ---- entity caps ----------------------------------------------------
        // These are match rules, not device settings. In a lockstep match every
        // peer must simulate the same world, so a cap that varied by device would
        // be a desync rather than a graceful degradation.

        public const int MaxEntitiesStandard = 4096;
        public const int MaxEntitiesCompact = 1536;
        public const int MaxTethersStandard = 512;
        public const int MaxTethersCompact = 192;
    }
}
