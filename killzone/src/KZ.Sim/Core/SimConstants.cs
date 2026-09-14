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
