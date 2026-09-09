using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsXTurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddFacetedShell(turret, color);
            AddRoofCarrier(turret, color);
            AddPanoramicHeads(turret, color);
            AddRemoteWeaponStation(turret, color);
            AddSensorsAndSmoke(turret, color);
            AddAntennas(turret, color);
        }

        private static void AddFacetedShell(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-AbramsX-TurretBearing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.46f, -0.02f),
                new Vector3(1.14f, 0.1f, 1.14f),
                color * 0.48f);
            TankAbramsXGeometry.Frustum(
                "Painted-AbramsX-LowTurretCore",
                turret,
                new Vector3(0f, -0.18f, -0.5f),
                1.58f,
                1.21f,
                1.56f,
                1.22f,
                0.54f,
                color * 0.62f);
            TankAbramsXGeometry.Frustum(
                "Painted-AbramsX-RearCassette",
                turret,
                new Vector3(0f, 0.08f, -1.63f),
                1.34f,
                1.16f,
                0.78f,
                0.65f,
                0.42f,
                color * 0.58f);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform cheek = Part(
                    "Painted-AbramsX-FacetedCheek",
                    turret,
                    new Vector3(side * 0.97f, -0.14f, 1.25f),
                    new Vector3(1.32f, 0.62f, 1.58f),
                    color * 0.72f);
                cheek.localRotation =
                    Quaternion.Euler(
                        -7f,
                        side * 24f,
                        side * 2f);
                Part("Painted-AbramsX-OuterShoulder", turret,
                    new Vector3(side * 1.5f, -0.12f, -0.2f),
                    new Vector3(0.28f, 0.55f, 2.55f),
                    color * 0.56f);
                Part("Painted-AbramsX-RearTerrace", turret,
                    new Vector3(side * 1.28f, 0.18f, -1.35f),
                    new Vector3(0.55f, 0.26f, 1.45f),
                    color * 0.64f);
                Part("AbramsX-TerraceChannel", turret,
                    new Vector3(side * 0.88f, 0.29f, -1.23f),
                    new Vector3(0.2f, 0.035f, 1.55f),
                    TankAbramsXFamilyDetails.Dark());
                Part("Painted-AbramsX-GunTunnelJamb", turret,
                    new Vector3(side * 0.31f, -0.04f, 2f),
                    new Vector3(0.16f, 0.4f, 0.78f),
                    color * 0.5f);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Transform roof = Part(
                    "Painted-AbramsX-SlopedRoofShoulder",
                    turret,
                    new Vector3(side * 0.76f, 0.23f, 0.48f),
                    new Vector3(1.02f, 0.1f, 2.1f),
                    color * 0.69f);
                roof.localRotation =
                    Quaternion.Euler(
                        -4f,
                        side * 5f,
                        side * 8f);
            }
            Part("Painted-AbramsX-RoofSpine", turret,
                new Vector3(0f, 0.38f, 0.49f),
                new Vector3(0.74f, 0.16f, 1.34f),
                color * 0.7f);
            Part("AbramsX-TunnelShadow", turret,
                new Vector3(0f, -0.13f, 2.19f),
                new Vector3(0.43f, 0.42f, 0.5f),
                TankAbramsXFamilyDetails.Dark());
            Part("AbramsX-FaceSensorSlit", turret,
                new Vector3(0f, 0.04f, 2.44f),
                new Vector3(0.56f, 0.05f, 0.018f),
                TankAbramsXFamilyDetails.Glass());
        }

        private static void AddRoofCarrier(
            Transform turret,
            Color color)
        {
            Part("Painted-AbramsX-RoofCarrier", turret,
                new Vector3(0f, -0.39f, 0.61f),
                new Vector3(2.32f, 0.2f, 1.9f),
                color * 0.6f);
            Part("AbramsX-RoofCarrierInset", turret,
                new Vector3(0f, -0.23f, 0.61f),
                new Vector3(1.95f, 0.025f, 1.85f),
                TankAbramsXFamilyDetails.Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsX-CornerSensorPod", turret,
                    new Vector3(side * 1.26f, 0.12f, 0.7f),
                    new Vector3(0.46f, 0.62f, 0.72f),
                    color * 0.57f);
                Part("Painted-AbramsX-SensorWing", turret,
                    new Vector3(side * 1.58f, 0.18f, 0.7f),
                    new Vector3(0.17f, 0.36f, 0.75f),
                    color * 0.52f);
                Part("AbramsX-SensorWingFace", turret,
                    new Vector3(side * 1.58f, 0.21f, 1.085f),
                    new Vector3(0.12f, 0.075f, 0.018f),
                    TankAbramsXFamilyDetails.Glass());
            }
        }

        private static void AddPanoramicHeads(
            Transform turret,
            Color color)
        {
            AddPanoramicHead(
                turret,
                color,
                0.702f,
                0.643f,
                0.195f,
                0.579f);
            AddPanoramicHead(
                turret,
                color,
                -0.759f,
                0.866f,
                0.165f,
                0.55f);
        }

        private static void AddPanoramicHead(
            Transform turret,
            Color color,
            float x,
            float z,
            float bottom,
            float top)
        {
            TankDetailGeometry.Part(
                "Painted-AbramsX-PanoramaBearing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, bottom + 0.04f, z),
                new Vector3(0.21f, 0.065f, 0.21f),
                color * 0.48f);
            Part("Painted-AbramsX-PanoramaDRear", turret,
                new Vector3(
                    x,
                    (bottom + top) * 0.5f,
                    z - 0.1f),
                new Vector3(0.5f, top - bottom, 0.28f),
                color * 0.59f);
            for (int side = -1; side <= 1; side += 2)
                Part("Painted-AbramsX-PanoramaHoodCheek", turret,
                    new Vector3(
                        x + side * 0.205f,
                        (bottom + top) * 0.5f,
                        z + 0.1f),
                    new Vector3(
                        0.09f,
                        (top - bottom) * 0.72f,
                        0.28f),
                    color * 0.57f);
            Part("Painted-AbramsX-PanoramaBrow", turret,
                new Vector3(x, top - 0.035f, z + 0.14f),
                new Vector3(0.49f, 0.07f, 0.18f),
                color * 0.64f);
            Part("AbramsX-PanoramaRecess", turret,
                new Vector3(
                    x,
                    (bottom + top) * 0.5f,
                    z + 0.096f),
                new Vector3(
                    0.27f,
                    (top - bottom) * 0.56f,
                    0.025f),
                TankAbramsXFamilyDetails.Dark());
            for (int lens = -1; lens <= 1; lens += 2)
                Part("AbramsX-PanoramaLens", turret,
                    new Vector3(
                        x + lens * 0.06f,
                        (bottom + top) * 0.5f,
                        z + 0.113f),
                    new Vector3(0.075f, 0.09f, 0.012f),
                    TankAbramsXFamilyDetails.Glass());
        }

        private static void AddRemoteWeaponStation(
            Transform turret,
            Color color)
        {
            const float z = -0.265f;
            TankDetailGeometry.Part(
                "Painted-AbramsX-XM914Foundation",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.57f, z),
                new Vector3(0.34f, 0.19f, 0.34f),
                color * 0.54f);
            TankDetailGeometry.Part(
                "AbramsX-XM914SlewRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.7f, z),
                new Vector3(0.3f, 0.12f, 0.3f),
                TankAbramsXFamilyDetails.Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsX-XM914Yoke", turret,
                    new Vector3(side * 0.235f, 1.02f, z - 0.02f),
                    new Vector3(0.07f, 0.4f, 0.11f),
                    color * 0.55f);
                Transform pivot = TankDetailGeometry.Part(
                    "AbramsX-XM914Pivot",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.235f, 1.2f, z),
                    new Vector3(0.06f, 0.08f, 0.06f),
                    TankAbramsXFamilyDetails.Gunmetal());
                pivot.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
            }
            Part("AbramsX-XM914Receiver", turret,
                new Vector3(-0.04f, 1.22f, z + 0.03f),
                new Vector3(0.36f, 0.18f, 0.42f),
                TankAbramsXFamilyDetails.Gunmetal());
            Axial("AbramsX-XM914Barrel", turret,
                0.03f, 1.76f, 0.77f,
                TankAbramsXFamilyDetails.Dark(),
                0f, 1.26f);
            Axial("AbramsX-XM914Bore", turret,
                0.018f, 0.014f, 1.657f,
                Color.black,
                0f, 1.26f);
            Part("Painted-AbramsX-XM914EoHead", turret,
                new Vector3(-0.245f, 1.08f, 0.49f),
                new Vector3(0.36f, 0.27f, 0.32f),
                color * 0.55f);
            Part("AbramsX-XM914EoLens", turret,
                new Vector3(-0.245f, 1.08f, 0.66f),
                new Vector3(0.14f, 0.11f, 0.018f),
                TankAbramsXFamilyDetails.Glass());
            Part("Painted-AbramsX-XM914AmmoBox", turret,
                new Vector3(0.54f, 1.02f, -0.26f),
                new Vector3(0.34f, 0.24f, 0.36f),
                color * 0.58f);
            Part("AbramsX-XM914FeedMouth", turret,
                new Vector3(0.57f, 1.15f, -0.26f),
                new Vector3(0.13f, 0.08f, 0.14f),
                TankAbramsXFamilyDetails.Dark());
            AddAmmunitionFeed(turret);
        }

        private static void AddAmmunitionFeed(
            Transform turret)
        {
            for (int link = 0; link < 28; link++)
            {
                float t = link / 27f;
                float x = 0.08f + 0.52f *
                    Mathf.Pow(
                        Mathf.Sin(t * Mathf.PI * 0.5f),
                        0.7f);
                float y =
                    1.28f +
                    0.14f * t +
                    0.2f * Mathf.Sin(t * Mathf.PI);
                float z =
                    -0.47f +
                    0.32f *
                    Mathf.Pow(
                        1f -
                        Mathf.Cos(t * Mathf.PI * 0.5f),
                        0.8f);
                Axial("AbramsX-XM914Cartridge", turret,
                    0.016f, 0.16f, z,
                    link % 5 == 0
                        ? TankAbramsXFamilyDetails.Dark()
                        : TankAbramsXFamilyDetails.Gunmetal(),
                    x, y);
            }
            for (int link = 0; link < 8; link++)
            {
                float t = (link + 1f) / 8f;
                float smooth = t * t * (3f - 2f * t);
                Axial("AbramsX-XM914FeedReturn", turret,
                    0.016f,
                    0.14f,
                    Mathf.Lerp(-0.15f, -0.26f, smooth),
                    TankAbramsXFamilyDetails.Gunmetal(),
                    Mathf.Lerp(0.6f, 0.57f, smooth),
                    Mathf.Lerp(1.42f, 1.17f, smooth));
            }
        }

        private static void AddSensorsAndSmoke(
            Transform turret,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsX-EdgeElectronics", turret,
                    new Vector3(side * 1.52f, 0.2f, -0.72f),
                    new Vector3(0.18f, 0.18f, 0.42f),
                    color * 0.54f);
                Part("AbramsX-EdgeSensor", turret,
                    new Vector3(side * 1.62f, 0.22f, -0.49f),
                    new Vector3(0.08f, 0.09f, 0.018f),
                    TankAbramsXFamilyDetails.Glass());
                for (int smoke = 0; smoke < 4; smoke++)
                {
                    Transform tube =
                        TankDetailGeometry.Part(
                            "Painted-AbramsX-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * (1.04f + smoke * 0.06f),
                                0.12f + smoke * 0.025f,
                                2.04f - smoke * 0.05f),
                            new Vector3(0.04f, 0.13f, 0.04f),
                            color * 0.44f);
                    tube.localRotation =
                        Quaternion.Euler(
                            62f,
                            0f,
                            side * 30f);
                }
            }
            Part("Painted-AbramsX-CenterSensorPost", turret,
                new Vector3(0.75f, 0.29f, -0.85f),
                new Vector3(0.3f, 0.24f, 0.3f),
                color * 0.52f);
            Part("AbramsX-CenterSensorFace", turret,
                new Vector3(0.75f, 0.35f, -0.69f),
                new Vector3(0.22f, 0.1f, 0.025f),
                TankAbramsXFamilyDetails.Glass());
        }

        private static void AddAntennas(
            Transform turret,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-AbramsX-AntennaBase", turret,
                    new Vector3(side * 1.14f, 0.08f, -1.99f),
                    new Vector3(0.15f, 0.1f, 0.15f),
                    color * 0.49f);
                TankDetailGeometry.Part(
                    "AbramsX-NetworkMast",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 1.14f, 1.13f, -1.99f),
                    new Vector3(0.009f, 1.05f, 0.009f),
                    TankAbramsXFamilyDetails.Gunmetal());
            }
        }

        private static Transform Part(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                PrimitiveType.Cube,
                parent,
                position,
                scale,
                color);
        }

        private static void Axial(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color,
            float x = 0f,
            float y = 0f)
        {
            Transform part = TankDetailGeometry.Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                new Vector3(x, y, z),
                new Vector3(
                    radius,
                    length * 0.5f,
                    radius),
                color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
