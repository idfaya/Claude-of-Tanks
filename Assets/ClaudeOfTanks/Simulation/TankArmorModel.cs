using System;

namespace ClaudeOfTanks.Simulation
{
    public sealed class TankArmorModel
    {
        public Float3 TurretPivot;
        public Float3 GunPivot;
        public float GunBarrelLengthM;
        public float GunBarrelRadiusM;
        public bool Turretless;
        public float BoundingRadiusM;
        public ArmorPlateModel[] HullPlates = Array.Empty<ArmorPlateModel>();
        public ArmorPlateModel[] TurretPlates = Array.Empty<ArmorPlateModel>();
        public ArmorVolumeModel[] Modules = Array.Empty<ArmorVolumeModel>();
        public ArmorVolumeModel[] Crew = Array.Empty<ArmorVolumeModel>();
    }

    public sealed class ArmorPlateModel
    {
        public string Name;
        public string Kind;
        public float PhysicalMm;
        public float KeMm;
        public float CeMm;
        public float EraKeReduction;
        public float EraCeFlatMm;
        public string ModuleLink;
        public bool GunFollow;
        public Float3[] Vertices = Array.Empty<Float3>();
    }

    public sealed class ArmorVolumeModel
    {
        public string Id;
        public bool TurretLocal;
        public bool External;
        public Float3 Minimum;
        public Float3 Maximum;
        public ArmorVolumeShapeModel[] Shapes =
            Array.Empty<ArmorVolumeShapeModel>();
    }

    public sealed class ArmorVolumeShapeModel
    {
        public string Kind;
        public Float3 Center;
        public Float3 Radii;
        public Float3 A;
        public Float3 B;
        public float Radius;
        public float RadiusA;
        public float RadiusB;
        public int Axis;
        public float HalfLength;
    }

    public struct ArmorPlateTrace
    {
        public float Fraction;
        public ArmorPlateModel Plate;
        public Float3 Point;
        public Float3 Normal;
    }

    public struct ArmorVolumeTrace
    {
        public float Fraction;
        public float ExitFraction;
        public ArmorVolumeModel Volume;
    }

    public static class TankArmorTrace
    {
        private const float Epsilon = 0.000001f;

        public static int TracePlates(
            Float3 start,
            Float3 end,
            TankState tank,
            ArmorPlateTrace[] output)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            if (output == null) throw new ArgumentNullException(nameof(output));
            TankArmorModel armor = tank.Spec.Armor;
            if (armor == null) return 0;
            int count = 0;
            count = TracePlateSet(
                start,
                end,
                tank,
                armor.HullPlates,
                false,
                output,
                count);
            count = TracePlateSet(
                start,
                end,
                tank,
                armor.TurretPlates,
                true,
                output,
                count);
            InsertionSort(output, count);
            return count;
        }

        public static int TraceVolumes(
            Float3 start,
            Float3 end,
            TankState tank,
            bool crew,
            ArmorVolumeTrace[] output)
        {
            if (tank == null) throw new ArgumentNullException(nameof(tank));
            if (output == null) throw new ArgumentNullException(nameof(output));
            TankArmorModel armor = tank.Spec.Armor;
            if (armor == null) return 0;
            ArmorVolumeModel[] volumes = crew ? armor.Crew : armor.Modules;
            int count = 0;
            for (int i = 0; i < volumes.Length && count < output.Length; i++)
            {
                ArmorVolumeModel volume = volumes[i];
                Float3 localStart = ToLocal(
                    start,
                    tank,
                    volume.TurretLocal,
                    false);
                Float3 localEnd = ToLocal(
                    end,
                    tank,
                    volume.TurretLocal,
                    false);
                float enter;
                float exit;
                if (!ArmorVolumeIntersection.TryIntersect(
                        localStart,
                        localEnd,
                        volume,
                        out enter,
                        out exit))
                {
                    continue;
                }
                output[count++] = new ArmorVolumeTrace
                {
                    Fraction = enter,
                    ExitFraction = exit,
                    Volume = volume
                };
            }
            InsertionSort(output, count);
            return count;
        }

        private static int TracePlateSet(
            Float3 start,
            Float3 end,
            TankState tank,
            ArmorPlateModel[] plates,
            bool turretLocal,
            ArmorPlateTrace[] output,
            int count)
        {
            for (int i = 0; i < plates.Length && count < output.Length; i++)
            {
                ArmorPlateModel plate = plates[i];
                if (plate == null ||
                    (string.Equals(
                        plate.Kind,
                        "era",
                        StringComparison.OrdinalIgnoreCase) &&
                     tank.Combat.EraSpent.Contains(
                         plate.Name ?? string.Empty)) ||
                    plate.Vertices == null ||
                    plate.Vertices.Length < 3 ||
                    plate.PhysicalMm <= 0f ||
                    plate.KeMm <= 0f ||
                    plate.CeMm <= 0f)
                {
                    continue;
                }
                Float3 localStart = ToLocal(
                    start,
                    tank,
                    turretLocal,
                    plate.GunFollow);
                Float3 localEnd = ToLocal(
                    end,
                    tank,
                    turretLocal,
                    plate.GunFollow);
                Float3 direction =
                    localEnd -
                    localStart;
                float fraction;
                Float3 localNormal;
                if (!IntersectPolygon(
                        localStart,
                        direction,
                        plate.Vertices,
                        out fraction,
                        out localNormal))
                {
                    continue;
                }
                Float3 worldNormal = ToWorldDirection(
                    localNormal,
                    tank,
                    turretLocal,
                    plate.GunFollow);
                output[count++] = new ArmorPlateTrace
                {
                    Fraction = fraction,
                    Plate = plate,
                    Point = start + (end - start) * fraction,
                    Normal = worldNormal
                };
            }
            return count;
        }

