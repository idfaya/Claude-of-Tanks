using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankM1A3GunDetails
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
                    "M1A3-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    5.65f);
            Part("Painted-M1A3-GunCradle", fittings,
                new Vector3(0f, 0f, 0.19f),
                new Vector3(0.82f, 0.58f, 0.5f),
                color * 0.55f);
            Part("Painted-M1A3-MantletCollar", fittings,
                new Vector3(0f, 0f, 0.5f),
                new Vector3(1.05f, 0.76f, 0.54f),
                color * 0.59f);
            AddSegment(fittings,
                "Painted-M1A3-ThermalShroud",
                0.145f,
                0.6f,
                1.46f,
                color * 0.49f);
            AddSegment(fittings,
                "Painted-M1A3-ThermalShroud",
                0.14f,
                1.52f,
                2.38f,
                color * 0.5f);
            AddSegment(fittings,
                "Painted-M1A3-ThermalShroud",
                0.136f,
                2.44f,
                3.18f,
                color * 0.51f);
            AddSegment(fittings,
                "Painted-M1A3-Barrel",
                0.115f,
                3.18f,
                length,
                color * 0.44f);
            AddSegment(fittings,
                "Painted-M1A3-BoreEvacuator",
                0.178f,
                3.22f,
                3.7f,
                color * 0.47f);
            AddSegment(fittings,
                "Painted-M1A3-MuzzleCollar",
                0.15f,
                length - 0.39f,
                length - 0.01f,
                color * 0.43f);
            AddAxial(
                "M1A3-MuzzleBore",
                fittings,
                0.099f,
                0.024f,
                length - 0.002f,
                Color.black);
            AddAxial(
                "M1A3-CoaxBarrel",
                fittings,
                0.02f,
                2.25f,
                1.58f,
                TankM1A3FamilyDetails.Gunmetal(),
                0.38f,
                -0.08f);
            Part("Painted-M1A3-CoaxHousing", fittings,
                new Vector3(0.38f, -0.08f, 0.43f),
                new Vector3(0.18f, 0.16f, 0.45f),
                color * 0.5f);
        }

        private static void AddSegment(
            Transform parent,
            string name,
            float radius,
            float start,
            float end,
            Color color)
        {
            AddAxial(
                name,
                parent,
                radius,
                end - start,
                (start + end) * 0.5f,
                color);
        }

        private static void AddAxial(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color,
            float x = 0f,
            float y = 0f)
        {
            Transform part = TankDetailGeometry.Part(
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

        private static Transform Part(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                PrimitiveType.Cube,
                parent,
                position,
                scale,
                color);
        }
    }
}
