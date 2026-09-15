using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSwedishPrimaryHullDetails
    {
        public static void Build(
            Transform root,
            string id,
            Color color)
        {
            if (TankSwedishFamilyDetails.IsCasemate(id))
            {
                AddCasemateHull(root, id, color);
            }
            else if (id == "cv90" || id == "cv90_mkiv")
            {
                AddCv90Hull(root, id, color);
            }
            else if (id == "strv81")
            {
                AddStrv81Hull(root, color);
            }
        }

        private static void AddStrv81Hull(
            Transform root,
            Color color)
        {
            TankHullLoftShapeFactory.Build(
                "Painted-Swedish-Strv81-HullLoft",
                root,
                Curve(
                    -3.75f, 1.58f,
                    -3.13f, 1.74f,
                    -0.40f, 1.74f,
                    1.70f, 1.66f,
                    3.37f, 1.46f,
                    4.07f, 1.09f),
                Curve(
                    -3.75f, 0.71f,
                    -3.15f, 0.53f,
                    3.37f, 0.53f,
                    4.07f, 0.69f),
                Curve(
                    -3.75f, 1.04f,
                    -2.90f, 1.67f,
                    3.37f, 1.67f,
                    4.07f, 1.00f),
                Curve(
                    -3.75f, 0.77f,
                    -2.90f, 1.20f,
                    3.37f, 1.20f,
                    4.07f, 0.77f),
                Curve(
                    -3.75f, 1.40f,
                    -2.90f, 1.50f,
                    3.37f, 1.44f,
                    4.07f, 1.20f),
                color * 0.66f);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int panel = 0; panel < 8; panel++)
                {
                    Transform skirt = TankShapeFactory.BoxPart(
                        "Painted-Swedish-Strv81-SkirtPanel",
                        root,
                        new Vector3(0.065f, 0.58f, 0.82f),
                        color * 0.54f);
                    skirt.localPosition =
                        new Vector3(
                            side * 1.64f,
                            0.92f,
                            -3.20f + (panel + 0.5f) * 0.806f);
                }
            }
        }

        private static void AddCv90Hull(
            Transform root,
            string id,
            Color color)
        {
            bool mkiv = id == "cv90_mkiv";
            string prefix = mkiv
                ? "Swedish-CV90MkIV"
                : "Swedish-CV90";
            Transform belly = TankShapeFactory.BoxPart(
                "Painted-" + prefix + "-ArmoredBelly",
                root,
                mkiv
                    ? new Vector3(2.24f, 1.10f, 6.78f)
                    : new Vector3(2.12f, 1.06f, 6.42f),
                color * 0.62f);
            belly.localPosition = mkiv
                ? new Vector3(0f, 0.75f, -0.12f)
                : new Vector3(0f, 0.77f, -0.10f);

            Transform lowerGlacis =
                TankShapeFactory.OrientedSlabPart(
                    "Painted-" + prefix + "-LowerGlacis",
                    root,
                    mkiv
                        ? new Vector3(-1.02f, 0.31f, 2.29f)
                        : new Vector3(-0.96f, 0.32f, 2.18f),
                    mkiv
                        ? new Vector3(1.02f, 0.31f, 2.29f)
                        : new Vector3(0.96f, 0.32f, 2.18f),
                    mkiv
                        ? new Vector3(0.88f, 0.59f, 3.49f)
                        : new Vector3(0.82f, 0.55f, 3.28f),
                    mkiv
                        ? new Vector3(-0.88f, 0.59f, 3.49f)
                        : new Vector3(-0.82f, 0.55f, 3.28f),
                    mkiv
                        ? new Vector3(-1.12f, 1.26f, 2.45f)
                        : new Vector3(-1.05f, 1.18f, 2.32f),
                    mkiv
                        ? new Vector3(1.12f, 1.26f, 2.45f)
                        : new Vector3(1.05f, 1.18f, 2.32f),
                    mkiv
                        ? new Vector3(1.02f, 1.12f, 3.49f)
                        : new Vector3(0.96f, 1.05f, 3.28f),
                    mkiv
                        ? new Vector3(-1.02f, 1.12f, 3.49f)
                        : new Vector3(-0.96f, 1.05f, 3.28f),
                    color * 0.66f);
            lowerGlacis.localPosition = Vector3.zero;

            TankShapeFactory.OrientedSlabPart(
                "Painted-" + prefix + "-UpperGlacis",
                root,
                mkiv
                    ? new Vector3(-1.02f, 1.07f, 3.49f)
                    : new Vector3(-0.96f, 1.00f, 3.28f),
                mkiv
                    ? new Vector3(1.02f, 1.07f, 3.49f)
                    : new Vector3(0.96f, 1.00f, 3.28f),
                mkiv
                    ? new Vector3(1.61f, 1.78f, 1.77f)
                    : new Vector3(1.49f, 1.66f, 1.82f),
                mkiv
                    ? new Vector3(-1.61f, 1.78f, 1.77f)
                    : new Vector3(-1.49f, 1.66f, 1.82f),
                mkiv
                    ? new Vector3(-1.09f, 1.20f, 3.49f)
                    : new Vector3(-1.02f, 1.12f, 3.28f),
                mkiv
                    ? new Vector3(1.09f, 1.20f, 3.49f)
                    : new Vector3(1.02f, 1.12f, 3.28f),
                mkiv
                    ? new Vector3(1.61f, 1.93f, 1.77f)
                    : new Vector3(1.49f, 1.79f, 1.82f),
                mkiv
                    ? new Vector3(-1.61f, 1.93f, 1.77f)
                    : new Vector3(-1.49f, 1.79f, 1.82f),
                color * 0.70f);

            Vector2[] plan = mkiv
                ? new[]
                {
                    new Vector2(-1.61f, 1.72f),
                    new Vector2(1.61f, 1.72f),
                    new Vector2(1.65f, 1.46f),
                    new Vector2(1.65f, -2.76f),
                    new Vector2(1.50f, -3.55f),
                    new Vector2(1.02f, -3.66f),
                    new Vector2(-1.02f, -3.66f),
                    new Vector2(-1.50f, -3.55f),
                    new Vector2(-1.65f, -2.76f),
                    new Vector2(-1.65f, 1.50f)
                }
                : new[]
                {
                    new Vector2(-1.49f, 1.76f),
                    new Vector2(1.49f, 1.76f),
                    new Vector2(1.52f, 1.50f),
                    new Vector2(1.52f, -3.12f),
                    new Vector2(1.34f, -3.45f),
                    new Vector2(-1.34f, -3.45f),
                    new Vector2(-1.52f, -3.12f),
                    new Vector2(-1.52f, 1.56f)
                };
            float baseY = mkiv ? 1.30f : 1.28f;
            Transform cell = TankShapeFactory.PolyMultiLoftPart(
                "Painted-" + prefix + "-MissionCell",
                root,
                plan,
                new[]
                {
                    new TankShapeLoftRing(
                        baseY,
                        1.00f,
                        Vector2.zero),
                    new TankShapeLoftRing(
                        baseY + (mkiv ? 0.28f : 0.22f),
                        0.96f,
                        Vector2.zero),
                    new TankShapeLoftRing(
                        baseY + (mkiv ? 0.58f : 0.46f),
                        0.89f,
                        Vector2.zero)
                },
                color * 0.68f);
            cell.localPosition = Vector3.zero;
        }

        private static void AddCasemateHull(
            Transform root,
            string id,
            Color color)
        {
            if (id == "udes03")
            {
                TankHullLoftShapeFactory.Build(
                    "Painted-Swedish-UDES03-HullLoft",
                    root,
                    Curve(
                        -2.96f, 1.27f,
                        -2.52f, 1.47f,
                        -1.88f, 1.60f,
                        -0.72f, 1.62f,
                        0.72f, 1.55f,
                        1.84f, 1.37f,
                        2.62f, 1.13f,
                        2.98f, 0.98f),
                    Curve(
                        -2.96f, 0.74f,
                        -2.52f, 0.53f,
                        -1.88f, 0.36f,
                        -0.72f, 0.31f,
                        0.72f, 0.30f,
                        1.84f, 0.33f,
                        2.62f, 0.48f,
                        2.98f, 0.64f),
                    Curve(
                        -2.96f, 1.16f,
                        -2.52f, 1.29f,
                        -1.88f, 1.36f,
                        -0.72f, 1.38f,
                        0.72f, 1.38f,
                        1.84f, 1.34f,
                        2.62f, 1.22f,
                        2.98f, 0.94f),
                    Curve(
                        -2.96f, 0.72f,
                        -2.52f, 0.80f,
                        -1.88f, 0.86f,
                        -0.72f, 0.88f,
                        0.72f, 0.87f,
                        1.84f, 0.84f,
                        2.62f, 0.78f,
                        2.98f, 0.70f),
                    0.90f,
                    color * 0.68f);
                AddUdesGunSpine(root, color);
                return;
            }

            bool bModel = id == "strv103";
            string prefix = bModel
                ? "Swedish-Strv103B"
                : "Swedish-Strv103A";
            TankHullLoftShapeFactory.Build(
                "Painted-" + prefix + "-HullLoft",
                root,
                bModel
                    ? Curve(
                        -3.91f, 1.52f,
                        -2.75f, 1.74f,
                        -2.10f, 1.80f,
                        0.75f, 1.80f,
                        1.10f, 1.63f,
                        1.60f, 1.58f,
                        2.61f, 1.48f,
                        3.30f, 1.50f)
                    : Curve(
                        -3.52f, 1.77f,
                        -3.18f, 1.84f,
                        -2.62f, 1.90f,
                        -2.02f, 1.88f,
                        -0.60f, 1.88f,
                        0.62f, 1.85f,
                        1.55f, 1.70f,
                        2.36f, 1.57f,
                        2.98f, 1.47f,
                        3.52f, 1.50f),
                bModel
                    ? Curve(
                        -3.91f, 1.19f,
                        -2.75f, 0.80f,
                        -2.10f, 0.33f,
                        0.75f, 0.33f,
                        1.60f, 0.33f,
                        2.61f, 0.72f,
                        3.30f, 1.02f)
                    : Curve(
                        -3.52f, 1.22f,
                        -3.18f, 1.10f,
                        -2.62f, 0.78f,
                        -2.02f, 0.42f,
                        -0.60f, 0.40f,
                        0.62f, 0.40f,
                        1.55f, 0.40f,
                        2.36f, 0.64f,
                        2.98f, 0.88f,
                        3.52f, 1.26f),
                bModel
                    ? Curve(
                        -3.91f, 1.50f,
                        -2.75f, 1.56f,
                        -2.10f, 1.64f,
                        0.75f, 1.64f,
                        1.60f, 1.64f,
                        2.61f, 1.50f,
                        3.30f, 0.78f)
                    : Curve(
                        -3.52f, 1.44f,
                        -3.18f, 1.52f,
                        -2.62f, 1.64f,
                        -2.02f, 1.68f,
                        -0.60f, 1.68f,
                        0.62f, 1.68f,
                        1.55f, 1.67f,
                        2.36f, 1.65f,
                        2.98f, 1.60f,
                        3.52f, 1.46f),
                bModel
                    ? Curve(
                        -3.91f, 1.10f,
                        -2.75f, 0.92f,
                        -2.10f, 0.90f,
                        1.60f, 0.90f,
                        2.61f, 0.80f,
                        3.30f, 0.50f)
                    : Curve(
                        -3.52f, 1.18f,
                        -3.18f, 1.30f,
                        -2.62f, 1.50f,
                        -2.02f, 1.54f,
                        0.62f, 1.54f,
                        1.55f, 1.52f,
                        2.36f, 1.48f,
                        2.98f, 1.44f,
                        3.52f, 1.30f),
                1.38f,
                color * 0.68f);
        }

        private static void AddUdesGunSpine(
            Transform root,
            Color color)
        {
            Transform spine = TankShapeFactory.OrientedSlabPart(
                "Painted-Swedish-UDES03-GunSpine",
                root,
                new Vector3(-0.24f, 1.17f, 2.78f),
                new Vector3(0.24f, 1.17f, 2.78f),
                new Vector3(0.30f, 1.43f, 0.62f),
                new Vector3(-0.30f, 1.43f, 0.62f),
                new Vector3(-0.14f, 1.37f, 2.78f),
                new Vector3(0.14f, 1.37f, 2.78f),
                new Vector3(0.20f, 1.58f, 0.62f),
                new Vector3(-0.20f, 1.58f, 0.62f),
                color * 0.62f);
            spine.localPosition = Vector3.zero;
        }

        private static TankHullProfilePoint[] Curve(
            params float[] values)
        {
            TankHullProfilePoint[] curve =
                new TankHullProfilePoint[values.Length / 2];
            for (int index = 0; index < curve.Length; index++)
            {
                curve[index] =
                    new TankHullProfilePoint(
                        values[index * 2],
                        values[index * 2 + 1]);
            }
            return curve;
        }

    }
}
