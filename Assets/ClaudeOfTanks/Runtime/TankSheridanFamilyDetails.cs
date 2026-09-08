using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSheridanFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "m551_sheridan" ||
                id == "m551a1_tts";
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

            TankSheridanHullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            TankSheridanTurretDetails.Build(
                turret,
                definition,
                color,
                width);
            TankSheridanProtectionDetails.Build(
                turret,
                definition,
                color,
                width);
        }

        internal static void AddMachineGun(
            Transform parent,
            float roof,
            Color color,
            Vector3 seat,
            string prefix,
            bool heavy)
        {
            float receiverWidth =
                heavy ? 0.2f : 0.15f;
            float receiverLength =
                heavy ? 0.42f : 0.32f;
            float barrelLength =
                heavy ? 0.82f : 0.62f;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Mount",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(
                    heavy ? 0.13f : 0.1f,
                    0.045f,
                    heavy ? 0.13f : 0.1f),
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-Receiver",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.13f,
                    seat.z + 0.1f),
                new Vector3(
                    receiverWidth,
                    heavy ? 0.15f : 0.1f,
                    receiverLength),
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
            if (!heavy) return;
            TankDetailGeometry.Part(
                prefix + "-AmmoBox",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x - 0.17f,
                    roof + seat.y + 0.1f,
                    seat.z + 0.01f),
                new Vector3(0.15f, 0.14f, 0.24f),
                Gunmetal());
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
                    size.y * 0.48f,
                    0.014f),
                Lens());
        }

        internal static Color Gunmetal()
        {
            return new Color(0.11f, 0.12f, 0.1f);
        }

        internal static Color Lens()
        {
            return new Color(0.035f, 0.075f, 0.085f);
        }
    }
}
