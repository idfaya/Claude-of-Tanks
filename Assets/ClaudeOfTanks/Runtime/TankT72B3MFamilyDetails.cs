using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT72B3MFamilyDetails
    {
        private static readonly string[] HiddenArmorNames =
        {
            "Armor-upper_glacis",
            "Armor-lower_front",
            "Armor-hull_side_upper_R",
            "Armor-hull_side_upper_L",
            "Armor-hull_side_lower_R",
            "Armor-hull_side_lower_L",
            "Armor-skirt_rubber_R",
            "Armor-skirt_rubber_L",
            "Armor-track_R",
            "Armor-track_L",
            "Armor-hull_rear",
            "Armor-hull_roof",
            "Armor-turret_cheek_R",
            "Armor-turret_cheek_L",
            "Armor-mantlet",
            "Armor-turret_side_R",
            "Armor-turret_side_L",
            "Armor-turret_rear",
            "Armor-turret_roof"
        };

        public static bool Supports(string id)
        {
            return id == "t72b3m";
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
            Hide(turret.Find("Turret"));
            Hide(turret.Find("Gun"));
            HideByPrefix(root, "Soviet-");
            HideByPrefix(root, "Painted-Soviet-");
            HideByPrefix(root, "ReturnRoller-");
            for (int index = 0;
                index < HiddenArmorNames.Length;
                index++)
            {
                HideAll(root, HiddenArmorNames[index]);
            }

            TankT72B3MHullDetails.Build(root, color);
            TankT72B3MRunningGearDetails.Build(root, color);
            TankT72B3MTurretDetails.Build(turret, color);
            TankT72B3MGunDetails.Build(turret, definition, color);
        }

        private static void HideByPrefix(
            Transform root,
            string prefix)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>(true);
            for (int index = 0;
                index < parts.Length;
                index++)
            {
                if (parts[index].name.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
                {
                    Hide(parts[index]);
                }
            }
        }

        private static void HideAll(
            Transform root,
            string name)
        {
            Transform[] parts =
                root.GetComponentsInChildren<Transform>(true);
            for (int index = 0;
                index < parts.Length;
                index++)
            {
                if (string.Equals(
                    parts[index].name,
                    name,
                    StringComparison.Ordinal))
                {
                    Hide(parts[index]);
                }
            }
        }

        private static void Hide(Transform part)
        {
            Renderer renderer =
                part == null
                    ? null
                    : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        internal static Color Dark()
        {
            return new Color(0.06f, 0.07f, 0.055f);
        }

        internal static Color Rubber()
        {
            return new Color(0.085f, 0.09f, 0.075f);
        }

        internal static Color Glass()
        {
            return new Color(0.025f, 0.08f, 0.095f);
        }

        internal static Color Cloth()
        {
            return new Color(0.17f, 0.19f, 0.135f);
        }
    }
}
