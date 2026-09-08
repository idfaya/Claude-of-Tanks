using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLeopardFamilyDetails
    {
        public static bool Supports(string id)
        {
            switch (id)
            {
                case "leopard2_proto":
                case "leo2a4":
                case "leo2a4_otco":
                case "leo2a4m":
                case "leo2a5":
                case "leo2a5_a5nl":
                case "leo2a6":
                case "leo2a6m":
                case "leo2_revolution":
                case "leo2a7v":
                case "leo2a6_ua":
                    return true;
                default:
                    return false;
            }
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

            AddEngineDeck(
                root,
                definition,
                color,
                width,
                length);
            AddHatches(
                turret,
                definition,
                color,
                width);
            AddSmokeBanks(
                turret,
                definition,
                color,
                width);
            AddAntennas(
                turret,
                definition,
                color,
                width);
            AddBustleRack(
                turret,
                definition,
                color,
                width);
            AddLoaderMg3(
                turret,
                definition,
                width);
            if (definition.id != "leopard2_proto")
            {
                AddProductionOptics(
                    turret,
                    definition,
                    color,
                    width);
            }
            if (definition.id == "leopard2_proto" ||
                definition.id == "leo2a4")
            {
                AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    0f,
                    -0.62f,
                    "Leopard-FLW200");
            }

            TankLeopardVariantDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                height,
                length);
            TankLeopardProtectionDetails.Build(
                root,
                turret,
                definition,
                color,
                width,
                length);
        }

        internal static void AddRemoteWeaponStation(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float x,
            float z,
            string prefix)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Base",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, roof + 0.06f, z),
                new Vector3(0.2f, 0.07f, 0.2f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.2f,
                    z + 0.06f),
                new Vector3(0.34f, 0.18f, 0.42f),
                color * 0.66f);
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.21f,
                    z + 0.48f),
                new Vector3(0.04f, 0.04f, 0.72f),
                new Color(0.08f, 0.09f, 0.08f));
            TankDetailGeometry.Part(
                prefix + "-Sensor",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x + 0.23f,
                    roof + 0.23f,
                    z - 0.02f),
                new Vector3(0.13f, 0.16f, 0.14f),
                new Color(0.11f, 0.12f, 0.1f));
            TankDetailGeometry.Part(
                prefix + "-Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x + 0.23f,
                    roof + 0.24f,
                    z + 0.058f),
                new Vector3(0.08f, 0.07f, 0.014f),
                new Color(0.05f, 0.18f, 0.2f));
        }

        private static void AddEngineDeck(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            float roof = TankDetailGeometry.HullRoofY(
                definition,
                1.65f);
            float rear = TankDetailGeometry.HullRearZ(
                definition,
                -length * 0.5f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Leopard-EngineGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.2f,
                        roof + 0.022f,
                        rear + length * 0.18f),
                    new Vector3(
                        width * 0.34f,
                        0.03f,
                        length * 0.2f),
                    color * 0.38f);
            }
        }

        private static void AddHatches(
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
                    "Painted-Leopard-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.17f,
                        roof + 0.035f,
                        -0.35f),
                    new Vector3(
                        width * 0.12f,
                        0.04f,
                        width * 0.12f),
                    color * 0.82f);
            }
        }

        private static void AddSmokeBanks(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.34f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < 8;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Leopard-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (halfWidth +
                                     0.04f +
                                     (tube % 4) *
                                     0.008f),
                                roof -
                                    0.12f +
                                    (tube / 4) *
                                    0.14f,
                                Mathf.Max(
                                    rear + 0.45f,
                                    -0.9f) -
                                    (tube % 4) *
                                    0.1f),
                            new Vector3(
                                0.042f,
                                0.14f,
                                0.042f),
                            color * 0.53f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            62f,
                            0f,
                            side * 18f);
                }
            }
        }

        private static void AddAntennas(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Leopard-Antenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.27f,
                        roof + 0.47f,
                        rear + 0.22f),
                    new Vector3(
                        0.012f,
                        0.5f,
                        0.012f),
                    color * 0.3f);
            }
        }

        private static void AddBustleRack(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            float rackZ = rear - 0.22f;
            Color rail = color * 0.45f;
            for (int bar = 0; bar < 5; bar++)
            {
                TankDetailGeometry.Part(
                    "Leopard-BustleRackBar",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.18f +
                            bar * 0.09f,
                        rackZ),
                    new Vector3(
                        width * 0.7f,
                        0.035f,
                        0.035f),
                    rail);
            }
            TankDetailGeometry.Part(
                "Painted-Leopard-BustleStowage",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    roof - 0.04f,
                    rackZ + 0.1f),
                new Vector3(
                    width * 0.48f,
                    0.28f,
                    0.32f),
                color * 0.65f);
        }

        private static void AddLoaderMg3(
            Transform turret,
            VehicleDefinition definition,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float x = -width * 0.16f;
            TankDetailGeometry.Part(
                "Leopard-MG3-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(x, roof + 0.17f, -0.2f),
                new Vector3(0.14f, 0.12f, 0.3f),
                new Color(0.09f, 0.1f, 0.08f));
            TankDetailGeometry.Part(
                "Leopard-MG3-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(x, roof + 0.18f, 0.2f),
                new Vector3(0.035f, 0.035f, 0.62f),
                new Color(0.07f, 0.08f, 0.07f));
        }

        private static void AddProductionOptics(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            TankDetailGeometry.Part(
                "Painted-Leopard-EMESHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -width * 0.18f,
                    roof + 0.12f,
                    0.35f),
                new Vector3(0.42f, 0.22f, 0.34f),
                color * 0.78f);
            TankDetailGeometry.Part(
                "Leopard-EMESLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -width * 0.18f,
                    roof + 0.12f,
                    0.528f),
                new Vector3(0.23f, 0.1f, 0.014f),
                new Color(0.04f, 0.18f, 0.2f));
            TankDetailGeometry.Part(
                "Painted-Leopard-PERISight",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.15f,
                    roof + 0.22f,
                    -0.55f),
                new Vector3(0.24f, 0.35f, 0.24f),
                color * 0.74f);
            TankDetailGeometry.Part(
                "Leopard-PERILens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.15f,
                    roof + 0.23f,
                    -0.423f),
                new Vector3(0.13f, 0.1f, 0.014f),
                new Color(0.05f, 0.18f, 0.2f));
        }
    }
}
