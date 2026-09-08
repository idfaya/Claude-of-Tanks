using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKf51FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "kf51" ||
                id == "kf51b";
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

            TankKf51HullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            TankKf51TurretDetails.Build(
                turret,
                definition,
                color,
                width);
            TankKf51RoofEquipment.Build(
                turret,
                definition,
                color,
                width);
            TankKf51GunDetails.Build(
                turret,
                definition,
                color);
        }

        internal static Color Gunmetal()
        {
            return new Color(0.075f, 0.085f, 0.07f);
        }

        internal static Color Dark()
        {
            return new Color(0.12f, 0.12f, 0.095f);
        }

        internal static Color Lens()
        {
            return new Color(0.035f, 0.12f, 0.13f);
        }
    }
}
