using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90ABurlakTurretDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            HideRenderer(turret.Find("Turret"));
            Transform presentation = Root(
                "T90ABurlak-PresentationRoot",
                turret,
                new Vector3(0f, 0.11f, 0f),
                new Vector3(1f, 0.85f, 1f));
            AddCore(presentation, color);
            AddRingApron(presentation, color);
            AddShoulderFoundation(presentation, color);
            AddBustle(presentation, color);
        }

        private static void AddCore(
            Transform root,
            Color color)
        {
            Vector2[] plan =
            {
                V2(-0.25f, 1.28f), V2(0.25f, 1.28f),
                V2(0.76f, 1.10f), V2(1.38f, 0.78f),
                V2(1.40f, 0.45f), V2(1.46f, 0.05f),
                V2(1.58f, -0.48f), V2(1.64f, -0.72f),
                V2(1.36f, -1.08f), V2(0.76f, -1.20f),
                V2(-0.76f, -1.20f), V2(-1.36f, -1.08f),
                V2(-1.64f, -0.72f), V2(-1.58f, -0.48f),
                V2(-1.46f, 0.05f), V2(-1.40f, 0.45f),
                V2(-1.38f, 0.78f), V2(-0.76f, 1.10f)
            };
            float[] shoulder =
            {
                0.37f, 0.37f, 0.40f, 0.43f, 0.45f, 0.47f,
                0.47f, 0.46f, 0.44f, 0.41f, 0.41f, 0.44f,
                0.46f, 0.47f, 0.47f, 0.45f, 0.43f, 0.40f
            };
            float[] crown =
            {
                0.54f, 0.54f, 0.58f, 0.62f, 0.64f, 0.66f,
                0.66f, 0.69f, 0.65f, 0.61f, 0.61f, 0.65f,
                0.69f, 0.66f, 0.66f, 0.64f, 0.62f, 0.58f
            };
            TankPolyLoftShapeFactory.BuildMultiLoft(
                "Painted-T90ABurlak-Core",
                root,
                plan,
                new[]
                {
                    new TankShapeLoftRing(
                        0.02f, 1f, Vector2.zero),
                    new TankShapeLoftRing(
                        shoulder, 0.98f, Vector2.zero),
                    new TankShapeLoftRing(
                        crown, 0.82f, Vector2.zero)
                },
                color * 0.63f);
        }

        private static void AddRingApron(
            Transform root,
            Color color)
        {
            Vector2[] plan =
            {
                V2(-0.22f, 1.02f), V2(0.22f, 1.02f),
                V2(0.92f, 0.68f), V2(1.36f, 0.08f),
                V2(1.26f, -0.68f), V2(0.82f, -1.02f),
                V2(-0.82f, -1.02f), V2(-1.26f, -0.68f),
                V2(-1.36f, 0.08f), V2(-0.92f, 0.68f)
            };
            Transform apron = TankPolyLoftShapeFactory.BuildTurret(
                "Painted-T90ABurlak-RingApron",
                root,
                plan,
                0.24f,
                0.94f,
                0.95f,
                color * 0.58f);
            apron.localPosition = V(0f, -0.18f, -0.06f);
            Transform lip = TankPolyLoftShapeFactory.BuildTurret(
                "T90ABurlak-RingApronLip",
                root,
                plan,
                0.022f,
                0.95f,
                0.97f,
                Dark());
            lip.localPosition = V(0f, 0.04f, -0.06f);
        }

        private static void AddShoulderFoundation(
            Transform root,
            Color color)
        {
            float[,] cassette =
            {
                { 0.72f, 0.39f, 1.28f, 0.31f, -0.24f,
                    0.44f, 0.36f, 0.10f },
                { 1.04f, 0.37f, 1.04f, 0.47f, -0.20f,
                    0.48f, 0.38f, 0.11f },
                { 1.43f, 0.33f, 0.78f, 0.62f, -0.14f,
                    0.44f, 0.35f, 0.12f },
                { 1.72f, 0.37f, 0.92f, 0.28f, -0.08f,
                    0.34f, 0.10f, 0.11f }
            };
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2[] shoulder =
                {
                    V2(side * 0.28f, 1.30f),
                    V2(side * 0.82f, 1.30f),
                    V2(side * 1.84f, 0.98f),
                    V2(side * 1.80f, 0.84f),
                    V2(side * 1.46f, 0.62f),
                    V2(side * 1.44f, 0.05f),
                    V2(side * 0.42f, 0.48f)
                };
                Transform carrier =
                    TankPolyLoftShapeFactory.BuildTurret(
                        "Painted-T90ABurlak-ShoulderCarrier",
                        root,
                        shoulder,
                        0.07f,
                        1f,
                        1f,
                        color * 0.6f);
                carrier.localPosition = V(0f, 0.40f, 0f);

                for (int index = 0;
                    index < cassette.GetLength(0);
                    index++)
                {
                    float x = cassette[index, 0];
                    float y = cassette[index, 1];
                    float z = cassette[index, 2];
                    float yaw = cassette[index, 3];
                    float roll = cassette[index, 4];
                    float width = cassette[index, 5];
                    float height = cassette[index, 6];
                    float depth = cassette[index, 7];
                    Transform armor = Box(
                        "Painted-T90ABurlak-ShoulderK5",
                        root,
                        V(side * x, y, z - 0.045f),
                        V(width, height, depth),
                        color * 0.53f);
                    armor.localRotation = Quaternion.Euler(
                        roll * Mathf.Rad2Deg,
                        -side * yaw * Mathf.Rad2Deg,
                        0f);
                    Transform seam = Box(
                        "T90ABurlak-ShoulderK5Seam",
                        root,
                        V(side * x, y + height * 0.52f, z + 0.03f),
                        V(width * 0.76f, 0.012f, depth * 0.72f),
                        Dark());
                    seam.localRotation = armor.localRotation;
                }

                Transform frontReturn = Box(
                    "Painted-T90ABurlak-ShoulderReturn",
                    root,
                    V(side * 1.63f, 0.27f, -0.50f),
                    V(0.16f, 0.30f, 0.36f),
                    color * 0.58f);
                frontReturn.localRotation =
                    Quaternion.Euler(0f, 0f, -side * 5.72958f);
                Transform returnFace = Box(
                    "T90ABurlak-ShoulderReturnFace",
                    root,
                    V(side * 1.72f, 0.27f, -0.50f),
                    V(0.018f, 0.24f, 0.30f),
                    Dark());
                returnFace.localRotation =
                    frontReturn.localRotation;
                Transform rear = Box(
                    "Painted-T90ABurlak-RearShoulder",
                    root,
                    V(side * 1.42f, 0.31f, -0.84f),
                    V(0.30f, 0.31f, 0.68f),
                    color * 0.58f);
                rear.localRotation =
                    Quaternion.Euler(-3.43775f, -side * 5.72958f, 0f);
                Box(
                    "T90ABurlak-RearShoulderFace",
                    root,
                    V(side * 1.58f, 0.31f, -0.84f),
                    V(0.018f, 0.24f, 0.56f),
                    Dark()).localRotation = rear.localRotation;
                Box(
                    "T90ABurlak-RearShoulderRail",
                    root,
                    V(side * 1.42f, 0.48f, -0.61f),
                    V(0.23f, 0.016f, 0.04f),
                    Detail()).localRotation = rear.localRotation;
            }
            Transform bridge = Box(
                "Painted-T90ABurlak-MantletBridge",
                root,
                V(0f, 0.18f, 1.43f),
                V(0.90f, 0.36f, 0.34f),
                color * 0.56f);
            bridge.localRotation =
                Quaternion.Euler(-4.58366f, 0f, 0f);
            Transform chin = Box(
                "T90ABurlak-MantletChin",
                root,
                V(0f, 0.17f, 1.605f),
                V(0.80f, 0.29f, 0.028f),
                Dark());
            chin.localRotation = bridge.localRotation;
        }

        private static void AddBustle(
            Transform root,
            Color color)
        {
            TankWeldedStationLoftShapeFactory.Build(
                "Painted-T90ABurlak-BustleLoft",
                root,
                new[]
                {
                    S(-1.08f, 0f, 0.64f,
                        -1.10f, 1.10f, -0.98f, 0.98f, -1.04f, 1.04f),
                    S(-1.50f, 0.05f, 0.67f,
                        -1.02f, 1.02f, -0.90f, 0.90f, -0.96f, 0.96f),
                    S(-2.08f, 0.07f, 0.65f,
                        -0.84f, 0.84f, -0.72f, 0.72f, -0.79f, 0.79f),
                    S(-2.68f, 0.08f, 0.60f,
                        -0.72f, 0.72f, -0.60f, 0.60f, -0.67f, 0.67f),
                    S(-3.30f, 0f, 0.58f,
                        -0.58f, 0.58f, -0.47f, 0.47f, -0.54f, 0.54f)
                },
                color * 0.56f);
            float[,] lids =
            {
                { -1.34f, 1.54f, 0.32f, 0.68f },
                { -1.80f, 1.44f, 0.35f, 0.69f },
                { -2.29f, 1.26f, 0.36f, 0.67f },
                { -2.83f, 1.04f, 0.36f, 0.62f }
            };
            for (int index = 0; index < lids.GetLength(0); index++)
            {
                float z = lids[index, 0];
                float width = lids[index, 1];
                float depth = lids[index, 2];
                float y = lids[index, 3];
                Box(
                    "T90ABurlak-BustleLid",
                    root,
                    V(0f, y, z),
                    V(width, 0.018f, depth),
                    Dark());
                Box(
                    "T90ABurlak-BustleLidRail",
                    root,
                    V(0f, y + 0.015f, z + depth * 0.39f),
                    V(width * 0.8f, 0.012f, 0.04f),
                    Detail());
            }
            AddBustleSidePods(root, color);
            AddBustleServiceGrid(root);
        }

        private static void AddBustleSidePods(
            Transform root,
            Color color)
        {
            float[,] pods =
            {
                { 1.00f, -2.12f, 0.33f },
                { 0.92f, -2.53f, 0.31f },
                { 0.82f, -2.91f, 0.29f }
            };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < pods.GetLength(0); index++)
                {
                    float x = pods[index, 0];
                    float z = pods[index, 1];
                    float depth = pods[index, 2];
                    Transform pod = Box(
                        "Painted-T90ABurlak-BustleSidePod",
                        root,
                        V(side * x, 0.37f, z),
                        V(0.19f, 0.28f, depth),
                        color * 0.54f);
                    pod.localRotation =
                        Quaternion.Euler(0f, -side * 18.33465f, 0f);
                    Transform face = Box(
                        "T90ABurlak-BustleSidePodFace",
                        root,
                        V(side * (x + 0.105f), 0.37f, z),
                        V(0.015f, 0.22f, depth * 0.8f),
                        Dark());
                    face.localRotation = pod.localRotation;
                }
                for (int index = 0; index < 3; index++)
                {
                    Box(
                        "T90ABurlak-BustleRearRail",
                        root,
                        V(side * 0.50f, 0.32f + index * 0.11f, -3.00f),
                        V(0.03f, 0.022f, 0.68f),
                        Detail());
                }
                float[] posts = { -3.32f, -3.08f, -2.84f, -2.68f };
                for (int index = 0; index < posts.Length; index++)
                {
                    Box(
                        "T90ABurlak-BustleRearPost",
                        root,
                        V(side * 0.50f, 0.43f, posts[index]),
                        V(0.03f, 0.24f, 0.03f),
                        Detail());
                }
            }
        }

        private static void AddBustleServiceGrid(Transform root)
        {
            float[,] bays =
            {
                { -0.42f, 0.42f, 4f },
                { 0.10f, 0.34f, 3f },
                { 0.43f, 0.24f, 2f }
            };
            for (int bay = 0; bay < bays.GetLength(0); bay++)
            {
                float x = bays[bay, 0];
                float width = bays[bay, 1];
                int count = Mathf.RoundToInt(bays[bay, 2]);
                Box(
                    "T90ABurlak-BustleServiceBay",
                    root,
                    V(x, 0.43f, -3.315f),
                    V(width, 0.20f, 0.022f),
                    Dark());
                for (int index = 0; index < count; index++)
                {
                    float offset = count == 1
                        ? 0f
                        : index * (width * 0.68f / (count - 1));
                    Box(
                        "T90ABurlak-BustleServiceLatch",
                        root,
                        V(x - width * 0.34f + offset, 0.43f, -3.332f),
                        V(0.02f, 0.14f, 0.016f),
                        Detail());
                }
            }
        }

        private static TankWeldedStation S(
            float z,
            float bottom,
            float top,
            float middleLeft,
            float middleRight,
            float bottomLeft,
            float bottomRight,
            float topLeft,
            float topRight)
        {
            return new TankWeldedStation(
                z, bottom, top,
                middleLeft, middleRight,
                bottomLeft, bottomRight,
                topLeft, topRight);
        }

        private static Transform Root(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            root.localScale = scale;
            return root;
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, parent, size, color);
            part.localPosition = position;
            return part;
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer =
                part == null ? null : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        private static Vector2 V2(float x, float y)
        {
            return new Vector2(x, y);
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return TankT90AFamilyDetails.Dark();
        }

        private static Color Detail()
        {
            return new Color(0.16f, 0.17f, 0.15f);
        }
    }
}
