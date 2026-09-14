using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class MapStructureRuntime
    {
        private static void AddBox(
            MeshBucket bucket,
            Vector3 center,
            Vector3 size,
            float yaw)
        {
            AddOrientedBox(
                bucket,
                center,
                size,
                Quaternion.AngleAxis(
                    yaw * Mathf.Rad2Deg,
                    Vector3.up));
        }

        private static void AddGableRoof(
            MeshBucket bucket,
            Vector3 baseCenter,
            float width,
            float depth,
            float height,
            float yaw)
        {
            int start = bucket.Vertices.Count;
            float hx = width * 0.5f;
            float hz = depth * 0.5f;
            Vector3[] local =
            {
                new Vector3(-hx, 0f, -hz),
                new Vector3(hx, 0f, -hz),
                new Vector3(-hx, 0f, hz),
                new Vector3(hx, 0f, hz),
                new Vector3(0f, height, -hz),
                new Vector3(0f, height, hz)
            };
            for (int i = 0; i < local.Length; i++)
                bucket.Vertices.Add(
                    baseCenter +
                    Local(
                        local[i].x,
                        local[i].y,
                        local[i].z,
                        yaw));
            int[] indices =
            {
                0, 1, 4, 2, 5, 3,
                0, 4, 5, 0, 5, 2,
                1, 3, 5, 1, 5, 4,
                0, 2, 3, 0, 3, 1
            };
            for (int i = 0; i < indices.Length; i++)
                bucket.Triangles.Add(
                    start + indices[i]);
        }

        private static void AddCylinder(
            MeshBucket bucket,
            Vector3 baseCenter,
            float radius,
            float height,
            float yaw,
            int segments)
        {
            AddTaperedCylinder(
                bucket,
                baseCenter,
                radius,
                radius,
                height,
                yaw,
                segments);
        }

        private static Vector3 Local(
            float x,
            float y,
            float z,
            float yaw)
        {
            float cosine = Mathf.Cos(yaw);
            float sine = Mathf.Sin(yaw);
            return new Vector3(
                x * cosine + z * sine,
                y,
                -x * sine + z * cosine);
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < value.Length; i++)
                    hash = hash * 31 + value[i];
                return hash;
            }
        }

        private static void Cone(
            BuildingShape shape,
            MeshBucket bucket,
            float x,
            float y,
            float z,
            float radius,
            float height,
            int segments = 8)
        {
            AddTaperedCylinder(
                bucket,
                shape.Center +
                    Local(x, y, z, shape.Yaw),
                radius,
                0f,
                height,
                shape.Yaw,
                segments);
        }

        private static void Cylinder(
            BuildingShape shape,
            MeshBucket bucket,
            float x,
            float y,
            float z,
            float radius,
            float height,
            int segments = 10)
        {
            AddTaperedCylinder(
                bucket,
                shape.Center +
                    Local(x, y, z, shape.Yaw),
                radius,
                radius,
                height,
                shape.Yaw,
                segments);
        }

        private static void Dome(
            BuildingShape shape,
            MeshBucket bucket,
            float x,
            float y,
            float z,
            float radius,
            float verticalScale = 0.55f,
            int segments = 12)
        {
            int rings = 4;
            int start = bucket.Vertices.Count;
            Vector3 origin =
                shape.Center +
                Local(x, y, z, shape.Yaw);
            for (int ring = 0; ring <= rings; ring++)
            {
                float latitude =
                    ring * Mathf.PI * 0.5f / rings;
                float ringRadius =
                    Mathf.Cos(latitude) * radius;
                float ringY =
                    Mathf.Sin(latitude) *
                    radius * verticalScale;
                for (int segment = 0;
                    segment < segments;
                    segment++)
                {
                    float angle =
                        shape.Yaw +
                        segment * Mathf.PI * 2f /
                        segments;
                    bucket.Vertices.Add(
                        origin +
                        new Vector3(
                            Mathf.Cos(angle) *
                                ringRadius,
                            ringY,
                            Mathf.Sin(angle) *
                                ringRadius));
                }
            }
            for (int ring = 0; ring < rings; ring++)
            {
                for (int segment = 0;
                    segment < segments;
                    segment++)
                {
                    int next =
                        (segment + 1) % segments;
                    int lower =
                        start + ring * segments +
                        segment;
                    int lowerNext =
                        start + ring * segments +
                        next;
                    int upper =
                        lower + segments;
                    int upperNext =
                        lowerNext + segments;
                    bucket.Triangles.Add(lower);
                    bucket.Triangles.Add(upper);
                    bucket.Triangles.Add(lowerNext);
                    bucket.Triangles.Add(lowerNext);
                    bucket.Triangles.Add(upper);
                    bucket.Triangles.Add(upperNext);
                }
            }
        }

        private static void PitchedBox(
            BuildingShape shape,
            MeshBucket bucket,
            float x,
            float y,
            float z,
            float width,
            float height,
            float depth,
            float pitchDeg,
            float rollDeg = 0f,
            float localYawDeg = 0f)
        {
            Vector3 center =
                shape.Center +
                Local(x, y, z, shape.Yaw);
            Quaternion rotation =
                Quaternion.AngleAxis(
                    shape.Yaw * Mathf.Rad2Deg,
                    Vector3.up) *
                Quaternion.Euler(
                    pitchDeg,
                    localYawDeg,
                    rollDeg);
            AddOrientedBox(
                bucket,
                center,
                new Vector3(width, height, depth),
                rotation);
        }

        private static void AddTaperedCylinder(
            MeshBucket bucket,
            Vector3 baseCenter,
            float bottomRadius,
            float topRadius,
            float height,
            float yaw,
            int segments)
        {
            int start = bucket.Vertices.Count;
            bucket.Vertices.Add(baseCenter);
            bucket.Vertices.Add(
                baseCenter + Vector3.up * height);
            for (int i = 0; i < segments; i++)
            {
                float angle =
                    yaw + i * Mathf.PI * 2f /
                    segments;
                Vector3 direction =
                    new Vector3(
                        Mathf.Cos(angle),
                        0f,
                        Mathf.Sin(angle));
                bucket.Vertices.Add(
                    baseCenter +
                    direction * bottomRadius);
                bucket.Vertices.Add(
                    baseCenter +
                    Vector3.up * height +
                    direction * topRadius);
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int bottom = start + 2 + i * 2;
                int top = bottom + 1;
                int nextBottom =
                    start + 2 + next * 2;
                int nextTop = nextBottom + 1;
                bucket.Triangles.Add(start);
                bucket.Triangles.Add(nextBottom);
                bucket.Triangles.Add(bottom);
                bucket.Triangles.Add(start + 1);
                bucket.Triangles.Add(top);
                bucket.Triangles.Add(nextTop);
                bucket.Triangles.Add(bottom);
                bucket.Triangles.Add(nextBottom);
                bucket.Triangles.Add(top);
                bucket.Triangles.Add(top);
                bucket.Triangles.Add(nextBottom);
                bucket.Triangles.Add(nextTop);
            }
        }

        private static void AddOrientedBox(
            MeshBucket bucket,
            Vector3 center,
            Vector3 size,
            Quaternion rotation)
        {
            int start = bucket.Vertices.Count;
            float hx = size.x * 0.5f;
            float hy = size.y * 0.5f;
            float hz = size.z * 0.5f;
            Vector3[] corners =
            {
                new Vector3(-hx, -hy, -hz),
                new Vector3(hx, -hy, -hz),
                new Vector3(hx, hy, -hz),
                new Vector3(-hx, hy, -hz),
                new Vector3(-hx, -hy, hz),
                new Vector3(hx, -hy, hz),
                new Vector3(hx, hy, hz),
                new Vector3(-hx, hy, hz)
            };
            for (int i = 0; i < corners.Length; i++)
                bucket.Vertices.Add(
                    center + rotation * corners[i]);
            int[] indices =
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 4, 7, 0, 7, 3,
                1, 2, 6, 1, 6, 5,
                3, 7, 6, 3, 6, 2,
                0, 1, 5, 0, 5, 4
            };
            for (int i = 0; i < indices.Length; i++)
                bucket.Triangles.Add(
                    start + indices[i]);
        }

        private static void AddCrossBraces(
            BuildingShape shape,
            float x,
            float y,
            float z,
            float width,
            float height,
            bool front = true)
        {
            float length =
                Mathf.Sqrt(width * width +
                    height * height);
            float angle =
                Mathf.Atan2(width, height) *
                Mathf.Rad2Deg;
            if (front)
            {
                PitchedBox(
                    shape,
                    shape.Details,
                    x,
                    y,
                    z,
                    0.14f,
                    length,
                    0.12f,
                    0f,
                    angle);
                PitchedBox(
                    shape,
                    shape.Details,
                    x,
                    y,
                    z + 0.02f,
                    0.14f,
                    length,
                    0.12f,
                    0f,
                    -angle);
            }
        }
    }
}
