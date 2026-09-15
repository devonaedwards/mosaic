// KILL ZONE - a real-time strategy video game.
// Player orders, and the queue they arrive through.
//
// Everything that happens in this game happens because a command was executed:
// the player tapping a target, the computer opponent deciding to attack, a
// campaign script firing a trigger. All three produce the same structs and go
// through the same door.
//
// That is not tidiness for its own sake. It is what makes a replay a few
// kilobytes instead of a video - a match is fully described by its starting
// seed and its list of commands. It is what lets a thousand matches be played
// headless overnight to test a balance change. And it is what makes networked
// play possible at all: the two machines exchange these, and nothing else.

using System.Collections.Generic;

namespace KZ.Sim
{
    public enum CommandKind : byte
    {
        None = 0,
        MoveTo,
        Attack,
        Stop,
        LaunchSortie,
        DropPin,
        PlaceDecoy,
        LayMines,
        SetAltitude,
        SetAutonomyBox,
        FitThermalBlanket,
        FitCage
    }

    public struct Command
    {
        public CommandKind Kind;
        public byte Team;
        public EntityHandle Subject;   // the unit being ordered
        public EntityHandle Target;
        public Fix2 Point;
        public int DefId;              // for a launch: which airframe
        public int Param;              // launch index, decoy lifetime, and so on
        // The second point of a two-point order: the far end of a mine-laying
        // run, or the opposite corner of an autonomy no-go box.
        public Fix2 MineEnd;

        public static Command MoveTo(byte team, EntityHandle subject, Fix2 point)
        {
            Command c = new Command();
            c.Kind = CommandKind.MoveTo; c.Team = team; c.Subject = subject; c.Point = point;
            return c;
        }

        public static Command Attack(byte team, EntityHandle subject, EntityHandle target)
        {
            Command c = new Command();
            c.Kind = CommandKind.Attack; c.Team = team; c.Subject = subject; c.Target = target;
            return c;
        }

        public static Command Stop(byte team, EntityHandle subject)
        {
            Command c = new Command();
            c.Kind = CommandKind.Stop; c.Team = team; c.Subject = subject;
            return c;
        }

        /// <summary>
        /// Put one airframe in the air. Note what this does *not* carry: the
        /// identity of the crew. Which crew flies is decided inside the simulation,
        /// by a rule both machines run identically, because if the two players'
        /// machines picked different crews the match would diverge immediately.
        /// </summary>
        public static Command LaunchSortie(byte team, int defId, Fix2 pad, EntityHandle target, int launchIndex)
        {
            Command c = new Command();
            c.Kind = CommandKind.LaunchSortie; c.Team = team; c.DefId = defId;
            c.Point = pad; c.Target = target; c.Param = launchIndex;
            return c;
        }

        /// <summary>Order a heavy drone to lay its mines along a line.</summary>
        public static Command LayMines(byte team, EntityHandle bomber, Fix2 from, Fix2 to)
        {
            Command c = new Command();
            c.Kind = CommandKind.LayMines; c.Team = team; c.Subject = bomber;
            c.Point = from; c.Target = EntityHandle.None;
            c.MineEnd = to;
            return c;
        }

        /// <summary>
        /// Climb or descend. Height is cover from anything that shoots upward and
        /// from anything that listens, and it is exposure to anything with a radar.
        /// Coming in low and going in high are different attacks against the same
        /// position, and sending both at once is a third.
        /// </summary>
        public static Command SetAltitude(byte team, EntityHandle subject, Layer layer)
        {
            Command c = new Command();
            c.Kind = CommandKind.SetAltitude; c.Team = team; c.Subject = subject;
            c.Param = (int)layer;
            return c;
        }

        public static Command PlaceDecoy(byte team, Fix2 point, int lifetimeTicks)
        {
            Command c = new Command();
            c.Kind = CommandKind.PlaceDecoy; c.Team = team; c.Point = point; c.Param = lifetimeTicks;
            return c;
        }

        /// <summary>
        /// Draw a no-go bubble over your own ground: a rectangle your autonomous
        /// munitions will not pick a target out of, whoever is standing in it.
        ///
        /// autonomy.md §9.3 asks for exactly this, and asks for it as a thing the
        /// player builds rather than a difficulty setting, because a no-go bubble
        /// is what brigades actually improvise - §5 notes that every mitigation
        /// in use against autonomous misidentification is procedural.
        ///
        /// Note the two things it is not. It is not per-airframe: the order is
        /// the team's and every autonomous unit it owns, in the air now or
        /// launched later, obeys it. And it is not a friend filter - an enemy
        /// who parks inside your bubble is as safe from your munitions as your
        /// own tanks are, which is the price of drawing one and the reason to
        /// draw it tightly.
        ///
        /// lifetimeTicks is the player's, as a decoy's is, and it is not
        /// optional: see AutonomyState.BoxExpiryTick for why a bubble lapses.
        /// Zero or less erases the current one.
        /// </summary>
        public static Command SetAutonomyBox(byte team, Fix2 corner, Fix2 opposite, int lifetimeTicks)
        {
            Command c = new Command();
            c.Kind = CommandKind.SetAutonomyBox; c.Team = team;
            c.Point = corner; c.MineEnd = opposite; c.Param = lifetimeTicks;
            return c;
        }

