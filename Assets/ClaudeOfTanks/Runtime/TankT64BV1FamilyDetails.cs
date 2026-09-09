using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT64BV1FamilyDetails
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
            return id == "t64bv1";
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
            for (int index = 0;
                index < HiddenArmorNames.Length;
                index++)
            {
                HideAll(root, HiddenArmorNames[index]);
            }
            HideAll(root, "ReturnRoller-L");
            HideAll(root, "ReturnRoller-R");

            TankT64BV1HullDetails.Build(root, color);
            TankT64BV1RunningGearDetails.Build(root, color);
            TankT64BV1TurretDetails.Build(turret, color);
            TankT64BV1GunDetails.Build(
                turret,
                definition,
                color);
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
            return new Color(0.065f, 0.075f, 0.06f);
        }

        internal static Color Rubber()
        {
            return new Color(0.095f, 0.1f, 0.085f);
        }

        internal static Color Glass()
        {
            return new Color(0.025f, 0.09f, 0.075f);
        }

        internal static Color Wood()
        {
            return new Color(0.21f, 0.13f, 0.065f);
        }
    }
}
