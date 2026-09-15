using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSwedishWeaponDetails
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            if (TankSwedishFamilyDetails
                .IsCasemate(definition.id))
                return;
            if (definition.id == "strv81" ||
                definition.id == "strv122")
            {
                AddTankGunPlant(
                    turret,
                    definition.id,
                    color);
                return;
            }
            if (definition.id == "cv90" ||
                definition.id == "cv90_mkiv")
            {
                AddCv90GunPlant(
                    turret,
                    definition.id,
                    color);
                AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    width);
                if (definition.id == "cv90_mkiv")
                    AddMissilePod(
                        turret,
                        definition,
                        color,
                        width);
            }
        }

        private static void AddTankGunPlant(
            Transform turret,
            string id,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            string prefix =
                id == "strv122"
                    ? "Swedish-Strv122"
                    : "Swedish-Strv81";
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    prefix + "-GunFittings");
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-GunMask",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.1f),
                id == "strv122"
                    ? new Vector3(0.86f, 0.56f, 0.4f)
                    : new Vector3(0.72f, 0.54f, 0.36f),
                color * 0.64f);
            Transform collar =
                TankDetailGeometry.Part(
                    prefix + "-GunMaskCollar",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, 0.4f),
                    new Vector3(0.18f, 0.2f, 0.18f),
                    Gunmetal());
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                prefix + "-CoaxHousing",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.28f, 0.07f, 0.54f),
                new Vector3(0.14f, 0.17f, 0.28f),
                Gunmetal());
            if (id == "strv81")
            {
                const float barrelLength = 6.132f;
                const float barrelRadius = 0.082f;
                Transform rootSleeve =
                    TankShapeFactory.CylinderPart(
                        "Painted-Swedish-Strv81-20PdrRootSleeve",
                        fittings,
                        0.145f,
                        0.135f,
                        0.72f,
                        18,
                        TankShapeAxis.Z,
                        color * 0.46f);
                rootSleeve.localPosition =
                    new Vector3(0f, 0f, 0.78f);
                Transform tube =
                    TankShapeFactory.CylinderPart(
                        "Painted-Swedish-Strv81-20PdrGunTube",
                        fittings,
                        barrelRadius,
                        barrelRadius * 0.96f,
                        barrelLength - 1.14f,
                        24,
                        TankShapeAxis.Z,
                        color * 0.46f);
                tube.localPosition =
                    new Vector3(
                        0f,
                        0f,
                        (1.14f + barrelLength) * 0.5f);
                Transform bore =
                    TankShapeFactory.CylinderPart(
                        "Swedish-Strv81-20PdrMuzzleBore",
                        fittings,
                        0.050f,
                        0.050f,
                        0.045f,
                        16,
                        TankShapeAxis.Z,
                        new Color(0.025f, 0.028f, 0.024f));
                bore.localPosition =
                    new Vector3(0f, 0f, barrelLength - 0.01f);
            }
        }

        private static void AddCv90GunPlant(
            Transform turret,
            string id,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            bool mkiv = id == "cv90_mkiv";
            string prefix =
                mkiv
                    ? "Swedish-CV90MkIV"
                    : "Swedish-CV90";
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    prefix + "-GunFittings");
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-GunShroud",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, mkiv ? 0.62f : 0.52f),
                mkiv
                    ? new Vector3(0.74f, 0.5f, 1.25f)
                    : new Vector3(0.64f, 0.44f, 1.05f),
                color * 0.62f);
            if (!mkiv)
            {
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    for (int brace = 0;
                        brace < 3;
                        brace++)
                    {
                        Transform item =
                            TankDetailGeometry.Part(
                                "Swedish-CV90-CradleBrace",
                                PrimitiveType.Cube,
                                fittings,
                                new Vector3(
                                    side * 0.31f,
                                    -0.12f +
                                        brace * 0.12f,
                                    0.45f +
                                        brace * 0.26f),
                                new Vector3(0.035f, 0.42f, 0.035f),
                                Gunmetal());
                        item.localRotation =
                            Quaternion.Euler(
                                side * (brace % 2 == 0
                                    ? 28f
                                    : -28f),
                                0f,
                                0f);
                    }
                }
            }
            Transform collar =
                TankDetailGeometry.Part(
                    prefix + "-GunCollar",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, mkiv ? 1.3f : 1.08f),
                    new Vector3(
                        mkiv ? 0.18f : 0.14f,
                        0.2f,
                        mkiv ? 0.18f : 0.14f),
                    Gunmetal());
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            float barrelLength = mkiv ? 3.384f : 2.808f;
            float barrelRadius = mkiv ? 0.074f : 0.058f;
            float barrelStart = mkiv ? 1.26f : 1.06f;
            Transform barrel = TankShapeFactory.CylinderPart(
                "Painted-" + prefix + "-MainGunTube",
                fittings,
                barrelRadius,
                barrelRadius * 0.94f,
                barrelLength - barrelStart,
                20,
                TankShapeAxis.Z,
                color * 0.46f);
            barrel.localPosition =
                new Vector3(
                    0f,
                    0f,
                    (barrelStart + barrelLength) * 0.5f);
            Transform bore = TankShapeFactory.CylinderPart(
                prefix + "-MuzzleBore",
                fittings,
                barrelRadius * 0.58f,
                barrelRadius * 0.58f,
                0.045f,
                16,
                TankShapeAxis.Z,
                new Color(0.025f, 0.028f, 0.024f));
            bore.localPosition =
                new Vector3(0f, 0f, barrelLength - 0.01f);
        }

        private static void AddRemoteWeaponStation(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            bool mkiv =
                definition.id == "cv90_mkiv";
            string prefix =
                mkiv
                    ? "Swedish-CV90MkIV-RWS"
                    : "Swedish-CV90-RWS";
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float x =
                width * (mkiv ? 0.19f : 0.15f);
            float z =
                mkiv ? -1.18f : -0.9f;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Base",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, roof + 0.055f, z),
                new Vector3(0.21f, 0.055f, 0.21f),
                color * 0.68f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    prefix + "-YokeArm",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x + side * 0.18f,
                        roof + 0.27f,
                        z),
                    new Vector3(0.04f, 0.36f, 0.16f),
                    Gunmetal());
            }
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.32f,
                    z + 0.06f),
                new Vector3(0.3f, 0.16f, 0.4f),
                color * 0.58f);
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.33f,
                    z + 0.58f),
                new Vector3(0.03f, 0.03f, 0.72f),
                Gunmetal());
            for (int link = 0;
                link < 4;
                link++)
            {
                TankDetailGeometry.Part(
                    prefix + "-FeedBelt",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x - 0.2f,
                        roof + 0.25f -
                            link * 0.04f,
                        z + 0.02f +
                            link * 0.05f),
                    new Vector3(0.04f, 0.04f, 0.08f),
                    Gunmetal());
            }
            TankSwedishFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    x - 0.24f,
                    roof + 0.31f,
                    z + 0.02f),
                new Vector3(0.14f, 0.17f, 0.14f),
                prefix + "-Sensor");
        }

        private static void AddMissilePod(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float x =
                -width * 0.33f;
            float z = -0.28f;
            TankDetailGeometry.Part(
                "Painted-Swedish-CV90MkIV-MissilePod",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof - 0.18f,
                    z),
                new Vector3(0.3f, 0.5f, 0.68f),
                color * 0.62f);
            for (int cell = 0;
                cell < 2;
                cell++)
            {
                Transform tube =
                    TankDetailGeometry.Part(
                        "Swedish-CV90MkIV-MissileCell",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            x,
                            roof - 0.29f +
                                cell * 0.28f,
                            z + 0.34f),
                        new Vector3(0.105f, 0.47f, 0.105f),
                        Gunmetal());
                tube.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                Transform collar =
                    TankDetailGeometry.Part(
                        "Swedish-CV90MkIV-MissileCollar",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            x,
                            roof - 0.29f +
                                cell * 0.28f,
                            z + 0.82f),
                        new Vector3(0.11f, 0.02f, 0.11f),
                        Steel());
                collar.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }

        private static Color Steel()
        {
            return new Color(0.12f, 0.13f, 0.12f);
        }
    }
}
