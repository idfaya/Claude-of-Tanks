using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MMaterialRoles
    {
        private enum Role
        {
            Hull,
            Barrel,
            Wheel,
            Tire,
            Rubber,
            Track,
            Detail,
            Dark,
            Glass,
            Canvas,
            Shadow
        }

        public static bool Supports(string id)
        {
            return string.Equals(id, "t90m", StringComparison.Ordinal) ||
                string.Equals(
                    id,
                    "t90m_proryv",
                    StringComparison.Ordinal);
        }

        public static void Apply(
            Renderer[] renderers,
            Color baseColor)
        {
            if (renderers == null) return;
            Texture camouflage = FindCamouflage(renderers);
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                Material material = renderer?.sharedMaterial;
                if (material == null ||
                    renderer.gameObject.name.StartsWith(
                        "VehicleMarking-",
                        StringComparison.Ordinal))
                {
                    continue;
                }
                Apply(
                    material,
                    Resolve(renderer.gameObject.name),
                    camouflage,
                    baseColor);
            }
        }

        private static Texture FindCamouflage(Renderer[] renderers)
        {
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer != null &&
                    renderer.gameObject.name == "Hull")
                {
                    return renderer.sharedMaterial?.mainTexture;
                }
            }
            return null;
        }

        private static Role Resolve(string name)
        {
            if (string.Equals(
                    name,
                    "T90M-2A46M5MuzzleBoreDisc",
                    StringComparison.Ordinal))
            {
                return Role.Shadow;
            }
            if (name.IndexOf(
                    "Lens",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf(
                    "Glass",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Role.Glass;
            }
            if (name.IndexOf(
                    "GunBootSection",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "RearFuelDrum",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "UnditchingLog",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "BustleCanvasRoll",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "FanRelikt",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "InnerBrowRelikt",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "FlankRelikt",
                    StringComparison.Ordinal) >= 0 &&
                name.StartsWith(
                    "Painted-",
                    StringComparison.Ordinal))
            {
                return Role.Canvas;
            }
            if (name.IndexOf(
                    "Tire",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Tire;
            }
            if (name.IndexOf(
                    "Mudguard",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Rubber;
            }
            if (name.IndexOf(
                    "TrackPad",
                    StringComparison.Ordinal) >= 0 ||
                name.StartsWith(
                    "TrackLinks-",
                    StringComparison.Ordinal))
            {
                return Role.Track;
            }
            if (name.IndexOf(
                    "RoadWheelDisc",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "RoadWheelHub",
                    StringComparison.Ordinal) >= 0 &&
                name.StartsWith(
                    "Painted-",
                    StringComparison.Ordinal) ||
                name.IndexOf(
                    "ReturnRoller",
                    StringComparison.Ordinal) >= 0 &&
                name.StartsWith(
                    "Painted-",
                    StringComparison.Ordinal) ||
                name.StartsWith(
                    "Painted-T90M-Sprocket",
                    StringComparison.Ordinal) ||
                name.StartsWith(
                    "Painted-T90M-Idler",
                    StringComparison.Ordinal))
            {
                return Role.Wheel;
            }
            if (name.IndexOf(
                    "2A46M5",
                    StringComparison.Ordinal) >= 0 &&
                name.StartsWith(
                    "Painted-",
                    StringComparison.Ordinal))
            {
                return Role.Barrel;
            }
            if (name.StartsWith(
                    "T90M-ProryvChevron-",
                    StringComparison.Ordinal) &&
                (name.EndsWith(
                     "-Carrier",
                     StringComparison.Ordinal) ||
                 name.EndsWith(
                     "-Tile",
                     StringComparison.Ordinal)))
            {
                return Role.Hull;
            }
            if (string.Equals(
                    name,
                    "Hull",
                    StringComparison.Ordinal) ||
                string.Equals(
                    name,
                    "UpperHull",
                    StringComparison.Ordinal) ||
                string.Equals(
                    name,
                    "Turret",
                    StringComparison.Ordinal))
            {
                return Role.Hull;
            }
            if (string.Equals(
                    name,
                    "Gun",
                    StringComparison.Ordinal))
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
                    "Rib",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "Louvre",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "RoadWheelOuterRim",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "RoadWheelInnerRim",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Detail;
            }
            return Role.Dark;
        }

        private static void Apply(
            Material material,
            Role role,
            Texture camouflage,
            Color baseColor)
        {
            float roughness;
            float metallic;
            Color emission = Color.black;
            switch (role)
            {
                case Role.Hull:
                    roughness = 0.88f;
                    metallic = 0.05f;
                    SetPaint(material, camouflage, Color.white);
                    break;
                case Role.Barrel:
                    roughness = 0.80f;
                    metallic = 0.08f;
                    SetPaint(material, camouflage, Color.white);
                    break;
                case Role.Wheel:
                    roughness = 0.92f;
                    metallic = 0.08f;
                    SetSolid(material, baseColor * 0.68f);
                    break;
                case Role.Tire:
                    roughness = 0.96f;
                    metallic = 0f;
                    emission = Rgb(0x05, 0x05, 0x04);
                    SetSolid(material, Rgb(0x22, 0x20, 0x1b));
                    break;
                case Role.Rubber:
                    roughness = 0.96f;
                    metallic = 0f;
                    SetSolid(material, Rgb(0x29, 0x2a, 0x28));
                    break;
                case Role.Track:
                    roughness = 0.95f;
                    metallic = 0.08f;
                    SetSolid(material, Rgb(0x35, 0x36, 0x34));
                    break;
                case Role.Detail:
                    roughness = 1f;
                    metallic = 0.04f;
                    SetSolid(material, baseColor * 0.45f);
                    break;
                case Role.Glass:
                    roughness = 0.12f;
                    metallic = 0.85f;
                    SetSolid(material, Rgb(0x2a, 0x35, 0x40));
                    break;
                case Role.Canvas:
                    roughness = 0.97f;
                    metallic = 0f;
                    emission = Rgb(0x0a, 0x0d, 0x08);
                    SetSolid(material, Rgb(0x39, 0x48, 0x2e));
                    break;
                case Role.Shadow:
                    roughness = 0.98f;
                    metallic = 0f;
                    SetSolid(material, Rgb(0x0b, 0x0c, 0x0a));
                    break;
                default:
                    roughness = 0.90f;
                    metallic = 0.18f;
                    SetSolid(material, Rgb(0x36, 0x34, 0x2f));
                    break;
            }
            SetFloat(material, "_Metallic", metallic);
            SetFloat(material, "_Glossiness", 1f - roughness);
            if (material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", emission);
            if (emission.maxColorComponent > 0f)
                material.EnableKeyword("_EMISSION");
            else
                material.DisableKeyword("_EMISSION");
        }

        private static void SetPaint(
            Material material,
            Texture texture,
            Color color)
        {
            material.color = Opaque(color);
            material.mainTexture = texture;
        }

        private static void SetSolid(
            Material material,
            Color color)
        {
            material.color = Opaque(color);
            material.mainTexture = null;
        }

        private static void SetFloat(
            Material material,
            string property,
            float value)
        {
            if (material.HasProperty(property))
                material.SetFloat(property, value);
        }

        private static Color Opaque(Color color)
        {
            color.a = 1f;
            return color;
        }

        private static Color Rgb(int red, int green, int blue)
        {
            const float scale = 1f / 255f;
            return new Color(
                red * scale,
                green * scale,
                blue * scale,
                1f);
        }
    }
}
