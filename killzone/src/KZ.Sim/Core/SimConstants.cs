// KILL ZONE - a real-time strategy video game.
// Every tuning number the simulation depends on, in one place, in ticks and
// map metres. These are balance values for a game: they exist to be argued
// about in playtests and changed.

namespace KZ.Sim
{
    public static class SimConstants
    {
        // ---- time ---------------------------------------------------------

        /// <summary>
        /// 32 rather than 30 because 1/32 is exact in binary, so dt introduces no
        /// rounding drift over a 25-minute match. It also makes every derived
        /// duration a clean power-of-two count of ticks.
        /// </summary>
        public const int TicksPerSecond = 32;

        public static readonly Fix Dt = new Fix(Fix.OneRaw / TicksPerSecond);

        public static int Seconds(int s) { return s * TicksPerSecond; }
        public static int Millis(int ms) { return (ms * TicksPerSecond) / 1000; }

        /// <summary>Commands are gathered into 4-tick turns (125 ms) for network play.</summary>
        public const int TicksPerCommandTurn = 4;

        // ---- the map ------------------------------------------------------

        /// <summary>One build tile is 8 m. Structure footprints are given in tiles.</summary>
        public const int BuildTileMetres = 8;

        /// <summary>Jamming and threat are tracked on a coarser 32 m grid.</summary>
        public const int SignalCellMetres = 32;

        /// <summary>Vision sits between the two, at 16 m.</summary>
        public const int VisionCellMetres = 16;

        /// <summary>The signal grid is rebuilt at 8 Hz; jam fields do not move fast.</summary>
        public const int SignalRebuildInterval = 4;

        /// <summary>The mesh relay graph is rebuilt at 8 Hz.</summary>
        public const int MeshRebuildInterval = 4;

        // ---- the control-link layer ---------------------------------------

        /// <summary>
        /// A jammed drone flies amber for four seconds before the link is
        /// considered lost. Long enough to react, short enough to hurt.
        /// </summary>
        public const int AmberToBlackTicks = 128;

        /// <summary>A drone with a dead link orbits for twelve seconds, then is lost.</summary>
        public const int BlackToLostTicks = 384;

        /// <summary>Dual-link airframes fall back to their alternate link after two seconds.</summary>
        public const int DualLinkSwitchTicks = 64;

        /// <summary>
        /// Jamming falls off linearly from the emitter and is scaled by 1.3, so a
        /// bubble bites hardest at its centre and frays at the edge. Skirting the
        /// rim of a jammer is a real and learnable play.
        /// </summary>
        public static readonly Fix JamFalloffScale = Fix.FromDoubleContentOnly(1.3);

        /// <summary>
        /// Within this margin of a drone's robustness, the coarse grid value is not
        /// trusted and the exact distance to each contributing emitter is evaluated.
        /// Without it a drone flickers green/amber as it crosses 32 m cell borders.
        /// </summary>
        public const int JamBoundaryRefineMargin = 12;

        /// <summary>A robustness of 255 means "nothing to jam" - fiber and autonomy.</summary>
        public const int UnjammableRobustness = 255;

        public const int MeshRangePerHopMetres = 700;
        public const int MeshMaxHops = 4;
        public const int MeshRobustnessPerAltHop = 5;
        public const int MeshRobustnessAltCap = 15;
        public const int MeshAcquisitionPenaltyTicksPerHop = 13;
        public const int RadioRangeMetres = 1000;

        /// <summary>A veteran crew pushes a drone through interference a rookie would lose.</summary>
        public const int VeteranLinkRobustnessBonus = 8;

        // ---- fiber tethers ------------------------------------------------

        public const int TetherMaxNodes = 64;
        public const int TetherNodeSpacingMetres = 12;

        /// <summary>Three seconds at full stretch before the line parts.</summary>
        public const int TetherTautGraceTicks = 96;

        /// <summary>A cut thread stays on the map for thirty seconds. It still leads home.</summary>
        public const int TetherLingerTicks = 960;

        /// <summary>Each tether checks its newest segment plus one older one per tick.</summary>
        public const int TetherSegmentsPerTick = 2;

        // ---- crews and sorties ---------------------------------------------

        public const int CrewRecoveryTicks = 256;          // 8 s
        public const int CrewRecoveryPriorityTicks = 160;  // 5 s with Priority Recovery
        public const int CrewFatiguedRecoveryTicks = 448;  // 14 s
        public const int CrewFatigueSortieThreshold = 5;
        public const int CrewFatigueWindowTicks = 3840;    // 120 s
        public const int CrewFatigueClearIdleTicks = 960;  // 30 s
        public const int CrewBenchedTicks = 1440;          // 45 s, for remote piloting bays

        public const int CrewRankUpKills2 = 3;
        public const int CrewRankUpKills3 = 9;
        public const int CrewRankUpKills4 = 20;

        public const int StartingCrews = 6;
        public const int CrewsPerQuarters = 4;
        public const int MaxCrews = 30;

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
        public const int SalvageDecayTicks = 1600;         // 50 s to nothing

