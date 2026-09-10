using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MaterialApplicator
    {
        internal enum Role
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
            Canvas,
            Shadow,
            Shtora
        }

        public static void Apply(
            Renderer[] renderers,
            string vehicleId,
            Color baseColor)
        {
            if (renderers == null) return;
            if (TankT90MMaterialRoles.Supports(vehicleId))
            {
                TankT90MMaterialRoles.Apply(renderers, baseColor);
                return;
            }
            bool isT90SM =
                string.Equals(
                    vehicleId,
                    "t90sm",
                    StringComparison.Ordinal);
            bool isT90MS =
                string.Equals(
                    vehicleId,
                    "t90ms",
                    StringComparison.Ordinal);
            bool usesT90AColors =
                string.Equals(
                    vehicleId,
                    "t90a",
                    StringComparison.Ordinal) ||
                string.Equals(
                    vehicleId,
                    "t90a_vladimir",
                    StringComparison.Ordinal) ||
                string.Equals(
                    vehicleId,
                    "t90a_burlak",
                    StringComparison.Ordinal) ||
                isT90SM ||
                isT90MS;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                Material material = renderer?.sharedMaterial;
                if (material == null) continue;
                if (renderer.gameObject.name.StartsWith(
                        "VehicleMarking-",
                        StringComparison.Ordinal))
                {
                    continue;
                }
                Role role = ResolveRole(
                    renderer.gameObject.name,
                    usesT90AColors,
                    isT90SM,
                    isT90MS);
                ApplyRole(
                    material,
                    role,
                    usesT90AColors,
                    isT90MS,
                    baseColor);
            }
        }

        private static Role ResolveRole(
            string name,
            bool isT90A,
            bool isT90SM,
            bool isT90MS)
        {
            Role? t90MSRole =
                isT90MS
                    ? TankT90MSMaterialRoles.Resolve(name)
                    : null;
            if (t90MSRole.HasValue) return t90MSRole.Value;
            if (isT90SM &&
                string.Equals(
                    name,
                    "T90SM-UnditchingLog",
                    StringComparison.Ordinal))
            {
                return Role.Wood;
            }
            if (isT90SM &&
                string.Equals(
                    name,
                    "T90SM-TurretRelikt",
                    StringComparison.Ordinal))
            {
                return Role.Track;
            }
            if (isT90SM &&
                (name.IndexOf(
                     "Aperture",
                     StringComparison.Ordinal) >= 0 ||
                 string.Equals(
                     name,
                     "T90SM-PanoramaWindow",
                     StringComparison.Ordinal)))
            {
                return Role.Glass;
            }
            if (isT90SM &&
                (string.Equals(
                     name,
                     "T90SM-FrontMudFlap",
                     StringComparison.Ordinal) ||
                 string.Equals(
                     name,
                     "T90SM-RearMudFlap",
                     StringComparison.Ordinal)))
            {
                return Role.Rubber;
            }
            if (string.Equals(
                    name,
                    "T90-ShtoraLens",
                    StringComparison.Ordinal) ||
                string.Equals(
                    name,
                    "T90A-ShtoraLens",
                    StringComparison.Ordinal) ||
                string.Equals(
                    name,
                    "T90AVladimir-ShtoraLens",
                    StringComparison.Ordinal))
            {
                return Role.Shtora;
            }
            if (isT90A &&
                string.Equals(
                    name,
                    "Painted-T90A-ShtoraHousing",
                    StringComparison.Ordinal))
            {
                return Role.Dark;
            }
            if (isT90A &&
                (name.IndexOf(
                        "ShtoraTop",
                        StringComparison.Ordinal) >= 0 ||
                 name.IndexOf(
                        "ShtoraRim",
                        StringComparison.Ordinal) >= 0 ||
                 name.IndexOf(
                        "ShtoraSidePlate",
                        StringComparison.Ordinal) >= 0))
            {
                return Role.Detail;
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
                    StringComparison.Ordinal) ||
                string.Equals(
                    name,
                    "T90A-SplitUnditchingLog",
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
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "FrontFlap",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Rubber;
            }
            if (name.IndexOf(
                    "RoadWheelDisc",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "RoadWheelDish",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "RoadWheelSpoke",
                    StringComparison.Ordinal) >= 0 ||
                name.StartsWith(
                    "RoadWheel-",
                    StringComparison.Ordinal) ||
                name.IndexOf(
                    "RoadWheelHub",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "ReturnRollerDisc",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "ReturnRoller",
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
                name.StartsWith(
                    "TrackLinks-",
                    StringComparison.Ordinal) ||
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
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "MuzzleCollar",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "T90AVladimir-Tube",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "T90AVladimir-RootSleeve",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "T90AVladimir-Saddle",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "T90AVladimir-CastGunRoot",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "T90AVladimir-FumeExtractor",
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "T90ABurlak-BarrelCourse",
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
                    StringComparison.Ordinal) >= 0 ||
                name.IndexOf(
                    "BustleRail",
                    StringComparison.Ordinal) >= 0)
            {
                return Role.Detail;
            }
            return Role.Dark;
        }

        private static void ApplyRole(
            Material material,
            Role role,
            bool isT90A,
            bool isT90MS,
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
                case Role.Canvas:
                    roughness = 0.97f;
                    metallic = 0f;
                    break;
                case Role.Shadow:
                    roughness = 0.98f;
                    metallic = 0f;
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
            if (isT90A)
                ApplyT90AColor(
                    material,
                    role,
                    isT90MS,
                    baseColor);
            if (emission.maxColorComponent <= 0f) return;
            material.EnableKeyword("_EMISSION");
            if (material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", emission);
        }

        private static void ApplyT90AColor(
            Material material,
            Role role,
            bool isT90MS,
            Color baseColor)
        {
            switch (role)
            {
                case Role.Dark:
                    material.color = isT90MS
                        ? Rgb(0x3a, 0x3a, 0x2e)
                        : Rgb(0x32, 0x36, 0x29);
                    material.mainTexture = null;
                    break;
                case Role.Rubber:
                    material.color = isT90MS
                        ? Rgb(0x3d, 0x3c, 0x35)
                        : Rgb(0x3b, 0x3a, 0x33);
                    material.mainTexture = null;
                    break;
                case Role.Track:
                    material.color = Rgb(0x35, 0x36, 0x34);
                    material.mainTexture = null;
                    break;
                case Role.Glass:
                    material.color = Rgb(0x2a, 0x35, 0x40);
                    material.mainTexture = null;
                    break;
                case Role.Wood:
                    material.color = Rgb(0x47, 0x3e, 0x32);
                    material.mainTexture = null;
                    break;
                case Role.Canvas:
                    material.color = Rgb(0x42, 0x45, 0x2f);
                    material.mainTexture = null;
                    break;
                case Role.Shadow:
                    material.color = Rgb(0x0b, 0x0c, 0x0a);
                    material.mainTexture = null;
                    break;
                case Role.Shtora:
                    material.color = Rgb(0x54, 0x18, 0x0e);
                    material.mainTexture = null;
                    break;
                case Role.Wheel:
                    material.color = baseColor * 0.48f;
                    material.color = WithOpaqueAlpha(material.color);
                    material.mainTexture = null;
                    break;
                case Role.Detail:
                    material.color = baseColor * 0.45f;
                    material.color = WithOpaqueAlpha(material.color);
                    material.mainTexture = null;
                    break;
            }
        }

        private static Color WithOpaqueAlpha(Color color)
        {
            color.a = 1f;
            return color;
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
