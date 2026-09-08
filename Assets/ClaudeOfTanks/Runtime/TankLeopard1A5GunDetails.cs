using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLeopard1A5GunDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;

            AddTurretReceiver(
                turret,
                color);
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "Leopard1A5-GunFittings");
            AddButterflyMantlet(
                fittings,
                color);
            AddL7A3(
                fittings,
                definition,
                color);
        }

        private static void AddTurretReceiver(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-MantletReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.47f, 0.79f),
                new Vector3(1.16f, 0.5f, 0.18f),
                color * 0.61f);
            TankDetailGeometry.Part(
                "Leopard1A5-ReceiverOpening",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.47f, 0.889f),
                new Vector3(1.1f, 0.44f, 0.018f),
                TankLeopard1A5FamilyDetails.Rubber());
        }

        private static void AddButterflyMantlet(
            Transform fittings,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-MantletRearPad",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, -0.34f),
                new Vector3(1.22f, 0.46f, 0.16f),
                color * 0.64f);
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-MantletCore",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.04f),
                new Vector3(1f, 0.32f, 0.68f),
                color * 0.7f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform wing =
                    TankDetailGeometry.Part(
                        "Painted-Leopard1A5-MantletWing",
                        PrimitiveType.Cube,
                        fittings,
                        new Vector3(
                            side * 0.49f,
                            0f,
                            -0.02f),
                        new Vector3(0.31f, 0.46f, 0.61f),
                        color * 0.66f);
                wing.localRotation =
                    Quaternion.Euler(
                        0f,
                        side * -8f,
                        side * 4f);
            }
            TankDetailGeometry.Part(
                "Leopard1A5-MantletShoulder",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, -0.13f),
                new Vector3(1.08f, 0.12f, 0.1f),
                TankLeopard1A5FamilyDetails.Gunmetal());
            AddZCylinder(
                "Painted-Leopard1A5-MantletBoss",
                fittings,
                new Vector3(0f, 0f, 0.38f),
                0.18f,
                0.3f,
                color * 0.62f);
            AddZCylinder(
                "Leopard1A5-Coax",
                fittings,
                new Vector3(0.37f, 0.06f, 0.37f),
                0.026f,
                0.1f,
                TankLeopard1A5FamilyDetails.Gunmetal());
            AddZCylinder(
                "Leopard1A5-Boresight",
                fittings,
                new Vector3(-0.37f, 0.08f, 0.37f),
                0.022f,
                0.095f,
                TankLeopard1A5FamilyDetails.Gunmetal());
        }

        private static void AddL7A3(
            Transform fittings,
            VehicleDefinition definition,
            Color color)
        {
            float length =
                definition.armor.gunBarrel.lengthM;
            AddZCylinder(
                "Painted-Leopard1A5-L7A3-ThermalSleeve",
                fittings,
                new Vector3(0f, 0f, 2.42f),
                0.078f,
                3.68f,
                color * 0.6f);
            AddZCylinder(
                "Painted-Leopard1A5-L7A3-FumeExtractor",
                fittings,
                new Vector3(0f, 0f, 2.65f),
                0.12f,
                0.6f,
                color * 0.56f);
            AddZCylinder(
                "Leopard1A5-L7A3-BaseCollar",
                fittings,
                new Vector3(0f, 0f, 0.78f),
                0.14f,
                0.18f,
                TankLeopard1A5FamilyDetails.Gunmetal());
            for (int cinch = 0;
                cinch < 3;
                cinch++)
            {
                AddZCylinder(
                    "Leopard1A5-L7A3-Cinch",
                    fittings,
                    new Vector3(
                        0f,
                        0f,
                        1.25f + cinch * 1.35f),
                    0.086f,
                    0.035f,
                    TankLeopard1A5FamilyDetails.Gunmetal());
            }
            TankDetailGeometry.Part(
                "Painted-Leopard1A5-L7A3-MRS",
                PrimitiveType.Cube,
                fittings,
                new Vector3(-0.11f, 0.11f, 4.15f),
                new Vector3(0.16f, 0.11f, 0.34f),
                color * 0.65f);
            AddZCylinder(
                "Painted-Leopard1A5-L7A3-MuzzleRing",
                fittings,
                new Vector3(0f, 0f, length - 0.08f),
                0.085f,
                0.16f,
                color * 0.58f);
            AddZCylinder(
                "Leopard1A5-L7A3-MuzzleBore",
                fittings,
                new Vector3(0f, 0f, length + 0.015f),
                0.043f,
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
