using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBwp1GunDetails
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
                    "Bwp1-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    2.52f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(
                    definition);

            TankDetailGeometry.Part(
                "Painted-Bwp1-GunCradle",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.3f),
                new Vector3(0.5f, 0.36f, 0.26f),
                color * 0.6f);
            AddAxialCylinder(
                "Painted-Bwp1-RootCollar",
                fittings,
                0.12f,
                0.32f,
                0.58f,
                color * 0.52f);

            const float start = 0.7f;
            const float muzzleLength = 0.18f;
            float tubeLength =
                Mathf.Max(
                    0.8f,
                    length - start - muzzleLength);
            float tubeCenter =
                start + tubeLength * 0.5f;
            AddAxialCylinder(
                "Painted-Bwp1-Mk30Barrel",
                fittings,
                Mathf.Max(radius, 0.04f),
                tubeLength,
                tubeCenter,
                color * 0.43f);
            AddAxialCylinder(
                "Painted-Bwp1-Evacuator",
                fittings,
                0.065f,
                0.38f,
                Mathf.Min(length - 0.45f, 1.55f),
                color * 0.5f);
            AddAxialCylinder(
                "Painted-Bwp1-FlashHider",
                fittings,
                0.058f,
                muzzleLength,
                length - muzzleLength * 0.5f,
                color * 0.38f);
            AddAxialCylinder(
                "Bwp1-MuzzleBore",
                fittings,
                0.021f,
                0.026f,
                length - 0.006f,
                Color.black);
        }

        private static void AddAxialCylinder(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(0f, 0f, z),
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
