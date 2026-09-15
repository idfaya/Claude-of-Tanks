using System;

namespace ClaudeOfTanks.Simulation
{
    public sealed class BotNavigationPlanner
    {
        private readonly BattleState _battle;

        public BotNavigationPlanner(
            BattleState battle)
        {
            _battle = battle ??
                throw new ArgumentNullException(
                    nameof(battle));
        }

        public Float3 NextWaypoint(
            TankState tank,
            Float3 goal)
        {
            Float3 start =
                tank.Position +
                new Float3(0f, 1f, 0f);
            Float3 end =
                goal +
                new Float3(0f, 1f, 0f);
            int index;
            StaticObstacle obstacle;
            float fraction;
            Float3 normal;
            if (!_battle
                    .TryFindFirstStaticObstacleHit(
                        StaticObstacleFlags
                            .Movement,
                        start,
                        end,
                        out index,
                        out obstacle,
                        out fraction,
                        out normal))
            {
                return goal;
            }

            Float3 direction =
                (goal - tank.Position)
                    .Normalized;
            Float3 side =
                new Float3(
                    direction.Z,
                    0f,
                    -direction.X);
            float clearance =
                MathF.Max(
                    obstacle.HalfWidthM,
                    obstacle.HalfLengthM) +
                tank.Spec.CollisionRadiusM +
                2f;
            float sign = StableSide(
                tank.Id,
                obstacle.Id);
            Float3 waypoint =
                obstacle.Center +
                side * (clearance * sign);
            waypoint.Y =
                _battle.HeightField.HeightAt(
                    waypoint.X,
                    waypoint.Z);
            return waypoint;
        }

        private static float StableSide(
            string tankId,
            string obstacleId)
        {
            unchecked
            {
                uint hash = 2166136261u;
                string text =
                    (tankId ?? string.Empty) +
                    "|" +
                    (obstacleId ??
                     string.Empty);
                for (int i = 0;
                    i < text.Length;
                    i++)
                {
                    hash ^= text[i];
                    hash *= 16777619u;
                }
                return (hash & 1u) == 0u
                    ? -1f
                    : 1f;
            }
        }
    }
}
