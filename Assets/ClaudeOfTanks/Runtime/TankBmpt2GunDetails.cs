using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBmpt2GunDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "Bmpt2-GunFittings");
            Transform plant =
                new GameObject("Bmpt2-TwinGunPlant").transform;
            plant.SetParent(fittings, false);
            plant.localPosition =
                new Vector3(0f, 0f, -0.51f);

            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bmpt2-GunCradle",
                    PrimitiveType.Cube,
                    plant,
                    new Vector3(side * 0.16f, 0f, 0.28f),
                    new Vector3(0.18f, 0.25f, 0.3f),
                    color * 0.59f);
                AddAxialCylinder(
                    "Painted-Bmpt2-RootCollar",
                    plant,
                    0.06f,
                    0.32f,
                    0.55f,
                    color * 0.5f,
                    side * 0.16f);
                AddAxialCylinder(
                    "Painted-Bmpt2-2A42Barrel",
                    plant,
                    0.038f,
                    2.45f,
                    1.82f,
                    color * 0.41f,
                    side * 0.16f);
                AddAxialCylinder(
                    "Painted-Bmpt2-MuzzleSleeve",
                    plant,
                    0.056f,
                    0.18f,
                    3.1f,
                    color * 0.37f,
                    side * 0.16f);
                AddAxialCylinder(
                    "Bmpt2-MuzzleBore",
                    plant,
                    0.021f,
                    0.025f,
                    3.205f,
                    Color.black,
                    side * 0.16f);
            }
        }

        private static void AddAxialCylinder(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color,
            float x)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(x, 0f, z),
                    new Vector3(
                        radius,
                        length * 0.5f,
                        radius),
                    color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
