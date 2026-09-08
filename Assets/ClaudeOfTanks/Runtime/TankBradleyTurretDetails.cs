using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBradleyTurretDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            if (TankBradleyFamilyDetails
                .IsM3A3(definition))
            {
                AddM3A3Turret(
                    turret,
                    color);
                return;
            }

            AddA2Turret(
                turret,
                color);
            if (TankBradleyFamilyDetails
                .IsUkrainian(definition))
            {
                AddUkrainianArmor(
                    turret,
                    color);
            }
        }

        private static void AddA2Turret(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bradley-A2RingCollar",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.055f, -0.1f),
                new Vector3(0.69f, 0.055f, 0.69f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Bradley-A2Core",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.31f, -0.12f),
                new Vector3(1.48f, 0.55f, 1.9f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Painted-Bradley-A2FrontStep",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.2975f, 0.775f),
                new Vector3(1.36f, 0.49f, 0.23f),
                color * 0.76f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform cheek =
                    TankDetailGeometry.Part(
                        "Painted-Bradley-A2Cheek",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 0.45f,
                            0.34f,
                            0.93f),
                        new Vector3(0.52f, 0.42f, 0.3f),
                        color * 0.72f);
                cheek.localRotation =
                    Quaternion.Euler(
                        -8f,
                        side * 12f,
                        0f);
            }
            TankDetailGeometry.Part(
                "Painted-Bradley-A2RoofRiser",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.3625f, 0.695f, -0.6f),
                new Vector3(0.665f, 0.26f, 0.6f),
                color * 0.76f);
            TankDetailGeometry.Part(
                "Painted-Bradley-A2RoofRiser",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.3575f, 0.695f, 0.125f),
                new Vector3(0.655f, 0.26f, 0.85f),
                color * 0.76f);
            TankDetailGeometry.Part(
                "Painted-Bradley-A2RightStowageWing",
                PrimitiveType.Cube,
                turret,
                new Vector3(1.0625f, 0.3f, -0.015f),
                new Vector3(0.525f, 0.3f, 1.31f),
                color * 0.63f);
            TankDetailGeometry.Part(
                "Painted-Bradley-A2RightBridge",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.765f, 0.3f, -0.015f),
                new Vector3(0.1f, 0.31f, 1.24f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Bradley-A2TowBridge",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.765f, 0.28f, -0.15f),
                new Vector3(0.09f, 0.43f, 1.22f),
                color * 0.6f);
            TankDetailGeometry.Part(
                "Bradley-A2CoaxSlit",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.24f, 0.42f, 1.045f),
                new Vector3(0.1f, 0.12f, 0.06f),
                TankBradleyFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Painted-Bradley-A2Bustle",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.05f, 0.36f, -1.17f),
                new Vector3(1.38f, 0.3f, 0.62f),
                color * 0.61f);
            TankDetailGeometry.Part(
                "Bradley-A2BustleBacking",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.05f, 0.4f, -1.515f),
                new Vector3(1.42f, 0.18f, 0.055f),
                TankBradleyFamilyDetails.Dark());
        }

        private static void AddUkrainianArmor(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tile = 0;
                    tile < 4;
                    tile++)
                {
                    TankDetailGeometry.Part(
                        "Painted-BradleyUA-TurretSideTile",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 1.17f,
                            0.47f,
                            0.53f -
                                tile * 0.42f),
                        new Vector3(0.12f, 0.3f, 0.34f),
                        color * (0.7f -
                            tile % 2 * 0.025f));
                }
            }
            TankDetailGeometry.Part(
                "BradleyUA-BustleRackBacking",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.35f, -1.5f),
                new Vector3(2.18f, 0.24f, 0.055f),
                TankBradleyFamilyDetails.Dark());
        }

        private static void AddM3A3Turret(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-BradleyM3-RingCollar",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.015f, -0.08f),
                new Vector3(0.845f, 0.065f, 0.845f),
                color * 0.6f);
            TankDetailGeometry.Part(
                "Painted-BradleyM3-WeldedCore",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.28f, -0.18f),
                new Vector3(1.6f, 0.5f, 2.06f),
                color * 0.69f);
            TankDetailGeometry.Part(
                "Painted-BradleyM3-RoofFoundation",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.55f, -0.24f),
                new Vector3(1.28f, 0.06f, 1.5f),
                color * 0.78f);
            TankDetailGeometry.Part(
                "BradleyM3-RecessedRoofField",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.59f, -0.27f),
                new Vector3(1.12f, 0.025f, 1.34f),
                TankBradleyFamilyDetails.Dark());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform cheek =
                    TankDetailGeometry.Part(
                        "Painted-BradleyM3-FacetedCheek",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 0.45f,
                            0.3f,
                            0.76f),
                        new Vector3(0.58f, 0.46f, 0.64f),
                        color * 0.73f);
                cheek.localRotation =
                    Quaternion.Euler(
                        -7f,
                        side * 16f,
                        side * -5f);
                TankDetailGeometry.Part(
                    "Painted-BradleyM3-TurretSideCarrier",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.8f,
                        0.288f,
                        -0.38f),
                    new Vector3(0.12f, 0.352f, 1.44f),
                    color * 0.54f);
            }
            TankDetailGeometry.Part(
                "Painted-BradleyM3-Bustle",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.31f, -1.13f),
                new Vector3(1.48f, 0.272f, 0.48f),
                color * 0.61f);
            TankDetailGeometry.Part(
                "BradleyM3-BustleBacking",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.32f, -1.39f),
                new Vector3(1.58f, 0.192f, 0.055f),
                TankBradleyFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Painted-BradleyM3-RightStowageWing",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.91f, 0.344f, -0.43f),
                new Vector3(0.34f, 0.272f, 1.08f),
                color * 0.6f);
            TankDetailGeometry.Part(
                "Painted-BradleyM3-IsuHood",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.3f, 0.472f, 0.28f),
                new Vector3(0.46f, 0.24f, 0.52f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "BradleyM3-IsuLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.3f, 0.476f, 0.55f),
                new Vector3(0.32f, 0.068f, 0.016f),
                TankBradleyFamilyDetails.Lens());
        }
    }
}
