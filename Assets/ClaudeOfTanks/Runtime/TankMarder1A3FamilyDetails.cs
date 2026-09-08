using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMarder1A3FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "marder1a3";
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

            HideRenderer(root.Find("Hull"));
            HideRenderer(root.Find("UpperHull"));
            HideRenderer(root.Find("Armor-track_L"));
            HideRenderer(root.Find("Armor-track_R"));
            HideRenderer(turret.Find("Turret"));
            HideRenderer(turret.Find("Gun"));
            TankMarder1A3HullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            TankMarder1A3TurretDetails.Build(
                turret,
                color);
            TankMarder1A3GunDetails.Build(
                turret,
                definition,
                color);
        }

        internal static Color Dark()
        {
            return new Color(
                0.065f,
                0.07f,
                0.06f);
        }

        internal static Color Gunmetal()
        {
            return new Color(
                0.12f,
                0.12f,
                0.105f);
        }

        internal static Color Lens()
        {
            return new Color(
                0.018f,
                0.082f,
                0.095f);
        }

        private static void HideRenderer(
            Transform part)
        {
            Renderer renderer =
                part == null
                    ? null
                    : part.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }
    }
}
