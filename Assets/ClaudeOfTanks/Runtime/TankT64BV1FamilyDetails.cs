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
            return id == "t64bv1" ||
                id == "ua_t64bv";
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
            if (definition.id == "ua_t64bv")
            {
                AddUkrainianDonbasFit(root, turret, definition, color);
            }
        }

        private static void AddUkrainianDonbasFit(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 7; index++)
                {
                    Box(
                        "Painted-UAT64BV-K1SideCassette",
                        root,
                        new Vector3(
                            side * 1.71f,
                            1.02f,
                            -1.47f + index * 0.69f),
                        new Vector3(0.060f, 0.28f, 0.48f),
                        color * 0.50f);
                }
                TankFittingShapeFactory.BuildAntennaWhip(
                    "UAT64BV-" + (side < 0 ? "Left" : "Right"),
                    turret,
                    new Vector3(side * 0.86f, 0.64f, -1.12f),
                    side < 0 ? 0.90f : 0.78f,
                    0.010f,
                    side * 0.05f,
                    color * 0.45f,
                    Dark());
            }

            for (int row = 0; row < 4; row++)
            for (int column = -3; column <= 3; column++)
            {
                if (Mathf.Abs(column) == 3 && row > 1) continue;
                Transform tile = Box(
                    "Painted-UAT64BV-K1GlacisTile",
                    root,
                    new Vector3(
                        column * 0.26f + (row % 2 == 0 ? 0f : 0.13f),
                        1.22f - row * 0.085f,
                        1.72f + row * 0.235f),
                    new Vector3(0.24f, 0.075f, 0.22f),
                    color * 0.50f);
                tile.localRotation =
                    Quaternion.Euler(-20f, 0f, 0f);
            }

            for (int side = -1; side <= 1; side += 2)
            for (int index = 0; index < 14; index++)
            {
                int row = index / 7;
                int column = index % 7;
                Transform tile = Box(
                    "Painted-UAT64BV-K1TurretHorseshoe",
                    turret,
                    new Vector3(
                        side * (0.18f + column * 0.15f),
                        0.24f + row * 0.12f,
                        1.02f - column * 0.09f - row * 0.12f),
                    new Vector3(0.14f, 0.09f, 0.18f),
                    color * 0.50f);
                tile.localRotation =
                    Quaternion.Euler(
                        -8f,
                        side * (20f + column * 6f),
                        0f);
            }

            Box(
                "Painted-UAT64BV-RightSnorkelRack",
                root,
                new Vector3(0.60f, 1.58f, -2.94f),
                new Vector3(0.84f, 0.42f, 0.54f),
                color * 0.55f);
            Cylinder(
                "Painted-UAT64BV-TransomDrum",
                root,
                new Vector3(-0.58f, 1.14f, -3.15f),
                0.14f,
                0.14f,
                0.46f,
                14,
                TankShapeAxis.X,
                color * 0.48f);
            Box(
                "Painted-UAT64BV-LeftRearRoofCrate",
                root,
                new Vector3(-0.78f, 1.50f, -2.58f),
                new Vector3(0.46f, 0.18f, 0.32f),
                color * 0.58f);
            Box(
                "UAT64BV-AkmProp",
                root,
                new Vector3(-0.74f, 1.65f, -2.40f),
                new Vector3(0.06f, 0.045f, 0.58f),
                Dark());

            string number = NumericMarking(definition.visual?.number);
            if (!string.IsNullOrEmpty(number))
            {
                TankTacticalNumberFactory.BuildPair(
                    "UAT64BV",
                    turret,
                    number,
                    0.23f,
                    new Vector3(1.10f, 0.34f, -0.62f),
                    Quaternion.Euler(0f, 90f, 0f),
                    new Vector3(-1.10f, 0.34f, -0.62f),
                    Quaternion.Euler(0f, -90f, 0f));
            }
        }

        private static string NumericMarking(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            char[] digits = new char[value.Length];
            int count = 0;
            for (int index = 0; index < value.Length; index++)
            {
                char c = value[index];
                if (c >= '0' && c <= '9')
                    digits[count++] = c;
            }
            return count == 0
                ? null
                : new string(digits, 0, count);
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            return part;
        }

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float top,
            float bottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                top,
                bottom,
                length,
                segments,
                axis,
                color);
            part.localPosition = position;
            return part;
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
