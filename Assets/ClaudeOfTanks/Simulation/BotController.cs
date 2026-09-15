using System;
using System.Collections.Generic;

namespace ClaudeOfTanks.Simulation
{
    public sealed class BotController
    {
        public const float PreferredMaximumRangeM = 34f;
        public const float PreferredMinimumRangeM = 18f;
        public const float FireRangeM = 105f;
        public const float FireAlignmentRad = 0.055f;

        private const float AimHeightM = 1.25f;
        private readonly SpottingSimulation _spotting;
        private readonly Func<Float3, Float3, bool> _isOccluded;
        private readonly BattleState _battle;
        private readonly Func<TankState, Float3?>
            _objectiveTarget;
        private readonly BotNavigationPlanner _navigation;

        public BotController(
            SpottingSimulation spotting,
            Func<Float3, Float3, bool> isOccluded = null,
            BattleState battle = null,
            Func<TankState, Float3?>
                objectiveTarget = null)
        {
            _spotting = spotting ?? throw new ArgumentNullException(nameof(spotting));
            _isOccluded = isOccluded;
            _battle = battle;
            _objectiveTarget = objectiveTarget;
            _navigation = battle != null
                ? new BotNavigationPlanner(battle)
                : null;
        }

        public TankInput Decide(TankState bot, IReadOnlyList<TankState> tanks)
        {
            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            TankInput input = default;
            input.AimPoint = bot.Position + Float3.Forward(bot.Yaw + bot.TurretYaw) * FireRangeM;
            if (bot.Destroyed || tanks == null)
            {
                return input;
            }

            TankState target = SelectTarget(bot, tanks);
            Float3? objective =
                _objectiveTarget?.Invoke(bot);
            if (target == null &&
                !objective.HasValue)
            {
                return input;
            }

            Float3 combatTarget =
                target != null
                    ? target.Position
                    : objective.Value;
            Float3 moveTarget =
                objective ??
                combatTarget;
            if (_navigation != null)
            {
                moveTarget =
                    _navigation.NextWaypoint(
                        bot,
                        moveTarget);
            }
            Float3 offset =
                moveTarget - bot.Position;
            float distance = offset.Magnitude;
            float desiredYaw = MathF.Atan2(offset.X, offset.Z);
            float hullDelta = MathUtil.DeltaAngle(bot.Yaw, desiredYaw);
            Float3 aimOffset =
                combatTarget - bot.Position;
            float combatDistance =
                aimOffset.Magnitude;
            float aimYaw = MathF.Atan2(
                aimOffset.X,
                aimOffset.Z);
            float gunDelta = MathUtil.DeltaAngle(
                bot.Yaw + bot.TurretYaw,
                aimYaw);

            input.AimPoint = combatTarget +
                new Float3(
                    0f,
                    AimHeightM,
                    0f);
            input.Steer = MathUtil.Clamp(hullDelta * 2.2f, -1f, 1f);

            if (target == null ||
                distance >
                    PreferredMaximumRangeM)
            {
                input.Throttle = MathF.Abs(hullDelta) > 1.2f ? 0.3f : 1f;
            }
            else if (distance < PreferredMinimumRangeM)
            {
                input.Throttle = -0.45f;
            }
            else
            {
                input.Brake = true;
            }

            input.Fire =
                target != null &&
                combatDistance <= FireRangeM &&
                MathF.Abs(gunDelta) <= FireAlignmentRad &&
                bot.ReloadRemainingS <= 0f &&
                !_isOccludedOrFalse(bot, target);
            return input;
        }

        public TankState SelectTarget(TankState bot, IReadOnlyList<TankState> tanks)
        {
            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (tanks == null || bot.Destroyed)
            {
                return null;
            }

            TankState best = null;
            float bestDistanceSquared = float.MaxValue;
            for (int i = 0; i < tanks.Count; i++)
            {
                TankState candidate = tanks[i];
                if (candidate == null || candidate.Destroyed || candidate.Team == bot.Team ||
                    !_spotting.CanSpot(
                        bot,
                        candidate,
                        _isOccluded,
                        _battle?.TimeS ?? 0f,
                        _battle != null
                            ? _battle
                                .ConcealmentBonusBetween(
                                    bot.Position,
                                    candidate.Position,
                                    SpottingSimulation
                                        .FireBloomAt(
                                            candidate,
                                            _battle.TimeS) >
                                        0f)
                            : 0f))
                {
                    continue;
                }

                float distanceSquared = (candidate.Position - bot.Position).SqrMagnitude;
                if (distanceSquared < bestDistanceSquared ||
                    (distanceSquared.Equals(bestDistanceSquared) &&
                     string.CompareOrdinal(candidate.Id, best.Id) < 0))
                {
                    best = candidate;
                    bestDistanceSquared = distanceSquared;
                }
            }

            return best;
        }

        private bool _isOccludedOrFalse(TankState bot, TankState target)
        {
            if (_isOccluded == null)
            {
                return false;
            }

            Float3 origin = bot.Position + new Float3(0f, 1.65f, 0f);
            Float3 destination = target.Position + new Float3(0f, AimHeightM, 0f);
            return _isOccluded(origin, destination);
        }
    }
}
