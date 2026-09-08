using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBradleyHullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            AddContinuousHull(
                root,
                color);
            AddTwoSlopeBow(
                root,
                color);
            AddDriverStation(
                root,
                color);
            AddDeckFurniture(
                root,
                color);
            AddTroopRamp(
                root,
                color);
            AddBowEquipment(
                root,
                color,
                width);
        }

        private static void AddContinuousHull(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bradley-NarrowTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.75f, -0.3f),
                new Vector3(1.72f, 0.6f, 5.35f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Bradley-UpperSpine",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.75f, -0.79f),
                new Vector3(2.1f, 0.32f, 4.82f),
                color * 0.76f);
            TankDetailGeometry.Part(
                "Painted-Bradley-RoofPlate",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.875f, -0.79f),
                new Vector3(2.04f, 0.06f, 4.82f),
                color * 0.82f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform camber =
                    TankDetailGeometry.Part(
                        "Painted-Bradley-RoofCamber",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.21f,
                            1.76f,
                            -0.72f),
                        new Vector3(
                            0.48f,
                            0.2f,
                            4.72f),
                        color * 0.72f);
                camber.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 18f);
                TankDetailGeometry.Part(
                    "Painted-Bradley-FlareClosure",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.28f,
                        1.38f,
                        -0.34f),
                    new Vector3(
                        0.56f,
                        0.28f,
                        5.7f),
                    color * 0.67f);
            }
            TankDetailGeometry.Part(
                "Painted-Bradley-BellyPan",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.48f, 2.66f),
                new Vector3(1.89f, 0.08f, 0.6f),
                color * 0.55f);
        }

        private static void AddTwoSlopeBow(
            Transform root,
            Color color)
        {
            Transform upper =
                TankDetailGeometry.Part(
                    "Painted-Bradley-UpperGlacis",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.71f, 2.03f),
                    new Vector3(2.55f, 0.12f, 1.42f),
                    color * 0.8f);
            upper.localRotation =
                Quaternion.Euler(
                    26.565f,
                    0f,
                    0f);
            Transform lower =
                TankDetailGeometry.Part(
                    "Painted-Bradley-LowerGlacis",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.4f, 2.66f),
                    new Vector3(2.65f, 0.16f, 0.78f),
                    color * 0.7f);
            lower.localRotation =
                Quaternion.Euler(
                    22f,
                    0f,
                    0f);
            TankDetailGeometry.Part(
                "Painted-Bradley-NoseShelf",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.3f, 3.05f),
                new Vector3(2.6f, 0.12f, 0.24f),
                color * 0.64f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform corner =
                    TankDetailGeometry.Part(
                        "Painted-Bradley-BowCornerClosure",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.23f,
                            0.88f,
                            2.92f),
                        new Vector3(0.38f, 0.62f, 0.38f),
                        color * 0.61f);
                corner.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 12f,
                        0f);
            }
        }

        private static void AddDriverStation(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bradley-DriverHatch",
                PrimitiveType.Cube,
                root,
                new Vector3(-0.85f, 1.5125f, 2.43f),
                new Vector3(0.62f, 0.075f, 0.3f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Bradley-DriverHatchSeam",
                PrimitiveType.Cube,
                root,
                new Vector3(-0.85f, 1.555f, 2.42f),
                new Vector3(0.56f, 0.018f, 0.24f),
                TankBradleyFamilyDetails.Dark());
            for (int scope = 0;
                scope < 3;
                scope++)
            {
                float x =
                    -1.05f + scope * 0.24f;
                TankDetailGeometry.Part(
                    "Painted-Bradley-DriverPeriscope",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(x, 1.6f, 2.28f),
                    new Vector3(0.16f, 0.07f, 0.1f),
                    color * 0.57f);
                TankDetailGeometry.Part(
                    "Bradley-DriverPeriscopeLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(x, 1.61f, 2.337f),
                    new Vector3(0.1f, 0.04f, 0.014f),
                    TankBradleyFamilyDetails.Lens());
            }
            Transform cutter =
                TankDetailGeometry.Part(
                    "Bradley-WireCutter",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(-0.85f, 1.26f, 3.02f),
                    new Vector3(0.045f, 0.38f, 0.045f),
                    TankBradleyFamilyDetails.Gunmetal());
            cutter.localRotation =
                Quaternion.Euler(-65.9f, 0f, 0f);
            Transform vane =
                TankDetailGeometry.Part(
                    "Painted-Bradley-TrimVane",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.475f, 2.72f),
                    new Vector3(2.3f, 0.045f, 0.3f),
                    color * 0.66f);
            vane.localRotation =
                Quaternion.Euler(-6.9f, 0f, 0f);
        }

        private static void AddDeckFurniture(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bradley-EngineDeckRaise",
                PrimitiveType.Cube,
                root,
                new Vector3(0.34f, 1.94f, 1.09f),
                new Vector3(1.58f, 0.075f, 0.87f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Painted-Bradley-EngineGrille",
                PrimitiveType.Cube,
                root,
                new Vector3(0.2f, 1.985f, 1.05f),
                new Vector3(1.18f, 0.02f, 0.82f),
                color * 0.45f);
            for (int louvre = 0;
                louvre < 4;
                louvre++)
            {
                TankDetailGeometry.Part(
                    "Bradley-EngineLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0.2f,
                        1.998f,
                        1.36f -
                            louvre * 0.21f),
                    new Vector3(1.1f, 0.024f, 0.06f),
                    TankBradleyFamilyDetails.Dark());
            }
            TankDetailGeometry.Part(
                "Painted-Bradley-CargoHatchHump",
                PrimitiveType.Cube,
                root,
                new Vector3(0.25f, 1.985f, -2.37f),
                new Vector3(1.1f, 0.155f, 0.58f),
                color * 0.73f);
            TankDetailGeometry.Part(
                "Bradley-CargoHatchSeam",
                PrimitiveType.Cube,
                root,
                new Vector3(0.25f, 2.066f, -2.37f),
                new Vector3(1.04f, 0.014f, 0.5f),
                TankBradleyFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Bradley-TroopHatchSeam",
                PrimitiveType.Cube,
                root,
                new Vector3(0.2f, 1.912f, -1.55f),
                new Vector3(0.72f, 0.015f, 1.28f),
                TankBradleyFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Painted-Bradley-IntakeVent",
                PrimitiveType.Cube,
                root,
                new Vector3(-0.85f, 1.865f, -1.3f),
                new Vector3(0.3f, 0.06f, 0.4f),
                color * 0.5f);
        }

        private static void AddTroopRamp(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bradley-RearRamp",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.54f, -3.1f),
                new Vector3(1.3f, 0.72f, 0.1f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Bradley-RearRampDoor",
                PrimitiveType.Cube,
                root,
                new Vector3(0.42f, 1.5625f, -3.158f),
                new Vector3(0.66f, 0.675f, 0.03f),
                TankBradleyFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Bradley-RearRampHinge",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.86f, -3.16f),
                new Vector3(2.58f, 0.06f, 0.06f),
                TankBradleyFamilyDetails.Gunmetal());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bradley-RearCornerPost",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.1f,
                        1.6f,
                        -3.22f),
                    new Vector3(0.42f, 0.59f, 0.13f),
                    color * 0.61f);
                TankDetailGeometry.Part(
                    "Bradley-RearBumperette",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.2f,
                        1.31f,
                        -3.18f),
                    new Vector3(0.52f, 0.18f, 0.24f),
                    TankBradleyFamilyDetails.Dark());
            }
        }

        private static void AddBowEquipment(
            Transform root,
            Color color,
            float width)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bradley-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.16f,
                        1.48f,
                        2.86f),
                    new Vector3(0.28f, 0.22f, 0.2f),
                    color * 0.58f);
                TankDetailGeometry.Part(
                    "Bradley-Headlight",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(
                        side * 1.16f,
                        1.5f,
                        2.97f),
                    new Vector3(0.11f, 0.09f, 0.07f),
                    new Color(0.75f, 0.76f, 0.58f));
                TankDetailGeometry.Part(
                    "Bradley-BowTowShackle",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.3f,
                        0.96f,
                        3.17f),
                    new Vector3(0.14f, 0.12f, 0.08f),
                    TankBradleyFamilyDetails.Gunmetal());
            }
        }
    }
}
