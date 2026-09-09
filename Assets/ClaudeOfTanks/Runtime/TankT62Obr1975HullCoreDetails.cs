using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT62Obr1975HullCoreDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            Part(
                "Painted-T62-LowerTub",
                root,
                new Vector3(0f, 0.77f, -0.05f),
                new Vector3(2.12f, 0.78f, 6.25f),
                color * 0.52f);
            Part(
                "Painted-T62-UpperHull",
                root,
                new Vector3(0f, 1.25f, -0.38f),
                new Vector3(3.42f, 0.46f, 5.56f),
                color * 0.67f);
            Part(
                "Painted-T62-Deck",
                root,
                new Vector3(0f, 1.5f, -0.42f),
                new Vector3(3.34f, 0.055f, 5.32f),
                color * 0.73f);

            Transform glacis = Part(
                "Painted-T62-UpperGlacis",
                root,
                new Vector3(0f, 1.25f, 2.42f),
                new Vector3(3.38f, 0.12f, 1.88f),
                color * 0.7f);
            glacis.localRotation =
                Quaternion.Euler(-18f, 0f, 0f);
            Transform lowerBow = Part(
                "Painted-T62-LowerBow",
                root,
                new Vector3(0f, 0.76f, 3.08f),
                new Vector3(2.08f, 0.15f, 1.06f),
                color * 0.49f);
            lowerBow.localRotation =
                Quaternion.Euler(27f, 0f, 0f);

            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform shoulder = Part(
                    "Painted-T62-BowShoulder",
                    root,
                    new Vector3(
                        side * 1.3f,
                        1.15f,
                        2.92f),
                    new Vector3(
                        0.72f,
                        0.18f,
                        1.05f),
                    color * 0.65f);
                shoulder.localRotation =
                    Quaternion.Euler(
                        -18f,
                        side * 12f,
                        0f);
                Part(
                    "Painted-T62-RearSponson",
                    root,
                    new Vector3(
                        side * 1.39f,
                        1.31f,
                        -2.32f),
                    new Vector3(
                        0.62f,
                        0.4f,
                        1.52f),
                    color * 0.62f);
            }

            Transform rear = Part(
                "Painted-T62-RearPlate",
                root,
                new Vector3(0f, 0.94f, -3.13f),
                new Vector3(2.95f, 0.72f, 0.12f),
                color * 0.48f);
            rear.localRotation =
                Quaternion.Euler(-8f, 0f, 0f);
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
