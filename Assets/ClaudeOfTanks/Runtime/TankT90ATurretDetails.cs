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
            AddShtoraAndEssa(turret, color);
            AddRoofStations(turret, color);
            AddBustleAndSmoke(turret, color);
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
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.3125f, -0.95f),
                new Vector3(1.9f, 0.425f, 0.7f),
                color * 0.63f);
            Part(
                "Painted-T90A-FoundationCrown",
                PrimitiveType.Cube,
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
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.58f, 0.38f, 1.34f),
                    new Vector3(0.72f, 0.18f, 0.62f),
                    color * 0.53f);
                upper.localRotation =
                    Quaternion.Euler(-24f, -side * 33f, 0f);
                Transform lower = Part(
                    "Painted-T90A-K5ChevronLower",
                    PrimitiveType.Cube,
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
                        PrimitiveType.Cube,
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
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(side * 0.7f, 0.27f, 1.58f),
                    new Vector3(0.32f, 0.34f, 0.38f),
                    color * 0.5f);
            }
            Part(
                "T90A-K5MantletVertex",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.32f, 1.65f),
                new Vector3(0.48f, 0.24f, 0.065f),
                TankT90AFamilyDetails.Dark());
        }

        private static void AddShtoraAndEssa(
            Transform turret,
            Color color)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform housing = Part(
                    "Painted-T90A-ShtoraHousing",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.7f, 0.52f, 1.86f),
                    new Vector3(0.15f, 0.12f, 0.15f),
                    color * 0.58f);
                housing.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                Transform lens = Part(
                    "T90A-ShtoraLens",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(side * 0.7f, 0.52f, 1.99f),
                    new Vector3(0.1f, 0.025f, 0.1f),
                    TankT90AFamilyDetails.ShtoraGlass());
                lens.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            Part(
                "Painted-T90A-ESSAHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.455f, 0.595f, 0.19f),
                new Vector3(0.25f, 0.3f, 0.82f),
                color * 0.6f);
            Part(
                "T90A-ESSALens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.455f, 0.625f, 0.615f),
                new Vector3(0.2f, 0.11f, 0.018f),
                TankT90AFamilyDetails.Glass());
            Part(
                "T90A-ESSAHoodLip",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.455f, 0.735f, 0.62f),
                new Vector3(0.22f, 0.025f, 0.05f),
                TankT90AFamilyDetails.Dark());
        }

        private static void AddRoofStations(
            Transform turret,
            Color color)
        {
            Vector3[] stations =
            {
                new Vector3(-0.35f, 0.58f, -0.48f),
                new Vector3(0.52f, 0.58f, -0.42f)
            };
            for (int index = 0;
                index < stations.Length;
                index++)
            {
                Part(
                    "Painted-T90A-RoofCupola",
                    PrimitiveType.Cylinder,
                    turret,
                    stations[index],
                    new Vector3(0.255f, 0.14f, 0.255f),
                    color * 0.6f);
                Part(
                    "T90A-CupolaPeriscope",
                    PrimitiveType.Cube,
                    turret,
                    stations[index] + new Vector3(0f, 0.16f, -0.17f),
                    new Vector3(0.055f, 0.025f, 0.105f),
                    TankT90AFamilyDetails.Dark());
            }
            Part(
                "T90A-RemoteNsvtReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.35f, 0.92f, -0.45f),
                new Vector3(0.24f, 0.18f, 0.44f),
                TankT90AFamilyDetails.Dark());
            Part(
                "T90A-RemoteNsvtBarrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.35f, 0.95f, 0.02f),
                new Vector3(0.04f, 0.04f, 0.82f),
                TankT90AFamilyDetails.Dark());
            Part(
                "T90A-CrosswindMast",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.23f, 1.08f, -1.28f),
                new Vector3(0.022f, 0.42f, 0.022f),
                TankT90AFamilyDetails.Dark());
            Part(
                "T90A-TKNHead",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.31f, 0.84f, -1.1f),
                new Vector3(0.13f, 0.1f, 0.13f),
                TankT90AFamilyDetails.Dark());
        }

        private static void AddBustleAndSmoke(
            Transform turret,
            Color color)
        {
            Part(
                "Painted-T90A-BustleBin",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.34f, -1.62f),
                new Vector3(1.86f, 0.46f, 0.5f),
                color * 0.58f);
            Part(
                "Painted-T90A-AsymmetricRightBin",
                PrimitiveType.Cube,
                turret,
                new Vector3(0.92f, 0.45f, -0.44f),
                new Vector3(0.42f, 0.3f, 0.62f),
                color * 0.58f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < 6;
                    tube++)
                {
                    Transform launcher = Part(
                        "Painted-T90A-SmokeLauncher",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * (0.98f + tube * 0.045f),
                            0.42f + tube * 0.018f,
                            -0.18f - tube * 0.035f),
                        new Vector3(0.035f, 0.11f, 0.035f),
                        color * 0.52f);
                    launcher.localRotation =
                        Quaternion.Euler(62f, 0f, side * 18f);
                }
            }
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            if (type == PrimitiveType.Cube)
            {
                Transform box = TankShapeFactory.BoxPart(
                    name,
                    parent,
                    scale,
                    color);
                box.localPosition = position;
                return box;
            }
            return TankDetailGeometry.Part(
                name,
                type,
                parent,
                position,
                scale,
                color);
        }
    }
}
