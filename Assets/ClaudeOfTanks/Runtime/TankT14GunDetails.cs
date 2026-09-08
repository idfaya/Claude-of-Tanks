using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT14GunDetails
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
                    "T14-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    8.7f);

            TankDetailGeometry.Part(
                "Painted-T14-2A82-ShroudChin",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0.02f, 0.16f),
                new Vector3(0.44f, 0.44f, 0.32f),
                color * 0.68f);
            AddAxialCylinder(
                "Painted-T14-2A82-BootCollar",
                fittings,
                0.15f,
                0.34f,
                0.4f,
                color * 0.6f);

            float sleeveStart = 0.62f;
            float sleeveEnd =
                Mathf.Max(
                    sleeveStart + 0.6f,
                    length - 0.48f);
            const int courses = 3;
            float gap = 0.07f;
            float courseLength =
                (sleeveEnd -
                 sleeveStart -
                 gap * (courses - 1)) /
                courses;
            for (int course = 0;
                course < courses;
                course++)
            {
                float start =
                    sleeveStart +
                    course *
                    (courseLength + gap);
                AddAxialCylinder(
                    "Painted-T14-2A82-ThermalSleeve",
                    fittings,
                    0.095f -
                        course * 0.004f,
                    courseLength,
                    start + courseLength * 0.5f,
                    color * (0.72f -
                        course * 0.035f));
            }
            float[] clamps =
            {
                sleeveStart,
                sleeveStart +
                    courseLength +
                    gap * 0.5f,
                sleeveStart +
                    courseLength * 2f +
                    gap * 1.5f,
                sleeveEnd
            };
            for (int clamp = 0;
                clamp < clamps.Length;
                clamp++)
            {
                AddAxialCylinder(
                    "T14-2A82-ThermalClamp",
                    fittings,
                    0.105f,
                    0.055f,
                    clamps[clamp],
                    TankT14FamilyDetails.Gunmetal());
            }
            AddAxialCylinder(
                "Painted-T14-2A82-MuzzleCollar",
                fittings,
                0.103f,
                0.22f,
                length - 0.16f,
                color * 0.58f);
            AddAxialCylinder(
                "T14-2A82-MuzzleBore",
                fittings,
                0.055f,
                0.026f,
                length - 0.035f,
                Color.black);
        }

        private static void AddAxialCylinder(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(0f, 0f, z),
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
