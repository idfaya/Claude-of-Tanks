using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBwp1TurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddBody(turret, color);
            AddStations(turret, color);
            AddSensorHead(turret, color);
            AddSideEquipment(turret, color);
            AddRoofEquipment(turret, color);
        }

        private static void AddBody(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bwp1-TurretRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.1f, -0.12f),
                new Vector3(0.96f, 0.22f, 1.08f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "Painted-Bwp1-LowTurret",
                PrimitiveType.Sphere,
                turret,
                new Vector3(0f, 0.34f, 0.02f),
                new Vector3(1.72f, 0.7f, 1.9f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Painted-Bwp1-RearRoof",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.55f, -0.54f),
                new Vector3(1.25f, 0.18f, 0.62f),
                color * 0.65f);
            TankDetailGeometry.Part(
                "Painted-Bwp1-Mantlet",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.3f, 0.93f),
                new Vector3(0.5f, 0.36f, 0.26f),
                color * 0.6f);
        }

        private static void AddStations(
            Transform turret,
            Color color)
        {
            float[] xs = { -0.38f, 0.34f };
            float[] zs = { -0.3f, -0.42f };
            float[] radii = { 0.22f, 0.18f };
            for (int index = 0;
                index < xs.Length;
                index++)
            {
                AddVerticalCylinder(
                    "Painted-Bwp1-CrewCupola",
                    turret,
                    radii[index],
                    0.075f,
                    xs[index],
                    0.705f,
                    zs[index],
                    color * 0.66f);
                AddVerticalCylinder(
                    "Bwp1-CupolaRing",
                    turret,
                    radii[index] * 0.8f,
                    0.018f,
                    xs[index],
                    0.752f,
                    zs[index],
                    TankBmp2FamilyDetails.Gunmetal());
                TankDetailGeometry.Part(
                    "Painted-Bwp1-CrewHatch",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        xs[index],
                        0.79f,
                        zs[index]),
                    new Vector3(
                        radii[index] * 1.42f,
                        0.055f,
                        radii[index] * 1.48f),
                    color * 0.71f);
            }
            TankDetailGeometry.Part(
                "Painted-Bwp1-ServiceLid",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.02f, 0.735f, -0.66f),
                new Vector3(0.42f, 0.055f, 0.32f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Bwp1-ServiceLatch",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.02f, 0.769f, -0.49f),
                new Vector3(0.3f, 0.018f, 0.045f),
                TankBmp2FamilyDetails.Gunmetal());
        }

        private static void AddSensorHead(
            Transform turret,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-Bwp1-SensorPedestal",
                turret,
                0.28f,
                0.09f,
                0.28f,
                0.73f,
                -0.18f,
                color * 0.61f);
            TankDetailGeometry.Part(
                "Painted-Bwp1-SensorHead",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.28f, 0.98f, -0.04f),
                new Vector3(0.54f, 0.42f, 0.46f),
                color * 0.64f);
            TankDetailGeometry.Part(
                "Bwp1-SensorLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.28f, 1f, 0.205f),
                new Vector3(0.34f, 0.18f, 0.025f),
                TankBmp2FamilyDetails.Lens());
            AddAxialCylinder(
                "Bwp1-SensorMast",
                turret,
                0.055f,
                0.64f,
                0.44f,
                TankBmp2FamilyDetails.Dark(),
                0.28f,
                1.11f);
            TankDetailGeometry.Part(
                "Painted-Bwp1-SensorBridge",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.025f, 0.84f, -0.36f),
                new Vector3(0.37f, 0.34f, 0.26f),
                color * 0.61f);
        }

        private static void AddSideEquipment(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 2;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Bwp1-TurretSidePanel",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side *
                                (0.76f +
                                 panel * 0.11f),
                            0.48f,
                            0.36f -
                                panel * 0.44f),
                        new Vector3(0.12f, 0.24f, 0.34f),
                        color * 0.66f);
                }
                TankDetailGeometry.Part(
                    "Painted-Bwp1-FlankSight",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.64f, 0.67f, -0.51f),
                    new Vector3(0.24f, 0.18f, 0.32f),
                    color * 0.59f);
                TankDetailGeometry.Part(
                    "Bwp1-FlankSightLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.64f, 0.7f, -0.33f),
                    new Vector3(0.15f, 0.08f, 0.024f),
                    TankBmp2FamilyDetails.Lens());
            }
            TankDetailGeometry.Part(
                "Bwp1-RearRackRail",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.49f, -0.88f),
                new Vector3(1.32f, 0.09f, 0.055f),
                TankBmp2FamilyDetails.Gunmetal());
            for (int rail = -1;
                rail <= 1;
                rail++)
            {
                TankDetailGeometry.Part(
                    "Bwp1-StowageRack",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(rail * 0.48f, 0.56f, -0.96f),
                    new Vector3(0.035f, 0.23f, 0.34f),
                    TankBmp2FamilyDetails.Gunmetal());
            }
        }

        private static void AddRoofEquipment(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < 4;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Bwp1-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (0.58f +
                                     tube * 0.075f),
                                0.57f +
                                    tube * 0.03f,
                                0.05f -
                                    tube * 0.08f),
                            new Vector3(0.04f, 0.11f, 0.04f),
                            color * 0.48f);
                    launcher.localRotation =
                        Quaternion.Euler(64f, 0f, side * 16f);
                }
                AddVerticalCylinder(
                    "Painted-Bwp1-AntennaBase",
                    turret,
                    0.08f,
                    0.055f,
                    side * 0.66f,
                    0.74f,
                    -0.72f,
                    color * 0.53f);
                TankDetailGeometry.Part(
                    "Bwp1-RadioAntenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.66f, 1.13f, -0.72f),
                    new Vector3(0.012f, 0.38f, 0.012f),
                    TankBmp2FamilyDetails.Dark());
            }
            TankDetailGeometry.Part(
                "Painted-Bwp1-RoofMgMount",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.42f, 0.79f, -0.26f),
                new Vector3(0.17f, 0.04f, 0.17f),
                color * 0.57f);
            TankDetailGeometry.Part(
                "Bwp1-RoofMgReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.42f, 0.88f, -0.18f),
                new Vector3(0.13f, 0.12f, 0.32f),
                TankBmp2FamilyDetails.Gunmetal());
            AddAxialCylinder(
                "Bwp1-RoofMgBarrel",
                turret,
                0.018f,
                0.75f,
                0.29f,
                TankBmp2FamilyDetails.Dark(),
                -0.42f,
                0.88f);
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
