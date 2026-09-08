using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankFrenchHullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    height * 0.58f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.45f);
            AddRearService(
                root,
                definition.id,
                color,
                width,
                roof,
                rear);
            AddEngineDeck(
                root,
                definition.id,
                color,
                width,
                roof,
                rear);
            AddRearHardware(
                root,
                color,
                width,
                roof,
                rear);
        }

        private static void AddRearService(
            Transform root,
            string id,
            Color color,
            float width,
            float roof,
            float rear)
        {
            bool amx30 =
                id == "amx30" ||
                id == "amx30b2";
            int fields = amx30 ? 2 : 3;
            int slats = amx30 ? 4 : 5;
            for (int field = 0;
                field < fields;
                field++)
            {
                float x =
                    (field - (fields - 1) * 0.5f) *
                    width * 0.24f;
                float fieldWidth =
                    width *
                    (amx30 ? 0.3f : 0.23f);
                TankDetailGeometry.Part(
                    "Painted-French-RearServiceFrame",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        x,
                        roof - 0.32f,
                        rear - 0.022f),
                    new Vector3(
                        fieldWidth,
                        0.42f,
                        0.05f),
                    color * 0.66f);
                TankDetailGeometry.Part(
                    "French-RearServiceGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        x,
                        roof - 0.32f,
                        rear - 0.052f),
                    new Vector3(
                        fieldWidth * 0.88f,
                        0.32f,
                        0.018f),
                    Gunmetal());
                for (int slat = 0;
                    slat < slats;
                    slat++)
                {
                    TankDetailGeometry.Part(
                        "French-RearServiceSlat",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            x,
                            roof - 0.45f +
                                slat *
                                (0.26f /
                                 Mathf.Max(
                                     1,
                                     slats - 1)),
                            rear - 0.065f),
                        new Vector3(
                            fieldWidth * 0.82f,
                            0.018f,
                            0.016f),
                        color * 0.35f);
                }
            }
        }

        private static void AddEngineDeck(
            Transform root,
            string id,
            Color color,
            float width,
            float roof,
            float rear)
        {
            bool amx30 =
                id == "amx30" ||
                id == "amx30b2";
            float depth = amx30 ? 1.25f : 1.5f;
            float centerZ =
                rear + depth * 0.94f;
            TankDetailGeometry.Part(
                "Painted-French-EngineDeck",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    roof + 0.018f,
                    centerZ),
                new Vector3(
                    width * 0.62f,
                    0.035f,
                    depth),
                color * 0.74f);
            for (int slat = 0;
                slat < 6;
                slat++)
            {
                TankDetailGeometry.Part(
                    "French-EngineDeckSlat",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0f,
                        roof + 0.044f,
                        centerZ -
                            depth * 0.35f +
                            slat * depth * 0.14f),
                    new Vector3(
                        width * 0.52f,
                        0.018f,
                        0.045f),
                    Gunmetal());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "French-EngineFan",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.18f,
                        roof + 0.052f,
                        centerZ),
                    new Vector3(0.2f, 0.018f, 0.2f),
                    Gunmetal());
            }
        }

        private static void AddRearHardware(
            Transform root,
            Color color,
            float width,
            float roof,
            float rear)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "French-RearMarker",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.34f,
                        roof - 0.24f,
                        rear - 0.07f),
                    new Vector3(0.14f, 0.09f, 0.03f),
                    new Color(0.5f, 0.04f, 0.025f));
                Transform clevis =
                    TankDetailGeometry.Part(
                        "French-RearTowClevis",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.22f,
                            roof - 0.58f,
                            rear - 0.08f),
                        new Vector3(0.08f, 0.035f, 0.08f),
                        color * 0.4f);
                clevis.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }
    }
}
