// KILL ZONE - a real-time strategy video game.
// Two-dimensional fixed-point vector. The game is played on a plane; altitude is
// a three-value enum (Ground / Low / High), not a continuous coordinate, so the
// simulation never needs a third axis.

using System;

namespace KZ.Sim
{
    public readonly struct Fix2 : IEquatable<Fix2>
    {
        public readonly Fix X;
        public readonly Fix Y;

        public Fix2(Fix x, Fix y) { X = x; Y = y; }

        public static readonly Fix2 Zero = new Fix2(Fix.Zero, Fix.Zero);

        public static Fix2 FromInts(int x, int y) { return new Fix2(Fix.FromInt(x), Fix.FromInt(y)); }

        public static Fix2 operator +(Fix2 a, Fix2 b) { return new Fix2(a.X + b.X, a.Y + b.Y); }
        public static Fix2 operator -(Fix2 a, Fix2 b) { return new Fix2(a.X - b.X, a.Y - b.Y); }
        public static Fix2 operator -(Fix2 a) { return new Fix2(-a.X, -a.Y); }
        public static Fix2 operator *(Fix2 a, Fix s) { return new Fix2(a.X * s, a.Y * s); }
        public static Fix2 operator /(Fix2 a, Fix s) { return new Fix2(a.X / s, a.Y / s); }

        public static bool operator ==(Fix2 a, Fix2 b) { return a.X == b.X && a.Y == b.Y; }
        public static bool operator !=(Fix2 a, Fix2 b) { return !(a == b); }

        public bool Equals(Fix2 o) { return X == o.X && Y == o.Y; }
        public override bool Equals(object o) { return o is Fix2 && Equals((Fix2)o); }
        public override int GetHashCode() { return X.GetHashCode() * 397 ^ Y.GetHashCode(); }
        public override string ToString() { return "(" + X + ", " + Y + ")"; }

        public static Fix Dot(Fix2 a, Fix2 b) { return a.X * b.X + a.Y * b.Y; }

        /// <summary>Squared length. Prefer this over Length for comparisons - it skips a sqrt.</summary>
        public Fix SqrMagnitude() { return X * X + Y * Y; }

        public Fix Magnitude() { return Fix.Sqrt(SqrMagnitude()); }

        public static Fix Distance(Fix2 a, Fix2 b) { return (a - b).Magnitude(); }
        public static Fix SqrDistance(Fix2 a, Fix2 b) { return (a - b).SqrMagnitude(); }

        /// <summary>Unit vector, or zero if the input is zero. Never throws.</summary>
        public Fix2 Normalized()
        {
            Fix m = Magnitude();
            if (m.Raw == 0) return Zero;
            return new Fix2(X / m, Y / m);
        }

        /// <summary>Clamp this vector's length to max, preserving direction.</summary>
        public Fix2 ClampMagnitude(Fix max)
        {
            Fix sq = SqrMagnitude();
            if (sq <= max * max) return this;
            return Normalized() * max;
        }

        /// <summary>Perpendicular (90 degrees counter-clockwise). Used for tether tangents.</summary>
        public Fix2 Perpendicular() { return new Fix2(-Y, X); }
    }
}
