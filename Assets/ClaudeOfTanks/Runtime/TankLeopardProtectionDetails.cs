using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLeopardProtectionDetails
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            switch (definition.id)
            {
                case "leo2a4m":
                    AddMineKit(root, definition, color, length, "A4M");
                    AddA4MProtection(
                        root,
                        turret,
                        definition,
                        color,
                        width,
                        length);
                    break;
                case "leo2a6m":
                    AddMineKit(root, definition, color, length, "A6M");
                    AddA6MProtection(
                        root,
                        turret,
                        definition,
                        color,
                        width,
                        length);
                    break;
                case "leo2a6_ua":
                    AddMineKit(root, definition, color, length, "UA");
                    AddUaProtection(
                        root,
                        turret,
                        definition,
                        color,
                        width,
                        length);
                    break;
                case "leo2a7v":
                    AddA7VRearSlat(
                        turret,
                        definition,
                        color,
                        width);
                    break;
            }
        }

        private static void AddMineKit(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float length,
            string variant)
        {
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f);
            TankDetailGeometry.Part(
                "Painted-Leopard-" +
                    variant +
                    "-MinePlate",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    0.5f,
                    rear + length * 0.52f),
                new Vector3(
                    1.88f,
                    0.07f,
                    length * 0.69f),
                color * 0.6f);
        }

        private static void AddA4MProtection(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            float hullRoof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    1.8f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int cassette = 0;
                    cassette < 7;
                    cassette++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Leopard-A4M-ArmorCassette",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.5f,
                            hullRoof - 0.62f,
                            length * 0.31f -
                                cassette *
                                length * 0.105f),
                        new Vector3(
                            0.1f,
                            0.52f,
                            length * 0.085f),
                        color * 0.76f);
                }
            }
            AddSideCage(
                root,
                "Leopard-A4M-HullSlat",
                width * 0.54f,
                0.9f,
                hullRoof - 0.22f,
                -length * 0.4f,
                length * 0.18f,
                5,
                color);
            AddSideCage(
                turret,
                "Leopard-A4M-TurretSlat",
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.4f) +
                    0.14f,
                0.08f,
                TankDetailGeometry.TurretRoofY(
                    definition) -
                    0.02f,
                -2.25f,
                0.75f,
                4,
                color);
            AddRearCage(
                root,
                "Leopard-A4M-RearSlat",
                width * 0.8f,
                0.7f,
                1.05f,
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f) -
                    0.12f,
                5,
                color);
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    0.42f,
                    -0.75f,
                    "Leopard-A4M-RWS");
        }

        private static void AddA6MProtection(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            float hullRoof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    1.8f);
            AddSideCage(
                root,
                "Leopard-A6M-HullSlat",
                width * 0.5f,
                0.88f,
                hullRoof - 0.42f,
                -length * 0.4f,
                length * 0.4f,
                6,
                color);
            AddRearCage(
                root,
                "Leopard-A6M-RearSlat",
                width * 0.82f,
                0.72f,
                1.42f,
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f) -
                    0.12f,
                7,
                color);
            AddSideCage(
                turret,
                "Leopard-A6M-TurretSlat",
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.4f) +
                    0.15f,
                0.08f,
                TankDetailGeometry.TurretRoofY(
                    definition) -
                    0.04f,
                -2.85f,
                -0.85f,
                3,
                color);
            TankDetailGeometry.Part(
                "Painted-Leopard-A6M-Cooler",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0.55f,
                    TankDetailGeometry.TurretRoofY(
                        definition) +
                        0.12f,
                    -2.55f),
                new Vector3(0.48f, 0.24f, 0.36f),
                color * 0.65f);
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    0.08f,
                    -1.5f,
                    "Leopard-A6M-RoofRWS");
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    -0.72f,
                    -1.52f,
                    "Leopard-A6M-AuxRWS");
        }

        private static void AddUaProtection(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            float hullRoof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    1.8f);
            AddSideCage(
                root,
                "Leopard-UA-HullSlat",
                width * 0.5f,
                0.76f,
                hullRoof - 0.3f,
                -length * 0.42f,
                length * 0.42f,
                7,
                color);
            AddRearCage(
                root,
                "Leopard-UA-RearSlat",
                width * 0.98f,
                1.42f,
                2.05f,
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f) -
                    0.14f,
                6,
                color);
            AddSideCage(
                turret,
                "Leopard-UA-TurretSlat",
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.4f) +
                    0.2f,
                0.06f,
                TankDetailGeometry.TurretRoofY(
                    definition) +
                    0.02f,
                -3.05f,
                1.2f,
                6,
                color);
            AddRoofBasket(
                turret,
                definition,
                color,
                width);
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    -0.7f,
                    -2f,
                    "Leopard-UA-HeavyRWS");
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    0.7f,
                    -0.7f,
                    "Leopard-UA-LightRWS");
        }

        private static void AddA7VRearSlat(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            AddRearCage(
                turret,
                "Leopard-A7V-RearSlat",
                width * 0.62f,
                0.18f,
                0.62f,
                TankDetailGeometry.TurretRearZ(
                    definition) -
                    0.18f,
                4,
                color);
        }

        private static void AddSideCage(
            Transform parent,
            string name,
            float x,
            float y0,
            float y1,
            float z0,
            float z1,
            int sections,
            Color color)
        {
            float sectionLength =
                (z1 - z0) / sections;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int section = 0;
                    section < sections;
                    section++)
                {
                    float centerZ =
                        z0 +
                        sectionLength *
                        (section + 0.5f);
                    for (int row = 0;
                        row < 5;
                        row++)
                    {
                        TankDetailGeometry.Part(
                            name,
                            PrimitiveType.Cube,
                            parent,
                            new Vector3(
                                side * x,
                                Mathf.Lerp(
                                    y0,
                                    y1,
                                    row / 4f),
                                centerZ),
                            new Vector3(
                                0.022f,
                                0.022f,
                                sectionLength -
                                    0.05f),
                            color * 0.37f);
                    }
                    for (int edge = -1;
                        edge <= 1;
                        edge += 2)
                    {
                        TankDetailGeometry.Part(
                            name + "-Post",
                            PrimitiveType.Cube,
                            parent,
                            new Vector3(
                                side * x,
                                (y0 + y1) * 0.5f,
                                centerZ +
                                    edge *
                                    (sectionLength *
                                     0.5f -
                                     0.02f)),
                            new Vector3(
                                0.028f,
                                y1 - y0 + 0.04f,
                                0.028f),
                            color * 0.42f);
                    }
                }
            }
        }

        private static void AddRearCage(
            Transform parent,
            string name,
            float width,
            float y0,
            float y1,
            float z,
            int rows,
            Color color)
        {
            for (int row = 0;
                row < rows;
                row++)
            {
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    parent,
                    new Vector3(
                        0f,
                        Mathf.Lerp(
                            y0,
                            y1,
                            row /
                                Mathf.Max(
                                    1f,
                                    rows - 1f)),
                        z),
                    new Vector3(
                        width,
                        0.024f,
                        0.024f),
                    color * 0.37f);
            }
            for (int post = 0;
                post < 7;
                post++)
            {
                TankDetailGeometry.Part(
                    name + "-Post",
                    PrimitiveType.Cube,
                    parent,
                    new Vector3(
                        -width * 0.47f +
                            width * 0.94f *
                            post / 6f,
                        (y0 + y1) * 0.5f,
                        z),
                    new Vector3(
                        0.028f,
                        y1 - y0 + 0.04f,
                        0.028f),
                    color * 0.42f);
            }
        }

        private static void AddRoofBasket(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Leopard-UA-RoofBasket",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.34f,
                        roof + 0.03f,
                        -1.1f),
                    new Vector3(
                        0.032f,
                        0.032f,
                        3.6f),
                    color * 0.4f);
            }
        }
    }
}
