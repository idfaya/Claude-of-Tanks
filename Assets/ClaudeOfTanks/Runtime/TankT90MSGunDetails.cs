using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSGunDetails
    {
        private const float MuzzleZ = 5.32f;

        public static void Build(
            Transform turret,
            Color color)
        {
            HideRenderer(turret.Find("Gun"));
            Transform presentation =
                turret.Find("T90MS-PresentationRoot");
            if (presentation == null) return;

            GameObject assemblyObject =
                new GameObject("T90MS-2A46M5Assembly");
            Transform assembly = assemblyObject.transform;
            assembly.SetParent(presentation, false);
            assembly.localPosition = V(0f, 0.38f, 1.00f);

            AddSaddle(assembly, color);
            AddMantletPlug(assembly, color);
            AddBoot(assembly);
            AddTube(assembly, color);
            AddMuzzle(assembly);
        }

        private static void AddSaddle(
            Transform root,
            Color color)
        {
            Cylinder("Painted-T90MS-2A46M5Saddle", root,
                V(), 0.215f, 0.215f, 0.62f, 14,
                TankShapeAxis.X, color * 0.50f);
            Cylinder("Painted-T90MS-2A46M5RootCone", root,
                V(0f, 0f, 0.40f),
                0.1333f, 0.140f, 0.70f, 12,
                TankShapeAxis.Z, color * 0.48f);
        }

        private static void AddMantletPlug(
            Transform root,
            Color color)
        {
            Transform plug = Cylinder(
                "Painted-T90MS-MantletPlug",
                root,
                V(0f, 0.01f, 0.17f),
                0.47f,
                0.43f,
                0.32f,
                18,
                TankShapeAxis.Z,
                color * 0.49f);
            plug.localScale = V(0.60f, 0.44f, 1f);
        }

        private static void AddBoot(Transform root)
        {
            Vector4[] points =
            {
                new Vector4(0.22f, 0.55f, 0.43f, 0f),
                new Vector4(0.36f, 0.41f, 0.33f, 0f),
                new Vector4(0.50f, 0.31f, 0.26f, 0f),
                new Vector4(0.66f, 0.25f, 0.22f, 0f)
            };
            for (int index = 0; index < points.Length - 1; index++)
            {
                Vector4 a = points[index];
                Vector4 b = points[index + 1];
                Transform section = TankShapeFactory.FrustumPart(
                    "Painted-T90MS-GunBootSection",
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
                    "T90MS-GunBootCrease",
                    a.x,
                    a.y + 0.014f,
                    a.z + 0.014f,
                    0.032f);
            }
            Vector4 end = points[points.Length - 1];
            AddBootRing(
                root,
                "T90MS-GunBootClamp",
                end.x - 0.02f,
                end.y + 0.012f,
                end.z + 0.012f,
                0.04f);
        }

        private static void AddBootRing(
            Transform root,
            string name,
            float z,
            float width,
            float height,
            float depth)
        {
            Transform ring = Cylinder(
                name,
                root,
                V(0f, 0f, z),
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
            Tube(root, color, 0.52f, 1.62f, 0.112f);
            Tube(root, color, 1.62f, 3.26f, 0.116f);
            Tube(root, color, 3.26f, 5.02f, 0.095f);
            Tube(root, color, 5.02f, MuzzleZ, 0.084f);

            Vector2[] rings =
            {
                new Vector2(1.10f, 0.114f),
                new Vector2(1.62f, 0.118f),
                new Vector2(2.35f, 0.118f),
                new Vector2(3.26f, 0.106f),
                new Vector2(3.98f, 0.098f),
                new Vector2(4.68f, 0.093f)
            };
            for (int index = 0; index < rings.Length; index++)
            {
                Cylinder("T90MS-2A46M5SleeveRing", root,
                    V(0f, 0f, rings[index].x),
                    rings[index].y,
                    rings[index].y,
                    0.045f,
                    24,
                    TankShapeAxis.Z,
                    Dark());
            }
            Cylinder("Painted-T90MS-2A46M5FumeExtractor", root,
                V(0f, 0f, 2.64f),
                0.128f,
                0.116f,
                0.46f,
                14,
                TankShapeAxis.Z,
                color * 0.47f);
            foreach (float z in new[] { 2.42f, 2.87f })
            {
                Cylinder("T90MS-2A46M5FumeBand", root,
                    V(0f, 0f, z),
                    0.130f,
                    0.130f,
                    0.035f,
                    14,
                    TankShapeAxis.Z,
                    Dark());
            }
        }

        private static void Tube(
            Transform root,
            Color color,
            float start,
            float end,
            float radius)
        {
            Cylinder("Painted-T90MS-2A46M5Tube", root,
                V(0f, 0f, (start + end) * 0.5f),
                radius,
                radius,
                end - start,
                24,
                TankShapeAxis.Z,
                color * 0.47f);
        }

        private static void AddMuzzle(Transform root)
        {
            const float boreY = 0.004f;
            const float tip = MuzzleZ - 0.02f;
            Transform rim = TankShapeFactory.TorusPart(
                "T90MS-2A46M5MuzzleBore",
                root,
                0.084f * 0.82f,
                0.084f * 0.18f,
                16,
                Dark());
            rim.localPosition = V(0f, boreY, tip + 0.016f);
            rim.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Cylinder("T90MS-2A46M5MuzzleBoreDisc", root,
                V(0f, boreY, tip + 0.006f),
                0.084f * 0.62f,
                0.084f * 0.62f,
                0.012f,
                14,
                TankShapeAxis.Z,
                Color.black);
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
                name,
                parent,
                top,
                bottom,
                length,
                segments,
                axis,
                color);
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
