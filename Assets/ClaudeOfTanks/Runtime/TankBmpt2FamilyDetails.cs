using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmpt2FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "bmpt_terminator2";
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
            HideRenderer(turret.Find("Turret"));
            HideRenderer(turret.Find("Gun"));
            AddDonorRearGear(root, color);
            AddHullProtection(root, color);
            Transform station = AddStation(turret, color);
            AddMissileRacks(station, color);
            AddRoofEquipment(station, color);
            TankBmpt2GunDetails.Build(
                turret,
                color);
        }

        private static void AddDonorRearGear(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform drum =
                    TankDetailGeometry.Part(
                        "Painted-Soviet-FuelDrum",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(side * 0.66f, 1.44f, -3.1f),
                        new Vector3(0.14f, 0.26f, 0.14f),
                        color * 0.67f);
                drum.localRotation =
                    Quaternion.Euler(90f, 0f, 90f);
            }
            Transform log =
                TankDetailGeometry.Part(
                    "Soviet-UnditchingLog",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(0f, 1.26f, -2.87f),
                    new Vector3(0.095f, 1f, 0.095f),
                    new Color(0.18f, 0.12f, 0.07f));
            log.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
        }

        private static void AddHullProtection(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Bmpt2-FenderNotchBridge",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(side * 1.655f, 1.02f, 2.04f),
                    new Vector3(0.11f, 0.105f, 0.045f),
                    new Color(0.12f, 0.15f, 0.12f));
                for (int panel = 0;
                    panel < 7;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Bmpt2-SidePanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.73f,
                            1.04f,
                            2.15f -
                                panel * 0.76f),
                        new Vector3(0.08f, 0.44f, 0.62f),
                        color * 0.64f);
                }
                for (int tile = 0;
                    tile < 4;
                    tile++)
                {
                    AddGlacisTile(
                        root,
                        color,
                        side *
                            (0.27f +
                             tile * 0.31f),
                        1.25f,
                        2.08f);
                }
                for (int tile = 0;
                    tile < 3;
                    tile++)
                {
                    AddGlacisTile(
                        root,
                        color,
                        side *
                            (0.425f +
                             tile * 0.31f),
                        1.33f,
                        1.81f);
                }
            }
        }

        private static void AddGlacisTile(
            Transform root,
            Color color,
            float x,
            float y,
            float z)
        {
            Transform tile =
                TankDetailGeometry.Part(
                    "Painted-Bmpt2-GlacisTile",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(x, y, z),
                    new Vector3(0.28f, 0.105f, 0.34f),
                    color * 0.7f);
            tile.localRotation =
                Quaternion.Euler(-16.6f, 0f, 0f);
        }

        private static Transform AddStation(
            Transform turret,
            Color color)
        {
            Transform station =
                new GameObject("Bmpt2-Station").transform;
            station.SetParent(turret, false);
            station.localPosition =
                new Vector3(0f, 0f, -0.32f);
            TankDetailGeometry.Part(
                "Painted-Bmpt2-Turntable",
                PrimitiveType.Cylinder,
                station,
                new Vector3(0f, -0.025f, 0f),
                new Vector3(1.04f, 0.04f, 1.1f),
                color * 0.55f);
            TankDetailGeometry.Part(
                "Painted-Bmpt2-LowBase",
                PrimitiveType.Cylinder,
                station,
                new Vector3(0f, 0.09f, 0f),
                new Vector3(0.98f, 0.11f, 1.16f),
                color * 0.67f);
            TankDetailGeometry.Part(
                "Painted-Bmpt2-SlopedCasemate",
                PrimitiveType.Cube,
                station,
                new Vector3(0f, 0.34f, -0.03f),
                new Vector3(1.48f, 0.48f, 1.68f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Painted-Bmpt2-WeaponTower",
                PrimitiveType.Cube,
                station,
                new Vector3(0f, 0.56f, 0.05f),
                new Vector3(0.72f, 0.44f, 1.2f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Bmpt2-MantletSlot",
                PrimitiveType.Cube,
                station,
                new Vector3(0f, 0.6f, 0.76f),
                new Vector3(0.56f, 0.2f, 0.28f),
                Dark());
            return station;
        }

        private static void AddMissileRacks(
            Transform station,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Bmpt2-AtakaMountBlock",
                    PrimitiveType.Cube,
                    station,
                    new Vector3(side * 0.78f, 0.46f, 0.15f),
                    new Vector3(0.46f, 0.12f, 0.42f),
                    Dark());
                TankDetailGeometry.Part(
                    "Painted-Bmpt2-AtakaRackArm",
                    PrimitiveType.Cube,
                    station,
                    new Vector3(side * 0.66f, 0.545f, 0.18f),
                    new Vector3(0.3f, 0.07f, 0.55f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "Painted-Bmpt2-AtakaHanger",
                    PrimitiveType.Cube,
                    station,
                    new Vector3(side * 0.84f, 0.46f, 0.16f),
                    new Vector3(0.06f, 0.34f, 0.44f),
                    color * 0.59f);
                for (int row = 0;
                    row < 2;
                    row++)
                {
                    float y = 0.335f + row * 0.24f;
                    AddAxialCylinder(
                        "Bmpt2-AtakaTube",
                        station,
                        0.085f,
                        0.8f,
                        0.24f,
                        Dark(),
                        side * 0.93f,
                        y);
                    AddAxialCylinder(
                        "Painted-Bmpt2-AtakaCap",
                        station,
                        0.092f,
                        0.035f,
                        0.645f,
                        color * 0.66f,
                        side * 0.93f,
                        y);
                    AddAxialCylinder(
                        "Bmpt2-AtakaMouth",
                        station,
                        0.062f,
                        0.022f,
                        0.668f,
                        Color.black,
                        side * 0.93f,
                        y);
                }
            }
        }

        private static void AddRoofEquipment(
            Transform station,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bmpt2-PanoramaBase",
                PrimitiveType.Cube,
                station,
                new Vector3(0.34f, 0.8f, -0.18f),
                new Vector3(0.26f, 0.08f, 0.28f),
                color * 0.61f);
            TankDetailGeometry.Part(
                "Painted-Bmpt2-PanoramaPost",
                PrimitiveType.Cube,
                station,
                new Vector3(0.34f, 0.97f, -0.18f),
                new Vector3(0.11f, 0.26f, 0.11f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Bmpt2-PanoramaHead",
                PrimitiveType.Cube,
                station,
                new Vector3(0.34f, 1.135f, -0.17f),
                new Vector3(0.26f, 0.17f, 0.24f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Bmpt2-PanoramaLens",
                PrimitiveType.Cube,
                station,
                new Vector3(0.34f, 1.145f, -0.045f),
                new Vector3(0.17f, 0.1f, 0.024f),
                Lens());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmpt2-AmmoFeed",
                    PrimitiveType.Cube,
                    station,
                    new Vector3(side * 0.2f, 0.82f, 0.38f),
                    new Vector3(0.16f, 0.1f, 0.34f),
                    color * 0.61f);
                for (int tube = 0;
                    tube < 4;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Bmpt2-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            station,
                            new Vector3(
                                side *
                                    (0.66f +
                                     tube * 0.055f),
                                0.55f +
                                    tube * 0.025f,
                                -0.38f -
                                    tube * 0.05f),
                            new Vector3(0.04f, 0.11f, 0.04f),
                            color * 0.48f);
                    launcher.localRotation =
                        Quaternion.Euler(62f, 0f, side * 18f);
                }
                TankDetailGeometry.Part(
                    "Painted-Bmpt2-AntennaShelf",
                    PrimitiveType.Cube,
                    station,
                    new Vector3(side * 0.4f, 0.755f, -0.58f),
                    new Vector3(0.16f, 0.05f, 0.16f),
                    color * 0.58f);
                TankDetailGeometry.Part(
                    "Bmpt2-RadioAntenna",
                    PrimitiveType.Cylinder,
                    station,
                    new Vector3(side * 0.43f, 1.17f, -0.6f),
                    new Vector3(0.011f, 0.39f, 0.011f),
                    Dark());
            }
            TankDetailGeometry.Part(
                "Bmpt2-CableTrunk",
                PrimitiveType.Cube,
                station,
                new Vector3(0f, 0.795f, -0.1f),
                new Vector3(0.08f, 0.045f, 0.8f),
                Dark());
            TankDetailGeometry.Part(
                "Bmpt2-RoofMgReceiver",
                PrimitiveType.Cube,
                station,
                new Vector3(-0.3f, 0.9f, -0.31f),
                new Vector3(0.13f, 0.12f, 0.32f),
                Gunmetal());
            AddAxialCylinder(
                "Bmpt2-RoofMgBarrel",
                station,
                0.018f,
                0.72f,
                0.08f,
                Dark(),
                -0.3f,
                0.9f);
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

        internal static Color Dark()
        {
            return new Color(0.045f, 0.055f, 0.045f);
        }

        internal static Color Gunmetal()
        {
            return new Color(0.1f, 0.11f, 0.09f);
        }

        internal static Color Lens()
        {
            return new Color(0.02f, 0.075f, 0.085f);
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
    }
}
