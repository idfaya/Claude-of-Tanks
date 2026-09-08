using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPumaSideProtectionDetails
    {
        public static void Build(
            Transform root,
            Color color,
            float width,
            float length,
            float scale,
            bool s1)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Puma-FenderBridge",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.423f,
                        1.94f * scale,
                        -0.4f * scale),
                    new Vector3(
                        0.24f * scale,
                        0.14f * scale,
                        length * 0.78f),
                    color * 0.72f);
                if (s1)
                {
                    AddS1Jacket(
                        root,
                        color,
                        side,
                        scale);
                }
                else
                {
                    AddProductionModules(
                        root,
                        color,
                        side);
                }
            }
        }

        private static void AddProductionModules(
            Transform root,
            Color color,
            int side)
        {
            const float front = 2.2f;
            const float pitch = 0.49f;
            for (int module = 0;
                module < 11;
                module++)
            {
                float z = front - pitch * module;
                TankDetailGeometry.Part(
                    "Painted-Puma-UpperSideModule",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.73f,
                        1.79f,
                        z),
                    new Vector3(
                        0.14f,
                        0.42f,
                        0.47f),
                    color * (0.76f -
                        module % 2 * 0.035f));
                TankDetailGeometry.Part(
                    "Painted-Puma-LowerSideModule",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.79f,
                        1.3f,
                        z),
                    new Vector3(
                        0.17f,
                        0.6f,
                        0.47f),
                    color * (0.69f -
                        module % 2 * 0.025f));
                TankDetailGeometry.Part(
                    "Puma-SideModuleSeam",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.885f,
                        1.52f,
                        z - pitch * 0.5f),
                    new Vector3(
                        0.018f,
                        0.92f,
                        0.018f),
                    TankPumaFamilyDetails.Dark());
            }
            TankDetailGeometry.Part(
                "Painted-Puma-SternSideModule",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    side * 1.78f,
                    1.35f,
                    -3.1f),
                new Vector3(0.17f, 0.55f, 0.42f),
                color * 0.66f);
        }

        private static void AddS1Jacket(
            Transform root,
            Color color,
            int side,
            float scale)
        {
            const int panels = 8;
            float rear = -3.4f * scale;
            float front = 2.48f * scale;
            float panelLength =
                (front - rear) / panels;
            TankDetailGeometry.Part(
                "Painted-PumaS1-JacketCarrier",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    side * 1.82f * scale,
                    1.3f * scale,
                    (front + rear) * 0.5f),
                new Vector3(
                    0.2f * scale,
                    1.16f * scale,
                    front - rear),
                color * 0.64f);
            for (int panel = 0;
                panel < panels;
                panel++)
            {
                float z =
                    rear +
                    panelLength *
                    (panel + 0.5f);
                float height =
                    panel == panels - 1
                        ? 0.95f * scale
                        : 1.14f * scale;
                float y =
                    panel == panels - 1
                        ? 1.295f * scale
                        : 1.31f * scale;
                TankDetailGeometry.Part(
                    "Painted-PumaS1-AmapCassette",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 1.91f * scale,
                        y,
                        z),
                    new Vector3(
                        0.18f * scale,
                        height,
                        panelLength -
                            0.028f * scale),
                    color * (0.72f -
                        panel % 2 * 0.03f));
                TankDetailGeometry.Part(
                    "Painted-PumaS1-AmapLid",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * 2.0175f * scale,
                        y,
                        z),
                    new Vector3(
                        0.035f * scale,
                        height -
                            0.12f * scale,
                        panelLength -
                            0.1f * scale),
                    color * 0.62f);
                if (panel > 0)
                {
                    TankDetailGeometry.Part(
                        "PumaS1-AmapSeam",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * 2.04f * scale,
                            y,
                            rear +
                                panelLength *
                                panel),
                        new Vector3(
                            0.02f * scale,
                            height -
                                0.14f * scale,
                            0.02f * scale),
                        TankPumaFamilyDetails.Dark());
                }
            }
            TankDetailGeometry.Part(
                "PumaS1-JacketWaist",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    side * 2.04f * scale,
                    1.31f * scale,
                    (front + rear) * 0.5f),
                new Vector3(
                    0.028f * scale,
                    0.026f * scale,
                    front - rear -
                        0.1f * scale),
                TankPumaFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "PumaS1-RubberLip",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    side * 2.02f * scale,
                    0.72f * scale,
                    (front + rear) * 0.5f),
                new Vector3(
                    0.045f * scale,
                    0.16f * scale,
                    front - rear),
                TankPumaFamilyDetails.Dark());
        }
    }
}
