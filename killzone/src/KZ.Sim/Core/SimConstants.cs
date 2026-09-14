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

        /// <summary>Optical sensors collapse at night unless the player has bought thermal.</summary>
        public static readonly Fix NightOpticalDetectionScale = Fix.FromDoubleContentOnly(0.35);

        /// <summary>
        /// Thermal imaging is worse in daylight, not better.
        ///
        /// It works on temperature contrast, and sunlight heats the background
        /// until there is very little contrast left - the same imager that picks a
        /// warm engine out of a cold sky at four in the morning struggles past a
        /// few hundred metres on a hot afternoon. This is the opposite of the
        /// intuition that "thermal is the night sensor and neutral by day", and the
        /// model had it wrong in exactly that way.
        /// </summary>
        public static readonly Fix ThermalDayScale = Fix.FromDoubleContentOnly(0.55);
        public static readonly Fix ThermalNightScale = Fix.FromDoubleContentOnly(1.25);
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
