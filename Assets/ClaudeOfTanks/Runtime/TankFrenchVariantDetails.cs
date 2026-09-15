using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankFrenchVariantDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddPrimaryTurretAndGun(
                turret,
                definition,
                color,
                width,
                roof);
            if (definition.id == "amx30" ||
                definition.id == "amx30b2")
            {
                AddAmx30Roof(
                    turret,
                    definition.id,
                    color,
                    width,
                    roof);
                return;
            }
            if (definition.id == "amx40")
            {
                AddAmx40Roof(
                    turret,
                    color,
                    width,
                    roof);
                return;
            }
            AddLeclercRoof(
                turret,
                definition.id,
                color,
                width,
                roof);
        }

        private static void AddPrimaryTurretAndGun(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float roof)
        {
            string id = definition.id;
            bool amx30 = id == "amx30" || id == "amx30b2";
            bool amx40 = id == "amx40";
            string prefix = amx30
                ? "French-AMX30"
                : amx40
                    ? "French-AMX40"
                    : "French-Leclerc";
            if (amx30)
            {
                TankShapeFactory.LathePart(
                    "Painted-" + prefix + "-CastTurretShell",
                    turret,
                    new[]
                    {
                        width * 0.28f,
                        width * 0.35f,
                        width * 0.38f,
                        width * 0.34f,
                        width * 0.22f,
                        width * 0.08f
                    },
                    new[]
                    {
                        -0.14f,
                        0.04f,
                        roof * 0.38f,
                        roof * 0.68f,
                        roof * 0.88f,
                        roof
                    },
                    32,
                    1.08f,
                    color * 0.62f,
                    width * 0.39f,
                    0.82f);
            }
            else
            {
                float half = width * (amx40 ? 0.38f : 0.42f);
                float front = amx40 ? 1.52f : 1.82f;
                float rear = amx40 ? -1.88f : -2.30f;
                Vector2[] plan =
                {
                    new Vector2(-0.30f, front),
                    new Vector2(0.30f, front),
                    new Vector2(half * 0.68f, front * 0.70f),
                    new Vector2(half, 0.34f),
                    new Vector2(half * 0.96f, rear * 0.62f),
                    new Vector2(half * 0.72f, rear),
                    new Vector2(-half * 0.72f, rear),
                    new Vector2(-half * 0.96f, rear * 0.62f),
                    new Vector2(-half, 0.34f),
                    new Vector2(-half * 0.68f, front * 0.70f)
                };
                TankShapeFactory.PolyMultiLoftPart(
                    "Painted-" + prefix + "-TurretShell",
                    turret,
                    plan,
                    new[]
                    {
                        new TankShapeLoftRing(
                            -0.08f,
                            1.00f,
                            Vector2.zero),
                        new TankShapeLoftRing(
                            roof * 0.42f,
                            amx40 ? 0.94f : 0.91f,
                            Vector2.zero),
                        new TankShapeLoftRing(
                            roof,
                            amx40 ? 0.68f : 0.60f,
                            new Vector2(0f, -0.10f))
                    },
                    color * 0.62f);
            }

            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    prefix + "-MainGunAssembly");
            float length =
                TankAuthoredDetails.ResolveGunLength(
                    definition,
                    amx30 ? 5.2f : 6.0f);
            float radius =
                TankAuthoredDetails.ResolveGunRadius(definition);
            float start = amx30 ? 0.84f : 1.02f;
            Transform sleeve = TankShapeFactory.CylinderPart(
                "Painted-" + prefix + "-GunRootSleeve",
                fittings,
                radius * 1.55f,
                radius * 1.40f,
                start,
                18,
                TankShapeAxis.Z,
                color * 0.48f);
            sleeve.localPosition =
                new Vector3(0f, 0f, start * 0.5f);
            Transform tube = TankShapeFactory.CylinderPart(
                "Painted-" + prefix + "-MainGunTube",
                fittings,
                radius,
                radius * 0.94f,
                Mathf.Max(0.2f, length - start),
                24,
                TankShapeAxis.Z,
                color * 0.46f);
            tube.localPosition =
                new Vector3(0f, 0f, (start + length) * 0.5f);
            Transform bore = TankShapeFactory.CylinderPart(
                prefix + "-MuzzleBore",
                fittings,
                radius * 0.58f,
                radius * 0.58f,
                0.045f,
                16,
                TankShapeAxis.Z,
                new Color(0.025f, 0.028f, 0.024f));
            bore.localPosition =
                new Vector3(0f, 0f, length - 0.01f);
        }

        private static void AddAmx30Roof(
            Transform turret,
            string id,
            Color color,
            float width,
            float roof)
        {
            const float cupolaX = 0.45f;
            const float cupolaZ = -0.38f;
            TankDetailGeometry.Part(
                "Painted-French-AMX30-CupolaCollar",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    cupolaX,
                    roof + 0.1f,
                    cupolaZ),
                new Vector3(0.37f, 0.1f, 0.37f),
                color * 0.76f);
            TankDetailGeometry.Part(
                "Painted-French-AMX30-CupolaDrum",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    cupolaX,
                    roof + 0.23f,
                    cupolaZ),
                new Vector3(0.31f, 0.13f, 0.31f),
                color * 0.8f);
            for (int block = 0;
                block < 10;
                block++)
            {
                float angle =
                    (block + 0.5f) *
                    Mathf.PI * 0.2f;
                TankDetailGeometry.Part(
                    "French-AMX30-CupolaVision",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        cupolaX +
                            Mathf.Cos(angle) * 0.32f,
                        roof + 0.25f,
                        cupolaZ +
                            Mathf.Sin(angle) * 0.32f),
                    new Vector3(0.09f, 0.055f, 0.035f),
                    Lens());
            }
            TankFrenchFamilyDetails.AddSight(
                turret,
                color,
                id == "amx30b2"
                    ? new Vector3(
                        width * 0.15f,
                        roof + 0.16f,
                        0.3f)
                    : new Vector3(
                        width * 0.145f,
                        roof + 0.1f,
                        0.34f),
                id == "amx30b2"
                    ? new Vector3(0.34f, 0.22f, 0.34f)
                    : new Vector3(0.26f, 0.14f, 0.3f),
                id == "amx30b2"
                    ? "French-AMX30B2-COTAC"
                    : "French-AMX30-GunnerSight");
            TankFrenchFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.18f,
                    0.34f,
                    -0.14f),
                "French-AMX30-AANF1",
                false,
                false);
        }

        private static void AddAmx40Roof(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            TankFrenchFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.045f,
                    roof + 0.21f,
                    0.24f),
                new Vector3(0.28f, 0.34f, 0.32f),
                "French-AMX40-PanoramicSight");
            TankFrenchFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.22f,
                    roof + 0.08f,
                    0.92f),
                new Vector3(0.4f, 0.24f, 0.34f),
                "French-AMX40-LLLTV");
            for (int block = 0;
                block < 7;
                block++)
            {
                float angle =
                    block * Mathf.PI * 2f / 7f;
                TankDetailGeometry.Part(
                    "French-AMX40-CupolaVision",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.24f +
                            Mathf.Cos(angle) * 0.28f,
                        roof + 0.11f,
                        -0.16f +
                            Mathf.Sin(angle) * 0.19f),
                    new Vector3(0.08f, 0.05f, 0.035f),
                    Lens());
            }
            TankFrenchFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.14f,
                    0.08f,
                    0.3f),
                "French-AMX40-AANF1",
                false,
                false);
        }

        private static void AddLeclercRoof(
            Transform turret,
            string id,
            Color color,
            float width,
            float roof)
        {
            AddHl70Tower(
                turret,
                color,
                width,
                roof);
            AddLeclercCupolaVision(
                turret,
                width,
                roof);
            TankFrenchFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.236f,
                    0.02f,
                    0.41f),
                "French-Leclerc-ANF1",
                false,
                false);
            TankFrenchFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.244f,
                    0.01f,
                    0.36f),
                "French-Leclerc-M2",
                true,
                false);
            TankFrenchFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    0.055f,
                    roof + 0.16f,
                    -1.56f),
                new Vector3(0.16f, 0.24f, 0.16f),
                "French-Leclerc-HL15");
            if (id != "amx56") return;
            TankFrenchFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.167f,
                    roof + 0.12f,
                    0.18f),
                new Vector3(0.42f, 0.23f, 0.4f),
                "French-AMX56-GunnerSight");
        }

        private static void AddHl70Tower(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            float x = width * 0.153f;
            TankDetailGeometry.Part(
                "Painted-French-Leclerc-HL70Pedestal",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    x,
                    roof + 0.07f,
                    0.98f),
                new Vector3(0.15f, 0.14f, 0.15f),
                color * 0.7f);
            TankDetailGeometry.Part(
                "Painted-French-Leclerc-HL70Head",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.28f,
                    1.02f),
                new Vector3(0.4f, 0.34f, 0.34f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "French-Leclerc-HL70Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.25f,
                    1.196f),
                new Vector3(0.27f, 0.14f, 0.014f),
                Lens());
            TankDetailGeometry.Part(
                "French-Leclerc-HL70Wiper",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x + 0.04f,
                    roof + 0.25f,
                    1.206f),
                new Vector3(0.012f, 0.12f, 0.012f),
                Gunmetal());
        }

        private static void AddLeclercCupolaVision(
            Transform turret,
            float width,
            float roof)
        {
            for (int block = 0;
                block < 6;
                block++)
            {
                float angle =
                    block * Mathf.PI / 3f - 0.5f;
                TankDetailGeometry.Part(
                    "French-Leclerc-CupolaVision",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.155f +
                            Mathf.Cos(angle) * 0.22f,
                        roof + 0.1f,
                        -0.48f +
                            Mathf.Sin(angle) * 0.22f),
                    new Vector3(0.085f, 0.05f, 0.04f),
                    Lens());
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }

        private static Color Lens()
        {
            return new Color(0.025f, 0.14f, 0.17f);
        }
    }
}
