using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankUpiorHullDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddFacetedBody(root, color);
            AddBow(root, color);
            AddStern(root, color);
            AddDeck(root, color);
            AddFlanks(root, color);
            AddReturnRollers(root);
        }

        private static void AddFacetedBody(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Upior-CenterTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.55f, -0.05f),
                new Vector3(1.44f, 0.54f, 4.1f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "Painted-Upior-CenterSponson",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.12f, -0.25f),
                new Vector3(1.48f, 0.62f, 3.85f),
                color * 0.68f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Upior-OutboardSponson",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.02f, 1.18f, -0.1f),
                    new Vector3(0.56f, 0.5f, 3.42f),
                    color * 0.72f);
                Transform roof =
                    TankDetailGeometry.Part(
                        "Painted-Upior-CrownFacet",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 0.64f,
                            1.51f,
                            -0.32f),
                        new Vector3(1.34f, 0.08f, 3.9f),
                        color * 0.78f);
                roof.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 1.5f,
                        side * 7.5f);
            }
        }

        private static void AddBow(
            Transform root,
            Color color)
        {
            Transform glacis =
                TankDetailGeometry.Part(
                    "Painted-Upior-RakedGlacis",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.15f, 2.1f),
                    new Vector3(2.48f, 0.11f, 1.2f),
                    color * 0.76f);
            glacis.localRotation =
                Quaternion.Euler(-31.5f, 0f, 0f);
            Transform lower =
                TankDetailGeometry.Part(
                    "Painted-Upior-LowerBow",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 0.51f, 2.2f),
                    new Vector3(1.42f, 0.12f, 1.05f),
                    color * 0.55f);
            lower.localRotation =
                Quaternion.Euler(43f, 0f, 0f);
            TankDetailGeometry.Part(
                "Painted-Upior-NoseBeam",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.82f, 2.475f),
                new Vector3(1.4f, 0.2f, 0.175f),
                color * 0.64f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform cheek =
                    TankDetailGeometry.Part(
                        "Painted-Upior-BowCheek",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.31f, 0.94f, 2.08f),
                        new Vector3(0.38f, 0.98f, 0.58f),
                        color * 0.7f);
                cheek.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 28f,
                        0f);
                TankDetailGeometry.Part(
                    "Upior-TowShacklePlate",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.62f, 0.82f, 2.57f),
                    new Vector3(0.1f, 0.16f, 0.05f),
                    TankUpiorFamilyDetails.Dark());
                Transform shackle =
                    TankDetailGeometry.Part(
                        "Upior-TowShackle",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(side * 0.62f, 0.8f, 2.605f),
                        new Vector3(0.055f, 0.018f, 0.055f),
                        TankUpiorFamilyDetails.Gunmetal());
                shackle.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                Transform pod =
                    TankDetailGeometry.Part(
                        "Painted-Upior-HeadlightPod",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 0.98f, 1.345f, 1.845f),
                        new Vector3(0.16f, 0.06f, 0.05f),
                        color * 0.62f);
                pod.localRotation =
                    Quaternion.Euler(-56f, 0f, 0f);
                TankDetailGeometry.Part(
                    "Upior-Headlight",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.98f, 1.375f, 1.875f),
                    new Vector3(0.12f, 0.035f, 0.02f),
                    TankUpiorFamilyDetails.Lens());
            }
            for (int row = 0;
                row < 4;
                row++)
            {
                Transform rib =
                    TankDetailGeometry.Part(
                        "Upior-GlacisRivetStrip",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            0f,
                            1.01f + row * 0.12f,
                            2.355f - row * 0.18f),
                        new Vector3(1.66f, 0.022f, 0.022f),
                        TankUpiorFamilyDetails.Gunmetal());
                rib.localRotation =
                    Quaternion.Euler(-56f, 0f, 0f);
            }
        }

        private static void AddStern(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Upior-SternPlate",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.205f, -2.49f),
                new Vector3(2f, 0.61f, 0.12f),
                color * 0.63f);
            TankDetailGeometry.Part(
                "Painted-Upior-LowerStern",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.675f, -2.49f),
                new Vector3(1.44f, 0.45f, 0.12f),
                color * 0.55f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform corner =
                    TankDetailGeometry.Part(
                        "Painted-Upior-SternCheek",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.31f, 0.96f, -2.1f),
                        new Vector3(0.38f, 1.02f, 0.58f),
                        color * 0.68f);
                corner.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * -28f,
                        0f);
                TankDetailGeometry.Part(
                    "Painted-Upior-RearDoor",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.44f, 1.06f, -2.565f),
                    new Vector3(0.4f, 0.66f, 0.035f),
                    color * 0.57f);
                TankDetailGeometry.Part(
                    "Upior-RearDoorHinge",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.6f, 1.02f, -2.59f),
                    new Vector3(0.05f, 0.09f, 0.045f),
                    TankUpiorFamilyDetails.Gunmetal());
                TankDetailGeometry.Part(
                    "Upior-Taillight",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.66f, 0.56f, -2.59f),
                    new Vector3(0.12f, 0.07f, 0.04f),
                    new Color(0.34f, 0.03f, 0.025f));
                Transform jet =
                    TankDetailGeometry.Part(
                        "Painted-Upior-Waterjet",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(side * 0.5f, 0.3f, -2.52f),
                        new Vector3(0.09f, 0.13f, 0.09f),
                        color * 0.5f);
                jet.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
                Transform bore =
                    TankDetailGeometry.Part(
                        "Upior-WaterjetBore",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(side * 0.5f, 0.3f, -2.6f),
                        new Vector3(0.06f, 0.025f, 0.06f),
                        TankUpiorFamilyDetails.Dark());
                bore.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            TankDetailGeometry.Part(
                "Upior-RearDoorJamb",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.06f, -2.59f),
                new Vector3(0.045f, 0.6f, 0.03f),
                TankUpiorFamilyDetails.Gunmetal());
            for (int segment = 0;
                segment < 5;
                segment++)
            {
                TankDetailGeometry.Part(
                    "Upior-TowCable",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        -0.48f + segment * 0.24f,
                        1.05f -
                            Mathf.Abs(segment - 2) * 0.05f,
                        -2.62f),
                    new Vector3(0.022f, 0.13f, 0.022f),
                    TankUpiorFamilyDetails.Gunmetal());
            }
        }

        private static void AddDeck(
            Transform root,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-Upior-DriverHatch",
                root,
                0.22f,
                0.028f,
                -0.55f,
                1.6f,
                1.05f,
                color * 0.68f);
            for (int scope = 0;
                scope < 2;
                scope++)
            {
                TankDetailGeometry.Part(
                    "Upior-DriverPeriscope",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.66f + scope * 0.22f,
                        1.63f,
                        1.32f),
                    new Vector3(0.14f, 0.05f, 0.035f),
                    TankUpiorFamilyDetails.Lens());
            }
            TankDetailGeometry.Part(
                "Painted-Upior-CodriverSight",
                PrimitiveType.Cube,
                root,
                new Vector3(0.1f, 1.6f, 1.28f),
                new Vector3(0.42f, 0.045f, 0.4f),
                color * 0.64f);
            TankDetailGeometry.Part(
                "Painted-Upior-EngineRiser",
                PrimitiveType.Cube,
                root,
                new Vector3(0.62f, 1.6f, 0.9f),
                new Vector3(0.62f, 0.055f, 0.6f),
                color * 0.61f);
            TankDetailGeometry.Part(
                "Upior-EngineGrille",
                PrimitiveType.Cube,
                root,
                new Vector3(0.62f, 1.632f, 0.9f),
                new Vector3(0.54f, 0.015f, 0.52f),
                TankUpiorFamilyDetails.Dark());
            for (int louvre = 0;
                louvre < 3;
                louvre++)
            {
                TankDetailGeometry.Part(
                    "Upior-EngineLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0.62f,
                        1.65f,
                        0.75f + louvre * 0.15f),
                    new Vector3(0.48f, 0.022f, 0.05f),
                    TankUpiorFamilyDetails.Gunmetal());
            }
            TankDetailGeometry.Part(
                "Painted-Upior-RearDeckRiser",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.575f, -1.98f),
                new Vector3(1.35f, 0.05f, 0.72f),
                color * 0.65f);
        }

        private static void AddFlanks(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 13;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Upior-SkirtPanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.4625f,
                            1.14f,
                            2.02f -
                                panel * 0.355f),
                        new Vector3(0.075f, 0.54f, 0.34f),
                        color * (0.66f -
                            panel % 2 * 0.02f));
                }
                for (int plate = 0;
                    plate < 10;
                    plate++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Upior-SponsonClosure",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.3575f,
                            1.44f,
                            -2.07f + plate * 0.46f),
                        new Vector3(0.115f, 0.03f, 0.46f),
                        color * 0.7f);
                }
                TankDetailGeometry.Part(
                    "Upior-SkirtRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.36f, 1.475f, -0.2f),
                    new Vector3(0.05f, 0.05f, 3.9f),
                    TankUpiorFamilyDetails.Gunmetal());
                TankDetailGeometry.Part(
                    "Upior-MudFlap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.38f, 0.76f, 2.24f),
                    new Vector3(0.07f, 0.22f, 0.36f),
                    TankUpiorFamilyDetails.Dark());
                TankDetailGeometry.Part(
                    "Upior-MudFlap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.38f, 0.74f, -2.24f),
                    new Vector3(0.07f, 0.24f, 0.28f),
                    TankUpiorFamilyDetails.Dark());
            }
        }

        private static void AddReturnRollers(
            Transform root)
        {
            float[] stations = { 1.28f, 0f, -1.3f };
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int index = 0;
                    index < stations.Length;
                    index++)
                {
                    Transform roller =
                        TankDetailGeometry.Part(
                            "Upior-ReturnRoller",
                            PrimitiveType.Cylinder,
                            root,
                            new Vector3(
                                side * 0.94f,
                                0.72f,
                                stations[index]),
                            new Vector3(0.055f, 0.12f, 0.055f),
                            TankUpiorFamilyDetails.Gunmetal());
                    roller.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                }
            }
        }

        private static void AddVerticalCylinder(
            string name,
            Transform parent,
            float radius,
            float height,
            float x,
            float y,
            float z,
            Color color)
        {
            TankDetailGeometry.Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                new Vector3(x, y, z),
                new Vector3(radius, height, radius),
                color);
        }
    }
}
