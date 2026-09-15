using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankLegacyRunningGearFactory
    {
        public static void BuildPair(
            Transform root,
            float centerX,
            float y,
            float length,
            float width,
            int wheelCount,
            VehicleDefinition definition)
        {
            BuildSide(root, -centerX, y, length, width, wheelCount, definition);
            BuildSide(root, centerX, y, length, width, wheelCount, definition);
        }

        private static void BuildSide(
            Transform root,
            float x,
            float y,
            float length,
            float width,
            int wheelCount,
            VehicleDefinition definition)
        {
            string side = x < 0f ? "L" : "R";
            Transform gear =
                new GameObject("RunningGear-" + side).transform;
            gear.SetParent(root, false);
            Color wheelColor = new Color(0.15f, 0.16f, 0.13f);
            Color hubColor = new Color(0.23f, 0.25f, 0.2f);
            float radius =
                TankRunningGearLayout.RoadWheelRadius(definition, y);
            float thickness = width * 0.72f;
            for (int index = 0; index < wheelCount; index++)
            {
                float z = TankRunningGearLayout.RoadWheelZ(
                    definition, index, wheelCount, length);
                Transform arm = Part(
                    "SuspensionArm-" + side,
                    PrimitiveType.Cube,
                    gear,
                    new Vector3(x * 0.82f, y * 1.02f, z - radius * 0.18f),
                    new Vector3(width * 0.2f, radius * 0.16f, radius * 0.9f),
                    wheelColor * 0.7f);
                arm.localRotation = Quaternion.Euler(18f, 0f, 0f);
                Transform joint = Part(
                    "SuspensionJoint-" + side,
                    PrimitiveType.Cylinder,
                    gear,
                    new Vector3(x * 0.82f, y * 1.18f, z - radius * 0.46f),
                    new Vector3(radius * 0.2f, width * 0.22f, radius * 0.2f),
                    wheelColor * 0.72f);
                joint.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Wheel(
                    "RoadWheel-" + side,
                    gear,
                    x,
                    y,
                    z,
                    radius,
                    thickness,
                    wheelColor,
                    false);
                Wheel(
                    "WheelHub-" + side,
                    gear,
                    x + Mathf.Sign(x) * width * 0.38f,
                    y,
                    z,
                    radius * 0.34f,
                    width * 0.09f,
                    hubColor,
                    false);
            }
            EndWheels(
                gear, side, x, y, length, radius, thickness,
                definition, wheelColor, hubColor);
            ReturnRollers(
                gear, side, x, y, length, radius, thickness,
                wheelCount, wheelColor);
            TrackLinks(
                gear, side, x, y, length, width, radius, definition);
        }

        private static void EndWheels(
            Transform gear,
            string side,
            float x,
            float y,
            float length,
            float roadRadius,
            float thickness,
            VehicleDefinition definition,
            Color wheelColor,
            Color hubColor)
        {
            Vector2 sprocket =
                TankRunningGearLayout.SprocketPosition(
                    definition, length, y, roadRadius);
            Vector2 idler =
                TankRunningGearLayout.IdlerPosition(
                    definition, length, y, roadRadius);
            Wheel(
                "Sprocket-" + side,
                gear,
                x,
                sprocket.y,
                sprocket.x,
                TankRunningGearLayout.SprocketRadius(
                    definition,
                    roadRadius),
                thickness,
                hubColor,
                true);
            Wheel(
                "Idler-" + side,
                gear,
                x,
                idler.y,
                idler.x,
                TankRunningGearLayout.IdlerRadius(
                    definition,
                    roadRadius),
                thickness,
                wheelColor,
                true);
        }

        private static void Wheel(
            string name,
            Transform parent,
            float x,
            float y,
            float z,
            float radius,
            float thickness,
            Color color,
            bool addHub)
        {
            Transform wheel = Part(
                name,
                PrimitiveType.Cylinder,
                parent,
                new Vector3(x, y, z),
                new Vector3(radius, thickness, radius),
                color);
            wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
            if (!addHub) return;
            Transform hub = Part(
                name + "-Hub",
                PrimitiveType.Cylinder,
                parent,
                new Vector3(
                    x + Mathf.Sign(x) * thickness * 0.52f,
                    y,
                    z),
                new Vector3(
                    radius * 0.3f,
                    thickness * 0.12f,
                    radius * 0.3f),
                color * 1.2f);
            hub.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        private static void ReturnRollers(
            Transform gear,
            string side,
            float x,
            float y,
            float length,
            float radius,
            float thickness,
            int wheelCount,
            Color color)
        {
            int count = Mathf.Clamp(wheelCount / 2, 2, 4);
            float topY = y + radius * 0.92f;
            for (int index = 0; index < count; index++)
            {
                float z = Mathf.Lerp(
                    -length * 0.27f,
                    length * 0.27f,
                    count == 1 ? 0.5f : index / (float)(count - 1));
                Transform roller = Part(
                    "ReturnRoller-" + side,
                    PrimitiveType.Cylinder,
                    gear,
                    new Vector3(x, topY - radius * 0.08f, z),
                    new Vector3(
                        radius * 0.3f,
                        thickness * 0.72f,
                        radius * 0.3f),
                    color * 0.9f);
                roller.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        private static void TrackLinks(
            Transform parent,
            string side,
            float x,
            float y,
            float length,
            float width,
            float radius,
            VehicleDefinition definition)
        {
            float front =
                TankRunningGearLayout.TrackFrontZ(definition, length);
            float rear =
                TankRunningGearLayout.TrackRearZ(definition, length);
            float bottom =
                TankRunningGearLayout.TrackBottomY(definition, y, radius);
            float top =
                TankRunningGearLayout.TrackTopY(definition, y, radius);
            float pitch = Mathf.Clamp(width * 0.34f, 0.11f, 0.24f);
            int straight = Mathf.Clamp(
                Mathf.CeilToInt((front - rear) / pitch), 18, 54);
            const int arc = 7;
            int total = straight * 2 + arc * 2;
            Vector3[] vertices = new Vector3[total * 8];
            int[] triangles = new int[total * 36];
            int link = 0;
            for (int index = 0; index < straight; index++)
            {
                float t = (index + 0.5f) / straight;
                float z = Mathf.Lerp(rear, front, t);
                AppendLink(
                    vertices, triangles, link++,
                    new Vector3(x, bottom, z),
                    width, 0.09f, pitch * 0.78f, 0f);
                AppendLink(
                    vertices, triangles, link++,
                    new Vector3(x, top, Mathf.Lerp(front, rear, t)),
                    width, 0.09f, pitch * 0.78f, 0f);
            }
            float centerY = (bottom + top) * 0.5f;
            float arcRadius = (top - bottom) * 0.5f;
            for (int end = -1; end <= 1; end += 2)
            for (int index = 0; index < arc; index++)
            {
                float angle =
                    -Mathf.PI * 0.5f +
                    (index + 0.5f) / arc * Mathf.PI;
                AppendLink(
                    vertices, triangles, link++,
                    new Vector3(
                        x,
                        centerY + Mathf.Sin(angle) * arcRadius,
                        (end < 0 ? rear : front) +
                            end * Mathf.Cos(angle) * arcRadius),
                    width,
                    0.09f,
                    pitch * 0.72f,
                    -end * angle * Mathf.Rad2Deg);
            }
            Mesh mesh = new Mesh
            {
                name = "TrackLinks-" + side,
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GameObject links = new GameObject("TrackLinks-" + side);
            links.transform.SetParent(parent, false);
            links.AddComponent<MeshFilter>().sharedMesh = mesh;
            links.AddComponent<MeshRenderer>().sharedMaterial =
                new Material(Shader.Find("Standard"))
                {
                    color = new Color(0.09f, 0.09f, 0.08f)
                };
        }

        private static void AppendLink(
            Vector3[] vertices,
            int[] triangles,
            int index,
            Vector3 center,
            float width,
            float height,
            float length,
            float rotationX)
        {
            int vertex = index * 8;
            Quaternion rotation =
                Quaternion.Euler(rotationX, 0f, 0f);
            Vector3 half =
                new Vector3(width, height, length) * 0.5f;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 local = new Vector3(
                    (corner & 1) == 0 ? -half.x : half.x,
                    (corner & 2) == 0 ? -half.y : half.y,
                    (corner & 4) == 0 ? -half.z : half.z);
                vertices[vertex + corner] =
                    center + rotation * local;
            }
            int triangle = index * 36;
            int[] faces =
            {
                0, 2, 3, 0, 3, 1,
                4, 5, 7, 4, 7, 6,
                0, 1, 5, 0, 5, 4,
                2, 6, 7, 2, 7, 3,
                0, 4, 6, 0, 6, 2,
                1, 3, 7, 1, 7, 5
            };
            for (int i = 0; i < faces.Length; i++)
                triangles[triangle + i] = vertex + faces[i];
        }

        private static Transform Part(
            string name,
            PrimitiveType type,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(collider);
                else
                    Object.DestroyImmediate(collider);
            }
            part.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Standard")) { color = color };
            return part.transform;
        }
    }
}
