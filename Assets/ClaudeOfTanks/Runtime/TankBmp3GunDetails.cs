using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmp3GunDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            bool rok)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "Bmp3-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    rok ? 2.52f : 2.95f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(
                    definition);

            AddCradle(
                fittings,
                color,
                rok);
            Add2A70(
                fittings,
                color,
                length,
                radius);
            Add2A72(
                fittings,
                color,
                length);
            AddCoaxPkt(
                fittings,
                length);
        }

        private static void AddCradle(
            Transform fittings,
            Color color,
            bool rok)
        {
            TankDetailGeometry.Part(
                "Painted-Bmp3-TripleGunCradle",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.3f),
                new Vector3(
                    rok ? 0.68f : 0.56f,
                    rok ? 0.42f : 0.4f,
                    0.36f),
                color * 0.61f);
            AddAxialCylinder(
                "Painted-Bmp3-2A70RootCollar",
                fittings,
                0.14f,
                0.34f,
                0.62f,
                color * 0.56f);
        }

        private static void Add2A70(
            Transform fittings,
            Color color,
            float length,
            float radius)
        {
            const float tubeStart = 0.78f;
            const float muzzleLength = 0.12f;
            float tubeLength =
                Mathf.Max(
                    0.9f,
                    length -
                    tubeStart -
                    muzzleLength);
            float tubeCenter =
                tubeStart +
                tubeLength * 0.5f;
            AddAxialCylinder(
                "Painted-Bmp3-2A70Barrel",
                fittings,
                Mathf.Max(radius, 0.058f),
                tubeLength,
                tubeCenter,
                color * 0.43f);
            AddAxialCylinder(
                "Painted-Bmp3-2A70MuzzleRing",
                fittings,
                Mathf.Max(radius * 1.25f, 0.072f),
                muzzleLength,
                length - muzzleLength * 0.5f,
                color * 0.38f);
            AddAxialCylinder(
                "Bmp3-2A70MuzzleBore",
                fittings,
                Mathf.Max(radius * 0.45f, 0.026f),
                0.026f,
                length - 0.007f,
                Color.black);
        }

        private static void Add2A72(
            Transform fittings,
            Color color,
            float length)
        {
            const float x = -0.16f;
            const float y = 0.01f;
            const float start = 0.72f;
            float secondaryLength =
                Mathf.Max(
                    1.4f,
                    Mathf.Min(2.3f, length - 0.45f));
            float end =
                start +
                secondaryLength;
            AddAxialCylinder(
                "Painted-Bmp3-2A72Barrel",
                fittings,
                0.03f,
                secondaryLength,
                start + secondaryLength * 0.5f,
                color * 0.39f,
                x,
                y);
            AddAxialCylinder(
                "Painted-Bmp3-2A72MuzzleSleeve",
                fittings,
                0.052f,
                0.15f,
                end - 0.075f,
                color * 0.34f,
                x,
                y);
            AddAxialCylinder(
                "Bmp3-2A72MuzzleBore",
                fittings,
                0.018f,
                0.025f,
                end + 0.007f,
                Color.black,
                x,
                y);
        }

        private static void AddCoaxPkt(
            Transform fittings,
            float length)
        {
            const float x = 0.2f;
            const float y = 0.04f;
            float end =
                Mathf.Min(
                    1.3f,
                    length - 0.35f);
            TankDetailGeometry.Part(
                "Bmp3-CoaxPktReceiver",
                PrimitiveType.Cube,
                fittings,
                new Vector3(x, y, 0.55f),
                new Vector3(0.1f, 0.1f, 0.24f),
                TankBmp3FamilyDetails.Gunmetal());
            AddAxialCylinder(
                "Bmp3-CoaxPktBarrel",
                fittings,
                0.014f,
                end - 0.62f,
                (end + 0.62f) * 0.5f,
                TankBmp3FamilyDetails.Dark(),
                x,
                y);
            AddAxialCylinder(
                "Bmp3-CoaxPktMuzzle",
                fittings,
                0.009f,
                0.024f,
                end,
                Color.black,
                x,
                y);
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
