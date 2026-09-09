using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT62Obr1975HullDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            TankT62Obr1975HullCoreDetails.Build(
                root,
                color);
            AddFenderCourse(root, color);
            AddDriverAndBowDetails(root, color);
            AddEngineDeck(root, color);
            AddRearServiceField(root, color);
            TankT62Obr1975RunningGearDetails.Build(
                root,
                color);
        }

        private static void AddFenderCourse(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T62-FenderShelf",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.65f,
                        1.482f,
                        -0.008f),
                    new Vector3(
                        0.286f,
                        0.03f,
                        4.46f),
                    color * 0.72f);
                Part(
                    "T62-FenderOuterRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.775f,
                        1.5f,
                        -0.008f),
                    new Vector3(
                        0.024f,
                        0.025f,
                        4.48f),
                    TankT62Obr1975FamilyDetails.Dark());
                for (int bin = 0;
                    bin < 9;
                    bin++)
                {
                    float z =
                        -1.908f + bin * 0.4816f;
                    Part(
                        "Painted-T62-FenderBin",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.773f,
                            1.338f,
                            z),
                        new Vector3(
                            0.061f,
                            0.29f,
                            0.445f),
                        color * 0.68f);
                    Part(
                        "T62-FenderBinSeam",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.778f,
                            1.333f,
                            z + 0.232f),
                        new Vector3(
                            0.018f,
                            0.25f,
                            0.02f),
                        TankT62Obr1975FamilyDetails.Dark());
                }
                Part(
                    "Painted-T62-NoseFender",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.5f,
                        1.34f,
                        3.11f),
                    new Vector3(
                        0.64f,
                        0.05f,
                        0.34f),
                    color * 0.72f);
                Part(
                    "T62-FrontMudFlap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.76f,
                        1.04f,
                        2.73f),
                    new Vector3(
                        0.028f,
                        0.43f,
                        0.53f),
                    TankT62Obr1975FamilyDetails.Rubber());
                Part(
                    "T62-RearMudFlap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.57f,
                        1.17f,
                        -2.96f),
                    new Vector3(
                        0.38f,
                        0.38f,
                        0.035f),
                    TankT62Obr1975FamilyDetails.Rubber());
            }
        }

        private static void AddDriverAndBowDetails(
            Transform root,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-T62-DriverHatch",
                root,
                new Vector3(
                    -0.605f,
                    1.445f,
                    2.13f),
                0.24f,
                0.05f,
                color * 0.75f);
            for (int scope = -1;
                scope <= 1;
                scope += 2)
            {
                Part(
                    "Painted-T62-DriverPeriscopeHousing",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.605f +
                            scope * 0.16f,
                        1.46f,
                        2.42f),
                    new Vector3(
                        0.13f,
                        0.065f,
                        0.09f),
                    color * 0.55f);
                Part(
                    "T62-DriverPeriscopeLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.605f +
                            scope * 0.16f,
                        1.468f,
                        2.471f),
                    new Vector3(
                        0.09f,
                        0.035f,
                        0.014f),
                    TankT62Obr1975FamilyDetails.Glass());
            }
            Transform splash = Part(
                "Painted-T62-SplashBoard",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.43f, 1.806f),
                new Vector3(2.75f, 0.035f, 0.37f),
                color * 0.71f);
            splash.localRotation =
                Quaternion.Euler(-9f, 0f, 0f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T62-HeadlightHousing",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.506f,
                        1.12f,
                        2.92f),
                    new Vector3(
                        0.19f,
                        0.17f,
                        0.17f),
                    color * 0.58f)
                    .localRotation =
                    Quaternion.Euler(-16f, 0f, 0f);
                Part(
                    "T62-HeadlightLens",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(
                        side * 0.506f,
                        1.13f,
                        3.015f),
                    new Vector3(
                        0.115f,
                        0.105f,
                        0.055f),
                    TankT62Obr1975FamilyDetails.Glass());
                Part(
                    "T62-TowEye",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * 1.034f,
                        0.66f,
                        3.3f),
                    new Vector3(
                        0.072f,
                        0.035f,
                        0.072f),
                    TankT62Obr1975FamilyDetails.Dark())
                    .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            for (int rib = 0;
                rib < 4;
                rib++)
            {
                Part(
                    "Painted-T62-BowStiffener",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.792f + rib * 0.528f,
                        0.66f,
                        3.555f),
                    new Vector3(
                        0.13f,
                        0.15f,
                        0.035f),
                    color * 0.56f);
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "T62-EngineLouvreBed",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.682f,
                        1.532f,
                        -1.82f),
                    new Vector3(
                        1.1f,
                        0.025f,
                        0.78f),
                    TankT62Obr1975FamilyDetails.Dark());
                for (int louvre = 0;
                    louvre < 5;
                    louvre++)
                {
                    Part(
                        "Painted-T62-EngineLouvre",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 0.682f,
                            1.55f,
                            -2.1f +
                                louvre * 0.14f),
                        new Vector3(
                            1.023f,
                            0.022f,
                            0.045f),
                        color * 0.48f);
                }
                Part(
                    "Painted-T62-DeckStowage",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.364f,
                        1.453f,
                        side > 0
                            ? 0.46f
                            : 1.3f),
                    new Vector3(
                        0.33f,
                        0.09f,
                        1.3f),
                    color * 0.69f);
            }
            for (int link = 0;
                link < 6;
                link++)
            {
                Part(
                    "T62-SpareTrackLink",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.86f +
                            link * 0.22f,
                        1.515f,
                        0.62f),
                    new Vector3(
                        0.18f,
                        0.055f,
                        0.21f),
                    TankT62Obr1975FamilyDetails.Dark());
            }
        }

        private static void AddRearServiceField(
            Transform root,
            Color color)
        {
            Part(
                "T62-RearServicePanel",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.82f, -3.192f),
                new Vector3(1.958f, 0.34f, 0.035f),
                TankT62Obr1975FamilyDetails.Dark());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform drum = Part(
                    "Painted-T62-RearFuelDrum",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * 0.626f,
                        1.68f,
                        -3.24f),
                    new Vector3(
                        0.275f,
                        0.54f,
                        0.275f),
                    color * 0.65f);
                drum.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
                for (int rim = -1;
                    rim <= 1;
                    rim += 2)
                {
                    Transform cap = Part(
                        "T62-FuelDrumRim",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * 0.626f +
                                rim * 0.52f,
                            1.68f,
                            -3.24f),
                        new Vector3(
                            0.282f,
                            0.012f,
                            0.282f),
                        TankT62Obr1975FamilyDetails.Dark());
                    cap.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                }
                Part(
                    "Painted-T62-DrumBracket",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.682f,
                        1.225f,
                        -3.12f),
                    new Vector3(
                        0.132f,
                        0.45f,
                        0.44f),
                    color * 0.55f);
                Part(
                    "T62-TailLamp",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * 1.232f,
                        1.08f,
                        -3.225f),
                    new Vector3(
                        0.08f,
                        0.03f,
                        0.08f),
                    new Color(0.35f, 0.035f, 0.025f))
                    .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            Transform log = Part(
                "T62-UnditchingLog",
                PrimitiveType.Cylinder,
                root,
                new Vector3(0f, 1.28f, -3.1f),
                new Vector3(0.08f, 0.88f, 0.08f),
                TankT62Obr1975FamilyDetails.Wood());
            log.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
            for (int strap = -1;
                strap <= 1;
                strap += 2)
            {
                Transform band = Part(
                    "T62-LogStrap",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        strap * 0.48f,
                        1.28f,
                        -3.1f),
                    new Vector3(
                        0.085f,
                        0.025f,
                        0.085f),
                    TankT62Obr1975FamilyDetails.Dark());
                band.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
            }
        }

        private static void AddVerticalCylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float height,
            Color color)
        {
            Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                position,
                new Vector3(
                    radius,
                    height * 0.5f,
                    radius),
                color);
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                type,
                parent,
                position,
                scale,
                color);
        }
    }
}
