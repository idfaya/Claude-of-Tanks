using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmp3HullDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddBoatHull(root, color);
            AddBow(root, color);
            AddRunningGearHousing(root, color);
            AddCrewDeck(root, color);
            AddRearDeck(root, color);
        }

        private static void AddBoatHull(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bmp3-CenterTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.76f, -0.1f),
                new Vector3(2.16f, 0.92f, 6.2f),
                color * 0.65f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-UpperBody",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.545f, -0.55f),
                new Vector3(3f, 0.51f, 5.42f),
                color * 0.73f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-RoofPlate",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.815f, -0.57f),
                new Vector3(2.9f, 0.05f, 5.3f),
                color * 0.78f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-MidDeckStrip",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.895f, -1.25f),
                new Vector3(1.9f, 0.11f, 1.35f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-ForeDeckCrown",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.875f, 0.9f),
                new Vector3(1.2f, 0.075f, 1.6f),
                color * 0.76f);
        }

        private static void AddBow(
            Transform root,
            Color color)
        {
            Transform upper =
                TankDetailGeometry.Part(
                    "Painted-Bmp3-UpperGlacis",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.68f, 2.66f),
                    new Vector3(2.82f, 0.11f, 1.42f),
                    color * 0.81f);
            upper.localRotation =
                Quaternion.Euler(7f, 0f, 0f);
            Transform lower =
                TankDetailGeometry.Part(
                    "Painted-Bmp3-LowerProw",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 0.65f, 2.72f),
                    new Vector3(2.06f, 0.12f, 1.42f),
                    color * 0.59f);
            lower.localRotation =
                Quaternion.Euler(-31f, 0f, 0f);
            Transform nose =
                TankDetailGeometry.Part(
                    "Painted-Bmp3-RakedNoseLip",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.31f, 3.46f),
                    new Vector3(2.1f, 0.12f, 0.48f),
                    color * 0.66f);
            nose.localRotation =
                Quaternion.Euler(-25f, 0f, 0f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-BowBellyPan",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.44f, 2.02f),
                new Vector3(1.96f, 0.06f, 0.52f),
                color * 0.55f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-TrimVaneRoll",
                PrimitiveType.Cylinder,
                root,
                new Vector3(0f, 1.665f, 3.06f),
                new Vector3(0.085f, 1.025f, 0.085f),
                color * 0.57f)
                .localRotation =
                Quaternion.Euler(0f, 0f, 90f);

            for (int rib = 0;
                rib < 4;
                rib++)
            {
                Transform waveRib =
                    TankDetailGeometry.Part(
                        "Bmp3-WaveBreakerRib",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            0f,
                            1.755f - rib * 0.026f,
                            2.46f + rib * 0.2f),
                        new Vector3(1.9f, 0.024f, 0.06f),
                        TankBmp3FamilyDetails.Gunmetal());
                waveRib.localRotation =
                    Quaternion.Euler(-9f, 0f, 0f);
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform cheek =
                    TankDetailGeometry.Part(
                        "Painted-Bmp3-BowCheek",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.25f,
                            1.48f,
                            2.85f),
                        new Vector3(0.52f, 0.34f, 1.04f),
                        color * 0.71f);
                cheek.localRotation =
                    Quaternion.Euler(
                        7f,
                        side * 18f,
                        0f);
                TankDetailGeometry.Part(
                    "Painted-Bmp3-BowMachineGunBall",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(
                        side * 0.88f,
                        1.735f,
                        2.52f),
                    new Vector3(0.17f, 0.145f, 0.17f),
                    color * 0.57f);
                AddAxialCylinder(
                    "Bmp3-BowPktBarrel",
                    root,
                    0.02f,
                    0.34f,
                    2.7f,
                    TankBmp3FamilyDetails.Dark(),
                    side * 0.88f,
                    1.76f);
                AddAxialCylinder(
                    "Bmp3-BowPktMuzzle",
                    root,
                    0.012f,
                    0.025f,
                    2.882f,
                    Color.black,
                    side * 0.88f,
                    1.76f);
                TankDetailGeometry.Part(
                    "Painted-Bmp3-TowShackle",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.55f,
                        0.97f,
                        3.13f),
                    new Vector3(0.13f, 0.08f, 0.12f),
                    color * 0.48f);
            }
        }

        private static void AddRunningGearHousing(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 14;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Bmp3-SponsonBin",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.58f,
                            1.165f,
                            2.85f - panel * 0.46f),
                        new Vector3(0.07f, 0.33f, 0.46f),
                        color * (0.68f -
                            panel % 2 * 0.02f));
                }
                TankDetailGeometry.Part(
                    "Painted-Bmp3-FenderBand",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.4f,
                        1.355f,
                        -0.14f),
                    new Vector3(0.42f, 0.06f, 6.4f),
                    color * 0.64f);
                TankDetailGeometry.Part(
                    "Bmp3-BowMudguard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.578f, 0.74f, 3.02f),
                    new Vector3(0.07f, 0.3f, 0.42f),
                    TankBmp3FamilyDetails.Dark());
                TankDetailGeometry.Part(
                    "Bmp3-SternMudguard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.578f, 0.72f, -3.3f),
                    new Vector3(0.07f, 0.32f, 0.3f),
                    TankBmp3FamilyDetails.Dark());
                TankDetailGeometry.Part(
                    "Bmp3-SideBandRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.6055f, 1.245f, -0.14f),
                    new Vector3(0.018f, 0.05f, 5.9f),
                    TankBmp3FamilyDetails.Gunmetal());
                TankDetailGeometry.Part(
                    "Painted-Bmp3-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.16f, 1.7f, 2.62f),
                    new Vector3(0.27f, 0.16f, 0.19f),
                    color * 0.61f);
                TankDetailGeometry.Part(
                    "Bmp3-Headlight",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(side * 1.16f, 1.72f, 2.725f),
                    new Vector3(0.09f, 0.09f, 0.065f),
                    TankBmp3FamilyDetails.Lens());
            }
        }

        private static void AddCrewDeck(
            Transform root,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-Bmp3-DriverHatch",
                root,
                0.24f,
                0.025f,
                -0.04f,
                1.852f,
                1.86f,
                color * 0.67f);
            for (int scope = 0;
                scope < 3;
                scope++)
            {
                TankDetailGeometry.Part(
                    "Bmp3-DriverPeriscope",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.25f + scope * 0.21f,
                        1.885f,
                        2.16f),
                    new Vector3(0.13f, 0.05f, 0.07f),
                    TankBmp3FamilyDetails.Lens());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                AddVerticalCylinder(
                    "Painted-Bmp3-FlankCrewHatch",
                    root,
                    0.2f,
                    0.025f,
                    side * 0.72f,
                    1.8f,
                    2.06f,
                    color * 0.66f);
                TankDetailGeometry.Part(
                    "Painted-Bmp3-RearStowageBin",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.06f, 1.875f, -2.4f),
                    new Vector3(0.5f, 0.13f, 0.85f),
                    color * 0.63f);
                for (int strap = -1;
                    strap <= 1;
                    strap += 2)
                {
                    TankDetailGeometry.Part(
                        "Bmp3-StowageStrap",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.06f,
                            1.95f,
                            -2.4f + strap * 0.25f),
                        new Vector3(0.52f, 0.025f, 0.045f),
                        TankBmp3FamilyDetails.Dark());
                }
            }
            AddAxialCylinder(
                "Bmp3-StowedSnorkel",
                root,
                0.055f,
                0.9f,
                -2.4f,
                TankBmp3FamilyDetails.Dark(),
                0f,
                1.878f);
        }

        private static void AddRearDeck(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bmp3-SternBody",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.155f, -3.395f),
                new Vector3(2.1f, 0.99f, 0.35f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-SternShoulder",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.475f, -3.5f),
                new Vector3(2.62f, 0.35f, 0.14f),
                color * 0.7f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmp3-TroopHatch",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.38f, 1.855f, -2.45f),
                    new Vector3(0.62f, 0.055f, 1.8f),
                    color * 0.69f);
                TankDetailGeometry.Part(
                    "Painted-Bmp3-RearDoor",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.4f, 1.32f, -3.575f),
                    new Vector3(0.55f, 0.6f, 0.035f),
                    color * 0.56f);
                AddAxialCylinder(
                    "Painted-Bmp3-WaterjetRim",
                    root,
                    0.1f,
                    0.035f,
                    -3.59f,
                    color * 0.48f,
                    side * 0.62f,
                    0.82f);
                AddAxialCylinder(
                    "Bmp3-WaterjetBore",
                    root,
                    0.05f,
                    0.025f,
                    -3.61f,
                    Color.black,
                    side * 0.62f,
                    0.82f);
                TankDetailGeometry.Part(
                    "Bmp3-Taillight",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 0.98f, 1.52f, -3.588f),
                    new Vector3(0.14f, 0.07f, 0.03f),
                    new Color(0.36f, 0.035f, 0.025f));
            }
            TankDetailGeometry.Part(
                "Bmp3-SternGrille",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.5f, -3.6f),
                new Vector3(1.34f, 0.26f, 0.02f),
                TankBmp3FamilyDetails.Dark());
            for (int louvre = 0;
                louvre < 3;
                louvre++)
            {
                TankDetailGeometry.Part(
                    "Bmp3-SternGrilleLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0f,
                        1.42f + louvre * 0.08f,
                        -3.616f),
                    new Vector3(1.28f, 0.022f, 0.045f),
                    TankBmp3FamilyDetails.Gunmetal());
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

        private static void AddAxialCylinder(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color,
            float x,
            float y)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(x, y, z),
                    new Vector3(radius, length * 0.5f, radius),
                    color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
