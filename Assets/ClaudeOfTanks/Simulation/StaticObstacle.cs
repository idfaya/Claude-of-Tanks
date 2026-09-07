using System;

namespace ClaudeOfTanks.Simulation
{
    [Flags]
    public enum StaticObstacleFlags : byte
    {
        None = 0,
        Movement = 1,
        Shells = 2,
        Vision = 4,
        All = Movement | Shells | Vision
    }

    public readonly struct StaticObstacle
    {
        public readonly string Id;
        public readonly Float3 Center;
        public readonly float HalfWidthM;
        public readonly float HalfLengthM;
        public readonly float HeightM;
        public readonly float YawRad;
        internal readonly float CosYaw;
        internal readonly float SinYaw;
        public readonly StaticObstacleFlags Flags;
        public readonly bool Destructible;

        public StaticObstacle(
            string id,
            Float3 center,
            float halfWidthM,
            float halfLengthM,
            float heightM,
            float yawRad,
            StaticObstacleFlags flags,
            bool destructible = false)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 96)
                throw new ArgumentException("Static obstacle id is invalid.", nameof(id));
            if (!IsFinite(center.X) || !IsFinite(center.Y) || !IsFinite(center.Z) ||
                !IsFinite(halfWidthM) || !IsFinite(halfLengthM) ||
                !IsFinite(heightM) || !IsFinite(yawRad))
            {
                throw new ArgumentException("Static obstacle values must be finite.");
            }
            if (halfWidthM <= 0f || halfLengthM <= 0f || heightM <= 0f)
                throw new ArgumentOutOfRangeException(nameof(halfWidthM));
            if (flags == StaticObstacleFlags.None ||
                (flags & ~StaticObstacleFlags.All) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(flags));
            }

            Id = id;
            Center = center;
            HalfWidthM = halfWidthM;
            HalfLengthM = halfLengthM;
            HeightM = heightM;
            YawRad = yawRad;
            CosYaw = MathF.Cos(yawRad);
            SinYaw = MathF.Sin(yawRad);
            Flags = flags;
            Destructible = destructible;
        }

        public bool HasFlag(StaticObstacleFlags flag)
        {
            return (Flags & flag) != 0;
        }

        public bool IsValid =>
            !string.IsNullOrEmpty(Id) &&
            Id.Length <= 96 &&
            IsFinite(Center.X) &&
            IsFinite(Center.Y) &&
            IsFinite(Center.Z) &&
            IsFinite(HalfWidthM) &&
            IsFinite(HalfLengthM) &&
            IsFinite(HeightM) &&
            IsFinite(YawRad) &&
            HalfWidthM > 0f &&
            HalfLengthM > 0f &&
            HeightM > 0f &&
            Flags != StaticObstacleFlags.None &&
            (Flags & ~StaticObstacleFlags.All) == 0;

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
