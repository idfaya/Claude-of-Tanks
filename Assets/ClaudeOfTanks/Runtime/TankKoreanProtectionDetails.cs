using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankKoreanProtectionDetails
    {
        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            if (definition.id == "k1a1")
            {
                AddK1SideCages(
                    turret,
                    definition,
                    color,
                    width);
            }
            else if (definition.id == "k2b")
            {
                AddK2BStealthSides(
                    root,
                    definition,
                    color,
                    width,
                    height,
                    length);
                AddK2BStealthTurret(
                    turret,
                    definition,
                    color,
                    width);
            }
        }

        private static void AddK1SideCages(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float shellHalfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.3f);
            float outerX = width * 0.428f;
            float[] stations =
            {
                0.9f,
                0.2f,
                -0.5f,
                -1.18f
            };
            Color rail = color * 0.38f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int row = 0;
                    row < 3;
                    row++)
                {
                    TankDetailGeometry.Part(
                        "Korean-K1-SideCageRail",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * outerX,
                            roof - 0.42f +
                                row * 0.185f,
                            -0.35f),
                        new Vector3(
                            0.028f,
                            0.028f,
                            3.2f),
                        rail);
                }
                for (int post = 0;
                    post < 6;
                    post++)
                {
                    TankDetailGeometry.Part(
                        "Korean-K1-SideCagePost",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * outerX,
                            roof - 0.235f,
                            1.1f - post * 0.58f),
                        new Vector3(
                            0.028f,
                            0.4f,
                            0.028f),
                        rail);
                }
                for (int bracket = 0;
                    bracket < stations.Length;
                    bracket++)
                {
                    float shellX =
                        shellHalfWidth *
                        (bracket == 0 ||
                         bracket == 3
                            ? 0.88f
                            : 0.97f);
                    float centerX =
                        (outerX + shellX) * 0.5f;
                    TankDetailGeometry.Part(
                        "Korean-K1-CageBracket",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * centerX,
                            roof - 0.235f,
                            stations[bracket]),
                        new Vector3(
                            outerX - shellX + 0.1f,
                            0.035f,
                            0.035f),
                        rail);
                    TankDetailGeometry.Part(
                        "Korean-K1-CageWeldFoot",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * shellX,
                            roof - 0.235f,
                            stations[bracket]),
                        new Vector3(
                            0.055f,
                            0.19f,
                            0.12f),
                        rail);
                }
                for (int row = 0;
                    row < 3;
                    row++)
                {
                    Transform corner =
                        TankDetailGeometry.Part(
                            "Korean-K1-CageCornerRail",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side * (outerX - 0.1f),
                                roof - 0.42f +
                                    row * 0.185f,
                                -1.84f),
                            new Vector3(
                                0.025f,
                                0.025f,
                                0.56f),
                            rail);
                    corner.localRotation =
                        Quaternion.Euler(
                            0f,
                            side * 44f,
                            0f);
                }
                for (int pack = 0;
                    pack < 2;
                    pack++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Korean-K1-CageStowage",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * (outerX - 0.04f),
                            roof - 0.24f,
                            -1.42f +
                                pack * 1f),
                        new Vector3(
                            0.12f,
                            0.28f,
                            pack == 0 ? 0.4f : 0.3f),
                        color * (0.62f +
                            pack * 0.06f));
                }
                TankDetailGeometry.Part(
                    "Korean-K1-CageHardCase",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * (outerX - 0.04f),
                        roof - 0.24f,
                        0.45f),
                    new Vector3(0.12f, 0.2f, 0.34f),
                    Gunmetal());
            }
        }

        private static void AddK2BStealthSides(
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
                    height * 0.6f);
            float front = length * 0.37f;
            float pitch = length * 0.105f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 8;
                    panel++)
                {
                    float panelHeight =
                        panel == 0 ||
                        panel == 7
                            ? 0.82f
                            : 0.94f;
                    float z =
                        front - panel * pitch;
                    Transform cassette =
                        TankDetailGeometry.Part(
                            "Painted-K2B-StealthSideCassette",
                            PrimitiveType.Cube,
                            root,
                            new Vector3(
                                side * width * 0.49f,
                                roof -
                                    panelHeight * 0.52f +
                                    (panel % 2) * 0.008f,
                                z),
                            new Vector3(
                                0.075f,
                                panelHeight,
                                pitch * 0.9f),
                            color * 0.76f);
                    cassette.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            side *
                                (panel < 2
                                    ? 2f
                                    : -0.7f));
                    TankDetailGeometry.Part(
                        "K2B-StealthCassetteSeam",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.5f,
                            roof - 0.18f,
                            z),
                        new Vector3(
                            0.02f,
                            0.04f,
                            pitch * 0.72f),
                        Gunmetal());
                }
                Transform shoulder =
                    TankDetailGeometry.Part(
                        "Painted-K2B-BowShoulder",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.266f,
                            roof - 0.33f,
                            length * 0.444f),
                        new Vector3(
                            width * 0.426f,
                            0.5f,
                            length * 0.117f),
                        color * 0.8f);
                shoulder.localRotation =
                    Quaternion.Euler(
                        -6f,
                        side * -5f,
                        side * -2f);
            }
        }

        private static void AddK2BStealthTurret(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float front =
                TankDetailGeometry.TurretFrontZ(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.42f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform frontPanel =
                    TankDetailGeometry.Part(
                        "Painted-K2B-StealthTurretPanel",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * halfWidth * 0.35f,
                            roof - 0.35f,
                            front * 0.52f),
                        new Vector3(
                            halfWidth * 0.72f,
                            0.58f,
                            front * 0.52f),
                        color * 0.76f);
                frontPanel.localRotation =
                    Quaternion.Euler(
                        -4f,
                        side * -7f,
                        side * -2f);
                Transform rearPanel =
                    TankDetailGeometry.Part(
                        "Painted-K2B-StealthTurretPanel",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * halfWidth * 0.33f,
                            roof - 0.33f,
                            rear * 0.42f),
                        new Vector3(
                            halfWidth * 0.7f,
                            0.54f,
                            Mathf.Abs(rear) * 0.72f),
                        color * 0.72f);
                rearPanel.localRotation =
                    Quaternion.Euler(
                        2f,
                        side * 3f,
                        side);
            }
            TankDetailGeometry.Part(
                "K2B-StealthRoofPanel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    0f,
                    roof + 0.025f,
                    -0.78f),
                new Vector3(
                    width * 0.56f,
                    0.035f,
                    Mathf.Abs(rear) * 0.82f),
                Gunmetal());
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }
    }
}
