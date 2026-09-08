using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankPattonHullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            string id = definition.id;
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    height * 0.55f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f);

            AddEngineDeck(
                root,
                color,
                width,
                roof,
                rear);
            AddRearServiceWall(
                root,
                color,
                width,
                roof,
                rear,
                id == "m48" ? 9 : 6);
            AddBowEquipment(
                root,
                color,
                width,
                roof,
                length);

            if (id == "m46_patton" ||
                id == "m47_patton")
            {
                AddEarlyFenderPackage(
                    root,
                    color,
                    width,
                    roof,
                    length,
                    id);
                return;
            }
            if (id == "m48")
            {
                AddM48ServiceBoxes(
                    root,
                    color,
                    width,
                    roof,
                    length);
                return;
            }

            AddM60ProtectionCourse(
                root,
                color,
                width,
                roof,
                id);
            if (id == "m60a2")
            {
                AddStarshipShoulders(
                    root,
                    color,
                    width,
                    roof,
                    length);
            }
        }

        private static void AddEngineDeck(
            Transform root,
            Color color,
            float width,
            float roof,
            float rear)
        {
            TankDetailGeometry.Part(
                "Painted-Patton-EngineDeck",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, roof + 0.025f, rear + 1.15f),
                new Vector3(width * 0.56f, 0.05f, 1.45f),
                color * 0.88f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int row = 0; row < 2; row++)
                {
                    TankDetailGeometry.Part(
                        "Patton-EngineGrille",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.17f,
                            roof + 0.058f,
                            rear + 0.82f + row * 0.48f),
                        new Vector3(
                            width * 0.27f,
                            0.018f,
                            0.35f),
                        color * 0.42f);
                }
            }
        }

        private static void AddRearServiceWall(
            Transform root,
            Color color,
            float width,
            float roof,
            float rear,
            int louvreCount)
        {
            float centerY = roof * 0.68f;
            TankDetailGeometry.Part(
                "Painted-Patton-RearServiceFrame",
                PrimitiveType.Cube,
                root,
                new Vector3(0f, centerY, rear - 0.025f),
                new Vector3(width * 0.48f, roof * 0.55f, 0.045f),
                color * 0.68f);
            for (int row = 0; row < louvreCount; row++)
            {
                Transform louvre =
                    TankDetailGeometry.Part(
                        "Patton-RearServiceLouvre",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            0f,
                            centerY -
                                (louvreCount - 1) * 0.035f +
                                row * 0.07f,
                            rear - 0.052f),
                        new Vector3(
                            width * 0.39f,
                            0.025f,
                            0.018f),
                        color * 0.38f);
                louvre.localRotation =
                    Quaternion.Euler(0f, 0f, -18f);
            }
        }

        private static void AddBowEquipment(
            Transform root,
            Color color,
            float width,
            float roof,
            float length)
        {
            float front = length * 0.43f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Patton-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.29f,
                        roof - 0.14f,
                        front),
                    new Vector3(0.3f, 0.2f, 0.06f),
                    color * 0.66f);
                TankDetailGeometry.Part(
                    "Patton-HeadlightLens",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.29f,
                        roof - 0.14f,
                        front + 0.04f),
                    new Vector3(0.065f, 0.025f, 0.065f),
                    TankPattonFamilyDetails.Lens())
                    .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void AddEarlyFenderPackage(
            Transform root,
            Color color,
            float width,
            float roof,
            float length,
            string id)
        {
            string prefix = id == "m46_patton"
                ? "Patton-M46"
                : "Patton-M47";
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform muffler =
                    TankDetailGeometry.Part(
                        prefix + "-FenderMuffler",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.36f,
                            roof + 0.08f,
                            -length * 0.28f),
                        new Vector3(0.15f, length * 0.26f, 0.15f),
                        TankPattonFamilyDetails.Gunmetal());
                muffler.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
                for (int strap = -1;
                    strap <= 1;
                    strap += 2)
                {
                    TankDetailGeometry.Part(
                        "Painted-" + prefix +
                            "-MufflerStrap",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.36f,
                            roof + 0.08f,
                            -length * 0.28f +
                                strap * length * 0.09f),
                        new Vector3(0.34f, 0.035f, 0.055f),
                        color * 0.66f);
                }
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-FenderBox",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.4f,
                        roof + 0.1f,
                        length * 0.24f),
                    new Vector3(
                        width * 0.18f,
                        0.2f,
                        length * 0.18f),
                    color * 0.78f);
            }
        }

        private static void AddM48ServiceBoxes(
            Transform root,
            Color color,
            float width,
            float roof,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int box = 0; box < 2; box++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Patton-M48-FenderBox",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.42f,
                            roof + 0.09f,
                            length * (0.12f - box * 0.25f)),
                        new Vector3(
                            width * 0.18f,
                            0.18f,
                            length * (box == 0
                                ? 0.13f
                                : 0.1f)),
                        color * 0.78f);
                }
            }
        }

        private static void AddM60ProtectionCourse(
            Transform root,
            Color color,
            float width,
            float roof,
            string id)
        {
            int sideCount =
                id == "m60a1" ? 8 : 9;
            float spacing = id == "m60a1"
                ? 0.66f
                : 0.61f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < sideCount;
                    panel++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Patton-" +
                            id.ToUpperInvariant() +
                            "-SideCassette",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.49f,
                            roof - 0.42f,
                            (panel -
                                (sideCount - 1) * 0.5f) *
                                spacing),
                        new Vector3(
                            0.075f,
                            id == "m60a1"
                                ? 0.58f
                                : 0.54f,
                            spacing * 0.82f),
                        color * 0.8f);
                }
                TankDetailGeometry.Part(
                    "Painted-Patton-" +
                        id.ToUpperInvariant() +
                        "-SideRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.48f,
                        roof - 0.08f,
                        0f),
                    new Vector3(
                        0.06f,
                        0.07f,
                        sideCount * spacing),
                    color * 0.62f);
            }

            int glacisCount =
                id == "m60a1" ? 8 : 10;
            for (int panel = 0;
                panel < glacisCount;
                panel++)
            {
                int row = panel < glacisCount / 2
                    ? 0
                    : 1;
                int columns = glacisCount / 2;
                int column = panel % columns;
                TankDetailGeometry.Part(
                    "Painted-Patton-" +
                        id.ToUpperInvariant() +
                        "-GlacisCassette",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        (column -
                            (columns - 1) * 0.5f) * 0.45f,
                        roof - 0.075f + row * 0.05f,
                        1.8f + row * 0.43f),
                    new Vector3(0.38f, 0.09f, 0.33f),
                    color * 0.82f);
            }
        }

        private static void AddStarshipShoulders(
            Transform root,
            Color color,
            float width,
            float roof,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Patton-M60A2-RaisedShoulder",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.39f,
                        roof - 0.02f,
                        -length * 0.21f),
                    new Vector3(
                        width * 0.2f,
                        0.34f,
                        length * 0.48f),
                    color * 0.78f);
            }
        }
    }
}
