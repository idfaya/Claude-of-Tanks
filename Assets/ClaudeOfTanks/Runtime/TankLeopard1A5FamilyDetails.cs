using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLeopard1A5FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "leo1a5";
        }

        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            if (!Supports(definition?.id)) return;

            TankLeopard1A5HullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            TankLeopard1A5TurretDetails.Build(
                turret,
                definition,
                color,
                width);
            TankLeopard1A5GunDetails.Build(
                turret,
                definition,
                color);
        }

        internal static Color Gunmetal()
        {
            return new Color(0.105f, 0.115f, 0.095f);
        }

        internal static Color Rubber()
        {
            return new Color(0.13f, 0.14f, 0.115f);
        }

        internal static Color Lens()
        {
            return new Color(0.035f, 0.085f, 0.085f);
        }
    }
}
