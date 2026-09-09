using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsXGeometry
    {
        public static Transform Frustum(
            string name,
            Transform parent,
            Vector3 position,
            float bottomHalfWidth,
            float topHalfWidth,
            float bottomHalfLength,
            float topHalfLength,
            float height,
            Color color)
        {
            float bottom = -height * 0.5f;
            float top = height * 0.5f;
            Vector3[] vertices =
            {
                new Vector3(-bottomHalfWidth, bottom, -bottomHalfLength),
                new Vector3(bottomHalfWidth, bottom, -bottomHalfLength),
                new Vector3(bottomHalfWidth, bottom, bottomHalfLength),
                new Vector3(-bottomHalfWidth, bottom, bottomHalfLength),
                new Vector3(-topHalfWidth, top, -topHalfLength),
                new Vector3(topHalfWidth, top, -topHalfLength),
                new Vector3(topHalfWidth, top, topHalfLength),
                new Vector3(-topHalfWidth, top, topHalfLength)
            };
            int[] triangles =
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7
            };
            Mesh mesh = new Mesh { name = name + "-Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.uv =
                TankCamouflageMaterialApplicator.ProjectUvs(
                    vertices,
                    mesh.bounds,
                    Vector3.up);
            GameObject part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.AddComponent<MeshRenderer>().sharedMaterial =
                new Material(Shader.Find("Standard"))
                {
                    color = color
                };
            return part.transform;
        }
    }
}
