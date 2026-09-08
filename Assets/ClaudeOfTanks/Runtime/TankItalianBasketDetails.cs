using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankItalianBasketDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            if (definition.id == "carro45t")
            {
                AddCarroRack(
                    turret,
                    definition,
                    color,
                    width);
                return;
            }
            AddArieteRearBasket(
                turret,
                definition,
                color,
                width);
            if (definition.id == "ariete_c1" ||
                definition.id == "ariete_c2")
            {
                AddArieteSideRacks(
                    turret,
                    definition,
                    color,
                    width);
            }
        }

        private static void AddCarroRack(
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
                    "Italian-Carro45T-RearRackRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.38f +
                            row * 0.24f,
                        rear - 0.24f),
                    new Vector3(
                        width * 0.5f,
                        0.03f,
                        0.03f),
                    rail);
            }
            for (int post = 0;
                post < 5;
                post++)
            {
                TankDetailGeometry.Part(
                    "Italian-Carro45T-RearRackRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.22f +
                            post * width * 0.11f,
                        roof - 0.26f,
                        rear - 0.24f),
                    new Vector3(0.03f, 0.27f, 0.03f),
                    rail);
            }
            TankDetailGeometry.Part(
                "Italian-Carro45T-RearRackRail",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    roof - 0.4f,
                    rear - 0.02f),
                new Vector3(
                    width * 0.48f,
                    0.03f,
                    0.45f),
                rail);
            for (int pack = 0;
                pack < 2;
                pack++)
            {
                TankDetailGeometry.Part(
                    "Painted-Italian-Carro45T-RackStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        (pack == 0 ? -1f : 1f) *
                            width * 0.12f,
                        roof - 0.25f,
                        rear - 0.08f),
                    new Vector3(
                        width * 0.22f,
                        0.18f,
                        0.3f),
                    color * (0.58f +
                        pack * 0.08f));
            }
        }

        private static void AddArieteRearBasket(
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
                row < 3;
                row++)
            {
                TankDetailGeometry.Part(
                    "Italian-Ariete-RearBasketRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.45f +
                            row * 0.18f,
                        rear - 0.4f),
                    new Vector3(
                        width * 0.64f,
                        0.03f,
                        0.03f),
                    rail);
            }
            for (int post = 0;
                post < 7;
                post++)
            {
                TankDetailGeometry.Part(
                    "Italian-Ariete-RearBasketRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.29f +
                            post * width * 0.097f,
                        roof - 0.27f,
                        rear - 0.4f),
                    new Vector3(0.03f, 0.39f, 0.03f),
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
                        "Italian-Ariete-RearBasketRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.3f,
                            roof - 0.45f +
                                row * 0.36f,
                            rear - 0.18f),
                        new Vector3(0.03f, 0.03f, 0.45f),
                        rail);
                }
            }
            for (int pack = 0;
                pack < 4;
                pack++)
            {
                TankDetailGeometry.Part(
                    "Painted-Italian-Ariete-BasketStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.22f +
                            pack * width * 0.145f,
                        roof - 0.29f +
                            (pack % 2) * 0.05f,
                        rear - 0.14f),
                    new Vector3(
                        width * 0.18f,
                        0.18f,
                        0.3f),
                    color * (0.58f +
                        pack * 0.04f));
            }
        }

        private static void AddArieteSideRacks(
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
                        "Italian-Ariete-SideRackRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.385f,
                            roof - 0.4f +
                                row * 0.24f,
                            rear * 0.48f),
                        new Vector3(
                            0.03f,
                            0.03f,
                            Mathf.Abs(rear) * 1.12f),
                        rail);
                }
                for (int arm = 0;
                    arm < 3;
                    arm++)
                {
                    TankDetailGeometry.Part(
                        "Italian-Ariete-SideRackArm",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.34f,
                            roof - 0.27f,
                            0.2f -
                                arm * 0.75f),
                        new Vector3(
                            width * 0.12f,
                            0.04f,
                            0.04f),
                        rail);
                }
                TankDetailGeometry.Part(
                    "Painted-Italian-Ariete-SidePanel",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.31f,
                        roof - 0.32f,
                        rear * 0.62f),
                    new Vector3(
                        width * 0.06f,
                        0.43f,
                        Mathf.Abs(rear) * 0.72f),
                    color * 0.68f);
                TankDetailGeometry.Part(
                    "Italian-Ariete-BasketBridge",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.3f,
                        roof - 0.11f,
                        rear * 0.74f),
                    new Vector3(
                        width * 0.12f,
                        0.12f,
                        0.06f),
                    color * 0.55f);
            }
        }
    }
}
