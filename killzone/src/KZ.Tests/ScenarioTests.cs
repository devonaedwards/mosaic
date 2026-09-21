// KILL ZONE - a real-time strategy video game.
// Tests for the one shipped scenario and for the match's losing condition.
//
// This file is new and so is the reference that lets it exist: until now
// KZ.Tests saw only KZ.Sim, so nothing in the suite could reach Scenario.cs or
// MatchLoop.cs - and MatchLoop's losing branch shipped for three sessions
// unreachable and unexercised. A win and a loss condition are production code
// and the scenario is the only thing a player ever plays.
//
// Everything below drives the real thing. The MatchLoop tests call StepOnce,
// which is the function the host's timer calls, and post player orders through
// Enqueue, which is the queue the browser posts onto. The scenario tests call
// Scenario.Build, Scenario.DefenderOrders and World.Step in the same order
// MatchLoop.StepOnce calls them, because that pair *is* the production path for
// the opposition's doctrine. Nothing here writes a component, spawns a unit of
// its own or calls a system directly - which is the point: the failure this
// repository is organised around is a test that builds the state it asserts on.
//
// Nothing here needed a new public member on MatchLoop or Scenario either. A
// row of accessors that only this file calls is the dead-symbol guard's loudest
// category and it would be this same mistake wearing a lab coat.

using KZ.Sim;
using KZ.Play;

namespace KZ.Tests
{
    public static class ScenarioTests
    {
        static Fix F(double v) { return Fix.FromDoubleContentOnly(v); }
        static Fix2 P(double x, double y) { return new Fix2(F(x), F(y)); }

        const ulong Seed = 20260917UL;

        /// <summary>Advance a match by whole play-seconds through the host's own path.</summary>
        static void Run(MatchLoop m, int playSeconds)
        {
            int ticks = playSeconds * SimConstants.TicksPerSecond;
            for (int i = 0; i < ticks; i++) m.StepOnce();
        }

        /// <summary>
        /// The same two calls MatchLoop.StepOnce makes, for the tests that need to
        /// look at the world rather than at the match's verdict on it.
        /// </summary>
        static void Tick(World w)
        {
            Scenario.DefenderOrders(w, w.Tick + 1);
            w.Step();
        }

        static World NewMatch()
        {
            World w = Scenario.Build(Seed, 0);
            Scenario.DefenderOrders(w, 0);
            return w;
        }

        static Fix XOf(World w, EntityHandle h)
        {
            return w.Entities.IsAlive(h) ? w.Entities.Position[h.Index].X : Fix.Zero;
        }

