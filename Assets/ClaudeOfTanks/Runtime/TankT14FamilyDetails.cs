using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT14FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "t14";
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

            TankT14HullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            TankT14TurretDetails.Build(
                turret,
                definition,
                color);
            TankT14RoofEquipment.Build(
                turret,
                definition,
                color);
            TankT14GunDetails.Build(
                turret,
                definition,
                color);
        }

        internal static Color Dark()
        {
            return new Color(0.075f, 0.085f, 0.07f);
        }

        internal static Color Gunmetal()
        {
            return new Color(0.11f, 0.12f, 0.1f);
        }

        internal static Color Lens()
        {
            return new Color(0.02f, 0.095f, 0.11f);
        }
    }
}
