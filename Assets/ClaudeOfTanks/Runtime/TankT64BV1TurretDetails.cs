using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT64BV1TurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddCasting(turret, color);
            AddCrewStations(turret, color);
            AddSights(turret, color);
            AddNsvt(turret, color);
            AddSmokeBank(turret, color);
            AddRearRack(turret, color);
        }

        private static void AddCasting(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T64-CastDome",
                PrimitiveType.Sphere,
                turret,
                new Vector3(0f, 0.29f, -0.08f),
                new Vector3(2.28f, 0.78f, 2.25f),
                color * 0.72f);
            Part(
                "Painted-T64-TurretRace",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.02f, 0f),
                new Vector3(0.86f, 0.1f, 0.86f),
                color * 0.54f);
            Part(
                "Painted-T64-RearBustle",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.32f, -1.04f),
                new Vector3(1.98f, 0.43f, 0.68f),
                color * 0.66f);
            Part(
                "Painted-T64-MantletThroat",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.15f, 1.08f),
                new Vector3(0.56f, 0.3f, 0.26f),
                color * 0.58f);
            Part(
                "T64-MantletBoot",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.16f, 1.22f),
                new Vector3(0.3f, 0.27f, 0.18f),
                TankT64BV1FamilyDetails.Rubber());

            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "T64-EraStandoff",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.1f,
                        0.29f,
                        0.1f),
                    new Vector3(
                        0.24f,
                        0.15f,
                        0.11f),
                    TankT64BV1FamilyDetails.Dark());
                Part(
                    "T64-EraStandoff",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.1f,
                        0.29f,
                        -0.56f),
                    new Vector3(
                        0.3f,
                        0.15f,
                        0.11f),
                    TankT64BV1FamilyDetails.Dark());
            }
        }

        private static void AddCrewStations(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T64-CommanderGallery",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.66f, 0.63f, 0.03f),
                new Vector3(0.46f, 0.3f, 0.78f),
                color * 0.75f);
            Part(
                "Painted-T64-CommanderCupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.67f, 0.82f, -0.02f),
                new Vector3(0.25f, 0.12f, 0.268f),
                color * 0.68f);
            Part(
                "T64-CommanderHatch",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.67f, 0.91f, -0.02f),
                new Vector3(0.212f, 0.035f, 0.212f),
                TankT64BV1FamilyDetails.Dark());

            for (int block = 0;
                block < 3;
                block++)
            {
                float x = -0.5f - block * 0.17f;
                float z = block == 1
                    ? 0.3f
                    : 0.26f;
                Part(
                    "Painted-T64-CommanderPeriscope",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(x, 0.9f, z),
                    new Vector3(0.11f, 0.06f, 0.09f),
                    color * 0.63f);
                Part(
                    "T64-PeriscopeLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(x, 0.925f, z + 0.05f),
                    new Vector3(0.075f, 0.045f, 0.022f),
                    TankT64BV1FamilyDetails.Glass());
            }

            Part(
                "Painted-T64-GunnerHatch",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.48f, 0.58f, -0.14f),
                new Vector3(0.235f, 0.04f, 0.235f),
                color * 0.68f);
            Part(
                "T64-GunnerHatchInset",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.48f, 0.61f, -0.14f),
                new Vector3(0.208f, 0.025f, 0.208f),
                TankT64BV1FamilyDetails.Dark());
        }

        private static void AddSights(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T64-1G42Base",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.44f, 0.5f, 0.92f),
                new Vector3(0.3f, 0.24f, 0.24f),
                color * 0.64f);
            Part(
                "Painted-T64-1G42Tower",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.44f, 0.78f, 1.06f),
                new Vector3(0.28f, 0.34f, 0.36f),
                color * 0.72f);
            Part(
                "T64-1G42Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.44f, 0.8f, 1.25f),
                new Vector3(0.19f, 0.12f, 0.028f),
                TankT64BV1FamilyDetails.Glass());

            Part(
                "Painted-T64-LunaBracket",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.54f, 0.29f, 0.96f),
                new Vector3(0.12f, 0.14f, 0.22f),
                color * 0.58f);
            Part(
                "T64-LunaSearchlight",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.54f, 0.3f, 1.1f),
                new Vector3(0.17f, 0.15f, 0.17f),
                TankT64BV1FamilyDetails.Dark())
                .localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            Part(
                "T64-LunaLens",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.54f, 0.3f, 1.19f),
                new Vector3(0.14f, 0.024f, 0.14f),
                TankT64BV1FamilyDetails.Glass())
                .localRotation =
                Quaternion.Euler(90f, 0f, 0f);

            for (int sight = 0;
                sight < 5;
                sight++)
            {
                float angle =
                    -55f + sight * 27.5f;
                float radians =
                    angle * Mathf.Deg2Rad;
                Vector3 position =
                    new Vector3(
                        Mathf.Sin(radians) * 0.57f,
                        0.64f,
                        Mathf.Cos(radians) * 0.52f);
                Part(
                    "T64-RoofPeriscopeLens",
                    PrimitiveType.Cube,
                    turret,
                    position,
                    new Vector3(0.095f, 0.045f, 0.025f),
                    TankT64BV1FamilyDetails.Glass())
                    .localRotation =
                    Quaternion.Euler(
                        0f,
                        angle,
                        0f);
            }
        }

        private static void AddNsvt(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T64-NsvtPintle",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.62f, 1.04f, 0.34f),
                new Vector3(0.06f, 0.16f, 0.06f),
                color * 0.62f);
            Part(
                "T64-NsvtReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.62f, 1.17f, 0.43f),
                new Vector3(0.16f, 0.16f, 0.42f),
                TankT64BV1FamilyDetails.Dark());
            Transform barrel = Part(
                "T64-NsvtBarrel",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.62f, 1.18f, 0.88f),
                new Vector3(0.025f, 0.42f, 0.025f),
                TankT64BV1FamilyDetails.Dark());
            barrel.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            Part(
                "Painted-T64-NsvtShield",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.62f, 1.12f, 0.23f),
                new Vector3(0.46f, 0.32f, 0.05f),
                color * 0.7f);
            Part(
                "T64-NsvtAmmoBox",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.42f, 1.08f, 0.43f),
                new Vector3(0.18f, 0.22f, 0.28f),
                TankT64BV1FamilyDetails.Dark());
        }

        private static void AddSmokeBank(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T64-SmokeBankBase",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.98f, 0.36f, -0.62f),
                new Vector3(0.4f, 0.1f, 0.26f),
                color * 0.55f);
            for (int tube = 0;
                tube < 4;
                tube++)
            {
                Transform launcher = Part(
                    "T64-902ALauncher",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        -1.04f + tube * 0.06f,
                        0.45f,
                        -0.78f + tube * 0.1f),
                    new Vector3(
                        0.042f,
                        0.14f,
                        0.042f),
                    TankT64BV1FamilyDetails.Dark());
                launcher.localRotation =
                    Quaternion.Euler(
                        65f,
                        0f,
                        -25f);
            }
        }

        private static void AddRearRack(
            Transform turret,
            Color color)
        {
            for (int rail = 0;
                rail < 2;
                rail++)
            {
                Transform rack = Part(
                    "T64-BustleRackRail",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        0f,
                        0.27f + rail * 0.19f,
                        -1.5f - rail * 0.05f),
                    new Vector3(
                        0.017f,
                        1f,
                        0.017f),
                    TankT64BV1FamilyDetails.Dark());
                rack.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f);
            }

            Transform drum = Part(
                "Painted-T64-RackFuelDrum",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.94f, 0.36f, -1.6f),
                new Vector3(0.142f, 0.3f, 0.142f),
                color * 0.58f);
            drum.localRotation =
                Quaternion.Euler(90f, 0f, 0f);

            for (int tube = 0;
                tube < 2;
                tube++)
            {
                Transform opvt = Part(
                    "Painted-T64-OpvtTube",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        -0.9f,
                        0.285f + tube * 0.13f,
                        -1.56f),
                    new Vector3(
                        0.062f,
                        0.33f,
                        0.062f),
                    color * 0.6f);
                opvt.localRotation =
                    Quaternion.Euler(
                        90f,
                        0f,
                        0f);
            }

            Part(
                "T64-AntennaBase",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.1f, 0.6f, -1.06f),
                new Vector3(0.055f, 0.07f, 0.055f),
                TankT64BV1FamilyDetails.Dark());
            Transform whip = Part(
                "T64-RadioWhip",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.1f, 0.86f, -1.06f),
                new Vector3(0.011f, 0.25f, 0.011f),
                TankT64BV1FamilyDetails.Dark());
            whip.localRotation =
                Quaternion.Euler(0f, 0f, -2f);
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                type,
                parent,
                position,
                scale,
                color);
        }
    }
}
