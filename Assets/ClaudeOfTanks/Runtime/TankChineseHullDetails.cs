using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankChineseHullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            TankChinesePrimaryDetails.BuildHull(
                root,
                definition,
                color,
                width,
                height,
                length);
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    height * 0.58f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.45f);
            AddRearService(
                root,
                definition.id,
                color,
                width,
                roof,
                rear);
            AddEngineDeck(
                root,
                definition.id,
                color,
                width,
                roof,
                rear);
            AddRearHardware(
                root,
                color,
                width,
                roof,
                rear);
            if (definition.id == "type59")
                AddType59Hull(
                    root,
                    color,
                    width,
                    roof,
                    rear,
                    length);
            if (definition.id == "type99a" ||
                definition.id == "ztz99a2" ||
                definition.id == "vt4a1")
            {
                AddRearFuelDrums(
                    root,
                    definition.id,
                    color,
                    width,
                    roof,
                    rear);
            }
        }

        private static void AddRearService(
            Transform root,
            string id,
            Color color,
            float width,
            float roof,
            float rear)
        {
            int fields =
                id == "ztz85_iii" ? 3 : 2;
            int slats =
                id == "type59" ? 4 : 5;
            for (int field = 0;
                field < fields;
                field++)
            {
                float x =
                    (field - (fields - 1) * 0.5f) *
                    width * 0.24f;
                float fieldWidth =
                    width *
                    (fields == 3 ? 0.22f : 0.32f);
                TankDetailGeometry.Part(
                    "Painted-Chinese-RearServiceFrame",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        x,
                        roof - 0.32f,
                        rear - 0.022f),
                    new Vector3(
                        fieldWidth,
                        0.42f,
                        0.05f),
                    color * 0.66f);
                TankDetailGeometry.Part(
                    "Chinese-RearServiceGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        x,
                        roof - 0.32f,
                        rear - 0.052f),
                    new Vector3(
                        fieldWidth * 0.88f,
                        0.32f,
                        0.018f),
                    TankChineseFamilyDetails.Gunmetal());
                for (int slat = 0;
                    slat < slats;
                    slat++)
                {
                    TankDetailGeometry.Part(
                        "Chinese-RearServiceSlat",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            x,
                            roof - 0.45f +
                                slat *
                                (0.26f /
                                 Mathf.Max(
                                     1,
                                     slats - 1)),
                            rear - 0.065f),
                        new Vector3(
                            fieldWidth * 0.82f,
                            0.018f,
                            0.016f),
                        color * 0.35f);
                }
            }
        }

        private static void AddEngineDeck(
            Transform root,
            string id,
            Color color,
            float width,
            float roof,
            float rear)
        {
            float depth =
                id == "type59"
                    ? 1.2f
                    : id == "ztz85_iii"
                        ? 1.42f
                        : 1.58f;
            float centerZ =
                rear + depth * 0.94f;
            TankDetailGeometry.Part(
                "Painted-Chinese-EngineDeck",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    roof + 0.018f,
                    centerZ),
                new Vector3(
                    width * 0.62f,
                    0.035f,
                    depth),
                color * 0.74f);
            int slatCount =
                id == "type99a" ? 8 : 6;
            for (int slat = 0;
                slat < slatCount;
                slat++)
            {
                TankDetailGeometry.Part(
                    "Chinese-EngineDeckSlat",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        0f,
                        roof + 0.044f,
                        centerZ -
                            depth * 0.36f +
                            slat * depth *
                            (0.72f /
                             Mathf.Max(
                                 1,
                                 slatCount - 1))),
                    new Vector3(
                        width * 0.52f,
                        0.018f,
                        0.045f),
                    TankChineseFamilyDetails.Gunmetal());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Chinese-EngineFan",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.18f,
                        roof + 0.052f,
                        centerZ),
                    new Vector3(0.2f, 0.018f, 0.2f),
                    TankChineseFamilyDetails.Gunmetal());
            }
        }

        private static void AddRearHardware(
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
                TankDetailGeometry.Part(
                    "Chinese-RearMarker",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.34f,
                        roof - 0.24f,
                        rear - 0.07f),
                    new Vector3(0.14f, 0.09f, 0.03f),
                    new Color(0.5f, 0.04f, 0.025f));
                Transform clevis =
                    TankDetailGeometry.Part(
                        "Chinese-RearTowClevis",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.22f,
                            roof - 0.58f,
                            rear - 0.08f),
                        new Vector3(0.08f, 0.035f, 0.08f),
                        color * 0.4f);
                clevis.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void AddType59Hull(
            Transform root,
            Color color,
            float width,
            float roof,
            float rear,
            float length)
        {
            Transform infrared =
                TankDetailGeometry.Part(
                    "Chinese-Type59-GlacisInfrared",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        width * 0.15f,
                        roof - 0.05f,
                        length * 0.34f),
                    new Vector3(0.13f, 0.1f, 0.13f),
                    TankChineseFamilyDetails.Gunmetal());
            infrared.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                "Chinese-Type59-GlacisInfraredLens",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    width * 0.15f,
                    roof - 0.04f,
                    length * 0.36f),
                new Vector3(0.1f, 0.015f, 0.1f),
                TankChineseFamilyDetails.Lens())
                .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                "Chinese-Type59-BowMachineGunPort",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    width * 0.28f,
                    roof - 0.12f,
                    length * 0.42f),
                new Vector3(0.08f, 0.035f, 0.08f),
                TankChineseFamilyDetails.Gunmetal())
                .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                "Chinese-Type59-Muffler",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    -width * 0.43f,
                    roof + 0.08f,
                    rear + 0.58f),
                new Vector3(0.1f, 0.38f, 0.1f),
                TankChineseFamilyDetails.Gunmetal())
                .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                "Chinese-Type59-OPVTSnorkel",
                PrimitiveType.Cylinder,
                root,
                new Vector3(
                    0f,
                    roof + 0.09f,
                    rear + 0.82f),
                new Vector3(0.09f, width * 0.27f, 0.09f),
                color * 0.55f)
                .localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
            for (int panel = 0;
                panel < 3;
                panel++)
            {
                TankDetailGeometry.Part(
                    "Painted-Chinese-Type59-GlacisPanel",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        (panel - 1) * width * 0.25f,
                        roof - 0.03f,
                        length * 0.29f),
                    new Vector3(
                        panel == 1 ? 0.7f : 0.62f,
                        0.075f,
                        1.18f),
                    color * 0.78f)
                    .localRotation =
                        Quaternion.Euler(-10f, 0f, 0f);
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 7;
                    panel++)
                {
                    float z =
                        length * 0.34f -
                        panel * length * 0.115f;
                    TankDetailGeometry.Part(
                        "Painted-Chinese-Type59-SkirtPanel",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.505f,
                            roof - 0.42f,
                            z),
                        new Vector3(
                            0.055f,
                            0.48f,
                            length * 0.102f),
                        color * 0.72f);
                    TankDetailGeometry.Part(
                        "Chinese-Type59-SkirtHanger",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.48f,
                            roof - 0.12f,
                            z),
                        new Vector3(
                            0.18f,
                            0.08f,
                            length * 0.09f),
                        TankChineseFamilyDetails.Gunmetal());
                }
            }
        }

        private static void AddRearFuelDrums(
            Transform root,
            string id,
            Color color,
            float width,
            float roof,
            float rear)
        {
            float radius =
                id == "type99a" ? 0.18f : 0.25f;
            float z =
                rear - (id == "type99a" ? 0.3f : 0.42f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform drum =
                    TankDetailGeometry.Part(
                        "Painted-Chinese-RearFuelDrum",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.21f,
                            roof + radius * 0.15f,
                            z),
                        new Vector3(
                            radius,
                            width * 0.11f,
                            radius),
                        color * 0.58f);
                drum.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
                for (int band = -1;
                    band <= 1;
                    band += 2)
                {
                    TankDetailGeometry.Part(
                        "Chinese-RearFuelDrumBand",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.21f +
                                band * width * 0.055f,
                            roof + radius * 0.15f,
                            z),
                        new Vector3(
                            0.035f,
                            radius * 2.06f,
                            radius * 2.06f),
                        TankChineseFamilyDetails.Gunmetal());
                }
            }
        }
    }
}
