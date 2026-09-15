using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKoreanHullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            AddPrimaryHull(
                root,
                definition,
                color,
                width,
                height,
                length);
            AddRearServiceField(
                root,
                definition,
                color,
                width,
                height,
                length);
            AddEngineDeck(
                root,
                definition,
                color,
                width,
                length);
        }

        private static void AddPrimaryHull(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            bool k1 = definition.id == "k1a1";
            string prefix = k1 ? "Korean-K1" : "Korean-K2";
            float rear = -length * 0.49f;
            float front = length * 0.51f;
            float half = width * 0.47f;
            float belly = height * 0.18f;
            float deck = height * (k1 ? 0.57f : 0.61f);
            TankHullLoftShapeFactory.Build(
                "Painted-" + prefix + "-HullLoft",
                root,
                Curve(
                    rear, deck - 0.18f,
                    rear + length * 0.10f, deck,
                    -length * 0.08f, deck,
                    length * 0.20f, deck - 0.08f,
                    front - length * 0.14f, deck - 0.30f,
                    front, deck - 0.72f),
                Curve(
                    rear, belly + 0.18f,
                    rear + length * 0.10f, belly,
                    front - length * 0.14f, belly,
                    front, belly + 0.17f),
                Curve(
                    rear, half * 0.68f,
                    rear + length * 0.12f, half,
                    front - length * 0.14f, half,
                    front, half * 0.56f),
                Curve(
                    rear, half * 0.48f,
                    rear + length * 0.12f, half * 0.72f,
                    front - length * 0.14f, half * 0.70f,
                    front, half * 0.44f),
                Curve(
                    rear, deck - 0.36f,
                    rear + length * 0.12f, deck - 0.22f,
                    front - length * 0.14f, deck - 0.31f,
                    front, deck - 0.57f),
                color * 0.66f);
        }

        private static TankHullProfilePoint[] Curve(
            params float[] values)
        {
            TankHullProfilePoint[] curve =
                new TankHullProfilePoint[values.Length / 2];
            for (int index = 0; index < curve.Length; index++)
            {
                curve[index] =
                    new TankHullProfilePoint(
                        values[index * 2],
                        values[index * 2 + 1]);
            }
            return curve;
        }

        private static void AddRearServiceField(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            bool k1 = definition.id == "k1a1";
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    height * 0.62f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f);
            int bayCount = k1 ? 2 : 3;
            int[] slatCounts = k1
                ? new[] { 4, 4 }
                : new[] { 4, 6, 3 };
            for (int bay = 0;
                bay < bayCount;
                bay++)
            {
                float x = k1
                    ? (bay == 0 ? -1f : 1f) *
                        width * 0.245f
                    : (bay - 1) * width * 0.2f;
                float bayWidth = k1
                    ? width * 0.37f
                    : width * (bay == 1 ? 0.32f : 0.25f);
                TankDetailGeometry.Part(
                    "Korean-RearServiceGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        x,
                        roof - 0.28f,
                        rear - 0.025f),
                    new Vector3(
                        bayWidth,
                        k1 ? 0.34f : 0.3f,
                        0.025f),
                    color * 0.32f);
                for (int slat = 0;
                    slat < slatCounts[bay];
                    slat++)
                {
                    float fraction =
                        slatCounts[bay] == 1
                            ? 0.5f
                            : slat /
                                (float)(slatCounts[bay] - 1);
                    TankDetailGeometry.Part(
                        "Korean-RearServiceSlat",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            x,
                            roof - 0.39f +
                                fraction * 0.22f,
                            rear - 0.042f),
                        new Vector3(
                            bayWidth * 0.9f,
                            0.018f,
                            0.018f),
                        Gunmetal());
                }
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Korean-RearMarker",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.39f,
                        roof - 0.12f,
                        rear - 0.05f),
                    new Vector3(0.13f, 0.07f, 0.025f),
                    new Color(0.55f, 0.06f, 0.025f));
                TankDetailGeometry.Part(
                    "Korean-RearTowClevis",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.18f,
                        roof - 0.72f,
                        rear - 0.06f),
                    new Vector3(0.2f, 0.12f, 0.08f),
                    Gunmetal());
            }
        }

        private static void AddEngineDeck(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    definition.dims.heightM * 0.62f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f);
            float centerZ = rear + length * 0.14f;
            TankDetailGeometry.Part(
                "Korean-EngineDeck",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, roof + 0.012f, centerZ),
                new Vector3(
                    width * 0.65f,
                    0.022f,
                    length * 0.17f),
                color * 0.34f);
            for (int slat = 0;
                slat < 5;
                slat++)
            {
                TankDetailGeometry.Part(
                    "Korean-EngineDeckSlat",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0f,
                        roof + 0.027f,
                        centerZ -
                            length * 0.06f +
                            slat * length * 0.03f),
                    new Vector3(
                        width * 0.61f,
                        0.015f,
                        0.04f),
                    Gunmetal());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Korean-EngineFan",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.22f,
                        roof + 0.035f,
                        rear + length * 0.24f),
                    new Vector3(0.25f, 0.018f, 0.25f),
                    Gunmetal());
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }
    }
}
