using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLeopardVariantDetails
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
            switch (definition.id)
            {
                case "leopard2_proto":
                    AddPrototype(
                        turret,
                        definition,
                        color,
                        width);
                    break;
                case "leo2a4_otco":
                    AddOtco(
                        turret,
                        definition,
                        color,
                        width);
                    break;
                case "leo2a5_a5nl":
                    AddA5Nl(
                        turret,
                        definition,
                        color,
                        width);
                    break;
                case "leo2_revolution":
                    AddRevolution(
                        turret,
                        definition,
                        color);
                    break;
                case "leo2a7v":
                    AddA7V(
                        root,
                        turret,
                        definition,
                        color,
                        width,
                        length);
                    break;
            }
        }

        private static void AddPrototype(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float front =
                TankDetailGeometry.TurretFrontZ(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.34f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard-Prototype-Rangefinder",
                    PrimitiveType.Sphere,
                    turret,
                    new Vector3(
                        side * (halfWidth - 0.12f),
                        roof - 0.1f,
                        front - 0.55f),
                    new Vector3(0.28f, 0.2f, 0.38f),
                    color * 0.8f);
                Transform lens =
                    TankDetailGeometry.Part(
                        "Leopard-Prototype-RangefinderLens",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * (halfWidth + 0.03f),
                            roof - 0.1f,
                            front - 0.55f),
                        new Vector3(
                            0.07f,
                            0.03f,
                            0.07f),
                        new Color(
                            0.04f,
                            0.15f,
                            0.17f));
                lens.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f);
            }
            TankDetailGeometry.Part(
                "Painted-Leopard-Prototype-Searchlight",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -width * 0.15f,
                    roof + 0.08f,
                    front - 0.45f),
                new Vector3(0.3f, 0.22f, 0.26f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Leopard-Prototype-SearchlightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -width * 0.15f,
                    roof + 0.08f,
                    front - 0.313f),
                new Vector3(0.19f, 0.12f, 0.014f),
                new Color(0.16f, 0.2f, 0.16f));
        }

        private static void AddOtco(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.4f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 4;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Leopard-OTCO-Shroud",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * (halfWidth - 0.06f),
                            roof - 0.28f,
                            0.72f - panel * 0.58f),
                        new Vector3(
                            0.12f,
                            0.36f,
                            0.5f),
                        color * 0.76f);
                }
            }
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    width * 0.15f,
                    -0.7f,
                    "Leopard-OTCO-RoofWeapon");
            AddRadioPair(
                turret,
                definition,
                width);
        }

        private static void AddA5Nl(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.4f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard-A5NL-AwarenessPod",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * (halfWidth - 0.1f),
                        roof - 0.12f,
                        0.05f),
                    new Vector3(0.18f, 0.28f, 0.4f),
                    color * 0.68f);
                TankDetailGeometry.Part(
                    "Leopard-A5NL-AwarenessLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * (halfWidth + 0.002f),
                        roof - 0.12f,
                        0.09f),
                    new Vector3(0.014f, 0.16f, 0.2f),
                    new Color(0.04f, 0.17f, 0.19f));
                TankDetailGeometry.Part(
                    "Leopard-A5NL-Beacon",
                    PrimitiveType.Sphere,
                    turret,
                    new Vector3(
                        side * width * 0.23f,
                        roof + 0.1f,
                        -1.85f),
                    new Vector3(0.09f, 0.11f, 0.09f),
                    new Color(0.9f, 0.52f, 0.08f));
            }
            TankDetailGeometry.Part(
                "Painted-Leopard-A5NL-Panorama",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.13f,
                    roof + 0.2f,
                    -0.7f),
                new Vector3(0.32f, 0.3f, 0.3f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Leopard-A5NL-PanoramaLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.13f,
                    roof + 0.21f,
                    -0.542f),
                new Vector3(0.2f, 0.11f, 0.014f),
                new Color(0.04f, 0.17f, 0.19f));
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    -width * 0.16f,
                    -1.3f,
                    "Leopard-A5NL-RWS");
        }

        private static void AddRevolution(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            TankDetailGeometry.Part(
                "Painted-Leopard-Revolution-SEOSS",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.8f, roof + 0.18f, -0.55f),
                new Vector3(0.46f, 0.28f, 0.36f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Leopard-Revolution-SEOSSLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.8f, roof + 0.18f, -0.362f),
                new Vector3(0.25f, 0.12f, 0.014f),
                new Color(0.04f, 0.16f, 0.2f));
            TankDetailGeometry.Part(
                "Painted-Leopard-Revolution-Electronics",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.82f, roof - 0.01f, -1.25f),
                new Vector3(0.78f, 0.25f, 0.72f),
                color * 0.65f);
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    0.45f,
                    -1.25f,
                    "Leopard-Revolution-RWS");
        }

        private static void AddA7V(
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
                    1.7f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Leopard-A7V-APU",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.33f,
                        hullRoof + 0.12f,
                        rear + 0.55f),
                    new Vector3(0.34f, 0.22f, 0.7f),
                    color * 0.7f);
            }
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            TankDetailGeometry.Part(
                "Painted-Leopard-A7V-ACUnit",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.78f, roof + 0.06f, -1.75f),
                new Vector3(0.38f, 0.12f, 0.48f),
                color * 0.68f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int station = 0;
                    station < 2;
                    station++)
                {
                    TankDetailGeometry.Part(
                        "Leopard-A7V-ADSSensor",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.27f,
                            roof + 0.08f,
                            station == 0
                                ? 0.3f
                                : -1.9f),
                        new Vector3(
                            0.1f,
                            0.09f,
                            0.1f),
                        new Color(
                            0.12f,
                            0.13f,
                            0.11f));
                }
            }
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    -0.12f,
                    -1.35f,
                    "Leopard-A7V-FLW200");
            TankLeopardFamilyDetails
                .AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    0.72f,
                    -1.48f,
                    "Leopard-A7V-AuxRWS");
        }

        private static void AddRadioPair(
            Transform turret,
            VehicleDefinition definition,
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
                    "Leopard-OTCO-Radio",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.2f,
                        roof + 0.1f,
                        -2.05f),
                    new Vector3(0.2f, 0.18f, 0.28f),
                    new Color(0.11f, 0.12f, 0.1f));
            }
        }
    }
}
