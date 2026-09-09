using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankT90ABurlakGunDetails
    {
        public static void Build(
            Transform turret,
            Color color)
        {
            Transform generic = turret.Find("Gun");
            HideRenderer(generic);
            Transform presentation =
                turret.Find("T90ABurlak-PresentationRoot");
            if (presentation == null) return;
            Transform assembly = TankT90AGunDetails.BuildAssembly(
                "T90ABurlak-GunAssembly",
                presentation,
                new Vector3(0f, 0.225f, 0.615f),
                new Vector3(1f, 1.318f, 0.955f),
                color);
            AddSupplementalCourses(assembly, color);
        }

        private static void AddSupplementalCourses(
            Transform assembly,
            Color color)
        {
            Cylinder(
                "Painted-T90ABurlak-BarrelCourse",
                assembly,
                V(0f, 0f, 1.22f),
                0.105f,
                1.36f,
                18,
                color * 0.47f);
            Cylinder(
                "Painted-T90ABurlak-BarrelCourse",
                assembly,
                V(0f, 0f, 3.05f),
                0.082f,
                2.34f,
                18,
                color * 0.47f);
            float[] rings = { 0.56f, 1.90f, 2.18f, 3.02f, 4.18f };
            for (int index = 0; index < rings.Length; index++)
            {
                Cylinder(
                    "T90ABurlak-BarrelRing",
                    assembly,
                    V(0f, 0f, rings[index]),
                    rings[index] < 2f ? 0.116f : 0.098f,
                    0.03f,
                    16,
                    Dark());
            }
        }

        private static Transform Cylinder(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            float length,
            int segments,
            Color color)
        {
            Transform part = TankShapeFactory.CylinderPart(
                name,
                parent,
                radius,
                radius,
                length,
                segments,
                TankShapeAxis.Z,
                color);
            part.localPosition = position;
            return part;
        }

        private static void HideRenderer(Transform part)
        {
            Renderer renderer =
                part == null ? null : part.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        private static Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        private static Color Dark()
        {
            return TankT90AFamilyDetails.Dark();
        }
    }
}
