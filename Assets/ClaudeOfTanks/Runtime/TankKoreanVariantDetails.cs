using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKoreanVariantDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddHatches(
                turret,
                definition,
                color,
                width,
                roof);
            if (definition.id == "k1a1")
            {
                AddK1Roof(
                    turret,
                    color,
                    width,
                    roof);
                return;
            }

            AddK2Optics(
                turret,
                color,
                width,
                roof);
            AddK2LowWeapon(
                turret,
                color,
                width,
                roof);
            if (definition.id == "k2b")
                TankKoreanK2BDetails.Build(
                    turret,
                    color,
                    width,
                    roof);
        }

        private static void AddHatches(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float roof)
        {
            bool k1 = definition.id == "k1a1";
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float x = side * width *
                    (k1 ? 0.15f : 0.18f);
                TankDetailGeometry.Part(
                    "Painted-Korean-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.035f,
                        -0.6f),
                    new Vector3(
                        k1 ? 0.22f : 0.18f,
                        0.035f,
                        k1 ? 0.22f : 0.18f),
                    color * 0.82f);
                TankDetailGeometry.Part(
                    "Korean-HatchHandle",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.078f,
                        -0.6f),
                    new Vector3(0.2f, 0.02f, 0.045f),
                    Gunmetal());
            }
        }

        private static void AddK1Roof(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.15f,
                    roof + 0.1f,
                    0.42f),
                new Vector3(0.42f, 0.2f, 0.5f),
                "Painted-Korean-K1-GunnerDoghouse",
                "Korean-K1-GunnerLens");
            for (int block = 0;
                block < 8;
                block++)
            {
                float angle =
                    block * Mathf.PI * 0.25f;
                TankDetailGeometry.Part(
                    "Korean-K1-CupolaVision",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        width * 0.16f +
                            Mathf.Cos(angle) * 0.23f,
                        roof + 0.105f,
                        -0.62f +
                            Mathf.Sin(angle) * 0.23f),
                    new Vector3(0.1f, 0.04f, 0.025f),
                    Sensor());
            }
            for (int scope = 0;
                scope < 6;
                scope++)
            {
                int side = scope < 3 ? -1 : 1;
                int column = scope % 3;
                TankDetailGeometry.Part(
                    "Korean-K1-Periscope",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width *
                            (0.08f +
                             column * 0.055f),
                        roof + 0.09f,
                        -0.3f +
                            column * 0.1f),
                    new Vector3(0.09f, 0.06f, 0.07f),
                    Sensor());
            }
            TankKoreanFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.16f,
                    0.1f,
                    -0.58f),
                "Korean-K1-K6",
                true);
            TankKoreanFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.15f,
                    0.07f,
                    -0.5f),
                "Korean-K1-LoaderMAG",
                false);
            TankDetailGeometry.Part(
                "Korean-K1-CrosswindMast",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    -0.055f,
                    roof + 0.28f,
                    -0.79f),
                new Vector3(0.016f, 0.36f, 0.016f),
                Gunmetal());
            TankDetailGeometry.Part(
                "Korean-K1-CrosswindSensor",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -0.055f,
                    roof + 0.48f,
                    -0.79f),
                new Vector3(0.06f, 0.05f, 0.06f),
                Sensor());
        }

        private static void AddK2Optics(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.24f,
                    roof + 0.08f,
                    1.14f),
                new Vector3(0.63f, 0.2f, 0.44f),
                "Painted-Korean-K2-KGPSHousing",
                "Korean-K2-KGPSLens");
            AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.18f,
                    roof + 0.1f,
                    1.09f),
                new Vector3(0.43f, 0.22f, 0.42f),
                "Painted-Korean-K2-KCPSHousing",
                "Korean-K2-KCPSLens");
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float x = side < 0
                    ? -width * 0.28f
                    : width * 0.29f;
                float z = side < 0 ? 0.72f : 0.42f;
                TankDetailGeometry.Part(
                    "Painted-Korean-K2-KAPSRoofHead",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.065f,
                        z),
                    new Vector3(0.3f, 0.13f, 0.34f),
                    color * 0.7f);
                TankDetailGeometry.Part(
                    "Korean-K2-KAPSRoofLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.075f,
                        z + 0.18f),
                    new Vector3(0.12f, 0.06f, 0.014f),
                    Lens());
                TankDetailGeometry.Part(
                    "Painted-Korean-K2-CheekRadar",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.43f,
                        roof - 0.38f,
                        0.58f),
                    new Vector3(0.05f, 0.32f, 0.3f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "Korean-K2-CheekRadarFace",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.437f,
                        roof - 0.38f,
                        0.6f),
                    new Vector3(0.018f, 0.2f, 0.19f),
                    Sensor());
            }
            for (int scope = 0;
                scope < 6;
                scope++)
            {
                TankDetailGeometry.Part(
                    "Korean-K2-Periscope",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        width * (0.08f +
                            scope * 0.025f),
                        roof + 0.075f,
                        -0.48f +
                            (scope % 3) * 0.08f),
                    new Vector3(0.07f, 0.045f, 0.06f),
                    Sensor());
            }
        }

        private static void AddK2LowWeapon(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            TankKoreanFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.15f,
                    0.04f,
                    0.98f),
                "Korean-K2-K6",
                true);
        }

        private static void AddSight(
            Transform turret,
            Color color,
            Vector3 center,
            Vector3 size,
            string housingName,
            string lensName)
        {
            TankDetailGeometry.Part(
                housingName,
                PrimitiveType.Cube,
                turret,
                center,
                size,
                color * 0.68f);
            TankDetailGeometry.Part(
                lensName,
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    center.x,
                    center.y,
                    center.z + size.z * 0.52f),
                new Vector3(
                    size.x * 0.58f,
                    size.y * 0.45f,
                    0.014f),
                Lens());
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }

        private static Color Sensor()
        {
            return new Color(0.1f, 0.12f, 0.1f);
        }

        private static Color Lens()
        {
            return new Color(0.025f, 0.14f, 0.17f);
        }
    }
}
