using System;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public static class TankCamouflage
    {
        private const int DefaultTextureSize = 128;

        public static Color ResolveColor(
            VehicleDefinition vehicle,
            string camouflageId,
            string mapId)
        {
            Color authored = vehicle?.visual != null
                ? vehicle.visual.Color
                : new Color(0.28f, 0.32f, 0.24f);
            string resolved = ResolveId(
                camouflageId,
                mapId);
            if (resolved == "factory" ||
                resolved.StartsWith(
                    "signature",
                    StringComparison.Ordinal) ||
                resolved.StartsWith(
                    "sig_",
                    StringComparison.Ordinal) ||
                resolved.StartsWith(
                    "service_",
                    StringComparison.Ordinal))
            {
                return Variation(authored, resolved);
            }
            if (ContainsAny(
                    resolved,
                    "winter",
                    "wash",
                    "ardennes"))
            {
                return Variation(
                    new Color(0.58f, 0.62f, 0.61f),
                    resolved);
            }
            if (ContainsAny(
                    resolved,
                    "desert",
                    "choc",
                    "pink",
                    "rasputitsa"))
            {
                return Variation(
                    new Color(0.52f, 0.43f, 0.29f),
                    resolved);
            }
            if (ContainsAny(
                    resolved,
                    "urban",
                    "berlin",
                    "midnight",
                    "dazzle",
                    "naval"))
            {
                return Variation(
                    new Color(0.31f, 0.34f, 0.34f),
                    resolved);
            }
            if (ContainsAny(
                    resolved,
                    "flames",
                    "racing",
                    "spark",
                    "bolt"))
            {
                return Variation(
                    new Color(0.42f, 0.18f, 0.11f),
                    resolved);
            }
            return Variation(
                new Color(0.24f, 0.34f, 0.2f),
                resolved);
        }

        public static Texture2D CreateTexture(
            ContentCatalog catalog,
            VehicleDefinition vehicle,
            string camouflageId,
            string mapId,
            Team team,
            int size = DefaultTextureSize)
        {
            return TankCamouflageTexturePainter.Create(
                catalog,
                vehicle,
                camouflageId,
                mapId,
                team,
                size);
        }

        public static float ResolveScale(
            VehicleDefinition vehicle,
            CamouflageDefinition definition)
        {
            CamouflageRecipe recipe =
                definition?.RecipeFor(vehicle?.nation);
            if (definition != null &&
                definition.usesVehicleScale &&
                vehicle?.visual != null &&
                vehicle.visual.camoScale > 0f)
            {
                return vehicle.visual.camoScale;
            }
            return recipe != null && recipe.camoScale > 0f
                ? recipe.camoScale
                : 0.34f;
        }

        public static string ResolveId(
            VehicleDefinition vehicle,
            string camouflageId,
            string mapId)
        {
            string resolved = ResolveId(
                camouflageId,
                mapId);
            if (resolved == "factory")
            {
                return string.IsNullOrEmpty(
                    vehicle?.factoryCamouflageId)
                        ? resolved
                        : vehicle.factoryCamouflageId;
            }
            if (resolved == "signature")
            {
                if (!string.IsNullOrEmpty(
                        vehicle?.signatureCamouflageId))
                {
                    return vehicle.signatureCamouflageId;
                }
                return string.IsNullOrEmpty(
                    vehicle?.factoryCamouflageId)
                        ? "factory"
                        : vehicle.factoryCamouflageId;
            }
            return resolved;
        }

        public static string ResolveId(
            string camouflageId,
            string mapId)
        {
            string selected = string.IsNullOrEmpty(camouflageId)
                ? "factory"
                : camouflageId;
            if (selected != "auto") return selected;
            switch (mapId)
            {
                case "winter":
                case "frosthollow":
                    return "winter";
                case "desert":
                case "sirocco":
                case "steppe":
                    return "desert";
                case "urban":
                case "cinder":
                case "railyard":
                    return "urbanblock";
                case "amberford":
                    return "autumn";
                default:
                    return "summer";
            }
        }

        private static Color Variation(Color color, string id)
        {
            uint hash = 2166136261u;
            string value = id ?? string.Empty;
            for (int i = 0; i < value.Length; i++)
                hash = (hash ^ value[i]) * 16777619u;
            float shift = ((hash & 255u) / 255f - 0.5f) * 0.12f;
            return new Color(
                Mathf.Clamp01(color.r + shift),
                Mathf.Clamp01(color.g + shift * 0.7f),
                Mathf.Clamp01(color.b - shift * 0.35f),
                color.a);
        }

        private static bool ContainsAny(
            string value,
            params string[] fragments)
        {
            for (int i = 0; i < fragments.Length; i++)
            {
                if (value.IndexOf(
                        fragments[i],
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
