using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT14TurretDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddMoldedCrown(
                turret,
                color,
                roof);
            AddBustle(
                turret,
                color,
                roof);
            AddCheekSensors(
                turret,
                color,
                roof);
            AddAfganitRing(
                turret,
                color);
            AddSmokeBanks(
                turret,
                color,
                roof);
            AddDistributedElectronics(
                turret,
                color,
                roof);
        }

        private static void AddMoldedCrown(
            Transform turret,
            Color color,
            float roof)
        {
            float[] widths =
            {
                0.48f,
                1.3f,
                1.9f,
                1.84f,
                1.8f
            };
            float[] stations =
            {
                1.48f,
                1.18f,
                0.72f,
                0.16f,
                -0.32f
            };
            for (int course = 0;
                course < widths.Length;
                course++)
            {
                Transform crown =
                    TankDetailGeometry.Part(
                        "Painted-T14-MoldedCrownCourse",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            0f,
                            roof + 0.025f +
                                course * 0.006f,
                            stations[course]),
                        new Vector3(
                            widths[course],
                            0.07f,
                            course == 0
                                ? 0.35f
                                : 0.54f),
                        color * (0.72f +
                            course * 0.025f));
                if (course == 1)
                {
                    crown.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            0f);
                }
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform ridge =
                    TankDetailGeometry.Part(
                        "T14-CrownFacetRidge",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 0.44f,
                            roof + 0.067f,
                            0.94f),
                        new Vector3(0.025f, 0.02f, 1.02f),
                        TankT14FamilyDetails.Dark());
                ridge.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 22f,
                        0f);
                TankDetailGeometry.Part(
                    "T14-KnuckleSeam",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.42f,
                        0.34f,
                        0.55f),
                    new Vector3(0.02f, 0.02f, 0.95f),
                    TankT14FamilyDetails.Dark());
            }
        }

        private static void AddBustle(
            Transform turret,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "Painted-T14-BustleFront",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.56f, -0.93f),
                new Vector3(2.04f, 0.55f, 0.86f),
                color * 0.76f);
            TankDetailGeometry.Part(
                "Painted-T14-BustleRear",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.59f, -1.61f),
                new Vector3(1.9f, 0.5f, 0.54f),
                color * 0.7f);
            Transform tail =
                TankDetailGeometry.Part(
                    "Painted-T14-BustleTail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(0f, 0.72f, -2.06f),
                    new Vector3(1.76f, 0.24f, 0.5f),
                    color * 0.66f);
            tail.localRotation =
                Quaternion.Euler(-8f, 0f, 0f);
            TankDetailGeometry.Part(
                "T14-BustleRoofSeam",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, roof + 0.015f, -1.3f),
                new Vector3(1.72f, 0.018f, 0.03f),
                TankT14FamilyDetails.Dark());
        }

        private static void AddCheekSensors(
            Transform turret,
            Color color,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform pocket =
                    TankDetailGeometry.Part(
                        "Painted-T14-CheekSensorPocket",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * 0.82f,
                            roof - 0.27f,
                            1.28f),
                        new Vector3(0.47f, 0.43f, 0.1f),
                        color * 0.46f);
                pocket.localRotation =
                    Quaternion.Euler(
                        -9f,
                        side * 31f,
                        0f);
                float[] vertical =
                {
                    -0.11f,
                    0.02f,
                    0.13f
                };
                for (int lens = 0;
                    lens < vertical.Length;
                    lens++)
                {
                    float lensX =
                        side *
                        (lens == 0
                            ? 0.72f
                            : 0.86f);
                    TankDetailGeometry.Part(
                        "T14-CheekSensorLens",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            lensX,
                            roof - 0.27f +
                                vertical[lens],
                            1.345f),
                        new Vector3(
                            lens == 0 ? 0.2f : 0.11f,
                            lens == 0 ? 0.2f : 0.09f,
                            0.018f),
                        TankT14FamilyDetails.Lens())
                        .localRotation =
                        Quaternion.Euler(
                            -9f,
                            side * 31f,
                            0f);
                }
                TankDetailGeometry.Part(
                    "Painted-T14-CheekController",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.19f,
                        0.44f,
                        -0.24f),
                    new Vector3(0.28f, 0.26f, 0.04f),
                    color * 0.58f)
                    .localRotation =
                    Quaternion.Euler(
                        6f,
                        side * 149f,
                        0f);
            }
        }

        private static void AddAfganitRing(
            Transform turret,
            Color color)
        {
            for (int bank = 0;
                bank < 5;
                bank++)
            {
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    float x =
                        side *
                        (1.24f - bank * 0.13f);
                    float z =
                        bank == 0
                            ? 1.12f
                            : 1.52f -
                              bank * 0.42f;
                    Transform tube =
                        TankDetailGeometry.Part(
                            "T14-AfganitLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(x, 0.1f, z),
                            new Vector3(0.05f, 0.15f, 0.05f),
                            TankT14FamilyDetails.Dark());
                    tube.localRotation =
                        Quaternion.Euler(
                            90f + 14f,
                            side *
                                (29f +
                                 bank * 10f),
                            0f);
                    Transform cap =
                        TankDetailGeometry.Part(
                            "T14-AfganitLauncherCap",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                x,
                                0.13f,
                                z + 0.12f),
                            new Vector3(0.036f, 0.012f, 0.036f),
                            color * 0.28f);
                    cap.localRotation =
                        tube.localRotation;
                }
            }
        }

        private static void AddSmokeBanks(
            Transform turret,
            Color color,
            float roof)
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
                            "Painted-T14-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (0.72f +
                                     tube * 0.09f),
                                roof + 0.01f -
                                    tube * 0.02f,
                                -0.68f),
                            new Vector3(0.035f, 0.15f, 0.035f),
                            color * 0.52f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            side * 7f);
                }
            }
        }

        private static void AddDistributedElectronics(
            Transform turret,
            Color color,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-T14-ShoulderCamera",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.61f,
                        roof + 0.06f,
                        0.57f),
                    new Vector3(0.2f, 0.12f, 0.27f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "T14-ShoulderCameraLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.61f,
                        roof + 0.07f,
                        0.716f),
                    new Vector3(0.098f, 0.046f, 0.014f),
                    TankT14FamilyDetails.Lens());
                TankDetailGeometry.Part(
                    "Painted-T14-SideApsController",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.12f,
                        0.61f,
                        0.31f),
                    new Vector3(0.14f, 0.17f, 0.18f),
                    color * 0.58f);
                TankDetailGeometry.Part(
                    "T14-SideApsLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.198f,
                        0.63f,
                        0.31f),
                    new Vector3(0.014f, 0.075f, 0.085f),
                    TankT14FamilyDetails.Lens());
                TankDetailGeometry.Part(
                    "Painted-T14-RearObservationCamera",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.57f,
                        roof + 0.055f,
                        -0.45f),
                    new Vector3(0.16f, 0.11f, 0.2f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "T14-RearObservationLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.57f,
                        roof + 0.06f,
                        -0.558f),
                    new Vector3(0.075f, 0.04f, 0.012f),
                    TankT14FamilyDetails.Lens());
            }
        }
    }
}
