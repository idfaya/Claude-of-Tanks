using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPattonRoofEquipment
    {
        public static void AddCupola(
            Transform turret,
            Color color,
            float roof,
            Vector3 seat,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Cupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.27f, 0.12f, 0.27f),
                color * 0.78f);
        }

        public static void AddM19Cupola(
            Transform turret,
            Color color,
            float roof,
            float width,
            string prefix)
        {
            float x = -width * 0.16f;
            float z = 0.2f;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-M19Cupola",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, roof + 0.12f, z),
                new Vector3(0.3f, 0.2f, 0.3f),
                color * 0.78f);
            for (int block = 0;
                block < 7;
                block++)
            {
                float angle =
                    block * Mathf.PI * 2f / 7f;
                TankDetailGeometry.Part(
                    prefix + "-M19VisionBlock",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x + Mathf.Sin(angle) * 0.25f,
                        roof + 0.16f,
                        z + Mathf.Cos(angle) * 0.25f),
                    new Vector3(0.07f, 0.06f, 0.035f),
                    TankPattonFamilyDetails.Lens())
                    .localRotation =
                    Quaternion.Euler(
                        0f,
                        angle * Mathf.Rad2Deg,
                        0f);
            }
            TankDetailGeometry.Part(
                prefix + "-M85-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(x, roof + 0.29f, z + 0.12f),
                new Vector3(0.13f, 0.1f, 0.34f),
                TankPattonFamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-M85-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(x, roof + 0.3f, z + 0.55f),
                new Vector3(0.028f, 0.028f, 0.62f),
                TankPattonFamilyDetails.Gunmetal());
        }

        public static void AddRemoteWeaponStation(
            Transform turret,
            Color color,
            float roof,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-RWS-Body",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.58f, roof + 0.3f, -0.34f),
                new Vector3(0.42f, 0.28f, 0.46f),
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-RWS-M2-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.58f, roof + 0.43f, -0.05f),
                new Vector3(0.18f, 0.13f, 0.4f),
                TankPattonFamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-RWS-M2-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.58f, roof + 0.44f, 0.48f),
                new Vector3(0.034f, 0.034f, 0.78f),
                TankPattonFamilyDetails.Gunmetal());
            TankPattonFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(-0.84f, roof + 0.34f, -0.2f),
                new Vector3(0.16f, 0.2f, 0.18f),
                prefix + "-RWS-Sensor");
        }

        public static void AddCheekCourse(
            Transform turret,
            Color color,
            float roof,
            float width,
            string prefix,
            int columns,
            int rows)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int row = 0;
                    row < rows;
                    row++)
                {
                    for (int column = 0;
                        column < columns;
                        column++)
                    {
                        TankDetailGeometry.Part(
                            "Painted-" + prefix +
                                "-CheekCassette",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side * width *
                                    (0.28f + row * 0.035f),
                                roof - 0.34f + row * 0.23f,
                                0.78f - column * 0.39f),
                            new Vector3(
                                0.09f,
                                0.19f,
                                0.32f),
                            color * 0.8f);
                    }
                }
            }
        }

        public static void AddSmokeBanks(
            Transform turret,
            Color color,
            float roof,
            float width,
            string prefix)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < 6;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-" + prefix +
                                "-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * width *
                                    (0.25f +
                                     tube * 0.012f),
                                roof - 0.2f +
                                    tube * 0.025f,
                                0.68f -
                                    tube * 0.08f),
                            new Vector3(0.05f, 0.16f, 0.05f),
                            color * 0.6f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            64f,
                            0f,
                            side * 18f);
                }
            }
        }

        public static void AddRack(
            Transform turret,
            Color color,
            float width,
            float depth,
            float y,
            float z,
            string prefix)
        {
            for (int rail = -1;
                rail <= 1;
                rail += 2)
            {
                TankDetailGeometry.Part(
                    prefix + "-BustleSideRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        rail * width * 0.5f,
                        y,
                        z),
                    new Vector3(0.035f, 0.28f, depth),
                    TankPattonFamilyDetails.Gunmetal());
            }
            for (int bar = 0;
                bar < 3;
                bar++)
            {
                TankDetailGeometry.Part(
                    prefix + "-BustleCrossRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        y - 0.12f + bar * 0.12f,
                        z - depth * 0.25f),
                    new Vector3(width, 0.035f, 0.035f),
                    TankPattonFamilyDetails.Gunmetal());
            }
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-BustleStowage",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, y - 0.02f, z + 0.03f),
                new Vector3(
                    width * 0.72f,
                    0.24f,
                    depth * 0.58f),
                color * 0.7f);
        }

        public static void AddTwinAntennas(
            Transform turret,
            float roof,
            float width,
            string prefix)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankPattonFamilyDetails.AddAntenna(
                    turret,
                    roof,
                    new Vector3(
                        side * width * 0.27f,
                        0.08f,
                        -0.78f),
                    prefix);
            }
        }

        public static void AddTurretSearchlight(
            Transform turret,
            Color color,
            float roof,
            Vector3 seat,
            string prefix)
        {
            Transform housing =
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-Searchlight",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        seat.x,
                        roof + seat.y,
                        seat.z),
                    new Vector3(0.28f, 0.34f, 0.28f),
                    color * 0.7f);
            housing.localRotation =
                Quaternion.Euler(90f, 0f, 12f);
            TankDetailGeometry.Part(
                prefix + "-SearchlightLens",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z + 0.18f),
                new Vector3(0.22f, 0.025f, 0.22f),
                TankPattonFamilyDetails.Lens())
                .localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
