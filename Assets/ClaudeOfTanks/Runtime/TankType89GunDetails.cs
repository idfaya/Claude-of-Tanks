using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankType89GunDetails
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
                    "Type89-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    3.45f);
            TankDetailGeometry.Part(
                "Painted-Type89-KdeMantlet",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.33f),
                new Vector3(0.44f, 0.34f, 0.22f),
                color * 0.57f);
            AddAxial(
                "Painted-Type89-TrunnionCollar",
                fittings,
                0.11f,
                0.3f,
                0.42f,
                color * 0.48f);
            AddAxial(
                "Painted-Type89-RecoilSleeve",
                fittings,
                0.058f,
                0.85f,
                0.98f,
                color * 0.43f);
            AddAxial(
                "Painted-Type89-KdeBarrel",
                fittings,
                0.044f,
                Mathf.Max(1.2f, length - 1.51f),
                1.35f +
                    Mathf.Max(1.2f, length - 1.51f) * 0.5f,
                color * 0.39f);
            AddAxial(
                "Painted-Type89-FlashHider",
                fittings,
                0.066f,
                0.22f,
                length - 0.15f,
                color * 0.36f);
            for (int ring = 0;
                ring < 3;
                ring++)
            {
                AddAxial(
                    "Type89-FlashVentRing",
                    fittings,
                    0.072f,
                    0.016f,
                    length - 0.21f + ring * 0.06f,
                    TankType89FamilyDetails.Dark());
            }
            AddAxial(
                "Type89-MuzzleBore",
                fittings,
                0.025f,
                0.024f,
                length - 0.006f,
                Color.black);
            AddAxial(
                "Type89-CoaxBarrel",
                fittings,
                0.013f,
                0.16f,
                0.77f,
                TankType89FamilyDetails.Dark(),
                -0.32f,
                0.26f);
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
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(x, y, z),
                    new Vector3(radius, length * 0.5f, radius),
                    color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
