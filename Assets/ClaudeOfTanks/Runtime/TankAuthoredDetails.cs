using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankAuthoredDetails
    {
        public static Vector3 ResolveTurretPivot(
            VehicleDefinition definition,
            float height,
            float length)
        {
            if (definition?.armor?.turretPivot != null)
                return definition.armor.turretPivot.ToVector3();
            return new Vector3(
                0f,
                height * 0.63f,
                length * 0.03f);
        }

        public static float ResolveGunLength(
            VehicleDefinition definition,
            float hullLength)
        {
            float authored =
                definition?.armor?.gunBarrel?.lengthM ?? 0f;
            if (authored > 0.1f) return authored;
            return definition?.gun != null &&
                definition.gun.caliberMm < 80f
                    ? hullLength * 0.35f
                    : hullLength * 0.62f;
        }

        public static float ResolveGunRadius(
            VehicleDefinition definition)
        {
            float authored =
                definition?.armor?.gunBarrel?.radiusM ?? 0f;
            if (authored > 0.01f)
                return authored * 2f;
            return Mathf.Clamp(
                (definition?.gun != null
                    ? definition.gun.caliberMm
                    : 120f) / 500f,
                0.08f,
                0.32f);
        }

        public static Vector3 ResolveGunCenter(
            VehicleDefinition definition,
            float fallbackStartZ,
            float gunLength)
        {
            if (definition?.armor?.gunPivot != null)
            {
                Vector3 pivot =
                    definition.armor.gunPivot.ToVector3();
                pivot.z += gunLength * 0.5f;
                return pivot;
            }
            return new Vector3(
                0f,
                0.05f,
                fallbackStartZ + gunLength * 0.45f);
        }

        public static void Build(
            Transform hull,
            Transform turret,
            VehicleDefinition definition,
            Color color)
        {
            ArmorModuleDefinition[] modules =
                definition?.armor?.modules;
            if (modules == null) return;
            int detailIndex = 0;
            for (int moduleIndex = 0;
                moduleIndex < modules.Length;
                moduleIndex++)
            {
                ArmorModuleDefinition module =
                    modules[moduleIndex];
                if (module == null ||
                    module.parts == null ||
                    module.parts.Length == 0)
                {
                    continue;
                }
                Transform parent = module.turretLocal
                    ? turret
                    : hull;
                for (int partIndex = 0;
                    partIndex < module.parts.Length;
                    partIndex++)
                {
                    ArmorModulePartDefinition part =
                        module.parts[partIndex];
                    if (!TryBounds(
                        part,
                        out Vector3 center,
                        out Vector3 size))
                    {
                        continue;
                    }
                    CreatePart(
                        "Module-" + module.module + "-" +
                        detailIndex++,
                        parent,
                        center,
                        size,
                        ModuleColor(module.module, color));
                }
            }
        }

        private static bool TryBounds(
            ArmorModulePartDefinition part,
            out Vector3 center,
            out Vector3 size)
        {
            center = Vector3.zero;
            size = Vector3.zero;
            if (part?.min == null ||
                part.max == null ||
                part.min.Length < 3 ||
                part.max.Length < 3)
            {
                return false;
            }
            Vector3 minimum = new Vector3(
                part.min[0],
                part.min[1],
                part.min[2]);
            Vector3 maximum = new Vector3(
                part.max[0],
                part.max[1],
                part.max[2]);
            center = (minimum + maximum) * 0.5f;
            size = maximum - minimum;
            return size.x > 0.003f &&
                size.y > 0.003f &&
                size.z > 0.003f;
        }

        private static Color ModuleColor(
            string module,
            Color color)
        {
            if (module == "optics")
            {
                return Color.Lerp(
                    new Color(0.035f, 0.07f, 0.075f),
                    color,
                    0.12f);
            }
            if (module == "engine" ||
                module == "transmission")
            {
                return color * 0.72f;
            }
            return color * 0.84f;
        }

        private static void CreatePart(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color)
        {
            GameObject part = GameObject.CreatePrimitive(
                PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Collider collider =
                part.GetComponent<Collider>();
            if (Application.isPlaying)
                Object.Destroy(collider);
            else
                Object.DestroyImmediate(collider);
            part.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Standard"))
                {
                    color = color
                };
        }
    }
}
