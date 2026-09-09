using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT62Obr1975FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "t62mv1";
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
            HideByPrefix(root, "Armor-");
            HideByPrefix(root, "ReturnRoller-");

            TankT62Obr1975HullDetails.Build(
                root,
                color);
            TankT62Obr1975TurretDetails.Build(
                turret,
                color);
            TankT62Obr1975GunDetails.Build(
                turret,
                definition,
                color);
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
            return new Color(0.075f, 0.08f, 0.065f);
        }

        internal static Color Rubber()
        {
            return new Color(0.105f, 0.11f, 0.09f);
        }

        internal static Color Glass()
        {
            return new Color(0.025f, 0.075f, 0.07f);
        }

        internal static Color Wood()
        {
            return new Color(0.19f, 0.12f, 0.065f);
        }
    }
}
