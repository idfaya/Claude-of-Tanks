using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90MSTurretArmorDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddFrontChevron(root, color);
            AddNoseTiles(root, color);
            AddFrontalOptics(root, color);
            AddProjectedFlankArmor(root, color);
        }

        private static void AddFrontChevron(
            Transform root,
            Color color)
        {
            Vector2[] inner =
            {
                V2(0.28f, 1.35f), V2(0.42f, 1.50f),
                V2(0.96f, 1.17f), V2(0.82f, 1.02f)
            };
            Vector2[] outer =
            {
                V2(0.79f, 1.04f), V2(0.94f, 1.18f),
                V2(1.50f, 0.61f), V2(1.35f, 0.47f)
            };
            float[,] rows =
            {
                { 0.11f, 0.34f, -0.10f, 0.09f },
                { 0.34f, 0.59f, 0.09f, -0.11f }
            };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int row = 0; row < rows.GetLength(0); row++)
                {
                    ChevronSlab(
                        "Painted-T90MS-ChevronCarrier",
                        root,
                        side,
                        inner,
                        rows[row, 0],
                        rows[row, 1],
                        rows[row, 2],
                        rows[row, 3],
                        color * 0.54f);
                    ChevronSlab(
                        "Painted-T90MS-ChevronCarrier",
                        root,
                        side,
                        outer,
                        rows[row, 0],
                        rows[row, 1],
                        rows[row, 2],
                        rows[row, 3],
                        color * 0.54f);
                }
            }
        }

        private static void ChevronSlab(
            string name,
            Transform root,
            int side,
            Vector2[] plan,
            float bottomY,
            float topY,
            float rearOffset,
            float ridgeOffset,
            Color color)
        {
            Vector3[] corners = new Vector3[8];
            for (int index = 0; index < 4; index++)
            {
                corners[index] = V(
                    side * plan[index].x,
                    bottomY,
                    plan[index].y + rearOffset);
                corners[index + 4] = V(
                    side * plan[index].x,
                    topY,
                    plan[index].y + ridgeOffset);
            }
            Slab(name, root, corners, color);
        }

        private static void AddNoseTiles(
            Transform root,
            Color color)
        {
            Vector4[] segments =
            {
                new Vector4(0.42f, 1.50f, 0.96f, 1.17f),
                new Vector4(0.94f, 1.18f, 1.50f, 0.61f)
            };
            float[,] rows =
            {
                { 0.11f, 0.34f, -0.10f, 0.09f },
                { 0.34f, 0.59f, 0.09f, -0.11f }
            };
            Vector2[] ranges =
            {
                V2(0.080f, 0.340f),
                V2(0.370f, 0.630f),
                V2(0.660f, 0.920f)
            };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int segment = 0;
                    segment < segments.Length;
                    segment++)
                {
                    for (int row = 0;
                        row < rows.GetLength(0);
                        row++)
                    {
                        for (int tile = 0;
                            tile < ranges.Length;
                            tile++)
                        {
                            TileSlab(
                                "T90MS-NoseReliktBacking",
                                root,
                                side,
                                segments[segment],
                                rows,
                                row,
                                ranges[tile].x,
                                ranges[tile].y,
                                0.030f,
                                0.018f,
                                -0.008f,
                                Dark());
                            TileSlab(
                                "Painted-T90MS-NoseReliktFace",
                                root,
                                side,
                                segments[segment],
                                rows,
                                row,
                                ranges[tile].x,
                                ranges[tile].y,
                                0.070f,
                                0f,
                                0.015f,
                                color * 0.54f);
                        }
                    }
                }
            }
            Transform gap = Box(
                "T90MS-ChevronGapPlate",
                root,
                V(0f, 0.30f, 1.52f),
                V(0.42f, 0.28f, 0.055f),
                Dark());
            gap.localRotation =
                Quaternion.Euler(-0.28f * Mathf.Rad2Deg, 0f, 0f);
        }

        private static void TileSlab(
            string name,
            Transform root,
            int side,
            Vector4 segment,
            float[,] rows,
            int row,
            float t0,
            float t1,
            float depth,
            float padT,
            float padY,
            Color color)
        {
            float dx = segment.z - segment.x;
            float dz = segment.w - segment.y;
            float edgeLength = Mathf.Sqrt(dx * dx + dz * dz);
            float nx = side * (-dz / edgeLength);
            float nz = dx / edgeLength;
            float lowT = Mathf.Max(0f, t0 - padT);
            float highT = Mathf.Min(1f, t1 + padT);
            float lowY = rows[row, 0] + padY;
            float highY = rows[row, 1] - padY;
            Vector3[] corners =
            {
                TilePoint(side, segment, rows, row,
                    lowT, lowY, 0f, nx, nz),
                TilePoint(side, segment, rows, row,
                    highT, lowY, 0f, nx, nz),
                TilePoint(side, segment, rows, row,
                    highT, highY, 0f, nx, nz),
                TilePoint(side, segment, rows, row,
                    lowT, highY, 0f, nx, nz),
                TilePoint(side, segment, rows, row,
                    lowT, lowY, depth, nx, nz),
                TilePoint(side, segment, rows, row,
                    highT, lowY, depth, nx, nz),
                TilePoint(side, segment, rows, row,
                    highT, highY, depth, nx, nz),
                TilePoint(side, segment, rows, row,
                    lowT, highY, depth, nx, nz)
            };
            Slab(name, root, corners, color);
        }

        private static Vector3 TilePoint(
            int side,
            Vector4 segment,
            float[,] rows,
            int row,
            float t,
            float y,
            float push,
            float nx,
            float nz)
        {
            float rowSpan = rows[row, 1] - rows[row, 0];
            float zAtY = rows[row, 2] +
                (rows[row, 3] - rows[row, 2]) *
                ((y - rows[row, 0]) / rowSpan);
            return V(
                side * (segment.x + (segment.z - segment.x) * t) +
                nx * push,
                y,
                segment.y + (segment.w - segment.y) * t +
                zAtY + nz * push);
        }

        private static void AddFrontalOptics(
            Transform root,
            Color color)
        {
            float[,] optics =
            {
                { -0.63f, 0.135f },
                { 0.61f, 0.125f }
            };
            for (int index = 0; index < optics.GetLength(0); index++)
            {
                float x = optics[index, 0];
                float radius = optics[index, 1];
                Transform cradle = Box(
                    "Painted-T90MS-FrontalOpticCradle",
                    root,
                    V(x, 0.42f, 1.48f),
                    V(0.36f, 0.32f, 0.30f),
                    color * 0.56f);
                cradle.localRotation =
                    Quaternion.Euler(-0.12f * Mathf.Rad2Deg, 0f, 0f);
                Cylinder("T90MS-FrontalOpticRim", root,
                    V(x, 0.43f, 1.62f),
                    radius + 0.030f,
                    0.080f,
                    20,
                    Dark());
                Cylinder("T90MS-FrontalOpticLens", root,
                    V(x, 0.43f, 1.671f),
                    radius,
                    0.018f,
                    20,
                    Glass());
            }
        }

        private static void AddProjectedFlankArmor(
            Transform root,
            Color color)
        {
            float[,] carriers =
            {
                { 1f, -0.676f, 0.610f, 0.50f, 0.91f },
                { 1f, -1.350f, 0.735f, 0.48f, 0.92f },
                { -1f, -1.175f, 0.625f, 0.48f, 0.92f }
            };
            for (int index = 0;
                index < carriers.GetLength(0);
                index++)
            {
                int side = Mathf.RoundToInt(carriers[index, 0]);
                float z = carriers[index, 1];
                float span = carriers[index, 2];
                float lower = carriers[index, 3];
                float upper = carriers[index, 4];
                TankT90MSSurfaceArmorFactory.BuildSkinPatch(
                    "T90MS-FlankReliktBacking", root,
                    side, z + span * 0.5f + 0.014f,
                    z - span * 0.5f - 0.014f,
                    lower - 0.018f, upper + 0.018f,
                    0.024f, 0.001f, Dark());
                TankT90MSSurfaceArmorFactory.BuildSkinPatch(
                    "Painted-T90MS-FlankRelikt", root,
                    side, z + span * 0.5f, z - span * 0.5f,
                    lower, upper, 0.088f, 0.003f,
                    color * 0.53f);
                TankT90MSSurfaceArmorFactory.BuildSkinPatch(
                    "T90MS-FlankReliktInsert", root,
                    side, z + span * 0.5f - 0.035f,
                    z - span * 0.5f + 0.035f,
                    lower + 0.045f, upper - 0.045f,
                    0.094f, 0.004f, Dark());
            }
            AddLowerFlankShoes(root, color);
            AddShoulderAndRoofArmor(root, color);
        }

        private static void AddLowerFlankShoes(
            Transform root,
            Color color)
        {
            float[,] shoes =
            {
                { 1f, -0.486f }, { 1f, -0.839f },
                { 1f, -1.190f }, { 1f, -1.543f },
                { -1f, -1.205f }, { -1f, -0.875f },
                { -1f, -0.545f }, { -1f, -0.215f }
            };
            for (int index = 0; index < shoes.GetLength(0); index++)
            {
                int side = Mathf.RoundToInt(shoes[index, 0]);
                float z = shoes[index, 1];
                float stagger =
                    Mathf.RoundToInt(Mathf.Abs(z) * 10f) % 2 != 0
                        ? -0.028f
                        : 0.018f;
                float center = z + stagger;
                TankT90MSSurfaceArmorFactory.BuildSkinPatch(
                    "Painted-T90MS-LowerFlankRelikt", root,
                    side, center + 0.185f, center - 0.185f,
                    0.11f, 0.42f, 0.064f, 0.002f,
                    color * 0.53f);
                TankT90MSSurfaceArmorFactory.BuildSkinPatch(
                    "T90MS-LowerFlankReliktInsert", root,
                    side, center + 0.150f, center - 0.150f,
                    0.145f, 0.385f, 0.069f, 0.004f,
                    Dark());
            }
        }

        private static void AddShoulderAndRoofArmor(
            Transform root,
            Color color)
        {
            float[,] shoulders =
            {
                { 0.46f, 0.46f, 0.58f, 0.95f },
                { -0.02f, 0.48f, 0.57f, 0.95f },
                { -0.52f, 0.46f, 0.56f, 0.94f },
                { -0.98f, 0.42f, 0.55f, 0.93f }
            };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0;
                    index < shoulders.GetLength(0);
                    index++)
                {
                    float z = shoulders[index, 0];
                    float span = shoulders[index, 1];
                    float lower = shoulders[index, 2];
                    float upper = shoulders[index, 3];
                    TankT90MSSurfaceArmorFactory.BuildSkinPatch(
                        "T90MS-ShoulderReliktBacking", root,
                        side, z + span * 0.5f + 0.012f,
                        z - span * 0.5f - 0.012f,
                        lower - 0.015f, upper + 0.015f,
                        0.020f, 0.001f, Dark());
                    TankT90MSSurfaceArmorFactory.BuildSkinPatch(
                        "Painted-T90MS-ShoulderRelikt", root,
                        side, z + span * 0.5f, z - span * 0.5f,
                        lower, upper, 0.076f, 0.002f,
                        color * 0.53f);
                    TankT90MSSurfaceArmorFactory.BuildSkinPatch(
                        "T90MS-ShoulderReliktInsert", root,
                        side, z + span * 0.5f - 0.030f,
                        z - span * 0.5f + 0.030f,
                        lower + 0.040f, upper - 0.040f,
                        0.081f, 0.004f, Dark());
                }
            }
            float[,] roof =
            {
                { -0.235f, -0.153f, 0.29f, 0.52f },
                { 0.120f, -0.154f, 0.29f, 0.52f },
                { -0.298f, 0.386f, 0.29f, 0.40f },
                { -0.052f, 0.550f, 0.66f, 0.24f },
                { 0.188f, 0.386f, 0.29f, 0.40f }
            };
            for (int index = 0; index < roof.GetLength(0); index++)
            {
                float x = roof[index, 0];
                float z = roof[index, 1];
                float width = roof[index, 2];
                float depth = roof[index, 3];
                TankT90MSSurfaceArmorFactory.BuildRoofPatch(
                    "Painted-T90MS-RoofRelikt", root,
                    x - width * 0.5f,
                    x + width * 0.5f,
                    z + depth * 0.5f,
                    z - depth * 0.5f,
                    0.052f,
                    0.002f,
                    color * 0.53f);
            }
        }

        private static Transform Slab(
            string name,
            Transform root,
            Vector3[] corners,
            Color color)
        {
            return TankShapeFactory.OrientedSlabPart(
                name,
                root,
                corners[0],
                corners[1],
                corners[2],
                corners[3],
                corners[4],
                corners[5],
                corners[6],
                corners[7],
                color);
        }

        private static Transform Box(
            string name,
            Transform root,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part =
                TankShapeFactory.BoxPart(name, root, size, color);
            part.localPosition = position;
            return part;
        }

        private static Transform Cylinder(
            string name,
            Transform root,
            Vector3 position,
            float radius,
            float length,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                root,
                radius,
                radius,
                length,
                segments,
                TankShapeAxis.Z,
                color);
            part.localPosition = position;
            return part;
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

        private static Color Glass()
        {
            return TankT90AFamilyDetails.Glass();
        }
    }
}
