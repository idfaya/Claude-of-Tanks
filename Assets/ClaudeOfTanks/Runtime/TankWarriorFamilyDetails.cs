using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankWarriorFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "fv510" ||
                id == "fv510_milan";
        }

        public static bool IsMilan(string id)
        {
            return id == "fv510_milan";
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
            bool milan = IsMilan(definition.id);
            if (milan) AddMilanKit(root, turret, color);
            TankWarriorGunDetails.Build(
                turret,
                definition,
                color);
        }

        private static void AddHull(
            Transform root,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Warrior-InnerTub",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1f, 0.1f),
                new Vector3(1.72f, 0.66f, 5.3f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "Painted-Warrior-SponsonBand",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.5f, -0.25f),
                new Vector3(2.3f, 0.64f, 5.15f),
                color * 0.69f);
            Transform glacis =
                TankDetailGeometry.Part(
                    "Painted-Warrior-Glacis",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 1.38f, 2.68f),
                    new Vector3(2.82f, 0.12f, 1.12f),
                    color * 0.72f);
            glacis.localRotation =
                Quaternion.Euler(-28.8f, 0f, 0f);
            Transform lower =
                TankDetailGeometry.Part(
                    "Painted-Warrior-LowerBow",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0f, 0.7f, 2.8f),
                    new Vector3(1.8f, 0.13f, 0.74f),
                    color * 0.53f);
            lower.localRotation =
                Quaternion.Euler(39f, 0f, 0f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 6;
                    panel++)
                {
                    float z = 1.89f - panel * 0.82f;
                    TankDetailGeometry.Part(
                        "Painted-Warrior-WrapPanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(side * 1.485f, 1.3f, z),
                        new Vector3(0.1f, 1.08f, 0.76f),
                        color * 0.65f);
                    for (int rib = -1;
                        rib <= 1;
                        rib += 2)
                    {
                        Transform strake =
                            TankDetailGeometry.Part(
                                "Warrior-WrapStrake",
                                PrimitiveType.Cube,
                                root,
                                new Vector3(
                                    side * 1.542f,
                                    1.45f,
                                    z + rib * 0.2f),
                                new Vector3(0.03f, 0.1f, 0.66f),
                                Gunmetal());
                        strake.localRotation =
                            Quaternion.Euler(rib * 26f, 0f, 0f);
                    }
                }
                for (int drop = 0;
                    drop < 5;
                    drop++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Warrior-LowerDrop",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.48f,
                            0.75f,
                            1.8f - drop * 0.9f),
                        new Vector3(0.09f, 0.22f, 0.46f),
                        color * 0.59f);
                }
                TankDetailGeometry.Part(
                    "Warrior-MudFlap",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.25f, 1.14f, 3.12f),
                    new Vector3(0.36f, 0.27f, 0.028f),
                    Dark());
                TankDetailGeometry.Part(
                    "Warrior-MirrorPost",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.26f, 1.62f, 3.02f),
                    new Vector3(0.024f, 0.58f, 0.024f),
                    Dark());
                TankDetailGeometry.Part(
                    "Warrior-MirrorHead",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.4f, 1.83f, 3.045f),
                    new Vector3(0.17f, 0.24f, 0.022f),
                    Lens());
                TankDetailGeometry.Part(
                    "Painted-Warrior-RearBin",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.1f, 1.72f, -2.89f),
                    new Vector3(0.42f, 0.46f, 0.1f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "Warrior-RearLight",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.22f, 1.1f, -2.93f),
                    new Vector3(0.12f, 0.08f, 0.035f),
                    new Color(0.34f, 0.03f, 0.025f));
            }
            AddVerticalCylinder(
                "Painted-Warrior-DriverHatch",
                root,
                0.235f,
                0.025f,
                -0.6f,
                1.955f,
                1.22f,
                color * 0.67f);
            TankDetailGeometry.Part(
                "Warrior-DriverPeriscope",
                PrimitiveType.Cube,
                root,
                new Vector3(-0.6f, 1.99f, 1.54f),
                new Vector3(0.34f, 0.04f, 0.025f),
                Lens());
            TankDetailGeometry.Part(
                "Painted-Warrior-PowerpackDeck",
                PrimitiveType.Cube,
                root,
                new Vector3(0.66f, 1.943f, 1f),
                new Vector3(1.16f, 0.05f, 1.26f),
                color * 0.64f);
            for (int louvre = 0;
                louvre < 6;
                louvre++)
            {
                TankDetailGeometry.Part(
                    "Warrior-PowerpackLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(0.66f, 1.972f, 0.5f + louvre * 0.2f),
                    new Vector3(1.02f, 0.018f, 0.115f),
                    Dark());
            }
            TankDetailGeometry.Part(
                "Painted-Warrior-LeftExhaustCowl",
                PrimitiveType.Cube,
                root,
                new Vector3(-1.375f, 1.9f, 0.8f),
                new Vector3(0.26f, 0.32f, 0.94f),
                color * 0.57f);
            TankDetailGeometry.Part(
                "Warrior-ExhaustOutlet",
                PrimitiveType.Cube,
                root,
                new Vector3(-1.51f, 1.9f, 0.8f),
                new Vector3(0.02f, 0.24f, 0.85f),
                Dark());
            TankDetailGeometry.Part(
                "Painted-Warrior-TroopHatch",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 2.062f, -1.62f),
                new Vector3(1.18f, 0.032f, 0.94f),
                color * 0.67f);
            TankDetailGeometry.Part(
                "Painted-Warrior-RearDoor",
                PrimitiveType.Cube,
                root,
                new Vector3(-0.03f, 1.32f, -2.88f),
                new Vector3(0.86f, 0.96f, 0.05f),
                color * 0.58f);
        }

        private static void AddTurret(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Warrior-TurretRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, -0.06f, 0f),
                new Vector3(0.74f, 0.08f, 0.78f),
                color * 0.54f);
            TankDetailGeometry.Part(
                "Painted-Warrior-WeldedTurret",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.28f, -0.25f),
                new Vector3(1.6f, 0.7f, 1.65f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Painted-Warrior-Mantlet",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.07f, 0.34f, 0.68f),
                new Vector3(0.62f, 0.34f, 0.24f),
                color * 0.58f);
            AddVerticalCylinder(
                "Painted-Warrior-CommanderCupola",
                turret,
                0.2f,
                0.07f,
                -0.45f,
                0.7f,
                -0.3f,
                color * 0.65f);
            TankDetailGeometry.Part(
                "Warrior-GunnerSight",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.3f, 0.65f, 0.45f),
                new Vector3(0.22f, 0.18f, 0.2f),
                color * 0.57f);
            TankDetailGeometry.Part(
                "Warrior-GunnerSightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.3f, 0.67f, 0.565f),
                new Vector3(0.14f, 0.07f, 0.02f),
                Lens());
            TankDetailGeometry.Part(
                "Warrior-BustleRack",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.2f, -1.25f),
                new Vector3(1.18f, 0.32f, 0.42f),
                Gunmetal());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < 3;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Warrior-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * (0.58f + tube * 0.06f),
                                0.42f + tube * 0.03f,
                                0.2f - tube * 0.08f),
                            new Vector3(0.04f, 0.1f, 0.04f),
                            color * 0.47f);
                    launcher.localRotation =
                        Quaternion.Euler(64f, 0f, side * 15f);
                }
            }
        }

        private static void AddMilanKit(
            Transform root,
            Transform turret,
            Color color)
        {
            float[] xs = { -0.62f, 0f, 0.62f, -0.58f, 0.1f, 0.78f };
            for (int index = 0;
                index < xs.Length;
                index++)
            {
                Transform tile =
                    TankDetailGeometry.Part(
                        "Painted-WarriorMilan-GlacisTile",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            xs[index],
                            index < 3 ? 1.235f : 1.43f,
                            index < 3 ? 2.71f : 2.35f),
                        new Vector3(
                            index == 5 ? 0.56f : 0.58f,
                            0.085f,
                            index < 3 ? 0.42f : 0.48f),
                        color * 0.69f);
                tile.localRotation =
                    Quaternion.Euler(
                        index < 3 ? -32f : -26f,
                        0f,
                        0f);
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 6;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-WarriorMilan-SideArmor",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.55f,
                            1.47f,
                            1.95f - panel * 0.77f),
                        new Vector3(0.05f, 0.32f, 0.61f),
                        color * 0.62f);
                }
            }
            TankDetailGeometry.Part(
                "Painted-WarriorMilan-Cradle",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.56f, 0.63f, -0.08f),
                new Vector3(0.36f, 0.16f, 0.48f),
                color * 0.57f);
            AddAxialCylinder(
                "WarriorMilan-LauncherTube",
                turret,
                0.112f,
                1.12f,
                0.36f,
                Dark(),
                0.56f,
                0.92f);
            for (int spare = 0;
                spare < 2;
                spare++)
            {
                AddAxialCylinder(
                    "WarriorMilan-SpareTube",
                    turret,
                    0.074f,
                    0.72f,
                    -1.2f,
                    Dark(),
                    -0.33f + spare * 0.35f,
                    0.55f + spare * 0.04f);
            }
            TankDetailGeometry.Part(
                "WarriorMilan-SightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.37f, 0.89f, 0.01f),
                new Vector3(0.14f, 0.1f, 0.018f),
                Lens());
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
