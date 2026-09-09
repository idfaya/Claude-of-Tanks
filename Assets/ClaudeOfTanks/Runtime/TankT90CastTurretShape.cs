using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal readonly struct TankCastSectionLevel
    {
        public readonly float Height;
        public readonly float LeftX;
        public readonly float RightX;

        public TankCastSectionLevel(
            float height,
            float leftX,
            float rightX)
        {
            Height = height;
            LeftX = leftX;
            RightX = rightX;
        }
    }

    internal readonly struct TankCastSection
    {
        public readonly float Z;
        public readonly TankCastSectionLevel[] Levels;

        public TankCastSection(
            float z,
            TankCastSectionLevel[] levels)
        {
            Z = z;
            Levels = levels;
        }
    }

    internal static class TankT90CastTurretShape
    {
        public static Transform BuildBase(
            string name,
            Transform parent,
            Color color)
        {
            return Build(
                name,
                parent,
                new[]
                {
                    S(1.38f,
                        L(-0.04f, -0.54f, 0.58f),
                        L(0.10f, -0.82f, 0.86f),
                        L(0.29f, -0.70f, 0.74f),
                        L(0.48f, -0.42f, 0.46f),
                        L(0.58f, -0.22f, 0.26f)),
                    S(1.10f,
                        L(-0.05f, -0.98f, 1.04f),
                        L(0.12f, -1.34f, 1.40f),
                        L(0.34f, -1.22f, 1.31f),
                        L(0.54f, -0.86f, 0.94f),
                        L(0.64f, -0.46f, 0.54f)),
                    S(0.70f,
                        L(-0.06f, -1.25f, 1.31f),
                        L(0.13f, -1.55f, 1.60f),
                        L(0.37f, -1.42f, 1.51f),
                        L(0.58f, -1.00f, 1.10f),
                        L(0.69f, -0.61f, 0.70f)),
                    S(0.20f,
                        L(-0.06f, -1.39f, 1.44f),
                        L(0.13f, -1.62f, 1.64f),
                        L(0.39f, -1.46f, 1.55f),
                        L(0.61f, -1.02f, 1.12f),
                        L(0.71f, -0.68f, 0.76f)),
                    S(-0.34f,
                        L(-0.06f, -1.40f, 1.44f),
                        L(0.13f, -1.58f, 1.61f),
                        L(0.39f, -1.42f, 1.51f),
                        L(0.61f, -1.00f, 1.10f),
                        L(0.71f, -0.66f, 0.74f)),
                    S(-0.79f,
                        L(-0.05f, -1.31f, 1.38f),
                        L(0.12f, -1.47f, 1.53f),
                        L(0.35f, -1.29f, 1.39f),
                        L(0.57f, -0.88f, 0.98f),
                        L(0.66f, -0.56f, 0.64f)),
                    S(-1.16f,
                        L(-0.03f, -1.12f, 1.23f),
                        L(0.11f, -1.29f, 1.37f),
                        L(0.30f, -1.11f, 1.23f),
                        L(0.49f, -0.72f, 0.84f),
                        L(0.57f, -0.43f, 0.52f)),
                    S(-1.46f,
                        L(-0.01f, -0.78f, 0.91f),
                        L(0.09f, -1.00f, 1.10f),
                        L(0.24f, -0.86f, 0.98f),
                        L(0.40f, -0.53f, 0.66f),
                        L(0.46f, -0.31f, 0.40f))
                },
                color);
        }

        public static Transform Build(
            string name,
            Transform parent,
            TankCastSection[] sections,
            Color color)
        {
            Validate(sections);
            int levelCount = sections[0].Levels.Length;
            Vector3[,,] rings =
                new Vector3[sections.Length, levelCount, 2];
            for (int section = 0; section < sections.Length; section++)
            for (int level = 0; level < levelCount; level++)
            {
                TankCastSectionLevel source =
                    sections[section].Levels[level];
                rings[section, level, 0] =
                    new Vector3(
                        source.LeftX,
                        source.Height,
                        sections[section].Z);
                rings[section, level, 1] =
                    new Vector3(
                        source.RightX,
                        source.Height,
                        sections[section].Z);
            }

            List<Vector3> vertices =
                new List<Vector3>(
                    (sections.Length - 1) *
                    (levelCount * 2) * 6);
            for (int section = 0;
                section < sections.Length - 1;
                section++)
            {
                for (int level = 0;
                    level < levelCount - 1;
                    level++)
                {
                    AddQuad(
                        vertices,
                        rings[section, level, 0],
                        rings[section + 1, level, 0],
                        rings[section + 1, level + 1, 0],
                        rings[section, level + 1, 0],
                        Vector3.left);
                    AddQuad(
                        vertices,
                        rings[section, level, 1],
                        rings[section, level + 1, 1],
                        rings[section + 1, level + 1, 1],
                        rings[section + 1, level, 1],
                        Vector3.right);
                }
                AddQuad(
                    vertices,
                    rings[section, levelCount - 1, 0],
                    rings[section, levelCount - 1, 1],
                    rings[section + 1, levelCount - 1, 1],
                    rings[section + 1, levelCount - 1, 0],
                    Vector3.up);
                AddQuad(
                    vertices,
                    rings[section, 0, 0],
                    rings[section + 1, 0, 0],
                    rings[section + 1, 0, 1],
                    rings[section, 0, 1],
                    Vector3.down);
            }

            float zDirection = Mathf.Sign(
                sections[sections.Length - 1].Z -
                sections[0].Z);
            if (zDirection == 0f) zDirection = 1f;
            AddCap(
                vertices,
                rings,
                0,
                levelCount,
                Vector3.back * zDirection);
            AddCap(
                vertices,
                rings,
                sections.Length - 1,
                levelCount,
                Vector3.forward * zDirection);

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

        private static void Validate(TankCastSection[] sections)
        {
            if (sections == null || sections.Length < 2)
                throw new ArgumentException(
                    "Cast loft requires at least two sections.",
                    nameof(sections));
            int levelCount = sections[0].Levels == null
                ? 0
                : sections[0].Levels.Length;
            if (levelCount < 2)
                throw new ArgumentException(
                    "Cast loft requires at least two levels.",
                    nameof(sections));
            for (int index = 1; index < sections.Length; index++)
            {
                if (sections[index].Levels == null ||
                    sections[index].Levels.Length != levelCount)
                {
                    throw new ArgumentException(
                        "Cast loft sections must share one level count.",
                        nameof(sections));
                }
            }
        }

        private static TankCastSection S(
            float z,
            params TankCastSectionLevel[] levels)
        {
            return new TankCastSection(z, levels);
        }

        private static TankCastSectionLevel L(
            float height,
            float leftX,
            float rightX)
        {
            return new TankCastSectionLevel(
                height,
                leftX,
                rightX);
        }

        private static void AddCap(
            List<Vector3> vertices,
            Vector3[,,] rings,
            int section,
            int levelCount,
            Vector3 expectedNormal)
        {
            for (int level = 0; level < levelCount - 1; level++)
            {
                AddQuad(
                    vertices,
                    rings[section, level, 0],
                    rings[section, level, 1],
                    rings[section, level + 1, 1],
                    rings[section, level + 1, 0],
                    expectedNormal);
            }
        }

        private static void AddQuad(
            List<Vector3> vertices,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            Vector3 expectedNormal)
        {
            AddTriangle(vertices, a, b, c, expectedNormal);
            AddTriangle(vertices, a, c, d, expectedNormal);
        }

        private static void AddTriangle(
            List<Vector3> vertices,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 expectedNormal)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            vertices.Add(a);
            if (Vector3.Dot(normal, expectedNormal) < 0f)
            {
                vertices.Add(c);
                vertices.Add(b);
            }
            else
            {
                vertices.Add(b);
                vertices.Add(c);
            }
        }
    }
}
