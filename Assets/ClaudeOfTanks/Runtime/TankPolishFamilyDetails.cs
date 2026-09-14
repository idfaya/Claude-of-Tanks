using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPolishFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "pt91m" ||
                id == "t72m1_jaguar" ||
                id == "pt91_twardy" ||
                id == "pl01" ||
                id == "pl01_105";
        }

        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            if (definition == null || !Supports(definition.id)) return;

            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideRenderer(turret.Find("Turret"));
            HideRenderer(turret.Find("Gun"));
            HideByPrefix(root, "Armor-");
            HideByPrefix(root, "Soviet-");
            HideByPrefix(root, "Painted-Soviet-");

            if (definition.id == "pl01" ||
                definition.id == "pl01_105")
            {
                BuildPl01(root, turret, definition, color);
                return;
            }

            BuildPt91(root, turret, definition, color);
        }

        private static void BuildPt91(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            bool pendekar = definition.id == "pt91m";
            bool jaguar = definition.id == "t72m1_jaguar";
            string prefix = pendekar
                ? "PT91M"
                : jaguar ? "T72M1Jaguar" : "PT91Twardy";
            Transform body = NewRoot(root, prefix + "-HullPresentationRoot");
            TankHullLoftShapeFactory.Build(
                "Painted-" + prefix + "-HullLoft",
                body,
                jaguar
                    ? Curve(
                        -3.26f, 0.96f,
                        -2.94f, 1.42f,
                        -1.70f, 1.46f,
                        0.60f, 1.44f,
                        1.30f, 1.40f,
                        2.10f, 1.19f,
                        2.90f, 0.99f,
                        3.66f, 0.87f)
                    : Curve(
                        -3.41f, 1.30f,
                        -3.04f, 1.46f,
                        -2.52f, pendekar ? 1.50f : 1.56f,
                        -0.80f, 1.48f,
                        1.40f, 1.47f,
                        2.30f, 1.34f,
                        3.10f, 1.22f,
                        3.52f, 1.02f),
                Curve(
                    jaguar ? -3.26f : -3.41f,
                    jaguar ? 0.80f : 0.84f,
                    -2.70f, jaguar ? 0.43f : 0.43f,
                    2.30f, 0.43f,
                    3.05f, 0.56f,
                    jaguar ? 3.66f : 3.52f,
                    jaguar ? 0.80f : 0.78f),
                Curve(
                    jaguar ? -3.26f : -3.41f,
                    1.60f,
                    2.60f, 1.60f,
                    3.18f, 1.30f,
                    jaguar ? 3.66f : 3.52f,
                    jaguar ? 0.94f : 1.02f),
                Curve(
                    -3.41f, 0.96f,
                    2.50f, 0.96f,
                    3.52f, 0.80f),
                Curve(
                    -3.41f, 1.14f,
                    -2.62f, 1.18f,
                    2.42f, 1.18f,
                    3.52f, 0.92f),
                color * 0.66f);
            AddPt91FendersAndSkirts(body, prefix, color, pendekar);
            AddPt91Glacis(body, prefix, color, pendekar);
            AddPt91DeckAndRear(body, prefix, color, pendekar);
            BuildPt91Turret(turret, definition, prefix, color, pendekar);
            AddPt91Gun(turret, prefix, color, definition);
        }

        private static void AddPt91FendersAndSkirts(
            Transform root,
            string prefix,
            Color color,
            bool pendekar)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "Painted-" + prefix + "-BowFender",
                    root,
                    V(side * 1.42f, 1.08f, 3.30f),
                    V(0.62f, 0.14f, 0.42f),
                    color * 0.58f);
                Box(
                    prefix + "-FenderNoseRubber",
                    root,
                    V(side * 1.38f, 0.92f, 3.52f),
                    V(0.58f, 0.16f, 0.05f),
                    Rubber());
                Box(
                    "Painted-" + prefix + "-LongFenderRun",
                    root,
                    V(side * 1.70f, 1.20f, 0.25f),
                    V(0.16f, 0.06f, 5.62f),
                    color * 0.55f);
                for (int panel = 0; panel < 7; panel++)
                {
                    Box(
                        "Painted-" + prefix + "-RubberSkirtPanel",
                        root,
                        V(side * 1.74f, 0.88f, -2.05f + panel * 0.70f),
                        V(0.050f, 0.38f, 0.62f),
                        panel < 3 ? color * 0.48f : Rubber());
                }
            int eraPanels = pendekar ? 4 : prefix == "T72M1Jaguar" ? 7 : 3;
                for (int panel = 0; panel < eraPanels; panel++)
                {
                    Box(
                        "Painted-" + prefix + "-ERAWASkirtCassette",
                        root,
                        V(side * 1.79f, 1.06f, 2.30f - panel * 0.54f),
                        V(0.065f, 0.40f, 0.46f),
                        color * 0.50f);
                }
            }
        }

        private static void AddPt91Glacis(
            Transform root,
            string prefix,
            Color color,
            bool pendekar)
        {
            int columns = pendekar ? 6 : 9;
            float start = -(columns - 1) * 0.145f;
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    if (!pendekar &&
                        row == 2 &&
                        column >= 3 &&
                        column <= 5)
                    {
                        continue;
                    }
                    Transform tile = Box(
                        "Painted-" + prefix + "-ERAWAGlacisTile",
                        root,
                        V(start + column * 0.29f,
                            1.32f - row * 0.055f,
                            2.04f + row * 0.25f),
                        V(0.27f, 0.055f, 0.22f),
                        color * 0.50f);
                    tile.localRotation =
                        Quaternion.Euler(-16f, 0f, 0f);
                    Box(
                        prefix + "-ERAWAGlacisTileSeam",
                        root,
                        tile.localPosition + V(0f, 0.032f, 0f),
                        V(0.21f, 0.010f, 0.15f),
                        Dark()).localRotation =
                        tile.localRotation;
                }
            }
            Box(
                "Painted-" + prefix + "-SplashRidge",
                root,
                V(0f, 1.34f, 2.55f),
                V(2.24f, 0.045f, 0.15f),
                color * 0.58f);
            TankFittingShapeFactory.BuildTowCable(
                prefix + "-BowTowCable",
                root,
                new[]
                {
                    V(-1.20f, 1.33f, 1.92f),
                    V(-0.28f, 1.42f, 1.58f),
                    V(0.92f, 1.34f, 1.92f)
                },
                0.020f,
                24,
                6,
                Dark());
        }

        private static void AddPt91DeckAndRear(
            Transform root,
            string prefix,
            Color color,
            bool pendekar)
        {
            Box(
                prefix + "-EngineDeckGrille",
                root,
                V(0f, 1.50f, -1.42f),
                V(1.56f, 0.026f, 0.72f),
                Dark());
            for (int rib = 0; rib < 5; rib++)
            {
                Box(
                    prefix + "-EngineDeckLouvre",
                    root,
                    V(0f, 1.522f, -1.10f - rib * 0.15f),
                    V(1.48f, 0.020f, 0.045f),
                    Detail());
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "Painted-" + prefix + "-RearPowerpackHump",
                    root,
                    V(side * 0.82f, 1.55f, -2.58f),
                    V(0.50f, 0.12f, 0.96f),
                    color * 0.56f);
                Cylinder(
                    prefix + "-RearFuelDrum",
                    root,
                    V(side * 0.55f, 1.18f, -3.18f),
                    0.235f,
                    0.235f,
                    0.74f,
                    18,
                    TankShapeAxis.X,
                    color * 0.48f);
                for (int ring = -1; ring <= 1; ring++)
                {
                    Cylinder(
                        prefix + "-RearFuelDrumStrap",
                        root,
                        V(side * (0.55f + ring * 0.17f),
                            1.18f,
                            -3.18f),
                        0.244f,
                        0.244f,
                        0.018f,
                        18,
                        TankShapeAxis.X,
                        Dark());
                }
            }
            TankFittingShapeFactory.BuildAntennaWhip(
                prefix + "-RearLeft",
                root,
                V(-1.04f, 1.50f, -1.32f),
                pendekar ? 1.10f : 1.42f,
                0.011f,
                -0.04f,
                color * 0.42f,
                Dark());
        }

        private static void BuildPt91Turret(
            Transform turret,
            VehicleDefinition definition,
            string prefix,
            Color color,
            bool pendekar)
        {
            Transform root =
                NewRoot(turret, prefix + "-TurretPresentationRoot");
            float zScale = pendekar ? 0.88f : 0.98f;
            TankShapeFactory.LathePart(
                "Painted-" + prefix + "-CastDome",
                root,
                new[] { 1.28f, 1.36f, 1.22f, 0.86f, 0.45f, 0.05f },
                new[] { 0.02f, 0.14f, 0.32f, 0.54f, 0.67f, 0.70f },
                32,
                zScale,
                color * 0.62f,
                1.75f,
                0.78f);
            Box(
                "Painted-" + prefix + "-TurretBustle",
                root,
                V(0f, 0.43f, -1.40f),
                V(1.38f, 0.34f, 0.52f),
                color * 0.56f);
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "Painted-" + prefix + "-PolishFlankBin",
                    root,
                    V(side * 1.30f, 0.39f, -0.58f),
                    V(0.24f, 0.34f, 0.88f),
                    color * 0.55f)
                    .localRotation =
                    Quaternion.Euler(0f, side * 8f, 0f);
                for (int row = 0; row < 2; row++)
                for (int column = 0; column < 3; column++)
                {
                    Transform cassette = Box(
                        "Painted-" + prefix + "-ERAWATurretCheek",
                        root,
                        V(side * (0.34f + column * 0.28f),
                            0.38f + row * 0.14f,
                            1.32f - column * 0.24f - row * 0.11f),
                        V(0.25f, 0.11f, 0.20f),
                        color * 0.50f);
                    cassette.localRotation =
                        Quaternion.Euler(
                            -8f,
                            side * (24f + column * 9f),
                            0f);
                }
                TankFittingShapeFactory.BuildSmokeBank(
                    prefix + (side < 0 ? "-Left" : "-Right"),
                    root,
                    V(side * 1.08f, 0.48f, 0.38f),
                    Quaternion.Euler(0f, side * 56f, 0f),
                    side < 0 ? 6 : 3,
                    0.042f,
                    0.28f,
                    -0.42f,
                    side * 0.35f,
                    0.45f,
                    0.10f,
                    color * 0.44f,
                    Dark());
            }
            for (int tile = 0; tile < 5; tile++)
            {
                Box(
                    "Painted-" + prefix + "-ERAWARoofTile",
                    root,
                    V(-0.60f + tile * 0.30f, 0.70f, 0.20f),
                    V(0.27f, 0.045f, 0.22f),
                    color * 0.51f);
            }
            Cylinder(
                "Painted-" + prefix + "-CommanderCupola",
                root,
                V(-0.38f, 0.76f, -0.08f),
                0.29f,
                0.31f,
                0.10f,
                18,
                TankShapeAxis.Y,
                color * 0.60f);
            Box(
                "Painted-" + prefix + "-PCODrawaSightHousing",
                root,
                V(0.52f, 0.69f, 0.66f),
                V(0.34f, 0.22f, 0.34f),
                color * 0.57f);
            Box(
                prefix + "-PCODrawaSightLens",
                root,
                V(0.52f, 0.71f, 0.84f),
                V(0.20f, 0.09f, 0.020f),
                Glass());
            Box(
                "Painted-" + prefix + "-SearchlightBlock",
                root,
                V(0.58f, 0.55f, 1.26f),
                V(0.30f, 0.30f, 0.24f),
                color * 0.54f);
            TankPintleMachineGunFactory.Build(
                prefix + "-WkmB",
                root,
                V(0.96f, 0.61f, -0.30f),
                Quaternion.Euler(0f, -8f, 0f),
                TankMachineGunClass.Nsvt,
                0.55f,
                0.25f,
                true,
                TankMachineGunShield.Low,
                false,
                Dark(),
                Detail());
            string number = NumericMarking(definition.visual?.number);
            if (!string.IsNullOrEmpty(number))
            {
                TankTacticalNumberFactory.BuildPair(
                    prefix,
                    root,
                    number,
                    0.24f,
                    V(1.26f, 0.35f, -0.88f),
                    Quaternion.Euler(0f, 90f, 0f),
                    V(-1.26f, 0.35f, -0.88f),
                    Quaternion.Euler(0f, -90f, 0f));
            }
        }

        private static void AddPt91Gun(
            Transform turret,
            string prefix,
            Color color,
            VehicleDefinition definition)
        {
            Transform gun = turret.Find("Gun");
            Transform root = NewGunRoot(
                gun,
                prefix + "-2A46MSGunAssembly",
                V(0f, 0.32f, 0.56f));
            float length =
                TankAuthoredDetails.ResolveGunLength(definition, 6.0f);
            Box(
                "Painted-" + prefix + "-MantletBlock",
                root,
                V(0f, 0f, 0.10f),
                V(0.62f, 0.32f, 0.28f),
                color * 0.50f);
            Cylinder(
                "Painted-" + prefix + "-2A46MSRootSleeve",
                root,
                V(0f, -0.02f, 0.74f),
                0.145f,
                0.135f,
                0.74f,
                18,
                TankShapeAxis.Z,
                color * 0.45f);
            Cylinder(
                "Painted-" + prefix + "-2A46MSThermalSleeve",
                root,
                V(0f, -0.03f, 2.36f),
                0.128f,
                0.118f,
                2.10f,
                22,
                TankShapeAxis.Z,
                color * 0.45f);
            Cylinder(
                prefix + "-2A46MSFumeExtractor",
                root,
                V(0f, -0.03f, 4.20f),
                0.175f,
                0.160f,
                0.48f,
                18,
                TankShapeAxis.Z,
                color * 0.42f);
            Cylinder(
                "Painted-" + prefix + "-2A46MSForwardTube",
                root,
                V(0f, -0.04f, (4.46f + length) * 0.5f),
                0.108f,
                0.104f,
                Mathf.Max(0.20f, length - 4.46f),
                22,
                TankShapeAxis.Z,
                color * 0.45f);
            Cylinder(
                prefix + "-2A46MSMuzzleBore",
                root,
                V(0f, -0.04f, length - 0.02f),
                0.064f,
                0.064f,
                0.045f,
                16,
                TankShapeAxis.Z,
                Dark());
        }

        private static void BuildPl01(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            bool variant105 = definition.id == "pl01_105";
            Transform body = NewRoot(root, "PL01-HullPresentationRoot");
            BuildPl01Hull(body, color, variant105);
            BuildPl01Turret(turret, color, variant105);
            AddPl01Gun(turret, color, variant105, definition);
        }

        private static void BuildPl01Hull(
            Transform root,
            Color color,
            bool variant105)
        {
            TankHullLoftShapeFactory.Build(
                "Painted-PL01-FacetedHullLoft",
                root,
                Curve(
                    -3.50f, 2.08f,
                    -1.50f, 2.06f,
                    0.50f, 2.02f,
                    1.30f, 1.98f,
                    2.60f, 1.73f,
                    3.42f, 1.46f),
                Curve(
                    -3.50f, 0.92f,
                    -2.60f, 0.30f,
                    2.35f, 0.30f,
                    3.42f, 1.29f),
                Curve(
                    -3.50f, 1.62f,
                    2.55f, 1.62f,
                    2.66f, 0.94f,
                    3.42f, 0.90f),
                Curve(
                    -3.50f, 0.94f,
                    3.42f, 0.86f),
                Curve(
                    -3.50f, 1.62f,
                    -1.50f, 1.62f,
                    1.88f, 1.62f,
                    3.42f, 0.90f),
                color * 0.64f);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int panel = 0; panel < 8; panel++)
                {
                    float z = -2.92f + panel * 0.82f;
                    float topY = z > 2.10f ? 1.66f : 1.98f;
                    Box(
                        "Painted-PL01-FullHeightSkirtFacet",
                        root,
                        V(side * 1.89f, (topY + 0.72f) * 0.5f, z),
                        V(0.070f, topY - 0.72f, 0.74f),
                        color * 0.50f);
                    Box(
                        "PL01-SkirtVerticalSeam",
                        root,
                        V(side * 1.925f, 1.58f, z + 0.30f),
                        V(0.014f, 0.45f, 0.022f),
                        Dark());
                }
                Box(
                    "Painted-PL01-BowShoulderBridge",
                    root,
                    V(side * 1.32f, 1.48f, 3.08f),
                    V(0.68f, 0.16f, 0.74f),
                    color * 0.56f)
                    .localRotation =
                    Quaternion.Euler(-18f, side * 5f, 0f);
                Box(
                    "PL01-StealthSideLight",
                    root,
                    V(side * 1.92f, 1.68f, 2.32f),
                    V(0.014f, 0.06f, 0.22f),
                    Glass());
            }
            Box(
                "PL01-RearDeckGrille",
                root,
                V(0f, 2.11f, -3.28f),
                V(2.90f, 0.018f, 0.22f),
                Dark());
            Box(
                "PL01-EngineDeckVent",
                root,
                V(-0.20f, 2.11f, -2.55f),
                V(1.80f, 0.018f, 0.55f),
                Dark());
            for (int rib = 0; rib < 4; rib++)
            {
                Box(
                    "PL01-EngineDeckVentRib",
                    root,
                    V(-0.20f, 2.13f, -2.36f - rib * 0.13f),
                    V(1.70f, 0.020f, 0.045f),
                    Detail());
            }
            Cylinder(
                "Painted-PL01-DriverHatch",
                root,
                V(-0.58f, 1.96f, 1.06f),
                0.26f,
                0.26f,
                0.035f,
                18,
                TankShapeAxis.Y,
                color * 0.58f);
            if (variant105)
            {
                Box(
                    "Painted-PL01-105GlacisSpareTrackPack",
                    root,
                    V(0f, 1.82f, 2.04f),
                    V(0.66f, 0.07f, 0.18f),
                    Dark())
                    .localRotation =
                    Quaternion.Euler(-14f, 0f, 0f);
                for (int side = -1; side <= 1; side += 2)
                for (int tile = 0; tile < 3; tile++)
                {
                    Box(
                        "Painted-PL01-105GlacisEraCassette",
                        root,
                        V(side * 0.72f, 1.78f, 1.78f + tile * 0.30f),
                        V(0.34f, 0.060f, 0.245f),
                        color * 0.52f)
                        .localRotation =
                        Quaternion.Euler(-14f, 0f, 0f);
                }
            }
        }

        private static void BuildPl01Turret(
            Transform turret,
            Color color,
            bool variant105)
        {
            Transform root =
                NewRoot(turret, "PL01-TurretPresentationRoot");
            Vector2[] plan =
            {
                new Vector2(0.00f, 1.88f),
                new Vector2(0.72f, 1.34f),
                new Vector2(1.48f, 0.02f),
                new Vector2(1.24f, -1.27f),
                new Vector2(0.66f, -2.70f),
                new Vector2(0.09f, -2.98f),
                new Vector2(-0.66f, -2.70f),
                new Vector2(-1.24f, -1.27f),
                new Vector2(-1.48f, 0.02f),
                new Vector2(-0.72f, 1.34f)
            };
            TankShapeFactory.PolyTurretPart(
                "Painted-PL01-FacetedTurretShell",
                root,
                plan,
                0.70f,
                0.10f,
                0.34f,
                color * 0.62f);
            root.localPosition = V(0f, -0.03f, 0f);
            Box(
                "PL01-RoofCenterSeam",
                root,
                V(0f, 0.71f, -0.80f),
                V(0.018f, 0.014f, 2.10f),
                Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                for (int panel = 0; panel < 4; panel++)
                {
                    Box(
                        "Painted-PL01-StealthTurretSidePanel",
                        root,
                        V(side * (1.15f + panel * 0.05f),
                            0.38f,
                            0.36f - panel * 0.52f),
                        V(0.060f, 0.24f, 0.42f),
                        color * 0.52f)
                        .localRotation =
                        Quaternion.Euler(
                            -5f,
                            side * (8f + panel * 3f),
                            0f);
                }
                Cylinder(
                    "Painted-PL01-ShoulderEODome",
                    root,
                    V(side * 1.02f, 0.66f, -0.11f),
                    0.24f,
                    0.275f,
                    0.075f,
                    18,
                    TankShapeAxis.Y,
                    color * 0.58f);
                Box(
                    "PL01-LaserWarningReceiver",
                    root,
                    V(side * 1.20f, 0.60f, 0.62f),
                    V(0.11f, 0.10f, 0.12f),
                    Detail());
                TankFittingShapeFactory.BuildSmokeBank(
                    "PL01-" + (side < 0 ? "Left" : "Right"),
                    root,
                    V(side * 0.42f, 0.72f, -1.78f),
                    Quaternion.Euler(0f, side * 52f, -side * 4f),
                    6,
                    0.035f,
                    0.24f,
                    -0.35f,
                    side * 0.40f,
                    0.48f,
                    0.078f,
                    color * 0.44f,
                    Dark());
            }
            Box(
                "Painted-PL01-PanoramicSight",
                root,
                V(-0.20f, 0.90f, 0.43f),
                V(0.29f, 0.18f, 0.25f),
                color * 0.56f);
            Box(
                "PL01-PanoramicSightLens",
                root,
                V(-0.20f, 0.91f, 0.57f),
                V(0.15f, 0.07f, 0.014f),
                Glass());
            Box(
                "Painted-PL01-GunnerPrimarySight",
                root,
                V(0.52f, 0.80f, 0.36f),
                V(0.31f, 0.16f, 0.28f),
                color * 0.56f);
            if (variant105)
            {
                BuildPl01Crows(root, color);
            }
            else
            {
                BuildPl01LowObservableRws(root, color);
            }
            TankPintleMachineGunFactory.Build(
                "PL01-LoaderMG",
                root,
                V(0.61f, 0.84f, -0.48f),
                Quaternion.Euler(0f, 5f, 0f),
                TankMachineGunClass.Mag,
                0.48f,
                0.05f,
                true,
                TankMachineGunShield.Low,
                false,
                Dark(),
                Detail());
            TankFittingShapeFactory.BuildAntennaWhip(
                "PL01-Left",
                root,
                V(-0.88f, 0.78f, -1.72f),
                0.42f,
                0.010f,
                -0.05f,
                color * 0.40f,
                Dark());
            TankFittingShapeFactory.BuildAntennaWhip(
                "PL01-Right",
                root,
                V(0.86f, 0.78f, -1.88f),
                0.36f,
                0.010f,
                0.05f,
                color * 0.40f,
                Dark());
        }

        private static void BuildPl01LowObservableRws(
            Transform root,
            Color color)
        {
            Box(
                "Painted-PL01-LowObservableRwsTower",
                root,
                V(-0.05f, 1.02f, -1.33f),
                V(0.52f, 0.42f, 0.18f),
                color * 0.53f);
            TankPintleMachineGunFactory.Build(
                "PL01-ParkedRws",
                root,
                V(-0.05f, 1.05f, -1.33f),
                Quaternion.Euler(0f, 90f, 0f),
                TankMachineGunClass.Mag,
                0.62f,
                0.12f,
                true,
                TankMachineGunShield.Armored,
                false,
                Dark(),
                Detail());
        }

        private static void BuildPl01Crows(
            Transform root,
            Color color)
        {
            Box(
                "Painted-PL01-105CrowsBasePlate",
                root,
                V(-0.05f, 0.82f, -1.26f),
                V(0.62f, 0.04f, 0.50f),
                color * 0.55f);
            Cylinder(
                "PL01-105CrowsSlewRing",
                root,
                V(-0.05f, 0.86f, -1.26f),
                0.205f,
                0.215f,
                0.055f,
                18,
                TankShapeAxis.Y,
                Dark());
            Box(
                "Painted-PL01-105CrowsCradle",
                root,
                V(-0.05f, 1.04f, -1.18f),
                V(0.44f, 0.20f, 0.38f),
                color * 0.53f);
            TankPintleMachineGunFactory.Build(
                "PL01-105CrowsWeapon",
                root,
                V(-0.05f, 1.02f, -1.20f),
                Quaternion.identity,
                TankMachineGunClass.M2,
                0.78f,
                0.05f,
                false,
                TankMachineGunShield.None,
                false,
                Dark(),
                Detail());
        }

        private static void AddPl01Gun(
            Transform turret,
            Color color,
            bool variant105,
            VehicleDefinition definition)
        {
            Transform gun = turret.Find("Gun");
            Transform root =
                NewGunRoot(gun, "PL01-GunAssembly", V(0f, 0.31f, 1.45f));
            float radius = TankAuthoredDetails.ResolveGunRadius(definition);
            float length =
                TankAuthoredDetails.ResolveGunLength(definition, 4.7f);
            Box(
                "Painted-PL01-FacetedGunHousing",
                root,
                V(0f, 0.02f, 1.02f),
                V(0.54f, 0.26f, 1.90f),
                color * 0.50f);
            Box(
                "PL01-GunCoverSpine",
                root,
                V(0f, 0.19f, 2.10f),
                V(0.38f, 0.030f, 0.05f),
                Dark());
            Cylinder(
                "Painted-PL01-MainGunTube",
                root,
                V(0f, -0.03f, (3.26f + length) * 0.5f),
                radius,
                radius * 0.94f,
                Mathf.Max(0.20f, length - 3.26f),
                22,
                TankShapeAxis.Z,
                color * 0.44f);
            Cylinder(
                variant105
                    ? "PL01-105MuzzleCollar"
                    : "PL01-MuzzleCollar",
                root,
                V(0f, -0.03f, length - 0.07f),
                radius * 1.10f,
                radius * 1.06f,
                0.12f,
                16,
                TankShapeAxis.Z,
                Dark());
        }

        private static Transform NewRoot(
            Transform parent,
            string name)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            return root;
        }

        private static Transform NewGunRoot(
            Transform gun,
            string name,
            Vector3 sourcePivot)
        {
            Transform root = NewRoot(gun, name);
            Vector3 scale = gun.localScale;
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
            return root;
        }

        private static TankHullProfilePoint[] Curve(
            params float[] values)
        {
            TankHullProfilePoint[] curve =
                new TankHullProfilePoint[values.Length / 2];
            for (int index = 0; index < curve.Length; index++)
            {
                curve[index] =
                    new TankHullProfilePoint(
                        values[index * 2],
                        values[index * 2 + 1]);
            }
            return curve;
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

        private static void HideByPrefix(
            Transform root,
            string prefix)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < parts.Length; index++)
            {
                if (parts[index].name.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
                {
                    HideRenderer(parts[index]);
                }
            }
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

        private static Color Detail()
        {
            return new Color(0.22f, 0.23f, 0.19f);
        }

        private static Color Glass()
        {
            return new Color(0.025f, 0.075f, 0.085f);
        }

        private static Color Rubber()
        {
            return new Color(0.045f, 0.046f, 0.040f);
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
