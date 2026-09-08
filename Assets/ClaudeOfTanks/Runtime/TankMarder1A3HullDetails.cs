using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMarder1A3HullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            TankBradleyHullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            AddA3SideArmor(
                root,
                color);
            AddFlankStowage(
                root,
                color);
        }

        private static void AddA3SideArmor(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 8;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Marder-A3SidePanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.68f,
                            1.22f,
                            2.42f -
                                panel * 0.74f),
                        new Vector3(0.12f, 0.42f, 0.72f),
                        color * (0.69f -
                            panel % 2 * 0.025f));
                }
                for (int rail = 0;
                    rail < 3;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "Marder-A3AppliqueRail",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.747f,
                            1.02f +
                                rail * 0.18f,
                            -0.18f),
                        new Vector3(0.026f, 0.045f, 5.25f),
                        TankMarder1A3FamilyDetails.Gunmetal());
                }
                TankDetailGeometry.Part(
                    "Painted-Marder-A3PanelTopRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.69f,
                        1.46f,
                        -0.18f),
                    new Vector3(0.1f, 0.06f, 5.4f),
                    color * 0.56f);
                TankDetailGeometry.Part(
                    "Marder-RubberMudguard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.64f,
                        0.82f,
                        2.86f),
                    new Vector3(0.08f, 0.28f, 0.38f),
                    TankMarder1A3FamilyDetails.Dark());
                TankDetailGeometry.Part(
                    "Marder-RubberMudguard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.64f,
                        0.82f,
                        -2.95f),
                    new Vector3(0.08f, 0.28f, 0.32f),
                    TankMarder1A3FamilyDetails.Dark());
            }
        }

        private static void AddFlankStowage(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Marder-LeftRearStowage",
                PrimitiveType.Cube,
                root,
                new Vector3(-1.63f, 1.53f, -2.25f),
                new Vector3(0.22f, 0.3f, 0.92f),
                color * 0.56f);
            TankDetailGeometry.Part(
                "Marder-LeftRearStowageLid",
                PrimitiveType.Cube,
                root,
                new Vector3(-1.755f, 1.54f, -2.25f),
                new Vector3(0.035f, 0.22f, 0.78f),
                TankMarder1A3FamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Painted-Marder-RightExhaustHousing",
                PrimitiveType.Cube,
                root,
                new Vector3(1.58f, 1.54f, 1.45f),
                new Vector3(0.22f, 0.32f, 0.92f),
                color * 0.54f);
            for (int louvre = 0;
                louvre < 4;
                louvre++)
            {
                TankDetailGeometry.Part(
                    "Marder-RightExhaustLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        1.705f,
                        1.43f +
                            louvre * 0.075f,
                        1.45f),
                    new Vector3(0.035f, 0.025f, 0.72f),
                    TankMarder1A3FamilyDetails.Dark());
            }
        }
    }
}
