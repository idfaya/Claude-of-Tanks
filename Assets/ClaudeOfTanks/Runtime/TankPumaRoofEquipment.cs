using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPumaRoofEquipment
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            if (TankPumaFamilyDetails.IsS1(
                    definition))
            {
                AddS1PanoramicStation(
                    turret,
                    color);
                AddS1RemoteWeaponStation(
                    turret,
                    color);
                AddS1Mells(
                    turret,
                    color);
                return;
            }

            AddProductionPeriMast(
                turret,
                color);
            AddProductionWaoSight(
                turret,
                color);
        }

        private static void AddProductionPeriMast(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Puma-PeriMastBase",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.14f, 0.9f, 0.13f),
                new Vector3(0.24f, 0.42f, 0.3f),
                color * 0.65f);
            TankDetailGeometry.Part(
                "Painted-Puma-PeriMastStep",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.14f, 1.27f, 0.14f),
                new Vector3(0.18f, 0.34f, 0.22f),
                color * 0.58f);
            TankDetailGeometry.Part(
                "Puma-PeriHead",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.14f, 1.49f, 0.14f),
                new Vector3(0.3f, 0.145f, 0.34f),
                TankPumaFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "Puma-PeriLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.14f, 1.49f, 0.318f),
                new Vector3(0.22f, 0.052f, 0.014f),
                TankPumaFamilyDetails.Lens());
        }

        private static void AddProductionWaoSight(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Puma-WaoHood",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.32f, 0.68f, 0.42f),
                new Vector3(0.3f, 0.22f, 0.26f),
                color * 0.64f);
            TankDetailGeometry.Part(
                "Puma-WaoLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.32f, 0.715f, 0.562f),
                new Vector3(0.2f, 0.05f, 0.014f),
                TankPumaFamilyDetails.Lens());
        }

        private static void AddS1PanoramicStation(
            Transform turret,
            Color color)
        {
            const float scale = 0.9f;
            Transform root =
                CreateRoot(
                    turret,
                    "PumaS1-PanoramicStation",
                    new Vector3(
                        0.62f * scale,
                        0.88f * scale,
                        0.52f * scale));
            TankDetailGeometry.Part(
                "Painted-PumaS1-PanoramicPedestal",
                PrimitiveType.Cube,
                root,
                Vector3.zero,
                new Vector3(
                    0.52f * scale,
                    0.2f * scale,
                    0.54f * scale),
                color * 0.62f);
            TankDetailGeometry.Part(
                "PumaS1-PanoramicTurntable",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    0f,
                    0.15f * scale,
                    0f),
                new Vector3(
                    0.25f * scale,
                    0.04f * scale,
                    0.25f * scale),
                TankPumaFamilyDetails.Gunmetal());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform yoke =
                    TankDetailGeometry.Part(
                        "Painted-PumaS1-PanoramicYoke",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 0.17f * scale,
                            0.34f * scale,
                            0f),
                        new Vector3(
                            0.07f * scale,
                            0.35f * scale,
                            0.08f * scale),
                        color * 0.58f);
                yoke.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * -7f);
            }
            TankDetailGeometry.Part(
                "PumaS1-PanoramicSensorHead",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    0.5f * scale,
                    0f),
                new Vector3(
                    0.34f * scale,
                    0.22f * scale,
                    0.26f * scale),
                TankPumaFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "PumaS1-PanoramicLens",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    0.5f * scale,
                    0.14f * scale),
                new Vector3(
                    0.17f * scale,
                    0.09f * scale,
                    0.014f * scale),
                TankPumaFamilyDetails.Lens());
        }

        private static void AddS1RemoteWeaponStation(
            Transform turret,
            Color color)
        {
            const float scale = 0.9f;
            Transform root =
                CreateRoot(
                    turret,
                    "PumaS1-CompactRws",
                    new Vector3(
                        0.42f * scale,
                        0.74f * scale,
                        -0.9f * scale));
            TankDetailGeometry.Part(
                "Painted-PumaS1-RwsPedestal",
                PrimitiveType.Cylinder,
                root,
                Vector3.zero,
                new Vector3(
                    0.18f * scale,
                    0.05f * scale,
                    0.18f * scale),
                color * 0.6f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform yoke =
                    TankDetailGeometry.Part(
                        "Painted-PumaS1-RwsYoke",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 0.13f * scale,
                            0.2f * scale,
                            0.02f * scale),
                        new Vector3(
                            0.055f * scale,
                            0.3f * scale,
                            0.07f * scale),
                        color * 0.56f);
                yoke.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * -9f);
            }
            TankDetailGeometry.Part(
                "PumaS1-RwsReceiver",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    0.33f * scale,
                    0.12f * scale),
                new Vector3(
                    0.24f * scale,
                    0.16f * scale,
                    0.36f * scale),
                TankPumaFamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "Painted-PumaS1-RwsAmmoBox",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0.2f * scale,
                    0.3f * scale,
                    0.08f * scale),
                new Vector3(
                    0.16f * scale,
                    0.2f * scale,
                    0.3f * scale),
                color * 0.47f);
            for (int link = 0;
                link < 5;
                link++)
            {
                Transform feed =
                    TankDetailGeometry.Part(
                        "PumaS1-RwsFeedLink",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            (0.11f +
                             link * 0.028f) *
                            scale,
                            (0.29f +
                             link * 0.012f) *
                            scale,
                            (0.12f +
                             link * 0.015f) *
                            scale),
                        new Vector3(
                            0.015f * scale,
                            0.015f * scale,
                            0.015f * scale),
                        new Color(
                            0.43f,
                            0.34f,
                            0.14f));
                feed.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f);
            }
            AddAxialCylinder(
                "PumaS1-RwsMachineGunBarrel",
                root,
                0.022f * scale,
                0.72f * scale,
                0.62f * scale,
                TankPumaFamilyDetails.Dark());
        }

        private static void AddS1Mells(
            Transform turret,
            Color color)
        {
            const float scale = 0.9f;
            const float x = -1.02f;
            TankDetailGeometry.Part(
                "Painted-PumaS1-MellsCarrier",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x * scale,
                    0.47f * scale,
                    0.08f * scale),
                new Vector3(
                    0.28f * scale,
                    0.48f * scale,
                    0.6f * scale),
                color * 0.61f);
            float[] cells =
            {
                0.36f,
                0.59f
            };
            for (int cell = 0;
                cell < cells.Length;
                cell++)
            {
                float y = cells[cell] * scale;
                TankDetailGeometry.Part(
                    "Painted-PumaS1-MellsSquareCell",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x * scale,
                        y,
                        0.4f * scale),
                    new Vector3(
                        0.3f * scale,
                        0.2f * scale,
                        0.78f * scale),
                    color * 0.66f);
                TankDetailGeometry.Part(
                    "PumaS1-MellsSquareMouth",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x * scale,
                        y,
                        0.802f * scale),
                    new Vector3(
                        0.205f * scale,
                        0.135f * scale,
                        0.024f * scale),
                    TankPumaFamilyDetails.Dark());
            }
        }

        private static Transform CreateRoot(
            Transform parent,
            string name,
            Vector3 position)
        {
            GameObject root =
                new GameObject(name);
            root.transform.SetParent(
                parent,
                false);
            root.transform.localPosition =
                position;
            return root.transform;
        }

        private static void AddAxialCylinder(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(0f, 0f, z),
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
