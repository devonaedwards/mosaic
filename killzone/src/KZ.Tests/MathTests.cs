// KILL ZONE - a real-time strategy video game.
// Tests for the fixed-point math the whole simulation stands on.

using System;
using KZ.Sim;

namespace KZ.Tests
{
    public static class MathTests
    {
        static Fix F(double v) { return Fix.FromDoubleContentOnly(v); }

        public static void Register(TestRunner r)
        {
            r.Group("fixed-point arithmetic");

            r.Run("addition and subtraction round-trip", delegate
            {
                Fix a = F(123.25), b = F(77.5);
                Assert.Near(200.75, (a + b).ToDoubleForDisplay(), 1e-9, "sum");
                Assert.Near(45.75, (a - b).ToDoubleForDisplay(), 1e-9, "difference");
            });

            r.Run("multiplication keeps precision through the 128-bit intermediate", delegate
            {
                Assert.Near(6.0, (F(2.5) * F(2.4)).ToDoubleForDisplay(), 1e-8, "2.5 * 2.4");
                Assert.Near(-6.0, (F(-2.5) * F(2.4)).ToDoubleForDisplay(), 1e-8, "-2.5 * 2.4");
                Assert.Near(0.0625, (F(0.25) * F(0.25)).ToDoubleForDisplay(), 1e-9, "0.25 squared");
                // Large times small is where a naive shift-then-multiply loses everything.
                Assert.Near(1.0, (F(100000.0) * F(0.00001)).ToDoubleForDisplay(), 1e-4, "1e5 * 1e-5");
            });

            r.Run("division", delegate
            {
                Assert.Near(4.0, (F(10.0) / F(2.5)).ToDoubleForDisplay(), 1e-8, "10 / 2.5");
                Assert.Near(-4.0, (F(-10.0) / F(2.5)).ToDoubleForDisplay(), 1e-8, "-10 / 2.5");
                Assert.Near(0.333333, (Fix.One / Fix.FromInt(3)).ToDoubleForDisplay(), 1e-6, "one third");
            });

            r.Run("division by zero throws rather than corrupting state", delegate
            {
                bool threw = false;
                try { Fix x = Fix.One / Fix.Zero; GC.KeepAlive(x); }
                catch (DivideByZeroException) { threw = true; }
                Assert.True(threw, "divide by zero throws");
            });

            r.Run("square root", delegate
            {
                Assert.Near(4.0, Fix.Sqrt(F(16.0)).ToDoubleForDisplay(), 1e-8, "sqrt 16");
                Assert.Near(1.41421356, Fix.Sqrt(F(2.0)).ToDoubleForDisplay(), 1e-7, "sqrt 2");
                Assert.Near(0.0, Fix.Sqrt(Fix.Zero).ToDoubleForDisplay(), 1e-12, "sqrt 0");
                Assert.Near(31.6227766, Fix.Sqrt(F(1000.0)).ToDoubleForDisplay(), 1e-6, "sqrt 1000");
            });

            r.Run("Ratio builds exact fractions without touching a float", delegate
            {
                Assert.Near(0.75, Fix.Ratio(3, 4).ToDoubleForDisplay(), 1e-9, "3/4");
                Assert.Near(0.03125, Fix.Ratio(1, 32).ToDoubleForDisplay(), 1e-12, "1/32");
            });

            r.Run("the simulation timestep is exactly representable", delegate
            {
                // This is the reason the game ticks at 32 Hz and not 30: 1/32 is
                // exact in binary, so dt accumulates no rounding error over a
                // match, and "22 metres per second" means 22 metres per second.
                Fix accumulated = Fix.Zero;
                for (int i = 0; i < SimConstants.TicksPerSecond; i++) accumulated += SimConstants.Dt;
                Assert.Equal(Fix.OneRaw, accumulated.Raw, "32 ticks sum to exactly one second");
            });

            r.Group("vectors");

            r.Run("magnitude and distance", delegate
            {
                Fix2 v = new Fix2(F(3), F(4));
                Assert.Near(5.0, v.Magnitude().ToDoubleForDisplay(), 1e-7, "3-4-5 triangle");
                Assert.Near(25.0, v.SqrMagnitude().ToDoubleForDisplay(), 1e-7, "squared magnitude");
                Assert.Near(5.0, Fix2.Distance(Fix2.Zero, v).ToDoubleForDisplay(), 1e-7, "distance from origin");
            });

            r.Run("normalize, including the degenerate case", delegate
            {
                Fix2 n = new Fix2(F(3), F(4)).Normalized();
                Assert.Near(1.0, n.Magnitude().ToDoubleForDisplay(), 1e-6, "unit length");
                Assert.True(Fix2.Zero.Normalized() == Fix2.Zero, "zero vector normalizes to zero, not NaN");
            });

            r.Run("clamp magnitude", delegate
            {
                Fix2 v = new Fix2(F(30), F(40)).ClampMagnitude(F(10));
                Assert.Near(10.0, v.Magnitude().ToDoubleForDisplay(), 1e-6, "clamped to 10");
                Fix2 small = new Fix2(F(1), F(1)).ClampMagnitude(F(10));
                Assert.Near(1.41421, small.Magnitude().ToDoubleForDisplay(), 1e-4, "short vector untouched");
            });

            r.Group("trigonometry");

            r.Run("sine at the cardinal angles", delegate
            {
                Assert.Near(0.0, Trig.Sin(0).ToDoubleForDisplay(), 1e-5, "sin 0");
                Assert.Near(1.0, Trig.Sin(16384).ToDoubleForDisplay(), 1e-4, "sin 90 degrees");
                Assert.Near(0.0, Trig.Sin(32768).ToDoubleForDisplay(), 1e-4, "sin 180 degrees");
                Assert.Near(-1.0, Trig.Sin(49152).ToDoubleForDisplay(), 1e-4, "sin 270 degrees");
            });

            r.Run("sine tracks the real function across a full turn", delegate
            {
                double worst = 0;
                for (int bam = 0; bam < 65536; bam += 97)
                {
                    double expected = Math.Sin(bam * 2.0 * Math.PI / 65536.0);
                    double actual = Trig.Sin((ushort)bam).ToDoubleForDisplay();
                    double err = Math.Abs(expected - actual);
                    if (err > worst) worst = err;
                }
                Assert.InRange(0.0, 1e-4, worst, "worst-case sine error over a full turn");
            });

            r.Run("cosine is sine a quarter turn on", delegate
            {
                Assert.Near(1.0, Trig.Cos(0).ToDoubleForDisplay(), 1e-4, "cos 0");
                Assert.Near(0.0, Trig.Cos(16384).ToDoubleForDisplay(), 1e-4, "cos 90 degrees");
                Assert.Near(-1.0, Trig.Cos(32768).ToDoubleForDisplay(), 1e-4, "cos 180 degrees");
            });

            r.Run("direction vectors are unit length at every heading", delegate
            {
                for (int bam = 0; bam < 65536; bam += 1024)
                {
                    Fix2 d = Trig.Direction((ushort)bam);
                    Assert.Near(1.0, d.Magnitude().ToDoubleForDisplay(), 1e-3, "unit direction at " + bam);
                }
            });

            r.Run("atan2 recovers the heading it was given", delegate
            {
                for (int bam = 0; bam < 65536; bam += 512)
                {
                    Fix2 d = Trig.Direction((ushort)bam);
                    ushort recovered = Trig.Atan2(d.Y, d.X);
                    int error = Math.Abs(Trig.Delta((ushort)bam, recovered));
                    Assert.InRange(0, 120, error, "atan2 round-trip error at heading " + bam);
                }
            });

            r.Run("heading rotation takes the short way round", delegate
            {
                // From just before a full turn to just after zero is a small step
                // forward, not a long trip backwards.
                ushort result = Trig.RotateToward(65000, 200, 1000);
                Assert.True(result > 65000 || result < 1000, "wrapped forwards, not backwards");
                Assert.Equal(-1000, Trig.RotateToward(1000, 0, 5000) - 1000, "clean step to zero");
            });

            r.Group("deterministic random");

            r.Run("the same seed replays the same sequence", delegate
            {
                DetRandom a = new DetRandom(12345);
                DetRandom b = new DetRandom(12345);
                for (int i = 0; i < 200; i++)
                    Assert.Equal((long)a.NextULong(), (long)b.NextULong(), "value " + i);
            });

            r.Run("different seeds diverge", delegate
            {
                DetRandom a = new DetRandom(1);
                DetRandom b = new DetRandom(2);
                bool anyDifferent = false;
                for (int i = 0; i < 20; i++)
                    if (a.NextULong() != b.NextULong()) anyDifferent = true;
                Assert.True(anyDifferent, "streams diverge");
            });

            r.Run("bounded integers stay in range and cover it", delegate
            {
                DetRandom rng = new DetRandom(99);
                bool[] seen = new bool[10];
                for (int i = 0; i < 5000; i++)
                {
                    int v = rng.NextInt(10);
                    Assert.InRange(0, 9, v, "in range");
                    seen[v] = true;
                }
                for (int i = 0; i < 10; i++) Assert.True(seen[i], "value " + i + " appeared");
            });

            r.Run("percentage chances converge on their stated rate", delegate
            {
                DetRandom rng = new DetRandom(7);
                int hits = 0;
                for (int i = 0; i < 20000; i++) if (rng.ChancePercent(35)) hits++;
                Assert.InRange(0.33, 0.37, hits / 20000.0, "35 percent over 20,000 trials");
            });

            r.Run("streams are independent, so adding a roll cannot shift another system", delegate
            {
                RandomStreams s1 = new RandomStreams(555);
                RandomStreams s2 = new RandomStreams(555);

                // Burn a lot of interception rolls in the first world only.
                for (int i = 0; i < 100; i++) s1.Get(RandomStream.Interception).NextULong();

                // The snag stream must be untouched by that.
                Assert.Equal((long)s1.Get(RandomStream.TetherSnag).NextULong(),
                             (long)s2.Get(RandomStream.TetherSnag).NextULong(),
                             "snag stream unaffected by interception rolls");
            });
        }
    }
}
