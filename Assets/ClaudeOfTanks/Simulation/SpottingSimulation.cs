using System;

namespace ClaudeOfTanks.Simulation
{
    public sealed class SpottingSimulation
    {
        public const float DefaultViewRangeM = 445f;
        public const float DefaultFieldOfViewDeg = 120f;
        public const float ProximitySpotRangeM = 50f;

        private const float EyeHeightM = 1.65f;
        private const float TargetHeightM = 1.25f;
        private readonly float _viewRangeM;
        private readonly float _viewCosine;

        public SpottingSimulation(
            float viewRangeM = DefaultViewRangeM,
            float fieldOfViewDeg = DefaultFieldOfViewDeg)
        {
            if (viewRangeM < ProximitySpotRangeM)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viewRangeM),
                    "View range cannot be shorter than proximity spotting range.");
            }

            if (fieldOfViewDeg <= 0f || fieldOfViewDeg > 360f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fieldOfViewDeg),
                    "Field of view must be in the range (0, 360].");
            }

            _viewRangeM = viewRangeM;
            _viewCosine = fieldOfViewDeg >= 360f
                ? -1f
                : MathF.Cos(fieldOfViewDeg * 0.5f * MathUtil.Deg2Rad);
        }

        public float ViewRangeM => _viewRangeM;

        public bool CanSpot(TankState spotter, TankState target)
        {
            return CanSpot(spotter, target, null);
        }

        public bool CanSpot(
            TankState spotter,
            TankState target,
            Func<Float3, Float3, bool> isOccluded)
        {
            if (spotter == null)
            {
                throw new ArgumentNullException(nameof(spotter));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (spotter.Destroyed || target.Destroyed ||
                spotter.Team == target.Team || spotter.Id == target.Id)
            {
                return false;
            }

            Float3 offset = target.Position - spotter.Position;
            float distanceSquared = offset.SqrMagnitude;
            if (distanceSquared <= ProximitySpotRangeM * ProximitySpotRangeM)
            {
                return true;
            }

            if (distanceSquared > _viewRangeM * _viewRangeM)
            {
                return false;
            }

            float horizontalSquared = offset.X * offset.X + offset.Z * offset.Z;
            if (horizontalSquared > 0.000001f)
            {
                Float3 forward = Float3.Forward(spotter.Yaw);
                float inverseDistance = 1f / MathF.Sqrt(horizontalSquared);
                float facing = (forward.X * offset.X + forward.Z * offset.Z) * inverseDistance;
                if (facing < _viewCosine)
                {
                    return false;
                }
            }

            if (isOccluded == null)
            {
                return true;
            }

            Float3 origin = spotter.Position + new Float3(0f, EyeHeightM, 0f);
            Float3 destination = target.Position + new Float3(0f, TargetHeightM, 0f);
            return !isOccluded(origin, destination);
        }
    }
}
