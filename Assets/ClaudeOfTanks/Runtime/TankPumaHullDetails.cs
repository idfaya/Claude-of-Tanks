using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPumaHullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            bool s1 =
                TankPumaFamilyDetails.IsS1(
                    definition);
            float scale = s1 ? 0.9f : 1f;
            AddNarrowHullCore(
                root,
                color,
                scale,
                s1);
            AddHighBow(
                root,
                color,
                scale);
            TankPumaSideProtectionDetails.Build(
                root,
                color,
                width,
                length,
                scale,
                s1);
            AddDriverAndPowerpack(
                root,
                color,
                scale);
            AddRearRamp(
                root,
                color,
                scale);
            AddHullSensors(
                root,
                color,
                width,
                scale,
                s1);
        }

        private static void AddNarrowHullCore(
            Transform root,
            Color color,
            float scale,
            bool s1)
        {
            TankDetailGeometry.Part(
                "Painted-Puma-LowerTub",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    (s1 ? 0.73f : 0.94f) *
                    scale,
                    (s1 ? 0f : -0.42f) *
                    scale),
                new Vector3(
                    (s1 ? 2.22f : 2f) *
                    scale,
                    (s1 ? 0.66f : 0.92f) *
                    scale,
                    (s1 ? 7.18f : 6.44f) *
                    scale),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Puma-UpperBody",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    (s1 ? 1.69f : 1.71f) *
                    scale,
                    (s1 ? -0.8f : -1.095f) *
                    scale),
                new Vector3(
                    3.32f * scale,
                    (s1 ? 0.7f : 0.62f) *
                    scale,
                    (s1 ? 5.7f : 5.01f) *
                    scale),
                color * 0.75f);
            TankDetailGeometry.Part(
                "Painted-Puma-DeckCrown",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    2.05f * scale,
                    -1.15f * scale),
                new Vector3(
                    2.12f * scale,
                    0.075f * scale,
                    4.9f * scale),
                color * 0.8f);
        }

        private static void AddHighBow(
            Transform root,
            Color color,
            float scale)
        {
            Transform glacis =
                TankDetailGeometry.Part(
                    "Painted-Puma-HighGlacis",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0f,
                        1.67f * scale,
                        2.66f * scale),
                    new Vector3(
                        2.7f * scale,
                        0.16f * scale,
                        2.12f * scale),
                    color * 0.82f);
            glacis.localRotation =
                Quaternion.Euler(
                    15f,
                    0f,
                    0f);
            TankDetailGeometry.Part(
                "Painted-Puma-BowFace",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    1.19f * scale,
                    3.66f * scale),
                new Vector3(
                    2.36f * scale,
                    0.44f * scale,
                    0.1f * scale),
                color * 0.7f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform shoulder =
                    TankDetailGeometry.Part(
                        "Painted-Puma-BowShoulder",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.43f * scale,
                            1.66f * scale,
                            2.43f * scale),
                        new Vector3(
                            0.52f * scale,
                            0.18f * scale,
                            1.72f * scale),
                        color * 0.77f);
                shoulder.localRotation =
                    Quaternion.Euler(
                        15f,
                        side * 5f,
                        0f);
                TankDetailGeometry.Part(
                    "Puma-BowTowHook",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.91f * scale,
                        1.14f * scale,
                        3.76f * scale),
                    new Vector3(
                        0.16f * scale,
                        0.14f * scale,
                        0.1f * scale),
                    TankPumaFamilyDetails.Gunmetal());
            }
        }

        private static void AddDriverAndPowerpack(
            Transform root,
            Color color,
            float scale)
        {
            TankDetailGeometry.Part(
                "Painted-Puma-DriverHatch",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    0.62f * scale,
                    2.1f * scale,
                    1.47f * scale),
                new Vector3(
                    0.3f * scale,
                    0.035f * scale,
                    0.3f * scale),
                color * 0.73f);
            for (int scope = 0;
                scope < 3;
                scope++)
            {
                float x =
                    (0.4f +
                     scope * 0.22f) * scale;
                TankDetailGeometry.Part(
                    "Painted-Puma-DriverPeriscope",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        x,
                        2.13f * scale,
                        1.78f * scale),
                    new Vector3(
                        0.16f * scale,
                        0.07f * scale,
                        0.1f * scale),
                    color * 0.58f);
                TankDetailGeometry.Part(
                    "Puma-DriverPeriscopeLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        x,
                        2.14f * scale,
                        1.835f * scale),
                    new Vector3(
                        0.1f * scale,
                        0.04f * scale,
                        0.012f * scale),
                    TankPumaFamilyDetails.Lens());
            }
            Transform intake =
                TankDetailGeometry.Part(
                    "Painted-Puma-PowerpackIntake",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.42f * scale,
                        1.8f * scale,
                        2.35f * scale),
                    new Vector3(
                        1f * scale,
                        0.04f * scale,
                        0.72f * scale),
                    color * 0.48f);
            intake.localRotation =
                Quaternion.Euler(
                    15f,
                    0f,
                    0f);
            for (int louvre = 0;
                louvre < 4;
                louvre++)
            {
                Transform strip =
                    TankDetailGeometry.Part(
                        "Puma-PowerpackLouvre",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            -0.42f * scale,
                            (1.73f +
                             louvre * 0.043f) *
                            scale,
                            (2.62f -
                             louvre * 0.16f) *
                            scale),
                        new Vector3(
                            0.92f * scale,
                            0.026f * scale,
                            0.06f * scale),
                        TankPumaFamilyDetails.Dark());
                strip.localRotation =
                    intake.localRotation;
            }
            for (int louvre = 0;
                louvre < 3;
                louvre++)
            {
                TankDetailGeometry.Part(
                    "Puma-FlankExhaustLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -1.43f * scale,
                        (1.6f +
                         louvre * 0.12f) *
                        scale,
                        1.95f * scale),
                    new Vector3(
                        0.04f * scale,
                        0.05f * scale,
                        0.56f * scale),
                    TankPumaFamilyDetails.Dark());
            }
        }

        private static void AddRearRamp(
            Transform root,
            Color color,
            float scale)
        {
            TankDetailGeometry.Part(
                "Painted-Puma-RearRamp",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    1.68f * scale,
                    -3.7f * scale),
                new Vector3(
                    2.84f * scale,
                    0.82f * scale,
                    0.1f * scale),
                color * 0.67f);
            TankDetailGeometry.Part(
                "Puma-RearRampDoor",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0.32f * scale,
                    1.66f * scale,
                    -3.758f * scale),
                new Vector3(
                    0.62f * scale,
                    0.7f * scale,
                    0.025f * scale),
                TankPumaFamilyDetails.Dark());
            for (int hinge = 0;
                hinge < 3;
                hinge++)
            {
                Transform pin =
                    TankDetailGeometry.Part(
                        "Puma-RearRampHinge",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            (-0.58f +
                             hinge * 0.58f) *
                            scale,
                            2.1f * scale,
                            -3.76f * scale),
                        new Vector3(
                            0.055f * scale,
                            0.095f * scale,
                            0.055f * scale),
                        TankPumaFamilyDetails.Gunmetal());
                pin.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f);
            }
        }

        private static void AddHullSensors(
            Transform root,
            Color color,
            float width,
            float scale,
            bool s1)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Puma-BowCameraPod",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.485f,
                        1.83f * scale,
                        2.77f * scale),
                    new Vector3(
                        0.12f * scale,
                        0.3f * scale,
                        0.36f * scale),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "Puma-BowCameraLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.5f,
                        1.85f * scale,
                        2.95f * scale),
                    new Vector3(
                        0.035f * scale,
                        0.13f * scale,
                        0.12f * scale),
                    TankPumaFamilyDetails.Lens());
                TankDetailGeometry.Part(
                    "Puma-Headlight",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(
                        side * 1.05f * scale,
                        1.5f * scale,
                        3.43f * scale),
                    new Vector3(
                        0.11f * scale,
                        0.09f * scale,
                        0.07f * scale),
                    new Color(0.72f, 0.74f, 0.55f));
                if (s1)
                {
                    TankDetailGeometry.Part(
                        "PumaS1-FlankCameraLens",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 2.06f * scale,
                            1.82f * scale,
                            -1.34f * scale),
                        new Vector3(
                            0.018f * scale,
                            0.1f * scale,
                            0.15f * scale),
                        TankPumaFamilyDetails.Lens());
                }
            }
        }
    }
}
