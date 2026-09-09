using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90TurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddCastDome(turret, color);
            AddKontakt5(turret, color);
            AddShtoraAndSights(turret, color);
            AddRoofWeapons(turret, color);
            AddBustleAndMasts(turret, color);
        }

        private static void AddCastDome(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T90-CastDome",
                PrimitiveType.Sphere,
                turret,
                new Vector3(0f, 0.26f, -0.03f),
                new Vector3(3.18f, 0.78f, 2.18f),
                color * 0.64f);
            Part(
                "Painted-T90-TurretRingCollar",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.02f, -0.02f),
                new Vector3(1.58f, 0.05f, 1.58f),
                color * 0.45f);
            Part(
                "Painted-T90-MantletTunnel",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.22f, 1.0f),
                new Vector3(0.54f, 0.3f, 0.6f),
                color * 0.43f);
        }

        private static void AddKontakt5(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T90-K5RoofPanel",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.72f, 0.69f, 0.08f),
                new Vector3(0.72f, 0.06f, 0.74f),
                color * 0.57f);
            Part(
                "Painted-T90-K5RoofPanel",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.76f, -0.16f),
                new Vector3(0.76f, 0.06f, 0.86f),
                color * 0.57f);
            Part(
                "Painted-T90-K5RoofPanel",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.72f, 0.69f, 0.08f),
                new Vector3(0.72f, 0.06f, 0.74f),
                color * 0.57f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform inner = Part(
                    "Painted-T90-K5CheekLeaf",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.62f, 0.44f, 1.16f),
                    new Vector3(0.7f, 0.34f, 0.48f),
                    color * 0.53f);
                inner.localRotation =
                    Quaternion.Euler(-25f, -side * 25f, 0f);
                Transform outer = Part(
                    "Painted-T90-K5CheekLeaf",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 1.25f, 0.4f, 0.72f),
                    new Vector3(0.76f, 0.34f, 0.36f),
                    color * 0.53f);
                outer.localRotation =
                    Quaternion.Euler(-23f, -side * 41f, 0f);
                Part(
                    "T90-K5CheekSeam",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.98f, 0.44f, 0.93f),
                    new Vector3(0.035f, 0.3f, 0.24f),
                    TankT90FamilyDetails.Dark());
            }
            Part(
                "T90-K5VertexGapPlate",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.42f, 1.31f),
                new Vector3(0.56f, 0.34f, 0.04f),
                TankT90FamilyDetails.Dark());
        }

        private static void AddShtoraAndSights(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T90-ShtoraHousing",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.52f, 0.5f, 1.3f),
                    new Vector3(0.14f, 0.11f, 0.14f),
                    color * 0.58f)
                    .localRotation =
                        Quaternion.Euler(90f, 0f, 0f);
                Part(
                    "T90-ShtoraLens",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.52f, 0.5f, 1.43f),
                    new Vector3(0.09f, 0.025f, 0.09f),
                    TankT90FamilyDetails.ShtoraGlass())
                    .localRotation =
                        Quaternion.Euler(90f, 0f, 0f);
            }
            Part(
                "Painted-T90-1G46SightHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.42f, 0.7f, 0.42f),
                new Vector3(0.26f, 0.15f, 0.3f),
                color * 0.6f);
            Part(
                "T90-1G46Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.42f, 0.71f, 0.59f),
                new Vector3(0.22f, 0.1f, 0.018f),
                TankT90FamilyDetails.Glass());
        }

        private static void AddRoofWeapons(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T90-CommanderCupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.52f, 0.7f, -0.3f),
                new Vector3(0.25f, 0.12f, 0.25f),
                color * 0.6f);
            Part(
                "T90-NsvtReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.6f, 0.91f, -0.42f),
                new Vector3(0.26f, 0.18f, 0.46f),
                TankT90FamilyDetails.Dark());
            Part(
                "T90-NsvtShield",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.49f, 0.92f, -0.18f),
                new Vector3(0.32f, 0.22f, 0.04f),
                TankT90FamilyDetails.Dark());
            Part(
                "T90-NsvtBarrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.64f, 0.94f, 0.2f),
                new Vector3(0.04f, 0.04f, 0.76f),
                TankT90FamilyDetails.Dark());
        }

        private static void AddBustleAndMasts(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T90-BustleRack",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.55f, -1.52f),
                new Vector3(1.5f, 0.53f, 0.46f),
                color * 0.58f);
            for (int rail = 0;
                rail < 5;
                rail++)
            {
                Part(
                    "T90-BustleRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(-0.65f + rail * 0.325f, 0.76f, -1.75f),
                    new Vector3(0.04f, 0.3f, 0.04f),
                    TankT90FamilyDetails.Dark());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < 4;
                    tube++)
                {
                    Transform launcher = Part(
                        "Painted-T90-SmokeLauncher",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * (1.02f + tube * 0.06f),
                            0.52f + tube * 0.024f,
                            -0.18f - tube * 0.04f),
                        new Vector3(0.04f, 0.13f, 0.04f),
                        color * 0.52f);
                    launcher.localRotation =
                        Quaternion.Euler(62f, 0f, side * 18f);
                }
            }
            Part(
                "T90-OpvtMast",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0.32f, 0.98f, -1.13f),
                new Vector3(0.052f, 0.64f, 0.052f),
                TankT90FamilyDetails.Dark());
            Part(
                "T90-RadioWhip",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.27f, 1.24f, -1.1f),
                new Vector3(0.014f, 1.2f, 0.014f),
                TankT90FamilyDetails.Dark());
            Part(
                "T90-RadioWhip",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(1.04f, 1.15f, 0.6f),
                new Vector3(0.014f, 1.1f, 0.014f),
                TankT90FamilyDetails.Dark());
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
