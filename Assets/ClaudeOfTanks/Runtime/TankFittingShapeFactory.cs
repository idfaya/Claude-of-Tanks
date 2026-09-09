using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankFittingShapeFactory
    {
        public static Transform BuildSmokeBank(
            string prefix,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            int count,
            float radius,
            float length,
            float pitch,
            float splay,
            float arc,
            float spacing,
            Color detailColor,
            Color darkColor,
            bool includeCaps = true,
            bool includeBase = true)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException(
                    "Smoke-bank prefix is required.",
                    nameof(prefix));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            int tubeCount = Mathf.Clamp(count, 1, 8);
            Transform root = Root(
                prefix + "-SmokeBank",
                parent,
                position,
                rotation);
            for (int index = 0; index < tubeCount; index++)
            {
                float offset = index - (tubeCount - 1) * 0.5f;
                float yaw = splay + offset * (arc / tubeCount);
                Vector3 center = new Vector3(
                    Mathf.Cos(splay) * offset * spacing,
                    0f,
                    -Mathf.Sin(splay) * offset * spacing);
                Quaternion tubeRotation = Quaternion.Euler(
                    pitch * Mathf.Rad2Deg,
                    yaw * Mathf.Rad2Deg,
                    0f);
                Transform tube = Cylinder(
                    prefix + "-Detail-SmokeLauncher",
                    root,
                    center,
                    radius,
                    radius,
                    length,
                    8,
                    TankShapeAxis.Z,
                    detailColor);
                tube.localRotation = tubeRotation;
                if (!includeCaps) continue;
                Transform cap = Cylinder(
                    prefix + "-SmokeLauncherCap",
                    root,
                    center + tubeRotation *
                        new Vector3(
                            0f,
                            0f,
                            length * 0.5f + 0.007f),
                    radius * 0.88f,
                    radius * 0.88f,
                    0.012f,
                    8,
                    TankShapeAxis.Z,
                    darkColor);
                cap.localRotation = tubeRotation;
            }
            if (includeBase)
            {
                Transform bracket = Box(
                    prefix + "-SmokeBankBase",
                    root,
                    new Vector3(0f, -0.06f, -0.06f),
                    new Vector3(
                        tubeCount * spacing + 0.06f,
                        0.05f,
                        0.08f),
                    darkColor);
                bracket.localRotation = Quaternion.Euler(
                    0f,
                    splay * 0.5f * Mathf.Rad2Deg,
                    0f);
            }
            return root;
        }

        public static Transform BuildAntennaWhip(
            string prefix,
            Transform parent,
            Vector3 position,
            float height,
            float radius,
            float rake,
            Color detailColor,
            Color darkColor,
            bool includeBase = true)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException(
                    "Antenna prefix is required.",
                    nameof(prefix));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            Transform root = Root(
                prefix + "-AntennaWhip",
                parent,
                position,
                Quaternion.identity);
            if (includeBase)
            {
                Cylinder(
                    prefix + "-AntennaBasePot",
                    root,
                    new Vector3(0f, 0.04f, 0f),
                    0.035f,
                    0.045f,
                    0.08f,
                    10,
                    TankShapeAxis.Y,
                    darkColor);
                Cylinder(
                    prefix + "-AntennaCollar",
                    root,
                    new Vector3(0f, 0.10f, 0f),
                    0.02f,
                    0.02f,
                    0.05f,
                    8,
                    TankShapeAxis.Y,
                    darkColor);
            }
            float baseTop = includeBase ? 0.12f : 0f;
            Transform whip = Box(
                prefix + "-Detail-RadioWhip",
                root,
                new Vector3(
                    -Mathf.Sin(rake) * height * 0.5f,
                    baseTop + Mathf.Cos(rake) * height * 0.5f,
                    0f),
                new Vector3(
                    radius * 2f,
                    height,
                    radius * 2f),
                detailColor);
            whip.localRotation =
                Quaternion.Euler(0f, 0f, rake * Mathf.Rad2Deg);
            return root;
        }

        public static Transform BuildTowCable(
            string name,
            Transform parent,
            Vector3[] points,
            float radius,
            int tubularSegments,
            int radialSegments,
            Color color)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(
                    "Tow-cable name is required.",
                    nameof(name));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (points == null || points.Length < 2)
                throw new ArgumentException(
                    "Tow cable requires at least two points.",
                    nameof(points));
            if (radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));
            if (tubularSegments < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(tubularSegments));
            if (radialSegments < 3)
                throw new ArgumentOutOfRangeException(
                    nameof(radialSegments));

            int ringCount = tubularSegments + 1;
            Vector3[] vertices =
                new Vector3[ringCount * radialSegments];
            Vector3[] normals =
                new Vector3[vertices.Length];
            Vector2[] uvs =
                new Vector2[vertices.Length];
            for (int ring = 0; ring < ringCount; ring++)
            {
                float t = ring / (float)tubularSegments;
                Vector3 center = CatmullRom(points, t);
                float beforeT = Mathf.Max(0f, t - 0.001f);
                float afterT = Mathf.Min(1f, t + 0.001f);
                Vector3 tangent =
                    (CatmullRom(points, afterT) -
                     CatmullRom(points, beforeT)).normalized;
                Vector3 side =
                    Vector3.Cross(tangent, Vector3.up);
                if (side.sqrMagnitude < 0.000001f)
                    side = Vector3.Cross(tangent, Vector3.right);
                side.Normalize();
                Vector3 normal =
                    Vector3.Cross(side, tangent).normalized;
                for (int radial = 0;
                    radial < radialSegments;
                    radial++)
                {
                    float angle =
                        radial * Mathf.PI * 2f / radialSegments;
                    Vector3 outward =
                        normal * Mathf.Cos(angle) +
                        side * Mathf.Sin(angle);
                    int vertex = ring * radialSegments + radial;
                    vertices[vertex] =
                        center + outward * radius;
                    normals[vertex] = outward;
                    uvs[vertex] = new Vector2(
                        radial / (float)radialSegments,
                        t);
                }
            }

            int[] triangles =
                new int[tubularSegments * radialSegments * 6];
            int triangle = 0;
            for (int ring = 0; ring < tubularSegments; ring++)
            {
                for (int radial = 0;
                    radial < radialSegments;
                    radial++)
                {
                    int next = (radial + 1) % radialSegments;
                    int a = ring * radialSegments + radial;
                    int b = ring * radialSegments + next;
                    int c = (ring + 1) * radialSegments + radial;
                    int d = (ring + 1) * radialSegments + next;
                    triangles[triangle++] = a;
                    triangles[triangle++] = c;
                    triangles[triangle++] = d;
                    triangles[triangle++] = a;
                    triangles[triangle++] = d;
                    triangles[triangle++] = b;
                }
            }
            return TankShapeFactory.MeshPart(
                name,
                parent,
                vertices,
                triangles,
                color,
                normals,
                uvs);
        }

        private static Vector3 CatmullRom(
            Vector3[] points,
            float t)
        {
            int spans = points.Length - 1;
            float scaled = Mathf.Clamp01(t) * spans;
            int index = Mathf.Min(
                Mathf.FloorToInt(scaled),
                spans - 1);
            float localT =
                index == spans - 1 && t >= 1f
                    ? 1f
                    : scaled - index;
            Vector3 p1 = points[index];
            Vector3 p2 = points[index + 1];
            Vector3 p0 = index > 0
                ? points[index - 1]
                : p1 * 2f - p2;
            Vector3 p3 = index + 2 < points.Length
                ? points[index + 2]
                : p2 * 2f - p1;
            float dt0 = Mathf.Pow(
                (p0 - p1).sqrMagnitude,
                0.25f);
            float dt1 = Mathf.Pow(
                (p1 - p2).sqrMagnitude,
                0.25f);
            float dt2 = Mathf.Pow(
                (p2 - p3).sqrMagnitude,
                0.25f);
            if (dt1 < 0.0001f) dt1 = 1f;
            if (dt0 < 0.0001f) dt0 = dt1;
            if (dt2 < 0.0001f) dt2 = dt1;
            Vector3 tangent1 =
                ((p1 - p0) / dt0 -
                 (p2 - p0) / (dt0 + dt1) +
                 (p2 - p1) / dt1) * dt1;
            Vector3 tangent2 =
                ((p2 - p1) / dt1 -
                 (p3 - p1) / (dt1 + dt2) +
                 (p3 - p2) / dt2) * dt1;
            float t2 = localT * localT;
            float t3 = t2 * localT;
            return
                p1 +
                tangent1 * localT +
                (-3f * p1 + 3f * p2 -
                 2f * tangent1 - tangent2) * t2 +
                (2f * p1 - 2f * p2 +
                 tangent1 + tangent2) * t3;
        }

        private static Transform Root(
            string name,
            Transform parent,
            Vector3 position,
            Quaternion rotation)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            root.localRotation = rotation;
            return root;
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            return part;
        }

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float top,
            float bottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name, parent, top, bottom, length, segments, axis, color);
            part.localPosition = position;
            return part;
        }
    }
}
