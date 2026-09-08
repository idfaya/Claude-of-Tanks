using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLeopard1A5TurretDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddBvCheeks(
                turret,
                color);
            AddEmes18(
                turret,
                color,
                roof);
            AddCrewStations(
                turret,
                color,
                roof);
            AddSmokeBanks(
                turret,
                color,
                width,
                roof);
            AddShieldedMg3(
                turret,
                color,
                roof);
            AddBustleBasket(
                turret,
                color);
            AddSideRacks(
                turret,
                color,
                width);
            AddAntennas(
                turret,
                color,
                width,
                roof);
        }

        private static void AddBvCheeks(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform front =
                    TankDetailGeometry.Part(
                        "Painted-Leopard1A5-BVCheekFront",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 0.69f,
                            0.41f,
                            0.72f),
                        new Vector3(0.72f, 0.48f, 0.58f),
                        color * 0.78f);
                front.localRotation =
                    Quaternion.Euler(
                        -8f,
                        side * 28f,
                        side * -4f);
                Transform returnPanel =
                    TankDetailGeometry.Part(
                        "Painted-Leopard1A5-BVCheekReturn",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 1.12f,
                            0.46f,
                            -0.29f),
                        new Vector3(0.22f, 0.48f, 1.02f),
                        color * 0.68f);
                returnPanel.localRotation =
                    Quaternion.Euler(
                        -5f,
                        side * 12f,
                        side * -5f);
                TankDetailGeometry.Part(
                    "Leopard1A5-CheekSeam",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.18f,
                        0.52f,
                        -0.28f),
                    new Vector3(0.025f, 0.035f, 0.64f),
                    TankLeopard1A5FamilyDetails.Gunmetal())
                    .localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 14f,
                        0f);
            }
        }

        private static void AddEmes18(
            Transform turret,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-EMES18-Base",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.48f, roof + 0.05f, 0.32f),
                new Vector3(0.46f, 0.1f, 0.44f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-EMES18-Housing",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.48f, roof + 0.18f, 0.32f),
                new Vector3(0.5f, 0.24f, 0.47f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-EMES18-Lid",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.48f, roof + 0.32f, 0.32f),
                new Vector3(0.54f, 0.035f, 0.51f),
                color * 0.76f);
            for (int aperture = -1;
                aperture <= 1;
                aperture += 2)
            {
                TankDetailGeometry.Part(
                    "Leopard1A5-EMES18-Aperture",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0.48f +
                            aperture * 0.09f,
                        roof + 0.18f,
                        0.565f),
                    new Vector3(0.115f, 0.105f, 0.014f),
                    TankLeopard1A5FamilyDetails.Lens());
            }
        }

        private static void AddCrewStations(
            Transform turret,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-CommanderHatch",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.52f, roof + 0.055f, -0.62f),
                new Vector3(0.27f, 0.055f, 0.27f),
                color * 0.74f);
            for (int block = 0;
                block < 8;
                block++)
            {
                float angle =
                    block * Mathf.PI * 0.25f;
                TankDetailGeometry.Part(
                    "Leopard1A5-CommanderVisionBlock",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0.52f +
                            Mathf.Sin(angle) * 0.235f,
                        roof + 0.1f,
                        -0.62f +
                            Mathf.Cos(angle) * 0.235f),
                    new Vector3(0.055f, 0.045f, 0.018f),
                    TankLeopard1A5FamilyDetails.Lens())
                    .localRotation =
                    Quaternion.Euler(
                        0f,
                        angle * Mathf.Rad2Deg,
                        0f);
            }
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-LoaderHatch",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.55f, roof + 0.045f, -0.46f),
                new Vector3(0.24f, 0.045f, 0.24f),
                color * 0.72f);
            for (int scope = 0;
                scope < 3;
                scope++)
            {
                TankDetailGeometry.Part(
                    "Leopard1A5-LoaderPeriscope",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -0.28f +
                            scope * 0.18f,
                        roof + 0.075f,
                        0.28f),
                    new Vector3(0.09f, 0.035f, 0.025f),
                    TankLeopard1A5FamilyDetails.Lens());
            }
        }

        private static void AddSmokeBanks(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-SmokeBank",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.36f,
                        roof - 0.47f,
                        -0.08f),
                    new Vector3(0.08f, 0.28f, 0.42f),
                    color * 0.58f);
                for (int tube = 0;
                    tube < 8;
                    tube++)
                {
                    int row = tube / 4;
                    int column = tube % 4;
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Leopard1A5-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (width * 0.38f +
                                     row * 0.035f),
                                roof - 0.58f +
                                    row * 0.18f,
                                0.15f -
                                    column * 0.14f),
                            new Vector3(0.038f, 0.105f, 0.038f),
                            color * 0.48f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            59f,
                            0f,
                            side * 10f);
                }
            }
        }

        private static void AddShieldedMg3(
            Transform turret,
            Color color,
            float roof)
        {
            Vector3 seat =
                new Vector3(-0.45f, roof + 0.16f, -0.3f);
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-MG3-Bearing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    seat.x,
                    seat.y - 0.09f,
                    seat.z),
                new Vector3(0.14f, 0.065f, 0.14f),
                color * 0.65f);
            TankDetailGeometry.Part(
                "Leopard1A5-MG3-Receiver",
                PrimitiveType.Cube,
                turret,
                seat,
                new Vector3(0.22f, 0.14f, 0.42f),
                TankLeopard1A5FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "Leopard1A5-MG3-Barrel",
                PrimitiveType.Cube,
                turret,
                seat + new Vector3(0f, 0.01f, 0.55f),
                new Vector3(0.032f, 0.032f, 0.72f),
                TankLeopard1A5FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "Leopard1A5-MG3-AmmoBox",
                PrimitiveType.Cube,
                turret,
                seat + new Vector3(-0.17f, -0.02f, -0.03f),
                new Vector3(0.13f, 0.14f, 0.22f),
                TankLeopard1A5FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-MG3-Shield",
                PrimitiveType.Cube,
                turret,
                seat + new Vector3(0f, 0.09f, 0.17f),
                new Vector3(0.38f, 0.3f, 0.045f),
                color * 0.64f);
        }

        private static void AddBustleBasket(
            Transform turret,
            Color color)
        {
            const float rear = -2.84f;
            const float front = -2.24f;
            const float center = -2.54f;
            for (int rail = 0;
                rail < 2;
                rail++)
            {
                TankDetailGeometry.Part(
                    "Leopard1A5-BustleRearRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        0.29f + rail * 0.49f,
                        rear),
                    new Vector3(1.88f, 0.04f, 0.04f),
                    color * 0.42f);
            }
            TankDetailGeometry.Part(
                "Leopard1A5-BustleFloor",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.31f, center),
                new Vector3(1.8f, 0.025f, 0.6f),
                color * 0.4f);
            for (int post = 0;
                post < 7;
                post++)
            {
                float x = -0.86f + post * (1.72f / 6f);
                TankDetailGeometry.Part(
                    "Leopard1A5-BustlePost",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(x, 0.535f, rear),
                    new Vector3(0.03f, 0.49f, 0.03f),
                    color * 0.4f);
                TankDetailGeometry.Part(
                    "Leopard1A5-BustleFloorRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(x, 0.37f, center),
                    new Vector3(0.034f, 0.034f, 0.57f),
                    color * 0.38f)
                    .localRotation =
                    Quaternion.Euler(-6f, 0f, 0f);
            }
            for (int cargo = -1;
                cargo <= 1;
                cargo++)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-BustleCargo",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        cargo * 0.55f,
                        0.52f,
                        center),
                    new Vector3(0.46f, 0.3f, 0.44f),
                    color * (0.58f +
                        (cargo + 1) * 0.035f));
            }
            TankDetailGeometry.Part(
                "Leopard1A5-BustleFrontRail",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.74f, front),
                new Vector3(1.84f, 0.035f, 0.1f),
                color * 0.42f);
        }

        private static void AddSideRacks(
            Transform turret,
            Color color,
            float width)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int rail = 0;
                    rail < 3;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "Leopard1A5-SideRackRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.325f,
                            0.22f + rail * 0.13f,
                            -1.68f),
                        new Vector3(0.035f, 0.035f, 0.64f),
                        color * 0.4f);
                }
                for (int post = -1;
                    post <= 1;
                    post += 2)
                {
                    TankDetailGeometry.Part(
                        "Leopard1A5-SideRackPost",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.325f,
                            0.35f,
                            -1.68f + post * 0.3f),
                        new Vector3(0.035f, 0.3f, 0.035f),
                        color * 0.4f);
                }
            }
        }

        private static void AddAntennas(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-AntennaBase",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.235f,
                        roof + 0.04f,
                        -1.42f),
                    new Vector3(0.055f, 0.055f, 0.055f),
                    color * 0.55f);
                Transform whip =
                    TankDetailGeometry.Part(
                        "Leopard1A5-Antenna",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * width * 0.235f,
                            roof + 0.4f,
                            -1.42f),
                        new Vector3(0.014f, 0.36f, 0.014f),
                        TankLeopard1A5FamilyDetails.Gunmetal());
                whip.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * -1.5f);
            }
        }
    }
}
