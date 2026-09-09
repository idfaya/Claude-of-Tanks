using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSovietFamilyDetails
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
            string id = definition?.id;
            if (!IsFamily(id)) return;
            if (TankT90AFamilyDetails.Supports(id)) return;
            if (TankT90FamilyDetails.Supports(id)) return;
            if (TankT72BUFamilyDetails.Supports(id)) return;
            if (TankT72B3MFamilyDetails.Supports(id)) return;

            if (TankT90AVladimirFamilyDetails.Supports(id))
            {
                TankT90AVladimirFamilyDetails.BuildHull(
                    root,
                    color);
            }
            else
            {
                AddRearGear(
                    root,
                    definition,
                    color,
                    width,
                    height,
                    length);
            }
            if (id == "bmpt_t90")
            {
                TankSovietBmptDetails.Build(
                    root,
                    turret,
                    color,
                    width,
                    height);
                return;
            }

            AddSmokeBanks(
                turret,
                definition,
                color,
                width,
                IsT90(id) ? 6 : 4);
            AddRoofEquipment(
                turret,
                definition,
                color,
                width);
            AddBustleEquipment(
                turret,
                definition,
                color,
                width);
            if (IsT80(id))
            {
                AddTurbineDeck(
                    root,
                    definition,
                    color,
                    width,
                    length);
            }
            TankSovietVariantDetails.Build(
                turret,
                definition,
                color,
                width);
        }

        private static bool IsFamily(string id)
        {
            return !string.IsNullOrEmpty(id) &&
                (id.StartsWith("t72",
                    StringComparison.Ordinal) ||
                 id.StartsWith("t80",
                    StringComparison.Ordinal) ||
                 id.StartsWith("t90",
                    StringComparison.Ordinal) ||
                 id.StartsWith("ua_t80",
                    StringComparison.Ordinal) ||
                 id == "bmpt_t90");
        }

        private static bool IsT80(string id)
        {
            return id.StartsWith("t80",
                    StringComparison.Ordinal) ||
                id.StartsWith("ua_t80",
                    StringComparison.Ordinal);
        }

        private static bool IsT90(string id)
        {
            return id.StartsWith("t90",
                    StringComparison.Ordinal) ||
                id == "t72bu";
        }

        private static void AddRearGear(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            float roof = TankDetailGeometry.HullRoofY(
                definition,
                height * 0.54f);
            float rear = TankDetailGeometry.HullRearZ(
                definition,
                -length * 0.47f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform drum = TankDetailGeometry.Part(
                    "Painted-Soviet-FuelDrum",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.23f,
                        roof + 0.12f,
                        rear + 0.3f),
                    new Vector3(
                        0.17f,
                        width * 0.17f,
                        0.17f),
                    color * 0.67f);
                drum.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
            }
            Transform log = TankDetailGeometry.Part(
                "Soviet-UnditchingLog",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    0f,
                    roof - 0.08f,
                    rear - 0.04f),
                new Vector3(
                    0.12f,
                    width * 0.43f,
                    0.12f),
                new Color(0.18f, 0.12f, 0.07f));
            log.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
        }

        private static void AddSmokeBanks(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            int count)
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
                    width * 0.33f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < count;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Soviet-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (halfWidth +
                                     0.045f +
                                     tube * 0.008f),
                                roof -
                                    0.2f +
                                    tube * 0.022f,
                                front -
                                    0.32f -
                                    tube * 0.052f),
                            new Vector3(
                                0.042f,
                                0.12f,
                                0.042f),
                            color * 0.55f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            62f,
                            0f,
                            side * 18f);
                }
            }
        }

        private static void AddRoofEquipment(
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
            TankDetailGeometry.Part(
                "Soviet-MachineGunReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.12f,
                    roof + 0.17f,
                    rear * 0.22f),
                new Vector3(
                    0.18f,
                    0.14f,
                    0.34f),
                new Color(0.1f, 0.11f, 0.09f));
            TankDetailGeometry.Part(
                "Soviet-MachineGunBarrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.12f,
                    roof + 0.18f,
                    rear * 0.22f + 0.5f),
                new Vector3(
                    0.045f,
                    0.045f,
                    0.75f),
                new Color(0.08f, 0.09f, 0.08f));
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Soviet-Antenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.23f,
                        roof + 0.38f,
                        rear + 0.16f),
                    new Vector3(
                        0.014f,
                        0.42f,
                        0.014f),
                    color * 0.34f);
            }
        }

        private static void AddBustleEquipment(
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
            bool modern =
                TankSovietVariantDetails.IsModernT90(
                    definition.id);
            Color rail = color * 0.48f;
            TankDetailGeometry.Part(
                modern
                    ? "Painted-Soviet-ModernStowage"
                    : "Painted-Soviet-RearStowage",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    roof - 0.12f,
                    rear - 0.18f),
                new Vector3(
                    width *
                        (modern ? 0.55f : 0.38f),
                    modern ? 0.28f : 0.2f,
                    modern ? 0.42f : 0.3f),
                color *
                    (modern ? 0.72f : 0.66f));
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Soviet-BustleRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.28f,
                        roof - 0.05f,
                        rear - 0.31f),
                    new Vector3(
                        0.035f,
                        0.24f,
                        0.5f),
                    rail);
            }
            for (int bar = 0; bar < 2; bar++)
            {
                TankDetailGeometry.Part(
                    "Soviet-BustleBar",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof -
                            0.15f +
                            bar * 0.17f,
                        rear - 0.45f),
                    new Vector3(
                        width * 0.57f,
                        0.035f,
                        0.035f),
                    rail);
            }
        }

        private static void AddTurbineDeck(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float length)
        {
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    1.5f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.47f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Soviet-TurbineGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.22f,
                        roof + 0.025f,
                        rear + length * 0.18f),
                    new Vector3(
                        width * 0.36f,
                        0.03f,
                        length * 0.18f),
                    color * 0.42f);
            }
        }
    }
}
