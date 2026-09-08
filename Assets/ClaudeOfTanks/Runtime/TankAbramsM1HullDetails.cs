using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsM1HullDetails
    {
        public static void Build(
            Transform root,
            Color color,
            bool heavyArmor)
        {
            AddCore(root, color);
            AddBow(root, color);
            AddSideGear(root, color, heavyArmor);
            AddDeck(root, color);
            AddRear(root, color);
        }

        private static void AddCore(
            Transform root,
            Color color)
        {
            Part("Painted-AbramsM1-Belly", root,
                new Vector3(0f, 0.78f, -0.02f),
                new Vector3(2.08f, 0.72f, 7.18f),
                color * 0.54f);
            Part("Painted-AbramsM1-UpperHull", root,
                new Vector3(0f, 1.46f, -0.42f),
                new Vector3(3.48f, 0.48f, 6.78f),
                color * 0.68f);
            Part("Painted-AbramsM1-Deck", root,
                new Vector3(0f, 1.71f, -0.85f),
                new Vector3(3.42f, 0.045f, 5.92f),
                color * 0.72f);
        }

        private static void AddBow(
            Transform root,
            Color color)
        {
            Transform glacis = Part(
                "Painted-AbramsM1-LongGlacis",
                root,
                new Vector3(0f, 1.28f, 2.92f),
                new Vector3(3.36f, 0.12f, 1.92f),
                color * 0.7f);
            glacis.localRotation =
                Quaternion.Euler(-18f, 0f, 0f);
            Transform lower = Part(
                "Painted-AbramsM1-LowerBow",
                root,
                new Vector3(0f, 0.72f, 3.48f),
                new Vector3(2.08f, 0.14f, 1.08f),
                color * 0.48f);
            lower.localRotation =
                Quaternion.Euler(28f, 0f, 0f);
            Part("Painted-AbramsM1-SplashBoard", root,
                new Vector3(0f, 1.49f, 2.32f),
                new Vector3(2.3f, 0.075f, 0.12f),
                color * 0.62f);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsM1-HeadlightPod", root,
                    new Vector3(side * 1.05f, 1.27f, 3.87f),
                    new Vector3(0.24f, 0.22f, 0.14f),
                    color * 0.56f);
                Part("AbramsM1-Headlight", root,
                    new Vector3(side * 1.05f, 1.27f, 3.95f),
                    new Vector3(0.13f, 0.12f, 0.025f),
                    TankAbramsM1FamilyDetails.Glass());
                Part("Painted-AbramsM1-TowShackle", root,
                    new Vector3(side * 0.73f, 0.68f, 3.78f),
                    new Vector3(0.16f, 0.18f, 0.08f),
                    color * 0.48f);
            }
            Part("Painted-AbramsM1-DriverHatch", root,
                new Vector3(-0.1f, 1.54f, 1.78f),
                new Vector3(0.72f, 0.06f, 0.58f),
                color * 0.68f);
            for (int scope = -1; scope <= 1; scope++)
                Part("AbramsM1-DriverPeriscope", root,
                    new Vector3(scope * 0.21f - 0.1f, 1.6f, 2.06f),
                    new Vector3(0.13f, 0.055f, 0.05f),
                    TankAbramsM1FamilyDetails.Glass());
        }

        private static void AddSideGear(
            Transform root,
            Color color,
            bool heavyArmor)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsM1-Fender", root,
                    new Vector3(side * 1.71f, 1.45f, -0.05f),
                    new Vector3(0.2f, 0.09f, 7.18f),
                    color * 0.62f);
                for (int panel = 0; panel < 7; panel++)
                {
                    float z = 2.64f - panel * 0.92f;
                    Part(
                        heavyArmor
                            ? "Painted-AbramsM1Ha-SideModule"
                            : "Painted-AbramsM1-SideSkirt",
                        root,
                        new Vector3(
                            side * (heavyArmor ? 1.93f : 1.79f),
                            heavyArmor ? 1.09f : 1.05f,
                            z),
                        new Vector3(
                            heavyArmor ? 0.12f : 0.055f,
                            heavyArmor ? 0.72f : 0.66f,
                            0.84f),
                        color * (heavyArmor ? 0.59f : 0.61f));
                    Part("AbramsM1-SkirtHinge", root,
                        new Vector3(
                            side * (heavyArmor ? 1.997f : 1.822f),
                            1.43f,
                            z),
                        new Vector3(0.025f, 0.04f, 0.12f),
                        TankAbramsM1FamilyDetails.Dark());
                }
                AddTowCable(root, side, heavyArmor, color);
            }
        }

        private static void AddTowCable(
            Transform root,
            int side,
            bool heavyArmor,
            Color color)
        {
            bool ownsCable =
                (!heavyArmor && side < 0) ||
                (heavyArmor && side > 0);
            if (!ownsCable) return;
            for (int segment = 0; segment < 6; segment++)
            {
                float z = -2.2f + segment * 0.66f;
                Transform cable = TankDetailGeometry.Part(
                    heavyArmor
                        ? "AbramsM1Ha-TowCable"
                        : "AbramsM1-TowCable",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(side * 1.79f, 1.46f, z),
                    new Vector3(0.022f, 0.34f, 0.022f),
                    TankAbramsM1FamilyDetails.Dark());
                cable.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            Part("Painted-AbramsM1-CableClamp", root,
                new Vector3(side * 1.79f, 1.45f, -2.25f),
                new Vector3(0.06f, 0.05f, 0.1f),
                color * 0.48f);
            Part("Painted-AbramsM1-CableClamp", root,
                new Vector3(side * 1.79f, 1.45f, 1.15f),
                new Vector3(0.06f, 0.05f, 0.1f),
                color * 0.48f);
        }

        private static void AddDeck(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsM1-RearDeckPod", root,
                    new Vector3(side * 1.55f, 1.735f, -3.38f),
                    new Vector3(0.34f, 0.05f, 0.5f),
                    color * 0.62f);
                for (int louvre = 0; louvre < 6; louvre++)
                    Part("AbramsM1-DeckLouvre", root,
                        new Vector3(
                            side * 1.55f,
                            1.766f,
                            -3.58f + louvre * 0.08f),
                        new Vector3(0.28f, 0.012f, 0.035f),
                        TankAbramsM1FamilyDetails.Dark());
            }
            for (int grille = 0; grille < 9; grille++)
                Part("AbramsM1-EngineDeckGrille", root,
                    new Vector3(
                        -0.88f + grille * 0.22f,
                        1.755f,
                        -2.72f),
                    new Vector3(0.15f, 0.014f, 0.82f),
                    TankAbramsM1FamilyDetails.Gunmetal());
        }

        private static void AddRear(
            Transform root,
            Color color)
        {
            Part("Painted-AbramsM1-RearPlate", root,
                new Vector3(0f, 1.28f, -3.86f),
                new Vector3(2.15f, 0.72f, 0.12f),
                color * 0.52f);
            Part("AbramsM1-RearGrille", root,
                new Vector3(0f, 1.4f, -3.93f),
                new Vector3(1.72f, 0.38f, 0.03f),
                TankAbramsM1FamilyDetails.Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsM1-TaillightGuard", root,
                    new Vector3(side * 1.36f, 1.57f, -3.94f),
                    new Vector3(0.22f, 0.16f, 0.05f),
                    color * 0.48f);
                Part("AbramsM1-Taillight", root,
                    new Vector3(side * 1.36f, 1.57f, -3.975f),
                    new Vector3(0.08f, 0.07f, 0.02f),
                    new Color(0.38f, 0.025f, 0.018f));
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
