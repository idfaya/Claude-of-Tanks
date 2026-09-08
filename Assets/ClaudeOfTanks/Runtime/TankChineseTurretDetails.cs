using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankChineseTurretDetails
    {
        public static void Build(
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
                    width * 0.35f);
            AddHatches(
                turret,
                definition.id,
                color,
                width,
                roof);
            switch (definition.id)
            {
                case "type59":
                    AddType59(
                        turret,
                        color,
                        width,
                        roof,
                        rear,
                        halfWidth);
                    break;
                case "ztz85_iii":
                    AddZtz85(
                        turret,
                        color,
                        width,
                        roof,
                        rear,
                        halfWidth);
                    break;
                case "type99a":
                    AddType99(
                        turret,
                        color,
                        width,
                        roof,
                        rear,
                        halfWidth);
                    break;
                case "ztz99a2":
                    AddZtz99A2(
                        turret,
                        color,
                        width,
                        roof,
                        rear,
                        halfWidth);
                    break;
                case "vt4a1":
                    AddVt4(
                        turret,
                        color,
                        width,
                        roof,
                        rear,
                        halfWidth);
                    break;
            }
        }

        private static void AddHatches(
            Transform turret,
            string id,
            Color color,
            float width,
            float roof)
        {
            float xScale =
                id == "type59" ? 0.19f : 0.14f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float z =
                    id == "type59"
                        ? (side < 0 ? -0.1f : 0.12f)
                        : (side < 0 ? -0.58f : -0.42f);
                TankDetailGeometry.Part(
                    "Painted-Chinese-Hatch",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * xScale,
                        roof + 0.035f,
                        z),
                    new Vector3(0.22f, 0.035f, 0.22f),
                    color * 0.82f);
                TankDetailGeometry.Part(
                    "Chinese-HatchHandle",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * xScale,
                        roof + 0.078f,
                        z),
                    new Vector3(0.18f, 0.02f, 0.04f),
                    TankChineseFamilyDetails.Gunmetal());
            }
        }

        private static void AddType59(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear,
            float halfWidth)
        {
            TankChineseFamilyDetails.AddSmokeBanks(
                turret,
                color,
                roof,
                halfWidth,
                0.16f,
                8,
                "Chinese-Type59");
            TankChineseFamilyDetails.AddAntennas(
                turret,
                color,
                roof,
                rear,
                width,
                2,
                "Chinese-Type59");
            TankChineseRoofEquipment.AddVisionRing(
                turret,
                -width * 0.19f,
                -0.09f,
                roof,
                6,
                "Chinese-Type59-CommanderVision");
            TankChineseRoofEquipment.AddVentilator(
                turret,
                color,
                roof,
                "Chinese-Type59");
            TankChineseFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.18f,
                    0.08f,
                    0.23f),
                "Chinese-Type59-DShK",
                true,
                true);
            TankChineseFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.19f,
                    0.09f,
                    0.02f),
                "Chinese-Type59-CommanderMG",
                false,
                false);
        }

        private static void AddZtz85(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear,
            float halfWidth)
        {
            TankChineseFamilyDetails.AddSmokeBanks(
                turret,
                color,
                roof,
                halfWidth,
                0.58f,
                8,
                "Chinese-ZTZ85III");
            TankChineseFamilyDetails.AddAntennas(
                turret,
                color,
                roof,
                rear,
                width,
                2,
                "Chinese-ZTZ85III");
            TankChineseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.1f,
                    roof + 0.18f,
                    0.46f),
                new Vector3(0.3f, 0.24f, 0.34f),
                "Chinese-ZTZ85III-ISFCS212");
            TankChineseFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.16f,
                    0.07f,
                    -0.42f),
                "Chinese-ZTZ85III-W85",
                true,
                false);
            TankChineseRoofEquipment.AddRadioMast(
                turret,
                color,
                new Vector3(
                    -width * 0.25f,
                    roof + 0.78f,
                    rear - 0.12f),
                1.34f,
                "Chinese-ZTZ85III");
        }

        private static void AddType99(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear,
            float halfWidth)
        {
            TankChineseFamilyDetails.AddSmokeBanks(
                turret,
                color,
                roof,
                halfWidth,
                0.42f,
                20,
                "Chinese-Type99A");
            TankChineseFamilyDetails.AddAntennas(
                turret,
                color,
                roof,
                rear,
                width,
                2,
                "Chinese-Type99A");
            TankChineseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.26f,
                    roof + 0.5f,
                    -0.75f),
                new Vector3(0.48f, 0.8f, 0.5f),
                "Chinese-Type99A-GunnerTower");
            TankChineseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.19f,
                    roof + 0.31f,
                    -1.16f),
                new Vector3(0.3f, 0.52f, 0.3f),
                "Chinese-Type99A-PanoramicSight");
            TankChineseRoofEquipment.AddWarningHeads(
                turret,
                color,
                width,
                roof,
                0.34f,
                "Chinese-Type99A");
            TankChineseFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.14f,
                    0.08f,
                    -0.18f),
                "Chinese-Type99A-QJC88",
                true,
                false);
        }

        private static void AddZtz99A2(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear,
            float halfWidth)
        {
            TankChineseFamilyDetails.AddSmokeBanks(
                turret,
                color,
                roof,
                halfWidth,
                0.38f,
                10,
                "Chinese-ZTZ99A2");
            TankChineseFamilyDetails.AddAntennas(
                turret,
                color,
                roof,
                rear,
                width,
                2,
                "Chinese-ZTZ99A2");
            TankChineseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.15f,
                    roof + 0.25f,
                    0.3f),
                new Vector3(0.36f, 0.42f, 0.32f),
                "Chinese-ZTZ99A2-PanoramicSight");
            TankChineseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.12f,
                    roof + 0.17f,
                    0.42f),
                new Vector3(0.42f, 0.28f, 0.4f),
                "Chinese-ZTZ99A2-GunnerSight");
            TankChineseRoofEquipment.AddWarningHeads(
                turret,
                color,
                width,
                roof,
                0.12f,
                "Chinese-ZTZ99A2");
            TankChineseFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.14f,
                    0.08f,
                    -0.8f),
                "Chinese-ZTZ99A2-W85",
                true,
                false);
        }

        private static void AddVt4(
            Transform turret,
            Color color,
            float width,
            float roof,
            float rear,
            float halfWidth)
        {
            TankChineseFamilyDetails.AddSmokeBanks(
                turret,
                color,
                roof,
                halfWidth,
                -0.08f,
                12,
                "Chinese-VT4A1");
            TankChineseFamilyDetails.AddAntennas(
                turret,
                color,
                roof,
                rear,
                width,
                2,
                "Chinese-VT4A1");
            TankChineseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.14f,
                    roof + 0.18f,
                    0.16f),
                new Vector3(0.44f, 0.34f, 0.4f),
                "Chinese-VT4A1-GunnerSight");
            TankChineseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.13f,
                    roof + 0.12f,
                    -0.02f),
                new Vector3(0.48f, 0.18f, 0.43f),
                "Chinese-VT4A1-PanoramicSight");
            TankChineseRoofEquipment.AddWarningHeads(
                turret,
                color,
                width,
                roof,
                -1.5f,
                "Chinese-VT4A1");
            TankChineseRoofEquipment.AddRemoteWeaponStation(
                turret,
                color,
                width * 0.1f,
                roof,
                -0.86f);
        }

    }
}
