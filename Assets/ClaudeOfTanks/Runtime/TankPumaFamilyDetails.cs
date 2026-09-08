using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPumaFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "spz_puma" ||
                id == "spz_puma_s1";
        }

        public static bool IsS1(
            VehicleDefinition definition)
        {
            return definition?.id == "spz_puma_s1";
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

            HideGenericRenderer(
                root.Find("Hull"));
            HideGenericRenderer(
                root.Find("UpperHull"));
            HideGenericRenderer(
                root.Find("Armor-track_L"));
            HideGenericRenderer(
                root.Find("Armor-track_R"));
            HideGenericRenderer(
                turret.Find("Turret"));
            HideGenericRenderer(
                turret.Find("Gun"));
            TankPumaHullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            TankPumaTurretDetails.Build(
                turret,
                definition,
                color);
            TankPumaRoofEquipment.Build(
                turret,
                definition,
                color);
            TankPumaGunDetails.Build(
                turret,
                definition,
                color);
        }

        internal static Color Dark()
        {
            return new Color(0.065f, 0.072f, 0.062f);
        }

        internal static Color Gunmetal()
        {
            return new Color(0.12f, 0.125f, 0.11f);
        }

        internal static Color Lens()
        {
            return new Color(0.018f, 0.085f, 0.095f);
        }

        private static void HideGenericRenderer(
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
