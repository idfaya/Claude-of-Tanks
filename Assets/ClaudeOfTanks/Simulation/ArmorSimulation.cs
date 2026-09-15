using System;

namespace ClaudeOfTanks.Simulation
{
    public enum ArmorDirection
    {
        Front,
        Side,
        Rear
    }

    public enum ArmorShellType
    {
        AP,
        APCR,
        APFSDS,
        HEAT,
        HE,
        HESH
    }

    public struct ArmorPlateSpec
    {
        public float PhysicalMm;
        public float KeMm;
        public float CeMm;

        public ArmorPlateSpec(float physicalMm, float keMm, float ceMm)
        {
            PhysicalMm = physicalMm;
            KeMm = keMm;
            CeMm = ceMm;
        }

        public ArmorPlateSpec(float thicknessMm)
            : this(thicknessMm, thicknessMm, thicknessMm)
        {
        }
    }

    public struct ArmorHitResult
    {
        public ArmorDirection Direction;
        public float ImpactAngleDeg;
        public float EffectiveAngleDeg;
        public float EffectiveThicknessMm;
        public float PenetrationRollMm;
        public bool Ricocheted;
        public bool Penetrated;
    }

    public static class ArmorSimulation
    {
        private const float FrontRearDotThreshold = 0.62f;
        private const float TwoCaliberOvermatch = 2f;
        private const float ThreeCaliberOvermatch = 3f;
        private const float MaximumEffectiveAngleDeg = 89f;

        private enum ShellClass
        {
            Kinetic,
            Chemical,
            HighExplosive
        }

        private struct ShellBehavior
        {
            public ShellClass Class;
            public float NormalizationDeg;
            public float RicochetDeg;
            public float SlopeExponent;

            public ShellBehavior(
                ShellClass shellClass,
                float normalizationDeg,
                float ricochetDeg,
                float slopeExponent)
            {
                Class = shellClass;
                NormalizationDeg = normalizationDeg;
                RicochetDeg = ricochetDeg;
                SlopeExponent = slopeExponent;
            }
        }

        public static ArmorDirection DirectionFromHit(Float3 shellDirection, Float3 targetForward)
        {
            Float3 travel = RequireDirection(shellDirection, nameof(shellDirection));
            Float3 forward = RequireDirection(targetForward, nameof(targetForward));
            float facing = Float3.Dot(travel, forward);
            if (facing < -FrontRearDotThreshold)
            {
                return ArmorDirection.Front;
            }

            if (facing > FrontRearDotThreshold)
            {
                return ArmorDirection.Rear;
            }

            return ArmorDirection.Side;
        }

