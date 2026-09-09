using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT64BV1GunDetails
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
                    "T64-GunFittings");
            AddSaddle(fittings, color);
            AddTube(
                fittings,
                definition,
                color);
        }

        private static void AddSaddle(
            Transform fittings,
            Color color)
        {
            AddZCylinder(
                "Painted-T64-2A46Saddle",
                fittings,
                new Vector3(0f, 0f, 0.18f),
                0.18f,
                0.36f,
                color * 0.56f,
                new Vector3(1.35f, 0.9f, 1f));
            AddZCylinder(
                "T64-2A46Boot",
                fittings,
                new Vector3(0f, 0f, 0.39f),
                0.15f,
                0.16f,
                TankT64BV1FamilyDetails.Rubber(),
                new Vector3(1.12f, 0.88f, 1f));
            for (int rib = 0;
                rib < 4;
                rib++)
            {
                AddZCylinder(
                    "T64-2A46BootRib",
                    fittings,
                    new Vector3(
                        0f,
                        0f,
                        0.31f + rib * 0.07f),
                    0.16f - rib * 0.012f,
                    0.025f,
                    TankT64BV1FamilyDetails.Dark());
            }
            AddZCylinder(
                "T64-PktCoax",
                fittings,
                new Vector3(0.22f, 0.01f, 0.29f),
                0.018f,
                0.14f,
                TankT64BV1FamilyDetails.Dark());
        }

        private static void AddTube(
            Transform fittings,
            VehicleDefinition definition,
            Color color)
        {
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    5.8831f);
            AddZCylinder(
                "Painted-T64-2A46Root",
                fittings,
                new Vector3(0f, 0f, 0.96f),
                0.097f,
                1.12f,
                color * 0.49f);
            AddZCylinder(
                "Painted-T64-2A46Sleeve",
                fittings,
                new Vector3(0f, 0f, 1.985f),
                0.1f,
                0.93f,
                color * 0.52f);
            AddZCylinder(
                "Painted-T64-2A46Evacuator",
                fittings,
                new Vector3(0f, 0f, 2.86f),
                0.135f,
                0.82f,
                color * 0.46f);
            AddZCylinder(
                "Painted-T64-2A46ForwardSleeve",
                fittings,
                new Vector3(0f, 0f, 3.72f),
                0.088f,
                0.9f,
                color * 0.5f);
            AddZCylinder(
                "Painted-T64-2A46MuzzleTube",
                fittings,
                new Vector3(
                    0f,
                    0f,
                    4.17f +
                        (length - 4.17f) *
                        0.5f),
                0.084f,
                length - 4.17f,
                color * 0.47f);

            float[] rings =
            {
                0.72f, 1.56f, 2.42f,
                3.31f, 3.97f, length - 0.14f
            };
            for (int ring = 0;
                ring < rings.Length;
                ring++)
            {
                AddZCylinder(
                    "T64-2A46SleeveRing",
                    fittings,
                    new Vector3(0f, 0f, rings[ring]),
                    ring == 2
                        ? 0.14f
                        : 0.105f,
                    0.035f,
                    TankT64BV1FamilyDetails.Dark());
            }
            AddZCylinder(
                "Painted-T64-2A46MuzzleRing",
                fittings,
                new Vector3(0f, 0f, length - 0.06f),
                0.095f,
                0.1f,
                color * 0.43f);
            AddZCylinder(
                "T64-2A46MuzzleBore",
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