        public static void Register(TestRunner r)
        {
            r.Group("the shipped scenario: losing it");

            // The headline, and the whole of FINDINGS 42. FINDINGS 35 said "you
            // cannot lose" and it was true for three sessions: the branch
            // existed and nothing in the game could reach it.
            r.Run("an unanswered armoured advance takes the sector", delegate
            {
                MatchLoop m = new MatchLoop(Seed, 0);
                Assert.Equal("playing", m.Outcome, "outcome at the start");
                Run(m, 540);
                Assert.Equal("lost", m.Outcome, "outcome after nine play-minutes of doing nothing");
            });

            // A loss the interface does not narrate is a cutscene with extra
            // steps, so the warning is under test with the loss.
            r.Run("the match warns about the advance long before it lands", delegate
            {
                MatchLoop m = new MatchLoop(Seed, 0);
                Run(m, 200);
                Assert.Equal("playing", m.Outcome, "still playing at T+200");
                Assert.True(m.Threat.Any, "the threat readout is holding a vehicle");
                Assert.True(m.Threat.Metres < 12000,
                            "the readout has it inside twelve kilometres");
                Assert.True(m.ViewJson(0).Contains("armour is moving on us"),
                            "the log has told the player, in words");
            });

            // The other half of the same requirement, through the same path: a
            // loss condition that fires whatever the player does is a cutscene.
            // This is the shortest competent answer - kill the column's escort
            // jammer with the one rung it cannot jam - driven entirely through
            // MatchLoop.Enqueue, which is the queue the browser posts onto.
            r.Run("a player who answers the advance keeps the sector", delegate
            {
                MatchLoop m = new MatchLoop(Seed, 0);

                // A MatchLoop does not hand out its world, so the target handle
                // comes from a second Build on the same seed. That is sound
                // rather than a trick: a handle is an index and a generation, and
                // Build spawns the same units in the same order, so the same
                // vehicle has the same handle in both. It is also what the
                // browser does - it learns handles from the snapshot and posts
                // them back - so this is the client's own path, not a back door.
                EntityHandle escort = Scenario.FindFirst(Scenario.Build(Seed, 0), 2, "EW Truck");
                Assert.True(!escort.IsNone, "the defence starts with an escort jammer");

                for (int s = 0; s < 540; s++)
                {
                    if (s % 8 == 0 && s < 240)
                        m.Enqueue(Command.LaunchSortie(1, Catalog.IdOf("Fiber FPV Team"),
                                                       P(9360, 9360), escort, 0));
                    Run(m, 1);
                }
                Assert.Equal("playing", m.Outcome, "outcome after nine play-minutes of answering");
                Assert.True(m.ViewJson(0).Contains("giving ground"),
                            "the log has told the player their answer is working");
            });

            r.Group("the shipped scenario: the advance, and stopping it");

            // The column is gated rather than scheduled: it moves only while it
            // has a jammer over it. This is the forward arm.
            r.Run("the column closes on the command post while it has cover", delegate
            {
                World w = NewMatch();
                EntityHandle tank = Scenario.FindFirst(w, 2, "Main Tank");
                Fix start = XOf(w, tank);
                for (int i = 0; i < 200 * SimConstants.TicksPerSecond; i++) Tick(w);
                Assert.True(w.Entities.IsAlive(tank), "the tank is alive at T+200");
                Assert.True(XOf(w, tank) < start - Fix.FromInt(4000),
                            "the tank has closed at least four kilometres");
            });

            // And the arm that makes it a decision rather than a clock. Radio
            // cannot do this job - the escort denies radio, which is what it is
            // for - so the answer is the unjammable rung, at the price
            // FINDINGS 39 put on it.
            r.Run("killing the escort jammer turns the column round", delegate
            {
                World w = NewMatch();
                EntityHandle escort = Scenario.FindFirst(w, 2, "EW Truck");
                EntityHandle tank = Scenario.FindFirst(w, 2, "Main Tank");
                Assert.True(!escort.IsNone, "the defence starts with an escort jammer");

                for (int s = 0; s < 200; s++)
                {
                    if (s % 8 == 0 && w.Entities.IsAlive(escort))
                        w.Enqueue(Command.LaunchSortie(1, Catalog.IdOf("Fiber FPV Team"),
                                                       P(9360, 9360), escort, 0));
                    for (int i = 0; i < SimConstants.TicksPerSecond; i++) Tick(w);
                }
                Assert.False(w.Entities.IsAlive(escort), "the escort jammer is destroyed");

                Fix afterwards = XOf(w, tank);
                for (int i = 0; i < 200 * SimConstants.TicksPerSecond; i++) Tick(w);
                Assert.True(XOf(w, tank) > afterwards,
                            "with no cover the column has given ground rather than taken it");
                Assert.True(w.Entities.IsAlive(Scenario.FindFirst(w, 1, "Command Post")),
                            "the player still holds the sector");
            });

            // FINDINGS 38 built ground-search radar with a Doppler notch and
            // recorded that it did nothing in the shipped scenario, because the
            // player's Radar Mast sits at x=5,400 and the nearest enemy vehicle
            // was 11.4 km away against 8,044 m of reach. An advance is what puts
            // a moving vehicle inside it.
            r.Run("the advance is what makes ground radar matter in this scenario", delegate
            {
                World w = NewMatch();
                EntityHandle tank = Scenario.FindFirst(w, 2, "Main Tank");
                Assert.True(w.TrackQualityOf(1, tank) != TrackQuality.Radar,
                            "no radar track on the tank where it starts");

                bool onRadar = false;
                for (int i = 0; i < 400 * SimConstants.TicksPerSecond && !onRadar; i++)
                {
                    Tick(w);
                    if (w.Entities.IsAlive(tank) && w.TrackQualityOf(1, tank) == TrackQuality.Radar)
                        onRadar = true;
                }
                Assert.True(onRadar, "the player's mast holds the advancing tank on radar");
            });

            r.Group("the shipped scenario: the geometry that hid the loss");

            // The premise this work began from was that the Command Post's 5,000
            // hit points made the loss unreachable. They did not: the defence's
            // radio could not carry a drone to the Command Post at all, so forty-
            // three sorties aimed at nothing else took zero hit points off it.
            // The fact is a property of three numbers in three different files -
            // the defender's pad, the radio anchor list and RadioRangeMetres -
            // and it is worth a test because moving any of them silently changes
            // what the opposition is capable of.
            r.Run("a radio raid off the defender's pad cannot reach the command post", delegate
            {
                World w = NewMatch();
                EntityHandle cp = Scenario.FindFirst(w, 1, "Command Post");
                Fix2 home = w.Entities.Position[cp.Index];

                Fix nearest = Fix.MaxValue;
                for (int i = 1; i < w.Entities.HighWater; i++)
                {
                    if (!w.Entities.IsSlotAlive(i)) continue;
                    if (w.Entities.Team[i] != 2) continue;
                    if (!w.Entities.Has(i, ComponentMask.Structure)) continue;
                    string name = Catalog.Get(w.Entities.DefId[i]).Name;
                    if (name != "Crew Quarters" && name != "Relay Mast" && name != "Command Post")
                        continue;
                    Fix d = Fix2.Distance(home, w.Entities.Position[i]);
                    if (d < nearest) nearest = d;
                }
                Assert.True(nearest > Fix.FromInt(SimConstants.RadioRangeMetres),
                            "the defence's nearest radio anchor is further from the player's "
                            + "command post than a radio link reaches");
            });
        }
    }
}
