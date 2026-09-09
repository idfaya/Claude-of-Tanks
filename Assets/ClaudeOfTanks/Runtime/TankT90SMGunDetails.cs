using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMGunDetails
    {
        private const float SleeveRadius = 0.108f;
        private const float ForwardRadius = 0.102f;
        private const float MuzzleZ = 4.97f;

        public static void Build(
            Transform turret,
            Color color)
        {
            HideRenderer(turret.Find("Gun"));
            Transform presentation =
                turret.Find("T90SM-PresentationRoot");
            if (presentation == null) return;
            GameObject assemblyObject =
                new GameObject("T90SM-2A46M5Assembly");
            Transform assembly = assemblyObject.transform;
            assembly.SetParent(presentation, false);
            assembly.localPosition = V(0f, 0.288f, 1.17f);

            AddSaddle(assembly, color);
            AddMantlet(assembly, color);
            AddTube(assembly, color);
            AddMuzzle(assembly, color);
        }

        private static void AddSaddle(
            Transform root,
            Color color)
        {
            Cylinder(
                "Painted-T90SM-2A46M5Saddle",
                root,
                V(),
                0.21f,
                0.21f,
                0.60f,
                14,
                TankShapeAxis.X,
                color * 0.50f);
            Cylinder(
                "Painted-T90SM-2A46M5RootCone",
                root,
                V(0f, 0f, 0.40f),
                0.1302f,
                0.13875f,
                0.70f,
                12,
                TankShapeAxis.Z,
                color * 0.48f);
        }

        private static void AddMantlet(
            Transform root,
            Color color)
        {
            Box("Painted-T90SM-MantletPlug", root,
                V(0f, 0.02f, 0.66f),
                V(0.64f, 0.22f, 0.26f), color * 0.50f);
            Box("Painted-T90SM-CanvasCover", root,
                V(0f, 0.02f, 0.80f),
                V(0.58f, 0.18f, 0.018f), Canvas());
            Box("T90SM-CanvasTopStrap", root,
                V(0f, 0.105f, 0.805f),
                V(0.58f, 0.024f, 0.022f), Dark());
            foreach (float x in new[] { -0.24f, 0.24f })
            {
                Box("T90SM-CanvasSideStrap", root,
                    V(x, 0.02f, 0.805f),
                    V(0.024f, 0.17f, 0.022f), Dark());
            }
            Cylinder(
                "T90SM-CollarBoot",
                root,
                V(0f, 0f, 0.93f),
                0.125f,
                0.108f,
                0.24f,
                14,
                TankShapeAxis.Z,
                Dark());
            Cylinder(
                "T90SM-BootCrease",
                root,
                V(0f, 0f, 0.885f),
                0.118f,
                0.118f,
                0.028f,
                14,
                TankShapeAxis.Z,
                Dark());
            Cylinder(
                "T90SM-BootCrease",
                root,
                V(0f, 0f, 0.995f),
                0.112f,
                0.112f,
                0.028f,
                14,
                TankShapeAxis.Z,
                Dark());
            Cylinder(
                "T90SM-BootEndClamp",
                root,
                V(0f, 0f, 1.065f),
                0.106f,
                0.106f,
                0.032f,
                14,
                TankShapeAxis.Z,
                Dark());
            Cylinder(
                "T90SM-CoaxPort",
                root,
                V(0.24f, 0.045f, 0.795f),
                0.022f,
                0.022f,
                0.05f,
                8,
                TankShapeAxis.Z,
                Dark());
            Cylinder(
                "T90SM-CoaxPortWasher",
                root,
                V(0.24f, 0.045f, 0.822f),
                0.032f,
                0.032f,
                0.010f,
                10,
                TankShapeAxis.Z,
                Dark());
        }

        private static void AddTube(
            Transform root,
            Color color)
        {
            Tube(root, color,
                "Painted-T90SM-2A46M5ThermalSleeve",
                0.72f, 1.92f, SleeveRadius, 0f);
            Tube(root, color,
                "Painted-T90SM-2A46M5ForwardTube",
                1.92f, 2.16f, ForwardRadius, 0f);
            Tube(root, color,
                "Painted-T90SM-2A46M5ForwardTube",
                2.16f, MuzzleZ, ForwardRadius, -0.012f);

            Vector2[] rings =
            {
                new Vector2(1.20f, 0.112f),
                new Vector2(1.90f, 0.112f),
                new Vector2(2.40f, 0.106f),
                new Vector2(3.20f, 0.106f),
                new Vector2(3.80f, 0.106f),
                new Vector2(4.45f, 0.106f)
            };
            for (int index = 0; index < rings.Length; index++)
            {
                Cylinder(
                    "T90SM-2A46M5SleeveRing",
                    root,
                    V(0f, 0f, rings[index].x),
                    rings[index].y,
                    rings[index].y,
                    0.045f,
                    24,
                    TankShapeAxis.Z,
                    Dark());
            }
            Cylinder(
                "Painted-T90SM-2A46M5FumeExtractor",
                root,
                V(0f, 0f, 2.64f),
                0.128f,
                0.116f,
                0.46f,
                16,
                TankShapeAxis.Z,
                color * 0.46f);
            foreach (float z in new[] { 2.41f, 2.87f })
            {
                Cylinder(
                    "T90SM-2A46M5FumeExtractorBand",
                    root,
                    V(0f, 0f, z),
                    0.130f,
                    0.130f,
                    0.035f,
                    16,
                    TankShapeAxis.Z,
                    Dark());
            }
        }

        private static void AddMuzzle(
            Transform root,
            Color color)
        {
            Transform collar = Cylinder(
                "Painted-T90SM-2A46M5MuzzleCollar",
                root,
                V(0f, 0.021f, MuzzleZ - 0.155f),
                0.124f,
                0.124f,
                0.15f,
                14,
                TankShapeAxis.Z,
                color * 0.46f);
            collar.localScale = V(0.839f, 1f, 1f);

            Transform rim = TankShapeFactory.TorusPart(
                "T90SM-2A46M5MuzzleBore",
                root,
                ForwardRadius * 0.82f,
                ForwardRadius * 0.18f,
                16,
                Dark());
            rim.localPosition =
                V(0f, -0.012f, MuzzleZ + 0.016f);
            rim.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            Cylinder(
                "T90SM-2A46M5MuzzleBoreDisc",
                root,
                V(0f, -0.012f, MuzzleZ + 0.006f),
                0.062f,
                0.062f,
                0.012f,
                14,
                TankShapeAxis.Z,
                Color.black);
        }

        private static void Tube(
            Transform root,
            Color color,
            string name,
            float start,
            float end,
            float radius,
            float y)
        {
            Cylinder(
                name,
                root,
                V(0f, y, (start + end) * 0.5f),
                radius,
                radius,
                end - start,
                24,
                TankShapeAxis.Z,
                color * 0.47f);
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
