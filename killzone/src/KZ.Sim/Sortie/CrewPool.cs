// KILL ZONE - a real-time strategy video game.
// The crew pool: this game's population cap, and the reason its army maths feel
// different from every other strategy game.
//
// A crew caps *concurrency*, not army size. Airframes are cheap stock sitting in
// a hangar; what you are actually limited by is hands on controls. So losing a
// drone never permanently costs you army value the way losing a tank does - it
// costs you materiel and eight seconds of a crew's time. You are never wiped,
// only suppressed.
//
// The exception is what makes rear areas worth raiding. Crews live in buildings.
// One faction flies from remote bays and merely benches its crews when a bay is
// destroyed; the other flies from forward dugouts, and killing a dugout kills
// the people in it for the rest of the match.

namespace KZ.Sim
{
    public sealed class CrewPool
    {
        readonly Crew[] crews;
        int count;
        public readonly byte Team;
        public bool CrewsDieWithQuarters; // true for the forward-dugout faction

        public CrewPool(byte team, int startingCrews, bool crewsDieWithQuarters)
        {
            Team = team;
            CrewsDieWithQuarters = crewsDieWithQuarters;
            crews = new Crew[SimConstants.MaxCrews];
            for (int i = 0; i < startingCrews; i++) AddCrew(EntityHandle.None, 1);
        }

        public int Count { get { return count; } }
        public Crew Get(int i) { return crews[i]; }

        public int AddCrew(EntityHandle homeQuarters, byte rank)
        {
            if (count >= SimConstants.MaxCrews) return -1;
            int id = count++;
            crews[id] = new Crew
            {
                State = CrewState.Ready,
                Rank = rank,
                VerifiedKills = 0,
                StateUntilTick = 0,
                Flying = EntityHandle.None,
                SortiesInWindow = 0,
                WindowStartTick = 0,
                HomeQuarters = homeQuarters,
                LastIdleTick = 0
            };
            return id;
        }

        /// <summary>Assign a crew to the building it flies from. Used at build time.</summary>
        public void SetHomeQuarters(int crewId, EntityHandle quarters)
        {
            if (crewId < 0 || crewId >= count) return;
            crews[crewId].HomeQuarters = quarters;
        }

