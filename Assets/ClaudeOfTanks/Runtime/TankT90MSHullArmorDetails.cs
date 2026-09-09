using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSHullArmorDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddFenders(root, color);
            AddFrontGuards(root, color);
            AddDeck(root, color);
            AddGlacisFittings(root, color);
            AddRelikt(root, color);
            AddStowage(root);
        }

        private static void AddFenders(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 11; index++)
                {
                    Box("Painted-T90MS-FenderLip", root,
                        V(
                            side * 1.70f,
                            1.475f,
                            -2.75f + index * 0.545f),
                        V(0.16f, 0.05f, 0.50f),
                        color * 0.56f);
                }
            }
        }

        private static void AddFrontGuards(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Guard(
                    "Painted-T90MS-FrontGuardShoulder",
                    root,
                    side,
                    color * 0.56f,
                    V(0.92f, 1.265f, 2.62f),
                    V(1.64f, 1.265f, 2.62f),
                    V(1.64f, 1.115f, 3.28f),
                    V(0.92f, 1.115f, 3.28f),
                    V(0.92f, 1.445f, 2.62f),
                    V(1.64f, 1.445f, 2.62f),
                    V(1.64f, 1.255f, 3.28f),
                    V(0.92f, 1.255f, 3.28f));
                Guard(
                    "Painted-T90MS-FrontGuardShell",
                    root,
                    side,
                    color * 0.54f,
                    V(1.62f, 1.225f, 2.76f),
                    V(1.79f, 1.225f, 2.76f),
                    V(1.79f, 0.985f, 3.49f),
                    V(1.62f, 0.985f, 3.49f),
                    V(1.62f, 1.415f, 2.76f),
                    V(1.79f, 1.415f, 2.76f),
                    V(1.79f, 1.115f, 3.49f),
                    V(1.62f, 1.115f, 3.49f));
            }
        }

        private static void Guard(
            string name,
            Transform root,
            int side,
            Color color,
            Vector3 b0,
            Vector3 b1,
            Vector3 b2,
            Vector3 b3,
            Vector3 t0,
            Vector3 t1,
            Vector3 t2,
            Vector3 t3)
        {
            Vector3[] bottom =
            {
                Mirror(b0, side),
                Mirror(b1, side),
                Mirror(b2, side),
                Mirror(b3, side)
            };
            Vector3[] top =
            {
                Mirror(t0, side),
                Mirror(t1, side),
                Mirror(t2, side),
                Mirror(t3, side)
            };
            int[] order =
                side < 0
                    ? new[] { 1, 0, 3, 2 }
                    : new[] { 0, 1, 2, 3 };
            TankShapeFactory.OrientedSlabPart(
                name,
                root,
                bottom[order[0]],
                bottom[order[1]],
                bottom[order[2]],
                bottom[order[3]],
                top[order[0]],
                top[order[1]],
                top[order[2]],
                top[order[3]],
                color);
        }

        private static void AddDeck(
            Transform root,
            Color color)
        {
            Cylinder("Painted-T90MS-DriverHatch", root,
                V(0f, 1.365f, 2.16f),
                0.24f, 0.24f, 0.04f, 14,
                TankShapeAxis.Y, color * 0.58f);
            Cylinder("T90MS-DriverHatchRim", root,
                V(0f, 1.372f, 2.16f),
                0.247f, 0.247f, 0.012f, 14,
                TankShapeAxis.Y, Dark());
            foreach (float x in new[] { -0.16f, 0.16f })
            {
                Box("T90MS-DriverPeriscope", root,
                    V(x, 1.26f, 2.46f),
                    V(0.11f, 0.055f, 0.075f), Detail());
                Box("T90MS-DriverPeriscopeLens", root,
                    V(x, 1.265f, 2.501f),
                    V(0.075f, 0.030f, 0.010f), Glass());
            }
            for (int index = 0; index < 5; index++)
            {
                float z = -1.72f - index * 0.24f;
                Box("T90MS-EngineGrille", root,
                    V(0f, 1.557f, z),
                    V(1.50f, 0.018f, 0.075f), Dark());
                Box("T90MS-EngineGrilleRib", root,
                    V(0f, 1.551f, z - 0.12f),
                    V(1.50f, 0.028f, 0.026f), Detail());
            }
        }

        private static void AddGlacisFittings(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Transform bar = Box(
                    "T90MS-GlacisBar",
                    root,
                    V(side * 0.60f, 1.15f, 2.72f),
                    V(1.11f, 0.045f, 0.05f),
                    Detail());
                bar.localRotation = Quaternion.Euler(
                    -0.35f * Mathf.Rad2Deg,
                    side * 0.25f * Mathf.Rad2Deg,
                    0f);
                Transform hook = Box(
                    "T90MS-TowHook",
                    root,
                    V(side * 0.82f, 0.66f, 3.05f),
                    V(0.10f, 0.12f, 0.14f),
                    Dark());
                hook.localRotation = Quaternion.Euler(
                    -0.30f * Mathf.Rad2Deg,
                    0f,
                    0f);
                Transform eye = TankShapeFactory.TorusPart(
                    "T90MS-TowEye",
                    root,
                    0.085f,
                    0.016f,
                    10,
                    Detail());
                eye.localPosition =
                    V(side * 0.82f, 0.50f, 2.98f);
                eye.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                AddHeadlight(
                    root,
                    side * 1.02f,
                    1.13f,
                    2.86f,
                    color);
            }
        }

        private static void AddHeadlight(
            Transform root,
            float x,
            float y,
            float z,
            Color color)
        {
            Transform body = Cylinder(
                "Painted-T90MS-Headlight",
                root,
                V(x, y, z),
                0.09f, 0.11f, 0.13f, 14,
                TankShapeAxis.Z, color * 0.48f);
            body.localRotation =
                Quaternion.Euler(-0.30f * Mathf.Rad2Deg, 0f, 0f);
            Transform lens = Cylinder(
                "T90MS-HeadlightLens",
                root,
                V(x, y + 0.007f, z + 0.071f),
                0.073f, 0.073f, 0.012f, 14,
                TankShapeAxis.Z, Glass());
            lens.localRotation = body.localRotation;
        }

        private static void AddRelikt(
            Transform root,
            Color color)
        {
            foreach (Vector2 row in new[]
            {
                new Vector2(1.155f, 2.69f),
                new Vector2(1.365f, 2.12f)
            })
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Quaternion rotation = Quaternion.Euler(
                        -0.35f * Mathf.Rad2Deg,
                        side * 0.14f * Mathf.Rad2Deg,
                        0f);
                    foreach (float x in new[]
                    {
                        0.225f, 0.60f, 0.975f
                    })
                    {
                        Vector3 anchor =
                            V(side * x, row.x, row.y);
                        Transform cassette = Box(
                            "Painted-T90MS-GlacisRelikt",
                            root,
                            anchor,
                            V(0.33f, 0.09f, 0.30f),
                            color * 0.52f);
                        cassette.localRotation = rotation;
                        Transform face = Box(
                            "T90MS-GlacisReliktFace",
                            root,
                            anchor + rotation * V(0f, 0.049f, 0f),
                            V(0.29f, 0.008f, 0.26f),
                            Dark());
                        face.localRotation = rotation;
                    }
                    foreach (float x in new[]
                    {
                        0.4125f, 0.7875f
                    })
                    {
                        Vector3 anchor =
                            V(side * x, row.x, row.y);
                        Transform seam = Box(
                            "T90MS-GlacisReliktSeam",
                            root,
                            anchor + rotation * V(0f, 0.048f, 0f),
                            V(0.026f, 0.008f, 0.26f),
                            Dark());
                        seam.localRotation = rotation;
                    }
                }
            }
        }

        private static void AddStowage(Transform root)
        {
            TankFittingShapeFactory.BuildTowCable(
                "T90MS-BowTowCable",
                root,
                new[]
                {
                    V(-1.25f, 1.46f, 2.30f),
                    V(0f, 1.38f, 1.90f),
                    V(1.25f, 1.46f, 2.30f)
                },
                0.022f,
                20,
                6,
                Dark());
            Cylinder("T90MS-UnditchingLog", root,
                V(0f, 1.42f, -3.05f),
                0.08f, 0.08f, 0.90f, 14,
                TankShapeAxis.X, Wood());
            foreach (float x in new[] { -0.252f, 0.252f })
            {
                Box("T90MS-LogStrap", root,
                    V(x, 1.42f, -3.05f),
                    V(0.045f, 0.18f, 0.025f), Dark());
            }
            for (int index = 0; index < 4; index++)
            {
                Box("T90MS-SpareTrackLink", root,
                    V(0.32f + index * 0.20f, 1.57f, 0.60f),
                    V(0.17f, 0.045f, 0.25f), Track());
            }
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

        private static Vector3 Mirror(Vector3 point, int side)
        {
            point.x *= side;
            return point;
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

        private static Color Glass()
        {
            return TankT90AFamilyDetails.Glass();
        }

        private static Color Track()
        {
            return new Color(0.208f, 0.212f, 0.204f);
        }

        private static Color Wood()
        {
            return new Color(0.278f, 0.243f, 0.196f);
        }
    }
}
