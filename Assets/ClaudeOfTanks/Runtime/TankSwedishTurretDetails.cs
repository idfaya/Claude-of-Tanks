using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSwedishTurretDetails
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            switch (definition.id)
            {
                case "strv81":
                    AddStrv81(
                        turret,
                        definition,
                        color,
                        width);
                    break;
                case "strv122":
                    AddStrv122(
                        turret,
                        definition,
                        color,
                        width);
                    break;
                case "cv90":
                case "cv90_mkiv":
                    AddCv90(
                        turret,
                        definition,
                        color,
                        width);
                    break;
            }
        }

        private static void AddStrv81(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            TankShapeFactory.LathePart(
                "Painted-Swedish-Strv81-CastTurretShell",
                turret,
                new[]
                {
                    1.16f,
                    1.30f,
                    1.36f,
                    1.24f,
                    0.88f,
                    0.42f
                },
                new[]
                {
                    -0.18f,
                    0.02f,
                    0.28f,
                    0.52f,
                    0.72f,
                    0.82f
                },
                32,
                1.10f,
                color * 0.62f,
                1.45f,
                0.82f);
            Transform bustle = TankShapeFactory.BoxPart(
                "Painted-Swedish-Strv81-BustleShell",
                turret,
                new Vector3(2.05f, 0.48f, 1.02f),
                color * 0.56f);
            bustle.localPosition =
                new Vector3(0f, 0.30f, -1.30f);
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.35f);
            AddHatches(
                turret,
                color,
                roof,
                width,
                "Swedish-Strv81");
            TankSwedishFamilyDetails.AddSmokeBanks(
                turret,
                color,
                roof,
                halfWidth,
                0.1f,
                10,
                "Swedish-Strv81");
            TankSwedishFamilyDetails.AddAntennas(
                turret,
                color,
                roof,
                rear * 0.8f,
                width,
                2,
                "Swedish-Strv81");
            TankSwedishFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.14f,
                    roof + 0.17f,
                    -0.36f),
                new Vector3(0.34f, 0.31f, 0.32f),
                "Swedish-Strv81-GunnerSight");
            TankSwedishFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.14f,
                    0.08f,
                    -0.5f),
                "Swedish-Strv81-Ksp58",
                false,
                true);
            AddVisionRing(
                turret,
                -width * 0.14f,
                -0.5f,
                roof,
                5,
                "Swedish-Strv81-CupolaVision");
            AddStrv81Ventilator(
                turret,
                color,
                width,
                roof);
            AddRearBasket(
                turret,
                color,
                width,
                roof,
                rear,
                "Swedish-Strv81",
                3,
                7);
        }

        private static void AddStrv122(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            TankSwedishFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.155f,
                    roof + 0.22f,
                    -0.62f),
                new Vector3(0.34f, 0.34f, 0.34f),
                "Swedish-Strv122-PanoramicSight");
            TankSwedishFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.14f,
                    0.08f,
                    -0.5f),
                "Swedish-Strv122-Ksp58",
                false,
                true);
            AddVisionRing(
                turret,
                width * 0.11f,
                -0.7f,
                roof,
                6,
                "Swedish-Strv122-CupolaVision");
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Swedish-Strv122-RoofServiceBox",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.21f,
                        roof + 0.08f,
                        -1.26f),
                    new Vector3(0.32f, 0.14f, 0.4f),
                    color * 0.68f);
            }
            AddRearBasket(
                turret,
                color,
                width,
                roof,
                rear,
                "Swedish-Strv122",
                4,
                9);
        }

        private static void AddCv90(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            bool mkiv =
                definition.id == "cv90_mkiv";
            string prefix =
                mkiv
                    ? "Swedish-CV90MkIV"
                    : "Swedish-CV90";
            AddCv90TurretShell(
                turret,
                prefix,
                color,
                mkiv);
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.34f);
            TankSwedishFamilyDetails.AddSmokeBanks(
                turret,
                color,
                roof,
                halfWidth,
                0.04f,
                mkiv ? 12 : 8,
                prefix);
            TankSwedishFamilyDetails.AddAntennas(
                turret,
                color,
                roof,
                rear * 0.78f,
                width,
                2,
                prefix);
            TankSwedishFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.15f,
                    roof + 0.17f,
                    0.5f),
                mkiv
                    ? new Vector3(0.4f, 0.35f, 0.4f)
                    : new Vector3(0.34f, 0.3f, 0.34f),
                prefix + "-GunnerSight");
            TankSwedishFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.14f,
                    roof + 0.27f,
                    -0.5f),
                mkiv
                    ? new Vector3(0.36f, 0.38f, 0.34f)
                    : new Vector3(0.32f, 0.3f, 0.3f),
                prefix + "-PanoramicSight");
            AddVisionRing(
                turret,
                -width * 0.14f,
                -0.5f,
                roof,
                5,
                prefix + "-CupolaVision");
            AddRearBasket(
                turret,
                color,
                width,
                roof,
                rear,
                prefix,
                mkiv ? 4 : 3,
                mkiv ? 7 : 5);
            if (mkiv)
                AddMkivSensorHeads(
                    turret,
                    color,
                    width,
                    roof);
        }

        private static void AddCv90TurretShell(
            Transform turret,
            string prefix,
            Color color,
            bool mkiv)
        {
            Vector2[] plan = mkiv
                ? new[]
                {
                    new Vector2(-0.28f, 1.82f),
                    new Vector2(0.28f, 1.82f),
                    new Vector2(0.90f, 1.27f),
                    new Vector2(1.46f, 0.46f),
                    new Vector2(1.42f, -1.18f),
                    new Vector2(1.16f, -1.82f),
                    new Vector2(0.72f, -2.08f),
                    new Vector2(-0.72f, -2.08f),
                    new Vector2(-1.16f, -1.82f),
                    new Vector2(-1.42f, -1.18f),
                    new Vector2(-1.46f, 0.46f),
                    new Vector2(-0.90f, 1.27f)
                }
                : new[]
                {
                    new Vector2(-0.27f, 1.52f),
                    new Vector2(0.27f, 1.52f),
                    new Vector2(0.80f, 1.12f),
                    new Vector2(1.14f, 0.44f),
                    new Vector2(1.10f, -0.92f),
                    new Vector2(0.90f, -1.56f),
                    new Vector2(0.64f, -1.72f),
                    new Vector2(-0.64f, -1.72f),
                    new Vector2(-0.90f, -1.56f),
                    new Vector2(-1.10f, -0.92f),
                    new Vector2(-1.14f, 0.44f),
                    new Vector2(-0.80f, 1.12f)
                };
            TankShapeFactory.PolyMultiLoftPart(
                "Painted-" + prefix + "-TurretShell",
                turret,
                plan,
                new[]
                {
                    new TankShapeLoftRing(
                        0f,
                        1.00f,
                        Vector2.zero),
                    new TankShapeLoftRing(
                        mkiv ? 0.30f : 0.25f,
                        mkiv ? 0.94f : 0.96f,
                        Vector2.zero),
                    new TankShapeLoftRing(
                        mkiv ? 0.86f : 0.72f,
                        mkiv ? 0.62f : 0.72f,
                        Vector2.zero)
                },
                color * 0.64f);
        }

        private static void AddHatches(
            Transform turret,
            Color color,
            float roof,
            float width,
            string prefix)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.15f,
                        roof + 0.035f,
                        -0.5f),
                    new Vector3(0.2f, 0.035f, 0.2f),
                    color * 0.82f);
            }
        }

        private static void AddVisionRing(
            Transform turret,
            float x,
            float z,
            float roof,
            int count,
            string name)
        {
            for (int block = 0;
                block < count;
                block++)
            {
                float angle =
                    block * Mathf.PI * 2f / count;
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x +
                            Mathf.Cos(angle) * 0.22f,
                        roof + 0.095f,
                        z +
                            Mathf.Sin(angle) * 0.22f),
                    new Vector3(0.085f, 0.05f, 0.035f),
                    Lens());
            }
        }

        private static void AddStrv81Ventilator(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            Transform housing =
                TankDetailGeometry.Part(
                    "Painted-Swedish-Strv81-SideVentilator",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        width * 0.34f,
                        roof - 0.31f,
                        -0.64f),
                    new Vector3(0.2f, 0.1f, 0.2f),
                    color * 0.64f);
            housing.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
            for (int vane = 0;
                vane < 6;
                vane++)
            {
                float angle =
                    vane * Mathf.PI / 3f;
                TankDetailGeometry.Part(
                    "Swedish-Strv81-VentilatorVane",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        width * 0.365f,
                        roof - 0.31f +
                            Mathf.Sin(angle) * 0.12f,
                        -0.64f +
                            Mathf.Cos(angle) * 0.12f),
                    new Vector3(0.03f, 0.18f, 0.03f),
                    Gunmetal());
            }
        }

        private static void AddRearBasket(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear,
            string prefix,
            int rows,
            int posts)
        {
            Color rail = color * 0.38f;
            for (int row = 0;
                row < rows;
                row++)
            {
                TankDetailGeometry.Part(
                    prefix + "-RearBasketRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.43f +
                            row * 0.14f,
                        rear - 0.32f),
                    new Vector3(
                        width * 0.7f,
                        0.03f,
                        0.03f),
                    rail);
            }
            for (int post = 0;
                post < posts;
                post++)
            {
                TankDetailGeometry.Part(
                    prefix + "-RearBasketRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.32f +
                            post *
                            (width * 0.64f /
                             Mathf.Max(1, posts - 1)),
                        roof - 0.27f,
                        rear - 0.32f),
                    new Vector3(0.03f, 0.36f, 0.03f),
                    rail);
            }
            for (int pack = 0;
                pack < 3;
                pack++)
            {
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-BasketStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.18f +
                            pack * width * 0.18f,
                        roof - 0.28f,
                        rear - 0.12f),
                    new Vector3(
                        width * 0.2f,
                        0.18f,
                        0.3f),
                    color * (0.58f +
                        pack * 0.05f));
            }
        }

        private static void AddMkivSensorHeads(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int end = -1;
                    end <= 1;
                    end += 2)
                {
                    TankSwedishFamilyDetails.AddSight(
                        turret,
                        color,
                        new Vector3(
                            side * width * 0.29f,
                            roof - 0.04f,
                            end > 0 ? 0.4f : -1.38f),
                        new Vector3(0.22f, 0.18f, 0.18f),
                        "Swedish-CV90MkIV-SensorHead");
                }
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }

        private static Color Lens()
        {
            return new Color(0.025f, 0.14f, 0.17f);
        }
    }
}
