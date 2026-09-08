using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankMerkavaFamilyDetails
    {
        public static bool Supports(string id)
        {
            switch (id)
            {
                case "merkava1b":
                case "merkava2b":
                case "merkava2d":
                case "merkava3c":
                case "merkava3d":
                case "merkava4b":
                    return true;
                default:
                    return false;
            }
        }

        public static void Build(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            if (!Supports(definition?.id)) return;

            AddRearServiceField(
                root,
                definition,
                color,
                width,
                height,
                length);
            AddSmokeBanks(
                turret,
                definition,
                color,
                width);
            AddAntennas(
                turret,
                definition,
                width);
            TankMerkavaProtectionDetails.Build(
                turret,
                definition,
                color,
                width);
            TankMerkavaVariantDetails.Build(
                turret,
                definition,
                color,
                width);
        }

        internal static void AddMachineGun(
            Transform parent,
            float roof,
            Color color,
            Vector3 seat,
            string prefix,
            bool heavy)
        {
            float receiverWidth = heavy ? 0.19f : 0.15f;
            float receiverLength = heavy ? 0.4f : 0.32f;
            float barrelLength = heavy ? 0.9f : 0.7f;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Mount",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(0.12f, 0.045f, 0.12f),
                color * 0.7f);
            TankDetailGeometry.Part(
                prefix + "-Receiver",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.13f,
                    seat.z + 0.08f),
                new Vector3(
                    receiverWidth,
                    heavy ? 0.14f : 0.11f,
                    receiverLength),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-AmmoBox",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x - receiverWidth * 0.78f,
                    roof + seat.y + 0.12f,
                    seat.z + 0.02f),
                new Vector3(
                    receiverWidth * 0.72f,
                    0.13f,
                    receiverLength * 0.58f),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                parent,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.14f,
                    seat.z +
                        receiverLength * 0.5f +
                        barrelLength * 0.5f),
                new Vector3(
                    heavy ? 0.032f : 0.024f,
                    heavy ? 0.032f : 0.024f,
                    barrelLength),
                Gunmetal());
        }

        private static void AddRearServiceField(
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
            float rear =
                TankDetailGeometry.HullRearZ(
                    definition,
                    -length * 0.5f);
            float doorY = Mathf.Max(
                height * 0.34f,
                roof - 0.48f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float x = side * width * 0.23f;
                TankDetailGeometry.Part(
                    "Merkava-RearServiceGrille",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        x,
                        roof - 0.17f,
                        rear - 0.026f),
                    new Vector3(
                        width * 0.34f,
                        0.22f,
                        0.035f),
                    color * 0.34f);
                for (int slat = 0;
                    slat < 5;
                    slat++)
                {
                    TankDetailGeometry.Part(
                        "Merkava-RearServiceSlat",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            x,
                            roof - 0.25f +
                                slat * 0.04f,
                            rear - 0.05f),
                        new Vector3(
                            width * 0.29f,
                            0.012f,
                            0.018f),
                        Gunmetal());
                }
                TankDetailGeometry.Part(
                    "Painted-Merkava-ClamshellDoor",
                    PrimitiveType.Cube,
                    root,
                    new Vector3(
                        side * width * 0.105f,
                        doorY,
                        rear - 0.035f),
                    new Vector3(
                        width * 0.2f,
                        0.5f,
                        0.045f),
                    color * 0.78f);
                for (int hinge = -1;
                    hinge <= 1;
                    hinge += 2)
                {
                    Transform part =
                        TankDetailGeometry.Part(
                            "Merkava-DoorHinge",
                            PrimitiveType.Cylinder,
                            root,
                            new Vector3(
                                side * width * 0.2f,
                                doorY +
                                    hinge * 0.15f,
                                rear - 0.065f),
                            new Vector3(
                                0.035f,
                                0.09f,
                                0.035f),
                            Gunmetal());
                    part.localRotation =
                        Quaternion.Euler(
                            90f,
                            0f,
                            0f);
                }
                TankDetailGeometry.Part(
                    "Merkava-RearMarker",
                    PrimitiveType.Cylinder,
                    root,
                    new Vector3(
                        side * width * 0.34f,
                        roof - 0.16f,
                        rear - 0.06f),
                    new Vector3(0.045f, 0.025f, 0.045f),
                    new Color(0.55f, 0.06f, 0.025f))
                    .localRotation =
                    Quaternion.Euler(
                        90f,
                        0f,
                        0f);
                Transform towEye =
                    TankDetailGeometry.Part(
                        "Merkava-RearTowEye",
                        PrimitiveType.Cylinder,
                        root,
                        new Vector3(
                            side * width * 0.25f,
                            doorY - 0.3f,
                            rear - 0.07f),
                        new Vector3(0.07f, 0.035f, 0.07f),
                        Gunmetal());
                towEye.localRotation =
                    Quaternion.Euler(
                        90f,
                        0f,
                        0f);
            }
            TankDetailGeometry.Part(
                "Merkava-DoorLatch",
                PrimitiveType.Cube,
                root,
                new Vector3(
                    0f,
                    doorY,
                    rear - 0.07f),
                new Vector3(
                    0.08f,
                    0.22f,
                    0.04f),
                Gunmetal());
        }

        private static void AddSmokeBanks(
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
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.35f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Painted-Merkava-SmokeBankShoe",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * (halfWidth - 0.14f),
                        roof - 0.17f,
                        front * 0.24f),
                    new Vector3(
                        0.3f,
                        0.08f,
                        0.22f),
                    color * 0.68f);
                for (int tube = 0;
                    tube < 6;
                    tube++)
                {
                    Transform launcher =
                        TankDetailGeometry.Part(
                            "Painted-Merkava-SmokeLauncher",
                            PrimitiveType.Cylinder,
                            turret,
                            new Vector3(
                                side *
                                    (halfWidth -
                                     0.16f +
                                     (tube % 3) *
                                     0.075f),
                                roof -
                                    0.12f +
                                    (tube / 3) *
                                    0.08f,
                                front * 0.24f -
                                    (tube % 3) *
                                    0.07f),
                            new Vector3(
                                0.038f,
                                0.14f,
                                0.038f),
                            color * 0.55f);
                    launcher.localRotation =
                        Quaternion.Euler(
                            66f,
                            0f,
                            side * 22f);
                }
            }
        }

        private static void AddAntennas(
            Transform turret,
            VehicleDefinition definition,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            int count = definition.id == "merkava3d"
                ? 1
                : 2;
            for (int antenna = 0;
                antenna < count;
                antenna++)
            {
                int side = antenna == 0 ? -1 : 1;
                float height =
                    definition.id == "merkava3c"
                        ? 1.5f
                        : definition.id == "merkava4b"
                            ? 0.9f
                            : 1.05f;
                TankDetailGeometry.Part(
                    "Merkava-AntennaBase",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.24f,
                        roof + 0.055f,
                        rear + 0.32f +
                            antenna * 0.18f),
                    new Vector3(
                        0.07f,
                        0.08f,
                        0.07f),
                    Gunmetal());
                TankDetailGeometry.Part(
                    "Merkava-Antenna",
                    PrimitiveType.Cylinder,
                    turret,
                    new Vector3(
                        side * width * 0.24f,
                        roof + height * 0.5f,
                        rear + 0.32f +
                            antenna * 0.18f),
                    new Vector3(
                        0.01f,
                        height,
                        0.01f),
                    Gunmetal());
            }
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.075f);
        }
    }
}
