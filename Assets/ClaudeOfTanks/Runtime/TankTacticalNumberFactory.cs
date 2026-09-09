using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    internal static class TankTacticalNumberFactory
    {
        private const int TextureWidth = 128;
        private const int TextureHeight = 64;

        public static Transform BuildRussianSet(
            string prefix,
            Transform parent,
            string text,
            float insigniaSize,
            Vector3 insigniaPosition,
            Quaternion insigniaRotation,
            float numberSize,
            Vector3 numberPosition,
            Quaternion numberRotation)
        {
            Validate(
                prefix,
                parent,
                text,
                insigniaSize,
                numberSize);
            GameObject rootObject =
                new GameObject(prefix + "-TacticalMarkings");
            Transform root = rootObject.transform;
            root.SetParent(parent, false);
            Texture2D insignia =
                BuildRussianStarTexture(prefix);
            Texture2D designation =
                BuildNumberTexture(prefix, text, true);
            rootObject.AddComponent<TankGeneratedTextureOwner>()
                .Initialize(insignia);
            rootObject.AddComponent<TankGeneratedTextureOwner>()
                .Initialize(designation);
            BuildSide(
                prefix + "-Insignia",
                root,
                insignia,
                insigniaSize,
                insigniaPosition,
                insigniaRotation);
            BuildSide(
                prefix + "-Designation",
                root,
                designation,
                numberSize,
                numberPosition,
                numberRotation);
            return root;
        }

        public static Transform BuildPair(
            string prefix,
            Transform parent,
            string text,
            float size,
            Vector3 rightPosition,
            Quaternion rightRotation,
            Vector3 leftPosition,
            Quaternion leftRotation)
        {
            Validate(prefix, parent, text, size, size);

            GameObject rootObject =
                new GameObject(prefix + "-TacticalNumbers");
            Transform root = rootObject.transform;
            root.SetParent(parent, false);
            Texture2D texture =
                BuildNumberTexture(prefix, text, false);
            rootObject.AddComponent<TankGeneratedTextureOwner>()
                .Initialize(texture);
            BuildSide(
                prefix + "-Right",
                root,
                texture,
                size,
                rightPosition,
                rightRotation);
            BuildSide(
                prefix + "-Left",
                root,
                texture,
                size,
                leftPosition,
                leftRotation);
            return root;
        }

        private static Transform BuildSide(
            string name,
            Transform parent,
            Texture2D texture,
            float size,
            Vector3 position,
            Quaternion rotation)
        {
            GameObject decal =
                new GameObject("VehicleMarking-" + name);
            Transform transform = decal.transform;
            transform.SetParent(parent, false);
            transform.localPosition = position;
            transform.localRotation = rotation;
            transform.localScale = Vector3.one * size;

            Mesh mesh = new Mesh
            {
                name = decal.name + "Mesh",
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, 0f),
                    new Vector3(0.5f, -0.5f, 0f),
                    new Vector3(0.5f, 0.5f, 0f),
                    new Vector3(-0.5f, 0.5f, 0f)
                },
                normals = new[]
                {
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward
                },
                uv = new[]
                {
                    new Vector2(1f, 0f),
                    new Vector2(0f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f)
                },
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };
            mesh.RecalculateBounds();
            decal.AddComponent<MeshFilter>().sharedMesh = mesh;
            decal.AddComponent<MeshRenderer>().sharedMaterial =
                BuildMaterial(name, texture);
            return transform;
        }

        private static Texture2D BuildNumberTexture(
            string prefix,
            string text,
            bool surfaceStyle)
        {
            int textureHeight =
                surfaceStyle ? TextureWidth : TextureHeight;
            int drawingHeight =
                surfaceStyle ? 94 : TextureHeight;
            bool[] mask =
                new bool[TextureWidth * textureHeight];
            int columns = text.Length * 5 +
                Mathf.Max(0, text.Length - 1);
            int unit = Mathf.Max(
                1,
                Mathf.FloorToInt(
                    Mathf.Min(
                        (TextureWidth - 12f) / columns,
                        (drawingHeight - 12f) / 7f)));
            int width = columns * unit;
            int height = 7 * unit;
            int originX = (TextureWidth - width) / 2;
            int originY = (textureHeight - height) / 2;
            for (int glyph = 0; glyph < text.Length; glyph++)
            {
                for (int row = 0; row < 7; row++)
                {
                    int bits = RowBits(text[glyph], row);
                    for (int column = 0; column < 5; column++)
                    {
                        if ((bits & (1 << (4 - column))) == 0)
                            continue;
                        int x0 =
                            originX + (glyph * 6 + column) * unit;
                        int y0 =
                            originY + (6 - row) * unit;
                        FillMask(mask, x0, y0, unit, unit);
                    }
                }
            }

            Color32[] pixels =
                new Color32[TextureWidth * textureHeight];
            Color32 outline =
                surfaceStyle
                    ? new Color32(23, 24, 21, 255)
                    : new Color32(20, 20, 20, 140);
            Color32 pigment =
                surfaceStyle
                    ? new Color32(216, 213, 201, 255)
                    : new Color32(174, 172, 162, 235);
            for (int y = 0; y < textureHeight; y++)
            for (int x = 0; x < TextureWidth; x++)
            {
                int index = y * TextureWidth + x;
                if (mask[index])
                {
                    pixels[index] = pigment;
                    continue;
                }
                if (TouchesMask(
                    mask,
                    x,
                    y,
                    2,
                    TextureWidth,
                    textureHeight))
                    pixels[index] = outline;
            }

            Texture2D texture = new Texture2D(
                TextureWidth,
                textureHeight,
                TextureFormat.RGBA32,
                true)
            {
                name =
                    "TankTacticalNumberTexture-" +
                    prefix +
                    "-" +
                    text,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private static Texture2D BuildRussianStarTexture(
            string prefix)
        {
            const int size = 128;
            const float center = size * 0.5f;
            const float outerRadius = 45f;
            const float innerRadius = 18.5f;
            Vector2[] star = new Vector2[10];
            for (int index = 0; index < star.Length; index++)
            {
                float angle =
                    -Mathf.PI * 0.5f + index * Mathf.PI / 5f;
                float radius =
                    index % 2 == 0
                        ? outerRadius
                        : innerRadius;
                star[index] = new Vector2(
                    center + Mathf.Cos(angle) * radius,
                    center + Mathf.Sin(angle) * radius);
            }
            bool[] mask = new bool[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                mask[y * size + x] =
                    Contains(star, x + 0.5f, y + 0.5f);

            Color32[] pixels = new Color32[size * size];
            Color32 outline =
                new Color32(238, 233, 219, 255);
            Color32 pigment =
                new Color32(182, 50, 46, 255);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                if (mask[index])
                {
                    pixels[index] = pigment;
                    continue;
                }
                if (TouchesMask(mask, x, y, 4, size, size))
                    pixels[index] = outline;
            }
            Texture2D texture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                true)
            {
                name =
                    "TankTacticalInsigniaTexture-" +
                    prefix +
                    "-RU",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private static void Validate(
            string prefix,
            Transform parent,
            string text,
            float firstSize,
            float secondSize)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException(
                    "Tactical-marking prefix is required.",
                    nameof(prefix));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException(
                    "Tactical-marking text is required.",
                    nameof(text));
            if (firstSize <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(firstSize));
            if (secondSize <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(secondSize));
        }

        private static void FillMask(
            bool[] mask,
            int x0,
            int y0,
            int width,
            int height)
        {
            for (int y = y0; y < y0 + height; y++)
            for (int x = x0; x < x0 + width; x++)
                mask[y * TextureWidth + x] = true;
        }

        private static bool TouchesMask(
            bool[] mask,
            int x,
            int y,
            int radius,
            int width,
            int height)
        {
            int minY = Mathf.Max(0, y - radius);
            int maxY = Mathf.Min(height - 1, y + radius);
            int minX = Mathf.Max(0, x - radius);
            int maxX = Mathf.Min(width - 1, x + radius);
            for (int sampleY = minY; sampleY <= maxY; sampleY++)
            for (int sampleX = minX; sampleX <= maxX; sampleX++)
                if (mask[sampleY * width + sampleX])
                    return true;
            return false;
        }

        private static bool Contains(
            Vector2[] polygon,
            float x,
            float y)
        {
            bool inside = false;
            for (int current = 0, previous = polygon.Length - 1;
                current < polygon.Length;
                previous = current++)
            {
                Vector2 a = polygon[current];
                Vector2 b = polygon[previous];
                bool crosses =
                    (a.y > y) != (b.y > y) &&
                    x < (b.x - a.x) * (y - a.y) /
                    (b.y - a.y) + a.x;
                if (crosses) inside = !inside;
            }
            return inside;
        }

        private static int RowBits(char character, int row)
        {
            int[] rows;
            switch (character)
            {
                case '0': rows = new[] { 14, 17, 19, 21, 25, 17, 14 }; break;
                case '1': rows = new[] { 4, 12, 4, 4, 4, 4, 14 }; break;
                case '2': rows = new[] { 14, 17, 1, 2, 4, 8, 31 }; break;
                case '3': rows = new[] { 30, 1, 1, 14, 1, 1, 30 }; break;
                case '4': rows = new[] { 2, 6, 10, 18, 31, 2, 2 }; break;
                case '5': rows = new[] { 31, 16, 16, 30, 1, 1, 30 }; break;
                case '6': rows = new[] { 14, 16, 16, 30, 17, 17, 14 }; break;
                case '7': rows = new[] { 31, 1, 2, 4, 8, 8, 8 }; break;
                case '8': rows = new[] { 14, 17, 17, 14, 17, 17, 14 }; break;
                case '9': rows = new[] { 14, 17, 17, 15, 1, 1, 14 }; break;
                case '-': rows = new[] { 0, 0, 0, 14, 0, 0, 0 }; break;
                case ' ': rows = new[] { 0, 0, 0, 0, 0, 0, 0 }; break;
                default:
                    throw new ArgumentException(
                        "Unsupported tactical-number character: " +
                        character);
            }
            return rows[row];
        }

        private static Material BuildMaterial(
            string name,
            Texture2D texture)
        {
            Shader shader =
                Shader.Find("Unlit/Transparent") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("Standard");
            Material material = new Material(shader)
            {
                name = "TankTacticalNumberMaterial-" + name,
                mainTexture = texture,
                color = Color.white,
                renderQueue = 3000
            };
            if (material.HasProperty("_ZWrite"))
                material.SetInt("_ZWrite", 0);
            if (material.HasProperty("_Cull"))
                material.SetInt("_Cull", 0);
            return material;
        }
    }
}
