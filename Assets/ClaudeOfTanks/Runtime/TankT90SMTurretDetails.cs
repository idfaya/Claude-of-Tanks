using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90SMTurretDetails
    {
        private static readonly Vector2[] Plan =
        {
            V2(-0.2325f, 1.26f),
            V2(0.2325f, 1.26f),
            V2(0.98f, 1.26f),
            V2(1.19f, 1.44f),
            V2(1.2985f, 1.377f),
            V2(1.4054f, 1.27f),
            V2(1.5035f, 1.12f),
            V2(1.55f, 0.55f),
            V2(1.44f, -0.395f),
            V2(1.09f, -0.80f),
            V2(-1.09f, -0.80f),
            V2(-1.44f, -0.395f),
            V2(-1.55f, 0.55f),
            V2(-1.5035f, 1.12f),
            V2(-1.4054f, 1.27f),
            V2(-1.2985f, 1.377f),
            V2(-1.19f, 1.44f),
            V2(-0.98f, 1.26f)
        };

        public static void Build(
            Transform turret,
            Color color)
        {
            HideRenderer(turret.Find("Turret"));
            GameObject rootObject =
                new GameObject("T90SM-PresentationRoot");
            Transform root = rootObject.transform;
            root.SetParent(turret, false);
            root.localPosition =
                new Vector3(0f, 0.09372385f, -0.06f);

            AddFoundation(root, color);
            AddTurretRing(root, color);
            AddCupolas(root, color);
            TankT90SMTurretArmorDetails.Build(root, color);
        }

        private static void AddFoundation(
            Transform root,
            Color color)
        {
            TankVariableBaseTurretShapeFactory.Build(
                "Painted-T90SM-WeldedFoundation",
                root,
                Plan,
                0.515f,
                1.02f,
                0.78f,
                BaseAtZ,
                new[] { 0.50f, 0.55f },
                color * 0.63f);
            Box(
                "Painted-T90SM-RearCastingShelf",
                root,
                V(0f, 0.3125f, -0.95f),
                V(1.90f, 0.425f, 0.70f),
                color * 0.61f);
            Box(
                "Painted-T90SM-CrownPlate",
                root,
                V(0f, 0.55f, -0.025f),
                V(1.24f, 0.07f, 1.05f),
                color * 0.65f);
        }

        private static void AddTurretRing(
            Transform root,
            Color color)
        {
            Transform ring = TankShapeFactory.CylinderPart(
                "Painted-T90SM-TurretRing",
                root,
                1.08f,
                1.14f,
                0.12f,
                28,
                TankShapeAxis.Y,
                color * 0.56f);
            ring.localPosition = V(0f, 0.02f, -0.06f);
        }

        private static void AddCupolas(
            Transform root,
            Color color)
        {
            Cylinder(
                "T90SM-CommanderCupolaBase",
                root,
                V(-0.395f, 0.591f, 0.10f),
                0.19f,
                0.19f,
                0.012f,
                Dark());
            Cylinder(
                "Painted-T90SM-CommanderCupolaDrum",
                root,
                V(-0.395f, 0.632f, 0.10f),
                0.155f,
                0.165f,
                0.07f,
                color * 0.60f);
            Cylinder(
                "T90SM-CommanderCupolaRim",
                root,
                V(-0.395f, 0.6735f, 0.10f),
                0.165f,
                0.165f,
                0.013f,
                Dark());
            Cylinder(
                "Painted-T90SM-CommanderCupolaLid",
                root,
                V(-0.395f, 0.680f, 0.10f),
                0.135f,
                0.135f,
                0.018f,
                color * 0.62f);
            Box(
                "Painted-T90SM-CommanderPeriscope",
                root,
                V(-0.28f, 0.665f, 0.10f),
                V(0.10f, 0.13f, 0.07f),
                color * 0.56f);
            Box(
                "T90SM-CommanderPeriscopeGlass",
                root,
                V(-0.28f, 0.675f, 0.139f),
                V(0.08f, 0.055f, 0.008f),
                Glass());

            Cylinder(
                "T90SM-GunnerCupolaBase",
                root,
                V(0.33f, 0.591f, -0.16f),
                0.17f,
                0.17f,
                0.012f,
                Dark());
            Cylinder(
                "Painted-T90SM-GunnerCupolaDrum",
                root,
                V(0.33f, 0.6325f, -0.16f),
                0.155f,
                0.17f,
                0.075f,
                color * 0.60f);
            Cylinder(
                "T90SM-GunnerCupolaRim",
                root,
                V(0.33f, 0.677f, -0.16f),
                0.17f,
                0.17f,
                0.014f,
                Dark());
            Cylinder(
                "Painted-T90SM-GunnerCupolaLid",
                root,
                V(0.33f, 0.678f, -0.16f),
                0.132f,
                0.132f,
                0.012f,
                color * 0.62f);
            Box(
                "T90SM-CommanderHatchHinge",
                root,
                V(-0.395f, 0.596f, 0.315f),
                V(0.10f, 0.022f, 0.03f),
                Detail());
            Box(
                "T90SM-GunnerHatchHinge",
                root,
                V(0.33f, 0.596f, 0.03f),
                V(0.09f, 0.022f, 0.03f),
                Detail());
        }

        private static float BaseAtZ(float z)
        {
            if (z <= 0.50f) return 0f;
            if (z >= 0.55f) return 0.08f;
            return (z - 0.50f) * 1.6f;
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
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                top,
                bottom,
                length,
                16,
                TankShapeAxis.Y,
                color);
            part.localPosition = position;
            return part;
        }

        private static void HideRenderer(Transform part)
        {
            if (part == null) return;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
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

        private static Color Detail()
        {
            return new Color(0.16f, 0.17f, 0.15f);
        }

        private static Color Glass()
        {
            return TankT90AFamilyDetails.Glass();
        }
    }
}
