// KILL ZONE - a real-time strategy video game.
// Deterministic trigonometry over binary angles.
//
// Angles are "bam16": a ushort where the full turn is 65,536 units. That makes
// wrapping free (it is just integer overflow) and gives 0.0055 degrees of
// resolution, far finer than a player can perceive a drone's heading.
//
// The sine table is built at startup from a fixed-point Taylor series evaluated
// with our own Fix.MulRaw - never from System.Math.Sin - because a table built
// from the host libm could differ in the last bit between an iPhone and a PC,
// and that is exactly the class of difference that desyncs a multiplayer match.

namespace KZ.Sim
{
    public static class Trig
    {
        public const int TableBits = 12;
        public const int TableSize = 1 << TableBits;      // 4096 entries per full turn
        public const int BamPerEntry = 65536 / TableSize; // 16 bam per entry

        static readonly Fix[] SinTable = new Fix[TableSize];

        // Taylor coefficients for sin(x) = x - x^3/3! + x^5/5! - x^7/7! + x^9/9! - x^11/11!
        // expressed as Q31.32 reciprocals of the factorials.
        const long Inv3Fact = 715827883L;   // 1/6
        const long Inv5Fact = 35791394L;    // 1/120
        const long Inv7Fact = 852067L;      // 1/5040
        const long Inv9Fact = 11833L;       // 1/362880
        const long Inv11Fact = 108L;        // 1/39916800

        static Trig()
        {
            // Build the table deterministically. Entry i is sin(2*pi*i/4096).
            for (int i = 0; i < TableSize; i++)
            {
                // angle = TwoPi * i / TableSize, computed in fixed point.
                long angle = Fix.MulRaw(Fix.TwoPi.Raw, Fix.DivRaw((long)i << Fix.FracBits,
                                                                  (long)TableSize << Fix.FracBits));
                SinTable[i] = new Fix(SinFullRange(angle));
            }
        }

        /// <summary>
        /// sin() over any angle, by quadrant folding onto a Taylor series valid on
        /// [0, pi/2]. Used only to build the table; the running game uses lookups.
        /// </summary>
        static long SinFullRange(long angle)
        {
            long twoPi = Fix.TwoPi.Raw;
            long pi = Fix.Pi.Raw;
            long halfPi = Fix.PiOverTwo.Raw;

            // Reduce into [0, 2pi).
            angle %= twoPi;
            if (angle < 0) angle += twoPi;

            bool negate = false;
            if (angle > pi) { angle -= pi; negate = true; }
            if (angle > halfPi) angle = pi - angle;

            long r = SinQuarter(angle);
            return negate ? -r : r;
        }

        /// <summary>Taylor series for sin on [0, pi/2]. Error below 4e-8, well under table lerp error.</summary>
        static long SinQuarter(long x)
        {
            long x2 = Fix.MulRaw(x, x);
            long term = x;
            long sum = term;

            term = Fix.MulRaw(term, x2); sum -= Fix.MulRaw(term, Inv3Fact);
            term = Fix.MulRaw(term, x2); sum += Fix.MulRaw(term, Inv5Fact);
            term = Fix.MulRaw(term, x2); sum -= Fix.MulRaw(term, Inv7Fact);
            term = Fix.MulRaw(term, x2); sum += Fix.MulRaw(term, Inv9Fact);
            term = Fix.MulRaw(term, x2); sum -= Fix.MulRaw(term, Inv11Fact);

            return sum;
        }

        /// <summary>sin of a binary angle, by table lookup with linear interpolation.</summary>
        public static Fix Sin(ushort bam)
        {
            int index = bam >> 4;
            int frac = bam & 15;
            Fix a = SinTable[index];
            if (frac == 0) return a;
            Fix b = SinTable[(index + 1) & (TableSize - 1)];
            // lerp by frac/16
            return a + new Fix(((b.Raw - a.Raw) * frac) >> 4);
        }