        public static float SelectDirectionalArmorMm(
            ArmorDirection direction,
            float frontMm,
            float sideMm,
            float rearMm)
        {
            switch (direction)
            {
                case ArmorDirection.Front:
                    return frontMm;
                case ArmorDirection.Side:
                    return sideMm;
                case ArmorDirection.Rear:
                    return rearMm;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        public static float ImpactAngleDegrees(Float3 shellDirection, Float3 outwardNormal)
        {
            Float3 travel = RequireDirection(shellDirection, nameof(shellDirection));
            Float3 normal = RequireDirection(outwardNormal, nameof(outwardNormal));
            float incidence = MathUtil.Clamp(-Float3.Dot(travel, normal), 0f, 1f);
            return MathF.Acos(incidence) / MathUtil.Deg2Rad;
        }

        public static float EffectiveOvermatchCaliberMm(
            ArmorShellType shellType,
            float caliberMm,
            float effectiveOvermatchCaliberMm = 0f)
        {
            RequirePositive(caliberMm, nameof(caliberMm));
            GetBehavior(shellType);
            if (effectiveOvermatchCaliberMm > 0f)
            {
                return effectiveOvermatchCaliberMm;
            }

            return shellType == ArmorShellType.APFSDS ? caliberMm * 0.6f : caliberMm;
        }

        public static float ApplyEraPenetration(
            ArmorShellType shellType,
            float penetrationMm,
            float kineticReduction,
            float chemicalFlatReductionMm,
            bool tandem)
        {
            if (penetrationMm <= 0f || tandem)
                return MathF.Max(0f, penetrationMm);
            ShellBehavior behavior = GetBehavior(shellType);
            if (behavior.Class == ShellClass.Chemical)
            {
                return MathF.Max(
                    0f,
                    penetrationMm -
                    MathF.Max(
                        0f,
                        chemicalFlatReductionMm));
            }
            if (behavior.Class == ShellClass.Kinetic)
            {
                return penetrationMm *
                    (1f -
                     MathUtil.Clamp(
                         kineticReduction,
                         0f,
                         1f));
            }
            return penetrationMm;
        }

        public static float NormalizationDegrees(
            ArmorShellType shellType,
            float caliberMm,
            float physicalThicknessMm,
            float effectiveOvermatchCaliberMm = 0f)
        {
            RequirePositive(physicalThicknessMm, nameof(physicalThicknessMm));
            ShellBehavior behavior = GetBehavior(shellType);
            float normalization = behavior.NormalizationDeg;
            float overmatchCaliber = EffectiveOvermatchCaliberMm(
                shellType, caliberMm, effectiveOvermatchCaliberMm);
            if (behavior.Class == ShellClass.Kinetic &&
                overmatchCaliber >= TwoCaliberOvermatch * physicalThicknessMm)
            {
                normalization *= 1.4f * overmatchCaliber / physicalThicknessMm;
            }

            return normalization;
        }

        public static bool WouldRicochet(
            ArmorShellType shellType,
            float caliberMm,
            float physicalThicknessMm,
            float impactAngleDeg,
            float effectiveOvermatchCaliberMm = 0f)
        {
            RequirePositive(physicalThicknessMm, nameof(physicalThicknessMm));
            RequireAngle(impactAngleDeg);
            ShellBehavior behavior = GetBehavior(shellType);
            if (behavior.Class == ShellClass.HighExplosive)
            {
                return false;
            }

            float overmatchCaliber = EffectiveOvermatchCaliberMm(
                shellType, caliberMm, effectiveOvermatchCaliberMm);
            if (behavior.Class == ShellClass.Kinetic &&
                overmatchCaliber >= ThreeCaliberOvermatch * physicalThicknessMm)
            {
                return false;
            }

            return impactAngleDeg > behavior.RicochetDeg;
        }

        public static float EffectiveThicknessMm(
            ArmorShellType shellType,
            float caliberMm,
            ArmorPlateSpec plate,
            float impactAngleDeg,
            out float effectiveAngleDeg,
            float effectiveOvermatchCaliberMm = 0f)
        {
            ValidatePlate(plate);
            RequireAngle(impactAngleDeg);
            ShellBehavior behavior = GetBehavior(shellType);
            float normalization = NormalizationDegrees(
                shellType, caliberMm, plate.PhysicalMm, effectiveOvermatchCaliberMm);
            effectiveAngleDeg = MathF.Max(0f, impactAngleDeg - normalization);
            float clampedAngle = MathF.Min(effectiveAngleDeg, MaximumEffectiveAngleDeg);
            float baseThickness = behavior.Class == ShellClass.Kinetic ? plate.KeMm : plate.CeMm;
            float cosine = MathF.Cos(clampedAngle * MathUtil.Deg2Rad);
            return baseThickness / MathF.Pow(cosine, behavior.SlopeExponent);
        }

        public static float PenetrationAtDistanceMm(ShellSpec shell, float distanceM)
        {
            if (shell == null)
            {
                throw new ArgumentNullException(nameof(shell));
            }

            if (distanceM < 0f || !IsFinite(distanceM))
            {
                throw new ArgumentOutOfRangeException(nameof(distanceM));
            }

            return BallisticsSimulation.PenetrationAtDistance(shell, distanceM);
        }

        public static float RollPenetrationMm(
            ShellSpec shell,
            float distanceM,
            DeterministicRandom random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            return PenetrationAtDistanceMm(shell, distanceM) * random.Range(0.75f, 1.25f);
        }

        public static ArmorHitResult ResolveHit(
            ShellSpec shell,
            ArmorShellType shellType,
            float caliberMm,
            ArmorPlateSpec plate,
            Float3 shellDirection,
            Float3 outwardNormal,
            Float3 targetForward,
            float distanceM,
            DeterministicRandom random,
            float effectiveOvermatchCaliberMm = 0f)
        {
            ArmorDirection direction = DirectionFromHit(shellDirection, targetForward);
            float impactAngle = ImpactAngleDegrees(shellDirection, outwardNormal);
            float penetrationRoll = RollPenetrationMm(shell, distanceM, random);
            bool ricocheted = WouldRicochet(
                shellType,
                caliberMm,
                plate.PhysicalMm,
                impactAngle,
                effectiveOvermatchCaliberMm);

            float effectiveAngle = impactAngle;
            float effectiveThickness = 0f;
            if (!ricocheted)
            {
                effectiveThickness = EffectiveThicknessMm(
                    shellType,
                    caliberMm,
                    plate,
                    impactAngle,
                    out effectiveAngle,
                    effectiveOvermatchCaliberMm);
            }

            return new ArmorHitResult
            {
                Direction = direction,
                ImpactAngleDeg = impactAngle,
                EffectiveAngleDeg = effectiveAngle,
                EffectiveThicknessMm = effectiveThickness,
                PenetrationRollMm = penetrationRoll,
                Ricocheted = ricocheted,
                Penetrated = !ricocheted && penetrationRoll >= effectiveThickness
            };
        }

        private static ShellBehavior GetBehavior(ArmorShellType shellType)
        {
            switch (shellType)
            {
                case ArmorShellType.AP:
                    return new ShellBehavior(ShellClass.Kinetic, 5f, 70f, 1.4f);
                case ArmorShellType.APCR:
                    return new ShellBehavior(ShellClass.Kinetic, 2f, 70f, 1.4f);
                case ArmorShellType.APFSDS:
                    return new ShellBehavior(ShellClass.Kinetic, 2f, 78f, 1f);
                case ArmorShellType.HEAT:
                    return new ShellBehavior(ShellClass.Chemical, 0f, 85f, 1f);
                case ArmorShellType.HE:
                case ArmorShellType.HESH:
                    return new ShellBehavior(ShellClass.HighExplosive, 0f, float.PositiveInfinity, 1f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(shellType));
            }
        }

        private static Float3 RequireDirection(Float3 value, string parameterName)
        {
            float magnitude = value.Magnitude;
            if (magnitude <= 0.000001f || !IsFinite(magnitude))
            {
                throw new ArgumentException("Direction must be finite and non-zero.", parameterName);
            }

            return value / magnitude;
        }

        private static void ValidatePlate(ArmorPlateSpec plate)
        {
            RequirePositive(plate.PhysicalMm, nameof(plate.PhysicalMm));
            RequirePositive(plate.KeMm, nameof(plate.KeMm));
            RequirePositive(plate.CeMm, nameof(plate.CeMm));
        }

        private static void RequirePositive(float value, string parameterName)
        {
            if (value <= 0f || !IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and positive.");
            }
        }

        private static void RequireAngle(float impactAngleDeg)
        {
            if (impactAngleDeg < 0f || impactAngleDeg > 90f || !IsFinite(impactAngleDeg))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(impactAngleDeg), "Impact angle must be between 0 and 90 degrees.");
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
