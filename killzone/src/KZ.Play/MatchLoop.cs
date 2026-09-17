// KILL ZONE - a real-time strategy video game.
// The match: a World, a clock that advances it, and the queue orders arrive on.
//
// Everything here is host-agnostic on purpose. It knows nothing about HTTP,
// sockets, files or Mono - it takes commands as already-parsed values and hands
// back JSON strings. Host.cs is the only file that knows how those strings get
// to a browser, and replacing it with an ASP.NET controller should not require
// reading this one.
//
// The clock is the only interesting part. The simulation is fixed-step at 32 Hz
// and must stay that way - determinism is the product - so real time never
// enters the simulation. It only decides *how many* whole ticks are owed, and a
// long stall is dropped rather than caught up, because a browser tab that was
// backgrounded for a minute must not be paid back with two thousand ticks in one
// frame.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using KZ.Sim;

namespace KZ.Play
{
    public sealed class MatchLoop
    {
        public struct Objective
        {
            public string Name;
            public EntityHandle Handle;
        }

        struct LoggedEvent
        {
            public int Seq;
            public int PlaySeconds;
            public string Kind;
            public string Text;
        }

        readonly object gate = new object();
        readonly List<Command> inbox = new List<Command>();
        readonly List<LoggedEvent> log = new List<LoggedEvent>();
        readonly Stopwatch clock = new Stopwatch();

        World world;
        ulong seed;
        int startTick;
        double owedTicks;
        int eventSeq;

        /// <summary>
        /// How many ticks may be made up in one service call. Eight is a quarter
        /// of a second of play: enough to ride out a garbage collection, short
        /// enough that a stall is visibly a stall rather than a silent jump.
        /// </summary>
        const int MaxCatchUpTicks = 8;

        /// <summary>The last 200 lines of narration. The log is a view, not state.</summary>
        const int MaxLog = 200;

        public bool Paused { get; private set; }
        public int Speed { get; private set; }
        public string Outcome { get; private set; }
        public Objective[] Objectives { get; private set; }

        public MatchLoop(ulong matchSeed, int startTick)
        {
            seed = matchSeed;
            this.startTick = startTick;
            Reset();
        }

        public void Reset()
        {
            lock (gate)
            {
                world = Scenario.Build(seed, startTick);
                Scenario.DefenderOrders(world, 0);
                log.Clear();
                inbox.Clear();
                eventSeq = 0;
                owedTicks = 0;
                Paused = true;
                Speed = 1;
                Outcome = "playing";
                // Three objectives, one per rung of the ladder the scenario
                // wants the player to climb. The jammer is shallow and inside a
                // fiber lane. The tank is inside the relay's radio range once
                // the jammer is down. The truck is deep, behind the power line
                // and outside radio, so it can only be reached by something
                // that flies on what it brought.
                //
                // Not the defender's command post, which was the first choice
                // and the wrong one: 5,000 structure hit points against a
                // 520-damage shaped warhead is ten sorties of grinding, which
                // teaches nothing the first three did not.
                Objectives = new Objective[]
                {
                    Named("EW Post", 2),
                    Named("Main Tank", 2),
                    Named("Supply Truck", 2)
                };
                clock.Restart();
            }
        }

        Objective Named(string defName, byte team)
        {
            Objective o = new Objective();
            o.Name = defName;
            o.Handle = Scenario.FindFirst(world, team, defName);
            return o;
        }

        public int EventSeq { get { return eventSeq; } }

        // ---- the clock ------------------------------------------------------

        /// <summary>
        /// Advance the match by however many whole ticks real time has earned.
        /// Called from the host's timer; the lock is what keeps a snapshot from
        /// being taken halfway through a Step.
        /// </summary>
        public void Service()
        {
            lock (gate)
            {
                double elapsed = clock.Elapsed.TotalSeconds;
                clock.Restart();
                if (Paused || Outcome != "playing") { DrainInboxOnly(); return; }

                owedTicks += elapsed * SimConstants.TicksPerSecond * Speed;
                int ticks = (int)owedTicks;
                if (ticks <= 0) return;
                owedTicks -= ticks;
                if (ticks > MaxCatchUpTicks) { ticks = MaxCatchUpTicks; owedTicks = 0; }

                for (int n = 0; n < ticks; n++)
                {
                    for (int i = 0; i < inbox.Count; i++) world.Enqueue(inbox[i]);
                    inbox.Clear();
                    Scenario.DefenderOrders(world, world.Tick + 1);
                    world.Step();
                    Harvest();
                    CheckOutcome();
                    if (Outcome != "playing") break;
                }
            }
        }

