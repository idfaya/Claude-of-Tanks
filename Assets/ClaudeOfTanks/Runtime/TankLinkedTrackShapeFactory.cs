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
                smoothRearTopTangent);
            Transform root =
                new GameObject(prefix + "-TrackCourse").transform;
            root.SetParent(parent, false);
            BuildLane(
                prefix,
                root,
                -laneX,
                width,
                linkPitch,
                loop,
                color);
            BuildLane(
                prefix,
                root,
                laneX,
                width,
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
            bool smoothRearTopTangent)
        {
            float frontRadius = Mathf.Max(
                0.05f,
                front.Radius + WrapClearance);
            float rearRadius = Mathf.Max(
                0.05f,
                rear.Radius + WrapClearance);
            float frontCenterY = bottomY + frontRadius;
            float rearCenterY = bottomY + rearRadius;
            float frontCenterZ = Mathf.Max(
                frontContactZ,
                front.Z);
            float rearCenterZ = Mathf.Min(
                rearContactZ,
                rear.Z);
            List<Vector2> points =
                new List<Vector2>(64)
                {
                    new Vector2(frontContactZ, bottomY)
                };
            int frontSteps = Mathf.Max(6, front.ArcSteps);
            for (int index = 0; index <= frontSteps; index++)
            {
                float angle = Mathf.PI * index / frontSteps;
                points.Add(new Vector2(
                    frontCenterZ +
                        frontRadius * Mathf.Sin(angle),
                    frontCenterY -
                        frontRadius * Mathf.Cos(angle)));
            }
            List<TankTrackSupport> ordered =
                new List<TankTrackSupport>(supports);
            ordered.Sort((left, right) =>
                left.Z.CompareTo(right.Z));
            for (int index = 0; index < ordered.Count; index++)
            {
                points.Add(new Vector2(
                    ordered[index].Z,
                    Mathf.Max(
                        topY,
                        ordered[index].Y +
                        ordered[index].Radius +
                        WrapClearance)));
            }
            if (smoothRearTopTangent)
            {
                float angle = Mathf.PI * 0.74f;
                points.Add(new Vector2(
                    rearCenterZ +
                        rearRadius * Mathf.Cos(angle),
                    rearCenterY +
                        rearRadius * Mathf.Sin(angle)));
            }
            int rearSteps = Mathf.Max(6, rear.ArcSteps);
            for (int index = 0; index <= rearSteps; index++)
            {
                float angle =
                    Mathf.PI + Mathf.PI * index / rearSteps;
                points.Add(new Vector2(
                    rearCenterZ +
                        rearRadius * Mathf.Sin(angle),
                    rearCenterY -
                        rearRadius * Mathf.Cos(angle)));
            }
            points.Add(new Vector2(rearContactZ, bottomY));
            return points;
        }

        private static void BuildLane(
            string prefix,
            Transform parent,
            float x,
            float width,
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
                        0.07f,
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
