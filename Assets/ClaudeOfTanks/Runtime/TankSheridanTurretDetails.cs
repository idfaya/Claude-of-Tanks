using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSheridanTurretDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            bool tts =
                definition.id == "m551a1_tts";
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);

            AddCommanderCupola(
                turret,
                color,
                roof,
                width);
            AddLoaderHatch(
                turret,
                color,
                roof,
                width);
            AddPeriscopes(
                turret,
                color,
                roof);
            AddSmokeBanks(
                turret,
                color,
                roof,
                width);
            AddAntennas(
                turret,
                roof,
                width);
            TankSheridanFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.115f,
                    0.13f,
                    -0.04f),
                "Sheridan-LoaderMAG",
                false);

            if (tts)
                AddTtsPackage(
                    turret,
                    color,
                    roof,
                    width);
            else
                AddBasePackage(
                    turret,
                    color,
                    roof,
                    width);
        }

        private static void AddCommanderCupola(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            float x = -width * 0.18f;
            float z = -0.25f;
            TankDetailGeometry.Part(
                "Painted-Sheridan-CommanderCupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, roof + 0.08f, z),
                new Vector3(0.43f, 0.16f, 0.41f),
                color * 0.78f);
            for (int block = 0;
                block < 8;
                block++)
            {
                float angle =
                    block * Mathf.PI * 0.25f;
                Transform vision =
                    TankDetailGeometry.Part(
                        "Sheridan-CommanderVisionBlock",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            x + Mathf.Sin(angle) * 0.4f,
                            roof + 0.12f,
                            z + Mathf.Cos(angle) * 0.4f),
                        new Vector3(0.13f, 0.075f, 0.055f),
                        TankSheridanFamilyDetails.Lens());
                vision.localRotation =
                    Quaternion.Euler(
                        0f,
                        angle * Mathf.Rad2Deg,
                        0f);
            }
            TankDetailGeometry.Part(
                "Painted-Sheridan-CommanderHatch",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, roof + 0.19f, z + 0.16f),
                new Vector3(0.3f, 0.035f, 0.24f),
                color * 0.82f)
                .localRotation =
                Quaternion.Euler(-18f, 0f, 0f);
        }

        private static void AddLoaderHatch(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            TankDetailGeometry.Part(
                "Painted-Sheridan-LoaderHatch",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    width * 0.115f,
                    roof + 0.035f,
                    -0.17f),
                new Vector3(0.23f, 0.035f, 0.29f),
                color * 0.82f);
        }

        private static void AddPeriscopes(
            Transform turret,
            Color color,
            float roof)
        {
            Vector3[] centers =
            {
                new Vector3(0.18f, roof + 0.08f, 0.64f),
                new Vector3(0.56f, roof + 0.08f, 0.32f),
                new Vector3(-0.58f, roof + 0.08f, 0.3f)
            };
            for (int index = 0;
                index < centers.Length;
                index++)
            {
                TankSheridanFamilyDetails.AddSight(
                    turret,
                    color,
                    centers[index],
                    new Vector3(0.16f, 0.11f, 0.13f),
                    "Sheridan-Periscope-" + index);
            }
        }

        private static void AddSmokeBanks(
            Transform turret,
            Color color,
            float roof,
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
                            "Painted-Sheridan-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * width *
                                    (0.29f + tube * 0.028f),
                                roof - 0.34f + tube * 0.02f,
                                0.9f - tube * 0.13f),
                            new Vector3(0.045f, 0.17f, 0.045f),
                            color * 0.58f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            56f,
                            0f,
                            side * (22f + tube * 3f));
                    TankDetailGeometry.Part(
                        "Sheridan-SmokeBore",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * width *
                                (0.29f + tube * 0.028f),
                            roof - 0.26f + tube * 0.02f,
                            0.97f - tube * 0.13f),
                        new Vector3(0.028f, 0.018f, 0.028f),
                        TankSheridanFamilyDetails.Gunmetal())
                        .localRotation =
                        launcher.localRotation;
                }
            }
        }

        private static void AddAntennas(
            Transform turret,
            float roof,
            float width)
        {
            Vector3[] seats =
            {
                new Vector3(width * 0.1f, -0.59f, 1.3f),
                new Vector3(width * 0.29f, 0.7f, 1.15f)
            };
            for (int index = 0;
                index < seats.Length;
                index++)
            {
                Vector3 seat = seats[index];
                TankDetailGeometry.Part(
                    "Painted-Sheridan-AntennaBase",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        seat.x,
                        roof + 0.07f,
                        seat.y),
                    new Vector3(0.06f, 0.08f, 0.06f),
                    new Color(0.18f, 0.19f, 0.16f));
                TankDetailGeometry.Part(
                    "Sheridan-Antenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        seat.x,
                        roof + 0.1f + seat.z * 0.5f,
                        seat.y),
                    new Vector3(0.009f, seat.z, 0.009f),
                    TankSheridanFamilyDetails.Gunmetal());
            }
        }

        private static void AddBasePackage(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            TankSheridanFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.18f,
                    0.25f,
                    0.3f),
                "Sheridan-CommanderM2",
                true);
            TankDetailGeometry.Part(
                "Painted-Sheridan-RearStowage",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, roof - 0.18f, -1.2f),
                new Vector3(
                    width * 0.56f,
                    0.3f,
                    0.52f),
                color * 0.7f);
        }

        private static void AddTtsPackage(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            AddRemoteAutocannon(
                turret,
                color,
                roof);
            AddProtectedSearchlight(
                turret,
                color,
                roof,
                width);
            AddOpenBustle(
                turret,
                color,
                roof,
                width);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankSheridanFamilyDetails.AddSight(
                    turret,
                    color,
                    new Vector3(
                        side * width * 0.27f,
                        roof + 0.08f,
                        -0.72f),
                    new Vector3(0.16f, 0.18f, 0.16f),
                    "Sheridan-TTS-WarningHead");
            }
            TankSheridanFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(0.44f, roof + 0.2f, -0.52f),
                new Vector3(0.46f, 0.26f, 0.46f),
                "Sheridan-TTS-RoofElectronics");
        }

        private static void AddRemoteAutocannon(
            Transform turret,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "Painted-Sheridan-TTS-AutocannonStation",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.42f, roof + 0.33f, -0.25f),
                new Vector3(0.46f, 0.34f, 0.48f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Sheridan-TTS-AutocannonMechanism",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.42f, roof + 0.45f, 0.08f),
                new Vector3(0.18f, 0.14f, 0.42f),
                TankSheridanFamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "Sheridan-TTS-AutocannonBarrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.42f, roof + 0.46f, 0.66f),
                new Vector3(0.047f, 0.047f, 0.9f),
                TankSheridanFamilyDetails.Gunmetal());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Sheridan-TTS-AutocannonWorkLight",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -0.42f + side * 0.2f,
                        roof + 0.34f,
                        0f),
                    new Vector3(0.09f, 0.08f, 0.025f),
                    TankSheridanFamilyDetails.Lens());
            }
        }

        private static void AddProtectedSearchlight(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            TankDetailGeometry.Part(
                "Painted-Sheridan-TTS-SearchlightHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.23f,
                    roof - 0.18f,
                    1.32f),
                new Vector3(0.48f, 0.54f, 0.4f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Sheridan-TTS-SearchlightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.23f,
                    roof - 0.18f,
                    1.53f),
                new Vector3(0.35f, 0.4f, 0.025f),
                TankSheridanFamilyDetails.Lens());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Sheridan-TTS-SearchlightYoke",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        width * 0.23f + side * 0.27f,
                        roof - 0.18f,
                        1.34f),
                    new Vector3(0.05f, 0.64f, 0.05f),
                    TankSheridanFamilyDetails.Gunmetal());
            }
        }

        private static void AddOpenBustle(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            TankDetailGeometry.Part(
                "Painted-Sheridan-TTS-BustleCore",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, roof - 0.28f, -1.38f),
                new Vector3(
                    width * 0.62f,
                    0.48f,
                    0.62f),
                color * 0.68f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int rail = 0;
                    rail < 2;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "Sheridan-TTS-BustleSideRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.39f,
                            roof - 0.48f + rail * 0.56f,
                            -1.84f),
                        new Vector3(0.05f, 0.05f, 0.92f),
                        TankSheridanFamilyDetails.Gunmetal());
                }
            }
            for (int post = 0;
                post < 5;
                post++)
            {
                TankDetailGeometry.Part(
                    "Sheridan-TTS-BustlePost",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        (post - 2) * width * 0.195f,
                        roof - 0.2f,
                        -2.26f),
                    new Vector3(0.045f, 0.56f, 0.045f),
                    TankSheridanFamilyDetails.Gunmetal());
            }
            for (int rail = 0;
                rail < 3;
                rail++)
            {
                TankDetailGeometry.Part(
                    "Sheridan-TTS-BustleCrossRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof + 0.08f,
                        -1.55f - rail * 0.35f),
                    new Vector3(
                        width * 0.78f,
                        0.04f,
                        0.04f),
                    TankSheridanFamilyDetails.Gunmetal());
            }
        }
    }
}
