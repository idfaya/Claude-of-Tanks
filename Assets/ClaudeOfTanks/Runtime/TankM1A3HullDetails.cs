using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankM1A3HullDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddCore(root, color);
            AddBow(root, color);
            AddSkirts(root, color);
            AddCrewDeck(root, color);
            AddHybridDeck(root, color);
            AddRearCage(root, color);
        }

        private static void AddCore(
            Transform root,
            Color color)
        {
            Part("Painted-M1A3-Belly", root,
                new Vector3(0f, 0.75f, -0.05f),
                new Vector3(2.08f, 0.64f, 7.15f),
                color * 0.52f);
            Part("Painted-M1A3-FacetedHull", root,
                new Vector3(0f, 1.29f, -0.18f),
                new Vector3(3.52f, 0.66f, 6.92f),
                color * 0.66f);
            Part("Painted-M1A3-HullRoof", root,
                new Vector3(0f, 1.655f, -0.72f),
                new Vector3(3.48f, 0.08f, 5.74f),
                color * 0.72f);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-M1A3-Sponson", root,
                    new Vector3(side * 1.48f, 1.2f, -0.15f),
                    new Vector3(0.58f, 0.62f, 7.15f),
                    color * 0.61f);
                Part("Painted-M1A3-Fender", root,
                    new Vector3(side * 1.78f, 1.56f, -0.04f),
                    new Vector3(0.28f, 0.08f, 7.48f),
                    color * 0.7f);
            }
        }

        private static void AddBow(
            Transform root,
            Color color)
        {
            Transform glacis = Part(
                "Painted-M1A3-IntegratedGlacis",
                root,
                new Vector3(0f, 1.22f, 3.2f),
                new Vector3(2.95f, 0.14f, 2.02f),
                color * 0.76f);
            glacis.localRotation =
                Quaternion.Euler(-22f, 0f, 0f);
            Transform lower = Part(
                "Painted-M1A3-LowerGlacis",
                root,
                new Vector3(0f, 0.67f, 3.67f),
                new Vector3(2.2f, 0.14f, 1.24f),
                color * 0.54f);
            lower.localRotation =
                Quaternion.Euler(35f, 0f, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform shoulder = Part(
                    "Painted-M1A3-GlacisShoulder",
                    root,
                    new Vector3(side * 1.28f, 1.5f, 3.3f),
                    new Vector3(0.78f, 0.18f, 1.45f),
                    color * 0.73f);
                shoulder.localRotation =
                    Quaternion.Euler(
                        -6f,
                        side * 7f,
                        side * 2f);
                Part("Painted-M1A3-HeadlightPod", root,
                    new Vector3(side * 0.82f, 1.45f, 3.88f),
                    new Vector3(0.32f, 0.2f, 0.2f),
                    color * 0.59f);
                Part("M1A3-Headlight", root,
                    new Vector3(side * 0.82f, 1.46f, 4f),
                    new Vector3(0.16f, 0.1f, 0.025f),
                    TankM1A3FamilyDetails.Glass());
            }
            Part("Painted-M1A3-BowSpine", root,
                new Vector3(0f, 1.47f, 3.35f),
                new Vector3(1.25f, 0.17f, 1.12f),
                color * 0.79f);
        }

        private static void AddSkirts(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int panel = 0; panel < 11; panel++)
                {
                    float z = -3.36f + panel * 0.64f;
                    float frontBias =
                        panel > 8
                            ? (panel - 8) * 0.055f
                            : 0f;
                    float height = 0.8f - frontBias;
                    Part("Painted-M1A3-SkirtCassette", root,
                        new Vector3(
                            side * 1.985f,
                            1.1f + frontBias * 0.35f,
                            z),
                        new Vector3(
                            0.17f,
                            height,
                            0.58f),
                        color * 0.61f);
                    Part("M1A3-SkirtRib", root,
                        new Vector3(
                            side * 2.082f,
                            1.1f + frontBias * 0.35f,
                            z + 0.27f),
                        new Vector3(
                            0.024f,
                            height * 0.76f,
                            0.025f),
                        TankM1A3FamilyDetails.Dark());
                }
                Part("Painted-M1A3-SkirtTopRail", root,
                    new Vector3(side * 1.995f, 1.53f, -0.02f),
                    new Vector3(0.2f, 0.14f, 7.18f),
                    color * 0.66f);
            }
        }

        private static void AddCrewDeck(
            Transform root,
            Color color)
        {
            for (int station = -1; station <= 1; station++)
            {
                float x = station * 0.72f;
                Part("Painted-M1A3-CrewCapsuleHatch", root,
                    new Vector3(x, 1.69f, 1.72f),
                    new Vector3(0.56f, 0.07f, 0.56f),
                    color * 0.68f);
                Part("M1A3-CrewHatchSeal", root,
                    new Vector3(x, 1.73f, 1.72f),
                    new Vector3(0.43f, 0.02f, 0.43f),
                    TankM1A3FamilyDetails.Dark());
                for (int optic = -1; optic <= 1; optic += 2)
                    Part("M1A3-CrewPeriscope", root,
                        new Vector3(
                            x + optic * 0.13f,
                            1.75f,
                            2.02f),
                        new Vector3(0.13f, 0.06f, 0.04f),
                        TankM1A3FamilyDetails.Glass());
            }
            Part("Painted-M1A3-RadioDeck", root,
                new Vector3(0f, 1.72f, 1.25f),
                new Vector3(0.88f, 0.13f, 0.54f),
                color * 0.62f);
        }

        private static void AddHybridDeck(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-M1A3-HybridPlenum", root,
                    new Vector3(side * 0.72f, 1.71f, -2.78f),
                    new Vector3(1.3f, 0.16f, 1.3f),
                    color * 0.55f);
                for (int slat = 0; slat < 7; slat++)
                    Part("M1A3-HybridLouvre", root,
                        new Vector3(
                            side * 0.72f,
                            1.825f,
                            -3.24f + slat * 0.15f),
                        new Vector3(1.02f, 0.02f, 0.045f),
                        TankM1A3FamilyDetails.Dark());
                Part("Painted-M1A3-InverterBox", root,
                    new Vector3(side * 1.36f, 1.7f, -3.45f),
                    new Vector3(0.42f, 0.2f, 0.55f),
                    color * 0.58f);
            }
            Part("Painted-M1A3-RearPowerpackPlate", root,
                new Vector3(0f, 1.12f, -4.02f),
                new Vector3(3.48f, 1.02f, 0.12f),
                color * 0.52f);
        }

        private static void AddRearCage(
            Transform root,
            Color color)
        {
            Color cage = color * 0.48f;
            float[] railYs = { 0.78f, 1.04f, 1.3f, 1.56f };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int rail = 0; rail < railYs.Length; rail++)
                    Part("Painted-M1A3-HullCageRail", root,
                        new Vector3(
                            side * 2.14f,
                            railYs[rail],
                            -3.02f),
                        new Vector3(0.035f, 0.035f, 1.86f),
                        cage);
                for (int post = 0; post < 7; post++)
                    Part("Painted-M1A3-HullCagePost", root,
                        new Vector3(
                            side * 2.14f,
                            1.17f,
                            -3.88f + post * 0.3f),
                        new Vector3(0.035f, 0.82f, 0.035f),
                        cage);
            }
            for (int rail = 0; rail < railYs.Length; rail++)
                Part("Painted-M1A3-SternCageRail", root,
                    new Vector3(0f, railYs[rail], -4.16f),
                    new Vector3(3.96f, 0.035f, 0.035f),
                    cage);
            for (int post = -3; post <= 3; post++)
                Part("Painted-M1A3-SternCagePost", root,
                    new Vector3(post * 0.54f, 1.17f, -4.16f),
                    new Vector3(0.035f, 0.82f, 0.035f),
                    cage);
        }

        private static Transform Part(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                PrimitiveType.Cube,
                parent,
                position,
                scale,
                color);
        }
    }
}
