using System;

namespace ClaudeOfTanks.Simulation
{
    public sealed class SpottingSimulation
    {
        public const float DefaultViewRangeM = 445f;
        public const float DefaultFieldOfViewDeg = 120f;
        public const float ProximitySpotRangeM = 50f;
        public const float MovingSpeedMps = 0.4f;

        private const float EyeHeightM = 1.65f;
        private const float TargetHeightM = 1.25f;
        private const float DamagedOpticsViewFactor = 0.5f;
        private readonly float _maximumSpotRangeM;
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

            _maximumSpotRangeM = viewRangeM;
            _viewCosine = fieldOfViewDeg >= 360f
                ? -1f
                : MathF.Cos(fieldOfViewDeg * 0.5f * MathUtil.Deg2Rad);
        }

        public float ViewRangeM => _maximumSpotRangeM;

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

            if (distanceSquared >
                _maximumSpotRangeM * _maximumSpotRangeM)
            {
                return false;
            }

            float viewRange = EffectiveViewRangeM(spotter);
            float targetCamouflage = EffectiveCamouflage(target);
            float spotRange = MathUtil.Clamp(
                viewRange -
                    (viewRange - ProximitySpotRangeM) *
                    targetCamouflage,
                ProximitySpotRangeM,
                _maximumSpotRangeM);
            if (distanceSquared > spotRange * spotRange)
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

        public static float EffectiveViewRangeM(TankState tank)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            bool moving = MathF.Abs(tank.SpeedMps) > MovingSpeedMps;
            float multiplier = tank.Combat.Equipment.ViewRange;
            if (!moving)
            {
                multiplier *=
                    tank.Combat.Equipment.StationaryViewRange;
            }

            DamageModuleState optics;
            if (tank.Combat.Modules.TryGetValue(
                    "optics",
                    out optics) &&
                optics.Condition != DamageModuleCondition.Ok)
            {
                multiplier *= DamagedOpticsViewFactor;
            }
            return MathF.Max(
                ProximitySpotRangeM,
                tank.Spec.ViewRangeM * multiplier);
        }

        public bool CanSpotMuzzleFlash(
            TankState spotter,
            TankState shooter,
            Func<Float3, Float3, bool> isOccluded)
        {
            if (spotter == null)
                throw new ArgumentNullException(nameof(spotter));
            if (shooter == null)
                throw new ArgumentNullException(nameof(shooter));
            if (spotter.Destroyed ||
                shooter.Destroyed ||
                spotter.Team == shooter.Team)
            {
                return false;
            }
            Float3 origin =
                spotter.Position +
                new Float3(0f, EyeHeightM, 0f);
            Float3 destination =
                shooter.Position +
                new Float3(0f, TargetHeightM, 0f);
            Float3 offset = destination - origin;
            float range = MathF.Min(
                _maximumSpotRangeM,
                EffectiveViewRangeM(spotter));
            return offset.SqrMagnitude <=
                    range * range &&
                (isOccluded == null ||
                 !isOccluded(origin, destination));
        }

        public static float EffectiveCamouflage(TankState tank)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            bool moving = MathF.Abs(tank.SpeedMps) > MovingSpeedMps;
            float camouflage = moving
                ? tank.Spec.CamouflageMoving
                : tank.Spec.CamouflageStill;
            camouflage += tank.Combat.Equipment.Camouflage;
            if (!moving)
            {
                camouflage +=
                    tank.Combat.Equipment.StationaryCamouflage;
            }
            return MathUtil.Clamp(camouflage, 0f, 0.95f);
        }

        public static float BaseViewRangeM(string id, string role)
        {
            switch (id)
            {
                case "m4a3e8": return 370f;
                case "tiger1": return 370f;
                case "t34_85": return 360f;
                case "is2": return 350f;
                case "panther_g": return 380f;
                case "m1a2": return 445f;
                case "t90m": return 430f;
                case "leo2a7": return 445f;
            }
            switch (role)
            {
                case "light": return 390f;
                case "heavy": return 360f;
                case "mbt": return 440f;
                case "td": return 370f;
                case "spg": return 340f;
                default: return 370f;
            }
        }

        public static float BaseCamouflage(
            string id,
            string role,
            bool moving)
        {
            float still;
            float mobile;
            switch (id)
            {
                case "m4a3e8": still = 0.24f; mobile = 0.18f; break;
                case "tiger1": still = 0.11f; mobile = 0.07f; break;
                case "t34_85": still = 0.26f; mobile = 0.20f; break;
                case "is2": still = 0.12f; mobile = 0.08f; break;
                case "panther_g": still = 0.20f; mobile = 0.15f; break;
                case "m1a2": still = 0.17f; mobile = 0.12f; break;
                case "t90m": still = 0.21f; mobile = 0.16f; break;
                case "leo2a7": still = 0.18f; mobile = 0.13f; break;
                case null:
                default:
                    switch (role)
                    {
                        case "light": still = 0.34f; mobile = 0.34f; break;
                        case "heavy": still = 0.12f; mobile = 0.08f; break;
                        case "mbt": still = 0.18f; mobile = 0.13f; break;
                        case "td": still = 0.30f; mobile = 0.18f; break;
                        case "spg": still = 0.08f; mobile = 0.05f; break;
                        default: still = 0.23f; mobile = 0.17f; break;
                    }
                    break;
            }
            return moving ? mobile : still;
        }
    }
}
