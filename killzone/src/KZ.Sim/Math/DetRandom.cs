// KILL ZONE - a real-time strategy video game.
// Deterministic random number generation, split into named streams.
//
// The simulation has exactly four places where chance is allowed: whether an
// interceptor connects, whether an autonomous munition picks the right target,
// whether a fiber tether snags on terrain, and a small amount of AI jitter so
// two AI players do not move in lockstep. Each gets its own stream so that
// adding a roll in one system can never shift the sequence seen by another -
// which is the classic way a "harmless" change breaks every saved replay.
//
// Cosmetic randomness (particle variation, idle animations, voice line choice)
// lives in the presentation layer and is deliberately excluded from the state
// hash, so a prettier explosion can never desync a match.

namespace KZ.Sim
{
    public enum RandomStream
    {
        Interception = 0,
        AutonomyClassify = 1,
        TetherSnag = 2,
        AIJitter = 3,
        Count = 4
    }

    /// <summary>xoshiro256** - small, fast, and specified purely in terms of integer ops.</summary>
    public sealed class DetRandom
    {
        ulong s0, s1, s2, s3;

        public DetRandom(ulong seed)
        {
            // SplitMix64 to spread a single seed across the state.
            s0 = SplitMix(ref seed);
            s1 = SplitMix(ref seed);
            s2 = SplitMix(ref seed);
            s3 = SplitMix(ref seed);
        }

        static ulong SplitMix(ref ulong x)
        {
            x += 0x9E3779B97F4A7C15UL;
            ulong z = x;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        static ulong Rotl(ulong x, int k) { return (x << k) | (x >> (64 - k)); }

        public ulong NextULong()
        {
            ulong result = Rotl(s1 * 5UL, 7) * 9UL;
            ulong t = s1 << 17;
            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;
            s2 ^= t;
            s3 = Rotl(s3, 45);
            return result;
        }

        /// <summary>Uniform in [0, bound). Rejection sampled so the distribution is exact.</summary>
        public int NextInt(int bound)
        {
            if (bound <= 0) return 0;
            ulong ubound = (ulong)bound;
            ulong limit = ulong.MaxValue - (ulong.MaxValue % ubound);
            ulong r;
            do { r = NextULong(); } while (r >= limit);
            return (int)(r % ubound);
        }

        /// <summary>Uniform in [0,1) as a Fix. Used for probability rolls.</summary>
        public Fix NextFix()
        {
            // Take 32 bits so the result lands exactly in the fractional field.
            return new Fix((long)(NextULong() >> 32));
        }

        /// <summary>True with probability p, where p is a Fix in [0,1].</summary>
        public bool Chance(Fix p)
        {
            if (p.Raw <= 0) return false;
            if (p.Raw >= Fix.OneRaw) return true;
            return NextFix() < p;
        }

        /// <summary>True with probability percent/100.</summary>
        public bool ChancePercent(int percent)
        {
            if (percent <= 0) return false;
            if (percent >= 100) return true;
            return NextInt(100) < percent;
        }

        public ulong PeekStateHash()
        {
            return s0 ^ Rotl(s1, 13) ^ Rotl(s2, 29) ^ Rotl(s3, 47);
        }
    }

    /// <summary>The set of streams owned by one world. Hashed as part of sim state.</summary>
    public sealed class RandomStreams
    {
        readonly DetRandom[] streams = new DetRandom[(int)RandomStream.Count];

        public RandomStreams(ulong matchSeed)
        {
            for (int i = 0; i < (int)RandomStream.Count; i++)
            {
                // Offset each stream so they never share a sequence.
                streams[i] = new DetRandom(matchSeed + (ulong)i * 0x1000193UL);
            }
        }

        public DetRandom Get(RandomStream s) { return streams[(int)s]; }

        public ulong StateHash()
        {
            ulong h = 1469598103934665603UL;
            for (int i = 0; i < streams.Length; i++)
            {
                h ^= streams[i].PeekStateHash();
                h *= 1099511628211UL;
            }
            return h;
        }
    }
}
