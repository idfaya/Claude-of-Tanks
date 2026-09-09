using System;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankHullLoftShapeFactoryTests
    {
        [Test]
        public void BuildsSubdividedUpperAndLowerStationSlabs()
        {
            GameObject root = new GameObject("HullLoftTestRoot");
            try
            {
                Transform part = TankHullLoftShapeFactory.Build(
                    "HullLoft",
                    root.transform,
                    Curve(0f, 2f, 1f, 2f),
                    Curve(0f, 0f, 1f, 0f),
                    Curve(0f, 2f, 1f, 2f),
                    Curve(0f, 1f, 1f, 1f),
                    1f,
                    Color.white);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(216));
                Assert.That(mesh.triangles.Length, Is.EqualTo(216));
                Assert.That(mesh.normals.Length, Is.EqualTo(216));
                Assert.That(mesh.uv.Length, Is.EqualTo(216));
                AssertBounds(
                    mesh.bounds,
                    new Vector3(-2f, 0f, 0f),
                    new Vector3(2f, 2f, 1f));
                Assert.That(
                    CountAtZ(mesh.vertices, 1f / 3f),
                    Is.GreaterThan(0));
                Assert.That(
                    CountAtZ(mesh.vertices, 2f / 3f),
                    Is.GreaterThan(0));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void ProfileSponsonAddsItsKnotsAndInterpolates()
        {
            GameObject root = new GameObject("HullLoftTestRoot");
            try
            {
                Transform part = TankHullLoftShapeFactory.Build(
                    "HullLoft",
                    root.transform,
                    Curve(0f, 2f, 1f, 2f),
                    Curve(0f, 0f, 1f, 0f),
                    Curve(0f, 2f, 1f, 2f),
                    Curve(0f, 1f, 1f, 1f),
                    new[]
                    {
                        new TankHullProfilePoint(0f, 0.5f),
                        new TankHullProfilePoint(0.5f, 0.7f),
                        new TankHullProfilePoint(1f, 1f)
                    },
                    Color.white);
                Mesh mesh = Mesh(part);

                Assert.That(mesh.vertexCount, Is.EqualTo(288));
                Assert.That(
                    Count(
                        mesh.vertices,
                        new Vector3(-2f, 0.7f, 0.5f)),
                    Is.GreaterThan(0));
                Assert.That(
                    CountAtZ(mesh.vertices, 0.25f),
                    Is.GreaterThan(0));
                Assert.That(
                    CountAtZ(mesh.vertices, 0.75f),
                    Is.GreaterThan(0));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void RejectsUnsortedProfile()
        {
            GameObject root = new GameObject("HullLoftTestRoot");
            try
            {
                Assert.Throws<ArgumentException>(() =>
                    TankHullLoftShapeFactory.Build(
                        "HullLoft",
                        root.transform,
                        Curve(1f, 2f, 0f, 2f),
                        Curve(0f, 0f, 1f, 0f),
                        Curve(0f, 2f, 1f, 2f),
                        Curve(0f, 1f, 1f, 1f),
                        1f,
                        Color.white));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static TankHullProfilePoint[] Curve(
            float z0,
            float value0,
            float z1,
            float value1)
        {
            return new[]
            {
                new TankHullProfilePoint(z0, value0),
                new TankHullProfilePoint(z1, value1)
            };
        }

        private static int CountAtZ(
            Vector3[] vertices,
            float z)
        {
            int count = 0;
            for (int index = 0; index < vertices.Length; index++)
            {
                if (Mathf.Abs(vertices[index].z - z) < 0.0001f)
                    count++;
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
            Assert.That(
                bounds.min.x,
                Is.EqualTo(minimum.x).Within(0.0001f));
            Assert.That(
                bounds.min.y,
                Is.EqualTo(minimum.y).Within(0.0001f));
            Assert.That(
                bounds.min.z,
                Is.EqualTo(minimum.z).Within(0.0001f));
            Assert.That(
                bounds.max.x,
                Is.EqualTo(maximum.x).Within(0.0001f));
            Assert.That(
                bounds.max.y,
                Is.EqualTo(maximum.y).Within(0.0001f));
            Assert.That(
                bounds.max.z,
                Is.EqualTo(maximum.z).Within(0.0001f));
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
