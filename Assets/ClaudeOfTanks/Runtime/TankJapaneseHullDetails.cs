using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankJapaneseHullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            TankJapanesePrimaryDetails.BuildHull(
                root,
                definition.id,
                color,
                width,
                height,
                length);
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    height * 0.58f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.45f);
            AddRearServiceField(
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

        private static void AddRearServiceField(
            Transform root,
            string id,
            Color color,
            float width,
            float roof,
            float rear)
        {
            int fields =
                id == "stb1" ||
                id == "type74"
                    ? 2
                    : 1;
            int slatsPerField =
                id == "stb1"
                    ? 5
                    : id == "type74"
                        ? 3
                        : id == "type90" ||
                          id == "type90a"
                            ? 6
                            : 5;
            for (int field = 0;
                field < fields;
                field++)
            {
                float x = fields == 1
                    ? 0f
                    : (field == 0 ? -1f : 1f) *
                        width * 0.2f;
                float fieldWidth = fields == 1
                    ? width * 0.56f
                    : width * 0.34f;
                TankDetailGeometry.Part(
                    "Painted-Japanese-RearServiceFrame",
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
                    "Japanese-RearServiceGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        x,
                        roof - 0.32f,
                        rear - 0.051f),
                    new Vector3(
                        fieldWidth * 0.88f,
                        0.32f,
                        0.018f),
                    Gunmetal());
                for (int slat = 0;
                    slat < slatsPerField;
                    slat++)
                {
                    float y =
                        roof - 0.46f +
                        slat *
                        (0.28f /
                         Mathf.Max(
                             1,
                             slatsPerField - 1));
                    TankDetailGeometry.Part(
                        "Japanese-RearServiceSlat",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            x,
                            y,
                            rear - 0.065f),
                        new Vector3(
                            fieldWidth * 0.82f,
                            0.018f,
                            0.016f),
                        color * 0.36f);
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
            bool cast =
                id == "stb1" ||
                id == "type74";
            float depth = cast ? 1.25f : 1.5f;
            float centerZ =
                rear + depth * 0.92f;
            TankDetailGeometry.Part(
                "Painted-Japanese-EngineDeck",
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
                slat < 5;
                slat++)
            {
                TankDetailGeometry.Part(
                    "Japanese-EngineDeckSlat",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0f,
                        roof + 0.044f,
                        centerZ -
                            depth * 0.34f +
                            slat * depth * 0.17f),
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
                    "Japanese-EngineFan",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.18f,
                        roof + 0.052f,
                        centerZ),
                    new Vector3(0.19f, 0.018f, 0.19f),
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
                    "Japanese-RearMarker",
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
                        "Japanese-RearTowClevis",
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
