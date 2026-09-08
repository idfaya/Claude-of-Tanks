using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmp2GunDetails
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
                    "Bmp2-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    2.52f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(
                    definition);

            AddCradle(
                fittings,
                color);
            Add2A42(
                fittings,
                color,
                length,
                radius);
            AddCoaxPkt(
                fittings);
        }

        private static void AddCradle(
            Transform fittings,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bmp2-2A42Cradle",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, -0.02f, 0.18f),
                new Vector3(0.18f, 0.2f, 0.35f),
                color * 0.61f);
            TankDetailGeometry.Part(
                "Painted-Bmp2-2A42RootCollar",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.6f),
                new Vector3(0.23f, 0.15f, 0.47f),
                color * 0.56f);
        }

        private static void Add2A42(
            Transform fittings,
            Color color,
            float length,
            float radius)
        {
            const float tubeStart = 0.82f;
            const float flashLength = 0.15f;
            float tubeLength =
                Mathf.Max(
                    0.8f,
                    length -
                    tubeStart -
                    flashLength);
            float tubeCenter =
                tubeStart +
                tubeLength * 0.5f;
            AddAxialCylinder(
                "Painted-Bmp2-2A42Barrel",
                fittings,
                Mathf.Max(radius, 0.05f),
                tubeLength,
                tubeCenter,
                color * 0.45f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                AddAxialCylinder(
                    "Bmp2-2A42GuideRail",
                    fittings,
                    0.016f,
                    tubeLength,
                    tubeCenter,
                    TankBmp2FamilyDetails.Gunmetal(),
                    side * 0.098f,
                    0f);
            }
            AddAxialCylinder(
                "Painted-Bmp2-2A42FlashHider",
                fittings,
                0.06f,
                flashLength,
                length - flashLength * 0.5f,
                color * 0.39f);
            AddAxialCylinder(
                "Bmp2-2A42MuzzleBore",
                fittings,
                0.022f,
                0.026f,
                length - 0.007f,
                Color.black);
        }

        private static void AddCoaxPkt(
            Transform fittings)
        {
            TankDetailGeometry.Part(
                "Bmp2-CoaxPktReceiver",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.2f, 0.06f, 0.62f),
                new Vector3(0.1f, 0.1f, 0.18f),
                TankBmp2FamilyDetails.Gunmetal());
            AddAxialCylinder(
                "Bmp2-CoaxPktBarrel",
                fittings,
                0.014f,
                0.52f,
                0.94f,
                TankBmp2FamilyDetails.Dark(),
                0.2f,
                0.06f);
            AddAxialCylinder(
                "Bmp2-CoaxPktMuzzle",
                fittings,
                0.009f,
                0.025f,
                1.205f,
                Color.black,
                0.2f,
                0.06f);
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
