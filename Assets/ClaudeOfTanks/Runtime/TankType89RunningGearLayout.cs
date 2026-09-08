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

        private static readonly float[] LightTigerStations =
        {
            2.025f,
            1.278f,
            0.513f,
            -0.27f,
            -1.044f,
            -1.8f
        };

        public static float RoadWheelZ(
            string id,
            int index)
        {
            return id == "type89_light_tiger"
                ? LightTigerStations[index]
                : Stations[index];
        }

        public static Vector2 Sprocket(string id)
        {
            return id == "type89_light_tiger"
                ? new Vector2(2.7f, 0.756f)
                : new Vector2(2.62f, 0.6f);
        }

        public static Vector2 Idler(string id)
        {
            return id == "type89_light_tiger"
                ? new Vector2(-2.664f, 0.693f)
                : new Vector2(-2.62f, 0.7f);
        }
    }
}
