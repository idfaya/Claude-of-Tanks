using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPattonFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "m46_patton" ||
                id == "m47_patton" ||
                id == "m48" ||
                id == "m60a1" ||
                id == "m60a2" ||
                id == "m60a3";
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

            TankPattonHullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            TankPattonTurretDetails.Build(
                turret,
                definition,
                color,
                width);
            TankPattonProtectionDetails.Build(
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
            bool shield)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Mount",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.13f, 0.05f, 0.13f),
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-Receiver",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.14f,
                    seat.z + 0.1f),
                new Vector3(0.21f, 0.15f, 0.43f),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-AmmoBox",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x - 0.17f,
                    roof + seat.y + 0.11f,
                    seat.z + 0.02f),
                new Vector3(0.14f, 0.14f, 0.24f),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.15f,
                    seat.z + 0.73f),
                new Vector3(0.034f, 0.034f, 0.9f),
                Gunmetal());
            if (!shield) return;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Shield",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.2f,
                    seat.z + 0.18f),
                new Vector3(0.36f, 0.28f, 0.055f),
                color * 0.64f);
        }

        internal static void AddAntenna(
            Transform turret,
            float roof,
            Vector3 seat,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-AntennaBase",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.07f, 0.06f, 0.07f),
                new Color(0.18f, 0.19f, 0.16f));
            Transform whip =
                TankDetailGeometry.Part(
                    prefix + "-Antenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        seat.x,
                        roof + seat.y + 0.43f,
                        seat.z),
                    new Vector3(0.009f, 0.8f, 0.009f),
                    Gunmetal());
            whip.localRotation =
                Quaternion.Euler(0f, 0f, seat.x < 0f ? 4f : -4f);
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
