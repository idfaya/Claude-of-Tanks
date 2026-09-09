using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90AVladimirHullEquipmentDetails
    {
        public static void Build(
            Transform root,
            Color color)
        {
            AddTailRack(root, color);
            AddEngineDeck(root, color);
            AddGlacisKit(root, color);
            AddSideKontakt5(root, color);
            AddRearQuarterSlatCage(root);
        }

        private static void AddTailRack(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Cylinder(
                    "Painted-T90AVladimir-TailDrum",
                    root,
                    V(side * 0.62f, 1.53f, -4.40f),
                    0.14f,
                    0.14f,
                    0.44f,
                    16,
                    TankShapeAxis.Z,
                    color * 0.52f);
                Cylinder(
                    "T90AVladimir-TailDrumRim",
                    root,
                    V(side * 0.62f, 1.53f, -4.20f),
                    0.144f,
                    0.144f,
                    0.03f,
                    16,
                    TankShapeAxis.Z,
                    Dark());
                Box(
                    "T90AVladimir-TailDrumStrap",
                    root,
                    V(side * 0.62f, 1.53f, -4.63f),
                    V(0.05f, 0.13f, 0.05f),
                    Dark());
            }
            Box("Painted-T90AVladimir-TailStowage", root,
                V(0f, 1.50f, -4.45f),
                V(1.40f, 0.15f, 0.40f), color * 0.54f);
            Box("Painted-T90AVladimir-RackBack", root,
                V(0f, 1.50f, -4.20f),
                V(2.12f, 0.22f, 0.08f), color * 0.56f);
            Box("Painted-T90AVladimir-ServiceFace", root,
                V(0f, 1.38f, -4.43f),
                V(1.42f, 0.38f, 0.04f), color * 0.50f);
            Box("T90AVladimir-ServiceInset", root,
                V(0f, 1.38f, -4.458f),
                V(1.18f, 0.26f, 0.014f), Dark());
            Box("T90AVladimir-ServiceDoorL", root,
                V(-0.31f, 1.37f, -4.645f),
                V(0.44f, 0.19f, 0.02f), Dark());
            Box("T90AVladimir-ServiceDoorR", root,
                V(0.39f, 1.35f, -4.645f),
                V(0.34f, 0.16f, 0.02f), Dark());
            float[] leftLouvres = { 1.31f, 1.37f, 1.43f };
            for (int index = 0; index < leftLouvres.Length; index++)
            {
                Box("T90AVladimir-ServiceLouvre", root,
                    V(-0.31f, leftLouvres[index], -4.665f),
                    V(0.39f, 0.024f, 0.04f), Detail());
            }
            float[] rightLouvres = { 1.32f, 1.39f };
            for (int index = 0; index < rightLouvres.Length; index++)
            {
                Box("T90AVladimir-ServiceLouvre", root,
                    V(0.39f, rightLouvres[index], -4.665f),
                    V(0.29f, 0.024f, 0.04f), Detail());
            }
            Cylinder(
                "T90AVladimir-UnditchingLog",
                root,
                V(0f, 1.12f, -4.43f),
                0.095f,
                0.095f,
                1.62f,
                16,
                TankShapeAxis.X,
                Dark());
            for (int side = -1; side <= 1; side += 2)
            {
                Cylinder(
                    "T90AVladimir-LogEnd",
                    root,
                    V(side * 0.56f, 1.12f, -4.43f),
                    0.102f,
                    0.102f,
                    0.045f,
                    16,
                    TankShapeAxis.X,
                    Dark());
                Box("T90AVladimir-LogStrap", root,
                    V(side * 0.56f, 1.23f, -4.42f),
                    V(0.07f, 0.18f, 0.05f), Dark());
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color)
        {
            for (int index = 0; index < 5; index++)
            {
                Box("T90AVladimir-EngineGrille", root,
                    V(-0.60f + index * 0.30f, 1.515f, -3.35f),
                    V(0.22f, 0.025f, 0.82f), Dark());
            }
            Box("Painted-T90AVladimir-ServiceHeadL", root,
                V(-0.6425f, 1.8975f, -1.52f),
                V(0.245f, 0.185f, 0.04f), color * 0.55f);
            Box("Painted-T90AVladimir-ServiceHeadR", root,
                V(1.093f, 1.84f, -0.919f),
                V(0.14f, 0.20f, 0.04f), color * 0.55f);
        }

        private static void AddGlacisKit(
            Transform root,
            Color color)
        {
            const float upperPitch = 26.5651f;
            const float lowerPitch = -48.8141f;
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T90AVladimir-Headlight", root,
                    V(side * 0.92f, 1.18f, 1.92f),
                    V(0.22f, 0.18f, 0.16f), color * 0.56f);
                Box("T90AVladimir-HeadlightLens", root,
                    V(side * 0.92f, 1.18f, 2.008f),
                    V(0.14f, 0.10f, 0.014f),
                    TankT90AFamilyDetails.Glass());
                Transform lug = Box(
                    "T90AVladimir-TowLug",
                    root,
                    V(side * 0.64f, 0.80f, 1.86f),
                    V(0.14f, 0.10f, 0.20f),
                    Dark());
                lug.localRotation =
                    Quaternion.Euler(lowerPitch, 0f, 0f);
                Transform eye = TankShapeFactory.TorusPart(
                    "T90AVladimir-TowEye",
                    root,
                    0.068f,
                    0.014f,
                    12,
                    Dark());
                eye.localPosition =
                    V(side * 0.64f, 0.81f, 1.88f);
                eye.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                for (int row = 0; row < 2; row++)
                {
                    float y = row == 0 ? 1.245f : 1.145f;
                    float z = row == 0 ? 1.78f : 1.99f;
                    float depth = row == 0 ? 0.26f : 0.20f;
                    Transform era = Box(
                        "Painted-T90AVladimir-GlacisK5",
                        root,
                        V(side * 0.42f, y, z),
                        V(0.72f, 0.075f, depth),
                        color * 0.52f);
                    era.localRotation = Quaternion.Euler(
                        upperPitch,
                        side * 16.04f,
                        0f);
                    Transform seam = Box(
                        "T90AVladimir-GlacisK5Seam",
                        root,
                        V(side * 0.80f, y, z),
                        V(0.025f, 0.08f, depth * 0.78f),
                        Dark());
                    seam.localRotation = era.localRotation;
                }
            }
        }

        private static void AddSideKontakt5(
            Transform root,
            Color color)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 4; index++)
                {
                    float z = 1.12f - index * 0.46f;
                    Box("Painted-T90AVladimir-SideK5", root,
                        V(side * 1.845f, 1.045f, z),
                        V(0.05f, 0.63f, 0.50f), color * 0.52f);
                    Box("Painted-T90AVladimir-SideK5Lip", root,
                        V(side * 1.883f, 1.25f, z),
                        V(0.014f, 0.18f, 0.50f), color * 0.55f);
                    Box("T90AVladimir-SideK5Seam", root,
                        V(side * 1.851f, 1.045f, z - 0.25f),
                        V(0.04f, 0.55f, 0.03f), Dark());
                }
            }
        }

        private static void AddRearQuarterSlatCage(Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 5; index++)
                {
                    float y = Mathf.Lerp(0.73f, 1.30f, index / 4f);
                    Box("T90AVladimir-SlatRail", root,
                        V(side * 1.98f, y, -3.2875f),
                        V(0.025f, 0.025f, 1.425f), Dark());
                }
                for (int index = 0; index < 7; index++)
                {
                    float z = Mathf.Lerp(-4.00f, -2.575f, index / 6f);
                    Box("T90AVladimir-SlatStile", root,
                        V(side * 1.98f, 1.015f, z),
                        V(0.03f, 0.57f, 0.03f), Dark());
                }
                for (int index = 0; index < 4; index++)
                {
                    float z = Mathf.Lerp(-3.90f, -2.675f, index / 3f);
                    Box("T90AVladimir-SlatBracket", root,
                        V(side * 1.89f, 0.98f, z),
                        V(0.18f, 0.04f, 0.04f), Dark());
                }
            }
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part = TankShapeFactory.BoxPart(
                name, parent, size, color);
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
                name,
                parent,
                top,
                bottom,
                length,
                segments,
                axis,
                color);
            part.localPosition = position;
            return part;
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.055f, 0.06f, 0.045f);
        }

        private static Color Detail()
        {
            return new Color(0.16f, 0.17f, 0.15f);
        }
    }
}