        /// <summary>
        /// Fit a thermal blanket to one of your vehicles. AUDIT-UNWIRED.md F16:
        /// the blanket's consumers - World.EffectiveSignature and the
        /// classifier's plausibility term - have worked all along, and until this
        /// order existed nothing but a test could ever put one on a vehicle.
        /// </summary>
        public static Command FitThermalBlanket(byte team, EntityHandle subject)
        {
            Command c = new Command();
            c.Kind = CommandKind.FitThermalBlanket; c.Team = team; c.Subject = subject;
            return c;
        }

        /// <summary>
        /// Fit a cage or slat screen. savePercent is the chance a shaped-charge
        /// hit on the roof is disrupted rather than let through - a *roll*, not
        /// a pool of extra hit points, because ground-force.md §2.1 is explicit
        /// that a cage disrupts the jet rather than absorbing it, and §8.2 that
        /// a cage which only adds standoff can raise penetration instead of
        /// lowering it. World.ApplyDamage carries both branches.
        ///
        /// The research gives a spread of 0.30 (coarse, poorly placed) to 0.80
        /// (fine, disruption-dominated) from one source of unclear provenance
        /// and no single figure, so **whatever a caller passes here is a
        /// designer estimate** and should say so at the call site. It rides as
        /// an integer percent because a command is a replay record first: the
        /// same few bytes have to mean the same thing on both machines.
        /// </summary>
        public static Command FitCage(byte team, EntityHandle subject, int savePercent)
        {
            Command c = new Command();
            c.Kind = CommandKind.FitCage; c.Team = team; c.Subject = subject; c.Param = savePercent;
            return c;
        }
    }

    public sealed class CommandBuffer
    {
        readonly List<Command> pending = new List<Command>();

        public int Count { get { return pending.Count; } }
        public Command this[int i] { get { return pending[i]; } }

        public void Enqueue(Command c) { pending.Add(c); }
        public void Clear() { pending.Clear(); }

        /// <summary>
        /// Execute every queued command, in the order they arrived.
        ///
        /// Order matters and is preserved exactly. Two commands issued in the same
        /// tick - say, two players both launching their last available sortie -
        /// must resolve the same way on every machine, and the only way to
        /// guarantee that is to run them in a fixed sequence rather than in
        /// whatever order a collection happens to iterate.
        /// </summary>
        public void Execute(World w)
        {
            for (int i = 0; i < pending.Count; i++) Apply(w, pending[i]);
            pending.Clear();
        }

        static void Apply(World w, Command c)
        {
            switch (c.Kind)
            {
                case CommandKind.MoveTo:
                    MovementSystem.OrderMoveTo(w, c.Subject, c.Point);
                    break;

                case CommandKind.Attack:
                    MovementSystem.OrderAttack(w, c.Subject, c.Target);
                    break;

                case CommandKind.Stop:
                    if (w.Entities.IsAlive(c.Subject)
                        && w.Entities.Has(c.Subject.Index, ComponentMask.Mover))
                    {
                        w.Entities.Mover[c.Subject.Index].HasOrder = false;
                        w.Entities.Mover[c.Subject.Index].OrderTarget = EntityHandle.None;
                    }
                    break;

                case CommandKind.LaunchSortie:
                {
                    EntityHandle spawned;
                    LaunchResult res = SortieSystem.Launch(w, c.Team, c.DefId, c.Point,
                                                           c.Target, c.Param, out spawned);
                    if (res == LaunchResult.Launched && !w.Entities.IsAlive(c.Target))
                        MovementSystem.OrderMoveTo(w, spawned, c.Point);
                    break;
                }

                case CommandKind.LayMines:
                    SortieSystem.LayMines(w, c.Subject, c.Point, c.MineEnd);
                    break;

                case CommandKind.SetAltitude:
                {
                    if (!w.Entities.IsAlive(c.Subject)) break;
                    int si = c.Subject.Index;
                    int defId = w.Entities.DefId[si];
                    if (defId < 0 || !Catalog.Get(defId).CanChangeAltitude) break;
                    if (w.Entities.EntityLayer[si] == Layer.Ground) break;
                    w.Entities.EntityLayer[si] = (Layer)c.Param;
                    break;
                }

                case CommandKind.PlaceDecoy:
                    w.SpawnDecoy(c.Team, c.Point, TargetKind.HighValue, c.Param);
                    break;

                case CommandKind.SetAutonomyBox:
                    w.SetAutonomyNoGoBox(c.Team, c.Point, c.MineEnd, c.Param);
                    break;

                // An upgrade is fitted to something you own. Every other case
                // above takes the subject on trust, which is fine while a
                // subject is something you are ordering about - a MoveTo aimed
                // at an enemy tank does nothing, because the tank has no order
                // to follow. These two would work, so they check.
                case CommandKind.FitThermalBlanket:
                    if (w.Entities.IsAlive(c.Subject) && w.Entities.Team[c.Subject.Index] == c.Team)
                        w.FitThermalBlanket(c.Subject);
                    break;

                case CommandKind.FitCage:
                    if (w.Entities.IsAlive(c.Subject) && w.Entities.Team[c.Subject.Index] == c.Team)
                        w.FitCage(c.Subject, Fix.FromInt(c.Param) / 100);
                    break;
            }
        }
    }
}
