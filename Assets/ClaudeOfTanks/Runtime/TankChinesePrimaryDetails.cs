using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankChinesePrimaryDetails
    {
        public static void BuildHull(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            bool legacy = definition.id == "type59" ||
                definition.id == "ztz85_iii";
            string prefix = definition.id == "type59"
                ? "Chinese-Type59"
                : definition.id == "ztz85_iii"
                    ? "Chinese-ZTZ85III"
                    : definition.id == "type99a"
                        ? "Chinese-Type99A"
                        : definition.id == "ztz99a2"
                            ? "Chinese-ZTZ99A2"
                            : "Chinese-VT4A1";
            float rear = -length * 0.49f;
            float front = length * 0.51f;
            float half = width * 0.47f;
            float belly = height * 0.18f;
            float deck = height * (legacy ? 0.58f : 0.57f);
            TankHullLoftShapeFactory.Build(
                "Painted-" + prefix + "-HullLoft",
                root,
                Curve(
                    rear, deck - 0.18f,
                    rear + length * 0.10f, deck,
                    -length * 0.08f, deck,
                    length * 0.20f, deck - 0.08f,
                    front - length * 0.14f, deck - 0.30f,
                    front, deck - 0.70f),
                Curve(
                    rear, belly + 0.18f,
                    rear + length * 0.10f, belly,
                    front - length * 0.14f, belly,
                    front, belly + 0.17f),
                Curve(
                    rear, half * 0.68f,
                    rear + length * 0.12f, half,
                    front - length * 0.14f, half,
                    front, half * 0.56f),
                Curve(
                    rear, half * 0.48f,
                    rear + length * 0.12f, half * 0.72f,
                    front - length * 0.14f, half * 0.70f,
                    front, half * 0.44f),
                Curve(
                    rear, deck - 0.36f,
                    rear + length * 0.12f, deck - 0.22f,
                    front - length * 0.14f, deck - 0.31f,
                    front, deck - 0.57f),
                color * 0.66f);
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


        public static void BuildTurretAndGun(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float roof)
        {
            string id = definition.id;
            bool cast = id == "type59";
            string prefix = id == "type59"
                ? "Chinese-Type59"
                : id == "ztz85_iii"
                    ? "Chinese-ZTZ85III"
                    : id == "type99a"
                        ? "Chinese-Type99A"
                        : id == "ztz99a2"
                            ? "Chinese-ZTZ99A2"
                            : "Chinese-VT4A1";
            if (cast)
            {
                TankShapeFactory.LathePart(
                    "Painted-" + prefix + "-CastTurretShell",
                    turret,
                    new[]
                    {
                        width * 0.28f,
                        width * 0.35f,
                        width * 0.38f,
                        width * 0.34f,
                        width * 0.22f,
                        width * 0.08f
                    },
                    new[]
                    {
                        -0.14f,
                        0.04f,
                        roof * 0.38f,
                        roof * 0.68f,
                        roof * 0.88f,
                        roof
                    },
                    32,
                    1.02f,
                    color * 0.62f,
                    width * 0.39f,
                    0.82f);
            }
            else
            {
                bool modern = id != "ztz85_iii";
                float half = width * (modern ? 0.42f : 0.38f);
                float front = modern ? 1.78f : 1.50f;
                float rear = modern ? -2.14f : -1.82f;
                Vector2[] plan =
                {
                    new Vector2(-0.30f, front),
                    new Vector2(0.30f, front),
                    new Vector2(half * 0.68f, front * 0.70f),
                    new Vector2(half, 0.34f),
                    new Vector2(half * 0.96f, rear * 0.62f),
                    new Vector2(half * 0.72f, rear),
                    new Vector2(-half * 0.72f, rear),
                    new Vector2(-half * 0.96f, rear * 0.62f),
                    new Vector2(-half, 0.34f),
                    new Vector2(-half * 0.68f, front * 0.70f)
                };
                TankShapeFactory.PolyMultiLoftPart(
                    "Painted-" + prefix + "-TurretShell",
                    turret,
                    plan,
                    new[]
                    {
                        new TankShapeLoftRing(
                            -0.08f,
                            1.00f,
                            Vector2.zero),
                        new TankShapeLoftRing(
                            roof * 0.42f,
                            modern ? 0.91f : 0.94f,
                            Vector2.zero),
                        new TankShapeLoftRing(
                            roof,
                            modern ? 0.60f : 0.68f,
                            new Vector2(0f, -0.10f))
                    },
                    color * 0.62f);
            }

            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    prefix + "-MainGunAssembly");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    cast ? 4.16f : 6.20f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(definition);
            float start = cast ? 0.82f : 1.00f;
            Transform sleeve = TankShapeFactory.CylinderPart(
                "Painted-" + prefix + "-GunRootSleeve",
                fittings,
                radius * 1.55f,
                radius * 1.40f,
                start,
                18,
                TankShapeAxis.Z,
                color * 0.48f);
            sleeve.localPosition =
                new Vector3(0f, 0f, start * 0.5f);
            Transform tube = TankShapeFactory.CylinderPart(
                "Painted-" + prefix + "-MainGunTube",
                fittings,
                radius,
                radius * 0.94f,
                Mathf.Max(0.2f, length - start),
                24,
                TankShapeAxis.Z,
                color * 0.46f);
            tube.localPosition =
                new Vector3(0f, 0f, (start + length) * 0.5f);
            Transform bore = TankShapeFactory.CylinderPart(
                prefix + "-MuzzleBore",
                fittings,
                radius * 0.58f,
                radius * 0.58f,
                0.045f,
                16,
                TankShapeAxis.Z,
                new Color(0.025f, 0.028f, 0.024f));
            bore.localPosition =
                new Vector3(0f, 0f, length - 0.01f);
        }

    }
}
