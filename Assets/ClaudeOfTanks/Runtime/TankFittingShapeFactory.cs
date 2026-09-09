using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankFittingShapeFactory
    {
        public static Transform BuildSmokeBank(
            string prefix,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            int count,
            float radius,
            float length,
            float pitch,
            float splay,
            float arc,
            float spacing,
            Color detailColor,
            Color darkColor,
            bool includeCaps = true,
            bool includeBase = true)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException(
                    "Smoke-bank prefix is required.",
                    nameof(prefix));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            int tubeCount = Mathf.Clamp(count, 1, 8);
            Transform root = Root(
                prefix + "-SmokeBank",
                parent,
                position,
                rotation);
            for (int index = 0; index < tubeCount; index++)
            {
                float offset = index - (tubeCount - 1) * 0.5f;
                float yaw = splay + offset * (arc / tubeCount);
                Vector3 center = new Vector3(
                    Mathf.Cos(splay) * offset * spacing,
                    0f,
                    -Mathf.Sin(splay) * offset * spacing);
                Quaternion tubeRotation = Quaternion.Euler(
                    pitch * Mathf.Rad2Deg,
                    yaw * Mathf.Rad2Deg,
                    0f);
                Transform tube = Cylinder(
                    prefix + "-Detail-SmokeLauncher",
                    root,
                    center,
                    radius,
                    radius,
                    length,
                    8,
                    TankShapeAxis.Z,
                    detailColor);
                tube.localRotation = tubeRotation;
                if (!includeCaps) continue;
                Transform cap = Cylinder(
                    prefix + "-SmokeLauncherCap",
                    root,
                    center + tubeRotation *
                        new Vector3(
                            0f,
                            0f,
                            length * 0.5f + 0.007f),
                    radius * 0.88f,
                    radius * 0.88f,
                    0.012f,
                    8,
                    TankShapeAxis.Z,
                    darkColor);
                cap.localRotation = tubeRotation;
            }
            if (includeBase)
            {
                Transform bracket = Box(
                    prefix + "-SmokeBankBase",
                    root,
                    new Vector3(0f, -0.06f, -0.06f),
                    new Vector3(
                        tubeCount * spacing + 0.06f,
                        0.05f,
                        0.08f),
                    darkColor);
                bracket.localRotation = Quaternion.Euler(
                    0f,
                    splay * 0.5f * Mathf.Rad2Deg,
                    0f);
            }
            return root;
        }

        public static Transform BuildAntennaWhip(
            string prefix,
            Transform parent,
            Vector3 position,
            float height,
            float radius,
            float rake,
            Color detailColor,
            Color darkColor,
            bool includeBase = true)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException(
                    "Antenna prefix is required.",
                    nameof(prefix));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            Transform root = Root(
                prefix + "-AntennaWhip",
                parent,
                position,
                Quaternion.identity);
            if (includeBase)
            {
                Cylinder(
                    prefix + "-AntennaBasePot",
                    root,
                    new Vector3(0f, 0.04f, 0f),
                    0.035f,
                    0.045f,
                    0.08f,
                    10,
                    TankShapeAxis.Y,
                    darkColor);
                Cylinder(
                    prefix + "-AntennaCollar",
                    root,
                    new Vector3(0f, 0.10f, 0f),
                    0.02f,
                    0.02f,
                    0.05f,
                    8,
                    TankShapeAxis.Y,
                    darkColor);
            }
            float baseTop = includeBase ? 0.12f : 0f;
            Transform whip = Box(
                prefix + "-Detail-RadioWhip",
                root,
                new Vector3(
                    -Mathf.Sin(rake) * height * 0.5f,
                    baseTop + Mathf.Cos(rake) * height * 0.5f,
                    0f),
                new Vector3(
                    radius * 2f,
                    height,
                    radius * 2f),
                detailColor);
            whip.localRotation =
                Quaternion.Euler(0f, 0f, rake * Mathf.Rad2Deg);
            return root;
        }

        private static Transform Root(
            string name,
            Transform parent,
            Vector3 position,
            Quaternion rotation)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            root.localRotation = rotation;
            return root;
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
    }
}
