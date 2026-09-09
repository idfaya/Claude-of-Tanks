using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankPolyLoftShapeFactoryTests
    {
        private static readonly Vector2[] SquarePlan =
        {
            new Vector2(-1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, -1f),
            new Vector2(-1f, -1f)
        };

        [Test]
        public void PolyTurretBuildsSidesAndTopWithoutBottomCap()
        {
            GameObject root = new GameObject("PolyLoftTestRoot");
            try
            {
                Transform part = TankShapeFactory.PolyTurretPart(
                    "PolyTurret",
                    root.transform,
                    SquarePlan,
                    1f,
                    1.2f,
                    0.8f,
                    Color.white);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(36));
                Assert.That(mesh.triangles.Length, Is.EqualTo(36));
                Assert.That(mesh.uv.Length, Is.EqualTo(36));
                Assert.That(CountHorizontalTriangles(mesh, 0f), Is.EqualTo(0));
                Assert.That(CountHorizontalTriangles(mesh, 1f), Is.EqualTo(4));
                AssertBounds(
                    mesh.bounds,
                    new Vector3(-1.2f, 0f, -1.2f),
                    new Vector3(1.2f, 1f, 1.2f));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void PolyLoftAcceptsPerStationHeightsAndInsets()
        {
            GameObject root = new GameObject("PolyLoftTestRoot");
            try
            {
                Transform part = TankShapeFactory.PolyLoftPart(
                    "PolyLoft",
                    root.transform,
                    SquarePlan,
                    new[] { 0f, 0.1f, 0.2f, 0.3f },
                    new[] { 1f, 1.1f, 1.2f, 1.3f },
                    new[] { 0.5f, 0.6f, 0.7f, 0.8f },
                    Color.white);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(36));
                Assert.That(mesh.normals.Length, Is.EqualTo(36));
                Assert.That(mesh.bounds.min.y, Is.EqualTo(0f).Within(0.0001f));
                Assert.That(mesh.bounds.max.y, Is.EqualTo(1.3f).Within(0.0001f));
                Assert.That(
                    Count(
                        mesh.vertices,
                        new Vector3(-0.5f, 1f, 0.5f)),
                    Is.GreaterThan(0));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void PolyMultiLoftBuildsIntermediateRingAndBothCaps()
        {
            GameObject root = new GameObject("PolyLoftTestRoot");
            try
            {
                Transform part = TankShapeFactory.PolyMultiLoftPart(
                    "PolyMultiLoft",
                    root.transform,
                    SquarePlan,
                    new[]
                    {
                        new TankShapeLoftRing(
                            0f,
                            1f,
                            Vector2.zero,
                            -0.1f),
                        new TankShapeLoftRing(
                            0.5f,
                            0.8f,
                            new Vector2(0.1f, 0.2f)),
                        new TankShapeLoftRing(
                            1f,
                            0.6f,
                            new Vector2(0.2f, 0.4f),
                            1.1f)
                    },
                    Color.white);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(72));
                Assert.That(mesh.triangles.Length, Is.EqualTo(72));
                Assert.That(mesh.normals.Length, Is.EqualTo(72));
                Assert.That(mesh.uv.Length, Is.EqualTo(72));
                Assert.That(
                    Count(mesh.vertices, new Vector3(0f, -0.1f, 0f)),
                    Is.EqualTo(4));
                Assert.That(
                    Count(mesh.vertices, new Vector3(0.2f, 1.1f, 0.4f)),
                    Is.EqualTo(4));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        private static int CountHorizontalTriangles(
            Mesh mesh,
            float height)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            int count = 0;
            for (int index = 0; index < triangles.Length; index += 3)
            {
                if (Mathf.Abs(vertices[triangles[index]].y - height) < 0.0001f &&
                    Mathf.Abs(vertices[triangles[index + 1]].y - height) <
                        0.0001f &&
                    Mathf.Abs(vertices[triangles[index + 2]].y - height) <
                        0.0001f)
                {
                    count++;
                }
            }
            return count;
        }

        private static int Count(
            Vector3[] vertices,
            Vector3 expected)
        {
            int count = 0;
            for (int index = 0; index < vertices.Length; index++)
            {
                if ((vertices[index] - expected).sqrMagnitude < 0.0000001f)
                    count++;
            }
            return count;
        }

        private static Mesh Mesh(Transform part)
        {
            return part.GetComponent<MeshFilter>().sharedMesh;
        }

        private static void AssertBounds(
            Bounds bounds,
            Vector3 minimum,
            Vector3 maximum)
        {
            Assert.That(bounds.min.x, Is.EqualTo(minimum.x).Within(0.0001f));
            Assert.That(bounds.min.y, Is.EqualTo(minimum.y).Within(0.0001f));
            Assert.That(bounds.min.z, Is.EqualTo(minimum.z).Within(0.0001f));
            Assert.That(bounds.max.x, Is.EqualTo(maximum.x).Within(0.0001f));
            Assert.That(bounds.max.y, Is.EqualTo(maximum.y).Within(0.0001f));
            Assert.That(bounds.max.z, Is.EqualTo(maximum.z).Within(0.0001f));
        }

        private static void DestroyGenerated(GameObject root)
        {
            MeshFilter[] filters =
                root.GetComponentsInChildren<MeshFilter>(true);
            for (int index = 0; index < filters.Length; index++)
                Object.DestroyImmediate(filters[index].sharedMesh);
            MeshRenderer[] renderers =
                root.GetComponentsInChildren<MeshRenderer>(true);
            for (int index = 0; index < renderers.Length; index++)
                Object.DestroyImmediate(renderers[index].sharedMaterial);
            Object.DestroyImmediate(root);
        }
    }
}
