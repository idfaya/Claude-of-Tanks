using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPattonTurretDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            string id = definition.id;
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddHatches(
                turret,
                color,
                width,
                roof,
                id);

            if (id == "m46_patton")
            {
                AddM46(
                    turret,
                    color,
                    roof,
                    width);
            }
            else if (id == "m47_patton")
            {
                AddM47(
                    turret,
                    color,
                    roof,
                    width);
            }
            else if (id == "m48")
            {
                AddM48(
                    turret,
                    color,
                    roof,
                    width);
            }
            else if (id == "m60a2")
            {
                AddM60A2(
                    turret,
                    color,
                    roof,
                    width);
            }
            else
            {
                AddM60(
                    turret,
                    color,
                    roof,
                    width,
                    id == "m60a3");
            }
        }

        private static void AddHatches(
            Transform turret,
            Color color,
            float width,
            float roof,
            string id)
        {
            bool m60 =
                id == "m60a1" ||
                id == "m60a2" ||
                id == "m60a3";
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float x = side * width *
                    (m60 ? 0.15f : 0.14f);
                float z = side < 0
                    ? -0.48f
                    : -0.16f;
                TankDetailGeometry.Part(
                    "Painted-Patton-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(x, roof + 0.035f, z),
                    new Vector3(
                        m60 ? 0.2f : 0.22f,
                        0.035f,
                        m60 ? 0.2f : 0.22f),
                    color * 0.82f);
            }
        }

        private static void AddM46(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            TankPattonRoofEquipment.AddCupola(
                turret,
                color,
                roof,
                new Vector3(-width * 0.16f, 0.09f, -0.44f),
                "Patton-M46");
            TankPattonFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(-0.42f, 0.2f, -0.38f),
                "Patton-M46-M2",
                false);
            TankPattonRoofEquipment.AddRack(
                turret,
                color,
                width * 0.72f,
                0.4f,
                roof - 0.08f,
                -1.55f,
                "Patton-M46");
            TankPattonRoofEquipment.AddTwinAntennas(
                turret,
                roof,
                width,
                "Patton-M46");
        }

        private static void AddM47(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Patton-M47-RangefinderBlister",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.31f,
                        roof - 0.22f,
                        0.06f),
                    new Vector3(0.15f, 0.3f, 0.15f),
                    color * 0.76f)
                    .localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
            }
            TankPattonRoofEquipment.AddCupola(
                turret,
                color,
                roof,
                new Vector3(-width * 0.15f, 0.11f, -0.55f),
                "Patton-M47");
            TankPattonFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(0.17f, 0.22f, -0.4f),
                "Patton-M47-M2",
                false);
            TankPattonRoofEquipment.AddRack(
                turret,
                color,
                width * 0.65f,
                0.5f,
                roof - 0.16f,
                -2.05f,
                "Patton-M47");
            TankPattonRoofEquipment.AddTwinAntennas(
                turret,
                roof,
                width,
                "Patton-M47");
        }

        private static void AddM48(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            TankPattonRoofEquipment.AddCupola(
                turret,
                color,
                roof,
                new Vector3(width * 0.16f, 0.12f, -0.31f),
                "Patton-M48");
            TankPattonFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(0.58f, 0.21f, 0.12f),
                "Patton-M48-CommanderM2",
                false);
            TankPattonFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(-0.82f, 0.32f, 0.2f),
                "Patton-M48-LoaderM2",
                false);
            TankPattonRoofEquipment.AddRack(
                turret,
                color,
                width * 0.72f,
                0.48f,
                roof - 0.16f,
                -1.43f,
                "Patton-M48");
            TankPattonRoofEquipment.AddTwinAntennas(
                turret,
                roof,
                width,
                "Patton-M48");
            TankPattonRoofEquipment.AddTurretSearchlight(
                turret,
                color,
                roof,
                new Vector3(-width * 0.31f, -0.2f, -0.88f),
                "Patton-M48");
            TankPattonFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(0.52f, roof + 0.09f, 0.92f),
                new Vector3(0.25f, 0.18f, 0.3f),
                "Patton-M48-GunnerSight");
        }

        private static void AddM60(
            Transform turret,
            Color color,
            float roof,
            float width,
            bool a3)
        {
            string prefix = a3
                ? "Patton-M60A3"
                : "Patton-M60A1";
            TankPattonRoofEquipment.AddM19Cupola(
                turret,
                color,
                roof,
                width,
                prefix);
            TankPattonFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(-0.58f, 0.27f, 0.2f),
                prefix + "-M2",
                a3);
            TankPattonRoofEquipment.AddRack(
                turret,
                color,
                width * 0.65f,
                0.42f,
                roof - 0.12f,
                -1.42f,
                prefix);
            TankPattonRoofEquipment.AddTwinAntennas(
                turret,
                roof,
                width,
                prefix);
            if (a3)
            {
                TankPattonRoofEquipment.AddSmokeBanks(
                    turret,
                    color,
                    roof,
                    width,
                    prefix);
                TankPattonFamilyDetails.AddSight(
                    turret,
                    color,
                    new Vector3(0.72f, roof + 0.07f, 0.85f),
                    new Vector3(0.32f, 0.26f, 0.34f),
                    prefix + "-TTS");
                TankDetailGeometry.Part(
                    prefix + "-CrosswindSensor",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(0.35f, roof + 0.24f, -1.1f),
                    new Vector3(0.035f, 0.34f, 0.035f),
                    TankPattonFamilyDetails.Gunmetal());
            }
            else
            {
                TankPattonRoofEquipment.AddCheekCourse(
                    turret,
                    color,
                    roof,
                    width,
                    prefix,
                    3,
                    2);
            }
        }

        private static void AddM60A2(
            Transform turret,
            Color color,
            float roof,
            float width)
        {
            const string Prefix = "Patton-M60A2";
            TankPattonRoofEquipment.AddCupola(
                turret,
                color,
                roof,
                new Vector3(-0.45f, 0.04f, -0.45f),
                Prefix);
            TankPattonFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(-0.07f, roof + 0.22f, -0.02f),
                new Vector3(0.4f, 0.28f, 0.26f),
                Prefix + "-M28");
            TankPattonRoofEquipment.AddRemoteWeaponStation(
                turret,
                color,
                roof,
                Prefix);
            TankPattonRoofEquipment.AddCheekCourse(
                turret,
                color,
                roof,
                width,
                Prefix,
                5,
                2);
            TankPattonRoofEquipment.AddSmokeBanks(
                turret,
                color,
                roof,
                width,
                Prefix);
            TankPattonRoofEquipment.AddRack(
                turret,
                color,
                width * 0.72f,
                0.5f,
                roof - 0.18f,
                -1.78f,
                Prefix);
            TankPattonRoofEquipment.AddTwinAntennas(
                turret,
                roof,
                width,
                Prefix);
        }

    }
}
