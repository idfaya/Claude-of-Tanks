using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankM1A2HullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color)
        {
            AddCommonDeckDetails(root, color);
            switch (definition.id)
            {
                case "m1a2":
                    AddCleanLoadout(root, color);
                    break;
                case "m1a2_tusk":
                    AddTuskUrbanKit(root, color);
                    break;
                case "m1a2_sepv2":
                    AddSepV2Kit(root, color);
                    break;
                case "m1a2_sepv3":
                    AddSepV3Kit(root, color);
                    break;
            }
        }

        private static void AddCommonDeckDetails(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int station = 0; station < 2; station++)
                {
                    float z = station == 0 ? 2.75f : -2.2f;
                    Transform ring =
                        TankDetailGeometry.Part(
                            "M1A2-DeckTieDown",
                            PrimitiveType.Cylinder,
                            root,
                            new Vector3(side * (station == 0 ? 0.55f : 0.86f),
                                station == 0 ? 1.54f : 1.77f,
                                z),
                            new Vector3(0.036f, 0.012f, 0.036f),
                            TankM1A2FamilyDetails.Gunmetal());
                    ring.localRotation =
                        Quaternion.Euler(90f, 0f, 0f);
                }
            }
        }

        private static void AddCleanLoadout(
            Transform root,
            Color color)
        {
            for (int link = 0; link < 4; link++)
                Part("M1A2-SpareTrackLink", root,
                    new Vector3(1.75f, 1.45f, -0.86f + link * 0.17f),
                    new Vector3(0.05f, 0.09f, 0.14f),
                    TankM1A2FamilyDetails.Gunmetal());
        }

        private static void AddTuskUrbanKit(
            Transform root,
            Color color)
        {
            Color frame = color * 0.46f;
            Part("Painted-M1A2-TuskSlatTop", root,
                new Vector3(0f, 1.58f, -4f),
                new Vector3(3.35f, 0.066f, 0.066f),
                frame);
            Part("Painted-M1A2-TuskSlatBottom", root,
                new Vector3(0f, 0.92f, -4f),
                new Vector3(3.35f, 0.066f, 0.066f),
                frame);
            for (int post = 0; post < 7; post++)
                Part("Painted-M1A2-TuskSlatPost", root,
                    new Vector3(-1.62f + post * 0.54f, 1.25f, -4f),
                    new Vector3(0.042f, 0.7f, 0.042f),
                    frame);
            for (int row = 0; row < 6; row++)
                Part("Painted-M1A2-TuskSlatRow", root,
                    new Vector3(0f, 0.985f + row * 0.098f, -4.006f),
                    new Vector3(3.3f, 0.045f, 0.024f),
                    frame);
            for (int brace = -1; brace <= 1; brace++)
                Part("Painted-M1A2-TuskSlatBrace", root,
                    new Vector3(brace * 1.05f, 1.35f, -3.72f),
                    new Vector3(0.05f, 0.05f, 0.6f),
                    frame);

            Part("Painted-M1A2-TankInfantryPhone", root,
                new Vector3(1.52f, 1.3f, -3.93f),
                new Vector3(0.16f, 0.24f, 0.07f),
                color * 0.55f);
            Part("M1A2-TipLidSeam", root,
                new Vector3(1.52f, 1.352f, -3.968f),
                new Vector3(0.13f, 0.014f, 0.012f),
                TankM1A2FamilyDetails.Dark());
            Part("M1A2-TipLatch", root,
                new Vector3(1.472f, 1.29f, -3.968f),
                new Vector3(0.024f, 0.05f, 0.012f),
                TankM1A2FamilyDetails.Dark());
            for (int segment = 0; segment < 5; segment++)
            {
                Transform cable =
                    TankDetailGeometry.Part(
                        "M1A2-TipCable",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            1.56f - segment * 0.008f,
                            1.18f - segment * 0.055f,
                            -3.98f),
                        new Vector3(0.01f, 0.035f, 0.01f),
                        TankM1A2FamilyDetails.Dark());
                cable.localRotation =
                    Quaternion.Euler(0f, 0f,
                        segment % 2 == 0 ? 18f : -18f);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-M1A2-TuskUrbanLightGuard", root,
                    new Vector3(side * 1.55f, 1.34f, 3.62f),
                    new Vector3(0.2f, 0.18f, 0.16f),
                    color * 0.55f);
                Part("M1A2-TuskUrbanLight", root,
                    new Vector3(side * 1.55f, 1.34f, 3.71f),
                    new Vector3(0.12f, 0.1f, 0.025f),
                    TankM1A2FamilyDetails.Glass());
                Part("Painted-M1A2-TuskMirrorMast", root,
                    new Vector3(side * 1.6f, 1.45f, 3.36f),
                    new Vector3(0.035f, 0.27f, 0.035f),
                    color * 0.48f);
                Part("M1A2-TuskMirror", root,
                    new Vector3(side * 1.61f, 1.59f, 3.31f),
                    new Vector3(0.035f, 0.12f, 0.16f),
                    TankM1A2FamilyDetails.Dark());
            }
        }

        private static void AddSepV2Kit(
            Transform root,
            Color color)
        {
            for (int segment = 0; segment < 6; segment++)
            {
                Transform cable =
                    TankDetailGeometry.Part(
                        "M1A2-SepV2-DeckTowCable",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            1.12f + segment * 0.035f,
                            1.5f - segment * 0.008f,
                            1.92f - segment * 0.24f),
                        new Vector3(0.014f, 0.14f, 0.014f),
                        TankM1A2FamilyDetails.Dark());
                cable.localRotation =
                    Quaternion.Euler(90f, 0f, -8f);
            }
            for (int side = -1; side <= 1; side += 2)
                Part("Painted-M1A2-SepV2-CipPanel", root,
                    new Vector3(side * 1.81f, 1.29f, 1.28f),
                    new Vector3(0.025f, 0.4f, 0.5f),
                    color * 0.82f);

            Part("Painted-M1A2-SepV2-UaapuFrameL", root,
                new Vector3(-0.885f, 1.2f, -3.94f),
                new Vector3(0.016f, 0.34f, 0.018f),
                color * 0.7f);
            Part("Painted-M1A2-SepV2-UaapuFrameR", root,
                new Vector3(-0.655f, 1.2f, -3.94f),
                new Vector3(0.016f, 0.34f, 0.018f),
                color * 0.7f);
            Transform outlet =
                TankDetailGeometry.Part(
                    "M1A2-SepV2-UaapuOutlet",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(-0.77f, 1.225f, -3.955f),
                    new Vector3(0.052f, 0.02f, 0.052f),
                    TankM1A2FamilyDetails.Dark());
            outlet.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            Part("Painted-M1A2-SepV2-RearCipPanel", root,
                new Vector3(0f, 1.24f, -3.97f),
                new Vector3(0.72f, 0.44f, 0.025f),
                color * 0.82f);
        }

        private static void AddSepV3Kit(
            Transform root,
            Color color)
        {
            Part("Painted-M1A2-SepV3-UaapuHousing", root,
                new Vector3(-1.56f, 1.84f, -3.41f),
                new Vector3(0.35f, 0.24f, 0.25f),
                color * 0.58f);
            Part("M1A2-SepV3-UaapuLouvre", root,
                new Vector3(-1.555f, 1.968f, -3.41f),
                new Vector3(0.29f, 0.014f, 0.21f),
                TankM1A2FamilyDetails.Dark());
            for (int seam = -1; seam <= 1; seam++)
                Part("M1A2-SepV3-UaapuSeam", root,
                    new Vector3(-1.555f, 1.977f, -3.41f + seam * 0.065f),
                    new Vector3(0.29f, 0.008f, 0.018f),
                    color * 0.75f);
            Transform exhaust =
                TankDetailGeometry.Part(
                    "M1A2-SepV3-UaapuExhaust",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(-1.76f, 1.86f, -3.35f),
                    new Vector3(0.034f, 0.06f, 0.034f),
                    TankM1A2FamilyDetails.Dark());
            exhaust.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
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
