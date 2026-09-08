using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankType89FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "type89";
        }

        public static bool SupportsRunningGear(string id)
        {
            return Supports(id) ||
                TankType89LightTigerDetails.Supports(id);
        }

        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            if (!Supports(definition?.id)) return;
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideRenderer(root.Find("Armor-track_L"));
            HideRenderer(root.Find("Armor-track_R"));
            HideRenderer(turret.Find("Turret"));
            HideRenderer(turret.Find("Gun"));
            AddHull(root, color);
            AddTurret(turret, color);
            TankType89GunDetails.Build(
                turret,
                definition,
                color);
        }

        private static void AddHull(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Type89-Tub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.9f, -0.325f),
                new Vector3(1.9f, 0.9f, 5.95f),
                color * 0.56f);
            TankDetailGeometry.Part(
                "Painted-Type89-UpperBody",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.52f, -1.04f),
                new Vector3(2.9f, 0.5f, 4.58f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Painted-Type89-Deck",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.78f, -1.1f),
                new Vector3(2.8f, 0.045f, 4.44f),
                color * 0.72f);
            Transform glacis =
                TankDetailGeometry.Part(
                    "Painted-Type89-LongGlacis",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.38f, 2.48f),
                    new Vector3(2.72f, 0.12f, 2f),
                    color * 0.72f);
            glacis.localRotation =
                Quaternion.Euler(-28.6f, 0f, 0f);
            TankDetailGeometry.Part(
                "Painted-Type89-NoseBlock",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 0.77f, 3.28f),
                new Vector3(2.4f, 0.34f, 0.26f),
                color * 0.55f);
            Transform lower =
                TankDetailGeometry.Part(
                    "Painted-Type89-LowerBow",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 0.56f, 3.02f),
                    new Vector3(1.9f, 0.12f, 0.78f),
                    color * 0.5f);
            lower.localRotation =
                Quaternion.Euler(23f, 0f, 0f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Type89-ProwWall",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.42f, 1.12f, 2.35f),
                    new Vector3(0.08f, 0.52f, 1.62f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "Painted-Type89-Fender",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.48f, 1.295f, -0.1f),
                    new Vector3(0.24f, 0.05f, 5.85f),
                    color * 0.63f);
                TankDetailGeometry.Part(
                    "Painted-Type89-ThinSkirt",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.5f, 1.02f, -0.1f),
                    new Vector3(0.03f, 0.2f, 5.7f),
                    color * 0.58f);
                for (int seam = 0;
                    seam < 3;
                    seam++)
                {
                    TankDetailGeometry.Part(
                        "Type89-SkirtSeam",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.516f,
                            1.01f,
                            1.4f - seam * 1.5f),
                        new Vector3(0.014f, 0.16f, 0.015f),
                        Dark());
                }
                TankDetailGeometry.Part(
                    "Type89-Headlight",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(side * 1.12f, 1.04f, 3.16f),
                    new Vector3(0.1f, 0.1f, 0.08f),
                    Lens());
                TankDetailGeometry.Part(
                    "Painted-Type89-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.12f, 1.13f, 3.17f),
                    new Vector3(0.3f, 0.02f, 0.15f),
                    color * 0.56f);
                TankDetailGeometry.Part(
                    "Type89-MirrorPost",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.26f, 1.32f, 2.58f),
                    new Vector3(0.022f, 0.3f, 0.022f),
                    Dark())
                    .localRotation =
                    Quaternion.Euler(-28.6f, 0f, 0f);
                TankDetailGeometry.Part(
                    "Type89-MirrorHead",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.31f, 1.5f, 2.46f),
                    new Vector3(0.13f, 0.17f, 0.02f),
                    Lens());
                for (int port = 0;
                    port < 3;
                    port++)
                {
                    float z = -1.35f - port * 0.7f;
                    TankDetailGeometry.Part(
                        "Type89-FiringPort",
                        PrimitiveType.Sphere,
                        root,
                        new Vector3(side * 1.455f, 1.38f, z),
                        new Vector3(0.05f, 0.05f, 0.05f),
                        Dark());
                    TankDetailGeometry.Part(
                        "Type89-FiringPortVision",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.457f, 1.565f, z + 0.09f),
                        new Vector3(0.047f, 0.02f, 0.08f),
                        Lens());
                }
                TankDetailGeometry.Part(
                    "Painted-Type89-RearBin",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.28f, 1.52f, -3.32f),
                    new Vector3(0.34f, 0.42f, 0.26f),
                    color * 0.6f);
            }
            Transform driver =
                TankDetailGeometry.Part(
                    "Painted-Type89-DriverHatch",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0.78f, 1.7f, 1.95f),
                    new Vector3(0.56f, 0.06f, 0.56f),
                    color * 0.66f);
            driver.localRotation =
                Quaternion.Euler(28.6f, 0f, 0f);
            for (int scope = 0;
                scope < 3;
                scope++)
            {
                TankDetailGeometry.Part(
                    "Type89-DriverPeriscope",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0.56f + scope * 0.22f,
                        1.87f,
                        1.58f),
                    new Vector3(0.12f, 0.05f, 0.05f),
                    Lens());
            }
            Transform powerpack =
                TankDetailGeometry.Part(
                    "Painted-Type89-PowerpackFrame",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(-0.62f, 1.505f, 2.3f),
                    new Vector3(1.1f, 0.055f, 1.05f),
                    color * 0.62f);
            powerpack.localRotation =
                Quaternion.Euler(28.6f, 0f, 0f);
            for (int slat = 0;
                slat < 5;
                slat++)
            {
                TankDetailGeometry.Part(
                    "Type89-PowerpackLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.62f,
                        1.325f + slat * 0.093f,
                        2.64f - slat * 0.17f),
                    new Vector3(1f, 0.02f, 0.13f),
                    Dark())
                    .localRotation =
                    Quaternion.Euler(28.6f, 0f, 0f);
            }
            TankDetailGeometry.Part(
                "Painted-Type89-RearDoor",
                PrimitiveType.Cube,
                root,
                new Vector3(-0.1f, 1.16f, -3.375f),
                new Vector3(2.1f, 1.06f, 0.09f),
                color * 0.57f);
            TankDetailGeometry.Part(
                "Type89-DoorLeaf",
                PrimitiveType.Cube,
                root,
                new Vector3(0.22f, 1.14f, -3.425f),
                new Vector3(0.72f, 0.94f, 0.025f),
                Dark());
            TankDetailGeometry.Part(
                "Type89-LeftExhaust",
                PrimitiveType.Cube,
                root,
                new Vector3(-1.44f, 1.5f, 0.1f),
                new Vector3(0.03f, 0.26f, 0.72f),
                Dark());
        }

        private static void AddTurret(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Type89-TurretRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.04f, -0.05f),
                new Vector3(0.66f, 0.09f, 0.66f),
                color * 0.53f);
            TankDetailGeometry.Part(
                "Painted-Type89-WeldedTurret",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.3f, -0.08f),
                new Vector3(1.44f, 0.56f, 1.18f),
                color * 0.69f);
            TankDetailGeometry.Part(
                "Painted-Type89-RakedFace",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.3f, 0.58f),
                new Vector3(1.16f, 0.56f, 0.22f),
                color * 0.62f)
                .localRotation =
                Quaternion.Euler(-14f, 0f, 0f);
            TankDetailGeometry.Part(
                "Painted-Type89-Bustle",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.21f, -0.86f),
                new Vector3(1.08f, 0.38f, 0.28f),
                color * 0.63f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform launcher =
                    TankDetailGeometry.Part(
                        "Painted-Type89-JyuMatBox",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(side * 0.92f, 0.32f, -0.16f),
                        new Vector3(0.36f, 0.36f, 0.92f),
                        color * 0.61f);
                launcher.localRotation =
                    Quaternion.Euler(-8f, 0f, 0f);
                TankDetailGeometry.Part(
                    "Type89-JyuMatMouth",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.92f, 0.385f, 0.285f),
                    new Vector3(0.1f, 0.02f, 0.1f),
                    Dark())
                    .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                for (int tube = 0;
                    tube < 3;
                    tube++)
                {
                    Transform smoke =
                        TankDetailGeometry.Part(
                            "Painted-Type89-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * (0.58f + tube * 0.06f),
                                0.36f + tube * 0.025f,
                                -0.68f - tube * 0.04f),
                            new Vector3(0.038f, 0.11f, 0.038f),
                            color * 0.46f);
                    smoke.localRotation =
                        Quaternion.Euler(67f, 0f, side * 18f);
                }
            }
            AddVertical(
                "Painted-Type89-CommanderCupola",
                turret,
                0.24f,
                0.06f,
                0.33f,
                0.65f,
                -0.26f,
                color * 0.64f);
            AddVertical(
                "Painted-Type89-GunnerHatch",
                turret,
                0.2f,
                0.03f,
                -0.32f,
                0.635f,
                -0.34f,
                color * 0.63f);
            TankDetailGeometry.Part(
                "Painted-Type89-GunnerSight",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.3f, 0.6f, 0.36f),
                new Vector3(0.3f, 0.18f, 0.26f),
                color * 0.59f);
            TankDetailGeometry.Part(
                "Type89-GunnerSightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.3f, 0.65f, 0.512f),
                new Vector3(0.2f, 0.045f, 0.014f),
                Lens());
            TankDetailGeometry.Part(
                "Type89-BustleBasket",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.17f, -1.05f),
                new Vector3(1.1f, 0.26f, 0.28f),
                Gunmetal());
            TankDetailGeometry.Part(
                "Type89-RadioAntenna",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.62f, 1.02f, -0.68f),
                new Vector3(0.01f, 0.42f, 0.01f),
                Dark());
        }

        private static void AddVertical(
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

        private static void HideRenderer(
            Transform part)
        {
            Renderer renderer =
                part == null
                    ? null
                    : part.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }

        internal static Color Dark()
        {
            return new Color(0.045f, 0.055f, 0.045f);
        }

        internal static Color Gunmetal()
        {
            return new Color(0.11f, 0.12f, 0.1f);
        }

        internal static Color Lens()
        {
            return new Color(0.02f, 0.075f, 0.08f);
        }
    }
}
