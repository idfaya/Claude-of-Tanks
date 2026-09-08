using System;

namespace ClaudeOfTanks.Simulation
{
    public static class TankMovement
    {
        private const float DriveAccelerationPerHpPerTon = 0.16f;
        private const float CoastDecelerationMps2 = 2.4f;
        private const float BrakeDecelerationMps2 = 9f;
        private const float BloomGrowTimeS = 0.05f;
        private const float AimSettledRatio = 6f;

        public static void Step(TankState tank, TankInput input, IHeightField heightField, float dt)
        {
            if (tank.Destroyed || dt <= 0f)
            {
                return;
            }

            float previousYaw = tank.Yaw;
            float previousTurretYaw = tank.TurretYaw;
            float throttle = MathUtil.Clamp(input.Throttle, -1f, 1f);
            float steer = MathUtil.Clamp(input.Steer, -1f, 1f);
            float forwardLimit = tank.Spec.TopSpeedKmh / 3.6f;
            float reverseLimit = tank.Spec.ReverseSpeedKmh / 3.6f;
            float targetSpeed = throttle >= 0f ? throttle * forwardLimit : throttle * reverseLimit;
            float powerToWeight = tank.Spec.EnginePowerHp / MathF.Max(1f, tank.Spec.WeightTons);
            ITerrainSurface surface = heightField as ITerrainSurface;
            Float3 groundNormal = surface != null
                ? surface.NormalAt(tank.Position.X, tank.Position.Z).Normalized
                : new Float3(0f, 1f, 0f);
            float surfaceResistance = surface != null
                ? MathF.Max(0.25f, surface.ResistanceAt(tank.Position.X, tank.Position.Z))
                : 1f;
            float slopeTraction = MathUtil.Clamp01(groundNormal.Y * MathF.Max(0.1f, tank.Spec.TrackTraction));
            float acceleration = MathUtil.Clamp(
                powerToWeight * DriveAccelerationPerHpPerTon,
                1.8f,
                7.5f) * slopeTraction / MathF.Max(0.25f, tank.Spec.TerrainResistance * surfaceResistance);

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
            tank.Yaw += steer * direction * tank.Spec.HullTraverseDegS *
                tank.TraverseMultiplier * MathUtil.Deg2Rad * pivotFactor * dt;

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
                float maxStep = tank.Spec.TurretTraverseDegS *
                    tank.TurretMultiplier * MathUtil.Deg2Rad * dt;
                tank.TurretYaw += MathUtil.Clamp(delta, -maxStep, maxStep);
            }

            tank.HullYawRateRadS =
                MathUtil.DeltaAngle(previousYaw, tank.Yaw) / dt;
            tank.TurretYawRateRadS =
                MathUtil.DeltaAngle(previousTurretYaw, tank.TurretYaw) / dt;
            UpdateAimBloom(tank, dt);
        }

        public static float DispersionSigmaRad(TankState tank)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            return MathF.Max(0f, tank.Spec.BaseAccuracyMAt100) *
                MathF.Max(1f, tank.AimBloom) / 200f;
        }

        public static void ApplyPostShotBloom(TankState tank)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            tank.AimBloom = MathF.Max(
                1f,
                tank.AimBloom *
                MathF.Max(1f, tank.Spec.AimBloomAfterShot));
        }

        private static void UpdateAimBloom(TankState tank, float dt)
        {
            TankSpec spec = tank.Spec;
            float move = spec.AimBloomMove *
                MathF.Abs(tank.SpeedMps) * 3.6f;
            float hull = spec.AimBloomHullRotation *
                MathF.Abs(tank.HullYawRateRadS) / MathUtil.Deg2Rad;
            float turret = spec.AimBloomTurretRotation *
                MathF.Abs(tank.TurretYawRateRadS) / MathUtil.Deg2Rad;
            float target = MathF.Sqrt(
                1f + move * move + hull * hull + turret * turret);
            float bloomMultiplier = MathF.Max(
                0f,
                tank.Combat.Equipment.Bloom);
            target = 1f + (target - 1f) * bloomMultiplier;

            float aimTime = MathF.Max(
                0.01f,
                spec.AimTimeS *
                MathF.Max(0.01f, tank.Combat.Equipment.AimTime));
            float tau = target > tank.AimBloom
                ? BloomGrowTimeS
                : aimTime / MathF.Log(AimSettledRatio);
            float blend = 1f - MathF.Exp(-dt / tau);
            tank.AimBloom += (target - tank.AimBloom) * blend;
            if (tank.AimBloom < 1f) tank.AimBloom = 1f;
        }
    }
}
