using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsM1FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "m1a1" || id == "m1a1ha";
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
            Hide(root.Find("Hull"));
            Hide(root.Find("UpperHull"));
            Hide(root.Find("Armor-track_L"));
            Hide(root.Find("Armor-track_R"));
            Hide(turret.Find("Turret"));
            Hide(turret.Find("Gun"));
            bool heavyArmor = definition.id == "m1a1ha";
            TankAbramsM1HullDetails.Build(root, color, heavyArmor);
            TankAbramsM1TurretDetails.Build(turret, color, heavyArmor);
            TankAbramsM1GunDetails.Build(
                turret,
                definition,
                color,
                heavyArmor);
        }

        private static void Hide(Transform part)
        {
            Renderer renderer =
                part == null
                    ? null
                    : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        public static Color Dark()
        {
            return new Color(0.045f, 0.05f, 0.038f);
        }

        public static Color Gunmetal()
        {
            return new Color(0.11f, 0.115f, 0.095f);
        }

        public static Color Glass()
        {
            return new Color(0.025f, 0.08f, 0.075f);
        }
    }
}
