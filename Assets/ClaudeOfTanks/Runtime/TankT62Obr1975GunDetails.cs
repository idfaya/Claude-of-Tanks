using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT62Obr1975GunDetails
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
                    "T62-GunFittings");
            AddMantlet(fittings, color);
            AddKtd2(fittings, color);
            AddLuna(fittings, color);
            AddU5Ts(
                fittings,
                definition,
                color);
        }

        private static void AddMantlet(
            Transform fittings,
            Color color)
        {
            AddZCylinder(
                "Painted-T62-CastMantlet",
                fittings,
                new Vector3(0f, -0.06f, 0.13f),
                0.2f,
                0.36f,
                color * 0.57f,
                new Vector3(1.43f, 0.83f, 1f));
            AddZCylinder(
                "T62-MantletBootRing",
                fittings,
                new Vector3(0f, -0.058f, 0.2f),
                0.19f,
                0.04f,
                TankT62Obr1975FamilyDetails.Rubber(),
                new Vector3(1.46f, 0.84f, 1f));
            AddZCylinder(
                "T62-MantletClamp",
                fittings,
                new Vector3(0f, -0.02f, 0.325f),
                0.15f,
                0.04f,
                TankT62Obr1975FamilyDetails.Dark());
            AddZCylinder(
                "T62-PktCoax",
                fittings,
                new Vector3(0.198f, 0.02f, 0.305f),
                0.02f,
                0.1f,
                TankT62Obr1975FamilyDetails.Dark());
        }

        private static void AddKtd2(
            Transform fittings,
            Color color)
        {
            Part(
                "Painted-T62-Ktd2Support",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0.32f, -0.072f),
                new Vector3(0.16f, 0.3f, 0.2f),
                color * 0.58f);
            AddZCylinder(
                "Painted-T62-Ktd2Pod",
                fittings,
                new Vector3(0f, 0.5f, -0.072f),
                0.14f,
                0.26f,
                color * 0.62f,
                new Vector3(1.18f, 1f, 1f));
            Part(
                "T62-Ktd2Lens",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0.5f, 0.066f),
                new Vector3(0.22f, 0.16f, 0.02f),
                TankT62Obr1975FamilyDetails.Glass());
        }

        private static void AddLuna(
            Transform fittings,
            Color color)
        {
            AddZCylinder(
                "Painted-T62-LunaSearchlight",
                fittings,
                new Vector3(-0.66f, 0.42f, -0.05f),
                0.26f,
                0.27f,
                color * 0.57f);
            AddZCylinder(
                "T62-LunaLens",
                fittings,
                new Vector3(-0.66f, 0.42f, 0.094f),
                0.235f,
                0.018f,
                TankT62Obr1975FamilyDetails.Glass());
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Part(
                    "Painted-T62-LunaYoke",
                    PrimitiveType.Cube,
                    fittings,
                    new Vector3(
                        -0.66f +
                            side * 0.322f,
                        0.35f,
                        -0.05f),
                    new Vector3(
                        0.045f,
                        0.36f,
                        0.3f),
                    color * 0.51f);
            }
            Part(
                "Painted-T62-LunaMount",
                PrimitiveType.Cube,
                fittings,
                new Vector3(
                    -0.66f,
                    0.24f,
                    -0.17f),
                new Vector3(
                    0.726f,
                    0.16f,
                    0.12f),
                color * 0.5f);
        }

        private static void AddU5Ts(
            Transform fittings,
            VehicleDefinition definition,
            Color color)
        {
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    4.8626f);
            AddZCylinder(
                "Painted-T62-U5TsRoot",
                fittings,
                new Vector3(0f, 0f, 1.725f),
                0.085f,
                2.65f,
                color * 0.5f);
            AddZCylinder(
                "Painted-T62-U5TsEvacuator",
                fittings,
                new Vector3(0f, 0f, 3.5f),
                0.135f,
                0.9f,
                color * 0.47f);
            AddZCylinder(
                "Painted-T62-U5TsMuzzleTube",
                fittings,
                new Vector3(
                    0f,
                    0f,
                    3.95f +
                        (length - 3.95f) *
                        0.5f),
                0.08f,
                length - 3.95f,
                color * 0.45f);
            AddZCylinder(
                "Painted-T62-U5TsMuzzleRing",
                fittings,
                new Vector3(
                    0f,
                    0f,
                    length - 0.07f),
                0.09f,
                0.12f,
                color * 0.42f);
            AddZCylinder(
                "T62-U5TsMuzzleBore",
                fittings,
                new Vector3(
                    0f,
                    0f,
                    length + 0.004f),
                0.052f,
                0.018f,
                Color.black);
            for (int cinch = 0;
                cinch < 4;
                cinch++)
            {
                AddZCylinder(
                    "T62-U5TsCinch",
                    fittings,
                    new Vector3(
                        0f,
                        0f,
                        0.72f +
                            cinch * 0.68f),
                    0.094f,
                    0.035f,
                    TankT62Obr1975FamilyDetails.Dark());
            }
        }

        private static void AddZCylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float length,
            Color color)
        {
            AddZCylinder(
                name,
                parent,
                position,
                radius,
                length,
                color,
                Vector3.one);
        }

        private static void AddZCylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float length,
            Color color,
            Vector3 scale)
        {
            Transform part = Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                position,
                new Vector3(
                    radius * scale.x,
                    length * 0.5f * scale.z,
                    radius * scale.y),
                color);
            part.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            return TankDetailGeometry.Part(
                name,
                type,
                parent,
                position,
                scale,
                color);
        }
    }
}
