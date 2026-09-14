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
        SetAutonomyBox
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
        public Fix2 MineEnd;           // far end of a mine-laying run

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
            }
        }
    }
}