        private static bool IntersectPolygon(
            Float3 start,
            Float3 direction,
            Float3[] vertices,
            out float fraction,
            out Float3 normal)
        {
            normal = Float3.Cross(
                vertices[1] - vertices[0],
                vertices[2] - vertices[0]).Normalized;
            float denominator = Float3.Dot(normal, direction);
            if (MathF.Abs(denominator) <= Epsilon)
            {
                fraction = 0f;
                return false;
            }
            fraction = Float3.Dot(
                normal,
                vertices[0] - start) / denominator;
            if (fraction < 0f || fraction > 1f) return false;
            Float3 point = start + direction * fraction;
            bool positive = false;
            bool negative = false;
            for (int i = 0; i < vertices.Length; i++)
            {
                Float3 edge =
                    vertices[(i + 1) % vertices.Length] -
                    vertices[i];
                float side = Float3.Dot(
                    Float3.Cross(edge, point - vertices[i]),
                    normal);
                positive |= side > Epsilon;
                negative |= side < -Epsilon;
                if (positive && negative) return false;
            }
            if (denominator > 0f) normal = normal * -1f;
            return true;
        }

        private static bool IntersectAabb(
            Float3 start,
            Float3 end,
            Float3 minimum,
            Float3 maximum,
            out float enter,
            out float exit)
        {
            Float3 direction = end - start;
            enter = 0f;
            exit = 1f;
            return Clip(start.X, direction.X, minimum.X, maximum.X, ref enter, ref exit) &&
                Clip(start.Y, direction.Y, minimum.Y, maximum.Y, ref enter, ref exit) &&
                Clip(start.Z, direction.Z, minimum.Z, maximum.Z, ref enter, ref exit);
        }

        private static bool Clip(
            float start,
            float direction,
            float minimum,
            float maximum,
            ref float enter,
            ref float exit)
        {
            if (MathF.Abs(direction) <= Epsilon)
                return start >= minimum && start <= maximum;
            float first = (minimum - start) / direction;
            float second = (maximum - start) / direction;
            if (first > second)
            {
                float swap = first;
                first = second;
                second = swap;
            }
            if (first > enter) enter = first;
            if (second < exit) exit = second;
            return enter <= exit && exit >= 0f && enter <= 1f;
        }

        private static Float3 ToLocal(
            Float3 point,
            TankState tank,
            bool turretLocal,
            bool gunFollow)
        {
            Float3 value = point - tank.Position;
            value = TankPoseMath.RotateYaw(
                value,
                -tank.Yaw);
            value = TankPoseMath.RotatePitch(
                value,
                TankPoseMath.VisualPitchRad(
                    tank));
            value = TankPoseMath.RotateRoll(
                value,
                -tank.HullRollRad);
            if (!turretLocal) return value;
            value -= tank.Spec.Armor.TurretPivot;
            value = TankPoseMath.RotateYaw(
                value,
                -tank.TurretYaw);
            if (gunFollow)
            {
                value -= tank.Spec.Armor.GunPivot;
                value = TankPoseMath.RotatePitch(
                    value,
                    tank.GunPitchRad);
                value += tank.Spec.Armor.GunPivot;
            }
            return value;
        }

        private static Float3 ToWorldDirection(
            Float3 direction,
            TankState tank,
            bool turretLocal,
            bool gunFollow)
        {
            Float3 value = direction;
            if (gunFollow)
            {
                value = TankPoseMath.RotatePitch(
                    value,
                    -tank.GunPitchRad);
            }
            if (turretLocal)
            {
                value = TankPoseMath.RotateYaw(
                    value,
                    tank.TurretYaw);
            }
            value = TankPoseMath.RotateRoll(
                value,
                tank.HullRollRad);
            value = TankPoseMath.RotatePitch(
                value,
                -TankPoseMath.VisualPitchRad(
                    tank));
            return TankPoseMath.RotateYaw(
                value,
                tank.Yaw).Normalized;
        }

        private static void InsertionSort(
            ArmorPlateTrace[] values,
            int count)
        {
            for (int i = 1; i < count; i++)
            {
                ArmorPlateTrace value = values[i];
                int previous = i - 1;
                while (previous >= 0 &&
                    values[previous].Fraction > value.Fraction)
                {
                    values[previous + 1] = values[previous];
                    previous--;
                }
                values[previous + 1] = value;
            }
        }

        private static void InsertionSort(
            ArmorVolumeTrace[] values,
            int count)
        {
            for (int i = 1; i < count; i++)
            {
                ArmorVolumeTrace value = values[i];
                int previous = i - 1;
                while (previous >= 0 &&
                    values[previous].Fraction > value.Fraction)
                {
                    values[previous + 1] = values[previous];
                    previous--;
                }
                values[previous + 1] = value;
            }
        }
    }
}
