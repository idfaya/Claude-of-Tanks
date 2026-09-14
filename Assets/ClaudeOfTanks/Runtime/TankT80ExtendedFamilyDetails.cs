using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT80ExtendedFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "t80u" ||
                id == "ua_t80bv" ||
                id == "ua_t80u_kursk" ||
                id == "t84" ||
                id == "ua_t84_oplot_m";
        }

        public static bool UsesT80CastTurret(string id)
        {
            return id == "t80u" ||
                id == "ua_t80bv" ||
                id == "ua_t80u_kursk";
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
            HideByPrefix(root, "Armor-");
            if (definition.id == "t84" ||
                definition.id == "ua_t84_oplot_m")
            {
                BuildT84(root, turret, definition, color);
                return;
            }

            BuildT80U(root, turret, definition, color);
        }

        private static void BuildT80U(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            TankHullLoftShapeFactory.Build(
                "Painted-T80U-HullLoft",
                root,
                Curve(
                    -3.28f, 1.37f,
                    -2.55f, 1.53f,
                    -1.20f, 1.53f,
                    1.20f, 1.53f,
                    2.46f, 1.36f,
                    3.06f, 1.28f,
                    3.40f, 1.07f),
                Curve(
                    -3.28f, 0.62f,
                    -2.30f, 0.44f,
                    2.46f, 0.49f,
                    3.32f, 0.71f),
                Curve(
                    -3.28f, 1.14f,
                    -2.55f, 1.70f,
                    2.42f, 1.70f,
                    3.40f, 1.10f),
                Curve(
                    -3.28f, 0.96f,
                    -2.20f, 1.06f,
                    1.72f, 1.06f,
                    2.50f, 0.96f),
                Curve(
                    -3.28f, 1.06f,
                    -2.55f, 1.26f,
                    2.42f, 1.29f,
                    3.40f, 0.89f),
                color * 0.66f);
            TankT80RunningGearDetails.Build(root, root, color);
            TankT80HullExteriorDetails.Build(root, color, "t80bv");
            TankT80SternDetails.Build(root, "t80bv");
            AddT80UGlacisKontakt5(root, color);
            AddT80USkirtsAndDeck(root, color, definition.id);
            TankT80TurretDetails.Build(turret, definition, color);
            AddT80UTurretKontakt5(turret, color, definition.id);
        }

        private static void AddT80UGlacisKontakt5(
            Transform root,
            Color color)
        {
            for (int row = 0; row < 3; row++)
            {
                int columns = row == 2 ? 4 : 5;
                float start = row == 2 ? -0.84f : -1.12f;
                for (int column = 0; column < columns; column++)
                {
                    float x = start + column * 0.56f;
                    float z = 2.35f + row * 0.26f;
                    Transform cassette = Box(
                        "Painted-T80U-K5GlacisTile",
                        root,
                        V(x, 1.31f - row * 0.035f, z),
                        V(0.48f, 0.055f, 0.22f),
                        color * 0.53f);
                    cassette.localRotation =
                        Quaternion.Euler(-6.5f, 0f, 0f);
                    Box(
                        "T80U-K5GlacisTileCap",
                        root,
                        cassette.localPosition + V(0f, 0.036f, 0f),
                        V(0.40f, 0.010f, 0.16f),
                        Dark()).localRotation =
                        cassette.localRotation;
                }
            }
        }

        private static void AddT80USkirtsAndDeck(
            Transform root,
            Color color,
            string id)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int panel = 0; panel < 11; panel++)
                {
                    float z = -2.72f + panel * 0.51f;
                    Box(
                        "Painted-T80U-DeepRubberSkirt",
                        root,
                        V(side * 1.72f, 0.90f, z),
                        V(0.055f, 0.58f, 0.47f),
                        panel < 3 ? color * 0.46f : Rubber());
                }
                Box(
                    "Painted-T80U-FenderFuelRun",
                    root,
                    V(side * 1.43f, 1.47f, -1.80f),
                    V(0.28f, 0.17f, 1.42f),
                    color * 0.58f);
                Box(
                    "T80U-FenderFuelRunStrap",
                    root,
                    V(side * 1.43f, 1.57f, -1.80f),
                    V(0.30f, 0.025f, 1.30f),
                    Dark());
                Cylinder(
                    "T80U-RearFuelDrum",
                    root,
                    V(side * 0.72f, 1.48f, -3.10f),
                    0.24f,
                    0.24f,
                    1.20f,
                    18,
                    TankShapeAxis.X,
                    color * 0.50f);
            }
            Box("T80U-TurbineDeckGrille", root,
                V(0f, 1.56f, -2.00f),
                V(1.70f, 0.025f, 1.10f), Dark());
            for (int index = 0; index < 6; index++)
            {
                Box("T80U-TurbineLouvre", root,
                    V(0f, 1.572f, -1.60f - index * 0.16f),
                    V(1.60f, 0.018f, 0.05f), Detail());
            }
            TankFittingShapeFactory.BuildTowCable(
                "T80U-BowTowCable",
                root,
                new[]
                {
                    V(-1.02f, 1.26f, 2.85f),
                    V(-0.35f, 1.21f, 3.02f),
                    V(0.50f, 1.22f, 2.92f)
                },
                0.022f,
                20,
                6,
                Dark());
            if (id.StartsWith("ua_", StringComparison.Ordinal))
            {
                Box("Painted-T80U-UkrainianStowageBin", root,
                    V(1.38f, 1.55f, -0.40f),
                    V(0.30f, 0.18f, 0.86f), color * 0.60f);
                TankFittingShapeFactory.BuildAntennaWhip(
                    "T80U-UA",
                    root,
                    V(-1.08f, 1.60f, -1.10f),
                    1.30f,
                    0.012f,
                    -0.03f,
                    color * 0.42f,
                    Dark());
            }
        }

        private static void AddT80UTurretKontakt5(
            Transform turret,
            Color color,
            string id)
        {
            Transform root = turret.Find("T80-TurretPresentationRoot");
            if (root == null) return;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int row = 0; row < 2; row++)
                for (int column = 0; column < 4; column++)
                {
                    Transform tile = Box(
                        "Painted-T80U-K5TurretChevron",
                        root,
                        V(side * (0.30f + column * 0.31f),
                            0.43f + row * 0.16f,
                            1.30f - column * 0.25f - row * 0.08f),
                        V(0.28f, 0.12f, 0.20f),
                        color * 0.50f);
                    tile.localRotation =
                        Quaternion.Euler(
                            -7f,
                            side * (31f + column * 8f),
                            0f);
                }
                for (int index = 0; index < 5; index++)
                {
                    Transform flank = Box(
                        "Painted-T80U-K5FlankReturn",
                        root,
                        V(side * (1.28f + index * 0.035f),
                            0.35f - index * 0.012f,
                            0.20f - index * 0.27f),
                        V(0.23f, 0.15f, 0.26f),
                        color * 0.52f);
                    flank.localRotation =
                        Quaternion.Euler(
                            -5f,
                            side * (33f + index * 6f),
                            0f);
                }
            }
            Box("Painted-T80U-CommanderSightPlinth", root,
                V(0.73f, 0.73f, 0.10f),
                V(0.24f, 0.14f, 0.22f), color * 0.58f);
            Box("T80U-CommanderSightLens", root,
                V(0.73f, 0.75f, 0.22f),
                V(0.12f, 0.07f, 0.018f), Glass());
            if (id == "ua_t80u_kursk")
            {
                Box("Painted-T80U-KurskBurrStowage", root,
                    V(-0.55f, 0.70f, -1.10f),
                    V(0.62f, 0.18f, 0.34f), color * 0.57f);
            }
        }

        private static void BuildT84(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            TankHullLoftShapeFactory.Build(
                "Painted-T84-HullLoft",
                root,
                Curve(
                    -4.30f, 1.36f,
                    -2.60f, 1.41f,
                    -0.10f, 1.37f,
                    0.90f, 1.27f,
                    1.99f, 1.08f),
                Curve(
                    -4.30f, 0.68f,
                    -4.05f, 0.37f,
                    1.30f, 0.35f,
                    1.99f, 0.50f),
                Curve(
                    -4.30f, 1.28f,
                    1.99f, 1.28f),
                Curve(
                    -4.30f, 0.84f,
                    1.99f, 0.84f),
                1.15f,
                color * 0.66f);
            TankT80RunningGearDetails.Build(root, root, color);
            AddT84HullDetails(root, color, definition.id);
            BuildT84WeldedTurret(turret, definition, color);
        }

        private static void AddT84HullDetails(
            Transform root,
            Color color,
            string id)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int panel = 0; panel < 10; panel++)
                {
                    Box(
                        "Painted-T84-DeepSkirtPanel",
                        root,
                        V(side * 1.69f, 0.82f, -3.55f + panel * 0.44f),
                        V(0.055f, 0.70f, 0.41f),
                        color * 0.48f);
                }
                Box("Painted-T84-BowMudguard", root,
                    V(side * 1.53f, 0.92f, 2.10f),
                    V(0.42f, 0.38f, 0.10f), color * 0.52f);
                Box("T84-RearRubberFlap", root,
                    V(side * 1.45f, 0.94f, -4.43f),
                    V(0.18f, 0.60f, 0.06f), Rubber());
            }
            Box("Painted-T84-EngineHump", root,
                V(0f, 1.45f, -2.86f),
                V(1.46f, 0.07f, 0.43f), color * 0.58f);
            Box("T84-EngineDeckLouvre", root,
                V(0f, 1.435f, -3.40f),
                V(1.70f, 0.025f, 0.90f), Dark());
            Cylinder("T84-RearStowageLog", root,
                V(0f, 0.79f, -3.17f),
                0.12f, 0.12f, 2.20f, 12,
                TankShapeAxis.X, Wood());
            for (int row = 0; row < 3; row++)
            for (int column = 0; column < 5; column++)
            {
                Box("Painted-T84-GlacisDupletTile", root,
                    V(-1.12f + column * 0.56f,
                        1.23f - row * 0.03f,
                        1.10f + row * 0.21f),
                    V(0.52f, 0.055f, 0.16f),
                    color * 0.52f).localRotation =
                    Quaternion.Euler(-6f, 0f, 0f);
            }
            if (id == "ua_t84_oplot_m")
            {
                Box("Painted-T84-OplotM-RightFenderBin", root,
                    V(1.42f, 1.39f, -0.64f),
                    V(0.26f, 0.16f, 0.48f), color * 0.60f);
                Box("Painted-T84-OplotM-LeftToolRun", root,
                    V(-1.43f, 1.40f, -0.38f),
                    V(0.05f, 0.04f, 0.92f), Wood());
            }
        }

        private static void BuildT84WeldedTurret(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            HideRenderer(turret.Find("Turret"));
            HideRenderer(turret.Find("Gun"));
            Transform root =
                new GameObject("T84-WeldedTurretPresentationRoot")
                    .transform;
            root.SetParent(turret, false);
            Box("Painted-T84-LowTurretCollar", root,
                V(0f, 0.10f, 0.12f),
                V(2.52f, 0.18f, 1.54f), color * 0.58f);
            Box("Painted-T84-CheekRamp", root,
                V(0f, 0.44f, 0.96f),
                V(1.72f, 0.36f, 1.02f), color * 0.62f)
                .localRotation = Quaternion.Euler(-5f, 0f, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform shoulder = Box(
                    "Painted-T84-WeldedShoulder",
                    root,
                    V(side * 1.12f, 0.36f, 0.26f),
                    V(0.30f, 0.56f, 1.28f),
                    color * 0.57f);
                shoulder.localRotation =
                    Quaternion.Euler(0f, side * 10f, 0f);
                Box("Painted-T84-BustleSideSheet", root,
                    V(side * 0.78f, 0.46f, -1.40f),
                    V(0.20f, 0.56f, 1.24f),
                    color * 0.56f)
                    .localRotation =
                    Quaternion.Euler(0f, -side * 12f, 0f);
            }
            Box("Painted-T84-BustleRoof", root,
                V(0f, 0.67f, -1.35f),
                V(1.52f, 0.12f, 1.36f), color * 0.60f);
            Box("Painted-T84-BustleRearClosure", root,
                V(0f, 0.50f, -2.08f),
                V(0.94f, 0.34f, 0.08f), color * 0.56f);
            AddT84Duplet(root, color);
            AddT84RoofKit(root, definition, color);
            AddT84Gun(turret, color);
        }

        private static void AddT84Duplet(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int row = 0; row < 2; row++)
                for (int column = 0; column < 4; column++)
                {
                    Transform tile = Box(
                        "Painted-T84-DupletCheekCassette",
                        root,
                        V(side * (0.26f + column * 0.24f),
                            0.42f + row * 0.13f,
                            1.58f - column * 0.14f - row * 0.40f),
                        V(0.23f, 0.10f, 0.30f),
                        color * 0.50f);
                    tile.localRotation =
                        Quaternion.Euler(
                            9f,
                            side * 18f,
                            side * 1.5f);
                    Box(
                        "T84-DupletCheekCassetteCap",
                        root,
                        tile.localPosition + V(0f, 0.058f, 0f),
                        V(0.16f, 0.012f, 0.23f),
                        Dark()).localRotation =
                        tile.localRotation;
                }
                for (int index = 0; index < 4; index++)
                {
                    Box(
                        "Painted-T84-DupletFlankCassette",
                        root,
                        V(side * 1.20f,
                            0.34f,
                            0.56f - index * 0.34f),
                        V(0.10f, 0.29f, 0.34f),
                        color * 0.51f);
                }
            }
        }

        private static void AddT84RoofKit(
            Transform root,
            VehicleDefinition definition,
            Color color)
        {
            Box("Painted-T84-GunnerSightHousing", root,
                V(-0.52f, 0.78f, 0.26f),
                V(0.46f, 0.22f, 0.40f), color * 0.58f);
            Box("T84-GunnerSightLens", root,
                V(-0.52f, 0.80f, 0.48f),
                V(0.22f, 0.08f, 0.018f), Glass());
            Cylinder("Painted-T84-CommanderCupola", root,
                V(0.42f, 0.82f, -0.35f),
                0.24f, 0.24f, 0.11f, 16,
                TankShapeAxis.Y, color * 0.60f);
            Box("Painted-T84-CommanderPanorama", root,
                V(0.35f, 0.85f, 0.51f),
                V(0.26f, 0.18f, 0.14f), color * 0.58f);
            Box("T84-CommanderPanoramaLens", root,
                V(0.35f, 0.86f, 0.59f),
                V(0.14f, 0.09f, 0.018f), Glass());
            for (int side = -1; side <= 1; side += 2)
            {
                Transform smoke =
                    TankFittingShapeFactory.BuildSmokeBank(
                        "T84",
                        root,
                        V(side * 0.62f, 0.43f, 1.55f),
                        Quaternion.Euler(0f, side * 64f, 0f),
                        5,
                        0.043f,
                        0.28f,
                        -0.40f,
                        0.30f,
                        0.56f,
                        0.095f,
                        color * 0.44f,
                        Dark());
                smoke.name = "T84-SmokeBank";
            }
            string number = definition?.visual?.number;
            number = NumericMarking(number);
            if (!string.IsNullOrEmpty(number))
            {
                TankTacticalNumberFactory.BuildPair(
                    "T84",
                    root,
                    number,
                    0.26f,
                    V(1.22f, 0.32f, 0.20f),
                    Quaternion.Euler(0f, 90f, 0f),
                    V(-1.22f, 0.32f, 0.20f),
                    Quaternion.Euler(0f, -90f, 0f));
            }
        }

        private static void AddT84Gun(
            Transform turret,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            Transform root =
                new GameObject("T84-KBA3GunAssembly").transform;
            root.SetParent(gun, false);
            Vector3 scale = gun.localScale;
            Vector3 sourcePivot = V(0f, 0.28f, 0.55f);
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
            Box("Painted-T84-MantletBlock", root,
                V(0f, 0.01f, 0.10f),
                V(0.64f, 0.36f, 0.20f), color * 0.50f);
            Cylinder("Painted-T84-KBA3RootSleeve", root,
                V(0f, -0.04f, 0.80f),
                0.14f, 0.14f, 0.56f, 18,
                TankShapeAxis.Z, color * 0.46f);
            Cylinder("Painted-T84-KBA3ThermalSleeve", root,
                V(0f, -0.048f, 1.86f),
                0.13f, 0.13f, 1.50f, 24,
                TankShapeAxis.Z, color * 0.46f);
            Cylinder("Painted-T84-KBA3FumeExtractor", root,
                V(0f, -0.048f, 2.58f),
                0.15f, 0.14f, 0.44f, 18,
                TankShapeAxis.Z, color * 0.45f);
            Cylinder("Painted-T84-KBA3ForwardTube", root,
                V(0f, -0.054f, 4.10f),
                0.128f, 0.128f, 2.76f, 24,
                TankShapeAxis.Z, color * 0.46f);
            Cylinder("Painted-T84-KBA3MuzzleCollar", root,
                V(0f, -0.052f, 5.50f),
                0.118f, 0.118f, 0.12f, 16,
                TankShapeAxis.Z, color * 0.46f);
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
            return new Color(0.21f, 0.23f, 0.18f);
        }

        private static Color Glass()
        {
            return new Color(0.025f, 0.075f, 0.085f);
        }

        private static Color Rubber()
        {
            return new Color(0.045f, 0.046f, 0.040f);
        }

        private static Color Wood()
        {
            return new Color(0.34f, 0.22f, 0.11f);
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
