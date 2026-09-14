// KILL ZONE - a real-time strategy video game. Fictional factions, hit points,
// build timers. Nothing here is operational guidance.
//
// Who owns the ground, for the purposes of the one thing that cares: satellite
// coverage.
//
// The distinction worth getting right is that a satellite constellation is
// licensed by *country*, and a country's border is not the front line. The
// border is a fixed political line drawn before the shooting started. The front
// is where the shooting currently is, and it moves.
//
// Modelling both separately falls out into a mechanic nobody had to design. Push
// an offensive past your own border and you are fighting on ground you hold with
// no satellite link over it, because the constellation does not care who is
// winning - it cares which side of a line on a map you are standing on. The
// deeper the attack, the longer the drones are on their own. That is the correct
// shape and it is the opposite of the intuition that the top rung of the link
// ladder is the one that works everywhere.
//
// Stored as a coarse grid rather than a polygon because the simulation is
// integer-only and a point-in-polygon test in fixed point is a determinism
// hazard for no gain: a border cell is 32 m, and no player is manoeuvring on a
// finer grain than that against a line they cannot see.

namespace KZ.Sim
{
    public sealed class Territory
    {
        public const int CellMetres = 32;

        public readonly int CellsX;
        public readonly int CellsY;

        // Team number per cell. Zero is nobody's - international water, a
        // demilitarised strip, or simply a map that never bothered to say.
        readonly byte[] owner;

        public Territory(int widthMetres, int heightMetres)
        {
            CellsX = (widthMetres + CellMetres - 1) / CellMetres;
            CellsY = (heightMetres + CellMetres - 1) / CellMetres;
            owner = new byte[CellsX * CellsY];
        }

        public byte OwnerAtCell(int cx, int cy)
        {
            if (cx < 0 || cy < 0 || cx >= CellsX || cy >= CellsY) return 0;
            return owner[cy * CellsX + cx];
        }

        public byte OwnerAt(Fix2 p)
        {
            return OwnerAtCell(p.X.FloorToInt() / CellMetres, p.Y.FloorToInt() / CellMetres);
        }

        public void SetCell(int cx, int cy, byte team)
        {
            if (cx < 0 || cy < 0 || cx >= CellsX || cy >= CellsY) return;
            owner[cy * CellsX + cx] = team;
        }

        public void Fill(byte team)
        {
            for (int i = 0; i < owner.Length; i++) owner[i] = team;
        }

        /// <summary>
        /// The common case: one border running north to south, one team west of
        /// it and one east. Everything within <paramref name="neutralMetres"/> of
        /// the line belongs to nobody, which is where a satellite drone finds out
        /// it is about to be on its own.
        /// </summary>
        public void SetVerticalBorder(int borderMetres, byte westTeam, byte eastTeam,
                                      int neutralMetres)
        {
            for (int cx = 0; cx < CellsX; cx++)
            {
                int centre = cx * CellMetres + CellMetres / 2;
                byte team;
                if (centre < borderMetres - neutralMetres) team = westTeam;
                else if (centre > borderMetres + neutralMetres) team = eastTeam;
                else team = 0;

                for (int cy = 0; cy < CellsY; cy++) SetCell(cx, cy, team);
            }
        }

        public ulong StateHash()
        {
            unchecked
            {
                ulong h = 1469598103934665603UL;
                for (int i = 0; i < owner.Length; i++)
                    h = (h ^ owner[i]) * 1099511628211UL;
                return h;
            }
        }
    }
}
