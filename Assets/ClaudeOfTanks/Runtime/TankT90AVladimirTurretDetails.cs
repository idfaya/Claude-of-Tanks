using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90AVladimirTurretDetails
    {
        private static readonly Vector2[] Plan =
        {
            new Vector2(-1.45f, 0.58f),
            new Vector2(-1.25f, 0.80f),
            new Vector2(-1.05f, 1.02f),
            new Vector2(-0.75f, 1.07f),
            new Vector2(-0.30f, 0.98f),
            new Vector2(0.30f, 0.98f),
            new Vector2(0.65f, 0.95f),
            new Vector2(0.80f, 1.07f),
            new Vector2(1.05f, 1.05f),
            new Vector2(1.25f, 0.80f),
            new Vector2(1.45f, 0.58f),
            new Vector2(1.45f, -0.10f),
            new Vector2(1.25f, -0.45f),
            new Vector2(1.05f, -0.62f),
            new Vector2(0.80f, -0.72f),
            new Vector2(0.30f, -0.80f),
            new Vector2(-0.30f, -0.80f),
            new Vector2(-0.80f, -0.72f),
            new Vector2(-1.05f, -0.62f),
            new Vector2(-1.25f, -0.45f),
            new Vector2(-1.45f, -0.10f)
        };

        public static void Build(
            Transform turret,
            Color color)
        {
            HideRenderer(turret.Find("Turret"));
            GameObject rootObject =
                new GameObject("T90AVladimir-PresentationRoot");
            Transform root = rootObject.transform;
            root.SetParent(turret, false);
            root.localPosition = new Vector3(0f, 0.10f, -0.90f);

            AddCastFoundation(root, color);
            AddKontakt5(root, color);
            AddShtora(root, color);
            AddEssa(root, color);
            AddSmokeBanks(root, color);
            AddKord(root, color);
            AddBustle(root, color);
        }

        private static void AddCastFoundation(
            Transform root,
            Color color)
        {
            Transform lower = TankShapeFactory.PolyTurretPart(
                "Painted-T90AVladimir-LowerCheek",
                root,
                Plan,
                0.25f,
                0.94f,
                1.02f,
                color * 0.62f);
            lower.localPosition = new Vector3(0f, -0.015f, 0f);
            Transform upper = TankShapeFactory.PolyTurretPart(
                "Painted-T90AVladimir-UpperCheek",
                root,
                Plan,
                0.333f,
                1.02f,
                0.58f,
                color * 0.62f);
            upper.localPosition = new Vector3(0f, 0.235f, 0f);
            TankShapeFactory.LathePart(
                "Painted-T90AVladimir-Crown",
                root,
                new[] { 0.82f, 0.70f, 0.52f, 0.30f, 0.02f },
                new[] { 0.44f, 0.48f, 0.51f, 0.54f, 0.55f },
                32,
                0.75f,
                color * 0.64f,
                1.60f);
            Box("Painted-T90AVladimir-LeftSideHead", root,
                V(-1.27f, 0.12f, -0.55f),
                V(0.19f, 0.38f, 0.50f), color * 0.56f)
                .localRotation = Quaternion.Euler(0f, 4.58f, 0f);
            Box("Painted-T90AVladimir-RightSideHead", root,
                V(1.28f, 0.15f, -0.44f),
                V(0.17f, 0.24f, 0.30f), color * 0.56f)
                .localRotation = Quaternion.Euler(0f, -4.58f, 0f);
            Cylinder("Painted-T90AVladimir-CommanderPad", root,
                V(-0.36f, 0.535f, -0.42f),
                0.44f, 0.54f, 0.026f, 18,
                TankShapeAxis.Y, color * 0.60f);
            Cylinder("Painted-T90AVladimir-GunnerPad", root,
                V(0.54f, 0.525f, -0.18f),
                0.34f, 0.42f, 0.024f, 18,
                TankShapeAxis.Y, color * 0.60f);
        }

        private static void AddKontakt5(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T90AVladimir-K5Inner", root,
                    V(side * 0.34f, 0.30f, 1.10f),
                    V(0.55f, 0.21f, 0.34f), color * 0.52f)
                    .localRotation =
                        Quaternion.Euler(-13f, -side * 22f, 0f);
                Box("Painted-T90AVladimir-K5Outer", root,
                    V(side * 0.98f, 0.26f, 0.72f),
                    V(0.72f, 0.21f, 0.40f), color * 0.52f)
                    .localRotation =
                        Quaternion.Euler(-13f, -side * 42f, 0f);
                float[] flankZ = { 0.40f, 0.64f, 0.85f };
                float[] flankX = { 1.42f, 1.30f, 1.20f };
                for (int column = 0; column < flankZ.Length; column++)
                {
                    for (int row = 0; row < 2; row++)
                    {
                        Box("Painted-T90AVladimir-K5Flank", root,
                            V(
                                side * flankX[column],
                                0.27f + row * 0.305f,
                                flankZ[column]),
                            V(0.11f, 0.30f, 0.34f),
                            color * 0.50f);
                    }
                }
            }
        }

        private static void AddShtora(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T90AVladimir-ShtoraSupport", root,
                    V(side * 0.60f, 0.20f, 1.04f),
                    V(0.34f, 0.28f, 0.44f), color * 0.48f)
                    .localRotation =
                        Quaternion.Euler(-12.6f, -side * 6.9f, 0f);
                Box("T90AVladimir-ShtoraHousing", root,
                    V(side * 0.60f, 0.28f, 1.24f),
                    V(0.36f, 0.405f, 0.33f),
                    TankT90AFamilyDetails.Dark());
                Cylinder("T90AVladimir-ShtoraLens", root,
                    V(side * 0.60f, 0.28f, 1.414f),
                    0.108f, 0.108f, 0.022f, 18,
                    TankShapeAxis.Z,
                    TankT90AFamilyDetails.ShtoraGlass());
            }
        }

        private static void AddEssa(
            Transform root,
            Color color)
        {
            Box("T90AVladimir-ESSAFoot", root,
                V(-0.72f, 0.535f, 0.18f),
                V(0.28f, 0.025f, 0.90f),
                TankT90AFamilyDetails.Dark());
            Box("Painted-T90AVladimir-ESSAHousing", root,
                V(-0.72f, 0.665f, 0.18f),
                V(0.25f, 0.26f, 0.90f), color * 0.60f);
            Box("T90AVladimir-ESSALens", root,
                V(-0.72f, 0.70f, 0.639f),
                V(0.20f, 0.11f, 0.018f),
                TankT90AFamilyDetails.Glass());
            Box("Painted-T90AVladimir-ESSAServiceBody", root,
                V(-0.98f, 0.69f, -0.22f),
                V(0.42f, 0.30f, 0.50f), color * 0.58f);
        }

        private static void AddSmokeBanks(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T90AVladimir-SmokeShoe", root,
                    V(side * 1.18f, 0.45f, 0.32f),
                    V(0.22f, 0.22f, 0.46f), color * 0.50f);
                GameObject bankObject = new GameObject(
                    side < 0
                        ? "T90AVladimir-SmokeBankL"
                        : "T90AVladimir-SmokeBankR");
                Transform bank = bankObject.transform;
                bank.SetParent(root, false);
                bank.localPosition = V(side * 1.21f, 0.51f, 0.35f);
                bank.localRotation =
                    Quaternion.Euler(0f, side * 59.59f, 0f);
                for (int index = 0; index < 6; index++)
                {
                    Transform tube = Cylinder(
                        "Painted-T90AVladimir-SmokeCanister",
                        bank,
                        V(
                            (index - 2.5f) * 0.10f,
                            0f,
                            0f),
                        0.042f, 0.042f, 0.30f, 12,
                        TankShapeAxis.Z,
                        color * 0.44f);
                    tube.localRotation =
                        Quaternion.Euler(-22.92f, 0f, 0f);
                }
            }
        }

        private static void AddKord(
            Transform root,
            Color color)
        {
            Box("Painted-T90AVladimir-KordTower", root,
                V(0.38f, 0.68f, -0.45f),
                V(0.34f, 0.32f, 0.38f), color * 0.52f);
            Box("T90AVladimir-KordReceiver", root,
                V(0.38f, 0.91f, -0.40f),
                V(0.18f, 0.14f, 0.42f),
                TankT90AFamilyDetails.Dark());
            Box("T90AVladimir-KordBarrel", root,
                V(0.38f, 0.94f, 0.02f),
                V(0.035f, 0.035f, 0.70f),
                TankT90AFamilyDetails.Dark());
        }

        private static void AddBustle(
            Transform root,
            Color color)
        {
            Box("Painted-T90AVladimir-Bustle", root,
                V(0f, 0.30f, -1.72f),
                V(2.10f, 0.50f, 1.55f), color * 0.55f);
            float[] xs = { -0.88f, -0.47f, 0.40f, 0.70f };
            for (int index = 0; index < xs.Length; index++)
            {
                Box("Painted-T90AVladimir-RearBin", root,
                    V(xs[index], 0.34f, -1.47f),
                    V(0.30f, 0.42f, 0.44f), color * 0.50f);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Box("T90AVladimir-BustleRail", root,
                    V(side * 1.15f, 0.34f, -1.45f),
                    V(0.055f, 0.055f, 1.55f),
                    TankT90AFamilyDetails.Dark());
            }
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part = TankShapeFactory.BoxPart(
                name, parent, size, color);
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

        private static Vector3 V(
            float x,
            float y,
            float z)
        {
            return new Vector3(x, y, z);
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer = part == null
                ? null
                : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }
}
