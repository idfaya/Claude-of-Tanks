using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankChineseBasketDetails
    {
        public static void Build(
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
            switch (definition.id)
            {
                case "type59":
                    AddType59Rack(
                        turret,
                        color,
                        width,
                        roof,
                        rear);
                    break;
                case "ztz85_iii":
                    AddZtz85Basket(
                        turret,
                        color,
                        width,
                        roof,
                        rear);
                    break;
                case "type99a":
                    AddType99Basket(
                        turret,
                        color,
                        width,
                        roof,
                        rear);
                    break;
                case "ztz99a2":
                    AddZtz99A2Basket(
                        turret,
                        color,
                        width,
                        roof,
                        rear);
                    break;
                case "vt4a1":
                    AddVt4Basket(
                        turret,
                        color,
                        width,
                        roof,
                        rear);
                    break;
            }
        }

        private static void AddType59Rack(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear)
        {
            AddRearRack(
                turret,
                color,
                width * 0.46f,
                roof - 0.28f,
                rear - 0.26f,
                2,
                7,
                "Chinese-Type59");
            for (int pack = 0;
                pack < 2;
                pack++)
            {
                TankDetailGeometry.Part(
                    "Painted-Chinese-Type59-RackStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        (pack == 0 ? -1f : 1f) *
                            width * 0.12f,
                        roof - 0.28f,
                        rear - 0.12f),
                    new Vector3(
                        width * 0.2f,
                        0.18f,
                        0.3f),
                    color * (0.58f +
                        pack * 0.07f));
            }
        }

        private static void AddZtz85Basket(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear)
        {
            Color rail = color * 0.36f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int row = 0;
                    row < 3;
                    row++)
                {
                    TankDetailGeometry.Part(
                        "Chinese-ZTZ85III-SideBasketRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.34f,
                            roof - 0.5f +
                                row * 0.16f,
                            rear * 0.48f),
                        new Vector3(
                            0.03f,
                            0.03f,
                            Mathf.Abs(rear) * 1.2f),
                        rail);
                }
                for (int post = 0;
                    post < 6;
                    post++)
                {
                    TankDetailGeometry.Part(
                        "Chinese-ZTZ85III-SideBasketPost",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.34f,
                            roof - 0.34f,
                            0.14f +
                                rear * post * 0.2f),
                        new Vector3(0.03f, 0.36f, 0.03f),
                        rail);
                }
                TankDetailGeometry.Part(
                    "Painted-Chinese-ZTZ85III-BustlePack",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.17f,
                        roof - 0.12f,
                        rear * 0.76f),
                    new Vector3(
                        width * 0.28f,
                        0.16f,
                        Mathf.Abs(rear) * 0.42f),
                    color * 0.58f);
            }
            AddRearRack(
                turret,
                color,
                width * 0.54f,
                roof - 0.36f,
                rear - 0.28f,
                3,
                7,
                "Chinese-ZTZ85III");
        }

        private static void AddType99Basket(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear)
        {
            AddRearRack(
                turret,
                color,
                width * 0.72f,
                roof - 0.34f,
                rear - 0.36f,
                3,
                11,
                "Chinese-Type99A");
            for (int pack = 0;
                pack < 4;
                pack++)
            {
                TankDetailGeometry.Part(
                    "Painted-Chinese-Type99A-BasketStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.25f +
                            pack * width * 0.165f,
                        roof - 0.3f +
                            (pack % 2) * 0.05f,
                        rear - 0.16f),
                    new Vector3(
                        width * 0.18f,
                        0.2f,
                        0.32f),
                    color * (0.56f +
                        pack * 0.04f));
            }
        }

        private static void AddZtz99A2Basket(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear)
        {
            AddSideRails(
                turret,
                color,
                width,
                roof,
                rear,
                "Chinese-ZTZ99A2");
            AddRearRack(
                turret,
                color,
                width * 0.72f,
                roof - 0.36f,
                rear - 0.46f,
                3,
                9,
                "Chinese-ZTZ99A2");
            AddRearServiceComplex(
                turret,
                color,
                width,
                roof,
                rear);
        }

        private static void AddVt4Basket(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear)
        {
            AddSideRails(
                turret,
                color,
                width,
                roof,
                rear,
                "Chinese-VT4A1");
            AddRearRack(
                turret,
                color,
                width * 0.74f,
                roof - 0.35f,
                rear - 0.48f,
                3,
                8,
                "Chinese-VT4A1");
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Chinese-VT4A1-BustleCase",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.31f,
                        roof - 0.3f,
                        rear - 0.3f),
                    new Vector3(0.3f, 0.34f, 0.58f),
                    color * 0.58f);
            }
        }

        private static void AddSideRails(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear,
            string prefix)
        {
            Color rail = color * 0.36f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int row = 0;
                    row < 2;
                    row++)
                {
                    TankDetailGeometry.Part(
                        prefix + "-SideRackRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.38f,
                            roof - 0.44f +
                                row * 0.28f,
                            rear * 0.62f),
                        new Vector3(
                            0.03f,
                            0.03f,
                            Mathf.Abs(rear) * 0.72f),
                        rail);
                }
                for (int post = 0;
                    post < 3;
                    post++)
                {
                    TankDetailGeometry.Part(
                        prefix + "-SideRackPost",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.38f,
                            roof - 0.3f,
                            rear * (0.32f +
                                post * 0.3f)),
                        new Vector3(0.03f, 0.31f, 0.03f),
                        rail);
                }
            }
        }

        private static void AddRearRack(
            Transform turret,
            Color color,
            float rackWidth,
            float centerY,
            float z,
            int rows,
            int posts,
            string prefix)
        {
            Color rail = color * 0.36f;
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
                        centerY -
                            0.16f +
                            row *
                            (0.32f /
                             Mathf.Max(1, rows - 1)),
                        z),
                    new Vector3(
                        rackWidth,
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
                        -rackWidth * 0.46f +
                            post *
                            rackWidth * 0.92f /
                            Mathf.Max(1, posts - 1),
                        centerY,
                        z),
                    new Vector3(0.03f, 0.36f, 0.03f),
                    rail);
            }
        }

        private static void AddRearServiceComplex(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear)
        {
            for (int cabinet = 0;
                cabinet < 3;
                cabinet++)
            {
                float x =
                    (cabinet - 1) * width * 0.2f;
                float cabinetWidth =
                    cabinet == 1 ? 0.66f : 0.52f;
                TankDetailGeometry.Part(
                    "Painted-Chinese-ZTZ99A2-RearServiceCabinet",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x,
                        roof - 0.38f,
                        rear - 0.2f),
                    new Vector3(
                        cabinetWidth,
                        cabinet == 1 ? 0.42f : 0.34f,
                        0.18f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "Chinese-ZTZ99A2-RearServiceDoor",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x,
                        roof - 0.38f,
                        rear - 0.3f),
                    new Vector3(
                        cabinetWidth * 0.88f,
                        cabinet == 1 ? 0.35f : 0.28f,
                        0.02f),
                    TankChineseFamilyDetails.Gunmetal());
            }
            for (int louvre = 0;
                louvre < 5;
                louvre++)
            {
                TankDetailGeometry.Part(
                    "Chinese-ZTZ99A2-RearServiceLouvre",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.52f +
                            louvre * 0.06f,
                        rear - 0.32f),
                    new Vector3(0.46f, 0.025f, 0.03f),
                    color * 0.36f);
            }
        }
    }
}
