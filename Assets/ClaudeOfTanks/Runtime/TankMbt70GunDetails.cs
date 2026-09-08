using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMbt70GunDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;

            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "MBT70-GunFittings");
            AddParabolicShield(
                fittings,
                color);
            AddXm150Launcher(
                fittings,
                definition,
                color);
        }

        private static void AddParabolicShield(
            Transform fittings,
            Color color)
        {
            float[] widths =
            {
                0.46f,
                0.6f,
                0.66f,
                0.6f,
                0.46f
            };
            float[] depths =
            {
                0.72f,
                0.96f,
                1.12f,
                0.96f,
                0.72f
            };
            for (int ring = 0;
                ring < widths.Length;
                ring++)
            {
                float y =
                    -0.32f + ring * 0.16f;
                Transform course =
                    TankDetailGeometry.Part(
                        "Painted-MBT70-ParabolicShieldCourse",
                        PrimitiveType.Cube,
                        fittings,
                        new Vector3(
                            0f,
                            y,
                            0.15f +
                                depths[ring] * 0.5f),
                        new Vector3(
                            widths[ring],
                            0.17f,
                            depths[ring]),
                        color * (0.62f +
                            ring * 0.025f));
                course.localRotation =
                    Quaternion.Euler(
                        ring < 2 ? -4f :
                        ring > 2 ? 4f : 0f,
                        0f,
                        0f);
            }
            TankDetailGeometry.Part(
                "Painted-MBT70-ShieldBrow",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0.18f, 0.64f),
                new Vector3(0.5f, 0.14f, 0.44f),
                color * 0.7f)
                .localRotation =
                Quaternion.Euler(-10f, 0f, 0f);
            AddZCylinder(
                "MBT70-GunRootRecess",
                fittings,
                new Vector3(0f, 0f, 0.94f),
                0.18f,
                0.045f,
                TankMbt70FamilyDetails.Dark());
            AddZCylinder(
                "Painted-MBT70-LauncherThroat",
                fittings,
                new Vector3(0f, 0f, 1.19f),
                0.22f,
                0.25f,
                color * 0.58f);
            AddZCylinder(
                "MBT70-LauncherThroatCollar",
                fittings,
                new Vector3(0f, 0f, 1.085f),
                0.225f,
                0.035f,
                TankMbt70FamilyDetails.Gunmetal());
        }

        private static void AddXm150Launcher(
            Transform fittings,
            VehicleDefinition definition,
            Color color)
        {
            float length =
                definition.armor.gunBarrel.lengthM;
            AddZCylinder(
                "Painted-MBT70-XM150-ThermalSleeve",
                fittings,
                new Vector3(0f, 0f, 2.35f),
                0.112f,
                2.5f,
                color * 0.6f);
            AddZCylinder(
                "Painted-MBT70-XM150-FumeExtractor",
                fittings,
                new Vector3(0f, 0f, 2.35f),
                0.17f,
                0.5f,
                color * 0.55f);
            AddZCylinder(
                "MBT70-XM150-BaseCollar",
                fittings,
                new Vector3(0f, 0f, 1.35f),
                0.19f,
                0.12f,
                TankMbt70FamilyDetails.Gunmetal());
            AddZCylinder(
                "MBT70-XM150-SensorClamp",
                fittings,
                new Vector3(0f, 0f, 3.38f),
                0.128f,
                0.075f,
                TankMbt70FamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                "Painted-MBT70-XM150-SensorHousing",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.14f, 0.14f, 3.38f),
                new Vector3(0.18f, 0.14f, 0.28f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "MBT70-XM150-SensorAperture",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.14f, 0.16f, 3.525f),
                new Vector3(0.1f, 0.065f, 0.025f),
                TankMbt70FamilyDetails.Lens());
            AddZCylinder(
                "Painted-MBT70-XM150-MuzzleRing",
                fittings,
                new Vector3(0f, 0f, length - 0.06f),
                0.112f,
                0.12f,
                color * 0.58f);
            AddZCylinder(
                "MBT70-XM150-MuzzleBore",
                fittings,
                new Vector3(0f, 0f, length + 0.01f),
                0.064f,
                0.012f,
                new Color(0.012f, 0.012f, 0.01f));
        }

        private static void AddZCylinder(
            string name,
            Transform parent,
            Vector3 center,
            float radius,
            float length,
            Color color)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    center,
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
