using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSovietVariantDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            string id = definition.id;
            if (HasShtora(id))
            {
                AddShtora(
                    turret,
                    definition,
                    color,
                    width);
            }
            else if (HasSearchlight(id))
            {
                AddSearchlight(
                    turret,
                    definition,
                    color,
                    width);
            }
            if (IsModernT90(id))
            {
                AddModernSight(
                    turret,
                    definition,
                    color,
                    width);
            }
            if (id == "t90a_burlak")
            {
                AddBurlakEquipment(
                    turret,
                    definition,
                    color,
                    width);
            }
            if (id.StartsWith("ua_t80",
                    StringComparison.Ordinal))
            {
                AddUkrainianEquipment(
                    turret,
                    definition,
                    color,
                    width);
            }
        }

        public static bool IsModernT90(string id)
        {
            return id == "t90sm" ||
                id == "t90ms" ||
                id == "t90m" ||
                id == "t90m_proryv";
        }

        private static bool HasShtora(string id)
        {
            return id == "t90" ||
                id == "t90a" ||
                id == "t90a_vladimir";
        }

        private static bool HasSearchlight(string id)
        {
            return id == "t72bu" ||
                id == "t80" ||
                id == "t80b" ||
                id == "t80bv" ||
                id == "t80u" ||
                id == "ua_t80bv" ||
                id == "ua_t80u_kursk";
        }

        private static void AddShtora(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float front =
                TankDetailGeometry.TurretFrontZ(
                    definition);
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform housing =
                    TankDetailGeometry.Part(
                        "Painted-Soviet-ShtoraHousing",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * width * 0.16f,
                            roof - 0.23f,
                            front + 0.1f),
                        new Vector3(
                            0.14f,
                            0.11f,
                            0.14f),
                        color * 0.62f);
                housing.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                Transform lens =
                    TankDetailGeometry.Part(
                        "Soviet-ShtoraLens",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * width * 0.16f,
                            roof - 0.23f,
                            front + 0.22f),
                        new Vector3(
                            0.09f,
                            0.025f,
                            0.09f),
                        new Color(
                            0.48f,
                            0.08f,
                            0.035f));
                lens.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void AddSearchlight(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float front =
                TankDetailGeometry.TurretFrontZ(
                    definition);
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            Transform housing =
                TankDetailGeometry.Part(
                    "Painted-Soviet-SearchlightHousing",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        -width * 0.2f,
                        roof - 0.18f,
                        front + 0.09f),
                    new Vector3(
                        0.19f,
                        0.13f,
                        0.19f),
                    color * 0.58f);
            housing.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            Transform lens =
                TankDetailGeometry.Part(
                    "Soviet-SearchlightLens",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        -width * 0.2f,
                        roof - 0.18f,
                        front + 0.23f),
                    new Vector3(
                        0.14f,
                        0.025f,
                        0.14f),
                    new Color(
                        0.16f,
                        0.2f,
                        0.16f));
            lens.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddModernSight(
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
                "Painted-Soviet-PanoramicSight",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -width * 0.2f,
                    roof + 0.23f,
                    rear * 0.05f),
                new Vector3(
                    0.3f,
                    0.42f,
                    0.28f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Soviet-PanoramicLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -width * 0.2f,
                    roof + 0.24f,
                    rear * 0.05f + 0.15f),
                new Vector3(
                    0.18f,
                    0.16f,
                    0.025f),
                new Color(
                    0.035f,
                    0.07f,
                    0.075f));
        }

        private static void AddBurlakEquipment(
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
                "Painted-Soviet-BurlakStowage",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    roof - 0.02f,
                    rear - 0.34f),
                new Vector3(
                    width * 0.64f,
                    0.44f,
                    0.68f),
                color * 0.7f);
            AddModernSight(
                turret,
                definition,
                color,
                width);
        }

        private static void AddUkrainianEquipment(
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
            Transform snorkel =
                TankDetailGeometry.Part(
                    "Painted-Soviet-UkrainianSnorkel",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        0f,
                        roof + 0.14f,
                        rear - 0.2f),
                    new Vector3(
                        0.11f,
                        width * 0.32f,
                        0.11f),
                    color * 0.55f);
            snorkel.localRotation =
                Quaternion.Euler(0f, 0f, 90f);
            TankDetailGeometry.Part(
                "Painted-Soviet-UkrainianTarp",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    width * 0.2f,
                    roof + 0.09f,
                    rear - 0.12f),
                new Vector3(
                    width * 0.24f,
                    0.16f,
                    0.42f),
                color * 0.64f);
        }

    }
}
