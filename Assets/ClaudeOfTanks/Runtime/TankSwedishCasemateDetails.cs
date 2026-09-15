using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSwedishCasemateDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    height * 0.72f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.45f);
            AddFixedGunCollar(
                root,
                definition,
                color);
            AddFixedGun(
                root,
                definition.id,
                color);
            if (definition.id == "udes03")
            {
                AddUdes(
                    root,
                    color,
                    width,
                    roof,
                    rear);
                return;
            }
            AddStrv103Common(
                root,
                definition,
                color,
                width,
                roof,
                rear);
            if (definition.id == "strv103")
                AddStrv103BPackage(
                    root,
                    color,
                    width,
                    roof,
                    length);
        }

        private static void AddFixedGun(
            Transform root,
            string id,
            Color color)
        {
            string prefix = id == "udes03"
                ? "Swedish-UDES03"
                : id == "strv103"
                    ? "Swedish-Strv103B"
                    : "Swedish-Strv103A";
            float axisY = id == "udes03" ? 1.43f : 1.59f;
            float muzzleZ = id == "udes03"
                ? 4.70f
                : id == "strv103"
                    ? 5.19f
                    : 5.47f;
            Transform assembly =
                new GameObject(prefix + "-FixedGunAssembly")
                    .transform;
            assembly.SetParent(root, false);

            AddGunSection(
                prefix + "-MuzzleCollar",
                assembly,
                axisY,
                muzzleZ - 0.15f,
                muzzleZ,
                id == "udes03" ? 0.102f : 0.105f,
                color * 0.46f);
            AddGunSection(
                "Painted-" + prefix + "-ForwardTube",
                assembly,
                axisY,
                id == "udes03" ? 3.02f : 3.55f,
                muzzleZ - 0.15f,
                id == "udes03" ? 0.082f : 0.090f,
                color * 0.48f);
            AddGunSection(
                "Painted-" + prefix + "-MidTube",
                assembly,
                axisY,
                id == "udes03" ? 1.92f : 2.30f,
                id == "udes03" ? 3.02f : 3.55f,
                id == "udes03" ? 0.090f : 0.098f,
                color * 0.50f);
            AddGunSection(
                "Painted-" + prefix + "-RootTube",
                assembly,
                axisY,
                id == "udes03" ? 0.72f : 1.10f,
                id == "udes03" ? 1.92f : 2.30f,
                id == "udes03" ? 0.116f : 0.116f,
                color * 0.52f);
            AddGunSection(
                prefix + "-MuzzleBore",
                assembly,
                axisY,
                muzzleZ - 0.025f,
                muzzleZ + 0.025f,
                id == "udes03" ? 0.065f : 0.070f,
                new Color(0.025f, 0.028f, 0.024f));
        }

        private static void AddGunSection(
            string name,
            Transform parent,
            float axisY,
            float startZ,
            float endZ,
            float radius,
            Color color)
        {
            Transform section =
                TankShapeFactory.CylinderPart(
                    name,
                    parent,
                    radius,
                    radius,
                    endZ - startZ,
                    18,
                    TankShapeAxis.Z,
                    color);
            section.localPosition =
                new Vector3(
                    0f,
                    axisY,
                    (startZ + endZ) * 0.5f);
        }

        private static void AddUdes(
            Transform root,
            Color color,
            float width,
            float roof,
            float rear)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform sleeve =
                    TankDetailGeometry.Part(
                        "Painted-Swedish-UDES03-HydraulicRam",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.24f,
                            roof - 0.28f,
                            2.25f),
                        new Vector3(0.065f, 0.36f, 0.065f),
                        color * 0.62f);
                sleeve.localRotation =
                    Quaternion.Euler(77f, 0f, 0f);
                Transform rod =
                    TankDetailGeometry.Part(
                        "Swedish-UDES03-HydraulicRod",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.24f,
                            roof - 0.39f,
                            2.7f),
                        new Vector3(0.038f, 0.28f, 0.038f),
                        Steel());
                rod.localRotation =
                    Quaternion.Euler(77f, 0f, 0f);
            }
            TankDetailGeometry.Part(
                "Painted-Swedish-UDES03-DriverHatch",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    -width * 0.22f,
                    roof + 0.035f,
                    -0.1f),
                new Vector3(0.28f, 0.035f, 0.28f),
                color * 0.8f);
            TankDetailGeometry.Part(
                "Painted-Swedish-UDES03-CommanderHatch",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    width * 0.13f,
                    roof + 0.04f,
                    -0.24f),
                new Vector3(0.5f, 0.08f, 0.4f),
                color * 0.8f);
            TankSwedishFamilyDetails.AddSight(
                root,
                color,
                new Vector3(
                    width * 0.22f,
                    roof + 0.15f,
                    -0.54f),
                new Vector3(0.24f, 0.18f, 0.26f),
                "Swedish-UDES03-Sight");
            TankSwedishFamilyDetails.AddMachineGun(
                root,
                roof,
                color,
                new Vector3(
                    width * 0.13f,
                    0.07f,
                    -0.28f),
                "Swedish-UDES03-Ksp58",
                false,
                false);
            TankSwedishFamilyDetails.AddAntennas(
                root,
                color,
                roof,
                rear * 0.45f,
                width,
                1,
                "Swedish-UDES03");
            AddLouvres(
                root,
                color,
                roof,
                width,
                14,
                "Swedish-UDES03");
        }

        private static void AddStrv103Common(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float roof,
            float rear)
        {
            bool bModel =
                definition.id == "strv103";
            string prefix =
                bModel
                    ? "Swedish-Strv103B"
                    : "Swedish-Strv103A";
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Cupola",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    width * 0.08f,
                    roof + 0.055f,
                    -0.35f),
                new Vector3(0.24f, 0.055f, 0.24f),
                color * 0.78f);
            TankSwedishFamilyDetails.AddSight(
                root,
                color,
                new Vector3(
                    width * 0.18f,
                    roof + 0.16f,
                    -0.72f),
                new Vector3(0.32f, 0.24f, 0.34f),
                prefix + "-CommanderSight");
            TankSwedishFamilyDetails.AddSight(
                root,
                color,
                new Vector3(
                    -width * 0.19f,
                    roof + 0.11f,
                    -0.82f),
                new Vector3(0.3f, 0.2f, 0.32f),
                prefix + "-DriverSight");
            TankSwedishFamilyDetails.AddMachineGun(
                root,
                roof,
                color,
                new Vector3(
                    width * 0.14f,
                    0.07f,
                    -0.2f),
                prefix + "-Ksp58",
                false,
                true);
            TankSwedishFamilyDetails.AddAntennas(
                root,
                color,
                roof,
                rear * 0.52f,
                width,
                2,
                prefix);
            AddLouvres(
                root,
                color,
                roof,
                width,
                bModel ? 14 : 12,
                prefix);
            AddRearRack(
                root,
                color,
                width,
                roof,
                rear,
                prefix);
        }

        private static void AddStrv103BPackage(
            Transform root,
            Color color,
            float width,
            float roof,
            float length)
        {
            TankDetailGeometry.Part(
                "Painted-Swedish-Strv103B-DozerBlade",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    roof - 0.85f,
                    length * 0.43f),
                new Vector3(
                    width * 0.84f,
                    0.22f,
                    0.6f),
                color * 0.62f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform arm =
                    TankDetailGeometry.Part(
                        "Swedish-Strv103B-DozerArm",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.25f,
                            roof - 0.6f,
                            length * 0.36f),
                        new Vector3(0.08f, 0.1f, 0.9f),
                        Steel());
                arm.localRotation =
                    Quaternion.Euler(-24f, 0f, 0f);
            }
            for (int row = 0;
                row < 2;
                row++)
            {
                TankDetailGeometry.Part(
                    "Swedish-Strv103B-NoseFenceRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0f,
                        roof - 0.18f +
                            row * 0.34f,
                        length * 0.36f),
                    new Vector3(
                        width * 0.74f,
                        0.03f,
                        0.03f),
                    Steel());
            }
            for (int post = 0;
                post < 11;
                post++)
            {
                TankDetailGeometry.Part(
                    "Swedish-Strv103B-NoseFencePost",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -width * 0.34f +
                            post * width * 0.068f,
                        roof - 0.01f,
                        length * 0.36f),
                    new Vector3(0.03f, 0.39f, 0.03f),
                    Steel());
            }
        }

        private static void AddFixedGunCollar(
            Transform root,
            VehicleDefinition definition,
            Color color)
        {
            float y =
                definition.armor.gunPivot.y;
            float z =
                definition.armor.gunPivot.z;
            Transform collar =
                TankDetailGeometry.Part(
                    "Painted-Swedish-FixedGunCollar",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(0f, y, z + 0.18f),
                    new Vector3(0.16f, 0.2f, 0.16f),
                    color * 0.62f);
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddLouvres(
            Transform root,
            Color color,
            float roof,
            float width,
            int count,
            string prefix)
        {
            int perSide = count / 2;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    prefix + "-GlacisLouvreWell",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.16f,
                        roof - 0.12f,
                        1.6f),
                    new Vector3(
                        width * 0.28f,
                        0.025f,
                        0.9f),
                    Gunmetal());
                for (int index = 0;
                    index < perSide;
                    index++)
                {
                    TankDetailGeometry.Part(
                        prefix + "-GlacisLouvre",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.16f,
                            roof - 0.09f,
                            1.25f +
                                index * 0.12f),
                        new Vector3(
                            width * 0.26f,
                            0.025f,
                            0.04f),
                        color * 0.38f);
                }
            }
        }

        private static void AddRearRack(
            Transform root,
            Color color,
            float width,
            float roof,
            float rear,
            string prefix)
        {
            for (int row = 0;
                row < 3;
                row++)
            {
                TankDetailGeometry.Part(
                    prefix + "-RearRackRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0f,
                        roof + 0.02f +
                            row * 0.1f,
                        rear - 0.18f),
                    new Vector3(
                        width * 0.55f,
                        0.03f,
                        0.03f),
                    color * 0.38f);
            }
            for (int post = 0;
                post < 5;
                post++)
            {
                TankDetailGeometry.Part(
                    prefix + "-RearRackRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        -width * 0.24f +
                            post * width * 0.12f,
                        roof + 0.12f,
                        rear - 0.18f),
                    new Vector3(0.03f, 0.24f, 0.03f),
                    color * 0.38f);
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }

        private static Color Steel()
        {
            return new Color(0.12f, 0.13f, 0.12f);
        }
    }
}
