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
        Birch,
        Horizon,
        Cloud
    }

    internal static class MapMaterialFactory
    {
        internal const string SplatAppliedProperty = "_CotSplatApplied";
        internal const string SplatTintAProperty = "_CotSplatTintA";
        internal const string SplatTintBProperty = "_CotSplatTintB";
        internal const string SplatTintCProperty = "_CotSplatTintC";
        internal const string SplatRoadTintProperty = "_CotSplatRoadTint";
        internal const string SplatMicroAmpProperty = "_CotSplatMicroAmp";
        internal const string SplatMidReliefProperty = "_CotSplatMidRelief";
        internal const string SplatMidReliefFarProperty = "_CotSplatMidReliefFar";
        internal const string SplatTownWearProperty = "_CotSplatTownWear";
        internal const string SplatIceDriftProperty = "_CotSplatIceDrift";
        internal const string SplatSandMacroProperty = "_CotSplatSandMacro";
        internal const string SplatRippleAmpProperty = "_CotSplatRippleAmp";
        internal const string TerrainCloudShadeProperty = "_CotTerrainCloudShade";
        internal const string TerrainDetailRangeProperty = "_CotTerrainDetailRange";
        internal const string TerrainMinLuminanceProperty = "_CotTerrainMinLuminance";
        internal const string TerrainMaxLuminanceProperty = "_CotTerrainMaxLuminance";
        internal const string TerrainRoleProperty = "_CotTerrainRole";
        internal const string TerrainShaderAppliedProperty = "_CotTerrainShaderApplied";
        internal const string HorizonBandingProperty = "_CotHorizonBanding";
        internal const string HorizonDetailStrengthProperty = "_CotHorizonDetailStrength";
        internal const string HorizonStyleProperty = "_CotHorizonStyle";
        internal const string HorizonMaxHeightProperty = "_CotHorizonMaxHeight";
        internal const string HorizonTextureRangeProperty = "_CotHorizonTextureRange";
        internal const string CloudAltitudeProperty = "_CotCloudAltitude";
        internal const string CloudScaleProperty = "_CotCloudScale";
        internal const string CloudHazeColorProperty = "_CotCloudHazeColor";
        internal const string CloudHazeRateProperty = "_CotCloudHazeK";
        internal const string CloudSunRotationProperty = "_CotCloudSunRot";
        internal const string CloudYFadeProperty = "_CotCloudYFade";
        internal const string CloudShadeStrengthProperty = "_CotCloudShadeStrength";

        public static Material Create(
            Color color,
            MapMaterialRole role,
            string seed)
        {
            return Create(color, role, seed, null);
        }

        public static Material Create(
            Color color,
            MapMaterialRole role,
            string seed,
            MapSplat splat)
        {
            bool foliageCard = IsFoliageCard(role);
            Shader shader = Shader.Find(foliageCard
                ? "ClaudeOfTanks/MapFoliageWindCutout"
                : role == MapMaterialRole.Cloud
                    ? "ClaudeOfTanks/MapCloudDeck"
                    : role == MapMaterialRole.Horizon
                        ? "ClaudeOfTanks/MapHorizonDetail"
                    : IsTerrainDetailRole(role)
                        ? "ClaudeOfTanks/MapTerrainDetail"
                        : "Standard");
            if (shader == null && role == MapMaterialRole.Cloud)
                shader = Shader.Find("Unlit/Transparent");
            if (shader == null && foliageCard)
                shader = Shader.Find("Unlit/Transparent Cutout");
            if (shader == null && role == MapMaterialRole.Cloud)
                shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null && role == MapMaterialRole.Horizon)
                shader = Shader.Find("Sprites/Default");
            if (shader == null && role == MapMaterialRole.Horizon)
                shader = Shader.Find("Unlit/Color");
            if (shader == null && IsTerrainDetailRole(role))
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Standard");
            MapSplatVisual splatVisual = MapSplatVisual.From(splat);
            Color gradedColor = GradeBaseColor(
                ApplySplatBaseColor(color, role, splatVisual),
                role);
            ProceduralTextureStats textureStats;
            Texture2D texture = Texture(
                gradedColor,
                role,
                seed,
                splatVisual,
                out textureStats);
            Material material = new Material(shader)
            {
                name = "MapMaterial-" + role + "-" + seed +
                    (splatVisual.Enabled ? "-splat" : string.Empty),
                color = Color.white,
                mainTexture = texture
            };
            material.mainTextureScale = TextureScale(role);
            if (!foliageCard)
                AssignNormalMap(material, role, seed, splatVisual);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", Smoothness(role, splatVisual));
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", Metallic(role));
            ApplySplatMetadata(material, splatVisual);
            ApplyTextureStatsMetadata(material, role, textureStats);
            ApplyTerrainDetailMetadata(material, role);
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
                        gradedColor * (role == MapMaterialRole.Conifer ? 0.14f : 0.18f));
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
            string seed,
            MapSplatVisual splat)
        {
            if (!material.HasProperty("_BumpMap")) return;
            if (!UsesNormalMap(role)) return;
            Texture2D normal = NormalTexture(role, seed);
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
            if (material.HasProperty("_BumpScale"))
                material.SetFloat("_BumpScale", NormalStrength(role, splat));
        }

        private static Texture2D Texture(
            Color color,
            MapMaterialRole role,
            string seed,
            MapSplatVisual splat,
            out ProceduralTextureStats stats)
        {
            int size = role == MapMaterialRole.Terrain ||
                role == MapMaterialRole.GroundVariation
                    ? 128
                    : role == MapMaterialRole.Bark ||
                        role == MapMaterialRole.Cloud
                        ? 96
                        : 64;
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
            stats = ProceduralTextureStats.Empty();
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float v = y / (float)size;
                    Color sample = Sample(
                        color,
                        role,
                        u,
                        v,
                        hash,
                        splat);
                    pixels[y * size + x] = sample;
                    stats.Add(sample);
                }
            }
            texture.SetPixels(pixels);
            bool keepReadable = role == MapMaterialRole.Terrain ||
                role == MapMaterialRole.GroundVariation;
            texture.Apply(true, !keepReadable);
            return texture;
        }

        private static Color GradeBaseColor(Color color, MapMaterialRole role)
        {
            float saturation = 1f;
            float exposure = 1f;
            switch (role)
            {
                case MapMaterialRole.Terrain:
                    saturation = 1.14f;
                    exposure = 0.94f;
                    break;
                case MapMaterialRole.GroundVariation:
                    saturation = 1.26f;
                    exposure = 0.88f;
                    break;
                case MapMaterialRole.Road:
                    saturation = 0.9f;
                    exposure = 0.78f;
                    break;
                case MapMaterialRole.RoadCasing:
                    saturation = 0.86f;
                    exposure = 0.92f;
                    break;
                case MapMaterialRole.Marsh:
                    saturation = 1.16f;
                    exposure = 0.82f;
                    break;
                case MapMaterialRole.Water:
                case MapMaterialRole.Ice:
                    saturation = 1.22f;
                    exposure = 1.04f;
                    break;
                case MapMaterialRole.Crater:
                    saturation = 0.86f;
                    exposure = 0.74f;
                    break;
                case MapMaterialRole.StructureBody:
                case MapMaterialRole.StructureWall:
                    saturation = 0.82f;
                    exposure = 0.92f;
                    break;
                case MapMaterialRole.StructureRoof:
                case MapMaterialRole.StructureCover:
                    saturation = 1.06f;
                    exposure = 0.82f;
                    break;
                case MapMaterialRole.StructureDetail:
                case MapMaterialRole.Rock:
                    saturation = 0.78f;
                    exposure = 0.72f;
                    break;
                case MapMaterialRole.Horizon:
                    saturation = 0.92f;
                    exposure = 0.78f;
                    break;
                case MapMaterialRole.Cloud:
                    saturation = 0.82f;
                    exposure = 1.08f;
                    break;
                case MapMaterialRole.Bark:
                    saturation = 1.12f;
                    exposure = 0.72f;
                    break;
                case MapMaterialRole.Broadleaf:
                case MapMaterialRole.Conifer:
                case MapMaterialRole.Palm:
                case MapMaterialRole.Birch:
                    saturation = 1.34f;
                    exposure = 0.86f;
                    break;
            }
            float luminance = color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
            Color gray = new Color(luminance, luminance, luminance, color.a);
            Color result = Color.Lerp(gray, color, saturation) * exposure;
            result.r = Mathf.Clamp01(result.r);
            result.g = Mathf.Clamp01(result.g);
            result.b = Mathf.Clamp01(result.b);
            result.a = color.a;
            return result;
        }

        private static Color ApplySplatBaseColor(
            Color color,
            MapMaterialRole role,
            MapSplatVisual splat)
        {
            if (!splat.Enabled) return color;
            switch (role)
            {
                case MapMaterialRole.Terrain:
                    {
                        Color tint = Color.Lerp(
                            splat.TintA,
                            splat.TintC,
                            Mathf.Clamp01(0.24f + splat.SandMacro * 0.38f));
                        if (splat.SandMacro > 0.001f)
                        {
                            tint = Color.Lerp(
                                tint,
                                new Color(0.86f, 0.81f, 0.70f, 1f),
                                splat.SandMacro * 0.42f);
                        }
                        return Multiply(color, tint);
                    }
                case MapMaterialRole.GroundVariation:
                    {
                        Color tint = Color.Lerp(splat.TintB, splat.TintC, 0.32f);
                        if (splat.SandMacro > 0.001f)
                        {
                            tint = Color.Lerp(tint, splat.TintB, splat.SandMacro * 0.62f);
                            tint = Multiply(tint, new Color(0.78f, 0.74f, 0.66f, 1f));
                        }
                        return Multiply(color, tint);
                    }
                case MapMaterialRole.Road:
                    {
                        Color tint = splat.RoadTint;
                        if (splat.SandMacro > 0.001f)
                        {
                            tint = Color.Lerp(
                                tint,
                                Multiply(tint, new Color(0.78f, 0.72f, 0.62f, 1f)),
                                splat.SandMacro * 0.72f);
                        }
                        return Multiply(color, tint) *
                            Mathf.Lerp(1f, 0.78f, Mathf.Clamp01((splat.TownWear - 1f) * 0.44f));
                    }
                case MapMaterialRole.RoadCasing:
                    {
                        Color tint = Color.Lerp(splat.RoadTint, splat.TintB, 0.45f);
                        if (splat.SandMacro > 0.001f)
                        {
                            tint = Color.Lerp(
                                tint,
                                Multiply(tint, new Color(0.76f, 0.72f, 0.64f, 1f)),
                                splat.SandMacro * 0.62f);
                        }
                        return Multiply(color, tint);
                    }
                case MapMaterialRole.Marsh:
                    return Multiply(color, Color.Lerp(splat.TintB, splat.TintC, 0.35f));
                case MapMaterialRole.Ice:
                    return Color.Lerp(
                        Multiply(color, splat.IceSky),
                        Color.white,
                        Mathf.Clamp01(splat.IceDrift * 0.18f));
                case MapMaterialRole.Water:
                    return Color.Lerp(color, Multiply(color, splat.TintC), 0.22f);
                default:
                    return color;
            }
        }

        private static void ApplySplatMetadata(
            Material material,
            MapSplatVisual splat)
        {
            if (!splat.Enabled) return;
            material.SetFloat(SplatAppliedProperty, 1f);
            material.SetColor(SplatTintAProperty, splat.TintA);
            material.SetColor(SplatTintBProperty, splat.TintB);
            material.SetColor(SplatTintCProperty, splat.TintC);
            material.SetColor(SplatRoadTintProperty, splat.RoadTint);
            material.SetFloat(SplatMicroAmpProperty, splat.MicroAmp);
            material.SetFloat(SplatMidReliefProperty, splat.MidRelief);
            material.SetFloat(SplatMidReliefFarProperty, splat.MidReliefFar);
            material.SetFloat(SplatTownWearProperty, splat.TownWear);
            material.SetFloat(SplatIceDriftProperty, splat.IceDrift);
            material.SetFloat(SplatSandMacroProperty, splat.SandMacro);
            material.SetFloat(SplatRippleAmpProperty, splat.RippleAmp);
            material.SetFloat(TerrainCloudShadeProperty, TerrainCloudShade(splat));
        }

        private static float TerrainCloudShade(MapSplatVisual splat)
        {
            if (!splat.Enabled) return 0.16f;
            if (splat.IceDrift > 0.001f)
                return Mathf.Lerp(0.08f, 0.11f, splat.IceDrift);
            return Mathf.Lerp(0.16f, 0.26f, splat.SandMacro);
        }

        private static void ApplyTextureStatsMetadata(
            Material material,
            MapMaterialRole role,
            ProceduralTextureStats stats)
        {
            if (role == MapMaterialRole.Terrain ||
                role == MapMaterialRole.GroundVariation)
            {
                material.SetFloat(TerrainDetailRangeProperty, stats.Range);
                material.SetFloat(TerrainMinLuminanceProperty, stats.MinLuminance);
                material.SetFloat(TerrainMaxLuminanceProperty, stats.MaxLuminance);
            }
            if (role == MapMaterialRole.Horizon)
                material.SetFloat(HorizonTextureRangeProperty, stats.Range);
        }

        private static void ApplyTerrainDetailMetadata(
            Material material,
            MapMaterialRole role)
        {
            if (!IsTerrainDetailRole(role)) return;
            material.SetFloat(TerrainShaderAppliedProperty, 1f);
            material.SetFloat(TerrainRoleProperty, TerrainRoleCode(role));
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
            int seed,
            MapSplatVisual splat)
        {
            float noise = Noise(u, v, seed);
            float fine = Noise(u * 5.3f, v * 5.3f, seed ^ 0x5f37);
            float mottled = Noise(u * 12.7f, v * 12.7f, seed ^ 0x2f6d);
            float stripe = Mathf.Sin((u + seed * 0.00013f) *
                Mathf.PI * 26f);
            float value = 1f;
            switch (role)
            {
                case MapMaterialRole.Terrain:
                    value = 0.9f + noise * 0.34f +
                        fine * 0.13f + mottled * 0.08f;
                    break;
                case MapMaterialRole.GroundVariation:
                    value = 0.76f + noise * 0.31f + fine * 0.08f;
                    break;
                case MapMaterialRole.Road:
                    value = 0.62f + noise * 0.23f +
                        Mathf.Abs(stripe) * 0.14f + fine * 0.05f;
                    break;
                case MapMaterialRole.RoadCasing:
                    value = 0.7f + fine * 0.22f + noise * 0.08f;
                    break;
                case MapMaterialRole.Marsh:
                    value = 0.64f + noise * 0.22f + fine * 0.09f;
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
                    value = 0.54f + noise * 0.28f + fine * 0.1f;
                    break;
                case MapMaterialRole.StructureBody:
                    value = 0.78f + noise * 0.24f +
                        Mathf.Abs(stripe) * 0.08f + fine * 0.05f;
                    break;
                case MapMaterialRole.StructureRoof:
                    value = 0.6f + fine * 0.3f + Mathf.Abs(stripe) * 0.06f;
                    break;
                case MapMaterialRole.StructureDetail:
                    value = 0.5f + Mathf.Abs(stripe) * 0.28f + fine * 0.08f;
                    break;
                case MapMaterialRole.StructureWall:
                    value = 0.68f + noise * 0.24f + fine * 0.08f;
                    break;
                case MapMaterialRole.StructureCover:
                    value = 0.58f + fine * 0.32f + noise * 0.06f;
                    break;
                case MapMaterialRole.Rock:
                    value = 0.62f + noise * 0.24f +
                        fine * 0.18f;
                    break;
                case MapMaterialRole.Horizon:
                    {
                        float strata = Mathf.Abs(Mathf.Sin(v * Mathf.PI * 18f +
                            seed * 0.00031f));
                        float ridges = Mathf.Abs(Mathf.Sin((u + fine * 0.12f) *
                            Mathf.PI * 9f));
                        value = 0.58f + noise * 0.18f + fine * 0.12f +
                            strata * 0.08f + ridges * 0.06f;
                        break;
                    }
                case MapMaterialRole.Cloud:
                    {
                        float warpU = (ValueNoise01(u * 2.4f, v * 2.4f, seed ^ 0x511d) - 0.5f) * 0.18f;
                        float warpV = (ValueNoise01(u * 2.4f + 13.7f, v * 2.4f - 8.1f, seed ^ 0x73af) - 0.5f) * 0.18f;
                        float macro = ValueNoise01(u * 2.05f + warpU, v * 2.05f + warpV, seed ^ 0x0d35);
                        float billow = ValueNoise01(u * 7.2f + warpU * 2f, v * 7.2f + warpV * 2f, seed ^ 0x5f37);
                        float detail = ValueNoise01(u * 18.5f, v * 18.5f, seed ^ 0x2f6d);
                        float edge = Mathf.SmoothStep(0.42f, 0.58f, macro * 0.56f + billow * 0.31f + detail * 0.13f);
                        value = 0.54f + macro * 0.34f + billow * 0.18f + detail * 0.08f + edge * 0.18f;
                        break;
                    }
                case MapMaterialRole.Bark:
                    value = 0.56f + noise * 0.2f -
                        Mathf.Max(0f, stripe) * 0.28f;
                    break;
                case MapMaterialRole.Broadleaf:
                    value = 0.66f + noise * 0.3f +
                        fine * 0.18f;
                    break;
                case MapMaterialRole.Conifer:
                    value = 0.54f + noise * 0.22f +
                        fine * 0.13f;
                    break;
                case MapMaterialRole.Palm:
                    value = 0.66f + Mathf.Abs(stripe) * 0.18f +
                        fine * 0.13f;
                    break;
                case MapMaterialRole.Birch:
                    value = 0.78f + noise * 0.18f -
                        Mathf.Max(0f, stripe) * 0.2f;
                    break;
            }
            value = ApplySplatValue(role, value, noise, fine, mottled, splat);
            Color color = ApplySplatSample(
                baseColor * Mathf.Clamp(value, 0.18f, 1.45f),
                role,
                u,
                v,
                noise,
                fine,
                mottled,
                splat);
            color.a = IsFoliageCard(role)
                ? FoliageAlpha(role, u, v, noise, fine, seed)
                : role == MapMaterialRole.Cloud
                    ? CloudAlpha(baseColor.a, u, v, seed)
                : baseColor.a;
            return color;
        }

        private static float CloudAlpha(
            float baseAlpha,
            float u,
            float v,
            int seed)
        {
            float warpU = (ValueNoise01(u * 2.4f, v * 2.4f, seed ^ 0x511d) - 0.5f) * 0.18f;
            float warpV = (ValueNoise01(u * 2.4f + 13.7f, v * 2.4f - 8.1f, seed ^ 0x73af) - 0.5f) * 0.18f;
            float macro = ValueNoise01(u * 2.05f + warpU, v * 2.05f + warpV, seed ^ 0x0d35);
            float billow = ValueNoise01(u * 7.2f + warpU * 2f, v * 7.2f + warpV * 2f, seed ^ 0x5f37);
            float detail = ValueNoise01(u * 18.5f, v * 18.5f, seed ^ 0x2f6d);
            float field = macro * 0.56f + billow * 0.31f + detail * 0.13f;
            float coverage = Mathf.SmoothStep(0.42f, 0.58f, field);
            float core = Mathf.SmoothStep(0.58f, 0.78f, field);
            float tornEdge = Mathf.Lerp(0.78f, 1.12f, billow) *
                Mathf.Lerp(0.88f, 1.08f, detail);
            float horizonBreakup = 0.86f +
                Mathf.Sin((u * 5.7f + billow * 0.21f) * Mathf.PI) * 0.14f;
            return Mathf.Clamp01(
                baseAlpha *
                coverage *
                Mathf.Lerp(0.64f, 1f, core) *
                tornEdge *
                horizonBreakup);
        }

        private static float ApplySplatValue(
            MapMaterialRole role,
            float value,
            float noise,
            float fine,
            float mottled,
            MapSplatVisual splat)
        {
            if (!splat.Enabled) return value;
            switch (role)
            {
                case MapMaterialRole.Terrain:
                    {
                        float detail = Mathf.Lerp(
                            0.72f,
                            1.35f,
                            Mathf.Clamp01(splat.MicroAmp));
                        float relief = (noise * 0.13f + fine * 0.08f + mottled * 0.07f) *
                            Mathf.Clamp(splat.MidRelief, 0.35f, 1.8f);
                        return 1f + (value - 1f) * detail + relief;
                    }
                case MapMaterialRole.GroundVariation:
                    return value * Mathf.Lerp(
                        1f,
                        0.86f,
                        Mathf.Clamp01((splat.TownWear - 1f) * 0.38f)) *
                        Mathf.Lerp(1f, 0.72f, splat.SandMacro);
                case MapMaterialRole.Road:
                    return value * Mathf.Lerp(
                        1f,
                        0.78f,
                        Mathf.Clamp01((splat.TownWear - 1f) * 0.42f)) *
                        Mathf.Lerp(1f, 0.74f, splat.SandMacro);
                case MapMaterialRole.RoadCasing:
                    return value * Mathf.Lerp(
                        1f,
                        0.9f,
                        Mathf.Clamp01((splat.RoadTexMix - 0.35f) * 1.3f)) *
                        Mathf.Lerp(1f, 0.80f, splat.SandMacro);
                case MapMaterialRole.Ice:
                    return value * Mathf.Lerp(
                        1.05f,
                        0.86f,
                        Mathf.Clamp01(splat.IceDrift));
                default:
                    return value;
            }
        }

        private static Color ApplySplatSample(
            Color color,
            MapMaterialRole role,
            float u,
            float v,
            float noise,
            float fine,
            float mottled,
            MapSplatVisual splat)
        {
            if (!splat.Enabled) return color;
            switch (role)
            {
                case MapMaterialRole.Terrain:
                    {
                        float field = Mathf.Clamp01(0.5f + noise * 0.5f);
                        float dry = Mathf.Clamp01(0.5f + fine * 0.5f);
                        float patch = Mathf.Clamp01(splat.FieldPatch);
                        Color tint = Color.Lerp(splat.TintA, splat.TintB, field);
                        tint = Color.Lerp(tint, splat.TintC, dry * (0.24f + patch * 0.36f));
                        float macroA = Mathf.Clamp01(0.5f + Noise(
                            u * 0.23f + 0.37f,
                            v * 0.23f - 0.19f,
                            3011) * 0.5f);
                        float macroB = Mathf.Clamp01(0.5f + Noise(
                            u * 0.071f - 0.11f,
                            v * 0.071f + 0.43f,
                            3011 ^ 0x399f) * 0.5f);
                        float gravel = Mathf.SmoothStep(
                            0.56f,
                            0.82f,
                            macroA + noise * 0.12f) * splat.SandMacro;
                        float scour = Mathf.SmoothStep(
                            0.60f,
                            0.90f,
                            macroB) * splat.SandMacro * (1f - gravel);
                        if (splat.SandMacro > 0.001f)
                        {
                            Color gravelTint = new Color(0.80f, 0.755f, 0.70f, 1f);
                            tint = Color.Lerp(tint, Multiply(tint, gravelTint), gravel * 0.8f);
                            tint = Color.Lerp(tint, Multiply(tint, new Color(1.055f, 1.035f, 1f, 1f)), scour * 0.55f);
                        }
                        if (splat.Sandstone)
                        {
                            float strata = Mathf.Abs(Mathf.Sin(
                                (v * 19f + mottled * 0.4f) * Mathf.PI));
                            tint = Color.Lerp(
                                tint,
                                new Color(1.22f, 0.92f, 0.68f, 1f),
                                Mathf.Clamp01(strata * (0.18f + splat.Strata * 0.55f)));
                        }
                        Color result = Multiply(color, tint);
                        if (splat.RippleAmp > 0.001f)
                        {
                            float ripplePhase =
                                (u * splat.RippleDir.x + v * splat.RippleDir.y) *
                                Mathf.PI * 13.2f +
                                Noise(u * 0.7f, v * 0.7f, 3011 ^ 0x74a1) * 2.8f;
                            float dunePhase =
                                (u * splat.RippleDir.x + v * splat.RippleDir.y) *
                                Mathf.PI * 4.1f +
                                macroB * 4.0f;
                            float ripple =
                                Mathf.Sin(ripplePhase) * 0.075f +
                                Mathf.Sin(dunePhase) * 0.105f * splat.SandMacro;
                            result *= 1f + ripple * Mathf.Clamp01(splat.RippleAmp);
                        }
                        if (splat.SandMacro > 0.001f)
                        {
                            float luminance = Luminance(result);
                            float shoulder = Mathf.SmoothStep(0.60f, 0.95f, luminance) *
                                0.16f * splat.SandMacro;
                            result *= 1f - shoulder;
                        }
                        result.r = Mathf.Clamp01(result.r);
                        result.g = Mathf.Clamp01(result.g);
                        result.b = Mathf.Clamp01(result.b);
                        result.a = color.a;
                        return result;
                    }
                case MapMaterialRole.GroundVariation:
                    {
                        Color tint = Color.Lerp(splat.TintB, splat.TintC, 0.4f);
                        if (splat.SandMacro > 0.001f)
                        {
                            float ripplePhase =
                                (u * splat.RippleDir.x + v * splat.RippleDir.y) *
                                Mathf.PI * 10.5f +
                                noise * 2.4f;
                            float ripple =
                                Mathf.Sin(ripplePhase) *
                                0.08f *
                                Mathf.Clamp01(splat.RippleAmp);
                            tint = Color.Lerp(
                                tint,
                                Multiply(tint, new Color(0.82f, 0.78f, 0.69f, 1f)),
                                splat.SandMacro * 0.7f);
                            color *= 1f + ripple;
                        }
                        Color result = Multiply(color, tint);
                        if (splat.SandMacro > 0.001f)
                        {
                            float shoulder = Mathf.SmoothStep(
                                0.54f,
                                0.90f,
                                Luminance(result)) * 0.22f * splat.SandMacro;
                            result *= 1f - shoulder;
                            result.r = Mathf.Clamp01(result.r);
                            result.g = Mathf.Clamp01(result.g);
                            result.b = Mathf.Clamp01(result.b);
                        }
                        return result;
                    }
                case MapMaterialRole.Road:
                    {
                        float rut = Mathf.Abs(Mathf.Sin((u * 5.5f + fine * 0.5f) * Mathf.PI));
                        Color worn = Color.Lerp(
                            splat.RoadTint,
                            splat.TintB,
                            Mathf.Clamp01(rut * (splat.TownWear - 1f) * 0.28f));
                        return Multiply(color, worn);
                    }
                case MapMaterialRole.RoadCasing:
                    return Multiply(color, Color.Lerp(splat.RoadTint, splat.TintB, 0.52f));
                case MapMaterialRole.Ice:
                    {
                        float drift = Mathf.Clamp01(
                            splat.IceDrift * (0.55f + Mathf.Abs(noise) * 0.45f));
                        Color ice = Multiply(color, splat.IceSky);
                        return Color.Lerp(ice, Color.white, drift * 0.32f);
                    }
                case MapMaterialRole.Water:
                    return Color.Lerp(color, Multiply(color, splat.TintC), 0.2f);
                default:
                    return color;
            }
        }

        private static float Luminance(Color color)
        {
            return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
        }

        private static bool IsFoliageCard(MapMaterialRole role)
        {
            return role == MapMaterialRole.Broadleaf ||
                role == MapMaterialRole.Conifer ||
                role == MapMaterialRole.Palm ||
                role == MapMaterialRole.Birch;
        }

        private static bool IsTerrainDetailRole(MapMaterialRole role)
        {
            return role == MapMaterialRole.Terrain ||
                role == MapMaterialRole.GroundVariation ||
                role == MapMaterialRole.Road ||
                role == MapMaterialRole.RoadCasing ||
                role == MapMaterialRole.Marsh ||
                role == MapMaterialRole.Crater;
        }

        private static float TerrainRoleCode(MapMaterialRole role)
        {
            switch (role)
            {
                case MapMaterialRole.GroundVariation: return 1f;
                case MapMaterialRole.Road: return 2f;
                case MapMaterialRole.RoadCasing: return 3f;
                case MapMaterialRole.Marsh: return 4f;
                case MapMaterialRole.Crater: return 5f;
                default: return 0f;
            }
        }

        private static bool UsesNormalMap(MapMaterialRole role)
        {
            return role != MapMaterialRole.GroundVariation &&
                role != MapMaterialRole.Crater &&
                role != MapMaterialRole.Cloud &&
                role != MapMaterialRole.Horizon;
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
                case MapMaterialRole.Horizon:
                    return new Vector2(8f, 2f);
                case MapMaterialRole.Cloud:
                    return Vector2.one;
                default:
                    return new Vector2(2f, 2f);
            }
        }

        private static float Smoothness(MapMaterialRole role)
        {
            return Smoothness(role, default(MapSplatVisual));
        }

        private static float Smoothness(MapMaterialRole role, MapSplatVisual splat)
        {
            switch (role)
            {
                case MapMaterialRole.Water:
                    return splat.Enabled
                        ? Mathf.Clamp01(0.78f + splat.MarshGloss * 0.1f)
                        : 0.86f;
                case MapMaterialRole.Ice:
                    return splat.Enabled
                        ? Mathf.Clamp01(Mathf.Lerp(0.78f, 0.48f, splat.IceDrift))
                        : 0.68f;
                case MapMaterialRole.Cloud: return 0.02f;
                case MapMaterialRole.Horizon: return 0.04f;
                case MapMaterialRole.Road:
                    return splat.Enabled
                        ? Mathf.Clamp01(Mathf.Lerp(0.14f, 0.28f, splat.RoadTexMix))
                        : 0.18f;
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
            return NormalStrength(role, default(MapSplatVisual));
        }

        private static float NormalStrength(MapMaterialRole role, MapSplatVisual splat)
        {
            float splatScale = 1f;
            if (splat.Enabled)
            {
                switch (role)
                {
                    case MapMaterialRole.Terrain:
                    case MapMaterialRole.GroundVariation:
                        splatScale = Mathf.Lerp(0.72f, 1.32f, Mathf.Clamp01(splat.MicroAmp));
                        break;
                    case MapMaterialRole.Road:
                    case MapMaterialRole.RoadCasing:
                        splatScale = Mathf.Lerp(0.9f, 1.18f, Mathf.Clamp01(splat.RoadTexMix));
                        break;
                    case MapMaterialRole.Ice:
                        splatScale = Mathf.Lerp(1.2f, 0.72f, Mathf.Clamp01(splat.IceDrift));
                        break;
                }
            }
            switch (role)
            {
                case MapMaterialRole.Water: return 0.36f * splatScale;
                case MapMaterialRole.Ice: return 0.28f * splatScale;
                case MapMaterialRole.Terrain: return 0.24f * splatScale;
                case MapMaterialRole.Road: return 0.24f * splatScale;
                case MapMaterialRole.RoadCasing: return 0.2f * splatScale;
                case MapMaterialRole.StructureBody:
                case MapMaterialRole.StructureWall:
                    return 0.28f;
                case MapMaterialRole.StructureRoof:
                case MapMaterialRole.StructureCover:
                    return 0.34f;
                case MapMaterialRole.Bark:
                    return 0.42f;
                default:
                    return 0.16f;
            }
        }

        private static Color Multiply(Color color, Color multiplier)
        {
            return new Color(
                Mathf.Clamp01(color.r * multiplier.r),
                Mathf.Clamp01(color.g * multiplier.g),
                Mathf.Clamp01(color.b * multiplier.b),
                color.a);
        }

        private static Color ColorFromTriple(
            float[] values,
            Color fallback)
        {
            if (values == null || values.Length < 3) return fallback;
            return new Color(
                Mathf.Clamp(values[0], 0.05f, 2f),
                Mathf.Clamp(values[1], 0.05f, 2f),
                Mathf.Clamp(values[2], 0.05f, 2f),
                1f);
        }

        private struct MapSplatVisual
        {
            public bool Enabled;
            public Color TintA;
            public Color TintB;
            public Color TintC;
            public Color RoadTint;
            public Color IceSky;
            public float FieldPatch;
            public float MicroAmp;
            public float MidRelief;
            public float MidReliefFar;
            public float TownWear;
            public float IceDrift;
            public float MarshGloss;
            public float RoadTexMix;
            public float SandMacro;
            public float Strata;
            public float RippleAmp;
            public Vector2 RippleDir;
            public bool Sandstone;

            public static MapSplatVisual From(MapSplat splat)
            {
                if (splat == null) return default(MapSplatVisual);
                return new MapSplatVisual
                {
                    Enabled = true,
                    TintA = ColorFromTriple(splat.tintA, new Color(1.16f, 1.08f, 0.76f, 1f)),
                    TintB = ColorFromTriple(splat.tintB, new Color(0.78f, 0.90f, 0.72f, 1f)),
                    TintC = ColorFromTriple(splat.tintC, new Color(1.10f, 1.04f, 0.84f, 1f)),
                    RoadTint = ColorFromTriple(splat.roadTint, new Color(1.08f, 1.04f, 0.96f, 1f)),
                    IceSky = ColorFromTriple(splat.iceSky, new Color(0.66f, 0.72f, 0.82f, 1f)),
                    FieldPatch = Mathf.Clamp01(splat.fieldPatch),
                    MicroAmp = splat.microAmp > 0f ? splat.microAmp : 1f,
                    MidRelief = splat.midRelief > 0f ? splat.midRelief : 1f,
                    MidReliefFar = splat.midReliefFar > 0f ? splat.midReliefFar : 480f,
                    TownWear = splat.townWear > 0f ? splat.townWear : 1f,
                    IceDrift = splat.iceLake
                        ? Mathf.Clamp01(splat.iceDrift > 0f ? splat.iceDrift : 0.85f)
                        : 0f,
                    MarshGloss = Mathf.Clamp01(splat.marshGloss),
                    RoadTexMix = Mathf.Clamp01(splat.roadTexMix),
                    SandMacro = Mathf.Clamp01(splat.sandMacro),
                    Strata = Mathf.Clamp01(splat.strata),
                    RippleAmp = Mathf.Clamp(splat.rippleAmp, 0f, 1.2f),
                    RippleDir = NormalizedRippleDir(splat.rippleDir),
                    Sandstone = splat.sandstone
                };
            }
        }

        private struct ProceduralTextureStats
        {
            public float MinLuminance;
            public float MaxLuminance;

            public float Range => MaxLuminance - MinLuminance;

            public static ProceduralTextureStats Empty()
            {
                return new ProceduralTextureStats
                {
                    MinLuminance = float.PositiveInfinity,
                    MaxLuminance = 0f
                };
            }

            public void Add(Color color)
            {
                float luminance = Luminance(color);
                if (luminance < MinLuminance) MinLuminance = luminance;
                if (luminance > MaxLuminance) MaxLuminance = luminance;
            }
        }

        private static Vector2 NormalizedRippleDir(float[] values)
        {
            Vector2 direction = values != null && values.Length >= 2
                ? new Vector2(values[0], values[1])
                : new Vector2(0.8f, 0.6f);
            if (direction.sqrMagnitude < 0.0001f)
                direction = new Vector2(0.8f, 0.6f);
            return direction.normalized;
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

        private static float ValueNoise01(float x, float y, int seed)
        {
            int ix = Mathf.FloorToInt(x);
            int iy = Mathf.FloorToInt(y);
            float fx = x - ix;
            float fy = y - iy;
            float ux = fx * fx * (3f - 2f * fx);
            float uy = fy * fy * (3f - 2f * fy);
            float a = Hash01(ix, iy, seed);
            float b = Hash01(ix + 1, iy, seed);
            float c = Hash01(ix, iy + 1, seed);
            float d = Hash01(ix + 1, iy + 1, seed);
            return Mathf.Lerp(
                Mathf.Lerp(a, b, ux),
                Mathf.Lerp(c, d, ux),
                uy);
        }

        private static float Hash01(int x, int y, int seed)
        {
            int hash = seed;
            hash = hash * 73856093 ^ x * 19349663;
            hash ^= y * 83492791;
            hash ^= hash >> 13;
            hash *= 1274126177;
            return (hash & 0xffff) / 65535f;
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
