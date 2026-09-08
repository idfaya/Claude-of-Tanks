using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleImpactDecalAssets : IDisposable
    {
        public BattleImpactDecalAssets()
        {
            Atlas = BuildAtlas();
            Material = BuildMaterial(Atlas);
            PenetrationMesh = BuildMesh(
                AtlasUv(0),
                AtlasUv(1),
                true);
            GougeMesh = BuildMesh(
                AtlasUv(2),
                default,
                false);
            ScuffMesh = BuildMesh(
                AtlasUv(0),
                default,
                false);
            ScorchMesh = BuildMesh(
                AtlasUv(3),
                default,
                false);
        }

        public Material Material { get; }
        public Texture2D Atlas { get; }
        public Mesh PenetrationMesh { get; }
        public Mesh GougeMesh { get; }
        public Mesh ScuffMesh { get; }
        public Mesh ScorchMesh { get; }

        public void Dispose()
        {
            Release(Material);
            Release(Atlas);
            Release(PenetrationMesh);
            Release(GougeMesh);
            Release(ScuffMesh);
            Release(ScorchMesh);
        }

        private static Texture2D BuildAtlas()
        {
            const int size = 128;
            const int cell = size / 2;
            Texture2D texture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                true)
            {
                name = "ImpactDecalAtlas",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            Color32[] pixels = new Color32[size * size];
            for (int cellY = 0; cellY < 2; cellY++)
            {
                for (int cellX = 0; cellX < 2; cellX++)
                {
                    int family = cellY * 2 + cellX;
                    PaintCell(
                        pixels,
                        size,
                        cell,
                        cellX,
                        cellY,
                        family);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(true, true);
            return texture;
        }

        private static void PaintCell(
            Color32[] pixels,
            int atlasSize,
            int cellSize,
            int cellX,
            int cellY,
            int family)
        {
            for (int y = 0; y < cellSize; y++)
            {
                for (int x = 0; x < cellSize; x++)
                {
                    float nx =
                        (x + 0.5f) / cellSize * 2f - 1f;
                    float ny =
                        (y + 0.5f) / cellSize * 2f - 1f;
                    float radius =
                        Mathf.Sqrt(nx * nx + ny * ny);
                    float angle = Mathf.Atan2(ny, nx);
                    float noise =
                        Mathf.Sin(angle * 7f) * 0.055f +
                        Mathf.Sin(
                            nx * 23f + ny * 17f) * 0.035f;
                    Color color = Pixel(
                        family,
                        nx,
                        ny,
                        radius,
                        noise);
                    int px = cellX * cellSize + x;
                    int py = cellY * cellSize + y;
                    pixels[py * atlasSize + px] = color;
                }
            }
        }

        private static Color Pixel(
            int family,
            float x,
            float y,
            float radius,
            float noise)
        {
            if (family == 0)
            {
                float alpha = Mathf.Clamp01(
                    (0.92f + noise - radius) * 5.5f);
                float center = Mathf.Clamp01(
                    (0.38f - radius) * 8f);
                return new Color(
                    0.055f + center * 0.02f,
                    0.045f,
                    0.035f,
                    alpha);
            }
            if (family == 1)
            {
                float ring = Mathf.Clamp01(
                    1f - Mathf.Abs(radius - 0.52f) * 13f);
                float rays = Mathf.Clamp01(
                    Mathf.Sin(
                        Mathf.Atan2(y, x) * 9f) *
                    0.5f + 0.5f);
                float alpha = Mathf.Max(
                    ring,
                    rays *
                    Mathf.Clamp01(
                        (0.95f - radius) * 2f) *
                    0.42f);
                return new Color(
                    1f,
                    0.38f + ring * 0.28f,
                    0.08f,
                    alpha * 0.9f);
            }
            if (family == 2)
            {
                float width =
                    Mathf.Lerp(0.1f, 0.34f, (y + 1f) * 0.5f);
                float alpha = Mathf.Clamp01(
                    (width - Mathf.Abs(x)) * 12f) *
                    Mathf.Clamp01(
                        (1f - Mathf.Abs(y)) * 5f);
                float edge = Mathf.Clamp01(
                    Mathf.Abs(x) /
                    Mathf.Max(width, 0.01f));
                return new Color(
                    Mathf.Lerp(0.76f, 0.22f, edge),
                    Mathf.Lerp(0.72f, 0.18f, edge),
                    Mathf.Lerp(0.62f, 0.13f, edge),
                    alpha);
            }
            float scorch = Mathf.Clamp01(
                (0.96f + noise - radius) * 3.2f);
            return new Color(
                0.045f,
                0.036f,
                0.027f,
                scorch * 0.78f);
        }

        private static Rect AtlasUv(int family)
        {
            int x = family & 1;
            int y = family >> 1;
            const float inset = 0.012f;
            return new Rect(
                x * 0.5f + inset,
                y * 0.5f + inset,
                0.5f - inset * 2f,
                0.5f - inset * 2f);
        }

        private static Mesh BuildMesh(
            Rect coreUv,
            Rect accentUv,
            bool accent)
        {
            int count = accent ? 8 : 4;
            Vector3[] vertices = new Vector3[count];
            Vector2[] uvs = new Vector2[count];
            WriteQuad(vertices, uvs, 0, coreUv, 0f);
            int[] triangles;
            if (accent)
            {
                WriteQuad(
                    vertices,
                    uvs,
                    4,
                    accentUv,
                    0.002f);
                triangles = new[]
                {
                    0, 2, 1, 0, 3, 2,
                    4, 6, 5, 4, 7, 6
                };
            }
            else
            {
                triangles = new[]
                    { 0, 2, 1, 0, 3, 2 };
            }
            Mesh mesh = new Mesh
            {
                name = accent
                    ? "PenetrationDecalMesh"
                    : "ImpactDecalMesh"
            };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void WriteQuad(
            Vector3[] vertices,
            Vector2[] uvs,
            int offset,
            Rect uv,
            float depth)
        {
            vertices[offset] =
                new Vector3(-0.5f, -0.5f, depth);
            vertices[offset + 1] =
                new Vector3(0.5f, -0.5f, depth);
            vertices[offset + 2] =
                new Vector3(0.5f, 0.5f, depth);
            vertices[offset + 3] =
                new Vector3(-0.5f, 0.5f, depth);
            uvs[offset] = new Vector2(uv.xMin, uv.yMin);
            uvs[offset + 1] =
                new Vector2(uv.xMax, uv.yMin);
            uvs[offset + 2] =
                new Vector2(uv.xMax, uv.yMax);
            uvs[offset + 3] =
                new Vector2(uv.xMin, uv.yMax);
        }

        private static Material BuildMaterial(Texture texture)
        {
            Shader shader = Shader.Find("Unlit/Transparent") ??
                Shader.Find("Standard");
            Material material = new Material(shader)
            {
                name = "ImpactDecalAtlasMaterial",
                mainTexture = texture,
                color = Color.white,
                renderQueue = 3000
            };
            material.SetInt("_ZWrite", 0);
            material.SetInt("_Cull", 0);
            return material;
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(value);
            else
                UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
