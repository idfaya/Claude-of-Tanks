using System;

namespace ClaudeOfTanks.Simulation
{
    public static class BallisticsSimulation
    {
        public const float GravityMps2 = 9.81f;
        public const float MaxLifetimeS = 6f;

        public static float PenetrationAtDistance(ShellSpec spec, float distanceM)
        {
            if (distanceM > 1000f && spec.Pen2000Mm > 0f)
            {
                float farT = MathUtil.Clamp01((distanceM - 1000f) / 1000f);
                return spec.Pen1000Mm + (spec.Pen2000Mm - spec.Pen1000Mm) * farT;
            }

            float t = MathUtil.Clamp01((distanceM - 100f) / 900f);
            return spec.Pen100Mm + (spec.Pen1000Mm - spec.Pen100Mm) * t;
        }

        public static float AimElevationRad(float distanceM, float velocityMps)
        {
            if (velocityMps <= 0f) return 0f;
            float ratio = MathUtil.Clamp(
                GravityMps2 * distanceM / (velocityMps * velocityMps), -1f, 1f);
            return 0.5f * MathF.Asin(ratio);
        }

        public static void Step(ShellState shell, float dt)
        {
            shell.PreviousPosition = shell.Position;
            shell.Position += shell.Velocity * dt;
            float gravity = shell.Spec.Guided ? 0f : GravityMps2 * MathF.Max(0f, shell.Spec.GravityScale);
            shell.Position.Y -= 0.5f * gravity * dt * dt;
            shell.Velocity.Y -= gravity * dt;
            shell.DistanceM += Float3.Distance(shell.PreviousPosition, shell.Position);
            shell.AgeS += dt;
            if (shell.AgeS > MaxLifetimeS) shell.Dead = true;
        }

        public static bool GuideToward(ShellState shell, Float3 target, float dt)
        {
            if (!shell.Spec.Guided || dt <= 0f) return false;
            float speed = shell.Velocity.Magnitude;
            Float3 desired = (target - shell.Position).Normalized;
            if (speed <= 0.000001f || desired.SqrMagnitude < 0.5f) return false;
            Float3 current = shell.Velocity / speed;
            float dot = MathUtil.Clamp(Float3.Dot(current, desired), -1f, 1f);
            float angle = MathF.Acos(dot);
            if (angle <= 0.000001f) return true;
            float fraction = MathUtil.Clamp01(shell.Spec.GuidanceTurnRateRadS * dt / angle);
            shell.Velocity = Float3.Lerp(current, desired, fraction).Normalized * speed;
            return true;
        }

        public static Float3 ApplyDispersion(
            Float3 direction, float sigmaRad, DeterministicRandom random)
        {
            if (sigmaRad <= 0f) return direction.Normalized;
            float u1 = MathF.Max(random.NextFloat(), 0.0000001f);
            float u2 = random.NextFloat();
            float radius = MathF.Sqrt(-2f * MathF.Log(u1));
            float x = radius * MathF.Cos(2f * MathUtil.Pi * u2);
            float y = radius * MathF.Sin(2f * MathUtil.Pi * u2);
            if (x * x + y * y > 4f)
            {
                radius = 2f * MathF.Sqrt(random.NextFloat());
                float theta = 2f * MathUtil.Pi * random.NextFloat();
                x = radius * MathF.Cos(theta);
                y = radius * MathF.Sin(theta);
            }

            Float3 forward = direction.Normalized;
            Float3 reference = MathF.Abs(forward.Y) > 0.99f
                ? new Float3(1f, 0f, 0f) : new Float3(0f, 1f, 0f);
            Float3 right = Float3.Cross(forward, reference).Normalized;
            Float3 up = Float3.Cross(right, forward).Normalized;
            return (forward + right * MathF.Tan(x * sigmaRad) +
                    up * MathF.Tan(y * sigmaRad)).Normalized;
        }
    }
}
