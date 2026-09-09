using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMTurretEquipmentDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddRoofBins(root, color);
            AddSosna(root, color);
            AddPanoramicSight(root);
            AddBackupSight(root);
            AddAutomatedNsvt(root, color);
        }

        private static void AddRoofBins(
            Transform root,
            Color color)
        {
            Box("Painted-T90SM-LeftPlateauBin", root,
                V(-0.77f, 0.70f, -1.28f),
                V(0.12f, 0.29f, 0.46f), color * 0.54f);
            Box("T90SM-LeftPlateauBinLid", root,
                V(-0.77f, 0.826f, -1.28f),
                V(0.10f, 0.022f, 0.42f), Dark());
            Box("Painted-T90SM-LeftPlateauBinBracket", root,
                V(-0.77f, 0.52f, -1.40f),
                V(0.08f, 0.06f, 0.10f), color * 0.56f);

            Box("Painted-T90SM-RightRoofBin", root,
                V(0.79f, 0.6175f, 0.035f),
                V(0.20f, 0.185f, 0.69f), color * 0.56f);
            Box("T90SM-RightRoofBinLid", root,
                V(0.79f, 0.699f, 0.035f),
                V(0.17f, 0.022f, 0.63f), Dark());
            Box("Painted-T90SM-RightRoofCrown", root,
                V(0.79f, 0.745f, -0.205f),
                V(0.18f, 0.07f, 0.21f), color * 0.56f);
            Box("T90SM-RightRoofCrownLid", root,
                V(0.79f, 0.777f, -0.205f),
                V(0.15f, 0.014f, 0.18f), Dark());
            Box("Painted-T90SM-SosnaServiceCassette", root,
                V(0.95f, 0.6275f, 0.035f),
                V(0.12f, 0.155f, 0.69f), color * 0.54f);
            Box("Painted-T90SM-SosnaServiceCrown", root,
                V(0.95f, 0.7475f, -0.205f),
                V(0.12f, 0.085f, 0.21f), color * 0.54f);
            Box("T90SM-SosnaServiceCrownLid", root,
                V(0.95f, 0.782f, -0.205f),
                V(0.10f, 0.016f, 0.19f), Dark());
            foreach (float x in new[] { 0.88f, 0.70f })
            {
                Box("T90SM-RightRoofBinLatch", root,
                    V(x, 0.60f, 0.39f),
                    V(0.022f, 0.05f, 0.014f), Dark());
            }
        }

        private static void AddSosna(
            Transform root,
            Color color)
        {
            Box("T90SM-SosnaHousing", root,
                V(0.30f, 0.6675f, 0.335f),
                V(0.34f, 0.165f, 0.15f), Detail());
            Box("T90SM-SosnaForwardStep", root,
                V(0.30f, 0.64f, 0.45f),
                V(0.30f, 0.11f, 0.08f), Detail());
            Box("Painted-T90SM-SosnaBase", root,
                V(0.30f, 0.565f, 0.645f),
                V(0.30f, 0.10f, 0.31f), color * 0.56f);
            Box("T90SM-SosnaHood", root,
                V(0.30f, 0.682f, 0.475f),
                V(0.30f, 0.026f, 0.03f), Dark());
            Box("T90SM-SosnaAperture", root,
                V(0.30f, 0.635f, 0.497f),
                V(0.24f, 0.07f, 0.012f), Glass());
        }

        private static void AddPanoramicSight(Transform root)
        {
            Cylinder("T90SM-PanoramaNeck", root,
                V(-0.455f, 0.675f, -0.665f),
                0.045f, 0.045f, 0.30f, 12,
                TankShapeAxis.Y, Detail());
            Box("T90SM-PanoramaHead", root,
                V(-0.455f, 0.78f, -0.665f),
                V(0.23f, 0.13f, 0.22f), Detail());
            Box("T90SM-PanoramaWindowSlot", root,
                V(-0.455f, 0.778f, -0.518f),
                V(0.17f, 0.08f, 0.012f), Dark());
            Box("T90SM-PanoramaWindow", root,
                V(-0.455f, 0.778f, -0.512f),
                V(0.13f, 0.06f, 0.008f), Glass());
            Box("T90SM-PanoramaCap", root,
                V(-0.455f, 0.8375f, -0.665f),
                V(0.25f, 0.015f, 0.24f), Dark());
            Box("T90SM-MeteoCluster", root,
                V(-0.635f, 0.745f, -0.665f),
                V(0.13f, 0.13f, 0.26f), Detail());
            Box("T90SM-MeteoClusterLid", root,
                V(-0.635f, 0.80f, -0.665f),
                V(0.09f, 0.022f, 0.20f), Dark());
        }

        private static void AddBackupSight(Transform root)
        {
            Box("T90SM-BackupSightPedestal", root,
                V(0.24f, 0.6625f, -1.28f),
                V(0.18f, 0.285f, 0.18f), Detail());
            Box("T90SM-BackupSightSlot", root,
                V(0.24f, 0.745f, -1.184f),
                V(0.14f, 0.08f, 0.012f), Dark());
            Box("T90SM-BackupSightAperture", root,
                V(0.24f, 0.745f, -1.181f),
                V(0.10f, 0.055f, 0.008f), Glass());
        }

        private static void AddAutomatedNsvt(
            Transform root,
            Color color)
        {
            const float x = 0.40f;
            const float z = -0.95f;
            const float seatY = 0.50f;
            const float scale = 0.68f;
            const float heightScale = 0.78f;
            float scaleY = scale * heightScale;
            float raceTop = seatY + 0.24f * scaleY;
            float foundationTop = raceTop + 0.15f * scaleY;
            float headCenter = foundationTop + 0.18f * scaleY;
            float weaponFoot = foundationTop + 0.04f * scaleY;

            Cylinder("Painted-T90SM-NsvtRace", root,
                V(x, (seatY + raceTop) * 0.5f, z),
                0.24f * scale, 0.27f * scale,
                raceTop - seatY, 18,
                TankShapeAxis.Y, color * 0.56f);
            Transform raceRing = TankShapeFactory.TorusPart(
                "T90SM-NsvtRaceRing",
                root,
                0.255f * scale,
                0.022f * scale,
                20,
                Dark());
            raceRing.localPosition =
                V(x, raceTop + 0.01f * scale, z);
            Box("Painted-T90SM-NsvtFoundation", root,
                V(x, raceTop + 0.08f * scaleY, z + 0.03f * scale),
                V(0.62f * scale, 0.18f * scaleY, 0.58f * scale),
                color * 0.54f);

            Transform taper = TankShapeFactory.OrientedSlabPart(
                "Painted-T90SM-NsvtTaper",
                root,
                V(-0.25f * scale, -0.15f * scaleY, -0.21f * scale),
                V(0.25f * scale, -0.15f * scaleY, -0.21f * scale),
                V(0.25f * scale, -0.15f * scaleY, 0.21f * scale),
                V(-0.25f * scale, -0.15f * scaleY, 0.21f * scale),
                V(-0.18f * scale, 0.15f * scaleY, -0.15f * scale),
                V(0.18f * scale, 0.15f * scaleY, -0.15f * scale),
                V(0.18f * scale, 0.15f * scaleY, 0.15f * scale),
                V(-0.18f * scale, 0.15f * scaleY, 0.15f * scale),
                color * 0.52f);
            taper.localPosition =
                V(x, foundationTop, z + 0.03f * scale);

            Box("Painted-T90SM-NsvtHead", root,
                V(x, headCenter, z + 0.04f * scale),
                V(0.40f * scale, 0.34f * scaleY, 0.36f * scale),
                color * 0.52f);
            Box("T90SM-NsvtHeadCap", root,
                V(x, headCenter + 0.19f * scaleY, z + 0.04f * scale),
                V(0.44f * scale, 0.045f * scaleY, 0.40f * scale),
                Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Transform plate = Box(
                    "Painted-T90SM-NsvtHeadSide",
                    root,
                    V(
                        x + side * 0.23f * scale,
                        headCenter + 0.06f * scaleY,
                        z + 0.10f * scale),
                    V(
                        0.070f * scale,
                        0.27f * scaleY,
                        0.28f * scale),
                    color * 0.50f);
                plate.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    side * 0.08f * scale * Mathf.Rad2Deg);
            }
            Box("Painted-T90SM-NsvtOptic", root,
                V(x + 0.27f * scale,
                    headCenter - 0.02f * scaleY,
                    z + 0.21f * scale),
                V(0.19f * scale, 0.21f * scaleY, 0.22f * scale),
                color * 0.50f);
            Box("T90SM-NsvtOpticLens", root,
                V(x + 0.27f * scale,
                    headCenter,
                    z + 0.328f * scale),
                V(0.13f * scale, 0.13f * scaleY, 0.014f * scale),
                Glass());
            Box("Painted-T90SM-NsvtServiceBox", root,
                V(x - 0.27f * scale,
                    headCenter - 0.04f * scaleY,
                    z - 0.01f * scale),
                V(0.22f * scale, 0.18f * scaleY, 0.28f * scale),
                color * 0.50f);
            Cylinder("T90SM-NsvtWorkLight", root,
                V(x - 0.25f * scale,
                    headCenter + 0.08f * scaleY,
                    z + 0.25f * scale),
                0.070f * scale, 0.070f * scale,
                0.075f * scale, 14,
                TankShapeAxis.Z, Dark());
            Cylinder("T90SM-NsvtWorkLightLens", root,
                V(x - 0.25f * scale,
                    headCenter + 0.08f * scaleY,
                    z + 0.294f * scale),
                0.054f * scale, 0.054f * scale,
                0.012f * scale, 14,
                TankShapeAxis.Z, Glass());

            Transform weapon = TankPintleMachineGunFactory.Build(
                "T90SM-Nsvt",
                root,
                V(x, weaponFoot, z + 0.10f * scale),
                Quaternion.identity,
                TankMachineGunClass.Nsvt,
                0.72f,
                -0.075f,
                true,
                TankMachineGunShield.Standard,
                true,
                Dark(),
                Detail());
            weapon.name = "T90SM-RemoteNsvt";
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
