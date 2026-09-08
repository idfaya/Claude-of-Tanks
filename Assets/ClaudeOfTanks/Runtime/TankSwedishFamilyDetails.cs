using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSwedishFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "strv81" ||
                id == "udes03" ||
                id == "strv103a" ||
                id == "strv103" ||
                id == "cv90" ||
                id == "strv122" ||
                id == "cv90_mkiv";
        }

        public static bool OwnsIfvWeaponPackage(
            string id)
        {
            return id == "cv90" ||
                id == "cv90_mkiv";
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

            TankSwedishHullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            if (IsCasemate(definition.id))
            {
                TankSwedishCasemateDetails.Build(
                    root,
                    definition,
                    color,
                    width,
                    height,
                    length);
            }
            else
            {
                TankSwedishTurretDetails.Build(
                    root,
                    turret,
                    definition,
                    color,
                    width,
                    height,
                    length);
            }
            TankSwedishWeaponDetails.Build(
                root,
                turret,
                definition,
                color,
                width);
        }

        internal static bool IsCasemate(
            string id)
        {
            return id == "udes03" ||
                id == "strv103a" ||
                id == "strv103";
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
            Transform parent,
            Color color,
            Vector3 center,
            Vector3 size,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Housing",
                PrimitiveType.Cube,
                parent,
                center,
                size,
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-Lens",
                PrimitiveType.Cube,
                parent,
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

        internal static void AddSmokeBanks(
            Transform parent,
            Color color,
            float roof,
            float halfWidth,
            float z,
            int total,
            string prefix)
        {
            int perSide = total / 2;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-SmokeBankShoe",
                    PrimitiveType.Cube,
                    parent,
                    new Vector3(
                        side * halfWidth * 0.84f,
                        roof - 0.25f,
                        z),
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
                            "Painted-" + prefix +
                                "-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            parent,
                            new Vector3(
                                side * halfWidth *
                                    (0.78f +
                                     column * 0.028f),
                                roof - 0.2f +
                                    row * 0.09f,
                                z + column * 0.08f),
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

        internal static void AddAntennas(
            Transform parent,
            Color color,
            float roof,
            float rear,
            float width,
            int count,
            string prefix)
        {
            for (int index = 0;
                index < count;
                index++)
            {
                float t = count == 1
                    ? 0f
                    : index / (float)(count - 1);
                float x =
                    Mathf.Lerp(
                        -width * 0.26f,
                        width * 0.26f,
                        t);
                TankDetailGeometry.Part(
                    prefix + "-AntennaBase",
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(
                        x,
                        roof + 0.04f,
                        rear),
                    new Vector3(0.05f, 0.08f, 0.05f),
                    color * 0.42f);
                Transform antenna =
                    TankDetailGeometry.Part(
                        prefix + "-Antenna",
                        PrimitiveType.Cylinder,
                        parent,
                        new Vector3(
                            x,
                            roof + 0.55f -
                                index * 0.06f,
                            rear),
                        new Vector3(
                            0.012f,
                            1.02f -
                                index * 0.12f,
                            0.012f),
                        Gunmetal());
                antenna.localRotation =
                    Quaternion.Euler(
                        index == 0 ? -8f : 8f,
                        0f,
                        index == 0 ? 4f : -4f);
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
