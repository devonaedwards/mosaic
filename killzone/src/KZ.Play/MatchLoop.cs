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

        /// <summary>
        /// What is driving at the player's command post, in the player's own
        /// words, for the one panel that has to say why they are about to lose.
        ///
        /// It exists because the losing condition in this scenario is a ground
        /// advance (Scenario.AdvanceBoundPlaySeconds) and because a loss the
        /// player cannot see coming is a cutscene. Measured with nobody at the
        /// keyboard the column takes eight play-minutes to arrive, which is a long
        /// warning - and a warning nothing draws is not a warning.
        ///
        /// **It reports what the player knows, not what is there.** The first
        /// version reported live contacts only, by the same IsDetectedBy rule the
        /// map draws by, and went blank the moment the player's sensors died -
        /// measured, one unbroken window of 61 play-seconds from T+339 to T+399 in
        /// the passive losing run, during which the thing about to kill the player
        /// closed from 4,483 m to 2,610 m and crossed two of the four warning
        /// bands unannounced. A blank panel is not the player experiencing
        /// blindness; it is the player experiencing a panel that has stopped
        /// working, and the two look identical.
        ///
        /// The fix is not to reveal the vehicle - the fog is the best thing in
        /// this build (FINDINGS 35) and a panel that saw through it would be worse
        /// than silence. It is to report three things the player has already
        /// earned and one fact about their own side:
        ///
        /// - the last position their own sensors held (<see cref="Metres"/>),
        ///   frozen at the observation rather than tracking the vehicle;
        /// - how old that is (<see cref="StaleSeconds"/>), so the number visibly
        ///   decays instead of quietly lying;
        /// - whether it is a live hold or a memory (<see cref="Live"/>);
        /// - and whether the player has lost eyes since (<see cref="Blind"/>),
        ///   which is a count of the player's own surviving ground sensors and
        ///   therefore not information about the enemy at all.
        ///
        /// Nothing here is derived from an entity the player is not holding or has
        /// not held. Going blind becomes an event that happened *to* the player
        /// rather than an absence of events.
        /// </summary>
        public struct ThreatReport
        {
            /// <summary>
            /// Whether the player knows about an enemy ground vehicle at all -
            /// live now, or held at some point and remembered. False only before
            /// the first contact of the match.
            /// </summary>
            public bool Any;

            /// <summary>
            /// Whether the player's own sensors are holding it now. When false and
            /// <see cref="Any"/> is true, everything below is a memory.
            /// </summary>
            public bool Live;

            public string Name;

            /// <summary>
            /// Metres from the player's command post to the last position their
            /// sensors held this vehicle at. It does not move while the contact is
            /// stale, which is the point: a range that kept updating would be the
            /// fog-of-war leak this panel exists not to be.
            /// </summary>
            public int Metres;

            /// <summary>
            /// Whether it was standing inside a jamming bubble the player can see -
            /// which is to say whether a radio-linked drone sent at it right now
            /// would probably arrive with nobody flying it. An instantaneous fact
            /// about the bubble and deliberately not a claim about the column's
            /// doctrine: see the "giving ground" line below for why those two got
            /// confused once already. As stale as the rest of the report when
            /// <see cref="Live"/> is false.
            /// </summary>
            public bool Jammed;

            /// <summary>
            /// Play-seconds since the observation above. Zero while
            /// <see cref="Live"/>.
            /// </summary>
            public int StaleSeconds;

            /// <summary>
            /// How many of the player's own ground units still carry a sensor, and
            /// how many they have had shot off them since the match began.
            ///
            /// Counted over the player's own units and nothing else, so it says
            /// nothing whatever about where the enemy is. It is what turns "we
            /// cannot see it" into "we cannot see", and only the second tells the
            /// player what to do about it.
            ///
            /// Two counts rather than a "blind" flag, because a flag would have had
            /// to be a judgement and these are facts. The first version of this
            /// field *was* a flag - fewer eyes than at the last hold - and it never
            /// once fired in the run it was written for: the Radar Mast dies at
            /// T+338 and the contact survives it by a second, so by the last hold
            /// the loss had already been folded into the baseline it compared
            /// against. A count has no such moment to be on the wrong side of.
            /// </summary>
            public int Eyes;
            public int EyesLost;
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
        public ThreatReport Threat { get; private set; }

        /// <summary>
        /// The closest band the log has already shouted about, so the warnings
        /// fire once each on the way in rather than every tick. -1 is "nothing
        /// announced yet".
        /// </summary>
        int threatBand = -1;

        /// <summary>
        /// The closest the player has ever held a ground threat, and whether the
        /// log has already said it is going the other way. Int.MaxValue is "never
        /// held one".
        /// </summary>
        int threatClosestMetres = int.MaxValue;
        int givingGroundTicks;
        bool givingGroundAnnounced;

        /// <summary>
        /// What the log wants before it calls a column a withdrawal: 1,500 m back
        /// off its own closest approach - more than one bound - and held for forty
        /// play-seconds, which is one whole bound.
        ///
        /// The second condition is not belt and braces, it is the fix for a wrong
        /// line in the log. "The nearest enemy vehicle you are holding" flaps
        /// between vehicles: an advancing tank drops in and out of a radar track
        /// as its heading swings through the Doppler notch, and when it drops out
        /// the nearest thing the player still holds is the escort jammer two
        /// kilometres behind it, on its radio signature. Measured over the first
        /// two hundred play-seconds, the reported range steps 10,246 → 12,298 →
        /// 9,369 → 10,698 for that reason alone. One sample of that is
        /// indistinguishable from a retreat; forty play-seconds of it is not.
        /// </summary>
        const int GivingGroundMetres = 1500;
        static readonly int GivingGroundTicks = SimConstants.PlaySeconds(40);

        /// <summary>
        /// The last observation the player's own sensors made of a ground
        /// vehicle, and when. This is the whole of the panel's memory: a name, a
        /// range, whether it was jammed, the tick it was taken on, and how many
        /// eyes the player had on the ground at the time.
        ///
        /// It lives here rather than in the World because it is a view. Nothing
        /// below is hashed, nothing below is a command, and deleting the lot
        /// changes no simulation result - the same argument the rest of
        /// CheckThreat already makes for itself.
        /// </summary>
        string knownName;
        int knownMetres;
        bool knownJammed;
        int knownAtTick;
        bool everHeld;
        int eyesHighWater;
        bool contactLostAnnounced;
        bool staleAnnounced;

        /// <summary>
        /// How long the panel goes on calling a contact live after the last tick
        /// it was actually held.
        ///
        /// It is the track hold (SimConstants.TrackHoldTicks, audit F2) argued one
        /// layer up. Measured in the passive losing run, the nearest held ground
        /// vehicle drops out for exactly nine ticks - 0.28 of a real second - as
        /// the contact flips between the tank and the escort behind it, and the
        /// list of held vehicles changes name fifteen times over the match.
        /// Without a hold that nine-tick gap is a "contact lost" and a "we have it
        /// again" on the same play-second, which is the stutter this log has
        /// already been corrected for twice. Five play-seconds of a 7.5 m/s tank
        /// is 37 m, so the range the panel shows through the hold is not
        /// materially older than the one it showed before it. Designer estimate.
        /// </summary>
        static readonly int ThreatHoldTicks = SimConstants.PlaySeconds(5);

        /// <summary>
        /// How stale the panel's range has to get before the log says so in words,
        /// and how stale it has to have been for a regained contact to be news.
        ///
        /// Thirty play-seconds is a Main Tank's 225 m at its own top speed -
        /// about a twentieth of the last band the log warns at - so it is the
        /// point past which the number on the panel is worth distrusting. Pacing,
        /// so a designer estimate.
        /// </summary>
        const int StaleWarnPlaySeconds = 30;

        /// <summary>
        /// The ranges the log narrates a ground advance at, in metres to the
        /// player's command post. Twelve kilometres is about where the player's
        /// own forward sensors first hold a moving vehicle; three thousand is a
        /// Main Tank's main gun, so it is the last band at which anything can
        /// still be done from a standing start. Pacing, so designer estimates.
        /// </summary>
        static readonly int[] ThreatBandsMetres = { 12000, 8000, 4000, 3000 };

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
                Threat = new ThreatReport();
                threatBand = -1;
                threatClosestMetres = int.MaxValue;
                givingGroundTicks = 0;
                givingGroundAnnounced = false;
                knownName = null;
                knownMetres = 0;
                knownJammed = false;
                knownAtTick = 0;
                everHeld = false;
                eyesHighWater = 0;
                contactLostAnnounced = false;
                staleAnnounced = false;
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
                    CheckThreat();
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

        /// <summary>
        /// Work out what the player knows about what is driving at them, and say
        /// so in the log the first time it crosses each band - and, since the
        /// rewrite, the first time the player stops being able to see it.
        ///
        /// This is a view over the world, not state in it: nothing here is hashed,
        /// nothing here is a command, and removing the whole function changes no
        /// simulation result. The fields it keeps are so that the log stays a
        /// record of things happening rather than a stopwatch printing once a
        /// tick, and so that a range the player earned once survives the sensor
        /// that earned it.
        ///
        /// One pass over the entity table, which is the pass it always made: the
        /// player's own ground sensors are counted in the same loop that looks for
        /// the nearest enemy vehicle rather than in a second one.
        /// </summary>
        void CheckThreat()
        {
            EntityHandle cp = Scenario.FindFirst(world, Scenario.PlayerTeam, "Command Post");
            if (!world.Entities.IsAlive(cp)) return;
            Fix2 home = world.Entities.Position[cp.Index];

            bool held = false;
            string name = null;
            int metres = 0;
            bool jammed = false;
            int eyes = 0;
            Fix bestSq = Fix.MaxValue;
            for (int i = 1; i < world.Entities.HighWater; i++)
            {
                if (!world.Entities.IsSlotAlive(i)) continue;
                if (world.Entities.Team[i] == 0) continue;
                if (world.Entities.EntityLayer[i] != Layer.Ground) continue;

                // The player's own eyes on this ground, counted here because the
                // loop is already running. Ground only: the player's sorties carry
                // sensors too and there are dozens of them over a match, so
                // counting those would make "we have lost our eyes" flicker with
                // every FPV that finished its run.
                if (world.Entities.Team[i] == Scenario.PlayerTeam)
                {
                    if (world.Entities.Has(i, ComponentMask.Sensor)) eyes++;
                    continue;
                }

                if (world.Entities.Has(i, ComponentMask.Structure)) continue;
                EntityHandle h = world.Entities.HandleAt(i);
                if (!world.IsDetectedBy(Scenario.PlayerTeam, h)) continue;
                Fix dSq = Fix2.SqrDistance(home, world.Entities.Position[i]);
                if (dSq >= bestSq) continue;
                bestSq = dSq;
                held = true;
                name = Catalog.Get(world.Entities.DefId[i]).Name;
                metres = Fix2.Distance(home, world.Entities.Position[i]).RoundToInt();
                jammed = InVisibleJamming(world.Entities.Position[i]);
            }

            if (held)
            {
                // Regaining a contact after a long silence is news, and it is the
                // one place the player finds out what the blindness cost them: the
                // difference between two ranges they earned themselves. Not a leak
                // - both numbers are observations the player's own sensors made.
                if (everHeld && world.Tick - knownAtTick >= SimConstants.PlaySeconds(StaleWarnPlaySeconds))
                {
                    string line = "we have the " + name + " again - " + Km(metres);
                    if (name == knownName && knownMetres > metres)
                        line += ", " + Km(knownMetres - metres) + " closer than when we lost it";
                    Push(line);
                }
                knownName = name;
                knownMetres = metres;
                knownJammed = jammed;
                knownAtTick = world.Tick;
                everHeld = true;
                contactLostAnnounced = false;
                staleAnnounced = false;
            }

            // High water rather than the count at the last hold, so that a sensor
            // built during the match raises the bar and one shot off it never
            // lowers it. Nothing in this scenario builds a ground sensor; the rule
            // is written this way so that a scenario which does cannot make the
            // player look blinder than they are.
            if (eyes > eyesHighWater) eyesHighWater = eyes;

            int sinceHeld = everHeld ? world.Tick - knownAtTick : 0;
            bool live = held || (everHeld && sinceHeld < ThreatHoldTicks);
            int eyesLost = eyesHighWater - eyes;

            ThreatReport t = new ThreatReport();
            t.Any = everHeld;
            t.Live = live;
            t.Name = knownName;
            t.Metres = knownMetres;
            t.Jammed = knownJammed;
            t.StaleSeconds = live ? 0 : sinceHeld / SimConstants.TicksPerSecond;
            t.Eyes = eyes;
            t.EyesLost = eyesLost;
            Threat = t;

            if (!everHeld) return;

            // Losing the contact, said once. This is the line the first version of
            // this panel did not have and most needed: with nobody at the keyboard
            // the player's Radar Mast dies at T+337 and the panel simply stopped,
            // with a Main Tank 4.5 km from the command post and closing.
            //
            // Which of the two sentences it is matters more than either. "We
            // cannot see it" is the vehicle's doing and there may be nothing to be
            // done; "we cannot see" is the player's own loss and tells them what
            // to rebuild. The difference is a count of the player's own units, so
            // deciding it costs nothing and reveals nothing.
            if (!live && !contactLostAnnounced)
            {
                contactLostAnnounced = true;
                Push(eyesLost > 0
                     ? "we have lost the " + knownName + " at " + Km(knownMetres)
                       + " - we are down to " + eyes + " of the " + eyesHighWater
                       + " things we had watching that ground"
                     : "we have lost the " + knownName + " at " + Km(knownMetres));
            }

            // And the line that says the number on the panel has stopped being a
            // position and become a memory. A player who is told a range and never
            // told it is old will fly against it.
            if (!live && !staleAnnounced
                && sinceHeld >= SimConstants.PlaySeconds(StaleWarnPlaySeconds))
            {
                staleAnnounced = true;
                Push("no eyes on the " + knownName + " for " + StaleWarnPlaySeconds
                     + " seconds - it is not at " + Km(knownMetres) + " any more");
            }

            // The band warnings, once each, and only ever inward. A column that
            // has been driven back does not un-announce itself; the player was
            // told, and the log is a history.
            //
            // Live holds only. A band crossed on a remembered range is a warning
            // about where a vehicle was, which is the one thing this panel is for
            // not doing - and in the losing run the four-kilometre band would fire
            // off a thirty-second-old number while the tank was already at three.
            //
            // One line per pass, for the nearest band crossed, rather than one
            // per band. A player whose forward sensors have been shot off loses
            // the contact and regains it much closer - measured, at 2,280 m after
            // losing it at 4,483 - and the first version announced "four
            // kilometres" and "inside a tank's gun" on the same play-second, which
            // reads as a stutter rather than as news.
            if (!live) return;

            int worst = -1;
            for (int b = 0; b < ThreatBandsMetres.Length; b++)
                if (knownMetres <= ThreatBandsMetres[b] && b > threatBand) worst = b;
            if (worst >= 0)
            {
                threatBand = worst;
                switch (worst)
                {
                    case 0: Push("armour is moving on us - a " + knownName
                                 + " inside twelve kilometres of the command post"); break;
                    case 1: Push("the " + knownName + " is eight kilometres out and still coming"); break;
                    case 2: Push("four kilometres. it will be in range of the command post shortly"); break;
                    default: Push("the command post is inside a tank's gun - this is how the sector is lost"); break;
                }
            }

            // And the half that says the answer is working, stated once.
            //
            // This used to read the jamming bubble - "their column has nothing
            // jamming for it" the moment the threat was not inside a visible dome
            // - and it was wrong, visibly, in the first live run: it fired at
            // T+120 with the escort alive and merely in the quiet half of its
            // emission cycle, while the column carried on advancing. The gate on
            // the advance is whether the escort is *alive*; the dome is whether it
            // is *radiating*; the log had conflated them.
            //
            // The honest version does not read either. It reads the only thing the
            // player can actually verify - the range is going up - which is true
            // whatever the reason and is what they wanted to know.
            if (knownMetres < threatClosestMetres) threatClosestMetres = knownMetres;
            if (knownMetres > threatClosestMetres + GivingGroundMetres) givingGroundTicks++;
            else givingGroundTicks = 0;
            if (!givingGroundAnnounced && threatBand >= 0 && givingGroundTicks >= GivingGroundTicks)
            {
                givingGroundAnnounced = true;
                Push("they are giving ground - whatever you did to that column, keep doing it");
            }
        }

        /// <summary>
        /// A range in the units a player reads, to one decimal place, by integer
        /// arithmetic. This is a view and not a tick, so a double here would break
        /// nothing - but every other number in this file is exact and a rounding
        /// difference between two machines' logs would be a false determinism
        /// report the next time somebody diffs two runs.
        /// </summary>
        static string Km(int metres)
        {
            return (metres / 1000) + "." + ((metres % 1000) / 100) + " km";
        }

        /// <summary>
        /// Whether a point is inside an enemy jamming bubble the player can
        /// actually see. Same rule Snapshot draws a dome by: a bubble the player
        /// is not holding the emitter for is not on their map and must not be in
        /// their log either.
        /// </summary>
        bool InVisibleJamming(Fix2 at)
        {
            for (int e = 0; e < world.Signal.EmitterCount; e++)
            {
                JamEmitter em = world.Signal.GetEmitter(e);
                if (em.Team == Scenario.PlayerTeam) continue;
                if (!world.IsDetectedBy(Scenario.PlayerTeam, em.Source)) continue;
                if (Fix2.SqrDistance(at, em.Position) <= em.RadiusMetres * em.RadiusMetres) return true;
            }
            return false;
        }

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
                CheckThreat();
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
                case SimEventKind.TetherFound: return "somebody walked onto one of our threads - they have a bearing to the pad";
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
