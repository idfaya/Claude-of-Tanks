using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankWarriorRunningGearLayout
    {
        private static readonly float[] Stations =
        {
            2.0448f,
            1.2354f,
            0.426f,
            -0.426f,
            -1.2354f,
            -2.0448f
        };

        public static float RoadWheelZ(int index)
        {
            return Stations[index];
        }

        public static Vector2 Sprocket()
        {
            return new Vector2(2.6412f, 0.64f);
        }

        public static Vector2 Idler()
        {
            return new Vector2(-2.6093f, 0.62f);
        }
    }
}
