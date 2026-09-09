using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90GunTranslation
    {
        private const float MuzzleZ = 5.15f;

        public static void Build(
            Transform turret,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings = TankDetailGeometry.GunFittingsRoot(
                gun,
                "T90-GunFittings");
            SeatOnSourcePivot(fittings, gun);

            AddSaddle(fittings, color);
            AddBoot(fittings, color);
            AddTube(fittings, color);
            AddMuzzle(fittings);
        }

        private static void SeatOnSourcePivot(
            Transform fittings,
            Transform gun)
        {
            Vector3 scale = gun.localScale;
            fittings.localPosition = new Vector3(
                0f,
                scale.y == 0f ? 0f : 0.04f / scale.y,
                scale.z == 0f ? -0.5f : -0.5f + 0.33f / scale.z);
        }

        private static void AddSaddle(
            Transform parent,
            Color color)
        {
            Cylinder("Painted-T90-2A46MSaddle", parent,
                V(), 0.22f, 0.22f, 0.62f, 14,
                TankShapeAxis.X, color * 0.49f);
            Cylinder("Painted-T90-2A46MRoot", parent,
                V(0f, 0f, 0.38f), 0.1364f, 0.14375f, 0.66f, 12,
                TankShapeAxis.Z, color * 0.46f);
        }

        private static void AddBoot(
            Transform parent,
            Color color)
        {
            Transform collar = Cylinder(
                "T90-CannonBaseBoot",
                parent,
                V(0f, 0.02f, 0.14f),
                0.5f,
                0.46f,
                0.3f,
                16,
                TankShapeAxis.Z,
                color * 0.43f);
            collar.localScale = V(0.56f, 0.4f, 1f);
            Cylinder("T90-PktCoaxPort", parent,
                V(0.2f, 0.07f, 0.26f), 0.02f, 0.02f, 0.05f, 8,
                TankShapeAxis.Z, Dark());
            Cylinder("T90-PktCoaxPortRim", parent,
                V(0.2f, 0.07f, 0.282f), 0.03f, 0.03f, 0.012f, 10,
                TankShapeAxis.Z, Dark());
            Box("Painted-T90-GunSightStep", parent,
                V(-0.095f, 0.28f, 0.2f),
                V(0.09f, 0.2f, 0.26f), color * 0.46f);
            Box("Painted-T90-GunSightHousing", parent,
                V(-0.245f, 0.36f, 0.16f),
                V(0.21f, 0.22f, 0.2f), color * 0.46f);
            Box("T90-GunMountAccessPlate", parent,
                V(-0.245f, 0.36f, 0.251f),
                V(0.15f, 0.1f, 0.016f), Dark());

            float[] z = { 0.2f, 0.34f, 0.46f, 0.62f };
            float[] width = { 0.54f, 0.4f, 0.3f, 0.25f };
            float[] height = { 0.42f, 0.32f, 0.26f, 0.22f };
            for (int index = 0; index < z.Length - 1; index++)
            {
                Transform section = TankShapeFactory.FrustumPart(
                    "Painted-T90-GunBootSection",
                    parent,
                    width[index] * 0.5f,
                    height[index] * 0.5f,
                    -height[index] * 0.5f,
                    width[index + 1] * 0.5f,
                    height[index + 1] * 0.5f,
                    -height[index + 1] * 0.5f,
                    0f,
                    z[index + 1] - z[index],
                    color * 0.4f);
                section.localPosition = V(0f, 0f, z[index]);
                section.localRotation = Quaternion.Euler(90f, 0f, 0f);
                if (index == 0) continue;
                Transform crease = Cylinder(
                    "T90-GunBootCrease",
                    parent,
                    V(0f, 0f, z[index]),
                    0.5f,
                    0.5f,
                    0.032f,
                    14,
                    TankShapeAxis.Z,
                    Dark());
                crease.localScale =
                    V(width[index] + 0.014f, height[index] + 0.014f, 1f);
            }
            Transform clamp = Cylinder(
                "T90-GunBootClamp",
                parent,
                V(0f, 0f, z[z.Length - 1] - 0.02f),
                0.5f,
                0.5f,
                0.04f,
                14,
                TankShapeAxis.Z,
                Dark());
            clamp.localScale = V(
                width[width.Length - 1] + 0.012f,
                height[height.Length - 1] + 0.012f,
                1f);
        }

        private static void AddTube(
            Transform parent,
            Color color)
        {
            AddTubeSection(parent, color,
                "Painted-T90-2A46MTubeSection", 0.44f, 1.6f,
                0.115f, 0.115f, 0f);
            AddTubeSection(parent, color,
                "Painted-T90-2A46MThermalSleeve", 1.6f, 3.3f,
                0.118f, 0.118f, 0f);
            AddTubeSection(parent, color,
                "Painted-T90-2A46MTubeSection", 3.3f, 4.9f,
                0.073f, 0.073f, 0.004f);
            AddTubeSection(parent, color,
                "Painted-T90-2A46MForwardTube", 4.9f, MuzzleZ,
                0.066f, 0.066f, 0.004f);
            foreach (Vector2 ring in new[]
                {
                    new Vector2(1.6f, 0.12f),
                    new Vector2(2.25f, 0.12f),
                    new Vector2(3.3f, 0.1f),
                    new Vector2(4.05f, 0.075f),
                    new Vector2(4.6f, 0.075f)
                })
            {
                Cylinder("T90-2A46MSleeveRing", parent,
                    V(0f, 0f, ring.x), ring.y, ring.y, 0.045f, 24,
                    TankShapeAxis.Z, Dark());
            }
            Cylinder("Painted-T90-2A46MEvacuator", parent,
                V(0f, 0f, 2.6f), 0.126f, 0.118f, 0.42f, 14,
                TankShapeAxis.Z, color * 0.43f);
            Cylinder("T90-2A46MEvacuatorBand", parent,
                V(0f, 0f, 2.81f), 0.128f, 0.128f, 0.04f, 14,
                TankShapeAxis.Z, Dark());
        }

        private static void AddTubeSection(
            Transform parent,
            Color color,
            string name,
            float zStart,
            float zEnd,
            float radiusTop,
            float radiusBottom,
            float y)
        {
            Cylinder(
                name,
                parent,
                V(0f, y, (zStart + zEnd) * 0.5f),
                radiusTop,
                radiusBottom,
                zEnd - zStart,
                24,
                TankShapeAxis.Z,
                color * 0.47f);
        }

        private static void AddMuzzle(Transform parent)
        {
            Transform rim = TankShapeFactory.TorusPart(
                "T90-MuzzleBore",
                parent,
                0.05412f,
                0.01188f,
                16,
                Dark());
            rim.localPosition = V(0f, 0.004f, 5.146f);
            rim.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Cylinder("T90-MuzzleBoreDisc", parent,
                V(0f, 0.004f, 5.136f),
                0.04092f,
                0.04092f,
                0.012f,
                14,
                TankShapeAxis.Z,
                Color.black);
        }

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radiusTop,
            float radiusBottom,
            float length,
            int segments,
            TankShapeAxis axis,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                radiusTop,
                radiusBottom,
                length,
                segments,
                axis,
                color);
            part.localPosition = position;
            return part;
        }

        private static Transform Box(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Color color)
        {
            Transform part = TankShapeFactory.BoxPart(
                name,
                parent,
                size,
                color);
            part.localPosition = position;
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
    }
}
