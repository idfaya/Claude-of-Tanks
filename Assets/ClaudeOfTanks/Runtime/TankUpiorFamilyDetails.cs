using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankUpiorFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "upior";
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
            HideNamedRenderers(root, "ReturnRoller-L");
            HideNamedRenderers(root, "ReturnRoller-R");

            TankUpiorHullDetails.Build(root, color);
            TankUpiorTurretDetails.Build(turret, color);
            TankUpiorGunDetails.Build(
                turret,
                definition,
                color);
        }

        internal static Color Dark()
        {
            return new Color(0.045f, 0.052f, 0.045f);
        }

        internal static Color Gunmetal()
        {
            return new Color(0.11f, 0.12f, 0.105f);
        }

        internal static Color Lens()
        {
            return new Color(0.015f, 0.075f, 0.09f);
        }

        private static void HideNamedRenderers(
            Transform root,
            string name)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>();
            for (int index = 0;
                index < parts.Length;
                index++)
            {
                if (parts[index].name == name)
                    HideRenderer(parts[index]);
            }
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
