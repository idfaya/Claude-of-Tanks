using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBritishLegacyFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "chieftain5" ||
                id == "chieftain_mk10" ||
                id == "challenger1" ||
                id == "vickers_mk1" ||
                id == "centurion3" ||
                id == "centurion5";
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

            if (definition.id.StartsWith(
                    "centurion",
                    StringComparison.Ordinal))
            {
                BuildCenturion(root, turret, definition, color);
                return;
            }
            if (definition.id == "vickers_mk1")
            {
                BuildVickers(root, turret, definition, color);
                return;
            }
            if (definition.id == "challenger1")
            {
                BuildChallenger1(root, turret, definition, color);
                return;
            }
            BuildChieftain(root, turret, definition, color);
        }

        private static void BuildChieftain(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            bool mk10 = definition.id == "chieftain_mk10";
            BuildUkHullLoft(
                "Painted-Chieftain-HullLoft",
                root,
                color,
                -3.76f,
                3.76f,
                mk10 ? 1.815f : 1.795f,
                1.71f,
                0.46f);
            AddUkTrackCovers(root, "Chieftain", color, 1.78f, -3.45f, 3.28f);
            AddChieftainHullDetails(root, color, mk10);
            BuildChieftainTurret(turret, definition, color, mk10);
            AddL11Gun(turret, "Chieftain", color, 6.10f, 0.082f);
        }

        private static void BuildChallenger1(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            BuildUkHullLoft(
                "Painted-Challenger1-HullLoft",
                root,
                color,
                -4.05f,
                4.16f,
                1.76f,
                1.75f,
                0.51f);
            AddUkTrackCovers(root, "Challenger1", color, 1.83f, -3.70f, 3.45f);
            AddChallenger1HullDetails(root, color);
            BuildChallenger1Turret(turret, definition, color);
            AddL11Gun(turret, "Challenger1", color, 6.70f, 0.11f);
        }

        private static void BuildVickers(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            BuildUkHullLoft(
                "Painted-Vickers-HullLoft",
                root,
                color,
                -3.77f,
                3.38f,
                1.50f,
                1.47f,
                0.46f);
            AddUkTrackCovers(root, "Vickers", color, 1.51f, -3.20f, 3.05f);
            AddVickersHullDetails(root, color);
            BuildCenturionPatternTurret(
                turret,
                definition,
                color,
                "Vickers",
                1.28f,
                0.88f,
                true);
            AddL7Gun(turret, "Vickers", color, 6.42f);
        }

        private static void BuildCenturion(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            bool mk5 = definition.id == "centurion5";
            BuildUkHullLoft(
                "Painted-Centurion-HullLoft",
                root,
                color,
                -3.69f,
                3.95f,
                1.67f,
                1.75f,
                0.53f);
            AddUkTrackCovers(root, "Centurion", color, 1.64f, -3.20f, 3.25f);
            AddCenturionHullDetails(root, color, mk5);
            BuildCenturionPatternTurret(
                turret,
                definition,
                color,
                "Centurion",
                1.42f,
                mk5 ? 0.98f : 0.91f,
                mk5);
            AddL7Gun(turret, "Centurion", color, 6.13f);
        }

        private static void BuildUkHullLoft(
            string name,
            Transform root,
            Color color,
            float rear,
            float front,
            float halfWidth,
            float deckY,
            float bellyY)
        {
            TankHullLoftShapeFactory.Build(
                name,
                root,
                Curve(
                    rear, deckY - 0.16f,
                    rear + 0.62f, deckY,
                    -0.40f, deckY,
                    1.70f, deckY - 0.08f,
                    front - 0.70f, deckY - 0.28f,
                    front, deckY - 0.65f),
                Curve(
                    rear, bellyY + 0.18f,
                    rear + 0.60f, bellyY,
                    front - 0.70f, bellyY,
                    front, bellyY + 0.16f),
                Curve(
                    rear, halfWidth * 0.62f,
                    rear + 0.85f, halfWidth,
                    front - 0.70f, halfWidth,
                    front, halfWidth * 0.60f),
                Curve(
                    rear, halfWidth * 0.46f,
                    rear + 0.85f, halfWidth * 0.72f,
                    front - 0.70f, halfWidth * 0.72f,
                    front, halfWidth * 0.46f),
                Curve(
                    rear, deckY - 0.34f,
                    rear + 0.85f, deckY - 0.24f,
                    front - 0.70f, deckY - 0.30f,
                    front, deckY - 0.54f),
                color * 0.66f);
        }

        private static void AddUkTrackCovers(
            Transform root,
            string prefix,
            Color color,
            float sideX,
            float rear,
            float front)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float span = front - rear;
                for (int panel = 0; panel < 8; panel++)
                {
                    float z = rear + span * (panel + 0.5f) / 8f;
                    Box(
                        "Painted-" + prefix + "-SkirtPanel",
                        root,
                        V(side * sideX, 0.92f, z),
                        V(0.065f, 0.58f, span / 8f - 0.04f),
                        color * (panel < 2 ? 0.48f : 0.54f));
                }
                Box(
                    prefix + "-RubberTrackHem",
                    root,
                    V(side * (sideX + 0.018f), 0.47f, (front + rear) * 0.5f),
                    V(0.035f, 0.20f, span),
                    Rubber());
            }
        }

        private static void AddChieftainHullDetails(
            Transform root,
            Color color,
            bool mk10)
        {
            Box("Painted-Chieftain-GlacisPlate", root,
                V(0f, 1.47f, 2.70f),
                V(2.20f, 0.10f, 1.20f), color * 0.58f)
                .localRotation = Quaternion.Euler(-11f, 0f, 0f);
            Box("Painted-Chieftain-DriverPeriscopeHood", root,
                V(-0.18f, 1.70f, 1.42f),
                V(0.34f, 0.10f, 0.22f), color * 0.60f);
            Box("Chieftain-DriverPeriscopeLens", root,
                V(-0.18f, 1.75f, 1.54f),
                V(0.20f, 0.052f, 0.018f), Glass());
            Box("Chieftain-RearLouvre", root,
                V(0f, 1.68f, -2.85f),
                V(1.80f, 0.028f, 0.70f), Dark());
            for (int index = 0; index < 5; index++)
            {
                Box("Chieftain-RearLouvreRib", root,
                    V(0f, 1.705f, -2.58f - index * 0.12f),
                    V(1.70f, 0.018f, 0.035f), Detail());
            }
            if (mk10)
            {
                for (int side = -1; side <= 1; side += 2)
                for (int row = 0; row < 2; row++)
                for (int column = 0; column < 4; column++)
                {
                    Box("Painted-ChieftainMk10-StillbrewBrowTile",
                        root,
                        V(side * (0.28f + column * 0.26f),
                            1.64f + row * 0.08f,
                            2.28f - row * 0.16f),
                        V(0.24f, 0.10f, 0.18f),
                        color * 0.52f);
                }
            }
            AddRearBoxes(root, "Chieftain", color, -3.55f);
        }

        private static void AddChallenger1HullDetails(
            Transform root,
            Color color)
        {
            Box("Painted-Challenger1-GlacisWedge", root,
                V(0f, 1.44f, 3.12f),
                V(2.40f, 0.12f, 1.35f), color * 0.58f)
                .localRotation = Quaternion.Euler(-9f, 0f, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int column = 0; column < 5; column++)
                {
                    Box("Painted-Challenger1-BurlingtonSidePack",
                        root,
                        V(side * 1.78f, 1.18f, -1.80f + column * 0.78f),
                        V(0.16f, 0.42f, 0.66f),
                        color * 0.50f);
                }
            }
            Box("Challenger1-RearDeckGrille", root,
                V(0f, 1.70f, -3.00f),
                V(1.75f, 0.03f, 0.90f), Dark());
            AddRearBoxes(root, "Challenger1", color, -3.72f);
        }

        private static void AddVickersHullDetails(
            Transform root,
            Color color)
        {
            Box("Painted-Vickers-FoldedBowLock", root,
                V(0f, 1.27f, 2.62f),
                V(1.20f, 0.08f, 0.40f), color * 0.58f)
                .localRotation = Quaternion.Euler(-14f, 0f, 0f);
            Box("Vickers-EngineDeckGrille", root,
                V(0f, 1.48f, -2.42f),
                V(1.45f, 0.024f, 0.80f), Dark());
            AddRearBoxes(root, "Vickers", color, -3.38f);
        }

        private static void AddCenturionHullDetails(
            Transform root,
            Color color,
            bool mk5)
        {
            Box("Painted-Centurion-GlacisPlate", root,
                V(0f, 1.46f, 2.82f),
                V(2.08f, 0.10f, 1.10f), color * 0.58f)
                .localRotation = Quaternion.Euler(-10f, 0f, 0f);
            Box("Centurion-EngineDeckGrille", root,
                V(0f, 1.74f, -2.72f),
                V(1.52f, 0.026f, 0.78f), Dark());
            if (mk5)
            {
                Box("Painted-CenturionMk5-MantletDustCoverStowage",
                    root,
                    V(-1.22f, 1.63f, -0.40f),
                    V(0.22f, 0.16f, 0.80f),
                    color * 0.56f);
            }
            AddRearBoxes(root, "Centurion", color, -3.42f);
        }

        private static void AddRearBoxes(
            Transform root,
            string prefix,
            Color color,
            float z)
        {
            Box("Painted-" + prefix + "-RearStowageBin", root,
                V(-0.45f, 1.55f, z),
                V(0.70f, 0.22f, 0.28f), color * 0.56f);
            Box("Painted-" + prefix + "-RearStowageBin", root,
                V(0.45f, 1.55f, z),
                V(0.70f, 0.22f, 0.28f), color * 0.56f);
            TankFittingShapeFactory.BuildTowCable(
                prefix + "-BowTowCable",
                root,
                new[]
                {
                    V(-1.00f, 1.22f, 2.35f),
                    V(0f, 1.30f, 2.05f),
                    V(1.00f, 1.22f, 2.35f)
                },
                0.022f,
                18,
                6,
                Dark());
        }

        private static void BuildChieftainTurret(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            bool mk10)
        {
            Transform root = NewRoot(turret, "Chieftain-TurretPresentationRoot");
            Box("Painted-Chieftain-LowTurretShell", root,
                V(0f, 0.18f, -0.30f),
                V(2.40f, 0.42f, 1.74f), color * 0.58f);
            Box("Painted-Chieftain-CheekSlope", root,
                V(0f, 0.38f, 0.70f),
                V(1.95f, 0.38f, 1.20f), color * 0.62f)
                .localRotation = Quaternion.Euler(-7f, 0f, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-Chieftain-SideCheek", root,
                    V(side * 1.17f, 0.30f, 0.02f),
                    V(0.34f, 0.48f, 1.52f), color * 0.55f)
                    .localRotation = Quaternion.Euler(0f, side * 9f, 0f);
                if (mk10)
                {
                    Box("Painted-ChieftainMk10-StillbrewTurretCheek",
                        root,
                        V(side * 0.86f, 0.50f, 0.54f),
                        V(0.50f, 0.22f, 0.68f),
                        color * 0.50f)
                        .localRotation =
                        Quaternion.Euler(-6f, side * 20f, 0f);
                }
            }
            AddBritishRoofKit(root, definition, color, "Chieftain", mk10);
        }

        private static void BuildChallenger1Turret(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform root = NewRoot(turret, "Challenger1-TurretPresentationRoot");
            Box("Painted-Challenger1-BurlingtonTurretShell", root,
                V(0f, 0.17f, -0.34f),
                V(2.58f, 0.44f, 1.82f), color * 0.56f);
            Box("Painted-Challenger1-FrontalWedge", root,
                V(0f, 0.34f, 0.76f),
                V(2.35f, 0.36f, 1.22f), color * 0.60f)
                .localRotation = Quaternion.Euler(-6f, 0f, 0f);
            Box("Painted-Challenger1-BustleBox", root,
                V(0f, 0.43f, -1.35f),
                V(2.30f, 0.52f, 1.30f), color * 0.54f);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-Challenger1-SmokeBankArmorShoe", root,
                    V(side * 1.20f, 0.42f, 0.30f),
                    V(0.22f, 0.24f, 0.64f), color * 0.48f);
                TankFittingShapeFactory.BuildSmokeBank(
                    "Challenger1",
                    root,
                    V(side * 1.25f, 0.55f, 0.42f),
                    Quaternion.Euler(0f, side * 58f, 0f),
                    5,
                    0.044f,
                    0.27f,
                    -0.38f,
                    0.28f,
                    0.60f,
                    0.095f,
                    color * 0.42f,
                    Dark());
            }
            AddBritishRoofKit(root, definition, color, "Challenger1", true);
        }

        private static void BuildCenturionPatternTurret(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            string prefix,
            float halfWidth,
            float roofY,
            bool late)
        {
            Transform root = NewRoot(turret, prefix + "-TurretPresentationRoot");
            Box("Painted-" + prefix + "-CastTurretShell", root,
                V(0f, 0.28f, -0.12f),
                V(halfWidth * 1.72f, 0.56f, 1.55f),
                color * 0.60f);
            Box("Painted-" + prefix + "-CastNose", root,
                V(0f, 0.20f, 0.92f),
                V(halfWidth * 1.25f, 0.40f, 0.78f),
                color * 0.58f)
                .localRotation = Quaternion.Euler(-5f, 0f, 0f);
            Box("Painted-" + prefix + "-BustleShelf", root,
                V(0f, 0.48f, -1.10f),
                V(halfWidth * 1.55f, 0.26f, 0.82f),
                color * 0.55f);
            AddBritishRoofKit(root, definition, color, prefix, late);
            if (prefix == "Vickers")
            {
                TankPintleMachineGunFactory.Build(
                    "Vickers-L7A1",
                    root,
                    V(-0.56f, roofY + 0.15f, -0.48f),
                    Quaternion.Euler(0f, -10f, 0f),
                    TankMachineGunClass.Mag58,
                    0.82f,
                    0.02f,
                    true,
                    TankMachineGunShield.None,
                    true,
                    Dark(),
                    color * 0.50f);
            }
        }

        private static void AddBritishRoofKit(
            Transform root,
            VehicleDefinition definition,
            Color color,
            string prefix,
            bool late)
        {
            float y = late ? 0.72f : 0.66f;
            Cylinder("Painted-" + prefix + "-CommanderCupola", root,
                V(0.44f, y, -0.36f),
                0.25f, 0.27f, 0.12f, 16,
                TankShapeAxis.Y, color * 0.60f);
            Cylinder("Painted-" + prefix + "-LoaderHatch", root,
                V(-0.42f, y - 0.02f, -0.28f),
                0.22f, 0.22f, 0.045f, 14,
                TankShapeAxis.Y, color * 0.64f);
            Box(prefix + "-GunnerSightLens", root,
                V(-0.38f, y + 0.04f, 0.54f),
                V(0.18f, 0.08f, 0.018f), Glass());
            Box("Painted-" + prefix + "-GunnerSightHood", root,
                V(-0.38f, y + 0.04f, 0.42f),
                V(0.30f, 0.16f, 0.26f), color * 0.58f);
            TankPintleMachineGunFactory.Build(
                prefix + "-RoofMG",
                root,
                V(0.44f, y + 0.08f, -0.40f),
                Quaternion.Euler(0f, 4f, 0f),
                TankMachineGunClass.Mag58,
                0.72f,
                0.0f,
                true,
                TankMachineGunShield.None,
                true,
                Dark(),
                color * 0.50f);
            string number = NumericMarking(definition?.visual?.number);
            if (!string.IsNullOrEmpty(number))
            {
                TankTacticalNumberFactory.BuildPair(
                    prefix,
                    root,
                    number,
                    0.25f,
                    V(1.22f, 0.35f, 0.08f),
                    Quaternion.Euler(0f, 90f, 0f),
                    V(-1.22f, 0.35f, 0.08f),
                    Quaternion.Euler(0f, -90f, 0f));
            }
        }

        private static void AddL11Gun(
            Transform turret,
            string prefix,
            Color color,
            float length,
            float radius)
        {
            AddGun(turret, prefix, "L11A5", color, length, radius);
        }

        private static void AddL7Gun(
            Transform turret,
            string prefix,
            Color color,
            float length)
        {
            AddGun(turret, prefix, "L7", color, length, 0.082f);
        }

        private static void AddGun(
            Transform turret,
            string prefix,
            string weapon,
            Color color,
            float length,
            float radius)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform root =
                new GameObject(prefix + "-" + weapon + "GunAssembly")
                    .transform;
            root.SetParent(gun, false);
            Vector3 scale = gun.localScale;
            Vector3 sourcePivot = V(0f, 0.30f, 0.78f);
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
            Box("Painted-" + prefix + "-MantletBlock", root,
                V(0f, 0f, 0.06f),
                V(0.72f, 0.34f, 0.24f), color * 0.50f);
            Cylinder("Painted-" + prefix + "-" + weapon + "RootSleeve", root,
                V(0f, -0.015f, 0.70f),
                radius * 1.45f, radius * 1.45f, 0.72f, 18,
                TankShapeAxis.Z, color * 0.45f);
            Cylinder("Painted-" + prefix + "-" + weapon + "ThermalSleeve", root,
                V(0f, -0.030f, 2.15f),
                radius * 1.18f, radius * 1.18f, 1.88f, 24,
                TankShapeAxis.Z, color * 0.45f);
            Cylinder(prefix + "-" + weapon + "FumeExtractor", root,
                V(0f, -0.030f, 3.05f),
                radius * 1.55f, radius * 1.38f, 0.48f, 18,
                TankShapeAxis.Z, color * 0.42f);
            Cylinder("Painted-" + prefix + "-" + weapon + "ForwardTube", root,
                V(0f, -0.040f, (3.15f + length) * 0.5f),
                radius, radius, length - 3.15f, 24,
                TankShapeAxis.Z, color * 0.45f);
            Cylinder(prefix + "-" + weapon + "MuzzleCollar", root,
                V(0f, -0.035f, length - 0.08f),
                radius * 1.22f, radius * 1.22f, 0.12f, 16,
                TankShapeAxis.Z, color * 0.45f);
        }

        private static Transform NewRoot(
            Transform turret,
            string name)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(turret, false);
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
