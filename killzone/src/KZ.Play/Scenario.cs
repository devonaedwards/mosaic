// KILL ZONE - a real-time strategy video game.
// The one playable scenario, and the opposition's standing orders.
//
// This is the same shape as src/KZ.Headless/Program.cs's BuildMap - the same
// ground, the same border, the same reason for each placement - with two
// differences. The player's side is not scripted, because a person is flying it
// now. And the defender has standing orders rather than a fixed script, so that
// there is something on the other side of the map reacting to what the player
// does rather than a target range.
//
// Those orders are deliberately not an AI. They go through w.Enqueue like every
// other order in the game, they are a pure function of the world at a tick, and
// they draw no randomness of their own - so a match is still fully described by
// its seed and the player's command list, which is what makes it replayable.
// docs/SECTOR-COMMAND.md argues for doctrine over learning for exactly this
// reason and this is the smallest possible instance of it.

using KZ.Sim;

namespace KZ.Play
{
    public static class Scenario
    {
        static Fix F(double v) { return Fix.FromDoubleContentOnly(v); }
        static Fix2 P(double x, double y) { return new Fix2(F(x), F(y)); }

        /// <summary>The player's team. The defender is team 2.</summary>
        public const byte PlayerTeam = 1;

        /// <summary>
        /// Real metres. Mirrors KZ.Headless and src/KZ.Balance's
        /// StandardBorderMetres: the attacker's infrastructure sits well west of
        /// it and everything worth hitting is east, so a sortie that goes deep is
        /// a sortie that spends most of its flight with no satellite over it.
        /// </summary>
        public const int BorderMetres = 15600;
        const int NeutralMetres = 768;

        /// <summary>
        /// 28.8 x 19.2 real kilometres, which is the same ground the catalogue was
        /// always describing - see docs/SCALE.md's correction. The renderer never
        /// sees this number; it reads Terrain.WidthMetres and fits a viewport to
        /// it, which is the whole point of compression being a view parameter.
        /// </summary>
        public const int MapWidthMetres = 28800;
        public const int MapHeightMetres = 19200;

        /// <summary>
        /// What the player can put in the air. A subset of the catalogue rather
        /// than all of it, because a hangar bar is a row of cards and the
        /// interface spec's one-second rule does not survive forty of them.
        /// </summary>
        public static readonly string[] Hangar =
        {
            "Scout Quad",
            "FPV Team",
            "Fiber FPV Team",
            "Multirole Quad",
            "Interceptor FPV",
            "Recon Wing",
            "Heavy Strike Drone"
        };

