using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLeopard1A5HullDetails
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
                height * 0.69f);
            AddFenderCourse(
                root,
                color,
                width,
                length);
            AddDriverStation(
                root,
                color,
                width);
            AddEngineDeck(
                root,
                color,
                roof);
            AddBowEquipment(
                root,
                color,
                width);
            AddRearServiceFace(
                root,
                definition,
                color,
                width,
                length);
        }

        private static void AddFenderCourse(
            Transform root,
            Color color,
            float width,
            float length)
        {
            const float shelfY = 1.295f;
            const float outerX = 1.685f;
            const float apronX = 1.706f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-ContinuousFender",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * (width * 0.42f),
                        shelfY,
                        -0.02f),
                    new Vector3(
                        0.55f,
                        0.045f,
                        length * 0.956f),
                    color * 0.78f);
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-FenderFascia",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * outerX,
                        1.195f,
                        -0.1f),
                    new Vector3(
                        0.012f,
                        0.18f,
                        6.04f),
                    color * 0.66f);
                for (int segment = 0;
                    segment < 7;
                    segment++)
                {
                    float z = 2.4f - segment * 0.82f;
                    TankDetailGeometry.Part(
                        "Leopard1A5-RubberApron",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * apronX,
                            0.995f,
                            z),
                        new Vector3(0.014f, 0.31f, 0.76f),
                        TankLeopard1A5FamilyDetails.Rubber());
                    TankDetailGeometry.Part(
                        "Leopard1A5-ApronJoint",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.707f,
                            1f,
                            z - 0.395f),
                        new Vector3(0.012f, 0.34f, 0.026f),
                        TankLeopard1A5FamilyDetails.Gunmetal());
                    TankDetailGeometry.Part(
                        "Painted-Leopard1A5-FenderBrace",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.47f,
                            1.345f,
                            z),
                        new Vector3(0.36f, 0.035f, 0.055f),
                        color * 0.62f);
                }
                for (int locker = 0;
                    locker < 4;
                    locker++)
                {
                    float z = -2.58f + locker * 0.83f;
                    TankDetailGeometry.Part(
                        "Painted-Leopard1A5-FenderLocker",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.43f,
                            1.395f,
                            z),
                        new Vector3(0.42f, 0.16f, 0.72f),
                        color * 0.7f);
                    TankDetailGeometry.Part(
                        "Painted-Leopard1A5-LockerLid",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.43f,
                            1.487f,
                            z),
                        new Vector3(0.44f, 0.025f, 0.74f),
                        color * 0.8f);
                    TankDetailGeometry.Part(
                        "Leopard1A5-LockerLatch",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 1.655f,
                            1.395f,
                            z + 0.22f),
                        new Vector3(0.022f, 0.07f, 0.11f),
                        TankLeopard1A5FamilyDetails.Gunmetal());
                }
            }
        }

        private static void AddDriverStation(
            Transform root,
            Color color,
            float width)
        {
            Transform hatch =
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-DriverHatch",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(-0.45f, 1.67f, 0.86f),
                    new Vector3(0.68f, 0.08f, 0.42f),
                    color * 0.79f);
            hatch.localRotation =
                Quaternion.Euler(-2.3f, 0f, 0f);
            for (int scope = -1;
                scope <= 1;
                scope++)
            {
                float x = -0.45f + scope * 0.21f;
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-DriverPeriscopeHousing",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(x, 1.705f, 1.03f),
                    new Vector3(0.13f, 0.07f, 0.05f),
                    color * 0.58f);
                TankDetailGeometry.Part(
                    "Leopard1A5-DriverPeriscopeLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(x, 1.743f, 1.061f),
                    new Vector3(0.09f, 0.035f, 0.014f),
                    TankLeopard1A5FamilyDetails.Lens());
            }
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-ServiceHatch",
                PrimitiveType.Cylinder,
                root,
                new Vector3(0.63f, 1.69f, 0.7f),
                new Vector3(0.18f, 0.025f, 0.18f),
                color * 0.74f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-HeadlightHousing",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.39f,
                        1.3f,
                        3.27f),
                    new Vector3(0.2f, 0.18f, 0.16f),
                    color * 0.65f)
                    .localRotation =
                    Quaternion.Euler(-15f, 0f, 0f);
                TankDetailGeometry.Part(
                    "Leopard1A5-HeadlightLens",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.39f,
                        1.31f,
                        3.36f),
                    new Vector3(0.11f, 0.085f, 0.014f),
                    TankLeopard1A5FamilyDetails.Lens());
                TankDetailGeometry.Part(
                    "Leopard1A5-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.39f,
                        1.35f,
                        3.39f),
                    new Vector3(0.24f, 0.025f, 0.025f),
                    TankLeopard1A5FamilyDetails.Gunmetal());
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Leopard1A5-EngineIntake",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 0.52f,
                        roof + 0.014f,
                        -1.72f),
                    new Vector3(0.82f, 0.018f, 1.04f),
                    TankLeopard1A5FamilyDetails.Rubber());
                for (int louvre = 0;
                    louvre < 6;
                    louvre++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Leopard1A5-IntakeLouvre",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 0.52f,
                            roof + 0.023f,
                            -1.29f - louvre * 0.17f),
                        new Vector3(0.78f, 0.014f, 0.035f),
                        color * 0.5f);
                }
                TankDetailGeometry.Part(
                    "Leopard1A5-EngineFan",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * 0.55f,
                        roof + 0.025f,
                        -2.5f),
                    new Vector3(0.23f, 0.012f, 0.23f),
                    TankLeopard1A5FamilyDetails.Rubber());
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-FanHub",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * 0.55f,
                        roof + 0.04f,
                        -2.5f),
                    new Vector3(0.055f, 0.022f, 0.055f),
                    color * 0.55f);
            }
            TankDetailGeometry.Part(
                "Leopard1A5-RearExhaustBank",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, roof + 0.014f, -3f),
                new Vector3(1.78f, 0.018f, 0.44f),
                TankLeopard1A5FamilyDetails.Rubber());
            for (int louvre = 0;
                louvre < 4;
                louvre++)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-RearLouvre",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0f,
                        roof + 0.027f,
                        -2.84f - louvre * 0.11f),
                    new Vector3(1.7f, 0.014f, 0.045f),
                    color * 0.47f);
            }
        }

        private static void AddBowEquipment(
            Transform root,
            Color color,
            float width)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-GlacisLiftEye",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.35f,
                        1.5f,
                        1.65f),
                    new Vector3(0.045f, 0.11f, 0.045f),
                    color * 0.55f);
            }
            for (int link = 0;
                link < 7;
                link++)
            {
                TankDetailGeometry.Part(
                    "Leopard1A5-SpareTrackLink",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -0.72f + link * 0.24f,
                        0.69f,
                        3.38f),
                    new Vector3(0.2f, 0.14f, 0.06f),
                    TankLeopard1A5FamilyDetails.Gunmetal());
            }
        }

        private static void AddRearServiceFace(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            float rear = TankDetailGeometry.HullRearZ(
                definition,
                -length * 0.5f);
            TankDetailGeometry.Part(
                "Leopard1A5-RearServiceFace",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, 1.34f, rear + 0.02f),
                new Vector3(2.28f, 0.46f, 0.035f),
                TankLeopard1A5FamilyDetails.Rubber());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float canX = side * 0.72f;
                TankDetailGeometry.Part(
                    "Painted-Leopard1A5-RearFuelCan",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(canX, 1.44f, rear + 0.07f),
                    new Vector3(0.56f, 0.66f, 0.24f),
                    color * 0.7f);
                for (int rib = -1;
                    rib <= 1;
                    rib += 2)
                {
                    Transform brace =
                        TankDetailGeometry.Part(
                            "Leopard1A5-FuelCanRib",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                canX,
                                1.44f,
                                rear - 0.06f),
                            new Vector3(0.035f, 0.5f, 0.02f),
                            TankLeopard1A5FamilyDetails.Gunmetal());
                    brace.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            rib * 38f);
                }
                for (int edge = -1;
                    edge <= 1;
                    edge += 2)
                {
                    TankDetailGeometry.Part(
                        "Leopard1A5-FuelCanCarrier",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            canX + edge * 0.3f,
                            1.44f,
                            rear + 0.07f),
                        new Vector3(0.035f, 0.75f, 0.28f),
                        TankLeopard1A5FamilyDetails.Gunmetal());
                }
                TankDetailGeometry.Part(
                    "Leopard1A5-RearMarker",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.4f,
                        1.49f,
                        rear - 0.01f),
                    new Vector3(0.1f, 0.08f, 0.018f),
                    new Color(0.45f, 0.05f, 0.025f));
            }
        }
    }
}
