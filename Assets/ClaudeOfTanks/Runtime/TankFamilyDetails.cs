using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankFamilyDetails
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            if (definition == null) return;
            if (definition.role == "ifv" &&
                definition.id != "bmpt_t90")
            {
                Part(
                    "MissilePod",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        width * 0.3f,
                        height * 0.05f,
                        0f),
                    new Vector3(
                        width * 0.22f,
                        height * 0.18f,
                        width * 0.38f),
                    color * 0.78f);
            }
            if (definition.era == "modern")
            {
                for (int side = -1;
                    side <= 1;
                    side += 2)
                {
                    Part(
                        "SideArmor",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.48f,
                            height * 0.46f,
                            0f),
                        new Vector3(
                            width * 0.06f,
                            height * 0.24f,
                            length * 0.58f),
                        color * 0.82f);
                }
            }
            TankSovietFamilyDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            if (IsAbrams(definition.id))
                AddAbrams(
                    root,
                    turret,
                    definition,
                    color,
                    width,
                    height,
                    length);
            if (definition.id.Contains("leopard") ||
                definition.id.StartsWith("leo2"))
            {
                Part(
                    "TurretWedge",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        0f,
                        width * 0.4f),
                    new Vector3(
                        width * 0.78f,
                        height * 0.24f,
                        width * 0.42f),
                    color * 1.05f);
            }
        }

        private static bool IsAbrams(string id)
        {
            return id.Contains("abrams") ||
                id.StartsWith("m1a") ||
                id == "ua_m1a1";
        }

        private static void AddAbrams(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            Part(
                "Painted-Abrams-Bustle",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    height * 0.08f,
                    -width * 0.69f),
                new Vector3(
                    width * 0.84f,
                    height * 0.22f,
                    width * 0.62f),
                color * 0.88f);
            AddBustleRack(
                turret,
                color,
                width,
                height);
            AddSmokeLaunchers(
                turret,
                color,
                width,
                height);
            AddHatches(
                turret,
                color,
                width,
                height);
            AddEngineDeck(
                root,
                color,
                width,
                height,
                length);
            AddAntennas(
                turret,
                color,
                width,
                height);
            AddCommanderStation(
                turret,
                definition,
                color,
                width,
                height);
        }

        private static void AddBustleRack(
            Transform turret,
            Color color,
            float width,
            float height)
        {
            Color rail = color * 0.55f;
            float rear = -width * 1.02f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Abrams-RackRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.39f,
                        height * 0.02f,
                        rear),
                    new Vector3(
                        0.045f,
                        height * 0.27f,
                        width * 0.45f),
                    rail);
            }
            for (int bar = 0; bar < 3; bar++)
            {
                Part(
                    "Abrams-RackBar",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        height * (-0.08f + bar * 0.09f),
                        rear -
                            width * (0.13f - bar * 0.1f)),
                    new Vector3(
                        width * 0.82f,
                        0.045f,
                        0.045f),
                    rail);
            }
        }

        private static void AddSmokeLaunchers(
            Transform turret,
            Color color,
            float width,
            float height)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0; tube < 6; tube++)
                {
                    Transform launcher = Part(
                        "Painted-Abrams-SmokeLauncher",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * width *
                                (0.35f + tube * 0.012f),
                            height *
                                (0.18f + tube * 0.025f),
                            width *
                                (0.24f - tube * 0.055f)),
                        new Vector3(
                            0.055f,
                            0.16f,
                            0.055f),
                        color * 0.58f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            64f,
                            0f,
                            side * 20f);
                }
            }
        }

        private static void AddHatches(
            Transform turret,
            Color color,
            float width,
            float height)
        {
            float y = height * 0.34f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-Abrams-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.2f,
                        y,
                        -width * 0.1f),
                    new Vector3(
                        width * 0.13f,
                        0.045f,
                        width * 0.13f),
                    color * 0.82f);
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color,
            float width,
            float height,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Abrams-EngineGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.23f,
                        height * 0.51f,
                        -length * 0.34f),
                    new Vector3(
                        width * 0.38f,
                        0.035f,
                        length * 0.18f),
                    color * 0.48f);
            }
        }

        private static void AddAntennas(
            Transform turret,
            Color color,
            float width,
            float height)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform antenna = Part(
                    "Abrams-Antenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.31f,
                        height * 0.54f,
                        -width * 0.61f),
                    new Vector3(
                        0.018f,
                        height * 0.48f,
                        0.018f),
                    color * 0.38f);
                antenna.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * -5f);
            }
        }

        private static void AddCommanderStation(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height)
        {
            bool late = definition.id == "m1a2_tusk" ||
                definition.id == "m1a2_sepv2" ||
                definition.id == "m1a2_sepv3";
            if (!late) return;
            float roofY = TurretRoofY(definition);
            float towerHeight =
                definition.id == "m1a2_sepv3"
                    ? 0.42f
                    : 0.56f;
            float stationX = -width * 0.22f;
            float stationZ = -width * 0.04f;
            Part(
                "Painted-Abrams-CommanderStation",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    stationX,
                    roofY + towerHeight * 0.5f,
                    stationZ),
                new Vector3(
                    width * 0.2f,
                    towerHeight,
                    width * 0.18f),
                color * 0.66f);
            Part(
                "Abrams-CommanderSensor",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    stationX,
                    roofY + towerHeight * 0.72f,
                    stationZ + width * 0.095f),
                new Vector3(
                    width * 0.13f,
                    towerHeight * 0.32f,
                    0.035f),
                new Color(
                    0.035f,
                    0.07f,
                    0.075f));
            Transform weapon = Part(
                "Abrams-CommanderWeapon",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    stationX,
                    roofY + towerHeight + 0.08f,
                    stationZ + 0.62f),
                new Vector3(
                    0.09f,
                    0.09f,
                    1.35f),
                new Color(
                    0.11f,
                    0.12f,
                    0.1f));
            weapon.localRotation = Quaternion.identity;
        }

        private static float TurretRoofY(
            VehicleDefinition definition)
        {
            float roof = 0.72f;
            ArmorPlateDefinition[] plates =
                definition?.armor?.turretPlates;
            if (plates == null) return roof;
            for (int i = 0; i < plates.Length; i++)
            {
                CatalogPoint[] vertices = plates[i]?.verts;
                if (vertices == null) continue;
                for (int vertex = 0;
                    vertex < vertices.Length;
                    vertex++)
                {
                    roof = Mathf.Max(
                        roof,
                        vertices[vertex].y);
                }
            }
            return roof;
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            GameObject part =
                GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Collider collider =
                part.GetComponent<Collider>();
            if (Application.isPlaying)
                Object.Destroy(collider);
            else
                Object.DestroyImmediate(collider);
            part.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Standard"))
                {
                    color = color
                };
            return part.transform;
        }
    }
}
