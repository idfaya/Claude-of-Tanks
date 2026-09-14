using System;

namespace ClaudeOfTanks.Simulation
{
    internal static class ArmorVolumeIntersection
    {
        private const float Epsilon = 0.000001f;

        public static bool TryIntersect(
            Float3 start,
            Float3 end,
            ArmorVolumeModel volume,
            out float enter,
            out float exit)
        {
            ArmorVolumeShapeModel[] shapes =
                volume.Shapes;
            if (shapes == null || shapes.Length == 0)
                return IntersectAabb(
                    start,
                    end,
                    volume.Minimum,
                    volume.Maximum,
                    out enter,
                    out exit);
            enter = float.MaxValue;
            exit = float.MinValue;
            bool found = false;
            for (int i = 0; i < shapes.Length; i++)
            {
                float shapeEnter;
                float shapeExit;
                if (!IntersectShape(
                        start,
                        end,
                        shapes[i],
                        out shapeEnter,
                        out shapeExit))
                {
                    continue;
                }
                found = true;
                if (shapeEnter < enter) enter = shapeEnter;
                if (shapeExit > exit) exit = shapeExit;
            }
            return found;
        }

        private static bool IntersectShape(
            Float3 start,
            Float3 end,
            ArmorVolumeShapeModel shape,
            out float enter,
            out float exit)
        {
            if (shape == null)
            {
                enter = exit = 0f;
                return false;
            }
            if (shape.Kind == "ellipsoid")
                return IntersectEllipsoid(
                    start,
                    end,
                    shape.Center,
                    shape.Radii,
                    out enter,
                    out exit);
            if (shape.Kind == "capsule")
                return IntersectCapsule(
                    start,
                    end,
                    shape.A,
                    shape.B,
                    shape.Radius,
                    out enter,
                    out exit);
            if (shape.Kind == "ellipticCylinder")
                return IntersectEllipticCylinder(
                    start,
                    end,
                    shape,
                    out enter,
                    out exit);
            enter = exit = 0f;
            return false;
        }

        private static bool IntersectEllipsoid(
            Float3 start,
            Float3 end,
            Float3 center,
            Float3 radii,
            out float enter,
            out float exit)
        {
            if (radii.X <= Epsilon ||
                radii.Y <= Epsilon ||
                radii.Z <= Epsilon)
            {
                enter = exit = 0f;
                return false;
            }
            Float3 origin = new Float3(
                (start.X - center.X) / radii.X,
                (start.Y - center.Y) / radii.Y,
                (start.Z - center.Z) / radii.Z);
            Float3 delta = end - start;
            Float3 direction = new Float3(
                delta.X / radii.X,
                delta.Y / radii.Y,
                delta.Z / radii.Z);
            return IntersectQuadratic(
                Float3.Dot(direction, direction),
                2f * Float3.Dot(origin, direction),
                Float3.Dot(origin, origin) - 1f,
                out enter,
                out exit);
        }

        private static bool IntersectCapsule(
            Float3 start,
            Float3 end,
            Float3 a,
            Float3 b,
            float radius,
            out float enter,
            out float exit)
        {
            Float3 axis = b - a;
            float length = axis.Magnitude;
            if (radius <= Epsilon ||
                length <= Epsilon)
            {
                return IntersectSphere(
                    start,
                    end,
                    a,
                    radius,
                    out enter,
                    out exit);
            }
            Float3 unit = axis / length;
            Float3 direction = end - start;
            Float3 offset = start - a;
            float axialOrigin =
                Float3.Dot(offset, unit);
            float axialDirection =
                Float3.Dot(direction, unit);
            Float3 radialOrigin =
                offset - unit * axialOrigin;
            Float3 radialDirection =
                direction - unit * axialDirection;
            float radialEnter;
            float radialExit;
            bool radial = IntersectQuadratic(
                Float3.Dot(
                    radialDirection,
                    radialDirection),
                2f * Float3.Dot(
                    radialOrigin,
                    radialDirection),
                Float3.Dot(
                    radialOrigin,
                    radialOrigin) -
                    radius * radius,
                out radialEnter,
                out radialExit);
            bool found = false;
            enter = float.MaxValue;
            exit = float.MinValue;
            if (radial)
            {
                IncludeCylinderRoot(
                    radialEnter,
                    axialOrigin,
                    axialDirection,
                    length,
                    ref found,
                    ref enter,
                    ref exit);
                IncludeCylinderRoot(
                    radialExit,
                    axialOrigin,
                    axialDirection,
                    length,
                    ref found,
                    ref enter,
                    ref exit);
            }
            IncludeSphere(
                start,
                end,
                a,
                radius,
                ref found,
                ref enter,
                ref exit);
            IncludeSphere(
                start,
                end,
                b,
                radius,
                ref found,
                ref enter,
                ref exit);
            return found;
        }

