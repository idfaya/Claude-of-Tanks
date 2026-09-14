using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal enum MapMaterialRole
    {
        Terrain,
        GroundVariation,
        RoadCasing,
        Road,
        Marsh,
        Water,
        Ice,
        Crater,
        StructureBody,
        StructureRoof,
        StructureDetail,
        StructureWall,
        StructureCover,
        Rock,
        Bark,
        Broadleaf,
        Conifer,
        Palm,
        Birch
    }

    internal static class MapMaterialFactory
    {
        public static Material Create(
            Color color,
            MapMaterialRole role,
            string seed)
        {
            bool foliageCard = IsFoliageCard(role);
            Shader shader = Shader.Find(foliageCard
                ? "ClaudeOfTanks/MapFoliageWindCutout"
                : "Standard");
            if (shader == null && foliageCard)
                shader = Shader.Find("Unlit/Transparent Cutout");
            if (shader == null)
                shader = Shader.Find("Standard");
            Texture2D texture = Texture(color, role, seed);
            Material material = new Material(shader)
            {
                color = Color.white,
                mainTexture = texture
            };
            material.mainTextureScale = TextureScale(role);
            if (!foliageCard)
                AssignNormalMap(material, role, seed);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", Smoothness(role));
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", Metallic(role));
            if (foliageCard)
            {
                if (material.HasProperty("_Mode"))
                    material.SetFloat("_Mode", 1f);
                if (material.HasProperty("_Cutoff"))
                    material.SetFloat("_Cutoff", role == MapMaterialRole.Palm ? 0.55f : 0.38f);
                if (material.HasProperty("_SrcBlend"))
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                if (material.HasProperty("_DstBlend"))
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                if (material.HasProperty("_ZWrite"))
                    material.SetInt("_ZWrite", 1);
                if (material.HasProperty("_Cull"))
                    material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor(
                        "_EmissionColor",
                        color * (role == MapMaterialRole.Conifer ? 0.18f : 0.24f));
                }
                if (material.HasProperty("_WindStrength"))
                    material.SetFloat("_WindStrength", WindStrength(role));
                if (material.HasProperty("_WindSpeed"))
                    material.SetFloat("_WindSpeed", WindSpeed(role));
                if (material.HasProperty("_WindScale"))
                    material.SetFloat("_WindScale", 0.045f);
                if (material.HasProperty("_WindPhase"))
                    material.SetFloat("_WindPhase", (StableHash(seed) & 1023) * 0.006135923f);
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            }
            return material;
        }

        public static void Destroy(Material material)
        {
            if (material == null) return;
            Texture texture = material.mainTexture;
            if (texture != null &&
                texture.name.StartsWith(
                    "MapProcedural-",
                    System.StringComparison.Ordinal))
            {
                DestroyObject(texture);
            }
            if (material.HasProperty("_BumpMap"))
            {
                Texture normal = material.GetTexture("_BumpMap");
                if (normal != null &&
                    normal.name.StartsWith(
                        "MapProceduralNormal-",
                        System.StringComparison.Ordinal))
                {
                    DestroyObject(normal);
                }
            }
            DestroyObject(material);
        }

        private static void AssignNormalMap(
            Material material,
            MapMaterialRole role,
            string seed)
        {
            if (!material.HasProperty("_BumpMap")) return;
            if (!UsesNormalMap(role)) return;
            Texture2D normal = NormalTexture(role, seed);
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
            if (material.HasProperty("_BumpScale"))
                material.SetFloat("_BumpScale", NormalStrength(role));
        }

        private static Texture2D Texture(
            Color color,
            MapMaterialRole role,
            string seed)
        {
            int size = role == MapMaterialRole.Bark ? 96 : 64;
            Texture2D texture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                true)
            {
                name = "MapProcedural-" + role + "-" + seed,
                wrapMode = IsFoliageCard(role)
                    ? TextureWrapMode.Clamp
                    : TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear
            };
            Color[] pixels = new Color[size * size];
            int hash = StableHash(role + ":" + seed);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float v = y / (float)size;
                    pixels[y * size + x] = Sample(
                        color,
                        role,
                        u,
                        v,
                        hash);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(true, true);
            return texture;
        }

        private static Texture2D NormalTexture(
            MapMaterialRole role,
            string seed)
        {
            const int size = 64;
            Texture2D texture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                true,
                true)
            {
                name = "MapProceduralNormal-" + role + "-" + seed,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear
            };
            Color[] pixels = new Color[size * size];
            int hash = StableHash("normal:" + role + ":" + seed);
            float strength = NormalStrength(role);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float v = y / (float)size;
                    float hL = HeightSample(role, u - 1f / size, v, hash);
                    float hR = HeightSample(role, u + 1f / size, v, hash);
                    float hD = HeightSample(role, u, v - 1f / size, hash);
                    float hU = HeightSample(role, u, v + 1f / size, hash);
                    Vector3 n = new Vector3(
                        (hL - hR) * strength,
                        (hD - hU) * strength,
                        1f).normalized;
                    pixels[y * size + x] = new Color(
                        n.x * 0.5f + 0.5f,
                        n.y * 0.5f + 0.5f,
                        n.z * 0.5f + 0.5f,
                        1f);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(true, true);
            return texture;
        }

        private static float HeightSample(
            MapMaterialRole role,
            float u,
            float v,
            int seed)
        {
            float ripple = Mathf.Sin((u * 23f + v * 7f) * Mathf.PI);
            float cross = Mathf.Sin((u - v) * Mathf.PI * 11f);
            float noise = Noise(u * 3.1f, v * 3.1f, seed);
            float fine = Noise(u * 11.7f, v * 11.7f, seed ^ 0x45d9);
            switch (role)
            {
                case MapMaterialRole.Water:
                    return ripple * 0.45f + cross * 0.22f;
                case MapMaterialRole.Ice:
                    return cross * 0.55f + fine * 0.18f;
                case MapMaterialRole.Road:
                case MapMaterialRole.RoadCasing:
                    return ripple * 0.18f + noise * 0.32f;
                case MapMaterialRole.StructureBody:
                case MapMaterialRole.StructureWall:
                    return Mathf.Abs(ripple) * 0.22f + noise * 0.26f;
                case MapMaterialRole.StructureRoof:
                case MapMaterialRole.StructureCover:
                    return Mathf.Abs(cross) * 0.28f + fine * 0.25f;
                case MapMaterialRole.Bark:
                    return Mathf.Abs(Mathf.Sin(u * Mathf.PI * 38f)) * 0.5f +
                        noise * 0.22f;
                default:
                    return noise * 0.34f + fine * 0.16f;
            }
        }

        private static Color Sample(
            Color baseColor,
            MapMaterialRole role,
            float u,
            float v,
            int seed)
        {
            float noise = Noise(u, v, seed);
            float fine = Noise(u * 5.3f, v * 5.3f, seed ^ 0x5f37);
            float stripe = Mathf.Sin((u + seed * 0.00013f) *
                Mathf.PI * 26f);
            float value = 1f;
            switch (role)
            {
                case MapMaterialRole.Terrain:
                    value = 0.82f + noise * 0.24f +
                        fine * 0.08f;
                    break;
                case MapMaterialRole.GroundVariation:
                    value = 0.72f + noise * 0.22f;
                    break;
                case MapMaterialRole.Road:
                    value = 0.58f + noise * 0.18f +
                        Mathf.Abs(stripe) * 0.08f;
                    break;
                case MapMaterialRole.RoadCasing:
                    value = 0.68f + fine * 0.18f;
                    break;
                case MapMaterialRole.Marsh:
                    value = 0.62f + noise * 0.16f;
                    break;
                case MapMaterialRole.Water:
                    value = 0.86f + Mathf.Sin(
                        (u + v) * Mathf.PI * 18f) * 0.05f;
                    break;
                case MapMaterialRole.Ice:
                    value = 0.9f + Mathf.Sin(
                        (u - v) * Mathf.PI * 14f) * 0.08f;
                    break;
                case MapMaterialRole.Crater:
                    value = 0.5f + noise * 0.2f;
                    break;
                case MapMaterialRole.StructureBody:
                    value = 0.74f + noise * 0.18f +
                        Mathf.Abs(stripe) * 0.05f;
                    break;
                case MapMaterialRole.StructureRoof:
                    value = 0.58f + fine * 0.22f;
                    break;
                case MapMaterialRole.StructureDetail:
                    value = 0.48f + Mathf.Abs(stripe) * 0.22f;
                    break;
                case MapMaterialRole.StructureWall:
                    value = 0.65f + noise * 0.18f;
                    break;
                case MapMaterialRole.StructureCover:
                    value = 0.56f + fine * 0.26f;
                    break;
                case MapMaterialRole.Rock:
                    value = 0.62f + noise * 0.18f +
                        fine * 0.14f;
                    break;
                case MapMaterialRole.Bark:
                    value = 0.54f + noise * 0.16f -
                        Mathf.Max(0f, stripe) * 0.22f;
                    break;
                case MapMaterialRole.Broadleaf:
                    value = 0.62f + noise * 0.25f +
                        fine * 0.15f;
                    break;
                case MapMaterialRole.Conifer:
                    value = 0.52f + noise * 0.18f +
                        fine * 0.1f;
                    break;
                case MapMaterialRole.Palm:
                    value = 0.64f + Mathf.Abs(stripe) * 0.12f +
                        fine * 0.1f;
                    break;
                case MapMaterialRole.Birch:
                    value = 0.76f + noise * 0.14f -
                        Mathf.Max(0f, stripe) * 0.16f;
                    break;
            }
            Color color = baseColor * Mathf.Clamp(value, 0.25f, 1.35f);
            color.a = IsFoliageCard(role)
                ? FoliageAlpha(role, u, v, noise, fine, seed)
                : baseColor.a;
            return color;
        }

        private static bool IsFoliageCard(MapMaterialRole role)
        {
            return role == MapMaterialRole.Broadleaf ||
                role == MapMaterialRole.Conifer ||
                role == MapMaterialRole.Palm ||
                role == MapMaterialRole.Birch;
        }

        private static bool UsesNormalMap(MapMaterialRole role)
        {
            return role != MapMaterialRole.GroundVariation &&
                role != MapMaterialRole.Crater;
        }

        private static float FoliageAlpha(
            MapMaterialRole role,
            float u,
            float v,
            float noise,
            float fine,
            int seed)
        {
            float x = u * 2f - 1f;
            float y = v * 2f - 1f;
            float alpha = 1f;
            switch (role)
            {
                case MapMaterialRole.Broadleaf:
                    {
                        float radius = Mathf.Sqrt(x * x * 0.82f + y * y * 1.25f);
                        if (radius > 0.98f) return 0f;
                        float edge = Mathf.SmoothStep(0.98f, 0.52f, radius);
                        float clumps = Mathf.Sin((u * 9f + noise * 0.7f) * Mathf.PI) *
                            Mathf.Sin((v * 11f + fine * 0.5f) * Mathf.PI);
                        alpha = edge * (0.62f + Mathf.Abs(clumps) * 0.42f);
                        if (Noise(u * 2.2f, v * 2.2f, seed ^ 0x7499) > 0.58f &&
                            radius > 0.35f)
                        {
                            alpha *= 0.25f;
                        }
                        break;
                    }
                case MapMaterialRole.Conifer:
                    {
                        float tier = Mathf.Abs(Mathf.Sin(v * Mathf.PI * 7f + noise));
                        float width = Mathf.Lerp(0.06f, 0.76f, 1f - Mathf.Abs(y)) *
                            (0.82f + tier * 0.28f);
                        alpha = Mathf.SmoothStep(width, width * 0.55f, Mathf.Abs(x));
                        alpha *= 0.72f + fine * 0.22f;
                        break;
                    }
                case MapMaterialRole.Palm:
                    {
                        float spine = Mathf.Abs(x + Mathf.Sin(v * Mathf.PI) * 0.08f);
                        float taper = 1f - Mathf.Abs(y);
                        float width = 0.06f + taper * 0.54f;
                        float ribs = Mathf.Abs(Mathf.Sin(v * Mathf.PI * 18f));
                        alpha = Mathf.SmoothStep(width, width * 0.35f, spine) *
                            (0.64f + ribs * 0.48f);
                        break;
                    }
                case MapMaterialRole.Birch:
                    {
                        float crown = Mathf.SmoothStep(1f, 0.52f, Mathf.Sqrt(
                            x * x * 0.72f + y * y * 1.12f));
                        float twigA = 1f - Mathf.Abs(Mathf.Sin((u * 4.5f + v * 7f) * Mathf.PI));
                        float twigB = 1f - Mathf.Abs(Mathf.Sin((u * 7.5f - v * 5f) * Mathf.PI));
                        alpha = crown * Mathf.Max(twigA, twigB);
                        alpha *= 0.95f + fine * 0.18f;
                        if (alpha < 0.18f) alpha = 0f;
                        break;
                    }
            }
            return Mathf.Clamp01(alpha);
        }

        private static Vector2 TextureScale(
            MapMaterialRole role)
        {
            switch (role)
            {
                case MapMaterialRole.Terrain:
                case MapMaterialRole.GroundVariation:
                    return new Vector2(24f, 24f);
                case MapMaterialRole.Road:
                case MapMaterialRole.RoadCasing:
                    return new Vector2(12f, 3f);
                case MapMaterialRole.Bark:
                    return new Vector2(1.6f, 8f);
                case MapMaterialRole.Birch:
                    return new Vector2(1.2f, 6f);
                case MapMaterialRole.StructureBody:
                case MapMaterialRole.StructureWall:
                    return new Vector2(4f, 4f);
                default:
                    return new Vector2(2f, 2f);
            }
        }

        private static float Smoothness(MapMaterialRole role)
        {
            switch (role)
            {
                case MapMaterialRole.Water: return 0.86f;
                case MapMaterialRole.Ice: return 0.68f;
                case MapMaterialRole.Road: return 0.18f;
                case MapMaterialRole.StructureRoof: return 0.22f;
                case MapMaterialRole.StructureDetail: return 0.12f;
                default: return 0.06f;
            }
        }

        private static float Metallic(MapMaterialRole role)
        {
            return role == MapMaterialRole.Water ||
                role == MapMaterialRole.Ice
                    ? 0.08f
                    : 0f;
        }

        private static float NormalStrength(MapMaterialRole role)
        {
            switch (role)
            {
                case MapMaterialRole.Water: return 0.36f;
                case MapMaterialRole.Ice: return 0.28f;
                case MapMaterialRole.Road: return 0.18f;
                case MapMaterialRole.StructureBody:
                case MapMaterialRole.StructureWall:
                    return 0.22f;
                case MapMaterialRole.StructureRoof:
                case MapMaterialRole.StructureCover:
                    return 0.28f;
                case MapMaterialRole.Bark:
                    return 0.42f;
                default:
                    return 0.16f;
            }
        }

        private static float WindStrength(MapMaterialRole role)
        {
            switch (role)
            {
                case MapMaterialRole.Palm: return 0.34f;
                case MapMaterialRole.Conifer: return 0.18f;
                case MapMaterialRole.Birch: return 0.26f;
                default: return 0.24f;
            }
        }

        private static float WindSpeed(MapMaterialRole role)
        {
            switch (role)
            {
                case MapMaterialRole.Palm: return 1.35f;
                case MapMaterialRole.Conifer: return 1.05f;
                case MapMaterialRole.Birch: return 1.75f;
                default: return 1.55f;
            }
        }

        private static float Noise(float x, float y, int seed)
        {
            int ix = Mathf.FloorToInt(x * 64f);
            int iy = Mathf.FloorToInt(y * 64f);
            int hash = seed;
            hash = hash * 73856093 ^ ix * 19349663;
            hash ^= iy * 83492791;
            hash ^= hash >> 13;
            hash *= 1274126177;
            return ((hash & 0xffff) / 65535f) * 2f - 1f;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < value.Length; i++)
                    hash = hash * 31 + value[i];
                return hash;
            }
        }

        private static void DestroyObject(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Object.Destroy(value);
            else Object.DestroyImmediate(value);
        }
    }
}
