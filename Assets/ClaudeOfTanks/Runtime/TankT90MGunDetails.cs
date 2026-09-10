using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MGunDetails
    {
        private const float MuzzleZ = 4.92f;

        public static void Build(
            Transform turret,
            Color color)
        {
            HideRenderer(turret.Find("Gun"));
            Transform presentation =
                turret.Find("T90M-PresentationRoot");
            if (presentation == null) return;

            Transform assembly =
                new GameObject("T90M-2A46M5Assembly").transform;
            assembly.SetParent(presentation, false);
            assembly.localPosition = V(0f, 0.21f, 1.15f);
            assembly.localScale = V(1f / 0.95f, 1f / 0.65f, 1f);

            AddSaddle(assembly, color);
            AddBoot(assembly);
            AddTube(assembly, color);
            AddMuzzle(assembly, color);
        }

        private static void AddSaddle(
            Transform root,
            Color color)
        {
            Cylinder(
                "Painted-T90M-2A46M5Saddle",
                root,
                V(),
                0.17f,
                0.17f,
                0.56f,
                14,
                TankShapeAxis.X,
                color * 0.50f);
            Cylinder(
                "Painted-T90M-2A46M5RootCone",
                root,
                V(0f, 0f, 0.25f),
                0.145f,
                0.135f,
                0.40f,
                12,
                TankShapeAxis.Z,
                color * 0.48f);
        }

        private static void AddBoot(Transform root)
        {
            Vector4[] points =
            {
                new Vector4(0.01f, 0.60f, 0.30f, 0.060f),
                new Vector4(0.09f, 0.576f, 0.288f, 0.055f),
                new Vector4(0.17f, 0.588f, 0.294f, 0.058f),
                new Vector4(0.24f, 0.564f, 0.30f, 0.042f),
                new Vector4(0.31f, 0.558f, 0.32f, 0.030f)
            };
            for (int index = 0; index < points.Length - 1; index++)
            {
                Vector4 a = points[index];
                Vector4 b = points[index + 1];
                Transform section = TankShapeFactory.FrustumPart(
                    "Painted-T90M-GunBootSection",
                    root,
                    a.y * 0.5f,
                    -(a.w - a.z * 0.5f),
                    -(a.w + a.z * 0.5f),
                    b.y * 0.5f,
                    -(b.w - b.z * 0.5f),
                    -(b.w + b.z * 0.5f),
                    0f,
                    b.x - a.x,
                    Canvas());
                section.localPosition = V(0f, 0f, a.x);
                section.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                if (index == 0) continue;
                AddBootRing(
                    root,
                    "T90M-GunBootCrease",
                    a.x,
                    a.w,
                    a.y + 0.014f,
                    a.z + 0.014f,
                    0.035f);
            }

            Vector4 end = points[points.Length - 1];
            AddBootRing(
                root,
                "T90M-GunBootClamp",
                end.x - 0.02f,
                end.w,
                end.y + 0.012f,
                end.z + 0.012f,
                0.04f);
            Cylinder(
                "T90M-GunRootClamp",
                root,
                V(0f, 0f, 0.53f),
                0.124f,
                0.124f,
                0.04f,
                14,
                TankShapeAxis.Z,
                Dark());
            Cylinder(
                "T90M-CoaxPort",
                root,
                V(0.20f, 0.12f, 0.278f),
                0.020f,
                0.020f,
                0.05f,
                8,
                TankShapeAxis.Z,
                Dark());
            Cylinder(
                "T90M-CoaxPortWasher",
                root,
                V(0.20f, 0.12f, 0.300f),
                0.028f,
                0.028f,
                0.010f,
                10,
                TankShapeAxis.Z,
                Dark());
        }

        private static void AddBootRing(
            Transform root,
            string name,
            float z,
            float y,
            float width,
            float height,
            float depth)
        {
            Transform ring = Cylinder(
                name,
                root,
                V(0f, y, z),
                0.5f,
                0.5f,
                depth,
                14,
                TankShapeAxis.Z,
                Dark());
            ring.localScale = V(width, height, 1f);
        }

        private static void AddTube(
            Transform root,
            Color color)
        {
            float[,] segments =
            {
                { 0.55f, 0.85f, 0.108f, 0.006f },
                { 0.85f, 1.40f, 0.108f, 0.006f },
                { 1.40f, 1.92f, 0.108f, 0.006f },
                { 1.92f, 2.16f, 0.102f, 0f },
                { 2.16f, 2.62f, 0.102f, 0f },
                { 2.62f, 3.08f, 0.102f, 0f },
                { 3.08f, 3.54f, 0.102f, 0f },
                { 3.54f, 4.00f, 0.102f, 0f },
                { 4.00f, 4.46f, 0.102f, 0f },
                { 4.46f, 4.92f, 0.102f, 0f }
            };
            for (int index = 0;
                index < segments.GetLength(0);
                index++)
            {
                float start = segments[index, 0];
                float end = segments[index, 1];
                string name = index < 3
                    ? "Painted-T90M-2A46M5ThermalSleeve"
                    : "Painted-T90M-2A46M5ForwardTube";
                Cylinder(
                    name,
                    root,
                    V(
                        0f,
                        segments[index, 3],
                        (start + end) * 0.5f),
                    segments[index, 2],
                    segments[index, 2],
                    end - start,
                    24,
                    TankShapeAxis.Z,
                    color * 0.47f);
            }

            foreach (float z in new[] { 2.60f, 3.40f, 4.20f })
            {
                Cylinder(
                    "T90M-2A46M5SleeveRing",
                    root,
                    V(0f, 0f, z),
                    0.106f,
                    0.106f,
                    0.045f,
                    24,
                    TankShapeAxis.Z,
                    Dark());
            }
            Box(
                "Painted-T90M-2A46M5EvacuatorCrest",
                root,
                V(0f, 0.005f, 2.04f),
                V(0.02f, 0.186f, 0.24f),
                color * 0.47f);
        }

        private static void AddMuzzle(
            Transform root,
            Color color)
        {
            Transform collar = Cylinder(
                "Painted-T90M-2A46M5MuzzleCollar",
                root,
                V(0f, 0.033f, 4.765f),
                0.124f,
                0.124f,
                0.15f,
                14,
                TankShapeAxis.Z,
                color * 0.47f);
            collar.localScale = V(0.839f, 1f, 1f);

            Transform rim = TankShapeFactory.TorusPart(
                "Painted-T90M-2A46M5MuzzleRim",
                root,
                0.076f,
                0.006f,
                14,
                color * 0.47f);
            rim.localPosition = V(0f, 0f, MuzzleZ - 0.0055f);
            rim.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Cylinder(
                "T90M-2A46M5MuzzleBoreDisc",
                root,
                V(0f, 0f, MuzzleZ - 0.0045f),
                0.062f,
                0.062f,
                0.010f,
                14,
                TankShapeAxis.Z,
                Color.black);
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

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float top,
            float bottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name, parent, top, bottom, length, segments, axis, color);
            part.localPosition = position;
            return part;
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer =
                part == null ? null : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        private static Vector3 V(
            float x = 0f,
            float y = 0f,
            float z = 0f)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return TankT90AFamilyDetails.Dark();
        }

        private static Color Canvas()
        {
            return new Color(0.20f, 0.23f, 0.17f);
        }
    }
}
