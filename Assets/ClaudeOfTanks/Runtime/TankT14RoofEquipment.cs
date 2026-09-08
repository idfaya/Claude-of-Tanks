using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT14RoofEquipment
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddPanoramicTower(
                turret,
                color);
            AddMeteoMast(
                turret,
                color);
            AddRemoteAutocannon(
                turret,
                color);
            AddRoofMachineGun(
                turret,
                color,
                roof);
            AddRearAntennas(
                turret,
                color);
        }

        private static void AddPanoramicTower(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-T14-PanoramicTower",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.7f, 0.985f, -1.72f),
                new Vector3(0.09f, 0.3f, 0.1f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "T14-PanoramicShaft",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.7f, 1.215f, -1.72f),
                new Vector3(0.03f, 0.08f, 0.03f),
                TankT14FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "T14-PanoramicHead",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.7f, 1.32f, -1.72f),
                new Vector3(0.038f, 0.045f, 0.038f),
                TankT14FamilyDetails.Dark());
            TankDetailGeometry.Part(
                "T14-PanoramicLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.7f, 1.325f, -1.675f),
                new Vector3(0.05f, 0.05f, 0.012f),
                TankT14FamilyDetails.Lens());
            TankDetailGeometry.Part(
                "T14-PanoramicWhipBracket",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.765f, 1.13f, -1.72f),
                new Vector3(0.12f, 0.026f, 0.03f),
                TankT14FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "T14-PanoramicWhip",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.82f, 1.585f, -1.72f),
                new Vector3(0.01f, 0.45f, 0.01f),
                TankT14FamilyDetails.Dark());
        }

        private static void AddMeteoMast(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-T14-MeteoBase",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.7f, 0.895f, 0.72f),
                new Vector3(0.1f, 0.1f, 0.1f),
                color * 0.63f);
            TankDetailGeometry.Part(
                "T14-MeteoMast",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.7f, 1.22f, 0.72f),
                new Vector3(0.025f, 0.28f, 0.025f),
                TankT14FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "T14-MeteoCrossbar",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.7f, 1.38f, 0.72f),
                new Vector3(0.14f, 0.025f, 0.025f),
                TankT14FamilyDetails.Gunmetal());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform vane =
                    TankDetailGeometry.Part(
                        "T14-MeteoVane",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            -0.7f + side * 0.065f,
                            1.38f,
                            0.72f),
                        new Vector3(0.024f, 0.03f, 0.024f),
                        TankT14FamilyDetails.Dark());
                vane.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
            }
            TankDetailGeometry.Part(
                "T14-MeteoTip",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.7f, 1.64f, 0.72f),
                new Vector3(0.045f, 0.09f, 0.045f),
                TankT14FamilyDetails.Dark());
        }

        private static void AddRemoteAutocannon(
            Transform turret,
            Color color)
        {
            const float stationX = -0.23f;
            const float stationZ = -1.08f;
            TankDetailGeometry.Part(
                "Painted-T14-RwsPedestal",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.23f, 0.945f, -0.75f),
                new Vector3(0.78f, 0.22f, 1.1f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Painted-T14-RwsRearElectronics",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.23f, 1.185f, -1.1125f),
                new Vector3(0.74f, 0.26f, 0.555f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-T14-RwsFrontElectronics",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.25f, 1.155f, -0.395f),
                new Vector3(0.6f, 0.2f, 0.55f),
                color * 0.65f);
            TankDetailGeometry.Part(
                "T14-RwsElectronicsLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.25f, 1.195f, -0.11f),
                new Vector3(0.16f, 0.08f, 0.02f),
                TankT14FamilyDetails.Lens());

            TankDetailGeometry.Part(
                "T14-PrimaryRwsBearing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(stationX, 1.3525f, stationZ),
                new Vector3(0.18f, 0.04f, 0.18f),
                TankT14FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "T14-PrimaryRwsReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(stationX, 1.47f, stationZ + 0.08f),
                new Vector3(0.44f, 0.16f, 0.42f),
                TankT14FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "T14-PrimaryRwsFeedBox",
                PrimitiveType.Cube,
                turret,
                new Vector3(stationX - 0.34f, 1.475f, stationZ + 0.04f),
                new Vector3(0.25f, 0.2f, 0.38f),
                color * 0.42f);
            TankDetailGeometry.Part(
                "T14-PrimaryRwsOptic",
                PrimitiveType.Cube,
                turret,
                new Vector3(stationX + 0.31f, 1.495f, stationZ + 0.1f),
                new Vector3(0.2f, 0.22f, 0.25f),
                TankT14FamilyDetails.Dark());
            TankDetailGeometry.Part(
                "T14-PrimaryRwsOpticLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(stationX + 0.31f, 1.51f, stationZ + 0.236f),
                new Vector3(0.1f, 0.09f, 0.014f),
                TankT14FamilyDetails.Lens());
            Transform barrel =
                TankDetailGeometry.Part(
                    "T14-PrimaryRws30mmBarrel",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(stationX, 1.515f, stationZ + 1.08f),
                    new Vector3(0.052f, 0.63f, 0.052f),
                    TankT14FamilyDetails.Dark());
            barrel.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            Transform muzzle =
                TankDetailGeometry.Part(
                    "T14-PrimaryRws30mmMuzzle",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(stationX, 1.515f, stationZ + 1.79f),
                    new Vector3(0.067f, 0.08f, 0.067f),
                    TankT14FamilyDetails.Dark());
            muzzle.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddRoofMachineGun(
            Transform turret,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "Painted-T14-RoofMachineGunSocket",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.78f, roof + 0.035f, -0.08f),
                new Vector3(0.14f, 0.035f, 0.14f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "T14-RoofMachineGunReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.78f, roof + 0.15f, -0.01f),
                new Vector3(0.18f, 0.16f, 0.42f),
                TankT14FamilyDetails.Gunmetal());
            Transform barrel =
                TankDetailGeometry.Part(
                    "T14-RoofMachineGunBarrel",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(-0.78f, roof + 0.16f, 0.43f),
                    new Vector3(0.022f, 0.34f, 0.022f),
                    TankT14FamilyDetails.Dark());
            barrel.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                "T14-RoofMachineGunAmmoBox",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.94f, roof + 0.12f, -0.04f),
                new Vector3(0.16f, 0.2f, 0.28f),
                color * 0.4f);
        }

        private static void AddRearAntennas(
            Transform turret,
            Color color)
        {
            float[] seats =
            {
                -0.66f,
                0.46f
            };
            for (int antenna = 0;
                antenna < seats.Length;
                antenna++)
            {
                TankDetailGeometry.Part(
                    "Painted-T14-RearAntennaCollar",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        seats[antenna],
                        0.8625f,
                        -1.88f),
                    new Vector3(0.06f, 0.03f, 0.06f),
                    color * 0.52f);
                Transform whip =
                    TankDetailGeometry.Part(
                        "T14-RearAntenna",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            seats[antenna],
                            1.28f,
                            -1.88f),
                        new Vector3(0.01f, 0.39f, 0.01f),
                        TankT14FamilyDetails.Dark());
                whip.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        antenna == 0
                            ? -2f
                            : 2f);
            }
        }
    }
}
