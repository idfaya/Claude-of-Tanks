using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MTurretEquipmentDetails
    {
        private const float StationScale = 1.04f;
        private const float StationHeightScale = 1.42f;

        public static void Build(Transform root, Color color)
        {
            AddCrewStations(root, color);
            AddSosna(root, color);
            AddPeriscopes(root);
            AddAutomatedStation(root, color);
            AddSmokeBanks(root, color);
            AddAntennas(root);
            TankT90MRoofFinalDetails.Build(root, color);
        }

        private static void AddCrewStations(
            Transform root, Color color)
        {
            Cylinder(
                "Painted-T90M-CommanderCupola",
                root,
                V(-0.48f, 0.85f, -0.34f),
                0.34f,
                0.36f,
                0.30f,
                20,
                TankShapeAxis.Y,
                color * 0.58f);
            Torus(
                "T90M-CommanderCupolaRing",
                root,
                V(-0.48f, 1.005f, -0.34f),
                0.335f,
                0.020f,
                22,
                Dark());
            Cylinder(
                "Painted-T90M-GunnerCupola",
                root,
                V(0.38f, 0.8425f, -0.28f),
                0.29f,
                0.31f,
                0.285f,
                18,
                TankShapeAxis.Y,
                color * 0.58f);
            Torus(
                "T90M-GunnerCupolaRing",
                root,
                V(0.38f, 0.99f, -0.28f),
                0.285f,
                0.018f,
                20,
                Dark());
        }

        private static void AddSosna(
            Transform root,
            Color color)
        {
            Box(
                "Painted-T90M-SosnaCarrier",
                root,
                V(0.33f, 0.7675f, 0.49f),
                V(0.42f, 0.295f, 0.36f),
                color * 0.54f);
            Box(
                "Painted-T90M-SosnaHousing",
                root,
                V(0.33f, 0.98f, 0.49f),
                V(0.36f, 0.16f, 0.30f),
                color * 0.52f);
            Box(
                "T90M-SosnaLens",
                root,
                V(0.33f, 0.98f, 0.648f),
                V(0.27f, 0.10f, 0.016f),
                Glass());
            Box(
                "T90M-SosnaHood",
                root,
                V(0.33f, 1.07f, 0.62f),
                V(0.38f, 0.025f, 0.065f),
                Dark());
        }

        private static void AddPeriscopes(Transform root)
        {
            float[,] stations =
            {
                { -0.72f, -0.15f, -0.28f },
                { -0.50f, -0.09f, 0f },
                { -0.28f, -0.15f, 0.24f },
                { 0.17f, -0.05f, -0.18f },
                { 0.56f, -0.09f, 0.18f }
            };
            for (int index = 0;
                index < stations.GetLength(0);
                index++)
            {
                float x = stations[index, 0];
                float z = stations[index, 1];
                float yaw = stations[index, 2];
                Quaternion rotation =
                    Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f);
                Box(
                    "T90M-RoofPeriscopeSlot",
                    root,
                    V(x, 1.00f, z),
                    V(0.12f, 0.055f, 0.070f),
                    Dark(),
                    rotation);
                Box(
                    "T90M-RoofPeriscopeGlass",
                    root,
                    V(x, 1.012f, z + 0.040f),
                    V(0.080f, 0.032f, 0.010f),
                    Glass(),
                    rotation);
            }
        }

        private static void AddAutomatedStation(
            Transform root,
            Color color)
        {
            const float x = -0.58f;
            const float z = -0.88f;
            const float seatY = 0.59f;
            float scaleY = StationScale * StationHeightScale;
            float raceTop = seatY + 0.24f * scaleY;
            float foundationTop = raceTop + 0.15f * scaleY;
            float headCenter = foundationTop + 0.18f * scaleY;
            float weaponFoot = foundationTop + 0.04f * scaleY;

            Cylinder(
                "Painted-T90M-KordRace",
                root,
                V(x, (seatY + raceTop) * 0.5f, z),
                0.24f * StationScale,
                0.27f * StationScale,
                raceTop - seatY,
                18,
                TankShapeAxis.Y,
                color * 0.56f);
            Torus(
                "T90M-KordRaceRing",
                root,
                V(x, raceTop + 0.01f * StationScale, z),
                0.255f * StationScale,
                0.022f * StationScale,
                20,
                Dark());
            Box(
                "Painted-T90M-KordFoundation",
                root,
                V(
                    x,
                    raceTop + 0.08f * scaleY,
                    z + 0.03f * StationScale),
                V(
                    0.62f * StationScale,
                    0.18f * scaleY,
                    0.58f * StationScale),
                color * 0.54f);
            AddStationTaper(
                root,
                color,
                x,
                z,
                foundationTop,
                scaleY);
            AddStationHead(
                root,
                color,
                x,
                z,
                headCenter,
                scaleY);

            Transform weapon = TankPintleMachineGunFactory.Build(
                "T90M-Kord",
                root,
                V(
                    x,
                    weaponFoot,
                    z + 0.10f * StationScale),
                Quaternion.identity,
                TankMachineGunClass.Kord,
                1.12f,
                -0.075f,
                true,
                TankMachineGunShield.Standard,
                true,
                Dark(),
                Detail());
            weapon.name = "T90M-RemoteKord";
            weapon.localScale = V(1f, 1f / 0.65f, 1f);
        }

        private static void AddStationTaper(
            Transform root,
            Color color,
            float x,
            float z,
            float foundationTop,
            float scaleY)
        {
            Transform taper = TankShapeFactory.OrientedSlabPart(
                "Painted-T90M-KordTaper",
                root,
                V(-0.25f * StationScale,
                    -0.15f * scaleY, -0.21f * StationScale),
                V(0.25f * StationScale,
                    -0.15f * scaleY, -0.21f * StationScale),
                V(0.25f * StationScale,
                    -0.15f * scaleY, 0.21f * StationScale),
                V(-0.25f * StationScale,
                    -0.15f * scaleY, 0.21f * StationScale),
                V(-0.18f * StationScale,
                    0.15f * scaleY, -0.15f * StationScale),
                V(0.18f * StationScale,
                    0.15f * scaleY, -0.15f * StationScale),
                V(0.18f * StationScale,
                    0.15f * scaleY, 0.15f * StationScale),
                V(-0.18f * StationScale,
                    0.15f * scaleY, 0.15f * StationScale),
                color * 0.52f);
            taper.localPosition =
                V(x, foundationTop, z + 0.03f * StationScale);
        }

        private static void AddStationHead(
            Transform root,
            Color color,
            float x,
            float z,
            float centerY,
            float scaleY)
        {
            Box(
                "Painted-T90M-KordHead",
                root,
                V(x, centerY, z + 0.04f * StationScale),
                V(
                    0.40f * StationScale,
                    0.34f * scaleY,
                    0.36f * StationScale),
                color * 0.52f);
            Box(
                "T90M-KordHeadCap",
                root,
                V(
                    x,
                    centerY + 0.19f * scaleY,
                    z + 0.04f * StationScale),
                V(
                    0.44f * StationScale,
                    0.045f * scaleY,
                    0.40f * StationScale),
                Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "Painted-T90M-KordHeadSide",
                    root,
                    V(
                        x + side * 0.23f * StationScale,
                        centerY + 0.06f * scaleY,
                        z + 0.10f * StationScale),
                    V(
                        0.070f * StationScale,
                        0.27f * scaleY,
                        0.28f * StationScale),
                    color * 0.50f,
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 0.08f * StationScale *
                        Mathf.Rad2Deg));
            }
            AddStationOptics(root, color, x, z, centerY, scaleY);
        }

        private static void AddStationOptics(
            Transform root,
            Color color,
            float x,
            float z,
            float centerY,
            float scaleY)
        {
            Box(
                "Painted-T90M-KordOptic",
                root,
                V(
                    x + 0.27f * StationScale,
                    centerY - 0.02f * scaleY,
                    z + 0.21f * StationScale),
                V(
                    0.19f * StationScale,
                    0.21f * scaleY,
                    0.22f * StationScale),
                color * 0.50f);
            Box(
                "T90M-KordOpticLens",
                root,
                V(
                    x + 0.27f * StationScale,
                    centerY,
                    z + 0.328f * StationScale),
                V(
                    0.13f * StationScale,
                    0.13f * scaleY,
                    0.014f * StationScale),
                Glass());
            Box(
                "Painted-T90M-KordServiceBox",
                root,
                V(
                    x - 0.27f * StationScale,
                    centerY - 0.04f * scaleY,
                    z - 0.01f * StationScale),
                V(
                    0.22f * StationScale,
                    0.18f * scaleY,
                    0.28f * StationScale),
                color * 0.50f);
            Cylinder(
                "T90M-KordWorkLight",
                root,
                V(
                    x - 0.25f * StationScale,
                    centerY + 0.08f * scaleY,
                    z + 0.25f * StationScale),
                0.070f * StationScale,
                0.070f * StationScale,
                0.075f * StationScale,
                14,
                TankShapeAxis.Z,
                Dark());
            Cylinder(
                "T90M-KordWorkLightLens",
                root,
                V(
                    x - 0.25f * StationScale,
                    centerY + 0.08f * scaleY,
                    z + 0.294f * StationScale),
                0.054f * StationScale,
                0.054f * StationScale,
                0.012f * StationScale,
                14,
                TankShapeAxis.Z,
                Glass());
        }

        private static void AddSmokeBanks(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box(
                    "Painted-T90M-SmokeBankShoe",
                    root,
                    V(side * 1.24f, 0.35f, 0.02f),
                    V(0.22f, 0.24f, 0.48f),
                    color * 0.53f,
                    Quaternion.Euler(
                        0f,
                        0f,
                        -side * 0.16f * Mathf.Rad2Deg));
                TankFittingShapeFactory.BuildSmokeBank(
                    side < 0 ? "T90M-Left" : "T90M-Right",
                    root,
                    V(side * 1.27f, 0.47f, 0.04f),
                    Quaternion.Euler(
                        0f,
                        side * 1.03f * Mathf.Rad2Deg,
                        0f),
                    6,
                    0.042f,
                    0.29f,
                    -0.42f,
                    0.32f,
                    0.58f,
                    0.098f,
                    Detail(),
                    Dark());
            }
        }

        private static void AddAntennas(Transform root)
        {
            float[,] antennas =
            {
                { -0.96f, -1.20f, 0.50f, -0.025f },
                { 0.90f, -1.24f, 0.38f, 0.025f }
            };
            for (int index = 0;
                index < antennas.GetLength(0);
                index++)
            {
                float x = antennas[index, 0];
                float z = antennas[index, 1];
                TankFittingShapeFactory.BuildAntennaWhip(
                    "T90M-" + index,
                    root,
                    V(x, 0.55f, z),
                    antennas[index, 2],
                    0.013f,
                    antennas[index, 3],
                    Detail(),
                    Dark());
                Cylinder(
                    "T90M-AntennaFoundation",
                    root,
                    V(x, 0.55f, z),
                    0.040f,
                    0.055f,
                    0.11f,
                    8,
                    TankShapeAxis.Y,
                    Dark());
            }
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color,
            Quaternion? rotation = null)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            part.localRotation = rotation ?? Quaternion.identity;
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
            return new Color(0.20f, 0.22f, 0.16f);
        }

        private static Color Detail()
        {
            return new Color(0.28f, 0.30f, 0.24f);
        }

        private static Color Glass()
        {
            return new Color(0.10f, 0.17f, 0.18f);
        }
    }
}
