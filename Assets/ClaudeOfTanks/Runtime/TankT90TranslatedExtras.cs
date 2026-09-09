using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90TranslatedExtras
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            if (root == null || turret == null) return;
            AddHullMeshSurfaces(root, color);
            TankT90TranslatedGearPads.Build(root, color);
            TankT90TranslatedSuspension.Build(root, color);
            AddTurretMeshSurfaces(turret, color);
            AddKontakt5TurretSeams(turret, color);
            AddTurretRoofDetails(turret, color);
            AddBustleRackDetails(turret);
            AddGunMountDetails(turret);
            AddHullSkirtFasteners(root);
            AddRearTowEyes(root);
        }

        private static void AddHullMeshSurfaces(
            Transform root,
            Color color)
        {
            TankShapeFactory.FrustumPart(
                "Painted-T90-CSharpUpperHullWedge",
                root,
                1.55f,
                2.18f,
                -2.68f,
                1.33f,
                2.18f,
                -2.68f,
                0.96f,
                1.36f,
                color * 0.58f);
            TankShapeFactory.OrientedSlabPart(
                "Painted-T90-CSharpSweptGlacisMesh",
                root,
                V(-1.22f, 1.18f, 2.1f),
                V(1.22f, 1.18f, 2.1f),
                V(1.42f, 1.0f, 2.95f),
                V(-1.42f, 1.0f, 2.95f),
                V(-1.22f, 1.26f, 2.1f),
                V(1.22f, 1.26f, 2.1f),
                V(1.42f, 1.08f, 2.95f),
                V(-1.42f, 1.08f, 2.95f),
                color * 0.64f);
            TankShapeFactory.OrientedSlabPart(
                "T90-CSharpRearDeckStep",
                root,
                V(-1.28f, 1.42f, -2.72f),
                V(1.28f, 1.42f, -2.72f),
                V(1.45f, 1.32f, -1.4f),
                V(-1.45f, 1.32f, -1.4f),
                V(-1.28f, 1.48f, -2.72f),
                V(1.28f, 1.48f, -2.72f),
                V(1.45f, 1.38f, -1.4f),
                V(-1.45f, 1.38f, -1.4f),
                color * 0.45f);
        }

        private static void AddTurretMeshSurfaces(
            Transform turret,
            Color color)
        {
            TankShapeFactory.LathePart(
                "Painted-T90-CSharpCastDomeMesh",
                turret,
                new[]
                {
                    1.48f,
                    1.62f,
                    1.58f,
                    1.42f,
                    1.14f,
                    0.8f,
                    0.42f,
                    0.02f
                },
                new[]
                {
                    0f,
                    0.13f,
                    0.31f,
                    0.46f,
                    0.6f,
                    0.7f,
                    0.765f,
                    0.79f
                },
                30,
                0.72f,
                color * 0.64f,
                2.7f);
            TankShapeFactory.OrientedSlabPart(
                "Painted-T90-CSharpTurretCheekWedge-L",
                turret,
                V(-0.22f, 0.36f, 1.33f),
                V(-1.26f, 0.34f, 0.86f),
                V(-1.36f, 0.62f, 0.44f),
                V(-0.34f, 0.7f, 0.66f),
                V(-0.22f, 0.45f, 1.33f),
                V(-1.26f, 0.43f, 0.86f),
                V(-1.36f, 0.71f, 0.44f),
                V(-0.34f, 0.79f, 0.66f),
                color * 0.56f);
            TankShapeFactory.OrientedSlabPart(
                "Painted-T90-CSharpTurretCheekWedge-R",
                turret,
                V(0.22f, 0.36f, 1.33f),
                V(1.26f, 0.34f, 0.86f),
                V(1.36f, 0.62f, 0.44f),
                V(0.34f, 0.7f, 0.66f),
                V(0.22f, 0.45f, 1.33f),
                V(1.26f, 0.43f, 0.86f),
                V(1.36f, 0.71f, 0.44f),
                V(0.34f, 0.79f, 0.66f),
                color * 0.56f);
        }

        private static void AddKontakt5TurretSeams(
            Transform turret,
            Color color)
        {
            Part("T90-K5RoofVerticalSeam", PrimitiveType.Cube, turret,
                V(-0.4f, 0.802f, -0.05f), V(0.028f, 0.052f, 0.7f),
                V(), Dark());
            Part("T90-K5RoofVerticalSeam", PrimitiveType.Cube, turret,
                V(0.37f, 0.802f, -0.05f), V(0.028f, 0.052f, 0.7f),
                V(), Dark());
            Part("T90-K5RoofEdgeSeam", PrimitiveType.Cube, turret,
                V(-0.02f, 0.832f, 0.25f), V(0.7f, 0.012f, 0.03f),
                V(), Dark());
            Part("T90-K5RoofEdgeSeam", PrimitiveType.Cube, turret,
                V(-0.02f, 0.832f, -0.55f), V(0.7f, 0.012f, 0.03f),
                V(), Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-T90-K5InnerLeafCap", PrimitiveType.Cube,
                    turret, V(side * 0.98f, 0.44f, 1.03f),
                    V(0.06f, 0.32f, 0.46f),
                    V(-25f, -side * 25f, 0f), color * 0.53f);
                Part("Painted-T90-K5OuterLeafCap", PrimitiveType.Cube,
                    turret, V(side * 1.55f, 0.4f, 0.66f),
                    V(0.06f, 0.32f, 0.32f),
                    V(-23f, -side * 41f, 0f), color * 0.53f);
                Part("T90-K5LeafOuterEdgeSeam", PrimitiveType.Cube,
                    turret, V(side * 0.62f, 0.44f, 1.16f),
                    V(0.024f, 0.3f, 0.008f),
                    V(-25f, -side * 25f, 0f), Dark());
                Part("T90-K5LeafTopSeam", PrimitiveType.Cube,
                    turret, V(side * 0.62f, 0.31f, 1.16f),
                    V(0.66f, 0.03f, 0.008f),
                    V(-25f, -side * 25f, 0f), Dark());
                Part("T90-K5LeafOuterEdgeSeam", PrimitiveType.Cube,
                    turret, V(side * 1.265f, 0.4f, 0.72f),
                    V(0.024f, 0.3f, 0.008f),
                    V(-23f, -side * 41f, 0f), Dark());
                Part("T90-K5LeafTopSeam", PrimitiveType.Cube,
                    turret, V(side * 1.265f, 0.265f, 0.72f),
                    V(0.72f, 0.03f, 0.008f),
                    V(-23f, -side * 41f, 0f), Dark());
            }
        }

        private static void AddTurretRoofDetails(
            Transform turret,
            Color color)
        {
            Part("Painted-T90-CupolaGuardArc", PrimitiveType.Cube, turret,
                V(0.52f, 0.83f, -0.01f), V(0.5f, 0.035f, 0.055f),
                V(0f, 18f, 0f), color * 0.5f);
            Part("Painted-T90-CupolaGuardArc", PrimitiveType.Cube, turret,
                V(0.76f, 0.83f, -0.3f), V(0.055f, 0.035f, 0.42f),
                V(0f, -12f, 0f), color * 0.5f);
            Part("Painted-T90-GunnerThermalHead", PrimitiveType.Cube, turret,
                V(-0.34f, 0.86f, 0.03f), V(0.32f, 0.16f, 0.26f),
                V(0f, -8f, 0f), color * 0.62f);
            Part("T90-GunnerThermalLens", PrimitiveType.Cube, turret,
                V(-0.34f, 0.87f, 0.18f), V(0.22f, 0.09f, 0.018f),
                V(), Glass());
            for (int index = 0; index < 5; index++)
                Part("Painted-T90-CupolaPeriscope", PrimitiveType.Cube,
                    turret,
                    V(0.52f + (index - 2) * 0.1f, 0.87f,
                        -0.04f - Mathf.Abs(index - 2) * 0.04f),
                    V(0.075f, 0.035f, 0.08f),
                    V(0f, (index - 2) * 12f, 0f), color * 0.45f);
        }

        private static void AddBustleRackDetails(
            Transform turret)
        {
            Part("Painted-T90-BustleCargoBox", PrimitiveType.Cube, turret,
                V(0f, 0.42f, -1.78f), V(1.3f, 0.44f, 0.24f),
                V(), new Color(0.28f, 0.34f, 0.24f));
            Part("T90-BustleRearMesh", PrimitiveType.Cube, turret,
                V(0f, 0.5f, -1.755f), V(1.42f, 0.44f, 0.03f),
                V(), Dark());
            Part("T90-OpvtBaseCollar", PrimitiveType.Cylinder, turret,
                V(0.32f, 0.4f, -1.13f), V(0.058f, 0.05f, 0.058f),
                V(), Dark());
            Part("T90-OpvtRackStay", PrimitiveType.Cube, turret,
                V(0.32f, 0.62f, -1.16f), V(0.03f, 0.03f, 0.3f),
                V(), Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Part("T90-BustleSideRail", PrimitiveType.Cube, turret,
                    V(side * 0.82f, 0.78f, -1.53f),
                    V(0.035f, 0.3f, 0.5f), V(), Dark());
                for (int rung = 0; rung < 3; rung++)
                    Part("T90-BustleRung", PrimitiveType.Cube, turret,
                        V(side * 0.42f, 0.66f + rung * 0.095f, -1.78f),
                        V(0.68f, 0.025f, 0.035f), V(), Dark());
            }
            Transform cable = TankShapeFactory.TorusPart(
                "T90-BustleCableCoil",
                turret,
                0.15f,
                0.017f,
                20,
                Dark());
            cable.localPosition = V(0.45f, 0.42f, -1.43f);
            cable.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddGunMountDetails(
            Transform turret)
        {
            Part("T90-CannonBaseBoot", PrimitiveType.Cylinder, turret,
                V(0f, 0.32f, 1.08f), V(0.5f, 0.15f, 0.4f),
                V(90f, 0f, 0f), Dark());
            Part("T90-GunMountAccessPlate", PrimitiveType.Cube, turret,
                V(-0.245f, 0.55f, 0.341f), V(0.15f, 0.1f, 0.016f),
                V(), Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Part("T90-GunMountCheekPlate", PrimitiveType.Cube, turret,
                    V(side * 0.1425f, 0.35f, 0.958f),
                    V(0.175f, 0.18f, 0.016f), V(), Dark());
                Part("T90-GunMountLatch", PrimitiveType.Cube, turret,
                    V(side * 0.085f, 0.3015f, 1.302f),
                    V(0.06f, 0.043f, 0.014f), V(), Dark());
            }
            Part("T90-GunMountCenterPlate", PrimitiveType.Cube, turret,
                V(0f, 0.25f, 0.958f), V(0.11f, 0.1f, 0.016f),
                V(), Dark());
        }

        private static void AddHullSkirtFasteners(
            Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            for (int station = 0; station < 5; station++)
            {
                Part("T90-SkirtHingeBracket", PrimitiveType.Cube, root,
                    V(side * 1.86f, 1.39f, -1.28f + station * 0.76f),
                    V(0.035f, 0.08f, 0.12f), V(), Dark());
                Part("T90-SkirtLowerPin", PrimitiveType.Cylinder, root,
                    V(side * 1.86f, 0.66f, -1.28f + station * 0.76f),
                    V(0.022f, 0.026f, 0.022f), V(0f, 0f, 90f), Dark());
            }
        }

        private static void AddRearTowEyes(Transform root)
        {
            float[] xPositions = { -0.56f, 0.48f };
            float[] yPositions = { 0.72f, 0.69f };
            for (int index = 0; index < xPositions.Length; index++)
            {
                Transform eye = TankShapeFactory.TorusPart(
                    "T90-RearTowEye",
                    root,
                    0.084f,
                    0.02f,
                    14,
                    Dark());
                eye.localPosition =
                    V(xPositions[index], yPositions[index], -3.54f);
                eye.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Vector3 rotation,
            Color color)
        {
            Transform part;
            if (type == PrimitiveType.Cube)
            {
                part = TankShapeFactory.BoxPart(
                    name,
                    parent,
                    scale,
                    color);
                part.localPosition = position;
            }
            else
            {
                part = TankDetailGeometry.Part(
                    name,
                    type,
                    parent,
                    position,
                    scale,
                    color);
            }
            part.localRotation = Quaternion.Euler(rotation);
            return part;
        }

        private static Vector3 V(
            float x = 0f,
            float y = 0f,
            float z = 0f)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.055f, 0.065f, 0.05f);
        }

        private static Color Glass()
        {
            return new Color(0.025f, 0.08f, 0.075f);
        }
    }
}
