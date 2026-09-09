using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMudguardShapeFactory
    {
        public static Transform Build(
            string name,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            float thickness,
            float length,
            float height,
            Color panelColor,
            Color supportColor,
            float? crown = null,
            float? frontCut = null,
            float? rearCut = null,
            float rake = 0f,
            Vector2[] normalizedProfile = null,
            bool support = true,
            string supportName = null)
        {
            if (length <= 0f || height <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    "Mudguard length and height must be positive.");

            float resolvedLength = Mathf.Max(0.04f, length);
            float resolvedHeight = Mathf.Max(0.04f, height);
            float resolvedThickness = Mathf.Max(0.012f, thickness);
            float resolvedFrontCut = Mathf.Clamp(
                frontCut ?? resolvedHeight * 0.10f,
                0f,
                resolvedHeight * 0.45f);
            float resolvedRearCut = Mathf.Clamp(
                rearCut ?? resolvedHeight * 0.06f,
                0f,
                resolvedHeight * 0.45f);
            float resolvedCrown = Mathf.Clamp(
                crown ?? resolvedHeight * 0.025f,
                -resolvedHeight * 0.18f,
                resolvedHeight * 0.18f);
            float resolvedRake = Mathf.Clamp(
                rake,
                -resolvedHeight * 0.35f,
                resolvedHeight * 0.35f);
            Vector2[] outline = BuildOutline(
                resolvedLength,
                resolvedHeight,
                resolvedCrown,
                resolvedFrontCut,
                resolvedRearCut,
                resolvedRake,
                normalizedProfile);

            Transform panel = TankShapeFactory.MeshPart(
                name,
                parent,
                BuildVertices(outline, resolvedThickness),
                BuildTriangles(outline.Length),
                panelColor);
            panel.localPosition = position;
            panel.localRotation = rotation;

            if (support)
            {
                Transform hanger = TankShapeFactory.BoxPart(
                    supportName ?? name + "Support",
                    parent,
                    new Vector3(
                        resolvedThickness * 1.45f,
                        Mathf.Max(0.035f, resolvedHeight * 0.08f),
                        resolvedLength * 0.94f),
                    supportColor);
                hanger.localPosition = new Vector3(
                    position.x,
                    position.y + resolvedHeight * 0.5f -
                    Mathf.Max(0.012f, resolvedHeight * 0.035f),
                    position.z);
                hanger.localRotation = rotation;
            }
            return panel;
        }

        private static Vector2[] BuildOutline(
            float length,
            float height,
            float crown,
            float frontCut,
            float rearCut,
            float rake,
            Vector2[] normalizedProfile)
        {
            if (normalizedProfile != null)
            {
                if (normalizedProfile.Length < 3)
                    throw new ArgumentException(
                        "Mudguard profile requires at least three points.",
                        nameof(normalizedProfile));
                Vector2[] scaled =
                    new Vector2[normalizedProfile.Length];
                for (int index = 0;
                    index < normalizedProfile.Length;
                    index++)
                {
                    scaled[index] = new Vector2(
                        normalizedProfile[index].x * length,
                        normalizedProfile[index].y * height);
                }
                return scaled;
            }
            return new[]
            {
                new Vector2(-length * 0.5f, height * 0.5f),
                new Vector2(0f, height * 0.5f + crown),
                new Vector2(length * 0.5f, height * 0.5f - rake),
                new Vector2(
                    length * 0.5f,
                    -height * 0.5f + frontCut),
                new Vector2(length * 0.36f, -height * 0.5f),
                new Vector2(-length * 0.38f, -height * 0.5f),
                new Vector2(
                    -length * 0.5f,
                    -height * 0.5f + rearCut)
            };
        }

        private static Vector3[] BuildVertices(
            Vector2[] outline,
            float thickness)
        {
            List<Vector3> vertices =
                new List<Vector3>((outline.Length - 2) * 6 +
                    outline.Length * 6);
            float half = thickness * 0.5f;
            for (int index = 1;
                index < outline.Length - 1;
                index++)
            {
                AddTriangle(
                    vertices,
                    Point(outline[0], -half),
                    Point(outline[index], -half),
                    Point(outline[index + 1], -half));
                AddTriangle(
                    vertices,
                    Point(outline[0], half),
                    Point(outline[index + 1], half),
                    Point(outline[index], half));
            }
            for (int index = 0; index < outline.Length; index++)
            {
                int next = (index + 1) % outline.Length;
                Vector3 leftA = Point(outline[index], -half);
                Vector3 rightA = Point(outline[index], half);
                Vector3 rightB = Point(outline[next], half);
                Vector3 leftB = Point(outline[next], -half);
                AddTriangle(vertices, leftA, rightA, rightB);
                AddTriangle(vertices, leftA, rightB, leftB);
            }
            return vertices.ToArray();
        }

        private static int[] BuildTriangles(int pointCount)
        {
            int vertexCount =
                (pointCount - 2) * 6 + pointCount * 6;
            int[] triangles = new int[vertexCount];
            for (int index = 0; index < triangles.Length; index++)
                triangles[index] = index;
            return triangles;
        }

        private static Vector3 Point(Vector2 point, float x)
        {
            return new Vector3(x, point.y, -point.x);
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
