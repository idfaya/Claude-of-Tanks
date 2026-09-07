using System;

namespace ClaudeOfTanks.Simulation
{
    public static class TankMovement
    {
        private const float DriveAccelerationPerHpPerTon = 0.16f;
        private const float CoastDecelerationMps2 = 2.4f;
        private const float BrakeDecelerationMps2 = 9f;

        public static void Step(TankState tank, TankInput input, IHeightField heightField, float dt)
        {
            if (tank.Destroyed || dt <= 0f)
            {
                return;
            }

            float throttle = MathUtil.Clamp(input.Throttle, -1f, 1f);
            float steer = MathUtil.Clamp(input.Steer, -1f, 1f);
            float forwardLimit = tank.Spec.TopSpeedKmh / 3.6f;
            float reverseLimit = tank.Spec.ReverseSpeedKmh / 3.6f;
            float targetSpeed = throttle >= 0f ? throttle * forwardLimit : throttle * reverseLimit;
            float powerToWeight = tank.Spec.EnginePowerHp / MathF.Max(1f, tank.Spec.WeightTons);
            float acceleration = MathUtil.Clamp(
                powerToWeight * DriveAccelerationPerHpPerTon,
                1.8f,
                7.5f);

            if (input.Brake)
            {
                tank.SpeedMps = MathUtil.MoveTowards(tank.SpeedMps, 0f, BrakeDecelerationMps2 * dt);
            }
            else if (MathF.Abs(throttle) > 0.01f)
            {
                tank.SpeedMps = MathUtil.MoveTowards(tank.SpeedMps, targetSpeed, acceleration * dt);
            }
            else
            {
                tank.SpeedMps = MathUtil.MoveTowards(tank.SpeedMps, 0f, CoastDecelerationMps2 * dt);
            }

            float speedFraction = MathUtil.Clamp01(MathF.Abs(tank.SpeedMps) / MathF.Max(1f, forwardLimit));
            float pivotFactor = MathF.Abs(tank.SpeedMps) < 0.3f ? 0.72f : 1f - speedFraction * 0.35f;
            float direction = tank.SpeedMps < -0.05f ? -1f : 1f;
            tank.Yaw += steer * direction * tank.Spec.HullTraverseDegS * MathUtil.Deg2Rad * pivotFactor * dt;

            Float3 forward = Float3.Forward(tank.Yaw);
            Float3 next = tank.Position + forward * (tank.SpeedMps * dt);
            next.Y = heightField.HeightAt(next.X, next.Z);
            tank.Position = next;

            Float3 toAim = input.AimPoint - tank.Position;
            if (toAim.X * toAim.X + toAim.Z * toAim.Z > 0.001f)
            {
                float desiredWorldYaw = MathF.Atan2(toAim.X, toAim.Z);
                float desiredLocalYaw = MathUtil.DeltaAngle(tank.Yaw, desiredWorldYaw);
                float delta = MathUtil.DeltaAngle(tank.TurretYaw, desiredLocalYaw);
                float maxStep = tank.Spec.TurretTraverseDegS * MathUtil.Deg2Rad * dt;
                tank.TurretYaw += MathUtil.Clamp(delta, -maxStep, maxStep);
            }

            tank.ReloadRemainingS = MathF.Max(0f, tank.ReloadRemainingS - dt);
        }
    }
}
