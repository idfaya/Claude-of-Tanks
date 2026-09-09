using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankVariableBaseTurretShapeFactoryTests
    {
        [Test]
        public void InsertsBreakStationsAndBuildsOpenVariableBaseShell()
        {
            GameObject root = new GameObject("VariableBaseTurretTestRoot");
            try
            {
                Transform part = TankVariableBaseTurretShapeFactory.Build(
                    "VariableBaseTurret",
                    root.transform,
                    new[]
                    {
                        new Vector2(-1f, 1f),
                        new Vector2(1f, 1f),
                        new Vector2(1f, -1f),
                        new Vector2(-1f, -1f)
                    },
                    1f,
                    1f,
                    0.8f,
                    z => z > 0f ? 0.2f : 0f,
                    new[] { 0f },
                    Color.white);
                Mesh mesh =
                    part.GetComponent<MeshFilter>().sharedMesh;

                Assert.That(mesh.vertexCount, Is.EqualTo(54));
                Assert.That(mesh.triangles.Length, Is.EqualTo(54));
                Assert.That(mesh.normals.Length, Is.EqualTo(54));
                Assert.That(mesh.uv.Length, Is.EqualTo(54));
                Assert.That(
                    mesh.bounds.min.y,
                    Is.EqualTo(0f).Within(0.0001f));
                Assert.That(
                    mesh.bounds.max.y,
                    Is.EqualTo(1f).Within(0.0001f));
                Assert.That(
                    CountHeight(mesh.vertices, 0.2f),
                    Is.GreaterThan(0));
            }
            finally
            {
                DestroyGenerated(root);
            }
        }

        private static int CountHeight(
            Vector3[] vertices,
            float height)
        {
            int count = 0;
            for (int index = 0; index < vertices.Length; index++)
            {
                if (Mathf.Abs(vertices[index].y - height) < 0.0001f)
                    count++;
            }
            return count;
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
