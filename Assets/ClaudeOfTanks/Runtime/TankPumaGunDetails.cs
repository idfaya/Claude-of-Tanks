using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPumaGunDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "Puma-GunFittings");
            bool s1 =
                TankPumaFamilyDetails.IsS1(
                    definition);
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    s1 ? 2.025f : 3.3f);
            if (s1)
            {
                AddS1OpenCradle(
                    fittings,
                    color);
            }
            else
            {
                AddProductionCradle(
                    fittings,
                    color);
                AddProductionSpikePod(
                    fittings,
                    color);
            }
            AddMk30(
                fittings,
                color,
                length,
                s1);
        }

        private static void AddProductionCradle(
            Transform fittings,
            Color color)
        {
            AddAxialCylinder(
                "Painted-Puma-Mk30-CastCollar",
                fittings,
                0.11f,
                0.3f,
                0.3f,
                color * 0.64f);
            TankDetailGeometry.Part(
                "Painted-Puma-Mk30-CradleHousing",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, -0.02f, 0.18f),
                new Vector3(0.22f, 0.24f, 0.55f),
                color * 0.7f);
        }

        private static void AddS1OpenCradle(
            Transform fittings,
            Color color)
        {
            const float rearZ = 0.2f;
            const float frontZ = 1.432f;
            const float rearWidth = 0.245f;
            const float frontWidth = 0.168f;
            const float rearHeight = 0.154f;
            const float frontHeight = 0.098f;
            const float thickness = 0.032f;

            Transform top =
                TankDetailGeometry.Part(
                    "Painted-PumaS1-OpenCradleTop",
                    PrimitiveType.Cube,
                    fittings,
                    new Vector3(
                        0f,
                        0.126f,
                        (rearZ + frontZ) *
                        0.5f),
                    new Vector3(
                        rearWidth * 1.72f,
                        0.024f,
                        frontZ - rearZ),
                    color * 0.67f);
            top.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    0f);
            TankDetailGeometry.Part(
                "Painted-PumaS1-OpenCradleBottom",
                PrimitiveType.Cube,
                fittings,
                new Vector3(
                    0f,
                    -0.126f,
                    (rearZ + frontZ) *
                    0.5f),
                new Vector3(
                    rearWidth * 1.72f,
                    0.024f,
                    frontZ - rearZ),
                color * 0.62f);

            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int vertical = -1;
                    vertical <= 1;
                    vertical += 2)
                {
                    AddBeam(
                        "Painted-PumaS1-CradleCornerRail",
                        fittings,
                        new Vector3(
                            side * rearWidth,
                            vertical * rearHeight,
                            rearZ),
                        new Vector3(
                            side * frontWidth,
                            vertical * frontHeight,
                            frontZ),
                        thickness,
                        color * 0.72f);
                }
                for (int web = 0;
                    web < 4;
                    web++)
                {
                    float fraction =
                        0.16f +
                        web * 0.225f;
                    float z =
                        Mathf.Lerp(
                            rearZ,
                            frontZ,
                            fraction);
                    float width =
                        Mathf.Lerp(
                            rearWidth,
                            frontWidth,
                            fraction);
                    Transform brace =
                        TankDetailGeometry.Part(
                            "Painted-PumaS1-CradleSideWeb",
                            PrimitiveType.Cube,
                            fittings,
                            new Vector3(
                                side * width,
                                0f,
                                z),
                            new Vector3(
                                0.028f,
                                0.2f,
                                0.065f),
                            color * 0.68f);
                    brace.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            side * -18f);
                }
            }
            AddAxialCylinder(
                "PumaS1-DarkTrunnion",
                fittings,
                0.145f,
                0.46f,
                0.46f,
                TankPumaFamilyDetails.Dark());
        }

        private static void AddProductionSpikePod(
            Transform fittings,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Puma-SpikePod",
                PrimitiveType.Cube,
                fittings,
                new Vector3(1.3f, -0.085f, -0.19f),
                new Vector3(0.46f, 0.37f, 1.3f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Painted-Puma-SpikeElevationArm",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.96f, -0.1f, -0.3f),
                new Vector3(0.44f, 0.22f, 0.3f),
                color * 0.55f);
            for (int tube = 0;
                tube < 2;
                tube++)
            {
                AddAxialCylinder(
                    "Puma-SpikeTubeMouth",
                    fittings,
                    0.105f,
                    0.05f,
                    0.45f,
                    TankPumaFamilyDetails.Dark(),
                    1.19f +
                        tube * 0.22f,
                    -0.02f);
            }
            TankDetailGeometry.Part(
                "Puma-SpikeRearDoor",
                PrimitiveType.Cube,
                fittings,
                new Vector3(1.3f, -0.09f, -0.86f),
                new Vector3(0.42f, 0.3f, 0.05f),
                TankPumaFamilyDetails.Dark());
        }

        private static void AddMk30(
            Transform fittings,
            Color color,
            float length,
            bool s1)
        {
            float barrelStart =
                s1 ? 0.42f : 0.52f;
            float barrelLength =
                Mathf.Max(
                    0.5f,
                    length - barrelStart -
                    0.16f);
            AddAxialCylinder(
                "Painted-Puma-Mk30-Barrel",
                fittings,
                s1 ? 0.041f : 0.034f,
                barrelLength,
                barrelStart +
                    barrelLength * 0.5f,
                color * 0.54f);
            if (!s1)
            {
                AddAxialCylinder(
                    "Painted-Puma-Mk30-MuzzleBrake",
                    fittings,
                    0.048f,
                    0.14f,
                    length - 0.11f,
                    color * 0.44f);
                AddAxialCylinder(
                    "Puma-Mk30-BrakeBaffle",
                    fittings,
                    0.052f,
                    0.018f,
                    length - 0.15f,
                    TankPumaFamilyDetails.Dark());
                AddAxialCylinder(
                    "Puma-Mk30-BrakeBaffle",
                    fittings,
                    0.052f,
                    0.018f,
                    length - 0.07f,
                    TankPumaFamilyDetails.Dark());
            }
            else
            {
                AddAxialCylinder(
                    "Painted-PumaS1-Mk30-MuzzleJacket",
                    fittings,
                    0.068f,
                    0.18f,
                    length - 0.12f,
                    color * 0.48f);
            }
            AddAxialCylinder(
                "Puma-Mk30-MuzzleBore",
                fittings,
                s1 ? 0.024f : 0.02f,
                0.028f,
                length - 0.018f,
                Color.black);
            AddAxialCylinder(
                "Puma-Mg4-CoaxBarrel",
                fittings,
                0.018f,
                0.72f,
                0.74f,
                TankPumaFamilyDetails.Dark(),
                0.2f,
                -0.025f);
            TankDetailGeometry.Part(
                "Puma-Mg4-CoaxReceiver",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.2f, -0.025f, 0.28f),
                new Vector3(0.12f, 0.11f, 0.34f),
                TankPumaFamilyDetails.Gunmetal());
        }

        private static void AddBeam(
            string name,
            Transform parent,
            Vector3 start,
            Vector3 end,
            float thickness,
            Color color)
        {
            Vector3 direction = end - start;
            Transform beam =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    parent,
                    (start + end) * 0.5f,
                    new Vector3(
                        thickness,
                        thickness,
                        direction.magnitude),
                    color);
            beam.localRotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up);
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
