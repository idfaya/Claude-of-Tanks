using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankTorusShapeFactory
    {
        public static Transform Build(
            string name,
            Transform parent,
            float radius,
            float tubeRadius,
            int radialSegments,
            int tubularSegments,
            Color color)
        {
            if (radius <= 0f ||
                tubeRadius <= 0f ||
                tubeRadius >= radius)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(radius),
                    "Torus radii must be positive and tube radius " +
                    "must be smaller than the ring radius.");
            }
            if (radialSegments < 3 || tubularSegments < 3)
                throw new ArgumentOutOfRangeException(
                    nameof(radialSegments),
                    "Torus segment counts must be at least three.");

            int columns = radialSegments + 1;
            int rows = tubularSegments + 1;
            Vector3[] vertices = new Vector3[columns * rows];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int tube = 0; tube <= tubularSegments; tube++)
            {
                float v = tube / (float)tubularSegments;
                float tubeAngle = v * Mathf.PI * 2f;
                float tubeCosine = Mathf.Cos(tubeAngle);
                float tubeSine = Mathf.Sin(tubeAngle);
                for (int ring = 0; ring <= radialSegments; ring++)
                {
                    float u = ring / (float)radialSegments;
                    float ringAngle = u * Mathf.PI * 2f;
                    float ringCosine = Mathf.Cos(ringAngle);
                    float ringSine = Mathf.Sin(ringAngle);
                    int index = tube * columns + ring;
                    vertices[index] = new Vector3(
                        (radius + tubeRadius * tubeCosine) * ringCosine,
                        (radius + tubeRadius * tubeCosine) * ringSine,
                        tubeRadius * tubeSine);
                    Vector3 center = new Vector3(
                        radius * ringCosine,
                        radius * ringSine,
                        0f);
                    normals[index] =
                        (vertices[index] - center).normalized;
                    uvs[index] = new Vector2(u, v);
                }
            }

            int[] triangles =
                new int[radialSegments * tubularSegments * 6];
            int cursor = 0;
            for (int tube = 1; tube <= tubularSegments; tube++)
            for (int ring = 1; ring <= radialSegments; ring++)
            {
                int a = columns * tube + ring - 1;
                int b = columns * (tube - 1) + ring - 1;
                int c = columns * (tube - 1) + ring;
                int d = columns * tube + ring;
                triangles[cursor++] = a;
                triangles[cursor++] = b;
                triangles[cursor++] = d;
                triangles[cursor++] = b;
                triangles[cursor++] = c;
                triangles[cursor++] = d;
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
    }
}
