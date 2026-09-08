using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSheridanProtectionDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;

            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "Sheridan-GunFittings");
            float radius = Mathf.Max(
                0.1f,
                definition.armor?.gunBarrel?.radiusM ?? 0.1f);
            AddMantlet(
                fittings,
                color,
                width,
                radius);
            AddLauncherPlant(
                fittings,
                color,
                radius,
                definition.armor?.gunBarrel?.lengthM ?? 2.09f);
            AddCoaxAndSight(
                fittings,
                color);
        }

        private static void AddMantlet(
            Transform fittings,
            Color color,
            float width,
            float radius)
        {
            TankDetailGeometry.Part(
                "Painted-Sheridan-M81-Mantlet",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0.02f, 0.34f),
                new Vector3(width * 0.34f, 0.5f, 0.38f),
                color * 0.68f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform wing =
                    TankDetailGeometry.Part(
                        "Painted-Sheridan-M81-MantletWing",
                        PrimitiveType.Cube,
                        fittings,
                        new Vector3(
                            side * width * 0.19f,
                            0.01f,
                            0.25f),
                        new Vector3(
                            width * 0.14f,
                            0.38f,
                            0.32f),
                        color * 0.64f);
                wing.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * -14f,
                        side * 5f);
            }
            Transform rotor =
                TankDetailGeometry.Part(
                    "Painted-Sheridan-M81-Rotor",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0.02f, 0.52f),
                    new Vector3(
                        radius * 1.75f,
                        0.22f,
                        radius * 1.75f),
                    color * 0.6f);
            rotor.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddLauncherPlant(
            Transform fittings,
            Color color,
            float radius,
            float length)
        {
            float[] centers =
            {
                0.62f,
                0.96f,
                1.38f
            };
            float[] lengths =
            {
                0.26f,
                0.42f,
                0.5f
            };
            float[] scales =
            {
                1.34f,
                1.58f,
                1.22f
            };
            for (int section = 0;
                section < centers.Length;
                section++)
            {
                Transform sleeve =
                    TankDetailGeometry.Part(
                        "Painted-Sheridan-M81-LauncherSection",
                        PrimitiveType.Cylinder,
                        fittings,
                        new Vector3(0f, 0f, centers[section]),
                        new Vector3(
                            radius * scales[section],
                            lengths[section],
                            radius * scales[section]),
                        color * (0.68f - section * 0.04f));
                sleeve.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            Transform muzzle =
                TankDetailGeometry.Part(
                    "Painted-Sheridan-M81-MuzzleRing",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(
                        0f,
                        0f,
                        Mathf.Max(1.72f, length - 0.04f)),
                    new Vector3(
                        radius * 1.42f,
                        0.08f,
                        radius * 1.42f),
                    color * 0.55f);
            muzzle.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                "Sheridan-M81-MuzzleBore",
                PrimitiveType.Cylinder,
                fittings,
                new Vector3(
                    0f,
                    0f,
                    Mathf.Max(1.77f, length + 0.005f)),
                new Vector3(
                    radius * 0.72f,
                    0.018f,
                    radius * 0.72f),
                new Color(0.025f, 0.026f, 0.023f))
                .localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddCoaxAndSight(
            Transform fittings,
            Color color)
        {
            TankDetailGeometry.Part(
                "Sheridan-M81-CoaxHousing",
                PrimitiveType.Cube,
                fittings,
                new Vector3(-0.34f, 0.08f, 0.5f),
                new Vector3(0.15f, 0.13f, 0.32f),
                TankSheridanFamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "Sheridan-M81-CoaxBarrel",
                PrimitiveType.Cube,
                fittings,
                new Vector3(-0.34f, 0.08f, 0.92f),
                new Vector3(0.026f, 0.026f, 0.58f),
                TankSheridanFamilyDetails.Gunmetal());
            TankSheridanFamilyDetails.AddSight(
                fittings,
                color,
                new Vector3(0.31f, 0.18f, 0.48f),
                new Vector3(0.2f, 0.16f, 0.28f),
                "Sheridan-M81-GunnerSight");
            TankDetailGeometry.Part(
                "Sheridan-M81-RecoilCage",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, -0.12f, 0.65f),
                new Vector3(0.72f, 0.05f, 0.6f),
                TankSheridanFamilyDetails.Gunmetal());
        }
    }
}
