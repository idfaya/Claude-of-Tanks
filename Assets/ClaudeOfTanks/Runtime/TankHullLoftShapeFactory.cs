using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal readonly struct TankHullProfilePoint
    {
        public readonly float Z;
        public readonly float Value;

        public TankHullProfilePoint(float z, float value)
        {
            Z = z;
            Value = value;
        }
    }

    internal static class TankHullLoftShapeFactory
    {
        private const float MaximumStationPitch = 0.36f;
        private const float MinimumSlabDepth = 0.015f;

        public static Transform Build(
            string name,
            Transform parent,
            TankHullProfilePoint[] deck,
            TankHullProfilePoint[] belly,
            TankHullProfilePoint[] upperWidth,
            TankHullProfilePoint[] lowerWidth,
            float sponsonY,
            Color color)
        {
            return Build(
                name,
                parent,
                deck,
                belly,
                upperWidth,
                lowerWidth,
                null,
                sponsonY,
                color);
        }

        public static Transform Build(
            string name,
            Transform parent,
            TankHullProfilePoint[] deck,
            TankHullProfilePoint[] belly,
            TankHullProfilePoint[] upperWidth,
            TankHullProfilePoint[] lowerWidth,
            TankHullProfilePoint[] sponson,
            Color color)
        {
            return Build(
                name,
                parent,
                deck,
                belly,
                upperWidth,
                lowerWidth,
                sponson,
                0f,
                color);
        }

        private static Transform Build(
            string name,
            Transform parent,
            TankHullProfilePoint[] deck,
            TankHullProfilePoint[] belly,
            TankHullProfilePoint[] upperWidth,
            TankHullProfilePoint[] lowerWidth,
            TankHullProfilePoint[] sponson,
            float scalarSponsonY,
            Color color)
        {
            ValidateCurve(deck, nameof(deck));
            ValidateCurve(belly, nameof(belly));
            ValidateCurve(upperWidth, nameof(upperWidth));
            ValidateCurve(lowerWidth, nameof(lowerWidth));
            if (sponson != null)
                ValidateCurve(sponson, nameof(sponson));

            List<float> stations = BuildStations(
                deck,
                belly,
                upperWidth,
                lowerWidth,
                sponson);
            List<Vector3> vertices = new List<Vector3>(
                Math.Max(36, (stations.Count - 1) * 72));
            for (int index = 0; index < stations.Count - 1; index++)
            {
                float z0 = stations[index];
                float z1 = stations[index + 1];
                if (z1 - z0 < MinimumSlabDepth) continue;

                float deck0 = Resolve(deck, z0);
                float deck1 = Resolve(deck, z1);
                float belly0 = Resolve(belly, z0);
                float belly1 = Resolve(belly, z1);
                float sponson0 = Mathf.Min(
                    sponson == null
                        ? scalarSponsonY
                        : Resolve(sponson, z0),
                    deck0 - 0.01f);
                float sponson1 = Mathf.Min(
                    sponson == null
                        ? scalarSponsonY
                        : Resolve(sponson, z1),
                    deck1 - 0.01f);
                float upperBottom0 = Mathf.Max(sponson0, belly0);
                float upperBottom1 = Mathf.Max(sponson1, belly1);
                float upperWidth0 = Resolve(upperWidth, z0);
                float upperWidth1 = Resolve(upperWidth, z1);
                float lowerWidth0 = Resolve(lowerWidth, z0);
                float lowerWidth1 = Resolve(lowerWidth, z1);

                if (deck0 > upperBottom0 + 0.012f ||
                    deck1 > upperBottom1 + 0.012f)
                {
                    AppendSlab(
                        vertices,
                        V(-upperWidth1, upperBottom1, z1),
                        V(upperWidth1, upperBottom1, z1),
                        V(upperWidth0, upperBottom0, z0),
                        V(-upperWidth0, upperBottom0, z0),
                        V(-upperWidth1, deck1, z1),
                        V(upperWidth1, deck1, z1),
                        V(upperWidth0, deck0, z0),
                        V(-upperWidth0, deck0, z0));
                }
                if (upperBottom0 > belly0 + 0.012f ||
                    upperBottom1 > belly1 + 0.012f)
                {
                    AppendSlab(
                        vertices,
                        V(-lowerWidth1, belly1, z1),
                        V(lowerWidth1, belly1, z1),
                        V(lowerWidth0, belly0, z0),
                        V(-lowerWidth0, belly0, z0),
                        V(-lowerWidth1, upperBottom1, z1),
                        V(lowerWidth1, upperBottom1, z1),
                        V(lowerWidth0, upperBottom0, z0),
                        V(-lowerWidth0, upperBottom0, z0));
                }
            }
            if (vertices.Count == 0)
                throw new ArgumentException(
                    "Hull loft produced no visible slabs.");
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

        private static List<float> BuildStations(
            TankHullProfilePoint[] deck,
            TankHullProfilePoint[] belly,
            TankHullProfilePoint[] upperWidth,
            TankHullProfilePoint[] lowerWidth,
            TankHullProfilePoint[] sponson)
        {
            SortedSet<float> raw = new SortedSet<float>();
            AddStations(raw, deck);
            AddStations(raw, belly);
            AddStations(raw, upperWidth);
            AddStations(raw, lowerWidth);
            if (sponson != null) AddStations(raw, sponson);
            float[] knots = new float[raw.Count];
            raw.CopyTo(knots);
            List<float> stations = new List<float>();
            for (int index = 0; index < knots.Length; index++)
            {
                stations.Add(knots[index]);
                if (index >= knots.Length - 1) continue;
                float span = knots[index + 1] - knots[index];
                int cuts = Mathf.FloorToInt(
                    span / MaximumStationPitch);
                for (int cut = 1; cut <= cuts; cut++)
                {
                    stations.Add(
                        knots[index] +
                        span * cut / (cuts + 1));
                }
            }
            stations.Sort();
            return stations;
        }

        private static void AddStations(
            SortedSet<float> output,
            TankHullProfilePoint[] curve)
        {
            for (int index = 0; index < curve.Length; index++)
                output.Add(curve[index].Z);
        }

        private static float Resolve(
            TankHullProfilePoint[] curve,
            float z)
        {
            if (z <= curve[0].Z) return curve[0].Value;
            for (int index = 1; index < curve.Length; index++)
            {
                if (z > curve[index].Z) continue;
                TankHullProfilePoint previous = curve[index - 1];
                TankHullProfilePoint next = curve[index];
                float span = Mathf.Max(
                    0.000001f,
                    next.Z - previous.Z);
                return previous.Value +
                    (next.Value - previous.Value) *
                    ((z - previous.Z) / span);
            }
            return curve[curve.Length - 1].Value;
        }

        private static void ValidateCurve(
            TankHullProfilePoint[] curve,
            string parameter)
        {
            if (curve == null || curve.Length == 0)
                throw new ArgumentException(
                    "Hull loft curves require at least one point.",
                    parameter);
            for (int index = 1; index < curve.Length; index++)
            {
                if (curve[index].Z <= curve[index - 1].Z)
                    throw new ArgumentException(
                        "Hull loft curve stations must be strictly ascending.",
                        parameter);
            }
        }

        private static void AppendSlab(
            List<Vector3> vertices,
            Vector3 bottom0,
            Vector3 bottom1,
            Vector3 bottom2,
            Vector3 bottom3,
            Vector3 top0,
            Vector3 top1,
            Vector3 top2,
            Vector3 top3)
        {
            Vector3[] corners =
            {
                bottom0, bottom1, bottom2, bottom3,
                top0, top1, top2, top3
            };
            if (!HasOutwardRing(corners))
            {
                corners = new[]
                {
                    bottom0, bottom3, bottom2, bottom1,
                    top0, top3, top2, top1
                };
            }
            AddQuad(vertices, corners[0], corners[1], corners[5], corners[4]);
            AddQuad(vertices, corners[1], corners[2], corners[6], corners[5]);
            AddQuad(vertices, corners[2], corners[3], corners[7], corners[6]);
            AddQuad(vertices, corners[3], corners[0], corners[4], corners[7]);
            AddQuad(vertices, corners[4], corners[5], corners[6], corners[7]);
            AddQuad(vertices, corners[3], corners[2], corners[1], corners[0]);
        }

        private static bool HasOutwardRing(Vector3[] corners)
        {
            Vector3 center = Vector3.zero;
            for (int index = 0; index < corners.Length; index++)
                center += corners[index];
            center /= corners.Length;
            int[][] faces =
            {
                new[] { 0, 1, 5, 4 },
                new[] { 1, 2, 6, 5 },
                new[] { 2, 3, 7, 6 },
                new[] { 3, 0, 4, 7 },
                new[] { 4, 5, 6, 7 },
                new[] { 3, 2, 1, 0 }
            };
            int outward = 0;
            for (int index = 0; index < faces.Length; index++)
            {
                int[] face = faces[index];
                Vector3 normal = Vector3.Cross(
                    corners[face[1]] - corners[face[0]],
                    corners[face[2]] - corners[face[0]]);
                Vector3 faceCenter =
                    (corners[face[0]] +
                     corners[face[1]] +
                     corners[face[2]] +
                     corners[face[3]]) * 0.25f;
                if (Vector3.Dot(normal, faceCenter - center) > 0f)
                    outward++;
            }
            return outward >= 3;
        }

        private static void AddQuad(
            List<Vector3> vertices,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d)
        {
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(a);
            vertices.Add(c);
            vertices.Add(d);
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
    }
}
