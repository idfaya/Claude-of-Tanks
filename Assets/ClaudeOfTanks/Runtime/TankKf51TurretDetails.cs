using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKf51TurretDetails
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
                definition.id,
                color,
                roof);
            AddSmokeLaunchers(
                turret,
                color,
                width,
                roof);
            if (definition.id == "kf51")
            {
                AddClosedChevron(
                    turret,
                    color,
                    width,
                    roof);
                AddSeossTower(
                    turret,
                    color,
                    roof,
                    false);
            }
            else
            {
                AddConvexCrown(
                    turret,
                    color,
                    width,
                    roof);
                AddFlankPanels(
                    turret,
                    color,
                    width,
                    roof);
                AddRaisedMultispectralSight(
                    turret,
                    color,
                    roof);
                AddSeossTower(
                    turret,
                    color,
                    roof,
                    true);
            }
        }

        private static void AddHatches(
            Transform turret,
            string id,
            Color color,
            float roof)
        {
            Vector3[] seats = id == "kf51"
                ? new[]
                {
                    new Vector3(0.62f, roof + 0.035f, -0.75f),
                    new Vector3(-0.64f, roof + 0.035f, -0.65f)
                }
                : new[]
                {
                    new Vector3(-0.48f, roof + 0.04f, -0.12f),
                    new Vector3(0.47f, roof + 0.035f, -0.22f)
                };
            for (int hatch = 0;
                hatch < seats.Length;
                hatch++)
            {
                TankDetailGeometry.Part(
                    "Painted-KF51-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    seats[hatch],
                    new Vector3(
                        hatch == 0 ? 0.3f : 0.27f,
                        0.045f,
                        hatch == 0 ? 0.3f : 0.27f),
                    color * 0.75f);
                TankDetailGeometry.Part(
                    "KF51-HatchRace",
                    PrimitiveType.Cylinder,
                    turret,
                    seats[hatch] +
                        new Vector3(0f, 0.027f, 0f),
                    new Vector3(
                        hatch == 0 ? 0.25f : 0.22f,
                        0.012f,
                        hatch == 0 ? 0.25f : 0.22f),
                    TankKf51FamilyDetails.Dark());
            }
        }

        private static void AddSmokeLaunchers(
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
                    "Painted-KF51-SmokeBank",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.31f,
                        roof - 0.27f,
                        -0.15f),
                    new Vector3(0.08f, 0.24f, 0.54f),
                    color * 0.6f);
                for (int tube = 0;
                    tube < 4;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-KF51-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (width * 0.295f +
                                     tube * 0.035f),
                                roof - 0.2f +
                                    tube * 0.018f,
                                0.02f -
                                    tube * 0.1f),
                            new Vector3(0.04f, 0.15f, 0.04f),
                            color * 0.5f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            64f,
                            0f,
                            side * 20f);
                }
            }
        }

        private static void AddClosedChevron(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 3;
                    panel++)
                {
                    float x =
                        side *
                        (0.63f + panel * 0.3f);
                    float z =
                        1.82f - panel * 0.15f;
                    Transform upper =
                        TankDetailGeometry.Part(
                            "Painted-KF51-ChevronUpperPanel",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                x,
                                roof - 0.2f,
                                z),
                            new Vector3(
                                0.31f,
                                0.3f,
                                0.78f - panel * 0.07f),
                            color * (0.8f -
                                panel * 0.04f));
                    upper.localRotation =
                        Quaternion.Euler(
                            23f,
                            side * (10f + panel * 5f),
                            0f);
                    Transform lower =
                        TankDetailGeometry.Part(
                            "Painted-KF51-ChevronLowerPanel",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                x,
                                roof - 0.5f,
                                z),
                            new Vector3(
                                0.31f,
                                0.3f,
                                0.78f - panel * 0.07f),
                            color * (0.68f -
                                panel * 0.035f));
                    lower.localRotation =
                        Quaternion.Euler(
                            -23f,
                            side * (10f + panel * 5f),
                            0f);
                }
                TankDetailGeometry.Part(
                    "Painted-KF51-CheekCassette",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.31f,
                        roof - 0.28f,
                        1.98f),
                    new Vector3(0.3f, 0.24f, 0.12f),
                    color * 0.42f);
                TankDetailGeometry.Part(
                    "KF51-CheekMultispectralLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.31f,
                        roof - 0.28f,
                        2.048f),
                    new Vector3(0.17f, 0.1f, 0.014f),
                    TankKf51FamilyDetails.Lens());
            }
            TankDetailGeometry.Part(
                "Painted-KF51-Crown",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, roof + 0.07f, 0.08f),
                new Vector3(1.4f, 0.12f, 0.87f),
                color * 0.76f);
        }

        private static void AddConvexCrown(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            float[] widths =
            {
                0.95f,
                1.28f,
                1.55f,
                1.72f,
                1.52f
            };
            for (int course = 0;
                course < widths.Length;
                course++)
            {
                float z = 1.25f - course * 0.72f;
                float lift =
                    0.03f +
                    (2 - Mathf.Abs(course - 2)) *
                    0.025f;
                TankDetailGeometry.Part(
                    "Painted-KF51B-ConvexCrownCourse",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof + lift,
                        z),
                    new Vector3(
                        Mathf.Min(
                            widths[course],
                            width * 0.48f),
                        0.08f,
                        0.68f),
                    color * (0.72f +
                        course * 0.02f));
            }
        }

        private static void AddFlankPanels(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            float[] stations =
            {
                0.82f,
                0.42f,
                -0.04f,
                -0.54f,
                -1.05f,
                -1.56f,
                -2.03f
            };
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < stations.Length;
                    panel++)
                {
                    float taper =
                        1f -
                        panel * 0.022f;
                    TankDetailGeometry.Part(
                        "Painted-KF51B-FlankPanel",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side *
                                width * 0.425f *
                                taper,
                            roof - 0.3f,
                            stations[panel]),
                        new Vector3(
                            0.08f,
                            0.25f -
                                panel * 0.005f,
                            0.39f +
                                (panel % 3) * 0.035f),
                        color * (0.7f -
                            panel * 0.018f));
                    TankDetailGeometry.Part(
                        "KF51B-FlankPanelRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side *
                                (width * 0.425f *
                                 taper + 0.048f),
                            roof - 0.22f,
                            stations[panel]),
                        new Vector3(0.025f, 0.025f, 0.28f),
                        TankKf51FamilyDetails.Gunmetal());
                }
            }
        }

        private static void AddRaisedMultispectralSight(
            Transform turret,
            Color color,
            float roof)
        {
            TankDetailGeometry.Part(
                "Painted-KF51B-MultispectralSight",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -0.74f,
                    roof + 0.16f,
                    1.27f),
                new Vector3(0.44f, 0.34f, 0.18f),
                color * 0.58f)
                .localRotation =
                Quaternion.Euler(-11f, 0f, 0f);
            for (int aperture = -1;
                aperture <= 1;
                aperture += 2)
            {
                TankDetailGeometry.Part(
                    "KF51B-MultispectralAperture",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -0.74f +
                            aperture * 0.09f,
                        roof + 0.21f,
                        1.37f),
                    new Vector3(0.1f, 0.1f, 0.014f),
                    TankKf51FamilyDetails.Lens());
            }
        }

        private static void AddSeossTower(
            Transform turret,
            Color color,
            float roof,
            bool ownerExact)
        {
            float x = ownerExact ? -0.56f : -0.51f;
            float z = ownerExact ? -1.06f : -0.56f;
            TankDetailGeometry.Part(
                "Painted-KF51-SEOSS-Base",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, roof + 0.1f, z),
                new Vector3(0.26f, 0.1f, 0.26f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Painted-KF51-SEOSS-Tower",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + (ownerExact ? 0.34f : 0.38f),
                    z),
                new Vector3(
                    ownerExact ? 0.46f : 0.51f,
                    ownerExact ? 0.56f : 0.62f,
                    ownerExact ? 0.48f : 0.43f),
                color * 0.61f);
            for (int aperture = -1;
                aperture <= 1;
                aperture += 2)
            {
                TankDetailGeometry.Part(
                    "KF51-SEOSS-Aperture",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x + aperture * 0.1f,
                        roof +
                            (ownerExact ? 0.36f : 0.4f),
                        z +
                            (ownerExact ? 0.248f : 0.223f)),
                    new Vector3(0.09f, 0.12f, 0.014f),
                    TankKf51FamilyDetails.Lens());
            }
        }
    }
}
