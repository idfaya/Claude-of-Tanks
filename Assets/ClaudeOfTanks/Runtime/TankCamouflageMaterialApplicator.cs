using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankCamouflageMaterialApplicator
    {
        public static Texture2D CreateAndApply(
            ContentCatalog catalog,
            VehicleDefinition vehicle,
            string camouflageId,
            string mapId,
            Team team,
            Renderer[] renderers,
            Color baseColor)
        {
            if (catalog == null || vehicle == null)
                return null;
            Texture2D texture = TankCamouflage.CreateTexture(
                catalog,
                vehicle,
                camouflageId,
                mapId,
                team);
            string resolved = TankCamouflage.ResolveId(
                vehicle,
                camouflageId,
                mapId);
            CamouflageDefinition camouflage =
                catalog.ContainsCamouflage(resolved)
                    ? catalog.GetCamouflage(resolved)
                    : null;
            Apply(
                renderers,
                texture,
                TankCamouflage.ResolveScale(
                    vehicle,
                    camouflage),
                baseColor);
            if (vehicle.id == "t90" ||
                vehicle.id == "t90a" ||
                vehicle.id == "t90a_vladimir" ||
                vehicle.id == "t90a_burlak" ||
                vehicle.id == "t90sm")
            {
                TankT90MaterialApplicator.Apply(
                    renderers,
                    vehicle.id,
                    baseColor);
            }
            return texture;
        }

        public static Vector2[] ProjectUvs(
            Vector3[] vertices,
            Bounds bounds,
            Vector3 normal)
        {
            Vector2[] uv = new Vector2[vertices.Length];
            Vector3 abs = new Vector3(
                Mathf.Abs(normal.x),
                Mathf.Abs(normal.y),
                Mathf.Abs(normal.z));
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 point;
                Vector2 minimum;
                Vector2 size;
                if (abs.y >= abs.x && abs.y >= abs.z)
                {
                    point = new Vector2(
                        vertices[i].x,
                        vertices[i].z);
                    minimum = new Vector2(
                        bounds.min.x,
                        bounds.min.z);
                    size = new Vector2(
                        bounds.size.x,
                        bounds.size.z);
                }
                else if (abs.x >= abs.z)
                {
                    point = new Vector2(
                        vertices[i].z,
                        vertices[i].y);
                    minimum = new Vector2(
                        bounds.min.z,
                        bounds.min.y);
                    size = new Vector2(
                        bounds.size.z,
                        bounds.size.y);
                }
                else
                {
                    point = new Vector2(
                        vertices[i].x,
                        vertices[i].y);
                    minimum = new Vector2(
                        bounds.min.x,
                        bounds.min.y);
                    size = new Vector2(
                        bounds.size.x,
                        bounds.size.y);
                }
                uv[i] = new Vector2(
                    size.x > 0.0001f
                        ? (point.x - minimum.x) / size.x
                        : 0f,
                    size.y > 0.0001f
                        ? (point.y - minimum.y) / size.y
                        : 0f);
            }
            return uv;
        }

        public static void Apply(
            Renderer[] renderers,
            Texture2D texture,
            float repeatsPerMeter,
            Color baseColor)
        {
            float baseLuminance = Mathf.Max(
                0.01f,
                Luminance(baseColor));
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (!IsPaintedSurface(
                        renderer.gameObject.name))
                {
                    continue;
                }
                Material material = renderer.sharedMaterial;
                float brightness = Mathf.Clamp(
                    Luminance(material.color) /
                    baseLuminance,
                    0.55f,
                    1.2f);
                Bounds bounds = renderer.bounds;
                material.mainTexture = texture;
                material.mainTextureScale = new Vector2(
                    Mathf.Max(
                        0.5f,
                        Mathf.Max(
                            bounds.size.x,
                            bounds.size.z) *
                        repeatsPerMeter),
                    Mathf.Max(
                        0.5f,
                        Mathf.Max(
                            bounds.size.y,
                            bounds.size.z) *
                        repeatsPerMeter));
                material.color = new Color(
                    brightness,
                    brightness,
                    brightness,
                    1f);
            }
        }

        private static bool IsPaintedSurface(string name)
        {
            return name == "Hull" ||
                name == "UpperHull" ||
                name == "Turret" ||
                name == "MissilePod" ||
                name == "SideArmor" ||
                name == "ERA" ||
                name == "TurretBustle" ||
                name == "TurretWedge" ||
                name.StartsWith("Painted-") ||
                name.StartsWith("Armor-");
        }

        private static float Luminance(Color color)
        {
            return color.r * 0.2126f +
                color.g * 0.7152f +
                color.b * 0.0722f;
        }
    }
}
