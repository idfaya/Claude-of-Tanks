using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKf51HullDetails
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
                height * 0.58f);
            AddLeopardHullFittings(
                root,
                definition,
                color,
                width,
                roof,
                length);
            AddClosedMudguards(
                root,
                definition.id,
                color,
                width,
                roof,
                length);
            TankKf51HullProtectionDetails.Build(
                root,
                definition.id,
                color,
                width,
                roof,
                length);

            if (definition.id == "kf51")
            {
                AddAwarenessPods(
                    root,
                    color,
                    width,
                    roof);
            }
            else
            {
                AddKf51BDeckKit(
                    root,
                    color,
                    width,
                    roof,
                    length);
            }
        }

        private static void AddLeopardHullFittings(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float roof,
            float length)
        {
            float rear = TankDetailGeometry.HullRearZ(
                definition,
                -length * 0.5f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-KF51-EngineGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.18f,
                        roof + 0.024f,
                        rear + length * 0.14f),
                    new Vector3(
                        width * 0.28f,
                        0.035f,
                        length * 0.17f),
                    color * 0.42f);
                TankDetailGeometry.Part(
                    "Painted-KF51-HeadlightHousing",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.22f,
                        roof - 0.19f,
                        length * 0.4f),
                    new Vector3(0.28f, 0.16f, 0.18f),
                    color * 0.68f);
                TankDetailGeometry.Part(
                    "KF51-HeadlightLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.22f,
                        roof - 0.19f,
                        length * 0.412f),
                    new Vector3(0.16f, 0.08f, 0.014f),
                    TankKf51FamilyDetails.Lens());
                for (int guard = -1;
                    guard <= 1;
                    guard++)
                {
                    TankDetailGeometry.Part(
                        "KF51-HeadlightGuard",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.22f +
                                guard * 0.085f,
                            roof - 0.18f,
                            length * 0.417f),
                        new Vector3(0.018f, 0.15f, 0.018f),
                        TankKf51FamilyDetails.Gunmetal());
                }
            }
            TankDetailGeometry.Part(
                "Painted-KF51-DriverHatch",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    width * 0.16f,
                    roof + 0.035f,
                    length * 0.17f),
                new Vector3(0.28f, 0.04f, 0.28f),
                color * 0.78f);
            for (int scope = -1;
                scope <= 1;
                scope++)
            {
                TankDetailGeometry.Part(
                    "KF51-DriverPeriscope",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        width * 0.16f +
                            scope * 0.18f,
                        roof + 0.075f,
                        length * 0.23f -
                            Mathf.Abs(scope) * 0.025f),
                    new Vector3(0.11f, 0.075f, 0.12f),
                    TankKf51FamilyDetails.Lens());
            }
        }

        private static void AddClosedMudguards(
            Transform root,
            string id,
            Color color,
            float width,
            float roof,
            float length)
        {
            bool ownerExact = id == "kf51b";
            float outerX =
                width * (ownerExact ? 0.52f : 0.49f);
            float shoulderZ = length * 0.405f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform shoulder =
                    TankDetailGeometry.Part(
                        "Painted-KF51-ClosedMudguardShoulder",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * outerX,
                            roof - 0.22f,
                            shoulderZ),
                        new Vector3(
                            ownerExact ? 0.38f : 0.3f,
                            0.1f,
                            0.86f),
                        color * 0.75f);
                shoulder.localRotation =
                    Quaternion.Euler(-6f, 0f, 0f);
                Transform web =
                    TankDetailGeometry.Part(
                        "Painted-KF51-ClosedMudguardWeb",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side *
                                (outerX + 0.035f),
                            roof - 0.48f,
                            shoulderZ + 0.22f),
                        new Vector3(
                            ownerExact ? 0.13f : 0.09f,
                            0.4f,
                            0.74f),
                        color * 0.65f);
                web.localRotation =
                    Quaternion.Euler(-13f, 0f, 0f);
            }
        }

        private static void AddAwarenessPods(
            Transform root,
            Color color,
            float width,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int pod = 0;
                    pod < 2;
                    pod++)
                {
                    float z =
                        pod == 0 ? -1.74f : 1.52f;
                    TankDetailGeometry.Part(
                        "Painted-KF51-AwarenessPod",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.482f,
                            roof - 0.06f,
                            z),
                        new Vector3(0.08f, 0.2f, 0.25f),
                        color * 0.62f);
                    TankDetailGeometry.Part(
                        "KF51-AwarenessLens",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.496f,
                            roof - 0.05f,
                            z + 0.015f),
                        new Vector3(0.014f, 0.1f, 0.12f),
                        TankKf51FamilyDetails.Lens());
                }
                for (int aperture = -1;
                    aperture <= 1;
                    aperture++)
                {
                    TankDetailGeometry.Part(
                        "KF51-ShoulderAperture",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.39f +
                                aperture * 0.07f,
                            roof - 0.08f,
                            2.95f),
                        new Vector3(0.03f, 0.02f, 0.03f),
                        TankKf51FamilyDetails.Lens())
                        .localRotation =
                        Quaternion.Euler(90f, 0f, 0f);
                }
            }
        }

        private static void AddKf51BDeckKit(
            Transform root,
            Color color,
            float width,
            float roof,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-KF51B-FenderBin",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.43f,
                        roof + 0.055f,
                        length * 0.23f),
                    new Vector3(0.28f, 0.13f, 0.88f),
                    color * 0.68f);
                TankDetailGeometry.Part(
                    "Painted-KF51B-AftDeckBin",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.35f,
                        roof + 0.1f,
                        -length * 0.42f),
                    new Vector3(0.32f, 0.16f, 0.62f),
                    color * 0.62f);
                TankDetailGeometry.Part(
                    "KF51B-WidthIndicator",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.46f,
                        roof + 0.02f,
                        length * 0.445f),
                    new Vector3(0.008f, 0.3f, 0.008f),
                    TankKf51FamilyDetails.Gunmetal());
            }
        }
    }
}
