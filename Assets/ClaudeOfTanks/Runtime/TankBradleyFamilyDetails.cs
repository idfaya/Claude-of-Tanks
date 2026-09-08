using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBradleyFamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "m2a2_bradley" ||
                id == "ua_m2a3_bradley" ||
                id == "m3a3_bradley";
        }

        public static bool IsUkrainian(
            VehicleDefinition definition)
        {
            return definition?.id ==
                "ua_m2a3_bradley";
        }

        public static bool IsM3A3(
            VehicleDefinition definition)
        {
            return definition?.id ==
                "m3a3_bradley";
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
            TankBradleyHullDetails.Build(
                root,
                definition,
                color,
                width,
                height,
                length);
            TankBradleySideProtectionDetails.Build(
                root,
                definition,
                color);
            TankBradleyTurretDetails.Build(
                turret,
                definition,
                color);
            TankBradleyRoofEquipment.Build(
                turret,
                definition,
                color);
            TankBradleyGunDetails.Build(
                turret,
                definition,
                color);
        }

        internal static Color Dark()
        {
            return new Color(
                0.07f,
                0.072f,
                0.06f);
        }

        internal static Color Gunmetal()
        {
            return new Color(
                0.13f,
                0.13f,
                0.11f);
        }

        internal static Color Lens()
        {
            return new Color(
                0.018f,
                0.09f,
                0.105f);
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
