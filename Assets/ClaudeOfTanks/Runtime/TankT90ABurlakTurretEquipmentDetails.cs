using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90ABurlakTurretEquipmentDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            if (root == null) return;
            AddRoofHierarchy(root, color);
            AddBustleFeedDeck(root);
            AddAutomatedNsvt(root, color);
            AddSmokeBanks(root, color);
            AddAntenna(root);
        }

        private static void AddRoofHierarchy(
            Transform root,
            Color color)
        {
            Box("Painted-T90ABurlak-PanoramaSeat", root,
                V(0.38f, 0.63f, -0.70f),
                V(0.52f, 0.14f, 0.48f), color * 0.58f);
            Transform race = TankShapeFactory.TorusPart(
                "T90ABurlak-PanoramaRace",
                root,
                0.22f,
                0.022f,
                18,
                Dark());
            race.localPosition = V(0.38f, 0.71f, -0.70f);
            Transform head = TankShapeFactory.OrientedSlabPart(
                "Painted-T90ABurlak-PanoramaHead",
                root,
                V(-0.15f, -0.18f, -0.14f),
                V(0.15f, -0.18f, -0.14f),
                V(0.15f, -0.18f, 0.14f),
                V(-0.15f, -0.18f, 0.14f),
                V(-0.11f, 0.18f, -0.10f),
                V(0.11f, 0.18f, -0.10f),
                V(0.11f, 0.18f, 0.10f),
                V(-0.11f, 0.18f, 0.10f),
                color * 0.56f);
            head.localPosition = V(0.38f, 0.92f, -0.70f);
            Box("T90ABurlak-PanoramaLens", root,
                V(0.38f, 0.92f, -0.555f),
                V(0.19f, 0.16f, 0.012f), Glass());

            Cylinder("Painted-T90ABurlak-Hatch", root,
                V(-0.48f, 0.78f, -0.34f),
                0.25f, 0.28f, 0.10f, 18,
                TankShapeAxis.Y, color * 0.58f);
            Cylinder("T90ABurlak-HatchRim", root,
                V(-0.48f, 0.84f, -0.34f),
                0.22f, 0.22f, 0.022f, 16,
                TankShapeAxis.Y, Dark());
            Cylinder("Painted-T90ABurlak-Hatch", root,
                V(0.38f, 0.77f, -0.24f),
                0.20f, 0.23f, 0.09f, 16,
                TankShapeAxis.Y, color * 0.58f);
            Cylinder("T90ABurlak-HatchRim", root,
                V(0.38f, 0.825f, -0.24f),
                0.17f, 0.17f, 0.020f, 14,
                TankShapeAxis.Y, Dark());

            Box("Painted-T90ABurlak-LeftRoofFoundation", root,
                V(-1.12f, 0.67f, -0.28f),
                V(0.34f, 0.13f, 0.86f), color * 0.56f);
            Box("T90ABurlak-LeftRoofFoundationLid", root,
                V(-1.12f, 0.742f, -0.28f),
                V(0.29f, 0.014f, 0.78f), Dark());
            float[,] periscopes =
            {
                { -0.78f, -0.29f, -0.22f },
                { -0.58f, -0.22f, 0f },
                { -0.38f, -0.29f, 0.22f },
                { 0.18f, -0.07f, -0.18f },
                { 0.47f, -0.10f, 0.18f }
            };
            for (int index = 0; index < periscopes.GetLength(0); index++)
            {
                float x = periscopes[index, 0];
                float z = periscopes[index, 1];
                float yaw = periscopes[index, 2];
                Transform body = Box(
                    "T90ABurlak-PeriscopeBody",
                    root,
                    V(x, 0.79f, z),
                    V(0.12f, 0.055f, 0.07f),
                    Dark());
                body.localRotation =
                    Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f);
                Transform lens = Box(
                    "T90ABurlak-PeriscopeLens",
                    root,
                    V(x, 0.802f, z + 0.04f),
                    V(0.08f, 0.032f, 0.01f),
                    Glass());
                lens.localRotation = body.localRotation;
            }
            Transform sight = Box(
                "Painted-T90ABurlak-ForwardSight",
                root,
                V(0.36f, 0.57f, 0.31f),
                V(0.30f, 0.15f, 0.28f),
                color * 0.56f);
            sight.localRotation =
                Quaternion.Euler(0f, -0.12f * Mathf.Rad2Deg, 0f);
            Box("T90ABurlak-ForwardSightFace", root,
                V(0.36f, 0.58f, 0.462f),
                V(0.25f, 0.09f, 0.014f), Dark())
                .localRotation = sight.localRotation;
            Box("T90ABurlak-ForwardSightLens", root,
                V(0.36f, 0.58f, 0.471f),
                V(0.19f, 0.06f, 0.008f), Glass())
                .localRotation = sight.localRotation;
        }

        private static void AddBustleFeedDeck(Transform root)
        {
            Box("T90ABurlak-AutoloaderFeedLid", root,
                V(0.02f, 0.695f, -2.18f),
                V(0.72f, 0.014f, 0.66f), Dark());
            foreach (float x in new[] { -0.31f, 0.35f })
            {
                Box("T90ABurlak-AutoloaderRubRail", root,
                    V(x, 0.71f, -2.19f),
                    V(0.03f, 0.022f, 1.42f), Detail());
            }
        }

        private static void AddAutomatedNsvt(
            Transform root,
            Color color)
        {
            const float x = -0.46f;
            const float z = -0.42f;
            const float scale = 0.58f;
            const float heightScale = 0.65f;
            const float foundationTop = 0.80f +
                0.15f * scale * heightScale;
            const float headCenter = foundationTop +
                0.18f * scale * heightScale;
            const float weaponFoot = foundationTop +
                0.04f * scale * heightScale;
            Box("Painted-T90ABurlak-NsvtFoundation", root,
                V(x, 0.80f + 0.08f * scale * heightScale, z + 0.03f * scale),
                V(0.62f * scale, 0.18f * scale * heightScale, 0.58f * scale),
                color * 0.54f);
            Transform taper = TankShapeFactory.OrientedSlabPart(
                "Painted-T90ABurlak-NsvtTaper",
                root,
                V(-0.25f * scale, -0.15f * scale * heightScale, -0.21f * scale),
                V(0.25f * scale, -0.15f * scale * heightScale, -0.21f * scale),
                V(0.25f * scale, -0.15f * scale * heightScale, 0.21f * scale),
                V(-0.25f * scale, -0.15f * scale * heightScale, 0.21f * scale),
                V(-0.18f * scale, 0.15f * scale * heightScale, -0.15f * scale),
                V(0.18f * scale, 0.15f * scale * heightScale, -0.15f * scale),
                V(0.18f * scale, 0.15f * scale * heightScale, 0.15f * scale),
                V(-0.18f * scale, 0.15f * scale * heightScale, 0.15f * scale),
                color * 0.52f);
            taper.localPosition = V(x, foundationTop, z + 0.03f * scale);
            Box("Painted-T90ABurlak-NsvtHead", root,
                V(x, headCenter, z + 0.04f * scale),
                V(0.40f * scale, 0.34f * scale * heightScale, 0.36f * scale),
                color * 0.52f);
            Box("T90ABurlak-NsvtHeadCap", root,
                V(x, headCenter + 0.19f * scale * heightScale, z + 0.04f * scale),
                V(0.44f * scale, 0.045f * scale * heightScale, 0.40f * scale),
                Dark());
            Box("Painted-T90ABurlak-NsvtOptic", root,
                V(x + 0.27f * scale, headCenter, z + 0.21f * scale),
                V(0.19f * scale, 0.21f * scale * heightScale, 0.22f * scale),
                color * 0.5f);
            Box("T90ABurlak-NsvtOpticLens", root,
                V(x + 0.27f * scale, headCenter, z + 0.328f * scale),
                V(0.13f * scale, 0.13f * scale * heightScale, 0.014f * scale),
                Glass());

            Transform weapon = Root(
                "T90ABurlak-CommanderNsvt",
                root,
                V(x, weaponFoot, z + 0.10f * scale));
            const float classScale = scale * 0.98f;
            float receiverWidth = 0.095f * classScale;
            float receiverHeight = 0.10f * classScale;
            float receiverDepth = 0.42f * classScale;
            float columnHeight = 0.16f * classScale;
            float columnTop = 0.014f + columnHeight;
            float receiverY =
                columnTop + 0.08f * classScale + receiverHeight * 0.5f;
            float receiverZ = 0.06f * classScale;
            Cylinder("T90ABurlak-NsvtBearing", weapon,
                V(0f, 0.007f, 0f),
                0.03f * classScale, 0.038f * classScale,
                0.014f, 14, TankShapeAxis.Y, Dark());
            Cylinder("T90ABurlak-NsvtSpindle", weapon,
                V(0f, 0.014f + columnHeight * 0.5f, 0f),
                0.018f * classScale, 0.023f * classScale,
                columnHeight, 12, TankShapeAxis.Y, Dark());
            Box("T90ABurlak-NsvtReceiver", weapon,
                V(0f, receiverY, receiverZ),
                V(receiverWidth, receiverHeight, receiverDepth), Dark());
            Box("T90ABurlak-NsvtAmmoCan", weapon,
                V(-(receiverWidth * 0.5f + 0.055f * classScale),
                    receiverY - 0.005f,
                    receiverZ - 0.02f),
                V(0.085f * classScale, 0.11f * classScale, 0.17f * classScale),
                Detail());
            float trunnionY = receiverY + 0.004f;
            float trunnionZ = receiverZ + receiverDepth * 0.5f;
            const float elevation = -0.075f;
            Quaternion aim =
                Quaternion.Euler(-elevation * Mathf.Rad2Deg, 0f, 0f);
            Transform bridge = Cylinder(
                "T90ABurlak-NsvtBarrelBridge", weapon,
                V(0f, trunnionY, trunnionZ + 0.0525f * classScale),
                0.024f * classScale * 1.12f,
                0.024f * classScale * 1.12f,
                0.105f * classScale,
                10, TankShapeAxis.Z, Dark());
            bridge.localRotation = aim;
            float barrelLength = 0.55f * classScale;
            Transform barrel = Cylinder(
                "T90ABurlak-NsvtBarrel", weapon,
                V(0f, trunnionY,
                    trunnionZ + 0.10f * classScale + barrelLength * 0.5f),
                0.024f * classScale,
                0.024f * classScale,
                barrelLength,
                10, TankShapeAxis.Z, Dark());
            barrel.localRotation = aim;
            Transform flash = Cylinder(
                "T90ABurlak-NsvtFlashHider", weapon,
                V(0f, trunnionY,
                    trunnionZ + 0.10f * classScale + barrelLength +
                    0.05f * classScale),
                0.035f * classScale,
                0.035f * classScale,
                0.10f * classScale,
                12, TankShapeAxis.Z, Dark());
            flash.localRotation = aim;
            for (int side = -1; side <= 1; side += 2)
            {
                Transform shield = Box(
                    "T90ABurlak-NsvtShield",
                    weapon,
                    V(side * (0.075f + 0.145f * 0.5f) * classScale,
                        receiverY + 0.018f * classScale,
                        trunnionZ + 0.035f * classScale),
                    V(0.145f * classScale, 0.22f * classScale,
                        0.022f * classScale),
                    Dark());
                shield.localRotation =
                    Quaternion.Euler(0f, -side * 3.15127f, side * 2.00535f);
            }
        }

        private static void AddSmokeBanks(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T90ABurlak-SmokeShoe", root,
                    V(side * 1.30f, 0.32f, 0.12f),
                    V(0.20f, 0.16f, 0.36f), color * 0.52f)
                    .localRotation =
                        Quaternion.Euler(0f, 0f, -side * 8.59437f);
                TankFittingShapeFactory.BuildSmokeBank(
                    "T90ABurlak",
                    root,
                    V(side * 1.34f, 0.45f, 0.13f),
                    Quaternion.Euler(
                        0f,
                        side * 1.02f * Mathf.Rad2Deg,
                        0f),
                    side < 0 ? 6 : 5,
                    0.04f,
                    0.27f,
                    -0.41f,
                    0.30f,
                    0.56f,
                    0.098f,
                    Detail(),
                    Dark());
            }
        }

        private static void AddAntenna(Transform root)
        {
            TankFittingShapeFactory.BuildAntennaWhip(
                "T90ABurlak",
                root,
                V(0f, 0.63f, -1.02f),
                2.67f,
                0.014f,
                0.018f,
                Detail(),
                Dark());
            Cylinder("T90ABurlak-AntennaBase", root,
                V(0f, 0.63f, -1.02f),
                0.045f, 0.06f, 0.12f, 8,
                TankShapeAxis.Y, Dark());
        }

        private static Transform Root(
            string name,
            Transform parent,
            Vector3 position)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            return root;
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
                name, parent, top, bottom, length, segments, axis, color);
            part.localPosition = position;
            return part;
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return TankT90AFamilyDetails.Dark();
        }

        private static Color Detail()
        {
            return new Color(0.16f, 0.17f, 0.15f);
        }

        private static Color Glass()
        {
            return TankT90AFamilyDetails.Glass();
        }
    }
}
