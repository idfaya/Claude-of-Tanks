using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal readonly struct TankTrackLoopEnd
    {
        public readonly float Z;
        public readonly float Y;
        public readonly float Radius;
        public readonly int ArcSteps;

        public TankTrackLoopEnd(
            float z,
            float y,
            float radius,
            int arcSteps)
        {
            Z = z;
            Y = y;
            Radius = radius;
            ArcSteps = arcSteps;
        }
    }

    internal readonly struct TankTrackSupport
    {
        public readonly float Z;
        public readonly float Y;
        public readonly float Radius;

        public TankTrackSupport(
            float z,
            float y,
            float radius)
        {
            Z = z;
            Y = y;
            Radius = radius;
        }
    }

    internal static class TankLinkedTrackShapeFactory
    {
        private const float WrapClearance = 0.025f;

        public static Transform Build(
            string prefix,
            Transform parent,
            float laneX,
            float width,
            float trackThickness,
            float linkPitch,
            float topY,
            float bottomY,
            float frontContactZ,
            float rearContactZ,
            TankTrackLoopEnd front,
            TankTrackLoopEnd rear,
            TankTrackSupport[] supports,
            bool smoothRearTopTangent,
            Color color)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException(
                    "Track prefix is required.",
                    nameof(prefix));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (supports == null)
                throw new ArgumentNullException(nameof(supports));
            if (frontContactZ <= rearContactZ)
                throw new ArgumentException(
                    "Front contact must be ahead of rear contact.");
            List<Vector2> loop = BuildLoop(
                topY,
                bottomY,
                frontContactZ,
                rearContactZ,
                front,
                rear,
                supports,
                trackThickness,
                smoothRearTopTangent);
            Transform root =
                new GameObject(prefix + "-TrackCourse").transform;
            root.SetParent(parent, false);
            BuildLane(
                prefix,
                root,
                -laneX,
                width,
                trackThickness,
                linkPitch,
                loop,
                color);
            BuildLane(
                prefix,
                root,
                laneX,
                width,
                trackThickness,
                linkPitch,
                loop,
                color);
            return root;
        }

        private static List<Vector2> BuildLoop(
            float topY,
            float bottomY,
            float frontContactZ,
            float rearContactZ,
            TankTrackLoopEnd front,
            TankTrackLoopEnd rear,
            TankTrackSupport[] supports,
            float trackThickness,
            bool smoothRearTopTangent)
        {
            float frontRadius = Mathf.Max(
                0.05f,
                front.Radius + WrapClearance);
            float rearRadius = Mathf.Max(
                0.05f,
                rear.Radius + WrapClearance);
            Vector2 frontCenter =
                new Vector2(front.Z, front.Y);
            Vector2 rearCenter =
                new Vector2(rear.Z, rear.Y);
            List<TankTrackSupport> ordered =
                new List<TankTrackSupport>(supports);
            ordered.Sort((left, right) =>
                left.Z.CompareTo(right.Z));
            List<Vector2> inner = new List<Vector2>();
            for (int index = 0; index < ordered.Count; index++)
            {
                if (ordered[index].Z - rear.Z <= 0.12f ||
                    front.Z - ordered[index].Z <= 0.12f)
                {
                    continue;
                }
                inner.Add(new Vector2(
                    ordered[index].Z,
                    ordered[index].Y +
                        ordered[index].Radius +
                        trackThickness * 0.5f));
            }
            float rearTopDegrees = 0f;
            Vector2 rearTop = new Vector2(
                rear.Z,
                rear.Y + rearRadius);
            if (smoothRearTopTangent)
            {
                float candidate = TangentDegrees(
                    rearCenter,
                    rearRadius,
                    inner.Count > 0
                        ? inner[0]
                        : new Vector2(front.Z, topY),
                    1f);
                if (candidate > 0f && candidate < 90f)
                {
                    rearTopDegrees = candidate;
                    float angle = candidate * Mathf.Deg2Rad;
                    rearTop = new Vector2(
                        rear.Z + Mathf.Sin(angle) * rearRadius,
                        rear.Y + Mathf.Cos(angle) * rearRadius);
                }
            }
            List<Vector2> top = new List<Vector2> { rearTop };
            if (inner.Count > 0)
                top.AddRange(inner);
            else
                top.Add(new Vector2(
                    (rear.Z + front.Z) * 0.5f,
                    Mathf.Max(
                        topY,
                        (rear.Y + rearRadius +
                         front.Y + frontRadius) * 0.5f)));
            top.Add(new Vector2(
                front.Z,
                front.Y + frontRadius));

            List<Vector2> points = new List<Vector2>(96);
            for (int spanIndex = 0;
                spanIndex < top.Count - 1;
                spanIndex++)
            {
                Vector2 start = top[spanIndex];
                Vector2 end = top[spanIndex + 1];
                float span = Mathf.Abs(end.x - start.x);
                float dip = Mathf.Min(
                    0.022f,
                    0.022f * span * 1.6f);
                int steps = Mathf.Clamp(
                    Mathf.RoundToInt(span * 5f),
                    2,
                    6);
                for (int step = spanIndex == 0 ? 0 : 1;
                    step <= steps;
                    step++)
                {
                    float t = step / (float)steps;
                    points.Add(new Vector2(
                        Mathf.Lerp(start.x, end.x, t),
                        Mathf.Lerp(start.y, end.y, t) -
                            dip * Mathf.Sin(t * Mathf.PI)));
                }
            }

            float frontTangent = TangentDegrees(
                frontCenter,
                frontRadius,
                new Vector2(frontContactZ, bottomY),
                1f);
            float rearTangent = TangentDegrees(
                rearCenter,
                rearRadius,
                new Vector2(rearContactZ, bottomY),
                -1f);
            float frontGround =
                GroundDegrees(front.Y, frontRadius, bottomY);
            float rearGround =
                GroundDegrees(rear.Y, rearRadius, bottomY);
            float frontEndDegrees = Mathf.Min(
                Mathf.Max(
                    float.IsNaN(frontTangent)
                        ? 170f
                        : frontTangent,
                    120f),
                Mathf.Min(176f, frontGround));
            float rearStartDegrees = Mathf.Max(
                Mathf.Min(
                    float.IsNaN(rearTangent)
                        ? 190f
                        : rearTangent,
                    244f),
                Mathf.Max(184f, 360f - rearGround));
            AddArc(
                points,
                frontCenter,
                frontRadius,
                0f,
                frontEndDegrees,
                Mathf.Max(6, front.ArcSteps));

            float frontEnterZ =
                Mathf.Approximately(frontEndDegrees, frontGround)
                    ? front.Z +
                      Mathf.Sin(frontEndDegrees * Mathf.Deg2Rad) *
                      frontRadius
                    : frontContactZ;
            float rearEnterZ =
                Mathf.Approximately(
                    rearStartDegrees,
                    360f - rearGround)
                    ? rear.Z +
                      Mathf.Sin(rearStartDegrees * Mathf.Deg2Rad) *
                      rearRadius
                    : rearContactZ;
            float groundFront = Mathf.Min(
                frontContactZ,
                frontEnterZ);
            float groundRear = Mathf.Max(
                rearContactZ,
                rearEnterZ);
            for (int step = 0; step <= 5; step++)
            {
                points.Add(new Vector2(
                    Mathf.Lerp(
                        groundFront,
                        groundRear,
                        step / 5f),
                    bottomY));
            }
            AddArc(
                points,
                rearCenter,
                rearRadius,
                rearStartDegrees,
                360f + rearTopDegrees,
                Mathf.Max(6, rear.ArcSteps));
            if (points.Count > 0)
                points.RemoveAt(points.Count - 1);
            for (int index = 0; index < points.Count; index++)
            {
                if (points[index].y < bottomY)
                    points[index] =
                        new Vector2(points[index].x, bottomY);
            }
            float area2 = 0f;
            for (int index = 0; index < points.Count; index++)
            {
                Vector2 a = points[index];
                Vector2 b = points[(index + 1) % points.Count];
                area2 += a.x * b.y - b.x * a.y;
            }
            if (area2 > 0f) points.Reverse();
            return points;
        }

        private static void AddArc(
            List<Vector2> points,
            Vector2 center,
            float radius,
            float fromDegrees,
            float toDegrees,
            int steps)
        {
            for (int index = 0; index <= steps; index++)
            {
                float degrees = Mathf.Lerp(
                    fromDegrees,
                    toDegrees,
                    index / (float)steps);
                float angle = degrees * Mathf.Deg2Rad;
                points.Add(new Vector2(
                    center.x + Mathf.Sin(angle) * radius,
                    center.y + Mathf.Cos(angle) * radius));
            }
        }

        private static float TangentDegrees(
            Vector2 center,
            float radius,
            Vector2 point,
            float sign)
        {
            float z = point.x - center.x;
            float y = point.y - center.y;
            float distance = Mathf.Sqrt(z * z + y * y);
            if (distance <= radius + 0.0001f)
                return float.NaN;
            float phi = Mathf.Atan2(z, y);
            float angle =
                (phi - sign * Mathf.Acos(radius / distance)) *
                Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            return angle;
        }

        private static float GroundDegrees(
            float centerY,
            float radius,
            float bottomY)
        {
            float cosine = (bottomY - centerY) / radius;
            if (cosine <= -1f) return float.PositiveInfinity;
            return Mathf.Acos(Mathf.Min(1f, cosine)) *
                Mathf.Rad2Deg;
        }

        private static void BuildLane(
            string prefix,
            Transform parent,
            float x,
            float width,
            float trackThickness,
            float linkPitch,
            List<Vector2> points,
            Color color)
        {
            float[] cumulative = new float[points.Count + 1];
            float total = 0f;
            for (int index = 0; index < points.Count; index++)
            {
                Vector2 start = points[index];
                Vector2 end =
                    points[(index + 1) % points.Count];
                total += Vector2.Distance(start, end);
                cumulative[index + 1] = total;
            }
            int count = Mathf.Max(
                32,
                Mathf.RoundToInt(
                    total / Mathf.Max(0.08f, linkPitch)));
            float spacing = total / count;
            for (int index = 0; index < count; index++)
            {
                float distance = index * spacing;
                int segment = 0;
                while (segment + 1 < cumulative.Length &&
                    cumulative[segment + 1] < distance)
                {
                    segment++;
                }
                Vector2 start = points[segment % points.Count];
                Vector2 end =
                    points[(segment + 1) % points.Count];
                float segmentLength =
                    Mathf.Max(0.00001f, Vector2.Distance(start, end));
                float t = (distance - cumulative[segment]) /
                    segmentLength;
                Vector2 point = Vector2.Lerp(start, end, t);
                Vector2 direction = (end - start).normalized;
                Transform pad = TankShapeFactory.BoxPart(
                    prefix + "-TrackPad",
                    parent,
                    new Vector3(
                        width,
                        trackThickness,
                        spacing * 0.88f),
                    color);
                pad.localPosition =
                    new Vector3(x, point.y, point.x);
                float angle = Mathf.Atan2(
                    direction.y,
                    direction.x) * Mathf.Rad2Deg;
                pad.localRotation =
                    Quaternion.Euler(-angle, 0f, 0f);
            }
        }
    }
}
