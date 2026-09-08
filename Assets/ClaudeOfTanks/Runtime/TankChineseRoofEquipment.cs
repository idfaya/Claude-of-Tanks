using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankChineseRoofEquipment
    {
        public static void AddVisionRing(
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
                    TankChineseFamilyDetails.Lens());
            }
        }

        public static void AddVentilator(
            Transform turret,
            Color color,
            float roof,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-Ventilator",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(0f, roof + 0.08f, 0.76f),
                new Vector3(0.16f, 0.08f, 0.16f),
                color * 0.7f);
            TankDetailGeometry.Part(
                prefix + "-VentilatorCap",
                PrimitiveType.Sphere,
                turret,
                new Vector3(0f, roof + 0.15f, 0.76f),
                new Vector3(0.17f, 0.08f, 0.17f),
                color * 0.55f);
        }

        public static void AddWarningHeads(
            Transform turret,
            Color color,
            float width,
            float roof,
            float z,
            string prefix)
        {
            for (int side = -1;
                side <= 1;
                side += 2)
            {
                float x =
                    side * width * 0.28f;
                TankDetailGeometry.Part(
                    "Painted-" + prefix + "-WarningHead",
                    PrimitiveType.Cube,
                    turret,
                    new Vector3(x, roof + 0.14f, z),
                    new Vector3(0.24f, 0.28f, 0.2f),
                    color * 0.62f);
                for (int aperture = -1;
                    aperture <= 1;
                    aperture += 2)
                {
                    TankDetailGeometry.Part(
                        prefix + "-WarningLens",
                        PrimitiveType.Cylinder,
                        turret,
                        new Vector3(
                            x + aperture * 0.06f,
                            roof + 0.16f,
                            z + 0.108f),
                        new Vector3(0.04f, 0.012f, 0.04f),
                        TankChineseFamilyDetails.Lens())
                        .localRotation =
                            Quaternion.Euler(90f, 0f, 0f);
                }
            }
        }

        public static void AddRemoteWeaponStation(
            Transform turret,
            Color color,
            float x,
            float roof,
            float z)
        {
            TankDetailGeometry.Part(
                "Painted-Chinese-VT4A1-RWS-Base",
                PrimitiveType.Cylinder,
                turret,
                new Vector3(x, roof + 0.05f, z),
                new Vector3(0.2f, 0.05f, 0.2f),
                color * 0.68f);
            TankDetailGeometry.Part(
                "Painted-Chinese-VT4A1-RWS-Body",
                PrimitiveType.Cube,
                turret,
                new Vector3(x, roof + 0.21f, z),
                new Vector3(0.32f, 0.2f, 0.3f),
                color * 0.62f);
            TankChineseFamilyDetails.AddMachineGun(
                turret,
                roof,
                color,
                new Vector3(x, 0.28f, z + 0.03f),
                "Chinese-VT4A1-RWS",
                true,
                true);
            TankChineseFamilyDetails.AddSight(
                turret,
                color,
                new Vector3(
                    x - 0.22f,
                    roof + 0.25f,
                    z + 0.02f),
                new Vector3(0.14f, 0.18f, 0.14f),
                "Chinese-VT4A1-RWS-Sensor");
        }

        public static void AddRadioMast(
            Transform turret,
            Color color,
            Vector3 center,
            float height,
            string prefix)
        {
            TankDetailGeometry.Part(
                "Painted-" + prefix + "-RadioMastFoot",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    center.x,
                    center.y - height * 0.5f,
                    center.z),
                new Vector3(0.24f, 0.2f, 0.22f),
                color * 0.6f);
            TankDetailGeometry.Part(
                prefix + "-RadioMast",
                PrimitiveType.Cylinder,
                turret,
                center,
                new Vector3(0.035f, height * 0.5f, 0.035f),
                TankChineseFamilyDetails.Gunmetal());
            TankDetailGeometry.Part(
                prefix + "-RadioMastHead",
                PrimitiveType.Cube,
                turret,
                new Vector3(
                    center.x,
                    center.y + height * 0.5f,
                    center.z),
                new Vector3(0.11f, 0.09f, 0.11f),
                TankChineseFamilyDetails.Gunmetal());
        }
    }
}
