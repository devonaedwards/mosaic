// KILL ZONE - a real-time strategy video game. Fictional factions, hit points,
// build timers. Not operational guidance.
//
// What a drone knows about where it is, once the link is gone and nothing is
// telling it.
//
// The rule this whole system exists to express is one line:
//
//     Crossing the border costs you the operator. It costs you your position
//     only if you did not bring a map.
//
// That keeps the mechanic the geofence created - push an offensive past your own
// border and your drones are unsupported over ground you hold - while turning it
// from a flat tax into a procurement decision. A cheap airframe past the line is
// genuinely lost. An expensive one knows exactly where it is and has merely lost
// the human who was going to pick the target.
//
// Which makes reconnaissance of ground you do not control a prerequisite for
// deep strike, rather than a nice-to-have.

namespace KZ.Sim
{
    public static class NavigationSystem
    {
        public static void Step(World w)
        {
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (!w.Entities.Has(i, ComponentMask.Nav)) continue;
                StepOne(w, i);
            }
        }

        static void StepOne(World w, int i)
        {
            NavState nav = w.Entities.Nav[i];
            byte team = w.Entities.Team[i];
            Fix2 pos = w.Entities.Position[i];

            Fix travelled = w.Entities.Has(i, ComponentMask.Mover)
                ? w.Entities.Mover[i].LastStepDistance
                : Fix.Zero;

            if (!IsDenied(w, i))
            {
                // Someone is telling it where it is. Everything resets, and the
                // re-acquisition progress resets too - a drone that comes back
                // into coverage has not half-earned a lock it no longer needs.
                nav.ErrorMetres = Fix.Zero;
                nav.MetresSinceFix = Fix.Zero;
                nav.ReacquireProgress = Fix.Zero;
                nav.HasLock = nav.Aid == NavAid.SceneMatching;
                w.Entities.Nav[i] = nav;
                return;
            }

            nav.MetresSinceFix += travelled;

            if (nav.Aid == NavAid.SceneMatching)
            {
                bool matchable = CanMatchHere(w, team, pos);

                if (nav.HasLock)
                {
                    if (matchable)
                    {
                        // Bounded, and bounded tight enough that at this game's
                        // compression it rounds to nothing. The tier is not
                        // "more accurate" - it is "knows where it is".
                        nav.ErrorMetres = SimConstants.SceneMatchErrorMetres;
                        nav.ReacquireProgress = Fix.Zero;
                        w.Entities.Nav[i] = nav;
                        return;
                    }

                    // Lock is a cliff, not a slope. The estimator does not get
                    // gently worse; it latches onto the wrong answer and is
                    // simply lost.
                    nav.HasLock = false;
                    nav.ReacquireProgress = Fix.Zero;
                    w.Events.Push(SimEventKind.NavLockLost, w.Tick,
                                  w.Entities.HandleAt(i), EntityHandle.None, team, 0);
                }
                else if (matchable)
                {
                    // Recoverable, which is what makes losing it interesting
                    // rather than merely punishing. A drone that loses its lock
                    // over featureless ground can get it back over a river or a
                    // road - but not instantly, and not for free.
                    nav.ReacquireProgress += travelled;
                    if (nav.ReacquireProgress >= SimConstants.NavReacquireMetres)
                    {
                        nav.HasLock = true;
                        nav.ErrorMetres = SimConstants.SceneMatchErrorMetres;
                        nav.MetresSinceFix = Fix.Zero;
                        nav.ReacquireProgress = Fix.Zero;
                        w.Events.Push(SimEventKind.NavLockRegained, w.Tick,
                                      w.Entities.HandleAt(i), EntityHandle.None, team, 0);
                        w.Entities.Nav[i] = nav;
                        return;
                    }
                }
            }

            // Dead reckoning, either because that is all it has or because scene
            // matching has nothing to work with here.
            //
            // Error is a fraction of distance flown rather than a function of
            // time. The underlying growth is famously cubic in time, but for
            // anything cruising the dominant unknown is the wind it has been
            // flying through, and that scales with distance. Percent-of-distance
            // is both the better model and the one a player can reason about:
            // fly twice as deep, be twice as wrong.
            Fix rate = nav.CelestialHeading
                ? SimConstants.NavDriftRateCelestial
                : SimConstants.NavDriftRateInertial;

            nav.ErrorMetres += travelled * rate;
            w.Entities.Nav[i] = nav;
        }

        /// <summary>
        /// Whether anything is currently telling this drone where it is.
        ///
        /// This is emphatically not the same question as whether it still has an
        /// operator, and the first version of this system got that wrong by asking
        /// about the link. An autonomous munition has no link by design and is not
        /// thereby lost; a fiber drone on a perfect tether over jammed ground very
        /// much is. Losing the operator and losing your position are two failures
        /// with two different causes, and the whole point of the tier system is
        /// that a drone can suffer either without the other.
        ///
        /// What denies position is the same thing that denies most things here:
        /// being over ground the other side holds, where satellite navigation
        /// jamming is ambient rather than an event. That makes the border one
        /// line with two meanings - the operator ends there and so does the fix -
        /// which is legible, and it means a player only has to learn one line.
        /// </summary>
        public static bool IsDenied(World w, int i)
        {
            return w.Territory.OwnerAt(w.Entities.Position[i]) != w.Entities.Team[i];
        }

        /// <summary>
        /// Whether there is anything here worth matching against.
        ///
        /// Two conditions, and the second is the one that surprised us. The first
        /// is obvious: you need stored imagery of this ground, which is what
        /// reconnaissance buys and what bombardment destroys.
        ///
        /// The second is that matching needs the ground to *have features*. The
        /// intuition from the research was that matching terrain shape beats
        /// matching appearance, because shape survives snow and ploughing and
        /// craters while appearance does not. True - but elevation matching
        /// degenerates on flat ground, and this game is set on the East European
        /// plain. So the technique that is robust to everything else is the weaker
        /// one across most of the map, and open steppe is where a drone gets lost.
        /// Roads, rivers, forest edges and rubble are what save it.
        /// </summary>
        public static bool CanMatchHere(World w, byte team, Fix2 pos)
        {
            if (!w.Imagery.HasCoverage(team, pos)) return false;

            TileClass t = w.Terrain.AtPosition(pos);
            return t != TileClass.Open;
        }
    }
}
