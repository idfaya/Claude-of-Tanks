using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPattonProtectionDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;

            string id = definition.id;
            string prefix = Prefix(id);
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    prefix + "-GunFittings");
            float radius = Mathf.Max(
                0.08f,
                definition.armor?.gunBarrel?.radiusM ?? 0.08f);
            AddMask(
                fittings,
                color,
                width,
                radius,
                prefix,
                id == "m60a2");
            AddCoax(
                fittings,
                prefix);

            if (id == "m46_patton")
            {
                AddM46Gun(
                    fittings,
                    color,
                    radius,
                    prefix);
            }
            else if (id == "m47_patton")
            {
                AddM47Gun(
                    fittings,
                    color,
                    radius,
                    prefix);
            }
            else if (id == "m60a2")
            {
                AddStarshipGun(
                    fittings,
                    color,
                    radius,
                    prefix);
            }
            else
            {
                AddM68Gun(
                    fittings,
                    color,
                    radius,
                    prefix,
                    id == "m60a3");
                if (id == "m48")
                {
                    AddM48MantletHousing(
                        fittings,
                        color,
                        prefix);
                }
            }

            if (id == "m60a1" ||
                id == "m60a2" ||
                id == "m60a3")
            {
                AddSearchlight(
                    fittings,
                    color,
                    prefix,
                    id == "m60a2"
                        ? 0.72f
                        : id == "m60a3"
                            ? 1.42f
                            : 1f);
            }
        }

        private static string Prefix(string id)
        {
            if (id == "m46_patton") return "Patton-M46";
            if (id == "m47_patton") return "Patton-M47";
            if (id == "m48") return "Patton-M48";
            if (id == "m60a1") return "Patton-M60A1";
            if (id == "m60a2") return "Patton-M60A2";
            return "Patton-M60A3";
        }

        private static void AddMask(
            Transform fittings,
            Color color,
            float width,
            float radius,
            string prefix,
            bool starship)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-GunMask",
                PrimitiveType.Cube,
                fittings,
                new Vector3(
                    0f,
                    starship ? 0.08f : 0.03f,
                    starship ? 0.34f : 0.24f),
                new Vector3(
                    width * (starship ? 0.34f : 0.29f),
                    starship ? 0.7f : 0.48f,
                    starship ? 0.46f : 0.34f),
                color * 0.7f);
            Transform collar =
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-GunCollar",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, starship ? 0.62f : 0.47f),
                    new Vector3(
                        radius * (starship ? 2.4f : 1.9f),
                        0.2f,
                        radius * (starship ? 2.4f : 1.9f)),
                    color * 0.62f);
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddCoax(
            Transform fittings,
            string prefix)
        {
            TankDetailGeometry.Part(
                prefix + "-CoaxHousing",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.34f, -0.02f, 0.35f),
                new Vector3(0.12f, 0.11f, 0.28f),
                TankPattonFamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-CoaxBarrel",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.34f, -0.02f, 0.72f),
                new Vector3(0.026f, 0.026f, 0.5f),
                TankPattonFamilyDetails.Gunmetal());
        }

        private static void AddM46Gun(
            Transform fittings,
            Color color,
            float radius,
            string prefix)
        {
            AddSleeve(
                fittings,
                color,
                radius,
                prefix,
                2.65f,
                0.72f,
                1.5f);
            AddMuzzleDevice(
                fittings,
                color,
                radius,
                prefix + "-SingleBaffleBrake",
                4.05f,
                0.34f,
                2.15f);
        }

        private static void AddM47Gun(
            Transform fittings,
            Color color,
            float radius,
            string prefix)
        {
            AddSleeve(
                fittings,
                color,
                radius,
                prefix,
                2.55f,
                0.66f,
                1.35f);
            AddMuzzleDevice(
                fittings,
                color,
                radius,
                prefix + "-BlastDeflector",
                4.05f,
                0.46f,
                2.55f);
        }

        private static void AddM68Gun(
            Transform fittings,
            Color color,
            float radius,
            string prefix,
            bool sleeve)
        {
            AddSleeve(
                fittings,
                color,
                radius,
                prefix,
                2.2f,
                sleeve ? 1.8f : 0.34f,
                sleeve ? 1.28f : 1.55f);
            int clampCount = sleeve ? 3 : 1;
            for (int clamp = 0;
                clamp < clampCount;
                clamp++)
            {
                Transform ring =
                    TankDetailGeometry.Part(
                        "Painted-" + prefix +
                            "-ThermalSleeveClamp",
                        PrimitiveType.Cylinder,
                        fittings,
                        new Vector3(
                            0f,
                            0f,
                            1.55f + clamp * 0.78f),
                        new Vector3(
                            radius * 1.45f,
                            0.04f,
                            radius * 1.45f),
                        color * 0.58f);
                ring.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void AddStarshipGun(
            Transform fittings,
            Color color,
            float radius,
            string prefix)
        {
            for (int collar = 0;
                collar < 3;
                collar++)
            {
                Transform ring =
                    TankDetailGeometry.Part(
                        "Painted-" + prefix +
                            "-LauncherCollar",
                        PrimitiveType.Cylinder,
                        fittings,
                        new Vector3(
                            0f,
                            0f,
                            0.76f + collar * 0.18f),
                        new Vector3(
                            radius * (2.2f - collar * 0.25f),
                            0.04f,
                            radius * (2.2f - collar * 0.25f)),
                        color * 0.64f);
                ring.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            AddMuzzleDevice(
                fittings,
                color,
                radius,
                prefix + "-LauncherMuzzleRing",
                2.75f,
                0.08f,
                2.2f);
        }

        private static void AddM48MantletHousing(
            Transform fittings,
            Color color,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix +
                    "-MantletSightHousing",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0.38f, 0.72f),
                new Vector3(0.57f, 0.43f, 0.45f),
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-MantletSightLens",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0.38f, 0.96f),
                new Vector3(0.43f, 0.3f, 0.025f),
                TankPattonFamilyDetails.Lens());
        }

        private static void AddSleeve(
            Transform fittings,
            Color color,
            float radius,
            string prefix,
            float z,
            float length,
            float radiusScale)
        {
            Transform sleeve =
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-BoreEvacuator",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, z),
                    new Vector3(
                        radius * radiusScale,
                        length,
                        radius * radiusScale),
                    color * 0.66f);
            sleeve.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddMuzzleDevice(
            Transform fittings,
            Color color,
            float radius,
            string name,
            float z,
            float length,
            float radiusScale)
        {
            Transform device =
                TankDetailGeometry.Part(
                    "Painted-" + name,
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, z),
                    new Vector3(
                        radius * radiusScale,
                        length,
                        radius * radiusScale),
                    color * 0.58f);
            device.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddSearchlight(
            Transform fittings,
            Color color,
            string prefix,
            float scale)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-SearchlightYoke",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0.34f, 0.78f),
                new Vector3(
                    0.48f * scale,
                    0.08f,
                    0.34f),
                color * 0.62f);
            Transform housing =
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-Searchlight",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0.54f, 0.9f),
                    new Vector3(
                        0.22f * scale,
                        0.32f,
                        0.22f * scale),
                    color * 0.68f);
            housing.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                prefix + "-SearchlightLens",
                PrimitiveType.Cylinder,
                fittings,
                new Vector3(0f, 0.54f, 1.08f),
                new Vector3(
                    0.17f * scale,
                    0.025f,
                    0.17f * scale),
                TankPattonFamilyDetails.Lens())
                .localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
