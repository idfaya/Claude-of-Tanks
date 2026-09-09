using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT72B3MGunDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;

            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "T72B3M-GunFittings");
            AddRoot(fittings, color);
            AddTube(fittings, definition, color);
        }

        private static void AddRoot(
            Transform fittings,
            Color color)
        {
            AddZCylinder(
                "Painted-T72B3M-2A46M5Saddle",
                fittings,
                new Vector3(0f, 0f, 0.2f),
                0.19f,
                0.42f,
                color * 0.54f,
                new Vector3(1.45f, 0.92f, 1f));
            AddZCylinder(
                "T72B3M-ThermalSleeveBoot",
                fittings,
                new Vector3(0f, 0f, 0.48f),
                0.15f,
                0.2f,
                TankT72B3MFamilyDetails.Rubber(),
                new Vector3(1.18f, 0.9f, 1f));
            AddZCylinder(
                "T72B3M-Coax",
                fittings,
                new Vector3(0.22f, 0.01f, 0.32f),
                0.018f,
                0.16f,
                TankT72B3MFamilyDetails.Dark());
        }

        private static void AddTube(
            Transform fittings,
            VehicleDefinition definition,
            Color color)
        {
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    6.0f);
            AddZCylinder(
                "Painted-T72B3M-2A46M5Root",
                fittings,
                new Vector3(0f, 0f, 0.98f),
                0.094f,
                1.08f,
                color * 0.47f);
            AddZCylinder(
                "Painted-T72B3M-2A46M5ThermalSleeve",
                fittings,
                new Vector3(0f, 0f, 2.08f),
                0.102f,
                1.05f,
                color * 0.51f);
            AddZCylinder(
                "Painted-T72B3M-2A46M5Evacuator",
                fittings,
                new Vector3(0f, 0f, 2.92f),
                0.13f,
                0.72f,
                color * 0.44f);
            AddZCylinder(
                "Painted-T72B3M-2A46M5ForwardTube",
                fittings,
                new Vector3(
                    0f,
                    0f,
                    3.64f +
                        (length - 3.64f) *
                        0.5f),
                0.084f,
                length - 3.64f,
                color * 0.47f);

            float[] rings =
            {
                0.74f, 1.55f, 2.42f,
                3.24f, 4.24f, length - 0.14f
            };
            for (int ring = 0;
                ring < rings.Length;
                ring++)
            {
                AddZCylinder(
                    "T72B3M-2A46M5SleeveRing",
                    fittings,
                    new Vector3(0f, 0f, rings[ring]),
                    ring == 2
                        ? 0.14f
                        : 0.104f,
                    0.032f,
                    TankT72B3MFamilyDetails.Dark());
            }
            AddZCylinder(
                "Painted-T72B3M-MuzzleCollar",
                fittings,
                new Vector3(0f, 0f, length - 0.06f),
                0.095f,
                0.1f,
                color * 0.43f);
            AddZCylinder(
                "T72B3M-MuzzleBore",
                fittings,
                new Vector3(0f, 0f, length + 0.004f),
                0.068f,
                0.018f,
                Color.black);
        }

        private static void AddZCylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float length,
            Color color)
        {
            AddZCylinder(
                name,
                parent,
                position,
                radius,
                length,
                color,
                Vector3.one);
        }

        private static void AddZCylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float length,
            Color color,
            Vector3 scale)
        {
            Transform part = TankDetailGeometry.Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                position,
                new Vector3(
                    radius * scale.x,
                    length * 0.5f * scale.z,
                    radius * scale.y),
                color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
