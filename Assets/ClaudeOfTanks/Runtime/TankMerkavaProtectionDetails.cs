using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMerkavaProtectionDetails
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
                Mathf.Min(
                    width * 0.34f,
                    TankDetailGeometry.TurretHalfWidth(
                        definition,
                        width * 0.32f) * 0.82f);
            float length =
                definition.id == "merkava4b"
                    ? 1.25f
                    : definition.id.StartsWith(
                        "merkava3")
                        ? 1.05f
                        : 0.9f;
            float front = rear - 0.1f;
            float back = front - length;
            float top = roof - 0.12f;
            float bottom = roof - 0.62f;

            AddOpenBasket(
                turret,
                color,
                halfWidth,
                front,
                back,
                top,
                bottom);
            AddPackedStowage(
                turret,
                color,
                halfWidth,
                front,
                back,
                bottom);
            AddChainCurtain(
                turret,
                color,
                halfWidth,
                back,
                bottom);

            if (definition.id == "merkava3d")
            {
                AddRearLattice(
                    turret,
                    color,
                    halfWidth,
                    back,
                    bottom,
                    "Merkava3D",
                    11);
            }
            else if (definition.id == "merkava4b")
            {
                AddRearLattice(
                    turret,
                    color,
                    halfWidth,
                    back,
                    bottom,
                    "Merkava4B",
                    13);
            }
        }

        private static void AddOpenBasket(
            Transform turret,
            Color color,
            float halfWidth,
            float front,
            float back,
            float top,
            float bottom)
        {
            float middle = (front + back) * 0.5f;
            float length = front - back;
            Color rail = color * 0.46f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int level = 0;
                    level < 2;
                    level++)
                {
                    TankDetailGeometry.Part(
                        "Merkava-BasketRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * halfWidth,
                            level == 0
                                ? bottom
                                : top,
                            middle),
                        new Vector3(
                            0.035f,
                            0.035f,
                            length),
                        rail);
                }
                for (int post = 0;
                    post < 4;
                    post++)
                {
                    TankDetailGeometry.Part(
                        "Merkava-BasketRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * halfWidth,
                            (top + bottom) * 0.5f,
                            Mathf.Lerp(
                                front,
                                back,
                                post / 3f)),
                        new Vector3(
                            0.035f,
                            top - bottom,
                            0.035f),
                        rail);
                }
            }
            for (int cross = 0;
                cross < 5;
                cross++)
            {
                TankDetailGeometry.Part(
                    "Merkava-BasketRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        cross == 4
                            ? top
                            : bottom,
                        Mathf.Lerp(
                            front,
                            back,
                            cross / 4f)),
                    new Vector3(
                        halfWidth * 2f,
                        0.035f,
                        0.035f),
                    rail);
            }
        }

        private static void AddPackedStowage(
            Transform turret,
            Color color,
            float halfWidth,
            float front,
            float back,
            float bottom)
        {
            float span = halfWidth * 1.62f;
            for (int pack = 0;
                pack < 5;
                pack++)
            {
                float fraction = pack / 4f;
                float x = -span * 0.5f +
                    span * fraction;
                float z = Mathf.Lerp(
                    front - 0.18f,
                    back + 0.18f,
                    (pack * 3 % 5) / 4f);
                float packWidth =
                    0.28f + (pack % 2) * 0.08f;
                float packHeight =
                    0.18f + (pack % 3) * 0.035f;
                float packDepth =
                    0.28f + ((pack + 1) % 3) *
                    0.04f;
                TankDetailGeometry.Part(
                    "Painted-Merkava-BasketStowage",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x,
                        bottom + packHeight * 0.55f,
                        z),
                    new Vector3(
                        packWidth,
                        packHeight,
                        packDepth),
                    color * (0.62f +
                        pack * 0.025f));
                TankDetailGeometry.Part(
                    "Merkava-BasketStowageStrap",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x,
                        bottom + packHeight * 0.55f,
                        z),
                    new Vector3(
                        0.025f,
                        packHeight * 1.05f,
                        packDepth * 1.04f),
                    Gunmetal());
            }
        }

        private static void AddChainCurtain(
            Transform turret,
            Color color,
            float halfWidth,
            float back,
            float bottom)
        {
            TankDetailGeometry.Part(
                "Merkava-ChainRail",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    bottom + 0.03f,
                    back - 0.03f),
                new Vector3(
                    halfWidth * 1.82f,
                    0.035f,
                    0.035f),
                color * 0.5f);
            for (int chain = 0;
                chain < 16;
                chain++)
            {
                float fraction = chain / 15f;
                float x = Mathf.Lerp(
                    -halfWidth * 0.88f,
                    halfWidth * 0.88f,
                    fraction);
                float drop =
                    0.28f +
                    (chain * 5 % 4) * 0.035f;
                float lean =
                    ((chain * 3) % 3 - 1) * 2.5f;
                Transform rod =
                    TankDetailGeometry.Part(
                        "Merkava-BallChain",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            x,
                            bottom - drop * 0.5f,
                            back - 0.035f),
                        new Vector3(
                            0.012f,
                            drop,
                            0.012f),
                        color * 0.48f);
                rod.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        lean);
                TankDetailGeometry.Part(
                    "Merkava-ChainBall",
                    PrimitiveType.Sphere,
                    turret,
                    new Vector3(
                        x,
                        bottom - drop - 0.025f,
                        back - 0.035f),
                    new Vector3(
                        0.035f,
                        0.035f,
                        0.035f),
                    Gunmetal());
            }
        }

        private static void AddRearLattice(
            Transform turret,
            Color color,
            float halfWidth,
            float back,
            float bottom,
            string prefix,
            int count)
        {
            for (int item = 0;
                item < count;
                item++)
            {
                bool vertical = item % 2 == 0;
                float x = Mathf.Lerp(
                    -halfWidth * 0.92f,
                    halfWidth * 0.92f,
                    item / (float)(count - 1));
                Transform rail =
                    TankDetailGeometry.Part(
                        prefix + "-RearLattice",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            x,
                            bottom + 0.22f,
                            back - 0.12f),
                        vertical
                            ? new Vector3(
                                0.03f,
                                0.46f,
                                0.03f)
                            : new Vector3(
                                halfWidth * 0.34f,
                                0.03f,
                                0.03f),
                        item % 3 == 0
                            ? color * 0.5f
                            : Gunmetal());
                if (!vertical)
                {
                    rail.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            item % 4 == 1
                                ? 24f
                                : -24f);
                }
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.075f);
        }
    }
}
