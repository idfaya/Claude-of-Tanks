using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankFrenchBasketDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            string id = definition.id;
            if (id == "amx30" ||
                id == "amx30b2")
            {
                AddRearRack(
                    turret,
                    definition,
                    color,
                    width,
                    "French-AMX30",
                    4);
                AddAmx30Stowage(
                    turret,
                    definition,
                    color,
                    width);
                return;
            }
            if (id == "amx40")
            {
                AddRearRack(
                    turret,
                    definition,
                    color,
                    width,
                    "French-AMX40",
                    3);
                return;
            }
            AddLeclercSideBaskets(
                turret,
                definition,
                color,
                width);
            AddLeclercDrum(
                turret,
                definition,
                color,
                width);
        }

        private static void AddRearRack(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            string prefix,
            int posts)
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
                    prefix + "-RearRackRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.42f +
                            row * 0.28f,
                        rear - 0.24f),
                    new Vector3(
                        width * 0.45f,
                        0.03f,
                        0.03f),
                    rail);
            }
            for (int post = 0;
                post < posts;
                post++)
            {
                TankDetailGeometry.Part(
                    prefix + "-RearRackRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.2f +
                            post *
                            (width * 0.4f /
                             Mathf.Max(1, posts - 1)),
                        roof - 0.28f,
                        rear - 0.24f),
                    new Vector3(0.03f, 0.3f, 0.03f),
                    rail);
            }
            for (int pack = 0;
                pack < 2;
                pack++)
            {
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-RackStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        (pack == 0 ? -1f : 1f) *
                            width * 0.11f,
                        roof - 0.26f,
                        rear - 0.08f),
                    new Vector3(
                        width * 0.2f,
                        0.18f,
                        0.27f),
                    color * (0.58f +
                        pack * 0.08f));
            }
        }

        private static void AddAmx30Stowage(
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
            TankDetailGeometry.Part(
                "Painted-French-AMX30-DoubleLidBin",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.22f,
                    roof - 0.12f,
                    rear * 0.58f),
                new Vector3(0.38f, 0.18f, 0.55f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "French-AMX30-DoubleLidSeam",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.22f,
                    roof - 0.015f,
                    rear * 0.58f),
                new Vector3(0.39f, 0.018f, 0.045f),
                Gunmetal());
            Transform roll =
                TankDetailGeometry.Part(
                    "Painted-French-AMX30-NetRoll",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.13f,
                        rear - 0.05f),
                    new Vector3(0.11f, width * 0.38f, 0.11f),
                    color * 0.56f);
            roll.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
        }

        private static void AddLeclercSideBaskets(
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
                        "French-Leclerc-SideBasketRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.425f,
                            roof - 0.38f +
                                row * 0.28f,
                            rear * 0.55f),
                        new Vector3(
                            0.03f,
                            0.03f,
                            Mathf.Abs(rear) * 0.86f),
                        rail);
                }
                for (int post = 0;
                    post < 5;
                    post++)
                {
                    TankDetailGeometry.Part(
                        "French-Leclerc-SideBasketRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.425f,
                            roof - 0.24f,
                            -0.35f -
                                post * 0.34f),
                        new Vector3(0.03f, 0.3f, 0.03f),
                        rail);
                }
                TankDetailGeometry.Part(
                    "Painted-French-Leclerc-BasketStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.39f,
                        roof - 0.23f,
                        rear * 0.55f),
                    new Vector3(
                        0.2f,
                        0.2f,
                        Mathf.Abs(rear) * 0.66f),
                    color * 0.62f);
            }
            for (int link = 0;
                link < 4;
                link++)
            {
                TankDetailGeometry.Part(
                    "French-Leclerc-SpareTrackLink",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.425f,
                        roof - 0.22f,
                        0.4f - link * 0.26f),
                    new Vector3(0.045f, 0.2f, 0.2f),
                    Gunmetal());
            }
        }

        private static void AddLeclercDrum(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            Transform drum =
                TankDetailGeometry.Part(
                    "Painted-French-Leclerc-StowageDrum",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        width * 0.34f,
                        roof - 0.22f,
                        -0.68f),
                    new Vector3(0.2f, 0.72f, 0.2f),
                    color * 0.65f);
            drum.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            for (int strap = 0;
                strap < 3;
                strap++)
            {
                Transform ring =
                    TankDetailGeometry.Part(
                        "French-Leclerc-DrumRing",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            width * 0.34f,
                            roof - 0.22f,
                            -0.44f -
                                strap * 0.24f),
                        new Vector3(0.21f, 0.025f, 0.21f),
                        Gunmetal());
                ring.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }
    }
}
