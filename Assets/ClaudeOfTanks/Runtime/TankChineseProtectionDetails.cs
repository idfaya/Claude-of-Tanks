using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankChineseProtectionDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            if (definition.id == "type59")
                AddType59VisualPackage(
                    turret,
                    color);
            AddGunPlant(
                turret,
                definition,
                color);
        }

        private static void AddType59VisualPackage(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 3;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Chinese-Type59-CheekCassette",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side *
                                (0.58f +
                                 panel * 0.23f),
                            0.52f -
                                panel * 0.012f,
                            0.88f -
                                panel * 0.16f),
                        new Vector3(0.11f, 0.25f, 0.34f),
                        color * 0.76f)
                        .localRotation =
                            Quaternion.Euler(
                                0f,
                                -side * 39f,
                                side * 2f);
                }
                for (int panel = 0;
                    panel < 4;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Chinese-Type59-CheekCassette",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 1.49f,
                            0.43f,
                            0.38f -
                                panel * 0.35f),
                        new Vector3(0.12f, 0.28f, 0.3f),
                        color * 0.72f);
                }
                TankDetailGeometry.Part(
                    "Chinese-Type59-CassetteSupport",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.405f,
                        0.36f,
                        -0.12f),
                    new Vector3(0.055f, 0.055f, 1.55f),
                    TankChineseFamilyDetails.Gunmetal());
            }
        }

        private static void AddGunPlant(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            string prefix = Prefix(definition.id);
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    prefix + "-GunFittings");
            Vector3 maskSize =
                definition.id == "type59"
                    ? new Vector3(0.62f, 0.5f, 0.38f)
                    : definition.id == "vt4a1"
                        ? new Vector3(0.72f, 0.46f, 0.5f)
                        : new Vector3(0.68f, 0.48f, 0.42f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-GunMask",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.12f),
                maskSize,
                color * 0.64f);
            Transform collar =
                TankDetailGeometry.Part(
                    prefix + "-GunMaskCollar",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, 0.42f),
                    new Vector3(
                        definition.id == "type59"
                            ? 0.16f
                            : 0.19f,
                        0.2f,
                        definition.id == "type59"
                            ? 0.16f
                            : 0.19f),
                    TankChineseFamilyDetails.Gunmetal());
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            AddCoax(
                fittings,
                definition.id,
                prefix);
            AddThermalSleeve(
                fittings,
                definition,
                prefix);
            if (definition.id == "type59")
                AddType59Searchlight(
                    fittings,
                    color,
                    prefix);
            if (definition.id == "ztz85_iii")
                AddGunInfrared(
                    fittings,
                    color,
                    prefix,
                    -0.38f);
        }

        private static void AddCoax(
            Transform fittings,
            string id,
            string prefix)
        {
            float x =
                id == "type59" ? 0.3f : 0.27f;
            float y =
                id == "type59" ? -0.06f : 0.06f;
            float z =
                id == "type59" ? 0.32f : 0.54f;
            TankDetailGeometry.Part(
                prefix + "-CoaxHousing",
                PrimitiveType.Cube,
                fittings,
                new Vector3(x, y, z),
                id == "type59"
                    ? new Vector3(0.1f, 0.11f, 0.2f)
                    : new Vector3(0.14f, 0.17f, 0.28f),
                TankChineseFamilyDetails.Gunmetal());
            Transform barrel =
                TankDetailGeometry.Part(
                    prefix + "-CoaxBarrel",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(
                        x,
                        y,
                        z + 0.27f),
                    new Vector3(0.026f, 0.25f, 0.026f),
                    TankChineseFamilyDetails.Gunmetal());
            barrel.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddThermalSleeve(
            Transform fittings,
            VehicleDefinition definition,
            string prefix)
        {
            if (definition.id == "type59") return;
            float barrelLength =
                definition.armor?.gunBarrel?.lengthM ??
                4.5f;
            int clampCount =
                definition.id == "ztz85_iii" ? 3 : 5;
            float start =
                definition.id == "ztz85_iii"
                    ? 1.5f
                    : 1.6f;
            float usable =
                Mathf.Max(
                    0.6f,
                    barrelLength - start - 0.6f);
            for (int clamp = 0;
                clamp < clampCount;
                clamp++)
            {
                float z =
                    start +
                    usable * clamp /
                    Mathf.Max(1, clampCount - 1);
                Transform item =
                    TankDetailGeometry.Part(
                        prefix + "-ThermalClamp",
                        PrimitiveType.Cylinder,
                        fittings,
                        new Vector3(0f, 0f, z),
                        new Vector3(0.11f, 0.025f, 0.11f),
                        TankChineseFamilyDetails.Gunmetal());
                item.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void AddType59Searchlight(
            Transform fittings,
            Color color,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-SearchlightShelf",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.42f, 0.085f, 0.42f),
                new Vector3(0.52f, 0.055f, 0.34f),
                color * 0.64f);
            for (int cradle = -1;
                cradle <= 1;
                cradle += 2)
            {
                TankDetailGeometry.Part(
                    prefix + "-SearchlightCradle",
                    PrimitiveType.Cube,
                    fittings,
                    new Vector3(
                        0.52f,
                        0.16f,
                        0.42f +
                            cradle * 0.09f),
                    new Vector3(0.3f, 0.1f, 0.06f),
                    TankChineseFamilyDetails.Gunmetal());
            }
            Transform drum =
                TankDetailGeometry.Part(
                    prefix + "-SearchlightDrum",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0.52f, 0.36f, 0.5f),
                    new Vector3(0.24f, 0.15f, 0.24f),
                    TankChineseFamilyDetails.Gunmetal());
            drum.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-SearchlightBezel",
                PrimitiveType.Cylinder,
                fittings,
                new Vector3(0.52f, 0.36f, 0.65f),
                new Vector3(0.245f, 0.03f, 0.245f),
                color * 0.7f)
                .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                prefix + "-SearchlightLens",
                PrimitiveType.Cylinder,
                fittings,
                new Vector3(0.52f, 0.36f, 0.672f),
                new Vector3(0.19f, 0.01f, 0.19f),
                TankChineseFamilyDetails.Lens())
                .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            AddGunInfrared(
                fittings,
                color,
                prefix,
                0.39f);
        }

        private static void AddGunInfrared(
            Transform fittings,
            Color color,
            string prefix,
            float x)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-GunInfraredMount",
                PrimitiveType.Cube,
                fittings,
                new Vector3(x, 0.05f, 0.2f),
                new Vector3(0.05f, 0.12f, 0.05f),
                color * 0.6f);
            Transform housing =
                TankDetailGeometry.Part(
                    prefix + "-GunInfraredHousing",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(x, 0.14f, 0.3f),
                    new Vector3(0.095f, 0.065f, 0.095f),
                    TankChineseFamilyDetails.Gunmetal());
            housing.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                prefix + "-GunInfraredLens",
                PrimitiveType.Cylinder,
                fittings,
                new Vector3(x, 0.14f, 0.37f),
                new Vector3(0.075f, 0.01f, 0.075f),
                TankChineseFamilyDetails.Lens())
                .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
        }

        private static string Prefix(string id)
        {
            switch (id)
            {
                case "type59":
                    return "Chinese-Type59";
                case "ztz85_iii":
                    return "Chinese-ZTZ85III";
                case "type99a":
                    return "Chinese-Type99A";
                case "ztz99a2":
                    return "Chinese-ZTZ99A2";
                default:
                    return "Chinese-VT4A1";
            }
        }
    }
}
