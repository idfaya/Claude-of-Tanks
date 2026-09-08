using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankItalianVariantDetails
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
            if (definition.id == "carro45t")
            {
                AddCarroRoof(
                    turret,
                    color,
                    width,
                    roof);
                return;
            }
            AddArieteRoof(
                turret,
                definition.id,
                color,
                width,
                roof);
        }

        private static void AddCarroRoof(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            TankItalianFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.27f,
                    roof + 0.14f,
                    -0.35f),
                new Vector3(0.32f, 0.28f, 0.42f),
                "Italian-Carro45T-CommanderSight");
            TankItalianFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.16f,
                    roof + 0.08f,
                    0.7f),
                new Vector3(0.22f, 0.16f, 0.2f),
                "Italian-Carro45T-GunnerSight");
            AddVisionRing(
                turret,
                0f,
                -0.78f,
                roof,
                5,
                "Italian-Carro45T-CommanderVision");
            AddVisionRing(
                turret,
                -0.55f,
                0.1f,
                roof,
                4,
                "Italian-Carro45T-LoaderVision");
            TankItalianFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.09f,
                    0.08f,
                    -0.62f),
                "Italian-Carro45T-Breda",
                false,
                false);
            AddCarroCrownRails(
                turret,
                color,
                width,
                roof);
        }

        private static void AddCarroCrownRails(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            Color rail = color * 0.36f;
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Italian-Carro45T-CrownRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.2f,
                        roof + 0.065f,
                        -0.95f),
                    new Vector3(0.03f, 0.03f, 0.84f),
                    rail);
                for (int zIndex = 0;
                    zIndex < 2;
                    zIndex++)
                {
                    TankDetailGeometry.Part(
                        "Italian-Carro45T-CrownRailPost",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.2f,
                            roof + 0.025f,
                            -0.55f -
                                zIndex * 0.8f),
                        new Vector3(0.03f, 0.12f, 0.03f),
                        rail);
                }
            }
            for (int zIndex = 0;
                zIndex < 2;
                zIndex++)
            {
                TankDetailGeometry.Part(
                    "Italian-Carro45T-CrownCrossRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof + 0.065f,
                        -0.55f -
                            zIndex * 0.8f),
                    new Vector3(
                        width * 0.42f,
                        0.03f,
                        0.03f),
                    rail);
            }
        }

        private static void AddArieteRoof(
            Transform turret,
            string id,
            Color color,
            float width,
            float roof)
        {
            TankItalianFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.19f,
                    roof + 0.17f,
                    0.35f),
                new Vector3(0.34f, 0.32f, 0.38f),
                "Italian-Ariete-TURMS");
            TankItalianFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.075f,
                    roof + 0.25f,
                    -0.82f),
                new Vector3(0.24f, 0.4f, 0.28f),
                "Italian-Ariete-PanoramicSight");
            TankItalianFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.22f,
                    roof + 0.08f,
                    0.48f),
                new Vector3(0.22f, 0.18f, 0.24f),
                "Italian-Ariete-LoaderSight");
            AddVisionRing(
                turret,
                width * 0.13f,
                -0.5f,
                roof,
                6,
                "Italian-Ariete-CupolaVision");
            AddGalixPlatforms(
                turret,
                color,
                width,
                roof);
            AddRoofPanelCadence(
                turret,
                width,
                roof);

            if (id == "ariete")
            {
                TankItalianFamilyDetails.AddMachineGun(
                    turret,
                    roof,
                    color,
                    new Vector3(
                        -width * 0.15f,
                        0.06f,
                        -0.66f),
                    "Italian-Ariete-M2",
                    true,
                    false);
                return;
            }
            if (id == "ariete_c1")
            {
                TankItalianFamilyDetails.AddMachineGun(
                    turret,
                    roof,
                    color,
                    new Vector3(
                        width * 0.17f,
                        0.06f,
                        -0.24f),
                    "Italian-ArieteC1-CommanderMAG",
                    false,
                    false);
                TankItalianFamilyDetails.AddMachineGun(
                    turret,
                    roof,
                    color,
                    new Vector3(
                        -width * 0.11f,
                        0.05f,
                        -0.62f),
                    "Italian-ArieteC1-LoaderMAG",
                    false,
                    false);
            }
        }

        private static void AddVisionRing(
            Transform turret,
            float x,
            float z,
            float roof,
            int count,
            string name)
        {
            for (int block = 0;
                block < count;
                block++)
            {
                float angle =
                    block * Mathf.PI * 2f / count;
                TankDetailGeometry.Part(
                    name,
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x +
                            Mathf.Cos(angle) * 0.22f,
                        roof + 0.095f,
                        z +
                            Mathf.Sin(angle) * 0.22f),
                    new Vector3(0.085f, 0.05f, 0.035f),
                    Lens());
            }
        }

        private static void AddGalixPlatforms(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Italian-Ariete-GalixPlatform",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.36f,
                        roof - 0.34f,
                        -0.3f),
                    new Vector3(0.2f, 0.08f, 0.68f),
                    color * 0.66f);
                TankDetailGeometry.Part(
                    "Italian-Ariete-GalixBacking",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.375f,
                        roof - 0.23f,
                        -0.3f),
                    new Vector3(0.025f, 0.2f, 0.64f),
                    Gunmetal());
            }
        }

        private static void AddRoofPanelCadence(
            Transform turret,
            float width,
            float roof)
        {
            for (int panel = 0;
                panel < 5;
                panel++)
            {
                TankDetailGeometry.Part(
                    "Italian-Ariete-RoofServicePanel",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.18f +
                            panel * width * 0.09f,
                        roof + 0.012f,
                        -1.25f +
                            (panel % 2) * 0.08f),
                    new Vector3(
                        width * 0.075f,
                        0.018f,
                        0.34f),
                    Gunmetal());
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
