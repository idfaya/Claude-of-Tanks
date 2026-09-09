using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT72B3MHullDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddHull(root, color);
            AddReliktSkirts(root, color);
            AddGlacisAndBow(root, color);
            AddEngineDeck(root, color);
            AddRearCage(root, color);
        }

        private static void AddHull(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T72B3M-LowerTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.76f, -0.95f),
                new Vector3(2.28f, 0.68f, 5.8f),
                color * 0.5f);
            Part(
                "Painted-T72B3M-UpperHull",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.18f, -0.85f),
                new Vector3(3.05f, 0.32f, 5.85f),
                color * 0.68f);
            Part(
                "Painted-T72B3M-Deck",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.41f, -1.05f),
                new Vector3(2.95f, 0.08f, 5.15f),
                color * 0.78f);

            Transform glacis = Part(
                "Painted-T72B3M-UpperGlacis",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.17f, 1.57f),
                new Vector3(2.78f, 0.12f, 1.2f),
                color * 0.66f);
            glacis.localRotation =
                Quaternion.Euler(-24f, 0f, 0f);

            Transform lower = Part(
                "Painted-T72B3M-LowerGlacis",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.74f, 1.86f),
                new Vector3(2.35f, 0.12f, 0.72f),
                color * 0.5f);
            lower.localRotation =
                Quaternion.Euler(34f, 0f, 0f);
        }

        private static void AddReliktSkirts(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T72B3M-FenderShelf",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.55f, 1.28f, -1.05f),
                    new Vector3(0.18f, 0.08f, 5.75f),
                    color * 0.72f);
                for (int panel = 0;
                    panel < 9;
                    panel++)
                {
                    float z =
                        -3.38f + panel * 0.62f;
                    Part(
                        "Painted-T72B3M-ReliktSoftBag",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.68f, 0.93f, z),
                        new Vector3(0.09f, 0.52f, 0.5f),
                        TankT72B3MFamilyDetails.Cloth());
                    Part(
                        "T72B3M-SkirtRib",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.73f, 0.98f, z + 0.22f),
                        new Vector3(0.025f, 0.42f, 0.04f),
                        TankT72B3MFamilyDetails.Dark());
                }
            }
        }

        private static void AddGlacisAndBow(
            Transform root,
            Color color)
        {
            for (int row = 0;
                row < 2;
                row++)
            {
                for (int column = -3;
                    column <= 3;
                    column++)
                {
                    if (column == 0 && row == 1) continue;
                    Transform brick = Part(
                        "Painted-T72B3M-ReliktGlacisCassette",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            column * 0.28f,
                            0.86f + row * 0.24f,
                            1.72f),
                        new Vector3(0.23f, 0.16f, 0.075f),
                        color * 0.55f);
                    brick.localRotation =
                        Quaternion.Euler(-16f, 0f, 0f);
                }
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform shackle = Part(
                    "T72B3M-TowShackle",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(side * 0.55f, 0.55f, 1.67f),
                    new Vector3(0.07f, 0.018f, 0.07f),
                    TankT72B3MFamilyDetails.Dark());
                shackle.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                Part(
                    "T72B3M-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.42f, 1.2f, 1.55f),
                    new Vector3(0.15f, 0.12f, 0.09f),
                    TankT72B3MFamilyDetails.Dark());
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color)
        {
            for (int line = 0;
                line < 5;
                line++)
            {
                Part(
                    "T72B3M-EngineLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0.58f, 1.455f, -3.52f + line * 0.16f),
                    new Vector3(1.05f, 0.025f, 0.04f),
                    TankT72B3MFamilyDetails.Dark());
            }
            Part(
                "Painted-T72B3M-DeckStowageLid",
                PrimitiveType.Cube,
                root,
                new Vector3(-0.72f, 1.455f, -3.62f),
                new Vector3(0.78f, 0.045f, 0.52f),
                color * 0.82f);
        }

        private static void AddRearCage(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T72B3M-RearSlatBacker",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.28f, -4.48f),
                new Vector3(2.05f, 0.42f, 0.035f),
                color * 0.45f);
            for (int slat = 0;
                slat < 11;
                slat++)
            {
                Part(
                    "T72B3M-RearSlat",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(-0.95f + slat * 0.19f, 1.28f, -4.51f),
                    new Vector3(0.045f, 0.44f, 0.035f),
                    TankT72B3MFamilyDetails.Dark());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform drum = Part(
                    "Painted-T72B3M-RearFuelDrum",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(side * 1.26f, 1.14f, -4.12f),
                    new Vector3(0.235f, 0.25f, 0.235f),
                    color * 0.62f);
                drum.localRotation =
                    Quaternion.Euler(90f, 0f, side * 18f);
            }
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