        /// <summary>
        /// While paused, orders still queue - the player is allowed to line up a
        /// sortie in a frozen frame - but nothing is applied until time runs
        /// again, because a command applied outside a Step would not be in the
        /// tick that produced its events and would not replay.
        /// </summary>
        void DrainInboxOnly() { }

        void CheckOutcome()
        {
            bool allDead = true;
            for (int i = 0; i < Objectives.Length; i++)
                if (world.Entities.IsAlive(Objectives[i].Handle)) allDead = false;
            if (allDead) { Outcome = "won"; Push("the sector is yours - every objective is down"); return; }

            EntityHandle cp = Scenario.FindFirst(world, Scenario.PlayerTeam, "Command Post");
            if (!world.Entities.IsAlive(cp)) { Outcome = "lost"; Push("your command post is gone"); }
        }

        // ---- orders ---------------------------------------------------------

        public void Enqueue(Command c) { lock (gate) { inbox.Add(c); } }

        public void SetPaused(bool paused) { lock (gate) { Paused = paused; clock.Restart(); } }

        public void SetSpeed(int speed)
        {
            lock (gate)
            {
                Speed = speed < 1 ? 1 : (speed > 8 ? 8 : speed);
            }
        }

        /// <summary>
        /// One tick of play, applied while paused. This is how a pause becomes a
        /// usable instrument rather than a freeze: the whole point of pausing in
        /// this game is to read a link pip or a contact bearing, and one tick at a
        /// time is how you watch a turret finish slewing.
        /// </summary>
        public void StepOnce()
        {
            lock (gate)
            {
                if (Outcome != "playing") return;
                for (int i = 0; i < inbox.Count; i++) world.Enqueue(inbox[i]);
                inbox.Clear();
                Scenario.DefenderOrders(world, world.Tick + 1);
                world.Step();
                Harvest();
                CheckOutcome();
            }
        }

        // ---- reading --------------------------------------------------------

        public string StaticJson()
        {
            lock (gate) { return Snapshot.Static(world); }
        }

        public string ViewJson(int sinceSeq)
        {
            lock (gate) { return Snapshot.View(world, Scenario.PlayerTeam, this, sinceSeq); }
        }

        /// <summary>Resolve a handle the browser sent back, or None.</summary>
        public EntityHandle Resolve(long handleValue)
        {
            lock (gate)
            {
                EntityHandle h = new EntityHandle((uint)handleValue);
                return world.Entities.IsAlive(h) ? h : EntityHandle.None;
            }
        }

        public void WriteEventsSince(JsonWriter j, int sinceSeq)
        {
            for (int i = 0; i < log.Count; i++)
            {
                if (log[i].Seq <= sinceSeq) continue;
                j.BeginObject();
                j.Field("seq", log[i].Seq);
                j.Field("t", log[i].PlaySeconds);
                j.Field("kind", log[i].Kind);
                j.Field("text", log[i].Text);
                j.EndObject();
            }
        }

        // ---- narration ------------------------------------------------------

        /// <summary>
        /// Read this tick's events and keep the ones the player's side would know
        /// about. Run inside the tick's lock and before the next Step, because the
        /// event ring is cleared at the top of every Step and because an entity
        /// named by an event is still alive to be asked about here - deaths flush
        /// at the end of the tick that killed them.
        /// </summary>
        void Harvest()
        {
            for (int i = 0; i < world.Events.Count; i++)
            {
                SimEvent e = world.Events[i];
                if (!Visible(e)) continue;
                string text = Describe(e);
                if (text == null) continue;
                Push(text, e.Kind.ToString());
            }
        }

        /// <summary>
        /// Whether this side would know. Anything its own units did, plus anything
        /// that happened to something its sensors are holding. The enemy losing a
        /// link is the enemy's business and does not appear in your log, which is
        /// the same rule the map draws by.
        /// </summary>
        bool Visible(SimEvent e)
        {
            byte mine = Scenario.PlayerTeam;
            if (e.Team == mine) return true;
            if (world.Entities.IsAlive(e.A))
            {
                if (world.Entities.Team[e.A.Index] == mine) return true;
                return world.IsDetectedBy(mine, e.A);
            }
            return e.Team == 0;
        }

