// KILL ZONE - a real-time strategy video game.
// Q31.32 fixed-point arithmetic for the deterministic game simulation.
//
// Every number the simulation touches is one of these. The reason is cross-play:
// an iPad (ARM64) and a desktop PC (x86-64) must agree bit-for-bit on the state
// of a match for tens of thousands of ticks. IEEE-754 floats do not survive that
// across different compilers and CPUs - fused multiply-add, reassociation and
// libm differences each cost you a low bit, and one low bit fed back through a
// steering loop is a desynced match ten minutes later. A 64-bit integer multiply
// is exactly specified on every CPU ever shipped.

using System;

namespace KZ.Sim
{
    public readonly struct Fix : IEquatable<Fix>, IComparable<Fix>
    {
        public const int FracBits = 32;
        public const long OneRaw = 1L << FracBits;
        public const long HalfRaw = 1L << (FracBits - 1);

        public readonly long Raw;

        public Fix(long raw) { Raw = raw; }

        public static readonly Fix Zero = new Fix(0);
        public static readonly Fix One = new Fix(OneRaw);
        public static readonly Fix Half = new Fix(HalfRaw);
        public static readonly Fix MaxValue = new Fix(long.MaxValue);
        public static readonly Fix MinValue = new Fix(long.MinValue);

        // Pi to the precision Q31.32 can hold.
        public static readonly Fix Pi = new Fix(13493037705L);
        public static readonly Fix TwoPi = new Fix(26986075409L);
        public static readonly Fix PiOverTwo = new Fix(6746518852L);

        // ---- construction -------------------------------------------------

        public static Fix FromInt(int v) { return new Fix((long)v << FracBits); }

        /// <summary>numerator/denominator, evaluated in fixed point. Deterministic.</summary>
        public static Fix Ratio(int numerator, int denominator)
        {
            return new Fix(DivRaw((long)numerator << FracBits, (long)denominator << FracBits));
        }

        /// <summary>
        /// Parse from a double. Only legal at content-load and test-authoring time,
        /// never inside the simulation loop - the whole point of this type is that
        /// the sim never touches a float.
        /// </summary>
        public static Fix FromDoubleContentOnly(double v)
        {
            return new Fix((long)System.Math.Round(v * OneRaw));
        }

        public double ToDoubleForDisplay() { return Raw / (double)OneRaw; }
        public int ToInt() { return (int)(Raw >> FracBits); }
        public int RoundToInt() { return (int)((Raw + HalfRaw) >> FracBits); }
        public int FloorToInt() { return (int)(Raw >> FracBits); }

        // ---- operators ----------------------------------------------------

        public static Fix operator +(Fix a, Fix b) { return new Fix(a.Raw + b.Raw); }
        public static Fix operator -(Fix a, Fix b) { return new Fix(a.Raw - b.Raw); }
        public static Fix operator -(Fix a) { return new Fix(-a.Raw); }
        public static Fix operator *(Fix a, Fix b) { return new Fix(MulRaw(a.Raw, b.Raw)); }
        public static Fix operator /(Fix a, Fix b) { return new Fix(DivRaw(a.Raw, b.Raw)); }
        public static Fix operator *(Fix a, int b) { return new Fix(a.Raw * b); }
        public static Fix operator /(Fix a, int b) { return new Fix(a.Raw / b); }

        public static bool operator <(Fix a, Fix b) { return a.Raw < b.Raw; }
        public static bool operator >(Fix a, Fix b) { return a.Raw > b.Raw; }
        public static bool operator <=(Fix a, Fix b) { return a.Raw <= b.Raw; }
        public static bool operator >=(Fix a, Fix b) { return a.Raw >= b.Raw; }
        public static bool operator ==(Fix a, Fix b) { return a.Raw == b.Raw; }
        public static bool operator !=(Fix a, Fix b) { return a.Raw != b.Raw; }

        public bool Equals(Fix other) { return Raw == other.Raw; }
        public override bool Equals(object o) { return o is Fix && ((Fix)o).Raw == Raw; }
        public override int GetHashCode() { return Raw.GetHashCode(); }
        public int CompareTo(Fix other) { return Raw.CompareTo(other.Raw); }
        public override string ToString() { return ToDoubleForDisplay().ToString("0.######"); }

        // ---- core arithmetic ----------------------------------------------

        /// <summary>
        /// (a*b) >> 32, computed through the 128-bit intermediate product using
        /// 32x32 partial products. Exact and identical on every architecture.
        /// </summary>
        public static long MulRaw(long a, long b)
        {
            ulong alo = (ulong)(a & 0xFFFFFFFFL);
            long ahi = a >> FracBits;
            ulong blo = (ulong)(b & 0xFFFFFFFFL);
            long bhi = b >> FracBits;

            ulong lolo = alo * blo;
            long lohi = (long)alo * bhi;
            long hilo = ahi * (long)blo;
            long hihi = ahi * bhi;

            return (long)(lolo >> FracBits) + lohi + hilo + (hihi << FracBits);
        }

        /// <summary>Restoring long division. Saturates rather than wrapping on overflow.</summary>
        public static long DivRaw(long xl, long yl)
        {
            if (yl == 0) throw new DivideByZeroException("Fix divide by zero");

            ulong remainder = (ulong)(xl >= 0 ? xl : -xl);
            ulong divider = (ulong)(yl >= 0 ? yl : -yl);
            ulong quotient = 0UL;
            int bitPos = 64 / 2 + 1;

            // Strip trailing zero nibbles from the divisor; cheap and exact.
            while ((divider & 0xF) == 0 && bitPos >= 4)
            {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0)
            {
                int shift = CountLeadingZeroes(remainder);
                if (shift > bitPos) shift = bitPos;
                remainder <<= shift;
                bitPos -= shift;

                ulong div = remainder / divider;
                remainder = remainder % divider;
                quotient += div << bitPos;

                if (bitPos < 64 && (div & ~(0xFFFFFFFFFFFFFFFFUL >> bitPos)) != 0)
                    return ((xl ^ yl) & long.MinValue) == 0 ? long.MaxValue : long.MinValue;

                remainder <<= 1;
                --bitPos;
            }

            ++quotient;
            long result = (long)(quotient >> 1);
            if (((xl ^ yl) & long.MinValue) != 0) result = -result;
            return result;
        }

        static int CountLeadingZeroes(ulong x)
        {
            int result = 0;
            while ((x & 0xF000000000000000UL) == 0) { result += 4; x <<= 4; }
            while ((x & 0x8000000000000000UL) == 0) { result += 1; x <<= 1; }
            return result;
        }

        // ---- helpers ------------------------------------------------------

        public static Fix Abs(Fix a) { return new Fix(a.Raw < 0 ? -a.Raw : a.Raw); }
        public static Fix Min(Fix a, Fix b) { return a.Raw < b.Raw ? a : b; }
        public static Fix Max(Fix a, Fix b) { return a.Raw > b.Raw ? a : b; }

        public static Fix Clamp(Fix v, Fix lo, Fix hi)
        {
            if (v.Raw < lo.Raw) return lo;
            if (v.Raw > hi.Raw) return hi;
            return v;
        }

        public static Fix Lerp(Fix a, Fix b, Fix t) { return a + (b - a) * t; }

        public int Sign() { return Raw > 0 ? 1 : (Raw < 0 ? -1 : 0); }

        /// <summary>
        /// Digit-by-digit integer square root. Exact to the last representable bit,
        /// with no iteration count that could vary by platform.
        /// </summary>
        public static Fix Sqrt(Fix a)
        {
            long xl = a.Raw;
            if (xl < 0) throw new ArgumentOutOfRangeException("a", "Sqrt of a negative Fix");
            if (xl == 0) return Zero;

            ulong num = (ulong)xl;
            ulong result = 0UL;
            ulong bit = 1UL << 62;

            while (bit > num) bit >>= 2;

            for (int i = 0; i < 2; ++i)
            {
                while (bit != 0)
                {
                    if (num >= result + bit)
                    {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    }
                    else
                    {
                        result = result >> 1;
                    }
                    bit >>= 2;
                }

                if (i == 0)
                {
                    if (num > (1UL << 32) - 1)
                    {
                        num -= result;
                        num = (num << 32) - 0x80000000UL;
                        result = (result << 32) + 0x80000000UL;
                    }
                    else
                    {
                        num <<= 32;
                        result <<= 32;
                    }
                    bit = 1UL << 30;
                }
            }

            return new Fix((long)result);
        }
    }
}