        /// <summary>cos is sin shifted a quarter turn.</summary>
        public static Fix Cos(ushort bam) { return Sin((ushort)(bam + 16384)); }

        /// <summary>Unit direction vector for a heading.</summary>
        public static Fix2 Direction(ushort bam) { return new Fix2(Cos(bam), Sin(bam)); }

        /// <summary>
        /// Binary angle of a vector. Uses a rational approximation of atan accurate to
        /// about 0.3 degrees, which is finer than any facing decision in the game.
        /// </summary>
        public static ushort Atan2(Fix y, Fix x)
        {
            if (x.Raw == 0 && y.Raw == 0) return 0;

            Fix ax = Fix.Abs(x);
            Fix ay = Fix.Abs(y);
            ushort octantAngle;

            // atan(z) ~= z * (pi/4) - z*(|z|-1)*(0.2447 + 0.0663*|z|), for |z| <= 1,
            // rescaled so that a quarter turn is 16384 bam.
            if (ax >= ay)
            {
                Fix z = ay / ax;
                octantAngle = AtanUnit(z);
            }
            else
            {
                Fix z = ax / ay;
                octantAngle = (ushort)(16384 - AtanUnit(z));
            }

            int result = octantAngle;
            if (x.Raw < 0) result = 32768 - result;
            if (y.Raw < 0) result = -result;
            return (ushort)(result & 0xFFFF);
        }

        // 0.2447 and 0.0663 in Q31.32.
        const long C1 = 1050820608L;
        const long C2 = 284717875L;

        /// <summary>
        /// atan of a value in [0,1], returned in bam (0..8192).
        ///
        /// atan(z) = (pi/4)z - z(z-1)(0.2447 + 0.0663z) radians. A quarter turn is
        /// 8192 bam, so the first term scales by 8192 directly; the correction term
        /// is in radians and scales by 65536/(2*pi).
        /// </summary>
        const long BamPerRadian = 10430L;

        static ushort AtanUnit(Fix z)
        {
            long zr = z.Raw;
            long correction = Fix.MulRaw(Fix.MulRaw(zr, zr - Fix.OneRaw), C1 + Fix.MulRaw(C2, zr));
            long bam = ((zr * 8192L) >> Fix.FracBits) - ((correction * BamPerRadian) >> Fix.FracBits);
            if (bam < 0) bam = 0;
            if (bam > 8192) bam = 8192;
            return (ushort)bam;
        }

        /// <summary>
        /// Shortest signed difference from one heading to another, in bam.
        /// Positive means counter-clockwise. Range is -32768..32767.
        /// </summary>
        public static int Delta(ushort from, ushort to)
        {
            return (short)(to - from);
        }

        /// <summary>Rotate a heading toward a target by at most maxStep bam.</summary>
        public static ushort RotateToward(ushort from, ushort to, int maxStep)
        {
            int d = Delta(from, to);
            if (d > maxStep) d = maxStep;
            if (d < -maxStep) d = -maxStep;
            return (ushort)((from + d) & 0xFFFF);
        }

        /// <summary>
        /// Real degrees per real second converted to bam per tick.
        ///
        /// The multiplier belongs here for the same reason it belongs in Dt: a
        /// turret's traverse rate and a drone's turn rate are physical rates in
        /// the same world the drone is flying through, so if the world runs at 4x
        /// they do too. Leaving this at the bare tick rate is what would make a
        /// 150 deg/s mount take four times as long to come round as the airframe
        /// crossing in front of it thinks it should.
        /// </summary>
        public static int DegreesPerSecondToBamPerTick(int degreesPerSecond)
        {
            // 360 degrees = 65536 bam; one tick is 1/8 of a real second at 4x.
            return (degreesPerSecond * 65536 * SimConstants.TimeMultiplier)
                 / (360 * SimConstants.TicksPerSecond);
        }
    }
}