        string Describe(SimEvent e)
        {
            switch (e.Kind)
            {
                case SimEventKind.SortieLaunched: return "sortie away";
                case SimEventKind.SortieRejected: return "launch refused: " + Refusal(e.Param);
                case SimEventKind.SortieRefusedDaylight: return "launch refused: it only flies at night";
                case SimEventKind.SortieRefusedWeather: return "launch refused: the weather grounds it";
                case SimEventKind.LinkAmber: return "a link went amber";
                case SimEventKind.LinkBlack: return "a link went black";
                case SimEventKind.LinkRestored: return "a link came back";
                case SimEventKind.LinkFellBack: return "a drone fell back to its alternate link";
                case SimEventKind.DroneLostToLinkLoss: return "a drone fell out of the sky with nobody flying it";
                case SimEventKind.TetherCut: return "a fiber thread parted";
                case SimEventKind.TetherFound: return "somebody found one of our threads";
                case SimEventKind.CrewKilled: return "a crew was lost with its quarters";
                case SimEventKind.CrewPromoted: return "a crew was promoted";
                // A kill event belongs to whoever made the kill. The defender
                // scoring one on your drone is not "kill confirmed" on your
                // side of the screen, and the first draft of this log said it
                // was - which read as though you were winning while you lost
                // five airframes.
                case SimEventKind.KillVerified:
                    return e.Team == Scenario.PlayerTeam
                         ? "kill confirmed, " + e.Param + " tasking points" : null;
                case SimEventKind.KillUnverified:
                    return e.Team == Scenario.PlayerTeam
                         ? "something died out there - nobody saw it, no points" : null;
                // The one the player most needs and the one a renderer most
                // easily forgets: your own losses. Without this the log says
                // "sortie away" five times and then goes quiet, and a quiet log
                // reads as nothing having happened.
                case SimEventKind.UnitDied:
                    return e.Team == Scenario.PlayerTeam
                         ? "lost: " + NameOf(e.Param)
                         : "down: " + NameOf(e.Param);
                case SimEventKind.StructureDestroyed:
                    return e.Team == Scenario.PlayerTeam
                         ? "we lost the " + NameOf(e.Param)
                         : NameOf(e.Param) + " destroyed";
                case SimEventKind.AutonomyMisidentified: return "an autonomous munition picked the wrong target";
                case SimEventKind.NavLockLost: return "a drone lost its scene-matching lock";
                case SimEventKind.NavLockRegained: return "a drone got its lock back";
                case SimEventKind.NavMissedAimpoint:
                    return "a warhead went off on empty ground - it did not know where it was";
                case SimEventKind.SatelliteCoverageLost: return "over the border: satellite coverage lost";
                case SimEventKind.SatelliteCoverageRegained: return "back over our own ground: satellite restored";
                case SimEventKind.MinesLaid: return "mines laid";
                case SimEventKind.MineDetonated: return "a mine went off";
                case SimEventKind.SalvageCollected: return "salvage recovered";
                // Emission control, from both sides of the map. Theirs is the
                // window the player is waiting for; ours is the price of the
                // one the player just opened. Visible() already decides whether
                // the player would know - an enemy mast going quiet only
                // reaches this log while the player is holding it as a contact,
                // which is the same rule the jamming dome is drawn by.
                case SimEventKind.EmissionsChanged:
                {
                    string what = world.Entities.IsAlive(e.A)
                                ? NameOf(world.Entities.DefId[e.A.Index]) : "an emitter";
                    if (e.Team == Scenario.PlayerTeam)
                        return e.Param != 0 ? "our " + what + " is transmitting again"
                                            : "our " + what + " has gone quiet";
                    return e.Param != 0 ? what + " is transmitting"
                                        : what + " has stopped transmitting";
                }
                default: return null;
            }
        }

        static string Refusal(int param)
        {
            switch ((LaunchResult)param)
            {
                case LaunchResult.NoCrew: return "no crew free";
                case LaunchResult.NoUplinkCapacity: return "no satellite channel";
                case LaunchResult.NoTetherAvailable: return "no spool available";
                case LaunchResult.InsufficientMateriel: return "not enough materiel";
                case LaunchResult.EntityCapacityReached: return "the sky is full";
                default: return "refused";
            }
        }

        static string NameOf(int defId)
        {
            return defId >= 0 && defId < Catalog.Count ? Catalog.Get(defId).Name : "a structure";
        }

        void Push(string text) { Push(text, "Note"); }

        void Push(string text, string kind)
        {
            LoggedEvent le = new LoggedEvent();
            le.Seq = ++eventSeq;
            le.PlaySeconds = world.Tick / SimConstants.TicksPerSecond;
            le.Kind = kind;
            le.Text = text;
            log.Add(le);
            if (log.Count > MaxLog) log.RemoveRange(0, log.Count - MaxLog);
        }
    }
}
