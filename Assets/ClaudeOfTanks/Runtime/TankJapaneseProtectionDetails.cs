using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankJapaneseProtectionDetails
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
            if (definition.id == "type90a")
            {
                AddType90APackage(
                    root,
                    turret,
                    definition,
                    color,
                    width,
                    height,
                    length);
            }
            else if (definition.id == "type10b")
            {
                AddType10BPackage(
                    turret,
                    definition,
                    color,
                    width);
            }
        }

        private static void AddType90APackage(
            Transform root,
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width,
            float height,
            float length)
        {
            float turretRoof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float turretFront =
                TankDetailGeometry.TurretFrontZ(
                    definition);
            float turretHalfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.35f);
            float hullRoof =
                TankDetailGeometry.HullRoofY(
                    definition,
                    height * 0.58f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                Transform carrier =
                    TankDetailGeometry.Part(
                        "Painted-Type90A-CheekCarrier",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * turretHalfWidth * 0.58f,
                            turretRoof - 0.3f,
                            turretFront * 0.45f),
                        new Vector3(
                            turretHalfWidth * 0.86f,
                            0.54f,
                            turretFront * 0.68f),
                        color * 0.74f);
                carrier.localRotation =
                    Quaternion.Euler(
                        -5f,
                        side * -12f,
                        side * -2f);
                for (int cassette = 0;
                    cassette < 4;
                    cassette++)
                {
                    Transform item =
                        TankDetailGeometry.Part(
                            "Painted-Type90A-CheekCassette",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side *
                                    turretHalfWidth *
                                    (0.42f +
                                     cassette * 0.16f),
                                turretRoof -
                                    0.08f -
                                    cassette * 0.018f,
                                turretFront *
                                    (0.52f -
                                     cassette * 0.11f)),
                            new Vector3(0.22f, 0.18f, 0.21f),
                            color * 0.8f);
                    item.localRotation =
                        Quaternion.Euler(
                            -8f,
                            side *
                                (3f +
                                 cassette * 3f),
                            side);
                }
                for (int module = 0;
                    module < 6;
                    module++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Type90A-HullServiceModule",
                        PrimitiveType.Cube,
                        root,
                        new Vector3(
                            side * width * 0.495f,
                            hullRoof - 0.31f,
                            length * 0.24f -
                                module * length * 0.095f),
                        new Vector3(
                            0.06f,
                            0.4f,
                            length * 0.082f),
                        color * 0.72f);
                }
                for (int module = 0;
                    module < 5;
                    module++)
                {
                    TankDetailGeometry.Part(
                        "Painted-Type90A-TurretFlankModule",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * turretHalfWidth * 1.02f,
                            turretRoof - 0.27f,
                            0.35f -
                                module * 0.38f),
                        new Vector3(0.06f, 0.26f, 0.3f),
                        color * 0.68f);
                }
                TankJapaneseFamilyDetails.AddSight(
                    turret,
                    color,
                    new Vector3(
                        side * turretHalfWidth * 0.91f,
                        turretRoof + 0.03f,
                        turretFront * 0.25f),
                    new Vector3(0.2f, 0.16f, 0.22f),
                    "Type90A-APSHead");
            }
            AddRemoteWeaponStation(
                turret,
                turretRoof,
                color,
                new Vector3(
                    width * 0.15f,
                    0.08f,
                    -0.48f),
                "Type90A-RWS",
                false);
            AddGunMask(
                turret,
                color,
                "Type90A");
        }

        private static void AddType10BPackage(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                string prefix = side < 0
                    ? "Type10B-LeftEO"
                    : "Type10B-RightEO";
                Vector3 center =
                    side < 0
                        ? new Vector3(
                            -width * 0.155f,
                            roof + 0.12f,
                            -0.24f)
                        : new Vector3(
                            width * 0.216f,
                            roof + 0.1f,
                            0.11f);
                TankJapaneseFamilyDetails.AddSight(
                    turret,
                    color,
                    center,
                    side < 0
                        ? new Vector3(0.5f, 0.36f, 0.48f)
                        : new Vector3(0.37f, 0.28f, 0.34f),
                    prefix);
            }
            AddRemoteWeaponStation(
                turret,
                roof,
                color,
                new Vector3(
                    width * 0.13f,
                    0.06f,
                    -0.64f),
                "Type10B-RWS",
                true);
            AddKaiBasket(
                turret,
                definition,
                color,
                width);
            AddGunMask(
                turret,
                color,
                "Type10B");
        }

        private static void AddRemoteWeaponStation(
            Transform turret,
            float roof,
            Color color,
            Vector3 seat,
            string prefix,
            bool compact)
        {
            float scale = compact ? 0.86f : 1f;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Base",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y,
                    seat.z),
                new Vector3(
                    0.2f * scale,
                    0.065f,
                    0.2f * scale),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Receiver",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.22f,
                    seat.z + 0.08f),
                new Vector3(
                    0.3f * scale,
                    0.18f,
                    0.38f * scale),
                color * 0.6f);
            TankDetailGeometry.Part(
                prefix + "-Barrel",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x,
                    roof + seat.y + 0.23f,
                    seat.z + 0.54f),
                new Vector3(
                    0.032f,
                    0.032f,
                    0.68f * scale),
                Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-Sensor",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x - 0.2f * scale,
                    roof + seat.y + 0.23f,
                    seat.z + 0.03f),
                new Vector3(
                    0.13f,
                    0.16f,
                    0.14f),
                Sensor());
            TankDetailGeometry.Part(
                prefix + "-Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    seat.x - 0.2f * scale,
                    roof + seat.y + 0.23f,
                    seat.z + 0.105f),
                new Vector3(0.075f, 0.075f, 0.014f),
                Lens());
        }

        private static void AddKaiBasket(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float rear =
                TankDetailGeometry.TurretRearZ(
                    definition);
            Color rail = color * 0.38f;
            for (int row = 0;
                row < 3;
                row++)
            {
                TankDetailGeometry.Part(
                    "Type10B-KaiBasketRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        0f,
                        roof - 0.43f +
                            row * 0.16f,
                        rear - 0.78f),
                    new Vector3(
                        width * 0.78f,
                        0.03f,
                        0.03f),
                    rail);
            }
            for (int post = 0;
                post < 8;
                post++)
            {
                TankDetailGeometry.Part(
                    "Type10B-KaiBasketRail",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        -width * 0.35f +
                            post * width * 0.1f,
                        roof - 0.27f,
                        rear - 0.78f),
                    new Vector3(0.03f, 0.36f, 0.03f),
                    rail);
            }
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                TankDetailGeometry.Part(
                    "Type10B-KaiBasketJoin",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * width * 0.35f,
                        roof - 0.42f,
                        rear - 0.38f),
                    new Vector3(0.03f, 0.03f, 0.8f),
                    rail);
            }
        }

        private static void AddGunMask(
            Transform turret,
            Color color,
            string prefix)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-GunMask",
                PrimitiveType.Cube,
                gun,
                new Vector3(0f, 0f, 0.26f),
                prefix == "Type10B"
                    ? new Vector3(0.76f, 0.48f, 0.3f)
                    : new Vector3(0.92f, 0.44f, 0.28f),
                color * 0.66f);
            Transform collar =
                TankDetailGeometry.Part(
                    prefix + "-GunMaskCollar",
                    PrimitiveType.Cylinder,
                    gun,
                    new Vector3(0f, 0f, 0.58f),
                    new Vector3(0.2f, 0.2f, 0.2f),
                    Gunmetal());
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static Color Gunmetal()
        {
            return new Color(0.08f, 0.09f, 0.08f);
        }

        private static Color Sensor()
        {
            return new Color(0.1f, 0.12f, 0.1f);
        }

        private static Color Lens()
        {
            return new Color(0.025f, 0.14f, 0.17f);
        }
    }
}
