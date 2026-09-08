using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmp2FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "bmp2";
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
            TankBmp2HullDetails.Build(
                root,
                color);
            TankBmp2TurretDetails.Build(
                turret,
                color);
            TankBmp2GunDetails.Build(
                turret,
                definition,
                color);
        }

        internal static Color Dark()
        {
            return new Color(
                0.055f,
                0.06f,
                0.052f);
        }

        internal static Color Gunmetal()
        {
            return new Color(
                0.115f,
                0.12f,
                0.105f);
        }

        internal static Color Lens()
        {
            return new Color(
                0.016f,
                0.075f,
                0.09f);
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
