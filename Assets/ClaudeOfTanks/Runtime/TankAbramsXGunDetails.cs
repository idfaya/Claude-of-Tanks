using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsXGunDetails
    {
        private const float Xm360Length = 3.69f;

        public static void Build(
            Transform turret,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "AbramsX-GunFittings");
            Part("Painted-AbramsX-GunTunnel", fittings,
                new Vector3(0f, 0f, 0.18f),
                new Vector3(0.5f, 0.42f, 0.5f),
                color * 0.48f);
            Part("Painted-AbramsX-MantletCollar", fittings,
                new Vector3(0f, -0.01f, 0.29f),
                new Vector3(0.44f, 0.26f, 0.34f),
                color * 0.55f);
            Part("AbramsX-MantletLowerSeam", fittings,
                new Vector3(0f, -0.15f, 0.3f),
                new Vector3(0.42f, 0.035f, 0.12f),
                TankAbramsXFamilyDetails.Dark());
            AddSegment(fittings,
                "Painted-AbramsX-XM360Tube",
                0.112f,
                0.12f,
                2.77f,
                color * 0.44f);
            AddSegment(fittings,
                "Painted-AbramsX-XM360VentilatedShroud",
                0.125f,
                2.77f,
                Xm360Length,
                color * 0.5f);
            AddSegment(fittings,
                "Painted-AbramsX-XM360Clamp",
                0.129f,
                1.438f,
                1.59f,
                color * 0.56f);
            for (int band = 0; band < 4; band++)
                AddSegment(fittings,
                    "AbramsX-XM360ShroudBand",
                    0.132f,
                    2.78f + band * 0.23f,
                    2.805f + band * 0.23f,
                    TankAbramsXFamilyDetails.Gunmetal());
            for (int row = 0; row < 10; row++)
            {
                float z = 2.825f + row * 0.082f;
                for (int side = -1; side <= 1; side += 2)
                    Part("AbramsX-XM360Vent", fittings,
                        new Vector3(
                            side * 0.112f,
                            0f,
                            z),
                        new Vector3(0.018f, 0.038f, 0.032f),
                        TankAbramsXFamilyDetails.Dark());
            }
            AddSegment(fittings,
                "Painted-AbramsX-XM360MuzzleRing",
                0.132f,
                Xm360Length - 0.03f,
                Xm360Length - 0.005f,
                color * 0.48f);
            AddAxial(
                "AbramsX-XM360Bore",
                fittings,
                0.06f,
                0.016f,
                Xm360Length + 0.0005f,
                Color.black);
            AddAxial(
                "AbramsX-CoaxBarrel",
                fittings,
                0.018f,
                1.85f,
                1.18f,
                TankAbramsXFamilyDetails.Gunmetal(),
                0.19f,
                0.055f);
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
