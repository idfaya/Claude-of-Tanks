using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT72B3MRunningGearDetails
    {
        private static readonly float[] WheelStations =
        {
            -2.9f, -2.238f, -1.456f,
            -0.674f, 0.108f, 0.89f
        };

        private static readonly float[] RollerStations =
        {
            -2.5f, -1.1f, 0.4f
        };

        public static void Build(
            Transform root,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float outerX =
                    side * 1.56f;
                for (int wheel = 0;
                    wheel < WheelStations.Length;
                    wheel++)
                {
                    Vector3 center =
                        new Vector3(
                            outerX,
                            0.45f,
                            WheelStations[wheel]);
                    Transform disc = Part(
                        "T72B3M-RoadWheelInset",
                        PrimitiveType.Cylinder,
                        root,
                        center,
                        new Vector3(0.31f, 0.04f, 0.31f),
                        TankT72B3MFamilyDetails.Dark());
                    disc.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                    for (int spoke = 0;
                        spoke < 6;
                        spoke++)
                    {
                        Transform arm = Part(
                            "Painted-T72B3M-RoadWheelSpoke",
                            PrimitiveType.Cube,
                            root,
                            center,
                            new Vector3(0.035f, 0.07f, 0.43f),
                            color * 0.56f);
                        arm.localRotation =
                            Quaternion.Euler(
                                spoke * 30f,
                                0f,
                                0f);
                    }
                }

                for (int roller = 0;
                    roller < RollerStations.Length;
                    roller++)
                {
                    Transform part = Part(
                        "T72B3M-ReturnRoller",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * 1.46f,
                            0.82f,
                            RollerStations[roller]),
                        new Vector3(0.115f, 0.035f, 0.115f),
                        color * 0.48f);
                    part.localRotation =
                        Quaternion.Euler(0f, 0f, 90f);
                }
            }
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                type,
                parent,
                position,
                scale,
                color);
        }
    }
}
