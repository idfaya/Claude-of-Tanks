using System;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankWeldedStationLoftShapeFactoryTests
    {
        [Test]
        public void BuildsThreeLevelAsymmetricStations()
        {
            GameObject root = new GameObject("WeldedLoftTestRoot");
            try
            {
                Transform part =
                    TankWeldedStationLoftShapeFactory.Build(
                        "WeldedLoft",
                        root.transform,
                        new[]
                        {
                            Station(0f, -1.2f, 1.3f),
                            Station(0.5f, -1.0f, 1.1f),
                            Station(1f, -0.8f, 0.9f)
                        },
                        Color.white);
                Mesh mesh = part
                    .GetComponent<MeshFilter>()
                    .sharedMesh;

                Assert.That(mesh.vertexCount, Is.EqualTo(96));
                Assert.That(mesh.triangles.Length, Is.EqualTo(96));
                Assert.That(mesh.normals.Length, Is.EqualTo(96));
                Assert.That(mesh.uv.Length, Is.EqualTo(96));
                Assert.That(
                    mesh.bounds.min.x,
                    Is.EqualTo(-1.2f).Within(0.0001f));
                Assert.That(
                    mesh.bounds.max.x,
                    Is.EqualTo(1.3f).Within(0.0001f));
                Assert.That(
                    mesh.bounds.min.z,
                    Is.EqualTo(0f).Within(0.0001f));
                Assert.That(
                    mesh.bounds.max.z,
                    Is.EqualTo(1f).Within(0.0001f));
                Assert.That(
                    CountCapNormals(mesh, 0f, Vector3.back),
                    Is.EqualTo(12));
                Assert.That(
                    CountCapNormals(mesh, 1f, Vector3.forward),
                    Is.EqualTo(12));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void DescendingStationsReverseCapNormals()
        {
            GameObject root = new GameObject("WeldedLoftTestRoot");
            try
            {
                Transform part =
                    TankWeldedStationLoftShapeFactory.Build(
                        "WeldedLoft",
                        root.transform,
                        new[]
                        {
                            Station(1f, -0.8f, 0.9f),
                            Station(0.5f, -1.0f, 1.1f),
                            Station(0f, -1.2f, 1.3f)
                        },
                        Color.white);
                Mesh mesh = part
                    .GetComponent<MeshFilter>()
                    .sharedMesh;

                Assert.That(
                    CountCapNormals(mesh, 1f, Vector3.forward),
                    Is.EqualTo(12));
                Assert.That(
                    CountCapNormals(mesh, 0f, Vector3.back),
                    Is.EqualTo(12));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        [Test]
        public void RejectsMixedStationDirection()
        {
            GameObject root = new GameObject("WeldedLoftTestRoot");
            try
            {
                Assert.Throws<ArgumentException>(() =>
                    TankWeldedStationLoftShapeFactory.Build(
                        "WeldedLoft",
                        root.transform,
                        new[]
                        {
                            Station(0f, -1f, 1f),
                            Station(1f, -1f, 1f),
                            Station(0.5f, -1f, 1f)
                        },
                        Color.white));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static TankWeldedStation Station(
            float z,
            float left,
            float right)
        {
            return new TankWeldedStation(
                z,
                0f,
                1f,
                left,
                right,
                left * 0.9f,
                right * 0.9f,
                left * 0.7f,
                right * 0.7f);
        }

        private static int CountCapNormals(
            Mesh mesh,
            float z,
            Vector3 expected)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int count = 0;
            for (int index = 0; index < vertices.Length; index++)
            {
                if (Mathf.Abs(vertices[index].z - z) < 0.0001f &&
                    Vector3.Dot(normals[index], expected) > 0.99f)
                {
                    count++;
                }
            }
            return count;
        }

        private static void DestroyGenerated(GameObject root)
        {
            MeshFilter filter =
                root.GetComponentInChildren<MeshFilter>(true);
            MeshRenderer renderer =
                root.GetComponentInChildren<MeshRenderer>(true);
            Object.DestroyImmediate(filter.sharedMesh);
            Object.DestroyImmediate(renderer.sharedMaterial);
            Object.DestroyImmediate(root);
        }
    }
}
