using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLatheShapeFactory
    {
        public static Transform Build(
            string name,
            Transform parent,
            float[] radii,
            float[] heights,
            int segments,
            float zScale,
            Color color,
            float capRadius,
            float roofTiltScale)
        {
            Validate(radii, heights, segments, zScale);
            BuildProfile(
                radii,
                heights,
                out float[] profileRadii,
                out float[] profileHeights,
                out float[] profileAngles);
            int rings = profileRadii.Length;
            Vector3[] vertices = new Vector3[segments * rings];
            Vector3[] normals = new Vector3[segments * rings];
            for (int segment = 0; segment < segments; segment++)
            {
                float angle = segment * Mathf.PI * 2f / segments;
                float cosine = Mathf.Cos(angle);
                float sine = Mathf.Sin(angle);
                for (int ring = 0; ring < rings; ring++)
                {
                    float profileRadius = profileRadii[ring];
                    float radius = Mathf.Max(profileRadius, 0.001f);
                    int index = segment * rings + ring;
                    vertices[index] = new Vector3(
                        cosine * radius,
                        profileHeights[ring],
                        sine * radius * zScale);
                    float normalAngle = profileAngles[ring];
                    if (capRadius > 0f && normalAngle < 0.8f)
                    {
                        normalAngle = Mathf.Max(
                            normalAngle,
                            Mathf.Min(
                                0.8f,
                                Mathf.Asin(
                                    Mathf.Min(
                                        1f,
                                        profileRadius / capRadius))));
                    }
                    if (roofTiltScale != 0f &&
                        roofTiltScale != 1f &&
                        normalAngle < 0.8f)
                        normalAngle *= roofTiltScale;
                    normals[index] = new Vector3(
                        cosine * Mathf.Sin(normalAngle),
                        Mathf.Cos(normalAngle),
                        sine * Mathf.Sin(normalAngle) / zScale).normalized;
                }
            }

            int[] triangles = new int[segments * (rings - 1) * 6];
            int cursor = 0;
            for (int segment = 0; segment < segments; segment++)
            {
                int next = (segment + 1) % segments;
                for (int ring = 0; ring < rings - 1; ring++)
                {
                    int a = segment * rings + ring;
                    int b = next * rings + ring;
                    int c = next * rings + ring + 1;
                    int d = segment * rings + ring + 1;
                    triangles[cursor++] = a;
                    triangles[cursor++] = c;
                    triangles[cursor++] = b;
                    triangles[cursor++] = a;
                    triangles[cursor++] = d;
                    triangles[cursor++] = c;
                }
            }
            return TankShapeFactory.MeshPart(
                name,
                parent,
                vertices,
                triangles,
                color,
                normals);
        }

        private static void Validate(
            float[] radii,
            float[] heights,
            int segments,
            float zScale)
        {
            if (radii == null ||
                heights == null ||
                radii.Length != heights.Length ||
                radii.Length < 2)
            {
                throw new ArgumentException(
                    "Lathe profile requires matching radius/height arrays.");
            }
            if (segments < 3 || zScale <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(segments),
                    "Lathe segments and scale must be positive.");
        }

        private static void BuildProfile(
            float[] radii,
            float[] heights,
            out float[] profileRadii,
            out float[] profileHeights,
            out float[] profileAngles)
        {
            int sourceRings = radii.Length;
            float[] segmentAngles = new float[sourceRings - 1];
            int profileCount = 1;
            for (int index = 0; index < sourceRings - 1; index++)
            {
                float dr = radii[index + 1] - radii[index];
                float dy = heights[index + 1] - heights[index];
                segmentAngles[index] = Mathf.Atan2(dy, -dr);
                profileCount += Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        Mathf.Sqrt(dr * dr + dy * dy) / 0.055f));
            }

            float[] vertexAngles = new float[sourceRings];
            vertexAngles[0] = segmentAngles[0];
            for (int index = 1; index < sourceRings - 1; index++)
                vertexAngles[index] =
                    (segmentAngles[index - 1] + segmentAngles[index]) * 0.5f;
            vertexAngles[sourceRings - 1] =
                segmentAngles[sourceRings - 2];

            profileRadii = new float[profileCount];
            profileHeights = new float[profileCount];
            profileAngles = new float[profileCount];
            int cursor = 0;
            for (int index = 0; index < sourceRings - 1; index++)
            {
                float dr = radii[index + 1] - radii[index];
                float dy = heights[index + 1] - heights[index];
                int cuts = Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        Mathf.Sqrt(dr * dr + dy * dy) / 0.055f));
                for (int cut = 0; cut < cuts; cut++)
                {
                    float t = cut / (float)cuts;
                    profileRadii[cursor] = radii[index] + dr * t;
                    profileHeights[cursor] = heights[index] + dy * t;
                    profileAngles[cursor] = Mathf.Lerp(
                        vertexAngles[index],
                        vertexAngles[index + 1],
                        t);
                    cursor++;
                }
            }
            profileRadii[cursor] = radii[sourceRings - 1];
            profileHeights[cursor] = heights[sourceRings - 1];
            profileAngles[cursor] = vertexAngles[sourceRings - 1];
        }
    }
}
