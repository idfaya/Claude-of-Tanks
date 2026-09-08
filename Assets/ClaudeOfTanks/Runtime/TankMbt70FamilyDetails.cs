using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMbt70FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "mbt70";
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

            TankMbt70HullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            TankMbt70TurretDetails.Build(
                turret,
                definition,
                color,
                width);
            TankMbt70StowageDetails.Build(
                turret,
                definition,
                color,
                width);
            TankMbt70GunDetails.Build(
                turret,
                definition,
                color);
        }

        internal static Color Gunmetal()
        {
            return new Color(0.09f, 0.1f, 0.085f);
        }

        internal static Color Dark()
        {
            return new Color(0.12f, 0.13f, 0.105f);
        }

        internal static Color Lens()
        {
            return new Color(0.025f, 0.09f, 0.095f);
        }
    }
}
