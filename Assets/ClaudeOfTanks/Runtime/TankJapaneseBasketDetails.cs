using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankJapaneseBasketDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            string id = definition.id;
            if (id == "type74")
            {
                AddType74SideBaskets(
                    turret,
                    definition,
                    color,
                    width);
                return;
            }
            if (id == "type10" ||
                id == "type10b")
            {
                AddType10SlatRack(
                    turret,
                    definition,
                    color,
                    width);
                return;
            }
            AddRearBasket(
                turret,
                definition,
                color,
                width,
                id == "stb1"
                    ? "Japanese-STB1"
                    : id == "type90a"
                        ? "Japanese-Type90A"
                        : "Japanese-Type90");
        }

        private static void AddRearBasket(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            string prefix)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            Color rail = color * 0.38f;
            for (int row = 0;
                row < 3;
                row++)
            {
                TankDetailGeometry.Part(
                    prefix + "-BustleRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.4f +
                            row * 0.16f,
                        rear - 0.2f),
                    new Vector3(
                        width * 0.72f,
                        0.028f,
                        0.028f),
                    rail);
            }
            for (int post = 0;
                post < 7;
                post++)
            {
                TankDetailGeometry.Part(
                    prefix + "-BustleRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.32f +
                            post * width * 0.106f,
                        roof - 0.24f,
                        rear - 0.2f),
                    new Vector3(0.028f, 0.35f, 0.028f),
                    rail);
            }
            for (int pack = 0;
                pack < 4;
                pack++)
            {
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-BustleStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.2f +
                            pack * width * 0.13f,
                        roof - 0.24f +
                            (pack % 2) * 0.05f,
                        rear - 0.06f),
                    new Vector3(
                        width * 0.17f,
                        0.16f,
                        0.25f),
                    color * (0.6f +
                        pack * 0.04f));
            }
        }

        private static void AddType74SideBaskets(
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
            Color rail = color * 0.38f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int row = 0;
                    row < 2;
                    row++)
                {
                    TankDetailGeometry.Part(
                        "Japanese-Type74-SideBasketRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.405f,
                            roof - 0.34f +
                                row * 0.21f,
                            rear * 0.55f),
                        new Vector3(
                            0.026f,
                            0.026f,
                            Mathf.Abs(rear) * 0.9f),
                        rail);
                }
                for (int post = 0;
                    post < 4;
                    post++)
                {
                    TankDetailGeometry.Part(
                        "Japanese-Type74-SideBasketRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.405f,
                            roof - 0.235f,
                            -0.55f -
                                post * 0.38f),
                        new Vector3(0.026f, 0.24f, 0.026f),
                        rail);
                }
                TankDetailGeometry.Part(
                    "Painted-Japanese-Type74-BasketStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.365f,
                        roof - 0.22f,
                        rear * 0.55f),
                    new Vector3(
                        0.18f,
                        0.18f,
                        Mathf.Abs(rear) * 0.7f),
                    color * 0.62f);
            }
        }

        private static void AddType10SlatRack(
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
            Color rail = color * 0.38f;
            for (int row = 0;
                row < 2;
                row++)
            {
                TankDetailGeometry.Part(
                    "Japanese-Type10-BustleRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.42f +
                            row * 0.36f,
                        rear - 0.42f),
                    new Vector3(
                        width * 0.72f,
                        0.035f,
                        0.035f),
                    rail);
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int row = 0;
                    row < 2;
                    row++)
                {
                    TankDetailGeometry.Part(
                        "Japanese-Type10-BustleRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.35f,
                            roof - 0.42f +
                                row * 0.36f,
                            rear - 0.05f),
                        new Vector3(0.035f, 0.035f, 0.75f),
                        rail);
                }
            }
            for (int slat = 0;
                slat < 9;
                slat++)
            {
                TankDetailGeometry.Part(
                    "Japanese-Type10-BustleSlat",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.32f +
                            slat * width * 0.08f,
                        roof - 0.24f,
                        rear - 0.42f),
                    new Vector3(0.028f, 0.4f, 0.028f),
                    rail);
            }
            for (int pack = 0;
                pack < 4;
                pack++)
            {
                TankDetailGeometry.Part(
                    "Painted-Japanese-Type10-BustleStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.22f +
                            pack * width * 0.145f,
                        roof - 0.25f +
                            (pack % 2) * 0.055f,
                        rear - 0.12f),
                    new Vector3(
                        width * 0.18f,
                        0.18f,
                        0.35f),
                    color * (0.58f +
                        pack * 0.04f));
            }
        }
    }
}
