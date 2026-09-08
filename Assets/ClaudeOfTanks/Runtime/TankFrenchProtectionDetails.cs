using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankFrenchProtectionDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            string id = definition.id;
            if (id == "amx30" ||
                id == "amx30b2")
            {
                AddAmx30GunPlant(
                    turret,
                    id,
                    color);
                return;
            }
            if (id == "amx40")
            {
                AddAmx40ExternalPackage(
                    turret,
                    definition,
                    color,
                    width);
                AddAmx40GunPlant(
                    turret,
                    color);
                return;
            }
            AddLeclercGunPlant(
                turret,
                color,
                id);
            if (id == "leclerc_xlr")
                AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    -0.67f,
                    -0.33f,
                    "French-LeclercXLR-RWS",
                    false);
            else if (id == "amx56")
                AddRemoteWeaponStation(
                    turret,
                    definition,
                    color,
                    -0.67f,
                    -0.33f,
                    "French-AMX56-RWS",
                    true);
        }

        private static void AddAmx30GunPlant(
            Transform turret,
            string id,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                CreateGunFittingsRoot(
                    gun,
                    "French-AMX30-GunFittings");
            TankDetailGeometry.Part(
                "Painted-French-AMX30-GunMask",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0.02f, 0.08f),
                new Vector3(0.86f, 0.56f, 0.28f),
                color * 0.66f);
            Transform collar =
                TankDetailGeometry.Part(
                    "French-AMX30-GunMaskCollar",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, 0.32f),
                    new Vector3(0.16f, 0.2f, 0.16f),
                    Gunmetal());
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            AddCoax(
                fittings,
                -0.315f,
                0.075f,
                0.65f,
                1.3f,
                "French-AMX30-M693");
            AddGunSight(
                fittings,
                color,
                new Vector3(-0.76f, 0.16f, 0.2f),
                new Vector3(0.46f, 0.4f, 0.26f),
                "French-AMX30-PH8B");
            if (id == "amx30b2")
            {
                AddGunSight(
                    fittings,
                    color,
                    new Vector3(0.5f, 0.14f, 0.22f),
                    new Vector3(0.28f, 0.3f, 0.28f),
                    "French-AMX30B2-LLLTV");
            }
        }

        private static void AddAmx40ExternalPackage(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            float halfWidth =
                TankDetailGeometry.TurretHalfWidth(
                    definition,
                    width * 0.35f);
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                for (int panel = 0;
                    panel < 3;
                    panel++)
                {
                    Transform item =
                        TankDetailGeometry.Part(
                            "Painted-AMX40-FlankPanel",
                            PrimitiveType.Cube,
                            turret,
                            new Vector3(
                                side * halfWidth * 0.98f,
                                roof - 0.33f,
                                0.42f -
                                    panel * 0.54f),
                            new Vector3(0.07f, 0.42f, 0.46f),
                            color * 0.72f);
                    item.localRotation =
                        Quaternion.Euler(
                            0f,
                            side * (4f +
                                panel * 2f),
                            side * -2f);
                    TankDetailGeometry.Part(
                        "AMX40-FlankPanelSupport",
                        PrimitiveType.Cube,
                        turret,
                        new Vector3(
                            side * halfWidth * 0.92f,
                            roof - 0.33f,
                            0.42f -
                                panel * 0.54f),
                        new Vector3(0.1f, 0.08f, 0.34f),
                        color * 0.38f);
                }
                TankDetailGeometry.Part(
                    "Painted-AMX40-CheekTie",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(
                        side * halfWidth * 0.72f,
                        roof - 0.2f,
                        0.94f),
                    new Vector3(
                        halfWidth * 0.5f,
                        0.22f,
                        0.28f),
                    color * 0.7f);
            }
        }

        private static void AddAmx40GunPlant(
            Transform turret,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                CreateGunFittingsRoot(
                    gun,
                    "French-AMX40-GunFittings");
            TankDetailGeometry.Part(
                "Painted-French-AMX40-GunMask",
                PrimitiveType.Cube,
                fittings,
                new Vector3(-0.04f, 0.02f, 0.1f),
                new Vector3(1.22f, 0.48f, 0.4f),
                color * 0.65f);
            Transform collar =
                TankDetailGeometry.Part(
                    "French-AMX40-GunMaskCollar",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(-0.04f, 0.02f, 0.38f),
                    new Vector3(0.22f, 0.2f, 0.22f),
                    Gunmetal());
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            AddCoax(
                fittings,
                0.39f,
                0f,
                0.65f,
                0.48f,
                "French-AMX40-Coax20");
            AddGunSight(
                fittings,
                color,
                new Vector3(-0.68f, 0f, 0.38f),
                new Vector3(0.42f, 0.28f, 0.58f),
                "French-AMX40-Thermal");
        }

        private static void AddLeclercGunPlant(
            Transform turret,
            Color color,
            string id)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            string prefix =
                id == "leclerc_xlr"
                    ? "French-LeclercXLR"
                    : id == "amx56"
                        ? "French-AMX56"
                        : "French-Leclerc";
            Transform fittings =
                CreateGunFittingsRoot(
                    gun,
                    prefix + "-GunFittings");
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-GunMask",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.1f),
                id == "amx56"
                    ? new Vector3(1.02f, 0.48f, 0.34f)
                    : new Vector3(0.88f, 0.44f, 0.3f),
                color * 0.64f);
            Transform collar =
                TankDetailGeometry.Part(
                    prefix + "-GunMaskCollar",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, 0.36f),
                    new Vector3(0.19f, 0.2f, 0.19f),
                    Gunmetal());
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            AddCoax(
                fittings,
                0.26f,
                -0.02f,
                0.58f,
                0.46f,
                prefix + "-Coax");
            if (id != "amx56") return;
            for (int clamp = 0;
                clamp < 5;
                clamp++)
            {
                Transform item =
                    TankDetailGeometry.Part(
                        "French-AMX56-ThermalClamp",
                        PrimitiveType.Cylinder,
                        fittings,
                        new Vector3(
                            0f,
                            0f,
                            1.55f + clamp * 0.87f),
                        new Vector3(0.12f, 0.035f, 0.12f),
                        Gunmetal());
                item.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            AddGunSight(
                fittings,
                color,
                new Vector3(0.15f, 0.1f, 5.02f),
                new Vector3(0.18f, 0.15f, 0.36f),
                "French-AMX56-MuzzleReference");
        }

        private static Transform CreateGunFittingsRoot(
            Transform gun,
            string name)
        {
            GameObject root = new GameObject(name);
            Transform transform = root.transform;
            transform.SetParent(gun, false);
            transform.localPosition =
                new Vector3(0f, 0f, -0.5f);
            transform.localRotation =
                Quaternion.identity;
            Vector3 scale = gun.localScale;
            transform.localScale =
                new Vector3(
                    scale.x == 0f
                        ? 1f
                        : 1f / scale.x,
                    scale.y == 0f
                        ? 1f
                        : 1f / scale.y,
                    scale.z == 0f
                        ? 1f
                        : 1f / scale.z);
            return transform;
        }

        private static void AddRemoteWeaponStation(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float x,
            float z,
            string prefix,
            bool heavy)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Base",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, roof + 0.055f, z),
                new Vector3(0.25f, 0.055f, 0.25f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Body",
                PrimitiveType.Cube,
                turret,
                new Vector3(x, roof + 0.2f, z),
                new Vector3(
                    heavy ? 0.42f : 0.38f,
                    0.18f,
                    heavy ? 0.4f : 0.36f),
                color * 0.62f);
            TankFrenchFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    x,
                    0.28f,
                    z + 0.04f),
                prefix,
                heavy,
                true);
            TankDetailGeometry.Part(
                prefix + "-Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x - 0.16f,
                    roof + 0.23f,
                    z + 0.19f),
                new Vector3(0.11f, 0.09f, 0.014f),
                Lens());
        }

        private static void AddCoax(
            Transform gun,
            float x,
            float y,
            float z,
            float length,
            string prefix)
        {
            TankDetailGeometry.Part(
                prefix + "-Housing",
                PrimitiveType.Cube,
                gun,
                new Vector3(x, y, z),
                new Vector3(0.15f, 0.18f, 0.3f),
                Gunmetal());
            Transform barrel =
                TankDetailGeometry.Part(
                    prefix + "-Barrel",
                    PrimitiveType.Cylinder,
                    gun,
                    new Vector3(
                        x,
                        y,
                        z + length * 0.5f),
                    new Vector3(0.03f, length * 0.5f, 0.03f),
                    Gunmetal());
            barrel.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddGunSight(
            Transform gun,
            Color color,
            Vector3 center,
            Vector3 size,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Housing",
                PrimitiveType.Cube,
                gun,
                center,
                size,
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-Lens",
                PrimitiveType.Cube,
                gun,
                new Vector3(
                    center.x,
                    center.y,
                    center.z + size.z * 0.52f),
                new Vector3(
                    size.x * 0.6f,
                    size.y * 0.5f,
                    0.014f),
                Lens());
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
