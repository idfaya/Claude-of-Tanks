using System;

namespace ClaudeOfTanks.Simulation
{
    public sealed partial class BattleState
    {
        private const float TreeConcealmentBonus =
            0.08f;

        public float ConcealmentBonusBetween(
            Float3 spotter,
            Float3 target,
            bool targetFired)
        {
            const float maximumBonus = 0.5f;
            const float fireTransparentM = 15f;
            int minX;
            int minZ;
            int maxX;
            int maxZ;
            QueryCellRange(
                MathF.Min(spotter.X, target.X),
                MathF.Min(spotter.Z, target.Z),
                MathF.Max(spotter.X, target.X),
                MathF.Max(spotter.Z, target.Z),
                out minX,
                out minZ,
                out maxX,
                out maxZ);
            int mark = NextObstacleQueryMark();
            float bonus = 0f;
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX;
                    x <= maxX;
                    x++)
                {
                    var candidates =
                        _obstacleGrid[
                            z *
                            ObstacleGridAxis +
                            x];
                    if (candidates == null)
                        continue;
                    for (int item = 0;
                        item <
                            candidates.Count;
                        item++)
                    {
                        int index =
                            candidates[item];
                        if (_obstacleQueryMarks[
                                index] == mark)
                        {
                            continue;
                        }
                        _obstacleQueryMarks[index] =
                            mark;
                        if (_staticObstacleDestroyed[
                                index])
                        {
                            continue;
                        }
                        StaticObstacle obstacle =
                            StaticObstacles[index];
                        if (!obstacle.HasFlag(
                                StaticObstacleFlags
                                    .Concealment))
                        {
                            continue;
                        }
                        float radius =
                            MathF.Max(
                                obstacle.HalfWidthM,
                                obstacle.HeightM *
                                    0.42f);
                        if (!SegmentIntersectsDisc(
                                spotter,
                                target,
                                obstacle.Center,
                                radius))
                        {
                            continue;
                        }
                        if (targetFired &&
                            HorizontalDistance(
                                obstacle.Center,
                                target) -
                                radius <
                            fireTransparentM)
                        {
                            continue;
                        }
                        bonus +=
                            TreeConcealmentBonus;
                        if (bonus >= maximumBonus)
                            return maximumBonus;
                    }
                }
            }
            return MathF.Min(
                maximumBonus,
                bonus);
        }

        private static bool SegmentIntersectsDisc(
            Float3 start,
            Float3 end,
            Float3 center,
            float radius)
        {
            float dx = end.X - start.X;
            float dz = end.Z - start.Z;
            float lengthSquared =
                dx * dx + dz * dz;
            float t = lengthSquared >
                0.000001f
                    ? ((center.X - start.X) *
                       dx +
                       (center.Z - start.Z) *
                       dz) /
                      lengthSquared
                    : 0f;
            t = MathUtil.Clamp01(t);
            float x = start.X + dx * t;
            float z = start.Z + dz * t;
            float ox = center.X - x;
            float oz = center.Z - z;
            return ox * ox + oz * oz <=
                radius * radius;
        }

        private static float HorizontalDistance(
            Float3 first,
            Float3 second)
        {
            float x = first.X - second.X;
            float z = first.Z - second.Z;
            return MathF.Sqrt(x * x + z * z);
        }
    }
}
