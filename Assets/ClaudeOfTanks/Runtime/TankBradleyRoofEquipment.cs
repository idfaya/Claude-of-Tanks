using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankBradleyRoofEquipment
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            if (TankBradleyFamilyDetails.IsM3A3(
                    definition))
            {
                AddM3A3Equipment(
                    turret,
                    color);
                return;
            }

            AddA2CrewStations(
                turret,
                color);
            bool ukrainian =
                TankBradleyFamilyDetails
                    .IsUkrainian(definition);
            AddA2BustleRack(
                turret,
                color,
                ukrainian);
            AddSmokeBanks(
                turret,
                color,
                4,
                0.52f,
                0.42f,
                0.92f);
            AddAntennas(
                turret,
                color,
                0.78f,
                -1.25f,
                ukrainian
                    ? 0.98f
                    : 0.62f);
            if (ukrainian)
            {
                AddUkrainianRoofPackage(
                    turret,
                    color);
            }
            else
            {
                AddRoofMachineGun(
                    turret,
                    color,
                    0.15f,
                    0.72f,
                    -0.68f,
                    "Bradley-A2");
            }
        }
        private static void AddA2CrewStations(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-Bradley-A2IsuHood",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.32f, 0.545f, 0.14f),
                new Vector3(0.4f, 0.045f, 0.4f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Bradley-A2IsuLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.32f, 0.505f, 0.375f),
                new Vector3(0.28f, 0.05f, 0.02f),
                TankBradleyFamilyDetails.Lens());
            AddHatch(
                turret,
                color,
                0.38f,
                0.833f,
                0.02f,
                0.24f,
                "Bradley-A2CommanderHatch");
            AddHatch(
                turret,
                color,
                -0.4f,
                0.578f,
                -0.3f,
                0.22f,
                "Bradley-A2GunnerHatch");
            for (int scope = 0;
                scope < 3;
                scope++)
            {
                TankDetailGeometry.Part(
                    "Bradley-A2CommanderPeriscope",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0.24f +
                            scope * 0.14f,
                        0.845f,
                        0.24f),
                    new Vector3(0.07f, 0.04f, 0.05f),
                    TankBradleyFamilyDetails.Lens());
            }
        }
        private static void AddA2BustleRack(
            Transform turret,
            Color color,
            bool ukrainian)
        {
            float width =
                ukrainian ? 2.12f : 1.38f;
            float z =
                ukrainian ? -1.35f : -1.28f;
            for (int rail = 0;
                rail < 3;
                rail++)
            {
                TankDetailGeometry.Part(
                    "Bradley-A2BustleRackRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        0.47f +
                            rail * 0.12f,
                        z),
                    new Vector3(width, 0.025f, 0.42f),
                    TankBradleyFamilyDetails.Gunmetal());
            }
            for (int post = 0;
                post < 5;
                post++)
            {
                float x =
                    Mathf.Lerp(
                        -width * 0.5f,
                        width * 0.5f,
                        post / 4f);
                TankDetailGeometry.Part(
                    "Painted-Bradley-A2BustleRackPost",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(x, 0.59f, z),
                    new Vector3(0.035f, 0.3f, 0.38f),
                    color * 0.52f);
            }
            TankDetailGeometry.Part(
                "Painted-Bradley-A2BustleDuffel",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.58f, z),
                new Vector3(
                    width * 0.72f,
                    0.24f,
                    0.34f),
                color * 0.47f);
        }
        private static void AddUkrainianRoofPackage(
            Transform turret,
            Color color)
        {
            TankDetailGeometry.Part(
                "Painted-BradleyUA-MgPedestal",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.42f, 0.724f, -0.42f),
                new Vector3(0.23f, 0.159f, 0.23f),
                color * 0.6f);
            AddRoofMachineGun(
                turret,
                color,
                -0.42f,
                0.92f,
                -0.42f,
                "BradleyUA");
            TankDetailGeometry.Part(
                "Painted-BradleyUA-IsuPlinth",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.69f, 0.8125f, -0.02f),
                new Vector3(0.26f, 0.495f, 0.28f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "BradleyUA-IsuHead",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.69f, 1.13f, -0.02f),
                new Vector3(0.3f, 0.2f, 0.3f),
                TankBradleyFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "BradleyUA-IsuLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.69f, 1.13f, 0.138f),
                new Vector3(0.18f, 0.08f, 0.014f),
                TankBradleyFamilyDetails.Lens());
        }
        private static void AddM3A3Equipment(
            Transform turret,
            Color color)
        {
            AddHatch(
                turret,
                color,
                -0.31f,
                0.604f,
                -0.36f,
                0.235f,
                "BradleyM3-CommanderHatch");
            AddHatch(
                turret,
                color,
                0.34f,
                0.604f,
                -0.47f,
                0.215f,
                "BradleyM3-GunnerHatch");
            TankDetailGeometry.Part(
                "Painted-BradleyM3-CivPlinth",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.34f, 0.672f, -0.04f),
                new Vector3(0.34f, 0.224f, 0.28f),
                color * 0.61f);
            TankDetailGeometry.Part(
                "BradleyM3-CivDrum",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(-0.34f, 0.88f, -0.04f),
                new Vector3(0.15f, 0.12f, 0.15f),
                TankBradleyFamilyDetails.Dark());
            TankDetailGeometry.Part(
                "BradleyM3-CivLens",
                PrimitiveType.Cube,
                turret,
                new Vector3(-0.34f, 0.88f, 0.122f),
                new Vector3(0.16f, 0.08f, 0.02f),
                TankBradleyFamilyDetails.Lens());
            AddRoofMachineGun(
                turret,
                color,
                0.34f,
                0.688f,
                -0.47f,
                "BradleyM3-M2");
            AddRoofMachineGun(
                turret,
                color,
                -0.31f,
                0.688f,
                -0.36f,
                "BradleyM3-M240");
            Vector3[] serviceBins =
            {
                new Vector3(-0.62f, 0.656f, -0.76f),
                new Vector3(0.62f, 0.656f, -0.82f),
                new Vector3(-0.58f, 0.656f, 0.28f),
                new Vector3(0.58f, 0.656f, 0.16f)
            };
            for (int bin = 0;
                bin < serviceBins.Length;
                bin++)
            {
                TankDetailGeometry.Part(
                    "Painted-BradleyM3-RoofServiceBin",
                    PrimitiveType.Cube,
                    turret,
                    serviceBins[bin],
                    new Vector3(0.28f, 0.096f, 0.34f),
                    color * 0.54f);
            }
            AddM3BustleRack(
                turret,
                color);
            AddSmokeBanks(
                turret,
                color,
                4,
                0.76f,
                0.376f,
                0.3f);
            AddAntennas(
                turret,
                color,
                0.86f,
                -1.07f,
                0.7f);
        }
        private static void AddM3BustleRack(
            Transform turret,
            Color color)
        {
            for (int rail = 0;
                rail < 4;
                rail++)
            {
                TankDetailGeometry.Part(
                    "BradleyM3-BustleRackRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        0.48f +
                            rail * 0.09f,
                        -1.34f),
                    new Vector3(1.62f, 0.024f, 0.56f),
                    TankBradleyFamilyDetails.Gunmetal());
            }
            for (int post = 0;
                post < 5;
                post++)
            {
                TankDetailGeometry.Part(
                    "Painted-BradleyM3-BustleRackPost",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        Mathf.Lerp(
                            -0.8f,
                            0.8f,
                            post / 4f),
                        0.64f,
                        -1.34f),
                    new Vector3(0.035f, 0.32f, 0.52f),
                    color * 0.49f);
            }
            TankDetailGeometry.Part(
                "Painted-BradleyM3-BustleLoad",
                PrimitiveType.Cube,
                turret,
                new Vector3(0f, 0.65f, -1.24f),
                new Vector3(1.25f, 0.26f, 0.42f),
                color * 0.43f);
        }
        private static void AddHatch(
            Transform turret,
            Color color,
            float x,
            float y,
            float z,
            float radius,
            string name)
        {
            TankDetailGeometry.Part(
                "Painted-" + name,
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, y, z),
                new Vector3(radius, 0.035f, radius),
                color * 0.7f);
            TankDetailGeometry.Part(
                name + "Race",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, y + 0.04f, z),
                new Vector3(
                    radius * 0.82f,
                    0.012f,
                    radius * 0.82f),
                TankBradleyFamilyDetails.Dark());
        }
        private static void AddRoofMachineGun(
            Transform turret,
            Color color,
            float x,
            float y,
            float z,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-MgSocket",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, y, z),
                new Vector3(0.13f, 0.035f, 0.13f),
                color * 0.57f);
            TankDetailGeometry.Part(
                prefix + "-MgReceiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(x, y + 0.12f, z + 0.08f),
                new Vector3(0.18f, 0.16f, 0.38f),
                TankBradleyFamilyDetails.Gunmetal());
            Transform barrel =
                TankDetailGeometry.Part(
                    prefix + "-MgBarrel",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(x, y + 0.13f, z + 0.48f),
                    new Vector3(0.018f, 0.32f, 0.018f),
                    TankBradleyFamilyDetails.Dark());
            barrel.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-AmmoBox",
                PrimitiveType.Cube,
                turret,
                new Vector3(x + 0.17f, y + 0.09f, z + 0.04f),
                new Vector3(0.15f, 0.18f, 0.26f),
                color * 0.42f);
        }
        private static void AddSmokeBanks(
            Transform turret,
            Color color,
            int count,
            float x,
            float y,
            float z)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int tube = 0;
                    tube < count;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Bradley-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (x +
                                     tube * 0.04f),
                                y +
                                    tube * 0.02f,
                                z -
                                    tube * 0.055f),
                            new Vector3(0.038f, 0.12f, 0.038f),
                            color * 0.51f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            68f,
                            side * 45f,
                            side * 8f);
                }
            }
        }
        private static void AddAntennas(
            Transform turret,
            Color color,
            float x,
            float z,
            float height)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Bradley-AntennaPot",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * x,
                        0.78f,
                        z),
                    new Vector3(0.055f, 0.035f, 0.055f),
                    color * 0.5f);
                Transform whip =
                    TankDetailGeometry.Part(
                        "Bradley-AntennaWhip",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            side * x,
                            0.815f +
                                height * 0.5f,
                            z),
                        new Vector3(
                            0.009f,
                            height * 0.5f,
                            0.009f),
                        TankBradleyFamilyDetails.Dark());
                whip.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        side * 2f);
            }
        }
    }
}
