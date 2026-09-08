using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKoreanFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "k2" ||
                id == "k1a1" ||
                id == "k2b";
        }

        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            if (!Supports(definition?.id)) return;

            TankKoreanHullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            AddSmokeBanks(
                turret,
                definition,
                color,
                width);
            AddAntennas(
                turret,
                definition,
                color,
                width);
            AddBustleRack(
                turret,
                definition,
                color,
                width);
            TankKoreanVariantDetails.Build(
                turret,
                definition,
                color,
                width);
            TankKoreanProtectionDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
        }

        internal static void AddMachineGun(
            Transform parent,
            float roof,
            Color color,
            Vector3 seat,
            string prefix,
            bool heavy)
        {
            float receiverWidth = heavy ? 0.2f : 0.15f;
            float receiverLength = heavy ? 0.4f : 0.31f;
            float barrelLength = heavy ? 0.86f : 0.64f;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Mount",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.12f, 0.045f, 0.12f),
                color * 0.72f);
            TankDetailGeometry.Part(
                prefix + "-Receiver",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.13f,
                    seat.z + 0.08f),
                new Vector3(
                    receiverWidth,
                    heavy ? 0.14f : 0.1f,
                    receiverLength),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-AmmoBox",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x - receiverWidth * 0.75f,
                    roof + seat.y + 0.12f,
                    seat.z + 0.02f),
                new Vector3(
                    receiverWidth * 0.7f,
                    0.13f,
                    receiverLength * 0.56f),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.14f,
                    seat.z +
                        receiverLength * 0.5f +
                        barrelLength * 0.5f),
                new Vector3(
                    heavy ? 0.034f : 0.025f,
                    heavy ? 0.034f : 0.025f,
                    barrelLength),
                Gunmetal());
        }

        private static void AddSmokeBanks(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float front =
                TankDetailGeometry.TurretFrontZ(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.4f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Korean-SmokeBankShoe",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * halfWidth * 0.84f,
                        roof - 0.34f,
                        front * 0.38f),
                    new Vector3(0.25f, 0.06f, 0.46f),
                    color * 0.7f);
                for (int tube = 0;
                    tube < 6;
                    tube++)
                {
                    int row = tube / 3;
                    int column = tube % 3;
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Korean-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * halfWidth *
                                    (0.78f +
                                     column * 0.025f),
                                roof - 0.28f +
                                    row * 0.1f -
                                    column * 0.025f,
                                front * 0.4f +
                                    column * 0.09f),
                            new Vector3(0.05f, 0.15f, 0.05f),
                            color * 0.56f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            65f,
                            0f,
                            side * (32f +
                                column * 4f));
                }
            }
        }

        private static void AddAntennas(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            bool k1 = definition.id == "k1a1";
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Korean-AntennaBase",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * (k1 ? 0.3f : 0.22f),
                        roof + 0.035f,
                        rear * 0.72f),
                    new Vector3(0.05f, 0.07f, 0.05f),
                    color * 0.42f);
                Transform antenna =
                    TankDetailGeometry.Part(
                        "Korean-Antenna",
                        k1
                            ? PrimitiveType.Cube
                            : PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * width *
                                (k1 ? 0.29f : 0.22f),
                            roof +
                                (k1 ? 0.08f : 0.66f),
                            rear * 0.63f),
                        k1
                            ? new Vector3(
                                0.018f,
                                0.018f,
                                0.52f)
                            : new Vector3(
                                0.014f,
                                side < 0 ? 1.32f : 1.08f,
                                0.014f),
                        Gunmetal());
                if (k1)
                {
                    antenna.localRotation =
                        Quaternion.Euler(
                            3f,
                            side * 15f,
                            0f);
                }
                else
                {
                    antenna.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            side * -3f);
                }
            }
        }

        private static void AddBustleRack(
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
            if (definition.id == "k1a1")
            {
                for (int rail = -1;
                    rail <= 1;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "Korean-K1-RearRackRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            0f,
                            roof - 0.32f +
                                rail * 0.14f,
                            rear - 0.25f),
                        new Vector3(
                            width * 0.66f,
                            0.025f,
                            0.025f),
                        color * 0.4f);
                }
                for (int post = 0;
                    post < 4;
                    post++)
                {
                    TankDetailGeometry.Part(
                        "Korean-K1-RearRackRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            -width * 0.28f +
                                post * width * 0.19f,
                            roof - 0.32f,
                            rear - 0.25f),
                        new Vector3(
                            0.025f,
                            0.34f,
                            0.025f),
                        color * 0.4f);
                }
                return;
            }

            Color railColor = color * 0.4f;
            for (int row = 0;
                row < 3;
                row++)
            {
                TankDetailGeometry.Part(
                    "Korean-K2-BustleRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.48f +
                            row * 0.18f,
                        rear - 0.16f),
                    new Vector3(
                        width * 0.74f,
                        0.028f,
                        0.028f),
                    railColor);
            }
            for (int post = 0;
                post < 8;
                post++)
            {
                float fraction =
                    post / 7f;
                TankDetailGeometry.Part(
                    "Korean-K2-BustleRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.34f +
                            width * 0.68f *
                            fraction,
                        roof - 0.3f,
                        rear - 0.16f),
                    new Vector3(
                        0.028f,
                        0.4f,
                        0.028f),
                    railColor);
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int rail = 0;
                    rail < 3;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "Korean-K2-BustleRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.34f,
                            roof - 0.46f +
                                rail * 0.18f,
                            rear + 0.25f),
                        new Vector3(
                            0.028f,
                            0.028f,
                            0.52f),
                        railColor);
                }
                for (int pack = 0;
                    pack < 3;
                    pack++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Korean-K2-BustleStowage",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width *
                                (0.17f +
                                 pack * 0.045f),
                            roof - 0.32f +
                                (pack % 2) * 0.08f,
                            rear - 0.11f +
                                pack * 0.06f),
                        new Vector3(
                            width * 0.16f,
                            0.17f,
                            0.2f),
                        color * (0.62f +
                            pack * 0.04f));
                }
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }
    }
}
