using System;

namespace ClaudeOfTanks.Simulation
{
    public static class HydropneumaticAimSimulation
    {
        private const float AimOriginHeightM = 1.65f;
        private const float AutoTraverseRampRad =
            8f * MathUtil.Deg2Rad;

        public static float ResolveSteer(
            TankState tank,
            TankInput input,
            float manualSteer)
        {
            if (!tank.Spec.FixedHydraulicGun ||
                MathF.Abs(manualSteer) >= 0.2f)
            {
                return manualSteer;
            }
            Float3 delta = input.AimPoint - tank.Position;
            if (delta.X * delta.X +
                delta.Z * delta.Z <= 0.001f)
            {
                return manualSteer;
            }
            float desiredYaw =
                MathF.Atan2(delta.X, delta.Z);
            float yawError =
                MathUtil.DeltaAngle(
                    tank.Yaw,
                    desiredYaw);
            return MathUtil.Clamp(
                yawError / AutoTraverseRampRad,
                -1f,
                1f);
        }

        public static void Step(
            TankState tank,
            TankInput input,
            float dt)
        {
            if (tank == null)
                throw new ArgumentNullException(nameof(tank));
            HydropneumaticAimSpec spec =
                tank.Spec.HydropneumaticAim;
            if (spec == null || !spec.IsValid)
            {
                tank.HydropneumaticAimActive = false;
                tank.HullPitchRad = 0f;
                return;
            }
            if (input.ToggleHydropneumaticAim)
            {
                tank.HydropneumaticAimActive =
                    !tank.HydropneumaticAimActive;
            }

            float target = 0f;
            if (tank.HydropneumaticAimActive)
            {
                Float3 origin =
                    tank.Position +
                    new Float3(0f, AimOriginHeightM, 0f);
                Float3 delta = input.AimPoint - origin;
                float horizontal =
                    MathF.Sqrt(
                        delta.X * delta.X +
                        delta.Z * delta.Z);
                if (horizontal > 0.001f)
                {
                    target = MathF.Atan2(
                        delta.Y,
                        horizontal);
                    target = MathUtil.Clamp(
                        target,
                        -spec.NoseDownRad,
                        spec.NoseUpRad);
                }
            }
            tank.HullPitchRad = MathUtil.MoveTowards(
                tank.HullPitchRad,
                target,
                spec.SpeedRadS * dt);
        }
    }
}
