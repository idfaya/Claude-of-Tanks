using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankFrenchFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "amx30" ||
                id == "amx30b2" ||
                id == "amx40" ||
                id == "leclerc" ||
                id == "leclerc_xlr" ||
                id == "amx56";
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

            TankFrenchHullDetails.Build(
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
            TankFrenchBasketDetails.Build(
                turret,
                definition,
                color,
                width);
            TankFrenchVariantDetails.Build(
                turret,
                definition,
                color,
                width);
            TankFrenchProtectionDetails.Build(
                turret,
                definition,
                color,
                width);
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
            float receiverWidth =
                heavy ? 0.21f : 0.16f;
            float receiverLength =
                heavy ? 0.43f : 0.34f;
            float barrelLength =
                heavy ? 0.84f : 0.68f;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Mount",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.12f, 0.045f, 0.12f),
                color * 0.68f);
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
                    heavy ? 0.15f : 0.11f,
                    receiverLength),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-AmmoBox",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x - receiverWidth * 0.76f,
                    roof + seat.y + 0.1f,
                    seat.z),
                new Vector3(
                    receiverWidth * 0.72f,
                    0.13f,
                    receiverLength * 0.54f),
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
                    heavy ? 0.034f : 0.026f,
                    heavy ? 0.034f : 0.026f,
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
                    receiverWidth * 1.5f,
                    0.25f,
                    0.05f),
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
            bool amx30 =
                definition.id == "amx30" ||
                definition.id == "amx30b2";
            bool centered =
                definition.id == "leclerc_xlr" ||
                definition.id == "amx56";
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float x = centered
                    ? side * 0.67f
                    : side * width *
                        (amx30 ? 0.145f : 0.16f);
                float z = centered
                    ? -0.33f
                    : amx30
                        ? (side < 0 ? -0.58f : -0.38f)
                        : -0.58f;
                TankDetailGeometry.Part(
                    "Painted-French-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.035f,
                        z),
                    new Vector3(
                        amx30 ? 0.2f : 0.19f,
                        0.035f,
                        amx30 ? 0.2f : 0.19f),
                    color * 0.82f);
                TankDetailGeometry.Part(
                    "French-HatchHandle",
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
            int total = SmokeCount(definition.id);
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
                    width * 0.35f);
            bool leclerc =
                definition.id == "leclerc" ||
                definition.id == "leclerc_xlr" ||
                definition.id == "amx56";
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float bankZ = leclerc
                    ? -1.28f
                    : definition.id == "amx40"
                        ? -0.45f
                        : -1.12f;
                TankDetailGeometry.Part(
                    "Painted-French-SmokeBankShoe",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * halfWidth * 0.83f,
                        roof - (leclerc ? 0.31f : 0.24f),
                        bankZ),
                    new Vector3(
                        0.2f,
                        0.06f,
                        Mathf.Max(0.26f, perSide * 0.075f)),
                    color * 0.66f);
                for (int tube = 0;
                    tube < perSide;
                    tube++)
                {
                    int row = tube / 5;
                    int column = tube % 5;
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-French-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * halfWidth *
                                    (0.77f +
                                     column * 0.026f),
                                roof -
                                    (leclerc ? 0.25f : 0.19f) +
                                    row * 0.085f,
                                bankZ +
                                    column * 0.075f),
                            new Vector3(0.043f, 0.14f, 0.043f),
                            color * 0.52f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            63f,
                            0f,
                            side * (38f +
                                column * 3f));
                }
            }
        }

        private static int SmokeCount(string id)
        {
            if (id == "amx30") return 4;
            if (id == "amx30b2") return 6;
            if (id == "amx40") return 12;
            return 18;
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
            int count =
                definition.id == "amx30"
                    ? 1
                    : definition.id == "amx40"
                        ? 3
                        : 2;
            bool leclerc =
                definition.id == "leclerc" ||
                definition.id == "leclerc_xlr" ||
                definition.id == "amx56";
            for (int index = 0;
                index < count;
                index++)
            {
                float t = count == 1
                    ? 0f
                    : index / (float)(count - 1);
                float x =
                    Mathf.Lerp(
                        -width * 0.29f,
                        width * 0.29f,
                        t);
                TankDetailGeometry.Part(
                    "French-AntennaBase",
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
                        "French-Antenna",
                        PrimitiveType.Cylinder,
                        turret,
                        leclerc
                            ? new Vector3(
                                x,
                                roof + 0.07f,
                                rear * 0.68f)
                            : new Vector3(
                                x,
                                roof + 0.5f,
                                rear * 0.78f),
                        leclerc
                            ? new Vector3(
                                0.012f,
                                0.62f,
                                0.012f)
                            : new Vector3(
                                0.012f,
                                0.92f -
                                    index * 0.08f,
                                0.012f),
                        Gunmetal());
                antenna.localRotation =
                    leclerc
                        ? Quaternion.Euler(90f, 0f, 0f)
                        : Quaternion.Euler(
                            0f,
                            0f,
                            (index - 1) * 3f);
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
