using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MaterialApplicator
    {
        private enum Role
        {
            Hull,
            Barrel,
            Wheel,
            Rubber,
            Track,
            Detail,
            Dark,
            Glass,
            Wood,
            Shtora
        }

        public static void Apply(Renderer[] renderers)
        {
            if (renderers == null) return;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                Material material = renderer?.sharedMaterial;
                if (material == null) continue;
                Role role = ResolveRole(renderer.gameObject.name);
                ApplyRole(material, role);
            }
        }

        private static Role ResolveRole(string name)
        {
            if (string.Equals(
                    name,
                    "T90-ShtoraLens",
                    StringComparison.Ordinal))
            {
                return Role.Shtora;
            }
            if (name.IndexOf(
                    "Lens",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Role.Glass;
            }
            if (string.Equals(
                    name,
                    "T90-SplitUnditchingLog",
                    StringComparison.Ordinal))
            {
                return Role.Wood;
            }
            if (name.IndexOf(
                    "RoadWheelTire",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "RoadWheelShoulder",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "Rubber",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "Mudguard",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Rubber;
            }
            if (name.IndexOf(
                    "RoadWheelDisc",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "RoadWheelHub",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "ReturnRollerDisc",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "Sprocket",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "Idler",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Wheel;
            }
            if (name.IndexOf(
                    "TrackPad",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "TrackCleat",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "TrackUpperBand",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "TrackLowerBand",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "TrackFrontRise",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "TrackRearRise",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Track;
            }
            if (name.IndexOf(
                    "2A46M",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "GunBoot",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "CannonBaseBoot",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Barrel;
            }
            if (name.StartsWith(
                    "Painted-",
                    StringComparison.Ordinal) ||
                name.StartsWith(
                    "Armor-",
                    StringComparison.Ordinal))
            {
                return Role.Hull;
            }
            if (name.IndexOf(
                    "Detail",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "End",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Detail;
            }
            return Role.Dark;
        }

        private static void ApplyRole(
            Material material,
            Role role)
        {
            float roughness;
            float metallic;
            Color emission = Color.black;
            switch (role)
            {
                case Role.Hull:
                    roughness = 0.88f;
                    metallic = 0.05f;
                    break;
                case Role.Barrel:
                    roughness = 0.8f;
                    metallic = 0.08f;
                    break;
                case Role.Wheel:
                    roughness = 0.92f;
                    metallic = 0.08f;
                    break;
                case Role.Rubber:
                    roughness = 0.96f;
                    metallic = 0f;
                    emission = Rgb(0x0a, 0x0a, 0x08);
                    break;
                case Role.Track:
                    roughness = 0.95f;
                    metallic = 0.08f;
                    break;
                case Role.Detail:
                    roughness = 1f;
                    metallic = 0.04f;
                    break;
                case Role.Glass:
                    roughness = 0.12f;
                    metallic = 0.85f;
                    break;
                case Role.Wood:
                    roughness = 0.88f;
                    metallic = 0f;
                    emission = Rgb(0x0c, 0x0a, 0x07);
                    break;
                case Role.Shtora:
                    roughness = 0.9f;
                    metallic = 0.18f;
                    emission = Rgb(0x7c, 0x24, 0x10);
                    break;
                default:
                    roughness = 0.9f;
                    metallic = 0.18f;
                    emission = Rgb(0x0c, 0x10, 0x0a);
                    break;
            }
            SetFloat(material, "_Metallic", metallic);
            SetFloat(material, "_Glossiness", 1f - roughness);
            if (emission.maxColorComponent <= 0f) return;
            material.EnableKeyword("_EMISSION");
            if (material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", emission);
        }

        private static void SetFloat(
            Material material,
            string property,
            float value)
        {
            if (material.HasProperty(property))
                material.SetFloat(property, value);
        }

        private static Color Rgb(
            int red,
            int green,
            int blue)
        {
            const float scale = 1f / 255f;
            return new Color(
                red * scale,
                green * scale,
                blue * scale);
        }
    }
}
