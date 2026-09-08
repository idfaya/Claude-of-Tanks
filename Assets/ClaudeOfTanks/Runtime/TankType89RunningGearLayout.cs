using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankType89RunningGearLayout
    {
        private static readonly float[] Stations =
        {
            2.05f,
            1.23f,
            0.41f,
            -0.41f,
            -1.23f,
            -2.05f
        };

        public static float RoadWheelZ(int index)
        {
            return Stations[index];
        }

        public static Vector2 Sprocket()
        {
            return new Vector2(2.62f, 0.6f);
        }

        public static Vector2 Idler()
        {
            return new Vector2(-2.62f, 0.7f);
        }
    }
}