        /// <summary>Verified kills pay 12% of the victim's cost as Tasking Points.</summary>
        public static readonly Fix TaskingPointsUnitFraction = Fix.FromDoubleContentOnly(0.12);
        public static readonly Fix TaskingPointsStructureFraction = Fix.FromDoubleContentOnly(0.08);
        public static readonly Fix TaskingPointsUnverifiedScale = Fix.FromDoubleContentOnly(0.5);

        /// <summary>A kill counts as verified if a friendly saw it happen within this range.</summary>
        public const int VerificationWitnessRangeMetres = 400;

        // ---- day and night --------------------------------------------------

        public const int DayNightCycleTicks = 11520;       // 6:00
        public const int DayTicks = 5760;                  // 3:00
        public const int DuskTicks = 1440;                 // 0:45
        public const int NightTicks = 3360;                // 1:45
        public const int DawnTicks = 960;                  // 0:30

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
        /// What scene matching is wrong by, in map metres, when it has a lock.
        ///
        /// Four unrelated fielded systems agree on ten to thirty real metres of
        /// bounded error. At this game's twelve-to-one compression that is one to
        /// three map metres, which is why this tier is not "more accurate" but
        /// "knows where it is" - the error rounds to nothing and the interesting
        /// question becomes whether it has a lock at all.
        /// </summary>
        public static readonly Fix SceneMatchErrorMetres = Fix.FromDoubleContentOnly(2.0);

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
        /// The research puts re-acquisition at roughly 700-1,300 real metres.
        /// </summary>
        public static readonly Fix NavReacquireMetres = Fix.FromDoubleContentOnly(80.0);

        /// <summary>
        /// How far NavState.ErrorMetres can displace a one-way munition's
        /// aimpoint before the warhead goes off on empty ground instead of the
        /// target - see World.ApplyDamage. AUDIT-UNWIRED.md F5: the whole
        /// navigation system computed this number and nothing consumed it.
        ///
        /// navigation-denied.md §5 sources the error itself (scene matching's
        /// 1-3 map metres "rounds to nothing"; dead reckoning over a deep
        /// denied penetration reaches tens of map metres - the worked example
        /// is 50 at 20 real km). It does not say how much displacement a given
        /// warhead can tolerate and still land on something the size of a
        /// vehicle, so this radius is a designer estimate: comfortably above
        /// SceneMatchErrorMetres, so a drone that brought a map never misses on
        /// navigation grounds, and reachable by NavDriftRateInertial well
        /// inside a plausible flight (8 / 0.03 ~= 270 m of unescorted denied
        /// flight), so the mount that makes the geofence hurt actually does.
        /// </summary>
        public static readonly Fix MunitionMissRadiusMetres = Fix.FromDoubleContentOnly(8.0);

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

        /// <summary>How much of a sector one heavy hit churns past matching. Designer estimate - see HeavyBombardmentDamageThreshold.</summary>
        public static readonly Fix HeavyBombardmentInvalidateRadiusMetres = Fix.FromDoubleContentOnly(150.0);

        /// <summary>
        /// How often a reconnaissance airframe's camera pays into the imagery
        /// resource - navigation-denied.md §6's supply end, AUDIT-UNWIRED.md F6.
        /// Coverage is a persistent bit per cell, so granting it every tick buys
        /// nothing once a cell is already covered and only spends the tick
        /// budget detection already needs. Four times a second is enough that a
        /// recon airframe cruising at up to Recon Wing's 12 m/s (well under one
        /// 64 m imagery cell between grants) never skips a cell it flew over -
        /// an engineering cadence, not a content number, so it is not cited to
        /// a section.
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
        /// Once something has been seen, the track is held this long even if the
        /// sensor loses it. Real systems coast a track rather than dropping it the
        /// instant a return is missed, and without this a marginal contact strobes.
        /// </summary>
        public const int TrackHoldTicks = 64;   // 2 s

        // ---- mines -----------------------------------------------------------

        /// <summary>
        /// A short arming delay, so a bomber cannot drop a mine directly onto a
        /// vehicle and have it go off in the same instant.
        /// </summary>
        public const int MineArmingTicks = 48;   // 1.5 s

        /// <summary>How far apart a stick of mines is spaced when laid from the air.</summary>
        public const int MineSpacingMetres = 26;

        // ---- autonomy --------------------------------------------------------

        /// <summary>
        /// Each decoy inside a seeker's cone drags classifier confidence down by
        /// five points. This is the term that makes deception a real strategy and
        /// the reason the counter to autonomy is a cheap inflatable, not a jammer.
        /// </summary>
        public const int AutonomyDecoyQualityPenalty = 5;
        public const int AutonomyNightNoThermalPenalty = 20;
        public const int AutonomySmokePenalty = 15;
        public const int AutonomySeekerConeMetres = 120;
        public const int AutonomyMaxCandidates = 24;

        /// <summary>
        /// Deception fools machines, never people. An attack flown by a crew with a
        /// live link from inside this range ignores decoys completely.
        /// </summary>
        public const int PilotedDecoyImmunityRangeMetres = 400;

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
