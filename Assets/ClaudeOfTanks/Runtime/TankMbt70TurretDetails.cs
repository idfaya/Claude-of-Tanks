using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMbt70TurretDetails
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
            AddRoofCassettes(
                turret,
                color,
                roof);
            AddCrewStations(
                turret,
                color,
                roof);
            AddGunnerSight(
                turret,
                color,
                roof);
            AddSmokeBanks(
                turret,
                color,
                width);
            AddTurretEra(
                turret,
                color);
        }

        private static void AddRoofCassettes(
            Transform turret,
            Color color,
            float roof)
        {
            float[] zs =
            {
                -1.34f,
                -1.82f,
                -2.3f
            };
            float[] heights =
            {
                0.15f,
                0.13f,
                0.12f
            };
            for (int cassette = 0;
                cassette < zs.Length;
                cassette++)
            {
                float y = cassette < 2
                    ? roof + heights[cassette] * 0.5f
                    : 0.66f;
                Transform door =
                    TankDetailGeometry.Part(
                        "Painted-MBT70-BustleRoofDoor",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(0.42f, y, zs[cassette]),
                        new Vector3(
                            0.78f,
                            heights[cassette],
                            0.34f),
                        color * (0.74f -
                            cassette * 0.035f));
                door.localRotation =
                    Quaternion.Euler(-2f, 0f, 0f);
                TankDetailGeometry.Part(
                    "MBT70-BustleDoorSeam",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0.42f,
                        y + heights[cassette] * 0.52f,
                        zs[cassette] + 0.13f),
                    new Vector3(0.68f, 0.018f, 0.025f),
                    TankMbt70FamilyDetails.Gunmetal());
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    TankDetailGeometry.Part(
                        "MBT70-BustleDoorLatch",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            0.42f + side * 0.31f,
                            y + heights[cassette] * 0.52f,
                            zs[cassette] - 0.11f),
                        new Vector3(0.035f, 0.025f, 0.11f),
                        TankMbt70FamilyDetails.Gunmetal());
                }
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "MBT70-BustleGrabPost",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0.42f + side * 0.43f,
                        0.79f,
                        -2.66f),
                    new Vector3(0.035f, 0.22f, 0.035f),
                    TankMbt70FamilyDetails.Gunmetal());
            }
            TankDetailGeometry.Part(
                "MBT70-BustleGrabRail",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.42f, 0.9f, -2.66f),
                new Vector3(0.9f, 0.035f, 0.035f),
                TankMbt70FamilyDetails.Gunmetal());
        }

        private static void AddCrewStations(
            Transform turret,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "Painted-MBT70-CommanderCupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.62f, roof + 0.12f, -0.72f),
                new Vector3(0.32f, 0.12f, 0.32f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Painted-MBT70-LoaderCupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.58f, roof + 0.1f, -0.6f),
                new Vector3(0.29f, 0.1f, 0.29f),
                color * 0.69f);
            for (int block = 0;
                block < 6;
                block++)
            {
                float angle =
                    block * Mathf.PI / 3f;
                TankDetailGeometry.Part(
                    "MBT70-CommanderVisionBlock",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0.62f +
                            Mathf.Sin(angle) * 0.3f,
                        roof + 0.25f,
                        -0.72f +
                            Mathf.Cos(angle) * 0.3f),
                    new Vector3(0.1f, 0.08f, 0.08f),
                    TankMbt70FamilyDetails.Lens())
                    .localRotation =
                    Quaternion.Euler(
                        0f,
                        angle * Mathf.Rad2Deg,
                        0f);
            }
            TankDetailGeometry.Part(
                "Painted-MBT70-CommanderStation",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.62f, roof + 0.43f, -0.66f),
                new Vector3(0.42f, 0.28f, 0.42f),
                color * 0.64f);
            TankDetailGeometry.Part(
                "MBT70-CommanderOptic",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.62f, roof + 0.45f, -0.435f),
                new Vector3(0.24f, 0.13f, 0.025f),
                TankMbt70FamilyDetails.Lens());
            AddCommanderM2(
                turret,
                color,
                roof);
        }

        private static void AddCommanderM2(
            Transform turret,
            Color color,
            float roof)
        {
            Vector3 seat =
                new Vector3(0.62f, roof + 0.58f, -0.66f);
            TankDetailGeometry.Part(
                "Painted-MBT70-M2-Bearing",
                PrimitiveType.Cylinder,
                turret,
                seat + new Vector3(0f, -0.11f, 0f),
                new Vector3(0.14f, 0.06f, 0.14f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "MBT70-M2-Receiver",
                PrimitiveType.Cube,
                turret,
                seat,
                new Vector3(0.2f, 0.16f, 0.42f),
                TankMbt70FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "MBT70-M2-Barrel",
                PrimitiveType.Cube,
                turret,
                seat + new Vector3(0f, 0.01f, 0.55f),
                new Vector3(0.045f, 0.045f, 0.74f),
                TankMbt70FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "MBT70-M2-AmmoBox",
                PrimitiveType.Cube,
                turret,
                seat + new Vector3(-0.17f, -0.02f, -0.05f),
                new Vector3(0.13f, 0.15f, 0.24f),
                TankMbt70FamilyDetails.Gunmetal());
        }

        private static void AddGunnerSight(
            Transform turret,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "MBT70-GunnerSightGasket",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.64f, roof + 0.0125f, 0.02f),
                new Vector3(0.3f, 0.025f, 0.3f),
                TankMbt70FamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Painted-MBT70-GunnerSightHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.64f, roof + 0.2175f, 0.02f),
                new Vector3(0.34f, 0.435f, 0.34f),
                color * 0.63f);
            TankDetailGeometry.Part(
                "MBT70-GunnerSightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.64f, roof + 0.235f, 0.195f),
                new Vector3(0.22f, 0.17f, 0.025f),
                TankMbt70FamilyDetails.Lens());
            TankDetailGeometry.Part(
                "Painted-MBT70-AuxSightHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(-1.02f, roof + 0.12f, -0.88f),
                new Vector3(0.28f, 0.2f, 0.32f),
                color * 0.6f);
            TankDetailGeometry.Part(
                "MBT70-AuxSightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-1.02f, roof + 0.15f, -0.705f),
                new Vector3(0.17f, 0.1f, 0.02f),
                TankMbt70FamilyDetails.Lens());
        }

        private static void AddSmokeBanks(
            Transform turret,
            Color color,
            float width)
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
                            "Painted-MBT70-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (width * 0.35f +
                                     tube * 0.025f),
                                0.55f +
                                    tube * 0.025f,
                                -0.38f -
                                    tube * 0.16f),
                            new Vector3(0.045f, 0.14f, 0.045f),
                            color * 0.52f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            67f,
                            0f,
                            side * 22f);
                }
                TankDetailGeometry.Part(
                    "MBT70-SignatureSmokeCradle",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.31f,
                        0.34f,
                        0.05f),
                    new Vector3(0.2f, 0.15f, 0.3f),
                    TankMbt70FamilyDetails.Gunmetal())
                    .localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 33f,
                        0f);
                Transform signature =
                    TankDetailGeometry.Part(
                        "Painted-MBT70-SignatureSmokeCanister",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * 1.36f,
                            0.51f,
                            0.1f),
                        new Vector3(0.105f, 0.21f, 0.105f),
                        color * 0.48f);
                signature.localRotation =
                    Quaternion.Euler(
                        62.5f,
                        side * 33f,
                        0f);
            }
        }

        private static void AddTurretEra(
            Transform turret,
            Color color)
        {
            float[] xs =
            {
                1.42f,
                1.37f,
                1.22f
            };
            float[] zs =
            {
                -0.08f,
                0.34f,
                0.76f
            };
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int station = 0;
                    station < xs.Length;
                    station++)
                {
                    Transform cassette =
                        TankDetailGeometry.Part(
                            "Painted-MBT70-TurretEraCassette",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side * xs[station],
                                0.34f,
                                zs[station]),
                            new Vector3(0.2f, 0.24f, 0.34f),
                            color * 0.76f);
                    cassette.localRotation =
                        Quaternion.Euler(
                            -5.7f,
                            side *
                                (5f + station * 6f),
                            side * 4.6f);
                    TankDetailGeometry.Part(
                        "MBT70-TurretEraSeam",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side *
                                (xs[station] + 0.035f),
                            0.36f,
                            zs[station]),
                        new Vector3(0.15f, 0.025f, 0.26f),
                        TankMbt70FamilyDetails.Gunmetal());
                }
            }
        }

    }
}
