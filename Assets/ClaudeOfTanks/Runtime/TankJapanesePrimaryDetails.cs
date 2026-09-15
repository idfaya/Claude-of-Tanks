using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankJapanesePrimaryDetails
    {
        public static void BuildHull(
            Transform root,
            string id,
            Color color,
            float width,
            float height,
            float length)
        {
            bool cast = id == "stb1" || id == "type74";
            bool type10 = id == "type10" || id == "type10b";
            string prefix = id == "stb1"
                ? "Japanese-STB1"
                : id == "type74"
                    ? "Japanese-Type74"
                    : id == "type90" || id == "type90a"
                        ? "Japanese-Type90"
                        : "Japanese-Type10";
            float rear = -length * 0.49f;
            float front = length * 0.51f;
            float halfWidth = width * 0.47f;
            float belly = Mathf.Max(0.34f, height * 0.19f);
            float deck = cast
                ? height * 0.58f
                : type10
                    ? height * 0.55f
                    : height * 0.57f;
            TankHullLoftShapeFactory.Build(
                "Painted-" + prefix + "-HullLoft",
                root,
                Curve(
                    rear, deck - 0.16f,
                    rear + length * 0.09f, deck,
                    -length * 0.08f, deck,
                    length * 0.20f, deck - (cast ? 0.04f : 0.10f),
                    front - length * 0.14f, deck - (cast ? 0.22f : 0.30f),
                    front, deck - (cast ? 0.55f : 0.68f)),
                Curve(
                    rear, belly + 0.16f,
                    rear + length * 0.09f, belly,
                    front - length * 0.14f, belly,
                    front, belly + 0.16f),
                Curve(
                    rear, halfWidth * 0.68f,
                    rear + length * 0.11f, halfWidth,
                    front - length * 0.14f, halfWidth,
                    front, halfWidth * (cast ? 0.62f : 0.56f)),
                Curve(
                    rear, halfWidth * 0.48f,
                    rear + length * 0.11f, halfWidth * 0.72f,
                    front - length * 0.14f, halfWidth * 0.70f,
                    front, halfWidth * 0.45f),
                Curve(
                    rear, deck - 0.34f,
                    rear + length * 0.11f, deck - 0.22f,
                    front - length * 0.14f, deck - 0.30f,
                    front, deck - 0.54f),
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
            bool cast = id == "stb1" || id == "type74";
            bool type10 = id == "type10" || id == "type10b";
            string prefix = id == "stb1"
                ? "Japanese-STB1"
                : id == "type74"
                    ? "Japanese-Type74"
                    : id == "type90" || id == "type90a"
                        ? "Japanese-Type90"
                        : "Japanese-Type10";
            if (cast)
            {
                TankShapeFactory.LathePart(
                    "Painted-" + prefix + "-CastTurretShell",
                    turret,
                    new[]
                    {
                        width * 0.30f,
                        width * 0.36f,
                        width * 0.38f,
                        width * 0.34f,
                        width * 0.24f,
                        width * 0.10f
                    },
                    new[]
                    {
                        -0.16f,
                        0.02f,
                        roof * 0.36f,
                        roof * 0.68f,
                        roof * 0.88f,
                        roof
                    },
                    32,
                    1.04f,
                    color * 0.62f,
                    width * 0.40f,
                    0.82f);
            }
            else
            {
                float half = width * (type10 ? 0.42f : 0.40f);
                float front = type10 ? 1.78f : 1.54f;
                float rear = type10 ? -2.08f : -1.76f;
                Vector2[] plan =
                {
                    new Vector2(-0.28f, front),
                    new Vector2(0.28f, front),
                    new Vector2(half * 0.64f, front * 0.68f),
                    new Vector2(half, 0.36f),
                    new Vector2(half * 0.96f, rear * 0.62f),
                    new Vector2(half * 0.72f, rear),
                    new Vector2(-half * 0.72f, rear),
                    new Vector2(-half * 0.96f, rear * 0.62f),
                    new Vector2(-half, 0.36f),
                    new Vector2(-half * 0.64f, front * 0.68f)
                };
                TankShapeFactory.PolyMultiLoftPart(
                    "Painted-" + prefix + "-WedgeTurretShell",
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
                            type10 ? 0.92f : 0.94f,
                            Vector2.zero),
                        new TankShapeLoftRing(
                            roof,
                            type10 ? 0.60f : 0.68f,
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
                    cast ? 4.42f : 4.80f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(definition);
            float start = cast ? 0.78f : 0.96f;
            Transform sleeve = TankShapeFactory.CylinderPart(
                "Painted-" + prefix + "-GunRootSleeve",
                fittings,
                radius * 1.55f,
                radius * 1.38f,
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
