using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMbt70HullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            float roof = TankDetailGeometry.HullRoofY(
                definition,
                height * 0.62f);
            AddSkirtlessFenders(
                root,
                color,
                width,
                length);
            AddRearClosures(
                root,
                color);
            AddDriverStation(
                root,
                color,
                roof);
            AddEngineDeck(
                root,
                color,
                roof,
                length);
            AddHullEra(
                root,
                color);
            AddBowEquipment(
                root,
                color,
                width,
                length);
        }

        private static void AddSkirtlessFenders(
            Transform root,
            Color color,
            float width,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform front =
                    TankDetailGeometry.Part(
                        "Painted-MBT70-FrontFender",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.439f,
                            1.37f,
                            length * 0.442f),
                        new Vector3(0.42f, 0.09f, 1.18f),
                        color * 0.76f);
                front.localRotation =
                    Quaternion.Euler(-3.15f, 0f, 0f);
                Transform rear =
                    TankDetailGeometry.Part(
                        "Painted-MBT70-RearFender",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.439f,
                            1.64f,
                            -length * 0.45f),
                        new Vector3(0.42f, 0.09f, 1.32f),
                        color * 0.72f);
                rear.localRotation =
                    Quaternion.Euler(2f, 0f, 0f);
                TankDetailGeometry.Part(
                    "MBT70-RubRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.482f,
                        1.39f,
                        0.1f),
                    new Vector3(0.06f, 0.08f, 5.46f),
                    TankMbt70FamilyDetails.Dark());
            }
        }

        private static void AddRearClosures(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform wall =
                    TankDetailGeometry.Part(
                        "Painted-MBT70-RearShoulderWall",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 0.96f,
                            1.41f,
                            -3.05f),
                        new Vector3(0.12f, 0.48f, 1.38f),
                        color * 0.67f);
                wall.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 1.45f,
                        0f);
                Transform shelf =
                    TankDetailGeometry.Part(
                        "Painted-MBT70-RearShoulderShelf",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.31f,
                            1.69f,
                            -3.05f),
                        new Vector3(0.48f, 0.1f, 1.34f),
                        color * 0.7f);
                shelf.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 1.45f,
                        0f);
                TankDetailGeometry.Part(
                    "MBT70-RearClosureWeb",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1f,
                        1.43f,
                        -3.05f),
                    new Vector3(0.035f, 0.34f, 1.18f),
                    TankMbt70FamilyDetails.Gunmetal());
                float[] stations =
                {
                    -3.48f,
                    -3.05f,
                    -2.62f
                };
                for (int station = 0;
                    station < stations.Length;
                    station++)
                {
                    TankDetailGeometry.Part(
                        "MBT70-RearClosureStrut",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.2f,
                            1.66f,
                            stations[station]),
                        new Vector3(0.11f, 0.035f, 0.035f),
                        TankMbt70FamilyDetails.Gunmetal());
                }
            }
        }

        private static void AddDriverStation(
            Transform root,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "Painted-MBT70-DriverHatch",
                PrimitiveType.Cube,
                root,
                new Vector3(-0.5f, roof + 0.045f, 1.12f),
                new Vector3(0.67f, 0.07f, 0.46f),
                color * 0.78f)
                .localRotation =
                Quaternion.Euler(-4f, 0f, 0f);
            for (int scope = -1;
                scope <= 1;
                scope++)
            {
                TankDetailGeometry.Part(
                    "Painted-MBT70-DriverPeriscopeHousing",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.5f + scope * 0.2f,
                        roof + 0.09f,
                        1.3f),
                    new Vector3(0.13f, 0.07f, 0.08f),
                    color * 0.58f);
                TankDetailGeometry.Part(
                    "MBT70-DriverPeriscopeLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.5f + scope * 0.2f,
                        roof + 0.09f,
                        1.345f),
                    new Vector3(0.09f, 0.035f, 0.014f),
                    TankMbt70FamilyDetails.Lens());
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color,
            float roof,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "MBT70-EngineGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.56f,
                        roof + 0.025f,
                        -length * 0.31f),
                    new Vector3(0.88f, 0.025f, 1.08f),
                    TankMbt70FamilyDetails.Dark());
                for (int louvre = 0;
                    louvre < 5;
                    louvre++)
                {
                    TankDetailGeometry.Part(
                        "Painted-MBT70-EngineLouvre",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 0.56f,
                            roof + 0.043f,
                            -length * 0.266f -
                                louvre * 0.16f),
                        new Vector3(0.84f, 0.014f, 0.035f),
                        color * 0.47f);
                }
            }
            TankDetailGeometry.Part(
                "MBT70-RearExhaust",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    roof - 0.08f,
                    -length * 0.468f),
                new Vector3(1.65f, 0.18f, 0.1f),
                TankMbt70FamilyDetails.Dark());
        }

        private static void AddHullEra(
            Transform root,
            Color color)
        {
            float[] frontXs =
            {
                -0.82f,
                -0.29f,
                0.29f,
                0.82f
            };
            for (int station = 0;
                station < frontXs.Length;
                station++)
            {
                AddEraCassette(
                    root,
                    color,
                    frontXs[station],
                    1.405f,
                    2.86f);
            }
            float[] rearXs =
            {
                -0.78f,
                -0.26f,
                0.26f,
                0.78f
            };
            for (int station = 0;
                station < rearXs.Length;
                station++)
            {
                AddEraCassette(
                    root,
                    color,
                    rearXs[station],
                    1.505f,
                    2.38f);
            }
        }

        private static void AddEraCassette(
            Transform root,
            Color color,
            float x,
            float y,
            float z)
        {
            Transform cassette =
                TankDetailGeometry.Part(
                    "Painted-MBT70-HullEraCassette",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(x, y, z),
                    new Vector3(0.44f, 0.09f, 0.34f),
                    color * 0.78f);
            cassette.localRotation =
                Quaternion.Euler(-2f, 0f, 0f);
            TankDetailGeometry.Part(
                "MBT70-HullEraSeam",
                PrimitiveType.Cube,
                root,
                new Vector3(x, y + 0.052f, z + 0.12f),
                new Vector3(0.34f, 0.018f, 0.025f),
                TankMbt70FamilyDetails.Gunmetal());
        }

        private static void AddBowEquipment(
            Transform root,
            Color color,
            float width,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-MBT70-HeadlightHousing",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.38f,
                        1.27f,
                        length * 0.475f),
                    new Vector3(0.22f, 0.18f, 0.16f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "MBT70-HeadlightLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.38f,
                        1.28f,
                        length * 0.487f),
                    new Vector3(0.12f, 0.09f, 0.014f),
                    TankMbt70FamilyDetails.Lens());
                TankDetailGeometry.Part(
                    "MBT70-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.38f,
                        1.34f,
                        length * 0.49f),
                    new Vector3(0.26f, 0.025f, 0.025f),
                    TankMbt70FamilyDetails.Gunmetal());
            }
        }
    }
}
