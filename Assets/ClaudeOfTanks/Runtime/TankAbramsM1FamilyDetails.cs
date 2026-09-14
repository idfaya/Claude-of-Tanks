using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAbramsM1FamilyDetails
    {
        public static bool Supports(string id)
        {
            return id == "m1a1" ||
                id == "m1a1ha" ||
                id == "ua_m1a1";
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
            bool heavyArmor =
                definition.id == "m1a1ha" ||
                definition.id == "ua_m1a1";
            TankAbramsM1HullDetails.Build(root, color, heavyArmor);
            TankAbramsM1TurretDetails.Build(turret, color, heavyArmor);
            TankAbramsM1GunDetails.Build(
                turret,
                definition,
                color,
                heavyArmor);
            if (definition.id == "ua_m1a1")
                AddUkrainianDroneCage(root, turret, color);
        }

        private static void AddUkrainianDroneCage(
            Transform root,
            Transform turret,
            Color color)
        {
            Transform cage =
                new GameObject("UAM1A1-DroneCageRoot").transform;
            cage.SetParent(turret, false);
            Vector3[] stations =
            {
                new Vector3(1.94f, 0.20f, 2.62f),
                new Vector3(1.98f, 0.10f, 0.28f),
                new Vector3(2.04f, 0.08f, -1.28f),
                new Vector3(2.06f, 0.14f, -3.34f)
            };
            float[] roofs = { 1.16f, 1.30f, 1.34f, 1.28f };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < stations.Length; index++)
                {
                    Vector3 station = stations[index];
                    Box(
                        "Painted-UAM1A1-CageFoot",
                        cage,
                        new Vector3(
                            side * (station.x - 0.24f),
                            station.y,
                            station.z),
                        new Vector3(0.46f, 0.11f, 0.30f),
                        color * 0.52f);
                    AddCagePost(
                        cage,
                        side * station.x,
                        station.y,
                        station.z,
                        roofs[index] - station.y);
                    if (index > 0)
                    {
                        Vector3 previous = stations[index - 1];
                        AddCageRail(
                            cage,
                            side * ((previous.x + station.x) * 0.5f),
                            (roofs[index - 1] + roofs[index]) * 0.5f,
                            (previous.z + station.z) * 0.5f,
                            station.z - previous.z,
                            roofs[index] - roofs[index - 1]);
                        AddCageRail(
                            cage,
                            side * ((previous.x + station.x) * 0.5f),
                            (previous.y + station.y) * 0.5f,
                            (previous.z + station.z) * 0.5f,
                            station.z - previous.z,
                            station.y - previous.y);
                    }
                }
            }
            for (int index = 1; index < stations.Length; index++)
            {
                float z = stations[index].z;
                float y = roofs[index];
                Box(
                    "UAM1A1-CageCrossRib",
                    cage,
                    new Vector3(0f, y, z),
                    new Vector3(stations[index].x * 2f, 0.032f, 0.032f),
                    Dark());
            }
            for (int index = 0; index < 5; index++)
            {
                float x = -1.38f + index * 0.69f;
                AddCageRail(
                    cage,
                    x,
                    (roofs[0] + roofs[3]) * 0.5f,
                    (stations[0].z + stations[3].z) * 0.5f,
                    stations[3].z - stations[0].z,
                    roofs[3] - roofs[0]);
            }
            Box(
                "Painted-UAM1A1-RearFieldStowage",
                root,
                new Vector3(0.72f, 1.72f, -3.34f),
                new Vector3(0.72f, 0.24f, 0.34f),
                color * 0.56f);
            Box(
                "UAM1A1-DroneCageJammerBox",
                cage,
                new Vector3(-0.34f, 1.17f, -1.80f),
                new Vector3(0.26f, 0.18f, 0.20f),
                Dark());
        }

        private static void AddCagePost(
            Transform parent,
            float x,
            float baseY,
            float z,
            float height)
        {
            Box(
                "UAM1A1-CagePost",
                parent,
                new Vector3(x, baseY + height * 0.5f, z),
                new Vector3(0.032f, height, 0.032f),
                Dark());
        }

        private static void AddCageRail(
            Transform parent,
            float x,
            float y,
            float z,
            float depth,
            float heightDelta)
        {
            Transform rail = Box(
                "UAM1A1-CageLongitudinalRail",
                parent,
                new Vector3(x, y, z),
                new Vector3(0.032f, 0.032f, Mathf.Abs(depth)),
                Dark());
            rail.localRotation =
                Quaternion.Euler(
                    -Mathf.Atan2(heightDelta, depth) * Mathf.Rad2Deg,
                    0f,
                    0f);
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
            return new Color(0.045f, 0.05f, 0.038f);
        }

        public static Color Gunmetal()
        {
            return new Color(0.11f, 0.115f, 0.095f);
        }

        public static Color Glass()
        {
            return new Color(0.025f, 0.08f, 0.075f);
        }
    }
}
