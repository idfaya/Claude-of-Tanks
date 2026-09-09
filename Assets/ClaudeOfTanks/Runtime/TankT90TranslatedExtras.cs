using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90TranslatedExtras
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            if (root == null || turret == null) return;
            AddTurretRoofDetails(turret, color);
            AddBustleRackDetails(turret);
            AddHullSkirtFasteners(root);
        }

        private static void AddTurretRoofDetails(
            Transform turret,
            Color color)
        {
            Part("Painted-T90-CupolaGuardArc", PrimitiveType.Cube, turret,
                V(0.52f, 0.83f, -0.01f), V(0.5f, 0.035f, 0.055f),
                V(0f, 18f, 0f), color * 0.5f);
            Part("Painted-T90-CupolaGuardArc", PrimitiveType.Cube, turret,
                V(0.76f, 0.83f, -0.3f), V(0.055f, 0.035f, 0.42f),
                V(0f, -12f, 0f), color * 0.5f);
            Part("Painted-T90-GunnerThermalHead", PrimitiveType.Cube, turret,
                V(-0.34f, 0.86f, 0.03f), V(0.32f, 0.16f, 0.26f),
                V(0f, -8f, 0f), color * 0.62f);
            Part("T90-GunnerThermalLens", PrimitiveType.Cube, turret,
                V(-0.34f, 0.87f, 0.18f), V(0.22f, 0.09f, 0.018f),
                V(), Glass());
            for (int index = 0; index < 5; index++)
                Part("Painted-T90-CupolaPeriscope", PrimitiveType.Cube,
                    turret,
                    V(0.52f + (index - 2) * 0.1f, 0.87f,
                        -0.04f - Mathf.Abs(index - 2) * 0.04f),
                    V(0.075f, 0.035f, 0.08f),
                    V(0f, (index - 2) * 12f, 0f), color * 0.45f);
        }

        private static void AddBustleRackDetails(
            Transform turret)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Part("T90-BustleSideRail", PrimitiveType.Cube, turret,
                    V(side * 0.82f, 0.78f, -1.53f),
                    V(0.035f, 0.3f, 0.5f), V(), Dark());
                for (int rung = 0; rung < 3; rung++)
                    Part("T90-BustleRung", PrimitiveType.Cube, turret,
                        V(side * 0.42f, 0.66f + rung * 0.095f, -1.78f),
                        V(0.68f, 0.025f, 0.035f), V(), Dark());
            }
        }

        private static void AddHullSkirtFasteners(
            Transform root)
        {
            for (int side = -1; side <= 1; side += 2)
            for (int station = 0; station < 5; station++)
            {
                Part("T90-SkirtHingeBracket", PrimitiveType.Cube, root,
                    V(side * 1.86f, 1.39f, -1.28f + station * 0.76f),
                    V(0.035f, 0.08f, 0.12f), V(), Dark());
                Part("T90-SkirtLowerPin", PrimitiveType.Cylinder, root,
                    V(side * 1.86f, 0.66f, -1.28f + station * 0.76f),
                    V(0.022f, 0.026f, 0.022f), V(0f, 0f, 90f), Dark());
            }
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Vector3 rotation,
            Color color)
        {
            Transform part = TankDetailGeometry.Part(
                name,
                type,
                parent,
                position,
                scale,
                color);
            part.localRotation = Quaternion.Euler(rotation);
            return part;
        }

        private static Vector3 V(
            float x = 0f,
            float y = 0f,
            float z = 0f)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return new Color(0.055f, 0.065f, 0.05f);
        }

        private static Color Glass()
        {
            return new Color(0.025f, 0.08f, 0.075f);
        }
    }
}
