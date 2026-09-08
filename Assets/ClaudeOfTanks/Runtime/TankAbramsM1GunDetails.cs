using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsM1GunDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            bool heavyArmor)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "AbramsM1-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    5.28f);
            Part("Painted-AbramsM1-Mantlet", fittings,
                new Vector3(0f, 0f, 0.28f),
                new Vector3(0.64f, 0.58f, 0.42f),
                color * 0.52f);
            Part("Painted-AbramsM1-MantletCover", fittings,
                new Vector3(0f, 0.05f, 0.47f),
                new Vector3(0.56f, 0.4f, 0.13f),
                color * 0.47f);
            AddAxial("Painted-AbramsM1-ThermalJacket", fittings,
                0.125f, 1.36f, 1.22f, color * 0.42f);
            AddAxial("Painted-AbramsM1-Evacuator", fittings,
                0.166f, 0.34f, 1.72f, color * 0.4f);
            AddAxial("Painted-AbramsM1-M256Barrel", fittings,
                0.095f, Mathf.Max(1.6f, length - 2.05f),
                2.05f + Mathf.Max(1.6f, length - 2.05f) * 0.5f,
                color * 0.36f);
            AddAxial("Painted-AbramsM1-MuzzleCollar", fittings,
                0.13f, 0.16f, length - 0.1f, color * 0.34f);
            AddAxial("AbramsM1-MuzzleBore", fittings,
                0.058f, 0.024f, length - 0.006f, Color.black);
            AddAxial("AbramsM1-CoaxBarrel", fittings,
                0.022f, 0.42f, 0.7f,
                TankAbramsM1FamilyDetails.Dark(), 0.16f, 0.02f);
            if (heavyArmor)
            {
                Part("Painted-AbramsM1Ha-Searchlight", fittings,
                    new Vector3(-0.48f, 0.04f, 0.54f),
                    new Vector3(0.34f, 0.32f, 0.22f),
                    color * 0.46f);
                Part("AbramsM1Ha-SearchlightLens", fittings,
                    new Vector3(-0.48f, 0.04f, 0.665f),
                    new Vector3(0.245f, 0.215f, 0.018f),
                    TankAbramsM1FamilyDetails.Glass());
            }
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
                new Vector3(radius, length * 0.5f, radius),
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
