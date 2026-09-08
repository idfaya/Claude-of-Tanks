using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMarder1A3GunDetails
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
                    "Marder-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    2.55f);
            float diameter =
                TankAuthoredDetails.ResolveGunRadius(
                    definition);

            AddCradle(
                fittings,
                color);
            AddMk20(
                fittings,
                color,
                length,
                diameter);
            AddCoaxMg3(
                fittings);
        }

        private static void AddCradle(
            Transform fittings,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Marder-Mk20Cradle",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.16f),
                new Vector3(0.3f, 0.24f, 0.55f),
                color * 0.64f);
            AddAxialCylinder(
                "Painted-Marder-Mk20Collar",
                fittings,
                0.075f,
                0.24f,
                0.5f,
                color * 0.56f);
        }

        private static void AddMk20(
            Transform fittings,
            Color color,
            float length,
            float diameter)
        {
            const float barrelStart = 0.58f;
            float barrelLength =
                Mathf.Max(
                    0.6f,
                    length -
                    barrelStart -
                    0.12f);
            AddAxialCylinder(
                "Painted-Marder-Mk20Barrel",
                fittings,
                diameter,
                barrelLength,
                barrelStart +
                    barrelLength * 0.5f,
                color * 0.47f);
            AddAxialCylinder(
                "Painted-Marder-Mk20MuzzleCollar",
                fittings,
                diameter * 1.35f,
                0.14f,
                length - 0.08f,
                color * 0.4f);
            AddAxialCylinder(
                "Marder-Mk20MuzzleBore",
                fittings,
                diameter * 0.46f,
                0.026f,
                length - 0.007f,
                Color.black);
        }

        private static void AddCoaxMg3(
            Transform fittings)
        {
            TankDetailGeometry.Part(
                "Marder-CoaxMg3Receiver",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.16f, -0.05f, 0.42f),
                new Vector3(0.1f, 0.09f, 0.26f),
                TankMarder1A3FamilyDetails.Gunmetal());
            AddAxialCylinder(
                "Marder-CoaxMg3Barrel",
                fittings,
                0.014f,
                0.55f,
                0.65f,
                TankMarder1A3FamilyDetails.Dark(),
                0.16f,
                -0.05f);
            AddAxialCylinder(
                "Marder-CoaxMg3Muzzle",
                fittings,
                0.01f,
                0.024f,
                0.93f,
                Color.black,
                0.16f,
                -0.05f);
        }

        private static void AddAxialCylinder(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color,
            float x = 0f,
            float y = 0f)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(x, y, z),
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
