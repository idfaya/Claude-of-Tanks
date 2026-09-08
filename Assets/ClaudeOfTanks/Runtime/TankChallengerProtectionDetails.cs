using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankChallengerProtectionDetails
    {
        public static void Build(
            Transform root, Transform turret,
            VehicleDefinition definition, Color color,
            float width, float length)
        {
            if (definition.id == "ua_challenger2")
            {
                AddUkrainianCages(
                    root,
                    turret,
                    definition,
                    color,
                    width,
                    length);
            }
            if (definition.id == "challenger_3x")
            {
                AddChallenger3X(
                    root,
                    turret,
                    definition,
                    color,
                    width,
                    length);
            }
        }

        private static void AddUkrainianCages(
            Transform root, Transform turret,
            VehicleDefinition definition, Color color,
            float width, float length)
        {
            float hullRoof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    1.55f);
            float turretRoof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float railX = width * 0.49f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                AddSideCage(
                    root,
                    "Challenger2UA-HullCage",
                    side * railX,
                    hullRoof - 0.2f,
                    -length * 0.2f,
                    length * 0.46f,
                    8,
                    color);
                AddSideCage(
                    turret,
                    "Challenger2UA-TurretCage",
                    side * width * 0.43f,
                    turretRoof * 0.43f,
                    -1.45f,
                    2.9f,
                    6,
                    color);
            }
            AddRearCage(
                turret,
                "Challenger2UA-RearCage",
                TankDetailGeometry.TurretRearZ(
                    definition) - 0.16f,
                width * 0.86f,
                turretRoof * 0.48f,
                8,
                color);
            AddCanopy(
                turret,
                "Challenger2UA-Canopy",
                turretRoof,
                width * 0.72f,
                1.8f,
                color);
        }

        private static void AddChallenger3X(
            Transform root, Transform turret,
            VehicleDefinition definition, Color color,
            float width, float length)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                AddAutocannon(
                    turret,
                    roof,
                    color,
                    side * width * 0.37f,
                    "Challenger3X-Autocannon");
            }
            AddRadarMast(
                turret,
                roof,
                color);
            AddSearchlight(
                turret,
                roof,
                color,
                -width * 0.17f);
            AddRearCage(
                turret,
                "Challenger3X-BustleCage",
                TankDetailGeometry.TurretRearZ(
                    definition) - 0.2f,
                width * 0.82f,
                roof * 0.52f,
                8,
                color);
            AddStowage(
                turret,
                roof,
                color);
            AddSkirtHangers(
                root,
                definition,
                color,
                width,
                length);
        }

        private static void AddSideCage(
            Transform parent, string name,
            float x, float y, float z,
            float depth, int posts, Color color)
        {
            Color rail = color * 0.38f;
            for (int row = -1;
                row <= 1;
                row++)
            {
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    parent,
                    new Vector3(
                        x,
                        y + row * 0.24f,
                        z),
                    new Vector3(
                        0.028f,
                        0.028f,
                        depth),
                    rail);
            }
            for (int post = 0;
                post < posts;
                post++)
            {
                float fraction = posts == 1
                    ? 0.5f
                    : post / (float)(posts - 1);
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    parent,
                    new Vector3(
                        x,
                        y,
                        z - depth * 0.5f +
                        depth * fraction),
                    new Vector3(
                        0.028f,
                        0.52f,
                        0.028f),
                    rail);
            }
        }

        private static void AddRearCage(
            Transform turret, string name,
            float z, float width, float y,
            int posts, Color color)
        {
            Color rail = color * 0.38f;
            for (int row = -1;
                row <= 1;
                row++)
            {
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        y + row * 0.22f,
                        z),
                    new Vector3(
                        width,
                        0.028f,
                        0.028f),
                    rail);
            }
            for (int post = 0;
                post < posts;
                post++)
            {
                float fraction = post /
                    (float)(posts - 1);
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.5f +
                        width * fraction,
                        y,
                        z),
                    new Vector3(
                        0.028f,
                        0.48f,
                        0.028f),
                    rail);
            }
        }

        private static void AddCanopy(
            Transform turret, string name,
            float roof, float width,
            float depth, Color color)
        {
            Color rail = color * 0.38f;
            float y = roof + 0.72f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.5f,
                        y,
                        -0.2f),
                    new Vector3(
                        0.028f,
                        0.028f,
                        depth),
                    rail);
                for (int end = -1;
                    end <= 1;
                    end += 2)
                {
                    TankDetailGeometry.Part(
                        name,
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.5f,
                            roof + 0.36f,
                            -0.2f +
                            end * depth * 0.5f),
                        new Vector3(
                            0.028f,
                            0.72f,
                            0.028f),
                        rail);
                }
            }
            for (int end = -1;
                end <= 1;
                end += 2)
            {
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        y,
                        -0.2f +
                        end * depth * 0.5f),
                    new Vector3(
                        width,
                        0.028f,
                        0.028f),
                    rail);
            }
        }

        private static void AddAutocannon(
            Transform turret, float roof,
            Color color, float x, string name)
        {
            TankDetailGeometry.Part(
                "Painted-" + name + "-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.14f,
                    -0.45f),
                new Vector3(
                    0.34f,
                    0.3f,
                    0.5f),
                color * 0.66f);
            TankDetailGeometry.Part(
                name + "-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.2f,
                    0.43f),
                new Vector3(
                    0.055f,
                    0.055f,
                    1.3f),
                Gunmetal());
            TankDetailGeometry.Part(
                name + "-Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x - Mathf.Sign(x) *
                    0.18f,
                    roof + 0.18f,
                    -0.18f),
                new Vector3(
                    0.014f,
                    0.1f,
                    0.14f),
                Lens());
        }

        private static void AddRadarMast(
            Transform turret, float roof,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Challenger3X-RadarMast",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    roof + 0.34f,
                    -2.18f),
                new Vector3(
                    0.14f,
                    0.72f,
                    0.14f),
                color * 0.64f);
            TankDetailGeometry.Part(
                "Challenger3X-RadarArray",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    roof + 0.83f,
                    -2.18f),
                new Vector3(
                    0.9f,
                    0.52f,
                    0.08f),
                Sensor());
        }

        private static void AddSearchlight(
            Transform turret, float roof,
            Color color, float x)
        {
            TankDetailGeometry.Part(
                "Painted-Challenger3X-Searchlight",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    x,
                    roof * 0.54f,
                    1.42f),
                new Vector3(
                    0.24f,
                    0.16f,
                    0.24f),
                color * 0.62f)
                .localRotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f);
            TankDetailGeometry.Part(
                "Challenger3X-SearchlightLens",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    x,
                    roof * 0.54f,
                    1.586f),
                new Vector3(
                    0.19f,
                    0.012f,
                    0.19f),
                Lens())
                .localRotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f);
        }

        private static void AddStowage(
            Transform turret, float roof,
            Color color)
        {
            for (int item = 0;
                item < 7;
                item++)
            {
                TankDetailGeometry.Part(
                    "Painted-Challenger3X-Stowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -1.05f +
                        item * 0.35f,
                        roof * 0.48f,
                        -2.85f +
                        (item % 2) * 0.18f),
                    new Vector3(
                        0.3f,
                        0.18f,
                        0.36f),
                    color * (item % 2 == 0
                        ? 0.58f
                        : 0.72f));
            }
        }

        private static void AddSkirtHangers(
            Transform root, VehicleDefinition definition,
            Color color, float width, float length)
        {
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    1.6f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int station = 0;
                    station < 9;
                    station++)
                {
                    TankDetailGeometry.Part(
                        "Challenger3X-SkirtHanger",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.43f,
                            roof - 0.18f,
                            length * 0.36f -
                            station *
                            length * 0.09f),
                        new Vector3(
                            0.18f,
                            0.22f,
                            0.08f),
                        color * 0.5f);
                }
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }

        private static Color Sensor()
        {
            return new Color(0.11f, 0.12f, 0.1f);
        }

        private static Color Lens()
        {
            return new Color(0.04f, 0.18f, 0.2f);
        }
    }
}
