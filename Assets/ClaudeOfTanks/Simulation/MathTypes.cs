using System;

namespace ClaudeOfTanks.Simulation
{
    public struct Float3 : IEquatable<Float3>
    {
        public float X;
        public float Y;
        public float Z;

        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Float3 Zero => new Float3(0f, 0f, 0f);
        public float SqrMagnitude => X * X + Y * Y + Z * Z;
        public float Magnitude => MathF.Sqrt(SqrMagnitude);

        public Float3 Normalized
        {
            get
            {
                float length = Magnitude;
                return length > 0.000001f ? this / length : Zero;
            }
        }

        public static Float3 Forward(float yaw)
        {
            return new Float3(MathF.Sin(yaw), 0f, MathF.Cos(yaw));
        }

        public static float Dot(Float3 a, Float3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Float3 Cross(Float3 a, Float3 b)
        {
            return new Float3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static float Distance(Float3 a, Float3 b)
        {
            return (a - b).Magnitude;
        }

        public static Float3 Lerp(Float3 a, Float3 b, float t)
        {
            t = MathUtil.Clamp01(t);
            return a + (b - a) * t;
        }

        public static Float3 operator +(Float3 a, Float3 b)
        {
            return new Float3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Float3 operator -(Float3 a, Float3 b)
        {
            return new Float3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Float3 operator *(Float3 value, float scale)
        {
            return new Float3(value.X * scale, value.Y * scale, value.Z * scale);
        }

        public static Float3 operator /(Float3 value, float scale)
        {
            return new Float3(value.X / scale, value.Y / scale, value.Z / scale);
        }

        public bool Equals(Float3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is Float3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X.GetHashCode();
                hash = hash * 397 ^ Y.GetHashCode();
                return hash * 397 ^ Z.GetHashCode();
            }
        }
    }

    public static class MathUtil
    {
        public const float Pi = 3.14159265358979323846f;
        public const float Deg2Rad = Pi / 180f;

        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (MathF.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + MathF.Sign(target - current) * maxDelta;
        }

        public static float DeltaAngle(float current, float target)
        {
            float delta = Repeat(target - current, Pi * 2f);
            return delta > Pi ? delta - Pi * 2f : delta;
        }

        public static float Repeat(float value, float length)
        {
            return Clamp(value - MathF.Floor(value / length) * length, 0f, length);
        }
    }

    public sealed class DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(uint seed)
        {
            _state = seed;
        }

        public float NextFloat()
        {
            _state += 0x6D2B79F5u;
            uint value = _state;
            value = (value ^ value >> 15) * (value | 1u);
            value ^= value + (value ^ value >> 7) * (value | 61u);
            return (value ^ value >> 14) / 4294967296f;
        }

        public float Range(float min, float max)
        {
            return min + (max - min) * NextFloat();
        }
    }
}
