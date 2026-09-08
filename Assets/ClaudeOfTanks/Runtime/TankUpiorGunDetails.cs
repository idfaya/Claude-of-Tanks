using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankUpiorGunDetails
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
                    "Upior-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    2.4f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(
                    definition);

            TankDetailGeometry.Part(
                "Painted-Upior-GunCradle",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.26f),
                new Vector3(0.3f, 0.24f, 0.34f),
                color * 0.6f);
            AddAxialCylinder(
                "Painted-Upior-RootCollar",
                fittings,
                0.062f,
                0.2f,
                0.5f,
                color * 0.5f);
            const float start = 0.6f;
            const float muzzleLength = 0.12f;
            float tubeLength =
                Mathf.Max(
                    0.8f,
                    length - start - muzzleLength);
            AddAxialCylinder(
                "Painted-Upior-30mmBarrel",
                fittings,
                Mathf.Max(radius, 0.035f),
                tubeLength,
                start + tubeLength * 0.5f,
                color * 0.42f);
            AddAxialCylinder(
                "Painted-Upior-MuzzleCollar",
                fittings,
                0.05f,
                muzzleLength,
                length - muzzleLength * 0.5f,
                color * 0.38f);
            AddAxialCylinder(
                "Upior-MuzzleBore",
                fittings,
                0.018f,
                0.024f,
                length - 0.005f,
                Color.black);
            TankDetailGeometry.Part(
                "Upior-CoaxReceiver",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.15f, 0.03f, 0.7f),
                new Vector3(0.08f, 0.08f, 0.16f),
                TankUpiorFamilyDetails.Gunmetal());
            AddAxialCylinder(
                "Upior-CoaxBarrel",
                fittings,
                0.013f,
                0.5f,
                0.85f,
                TankUpiorFamilyDetails.Dark(),
                0.15f,
                0.03f);
            AddAxialCylinder(
                "Upior-CoaxMuzzle",
                fittings,
                0.009f,
                0.022f,
                1.105f,
                Color.black,
                0.15f,
                0.03f);
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
