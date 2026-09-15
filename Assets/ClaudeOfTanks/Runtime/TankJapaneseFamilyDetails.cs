using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankJapaneseFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "stb1" ||
                id == "type74" ||
                id == "type90" ||
                id == "type90a" ||
                id == "type10" ||
                id == "type10b";
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

            TankJapaneseHullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideRenderer(turret.Find("Turret"));
            HideRenderer(turret.Find("Gun"));
            AddHatches(
                turret,
                definition,
                color,
                width);
            AddSmokeBanks(
                turret,
                definition,
                color,
                width);
            AddAntennas(
                turret,
                definition,
                color,
                width);
            TankJapaneseBasketDetails.Build(
                turret,
                definition,
                color,
                width);
            TankJapaneseVariantDetails.Build(
                turret,
                definition,
                color,
                width);
            TankJapaneseProtectionDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer =
                part == null
                    ? null
                    : part.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }

        internal static void AddMachineGun(
            Transform parent,
            float roof,
            Color color,
            Vector3 seat,
            string prefix,
            bool heavy,
            bool shield)
        {
            float receiverWidth = heavy ? 0.2f : 0.15f;
            float receiverLength = heavy ? 0.42f : 0.32f;
            float barrelLength = heavy ? 0.86f : 0.66f;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Mount",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.12f, 0.045f, 0.12f),
                color * 0.7f);
            TankDetailGeometry.Part(
                prefix + "-Receiver",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.13f,
                    seat.z + 0.08f),
                new Vector3(
                    receiverWidth,
                    heavy ? 0.14f : 0.1f,
                    receiverLength),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-AmmoBox",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x - receiverWidth * 0.76f,
                    roof + seat.y + 0.11f,
                    seat.z),
                new Vector3(
                    receiverWidth * 0.72f,
                    0.13f,
                    receiverLength * 0.55f),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.14f,
                    seat.z +
                        receiverLength * 0.5f +
                        barrelLength * 0.5f),
                new Vector3(
                    heavy ? 0.034f : 0.025f,
                    heavy ? 0.034f : 0.025f,
                    barrelLength),
                Gunmetal());
            if (!shield) return;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Shield",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.19f,
                    seat.z + 0.14f),
                new Vector3(
                    receiverWidth * 1.45f,
                    0.25f,
                    0.055f),
                color * 0.62f);
        }

        internal static void AddSight(
            Transform turret,
            Color color,
            Vector3 center,
            Vector3 size,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Housing",
                PrimitiveType.Cube,
                turret,
                center,
                size,
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    center.x,
                    center.y,
                    center.z + size.z * 0.52f),
                new Vector3(
                    size.x * 0.62f,
                    size.y * 0.45f,
                    0.014f),
                Lens());
        }

        private static void AddHatches(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            bool cast =
                definition.id == "stb1" ||
                definition.id == "type74";
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float x = side * width *
                    (cast ? 0.14f : 0.15f);
                float z = cast
                    ? (side < 0 ? -0.1f : -0.45f)
                    : -0.55f;
                TankDetailGeometry.Part(
                    "Painted-Japanese-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.035f,
                        z),
                    new Vector3(
                        cast ? 0.21f : 0.18f,
                        0.035f,
                        cast ? 0.21f : 0.18f),
                    color * 0.82f);
                TankDetailGeometry.Part(
                    "Japanese-HatchHandle",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.078f,
                        z),
                    new Vector3(0.18f, 0.02f, 0.04f),
                    Gunmetal());
            }
        }

        private static void AddSmokeBanks(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            int total = SmokeLauncherCount(
                definition.id);
            int perSide = total / 2;
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float front =
                TankDetailGeometry.TurretFrontZ(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.36f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Japanese-SmokeBankShoe",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * halfWidth * 0.82f,
                        roof - 0.24f,
                        front * 0.34f),
                    new Vector3(
                        0.22f,
                        0.06f,
                        Mathf.Max(0.3f, perSide * 0.08f)),
                    color * 0.68f);
                for (int tube = 0;
                    tube < perSide;
                    tube++)
                {
                    int row = tube / 3;
                    int column = tube % 3;
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Japanese-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * halfWidth *
                                    (0.76f +
                                     column * 0.03f),
                                roof - 0.19f +
                                    row * 0.09f,
                                front * 0.35f +
                                    column * 0.08f),
                            new Vector3(0.045f, 0.14f, 0.045f),
                            color * 0.54f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            64f,
                            0f,
                            side * (35f +
                                column * 4f));
                }
            }
        }

        private static int SmokeLauncherCount(string id)
        {
            if (id == "stb1" ||
                id == "type90a")
                return 10;
            if (id == "type74")
                return 6;
            if (id == "type10b")
                return 12;
            return 8;
        }

        private static void AddAntennas(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            bool type10 =
                definition.id == "type10" ||
                definition.id == "type10b";
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float x = side * width *
                    (type10 ? 0.36f : 0.3f);
                TankDetailGeometry.Part(
                    "Japanese-AntennaBase",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.04f,
                        rear * 0.78f),
                    new Vector3(0.05f, 0.08f, 0.05f),
                    color * 0.42f);
                Transform antenna =
                    TankDetailGeometry.Part(
                        "Japanese-Antenna",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            x,
                            roof +
                                (side < 0 ? 0.56f : 0.48f),
                            rear * 0.78f),
                        new Vector3(
                            0.012f,
                            side < 0 ? 1.02f : 0.86f,
                            0.012f),
                        Gunmetal());
                antenna.localRotation =
                    Quaternion.Euler(
                        type10 && side > 0 ? 68f : 0f,
                        0f,
                        side * -3f);
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }

        private static Color Lens()
        {
            return new Color(0.025f, 0.14f, 0.17f);
        }
    }
}