        private static bool IntersectEllipticCylinder(
            Float3 start,
            Float3 end,
            ArmorVolumeShapeModel shape,
            out float enter,
            out float exit)
        {
            int axis = shape.Axis;
            int radialA = axis == 0 ? 1 : 0;
            int radialB = axis == 2 ? 1 : 2;
            if (axis == 1) radialB = 2;
            Float3 direction = end - start;
            if (shape.RadiusA <= Epsilon ||
                shape.RadiusB <= Epsilon ||
                shape.HalfLength <= Epsilon)
            {
                enter = exit = 0f;
                return false;
            }
            float originA =
                (Component(start, radialA) -
                 Component(shape.Center, radialA)) /
                shape.RadiusA;
            float originB =
                (Component(start, radialB) -
                 Component(shape.Center, radialB)) /
                shape.RadiusB;
            float directionA =
                Component(direction, radialA) /
                shape.RadiusA;
            float directionB =
                Component(direction, radialB) /
                shape.RadiusB;
            float quadraticA =
                directionA * directionA +
                directionB * directionB;
            float quadraticC =
                originA * originA +
                originB * originB - 1f;
            if (quadraticA <= Epsilon)
            {
                if (quadraticC > 0f)
                {
                    enter = exit = 0f;
                    return false;
                }
                enter = 0f;
                exit = 1f;
            }
            else if (!IntersectQuadratic(
                quadraticA,
                2f * (originA * directionA +
                    originB * directionB),
                quadraticC,
                out enter,
                out exit))
            {
                return false;
            }
            float axisStart =
                Component(start, axis) -
                Component(shape.Center, axis);
            float axisDirection =
                Component(direction, axis);
            return Clip(
                axisStart,
                axisDirection,
                -shape.HalfLength,
                shape.HalfLength,
                ref enter,
                ref exit);
        }

        private static bool IntersectSphere(
            Float3 start,
            Float3 end,
            Float3 center,
            float radius,
            out float enter,
            out float exit)
        {
            Float3 direction = end - start;
            Float3 origin = start - center;
            return IntersectQuadratic(
                Float3.Dot(direction, direction),
                2f * Float3.Dot(origin, direction),
                Float3.Dot(origin, origin) -
                    radius * radius,
                out enter,
                out exit);
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
            return Clip(
                    start.X,
                    direction.X,
                    minimum.X,
                    maximum.X,
                    ref enter,
                    ref exit) &&
                Clip(
                    start.Y,
                    direction.Y,
                    minimum.Y,
                    maximum.Y,
                    ref enter,
                    ref exit) &&
                Clip(
                    start.Z,
                    direction.Z,
                    minimum.Z,
                    maximum.Z,
                    ref enter,
                    ref exit);
        }

        private static bool IntersectQuadratic(
            float a,
            float b,
            float c,
            out float enter,
            out float exit)
        {
            float discriminant =
                b * b - 4f * a * c;
            if (a <= Epsilon ||
                discriminant < 0f)
            {
                enter = exit = 0f;
                return false;
            }
            float root = MathF.Sqrt(discriminant);
            enter = (-b - root) / (2f * a);
            exit = (-b + root) / (2f * a);
            if (exit < 0f || enter > 1f)
                return false;
            enter = MathF.Max(0f, enter);
            exit = MathF.Min(1f, exit);
            return enter <= exit;
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
                return start >= minimum &&
                    start <= maximum;
            float first =
                (minimum - start) / direction;
            float second =
                (maximum - start) / direction;
            if (first > second)
            {
                float swap = first;
                first = second;
                second = swap;
            }
            if (first > enter) enter = first;
            if (second < exit) exit = second;
            return enter <= exit &&
                exit >= 0f &&
                enter <= 1f;
        }

        private static void IncludeCylinderRoot(
            float fraction,
            float axialOrigin,
            float axialDirection,
            float length,
            ref bool found,
            ref float enter,
            ref float exit)
        {
            float position =
                axialOrigin +
                axialDirection * fraction;
            if (position < 0f ||
                position > length)
            {
                return;
            }
            if (!found || fraction < enter)
                enter = fraction;
            if (!found || fraction > exit)
                exit = fraction;
            found = true;
        }

        private static void IncludeSphere(
            Float3 start,
            Float3 end,
            Float3 center,
            float radius,
            ref bool found,
            ref float enter,
            ref float exit)
        {
            float first;
            float second;
            if (!IntersectSphere(
                    start,
                    end,
                    center,
                    radius,
                    out first,
                    out second))
            {
                return;
            }
            if (!found || first < enter) enter = first;
            if (!found || second > exit) exit = second;
            found = true;
        }

        private static float Component(
            Float3 value,
            int axis)
        {
            return axis == 0
                ? value.X
                : axis == 1
                    ? value.Y
                    : value.Z;
        }
    }
}
