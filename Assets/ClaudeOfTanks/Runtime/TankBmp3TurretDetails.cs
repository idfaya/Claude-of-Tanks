using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmp3TurretDetails
    {
        public static void Build(
            Transform turret,
            Color color,
            bool rok)
        {
            AddLowTurret(
                turret,
                color,
                rok);
            AddCrewStations(
                turret,
                color,
                rok);
            AddSmokeBanks(
                turret,
                color,
                rok);
            AddRoofMachineGun(
                turret,
                color,
                rok);
            AddAntennas(
                turret,
                color,
                rok);
        }

        private static void AddLowTurret(
            Transform turret,
            Color color,
            bool rok)
        {
            TankDetailGeometry.Part(
                "Painted-Bmp3-TurretRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    0f,
                    rok ? 0.18f : -0.015f,
                    rok ? -0.08f : 0.02f),
                new Vector3(
                    rok ? 1.16f : 1.06f,
                    rok ? 0.17f : 0.05f,
                    rok ? 1.02f : 1f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-LowTurret",
                PrimitiveType.Sphere,
                turret,
                new Vector3(
                    0f,
                    rok ? 0.46f : 0.27f,
                    rok ? -0.02f : 0.06f),
                new Vector3(
                    rok ? 2.08f : 2.12f,
                    rok ? 0.52f : 0.54f,
                    rok ? 1.98f : 2.18f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-CrewBasket",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    0f,
                    rok ? -0.34f : -0.6f,
                    0.02f),
                new Vector3(
                    rok ? 0.64f : 0.66f,
                    rok ? 0.34f : 0.7f,
                    rok ? 0.64f : 0.66f),
                color * 0.52f);
            Transform saddle =
                TankDetailGeometry.Part(
                    "Painted-Bmp3-GunSaddle",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        rok ? 0.42f : 0.28f,
                        rok ? 0.76f : 0.91f),
                    new Vector3(
                        rok ? 0.82f : 0.72f,
                        rok ? 0.4f : 0.34f,
                        rok ? 0.58f : 0.52f),
                    color * 0.62f);
            saddle.localRotation =
                Quaternion.Euler(-8f, 0f, 0f);
        }

        private static void AddCrewStations(
            Transform turret,
            Color color,
            bool rok)
        {
            float roof = rok ? 0.735f : 0.475f;
            float lid = rok ? 0.805f : 0.535f;
            AddCrewStation(
                turret,
                color,
                -0.39f,
                roof,
                lid,
                -0.25f,
                0.235f);
            AddCrewStation(
                turret,
                color,
                0.4f,
                roof,
                lid,
                -0.34f,
                0.255f);

            TankDetailGeometry.Part(
                "Painted-Bmp3-CommanderSightPedestal",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -0.4f,
                    rok ? 0.72f : 0.44f,
                    -0.18f),
                new Vector3(0.3f, 0.07f, 0.3f),
                color * 0.6f);
            AddVerticalCylinder(
                "Painted-Bmp3-CommanderSight",
                turret,
                0.14f,
                rok ? 0.16f : 0.14f,
                -0.4f,
                rok ? 0.84f : 0.54f,
                -0.18f,
                color * 0.57f);
            TankDetailGeometry.Part(
                "Bmp3-CommanderSightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -0.4f,
                    rok ? 0.88f : 0.56f,
                    -0.02f),
                new Vector3(0.17f, 0.1f, 0.024f),
                TankBmp3FamilyDetails.Lens());
            TankDetailGeometry.Part(
                "Painted-Bmp3-TknSight",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0.44f,
                    rok ? 0.79f : 0.52f,
                    0.02f),
                new Vector3(0.15f, 0.08f, 0.15f),
                color * 0.59f);
            TankDetailGeometry.Part(
                "Bmp3-TknSightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0.44f,
                    rok ? 0.815f : 0.545f,
                    0.105f),
                new Vector3(0.1f, 0.03f, 0.016f),
                TankBmp3FamilyDetails.Lens());
        }

        private static void AddCrewStation(
            Transform turret,
            Color color,
            float x,
            float roof,
            float lid,
            float z,
            float radius)
        {
            AddVerticalCylinder(
                "Painted-Bmp3-CrewCupola",
                turret,
                radius,
                0.085f,
                x,
                roof,
                z,
                color * 0.64f);
            Transform hatch =
                TankDetailGeometry.Part(
                    "Painted-Bmp3-CrewHatch",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(x, lid, z),
                    new Vector3(
                        radius * 1.45f,
                        0.055f,
                        radius * 1.5f),
                    color * 0.7f);
            hatch.localRotation =
                Quaternion.Euler(
                    0f,
                    x < 0f ? -5f : 6f,
                    0f);
            for (int optic = -1;
                optic <= 1;
                optic++)
            {
                TankDetailGeometry.Part(
                    "Bmp3-CupolaPeriscope",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x + optic * 0.09f,
                        lid + 0.035f,
                        z + radius * 0.73f),
                    new Vector3(0.078f, 0.045f, 0.026f),
                    TankBmp3FamilyDetails.Lens());
            }
        }

        private static void AddSmokeBanks(
            Transform turret,
            Color color,
            bool rok)
        {
            int count = rok ? 4 : 3;
            float x = rok ? 0.82f : 0.92f;
            float y = rok ? 0.58f : 0.29f;
            float z = rok ? 0.14f : -0.52f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmp3-SmokeBankCollar",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * (rok ? 0.94f : 0.88f),
                        rok ? 0.48f : 0.24f,
                        rok ? 0.05f : -0.53f),
                    new Vector3(
                        0.16f,
                        rok ? 0.28f : 0.26f,
                        rok ? 0.62f : 0.5f),
                    color * 0.6f);
                for (int tube = 0;
                    tube < count;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Bmp3-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (x +
                                     tube * 0.045f),
                                y +
                                    tube * 0.035f,
                                z -
                                    tube * 0.085f),
                            new Vector3(
                                rok ? 0.045f : 0.065f,
                                rok ? 0.14f : 0.22f,
                                rok ? 0.045f : 0.065f),
                            color * 0.48f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            66f,
                            side * 8f,
                            side * 15f);
                }
            }
        }

        private static void AddRoofMachineGun(
            Transform turret,
            Color color,
            bool rok)
        {
            float y = rok ? 0.89f : 0.59f;
            AddVerticalCylinder(
                "Painted-Bmp3-RoofMgMount",
                turret,
                0.15f,
                0.08f,
                0.38f,
                y,
                -0.3f,
                color * 0.55f);
            TankDetailGeometry.Part(
                "Bmp3-RoofMgReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.38f, y + 0.1f, -0.2f),
                new Vector3(0.14f, 0.13f, 0.3f),
                TankBmp3FamilyDetails.Gunmetal());
            AddAxialCylinder(
                "Bmp3-RoofMgBarrel",
                turret,
                0.018f,
                0.72f,
                0.26f,
                TankBmp3FamilyDetails.Dark(),
                0.38f,
                y + 0.11f);
            TankDetailGeometry.Part(
                "Painted-Bmp3-RoofMgShield",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.38f, y + 0.14f, -0.03f),
                new Vector3(0.42f, 0.22f, 0.04f),
                color * 0.52f);
        }

        private static void AddAntennas(
            Transform turret,
            Color color,
            bool rok)
        {
            float shelfY = rok ? 0.6325f : 0.5f;
            float baseY = rok ? 0.66f : 0.54f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmp3-AntennaShelf",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 0.575f,
                        shelfY,
                        -0.74f),
                    new Vector3(0.43f, 0.055f, 0.34f),
                    color * 0.58f);
                AddVerticalCylinder(
                    "Painted-Bmp3-AntennaBase",
                    turret,
                    0.055f,
                    0.07f,
                    side * 0.76f,
                    baseY,
                    -0.86f,
                    color * 0.48f);
                float height = side < 0 ? 0.72f : 0.6f;
                Transform whip =
                    TankDetailGeometry.Part(
                        "Bmp3-RadioAntenna",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * 0.76f,
                            baseY + 0.05f + height * 0.5f,
                            -0.86f),
                        new Vector3(
                            0.009f,
                            height * 0.5f,
                            0.009f),
                        TankBmp3FamilyDetails.Dark());
                whip.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 2.3f);
            }
        }

        private static void AddVerticalCylinder(
            string name,
            Transform parent,
            float radius,
            float height,
            float x,
            float y,
            float z,
            Color color)
        {
            TankDetailGeometry.Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                new Vector3(x, y, z),
                new Vector3(radius, height, radius),
                color);
        }

        private static void AddAxialCylinder(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color,
            float x,
            float y)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(x, y, z),
                    new Vector3(radius, length * 0.5f, radius),
                    color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
