using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankItalianFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "carro45t" ||
                id == "ariete" ||
                id == "ariete_c1" ||
                id == "ariete_c2";
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

            TankItalianHullDetails.Build(
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
            TankItalianBasketDetails.Build(
                turret,
                definition,
                color,
                width);
            TankItalianVariantDetails.Build(
                turret,
                definition,
                color,
                width);
            TankItalianProtectionDetails.Build(
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
            bool carro =
                definition.id == "carro45t";
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float x = carro
                    ? (side < 0 ? -0.55f : -0.01f)
                    : side * width * 0.14f;
                float z = carro
                    ? (side < 0 ? 0.1f : -0.78f)
                    : (side < 0 ? -0.62f : -0.42f);
                TankDetailGeometry.Part(
                    "Painted-Italian-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.035f,
                        z),
                    new Vector3(0.2f, 0.035f, 0.2f),
                    color * 0.82f);
                TankDetailGeometry.Part(
                    "Italian-HatchHandle",
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
            int total =
                definition.id == "carro45t"
                    ? 10
                    : 8;
            int perSide = total / 2;
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.35f);
            float bankZ =
                definition.id == "carro45t"
                    ? -0.92f
                    : -0.12f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Italian-SmokeBankShoe",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * halfWidth * 0.84f,
                        roof - 0.25f,
                        bankZ),
                    new Vector3(
                        0.22f,
                        0.06f,
                        Mathf.Max(0.3f, perSide * 0.08f)),
                    color * 0.66f);
                for (int tube = 0;
                    tube < perSide;
                    tube++)
                {
                    int row = tube / 4;
                    int column = tube % 4;
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Italian-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * halfWidth *
                                    (0.78f +
                                     column * 0.028f),
                                roof - 0.2f +
                                    row * 0.09f,
                                bankZ +
                                    column * 0.08f),
                            new Vector3(0.045f, 0.15f, 0.045f),
                            color * 0.52f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            63f,
                            0f,
                            side * (39f +
                                column * 3f));
                }
            }
        }

        private static void AddAntennas(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            int count =
                definition.id == "ariete_c1"
                    ? 1
                    : 2;
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            for (int index = 0;
                index < count;
                index++)
            {
                float t = count == 1
                    ? 1f
                    : index;
                float x =
                    Mathf.Lerp(
                        -width * 0.24f,
                        width * 0.24f,
                        t);
                TankDetailGeometry.Part(
                    "Italian-AntennaBase",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.04f,
                        rear * 0.78f),
                    new Vector3(0.05f, 0.08f, 0.05f),
                    color * 0.42f);
                TankDetailGeometry.Part(
                    "Italian-Antenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.55f -
                            index * 0.06f,
                        rear * 0.78f),
                    new Vector3(
                        0.012f,
                        1.02f -
                            index * 0.12f,
                        0.012f),
                    Gunmetal());
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
