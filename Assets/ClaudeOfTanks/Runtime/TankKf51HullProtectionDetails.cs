using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKf51HullProtectionDetails
    {
        public static void Build(
            Transform root,
            string id,
            Color color,
            float width,
            float roof,
            float length)
        {
            AddSkirtCarriers(
                root,
                id,
                color,
                width,
                roof,
                length);
            if (id == "kf51")
            {
                AddDemonstratorCage(
                    root,
                    color,
                    width,
                    roof,
                    length);
            }
            else
            {
                AddServiceGrilles(
                    root,
                    color,
                    width,
                    roof,
                    length);
            }
        }

        private static void AddSkirtCarriers(
            Transform root,
            string id,
            Color color,
            float width,
            float roof,
            float length)
        {
            bool ownerExact = id == "kf51b";
            float outerX =
                width * (ownerExact ? 0.515f : 0.485f);
            float courseLength =
                length * (ownerExact ? 0.73f : 0.67f);
            float centerZ =
                ownerExact ? -0.2f : -0.14f;
            float pitch = courseLength / 7f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-KF51-SkirtUpperBearer",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * (outerX - 0.11f),
                        roof - 0.1f,
                        centerZ),
                    new Vector3(
                        0.22f,
                        0.045f,
                        courseLength),
                    color * 0.68f);
                TankDetailGeometry.Part(
                    "KF51-SkirtRecessedCarrier",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * (outerX - 0.055f),
                        roof - 0.5f,
                        centerZ),
                    new Vector3(
                        0.07f,
                        ownerExact ? 0.86f : 0.8f,
                        courseLength - 0.04f),
                    TankKf51FamilyDetails.Dark());
                for (int station = 0;
                    station < 7;
                    station++)
                {
                    float z =
                        centerZ -
                        courseLength * 0.5f +
                        pitch * (station + 0.5f);
                    TankDetailGeometry.Part(
                        "KF51-SkirtCarrierFoot",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * (outerX - 0.035f),
                            roof - 0.34f,
                            z),
                        new Vector3(0.13f, 0.12f, 0.06f),
                        TankKf51FamilyDetails.Gunmetal());
                }
                for (int joint = 1;
                    joint < 7;
                    joint++)
                {
                    TankDetailGeometry.Part(
                        "KF51-SkirtStationJoint",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * outerX,
                            roof - 0.49f,
                            centerZ -
                                courseLength * 0.5f +
                                pitch * joint),
                        new Vector3(0.025f, 0.7f, 0.025f),
                        TankKf51FamilyDetails.Dark());
                }
            }
        }

        private static void AddDemonstratorCage(
            Transform root,
            Color color,
            float width,
            float roof,
            float length)
        {
            float outerX = width * 0.502f;
            float z0 = -length * 0.434f;
            float z1 = length * 0.335f;
            float cageLength = z1 - z0;
            float pitch = cageLength / 8f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int rail = 0;
                    rail < 3;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "KF51-DemonstratorCageRail",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * outerX,
                            roof - 0.72f +
                                rail * 0.31f,
                            (z0 + z1) * 0.5f),
                        new Vector3(
                            0.018f,
                            0.018f,
                            cageLength),
                        color * 0.36f);
                }
                for (int station = 0;
                    station <= 8;
                    station++)
                {
                    float z = z0 + station * pitch;
                    TankDetailGeometry.Part(
                        "KF51-DemonstratorCagePost",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * outerX,
                            roof - 0.41f,
                            z),
                        new Vector3(0.022f, 0.64f, 0.022f),
                        color * 0.38f);
                    if (station == 8) continue;
                    Transform brace =
                        TankDetailGeometry.Part(
                            "KF51-DemonstratorCageBrace",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                side * outerX,
                                roof - 0.41f,
                                z + pitch * 0.5f),
                            new Vector3(
                                0.02f,
                                0.02f,
                                Mathf.Sqrt(
                                    pitch * pitch +
                                    0.58f * 0.58f)),
                            color * 0.34f);
                    float angle =
                        Mathf.Atan2(0.58f, pitch) *
                        Mathf.Rad2Deg;
                    brace.localRotation =
                        Quaternion.Euler(
                            station % 2 == 0
                                ? angle
                                : -angle,
                            0f,
                            0f);
                }
            }
        }

        private static void AddServiceGrilles(
            Transform root,
            Color color,
            float width,
            float roof,
            float length)
        {
            float courseLength = length * 0.685f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int rail = 0;
                    rail < 4;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "KF51B-ServiceGrilleRail",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.533f,
                            roof - 0.24f +
                                rail * 0.052f,
                            -0.26f),
                        new Vector3(
                            0.018f,
                            0.018f,
                            courseLength),
                        color * 0.36f);
                }
                for (int post = 0;
                    post < 6;
                    post++)
                {
                    TankDetailGeometry.Part(
                        "KF51B-ServiceGrillePost",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.533f,
                            roof - 0.16f,
                            -courseLength * 0.5f +
                                post * courseLength / 5f -
                                0.26f),
                        new Vector3(0.022f, 0.2f, 0.022f),
                        color * 0.38f);
                }
            }
        }
    }
}
