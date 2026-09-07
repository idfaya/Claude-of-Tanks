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

        public BotController(
            SpottingSimulation spotting,
            Func<Float3, Float3, bool> isOccluded = null)
        {
            _spotting = spotting ?? throw new ArgumentNullException(nameof(spotting));
            _isOccluded = isOccluded;
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
            if (target == null)
            {
                return input;
            }

            Float3 offset = target.Position - bot.Position;
            float distance = offset.Magnitude;
            float desiredYaw = MathF.Atan2(offset.X, offset.Z);
            float hullDelta = MathUtil.DeltaAngle(bot.Yaw, desiredYaw);
            float gunDelta = MathUtil.DeltaAngle(bot.Yaw + bot.TurretYaw, desiredYaw);

            input.AimPoint = target.Position + new Float3(0f, AimHeightM, 0f);
            input.Steer = MathUtil.Clamp(hullDelta * 2.2f, -1f, 1f);

            if (distance > PreferredMaximumRangeM)
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
                distance <= FireRangeM &&
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
                    !_spotting.CanSpot(bot, candidate, _isOccluded))
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