        public static World Build(ulong seed, int startTick)
        {
            Terrain t = new Terrain(MapWidthMetres, MapHeightMetres);
            t.Fill(TileClass.Open);

            // The ground is laid out to make one question worth asking: where a
            // fiber thread can be dragged and where it cannot. Terrain.SnagRate
            // PerSecond is free over open ground, 0.015 on a road, 0.040 in
            // forest and 0.090 under power lines, so these rectangles are not
            // scenery - they are the map's only statement about which rung of
            // the link ladder reaches which target.
            //
            // Tile indices; a build tile is 96 real metres.

            // Two treelines, north and south, boxing in a clean lane across the
            // middle. Sightlines collapse inside them and threads part.
            t.FillRect(118, 26, 176, 60, TileClass.Forest);
            t.FillRect(150, 132, 205, 168, TileClass.Forest);

            // The lateral supply road, just south of the lane. The defender's
            // truck sits on it, which is what roads are for and why they are
            // watched.
            t.FillRect(0, 106, 299, 108, TileClass.Road);

            // The curtain: a power line running the full height of the map in
            // front of the defender's rear area. Everything shallow of it is
            // fiber country. Nothing deep of it is, which is the whole reason
            // the deep target needs a different answer rather than more of the
            // same one.
            t.FillRect(206, 0, 209, 199, TileClass.PowerLine);
            // A second, shorter line on the player's own side, south only, so
            // that the clean lane is a lane rather than the whole west.
            t.FillRect(128, 112, 130, 199, TileClass.PowerLine);

            // Rubble around the defender's command post. Short everything,
            // matchable ground, acoustically hostile.
            t.FillRect(252, 84, 276, 104, TileClass.Rubble);

            World w = new World(t, 1024, 128, seed, 2, startTick);

            w.Player(1).Faction = FactionId.KestrelPact;
            w.Player(2).Faction = FactionId.ObsidianDirectorate;
            w.Player(1).Materiel = Fix.FromInt(12000);
            w.Player(2).Materiel = Fix.FromInt(12000);

            // The border a satellite link and scene matching both read. Without
            // this every satellite link is permanently black - AUDIT-UNWIRED F6.
            w.Territory.SetVerticalBorder(BorderMetres, 1, 2, NeutralMetres);
            GrantHomeImagery(w, 1, true, BorderMetres - NeutralMetres);
            GrantHomeImagery(w, 2, false, BorderMetres + NeutralMetres);

            // The player's side. Everything here is either a launch pad, a pair
            // of eyes, or crews. Nothing of the player's shoots.
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(3600, 9360));
            w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(4560, 10320));
            w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(3600, 10800));
            w.Spawn(Catalog.IdOf("Drone Workshop"), 1, P(4560, 8400));
            w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(5400, 9360));
            // Radio control reaches 12 km from a mast, so this one is what puts
            // the defender's forward positions inside a live radio link and
            // leaves their rear outside it.
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(9360, 9360));
            // Two things the player can drive. Pushing eyes forward is the only
            // way to see anything, and it is done with the mouse.
            w.Spawn(Catalog.IdOf("Designator Team"), 1, P(11000, 8600));
            w.Spawn(Catalog.IdOf("Motorcycle Squad"), 1, P(11000, 10800));

            // The defence, in three belts. The jammer and the tank sit shallow,
            // inside fiber's reach and inside the relay's radio range. The
            // command post sits deep, behind the power line and outside both.
            w.Spawn(Catalog.IdOf("EW Post"), 2, P(18000, 9200));
            w.Spawn(Catalog.IdOf("Main Tank"), 2, P(16800, 9600));
            w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(19000, 8400));
            w.Spawn(Catalog.IdOf("IFV"), 2, P(20600, 11000));
            w.Spawn(Catalog.IdOf("Supply Truck"), 2, P(22800, 10300));
            w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(24600, 9360));
            w.Spawn(Catalog.IdOf("Command Post"), 2, P(25200, 9360));

            return w;
        }

        /// <summary>
        /// Grants a team imagery over its own side of the border, cell by cell.
        /// Same shape as KZ.Headless's and KZ.Balance's, reproduced rather than
        /// shared because the console apps do not reference each other.
        /// </summary>
        static void GrantHomeImagery(World w, byte team, bool west, int borderMetres)
        {
            int borderCell = borderMetres / ReferenceImagery.CellMetres;
            for (int cy = 0; cy < w.Imagery.CellsY; cy++)
                for (int cx = 0; cx < w.Imagery.CellsX; cx++)
                    if (west ? cx <= borderCell : cx >= borderCell)
                        w.Imagery.Grant(team, cx, cy);
        }

        /// <summary>
        /// The defender's opening housekeeping, and then its standing orders.
        /// Called once per tick before Step, exactly where KZ.Headless calls its
        /// Script.
        /// </summary>
        public static void DefenderOrders(World w, int tick)
        {
            if (tick == 0)
            {
                EntityHandle tank = FindFirst(w, 2, "Main Tank");
                if (w.Entities.IsAlive(tank))
                    // 55%: ground-force.md §2.1 gives a 0.30-0.80 spread for a
                    // cage's disruption chance and no single figure, so the
                    // midpoint is a designer estimate, not a sourced number.
                    w.Enqueue(Command.FitCage(2, tank, 55));

                EntityHandle truck = FindFirst(w, 2, "Supply Truck");
                if (w.Entities.IsAlive(truck))
                    w.Enqueue(Command.FitThermalBlanket(2, truck));
            }

            // The defender buys its own information, and this is the order that
            // turns it from a target range into an opponent.
            //
            // The raid below was already here and already correct, and it fired
            // exactly never, for two reasons that had nothing to do with it.
            //
            // The first is one character wide and is fixed below: it passed `tick`
            // where SortieSystem.Launch wants a *launch index* within a flight, and
            // EgressUntilTick is SortiePadEgressBaseTicks + index x 4. At forty
            // play-seconds in, that is a five-thousand-tick hold on the pad, and it
            // grows for the rest of the match. Every airframe the defender ever
            // launched sat on its pad until the match ended. Nothing was ever
            // inbound, which is the whole of FINDINGS 35's "you cannot intercept the
            // FPVs the defender sends": there were none to intercept.
            //
            // The second is that the raid is gated on IsDetectedBy, and the
            // defender's forward-most eye is a tank with 4.3 km of optics inside a
            // 90 degree arc, sitting 5.8 km from the nearest thing the player owns.
            // Left passive it sees nothing of the player's at all - measured, over
            // three hundred play-seconds - so it has nothing to launch at either.
            //
            // The answer to that second one is not to let the defender read the
            // entity table. It is to let it do what the player does: put something
            // up and go and look. A Recon Wing at High with 10.8 km of optics
            // sweeping west finds the player's rear, and everything downstream of it
            // already worked. It also hands the player the best decision in the
            // game running the other way - shoot the scout down and the raids stop -
            // which is the same "push eyes forward, watch four kilometres of the
            // enemy rear appear" moment FINDINGS 35 called the most game-like thing
            // in the whole build, now available to both sides.
            if (tick > 0 && tick % SimConstants.PlaySeconds(45) == 0
                && CountAloft(w, 2, "Recon Wing") == 0)
            {
                w.Enqueue(Command.LaunchSortie(2, Catalog.IdOf("Recon Wing"),
                                               P(24000, 9360), EntityHandle.None, 0));
            }

            // A sortie with nothing to do gets told where to go. LaunchSortie can
            // only be given a target handle, and the reconnaissance above has none
            // by definition, so its route is issued here on the tick after it
            // appears - a pure function of where it is, so two machines give the
            // same aircraft the same order.
            VectorIdleScouts(w, tick);

            // Every four play-seconds the defender looks at what it can actually
            // see and throws one cheap airframe at the nearest of it. This is the
            // whole opposition, and it is written against IsDetectedBy rather than
            // against the entity table on purpose: the defender plays the same fog
            // the player does, so a player who keeps their sensors back is not
            // shot at, and a player who pushes a designator forward has bought
            // their information with a target.
            //
            // The launch index rotates 0-3 rather than carrying the tick: it is
            // spacing within a flight, worth at most a dozen ticks, and it is not a
            // clock.
            if (tick > 0 && tick % SimConstants.PlaySeconds(4) == 0)
            {
                EntityHandle prey = NearestSeen(w, 2, P(25200, 9360));
                if (!prey.IsNone)
                    w.Enqueue(Command.LaunchSortie(2, Catalog.IdOf("FPV Team"),
                                                   P(24000, 9360), prey,
                                                   (tick / SimConstants.PlaySeconds(4)) % 4));
            }

            // And every hundred play-seconds, the thing the interceptor exists for.
            //
            // A Jet Strike Drone is 140 m/s against an interceptor's 85, carries no
            // crew, and flies on autonomy, so it is neither jammable nor stoppable by
            // chasing it - point-defence.md §5 is that the threat moved to 500-600
            // km/h while the propeller interceptor stayed at 300. It is aimed at the
            // player's radar mast on purpose. That mast is the only thing on the map
            // that produces a radar track, a radar track is what lets an interceptor
            // be vectored onto a meeting point rather than pointed at a contact, and
            // so losing it is what makes the *next* one unstoppable. ResolveIntercep
            // tion's own doc comment has claimed for months that "killing the radar
            // is how you open the sky"; until the vector existed, that sentence was
            // about nothing.
            if (tick > 0 && tick % SimConstants.PlaySeconds(100) == 0)
            {
                EntityHandle mast = FindFirst(w, 1, "Radar Mast");
                if (mast.IsNone) mast = FindFirst(w, 1, "Command Post");
                if (!mast.IsNone)
                    w.Enqueue(Command.LaunchSortie(2, Catalog.IdOf("Jet Strike Drone"),
                                                   P(26400, 11400), mast, 0));
            }

            // The tank walks its patrol between two points rather than standing
            // still, because a stationary target teaches the player nothing about
            // whether a strike arrives where it was aimed.
            int leg = SimConstants.PlaySeconds(50);
            if (tick % leg == 0)
            {
                EntityHandle tank = FindFirst(w, 2, "Main Tank");
                if (w.Entities.IsAlive(tank))
                {
                    bool north = (tick / leg) % 2 == 0;
                    w.Enqueue(Command.MoveTo(2, tank, north ? P(16800, 7600) : P(16800, 11600)));
                }
            }
        }

        /// <summary>
        /// The defender's reconnaissance leg, and the whole of its navigation.
        ///
        /// West as far as the player's launch area, then home, where it lands and
        /// gives its crew back so the next one can go. The turn is decided by where
        /// the aircraft is rather than by a stored waypoint, so there is no state
        /// here to hash and no way for two machines to disagree about which leg an
        /// aircraft is flying.
        ///
        /// It is deliberately a straight line down the lane and not an evasive
        /// route. A scout that cannot be caught is not a decision, and this one is
        /// meant to be shot down.
        /// </summary>
        static void VectorIdleScouts(World w, int tick)
        {
            int reconId = Catalog.IdOf("Recon Wing");
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (w.Entities.Team[i] != 2) continue;
                if (w.Entities.DefId[i] != reconId) continue;
                if (!w.Entities.Has(i, ComponentMask.Mover)) continue;
                if (w.Entities.Mover[i].HasOrder) continue;

                bool outbound = w.Entities.Position[i].X > Fix.FromInt(12000);
                w.Enqueue(Command.MoveTo(2, w.Entities.HandleAt(i),
                                         outbound ? P(9000, 9360) : P(24000, 9360)));
            }
        }

        /// <summary>How many of one airframe this team currently has in the air.</summary>
        static int CountAloft(World w, byte team, string defName)
        {
            int defId = Catalog.IdOf(defName);
            int n = 0;
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (w.Entities.Team[i] != team) continue;
                if (w.Entities.DefId[i] == defId) n++;
            }
            return n;
        }

        /// <summary>
        /// The nearest entity of any other team that <paramref name="team"/> can
        /// currently see, by index order so that two machines pick the same one.
        /// </summary>
        static EntityHandle NearestSeen(World w, byte team, Fix2 from)
        {
            EntityHandle best = EntityHandle.None;
            Fix bestSq = Fix.MaxValue;
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (w.Entities.Team[i] == team || w.Entities.Team[i] == 0) continue;
                EntityHandle h = w.Entities.HandleAt(i);
                if (!w.IsDetectedBy(team, h)) continue;
                Fix d = Fix2.SqrDistance(from, w.Entities.Position[i]);
                if (d < bestSq) { bestSq = d; best = h; }
            }
            return best;
        }

        public static EntityHandle FindFirst(World w, byte team, string defName)
        {
            int defId = Catalog.IdOf(defName);
            for (int i = 1; i < w.Entities.HighWater; i++)
            {
                if (!w.Entities.IsSlotAlive(i)) continue;
                if (w.Entities.Team[i] != team) continue;
                if (w.Entities.DefId[i] != defId) continue;
                return w.Entities.HandleAt(i);
            }
            return EntityHandle.None;
        }
    }
}
