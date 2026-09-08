using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKoreanK2BDetails
    {
        public static void Build(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            AddRemoteWeaponStation(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.075f,
                    0.08f,
                    -0.94f),
                "K2B-RoofRWS",
                false);
            AddRemoteWeaponStation(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.185f,
                    0.05f,
                    -0.68f),
                "K2B-AuxOpenYokeRWS",
                true);
            AddElectroOptics(
                turret,
                color,
                width,
                roof);
            AddWhips(
                turret,
                width,
                roof);
            AddGunMask(
                turret,
                color);
        }

        private static void AddElectroOptics(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-K2B-EOHead",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.19f,
                        roof + 0.18f,
                        0.22f),
                    new Vector3(0.42f, 0.27f, 0.42f),
                    color * 0.65f);
                for (int lens = -1;
                    lens <= 1;
                    lens += 2)
                {
                    TankDetailGeometry.Part(
                        "K2B-EOLens",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * width * 0.19f +
                                lens * 0.09f,
                            roof + 0.18f,
                            0.445f),
                        new Vector3(0.055f, 0.03f, 0.055f),
                        Lens())
                        .localRotation =
                        Quaternion.Euler(90f, 0f, 0f);
                }
            }
        }

        private static void AddWhips(
            Transform turret,
            float width,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "K2B-Antenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.26f,
                        roof +
                            (side < 0 ? 0.44f : 0.38f),
                        -1.72f),
                    new Vector3(
                        0.012f,
                        side < 0 ? 0.8f : 0.68f,
                        0.012f),
                    Gunmetal());
            }
        }

        private static void AddRemoteWeaponStation(
            Transform turret,
            float roof,
            Color color,
            Vector3 seat,
            string prefix,
            bool openYoke)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Base",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.21f, 0.07f, 0.21f),
                color * 0.7f);
            if (openYoke)
            {
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    TankDetailGeometry.Part(
                        prefix + "-YokeArm",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            seat.x + side * 0.14f,
                            roof + seat.y + 0.2f,
                            seat.z),
                        new Vector3(0.045f, 0.32f, 0.06f),
                        Gunmetal());
                }
            }
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.25f,
                    seat.z + 0.1f),
                new Vector3(0.3f, 0.18f, 0.4f),
                color * 0.62f);
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.26f,
                    seat.z + 0.58f),
                new Vector3(0.035f, 0.035f, 0.72f),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-Sensor",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x - 0.23f,
                    roof + seat.y + 0.27f,
                    seat.z + 0.04f),
                new Vector3(0.14f, 0.18f, 0.15f),
                Sensor());
            TankDetailGeometry.Part(
                prefix + "-Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x - 0.23f,
                    roof + seat.y + 0.27f,
                    seat.z + 0.122f),
                new Vector3(0.08f, 0.08f, 0.014f),
                Lens());
            if (!openYoke) return;
            for (int link = 0;
                link < 4;
                link++)
            {
                TankDetailGeometry.Part(
                    prefix + "-FeedBelt",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        seat.x + 0.19f,
                        roof + seat.y + 0.19f -
                            link * 0.035f,
                        seat.z + 0.08f -
                            link * 0.035f),
                    new Vector3(0.035f, 0.025f, 0.07f),
                    new Color(0.42f, 0.32f, 0.12f));
            }
        }

        private static void AddGunMask(
            Transform turret,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            TankDetailGeometry.Part(
                "Painted-K2B-GunMask",
                PrimitiveType.Cube,
                gun,
                new Vector3(0f, -0.01f, 0.24f),
                new Vector3(0.72f, 0.48f, 0.28f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "K2B-GunMaskCollar",
                PrimitiveType.Cylinder,
                gun,
                new Vector3(0f, 0f, 0.54f),
                new Vector3(0.2f, 0.18f, 0.2f),
                Gunmetal())
                .localRotation =
                Quaternion.Euler(90f, 0f, 0f);
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
