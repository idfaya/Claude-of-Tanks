using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT80TurretDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            string id = definition?.id;
            if (!TankT80FamilyDetails.Supports(id) &&
                !TankT80ExtendedFamilyDetails.UsesT80CastTurret(id))
                return;

            HideRenderer(turret.Find("Turret"));

            Transform root =
                new GameObject("T80-TurretPresentationRoot").transform;
            root.SetParent(turret, false);
            BuildCastDome(root, color, id == "t80bv");
            AddMantletAndCheeks(root, color, id);
            AddRoofEquipment(root, color, id);
            AddBustleAndRack(root, color, id);
            AddVariantArmor(root, color, id);
            AddSmokeBanks(root, color, id);
            AddGun(turret, color, id);
            AddMarkings(root, definition);
        }

        private static void BuildCastDome(
            Transform root,
            Color color,
            bool curvedNormals)
        {
            float[] radii =
            {
                1.44f, 1.465f, 1.435f, 1.30f, 1.19f,
                1.05f, 0.86f, 0.60f, 0.02f
            };
            float[] heights =
            {
                0.06f, 0.366f, 0.402f, 0.4965f, 0.5325f,
                0.5595f, 0.618f, 0.654f, 0.6675f
            };
            Transform dome = TankLatheShapeFactory.Build(
                "Painted-T80-CastDome",
                root,
                radii,
                heights,
                30,
                0.88f,
                color * 0.66f,
                curvedNormals ? 1.60f : 0f,
                curvedNormals ? 0.62f : 1f);
            dome.localPosition = V(0f, 0f, 0.22f);

            Cylinder(
                "Painted-T80-TurretRace",
                root,
                V(0f, 0.025f, 0.03f),
                0.82f,
                0.88f,
                0.08f,
                30,
                TankShapeAxis.Y,
                color * 0.48f);
        }

        private static void AddMantletAndCheeks(
            Transform root,
            Color color,
            string id)
        {
            bool bv = id == "t80bv";
            float seatLift = bv ? 0.0465f : 0f;
            if (bv)
            {
                Transform collar = Cylinder(
                    "Painted-T80BV-CastGunCollar",
                    root,
                    V(0f, 0.20f + seatLift, 1.42f),
                    0.50f,
                    0.46f,
                    0.36f,
                    18,
                    TankShapeAxis.Z,
                    color * 0.47f);
                collar.localScale = V(0.92f, 0.64f, 1f);
                Box("Painted-T80BV-MantletCover", root,
                    V(0f, 0.24f + seatLift, 1.70f),
                    V(0.30f, 0.20f, 0.14f), color * 0.55f);
                Box("Painted-T80BV-MantletHood", root,
                    V(0f, 0.22f + seatLift, 1.44f),
                    V(0.56f, 0.26f, 0.36f), color * 0.58f);
                Box("T80BV-MantletFold", root,
                    V(0f, 0.31f + seatLift, 1.616f),
                    V(0.55f, 0.02f, 0.018f), Dark());
                Cylinder("T80BV-CoaxPort", root,
                    V(0.17f, 0.26f + seatLift, 1.59f),
                    0.022f, 0.022f, 0.05f, 10,
                    TankShapeAxis.Z, Dark());
                Cylinder("T80BV-CoaxPortWasher", root,
                    V(0.17f, 0.26f + seatLift, 1.615f),
                    0.032f, 0.032f, 0.012f, 12,
                    TankShapeAxis.Z, Dark());
                Cylinder("Painted-T80BV-GunnerSightDrum", root,
                    V(0.55f, 0.40f + seatLift, 0.96f),
                    0.13f, 0.13f, 0.24f, 16,
                    TankShapeAxis.Z, color * 0.54f);
                Cylinder("T80BV-GunnerSightLens", root,
                    V(0.55f, 0.40f + seatLift, 1.09f),
                    0.09f, 0.09f, 0.02f, 16,
                    TankShapeAxis.Z, Glass());
            }
            else
            {
                Transform boot = Cylinder(
                    "Painted-T80-CastGunCollar",
                    root,
                    V(0f, -0.10f, 0.75f),
                    0.50f,
                    0.465f,
                    0.40f,
                    18,
                    TankShapeAxis.Z,
                    color * 0.45f);
                boot.localScale = V(0.92f, 1f, 1f);
                Box("Painted-T80-MantletHood", root,
                    V(0f, 0.34f, 1.44f),
                    V(1.30f, 0.32f, 0.50f), color * 0.58f);
                Box("Painted-T80-MantletStep", root,
                    V(0f, 0.545f, 1.155f),
                    V(0.90f, 0.12f, 0.24f), color * 0.62f);
                Box("Painted-T80-MantletNose", root,
                    V(0f, 0.26f, 1.84f),
                    V(0.30f, 0.40f, 0.28f), color * 0.56f);
                Cylinder("T80-CoaxPort", root,
                    V(0.30f, 0.30f, 1.658f),
                    0.022f, 0.022f, 0.06f, 10,
                    TankShapeAxis.Z, Dark());
                Cylinder("T80-CoaxPortWasher", root,
                    V(0.30f, 0.30f, 1.683f),
                    0.032f, 0.032f, 0.012f, 12,
                    TankShapeAxis.Z, Dark());
                Cylinder("Painted-T80-LunaSearchlight", root,
                    V(0.72f, 0.35f, 1.62f),
                    0.13f, 0.13f, 0.24f, 16,
                    TankShapeAxis.Z, color * 0.54f);
                Cylinder("T80-LunaLens", root,
                    V(0.72f, 0.35f, 1.75f),
                    0.09f, 0.09f, 0.02f, 16,
                    TankShapeAxis.Z, Glass());
            }

            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T80-CheekBlock", root,
                    V(side * 1.00f, 0.22f + seatLift, 1.08f),
                    V(0.34f, 0.30f, 0.46f),
                    color * 0.60f).localRotation =
                    Quaternion.Euler(0f, side * 24f, 0f);
                Box("Painted-T80-CheekShoulder", root,
                    V(side * (bv ? 1.28f : 1.27f),
                        (bv ? 0.16f : 0.23f) + seatLift,
                        bv ? 0.55f : 0.72f),
                    V(bv ? 0.30f : 0.28f,
                        bv ? 0.26f : 0.38f,
                        bv ? 0.40f : 0.40f),
                    color * 0.58f).localRotation =
                    Quaternion.Euler(0f, side * (bv ? 41f : 38f), 0f);
                Box("Painted-T80-FlankSlab", root,
                    V(side * (bv ? 1.10f : 1.07f),
                        0.14f + seatLift,
                        -0.08f),
                    V(bv ? 0.40f : 0.34f,
                        0.34f,
                        1.10f),
                    color * 0.57f).localRotation =
                    Quaternion.Euler(0f, side * 5f, 0f);
                Box("T80-FlankCap", root,
                    V(side * 1.10f, 0.335f + seatLift, -0.10f),
                    V(0.36f, 0.05f, 0.90f),
                    Dark()).localRotation =
                    Quaternion.Euler(0f, side * 5f, 0f);
            }
        }

        private static void AddRoofEquipment(
            Transform root,
            Color color,
            string id)
        {
            bool bv = id == "t80bv";
            float roofTop = bv ? 0.714f : 0.6675f;
            if (!bv)
            {
                Box("Painted-T80-CrownPlate", root,
                    V(0f, 0.6975f, 0.125f),
                    V(1.24f, 0.045f, 1.25f),
                    color * 0.70f);
                Box("Painted-T80-LeftCrownShelf", root,
                    V(-1.06f, 0.6725f, 0.10f),
                    V(0.36f, 0.05f, 0.90f),
                    color * 0.66f);
                Box("Painted-T80-RearCrownCap", root,
                    V(-0.445f, 0.6325f, -0.68f),
                    V(0.83f, 0.08f, 0.40f),
                    color * 0.68f);
            }

            AddCupola(root, "T80-Commander", V(0.52f, roofTop + 0.03f, -0.42f),
                0.31f, 0.33f, color);
            AddCupola(root, "T80-Loader", V(-0.48f, roofTop + 0.015f, -0.34f),
                0.255f, 0.275f, color);

            float[,] periscopes =
            {
                { 0.18f, -0.20f, 25f },
                { 0.32f, -0.07f, 13f },
                { 0.48f, 0.00f, -2f },
                { 0.66f, -0.10f, -16f },
                { -0.26f, -0.10f, -16f },
                { -0.60f, -0.09f, 17f }
            };
            for (int index = 0; index < periscopes.GetLength(0); index++)
            {
                float x = periscopes[index, 0];
                float z = periscopes[index, 1];
                float yaw = periscopes[index, 2];
                Box("Painted-T80-RoofPeriscopeBase", root,
                    V(x, roofTop + 0.01f, z),
                    V(0.14f, 0.10f, 0.10f),
                    color * 0.60f).localRotation =
                    Quaternion.Euler(0f, yaw, 0f);
                Box("T80-RoofPeriscopeLens", root,
                    V(x, roofTop + 0.06f, z + 0.045f),
                    V(0.084f, 0.038f, 0.014f),
                    Glass()).localRotation =
                    Quaternion.Euler(0f, yaw, 0f);
            }

            Cylinder("Painted-T80-Ventilator", root,
                V(-0.05f, roofTop + 0.042f, -0.73f),
                0.14f, 0.16f, 0.075f, 14,
                TankShapeAxis.Y, color * 0.58f);
            Box("Painted-T80-GunnerSightDoghouse", root,
                V(-0.45f, roofTop + 0.095f, 0.40f),
                V(0.42f, 0.22f, 0.44f), color * 0.60f);
            Box("T80-GunnerSightLens", root,
                V(-0.45f, roofTop + 0.165f, 0.635f),
                V(0.28f, 0.12f, 0.022f), Glass());
            if (!bv)
            {
                Box("Painted-T80-LeftSightHead", root,
                    V(-0.325f, roofTop + 0.06f, -0.56f),
                    V(0.18f, 0.20f, 0.18f), color * 0.58f);
                Box("T80-LeftSightGlass", root,
                    V(-0.325f, roofTop + 0.07f, -0.462f),
                    V(0.13f, 0.10f, 0.020f), Glass());
            }

            Transform antenna = TankFittingShapeFactory.BuildAntennaWhip(
                "T80",
                root,
                V(-0.78f, roofTop + 0.085f, -0.86f),
                1.24f,
                0.011f,
                -0.025f,
                color * 0.40f,
                Dark());
            antenna.name = "T80-RadioWhipAssembly";

            Transform mg = TankPintleMachineGunFactory.Build(
                "T80-NSVT",
                root,
                V(0.52f, roofTop + 0.105f, -0.44f),
                Quaternion.Euler(0f, id == "t80" ? -3f : 4f, 0f),
                TankMachineGunClass.Nsvt,
                bv ? 0.84f : 0.88f,
                -0.08f,
                true,
                TankMachineGunShield.Standard,
                true,
                Dark(),
                color * 0.50f);
            mg.name = "T80-NSVTStation";
        }

        private static void AddCupola(
            Transform root,
            string prefix,
            Vector3 center,
            float radiusX,
            float radiusZ,
            Color color)
        {
            Transform collar = Cylinder(
                "Painted-" + prefix + "CupolaCollar",
                root,
                center,
                radiusX,
                radiusZ,
                0.15f,
                18,
                TankShapeAxis.Y,
                color * 0.62f);
            collar.localScale = V(1f, 1f, radiusZ / radiusX);
            Cylinder("Painted-" + prefix + "CupolaHatch",
                root,
                center + V(0f, 0.085f, 0f),
                radiusX * 0.82f,
                radiusZ * 0.82f,
                0.04f,
                16,
                TankShapeAxis.Y,
                color * 0.68f).localScale =
                V(1f, 1f, radiusZ / radiusX);
            Box(prefix + "HatchHinge", root,
                center + V(0f, 0.108f, -0.19f),
                V(0.24f, 0.055f, 0.075f),
                Dark());
        }

        private static void AddBustleAndRack(
            Transform root,
            Color color,
            string id)
        {
            bool bv = id == "t80bv";
            Box("Painted-T80-RearBustle",
                root,
                V(bv ? 0f : 0.03f, bv ? 0.505f : 0.4775f, -1.245f),
                V(bv ? 1.76f : 1.70f, 0.45f, 0.31f),
                color * 0.61f);
            Box("Painted-T80-BustleTail",
                root,
                V(bv ? 0f : 0.03f, bv ? 0.575f : 0.5475f, bv ? -1.515f : -1.44f),
                V(bv ? 1.76f : 1.70f, bv ? 0.32f : 0.32f, bv ? 0.23f : 0.16f),
                color * 0.58f);
            Box("T80-BustleBackRail", root,
                V(0f, 0.43f, -1.72f),
                V(2.02f, 0.055f, 0.055f), Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Box("T80-BustleSideRail", root,
                    V(side * 1.00f, 0.46f, -1.38f),
                    V(0.055f, 0.055f, 0.76f), Dark())
                    .localRotation =
                    Quaternion.Euler(0f, side * 6f, 0f);
                Box("T80-BustleUpright", root,
                    V(side * 1.00f, 0.36f, -1.70f),
                    V(0.055f, 0.28f, 0.055f), Dark());
                Box("Painted-T80-BustlePack", root,
                    V(side * 0.64f, 0.53f, -1.47f),
                    V(0.42f, 0.20f, side < 0 ? 0.38f : 0.48f),
                    color * 0.54f);
            }
            Cylinder("T80-RearStowageTube", root,
                V(0f, 0.38f, -1.06f),
                0.07f, 0.07f, 1.50f, 12,
                TankShapeAxis.X, Dark());
            if (id == "t80b")
            {
                Box("Painted-T80B-BustleStowageRow", root,
                    V(-0.35f, 0.6425f, -0.92f),
                    V(0.72f, 0.13f, 0.28f), color * 0.62f);
                Box("Painted-T80B-TailBin", root,
                    V(-0.55f, 0.6175f, -1.575f),
                    V(0.30f, 0.18f, 0.09f), color * 0.58f);
            }
        }

        private static void AddVariantArmor(
            Transform root,
            Color color,
            string id)
        {
            if (id == "t80")
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Transform shoulder = Box(
                        "Painted-T80-AppliqueShoulder",
                        root,
                        V(side * 1.22f, 0.36f, 0.55f),
                        V(0.30f, 0.22f, 0.27f),
                        color * 0.56f);
                    shoulder.localRotation =
                        Quaternion.Euler(-8f, side * 41f, 0f);
                    Box("T80-AppliqueCap", root,
                        V(side * 1.22f, 0.480f, 0.55f),
                        V(0.23f, 0.020f, 0.21f), Dark())
                        .localRotation = shoulder.localRotation;
                }
                return;
            }

            for (int side = -1; side <= 1; side += 2)
            for (int index = 0; index < 2; index++)
            {
                Transform module = Box(
                    id == "t80bv"
                        ? "Painted-T80BV-CheekKontaktCarrier"
                        : "Painted-T80B-BrowApplique",
                    root,
                    V(side * (1.18f + index * 0.08f),
                        0.39f - index * 0.015f,
                        0.65f - index * 0.27f),
                    V(0.29f, 0.22f, 0.25f),
                    color * 0.56f);
                module.localRotation =
                    Quaternion.Euler(
                        -9f,
                        side * (39f + index * 9f),
                        0f);
                Box(
                    id == "t80bv"
                        ? "T80BV-CheekKontaktCap"
                        : "T80B-BrowAppliqueCap",
                    root,
                    V(side * (1.18f + index * 0.08f),
                        0.51f - index * 0.015f,
                        0.65f - index * 0.27f),
                    V(0.22f, 0.02f, 0.19f),
                    Dark()).localRotation = module.localRotation;
            }

            if (id != "t80bv") return;

            for (int side = -1; side <= 1; side += 2)
            {
                for (int row = 0; row < 2; row++)
                for (int column = 0; column < 3; column++)
                {
                    Transform cassette = Box(
                        "Painted-T80BV-K1ChevronTile",
                        root,
                        V(side * (0.36f + column * 0.30f),
                            0.22f + row * 0.19f,
                            1.18f - column * 0.25f - row * 0.07f),
                        V(0.26f, 0.13f, 0.18f),
                        color * 0.50f);
                    cassette.localRotation =
                        Quaternion.Euler(
                            -7f,
                            side * (34f + column * 8f),
                            0f);
                    Box("T80BV-K1ChevronTileCap", root,
                        cassette.localPosition + V(0f, 0.076f, 0f),
                        V(0.20f, 0.014f, 0.13f),
                        Dark()).localRotation =
                        cassette.localRotation;
                }
                for (int index = 0; index < 6; index++)
                {
                    Transform flank = Box(
                        "Painted-T80BV-K1FlankCassette",
                        root,
                        V(side * (1.24f + index * 0.03f),
                            0.15f - index * 0.006f,
                            0.46f - index * 0.235f),
                        V(0.22f, 0.14f, 0.22f),
                        color * 0.52f);
                    flank.localRotation =
                        Quaternion.Euler(
                            -4f,
                            side * (38f + index * 6f),
                            0f);
                    Box("T80BV-K1FlankCassetteCap", root,
                        flank.localPosition + V(0f, 0.076f, 0f),
                        V(0.17f, 0.012f, 0.15f),
                        Dark()).localRotation =
                        flank.localRotation;
                }
            }
        }

        private static void AddSmokeBanks(
            Transform root,
            Color color,
            string id)
        {
            if (id == "t80")
            {
                AddSmokeBank(root, color, -1, 5, 0.30f, 36);
                AddSmokeBank(root, color, 1, 4, 0.30f, 38);
                return;
            }
            if (id == "t80b")
            {
                AddSmokeBank(root, color, -1, 4, 0.37f, 40);
                AddSmokeBank(root, color, 1, 4, 0.37f, 42);
                return;
            }
            AddSmokeBank(root, color, -1, 7, 0.24f, 50);
            AddSmokeBank(root, color, 1, 5, 0.24f, 55);
        }

        private static void AddSmokeBank(
            Transform root,
            Color color,
            int side,
            int count,
            float z,
            int seed)
        {
            Transform shoe = Box(
                "Painted-T80-SmokeBankShoe",
                root,
                V(side * 1.17f, 0.42f, z),
                V(0.24f, 0.22f, count > 5 ? 0.58f : 0.46f),
                color * 0.53f);
            shoe.localRotation =
                Quaternion.Euler(0f, 0f, -side * 9f);
            Transform bank = TankFittingShapeFactory.BuildSmokeBank(
                "T80",
                root,
                V(side * 1.18f, 0.54f, z),
                Quaternion.Euler(0f, side * 57f, 0f),
                count,
                0.039f,
                0.25f,
                -0.42f,
                0.30f,
                0.64f,
                0.080f,
                color * 0.44f,
                Dark());
            bank.name = "T80-SmokeBank-" + seed;
        }

        private static void AddGun(
            Transform turret,
            Color color,
            string id)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            HideRenderer(gun);
            Transform root = new GameObject("T80-2A46M1Assembly").transform;
            root.SetParent(gun, false);
            Vector3 scale = gun.localScale;
            Vector3 sourcePivot = V(
                0f,
                id == "t80bv" ? 0.235f : 0.285f,
                0.60f);
            root.localPosition = new Vector3(
                scale.x == 0f
                    ? 0f
                    : (sourcePivot.x - gun.localPosition.x) / scale.x,
                scale.y == 0f
                    ? 0f
                    : (sourcePivot.y - gun.localPosition.y) / scale.y,
                scale.z == 0f
                    ? 0f
                    : (sourcePivot.z - gun.localPosition.z) / scale.z);
            root.localScale = new Vector3(
                scale.x == 0f ? 1f : 1f / scale.x,
                scale.y == 0f ? 1f : 1f / scale.y,
                scale.z == 0f ? 1f : 1f / scale.z);

            Cylinder("Painted-T80-2A46M1Saddle", root,
                V(0f, 0f, 0.02f),
                0.15f, 0.15f, 0.40f, 16,
                TankShapeAxis.X, color * 0.48f);
            Cylinder("Painted-T80-2A46M1RootSleeve", root,
                V(0f, -0.04f, 0.86f),
                0.28f, 0.128f, 0.62f, 18,
                TankShapeAxis.Z, color * 0.48f);
            Cylinder("T80-GunBootFold", root,
                V(0f, -0.045f, 0.84f),
                0.215f, 0.215f, 0.035f, 16,
                TankShapeAxis.Z, Dark());

            float muzzle = id == "t80bv"
                ? 5.22f
                : id == "t80b"
                    ? 5.73f
                    : 5.67f;
            Tube(root, color,
                "Painted-T80-2A46M1ThermalSleeve",
                0.55f,
                2.03f,
                0.128f,
                -0.040f);
            Tube(root, color,
                "Painted-T80-2A46M1ThermalSleeve",
                2.03f,
                2.78f,
                0.130f,
                -0.048f);
            Tube(root, color,
                "Painted-T80-2A46M1ForwardTube",
                2.78f,
                muzzle,
                0.128f,
                -0.054f);
            foreach (float z in new[]
            {
                3.60f, 4.40f, Mathf.Min(5.10f, muzzle - 0.18f)
            })
            {
                Cylinder("T80-2A46M1SleeveRing", root,
                    V(0f, -0.054f, z),
                    0.132f, 0.132f, 0.04f, 18,
                    TankShapeAxis.Z, Dark());
            }
            Box("T80-2A46M1ClampPlate", root,
                V(-0.005f, -0.056f,
                    id == "t80bv" ? 2.775f : 2.995f),
                V(0.37f, 0.014f,
                    id == "t80bv" ? 4.45f : 4.89f),
                Dark());
            Box("T80-2A46M1CrestFin", root,
                V(0f, -0.054f, 2.405f),
                V(0.022f, 0.30f, 0.75f),
                Dark());
            Cylinder("Painted-T80-2A46M1FumeExtractor", root,
                V(0f, -0.054f, 2.58f),
                0.148f, 0.136f, 0.46f, 18,
                TankShapeAxis.Z, color * 0.46f);
            Cylinder("Painted-T80-2A46M1MuzzleCollar", root,
                V(0f, -0.032f, muzzle - 0.13f),
                0.118f, 0.118f, 0.12f, 16,
                TankShapeAxis.Z, color * 0.46f);
            TankShapeFactory.TorusPart(
                "T80-2A46M1MuzzleBore",
                root,
                0.105f,
                0.022f,
                18,
                Dark(),
                8).localPosition =
                V(0f, -0.054f, muzzle + 0.002f);
            root.Find("T80-2A46M1MuzzleBore").localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void Tube(
            Transform root,
            Color color,
            string name,
            float start,
            float end,
            float radius,
            float y)
        {
            Cylinder(name, root,
                V(0f, y, (start + end) * 0.5f),
                radius, radius, end - start, 24,
                TankShapeAxis.Z, color * 0.46f);
        }

        private static void AddMarkings(
            Transform root,
            VehicleDefinition definition)
        {
            string number = definition?.visual?.number;
            number = NumericMarking(number);
            if (string.IsNullOrEmpty(number)) return;
            TankTacticalNumberFactory.BuildPair(
                "T80",
                root,
                number,
                0.25f,
                V(1.48f, 0.26f, -0.30f),
                Quaternion.Euler(0f, 90f, 0f),
                V(-1.48f, 0.26f, -0.30f),
                Quaternion.Euler(0f, -90f, 0f));
        }

        private static string NumericMarking(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            char[] digits = new char[value.Length];
            int count = 0;
            for (int index = 0; index < value.Length; index++)
            {
                char c = value[index];
                if (c >= '0' && c <= '9')
                    digits[count++] = c;
            }
            return count == 0
                ? null
                : new string(digits, 0, count);
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            return part;
        }

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float top,
            float bottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                top,
                bottom,
                length,
                segments,
                axis,
                color);
            part.localPosition = position;
            return part;
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer =
                part == null ? null : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        private static Color Dark()
        {
            return new Color(0.055f, 0.062f, 0.052f);
        }

        private static Color Glass()
        {
            return new Color(0.025f, 0.075f, 0.085f);
        }

        private static Vector3 V(
            float x = 0f,
            float y = 0f,
            float z = 0f)
        {
            return new Vector3(x, y, z);
        }
    }
}
