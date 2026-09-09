using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankM1A2RoofWeaponDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            AddCommanderStation(
                turret,
                definition.id,
                color);
            AddLoaderStation(
                turret,
                definition.id,
                color);
        }

        private static void AddCommanderStation(
            Transform turret,
            string id,
            Color color)
        {
            bool tusk = id == "m1a2_tusk";
            bool sepV2 = id == "m1a2_sepv2";
            bool sepV3 = id == "m1a2_sepv3";
            float x = -0.72f;
            float z = 0.04f;
            float pedestalY =
                sepV2 ? 1.14f :
                sepV3 ? 1.02f :
                tusk ? 1.08f : 1.1f;
            TankDetailGeometry.Part(
                "Painted-M1A2-CrowsSlewRing",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, 0.9f, z),
                new Vector3(0.32f, 0.08f, 0.32f),
                color * 0.52f);
            TankDetailGeometry.Part(
                "Painted-M1A2-CrowsPedestal",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, pedestalY, z),
                new Vector3(0.21f, pedestalY - 0.88f, 0.21f),
                color * 0.56f);

            float headY =
                sepV2 ? 1.48f :
                sepV3 ? 1.29f :
                tusk ? 1.38f : 1.4f;
            Vector3 headScale =
                sepV3
                    ? new Vector3(0.62f, 0.2f, 0.38f)
                    : sepV2
                        ? new Vector3(0.48f, 0.38f, 0.38f)
                        : new Vector3(0.52f, 0.31f, 0.38f);
            Part(
                sepV3
                    ? "Painted-M1A2-CrowsLowProfileHead"
                    : sepV2
                        ? "Painted-M1A2-CrowsArmoredHead"
                        : tusk
                            ? "Painted-M1A2-CrowsCompactHead"
                            : "Painted-M1A2-CrowsStandardHead",
                turret,
                new Vector3(x, headY, z + 0.12f),
                headScale,
                color * 0.58f);
            for (int channel = -1; channel <= 1; channel++)
            {
                Part("M1A2-CrowsAperture", turret,
                    new Vector3(
                        x + channel * 0.145f,
                        headY,
                        z + 0.325f),
                    new Vector3(
                        channel == 0 ? 0.09f : 0.075f,
                        channel == 0 ? 0.12f : 0.09f,
                        0.025f),
                    TankM1A2FamilyDetails.Glass());
            }

            float receiverY =
                headY + (sepV3 ? 0.19f : 0.25f);
            Part("M1A2-CrowsReceiver", turret,
                new Vector3(x, receiverY, z + 0.46f),
                new Vector3(0.18f, 0.16f, 0.48f),
                TankM1A2FamilyDetails.Gunmetal());
            AddAxial("M1A2-CrowsM2Barrel", turret,
                x, receiverY, z + 1.06f,
                0.021f, 0.82f,
                TankM1A2FamilyDetails.Dark());
            Part("M1A2-CrowsAmmoCan", turret,
                new Vector3(x - 0.23f, receiverY - 0.02f, z + 0.38f),
                new Vector3(0.2f, 0.22f, 0.3f),
                TankM1A2FamilyDetails.Gunmetal());
            Part("M1A2-CrowsFeedChute", turret,
                new Vector3(x - 0.13f, receiverY + 0.01f, z + 0.45f),
                new Vector3(0.1f, 0.08f, 0.24f),
                color * 0.45f);
            Part("M1A2-CrowsIrPod", turret,
                new Vector3(x + 0.23f, receiverY - 0.04f, z + 0.39f),
                new Vector3(0.12f, 0.1f, 0.17f),
                TankM1A2FamilyDetails.Dark());

            if (tusk)
                AddTuskCrowsArmor(
                    turret,
                    color,
                    x,
                    headY);
            else if (sepV2)
                AddSepV2CrowsArmor(
                    turret,
                    color,
                    x,
                    headY);
        }

        private static void AddTuskCrowsArmor(
            Transform turret,
            Color color,
            float x,
            float y)
        {
            for (int side = -1; side <= 1; side += 2)
                Part("Painted-M1A2-TuskCrowsArmorWing", turret,
                    new Vector3(x + side * 0.31f, y, 0.13f),
                    new Vector3(0.08f, 0.39f, 0.48f),
                    color * 0.62f);
            Part("Painted-M1A2-TuskCrowsArmorCrown", turret,
                new Vector3(x, y + 0.22f, 0.13f),
                new Vector3(0.69f, 0.06f, 0.48f),
                color * 0.65f);
            Part("Painted-M1A2-TuskCrowsArmorRear", turret,
                new Vector3(x, y, -0.13f),
                new Vector3(0.69f, 0.39f, 0.06f),
                color * 0.56f);
        }

        private static void AddSepV2CrowsArmor(
            Transform turret,
            Color color,
            float x,
            float y)
        {
            Part("Painted-M1A2-SepV2-CrowsCrown", turret,
                new Vector3(x, y + 0.23f, 0.14f),
                new Vector3(0.58f, 0.06f, 0.45f),
                color * 0.64f);
            Part("Painted-M1A2-SepV2-CrowsBrow", turret,
                new Vector3(x, y + 0.14f, 0.35f),
                new Vector3(0.54f, 0.13f, 0.06f),
                color * 0.6f);
        }

        private static void AddLoaderStation(
            Transform turret,
            string id,
            Color color)
        {
            if (id == "m1a2_tusk")
            {
                AddTuskLags(turret, color);
                return;
            }

            float x = 0.8f;
            bool sepV2 = id == "m1a2_sepv2";
            bool sepV3 = id == "m1a2_sepv3";
            Part("Painted-M1A2-LoaderSkate", turret,
                new Vector3(x, 0.91f, -0.04f),
                new Vector3(0.68f, 0.06f, 0.68f),
                color * 0.54f);
            float y = sepV2 ? 1.19f : 1.11f;
            Part(
                sepV2
                    ? "M1A2-SepV2-LoaderM2Receiver"
                    : "M1A2-LoaderM2Receiver",
                turret,
                new Vector3(x, y, 0.27f),
                new Vector3(
                    sepV2 ? 0.18f : 0.15f,
                    sepV2 ? 0.16f : 0.13f,
                    sepV2 ? 0.46f : 0.39f),
                TankM1A2FamilyDetails.Gunmetal());
            AddAxial(
                sepV2
                    ? "M1A2-SepV2-LoaderM2Barrel"
                    : "M1A2-LoaderM2Barrel",
                turret,
                x,
                y,
                0.86f,
                sepV2 ? 0.021f : 0.018f,
                sepV2 ? 0.8f : 0.68f,
                TankM1A2FamilyDetails.Dark());
            Part("M1A2-LoaderAmmoCan", turret,
                new Vector3(x + 0.2f, y - 0.02f, 0.2f),
                new Vector3(0.17f, 0.18f, 0.28f),
                TankM1A2FamilyDetails.Gunmetal());
            AddLoaderShield(
                turret,
                color,
                x,
                y,
                sepV2,
                sepV3);
        }

        private static void AddLoaderShield(
            Transform turret,
            Color color,
            float x,
            float y,
            bool armored,
            bool low)
        {
            float height =
                armored ? 0.42f : low ? 0.24f : 0.31f;
            float centerY =
                armored ? y + 0.02f : y - 0.03f;
            for (int side = -1; side <= 1; side += 2)
                Part(
                    armored
                        ? "Painted-M1A2-SepV2-LoaderShield"
                        : low
                            ? "Painted-M1A2-SepV3-LoaderShield"
                            : "Painted-M1A2-LoaderSplitShield",
                    turret,
                    new Vector3(
                        x + side * 0.19f,
                        centerY,
                        0.36f),
                    new Vector3(
                        armored ? 0.18f : 0.14f,
                        height,
                        0.08f),
                    color * 0.58f);
        }

        private static void AddTuskLags(
            Transform turret,
            Color color)
        {
            const float x = 1.08f;
            Part("Painted-M1A2-TuskLagsFront", turret,
                new Vector3(x, 1.15f, 0.32f),
                new Vector3(0.74f, 0.42f, 0.06f),
                color * 0.6f);
            Part("Painted-M1A2-TuskLagsOuterWing", turret,
                new Vector3(x - 0.38f, 1.15f, 0.05f),
                new Vector3(0.07f, 0.48f, 0.55f),
                color * 0.57f);
            Transform inner = Part(
                "Painted-M1A2-TuskLagsInnerWing",
                turret,
                new Vector3(x + 0.38f, 1.14f, 0.2f),
                new Vector3(0.42f, 0.46f, 0.07f),
                color * 0.57f);
            inner.localRotation =
                Quaternion.Euler(0f, -28f, 0f);
            Part("M1A2-TuskLagsWindow", turret,
                new Vector3(x, 1.18f, 0.36f),
                new Vector3(0.26f, 0.1f, 0.025f),
                TankM1A2FamilyDetails.Glass());
            Part("M1A2-TuskLoaderM2Receiver", turret,
                new Vector3(x, 1.32f, 0.2f),
                new Vector3(0.16f, 0.14f, 0.42f),
                TankM1A2FamilyDetails.Gunmetal());
            AddAxial("M1A2-TuskLoaderM2Barrel", turret,
                x, 1.32f, 0.84f,
                0.02f, 0.78f,
                TankM1A2FamilyDetails.Dark());
            Part("M1A2-TuskLoaderAmmoCan", turret,
                new Vector3(x + 0.21f, 1.28f, 0.17f),
                new Vector3(0.18f, 0.2f, 0.3f),
                TankM1A2FamilyDetails.Gunmetal());
        }

        private static void AddAxial(
            string name,
            Transform parent,
            float x,
            float y,
            float z,
            float radius,
            float length,
            Color color)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(x, y, z),
                    new Vector3(radius, length * 0.5f, radius),
                    color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
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
