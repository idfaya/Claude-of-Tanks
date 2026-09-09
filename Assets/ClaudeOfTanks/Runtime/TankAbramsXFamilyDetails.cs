using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsXFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "abramsx";
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
            TankAbramsXHullDetails.Build(root, color);
            TankAbramsXTurretDetails.Build(turret, color);
            TankAbramsXGunDetails.Build(
                turret,
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
            return new Color(0.055f, 0.058f, 0.05f);
        }

        public static Color Gunmetal()
        {
            return new Color(0.13f, 0.135f, 0.12f);
        }

        public static Color Glass()
        {
            return new Color(0.02f, 0.09f, 0.095f);
        }
    }
}
