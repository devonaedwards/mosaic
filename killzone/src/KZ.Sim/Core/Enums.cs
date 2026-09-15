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

    /// <summary>
    /// How well a side holds a track, and therefore how good a firing or intercept
    /// solution it can compute from it.
    ///
    /// This is not the same question as "can it see it". Detection is binary and
    /// IsDetectedBy answers it. This asks what the track is made of, and the answer
    /// decides whether an interceptor can be vectored at where a target is going to
    /// be or only pointed at where it is - which, against anything faster than the
    /// interceptor, is the difference between an interception and a stern chase.
    ///
    /// point-defence.md's effector table is explicit about the split and it is a
    /// property of the sensor, not of the shooter: the AI machine-gun turret is
    /// "passive EO/IR only... no velocity measurement" and infers range and closing
    /// rate from image scale, while the autocannon carries "organic AESA
    /// search/track + EO/IR; measured velocity". The same document says a firing
    /// solution is most sensitive to exactly that measurement.
    /// </summary>
    public enum TrackQuality : byte
    {
        /// <summary>Nobody holds it. Whatever is shooting is doing so on memory.</summary>
        None = 0,
        /// <summary>Somebody has eyes on it, and no measured velocity.</summary>
        Optical = 1,
        /// <summary>A radar holds it: range and closing rate are measured.</summary>
        Radar = 2
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
    /// reference and knows its position to within a few tens of metres, or it is
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
        /// metres of bounded error - well inside the radius at which a warhead
        /// still lands on the target, so near enough exact.
        ///
        /// So this is not a more accurate tier, it is a different *kind* of
        /// tier: it either has a lock and is essentially right, or it has lost
        /// lock and is dead reckoning. There is no useful middle to draw.
        /// </summary>
        SceneMatching = 1
    }

    /// <summary>
    /// The weather, as four states rather than a slider.
    ///
    /// Four and not one because the states are not degrees of the same badness -
    /// two of them are opposites. Rain grounds aircraft and leaves the sensors
    /// working; fog grounds nothing and blinds everything. Collapse those into a
    /// single "bad weather" axis and the only interesting decision disappears.
    ///
    /// The other reason there are four: each one kills a different thing. Wind
    /// kills the small, wet kills the electric, murk kills the sensors. A player
    /// can hold three rules in their head and plan against them, which is the
    /// whole test - a mechanic that needs a submenu gets cut.
    /// </summary>
    public enum WeatherState : byte
    {
        Clear = 0,

        /// <summary>
        /// Twelve to eighteen metres per second. Kills the small and cheap:
        /// quadcopters and interceptors cannot hold station, and fixed-wing
        /// aircraft are stopped by their landing limits rather than their cruise
        /// limits. Microphones stop working entirely. Cameras do not care.
        /// </summary>
        Wind = 1,

        /// <summary>
        /// Rain, and in winter this is icing instead, which is a hard stop rather
        /// than a penalty - a quarter of thrust is gone inside the first minute of
        /// accretion. Kills the electric. Combustion strike drones fly through it,
        /// which is the asymmetry that makes winter the attacker's season.
        /// </summary>
        Wet = 2,

        /// <summary>
        /// Fog, or a cloud base on the deck. Grounds nothing at all and blinds
        /// almost everything - and radar and passive listening are untouched, so
        /// the side that bought radar is suddenly the only side that can see.
        /// This is the assault window, and it is the state a designer is most
        /// likely to get wrong by filing it under "bad weather".
        /// </summary>
        Murk = 3
    }

    /// <summary>
    /// What the ground is doing, on a six-week clock rather than an hourly one -
    /// which is why it is separate from the weather rather than a fifth state.
    ///
    /// Mud does not slow the roads. It deletes everything that is not a road,
    /// which funnels every vehicle onto exactly the netted corridors that are
    /// already the most watched ground on the map.
    /// </summary>
    public enum GroundState : byte
    {
        Firm = 0,
        Mud = 1,

        /// <summary>
        /// Better than firm for going across country, and it halves the endurance
        /// of every electric airframe at the same time. Deep winter is a window
        /// rather than a penalty, and it opens for whoever burns fuel.
        /// </summary>
        Frozen = 2
    }

    /// <summary>
    /// What pushes an airframe along, which turns out to be the thing weather
    /// actually sorts on. Not size, not cost - what it burns.
    /// </summary>
    public enum Propulsion : byte
    {
        None = 0,
        SmallElectric = 1,   // quadcopters, interceptors: grounded by wind and by wet
        HeavyElectric = 2,   // heavy multirotors: grounded by wet
        Combustion = 3,      // flies through all of it
        Turbojet = 4
    }

    /// <summary>
    /// The two things that get called "autonomy", which are not the same thing and
    /// which the game modelled as one for far too long.
    ///
    /// The research settles which of them the world actually has. Terminal
    /// guidance became routine during 2026 - production airframes ship with it and
    /// the add-on costs about a hundred dollars. Autonomous target *selection* was
    /// still in initial combat testing at the same date, officially not fielded,
    /// with one contested single-source claim of a kill.
    ///
    /// So the near-future this game is set in is a <see cref="TerminalGuidance"/>
    /// world, and the important consequence is that terminal guidance **keeps the
    /// crew**. It is not a step towards replacing people; it is a way of making
    /// the last two seconds survivable when the link dies. Anyone modelling
    /// autonomy as a crew-saving technology has the arrow pointing the wrong way.
    /// </summary>
    public enum AutonomyTier : byte
    {
        /// <summary>A person flies it all the way in. Cheapest, and jamming kills it.</summary>
        None = 0,

        /// <summary>
        /// The machine flies the last few seconds onto a target a human already
        /// chose. Immune to jamming once committed, because there is nothing left
        /// to jam - and *more* accurate than a person, not less, because the hard
        /// part of an FPV attack is the final approach.
        ///
        /// Still consumes a crew. Decoys do not work on it, because the decision
        /// was made by someone who could see.
        /// </summary>
        TerminalGuidance = 1,

        /// <summary>
        /// The machine picks the target too. This is the one that frees a crew, and
        /// the one that pays for it - a camera choosing its own target is beaten by
        /// giving the camera something convincing to look at, which is what makes a
        /// hundred-and-fifty-Materiel inflatable a good answer to a six-hundred-
        /// Materiel munition.
        ///
        /// Speculative rather than fielded. It should be expensive and it should
        /// feel like a gamble, because in 2026 that is exactly what it was.
        /// </summary>
        TargetSelection = 2
    }

    public enum FactionId : byte
    {
        Neutral = 0,
        KestrelPact = 1,      // satellite uplink, few and precise, crews survive
        ObsidianDirectorate = 2 // mesh relay, mass and autonomy, crews do not
    }
}
