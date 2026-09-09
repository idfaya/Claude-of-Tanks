using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT64BV1RunningGearDetails
    {
        private static readonly float[] WheelStations =
        {
            1.875f, 1.125f, 0.4f,
            -0.325f, -1.075f, -1.775f
        };

        private static readonly float[] RollerStations =
        {
            -1.85f, -0.6f, 0.7f, 1.95f
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
                    side * (1.28f + 0.24f);
                for (int wheel = 0;
                    wheel < WheelStations.Length;
                    wheel++)
                {
                    Vector3 center =
                        new Vector3(
                            outerX,
                            0.49f,
                            WheelStations[wheel]);
                    Part(
                        "T64-RoadWheelInset",
                        PrimitiveType.Cylinder,
                        root,
                        center,
                        new Vector3(
                            0.235f,
                            0.035f,
                            0.235f),
                        TankT64BV1FamilyDetails.Dark())
                        .localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            90f);
                    for (int spoke = 0;
                        spoke < 8;
                        spoke++)
                    {
                        Transform arm = Part(
                            "Painted-T64-RoadWheelSpoke",
                            PrimitiveType.Cube,
                            root,
                            center,
                            new Vector3(
                                0.028f,
                                0.065f,
                                0.34f),
                            color * 0.55f);
                        arm.localRotation =
                            Quaternion.Euler(
                                spoke * 22.5f,
                                0f,
                                0f);
                    }
                }

                for (int roller = 0;
                    roller < RollerStations.Length;
                    roller++)
                {
                    Transform part = Part(
                        "T64-ReturnRoller",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * 1.28f,
                            0.98f,
                            RollerStations[roller]),
                        new Vector3(
                            0.078f,
                            0.22f,
                            0.078f),
                        TankT64BV1FamilyDetails.Dark());
                    part.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            90f);
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
