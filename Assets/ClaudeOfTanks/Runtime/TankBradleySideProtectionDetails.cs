using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBradleySideProtectionDetails
    {
        private static readonly float[] Cuts =
        {
            -2.97f,
            -2.21f,
            -1.45f,
            -0.69f,
            0.07f,
            0.83f,
            1.59f,
            2.35f,
            3.11f
        };

        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color)
        {
            AddMountedSkirts(
                root,
                color);
            if (TankBradleyFamilyDetails
                .IsUkrainian(definition))
            {
                AddUkrainianPackage(
                    root,
                    color);
            }
            else if (TankBradleyFamilyDetails
                .IsM3A3(definition))
            {
                AddM3CarrierBacking(
                    root,
                    color);
            }
            else
            {
                AddA2Applique(
                    root,
                    color);
            }
        }

        private static void AddMountedSkirts(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel + 1 < Cuts.Length;
                    panel++)
                {
                    bool low = panel >= 6;
                    float rear = Cuts[panel];
                    float front = Cuts[panel + 1];
                    TankDetailGeometry.Part(
                        "Painted-Bradley-SkirtPanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.6145f,
                            low ? 0.935f : 1.02f,
                            (rear + front) * 0.5f),
                        new Vector3(
                            0.075f,
                            low ? 0.63f : 0.8f,
                            front - rear - 0.02f),
                        color * (0.7f -
                            panel % 2 * 0.025f));
                }
                for (int joint = 1;
                    joint + 1 < Cuts.Length;
                    joint++)
                {
                    bool low = joint >= 6;
                    TankDetailGeometry.Part(
                        "Bradley-SkirtSeam",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.655f,
                            low ? 0.92f : 1f,
                            Cuts[joint]),
                        new Vector3(
                            0.05f,
                            low ? 0.56f : 0.7f,
                            0.024f),
                        TankBradleyFamilyDetails.Dark());
                    TankDetailGeometry.Part(
                        "Painted-Bradley-SkirtHanger",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.6125f,
                            low ? 1.3f : 1.46f,
                            Cuts[joint]),
                        new Vector3(0.085f, 0.1f, 0.06f),
                        color * 0.55f);
                    Transform bolt =
                        TankDetailGeometry.Part(
                            "Bradley-SkirtHangerBolt",
                            PrimitiveType.Cylinder,
                            root,
                            new Vector3(
                                side * 1.665f,
                                low ? 1.3f : 1.46f,
                                Cuts[joint]),
                            new Vector3(0.025f, 0.018f, 0.025f),
                            TankBradleyFamilyDetails.Gunmetal());
                    bolt.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            90f);
                }
                Transform apron =
                    TankDetailGeometry.Part(
                        "Painted-Bradley-SkirtApron",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.575f,
                            1.5f,
                            0.08f),
                        new Vector3(0.18f, 0.12f, 5.95f),
                        color * 0.66f);
                apron.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 33f);
                TankDetailGeometry.Part(
                    "Painted-Bradley-SkirtEndCap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.576f,
                        0.935f,
                        3.085f),
                    new Vector3(0.076f, 0.63f, 0.05f),
                    color * 0.64f);
                TankDetailGeometry.Part(
                    "Painted-Bradley-SkirtEndCap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.576f,
                        1.02f,
                        -2.945f),
                    new Vector3(0.076f, 0.8f, 0.05f),
                    color * 0.64f);
            }
        }

        private static void AddA2Applique(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bradley-A2LeftApplique",
                PrimitiveType.Cube,
                root,
                new Vector3(-1.478f, 1.35f, -0.84f),
                new Vector3(0.045f, 0.5f, 4.26f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Bradley-A2RightApplique",
                PrimitiveType.Cube,
                root,
                new Vector3(1.5825f, 1.43f, -2.13f),
                new Vector3(0.06f, 0.72f, 0.4f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Bradley-A2RightApplique",
                PrimitiveType.Cube,
                root,
                new Vector3(1.5125f, 1.43f, -0.225f),
                new Vector3(0.12f, 0.72f, 3.15f),
                color * 0.65f);
        }

        private static void AddUkrainianPackage(
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
                        "Painted-BradleyUA-HeavySideModule",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.73f,
                            1.43f,
                            2.42f -
                                panel * 0.71f),
                        new Vector3(0.15f, 0.58f, 0.62f),
                        color * (0.7f -
                            panel % 2 * 0.025f));
                }
                for (int row = 0;
                    row < 2;
                    row++)
                {
                    for (int tile = 0;
                        tile < 4;
                        tile++)
                    {
                        Transform glacisTile =
                            TankDetailGeometry.Part(
                                "Painted-BradleyUA-GlacisTile",
                                PrimitiveType.Cube,
                                root,
                                new Vector3(
                                    side *
                                        (0.25f +
                                         tile * 0.3f),
                                    1.57f -
                                        row * 0.1f,
                                    2.4f -
                                        row * 0.26f),
                                new Vector3(0.27f, 0.095f, 0.28f),
                                color * 0.73f);
                        glacisTile.localRotation =
                            Quaternion.Euler(
                                -19.5f,
                                0f,
                                0f);
                    }
                }
            }
        }

        private static void AddM3CarrierBacking(
            Transform root,
            Color color)
        {
            Transform carrier =
                TankDetailGeometry.Part(
                    "Painted-BradleyM3-GlacisEraCarrier",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.715f, 2.02f),
                    new Vector3(2.14f, 0.055f, 0.96f),
                    color * 0.45f);
            carrier.localRotation =
                Quaternion.Euler(
                    26.565f,
                    0f,
                    0f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-BradleyM3-SideEraCarrier",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.7f,
                        1.32f,
                        -0.05f),
                    new Vector3(0.08f, 0.72f, 5.55f),
                    color * 0.48f);
            }
        }
    }
}
