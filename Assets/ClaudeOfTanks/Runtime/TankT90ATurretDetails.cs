using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90ATurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            AddWeldedFoundation(turret, color);
            AddKontakt5Chevron(turret, color);
            AddEssa(turret, color);
            TankT90ATurretRevolutionDetails.Build(turret, color);
            AddRemainingRoofEquipment(turret);
            AddBustle(turret, color);
        }

        private static void AddWeldedFoundation(
            Transform turret,
            Color color)
        {
            float width = 1.55f;
            TankVariableBaseTurretShapeFactory.Build(
                "Painted-T90A-WeldedCheekFoundation",
                turret,
                new[]
                {
                    new Vector2(-width * 0.15f, 1.26f),
                    new Vector2(width * 0.15f, 1.26f),
                    new Vector2(0.98f, 1.26f),
                    new Vector2(1.19f, 1.44f),
                    new Vector2(1.2985f, 1.377f),
                    new Vector2(1.4054f, 1.27f),
                    new Vector2(width * 0.97f, 1.12f),
                    new Vector2(width, 0.55f),
                    new Vector2(1.44f, -0.395f),
                    new Vector2(1.09f, -0.8f),
                    new Vector2(-1.09f, -0.8f),
                    new Vector2(-1.44f, -0.395f),
                    new Vector2(-width, 0.55f),
                    new Vector2(-width * 0.97f, 1.12f),
                    new Vector2(-1.4054f, 1.27f),
                    new Vector2(-1.2985f, 1.377f),
                    new Vector2(-1.19f, 1.44f),
                    new Vector2(-0.98f, 1.26f)
                },
                0.515f,
                1.02f,
                0.78f,
                z => z <= 0.5f
                    ? 0f
                    : z >= 0.55f
                        ? 0.08f
                        : (z - 0.5f) * 1.6f,
                new[] { 0.5f, 0.55f },
                color * 0.63f);
            Part(
                "Painted-T90A-FoundationShelf",
                turret,
                new Vector3(0f, 0.3125f, -0.95f),
                new Vector3(1.9f, 0.425f, 0.7f),
                color * 0.63f);
            Part(
                "Painted-T90A-FoundationCrown",
                turret,
                new Vector3(0f, 0.55f, -0.025f),
                new Vector3(1.24f, 0.07f, 1.05f),
                color * 0.63f);
        }

        private static void AddKontakt5Chevron(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform upper = Part(
                    "Painted-T90A-K5ChevronUpper",
                    turret,
                    new Vector3(side * 0.58f, 0.38f, 1.34f),
                    new Vector3(0.72f, 0.18f, 0.62f),
                    color * 0.53f);
                upper.localRotation =
                    Quaternion.Euler(-24f, -side * 33f, 0f);
                Transform lower = Part(
                    "Painted-T90A-K5ChevronLower",
                    turret,
                    new Vector3(side * 0.92f, 0.25f, 0.98f),
                    new Vector3(0.86f, 0.16f, 0.42f),
                    color * 0.52f);
                lower.localRotation =
                    Quaternion.Euler(-18f, -side * 47f, 0f);
                for (int seam = 0;
                    seam < 5;
                    seam++)
                {
                    Part(
                        "T90A-K5ChevronSeam",
                        turret,
                        new Vector3(
                            side * (0.34f + seam * 0.13f),
                            0.43f - seam * 0.008f,
                            1.6f - seam * 0.12f),
                        new Vector3(0.026f, 0.16f, 0.18f),
                        TankT90AFamilyDetails.Dark());
                }
                Part(
                    "Painted-T90A-ShtoraPedestal",
                    turret,
                    new Vector3(side * 0.7f, 0.27f, 1.58f),
                    new Vector3(0.32f, 0.34f, 0.38f),
                    color * 0.5f);
            }
            Part(
                "T90A-K5MantletVertex",
                turret,
                new Vector3(0f, 0.32f, 1.65f),
                new Vector3(0.48f, 0.24f, 0.065f),
                TankT90AFamilyDetails.Dark());
        }

        private static void AddEssa(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T90A-ESSAHousing",
                turret,
                new Vector3(-0.455f, 0.595f, 0.19f),
                new Vector3(0.25f, 0.3f, 0.82f),
                color * 0.6f);
            Part(
                "T90A-ESSALens",
                turret,
                new Vector3(-0.455f, 0.625f, 0.615f),
                new Vector3(0.2f, 0.11f, 0.018f),
                TankT90AFamilyDetails.Glass());
            Part(
                "T90A-ESSAHoodLip",
                turret,
                new Vector3(-0.455f, 0.735f, 0.62f),
                new Vector3(0.22f, 0.025f, 0.05f),
                TankT90AFamilyDetails.Dark());
        }

        private static void AddRemainingRoofEquipment(
            Transform turret)
        {
            Part(
                "T90A-RemoteNsvtReceiver",
                turret,
                new Vector3(-0.35f, 0.92f, -0.45f),
                new Vector3(0.24f, 0.18f, 0.44f),
                TankT90AFamilyDetails.Dark());
            Part(
                "T90A-RemoteNsvtBarrel",
                turret,
                new Vector3(-0.35f, 0.95f, 0.02f),
                new Vector3(0.04f, 0.04f, 0.82f),
                TankT90AFamilyDetails.Dark());
            Part(
                "T90A-TKNHead",
                turret,
                new Vector3(0.31f, 0.84f, -1.1f),
                new Vector3(0.13f, 0.1f, 0.13f),
                TankT90AFamilyDetails.Dark());
        }

        private static void AddBustle(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T90A-BustleBin",
                turret,
                new Vector3(0f, 0.34f, -1.62f),
                new Vector3(1.86f, 0.46f, 0.5f),
                color * 0.58f);
            Part(
                "Painted-T90A-AsymmetricRightBin",
                turret,
                new Vector3(0.92f, 0.45f, -0.44f),
                new Vector3(0.42f, 0.3f, 0.62f),
                color * 0.58f);
        }

        private static Transform Part(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            Transform box = TankShapeFactory.BoxPart(
                name,
                parent,
                scale,
                color);
            box.localPosition = position;
            return box;
        }
    }
}
