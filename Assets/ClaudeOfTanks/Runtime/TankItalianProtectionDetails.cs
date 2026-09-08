using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankItalianProtectionDetails
    {
        public static void Build(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            if (definition.id == "carro45t")
            {
                AddCarroGunPlant(
                    turret,
                    color);
                return;
            }
            AddArieteGunPlant(
                turret,
                definition.id,
                color);
            if (definition.id == "ariete_c2")
                AddC2Package(
                    turret,
                    definition,
                    color,
                    width);
        }

        private static void AddCarroGunPlant(
            Transform turret,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    "Italian-Carro45T-GunFittings");
            TankDetailGeometry.Part(
                "Painted-Italian-Carro45T-GunMask",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.1f),
                new Vector3(0.78f, 0.5f, 0.42f),
                color * 0.65f);
            Transform collar =
                TankDetailGeometry.Part(
                    "Italian-Carro45T-GunMaskCollar",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, 0.39f),
                    new Vector3(0.18f, 0.2f, 0.18f),
                    Gunmetal());
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            AddCoax(
                fittings,
                0.26f,
                0.08f,
                0.5f,
                0.52f,
                "Italian-Carro45T-Coax");
            AddGunSight(
                fittings,
                color,
                new Vector3(0f, 0.14f, 4.85f),
                new Vector3(0.12f, 0.1f, 0.18f),
                "Italian-Carro45T-MuzzleReference");
        }

        private static void AddArieteGunPlant(
            Transform turret,
            string id,
            Color color)
        {
            Transform gun = turret.Find("Gun");
            if (gun == null) return;
            string prefix =
                id == "ariete_c2"
                    ? "Italian-ArieteC2"
                    : id == "ariete_c1"
                        ? "Italian-ArieteC1"
                        : "Italian-Ariete";
            Transform fittings =
                TankDetailGeometry.GunFittingsRoot(
                    gun,
                    prefix + "-GunFittings");
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-GunMask",
                PrimitiveType.Cube,
                fittings,
                new Vector3(0f, 0f, 0.1f),
                id == "ariete"
                    ? new Vector3(0.78f, 0.52f, 0.38f)
                    : new Vector3(0.88f, 0.48f, 0.4f),
                color * 0.64f);
            Transform collar =
                TankDetailGeometry.Part(
                    prefix + "-GunMaskCollar",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(0f, 0f, 0.4f),
                    new Vector3(0.18f, 0.2f, 0.18f),
                    Gunmetal());
            collar.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            AddCoax(
                fittings,
                0.28f,
                0.07f,
                0.55f,
                0.46f,
                prefix + "-Coax");
            for (int clamp = 0;
                clamp < 4;
                clamp++)
            {
                Transform item =
                    TankDetailGeometry.Part(
                        prefix + "-ThermalClamp",
                        PrimitiveType.Cylinder,
                        fittings,
                        new Vector3(
                            0f,
                            0f,
                            1.15f + clamp * 0.92f),
                        new Vector3(0.115f, 0.03f, 0.115f),
                        Gunmetal());
                item.localRotation =
                    Quaternion.Euler(90f, 0f, 0f);
            }
            AddGunSight(
                fittings,
                color,
                new Vector3(0f, 0.13f, 4.35f),
                new Vector3(0.11f, 0.09f, 0.14f),
                prefix + "-MRS");
        }

        private static void AddC2Package(
            Transform turret,
            VehicleDefinition definition,
            Color color,
            float width)
        {
            float roof =
                TankDetailGeometry.TurretRoofY(
                    definition);
            TankItalianFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    -width * 0.19f,
                    roof + 0.32f,
                    -0.16f),
                new Vector3(0.3f, 0.3f, 0.3f),
                "Italian-ArieteC2-CommanderSight");
            TankDetailGeometry.Part(
                "Painted-Italian-ArieteC2-APU",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -width * 0.29f,
                    roof - 0.28f,
                    -1.35f),
                new Vector3(0.44f, 0.34f, 0.6f),
                color * 0.62f);
            TankDetailGeometry.Part(
                "Italian-ArieteC2-APUGrille",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    -width * 0.29f,
                    roof - 0.28f,
                    -1.04f),
                new Vector3(0.38f, 0.26f, 0.018f),
                Gunmetal());
            AddRemoteWeaponStation(
                turret,
                roof,
                color,
                width * 0.136f,
                -0.57f);
        }

        private static void AddRemoteWeaponStation(
            Transform turret,
            float roof,
            Color color,
            float x,
            float z)
        {
            TankDetailGeometry.Part(
                "Painted-Italian-ArieteC2-RWS-Base",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, roof + 0.05f, z),
                new Vector3(0.2f, 0.05f, 0.2f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Painted-Italian-ArieteC2-RWS-Body",
                PrimitiveType.Cube,
                turret,
                new Vector3(x, roof + 0.19f, z),
                new Vector3(0.32f, 0.19f, 0.3f),
                color * 0.62f);
            TankItalianFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(
                    x,
                    0.28f,
                    z + 0.03f),
                "Italian-ArieteC2-RWS",
                true,
                true);
            TankDetailGeometry.Part(
                "Italian-ArieteC2-RWS-Lens",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    x + 0.2f,
                    roof + 0.23f,
                    z + 0.17f),
                new Vector3(0.1f, 0.09f, 0.014f),
                Lens());
        }

        private static void AddCoax(
            Transform fittings,
            float x,
            float y,
            float z,
            float length,
            string prefix)
        {
            TankDetailGeometry.Part(
                prefix + "-Housing",
                PrimitiveType.Cube,
                fittings,
                new Vector3(x, y, z),
                new Vector3(0.14f, 0.17f, 0.28f),
                Gunmetal());
            Transform barrel =
                TankDetailGeometry.Part(
                    prefix + "-Barrel",
                    PrimitiveType.Cylinder,
                    fittings,
                    new Vector3(
                        x,
                        y,
                        z + length * 0.5f),
                    new Vector3(0.028f, length * 0.5f, 0.028f),
                    Gunmetal());
            barrel.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
        }

        private static void AddGunSight(
            Transform fittings,
            Color color,
            Vector3 center,
            Vector3 size,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Housing",
                PrimitiveType.Cube,
                fittings,
                center,
                size,
                color * 0.68f);
            TankDetailGeometry.Part(
                prefix + "-Lens",
                PrimitiveType.Cube,
                fittings,
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
