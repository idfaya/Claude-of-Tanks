using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankUpiorTurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddDrum(turret, color);
            AddCrewStations(turret, color);
            AddSensorTower(turret, color);
            AddRoofEquipment(turret, color);
        }

        private static void AddDrum(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Upior-TurretRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.005f, 0.02f),
                new Vector3(0.82f, 0.04f, 0.88f),
                color * 0.55f);
            TankDetailGeometry.Part(
                "Painted-Upior-FacetedDrum",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.16f, 0.02f),
                new Vector3(0.84f, 0.15f, 0.86f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Painted-Upior-ShoulderRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.355f, 0.02f),
                new Vector3(0.62f, 0.045f, 0.83f),
                color * 0.74f);
            TankDetailGeometry.Part(
                "Painted-Upior-Crown",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.418f, 0.02f),
                new Vector3(0.6f, 0.035f, 0.62f),
                color * 0.76f);
            TankDetailGeometry.Part(
                "Painted-Upior-CrownPlate",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.44f, 0.02f),
                new Vector3(0.72f, 0.03f, 0.72f),
                color * 0.78f);
            TankDetailGeometry.Part(
                "Painted-Upior-MantletSaddle",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.2f, 0.72f),
                new Vector3(0.68f, 0.28f, 0.32f),
                color * 0.61f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Upior-TrunnionCheek",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.3f, 0.16f, 0.72f),
                    new Vector3(0.1f, 0.22f, 0.26f),
                    color * 0.58f);
            }
            TankDetailGeometry.Part(
                "Painted-Upior-RecoilHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.05f, 0.8f),
                new Vector3(0.4f, 0.16f, 0.22f),
                color * 0.56f);
        }

        private static void AddCrewStations(
            Transform turret,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-Upior-CommanderCupola",
                turret,
                0.21f,
                0.05f,
                0.34f,
                0.44f,
                -0.2f,
                color * 0.68f);
            AddVerticalCylinder(
                "Painted-Upior-GunnerHatch",
                turret,
                0.19f,
                0.045f,
                -0.36f,
                0.435f,
                -0.28f,
                color * 0.68f);
            TankDetailGeometry.Part(
                "Painted-Upior-TknStalk",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.34f, 0.52f, -0.04f),
                new Vector3(0.13f, 0.08f, 0.13f),
                color * 0.59f);
            TankDetailGeometry.Part(
                "Upior-TknLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.34f, 0.545f, 0.03f),
                new Vector3(0.09f, 0.028f, 0.014f),
                TankUpiorFamilyDetails.Lens());
        }

        private static void AddSensorTower(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Upior-SensorPost",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.3f, 0.58f, -0.52f),
                new Vector3(0.16f, 0.36f, 0.16f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Upior-SensorArm",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.3f, 0.8f, -0.43f),
                new Vector3(0.16f, 0.12f, 0.32f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Painted-Upior-SensorHead",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.3f, 0.86f, -0.32f),
                new Vector3(0.32f, 0.26f, 0.32f),
                color * 0.65f);
            TankDetailGeometry.Part(
                "Upior-SensorAperture",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.3f, 0.875f, -0.132f),
                new Vector3(0.2f, 0.075f, 0.015f),
                TankUpiorFamilyDetails.Lens());
            TankDetailGeometry.Part(
                "Upior-SensorCable",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.475f, 0.8f, -0.32f),
                new Vector3(0.03f, 0.06f, 0.28f),
                TankUpiorFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Painted-Upior-AtgmSaddle",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.3f, 0.985f, -0.32f),
                new Vector3(0.14f, 0.055f, 0.28f),
                color * 0.59f);
            AddAxialCylinder(
                "Upior-AtgmTube",
                turret,
                0.07f,
                0.92f,
                0.06f,
                TankUpiorFamilyDetails.Dark(),
                -0.3f,
                1.042f);
            AddAxialCylinder(
                "Painted-Upior-AtgmFrontCap",
                turret,
                0.077f,
                0.03f,
                0.525f,
                color * 0.55f,
                -0.3f,
                1.042f);
            AddAxialCylinder(
                "Upior-AtgmMouth",
                turret,
                0.058f,
                0.02f,
                0.548f,
                Color.black,
                -0.3f,
                1.042f);
        }

        private static void AddRoofEquipment(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Upior-RearRackRail",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.16f, -0.78f),
                new Vector3(0.88f, 0.07f, 0.045f),
                TankUpiorFamilyDetails.Gunmetal());
            for (int rail = -1;
                rail <= 1;
                rail++)
            {
                TankDetailGeometry.Part(
                    "Upior-StowageRack",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(rail * 0.32f, 0.24f, -0.84f),
                    new Vector3(0.035f, 0.16f, 0.24f),
                    TankUpiorFamilyDetails.Gunmetal());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < 3;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Upior-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (0.42f +
                                     tube * 0.07f),
                                0.26f +
                                    tube * 0.035f,
                                0.42f -
                                    tube * 0.06f),
                            new Vector3(0.04f, 0.1f, 0.04f),
                            color * 0.48f);
                    launcher.localRotation =
                        Quaternion.Euler(64f, 0f, side * 16f);
                }
                AddVerticalCylinder(
                    "Painted-Upior-AntennaBase",
                    turret,
                    0.06f,
                    0.04f,
                    side * 0.3f,
                    0.49f,
                    -0.68f,
                    color * 0.52f);
                TankDetailGeometry.Part(
                    "Upior-RadioAntenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.3f, 0.84f, -0.68f),
                    new Vector3(0.01f, 0.35f, 0.01f),
                    TankUpiorFamilyDetails.Dark());
            }
            TankDetailGeometry.Part(
                "Painted-Upior-RoofMgMount",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.36f, 0.48f, -0.6f),
                new Vector3(0.15f, 0.04f, 0.15f),
                color * 0.56f);
            TankDetailGeometry.Part(
                "Upior-RoofMgReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.36f, 0.56f, -0.52f),
                new Vector3(0.12f, 0.1f, 0.28f),
                TankUpiorFamilyDetails.Gunmetal());
            AddAxialCylinder(
                "Upior-RoofMgBarrel",
                turret,
                0.016f,
                0.58f,
                -0.04f,
                TankUpiorFamilyDetails.Dark(),
                -0.36f,
                0.56f);
        }

        private static void AddVerticalCylinder(
            string name,
            Transform parent,
            float radius,
            float height,
            float x,
            float y,
            float z,
            Color color)
        {
            TankDetailGeometry.Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                new Vector3(x, y, z),
                new Vector3(radius, height, radius),
                color);
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
                    new Vector3(radius, length * 0.5f, radius),
                    color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
