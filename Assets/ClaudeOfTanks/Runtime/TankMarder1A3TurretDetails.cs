using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMarder1A3TurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddCastBody(
                turret,
                color);
            AddExternalCarriage(
                turret,
                color);
            AddMilan(
                turret,
                color);
            AddOpticsAndCommanderStation(
                turret,
                color);
            AddServiceBoxesAndBasket(
                turret,
                color);
            AddSmokeBanks(
                turret,
                color);
            AddRoofMachineGun(
                turret,
                color);
            AddAntennas(
                turret,
                color);
        }

        private static void AddCastBody(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Marder-TurretCollar",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.03f, 0f),
                new Vector3(0.78f, 0.05f, 0.78f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "Painted-Marder-LowCastBody",
                PrimitiveType.Sphere,
                turret,
                new Vector3(0f, 0.2825f, 0.03f),
                new Vector3(1.44f, 0.565f, 1.71f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Painted-Marder-CastFrontLip",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.28f, 0.57f),
                new Vector3(0.58f, 0.18f, 0.58f),
                color * 0.68f)
                .localRotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f);
        }

        private static void AddExternalCarriage(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Marder-TrunnionTower",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.24f,
                        0.62f,
                        0.22f),
                    new Vector3(0.14f, 0.3f, 0.3f),
                    color * 0.64f);
            }
            TankDetailGeometry.Part(
                "Painted-Marder-CarriageBeam",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.9f, 0.2f),
                new Vector3(0.6f, 0.14f, 0.42f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Painted-Marder-CarriageRiser",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.76f, 0.21f),
                new Vector3(0.2f, 0.2f, 0.34f),
                color * 0.63f);
            TankDetailGeometry.Part(
                "Painted-Marder-CarriagePedestal",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.645f, -0.255f),
                new Vector3(0.34f, 0.45f, 0.67f),
                color * 0.59f);
        }

        private static void AddMilan(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Marder-MilanSeat",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.44f, 0.66f, -0.14f),
                new Vector3(0.14f, 0.36f, 0.3f),
                color * 0.61f);
            AddAxialCylinder(
                "Marder-MilanTube",
                turret,
                0.115f,
                1.05f,
                -0.1f,
                TankMarder1A3FamilyDetails.Dark(),
                0.55f,
                0.92f);
            AddAxialCylinder(
                "Painted-Marder-MilanMouthRing",
                turret,
                0.122f,
                0.03f,
                0.43f,
                color * 0.52f,
                0.55f,
                0.92f);
            TankDetailGeometry.Part(
                "Marder-MilanSightGrip",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.42f, 0.9f, -0.36f),
                new Vector3(0.1f, 0.16f, 0.22f),
                TankMarder1A3FamilyDetails.Gunmetal());
        }

        private static void AddOpticsAndCommanderStation(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Marder-PeriZ11Tower",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.32f, 0.7f, -0.1f),
                new Vector3(0.2f, 0.4f, 0.22f),
                color * 0.64f);
            TankDetailGeometry.Part(
                "Marder-PeriZ11Hood",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.32f, 0.86f, 0.02f),
                new Vector3(0.16f, 0.06f, 0.03f),
                TankMarder1A3FamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Marder-PeriZ11Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.32f, 0.855f, 0.038f),
                new Vector3(0.13f, 0.035f, 0.014f),
                TankMarder1A3FamilyDetails.Lens());
            TankDetailGeometry.Part(
                "Painted-Marder-GunnerSightHood",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.16f, 0.575f, 0.44f),
                new Vector3(0.16f, 0.14f, 0.16f),
                color * 0.61f);
            TankDetailGeometry.Part(
                "Marder-GunnerSightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.16f, 0.605f, 0.525f),
                new Vector3(0.11f, 0.045f, 0.015f),
                TankMarder1A3FamilyDetails.Lens());

            TankDetailGeometry.Part(
                "Painted-Marder-CommanderCupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.28f, 0.525f, -0.36f),
                new Vector3(0.25f, 0.045f, 0.25f),
                color * 0.67f);
            TankDetailGeometry.Part(
                "Marder-CommanderCupolaRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.28f, 0.585f, -0.36f),
                new Vector3(0.27f, 0.012f, 0.27f),
                TankMarder1A3FamilyDetails.Dark());
            Transform lid =
                TankDetailGeometry.Part(
                    "Painted-Marder-CommanderLid",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(-0.28f, 0.625f, -0.36f),
                    new Vector3(0.32f, 0.05f, 0.34f),
                    color * 0.7f);
            lid.localRotation =
                Quaternion.Euler(0f, -5.73f, 0f);
            for (int scope = -1;
                scope <= 1;
                scope++)
            {
                TankDetailGeometry.Part(
                    "Marder-CommanderPeriscope",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -0.28f +
                            scope * 0.085f,
                        0.665f,
                        -0.185f),
                    new Vector3(0.068f, 0.042f, 0.022f),
                    TankMarder1A3FamilyDetails.Lens())
                    .localRotation =
                    Quaternion.Euler(0f, -5.73f, 0f);
            }
        }

        private static void AddServiceBoxesAndBasket(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float depth =
                    side < 0
                        ? 0.46f
                        : 0.38f;
                TankDetailGeometry.Part(
                    "Painted-Marder-ServiceBox",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.585f,
                        0.27f,
                        -0.14f),
                    new Vector3(0.16f, 0.22f, depth),
                    color * 0.6f)
                    .localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 2.58f);
                TankDetailGeometry.Part(
                    "Marder-ServiceBoxLid",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.675f,
                        0.29f,
                        -0.14f),
                    new Vector3(
                        0.025f,
                        0.13f,
                        depth - 0.1f),
                    TankMarder1A3FamilyDetails.Dark());
            }
            TankDetailGeometry.Part(
                "Painted-Marder-RearEquipmentWall",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.3f, -0.8f),
                new Vector3(0.92f, 0.26f, 0.22f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "Marder-RearEquipmentBacking",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.31f, -0.92f),
                new Vector3(0.74f, 0.13f, 0.035f),
                TankMarder1A3FamilyDetails.Dark());
            for (int rail = 0;
                rail < 3;
                rail++)
            {
                TankDetailGeometry.Part(
                    "Marder-RearBasketRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        0.4f +
                            rail * 0.095f,
                        -1.01f),
                    new Vector3(0.9f, 0.025f, 0.025f),
                    TankMarder1A3FamilyDetails.Gunmetal());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Marder-RearBasketPost",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.43f,
                        0.495f,
                        -1.01f),
                    new Vector3(0.025f, 0.22f, 0.025f),
                    TankMarder1A3FamilyDetails.Gunmetal());
            }
        }

        private static void AddSmokeBanks(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Marder-SmokeCollar",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.6f,
                        0.3f,
                        -0.44f),
                    new Vector3(0.12f, 0.2f, 0.34f),
                    color * 0.58f)
                    .localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 5.73f);
                for (int tube = 0;
                    tube < 3;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Marder-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (0.62f +
                                     tube * 0.04f),
                                0.4f +
                                    tube * 0.055f,
                                -0.58f +
                                    tube * 0.105f),
                            new Vector3(0.044f, 0.15f, 0.044f),
                            color * 0.49f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            68f,
                            0f,
                            side * 8f);
                }
            }
        }

        private static void AddRoofMachineGun(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Marder-RoofMgMount",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.3f, 0.67f, -0.48f),
                new Vector3(0.08f, 0.045f, 0.08f),
                color * 0.55f);
            TankDetailGeometry.Part(
                "Marder-RoofMgReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.3f, 0.77f, -0.38f),
                new Vector3(0.12f, 0.12f, 0.28f),
                TankMarder1A3FamilyDetails.Gunmetal());
            AddAxialCylinder(
                "Marder-RoofMgBarrel",
                turret,
                0.018f,
                0.62f,
                0.07f,
                TankMarder1A3FamilyDetails.Dark(),
                0.3f,
                0.79f);
            TankDetailGeometry.Part(
                "Marder-RoofMgMagazine",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.39f, 0.79f, -0.38f),
                new Vector3(0.065f, 0.035f, 0.065f),
                TankMarder1A3FamilyDetails.Gunmetal())
                .localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    90f);
        }

        private static void AddAntennas(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Marder-AntennaPot",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * 0.38f,
                        0.59f,
                        -0.58f),
                    new Vector3(0.055f, 0.045f, 0.055f),
                    color * 0.54f);
                Transform antenna =
                    TankDetailGeometry.Part(
                        "Marder-RadioAntenna",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * 0.38f,
                            0.845f,
                            -0.58f),
                        new Vector3(0.009f, 0.21f, 0.009f),
                        TankMarder1A3FamilyDetails.Gunmetal());
                antenna.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 4f);
            }
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
                    new Vector3(
                        radius,
                        length * 0.5f,
                        radius),
                    color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
