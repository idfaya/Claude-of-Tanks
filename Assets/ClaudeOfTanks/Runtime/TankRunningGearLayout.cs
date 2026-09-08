using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankRunningGearLayout
    {
        public static int RoadWheelCount(
            VehicleDefinition definition,
            float hullLength)
        {
            if (definition?.id == "mbt70")
                return 7;

            return Mathf.Clamp(
                Mathf.RoundToInt(hullLength * 0.9f),
                4,
                8);
        }
    }
}