        public int ReadyCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < count; i++) if (crews[i].IsAvailable) n++;
                return n;
            }
        }

        public int FlyingCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < count; i++) if (crews[i].State == CrewState.Flying) n++;
                return n;
            }
        }

        /// <summary>
        /// Pick the crew that should fly the next sortie.
        ///
        /// The order is fully determined - readiness, then rank, then index - and
        /// it has to be. Two machines simulating the same match must choose the
        /// same crew for the same launch, or their worlds diverge on the very first
        /// contested sortie.
        /// </summary>
        public int SelectForLaunch()
        {
            int best = -1;
            for (int i = 0; i < count; i++)
            {
                Crew c = crews[i];
                if (c.State != CrewState.Ready && c.State != CrewState.Fatigued) continue;

                if (best < 0) { best = i; continue; }

                Crew b = crews[best];
                // A rested crew always beats a tired one.
                if (c.State == CrewState.Ready && b.State == CrewState.Fatigued) { best = i; continue; }
                if (c.State == CrewState.Fatigued && b.State == CrewState.Ready) continue;
                // Then the better crew, then the lower index.
                if (c.Rank > b.Rank) best = i;
            }
            return best;
        }

        public bool Assign(int crewId, EntityHandle drone, int tick)
        {
            if (crewId < 0 || crewId >= count) return false;
            if (!crews[crewId].IsAvailable) return false;

            if (tick - crews[crewId].WindowStartTick > SimConstants.CrewFatigueWindowTicks)
            {
                crews[crewId].WindowStartTick = tick;
                crews[crewId].SortiesInWindow = 0;
            }

            crews[crewId].State = CrewState.Flying;
            crews[crewId].Flying = drone;
            crews[crewId].SortiesInWindow++;
            return true;
        }

        /// <summary>
        /// Hand a crew back. They are unavailable for a few seconds while they
        /// recover, and a crew that has flown five sorties in two minutes comes
        /// back tired and stays that way until it gets a proper break.
        /// </summary>
        public void Release(int crewId, int tick)
        {
            if (crewId < 0 || crewId >= count) return;
            if (crews[crewId].State != CrewState.Flying) return;

            crews[crewId].Flying = EntityHandle.None;

            bool fatigued = crews[crewId].SortiesInWindow >= SimConstants.CrewFatigueSortieThreshold
                            && tick - crews[crewId].WindowStartTick <= SimConstants.CrewFatigueWindowTicks;

            crews[crewId].State = CrewState.Recovering;
            crews[crewId].StateUntilTick = tick + (fatigued
                ? SimConstants.CrewFatiguedRecoveryTicks
                : SimConstants.CrewRecoveryTicks);
        }

        public void Tick(int tick, EventRing events)
        {
            for (int i = 0; i < count; i++)
            {
                switch (crews[i].State)
                {
                    case CrewState.Recovering:
                        if (tick >= crews[i].StateUntilTick)
                        {
                            bool tired = crews[i].SortiesInWindow >= SimConstants.CrewFatigueSortieThreshold;
                            crews[i].State = tired ? CrewState.Fatigued : CrewState.Ready;
                            crews[i].LastIdleTick = tick;
                            events.Push(SimEventKind.CrewStateChanged, tick, EntityHandle.None,
                                        EntityHandle.None, Team, i);
                        }
                        break;

                    case CrewState.Fatigued:
                        if (tick - crews[i].LastIdleTick >= SimConstants.CrewFatigueClearIdleTicks)
                        {
                            crews[i].State = CrewState.Ready;
                            crews[i].SortiesInWindow = 0;
                            crews[i].WindowStartTick = tick;
                        }
                        break;

                    case CrewState.Benched:
                        if (tick >= crews[i].StateUntilTick)
                        {
                            crews[i].State = CrewState.Ready;
                            crews[i].LastIdleTick = tick;
                        }
                        break;
                }
            }
        }

        public void CreditKill(int crewId, int tick, EventRing events)
        {
            if (crewId < 0 || crewId >= count) return;
            crews[crewId].VerifiedKills++;
            int k = crews[crewId].VerifiedKills;
            byte oldRank = crews[crewId].Rank;

            byte newRank = 1;
            if (k >= SimConstants.CrewRankUpKills4) newRank = 4;
            else if (k >= SimConstants.CrewRankUpKills3) newRank = 3;
            else if (k >= SimConstants.CrewRankUpKills2) newRank = 2;

            if (newRank > oldRank)
            {
                crews[crewId].Rank = newRank;
                events.Push(SimEventKind.CrewPromoted, tick, EntityHandle.None,
                            EntityHandle.None, Team, crewId);
            }
        }

        /// <summary>
        /// A crew quarters has been destroyed. For the remote-piloting faction the
        /// people are elsewhere and merely lose their consoles for a while; for the
        /// forward-dugout faction they are gone for the rest of the match. Four
        /// successful raids on four dugouts do not cost that player buildings, they
        /// cost them the ability to fly anything at all.
        /// </summary>
        public int OnQuartersDestroyed(EntityHandle quarters, int tick, EventRing events)
        {
            int affected = 0;
            for (int i = 0; i < count; i++)
            {
                if (crews[i].HomeQuarters != quarters) continue;
                if (crews[i].State == CrewState.KIA) continue;

                affected++;
                if (CrewsDieWithQuarters)
                {
                    crews[i].State = CrewState.KIA;
                    crews[i].Flying = EntityHandle.None;
                    events.Push(SimEventKind.CrewKilled, tick, quarters, EntityHandle.None, Team, i);
                }
                else
                {
                    crews[i].State = CrewState.Benched;
                    crews[i].StateUntilTick = tick + SimConstants.CrewBenchedTicks;
                    crews[i].Flying = EntityHandle.None;
                    events.Push(SimEventKind.CrewStateChanged, tick, quarters, EntityHandle.None, Team, i);
                }
            }
            return affected;
        }

        /// <summary>Veterancy adds link robustness: an experienced crew pushes through interference.</summary>
        public int LinkBonusFor(int crewId)
        {
            if (crewId < 0 || crewId >= count) return 0;
            return crews[crewId].Rank >= 3 ? SimConstants.VeteranLinkRobustnessBonus : 0;
        }

        public ulong StateHash()
        {
            ulong h = 1469598103934665603UL;
            for (int i = 0; i < count; i++)
            {
                h = (h ^ (ulong)crews[i].State) * 1099511628211UL;
                h = (h ^ (ulong)crews[i].Rank) * 1099511628211UL;
                h = (h ^ (ulong)crews[i].VerifiedKills) * 1099511628211UL;
                h = (h ^ (ulong)crews[i].StateUntilTick) * 1099511628211UL;
                h = (h ^ crews[i].Flying.Value) * 1099511628211UL;
            }
            return h;
        }
    }
}
