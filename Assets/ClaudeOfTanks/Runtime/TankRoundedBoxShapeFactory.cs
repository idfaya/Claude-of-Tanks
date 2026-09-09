using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankRoundedBoxShapeFactory
    {
        private const float RoundedThreshold = 0.06f;
        private const float MaximumRadius = 0.024f;

        public static Transform Build(
            string name,
            Transform parent,
            Vector3 size,
            Color color)
        {
            if (size.x <= 0f || size.y <= 0f || size.z <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    "Box dimensions must be positive.");

            float minimum = Mathf.Min(size.x, Mathf.Min(size.y, size.z));
            if (minimum < RoundedThreshold)
                return BuildHardBox(name, parent, size, color);

            int bevelSegments = minimum > 0.5f ? 2 : 1;
            int totalSegments = bevelSegments * 2 + 1;
            BuildBox(
                Vector3.one,
                totalSegments,
                out List<Vector3> sourceVertices,
                out _,
                out _,
                out List<int> sourceTriangles,
                out List<int> sourceFaces);

            int vertexCount = sourceTriangles.Count;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[vertexCount];
            float radius = Mathf.Min(MaximumRadius, minimum * 0.24f);
            Vector3 innerHalf =
                size * 0.5f - Vector3.one * radius;
            float halfSegmentSize = 0.5f / totalSegments;
            for (int index = 0; index < vertexCount; index++)
            {
                int sourceIndex = sourceTriangles[index];
                Vector3 source = sourceVertices[sourceIndex];
                Vector3 normal = source;
                normal.x -= Sign(source.x) * halfSegmentSize;
                normal.y -= Sign(source.y) * halfSegmentSize;
                normal.z -= Sign(source.z) * halfSegmentSize;
                normal.Normalize();
                vertices[index] =
                    Vector3.Scale(innerHalf, Signs(source)) +
                    normal * radius;
                normals[index] = normal;
                uvs[index] = RoundedUv(
                    sourceFaces[sourceIndex],
                    normal,
                    radius,
                    size);
                triangles[index] = index;
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

        private static Transform BuildHardBox(
            string name,
            Transform parent,
            Vector3 size,
            Color color)
        {
            BuildBox(
                size,
                1,
                out List<Vector3> vertices,
                out List<Vector3> normals,
                out List<Vector2> uvs,
                out List<int> triangles,
                out _);
            return TankShapeFactory.MeshPart(
                name,
                parent,
                vertices.ToArray(),
                triangles.ToArray(),
                color,
                normals.ToArray(),
                uvs.ToArray());
        }

        private static void BuildBox(
            Vector3 size,
            int segments,
            out List<Vector3> vertices,
            out List<Vector3> normals,
            out List<Vector2> uvs,
            out List<int> triangles,
            out List<int> faces)
        {
            int faceVertexCount =
                (segments + 1) * (segments + 1);
            vertices = new List<Vector3>(faceVertexCount * 6);
            normals = new List<Vector3>(faceVertexCount * 6);
            uvs = new List<Vector2>(faceVertexCount * 6);
            triangles = new List<int>(segments * segments * 36);
            faces = new List<int>(faceVertexCount * 6);
            BuildPlane(vertices, normals, uvs, triangles, faces,
                2, 1, 0, -1f, -1f,
                size.z, size.y, size.x, segments, 0);
            BuildPlane(vertices, normals, uvs, triangles, faces,
                2, 1, 0, 1f, -1f,
                size.z, size.y, -size.x, segments, 1);
            BuildPlane(vertices, normals, uvs, triangles, faces,
                0, 2, 1, 1f, 1f,
                size.x, size.z, size.y, segments, 2);
            BuildPlane(vertices, normals, uvs, triangles, faces,
                0, 2, 1, 1f, -1f,
                size.x, size.z, -size.y, segments, 3);
            BuildPlane(vertices, normals, uvs, triangles, faces,
                0, 1, 2, 1f, -1f,
                size.x, size.y, size.z, segments, 4);
            BuildPlane(vertices, normals, uvs, triangles, faces,
                0, 1, 2, -1f, -1f,
                size.x, size.y, -size.z, segments, 5);
        }

        private static void BuildPlane(
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<int> triangles,
            List<int> faces,
            int uAxis,
            int vAxis,
            int wAxis,
            float uDirection,
            float vDirection,
            float width,
            float height,
            float depth,
            int segments,
            int face)
        {
            int start = vertices.Count;
            float segmentWidth = width / segments;
            float segmentHeight = height / segments;
            float widthHalf = width * 0.5f;
            float heightHalf = height * 0.5f;
            float depthHalf = depth * 0.5f;
            for (int yIndex = 0; yIndex <= segments; yIndex++)
            {
                float y = yIndex * segmentHeight - heightHalf;
                for (int xIndex = 0; xIndex <= segments; xIndex++)
                {
                    float x = xIndex * segmentWidth - widthHalf;
                    Vector3 vertex = Vector3.zero;
                    Set(ref vertex, uAxis, x * uDirection);
                    Set(ref vertex, vAxis, y * vDirection);
                    Set(ref vertex, wAxis, depthHalf);
                    vertices.Add(vertex);

                    Vector3 normal = Vector3.zero;
                    Set(ref normal, wAxis, depth > 0f ? 1f : -1f);
                    normals.Add(normal);
                    uvs.Add(new Vector2(
                        xIndex / (float)segments,
                        1f - yIndex / (float)segments));
                    faces.Add(face);
                }
            }

            int row = segments + 1;
            for (int yIndex = 0; yIndex < segments; yIndex++)
            for (int xIndex = 0; xIndex < segments; xIndex++)
            {
                int a = start + xIndex + row * yIndex;
                int b = start + xIndex + row * (yIndex + 1);
                int c = start + xIndex + 1 + row * (yIndex + 1);
                int d = start + xIndex + 1 + row * yIndex;
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(d);
            }
        }

        private static Vector2 RoundedUv(
            int face,
            Vector3 normal,
            float radius,
            Vector3 size)
        {
            if (face == 0)
            {
                return new Vector2(
                    GetUv(Vector3.right, normal, 2, 1, radius, size.z),
                    1f - GetUv(
                        Vector3.right, normal, 1, 2, radius, size.y));
            }
            if (face == 1)
            {
                return new Vector2(
                    1f - GetUv(
                        Vector3.left, normal, 2, 1, radius, size.z),
                    1f - GetUv(
                        Vector3.left, normal, 1, 2, radius, size.y));
            }
            if (face == 2)
            {
                return new Vector2(
                    1f - GetUv(
                        Vector3.up, normal, 0, 2, radius, size.x),
                    GetUv(Vector3.up, normal, 2, 0, radius, size.z));
            }
            if (face == 3)
            {
                return new Vector2(
                    1f - GetUv(
                        Vector3.down, normal, 0, 2, radius, size.x),
                    1f - GetUv(
                        Vector3.down, normal, 2, 0, radius, size.z));
            }
            if (face == 4)
            {
                return new Vector2(
                    1f - GetUv(
                        Vector3.forward, normal, 0, 1, radius, size.x),
                    1f - GetUv(
                        Vector3.forward, normal, 1, 0, radius, size.y));
            }
            return new Vector2(
                GetUv(Vector3.back, normal, 0, 1, radius, size.x),
                1f - GetUv(
                    Vector3.back, normal, 1, 0, radius, size.y));
        }

        private static float GetUv(
            Vector3 faceDirection,
            Vector3 normal,
            int uvAxis,
            int projectionAxis,
            float radius,
            float sideLength)
        {
            float arcLength = Mathf.PI * radius * 0.5f;
            float centerLength =
                Mathf.Max(sideLength - radius * 2f, 0f);
            float arcRatio =
                0.5f * arcLength / (arcLength + centerLength);
            Vector3 projected = normal;
            Set(ref projected, projectionAxis, 0f);
            float angle = projected.sqrMagnitude <= 0.0000001f
                ? Mathf.PI * 0.5f
                : Mathf.Acos(Mathf.Clamp(
                    Vector3.Dot(
                        projected.normalized,
                        faceDirection),
                    -1f,
                    1f));
            float angleRatio =
                1f - angle / (Mathf.PI * 0.25f);
            if (Component(projected, uvAxis) > 0f)
                return angleRatio * arcRatio;
            float centerRatio =
                centerLength / (arcLength + centerLength);
            return centerRatio +
                arcRatio +
                arcRatio * (1f - angleRatio);
        }

        private static Vector3 Signs(Vector3 value)
        {
            return new Vector3(
                Sign(value.x),
                Sign(value.y),
                Sign(value.z));
        }

        private static float Sign(float value)
        {
            if (value > 0f) return 1f;
            if (value < 0f) return -1f;
            return 0f;
        }

        private static float Component(Vector3 value, int axis)
        {
            if (axis == 0) return value.x;
            if (axis == 1) return value.y;
            return value.z;
        }

        private static void Set(
            ref Vector3 value,
            int axis,
            float component)
        {
            if (axis == 0) value.x = component;
            else if (axis == 1) value.y = component;
            else value.z = component;
        }
    }
}
