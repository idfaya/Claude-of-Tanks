using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT14HullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            float roof = TankDetailGeometry.HullRoofY(
                definition,
                height * 0.53f);
            AddFendersAndSkirts(
                root,
                color,
                width,
                length);
            AddCrewCapsule(
                root,
                color,
                roof);
            AddEngineDeck(
                root,
                color,
                roof,
                length);
            AddRearServiceKit(
                root,
                color,
                roof,
                length);
            AddBowEquipment(
                root,
                color,
                width,
                length);
        }

        private static void AddFendersAndSkirts(
            Transform root,
            Color color,
            float width,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-T14-Fender",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.455f,
                        1.67f,
                        -0.82f),
                    new Vector3(
                        0.16f,
                        0.07f,
                        length * 0.69f),
                    color * 0.74f);
                TankDetailGeometry.Part(
                    "Painted-T14-FenderTail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.43f,
                        1.68f,
                        -length * 0.46f),
                    new Vector3(0.12f, 0.07f, 0.48f),
                    color * 0.7f);

                for (int panel = 0;
                    panel < 4;
                    panel++)
                {
                    float z = 3.45f - panel * 1.06f;
                    float top = panel == 0
                        ? 1.44f
                        : panel == 1
                            ? 1.6f
                            : 1.66f;
                    Transform plate =
                        TankDetailGeometry.Part(
                            "Painted-T14-FrontSkirtPanel",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                side * width * 0.464f,
                                (top + 0.8f) * 0.5f,
                                z),
                            new Vector3(
                                0.1f,
                                top - 0.8f,
                                1f),
                            color * (0.77f -
                                panel * 0.025f));
                    plate.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            -side * 1.7f);
                    TankDetailGeometry.Part(
                        "T14-FrontSkirtSeam",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.477f,
                            (top + 0.86f) * 0.5f,
                            z - 0.51f),
                        new Vector3(
                            0.025f,
                            top - 0.86f,
                            0.035f),
                        TankT14FamilyDetails.Dark());
                }

                for (int slat = 0;
                    slat < 7;
                    slat++)
                {
                    TankDetailGeometry.Part(
                        "T14-RearScreenSlat",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.493f,
                            0.9f + slat * 0.115f,
                            -2.775f),
                        new Vector3(0.045f, 0.085f, 2.95f),
                        TankT14FamilyDetails.Dark());
                }
                float[] supports =
                {
                    -1.45f,
                    -2.55f,
                    -3.45f,
                    -4.1f
                };
                for (int support = 0;
                    support < supports.Length;
                    support++)
                {
                    TankDetailGeometry.Part(
                        "Painted-T14-RearScreenSupport",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.492f,
                            1.23f,
                            supports[support]),
                        new Vector3(0.055f, 0.86f, 0.055f),
                        color * 0.65f);
                }
                TankDetailGeometry.Part(
                    "T14-RubberFringe",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.427f,
                        0.65f,
                        2.1f),
                    new Vector3(0.07f, 0.3f, 3.3f),
                    TankT14FamilyDetails.Dark());
            }
        }

        private static void AddCrewCapsule(
            Transform root,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "Painted-T14-CapsuleHood",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, roof + 0.04f, 1.63f),
                new Vector3(2f, 0.075f, 0.36f),
                color * 0.83f);
            float[] seats =
            {
                -0.62f,
                0f,
                0.62f
            };
            for (int hatch = 0;
                hatch < seats.Length;
                hatch++)
            {
                TankDetailGeometry.Part(
                    "Painted-T14-CapsuleHatch",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        seats[hatch],
                        roof + 0.09f,
                        1.62f),
                    new Vector3(0.21f, 0.035f, 0.21f),
                    color * 0.75f);
                TankDetailGeometry.Part(
                    "T14-CapsuleHatchRace",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        seats[hatch],
                        roof + 0.127f,
                        1.62f),
                    new Vector3(0.165f, 0.012f, 0.165f),
                    TankT14FamilyDetails.Dark());
                TankDetailGeometry.Part(
                    "Painted-T14-CapsulePeriscope",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        seats[hatch],
                        roof + 0.145f,
                        1.86f +
                            Mathf.Abs(seats[hatch]) * -0.03f),
                    new Vector3(0.16f, 0.07f, 0.11f),
                    color * 0.66f);
                TankDetailGeometry.Part(
                    "T14-CapsulePeriscopeLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        seats[hatch],
                        roof + 0.15f,
                        1.922f +
                            Mathf.Abs(seats[hatch]) * -0.03f),
                    new Vector3(0.11f, 0.04f, 0.014f),
                    TankT14FamilyDetails.Lens());
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color,
            float roof,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-T14-IntakeHump",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.56f,
                        roof + 0.07f,
                        -length * 0.21f),
                    new Vector3(0.92f, 0.14f, 1.17f),
                    color * 0.72f);
                TankDetailGeometry.Part(
                    "Painted-T14-EngineGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.82f,
                        roof + 0.1f,
                        -length * 0.36f),
                    new Vector3(1.15f, 0.035f, 1.5f),
                    color * 0.55f);
                for (int louvre = 0;
                    louvre < 6;
                    louvre++)
                {
                    TankDetailGeometry.Part(
                        "T14-EngineLouvre",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 0.82f,
                            roof + 0.125f,
                            -2.7f - louvre * 0.2f),
                        new Vector3(1.05f, 0.025f, 0.06f),
                        TankT14FamilyDetails.Dark());
                }
            }
        }

        private static void AddRearServiceKit(
            Transform root,
            Color color,
            float roof,
            float length)
        {
            Transform log =
                TankDetailGeometry.Part(
                    "Painted-T14-UnditchingLog",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(0f, roof - 0.1f, -length * 0.455f),
                    new Vector3(0.095f, 1.15f, 0.095f),
                    color * 0.52f);
            log.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-T14-RearStowageBin",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.35f,
                        roof - 0.25f,
                        -length * 0.47f),
                    new Vector3(0.44f, 0.22f, 0.1f),
                    color * 0.68f);
                TankDetailGeometry.Part(
                    "T14-RearCamera",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.6f,
                        roof - 0.12f,
                        -length * 0.495f),
                    new Vector3(0.08f, 0.08f, 0.06f),
                    TankT14FamilyDetails.Lens());
                for (int louvre = 0;
                    louvre < 3;
                    louvre++)
                {
                    Transform vent =
                        TankDetailGeometry.Part(
                            "T14-RearHeatLouvre",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                side * 0.6f,
                                roof - 0.35f +
                                    louvre * 0.1f,
                                -length * 0.465f),
                            new Vector3(0.5f, 0.04f, 0.024f),
                            TankT14FamilyDetails.Gunmetal());
                    vent.localRotation =
                        Quaternion.Euler(-15.2f, 0f, 0f);
                }
            }
        }

        private static void AddBowEquipment(
            Transform root,
            Color color,
            float width,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "T14-BowTowHook",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.72f,
                        0.62f,
                        length * 0.435f),
                    new Vector3(0.14f, 0.12f, 0.18f),
                    TankT14FamilyDetails.Gunmetal());
                TankDetailGeometry.Part(
                    "Painted-T14-MudguardSupport",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.448f,
                        1.5f,
                        length * 0.462f),
                    new Vector3(0.08f, 0.08f, 0.58f),
                    color * 0.7f);
                TankDetailGeometry.Part(
                    "T14-FrontMudguard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.387f,
                        0.99f,
                        length * 0.491f),
                    new Vector3(0.42f, 0.26f, 0.026f),
                    TankT14FamilyDetails.Dark());
                TankDetailGeometry.Part(
                    "T14-Headlight",
                    PrimitiveType.Sphere,
                    root,
                    new Vector3(
                        side * width * 0.441f,
                        1.18f,
                        length * 0.466f),
                    new Vector3(0.12f, 0.1f, 0.08f),
                    new Color(0.7f, 0.74f, 0.58f));
            }
        }
    }
}
