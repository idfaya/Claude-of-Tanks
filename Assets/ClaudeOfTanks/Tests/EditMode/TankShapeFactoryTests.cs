using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankShapeFactoryTests
    {
        [Test]
        public void BoxUsesThreeRoundedSegmentPolicy()
        {
            GameObject root = new GameObject("ShapeFactoryTestRoot");
            try
            {
                Transform part = TankShapeFactory.BoxPart(
                    "RoundedBox",
                    root.transform,
                    V(1f, 1f, 1f),
                    Color.white);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(900));
                Assert.That(mesh.normals.Length, Is.EqualTo(900));
                Assert.That(mesh.uv.Length, Is.EqualTo(900));
                AssertVector(mesh.bounds.size, Vector3.one);
                Assert.That(HasRoundedCornerNormal(mesh.normals), Is.True);
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void BoxKeepsThinPartsHardEdged()
        {
            GameObject root = new GameObject("ShapeFactoryTestRoot");
            try
            {
                Transform part = TankShapeFactory.BoxPart(
                    "ThinBox",
                    root.transform,
                    V(1f, 0.05f, 2f),
                    Color.white);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(24));
                Assert.That(mesh.triangles.Length, Is.EqualTo(36));
                Assert.That(mesh.normals.Length, Is.EqualTo(24));
                Assert.That(mesh.uv.Length, Is.EqualTo(24));
                AssertVector(mesh.bounds.size, V(1f, 0.05f, 2f));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void OrientedSlabRepairsMirroredWindingAndKeepsFlatFaces()
        {
            GameObject root = new GameObject("ShapeFactoryTestRoot");
            try
            {
                Transform part = TankShapeFactory.OrientedSlabPart(
                    "MirroredSlab",
                    root.transform,
                    V(1f, 0f, 1f),
                    V(-1f, 0f, 1f),
                    V(-1f, 0f, -1f),
                    V(1f, 0f, -1f),
                    V(1f, 1f, 1f),
                    V(-1f, 1f, 1f),
                    V(-1f, 1f, -1f),
                    V(1f, 1f, -1f),
                    Color.white);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(36));
                Assert.That(mesh.normals.Length, Is.EqualTo(36));
                AssertAllTrianglesFaceOutward(mesh);
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void FrustumPreservesAuthoredExtents()
        {
            GameObject root = new GameObject("ShapeFactoryTestRoot");
            try
            {
                Transform part = TankShapeFactory.FrustumPart(
                    "Frustum",
                    root.transform,
                    2f,
                    4f,
                    -3f,
                    1f,
                    3f,
                    -2f,
                    -1f,
                    2f,
                    Color.white);
                Bounds bounds = Mesh(part).bounds;

                AssertVector(bounds.min, V(-2f, -1f, -3f));
                AssertVector(bounds.max, V(2f, 2f, 4f));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void CylinderSupportsAllSourceAxesAndCapNormals()
        {
            GameObject root = new GameObject("ShapeFactoryTestRoot");
            try
            {
                AssertCylinder(
                    root.transform,
                    TankShapeAxis.X,
                    V(4f, 1f, 1f),
                    Vector3.left);
                AssertCylinder(
                    root.transform,
                    TankShapeAxis.Y,
                    V(1f, 4f, 1f),
                    Vector3.up);
                AssertCylinder(
                    root.transform,
                    TankShapeAxis.Z,
                    V(1f, 1f, 4f),
                    Vector3.forward);
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void CylinderAllowsPointedCone()
        {
            GameObject root = new GameObject("ShapeFactoryTestRoot");
            try
            {
                Transform part = TankShapeFactory.CylinderPart(
                    "Cone",
                    root.transform,
                    0f,
                    0.5f,
                    2f,
                    8,
                    TankShapeAxis.Y,
                    Color.white);
                Mesh mesh = Mesh(part);

                AssertVector(mesh.bounds.size, V(1f, 2f, 1f));
                for (int index = 0; index < mesh.normals.Length; index++)
                {
                    Assert.That(float.IsNaN(mesh.normals[index].x), Is.False);
                    Assert.That(float.IsNaN(mesh.normals[index].y), Is.False);
                    Assert.That(float.IsNaN(mesh.normals[index].z), Is.False);
                }
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void LatheSubdividesProfileAndWritesOneNormalPerVertex()
        {
            GameObject root = new GameObject("ShapeFactoryTestRoot");
            try
            {
                Transform part = TankShapeFactory.LathePart(
                    "Lathe",
                    root.transform,
                    new[] { 1f, 1f },
                    new[] { 0f, 0.11f },
                    8,
                    0.5f,
                    Color.white,
                    2f,
                    0.5f);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(24));
                Assert.That(mesh.normals.Length, Is.EqualTo(24));
                AssertVector(mesh.bounds.size, V(2f, 0.11f, 1f));
                for (int index = 0; index < mesh.normals.Length; index++)
                {
                    Assert.That(
                        mesh.normals[index].magnitude,
                        Is.EqualTo(1f).Within(0.0001f));
                }
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void TorusMatchesThreeSegmentAndUvLayout()
        {
            GameObject root = new GameObject("ShapeFactoryTestRoot");
            try
            {
                Transform part = TankShapeFactory.TorusPart(
                    "Torus",
                    root.transform,
                    1f,
                    0.25f,
                    12,
                    Color.white,
                    8);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(117));
                Assert.That(mesh.triangles.Length, Is.EqualTo(576));
                Assert.That(mesh.normals.Length, Is.EqualTo(117));
                Assert.That(mesh.uv.Length, Is.EqualTo(117));
                AssertVector(mesh.bounds.size, V(2.5f, 2.5f, 0.5f));
                for (int index = 0; index < mesh.normals.Length; index++)
                {
                    Assert.That(
                        mesh.normals[index].magnitude,
                        Is.EqualTo(1f).Within(0.0001f));
                }
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        private static void AssertCylinder(
            Transform root,
            TankShapeAxis axis,
            Vector3 expectedSize,
            Vector3 capAxis)
        {
            Transform part = TankShapeFactory.CylinderPart(
                "Cylinder-" + axis,
                root,
                0.25f,
                0.5f,
                4f,
                8,
                axis,
                Color.white);
            Mesh mesh = Mesh(part);
            Assert.That(mesh.vertexCount, Is.EqualTo(96));
            AssertVector(mesh.bounds.size, expectedSize);
            Assert.That(
                HasNormal(mesh.normals, capAxis),
                Is.True,
                axis + " positive cap");
            Assert.That(
                HasNormal(mesh.normals, -capAxis),
                Is.True,
                axis + " negative cap");
            AssertRadiusAtAxis(mesh, axis, capAxis * 2f, 0.25f);
            AssertRadiusAtAxis(mesh, axis, -capAxis * 2f, 0.5f);
        }

        private static void AssertRadiusAtAxis(
            Mesh mesh,
            TankShapeAxis axis,
            Vector3 capCenter,
            float expectedRadius)
        {
            Vector3[] vertices = mesh.vertices;
            float maximum = 0f;
            for (int index = 0; index < vertices.Length; index++)
            {
                Vector3 delta = vertices[index] - capCenter;
                float axial = axis == TankShapeAxis.X
                    ? Mathf.Abs(delta.x)
                    : axis == TankShapeAxis.Y
                        ? Mathf.Abs(delta.y)
                        : Mathf.Abs(delta.z);
                if (axial > 0.0001f) continue;
                float radius = axis == TankShapeAxis.X
                    ? Mathf.Sqrt(delta.y * delta.y + delta.z * delta.z)
                    : axis == TankShapeAxis.Y
                        ? Mathf.Sqrt(delta.x * delta.x + delta.z * delta.z)
                        : Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y);
                maximum = Mathf.Max(maximum, radius);
            }
            Assert.That(
                maximum,
                Is.EqualTo(expectedRadius).Within(0.0001f),
                axis + " cap radius");
        }

        private static void AssertAllTrianglesFaceOutward(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            for (int index = 0; index < triangles.Length; index += 3)
            {
                Vector3 a = vertices[triangles[index]];
                Vector3 b = vertices[triangles[index + 1]];
                Vector3 c = vertices[triangles[index + 2]];
                Vector3 normal = Vector3.Cross(b - a, c - a);
                Vector3 center = (a + b + c) / 3f;
                Assert.That(
                    Vector3.Dot(normal, center - mesh.bounds.center),
                    Is.GreaterThan(0f),
                    "triangle " + (index / 3));
            }
        }

        private static bool HasNormal(
            Vector3[] normals,
            Vector3 expected)
        {
            for (int index = 0; index < normals.Length; index++)
            {
                if (Vector3.Dot(normals[index], expected) > 0.9999f)
                    return true;
            }
            return false;
        }

        private static bool HasRoundedCornerNormal(Vector3[] normals)
        {
            for (int index = 0; index < normals.Length; index++)
            {
                Vector3 normal = normals[index];
                if (Mathf.Abs(normal.x) > 0.5f &&
                    Mathf.Abs(normal.y) > 0.5f &&
                    Mathf.Abs(normal.z) > 0.5f)
                {
                    return true;
                }
            }
            return false;
        }

        private static Mesh Mesh(Transform part)
        {
            return part.GetComponent<MeshFilter>().sharedMesh;
        }

        private static void AssertVector(
            Vector3 actual,
            Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
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

        private static Vector3 V(
            float x,
            float y,
            float z)
        {
            return new Vector3(x, y, z);
        }
    }
}
