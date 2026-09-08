using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBradleyGunDetails
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
                    "Bradley-GunFittings");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    2.9f);
            bool m3 =
                TankBradleyFamilyDetails
                    .IsM3A3(definition);

            AddM242(
                fittings,
                color,
                length,
                m3);
            AddTowPod(
                fittings,
                color,
                m3);
        }

        private static void AddM242(
            Transform fittings,
            Color color,
            float length,
            bool m3)
        {
            TankDetailGeometry.Part(
                "Painted-Bradley-M242-Cradle",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.28f),
                new Vector3(
                    m3 ? 0.56f : 0.4f,
                    m3 ? 0.36f : 0.3f,
                    0.34f),
                color * 0.65f);
            AddAxialCylinder(
                "Painted-Bradley-M242-Collar",
                fittings,
                m3 ? 0.118f : 0.1f,
                0.38f,
                0.59f,
                color * 0.55f);
            float barrelStart = 0.68f;
            float barrelLength =
                Mathf.Max(
                    0.6f,
                    length -
                    barrelStart -
                    0.15f);
            AddAxialCylinder(
                "Painted-Bradley-M242-Barrel",
                fittings,
                0.037f,
                barrelLength,
                barrelStart +
                    barrelLength * 0.5f,
                color * 0.48f);
            AddAxialCylinder(
                "Painted-Bradley-M242-MuzzleJacket",
                fittings,
                0.058f,
                0.18f,
                length - 0.12f,
                color * 0.42f);
            AddAxialCylinder(
                "Bradley-M242-MuzzleBore",
                fittings,
                0.022f,
                0.026f,
                length - 0.015f,
                Color.black);
            AddAxialCylinder(
                "Bradley-M240-CoaxBarrel",
                fittings,
                0.016f,
                0.6f,
                0.72f,
                TankBradleyFamilyDetails.Dark(),
                0.19f,
                0.055f);
            TankDetailGeometry.Part(
                "Bradley-M240-CoaxReceiver",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0.19f, 0.055f, 0.3f),
                new Vector3(0.11f, 0.1f, 0.32f),
                TankBradleyFamilyDetails.Gunmetal());
        }

        private static void AddTowPod(
            Transform fittings,
            Color color,
            bool m3)
        {
            float x = m3 ? -0.91f : -0.97f;
            float y = m3 ? 0.38f : -0.08f;
            float z = m3 ? -0.08f : -0.35f;
            TankDetailGeometry.Part(
                m3
                    ? "Painted-BradleyM3-TowRoot"
                    : "Painted-Bradley-A2TowRoot",
                PrimitiveType.Cube,
                fittings,
                new Vector3(
                    x + 0.28f,
                    y - 0.03f,
                    z),
                new Vector3(0.32f, 0.3f, 0.38f),
                color * 0.56f);
            TankDetailGeometry.Part(
                m3
                    ? "Painted-BradleyM3-TowPod"
                    : "Painted-Bradley-A2TowPod",
                PrimitiveType.Cube,
                fittings,
                new Vector3(x, y, z),
                new Vector3(
                    m3 ? 0.4f : 0.46f,
                    m3 ? 0.44f : 0.48f,
                    m3 ? 1.08f : 1.27f),
                color * 0.63f);
            TankDetailGeometry.Part(
                "Bradley-TowRearDoor",
                PrimitiveType.Cube,
                fittings,
                new Vector3(
                    x,
                    y,
                    z -
                    (m3 ? 0.56f : 0.655f)),
                new Vector3(
                    m3 ? 0.36f : 0.42f,
                    m3 ? 0.38f : 0.42f,
                    0.04f),
                TankBradleyFamilyDetails.Dark());
            for (int tube = 0;
                tube < 2;
                tube++)
            {
                float tubeY =
                    y +
                    (tube == 0
                        ? 0.105f
                        : -0.105f);
                AddAxialCylinder(
                    "Bradley-TowTubeMouth",
                    fittings,
                    m3 ? 0.088f : 0.115f,
                    0.04f,
                    z +
                        (m3 ? 0.56f : 0.655f),
                    TankBradleyFamilyDetails.Dark(),
                    x,
                    tubeY);
                AddAxialCylinder(
                    "Painted-Bradley-TowTubeRim",
                    fittings,
                    m3 ? 0.097f : 0.125f,
                    0.022f,
                    z +
                        (m3 ? 0.585f : 0.682f),
                    color * 0.5f,
                    x,
                    tubeY);
            }
        }

        private static void AddAxialCylinder(
            string name,
            Transform parent,
            float radius,
            float length,
            float z,
            Color color,
            float x = 0f,
            float y = 0f)
        {
            Transform part =
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cylinder,
                    parent,
                    new Vector3(x, y, z),
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
