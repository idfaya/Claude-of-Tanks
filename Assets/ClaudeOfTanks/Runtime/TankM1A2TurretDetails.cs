using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankM1A2TurretDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            AddCitv(turret, color,
                definition.id == "m1a2_sepv3");
            AddSmokeLaunchers(turret, color);
            AddCommonRackLoad(turret, definition.id, color);
            switch (definition.id)
            {
                case "m1a2_tusk":
                    AddTuskElectronics(turret, color);
                    break;
                case "m1a2_sepv2":
                    AddSepV2Systems(turret, color);
                    break;
                case "m1a2_sepv3":
                    AddSepV3Systems(turret, color);
                    break;
            }
        }

        private static void AddCitv(
            Transform turret,
            Color color,
            bool enlarged)
        {
            float scale = enlarged ? 1.16f : 1f;
            TankDetailGeometry.Part(
                "Painted-M1A2-CitvBearing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.16f, 0.82f, 0.26f),
                new Vector3(0.15f, 0.1f, 0.15f),
                color * 0.5f);
            Part("Painted-M1A2-CitvHead", turret,
                new Vector3(-0.16f, 1.02f, 0.26f),
                new Vector3(0.32f * scale, 0.3f, 0.25f),
                color * 0.58f);
            Part("M1A2-CitvThermalWindow", turret,
                new Vector3(-0.16f, 1.02f, 0.397f),
                new Vector3(0.22f * scale, 0.13f, 0.025f),
                TankM1A2FamilyDetails.Glass());
            Part("Painted-M1A2-CitvCrown", turret,
                new Vector3(-0.16f, 1.18f, 0.26f),
                new Vector3(0.34f * scale, 0.035f, 0.27f),
                color * 0.7f);
        }

        private static void AddSmokeLaunchers(
            Transform turret,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Painted-M1A2-SmokeBank", turret,
                    new Vector3(side * 1.42f, 0.36f, 0.57f),
                    new Vector3(0.16f, 0.36f, 0.62f),
                    color * 0.5f);
                for (int tube = 0; tube < 6; tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-M1A2-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side * 1.5f,
                                0.19f + (tube / 3) * 0.19f,
                                0.38f + (tube % 3) * 0.18f),
                            new Vector3(0.045f, 0.16f, 0.045f),
                            color * 0.43f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            68f,
                            0f,
                            side * -18f);
                }
            }
        }

        private static void AddCommonRackLoad(
            Transform turret,
            string id,
            Color color)
        {
            Part("Painted-M1A2-RackRelayCase", turret,
                new Vector3(-0.24f, 0.83f, -1.3f),
                new Vector3(0.34f, 0.12f, 0.24f),
                color * 0.52f);
            Part("M1A2-RackRelayLatch", turret,
                new Vector3(-0.065f, 0.83f, -1.3f),
                new Vector3(0.025f, 0.05f, 0.025f),
                TankM1A2FamilyDetails.Dark());
            Part("Painted-M1A2-RackBag", turret,
                new Vector3(
                    id == "m1a2_sepv2" ? -0.66f : 0.74f,
                    0.82f,
                    -3.08f),
                new Vector3(0.26f, 0.16f, 0.24f),
                color * 0.5f);
            Part("M1A2-RackBagStrap", turret,
                new Vector3(
                    id == "m1a2_sepv2" ? -0.66f : 0.74f,
                    0.91f,
                    -3.08f),
                new Vector3(0.27f, 0.025f, 0.06f),
                TankM1A2FamilyDetails.Gunmetal());
        }

        private static void AddTuskElectronics(
            Transform turret,
            Color color)
        {
            Part("Painted-M1A2-TuskUrbanElectronics", turret,
                new Vector3(0.3f, 0.9f, -0.42f),
                new Vector3(0.48f, 0.22f, 0.42f),
                color * 0.56f);
            for (int channel = -1; channel <= 1; channel += 2)
            {
                Part("M1A2-TuskUrbanOpticBezel", turret,
                    new Vector3(0.3f + channel * 0.1f, 1.01f, -0.19f),
                    new Vector3(0.13f, 0.075f, 0.05f),
                    TankM1A2FamilyDetails.Dark());
                Part("M1A2-TuskUrbanOptic", turret,
                    new Vector3(0.3f + channel * 0.1f, 1.01f, -0.16f),
                    new Vector3(0.1f, 0.05f, 0.018f),
                    TankM1A2FamilyDetails.Glass());
            }
            for (int side = -1; side <= 1; side += 2)
                Part("M1A2-TuskWarningMast", turret,
                    new Vector3(side * 1.07f, 1.08f, -0.92f),
                    new Vector3(0.04f, side < 0 ? 0.34f : 0.28f, 0.04f),
                    TankM1A2FamilyDetails.Dark());
        }

        private static void AddSepV2Systems(
            Transform turret,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
                Part("Painted-M1A2-SepV2-CipPanel", turret,
                    new Vector3(side * 1.56f, 0.37f, 0.56f),
                    new Vector3(0.035f, 0.4f, 0.5f),
                    color * 0.82f);
            Part("Painted-M1A2-SepV2-ProcessingBox", turret,
                new Vector3(0.34f, 0.91f, -0.42f),
                new Vector3(0.56f, 0.24f, 0.46f),
                color * 0.57f);
            for (int channel = -1; channel <= 1; channel += 2)
            {
                Part("M1A2-SepV2-OpticBezel", turret,
                    new Vector3(0.34f + channel * 0.135f, 1.03f, -0.17f),
                    new Vector3(0.18f, 0.11f, 0.05f),
                    TankM1A2FamilyDetails.Dark());
                Part("M1A2-SepV2-Optic", turret,
                    new Vector3(0.34f + channel * 0.135f, 1.03f, -0.14f),
                    new Vector3(0.15f, 0.075f, 0.018f),
                    TankM1A2FamilyDetails.Glass());
            }
            Part("Painted-M1A2-SepV2-AmmunitionCrate", turret,
                new Vector3(0.38f, 0.6f, -3.14f),
                new Vector3(0.46f, 0.41f, 0.3f),
                color * 0.5f);
            for (int slat = -1; slat <= 1; slat += 2)
                Part("Painted-M1A2-SepV2-CrateLidSlat", turret,
                    new Vector3(0.38f, 0.82f, -3.14f + slat * 0.07f),
                    new Vector3(0.42f, 0.025f, 0.11f),
                    color * 0.7f);
            Part("M1A2-SepV2-CrateStrap", turret,
                new Vector3(0.38f, 0.835f, -3.14f),
                new Vector3(0.04f, 0.03f, 0.29f),
                TankM1A2FamilyDetails.Dark());
        }

        private static void AddSepV3Systems(
            Transform turret,
            Color color)
        {
            AddTrophy(turret, color);
            Part("Painted-M1A2-SepV3-BmsHousing", turret,
                new Vector3(0.32f, 0.91f, -0.42f),
                new Vector3(0.58f, 0.25f, 0.5f),
                color * 0.58f);
            for (int channel = -1; channel <= 1; channel += 2)
            {
                Part("M1A2-SepV3-IfLirBezel", turret,
                    new Vector3(0.32f + channel * 0.155f, 1.04f, -0.145f),
                    new Vector3(0.2f, 0.12f, 0.05f),
                    TankM1A2FamilyDetails.Dark());
                Part("M1A2-SepV3-IfLirWindow", turret,
                    new Vector3(0.32f + channel * 0.155f, 1.04f, -0.115f),
                    new Vector3(0.17f, 0.08f, 0.018f),
                    TankM1A2FamilyDetails.Glass());
            }
            for (int box = 0; box < 2; box++)
                Part("M1A2-SepV3-AdlBox", turret,
                    new Vector3(0.52f, 0.91f, -0.4f - box * 0.18f),
                    new Vector3(0.22f, 0.08f, box == 0 ? 0.16f : 0.11f),
                    TankM1A2FamilyDetails.Dark());
            Part("M1A2-SepV3-AdlConduit", turret,
                new Vector3(0.52f, 0.87f, -0.49f),
                new Vector3(0.035f, 0.035f, 0.1f),
                TankM1A2FamilyDetails.Gunmetal());
            AddIffPanels(turret, color);
            Part("Painted-M1A2-SepV3-GunnerSightWingL", turret,
                new Vector3(0.5f, 0.8f, 0.78f),
                new Vector3(0.05f, 0.13f, 0.2f),
                color * 0.58f);
            Part("Painted-M1A2-SepV3-GunnerSightWingR", turret,
                new Vector3(1.06f, 0.8f, 0.78f),
                new Vector3(0.05f, 0.13f, 0.2f),
                color * 0.58f);
            Part("M1A2-SepV3-GunnerSightWindow", turret,
                new Vector3(0.78f, 0.65f, 1.1f),
                new Vector3(0.42f, 0.095f, 0.025f),
                TankM1A2FamilyDetails.Glass());
        }

        private static void AddTrophy(
            Transform turret,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Transform launcher = Part(
                    "Painted-M1A2-SepV3-TrophyLauncher",
                    turret,
                    new Vector3(side * 1.76f, 0.81f, -1.15f),
                    new Vector3(0.28f, 0.44f, 0.62f),
                    color * 0.54f);
                launcher.localRotation =
                    Quaternion.Euler(0f, side * 22f, side * 7f);
                Part("M1A2-SepV3-TrophyCountermeasureFace", turret,
                    new Vector3(side * 1.93f, 0.81f, -1.08f),
                    new Vector3(0.04f, 0.32f, 0.48f),
                    TankM1A2FamilyDetails.Dark());
                for (int end = -1; end <= 1; end += 2)
                {
                    Part("Painted-M1A2-SepV3-TrophyRadarMount", turret,
                        new Vector3(
                            side * 1.78f,
                            0.5f,
                            end > 0 ? 0.65f : -2.68f),
                        new Vector3(0.12f, 0.34f, 0.34f),
                        color * 0.52f);
                    Part("M1A2-SepV3-TrophyRadar", turret,
                        new Vector3(
                            side * 1.85f,
                            0.5f,
                            end > 0 ? 0.65f : -2.68f),
                        new Vector3(0.025f, 0.27f, 0.27f),
                        TankM1A2FamilyDetails.Glass());
                }
            }
        }

        private static void AddIffPanels(
            Transform turret,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
                for (int panel = -1; panel <= 1; panel += 2)
                    Part("Painted-M1A2-SepV3-IffPanel", turret,
                        new Vector3(
                            side * 1.67f,
                            0.36f,
                            0.34f + panel * 0.12f),
                        new Vector3(0.025f, 0.3f, 0.19f),
                        color * 0.78f);
            Part("Painted-M1A2-SepV3-IffPanel", turret,
                new Vector3(-0.16f, 0.72f, -3.49f),
                new Vector3(0.3f, 0.24f, 0.025f),
                color * 0.78f);
        }

        private static Transform Part(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                PrimitiveType.Cube,
                parent,
                position,
                scale,
                color);
        }
    }
}
