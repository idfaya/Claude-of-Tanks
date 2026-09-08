using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMerkavaVariantDetails
    {
        public static void Build(
            Transform turret, VehicleDefinition definition,
            Color color, float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            AddHatches(
                turret,
                definition,
                color,
                width,
                roof);
            AddOptics(
                turret,
                definition,
                color,
                roof);
            TankMerkavaFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.15f,
                    0.06f,
                    -0.1f),
                "Merkava-CommanderMAG",
                false);

            switch (definition.id)
            {
                case "merkava1b":
                case "merkava2b":
                case "merkava2d":
                    AddEarlyGeneration(
                        turret,
                        definition,
                        color,
                        width,
                        roof);
                    break;
                case "merkava3c":
                case "merkava3d":
                    AddThirdGeneration(
                        turret,
                        definition,
                        color,
                        width,
                        roof);
                    break;
                case "merkava4b":
                    AddFourthGeneration(
                        turret,
                        definition,
                        color,
                        width,
                        roof);
                    break;
            }
        }
        private static void AddHatches(
            Transform turret, VehicleDefinition definition,
            Color color, float width, float roof)
        {
            float commanderX =
                definition.id == "merkava4b"
                    ? -width * 0.17f
                    : definition.id == "merkava3d"
                        ? width * 0.11f
                        : width * 0.2f;
            float loaderX =
                definition.id == "merkava4b"
                    ? width * 0.19f
                    : -commanderX * 0.78f;
            float commanderZ =
                definition.id.StartsWith("merkava3")
                    ? -1.5f
                    : -0.82f;
            float loaderZ =
                definition.id.StartsWith("merkava3")
                    ? -1.55f
                    : -1.42f;
            AddHatch(
                turret,
                color,
                commanderX,
                roof,
                commanderZ,
                width * 0.1f);
            AddHatch(
                turret,
                color,
                loaderX,
                roof,
                loaderZ,
                width * 0.085f);
        }
        private static void AddHatch(
            Transform turret, Color color, float x,
            float roof, float z, float radius)
        {
            TankDetailGeometry.Part(
                "Painted-Merkava-Hatch",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    x,
                    roof + 0.035f,
                    z),
                new Vector3(
                    radius,
                    0.04f,
                    radius),
                color * 0.82f);
            for (int scope = 0;
                scope < 5;
                scope++)
            {
                float angle = Mathf.Lerp(
                    -55f,
                    55f,
                    scope / 4f) *
                    Mathf.Deg2Rad;
                TankDetailGeometry.Part(
                    "Merkava-HatchPeriscope",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x + Mathf.Sin(angle) *
                            radius * 0.72f,
                        roof + 0.082f,
                        z + Mathf.Cos(angle) *
                            radius * 0.72f),
                    new Vector3(
                        0.06f,
                        0.045f,
                        0.045f),
                    Lens());
            }
        }
        private static void AddOptics(
            Transform turret, VehicleDefinition definition,
            Color color, float roof)
        {
            float side =
                definition.id == "merkava3c" ||
                definition.id == "merkava2b"
                    ? -1f
                    : 1f;
            float x = side *
                (definition.id.StartsWith("merkava3")
                    ? 0.72f
                    : 0.62f);
            TankDetailGeometry.Part(
                "Painted-Merkava-GunnerSight",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.12f,
                    -0.28f),
                new Vector3(
                    0.27f,
                    0.22f,
                    0.26f),
                color * 0.72f);
            TankDetailGeometry.Part(
                "Merkava-GunnerSightLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x,
                    roof + 0.13f,
                    -0.143f),
                new Vector3(
                    0.15f,
                    0.09f,
                    0.014f),
                Lens());
            if (definition.id == "merkava1b" ||
                definition.id == "merkava2b" ||
                definition.id == "merkava2d")
            {
                return;
            }
            float panoramicX =
                definition.id == "merkava3d"
                    ? -0.7f
                    : 0.64f;
            TankDetailGeometry.Part(
                "Painted-Merkava-PanoramicSight",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    panoramicX,
                    roof + 0.18f,
                    -1.02f),
                new Vector3(
                    0.14f,
                    0.26f,
                    0.14f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Merkava-PanoramicLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    panoramicX,
                    roof + 0.2f,
                    -0.873f),
                new Vector3(
                    0.12f,
                    0.09f,
                    0.014f),
                Lens());
        }

        private static void AddEarlyGeneration(
            Transform turret, VehicleDefinition definition,
            Color color, float width, float roof)
        {
            TankMerkavaFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.14f,
                    0.04f,
                    -0.78f),
                "Merkava-LoaderMAG",
                false);
            TankMerkavaFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    0.12f,
                    0.03f,
                    -0.42f),
                "Merkava-AuxiliaryMAG",
                false);
            AddGunCradleMachineGun(
                turret,
                color,
                "Merkava-GunCradleM2");
            TankDetailGeometry.Part(
                "Painted-Merkava-60mmMortarLid",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    -width * 0.08f,
                    roof + 0.025f,
                    -0.2f),
                new Vector3(
                    0.12f,
                    0.03f,
                    0.12f),
                color * 0.78f);

            int packs =
                definition.id == "merkava1b"
                    ? 3
                    : definition.id == "merkava2b"
                        ? 4
                        : 5;
            AddRoofPacks(
                turret,
                color,
                roof,
                packs,
                "Merkava-Early");
            if (definition.id == "merkava2d")
            {
                TankDetailGeometry.Part(
                    "Painted-Merkava2D-SightShoe",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -0.48f,
                        roof + 0.055f,
                        0.35f),
                    new Vector3(
                        0.28f,
                        0.1f,
                        0.24f),
                    color * 0.75f);
            }
        }

        private static void AddThirdGeneration(
            Transform turret, VehicleDefinition definition,
            Color color, float width, float roof)
        {
            TankMerkavaFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.2f,
                    0.04f,
                    -1.2f),
                "Merkava-LoaderMAG",
                false);
            TankMerkavaFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    0.12f,
                    0.03f,
                    -0.62f),
                "Merkava-CenterlineM2",
                true);
            TankMerkavaFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.24f,
                    0.03f,
                    -0.84f),
                "Merkava-PlinthM2",
                true);
            AddRoofPacks(
                turret,
                color,
                roof,
                definition.id == "merkava3d"
                    ? 7
                    : 6,
                definition.id == "merkava3d"
                    ? "Merkava3D"
                    : "Merkava3C");
            if (definition.id == "merkava3d")
            {
                for (int ring = 0;
                    ring < 2;
                    ring++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Merkava3D-WideHatchCollar",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            ring == 0
                                ? 0.4f
                                : -0.33f,
                            roof + 0.018f,
                            ring == 0
                                ? -1.5f
                                : -1.52f),
                        new Vector3(
                            ring == 0
                                ? 0.34f
                                : 0.32f,
                            0.02f,
                            ring == 0
                                ? 0.34f
                                : 0.32f),
                        color * 0.78f);
                }
            }
        }

        private static void AddFourthGeneration(
            Transform turret, VehicleDefinition definition,
            Color color, float width, float roof)
        {
            TankMerkavaFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.17f,
                    0.08f,
                    -0.92f),
                "Merkava4B-RoofMAG",
                false);
            AddGunCradleMachineGun(
                turret,
                color,
                "Merkava4B-GunCradleM2");
            for (int rackCase = 0;
                rackCase < 3;
                rackCase++)
            {
                float x = Mathf.Lerp(
                    -0.8f,
                    0.82f,
                    rackCase / 2f);
                TankDetailGeometry.Part(
                    "Painted-Merkava4B-RoofCase",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        x,
                        roof + 0.075f,
                        -1.72f -
                            rackCase * 0.06f),
                    new Vector3(
                        0.3f +
                            (rackCase % 2) * 0.1f,
                        0.12f,
                        0.34f),
                    color * 0.7f);
            }
            for (int pack = 0;
                pack < 5;
                pack++)
            {
                TankDetailGeometry.Part(
                    "Painted-Merkava4B-BustlePack",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        Mathf.Lerp(
                            -0.92f,
                            0.98f,
                            pack / 4f),
                        roof - 0.18f +
                            (pack % 2) * 0.04f,
                        -2.45f -
                            (pack % 3) * 0.13f),
                    new Vector3(
                        0.28f +
                            (pack % 2) * 0.08f,
                        0.22f +
                            (pack % 3) * 0.025f,
                        0.34f),
                    color * 0.62f);
            }
        }

        private static void AddGunCradleMachineGun(
            Transform turret, Color color, string prefix)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            TankMerkavaFamilyDetails.AddMachineGun(
                gun,
                0f,
                color,
                new Vector3(
                    0.12f,
                    0.34f,
                    -0.22f),
                prefix,
                true);
        }

        private static void AddRoofPacks(
            Transform turret, Color color, float roof,
            int count, string prefix)
        {
            for (int pack = 0;
                pack < count;
                pack++)
            {
                int side = pack % 2 == 0
                    ? -1
                    : 1;
                int lane = pack / 2;
                TankDetailGeometry.Part(
                    "Painted-" + prefix +
                    "-RoofPack",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side *
                            (0.42f +
                             lane * 0.16f),
                        roof + 0.065f +
                            (pack % 3) * 0.012f,
                        -1.85f -
                            lane * 0.27f),
                    new Vector3(
                        0.28f +
                            (pack % 3) * 0.04f,
                        0.13f,
                        0.28f +
                            ((pack + 1) % 3) *
                            0.04f),
                    color * (0.62f +
                        pack * 0.018f));
            }
        }

        private static Color Lens()
        {
            return new Color(0.04f, 0.18f, 0.2f);
        }
    }
}
