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
            // A treeline across the middle and power lines beside the road, so
            // there is a fast route and a safe route and they are not the same.
            // Tile indices, unchanged from the headless map: the build tile grew
            // with the map when the catalogue went to real metres.
            t.FillRect(120, 60, 190, 90, TileClass.Forest);
            t.FillRect(140, 100, 145, 199, TileClass.PowerLine);
            t.FillRect(0, 95, 299, 98, TileClass.Road);
            t.FillRect(205, 120, 240, 150, TileClass.Forest);
            t.FillRect(250, 40, 275, 70, TileClass.Rubble);

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

            // The player's side. Everything here is either a launch pad, a pair of
            // eyes, or crews.
            w.Spawn(Catalog.IdOf("Command Post"), 1, P(3600, 9360));
            w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(4560, 10320));
            w.Spawn(Catalog.IdOf("Crew Quarters"), 1, P(3600, 10800));
            w.Spawn(Catalog.IdOf("Drone Workshop"), 1, P(4560, 8400));
            w.Spawn(Catalog.IdOf("Radar Mast"), 1, P(5400, 9360));
            w.Spawn(Catalog.IdOf("Relay Mast"), 1, P(9360, 9360));
            // Two things the player can actually drive, so that pushing sensors
            // forward is a decision made with the mouse rather than a setting.
            w.Spawn(Catalog.IdOf("Designator Team"), 1, P(11000, 8400));
            w.Spawn(Catalog.IdOf("Motorcycle Squad"), 1, P(11000, 10800));

            // The defence. Ranged so that the tank and the jammer are inside the
            // player's reach from the forward pad and the command post is not.
            w.Spawn(Catalog.IdOf("Command Post"), 2, P(25200, 9360));
            w.Spawn(Catalog.IdOf("EW Post"), 2, P(18000, 9360));
            w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(24600, 9360));
            w.Spawn(Catalog.IdOf("Gun Mount"), 2, P(19200, 8600));
            w.Spawn(Catalog.IdOf("Main Tank"), 2, P(17400, 9360));
            w.Spawn(Catalog.IdOf("IFV"), 2, P(20400, 11400));
            w.Spawn(Catalog.IdOf("Supply Truck"), 2, P(22800, 10320));

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

            // Every four play-seconds the defender looks at what it can actually
            // see and throws one cheap airframe at the nearest of it. This is the
            // whole opposition, and it is written against IsDetectedBy rather than
            // against the entity table on purpose: the defender plays the same fog
            // the player does, so a player who keeps their sensors back is not
            // shot at, and a player who pushes a designator forward has bought
            // their information with a target.
            if (tick > 0 && tick % SimConstants.PlaySeconds(4) == 0)
            {
                EntityHandle prey = NearestSeen(w, 2, P(25200, 9360));
                if (!prey.IsNone)
                    w.Enqueue(Command.LaunchSortie(2, Catalog.IdOf("FPV Team"),
                                                   P(24000, 9360), prey, tick));
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
                    w.Enqueue(Command.MoveTo(2, tank, north ? P(17400, 6200) : P(17400, 12400)));
                }
            }
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
