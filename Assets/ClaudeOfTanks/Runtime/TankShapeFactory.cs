using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal enum TankShapeAxis
    {
        X,
        Y,
        Z
    }

    internal static class TankShapeFactory
    {
        public static Transform MeshPart(
            string name,
            Transform parent,
            Vector3[] vertices,
            int[] triangles,
            Color color,
            Vector3[] normals = null,
            Vector2[] uvs = null)
        {
            if (vertices == null || vertices.Length < 3)
                throw new ArgumentException(
                    "Mesh requires at least three vertices.",
                    nameof(vertices));
            if (triangles == null ||
                triangles.Length < 3 ||
                triangles.Length % 3 != 0)
            {
                throw new ArgumentException(
                    "Mesh triangles must contain complete faces.",
                    nameof(triangles));
            }
            if (normals != null && normals.Length != vertices.Length)
                throw new ArgumentException(
                    "Mesh normals must match the vertex count.",
                    nameof(normals));
            if (uvs != null && uvs.Length != vertices.Length)
                throw new ArgumentException(
                    "Mesh UVs must match the vertex count.",
                    nameof(uvs));
            for (int index = 0; index < triangles.Length; index++)
            {
                if (triangles[index] < 0 ||
                    triangles[index] >= vertices.Length)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(triangles),
                        "Mesh triangle index is outside the vertex array.");
                }
            }

            GameObject part = new GameObject(name);
            part.transform.SetParent(parent, false);
            Mesh mesh = new Mesh { name = name + "Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            if (normals != null)
            {
                mesh.normals = normals;
            }
            else
            {
                mesh.RecalculateNormals();
            }
            if (uvs != null) mesh.uv = uvs;
            mesh.RecalculateBounds();
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.AddComponent<MeshRenderer>().sharedMaterial =
                CreateMaterial(color);
            return part.transform;
        }

        public static Transform BoxPart(
            string name,
            Transform parent,
            Vector3 size,
            Color color)
        {
            return TankRoundedBoxShapeFactory.Build(
                name,
                parent,
                size,
                color);
        }

        public static Transform OrientedSlabPart(
            string name,
            Transform parent,
            Vector3 bottom0,
            Vector3 bottom1,
            Vector3 bottom2,
            Vector3 bottom3,
            Vector3 top0,
            Vector3 top1,
            Vector3 top2,
            Vector3 top3,
            Color color)
        {
            Vector3[] corners =
            {
                bottom0,
                bottom1,
                bottom2,
                bottom3,
                top0,
                top1,
                top2,
                top3
            };
            if (!HasOutwardRing(corners))
            {
                corners = new[]
                {
                    bottom0,
                    bottom3,
                    bottom2,
                    bottom1,
                    top0,
                    top3,
                    top2,
                    top1
                };
            }

            List<Vector3> vertices = new List<Vector3>(36);
            AddQuad(vertices, corners[0], corners[1], corners[5], corners[4]);
            AddQuad(vertices, corners[1], corners[2], corners[6], corners[5]);
            AddQuad(vertices, corners[2], corners[3], corners[7], corners[6]);
            AddQuad(vertices, corners[3], corners[0], corners[4], corners[7]);
            AddQuad(vertices, corners[4], corners[5], corners[6], corners[7]);
            AddQuad(vertices, corners[3], corners[2], corners[1], corners[0]);
            int[] triangles = new int[vertices.Count];
            for (int index = 0; index < triangles.Length; index++)
                triangles[index] = index;
            return MeshPart(
                name,
                parent,
                vertices.ToArray(),
                triangles,
                color);
        }

        public static Transform FrustumPart(
            string name,
            Transform parent,
            float bottomHalfWidth,
            float bottomFrontZ,
            float bottomRearZ,
            float topHalfWidth,
            float topFrontZ,
            float topRearZ,
            float bottomY,
            float topY,
            Color color)
        {
            return OrientedSlabPart(
                name,
                parent,
                new Vector3(-bottomHalfWidth, bottomY, bottomFrontZ),
                new Vector3(bottomHalfWidth, bottomY, bottomFrontZ),
                new Vector3(bottomHalfWidth, bottomY, bottomRearZ),
                new Vector3(-bottomHalfWidth, bottomY, bottomRearZ),
                new Vector3(-topHalfWidth, topY, topFrontZ),
                new Vector3(topHalfWidth, topY, topFrontZ),
                new Vector3(topHalfWidth, topY, topRearZ),
                new Vector3(-topHalfWidth, topY, topRearZ),
                color);
        }

        public static Transform CylinderPart(
            string name,
            Transform parent,
            float radiusTop,
            float radiusBottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color,
            bool openEnded = false)
        {
            if (radiusTop < 0f ||
                radiusBottom < 0f ||
                radiusTop + radiusBottom <= 0f ||
                length <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    "Cylinder dimensions must be positive.");
            if (segments < 3)
                throw new ArgumentOutOfRangeException(
                    nameof(segments),
                    "Cylinder requires at least three segments.");

            List<Vector3> vertices =
                new List<Vector3>(segments * (openEnded ? 6 : 12));
            List<Vector3> normals =
                new List<Vector3>(vertices.Capacity);
            float half = length * 0.5f;
            float slope = (radiusBottom - radiusTop) / length;
            for (int segment = 0; segment < segments; segment++)
            {
                float angle0 = segment * Mathf.PI * 2f / segments;
                float angle1 = (segment + 1) * Mathf.PI * 2f / segments;
                Vector3 bottom0 = Ring(radiusBottom, -half, angle0, axis);
                Vector3 bottom1 = Ring(radiusBottom, -half, angle1, axis);
                Vector3 top0 = Ring(radiusTop, half, angle0, axis);
                Vector3 top1 = Ring(radiusTop, half, angle1, axis);
                Vector3 normal0 = RingNormal(angle0, slope, axis);
                Vector3 normal1 = RingNormal(angle1, slope, axis);
                AddTriangle(
                    vertices,
                    normals,
                    bottom0,
                    top0,
                    top1,
                    normal0,
                    normal0,
                    normal1);
                AddTriangle(
                    vertices,
                    normals,
                    bottom0,
                    top1,
                    bottom1,
                    normal0,
                    normal1,
                    normal1);
                if (openEnded) continue;

                Vector3 topNormal = AxisVector(axis);
                Vector3 bottomNormal = -topNormal;
                Vector3 centerTop = AxisPoint(half, axis);
                Vector3 centerBottom = AxisPoint(-half, axis);
                AddTriangle(
                    vertices,
                    normals,
                    centerTop,
                    top1,
                    top0,
                    topNormal,
                    topNormal,
                    topNormal);
                AddTriangle(
                    vertices,
                    normals,
                    centerBottom,
                    bottom0,
                    bottom1,
                    bottomNormal,
                    bottomNormal,
                    bottomNormal);
            }
            int[] triangles = new int[vertices.Count];
            for (int index = 0; index < triangles.Length; index++)
                triangles[index] = index;
            return MeshPart(
                name,
                parent,
                vertices.ToArray(),
                triangles,
                color,
                normals.ToArray());
        }

        public static Transform LathePart(
            string name,
            Transform parent,
            float[] radii,
            float[] heights,
            int segments,
            float zScale,
            Color color,
            float capRadius = 0f,
            float roofTiltScale = 1f)
        {
            return TankLatheShapeFactory.Build(
                name,
                parent,
                radii,
                heights,
                segments,
                zScale,
                color,
                capRadius,
                roofTiltScale);
        }

        public static Transform TorusPart(
            string name,
            Transform parent,
            float radius,
            float tubeRadius,
            int radialSegments,
            Color color,
            int tubularSegments = 8)
        {
            return TankTorusShapeFactory.Build(
                name,
                parent,
                radius,
                tubeRadius,
                radialSegments,
                tubularSegments,
                color);
        }

        public static Transform PolyTurretPart(
            string name,
            Transform parent,
            Vector2[] plan,
            float height,
            float flare,
            float inset,
            Color color)
        {
            return TankPolyLoftShapeFactory.BuildTurret(
                name,
                parent,
                plan,
                height,
                flare,
                inset,
                color);
        }

        public static Transform PolyLoftPart(
            string name,
            Transform parent,
            Vector2[] plan,
            TankShapeStationValues bottom,
            TankShapeStationValues top,
            TankShapeStationValues inset,
            Color color)
        {
            return TankPolyLoftShapeFactory.BuildLoft(
                name,
                parent,
                plan,
                bottom,
                top,
                inset,
                color);
        }

        public static Transform PolyMultiLoftPart(
            string name,
            Transform parent,
            Vector2[] plan,
            TankShapeLoftRing[] rings,
            Color color)
        {
            return TankPolyLoftShapeFactory.BuildMultiLoft(
                name,
                parent,
                plan,
                rings,
                color);
        }

        private static bool HasOutwardRing(Vector3[] corners)
        {
            Vector3 center = Vector3.zero;
            for (int index = 0; index < corners.Length; index++)
                center += corners[index];
            center /= corners.Length;
            int[][] faces =
            {
                new[] { 0, 1, 5, 4 },
                new[] { 1, 2, 6, 5 },
                new[] { 2, 3, 7, 6 },
                new[] { 3, 0, 4, 7 },
                new[] { 4, 5, 6, 7 },
                new[] { 3, 2, 1, 0 }
            };
            int outward = 0;
            for (int index = 0; index < faces.Length; index++)
            {
                int[] face = faces[index];
                Vector3 normal = Vector3.Cross(
                    corners[face[1]] - corners[face[0]],
                    corners[face[2]] - corners[face[0]]);
                Vector3 faceCenter =
                    (corners[face[0]] +
                     corners[face[1]] +
                     corners[face[2]] +
                     corners[face[3]]) * 0.25f;
                if (Vector3.Dot(normal, faceCenter - center) > 0f)
                    outward++;
            }
            return outward >= 3;
        }

        private static void AddQuad(
            List<Vector3> vertices,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d)
        {
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(a);
            vertices.Add(c);
            vertices.Add(d);
        }

        private static void AddTriangle(
            List<Vector3> vertices,
            List<Vector3> normals,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 normalA,
            Vector3 normalB,
            Vector3 normalC)
        {
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            normals.Add(normalA);
            normals.Add(normalB);
            normals.Add(normalC);
        }

        private static Vector3 Ring(
            float radius,
            float axial,
            float angle,
            TankShapeAxis axis)
        {
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            if (axis == TankShapeAxis.X) return new Vector3(-axial, x, z);
            if (axis == TankShapeAxis.Z) return new Vector3(x, -z, axial);
            return new Vector3(x, axial, z);
        }

        private static Vector3 RingNormal(
            float angle,
            float slope,
            TankShapeAxis axis)
        {
            Vector3 normal =
                new Vector3(Mathf.Cos(angle), slope, Mathf.Sin(angle))
                    .normalized;
            if (axis == TankShapeAxis.X)
                return new Vector3(-normal.y, normal.x, normal.z);
            if (axis == TankShapeAxis.Z)
                return new Vector3(normal.x, -normal.z, normal.y);
            return normal;
        }

        private static Vector3 AxisPoint(
            float axial,
            TankShapeAxis axis)
        {
            if (axis == TankShapeAxis.X) return new Vector3(-axial, 0f, 0f);
            if (axis == TankShapeAxis.Z) return new Vector3(0f, 0f, axial);
            return new Vector3(0f, axial, 0f);
        }

        private static Vector3 AxisVector(TankShapeAxis axis)
        {
            if (axis == TankShapeAxis.X) return Vector3.left;
            if (axis == TankShapeAxis.Z) return Vector3.forward;
            return Vector3.up;
        }

        private static Material CreateMaterial(Color color)
        {
            return new Material(Shader.Find("Standard"))
            {
                color = color
            };
        }
    }
}
