// KILL ZONE - a real-time strategy video game.
// The jamming field: where on the map a radio-controlled drone loses its pilot.
//
// Jamming is stored on a coarse 32 m grid rebuilt a few times a second, because
// jammers do not move fast and a drone does not need to know the field to the
// centimetre. What it does need is for the *edge* of a bubble to be exactly
// where it looks like it is, because skirting the rim of a jammer is a real
// play, so any drone sitting near its own threshold falls back to an exact
// evaluation against the handful of emitters that actually reach that cell.

using System;

namespace KZ.Sim
{
    public struct JamEmitter
    {
        public Fix2 Position;
        public byte Strength;
        public Fix RadiusMetres;
        public byte Team;
        public EntityHandle Source;
    }

    public sealed class SignalGrid
    {
        public const int MaxEmittersPerCell = 4;
        public const int MaxEmitters = 64;

        public readonly int CellsX;
        public readonly int CellsY;
        readonly int cellSize;

        // Per cell: the strongest effective jamming at its centre, and which
        // emitters reach it. Only the maximum matters, so a fifth emitter in one
        // cell can be dropped without changing any outcome.
        readonly byte[] jamCentre;
        readonly byte[] emitterCount;
        readonly byte[] emitterIndex; // CellsX*CellsY*MaxEmittersPerCell

        readonly JamEmitter[] emitters = new JamEmitter[MaxEmitters];
        int emitterTotal;

        public int EmitterCount { get { return emitterTotal; } }
        public JamEmitter GetEmitter(int i) { return emitters[i]; }

        public SignalGrid(int mapWidthMetres, int mapHeightMetres)
        {
            cellSize = SimConstants.SignalCellMetres;
            CellsX = (mapWidthMetres + cellSize - 1) / cellSize;
            CellsY = (mapHeightMetres + cellSize - 1) / cellSize;
            jamCentre = new byte[CellsX * CellsY];
            emitterCount = new byte[CellsX * CellsY];
            emitterIndex = new byte[CellsX * CellsY * MaxEmittersPerCell];
        }

        public int CellOf(Fix2 pos)
        {
            int cx = pos.X.FloorToInt() / cellSize;
            int cy = pos.Y.FloorToInt() / cellSize;
            if (cx < 0) cx = 0; else if (cx >= CellsX) cx = CellsX - 1;
            if (cy < 0) cy = 0; else if (cy >= CellsY) cy = CellsY - 1;
            return cy * CellsX + cx;
        }

        public byte JamAtCell(int cell) { return jamCentre[cell]; }

        public void ClearEmitters() { emitterTotal = 0; }

        public bool AddEmitter(JamEmitter e)
        {
            if (emitterTotal >= MaxEmitters) return false;
            emitters[emitterTotal++] = e;
            return true;
        }

        /// <summary>
        /// Effective jamming strength at a distance from one emitter. Falls off
        /// linearly and is scaled by 1.3, so a bubble bites hardest at the centre
        /// and frays at the rim rather than ending at a hard wall.
        /// </summary>
        public static int EffectiveJam(byte strength, Fix radius, Fix distance)
        {
            if (distance >= radius) return 0;
            Fix falloff = Fix.One - (distance / radius);
            Fix eff = Fix.FromInt(strength) * falloff * SimConstants.JamFalloffScale;
            int v = eff.RoundToInt();
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }

        /// <summary>
        /// Rasterise every emitter onto the grid. Overlapping bubbles take the
        /// strongest single value rather than summing: two weak jammers must not
        /// quietly add up to a strong one, because nothing on screen would show it
        /// and the player would learn a rule the game never told them.
        /// </summary>
        public void Rebuild()
        {
            Array.Clear(jamCentre, 0, jamCentre.Length);
            Array.Clear(emitterCount, 0, emitterCount.Length);

            for (int e = 0; e < emitterTotal; e++)
            {
                JamEmitter em = emitters[e];
                if (em.Strength == 0 || em.RadiusMetres.Raw <= 0) continue;

                int radiusCells = (em.RadiusMetres.FloorToInt() / cellSize) + 1;
                int ecx = em.Position.X.FloorToInt() / cellSize;
                int ecy = em.Position.Y.FloorToInt() / cellSize;

                int minX = ecx - radiusCells; if (minX < 0) minX = 0;
                int maxX = ecx + radiusCells; if (maxX >= CellsX) maxX = CellsX - 1;
                int minY = ecy - radiusCells; if (minY < 0) minY = 0;
                int maxY = ecy + radiusCells; if (maxY >= CellsY) maxY = CellsY - 1;

                for (int cy = minY; cy <= maxY; cy++)
                {
                    for (int cx = minX; cx <= maxX; cx++)
                    {
                        Fix2 centre = CellCentre(cx, cy);
                        Fix d = Fix2.Distance(centre, em.Position);
                        int eff = EffectiveJam(em.Strength, em.RadiusMetres, d);
                        if (eff == 0) continue;

                        int cell = cy * CellsX + cx;
                        if (eff > jamCentre[cell]) jamCentre[cell] = (byte)eff;

                        int n = emitterCount[cell];
                        if (n < MaxEmittersPerCell)
                        {
                            emitterIndex[cell * MaxEmittersPerCell + n] = (byte)e;
                            emitterCount[cell] = (byte)(n + 1);
                        }
                    }
                }
            }
        }

        public Fix2 CellCentre(int cx, int cy)
        {
            int half = cellSize / 2;
            return new Fix2(Fix.FromInt(cx * cellSize + half), Fix.FromInt(cy * cellSize + half));
        }

        /// <summary>
        /// The jamming a drone at this position actually experiences.
        ///
        /// The coarse cell value is used when it is comfortably clear of the
        /// drone's own robustness. Near the threshold it is not trusted - across
        /// one 32 m cell a strong jammer's effective strength changes by several
        /// points, which would make a drone on the edge flicker between having a
        /// pilot and not having one. There, the exact distance to each emitter
        /// reaching this cell is evaluated instead.
        /// </summary>
        public int SampleFor(Fix2 pos, int robustness)
        {
            int cell = CellOf(pos);
            int j = jamCentre[cell];

            int diff = j - robustness;
            if (diff < 0) diff = -diff;
            if (diff >= SimConstants.JamBoundaryRefineMargin) return j;

            int exact = 0;
            int n = emitterCount[cell];
            for (int i = 0; i < n; i++)
            {
                JamEmitter em = emitters[emitterIndex[cell * MaxEmittersPerCell + i]];
                Fix d = Fix2.Distance(pos, em.Position);
                int eff = EffectiveJam(em.Strength, em.RadiusMetres, d);
                if (eff > exact) exact = eff;
            }
            return exact;
        }
    }
}
