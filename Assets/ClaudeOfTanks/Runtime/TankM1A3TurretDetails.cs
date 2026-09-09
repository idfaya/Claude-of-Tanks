using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankM1A3TurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddShell(turret, color);
            AddAutoloader(turret, color);
            AddProtection(turret, color);
            AddSensors(turret, color);
            AddRemoteWeaponStation(turret, color);
            AddSmokeAndAntennas(turret, color);
        }

        private static void AddShell(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-M1A3-TurretBearing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.07f, -0.35f),
                new Vector3(1.28f, 0.12f, 1.28f),
                color * 0.5f);
            Part("Painted-M1A3-LowTurretCore", turret,
                new Vector3(0f, 0.3f, -0.72f),
                new Vector3(3f, 0.78f, 3.35f),
                color * 0.65f);
            Part("Painted-M1A3-TurretRoof", turret,
                new Vector3(0f, 0.72f, -0.78f),
                new Vector3(2.86f, 0.08f, 3.5f),
                color * 0.72f);
            for (int side = -1; side <= 1; side += 2)
            {
                Transform cheek = Part(
                    "Painted-M1A3-IntegratedCheek",
                    turret,
                    new Vector3(side * 0.9f, 0.31f, 1.32f),
                    new Vector3(1.24f, 0.78f, 1.72f),
                    color * 0.73f);
                cheek.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * 24f,
                        side * 2f);
                Part("Painted-M1A3-TurretFlank", turret,
                    new Vector3(side * 1.47f, 0.3f, -0.72f),
                    new Vector3(0.28f, 0.66f, 3.28f),
                    color * 0.59f);
            }
            Part("Painted-M1A3-MantletBrow", turret,
                new Vector3(0f, 0.38f, 1.93f),
                new Vector3(0.82f, 0.56f, 0.34f),
                color * 0.56f);
        }

        private static void AddAutoloader(
            Transform turret,
            Color color)
        {
            Part("Painted-M1A3-IsolatedBustle", turret,
                new Vector3(0f, 0.37f, -2.42f),
                new Vector3(2.92f, 0.53f, 1.46f),
                color * 0.61f);
            Part("Painted-M1A3-BustleTail", turret,
                new Vector3(0f, 0.58f, -3.01f),
                new Vector3(3.12f, 0.22f, 0.74f),
                color * 0.57f);
            for (int panel = 0; panel < 6; panel++)
            {
                float x = -1.1f + panel * 0.44f;
                Part("Painted-M1A3-BlowoffPanel", turret,
                    new Vector3(x, 0.79f, -2.48f),
                    new Vector3(0.37f, 0.045f, 0.75f),
                    color * 0.75f);
                Part("M1A3-BlowoffSeam", turret,
                    new Vector3(x + 0.205f, 0.8f, -2.48f),
                    new Vector3(0.018f, 0.052f, 0.7f),
                    TankM1A3FamilyDetails.Dark());
            }
        }

        private static void AddProtection(
            Transform turret,
            Color color)
        {
            Color cage = color * 0.48f;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int block = 0; block < 5; block++)
                {
                    float z = -0.74f - block * 0.47f;
                    Part("Painted-M1A3-TurretSideModule", turret,
                        new Vector3(side * 1.68f, 0.35f, z),
                        new Vector3(0.18f, 0.48f, 0.44f),
                        color * 0.6f);
                    Part("M1A3-TurretModuleRib", turret,
                        new Vector3(side * 1.79f, 0.35f, z),
                        new Vector3(0.025f, 0.3f, 0.34f),
                        TankM1A3FamilyDetails.Dark());
                }
                for (int rail = 0; rail < 3; rail++)
                    Part("Painted-M1A3-TurretCageRail", turret,
                        new Vector3(
                            side * 1.86f,
                            0.1f + rail * 0.29f,
                            -2.25f),
                        new Vector3(0.035f, 0.035f, 2.22f),
                        cage);
                for (int post = 0; post < 7; post++)
                    Part("Painted-M1A3-TurretCagePost", turret,
                        new Vector3(
                            side * 1.86f,
                            0.39f,
                            -3.33f + post * 0.37f),
                        new Vector3(0.035f, 0.62f, 0.035f),
                        cage);
                for (int corner = -1; corner <= 1; corner += 2)
                {
                    float z = corner < 0 ? -0.76f : 0.72f;
                    Part("Painted-M1A3-HardKillLauncher", turret,
                        new Vector3(side * 1.48f, 0.81f, z),
                        new Vector3(0.25f, 0.28f, 0.3f),
                        color * 0.56f);
                    Part("M1A3-RadarFace", turret,
                        new Vector3(
                            side * 1.625f,
                            0.84f,
                            z + corner * 0.085f),
                        new Vector3(0.16f, 0.15f, 0.02f),
                        TankM1A3FamilyDetails.Glass());
                }
            }
            for (int rail = 0; rail < 3; rail++)
                Part("Painted-M1A3-BustleCageRail", turret,
                    new Vector3(0f, 0.12f + rail * 0.28f, -3.35f),
                    new Vector3(3.64f, 0.035f, 0.035f),
                    cage);
        }

        private static void AddSensors(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-M1A3-PanoramicBearing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.82f, -0.4f),
                new Vector3(0.31f, 0.1f, 0.31f),
                color * 0.57f);
            Part("Painted-M1A3-PanoramicHead", turret,
                new Vector3(0f, 1.01f, -0.4f),
                new Vector3(0.48f, 0.34f, 0.44f),
                color * 0.62f);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("M1A3-PanoramaLens", turret,
                    new Vector3(side * 0.18f, 1.04f, -0.17f),
                    new Vector3(0.17f, 0.13f, 0.022f),
                    TankM1A3FamilyDetails.Glass());
                Part("Painted-M1A3-EoTower", turret,
                    new Vector3(side * 0.88f, 0.94f, 0.46f),
                    new Vector3(0.26f, 0.31f, 0.27f),
                    color * 0.59f);
                Part("M1A3-EoLens", turret,
                    new Vector3(side * 0.88f, 0.96f, 0.605f),
                    new Vector3(0.17f, 0.15f, 0.018f),
                    TankM1A3FamilyDetails.Glass());
            }
            Part("Painted-M1A3-DatalinkBase", turret,
                new Vector3(0.76f, 0.92f, -1.08f),
                new Vector3(0.34f, 0.22f, 0.36f),
                color * 0.55f);
            TankDetailGeometry.Part(
                "Painted-M1A3-DatalinkMast",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.76f, 1.17f, -1.08f),
                new Vector3(0.13f, 0.3f, 0.13f),
                color * 0.51f);
            Part("Painted-M1A3-DatalinkPanel", turret,
                new Vector3(0.76f, 1.35f, -1.08f),
                new Vector3(0.42f, 0.045f, 0.42f),
                color * 0.64f);
        }

        private static void AddRemoteWeaponStation(
            Transform turret,
            Color color)
        {
            const float x = -0.64f;
            const float z = 0.14f;
            TankDetailGeometry.Part(
                "Painted-M1A3-RwsFoundation",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, 0.84f, z),
                new Vector3(0.34f, 0.15f, 0.34f),
                color * 0.54f);
            TankDetailGeometry.Part(
                "Painted-M1A3-RwsPedestal",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, 1.02f, z),
                new Vector3(0.25f, 0.27f, 0.25f),
                color * 0.58f);
            Part("Painted-M1A3-RwsBridge", turret,
                new Vector3(x, 1.18f, z),
                new Vector3(0.5f, 0.1f, 0.42f),
                color * 0.61f);
            for (int side = -1; side <= 1; side += 2)
                Part("Painted-M1A3-RwsYoke", turret,
                    new Vector3(x + side * 0.19f, 1.37f, z - 0.02f),
                    new Vector3(0.075f, 0.34f, 0.11f),
                    color * 0.57f);
            Transform shaft = TankDetailGeometry.Part(
                "M1A3-RwsCrossShaft",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, 1.49f, z + 0.01f),
                new Vector3(0.055f, 0.255f, 0.055f),
                TankM1A3FamilyDetails.Gunmetal());
            shaft.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Part("M1A3-RwsReceiver", turret,
                new Vector3(x, 1.51f, z + 0.18f),
                new Vector3(0.18f, 0.16f, 0.4f),
                TankM1A3FamilyDetails.Gunmetal());
            Axial("M1A3-RwsBarrel", turret,
                0.024f, 1.08f, z + 0.87f,
                TankM1A3FamilyDetails.Dark(), x, 1.52f);
            Part("Painted-M1A3-RwsAmmoBox", turret,
                new Vector3(x - 0.36f, 1.47f, z - 0.04f),
                new Vector3(0.24f, 0.3f, 0.34f),
                color * 0.56f);
            Part("Painted-M1A3-RwsSensor", turret,
                new Vector3(x + 0.36f, 1.52f, z - 0.02f),
                new Vector3(0.25f, 0.28f, 0.3f),
                color * 0.57f);
            Part("M1A3-RwsSensorLens", turret,
                new Vector3(x + 0.36f, 1.54f, z + 0.145f),
                new Vector3(0.15f, 0.14f, 0.018f),
                TankM1A3FamilyDetails.Glass());
        }

        private static void AddSmokeAndAntennas(
            Transform turret,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int smoke = 0; smoke < 6; smoke++)
                {
                    Transform tube = TankDetailGeometry.Part(
                        "Painted-M1A3-SmokeLauncher",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * (1.28f + smoke * 0.035f),
                            0.58f + smoke * 0.02f,
                            0.72f - smoke * 0.05f),
                        new Vector3(0.045f, 0.14f, 0.045f),
                        color * 0.45f);
                    tube.localRotation =
                        Quaternion.Euler(
                            68f,
                            0f,
                            side * 18f);
                }
                for (int row = 0; row < 2; row++)
                {
                    float antennaZ =
                        row == 0 ? -2.82f : -1.58f;
                    Transform whip = TankDetailGeometry.Part(
                        "M1A3-NetworkAntenna",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * (row == 0 ? 1.18f : 1.34f),
                            1.12f,
                            antennaZ),
                        new Vector3(0.012f, 0.44f, 0.012f),
                        TankM1A3FamilyDetails.Gunmetal());
                    whip.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            side * (row == 0 ? 5f : 3f));
                }
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
