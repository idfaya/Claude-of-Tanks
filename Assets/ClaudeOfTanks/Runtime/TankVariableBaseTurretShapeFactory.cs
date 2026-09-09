using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankVariableBaseTurretShapeFactory
    {
        public static Transform Build(
            string name,
            Transform parent,
            Vector2[] plan,
            float height,
            float flare,
            float inset,
            Func<float, float> baseAtZ,
            float[] breakZs,
            Color color)
        {
            if (plan == null || plan.Length < 3)
                throw new ArgumentException(
                    "Variable-base turret requires at least three plan points.",
                    nameof(plan));
            if (baseAtZ == null)
                throw new ArgumentNullException(nameof(baseAtZ));

            Vector2 center = Vector2.zero;
            for (int index = 0; index < plan.Length; index++)
                center += plan[index];
            center /= plan.Length;
            Vector3[] lowerSource =
                Ring(plan, flare, 0f, center);
            Vector3[] upperSource =
                Ring(plan, inset, height, center);
            List<Vector3> lower = new List<Vector3>();
            List<Vector3> upper = new List<Vector3>();
            for (int index = 0; index < plan.Length; index++)
            {
                int next = (index + 1) % plan.Length;
                List<float> cuts = new List<float> { 0f };
                if (breakZs != null)
                {
                    for (int split = 0; split < breakZs.Length; split++)
                    {
                        float deltaZ =
                            lowerSource[next].z -
                            lowerSource[index].z;
                        if (Mathf.Abs(deltaZ) < 0.000000001f)
                            continue;
                        float t =
                            (breakZs[split] -
                             lowerSource[index].z) /
                            deltaZ;
                        if (t > 0.000001f &&
                            t < 0.999999f)
                        {
                            cuts.Add(t);
                        }
                    }
                }
                cuts.Sort();
                for (int cut = 0; cut < cuts.Count; cut++)
                {
                    float t = cuts[cut];
                    Vector3 bottom = Vector3.Lerp(
                        lowerSource[index],
                        lowerSource[next],
                        t);
                    bottom.y = baseAtZ(bottom.z);
                    lower.Add(bottom);
                    upper.Add(Vector3.Lerp(
                        upperSource[index],
                        upperSource[next],
                        t));
                }
            }

            List<Vector3> vertices =
                new List<Vector3>(lower.Count * 9);
            for (int index = 0; index < lower.Count; index++)
            {
                int next = (index + 1) % lower.Count;
                float midpointX =
                    (lower[index].x + lower[next].x) * 0.5f -
                    center.x;
                float midpointZ =
                    (lower[index].z + lower[next].z) * 0.5f -
                    center.y;
                float edgeX = lower[next].x - lower[index].x;
                float edgeZ = lower[next].z - lower[index].z;
                if (edgeX * midpointZ - edgeZ * midpointX > 0f)
                {
                    Add(vertices,
                        lower[index],
                        lower[next],
                        upper[next]);
                    Add(vertices,
                        lower[index],
                        upper[next],
                        upper[index]);
                }
                else
                {
                    Add(vertices,
                        lower[next],
                        lower[index],
                        upper[index]);
                    Add(vertices,
                        lower[next],
                        upper[index],
                        upper[next]);
                }
            }
            Vector3 topCenter =
                new Vector3(center.x, height, center.y);
            for (int index = 0; index < upper.Count; index++)
            {
                int next = (index + 1) % upper.Count;
                float normalY =
                    (upper[next].z - upper[index].z) *
                    (center.x - upper[index].x) -
                    (upper[next].x - upper[index].x) *
                    (center.y - upper[index].z);
                if (normalY > 0f)
                    Add(vertices, upper[index], upper[next], topCenter);
                else
                    Add(vertices, upper[next], upper[index], topCenter);
            }

            int[] triangles = new int[vertices.Count];
            for (int index = 0; index < triangles.Length; index++)
                triangles[index] = index;
            return TankShapeFactory.MeshPart(
                name,
                parent,
                vertices.ToArray(),
                triangles,
                color,
                null,
                new Vector2[vertices.Count]);
        }

        private static Vector3[] Ring(
            Vector2[] plan,
            float scale,
            float height,
            Vector2 center)
        {
            Vector3[] ring = new Vector3[plan.Length];
            for (int index = 0; index < plan.Length; index++)
            {
                ring[index] = new Vector3(
                    center.x +
                        (plan[index].x - center.x) *
                        scale,
                    height,
                    center.y +
                        (plan[index].y - center.y) *
                        scale);
            }
            return ring;
        }

        private static void Add(
            List<Vector3> vertices,
            Vector3 a,
            Vector3 b,
            Vector3 c)
        {
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
        }
    }
}
