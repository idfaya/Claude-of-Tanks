using System;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankCamouflageTexturePainter
    {
        public static Texture2D Create(
            ContentCatalog catalog,
            VehicleDefinition vehicle,
            string camouflageId,
            string mapId,
            Team team,
            int size)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));
            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle));
            size = Mathf.Clamp(size, 16, 256);
            string resolved = TankCamouflage.ResolveId(
                vehicle,
                camouflageId,
                mapId);
            CamouflageDefinition definition =
                catalog.ContainsCamouflage(resolved)
                    ? catalog.GetCamouflage(resolved)
                    : null;
            CamouflageRecipe recipe =
                definition?.RecipeFor(vehicle.nation) ??
                FallbackRecipe(vehicle);
            Color authored = vehicle.visual != null
                ? vehicle.visual.Color
                : new Color(0.28f, 0.32f, 0.24f);
            Color baseColor = ParseColor(
                recipe.baseColor,
                authored);
            Color weatherColor = ParseColor(
                recipe.weatherColor,
                baseColor);
            string[] patchValues =
                recipe.patchColors ?? Array.Empty<string>();
            Color[] patches = new Color[patchValues.Length];
            for (int i = 0; i < patches.Length; i++)
            {
                bool authoredPatch =
                    definition != null &&
                    definition.usesAuthoredBasePatch &&
                    i == 0;
                patches[i] = ParseColor(
                    authoredPatch
                        ? vehicle.visual?.@base
                        : patchValues[i],
                    authoredPatch ? authored : baseColor);
            }
            baseColor = TeamTint(baseColor, team);
            weatherColor = TeamTint(weatherColor, team);
            for (int i = 0; i < patches.Length; i++)
                patches[i] = TeamTint(patches[i], team);

            uint seed = Hash(
                vehicle.id + "|" + resolved + "|" +
                (recipe.scheme ?? "solid"));
            PatternFamily family =
                ResolveFamily(recipe.scheme);
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size;
                    float v = (y + 0.5f) / size;
                    float weather = ValueNoise(
                        seed ^ 0x9e3779b9u,
                        u * 5f,
                        v * 5f);
                    Color color = Color.Lerp(
                        baseColor,
                        weatherColor,
                        0.08f + weather * 0.3f);
                    int patch = PatternIndex(
                        recipe,
                        family,
                        seed,
                        u,
                        v,
                        patches.Length);
                    if (patch >= 0)
                    {
                        color = Color.Lerp(
                            color,
                            patches[patch],
                            0.84f);
                    }
                    pixels[y * size + x] = color;
                }
            }

            Texture2D texture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                true,
                false)
            {
                name = "Camo-" + vehicle.id + "-" +
                    resolved + "-" + team,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = IsCrispScheme(recipe.scheme)
                    ? FilterMode.Point
                    : FilterMode.Bilinear,
                anisoLevel = 2
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private static CamouflageRecipe FallbackRecipe(
            VehicleDefinition vehicle)
        {
            return new CamouflageRecipe
            {
                scheme = vehicle.visual?.scheme ?? "solid",
                baseColor =
                    vehicle.visual?.@base ?? "#48523d",
                weatherColor =
                    vehicle.visual?.weather ??
                    vehicle.visual?.@base ??
                    "#566149",
                patchColors =
                    vehicle.visual?.patches ??
                    Array.Empty<string>(),
                camoScale =
                    vehicle.visual != null &&
                    vehicle.visual.camoScale > 0f
                        ? vehicle.visual.camoScale
                        : 0.34f
            };
        }

        private static int PatternIndex(
            CamouflageRecipe recipe,
            PatternFamily family,
            uint seed,
            float u,
            float v,
            int patchCount)
        {
            if (patchCount == 0) return -1;
            float patchK = Mathf.Max(0.5f, recipe.patchK);
            if (family == PatternFamily.Digital)
            {
                float cells =
                    7f * Mathf.Max(
                        0.6f,
                        recipe.digitalCellK);
                int cx = Mathf.FloorToInt(u * cells);
                int cy = Mathf.FloorToInt(v * cells);
                uint value = Hash(seed, cx, cy);
                return (value & 7u) < 2u
                    ? -1
                    : (int)((value >> 8) %
                        (uint)patchCount);
            }
            if (family == PatternFamily.Stripes)
            {
                float angle = recipe.bandAngle != 0f
                    ? recipe.bandAngle
                    : 0.72f +
                        Hash01(seed ^ 0xa341316cu, 0, 0) *
                        0.8f;
                float warp = ValueNoise(
                    seed ^ 0xc8013ea4u,
                    u * 3f,
                    v * 3f) - 0.5f;
                float wave = Mathf.Sin(
                    (u * Mathf.Cos(angle) +
                     v * Mathf.Sin(angle) +
                     warp * 0.16f) *
                    Mathf.PI * 6f * patchK);
                if (wave < -0.2f) return -1;
                return Mathf.Min(
                    patchCount - 1,
                    Mathf.FloorToInt(
                        (wave + 0.2f) * patchCount /
                        1.2f));
            }
            if (family == PatternFamily.Geometric)
            {
                float cells = 5f * patchK;
                int cx = Mathf.FloorToInt(u * cells);
                int cy = Mathf.FloorToInt(v * cells);
                float lx = u * cells - cx;
                float ly = v * cells - cy;
                uint value = Hash(seed, cx, cy);
                float diagonal =
                    (value & 1u) == 0u
                        ? lx + ly
                        : lx - ly + 1f;
                if (diagonal < 0.55f ||
                    ((value >> 3) & 3u) == 0u)
                {
                    return -1;
                }
                return (int)((value >> 8) %
                    (uint)patchCount);
            }
            if (family == PatternFamily.Dots)
            {
                const float cells = 10f;
                int cx = Mathf.FloorToInt(u * cells);
                int cy = Mathf.FloorToInt(v * cells);
                uint value = Hash(seed, cx, cy);
                float centerX =
                    0.25f + ((value >> 8) & 255u) /
                    510f;
                float centerY =
                    0.25f + ((value >> 16) & 255u) /
                    510f;
                float dx = u * cells - cx - centerX;
                float dy = v * cells - cy - centerY;
                float radius =
                    0.12f + (value & 255u) / 850f;
                if (dx * dx + dy * dy > radius * radius)
                    return -1;
                return (int)((value >> 24) %
                    (uint)patchCount);
            }

            float organic = ValueNoise(
                seed ^ 0xad90777du,
                u * 3.4f * patchK,
                v * 3.4f * patchK);
            if (organic < 0.48f) return -1;
            int index = Mathf.FloorToInt(
                (organic - 0.48f) / 0.52f *
                patchCount);
            return Mathf.Clamp(index, 0, patchCount - 1);
        }

        private static float ValueNoise(
            uint seed,
            float x,
            float y)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float tx = x - x0;
            float ty = y - y0;
            tx = tx * tx * (3f - 2f * tx);
            ty = ty * ty * (3f - 2f * ty);
            float a = Hash01(seed, x0, y0);
            float b = Hash01(seed, x0 + 1, y0);
            float c = Hash01(seed, x0, y0 + 1);
            float d = Hash01(seed, x0 + 1, y0 + 1);
            return Mathf.Lerp(
                Mathf.Lerp(a, b, tx),
                Mathf.Lerp(c, d, tx),
                ty);
        }

        private static float Hash01(
            uint seed,
            int x,
            int y)
        {
            return (Hash(seed, x, y) & 0x00ffffffu) /
                16777215f;
        }

        private static uint Hash(
            uint seed,
            int x,
            int y)
        {
            uint value = seed;
            value ^= (uint)x * 0x27d4eb2du;
            value = (value ^ (value >> 15)) *
                0x85ebca6bu;
            value ^= (uint)y * 0x165667b1u;
            value = (value ^ (value >> 13)) *
                0xc2b2ae35u;
            return value ^ (value >> 16);
        }

        private static uint Hash(string value)
        {
            uint hash = 2166136261u;
            string text = value ?? string.Empty;
            for (int i = 0; i < text.Length; i++)
                hash = (hash ^ text[i]) * 16777619u;
            return hash;
        }

        private static Color ParseColor(
            string value,
            Color fallback)
        {
            Color color;
            return ColorUtility.TryParseHtmlString(
                value,
                out color)
                    ? color
                    : fallback;
        }

        private static Color TeamTint(
            Color color,
            Team team)
        {
            return team == Team.Alpha
                ? Color.Lerp(
                    color,
                    new Color(0.16f, 0.55f, 0.25f),
                    0.35f)
                : Color.Lerp(
                    color,
                    new Color(0.68f, 0.18f, 0.12f),
                    0.48f);
        }

        private static bool IsCrispScheme(string scheme)
        {
            switch (scheme)
            {
                case "digital":
                case "blocks":
                case "splinter":
                case "dazzle":
                case "hexfield":
                case "circuit":
                case "bolt":
                    return true;
                default:
                    return false;
            }
        }

        private static PatternFamily ResolveFamily(
            string scheme)
        {
            switch (scheme)
            {
                case "digital":
                case "blocks":
                case "circuit":
                    return PatternFamily.Digital;
                case "stripes":
                case "caunter":
                case "tigerstripe":
                case "brush":
                case "brushwash":
                case "idband":
                case "usmc":
                case "racing":
                case "flames":
                    return PatternFamily.Stripes;
                case "splinter":
                case "dazzle":
                case "hexfield":
                case "bolt":
                case "stars":
                case "suits":
                case "claude":
                case "spark":
                    return PatternFamily.Geometric;
                case "fleck":
                case "chip6":
                case "ducky":
                case "leopardprint":
                case "daisy":
                case "paintball":
                    return PatternFamily.Dots;
                default:
                    return PatternFamily.Organic;
            }
        }

        private enum PatternFamily
        {
            Organic,
            Digital,
            Stripes,
            Geometric,
            Dots
        }
    }
}
