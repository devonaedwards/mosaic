// KILL ZONE - a real-time strategy video game.
// The small vocabularies the simulation is written in.

namespace KZ.Sim
{
    /// <summary>
    /// Altitude is three named bands, not a continuous coordinate. Continuous
    /// altitude would demand three-dimensional pathfinding, three-dimensional
    /// line of sight and an altitude readout in the interface, and it would buy
    /// nothing the enum does not already give: who can shoot whom, and what
    /// blocks sight.
    /// </summary>
    public enum Layer : byte
    {
        Ground = 0,
        Low = 1,    // rotary drones; hit by gun mounts and low-stance interceptors
        High = 2    // fixed-wing; reachable only by high-stance interceptors and batteries
    }

    public enum ArmourClass : byte
    {
        Soft = 0,       // infantry, unarmoured trucks
        Light = 1,      // technicals, robots, radar vehicles
        Heavy = 2,      // tanks and fighting vehicles
        Structure = 3,
        AirRotary = 4,
        AirFixed = 5
    }

    public enum DamageType : byte
    {
        Shaped = 0,       // strong into armour from above, poor against soft targets
        Fragmentation = 1,
        Kinetic = 2,
        Incendiary = 3,
        Ram = 4           // interceptor collision: kills air outright, does nothing else
    }

    /// <summary>
    /// The five rungs of the control-link ladder. Each buys resistance to jamming
    /// and pays for it somewhere else. The counter to any rung sits one rung back.
    /// </summary>
    public enum LinkKind : byte
    {
        None = 0,
        Radio = 1,      // cheapest, dies to any jammer
        Mesh = 2,       // every friendly drone is a repeater; kill the chain, lose the children
        Fiber = 3,      // unjammable, but leashed, slow, and physically traceable
        Satellite = 4,  // unlimited range, scarce capacity, broken only by a very strong jammer
        Autonomy = 5    // nothing to jam; pays in target-selection error instead
    }

    public enum LinkPip : byte
    {
        Green = 0,
        Amber = 1,
        Black = 2
    }

    /// <summary>What a drone does when its link dies. Upgrades change this.</summary>
    public enum BlackPolicy : byte
    {
        Abort = 0,      // orbit, then fall out of the sky
        LastMile = 1,   // carry on to the last designated point under onboard guidance
        DualLink = 2    // fall back to the alternate link at a speed penalty
    }

    public enum CrewState : byte
    {
        Ready = 0,
        Flying = 1,
        Recovering = 2,
        Fatigued = 3,
        Benched = 4,    // remote piloting bay destroyed; they come back
        Reserved = 5,   // held back by the player, excluded from auto-assignment
        KIA = 6         // forward dugout destroyed; they do not come back
    }

    public enum SortiePhase : byte
    {
        None = 0,
        Spawning = 1,
        Transit = 2,
        Engaged = 3,
        Terminal = 4,   // committed, ignores new orders, cannot be recalled
        Returning = 5
    }

    /// <summary>
    /// Terrain classes that matter to a fiber tether. A thread dragged through a
    /// forest or under a power line is far more likely to part than one crossing
    /// open ground.
    /// </summary>
    public enum TileClass : byte
    {
        Open = 0,
        Road = 1,
        Forest = 2,
        PowerLine = 3,
        Rubble = 4,
        Water = 5,
        Impassable = 6
    }

    /// <summary>
    /// What a mount is loaded with.
    ///
    /// Against something small and fast the problem is not that the rounds cannot
    /// reach, it is that the aiming solution is never quite right. Every option
    /// below is a different answer to that: be more precise, or stop needing to be.
    /// </summary>
    public enum AmmoType : byte
    {
        /// <summary>
        /// Plain solid rounds. Cheapest, hardest hitting, and it has to actually
        /// connect - so it falls off fastest with range and suffers most against a
        /// fast crosser.
        /// </summary>
        Solid = 0,

        /// <summary>
        /// Buckshot. A dense pattern at very short range and nothing at all beyond
        /// it. The last-ditch option, and against a drone in the final hundred
        /// metres it is the best thing there is.
        /// </summary>
        Buckshot = 1,

        /// <summary>
        /// A proximity fuze. Does not need to hit, only to pass close, which makes
        /// aim error far less costly and holds the odds up at range. Costs more per
        /// round and does less when it works.
        /// </summary>
        Proximity = 2,

        /// <summary>
        /// Programmable airburst, set to open at the target. The flattest falloff
        /// of the four and the least troubled by a fast crosser, because it is
        /// filling a volume rather than threading a needle. Needs a fuze setter on
        /// the mount, which is why it is not simply the answer to everything.
        /// </summary>
        Airburst = 3
    }

    /// <summary>The five ways one thing finds another.</summary>
    public enum SensorChannel : byte
    {
        Optical = 0,
        Thermal = 1,
        Acoustic = 2,
        Radar = 3,
        Esm = 4
    }

    public enum DayPhase : byte
    {
        Day = 0,
        Dusk = 1,
        Night = 2,
        Dawn = 3
    }

    /// <summary>Which kinds of thing a candidate is, for the autonomy classifier.</summary>
    public enum TargetKind : byte
    {
        HighValue = 0,   // tanks, jammers, radars, launchers
        LowValue = 1,    // infantry, light trucks
        Decoy = 2,       // inflatable with a thermal signature
        Friendly = 3,
        Neutral = 4
    }

    /// <summary>
    /// What a drone can work out about where it is, once nobody is telling it.
    ///
    /// The ladder is short because the research says it is short. There is no
    /// affordable middle: either an airframe carries an absolute optical
    /// reference and knows its position to within a map metre or two, or it is
    /// dead reckoning and its error grows without bound. Celestial navigation,
    /// which sounds like the missing rung, is not one - see CelestialHeading.
    /// </summary>
    public enum NavAid : byte
    {
        /// <summary>
        /// Inertial only. Error grows as a fraction of the distance flown and
        /// never stops growing. Cheap airframes live here and this is the tier
        /// that makes crossing a border hurt.
        /// </summary>
        DeadReckoning = 0,

        /// <summary>
        /// Matches what the camera sees against stored imagery of the ground
        /// below. Four unrelated fielded systems agree on ten to thirty real
        /// metres of bounded error, which at this game's compression is one to
        /// three map metres - near enough exact.
        ///
        /// So this is not a more accurate tier, it is a different *kind* of
        /// tier: it either has a lock and is essentially right, or it has lost
        /// lock and is dead reckoning. There is no useful middle to draw.
        /// </summary>
        SceneMatching = 1
    }

    public enum FactionId : byte
    {
        Neutral = 0,
        KestrelPact = 1,      // satellite uplink, few and precise, crews survive
        ObsidianDirectorate = 2 // mesh relay, mass and autonomy, crews do not
    }
}
