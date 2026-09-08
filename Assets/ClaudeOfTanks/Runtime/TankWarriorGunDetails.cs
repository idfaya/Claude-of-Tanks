using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankWarriorGunDetails
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
                    "Warrior-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    2.2263f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(
                    definition);
            TankDetailGeometry.Part(
                "Painted-Warrior-GunCradle",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.28f),
                new Vector3(0.52f, 0.32f, 0.3f),
                color * 0.58f);
            AddAxial(
                "Painted-Warrior-RardenBarrel",
                fittings,
                Mathf.Max(radius, 0.038f),
                length - 0.55f,
                0.55f + (length - 0.55f) * 0.5f,
                color * 0.4f);
            AddAxial(
                "Painted-Warrior-MuzzleSleeve",
                fittings,
                0.055f,
                0.15f,
                length - 0.075f,
                color * 0.36f);
            AddAxial(
                "Warrior-MuzzleBore",
                fittings,
                0.019f,
                0.022f,
                length - 0.004f,
                Color.black);
        }

        private static void AddAxial(
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
                    new Vector3(radius, length * 0.5f, radius),
                    color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
