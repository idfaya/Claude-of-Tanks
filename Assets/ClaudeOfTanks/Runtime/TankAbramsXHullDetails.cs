using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsXHullDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddHullCore(root, color);
            AddSpearBow(root, color);
            AddCrewDeck(root, color);
            AddSkirts(root, color);
            AddHybridDeck(root, color);
            AddStern(root, color);
        }

        private static void AddHullCore(
            Transform root,
            Color color)
        {
            Part("Painted-AbramsX-BellyPan", root,
                new Vector3(0f, 0.715f, -0.52f),
                new Vector3(1.92f, 0.61f, 6.08f),
                color * 0.52f);
            TankAbramsXGeometry.Frustum(
                "Painted-AbramsX-FacetedHull",
                root,
                new Vector3(0f, 1.18f, -0.48f),
                1.66f,
                1.5f,
                3.04f,
                2.78f,
                0.68f,
                color * 0.66f);
            Part("Painted-AbramsX-Deck", root,
                new Vector3(0f, 1.61f, -0.72f),
                new Vector3(3.28f, 0.08f, 4.88f),
                color * 0.72f);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsX-Sponson", root,
                    new Vector3(side * 1.38f, 1.28f, -0.48f),
                    new Vector3(0.62f, 0.55f, 6.05f),
                    color * 0.61f);
                Part("Painted-AbramsX-Fender", root,
                    new Vector3(side * 1.69f, 1.56f, -0.14f),
                    new Vector3(0.25f, 0.07f, 7.28f),
                    color * 0.69f);
            }
        }

        private static void AddSpearBow(
            Transform root,
            Color color)
        {
            Transform lower = Part(
                "Painted-AbramsX-LowerBow",
                root,
                new Vector3(0f, 0.78f, 3.48f),
                new Vector3(2.45f, 0.12f, 1.32f),
                color * 0.48f);
            lower.localRotation =
                Quaternion.Euler(39f, 0f, 0f);
            Transform glacis = Part(
                "Painted-AbramsX-KnifeGlacis",
                root,
                new Vector3(0f, 1.28f, 3.28f),
                new Vector3(3.05f, 0.11f, 1.55f),
                color * 0.76f);
            glacis.localRotation =
                Quaternion.Euler(-13f, 0f, 0f);
            Part("Painted-AbramsX-SpearSpine", root,
                new Vector3(0f, 1.29f, 3.72f),
                new Vector3(0.62f, 0.1f, 0.58f),
                color * 0.79f);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform shoulder = Part(
                    "Painted-AbramsX-BowShoulder",
                    root,
                    new Vector3(side * 1.08f, 1.29f, 3.36f),
                    new Vector3(1.18f, 0.14f, 1.02f),
                    color * 0.72f);
                shoulder.localRotation =
                    Quaternion.Euler(
                        -9f,
                        side * 19f,
                        side * 3f);
                Part("Painted-AbramsX-HeadlightRecess", root,
                    new Vector3(side * 1.29f, 1.39f, 3.37f),
                    new Vector3(0.2f, 0.1f, 0.13f),
                    TankAbramsXFamilyDetails.Dark());
                Part("AbramsX-Headlight", root,
                    new Vector3(side * 1.29f, 1.405f, 3.44f),
                    new Vector3(0.12f, 0.055f, 0.018f),
                    TankAbramsXFamilyDetails.Glass());
                Part("Painted-AbramsX-BowFenderCap", root,
                    new Vector3(side * 1.75f, 1.13f, 3.58f),
                    new Vector3(0.09f, 0.47f, 0.34f),
                    color * 0.43f);
            }
            Part("AbramsX-BowCrease", root,
                new Vector3(0f, 1.38f, 3.47f),
                new Vector3(0.02f, 0.02f, 0.86f),
                TankAbramsXFamilyDetails.Dark());
        }

        private static void AddCrewDeck(
            Transform root,
            Color color)
        {
            for (int station = -1; station <= 1; station++)
            {
                float x = station * 0.72f;
                Part("Painted-AbramsX-CrewHatch", root,
                    new Vector3(x, 1.65f, 1.73f),
                    new Vector3(0.56f, 0.055f, 0.58f),
                    color * 0.7f);
                Part("AbramsX-CrewHatchSeal", root,
                    new Vector3(x, 1.682f, 1.73f),
                    new Vector3(0.43f, 0.018f, 0.45f),
                    TankAbramsXFamilyDetails.Dark());
                for (int optic = -1; optic <= 1; optic += 2)
                    Part("AbramsX-CrewPeriscope", root,
                        new Vector3(
                            x + optic * 0.13f,
                            1.71f,
                            2.01f),
                        new Vector3(0.12f, 0.05f, 0.035f),
                        TankAbramsXFamilyDetails.Glass());
            }
            Part("Painted-AbramsX-ForwardRadioDeck", root,
                new Vector3(0f, 1.67f, 1.18f),
                new Vector3(0.88f, 0.12f, 0.54f),
                color * 0.59f);
        }

        private static void AddSkirts(
            Transform root,
            Color color)
        {
            float[] centers =
            {
                -3.24f, -2.73f, -2.18f, -1.56f, -0.92f,
                -0.3f, 0.31f, 0.91f, 1.48f, 2.12f, 2.73f, 3.34f
            };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int panel = 0;
                    panel < centers.Length;
                    panel++)
                {
                    float z = centers[panel];
                    float bottom =
                        z > 3f
                            ? 0.94f
                            : z > 2.7f
                                ? 0.73f
                                : z > 2.2f
                                    ? 0.54f
                                    : 0.66f;
                    float top =
                        Mathf.Clamp(
                            1.54f - z * 0.045f,
                            1.37f,
                            1.73f);
                    float height = top - bottom;
                    Transform cassette = Part(
                        "Painted-AbramsX-KneedSkirt",
                        root,
                        new Vector3(
                            side * 1.78f,
                            (top + bottom) * 0.5f,
                            z),
                        new Vector3(
                            0.1f,
                            height,
                            panel == 11 ? 0.48f : 0.54f),
                        color * 0.58f);
                    cassette.localRotation =
                        Quaternion.Euler(
                            z > 3f ? -8f : 0f,
                            0f,
                            side * 2f);
                    Part("AbramsX-SkirtJoint", root,
                        new Vector3(
                            side * 1.837f,
                            (top + bottom) * 0.5f,
                            z + 0.29f),
                        new Vector3(0.018f, height * 0.88f, 0.022f),
                        TankAbramsXFamilyDetails.Dark());
                    for (int bolt = -1; bolt <= 1; bolt++)
                        Part("AbramsX-SkirtBolt", root,
                            new Vector3(
                                side * 1.842f,
                                top - 0.16f,
                                z + bolt * 0.16f),
                            new Vector3(0.018f, 0.018f, 0.018f),
                            TankAbramsXFamilyDetails.Gunmetal());
                }
                Part("Painted-AbramsX-RubRail", root,
                    new Vector3(side * 1.817f, 0.79f, -0.06f),
                    new Vector3(0.023f, 0.042f, 6.86f),
                    color * 0.67f);
            }
        }

        private static void AddHybridDeck(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsX-HybridPlenum", root,
                    new Vector3(side * 0.68f, 1.71f, -3f),
                    new Vector3(1.05f, 0.08f, 0.75f),
                    color * 0.53f);
                for (int slat = 0; slat < 14; slat++)
                    Part("AbramsX-HybridLouvre", root,
                        new Vector3(
                            side * 0.68f,
                            1.765f,
                            -3.33f + slat * 0.052f),
                        new Vector3(0.98f, 0.012f, 0.018f),
                        TankAbramsXFamilyDetails.Dark());
            }
        }

        private static void AddStern(
            Transform root,
            Color color)
        {
            Part("Painted-AbramsX-SternPlate", root,
                new Vector3(0f, 1.11f, -3.91f),
                new Vector3(3.38f, 1.2f, 0.12f),
                color * 0.52f);
            Part("AbramsX-SternRadiatorWell", root,
                new Vector3(0f, 1.12f, -3.982f),
                new Vector3(1.78f, 0.82f, 0.018f),
                TankAbramsXFamilyDetails.Dark());
            for (int row = 0; row < 6; row++)
            {
                float y = 0.82f + row * 0.128f;
                for (int side = -1; side <= 1; side += 2)
                {
                    Transform vane = Part(
                        "AbramsX-HybridChevronVane",
                        root,
                        new Vector3(side * 0.43f, y, -4f),
                        new Vector3(0.72f, 0.035f, 0.08f),
                        TankAbramsXFamilyDetails.Gunmetal());
                    vane.localRotation =
                        Quaternion.Euler(0f, side * 16f, side * 12f);
                }
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsX-SternServiceBox", root,
                    new Vector3(
                        side > 0 ? 1.39f : -1.48f,
                        side > 0 ? 1.49f : 1.53f,
                        -3.67f),
                    new Vector3(
                        side > 0 ? 0.64f : 0.45f,
                        side > 0 ? 0.37f : 0.29f,
                        side > 0 ? 0.32f : 0.24f),
                    color * 0.57f);
                Part("AbramsX-TaillightGuard", root,
                    new Vector3(side * 0.7f, 1.32f, -3.988f),
                    new Vector3(0.19f, 0.11f, 0.03f),
                    TankAbramsXFamilyDetails.Dark());
                Part("AbramsX-Taillight", root,
                    new Vector3(side * 0.67f, 1.32f, -4.007f),
                    new Vector3(0.05f, 0.05f, 0.012f),
                    new Color(0.48f, 0.055f, 0.035f));
                Part("AbramsX-RecoveryPoint", root,
                    new Vector3(side * 0.45f, 0.88f, -4.006f),
                    new Vector3(0.11f, 0.13f, 0.035f),
                    TankAbramsXFamilyDetails.Gunmetal());
            }
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
