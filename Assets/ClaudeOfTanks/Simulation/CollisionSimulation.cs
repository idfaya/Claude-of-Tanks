using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    public struct RamDamageResult
    {
        public float Total;
        public float ToRammer;
        public float ToVictim;
    }

    public static class CollisionSimulation
    {
        public const float MinimumClosingSpeedMps = 2.5f;
        private const float Epsilon = 0.000001f;

        public static RamDamageResult RamDamage(
            float rammerMassTons, float victimMassTons, float closingSpeedMps)
        {
            float rammerMass = rammerMassTons > 0f ? rammerMassTons : 40f;
            float victimMass = victimMassTons > 0f ? victimMassTons : 40f;
            float speed = MathF.Abs(closingSpeedMps);
            if (speed < MinimumClosingSpeedMps) return default;
            float reducedMass = rammerMass * victimMass / (rammerMass + victimMass);
            float total = MathF.Min(900f, 0.2f * speed * speed * reducedMass);
            return new RamDamageResult
            {
                Total = total,
                ToRammer = total * victimMass / (rammerMass + victimMass) * 0.65f,
                ToVictim = total * rammerMass / (rammerMass + victimMass)
            };
        }

        public static bool CircleIntersectsObstacle(
            Float3 center,
            float radius,
            StaticObstacle obstacle)
        {
            float cos = obstacle.CosYaw;
            float sin = obstacle.SinYaw;
            float dx = center.X - obstacle.Center.X;
            float dz = center.Z - obstacle.Center.Z;
            float localX = dx * cos - dz * sin;
            float localZ = dx * sin + dz * cos;
            float closestX = MathUtil.Clamp(
                localX, -obstacle.HalfWidthM, obstacle.HalfWidthM);
            float closestZ = MathUtil.Clamp(
                localZ, -obstacle.HalfLengthM, obstacle.HalfLengthM);
            float offsetX = localX - closestX;
            float offsetZ = localZ - closestZ;
            return offsetX * offsetX + offsetZ * offsetZ < radius * radius;
        }

        public static bool SegmentIntersectsObstacle(
            Float3 start,
            Float3 end,
            StaticObstacle obstacle,
            out float fraction,
            out Float3 normal)
        {
            float cos = obstacle.CosYaw;
            float sin = obstacle.SinYaw;
            Float3 localStart = ToLocal(start, obstacle.Center, cos, sin);
            Float3 localEnd = ToLocal(end, obstacle.Center, cos, sin);
            Float3 direction = localEnd - localStart;
            float enter = 0f;
            float exit = 1f;
            Float3 enterNormal = Float3.Zero;

            if (!ClipAxis(
                    localStart.X,
                    direction.X,
                    -obstacle.HalfWidthM,
                    obstacle.HalfWidthM,
                    new Float3(-1f, 0f, 0f),
                    new Float3(1f, 0f, 0f),
                    ref enter,
                    ref exit,
                    ref enterNormal) ||
                !ClipAxis(
                    localStart.Y,
                    direction.Y,
                    0f,
                    obstacle.HeightM,
                    new Float3(0f, -1f, 0f),
                    new Float3(0f, 1f, 0f),
                    ref enter,
                    ref exit,
                    ref enterNormal) ||
                !ClipAxis(
                    localStart.Z,
                    direction.Z,
                    -obstacle.HalfLengthM,
                    obstacle.HalfLengthM,
                    new Float3(0f, 0f, -1f),
                    new Float3(0f, 0f, 1f),
                    ref enter,
                    ref exit,
                    ref enterNormal))
            {
                fraction = 0f;
                normal = Float3.Zero;
                return false;
            }

            fraction = enter;
            normal = new Float3(
                enterNormal.X * cos + enterNormal.Z * sin,
                enterNormal.Y,
                -enterNormal.X * sin + enterNormal.Z * cos);
            if (normal.SqrMagnitude <= Epsilon)
            {
                Float3 reverse = (start - end).Normalized;
                normal = reverse.SqrMagnitude > Epsilon ? reverse : new Float3(0f, 1f, 0f);
            }
            return true;
        }

        public static bool TryFindFirstObstacleHit(
            IReadOnlyList<StaticObstacle> obstacles,
            StaticObstacleFlags requiredFlag,
            Float3 start,
            Float3 end,
            out StaticObstacle obstacle,
            out float fraction,
            out Float3 normal)
        {
            obstacle = default;
            fraction = float.MaxValue;
            normal = Float3.Zero;
            bool found = false;
            if (obstacles == null) return false;

            for (int i = 0; i < obstacles.Count; i++)
            {
                StaticObstacle candidate = obstacles[i];
                if (!candidate.HasFlag(requiredFlag)) continue;
                float candidateFraction;
                Float3 candidateNormal;
                if (SegmentIntersectsObstacle(
                        start,
                        end,
                        candidate,
                        out candidateFraction,
                        out candidateNormal) &&
                    candidateFraction < fraction)
                {
                    obstacle = candidate;
                    fraction = candidateFraction;
                    normal = candidateNormal;
                    found = true;
                }
            }
            return found;
        }

        private static Float3 ToLocal(
            Float3 point,
            Float3 origin,
            float cos,
            float sin)
        {
            float dx = point.X - origin.X;
            float dz = point.Z - origin.Z;
            return new Float3(
                dx * cos - dz * sin,
                point.Y - origin.Y,
                dx * sin + dz * cos);
        }

        private static bool ClipAxis(
            float start,
            float direction,
            float minimum,
            float maximum,
            Float3 minimumNormal,
            Float3 maximumNormal,
            ref float enter,
            ref float exit,
            ref Float3 enterNormal)
        {
            if (MathF.Abs(direction) <= Epsilon)
                return start >= minimum && start <= maximum;

            float inverse = 1f / direction;
            float first = (minimum - start) * inverse;
            float second = (maximum - start) * inverse;
            Float3 firstNormal = minimumNormal;
            if (first > second)
            {
                float swap = first;
                first = second;
                second = swap;
                firstNormal = maximumNormal;
            }
            if (first > enter)
            {
                enter = first;
                enterNormal = firstNormal;
            }
            if (second < exit) exit = second;
            return enter <= exit && exit >= 0f && enter <= 1f;
        }
    }
}
