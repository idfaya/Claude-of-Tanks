using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKf51GunDetails
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
                    "KF51-GunFittings");
            if (definition.id == "kf51")
            {
                AddDemonstratorHousing(
                    fittings,
                    color);
            }
            else
            {
                AddOwnerExactHousing(
                    fittings,
                    color);
            }
        }

        private static void AddDemonstratorHousing(
            Transform fittings,
            Color color)
        {
            AddHousingCourse(
                fittings,
                color,
                "Painted-KF51-Rh130-Housing",
                Vector3.zero,
                new Vector3(0.78f, 0.59f, 1.68f));
            AddHousingCourse(
                fittings,
                color * 0.82f,
                "Painted-KF51-Rh130-CrownFacet",
                new Vector3(0f, 0.28f, 0.18f),
                new Vector3(0.62f, 0.12f, 1.46f));
            AddHousingCourse(
                fittings,
                TankKf51FamilyDetails.Dark(),
                "KF51-Rh130-ChinFacet",
                new Vector3(0f, -0.28f, 0.18f),
                new Vector3(0.62f, 0.1f, 1.46f));
            AddClamp(
                fittings,
                "KF51-Rh130-HexClamp",
                0.205f,
                1.8f);
            AddCoax(
                fittings,
                "KF51-Rh130-Coax",
                0.275f,
                0.055f,
                1.17f);
            AddThermalCourse(
                fittings,
                color,
                "Painted-KF51-Rh130-ThermalShroud",
                0.23f,
                3.2f,
                4.95f);
            AddCinch(
                fittings,
                "KF51-Rh130-Cinch",
                0.236f,
                3.62f);
            AddCinch(
                fittings,
                "KF51-Rh130-Cinch",
                0.236f,
                4.42f);
            AddMuzzle(
                fittings,
                color,
                "KF51-Rh130",
                0.19f,
                5.25f);
        }

        private static void AddOwnerExactHousing(
            Transform fittings,
            Color color)
        {
            AddHousingCourse(
                fittings,
                color,
                "Painted-KF51B-Rh130-Housing",
                Vector3.zero,
                new Vector3(0.74f, 0.43f, 1.25f));
            AddHousingCourse(
                fittings,
                color * 0.82f,
                "Painted-KF51B-Rh130-CrownFacet",
                new Vector3(0f, 0.21f, 0.12f),
                new Vector3(0.61f, 0.11f, 1.08f));
            AddHousingCourse(
                fittings,
                TankKf51FamilyDetails.Dark(),
                "KF51B-Rh130-ChinFacet",
                new Vector3(0f, -0.2f, 0.12f),
                new Vector3(0.61f, 0.09f, 1.08f));
            AddClamp(
                fittings,
                "KF51B-Rh130-HexClamp",
                0.175f,
                1.31f);
            AddCoax(
                fittings,
                "KF51B-Rh130-Coax",
                0.25f,
                0.05f,
                0.88f);
            AddCoax(
                fittings,
                "KF51B-Rh130-Boresight",
                -0.25f,
                0.076f,
                0.72f);
            AddThermalCourse(
                fittings,
                color,
                "Painted-KF51B-Rh130-ThermalShroud",
                0.216f,
                2.02f,
                1.34f);
            AddThermalCourse(
                fittings,
                color * 0.9f,
                "Painted-KF51B-Rh130-ThermalShroud",
                0.194f,
                3.55f,
                1.72f);
            AddCinch(
                fittings,
                "KF51B-Rh130-Cinch",
                0.224f,
                1.37f);
            AddCinch(
                fittings,
                "KF51B-Rh130-Cinch",
                0.216f,
                2.7f);
            AddCinch(
                fittings,
                "KF51B-Rh130-Cinch",
                0.196f,
                4.42f);
            TankDetailGeometry.Part(
                "Painted-KF51B-Rh130-MRS",
                PrimitiveType.Cube,
                fittings,
                new Vector3(-0.105f, 0.105f, 4.6f),
                new Vector3(0.15f, 0.1f, 0.31f),
                color * 0.68f)
                .localRotation =
                Quaternion.Euler(0f, -5f, 0f);
            AddMuzzle(
                fittings,
                color,
                "KF51B-Rh130",
                0.164f,
                5.1f);
        }

        private static void AddHousingCourse(
            Transform fittings,
            Color color,
            string name,
            Vector3 center,
            Vector3 size)
        {
            TankDetailGeometry.Part(
                name,
                PrimitiveType.Cube,
                fittings,
                center + new Vector3(0f, 0f, size.z * 0.5f),
                size,
                color);
        }

        private static void AddClamp(
            Transform fittings,
            string name,
            float radius,
            float z)
        {
            Transform clamp =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, z),
                    new Vector3(
                        radius,
                        0.1f,
                        radius),
                    TankKf51FamilyDetails.Gunmetal());
            clamp.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddCoax(
            Transform fittings,
            string name,
            float x,
            float y,
            float z)
        {
            Transform coax =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(x, y, z),
                    new Vector3(0.03f, 0.1f, 0.03f),
                    TankKf51FamilyDetails.Gunmetal());
            coax.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddThermalCourse(
            Transform fittings,
            Color color,
            string name,
            float diameter,
            float z,
            float length)
        {
            Transform shroud =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, z),
                    new Vector3(
                        diameter * 0.5f,
                        length * 0.5f,
                        diameter * 0.5f),
                    color * 0.72f);
            shroud.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddCinch(
            Transform fittings,
            string name,
            float diameter,
            float z)
        {
            Transform ring =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0.008f, z),
                    new Vector3(
                        diameter * 0.5f,
                        0.026f,
                        diameter * 0.5f),
                    TankKf51FamilyDetails.Gunmetal());
            ring.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddMuzzle(
            Transform fittings,
            Color color,
            string prefix,
            float diameter,
            float z)
        {
            Transform transition =
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-MuzzleTransition",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, z),
                    new Vector3(
                        diameter * 0.5f,
                        0.18f,
                        diameter * 0.5f),
                    color * 0.7f);
            transition.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            Transform bore =
                TankDetailGeometry.Part(
                    prefix + "-MuzzleBore",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, z + 0.19f),
                    new Vector3(
                        diameter * 0.32f,
                        0.015f,
                        diameter * 0.32f),
                    new Color(0.015f, 0.015f, 0.012f));
            bore.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
