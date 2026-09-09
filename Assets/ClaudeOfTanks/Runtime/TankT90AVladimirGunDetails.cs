using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90AVladimirGunDetails
    {
        private const float MuzzleZ = 5.32f;

        public static void Build(
            Transform turret,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Renderer renderer = gun.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
            Transform root = TankDetailGeometry.GunFittingsRoot(
                gun,
                "T90AVladimir-GunFittings");
            Seat(root, gun);

            Cylinder("Painted-T90AVladimir-Saddle", root,
                V(), 0.17f, 0.17f, 0.70f, 16,
                TankShapeAxis.X, color * 0.48f);
            Cylinder("Painted-T90AVladimir-RootSleeve", root,
                V(0f, 0f, 0.44f),
                0.125f, 0.135f, 0.78f, 14,
                TankShapeAxis.Z, color * 0.46f);
            Box("Painted-T90AVladimir-CastGunRoot", root,
                V(0f, 0.045f, 0.14f),
                V(0.56f, 0.18f, 0.30f), color * 0.45f);
            AddBoot(root);
            AddTube(root, color);
            AddMuzzle(root, color);
        }

        private static void Seat(
            Transform root,
            Transform gun)
        {
            Vector3 scale = gun.localScale;
            Vector3 pivot = V(0f, 0.34f, 0.15f);
            root.localPosition = new Vector3(
                (pivot.x - gun.localPosition.x) / scale.x,
                (pivot.y - gun.localPosition.y) / scale.y,
                (pivot.z - gun.localPosition.z) / scale.z);
        }

        private static void AddBoot(Transform root)
        {
            Vector4[] folds =
            {
                new Vector4(0.10f, 0.56f, 0.30f, 0.02f),
                new Vector4(0.32f, 0.46f, 0.26f, 0.01f),
                new Vector4(0.58f, 0.34f, 0.21f, 0.005f),
                new Vector4(0.84f, 0.23f, 0.17f, 0f)
            };
            for (int index = 0; index < folds.Length; index++)
            {
                Vector4 fold = folds[index];
                Transform part = Cylinder(
                    "T90AVladimir-GunBootFold",
                    root,
                    V(0f, fold.w, fold.x),
                    0.5f,
                    0.5f,
                    0.032f,
                    14,
                    TankShapeAxis.Z,
                    TankT90AFamilyDetails.Dark());
                part.localScale =
                    new Vector3(fold.y, fold.z, 1f);
            }
        }

        private static void AddTube(
            Transform root,
            Color color)
        {
            Tube(root, color, 0.52f, 1.62f, 0.108f);
            Tube(root, color, 1.62f, 3.26f, 0.116f);
            Tube(root, color, 3.26f, 5.02f, 0.102f);
            Tube(root, color, 5.02f, MuzzleZ, 0.102f);
            Vector2[] rings =
            {
                new Vector2(1.10f, 0.114f),
                new Vector2(1.62f, 0.118f),
                new Vector2(2.35f, 0.118f),
                new Vector2(3.26f, 0.106f),
                new Vector2(3.98f, 0.106f),
                new Vector2(4.68f, 0.106f)
            };
            for (int index = 0; index < rings.Length; index++)
            {
                Cylinder(
                    "T90AVladimir-SleeveRing",
                    root,
                    V(0f, 0f, rings[index].x),
                    rings[index].y,
                    rings[index].y,
                    0.045f,
                    18,
                    TankShapeAxis.Z,
                    TankT90AFamilyDetails.Dark());
            }
            Cylinder(
                "Painted-T90AVladimir-FumeExtractor",
                root,
                V(0f, 0f, 2.64f),
                0.128f,
                0.116f,
                0.46f,
                16,
                TankShapeAxis.Z,
                color * 0.44f);
            Cylinder(
                "T90AVladimir-FumeBand",
                root,
                V(0f, 0f, 2.41f),
                0.13f,
                0.13f,
                0.04f,
                16,
                TankShapeAxis.Z,
                TankT90AFamilyDetails.Dark());
            Cylinder(
                "T90AVladimir-FumeBand",
                root,
                V(0f, 0f, 2.87f),
                0.13f,
                0.13f,
                0.04f,
                16,
                TankShapeAxis.Z,
                TankT90AFamilyDetails.Dark());
        }

        private static void Tube(
            Transform root,
            Color color,
            float start,
            float end,
            float radius)
        {
            Cylinder(
                "Painted-T90AVladimir-Tube",
                root,
                V(0f, 0f, (start + end) * 0.5f),
                radius,
                radius,
                end - start,
                24,
                TankShapeAxis.Z,
                color * 0.47f);
        }

        private static void AddMuzzle(
            Transform root,
            Color color)
        {
            Transform collar = Cylinder(
                "Painted-T90AVladimir-MuzzleCollar",
                root,
                V(0f, 0.033f, MuzzleZ - 0.155f),
                0.124f,
                0.124f,
                0.15f,
                16,
                TankShapeAxis.Z,
                color * 0.45f);
            collar.localScale = new Vector3(0.839f, 1f, 1f);
            Transform rim = TankShapeFactory.TorusPart(
                "T90AVladimir-MuzzleBore",
                root,
                0.08364f,
                0.01836f,
                16,
                TankT90AFamilyDetails.Dark());
            rim.localPosition = V(0f, 0f, MuzzleZ + 0.016f);
            rim.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Cylinder(
                "T90AVladimir-MuzzleBoreDisc",
                root,
                V(0f, 0f, MuzzleZ + 0.006f),
                0.062f,
                0.062f,
                0.012f,
                16,
                TankShapeAxis.Z,
                Color.black);
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

        private static Vector3 V(
            float x = 0f,
            float y = 0f,
            float z = 0f)
        {
            return new Vector3(x, y, z);
        }
    }
}
