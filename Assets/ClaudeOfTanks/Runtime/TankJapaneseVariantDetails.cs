using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankJapaneseVariantDetails
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
            TankJapanesePrimaryDetails.BuildTurretAndGun(
                turret,
                definition,
                color,
                width,
                roof);
            switch (definition.id)
            {
                case "stb1":
                    AddCastGeneration(
                        turret,
                        color,
                        width,
                        roof,
                        true);
                    break;
                case "type74":
                    AddCastGeneration(
                        turret,
                        color,
                        width,
                        roof,
                        false);
                    break;
                case "type90":
                case "type90a":
                    AddType90Generation(
                        turret,
                        definition.id,
                        color,
                        width,
                        roof);
                    break;
                case "type10":
                case "type10b":
                    AddType10Generation(
                        turret,
                        definition.id,
                        color,
                        width,
                        roof);
                    break;
            }
        }

        private static void AddCastGeneration(
            Transform turret,
            Color color,
            float width,
            float roof,
            bool prototype)
        {
            string prefix = prototype
                ? "Japanese-STB1"
                : "Japanese-Type74";
            float searchlightX =
                (prototype ? 1f : -1f) *
                width * 0.2f;
            float searchlightZ =
                prototype ? 1.04f : 1.19f;
            Vector3 searchlightSize =
                prototype
                    ? new Vector3(0.54f, 0.4f, 0.38f)
                    : new Vector3(0.44f, 0.36f, 0.42f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-SearchlightHousing",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    searchlightX,
                    roof - 0.17f,
                    searchlightZ),
                searchlightSize,
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-SearchlightFace",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    searchlightX,
                    roof - 0.17f,
                    searchlightZ +
                        searchlightSize.z * 0.52f),
                new Vector3(
                    searchlightSize.x * 0.82f,
                    searchlightSize.y * 0.78f,
                    0.025f),
                Gunmetal());
            int panes = prototype ? 6 : 1;
            for (int pane = 0;
                pane < panes;
                pane++)
            {
                int row = prototype
                    ? pane / 2
                    : 0;
                int column = prototype
                    ? pane % 2
                    : 0;
                TankDetailGeometry.Part(
                    prefix + "-SearchlightLens",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        searchlightX +
                            (column - 0.5f) *
                            (prototype ? 0.18f : 0f),
                        roof - 0.27f +
                            row *
                            (prototype ? 0.11f : 0f),
                        searchlightZ +
                            searchlightSize.z * 0.56f),
                    prototype
                        ? new Vector3(0.16f, 0.08f, 0.014f)
                        : new Vector3(0.3f, 0.2f, 0.014f),
                    Lens());
            }
            AddCastVisionBlocks(
                turret,
                width,
                roof,
                prefix,
                prototype);
            TankJapaneseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    prototype ? 0.02f : width * 0.21f,
                    roof + 0.1f,
                    prototype ? 0.06f : 0.1f),
                prototype
                    ? new Vector3(0.22f, 0.19f, 0.2f)
                    : new Vector3(0.23f, 0.17f, 0.2f),
                prefix + "-RoofSight");
            TankJapaneseFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.14f,
                    0.08f,
                    -0.42f),
                prefix + "-CommanderMG",
                !prototype,
                false);
            if (prototype)
                AddStbFlankVentilation(
                    turret,
                    color,
                    width,
                    roof);
        }

        private static void AddCastVisionBlocks(
            Transform turret,
            float width,
            float roof,
            string prefix,
            bool prototype)
        {
            int blocks = prototype ? 14 : 6;
            for (int block = 0;
                block < blocks;
                block++)
            {
                int ringCount =
                    prototype && block >= 8
                        ? 6
                        : prototype
                            ? 8
                            : 6;
                int ringIndex =
                    prototype && block >= 8
                        ? block - 8
                        : block;
                float angle =
                    ringIndex *
                    Mathf.PI * 2f /
                    ringCount;
                float centerX =
                    prototype && block >= 8
                        ? -width * 0.135f
                        : width * 0.135f;
                float centerZ =
                    prototype && block >= 8
                        ? -0.12f
                        : -0.4f;
                TankDetailGeometry.Part(
                    prefix + "-CupolaVision",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        centerX +
                            Mathf.Cos(angle) * 0.25f,
                        roof + 0.095f,
                        centerZ +
                            Mathf.Sin(angle) * 0.25f),
                    new Vector3(0.085f, 0.055f, 0.026f),
                    Lens());
            }
        }

        private static void AddStbFlankVentilation(
            Transform turret,
            Color color,
            float width,
            float roof)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int cell = 0;
                    cell < 4;
                    cell++)
                {
                    TankDetailGeometry.Part(
                        "Japanese-STB1-FlankVent",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * width * 0.39f,
                            roof - 0.24f,
                            0.25f -
                                cell * 0.22f),
                        new Vector3(0.04f, 0.24f, 0.16f),
                        color * 0.32f);
                }
            }
        }

        private static void AddType90Generation(
            Transform turret,
            string id,
            Color color,
            float width,
            float roof)
        {
            TankJapaneseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.08f,
                    roof + 0.12f,
                    1.12f),
                new Vector3(0.35f, 0.24f, 0.56f),
                "Japanese-Type90-GunnerSight");
            TankJapaneseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    width * 0.02f,
                    roof + 0.18f,
                    0.48f),
                new Vector3(0.28f, 0.34f, 0.3f),
                "Japanese-Type90-CommanderSight");
            for (int block = 0;
                block < 4;
                block++)
            {
                float angle =
                    (130f + block * 30f) *
                    Mathf.Deg2Rad;
                TankDetailGeometry.Part(
                    "Japanese-Type90-CupolaVision",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        width * 0.16f +
                            Mathf.Cos(angle) * 0.2f,
                        roof + 0.1f,
                        -0.17f +
                            Mathf.Sin(angle) * 0.2f),
                    new Vector3(0.07f, 0.08f, 0.07f),
                    Lens());
            }
            TankJapaneseFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    -width * 0.05f,
                    0.04f,
                    -0.46f),
                "Japanese-Type90-M2",
                true,
                true);
            if (id != "type90a") return;
            TankJapaneseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.16f,
                    roof + 0.19f,
                    -0.18f),
                new Vector3(0.38f, 0.34f, 0.35f),
                "Type90A-PanoramicThermal");
        }

        private static void AddType10Generation(
            Transform turret,
            string id,
            Color color,
            float width,
            float roof)
        {
            bool kai = id == "type10b";
            TankJapaneseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    kai ? -width * 0.05f : width * 0.2f,
                    roof + (kai ? 0.22f : 0.14f),
                    kai ? 1.2f : 1.16f),
                kai
                    ? new Vector3(0.57f, 0.46f, 0.62f)
                    : new Vector3(0.32f, 0.28f, 0.34f),
                kai
                    ? "Japanese-Type10B-GunnerSight"
                    : "Japanese-Type10-GunnerSight");
            TankJapaneseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.08f,
                    roof + 0.38f,
                    0.25f),
                new Vector3(0.26f, 0.42f, 0.36f),
                "Japanese-Type10-PanoramicSight");
            AddType10CupolaVision(
                turret,
                width,
                roof);
            if (!kai)
            {
                TankJapaneseFamilyDetails.AddMachineGun(
                    turret,
                    roof,
                    color,
                    new Vector3(
                        width * 0.36f,
                        0.03f,
                        -0.06f),
                    "Japanese-Type10-M2",
                    true,
                    false);
            }
        }

        private static void AddType10CupolaVision(
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
                    "Japanese-Type10-CupolaVision",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.14f +
                            Mathf.Cos(angle) * 0.22f,
                        roof + 0.12f,
                        -0.57f +
                            Mathf.Sin(angle) * 0.22f),
                    new Vector3(0.09f, 0.055f, 0.05f),
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
