using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankM1A2GhillieDetails
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition)
        {
            if (definition.id != "m1a2_sepv3")
                return;

            Color net = new Color(0.19f, 0.22f, 0.16f);
            Color light = new Color(0.39f, 0.42f, 0.28f);
            Color dark = new Color(0.16f, 0.21f, 0.14f);
            AddHullCover(root, net, light, dark);
            AddTurretCover(turret, net, light, dark);
        }

        private static void AddHullCover(
            Transform root,
            Color net,
            Color light,
            Color dark)
        {
            for (int strip = 0; strip < 5; strip++)
            {
                float x = -1.18f + strip * 0.59f;
                Transform cover = Part(
                    "M1A2-SepV3-GhillieHullNet",
                    root,
                    new Vector3(x, 1.79f, 1.88f),
                    new Vector3(0.04f, 0.025f, 2.15f),
                    net);
                cover.localRotation =
                    Quaternion.Euler(-6f, 0f, 0f);
            }
            Part("M1A2-SepV3-GhillieEngineNet", root,
                new Vector3(0f, 1.79f, -3.22f),
                new Vector3(2.6f, 0.025f, 0.92f),
                net);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("M1A2-SepV3-GhillieSideNet", root,
                    new Vector3(side * 1.82f, 1.11f, 0.04f),
                    new Vector3(0.025f, 0.58f, 5.05f),
                    net);
                for (int leaf = 0; leaf < 10; leaf++)
                {
                    float z = -2.2f + leaf * 0.5f;
                    float y = 0.93f + (leaf % 3) * 0.17f;
                    Transform foliage = Part(
                        "M1A2-SepV3-GhillieHullLeaf",
                        root,
                        new Vector3(
                            side * 1.9f,
                            y,
                            z),
                        new Vector3(
                            0.04f,
                            0.25f + (leaf % 2) * 0.05f,
                            0.15f),
                        leaf % 2 == 0 ? light : dark);
                    foliage.localRotation =
                        Quaternion.Euler(
                            (leaf % 3 - 1) * 9f,
                            leaf * 21f,
                            side * 12f);
                }
            }
            for (int cluster = 0; cluster < 10; cluster++)
            {
                float x = -1.2f + (cluster % 5) * 0.6f;
                float z = 1.35f + (cluster / 5) * 0.78f;
                AddLeafCluster(
                    root,
                    x,
                    1.83f,
                    z,
                    cluster,
                    light,
                    dark);
            }
        }

        private static void AddTurretCover(
            Transform turret,
            Color net,
            Color light,
            Color dark)
        {
            Part("M1A2-SepV3-GhillieTurretNet", turret,
                new Vector3(0f, 0.83f, -1.75f),
                new Vector3(2.45f, 0.025f, 2.65f),
                net);
            for (int side = -1; side <= 1; side += 2)
                Part("M1A2-SepV3-GhillieTurretSideNet", turret,
                    new Vector3(side * 1.48f, 0.42f, -1.72f),
                    new Vector3(0.025f, 0.52f, 2.7f),
                    net);
            for (int cluster = 0; cluster < 16; cluster++)
            {
                int column = cluster % 4;
                int row = cluster / 4;
                float x = -1.05f + column * 0.7f;
                float z = -2.72f + row * 0.62f;
                AddLeafCluster(
                    turret,
                    x,
                    0.87f,
                    z,
                    cluster + 17,
                    light,
                    dark);
            }
            Part("M1A2-SepV3-GhillieCrowsVine", turret,
                new Vector3(-0.72f, 1.18f, 0.04f),
                new Vector3(0.035f, 0.56f, 0.035f),
                dark);
            for (int knot = 0; knot < 4; knot++)
                AddLeafCluster(
                    turret,
                    -0.88f + knot * 0.11f,
                    1.28f + knot * 0.1f,
                    0.12f + knot * 0.18f,
                    knot + 41,
                    light,
                    dark);
        }

        private static void AddLeafCluster(
            Transform parent,
            float x,
            float y,
            float z,
            int seed,
            Color light,
            Color dark)
        {
            for (int leaf = 0; leaf < 3; leaf++)
            {
                Transform part = Part(
                    "M1A2-SepV3-GhillieLeaf",
                    parent,
                    new Vector3(
                        x + (leaf - 1) * 0.075f,
                        y + 0.04f + (leaf % 2) * 0.06f,
                        z + (leaf - 1) * 0.055f),
                    new Vector3(
                        0.15f + (seed % 3) * 0.015f,
                        0.05f,
                        0.24f + (leaf % 2) * 0.035f),
                    (seed + leaf) % 2 == 0
                        ? light
                        : dark);
                part.localRotation =
                    Quaternion.Euler(
                        (seed + leaf * 7) % 18 - 9f,
                        (seed * 23 + leaf * 31) % 180,
                        (seed + leaf * 11) % 16 - 8f);
            }
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
