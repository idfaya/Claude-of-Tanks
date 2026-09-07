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
        public readonly bool Crushable;
        public readonly float CrushSpeedRetention;

        public StaticObstacle(
            string id,
            Float3 center,
            float halfWidthM,
            float halfLengthM,
            float heightM,
            float yawRad,
            StaticObstacleFlags flags,
            bool destructible = false,
            bool crushable = false,
            float crushSpeedRetention = 0.94f)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 96)
                throw new ArgumentException("Static obstacle id is invalid.", nameof(id));
            if (!IsFinite(center.X) || !IsFinite(center.Y) || !IsFinite(center.Z) ||
                !IsFinite(halfWidthM) || !IsFinite(halfLengthM) ||
                !IsFinite(heightM) || !IsFinite(yawRad) ||
                !IsFinite(crushSpeedRetention))
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
            if (crushSpeedRetention < 0f || crushSpeedRetention > 1f)
                throw new ArgumentOutOfRangeException(nameof(crushSpeedRetention));
            if (crushable && !destructible)
                throw new ArgumentException("Crushable obstacles must be destructible.");

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
            Crushable = crushable;
            CrushSpeedRetention = crushSpeedRetention;
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
            CrushSpeedRetention >= 0f &&
            CrushSpeedRetention <= 1f &&
            (!Crushable || Destructible) &&
            Flags != StaticObstacleFlags.None &&
            (Flags & ~StaticObstacleFlags.All) == 0;

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
