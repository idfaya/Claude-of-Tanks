using System;

namespace ClaudeOfTanks.Simulation
{
    public static class TankPoseMath
    {
        public static float VisualPitchRad(
            TankState tank)
        {
            if (tank == null)
                throw new ArgumentNullException(
                    nameof(tank));
            return tank.TerrainPitchRad +
                tank.HullPitchRad;
        }

        public static Float3 GunDirection(
            TankState tank)
        {
            if (tank == null)
                throw new ArgumentNullException(
                    nameof(tank));
            float yaw =
                tank.Yaw +
                tank.TurretYaw;
            float pitch =
                VisualPitchRad(tank) +
                tank.GunPitchRad;
            float horizontal =
                MathF.Cos(pitch);
            return new Float3(
                MathF.Sin(yaw) * horizontal,
                MathF.Sin(pitch),
                MathF.Cos(yaw) * horizontal)
                .Normalized;
        }

        public static Float3 HullPointToWorld(
            TankState tank,
            Float3 local)
        {
            if (tank == null)
                throw new ArgumentNullException(
                    nameof(tank));
            Float3 value = RotateRoll(
                local,
                tank.HullRollRad);
            value = RotatePitch(
                value,
                -VisualPitchRad(tank));
            value = RotateYaw(
                value,
                tank.Yaw);
            return tank.Position + value;
        }

        public static Float3 RotateYaw(
            Float3 value,
            float angle)
        {
            float cosine = MathF.Cos(angle);
            float sine = MathF.Sin(angle);
            return new Float3(
                value.X * cosine +
                    value.Z * sine,
                value.Y,
                -value.X * sine +
                    value.Z * cosine);
        }

        public static Float3 RotatePitch(
            Float3 value,
            float angle)
        {
            float cosine = MathF.Cos(angle);
            float sine = MathF.Sin(angle);
            return new Float3(
                value.X,
                value.Y * cosine -
                    value.Z * sine,
                value.Y * sine +
                    value.Z * cosine);
        }

        public static Float3 RotateRoll(
            Float3 value,
            float angle)
        {
            float cosine = MathF.Cos(angle);
            float sine = MathF.Sin(angle);
            return new Float3(
                value.X * cosine -
                    value.Y * sine,
                value.X * sine +
                    value.Y * cosine,
                value.Z);
        }
    }
}
