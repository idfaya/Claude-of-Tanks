using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSTurretEquipmentDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddCrewStations(root, color);
            AddPeriscopes(root);
            AddAutoloaderPort(root, color);
            AddSosna(root, color);
            AddTagilTower(root, color);
            AddSmokeBanks(root, color);
            AddAntenna(root, color);
        }

        private static void AddCrewStations(
            Transform root,
            Color color)
        {
            Cylinder("Painted-T90MS-CommanderCupolaBase", root,
                V(0.48f, 0.61f, -0.40f),
                0.27f, 0.30f, 0.10f, 20,
                TankShapeAxis.Y, color * 0.58f);
            Torus("T90MS-CommanderCupolaRing", root,
                V(0.48f, 0.655f, -0.40f),
                0.267f, 0.020f, 22, Dark());
            Cylinder("Painted-T90MS-CommanderCupolaLid", root,
                V(0.48f, 0.68f, -0.40f),
                0.22f, 0.24f, 0.075f, 18,
                TankShapeAxis.Y, color * 0.60f);

            Transform lowerHead = Box(
                "Painted-T90MS-CommanderOpticLower",
                root,
                V(0.48f, 0.80f, -0.40f),
                V(0.30f, 0.18f, 0.28f),
                color * 0.55f);
            lowerHead.localRotation = Quaternion.Euler(
                -0.04f * Mathf.Rad2Deg,
                0.10f * Mathf.Rad2Deg,
                0f);
            Transform upperHead = Box(
                "T90MS-CommanderOpticUpper",
                root,
                V(0.46f, 0.98f, -0.40f),
                V(0.24f, 0.22f, 0.22f),
                Detail());
            upperHead.localRotation = Quaternion.Euler(
                -0.05f * Mathf.Rad2Deg,
                0.12f * Mathf.Rad2Deg,
                0f);
            Transform windowSlot = Box(
                "T90MS-CommanderOpticWindowSlot",
                root,
                V(0.46f, 0.99f, -0.278f),
                V(0.19f, 0.14f, 0.014f),
                Dark());
            windowSlot.localRotation = upperHead.localRotation;
            Transform window = Box(
                "T90MS-CommanderOpticWindow",
                root,
                V(0.46f, 0.99f, -0.268f),
                V(0.15f, 0.10f, 0.010f),
                Glass());
            window.localRotation = upperHead.localRotation;

            Cylinder("Painted-T90MS-GunnerCupola", root,
                V(-0.38f, 0.61f, -0.52f),
                0.24f, 0.27f, 0.09f, 18,
                TankShapeAxis.Y, color * 0.58f);
            Torus("T90MS-GunnerCupolaRing", root,
                V(-0.38f, 0.65f, -0.52f),
                0.235f, 0.018f, 20, Dark());
        }

        private static void AddPeriscopes(Transform root)
        {
            float[,] stations =
            {
                { 0.25f, -0.20f, -0.30f },
                { 0.52f, -0.08f, 0.02f },
                { 0.72f, -0.31f, 0.28f },
                { -0.16f, -0.30f, -0.20f },
                { -0.48f, -0.22f, 0.10f },
                { -0.62f, -0.50f, 0.30f }
            };
            for (int index = 0;
                index < stations.GetLength(0);
                index++)
            {
                float x = stations[index, 0];
                float z = stations[index, 1];
                float yaw = stations[index, 2];
                Transform slot = Box(
                    "T90MS-RoofPeriscopeSlot",
                    root,
                    V(x, 0.696f, z),
                    V(0.16f, 0.018f, 0.055f),
                    Dark());
                slot.localRotation =
                    Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f);
                Transform glass = Box(
                    "T90MS-RoofPeriscopeGlass",
                    root,
                    V(x, 0.708f, z),
                    V(0.11f, 0.020f, 0.032f),
                    Glass());
                glass.localRotation = slot.localRotation;
            }
            Transform leftWeld = Box(
                "T90MS-RoofWeld",
                root,
                V(-0.12f, 0.686f, 0.12f),
                V(0.025f, 0.020f, 1.10f),
                Detail());
            leftWeld.localRotation =
                Quaternion.Euler(0f, -0.08f * Mathf.Rad2Deg, 0f);
            Transform rightWeld = Box(
                "T90MS-RoofWeld",
                root,
                V(0.74f, 0.672f, 0.08f),
                V(0.025f, 0.020f, 0.82f),
                Detail());
            rightWeld.localRotation =
                Quaternion.Euler(0f, 0.16f * Mathf.Rad2Deg, 0f);
        }

        private static void AddAutoloaderPort(
            Transform root,
            Color color)
        {
            Box("T90MS-AutoloaderPortWell", root,
                V(0.005f, 0.674f, -0.77f),
                V(0.39f, 0.035f, 0.48f), Dark());
            Box("Painted-T90MS-AutoloaderPort", root,
                V(0.005f, 0.695f, -0.77f),
                V(0.32f, 0.020f, 0.40f), color * 0.57f);
            foreach (float x in new[] { -0.13f, 0.14f })
            {
                Box("T90MS-AutoloaderPortWeld", root,
                    V(x, 0.708f, -0.77f),
                    V(0.025f, 0.018f, 0.38f), Detail());
            }
            Box("T90MS-AutoloaderPortLatch", root,
                V(0.005f, 0.708f, -0.56f),
                V(0.30f, 0.018f, 0.025f), Detail());
        }

        private static void AddSosna(
            Transform root,
            Color color)
        {
            Box("Painted-T90MS-SosnaHousing", root,
                V(0.32f, 0.61f, 0.48f),
                V(0.38f, 0.12f, 0.30f), color * 0.56f);
            Box("T90MS-SosnaLens", root,
                V(0.32f, 0.62f, 0.642f),
                V(0.28f, 0.075f, 0.016f), Glass());
            Box("T90MS-SosnaHood", root,
                V(0.32f, 0.68f, 0.60f),
                V(0.40f, 0.025f, 0.065f), Dark());
        }

        private static void AddTagilTower(
            Transform root,
            Color color)
        {
            GameObject towerObject =
                new GameObject("T90MS-TagilWeaponTower");
            Transform tower = towerObject.transform;
            tower.SetParent(root, false);
            tower.localPosition = V(-0.64f, 0f, -1.195f);

            TankShapeFactory.OrientedSlabPart(
                "Painted-T90MS-TagilTowerFoundation",
                tower,
                V(-0.25f, 0.70f, -0.29f),
                V(0.25f, 0.70f, -0.29f),
                V(0.25f, 0.70f, 0.29f),
                V(-0.25f, 0.70f, 0.29f),
                V(-0.16f, 0.98f, -0.165f),
                V(0.16f, 0.98f, -0.165f),
                V(0.16f, 0.98f, 0.165f),
                V(-0.16f, 0.98f, 0.165f),
                color * 0.54f);
            TankWeldedStationLoftShapeFactory.Build(
                "Painted-T90MS-TagilTowerArmoredHead",
                tower,
                new[]
                {
                    S(-0.165f, -0.23f, 0.23f, -0.16f, 0.16f,
                        -0.07f, 0.09f),
                    S(0.165f, -0.21f, 0.21f, -0.15f, 0.15f,
                        -0.06f, 0.08f)
                },
                color * 0.52f);
            Box("T90MS-TagilTowerCradle", tower,
                V(0.02f, 1.50f, 0.035f),
                V(0.30f, 0.045f, 0.15f), Dark());
            Box("T90MS-TagilTowerBrow", tower,
                V(0.01f, 1.555f, 0.015f),
                V(0.34f, 0.025f, 0.29f), Detail());

            Box("Painted-T90MS-TagilTowerOpticHousing", tower,
                V(0.245f, 1.285f, 0.055f),
                V(0.19f, 0.27f, 0.22f), color * 0.52f);
            Box("T90MS-TagilTowerThermalWindow", tower,
                V(0.245f, 1.315f, 0.171f),
                V(0.115f, 0.105f, 0.012f), Glass());
            Cylinder("T90MS-TagilTowerDayWindow", tower,
                V(0.20f, 1.225f, 0.174f),
                0.034f, 0.034f, 0.014f, 12,
                TankShapeAxis.Z, Glass());
            Cylinder("T90MS-TagilTowerLightHousing", tower,
                V(-0.245f, 1.25f, 0.10f),
                0.072f, 0.072f, 0.10f, 14,
                TankShapeAxis.Z, Dark());
            Cylinder("T90MS-TagilTowerWorkLight", tower,
                V(-0.245f, 1.25f, 0.156f),
                0.057f, 0.057f, 0.012f, 14,
                TankShapeAxis.Z, Glass());
            Box("T90MS-TagilTowerAmmoBox", tower,
                V(-0.255f, 1.105f, -0.035f),
                V(0.23f, 0.21f, 0.29f), Dark());
            Box("T90MS-TagilTowerAmmoLid", tower,
                V(-0.255f, 1.221f, -0.035f),
                V(0.245f, 0.022f, 0.305f), Detail());
            Box("T90MS-TagilTowerAmmoRetainer", tower,
                V(-0.385f, 1.105f, -0.035f),
                V(0.025f, 0.23f, 0.31f), Dark());
            for (int index = 0; index < 7; index++)
            {
                float t = index / 6f;
                Transform link = Box(
                    "T90MS-TagilTowerFeedLink",
                    tower,
                    V(
                        -0.25f + 0.18f * t,
                        1.225f + 0.035f * t,
                        0.09f + 0.10f * t),
                    V(0.032f, 0.038f, 0.026f),
                    Detail());
                link.localRotation =
                    Quaternion.Euler(0f, 0f, -0.18f * Mathf.Rad2Deg);
            }
            Transform weapon = TankPintleMachineGunFactory.Build(
                "T90MS-TagilKord",
                tower,
                V(0.09f, 1.12f, 0.075f),
                Quaternion.identity,
                TankMachineGunClass.Kord,
                1.16f,
                -0.10f,
                true,
                TankMachineGunShield.Standard,
                true,
                Dark(),
                Detail());
            weapon.name = "T90MS-TagilRemoteKord";
        }

        private static void AddSmokeBanks(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Transform shoe = Box(
                    "Painted-T90MS-SmokeBankShoe",
                    root,
                    V(side * 1.16f, 0.55f, -0.24f),
                    V(0.26f, 0.22f, 0.48f),
                    color * 0.53f);
                shoe.localRotation = Quaternion.Euler(
                    0f,
                    side * 0.82f * Mathf.Rad2Deg,
                    0f);
                TankFittingShapeFactory.BuildSmokeBank(
                    side < 0 ? "T90MS-Left" : "T90MS-Right",
                    root,
                    V(side * 1.15f, 0.66f, -0.24f),
                    Quaternion.Euler(
                        0f,
                        side * 1.02f * Mathf.Rad2Deg,
                        0f),
                    5,
                    0.040f,
                    0.36f,
                    -0.38f,
                    0.30f,
                    0.54f,
                    0.095f,
                    Detail(),
                    Dark());
            }
        }

        private static void AddAntenna(
            Transform root,
            Color color)
        {
            Box("Painted-T90MS-AntennaPedestal", root,
                V(0.555f, 0.76f, -1.36f),
                V(0.10f, 0.46f, 0.10f), color * 0.54f);
            Cylinder("T90MS-AntennaPot", root,
                V(0.555f, 0.96f, -1.36f),
                0.045f, 0.060f, 0.12f, 10,
                TankShapeAxis.Y, Dark());
            TankFittingShapeFactory.BuildAntennaWhip(
                "T90MS",
                root,
                V(0.555f, 1.01f, -1.36f),
                2.30f,
                0.014f,
                0.015f,
                Detail(),
                Dark(),
                false);
        }

        private static TankWeldedStation S(
            float z,
            float middleLeft,
            float middleRight,
            float bottomLeft,
            float bottomRight,
            float topLeft,
            float topRight)
        {
            return new TankWeldedStation(
                z,
                0.98f,
                1.54f,
                middleLeft,
                middleRight,
                bottomLeft,
                bottomRight,
                topLeft,
                topRight);
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

        private static Transform Torus(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float tube,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.TorusPart(
                name, parent, radius, tube, segments, color);
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
