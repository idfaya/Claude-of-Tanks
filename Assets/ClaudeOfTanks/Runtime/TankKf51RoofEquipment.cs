using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKf51RoofEquipment
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
            bool ownerExact =
                definition.id == "kf51b";
            AddOpenYokeRws(
                turret,
                color,
                roof,
                ownerExact);
            AddTwinAntennas(
                turret,
                width,
                roof,
                ownerExact);
            AddBustleStructure(
                turret,
                definition,
                color,
                width,
                roof,
                ownerExact);
        }

        private static void AddOpenYokeRws(
            Transform turret,
            Color color,
            float roof,
            bool ownerExact)
        {
            string prefix = ownerExact
                ? "KF51B-OpenYokeRWS"
                : "KF51-OpenYokeRWS";
            Vector3 seat = ownerExact
                ? new Vector3(0.3f, 0.06f, -2.16f)
                : new Vector3(0.42f, 0.07f, -1.72f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Bearing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.24f, 0.07f, 0.24f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Tower",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.17f,
                    seat.z),
                new Vector3(0.42f, 0.22f, 0.34f),
                color * 0.56f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    prefix + "-YokeArm",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        seat.x + side * 0.17f,
                        roof + seat.y + 0.39f,
                        seat.z + 0.04f),
                    new Vector3(0.045f, 0.36f, 0.07f),
                    TankKf51FamilyDetails.Gunmetal());
            }
            TankDetailGeometry.Part(
                prefix + "-Mechanism",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.43f,
                    seat.z + 0.1f),
                new Vector3(0.3f, 0.18f, 0.42f),
                TankKf51FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.44f,
                    seat.z + 0.63f),
                new Vector3(0.036f, 0.036f, 0.82f),
                TankKf51FamilyDetails.Gunmetal());
            float sensorX =
                seat.x + (ownerExact ? -0.28f : -0.3f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-SensorHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    sensorX,
                    roof + seat.y + 0.43f,
                    seat.z + 0.03f),
                new Vector3(0.16f, 0.22f, 0.18f),
                color * 0.5f);
            for (int aperture = -1;
                aperture <= 1;
                aperture += 2)
            {
                TankDetailGeometry.Part(
                    prefix + "-SensorAperture",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        sensorX +
                            aperture * 0.038f,
                        roof + seat.y + 0.43f,
                        seat.z + 0.128f),
                    new Vector3(0.055f, 0.085f, 0.014f),
                    TankKf51FamilyDetails.Lens());
            }
            for (int link = 0;
                link < 4;
                link++)
            {
                TankDetailGeometry.Part(
                    prefix + "-FeedBelt",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        seat.x + 0.22f,
                        roof + seat.y + 0.36f -
                            link * 0.035f,
                        seat.z + 0.12f -
                            link * 0.04f),
                    new Vector3(0.035f, 0.026f, 0.075f),
                    new Color(0.4f, 0.3f, 0.11f));
            }
            int lightCount = ownerExact ? 2 : 5;
            for (int light = 0;
                light < lightCount;
                light++)
            {
                float offset =
                    (light -
                     (lightCount - 1) * 0.5f) *
                    0.06f;
                TankDetailGeometry.Part(
                    prefix + "-WorkLight",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        seat.x + offset,
                        roof + seat.y + 0.25f,
                        seat.z + 0.181f),
                    new Vector3(0.045f, 0.045f, 0.014f),
                    TankKf51FamilyDetails.Lens());
            }
        }

        private static void AddTwinAntennas(
            Transform turret,
            float width,
            float roof,
            bool ownerExact)
        {
            float rear =
                ownerExact ? -2.42f : -2.8f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-KF51-AntennaBase",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.285f,
                        roof + 0.06f,
                        rear),
                    new Vector3(0.04f, 0.06f, 0.04f),
                    new Color(0.2f, 0.2f, 0.16f));
                TankDetailGeometry.Part(
                    "KF51-Antenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.285f,
                        roof +
                            (ownerExact ? 0.47f : 0.55f),
                        rear),
                    new Vector3(
                        0.011f,
                        ownerExact ? 0.78f : 0.95f,
                        0.011f),
                    TankKf51FamilyDetails.Gunmetal());
            }
        }

        private static void AddBustleStructure(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float roof,
            bool ownerExact)
        {
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            if (!ownerExact)
            {
                TankDetailGeometry.Part(
                    "Painted-KF51-BustleTower",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        width * 0.1f,
                        roof + 0.18f,
                        rear - 0.1f),
                    new Vector3(1.2f, 0.42f, 0.46f),
                    color * 0.64f);
                for (int rail = 0;
                    rail < 4;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "KF51-BustleRackRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            -width * 0.13f +
                                rail * width * 0.085f,
                            roof + 0.18f,
                            rear - 0.34f),
                        new Vector3(0.035f, 0.4f, 0.035f),
                        color * 0.36f);
                }
                return;
            }

            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int section = 0;
                    section < 2;
                    section++)
                {
                    float centerZ =
                        rear + 0.24f +
                        section * 0.62f;
                    for (int rail = 0;
                        rail < 6;
                        rail++)
                    {
                        TankDetailGeometry.Part(
                            "KF51B-BustleCageSideRail",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side * width * 0.415f,
                                roof - 0.5f +
                                    rail * 0.082f,
                                centerZ),
                            new Vector3(0.02f, 0.02f, 0.54f),
                            color * 0.37f);
                    }
                    for (int edge = -1;
                        edge <= 1;
                        edge += 2)
                    {
                        TankDetailGeometry.Part(
                            "KF51B-BustleCageSidePost",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side * width * 0.415f,
                                roof - 0.295f,
                                centerZ + edge * 0.28f),
                            new Vector3(0.024f, 0.5f, 0.024f),
                            color * 0.4f);
                    }
                }
            }
            float rearZ = rear - 0.42f;
            for (int rail = 0;
                rail < 6;
                rail++)
            {
                TankDetailGeometry.Part(
                    "KF51B-BustleCageRearRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.5f +
                            rail * 0.082f,
                        rearZ),
                    new Vector3(
                        width * 0.52f,
                        0.02f,
                        0.02f),
                    color * 0.37f);
            }
            for (int post = 0;
                post < 5;
                post++)
            {
                TankDetailGeometry.Part(
                    "KF51B-BustleCageRearPost",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.24f +
                            post * width * 0.12f,
                        roof - 0.295f,
                        rearZ),
                    new Vector3(0.024f, 0.46f, 0.024f),
                    color * 0.4f);
            }
        }
    }
}
