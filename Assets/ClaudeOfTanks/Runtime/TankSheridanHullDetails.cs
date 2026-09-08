using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankSheridanHullDetails
    {
        public static void Build(
            Transform root,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            bool tts =
                definition.id == "m551a1_tts";
            float roof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    height * 0.55f);
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f);

            AddDriverHatch(
                root,
                color,
                roof,
                length);
            AddEngineDeck(
                root,
                color,
                roof,
                rear,
                tts);
            AddHeadlights(
                root,
                color,
                width,
                roof,
                length);
            if (tts)
            {
                AddTtsRearExtension(
                    root,
                    color,
                    width,
                    roof,
                    rear,
                    length);
                AddTtsSkirtCage(
                    root,
                    width,
                    roof,
                    length);
            }
            else
            {
                AddFuelDrums(
                    root,
                    color,
                    width,
                    roof,
                    rear);
            }
        }

        private static void AddDriverHatch(
            Transform root,
            Color color,
            float roof,
            float length)
        {
            TankDetailGeometry.Part(
                "Painted-Sheridan-DriverHatch",
                PrimitiveType.Cylinder,
                root,
                new Vector3(0f, roof + 0.035f, length * 0.25f),
                new Vector3(0.4f, 0.04f, 0.34f),
                color * 0.82f);
            TankDetailGeometry.Part(
                "Sheridan-DriverHatchRing",
                PrimitiveType.Cylinder,
                root,
                new Vector3(0f, roof + 0.054f, length * 0.25f),
                new Vector3(0.43f, 0.018f, 0.37f),
                TankSheridanFamilyDetails.Gunmetal());
        }

        private static void AddEngineDeck(
            Transform root,
            Color color,
            float roof,
            float rear,
            bool tts)
        {
            float deckZ = tts
                ? rear + 0.72f
                : rear + 0.95f;
            for (int bank = 0;
                bank < 2;
                bank++)
            {
                float x = bank == 0
                    ? -0.48f
                    : 0.15f;
                TankDetailGeometry.Part(
                    "Painted-Sheridan-EngineGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(x, roof + 0.025f, deckZ),
                    new Vector3(
                        0.52f,
                        0.05f,
                        tts ? 0.72f : 0.6f),
                    color * 0.58f);
                int louvreCount = tts ? 7 : 6;
                for (int louvre = 0;
                    louvre < louvreCount;
                    louvre++)
                {
                    TankDetailGeometry.Part(
                        "Sheridan-EngineLouvre",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            x,
                            roof + 0.058f,
                            deckZ -
                                (louvreCount - 1) * 0.045f +
                                louvre * 0.09f),
                        new Vector3(0.46f, 0.016f, 0.03f),
                        color * 0.36f);
                }
            }
        }

        private static void AddHeadlights(
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
                    "Painted-Sheridan-HeadlightGuard",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.3f,
                        roof - 0.12f,
                        length * 0.43f),
                    new Vector3(0.3f, 0.22f, 0.06f),
                    color * 0.68f);
                TankDetailGeometry.Part(
                    "Sheridan-HeadlightLens",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.3f,
                        roof - 0.12f,
                        length * 0.43f + 0.04f),
                    new Vector3(0.065f, 0.025f, 0.065f),
                    TankSheridanFamilyDetails.Lens())
                    .localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void AddFuelDrums(
            Transform root,
            Color color,
            float width,
            float roof,
            float rear)
        {
            for (int rail = -1;
                rail <= 1;
                rail++)
            {
                TankDetailGeometry.Part(
                    "Sheridan-FuelDrumSupportRail",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        rail * width * 0.27f,
                        roof - 0.08f,
                        rear + 0.22f),
                    new Vector3(0.07f, 0.07f, 0.68f),
                    TankSheridanFamilyDetails.Gunmetal());
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform drum =
                    TankDetailGeometry.Part(
                        "Painted-Sheridan-RearFuelDrum",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.22f,
                            roof - 0.02f,
                            rear - 0.02f),
                        new Vector3(0.29f, 0.48f, 0.29f),
                        color * 0.72f);
                drum.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
                TankDetailGeometry.Part(
                    "Sheridan-FuelDrumBand",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.22f,
                        roof - 0.02f,
                        rear - 0.02f),
                    new Vector3(0.3f, 0.055f, 0.3f),
                    TankSheridanFamilyDetails.Gunmetal())
                    .localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
            }
        }

        private static void AddTtsRearExtension(
            Transform root,
            Color color,
            float width,
            float roof,
            float rear,
            float length)
        {
            TankDetailGeometry.Part(
                "Painted-Sheridan-TTS-RearDeckExtension",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    roof - 0.12f,
                    rear - length * 0.03f),
                new Vector3(
                    width * 0.84f,
                    0.5f,
                    length * 0.16f),
                color * 0.76f);
            TankDetailGeometry.Part(
                "Painted-Sheridan-TTS-APU",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    width * 0.29f,
                    roof + 0.14f,
                    rear + 0.4f),
                new Vector3(0.48f, 0.25f, 0.62f),
                color * 0.66f);
            TankDetailGeometry.Part(
                "Painted-Sheridan-TTS-ElectronicsBox",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    -width * 0.3f,
                    roof + 0.1f,
                    rear + 0.25f),
                new Vector3(0.4f, 0.19f, 0.42f),
                color * 0.7f);
        }

        private static void AddTtsSkirtCage(
            Transform root,
            float width,
            float roof,
            float length)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int rail = 0;
                    rail < 2;
                    rail++)
                {
                    TankDetailGeometry.Part(
                        "Sheridan-TTS-SkirtCageRail",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.58f,
                            roof - 0.78f + rail * 0.82f,
                            -0.14f),
                        new Vector3(0.045f, 0.045f, length * 0.8f),
                        TankSheridanFamilyDetails.Gunmetal());
                }
                for (int post = 0;
                    post < 8;
                    post++)
                {
                    TankDetailGeometry.Part(
                        "Sheridan-TTS-SkirtCagePost",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.58f,
                            roof - 0.37f,
                            length *
                                (0.35f - post * 0.1f)),
                        new Vector3(0.045f, 0.85f, 0.045f),
                        TankSheridanFamilyDetails.Gunmetal());
                    TankDetailGeometry.Part(
                        "Sheridan-TTS-SkirtCageStandOff",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.55f,
                            roof - 0.37f,
                            length *
                                (0.35f - post * 0.1f)),
                        new Vector3(
                            width * 0.08f,
                            0.04f,
                            0.04f),
                        TankSheridanFamilyDetails.Gunmetal());
                }
            }
        }
    }
}
