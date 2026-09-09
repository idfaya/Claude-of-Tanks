using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT62Obr1975RunningGearDetails
    {
        private static readonly float[] WheelStations =
        {
            2.235f,
            1.297f,
            0.293f,
            -0.791f,
            -1.933f
        };

        public static void Build(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int wheel = 0;
                    wheel < WheelStations.Length;
                    wheel++)
                {
                    AddWheelFace(
                        root,
                        color,
                        side,
                        WheelStations[wheel]);
                }
            }
        }

        private static void AddWheelFace(
            Transform root,
            Color color,
            int side,
            float z)
        {
            Vector3 center = new Vector3(
                side * 1.625f,
                0.455f,
                z);
            Transform inset =
                TankDetailGeometry.Part(
                    "T62-RoadWheelInset",
                    PrimitiveType.Cylinder,
                    root,
                    center,
                    new Vector3(
                        0.31f,
                        0.018f,
                        0.31f),
                    TankT62Obr1975FamilyDetails.Dark());
            inset.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
            for (int spoke = 0;
                spoke < 6;
                spoke++)
            {
                Transform arm =
                    TankDetailGeometry.Part(
                        "Painted-T62-RoadWheelSpoke",
                        PrimitiveType.Cube,
                        root,
                        center,
                        new Vector3(
                            0.025f,
                            0.105f,
                            0.56f),
                        color * 0.58f);
                arm.localRotation =
                    Quaternion.Euler(
                        spoke * 30f,
                        0f,
                        0f);
            }
        }
    }
}
