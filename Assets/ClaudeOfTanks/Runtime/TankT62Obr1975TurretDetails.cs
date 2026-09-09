using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT62Obr1975TurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddCasting(turret, color);
            AddCrewStations(turret, color);
            AddOptics(turret, color);
            AddRearTools(turret);
            AddDshk(turret, color);
            AddRadioWhip(turret, color);
        }

        private static void AddCasting(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T62-TurretRace",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.025f, -0.5f),
                new Vector3(1.48f, 0.05f, 1.58f),
                color * 0.58f);
            Part(
                "Painted-T62-CastDome",
                PrimitiveType.Sphere,
                turret,
                new Vector3(0f, 0.43f, -0.49f),
                new Vector3(3.02f, 1.55f, 3.05f),
                color * 0.72f);
            Part(
                "Painted-T62-MantletShoulder",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, 0.36f, 0.94f),
                new Vector3(0.43f, 0.18f, 0.43f),
                color * 0.61f)
                .localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T62-CheekStowageBracket",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * 1.364f,
                        0.47f,
                        0.41f),
                    new Vector3(
                        0.176f,
                        0.18f,
                        0.26f),
                    color * 0.63f)
                    .localRotation =
                    Quaternion.Euler(
                        -3f,
                        side * 8f,
                        0f);
                Transform roll = Part(
                    "Painted-T62-CheekStowageRoll",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * 1.43f,
                        0.47f,
                        0.57f),
                    new Vector3(
                        0.13f,
                        0.045f,
                        0.13f),
                    color * 0.66f);
                roll.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void AddCrewStations(
            Transform turret,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-T62-CommanderCupola",
                turret,
                new Vector3(
                    -0.77f,
                    0.91f,
                    -0.5f),
                0.27f,
                0.15f,
                color * 0.69f);
            AddVerticalCylinder(
                "Painted-T62-CommanderHatch",
                turret,
                new Vector3(
                    -0.77f,
                    1.015f,
                    -0.5f),
                0.22f,
                0.055f,
                color * 0.76f);
            AddVerticalCylinder(
                "Painted-T62-LoaderCupola",
                turret,
                new Vector3(
                    0.798f,
                    0.92f,
                    -0.324f),
                0.26f,
                0.14f,
                color * 0.69f);
            AddVerticalCylinder(
                "Painted-T62-LoaderHatch",
                turret,
                new Vector3(
                    0.798f,
                    1.005f,
                    -0.324f),
                0.2f,
                0.035f,
                color * 0.75f);
            Part(
                "Painted-T62-VentilatorDome",
                PrimitiveType.Sphere,
                turret,
                new Vector3(
                    0.286f,
                    0.78f,
                    0.278f),
                new Vector3(
                    0.25f,
                    0.18f,
                    0.25f),
                color * 0.68f);
        }

        private static void AddOptics(
            Transform turret,
            Color color)
        {
            float[] xs =
            {
                -1.045f,
                -0.946f,
                -0.495f,
                0.374f,
                0.836f,
                1.089f
            };
            float[] ys =
            {
                0.91f,
                0.94f,
                0.99f,
                0.94f,
                0.98f,
                0.88f
            };
            float[] zs =
            {
                -0.3f,
                -0.08f,
                -0.02f,
                -0.03f,
                -0.05f,
                -0.27f
            };
            for (int index = 0;
                index < xs.Length;
                index++)
            {
                Part(
                    "Painted-T62-PeriscopeHousing",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        xs[index],
                        ys[index] - 0.035f,
                        zs[index]),
                    new Vector3(
                        0.16f,
                        0.055f,
                        0.14f),
                    color * 0.6f);
                Part(
                    "T62-PeriscopeLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        xs[index],
                        ys[index],
                        zs[index] + 0.065f),
                    new Vector3(
                        0.12f,
                        0.055f,
                        0.025f),
                    TankT62Obr1975FamilyDetails.Glass());
            }
            Part(
                "Painted-T62-InfraredSightHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0.638f,
                    0.93f,
                    0.14f),
                new Vector3(
                    0.26f,
                    0.16f,
                    0.18f),
                color * 0.61f);
            Transform head = Part(
                "T62-InfraredSightLens",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    0.638f,
                    1.06f,
                    0.25f),
                new Vector3(
                    0.18f,
                    0.06f,
                    0.18f),
                TankT62Obr1975FamilyDetails.Glass());
            head.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddRearTools(
            Transform turret)
        {
            float[] ys = { 0.4f, 0.25f };
            float[] zs = { -1.61f, -1.68f };
            float[] radii = { 0.075f, 0.06f };
            float[] lengths = { 1.892f, 1.694f };
            for (int row = 0;
                row < ys.Length;
                row++)
            {
                Transform roll = Part(
                    "T62-TurretToolRoll",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        0f,
                        ys[row],
                        zs[row]),
                    new Vector3(
                        radii[row],
                        lengths[row] * 0.5f,
                        radii[row]),
                    TankT62Obr1975FamilyDetails.Rubber());
                roll.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    Part(
                        "T62-TurretToolSaddle",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side *
                                lengths[row] *
                                0.34f,
                            ys[row] - 0.03f,
                            zs[row] + 0.02f),
                        new Vector3(
                            0.07f,
                            0.19f,
                            0.12f),
                        TankT62Obr1975FamilyDetails.Dark());
                }
            }
        }

        private static void AddDshk(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T62-DshkCradle",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -0.682f,
                    1.035f,
                    -0.27f),
                new Vector3(
                    0.42f,
                    0.065f,
                    0.28f),
                color * 0.58f);
            Part(
                "T62-DshkReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -0.58f,
                    1.145f,
                    -0.1f),
                new Vector3(
                    0.18f,
                    0.17f,
                    0.42f),
                TankT62Obr1975FamilyDetails.Dark());
            Transform barrel = Part(
                "T62-DshkBarrel",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    -0.58f,
                    1.12f,
                    0.46f),
                new Vector3(
                    0.025f,
                    0.42f,
                    0.025f),
                TankT62Obr1975FamilyDetails.Dark());
            barrel.localRotation =
                Quaternion.Euler(92f, 0f, 0f);
            Part(
                "T62-DshkAmmoBox",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -0.76f,
                    1.12f,
                    -0.12f),
                new Vector3(
                    0.18f,
                    0.2f,
                    0.26f),
                TankT62Obr1975FamilyDetails.Dark());
        }

        private static void AddRadioWhip(
            Transform turret,
            Color color)
        {
            AddVerticalCylinder(
                "Painted-T62-AntennaBase",
                turret,
                new Vector3(
                    0.968f,
                    0.72f,
                    -0.48f),
                0.07f,
                0.08f,
                color * 0.56f);
            AddVerticalCylinder(
                "T62-RadioWhip",
                turret,
                new Vector3(
                    0.968f,
                    1.6f,
                    -0.48f),
                0.012f,
                1.72f,
                TankT62Obr1975FamilyDetails.Dark());
        }

        private static void AddVerticalCylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float height,
            Color color)
        {
            Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                position,
                new Vector3(
                    radius,
                    height * 0.5f,
                    radius),
                color);
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                type,
                parent,
                position,
                scale,
                color);
        }
    }
}
