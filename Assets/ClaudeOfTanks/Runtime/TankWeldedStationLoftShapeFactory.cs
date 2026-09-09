using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal readonly struct TankWeldedStation
    {
        public readonly float Z;
        public readonly float BottomY;
        public readonly float TopY;
        public readonly float MiddleLeftX;
        public readonly float MiddleRightX;
        public readonly float BottomLeftX;
        public readonly float BottomRightX;
        public readonly float TopLeftX;
        public readonly float TopRightX;

        public TankWeldedStation(
            float z,
            float bottomY,
            float topY,
            float middleLeftX,
            float middleRightX,
            float bottomLeftX,
            float bottomRightX,
            float topLeftX,
            float topRightX)
        {
            Z = z;
            BottomY = bottomY;
            TopY = topY;
            MiddleLeftX = middleLeftX;
            MiddleRightX = middleRightX;
            BottomLeftX = bottomLeftX;
            BottomRightX = bottomRightX;
            TopLeftX = topLeftX;
            TopRightX = topRightX;
        }
    }

    internal static class TankWeldedStationLoftShapeFactory
    {
        public static Transform Build(
            string name,
            Transform parent,
            TankWeldedStation[] stations,
            Color color)
        {
            Validate(stations);
            Vector3[,,] rings =
                new Vector3[stations.Length, 3, 2];
            for (int index = 0; index < stations.Length; index++)
            {
                TankWeldedStation station = stations[index];
                float middleY = station.BottomY +
                    (station.TopY - station.BottomY) * 0.46f;
                rings[index, 0, 0] = new Vector3(
                    station.BottomLeftX,
                    station.BottomY,
                    station.Z);
                rings[index, 0, 1] = new Vector3(
                    station.BottomRightX,
                    station.BottomY,
                    station.Z);
                rings[index, 1, 0] = new Vector3(
                    station.MiddleLeftX,
                    middleY,
                    station.Z);
                rings[index, 1, 1] = new Vector3(
                    station.MiddleRightX,
                    middleY,
                    station.Z);
                rings[index, 2, 0] = new Vector3(
                    station.TopLeftX,
                    station.TopY,
                    station.Z);
                rings[index, 2, 1] = new Vector3(
                    station.TopRightX,
                    station.TopY,
                    station.Z);
            }

            List<Vector3> vertices =
                new List<Vector3>(
                    (stations.Length - 1) * 36 + 24);
            for (int index = 0;
                index < stations.Length - 1;
                index++)
            {
                for (int level = 0; level < 2; level++)
                {
                    AddQuad(
                        vertices,
                        rings[index, level, 0],
                        rings[index + 1, level, 0],
                        rings[index + 1, level + 1, 0],
                        rings[index, level + 1, 0],
                        Vector3.left);
                    AddQuad(
                        vertices,
                        rings[index, level, 1],
                        rings[index, level + 1, 1],
                        rings[index + 1, level + 1, 1],
                        rings[index + 1, level, 1],
                        Vector3.right);
                }
                AddQuad(
                    vertices,
                    rings[index, 2, 0],
                    rings[index, 2, 1],
                    rings[index + 1, 2, 1],
                    rings[index + 1, 2, 0],
                    Vector3.up);
                AddQuad(
                    vertices,
                    rings[index, 0, 0],
                    rings[index + 1, 0, 0],
                    rings[index + 1, 0, 1],
                    rings[index, 0, 1],
                    Vector3.down);
            }

            float zDirection = Mathf.Sign(
                stations[stations.Length - 1].Z -
                stations[0].Z);
            if (zDirection == 0f) zDirection = 1f;
            AddCap(
                vertices,
                rings,
                0,
                Vector3.back * zDirection);
            AddCap(
                vertices,
                rings,
                stations.Length - 1,
                Vector3.forward * zDirection);

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

        private static void AddCap(
            List<Vector3> vertices,
            Vector3[,,] rings,
            int station,
            Vector3 expected)
        {
            for (int level = 0; level < 2; level++)
            {
                AddQuad(
                    vertices,
                    rings[station, level, 0],
                    rings[station, level, 1],
                    rings[station, level + 1, 1],
                    rings[station, level + 1, 0],
                    expected);
            }
        }

        private static void AddQuad(
            List<Vector3> vertices,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            Vector3 expected)
        {
            AddTriangle(vertices, a, b, c, expected);
            AddTriangle(vertices, a, c, d, expected);
        }

        private static void AddTriangle(
            List<Vector3> vertices,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 expected)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            vertices.Add(a);
            if (Vector3.Dot(normal, expected) < 0f)
            {
                vertices.Add(c);
                vertices.Add(b);
            }
            else
            {
                vertices.Add(b);
                vertices.Add(c);
            }
        }

        private static void Validate(TankWeldedStation[] stations)
        {
            if (stations == null || stations.Length < 2)
                throw new ArgumentException(
                    "Welded station loft requires at least two stations.",
                    nameof(stations));
            if (stations[0].TopY <= stations[0].BottomY)
                throw new ArgumentException(
                    "Welded station top must exceed its bottom.",
                    nameof(stations));
            float direction = Mathf.Sign(stations[1].Z - stations[0].Z);
            if (direction == 0f)
                throw new ArgumentException(
                    "Welded station loft requires distinct stations.",
                    nameof(stations));
            for (int index = 1; index < stations.Length; index++)
            {
                if (Mathf.Sign(
                        stations[index].Z -
                        stations[index - 1].Z) != direction)
                {
                    throw new ArgumentException(
                        "Welded stations must use one strict Z direction.",
                        nameof(stations));
                }
                if (stations[index].TopY <=
                    stations[index].BottomY)
                {
                    throw new ArgumentException(
                        "Welded station top must exceed its bottom.",
                        nameof(stations));
                }
            }
        }
    }
}
