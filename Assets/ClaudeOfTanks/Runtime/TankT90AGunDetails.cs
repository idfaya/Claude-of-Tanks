using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90AGunDetails
    {
        private const float MuzzleZ = 4.92f;
        private const float SleeveRadius = 0.108f;
        private const float ForwardRadius = 0.102f;

        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;

            Transform fittings = TankDetailGeometry.GunFittingsRoot(
                gun,
                "T90A-GunFittings");
            SeatOnSourcePivot(fittings, gun);
            AddSaddle(fittings, color);
            AddCollarAndRecoilHousing(fittings, color);
            AddTube(fittings, color);
            AddMuzzle(fittings, color);
        }

        public static Transform BuildAssembly(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            Transform fittings =
                new GameObject(name).transform;
            fittings.SetParent(parent, false);
            fittings.localPosition = position;
            fittings.localScale = scale;
            AddSaddle(fittings, color);
            AddCollarAndRecoilHousing(fittings, color);
            AddTube(fittings, color);
            AddMuzzle(fittings, color);
            return fittings;
        }

        private static void SeatOnSourcePivot(
            Transform fittings,
            Transform gun)
        {
            Vector3 scale = gun.localScale;
            Vector3 sourcePivot = new Vector3(0f, 0.18f, 0.615f);
            fittings.localPosition = new Vector3(
                scale.x == 0f
                    ? 0f
                    : (sourcePivot.x - gun.localPosition.x) / scale.x,
                scale.y == 0f
                    ? 0f
                    : (sourcePivot.y - gun.localPosition.y) / scale.y,
                scale.z == 0f
                    ? 0f
                    : (sourcePivot.z - gun.localPosition.z) / scale.z);
        }

        private static void AddSaddle(
            Transform parent,
            Color color)
        {
            Cylinder(
                "Painted-T90A-2A46M2Saddle",
                parent,
                V(),
                0.22f,
                0.22f,
                0.62f,
                14,
                TankShapeAxis.X,
                color * 0.49f);
            Cylinder(
                "Painted-T90A-2A46M2RootSleeve",
                parent,
                V(0f, 0f, 0.395f),
                0.1364f,
                0.15795f,
                0.69f,
                12,
                TankShapeAxis.Z,
                color * 0.46f);
        }

        private static void AddCollarAndRecoilHousing(
            Transform parent,
            Color color)
        {
            Transform collar = Cylinder(
                "Painted-T90A-CastGunCollar",
                parent,
                V(0f, 0.02f, 0.13f),
                0.5f,
                0.46f,
                0.3f,
                16,
                TankShapeAxis.Z,
                color * 0.43f);
            collar.localScale = V(0.56f, 0.4f, 1f);
            Cylinder(
                "T90A-CoaxPort",
                parent,
                V(0.2f, 0.07f, 0.25f),
                0.02f,
                0.02f,
                0.05f,
                8,
                TankShapeAxis.Z,
                Dark());
            Cylinder(
                "T90A-CoaxPortRim",
                parent,
                V(0.2f, 0.07f, 0.272f),
                0.03f,
                0.03f,
                0.012f,
                10,
                TankShapeAxis.Z,
                Dark());
            Box("Painted-T90A-GunSightStep", parent,
                V(-0.095f, 0.395f, 0.2f),
                V(0.09f, 0.11f, 0.28f), color * 0.46f);
            Box("Painted-T90A-GunSightHousing", parent,
                V(-0.245f, 0.55f, 0.2f),
                V(0.21f, 0.24f, 0.28f), color * 0.46f);
            Box("T90A-GunSightAperture", parent,
                V(-0.245f, 0.55f, 0.341f),
                V(0.15f, 0.1f, 0.016f), Dark());
            Box("Painted-T90A-GunSightBrow", parent,
                V(-0.245f, 0.655f, 0.32f),
                V(0.21f, 0.025f, 0.05f), color * 0.46f);

            for (int side = -1; side <= 1; side += 2)
            {
                Box("Painted-T90A-RecoilCover", parent,
                    V(side * 0.1425f, 0.35f, 0.565f),
                    V(0.175f, 0.2f, 0.8f), color * 0.44f);
                Box("Painted-T90A-RecoilNose", parent,
                    V(side * 0.085f, 0.38f, 1.0675f),
                    V(0.06f, 0.2f, 0.195f), color * 0.44f);
                TankShapeFactory.OrientedSlabPart(
                    "Painted-T90A-RecoilNoseSlope",
                    parent,
                    V(side * 0.055f, 0.28f, 1.165f),
                    V(side * 0.115f, 0.28f, 1.165f),
                    V(side * 0.115f, 0.28f, 1.309f),
                    V(side * 0.055f, 0.28f, 1.309f),
                    V(side * 0.055f, 0.48f, 1.165f),
                    V(side * 0.115f, 0.48f, 1.165f),
                    V(side * 0.115f, 0.323f, 1.309f),
                    V(side * 0.055f, 0.323f, 1.309f),
                    color * 0.44f);
                Transform chamfer = Box(
                    "Painted-T90A-RecoilChamfer",
                    parent,
                    V(side * 0.185f, 0.44f, 0.565f),
                    V(0.05f, 0.014f, 0.78f),
                    color * 0.44f);
                chamfer.localRotation =
                    Quaternion.Euler(0f, 0f, side * 28.6479f);
                Box("T90A-RecoilJoint", parent,
                    V(side * 0.1425f, 0.35f, 0.958f),
                    V(0.175f, 0.18f, 0.016f), Dark());
            }
            Box("Painted-T90A-RecoilChannel", parent,
                V(0f, 0.25f, 0.565f),
                V(0.11f, 0.14f, 0.8f), color * 0.44f);
            Box("Painted-T90A-RecoilChannelNose", parent,
                V(0f, 0.26f, 1.1395f),
                V(0.06f, 0.12f, 0.339f), color * 0.44f);
            Box("Painted-T90A-RecoilChin", parent,
                V(0f, -0.055f, 0.39f),
                V(0.62f, 0.14f, 0.55f), color * 0.44f);

            AddBootRing(parent, V(0f, 0f, 0.21f), 0.54f, 0.3f);
            AddBootRing(parent, V(0f, -0.008f, 0.36f), 0.6f, 0.21f);
            AddBootRing(parent, V(0f, -0.008f, 0.52f), 0.6f, 0.21f);
            AddBootRing(
                parent,
                V(0f, 0f, 0.645f),
                0.162f,
                0.162f);
        }

        private static void AddBootRing(
            Transform parent,
            Vector3 position,
            float width,
            float height)
        {
            Transform ring = Cylinder(
                "T90A-GunBootFold",
                parent,
                position,
                0.5f,
                0.5f,
                0.04f,
                14,
                TankShapeAxis.Z,
                Dark());
            ring.localScale = V(width, height, 1f);
        }

        private static void AddTube(
            Transform parent,
            Color color)
        {
            AddTubeSection(parent, color,
                "Painted-T90A-2A46M2ThermalSleeve",
                0.65f, 1.47f, SleeveRadius, 0f);
            AddTubeSection(parent, color,
                "Painted-T90A-2A46M2ThermalSleeve",
                1.47f, 3.17f, SleeveRadius, 0f);
            AddTubeSection(parent, color,
                "Painted-T90A-2A46M2ForwardTube",
                3.17f, 4.72f, ForwardRadius, 0.005f);
            AddTubeSection(parent, color,
                "Painted-T90A-2A46M2ForwardTube",
                4.72f, MuzzleZ, ForwardRadius, 0.005f);
            foreach (Vector2 ring in new[]
                {
                    new Vector2(1.47f, 0.112f),
                    new Vector2(2.12f, 0.112f),
                    new Vector2(3.17f, 0.106f),
                    new Vector2(3.87f, 0.106f),
                    new Vector2(4.3f, 0.106f)
                })
            {
                Cylinder(
                    "T90A-2A46M2SleeveRing",
                    parent,
                    V(0f, 0f, ring.x),
                    ring.y,
                    ring.y,
                    0.045f,
                    24,
                    TankShapeAxis.Z,
                    Dark());
            }
            Cylinder(
                "Painted-T90A-2A46M2Evacuator",
                parent,
                V(0f, 0f, 2.88f),
                0.128f,
                0.116f,
                0.46f,
                14,
                TankShapeAxis.Z,
                color * 0.43f);
            Cylinder(
                "T90A-2A46M2EvacuatorBand",
                parent,
                V(0f, 0f, 3.11f),
                0.13f,
                0.13f,
                0.04f,
                14,
                TankShapeAxis.Z,
                Dark());
        }

        private static void AddTubeSection(
            Transform parent,
            Color color,
            string name,
            float zStart,
            float zEnd,
            float radius,
            float y)
        {
            Cylinder(
                name,
                parent,
                V(0f, y, (zStart + zEnd) * 0.5f),
                radius,
                radius,
                zEnd - zStart,
                24,
                TankShapeAxis.Z,
                color * 0.47f);
        }

        private static void AddMuzzle(
            Transform parent,
            Color color)
        {
            Transform collar = Cylinder(
                "Painted-T90A-MuzzleCollar",
                parent,
                V(0f, 0.033f, MuzzleZ - 0.155f),
                0.124f,
                0.124f,
                0.15f,
                14,
                TankShapeAxis.Z,
                color * 0.46f);
            collar.localScale = V(0.839f, 1f, 1f);
            Transform rim = TankShapeFactory.TorusPart(
                "T90A-MuzzleBore",
                parent,
                ForwardRadius * 0.82f,
                ForwardRadius * 0.18f,
                16,
                Dark());
            rim.localPosition = V(0f, 0.005f, MuzzleZ + 0.016f);
            rim.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Cylinder(
                "T90A-MuzzleBoreDisc",
                parent,
                V(0f, 0.005f, MuzzleZ + 0.006f),
                0.062f,
                0.062f,
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
