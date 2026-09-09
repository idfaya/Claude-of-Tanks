using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal readonly struct TankShapeStationValues
    {
        private readonly float _scalar;
        private readonly float[] _values;

        public TankShapeStationValues(float scalar)
        {
            _scalar = scalar;
            _values = null;
        }

        public TankShapeStationValues(float[] values)
        {
            _scalar = 0f;
            _values = values;
        }

        public float Resolve(int index, int stationCount)
        {
            if (_values == null) return _scalar;
            if (_values.Length != stationCount)
                throw new ArgumentException(
                    "Loft station values must match the plan.");
            return _values[index];
        }

        public static implicit operator TankShapeStationValues(float value)
        {
            return new TankShapeStationValues(value);
        }

        public static implicit operator TankShapeStationValues(float[] values)
        {
            return new TankShapeStationValues(values);
        }
    }

    internal readonly struct TankShapeLoftRing
    {
        public readonly TankShapeStationValues Height;
        public readonly TankShapeStationValues Inset;
        public readonly Vector2 Offset;
        public readonly float? CenterHeight;

        public TankShapeLoftRing(
            TankShapeStationValues height,
            TankShapeStationValues inset,
            Vector2 offset,
            float? centerHeight = null)
        {
            Height = height;
            Inset = inset;
            Offset = offset;
            CenterHeight = centerHeight;
        }
    }

    internal static class TankPolyLoftShapeFactory
    {
        public static Transform BuildTurret(
            string name,
            Transform parent,
            Vector2[] plan,
            float height,
            float flare,
            float inset,
            Color color)
        {
            ValidatePlan(plan);
            Vector2 center = PlanCenter(plan);
            Vector3[] lower = ScaledRing(
                plan,
                0f,
                flare,
                center,
                Vector2.zero);
            Vector3[] upper = ScaledRing(
                plan,
                height,
                inset,
                center,
                Vector2.zero);
            List<Vector3> vertices =
                new List<Vector3>(plan.Length * 9);
            PushOrientedSides(vertices, lower, upper, center);
            PushTopFan(vertices, upper, center, height);
            return BuildMesh(name, parent, vertices, color);
        }

        public static Transform BuildLoft(
            string name,
            Transform parent,
            Vector2[] plan,
            TankShapeStationValues bottom,
            TankShapeStationValues top,
            TankShapeStationValues inset,
            Color color)
        {
            ValidatePlan(plan);
            Vector2 center = PlanCenter(plan);
            Vector3[] lower = new Vector3[plan.Length];
            for (int index = 0; index < plan.Length; index++)
            {
                lower[index] = new Vector3(
                    plan[index].x,
                    bottom.Resolve(index, plan.Length),
                    plan[index].y);
            }
            Vector3[] upper =
                ScaledRing(plan, top, inset, center, Vector2.zero);
            List<Vector3> vertices =
                new List<Vector3>(plan.Length * 9);
            PushOrientedSides(vertices, lower, upper, center);
            PushTopFan(vertices, upper, center, null);
            return BuildMesh(name, parent, vertices, color);
        }

        public static Transform BuildMultiLoft(
            string name,
            Transform parent,
            Vector2[] plan,
            TankShapeLoftRing[] rings,
            Color color)
        {
            ValidatePlan(plan);
            if (rings == null || rings.Length < 2)
                throw new ArgumentException(
                    "Poly multi-loft requires at least two rings.",
                    nameof(rings));

            Vector2 center = PlanCenter(plan);
            Vector3[][] resolved = new Vector3[rings.Length][];
            Vector2[] ringCenters = new Vector2[rings.Length];
            for (int index = 0; index < rings.Length; index++)
            {
                ringCenters[index] = center + rings[index].Offset;
                resolved[index] = ScaledRing(
                    plan,
                    rings[index].Height,
                    rings[index].Inset,
                    center,
                    rings[index].Offset);
            }

            List<Vector3> vertices = new List<Vector3>(
                plan.Length * (rings.Length * 6));
            for (int index = 0; index < resolved.Length - 1; index++)
            {
                PushOrientedSides(
                    vertices,
                    resolved[index],
                    resolved[index + 1],
                    (ringCenters[index] + ringCenters[index + 1]) * 0.5f);
            }
            PushOrderedFan(
                vertices,
                resolved[0],
                ringCenters[0],
                false,
                rings[0].CenterHeight);
            int finalIndex = resolved.Length - 1;
            PushOrderedFan(
                vertices,
                resolved[finalIndex],
                ringCenters[finalIndex],
                true,
                rings[finalIndex].CenterHeight);
            return BuildMesh(name, parent, vertices, color);
        }

        private static Transform BuildMesh(
            string name,
            Transform parent,
            List<Vector3> vertices,
            Color color)
        {
            int[] triangles = new int[vertices.Count];
            for (int index = 0; index < triangles.Length; index++)
                triangles[index] = index;
            return TankShapeFactory.MeshPart(
                name,
                parent,
                vertices.ToArray(),
                triangles,
                color,
                null,
                new Vector2[vertices.Count]);
        }

        private static Vector3[] ScaledRing(
            Vector2[] plan,
            TankShapeStationValues height,
            TankShapeStationValues inset,
            Vector2 center,
            Vector2 offset)
        {
            Vector3[] result = new Vector3[plan.Length];
            for (int index = 0; index < plan.Length; index++)
            {
                float scale = inset.Resolve(index, plan.Length);
                result[index] = new Vector3(
                    center.x +
                        (plan[index].x - center.x) * scale +
                        offset.x,
                    height.Resolve(index, plan.Length),
                    center.y +
                        (plan[index].y - center.y) * scale +
                        offset.y);
            }
            return result;
        }

        private static void PushOrientedSides(
            List<Vector3> vertices,
            Vector3[] lower,
            Vector3[] upper,
            Vector2 center)
        {
            for (int index = 0; index < lower.Length; index++)
            {
                int next = (index + 1) % lower.Length;
                float midpointX =
                    (lower[index].x + lower[next].x) * 0.5f -
                    center.x;
                float midpointZ =
                    (lower[index].z + lower[next].z) * 0.5f -
                    center.y;
                float edgeX = lower[next].x - lower[index].x;
                float edgeZ = lower[next].z - lower[index].z;
                if (edgeX * midpointZ - edgeZ * midpointX > 0f)
                {
                    AddTriangle(
                        vertices,
                        lower[index],
                        lower[next],
                        upper[next]);
                    AddTriangle(
                        vertices,
                        lower[index],
                        upper[next],
                        upper[index]);
                }
                else
                {
                    AddTriangle(
                        vertices,
                        lower[next],
                        lower[index],
                        upper[index]);
                    AddTriangle(
                        vertices,
                        lower[next],
                        upper[index],
                        upper[next]);
                }
            }
        }

        private static void PushTopFan(
            List<Vector3> vertices,
            Vector3[] ring,
            Vector2 center,
            float? centerHeight)
        {
            Vector3 centerPoint = new Vector3(
                center.x,
                centerHeight ?? AverageHeight(ring),
                center.y);
            for (int index = 0; index < ring.Length; index++)
            {
                int next = (index + 1) % ring.Length;
                float normalY =
                    (ring[next].z - ring[index].z) *
                    (center.x - ring[index].x) -
                    (ring[next].x - ring[index].x) *
                    (center.y - ring[index].z);
                if (normalY > 0f)
                    AddTriangle(
                        vertices,
                        ring[index],
                        ring[next],
                        centerPoint);
                else
                    AddTriangle(
                        vertices,
                        ring[next],
                        ring[index],
                        centerPoint);
            }
        }

        private static void PushOrderedFan(
            List<Vector3> vertices,
            Vector3[] ring,
            Vector2 center,
            bool top,
            float? centerHeight)
        {
            Vector3 centerPoint = new Vector3(
                center.x,
                centerHeight ?? AverageHeight(ring),
                center.y);
            for (int index = 0; index < ring.Length; index++)
            {
                int next = (index + 1) % ring.Length;
                if (top)
                    AddTriangle(
                        vertices,
                        ring[index],
                        ring[next],
                        centerPoint);
                else
                    AddTriangle(
                        vertices,
                        ring[next],
                        ring[index],
                        centerPoint);
            }
        }

        private static float AverageHeight(Vector3[] ring)
        {
            float sum = 0f;
            for (int index = 0; index < ring.Length; index++)
                sum += ring[index].y;
            return sum / ring.Length;
        }

        private static Vector2 PlanCenter(Vector2[] plan)
        {
            Vector2 sum = Vector2.zero;
            for (int index = 0; index < plan.Length; index++)
                sum += plan[index];
            return sum / plan.Length;
        }

        private static void ValidatePlan(Vector2[] plan)
        {
            if (plan == null || plan.Length < 3)
                throw new ArgumentException(
                    "Poly loft requires at least three plan points.",
                    nameof(plan));
        }

        private static void AddTriangle(
            List<Vector3> vertices,
            Vector3 a,
            Vector3 b,
            Vector3 c)
        {
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
        }
    }
}
