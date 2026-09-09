using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankM1A3FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "m1a3";
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
            TankM1A3HullDetails.Build(root, color);
            TankM1A3TurretDetails.Build(turret, color);
            TankM1A3GunDetails.Build(
                turret,
                definition,
                color);
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
            return new Color(0.045f, 0.052f, 0.043f);
        }

        public static Color Gunmetal()
        {
            return new Color(0.095f, 0.105f, 0.09f);
        }

        public static Color Glass()
        {
            return new Color(0.025f, 0.095f, 0.105f);
        }
    }
}
