using System;
using System.Collections.Generic;
using System.Globalization;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public sealed class BattleMinimap : IDisposable
    {
        public const float WorldSizeM = 1000f;
        public const int TextureSize = 256;
        public const int MaximumTankMarkers = 16;
        private static readonly Color AllyColor = new Color(0.35f, 0.9f, 0.4f);
        private static readonly Color EnemyColor = new Color(0.95f, 0.28f, 0.24f);

        private readonly RectTransform _root;
        private readonly RawImage _background;
        private readonly Marker[] _tankMarkers = new Marker[MaximumTankMarkers];
        private readonly Marker[] _objectiveMarkers = new Marker[5];
        private Texture2D _texture;

        private BattleMinimap(RectTransform root, RawImage background)
        {
            _root = root;
            _background = background;
        }

        public int VisibleTankMarkerCount { get; private set; }
        public int VisibleEnemyMarkerCount { get; private set; }
        public int VisibleObjectiveMarkerCount { get; private set; }

        public static BattleMinimap Create(Transform parent, Font font)
        {
            GameObject rootObject = new GameObject(
                "Minimap",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            rootObject.transform.SetParent(parent, false);
            RectTransform root = rootObject.GetComponent<RectTransform>();
            root.anchorMin = new Vector2(1f, 0f);
            root.anchorMax = new Vector2(1f, 0f);
            root.offsetMin = new Vector2(-236f, 16f);
            root.offsetMax = new Vector2(-16f, 236f);
            Image frame = rootObject.GetComponent<Image>();
            frame.color = new Color(0.025f, 0.035f, 0.032f, 0.92f);
            frame.raycastTarget = false;

            GameObject mapObject = new GameObject(
                "Map",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            mapObject.transform.SetParent(root, false);
            RectTransform mapRect = mapObject.GetComponent<RectTransform>();
            Stretch(mapRect, 7f);
            RawImage background = mapObject.GetComponent<RawImage>();
            background.raycastTarget = false;

            BattleMinimap minimap = new BattleMinimap(root, background);
            minimap.BuildGrid(font, mapRect);
            for (int i = 0; i < minimap._tankMarkers.Length; i++)
                minimap._tankMarkers[i] = CreateMarker(mapRect, font, "Tank-" + i, "^", 14);
            for (int i = 0; i < minimap._objectiveMarkers.Length; i++)
                minimap._objectiveMarkers[i] = CreateMarker(mapRect, font, "Objective-" + i, "O", 13);
            return minimap;
        }

        public void SetMap(MapDefinition map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (_texture != null) DestroyObject(_texture);
            _texture = BuildTexture(map);
            _background.texture = _texture;
        }

        public void SetTouchLayout(bool touch)
        {
            if (touch)
            {
                _root.anchorMin = new Vector2(0f, 1f);
                _root.anchorMax = new Vector2(0f, 1f);
                _root.offsetMin = new Vector2(16f, -224f);
                _root.offsetMax = new Vector2(166f, -74f);
            }
            else
            {
                _root.anchorMin = new Vector2(1f, 0f);
                _root.anchorMax = new Vector2(1f, 0f);
                _root.offsetMin = new Vector2(-236f, 16f);
                _root.offsetMax = new Vector2(-16f, 236f);
            }
        }

        public void Update(
            TankState player,
            IList<TankState> tanks,
            MatchModeState mode,
            SpottingSimulation spotting,
            Func<Float3, Float3, bool> isOccluded = null)
        {
            VisibleTankMarkerCount = 0;
            VisibleEnemyMarkerCount = 0;
            if (player != null && tanks != null)
            {
                for (int i = 0; i < tanks.Count &&
                    VisibleTankMarkerCount < _tankMarkers.Length; i++)
                {
                    TankState tank = tanks[i];
                    if (tank == null || tank.Destroyed) continue;
                    bool enemy = tank.Team != player.Team;
                    if (enemy &&
                        (spotting == null ||
                         !spotting.CanSpot(player, tank, isOccluded)))
                    {
                        continue;
                    }
                    Marker marker = _tankMarkers[VisibleTankMarkerCount++];
                    marker.Root.gameObject.SetActive(true);
                    marker.Label.color = tank.Id == player.Id
                        ? Color.white
                        : enemy ? EnemyColor : AllyColor;
                    SetMarkerPosition(marker.Root, tank.Position);
                    marker.Root.localRotation =
                        Quaternion.Euler(0f, 0f, -tank.Yaw * Mathf.Rad2Deg);
                    marker.Root.SetAsLastSibling();
                    if (enemy) VisibleEnemyMarkerCount++;
                }
            }
            HideUnused(_tankMarkers, VisibleTankMarkerCount);
            UpdateObjectives(mode);
        }

        public void Dispose()
        {
            if (_texture != null)
            {
                DestroyObject(_texture);
                _texture = null;
            }
        }

        private void UpdateObjectives(MatchModeState mode)
        {
            VisibleObjectiveMarkerCount = 0;
            if (mode == null)
            {
                HideUnused(_objectiveMarkers, 0);
                return;
            }
            if (mode.Id == GameModeId.CaptureTheFlag)
            {
                AddObjective(mode.AlphaFlag, "F", AllyColor);
                AddObjective(mode.BravoFlag, "F", EnemyColor);
            }
            else if (mode.Id == GameModeId.ZoneControl)
            {
                for (int i = 0; i < mode.Zones.Length; i++)
                {
                    Color color = mode.ZoneOwners[i] == Team.Alpha
                        ? AllyColor
                        : mode.ZoneOwners[i] == Team.Bravo
                            ? EnemyColor
                            : new Color(0.92f, 0.82f, 0.35f);
                    AddObjective(mode.Zones[i], ((char)('A' + i)).ToString(), color);
                }
            }
            else if (mode.Id == GameModeId.TurboBall)
            {
                AddObjective(mode.BallPosition, "O", new Color(0.98f, 0.82f, 0.3f));
            }
            HideUnused(_objectiveMarkers, VisibleObjectiveMarkerCount);
        }

        private void AddObjective(Float3 position, string glyph, Color color)
        {
            if (VisibleObjectiveMarkerCount >= _objectiveMarkers.Length) return;
            Marker marker = _objectiveMarkers[VisibleObjectiveMarkerCount++];
            marker.Root.gameObject.SetActive(true);
            SetMarkerPosition(marker.Root, position);
            marker.Root.localRotation = Quaternion.identity;
            marker.Label.text = glyph;
            marker.Label.color = color;
        }

        private static void HideUnused(Marker[] markers, int used)
        {
            for (int i = used; i < markers.Length; i++)
                markers[i].Root.gameObject.SetActive(false);
        }

        private static void SetMarkerPosition(RectTransform marker, Float3 position)
        {
            Vector2 anchor = new Vector2(
                Mathf.Clamp01(position.X / WorldSizeM + 0.5f),
                Mathf.Clamp01(position.Z / WorldSizeM + 0.5f));
            marker.anchorMin = anchor;
            marker.anchorMax = anchor;
            marker.anchoredPosition = Vector2.zero;
        }

        private void BuildGrid(Font font, RectTransform mapRect)
        {
            for (int i = 1; i < 10; i++)
            {
                float normalized = i / 10f;
                CreateGridLine(mapRect, true, normalized);
                CreateGridLine(mapRect, false, normalized);
            }
            for (int i = 0; i < 10; i++)
            {
                Text column = CreateText(mapRect, font, "Column-" + i, 8);
                column.text = ((i + 1) % 10).ToString();
                column.alignment = TextAnchor.UpperCenter;
                column.rectTransform.anchorMin = new Vector2((i + 0.5f) / 10f, 1f);
                column.rectTransform.anchorMax = column.rectTransform.anchorMin;
                column.rectTransform.sizeDelta = new Vector2(16f, 12f);
                column.rectTransform.anchoredPosition = new Vector2(0f, -1f);

                Text row = CreateText(mapRect, font, "Row-" + i, 8);
                row.text = "ABCDEFGHJK"[i].ToString();
                row.alignment = TextAnchor.MiddleLeft;
                row.rectTransform.anchorMin = new Vector2(0f, 1f - (i + 0.5f) / 10f);
                row.rectTransform.anchorMax = row.rectTransform.anchorMin;
                row.rectTransform.sizeDelta = new Vector2(14f, 12f);
                row.rectTransform.anchoredPosition = new Vector2(2f, 0f);
            }
        }

        private static void CreateGridLine(RectTransform parent, bool vertical, float normalized)
        {
            GameObject lineObject = new GameObject(
                vertical ? "Grid-V" : "Grid-H",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            lineObject.transform.SetParent(parent, false);
            RectTransform line = lineObject.GetComponent<RectTransform>();
            line.anchorMin = vertical
                ? new Vector2(normalized, 0f)
                : new Vector2(0f, normalized);
            line.anchorMax = vertical
                ? new Vector2(normalized, 1f)
                : new Vector2(1f, normalized);
            line.sizeDelta = vertical ? new Vector2(1f, 0f) : new Vector2(0f, 1f);
            line.anchoredPosition = Vector2.zero;
            Image image = lineObject.GetComponent<Image>();
            image.color = new Color(0.9f, 0.95f, 0.95f, 0.1f);
            image.raycastTarget = false;
        }

        private static Marker CreateMarker(
            RectTransform parent,
            Font font,
            string name,
            string glyph,
            int size)
        {
            Text label = CreateText(parent, font, name, size);
            label.text = glyph;
            label.alignment = TextAnchor.MiddleCenter;
            label.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            label.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            label.rectTransform.sizeDelta = new Vector2(18f, 18f);
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.gameObject.SetActive(false);
            return new Marker(label.rectTransform, label);
        }

        private static Text CreateText(Transform parent, Font font, string name, int size)
        {
            GameObject child = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            child.transform.SetParent(parent, false);
            Text text = child.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(1f, 1f, 1f, 0.58f);
            text.raycastTarget = false;
            return text;
        }

        private static Texture2D BuildTexture(MapDefinition map)
        {
            MapSurface surface = map.unitySurface;
            MinimapPalette palette = MinimapPalette.From(map);
            Color[] pixels = new Color[TextureSize * TextureSize];
            PaintTerrainBackground(pixels, map, palette);
            if (surface != null)
            {
                DrawDiscs(
                    pixels,
                    surface.marshes,
                    palette.Water,
                    palette.WaterStroke);
                DrawDiscs(
                    pixels,
                    surface.lakes,
                    surface.frozenWater
                        ? Color.Lerp(palette.Water, Color.white, 0.22f)
                        : palette.Water,
                    palette.WaterStroke);
                DrawRoads(pixels, surface.roads, palette.RoadCasing, 4);
                DrawRoads(pixels, surface.roads, palette.RoadFill, 2);
            }
            if (map.unityStructures != null)
            {
                DrawBuildings(
                    pixels,
                    map.unityStructures.buildings,
                    palette.BuildingFill);
            }
            Texture2D texture = new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.RGBA32,
                false,
                true)
            {
                name = "Minimap-" + map.id,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static void PaintTerrainBackground(
            Color[] pixels,
            MapDefinition map,
            MinimapPalette palette)
        {
            IHeightField heightField = MapSimulationAdapter.BuildHeightField(map);
            HeightRange range = SampleHeightRange(heightField);
            if (range.Span <= 0.001f)
            {
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = Opaque(palette.Base);
                return;
            }

            float half = WorldSizeM * 0.5f;
            float step = WorldSizeM / TextureSize;
            for (int y = 0; y < TextureSize; y++)
            {
                float z = -half + (y + 0.5f) * step;
                for (int x = 0; x < TextureSize; x++)
                {
                    float worldX = -half + (x + 0.5f) * step;
                    float height = heightField.HeightAt(worldX, z);
                    float tone = Mathf.Clamp01((height - range.Min) / range.Span);
                    tone = Mathf.Round(tone * 5f) / 5f;
                    float hx = heightField.HeightAt(worldX + step * 2f, z) -
                        heightField.HeightAt(worldX - step * 2f, z);
                    float hz = heightField.HeightAt(worldX, z + step * 2f) -
                        heightField.HeightAt(worldX, z - step * 2f);
                    float shade = Mathf.Clamp(0.88f - hx * 0.05f + hz * 0.05f, 0.55f, 1.2f);
                    shade = Mathf.Round(shade * 5f) / 5f;
                    Color low = Color.Lerp(palette.Soft, palette.Base, 0.65f);
                    Color high = Color.Lerp(palette.Base, palette.Hard, 0.55f);
                    Color color = Color.Lerp(low, high, tone) * shade;
                    color.a = 1f;
                    pixels[y * TextureSize + x] = color;
                }
            }
        }

        private static HeightRange SampleHeightRange(IHeightField heightField)
        {
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            float step = WorldSizeM / 64f;
            for (int z = 0; z <= 64; z++)
            {
                float worldZ = -WorldSizeM * 0.5f + z * step;
                for (int x = 0; x <= 64; x++)
                {
                    float worldX = -WorldSizeM * 0.5f + x * step;
                    float height = heightField.HeightAt(worldX, worldZ);
                    if (height < min) min = height;
                    if (height > max) max = height;
                }
            }
            if (float.IsInfinity(min) || float.IsInfinity(max))
                return new HeightRange(0f, 0f);
            return new HeightRange(min, max);
        }

        private static void DrawDiscs(
            Color[] pixels,
            MapDisc[] discs,
            Color fill,
            Color stroke)
        {
            if (discs == null) return;
            for (int i = 0; i < discs.Length; i++)
            {
                Vector2 center = WorldToPixel(discs[i].x, discs[i].z);
                int radius = Mathf.Max(1, Mathf.RoundToInt(
                    discs[i].r / WorldSizeM * TextureSize));
                int strokeRadius = radius + 1;
                int minX = Mathf.Max(0, Mathf.FloorToInt(center.x) - strokeRadius);
                int maxX = Mathf.Min(TextureSize - 1, Mathf.CeilToInt(center.x) + strokeRadius);
                int minY = Mathf.Max(0, Mathf.FloorToInt(center.y) - strokeRadius);
                int maxY = Mathf.Min(TextureSize - 1, Mathf.CeilToInt(center.y) + strokeRadius);
                float radiusSq = radius * radius;
                float strokeSq = strokeRadius * strokeRadius;
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        float dx = x - center.x;
                        float dy = y - center.y;
                        float distSq = dx * dx + dy * dy;
                        if (distSq <= radiusSq)
                            BlendPixel(pixels, y * TextureSize + x, fill);
                        else if (distSq <= strokeSq)
                            BlendPixel(pixels, y * TextureSize + x, stroke);
                    }
                }
            }
        }

        private static void DrawRoads(
            Color[] pixels,
            MapPolyline[] roads,
            Color color,
            int radius)
        {
            if (roads == null) return;
            for (int road = 0; road < roads.Length; road++)
            {
                MapPoint[] points = roads[road]?.points;
                if (points == null) continue;
                for (int point = 1; point < points.Length; point++)
                {
                    Vector2 a = WorldToPixel(points[point - 1].x, points[point - 1].z);
                    Vector2 b = WorldToPixel(points[point].x, points[point].z);
                    int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b) * 1.5f));
                    for (int step = 0; step <= steps; step++)
                    {
                        Vector2 position = Vector2.Lerp(a, b, step / (float)steps);
                        PaintSquare(pixels, Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), radius, color);
                    }
                }
            }
        }

        private static void DrawBuildings(
            Color[] pixels,
            MapBuilding[] buildings,
            Color color)
        {
            if (buildings == null) return;
            for (int i = 0; i < buildings.Length; i++)
            {
                MapBuilding building = buildings[i];
                Vector2 center = WorldToPixel(building.x, building.z);
                float halfW = Mathf.Max(1f, building.w / WorldSizeM * TextureSize * 0.5f);
                float halfD = Mathf.Max(1f, building.d / WorldSizeM * TextureSize * 0.5f);
                int radius = Mathf.CeilToInt(Mathf.Sqrt(halfW * halfW + halfD * halfD));
                float yaw = building.yawDeg * Mathf.Deg2Rad;
                float cos = Mathf.Cos(yaw);
                float sin = Mathf.Sin(yaw);
                for (int y = Mathf.Max(0, Mathf.FloorToInt(center.y) - radius);
                    y <= Mathf.Min(TextureSize - 1, Mathf.CeilToInt(center.y) + radius);
                    y++)
                {
                    for (int x = Mathf.Max(0, Mathf.FloorToInt(center.x) - radius);
                        x <= Mathf.Min(TextureSize - 1, Mathf.CeilToInt(center.x) + radius);
                        x++)
                    {
                        float dx = x - center.x;
                        float dy = y - center.y;
                        float localX = dx * cos - dy * sin;
                        float localY = dx * sin + dy * cos;
                        if (Mathf.Abs(localX) <= halfW && Mathf.Abs(localY) <= halfD)
                            BlendPixel(pixels, y * TextureSize + x, color);
                    }
                }
            }
        }

        private static void PaintSquare(Color[] pixels, int centerX, int centerY, int radius, Color color)
        {
            for (int y = Mathf.Max(0, centerY - radius);
                y <= Mathf.Min(TextureSize - 1, centerY + radius);
                y++)
                for (int x = Mathf.Max(0, centerX - radius);
                    x <= Mathf.Min(TextureSize - 1, centerX + radius);
                    x++)
                    BlendPixel(pixels, y * TextureSize + x, color);
        }

        private static Vector2 WorldToPixel(float x, float z)
        {
            return new Vector2(
                (x / WorldSizeM + 0.5f) * (TextureSize - 1),
                (z / WorldSizeM + 0.5f) * (TextureSize - 1));
        }

        private static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private static void DestroyObject(UnityEngine.Object value)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }

        private static void BlendPixel(Color[] pixels, int index, Color source)
        {
            Color destination = pixels[index];
            float alpha = Mathf.Clamp01(source.a);
            Color blended = source * alpha + destination * (1f - alpha);
            blended.a = 1f;
            pixels[index] = blended;
        }

        private static Color Opaque(Color color)
        {
            color.a = 1f;
            return color;
        }

        private readonly struct HeightRange
        {
            public HeightRange(float min, float max)
            {
                Min = min;
                Max = max;
                Span = max - min;
            }

            public readonly float Min;
            public readonly float Max;
            public readonly float Span;
        }

        private readonly struct MinimapPalette
        {
            private MinimapPalette(
                Color baseColor,
                Color hard,
                Color soft,
                Color water,
                Color waterStroke,
                Color roadCasing,
                Color roadFill,
                Color buildingFill)
            {
                Base = baseColor;
                Hard = hard;
                Soft = soft;
                Water = water;
                WaterStroke = waterStroke;
                RoadCasing = roadCasing;
                RoadFill = roadFill;
                BuildingFill = buildingFill;
            }

            public readonly Color Base;
            public readonly Color Hard;
            public readonly Color Soft;
            public readonly Color Water;
            public readonly Color WaterStroke;
            public readonly Color RoadCasing;
            public readonly Color RoadFill;
            public readonly Color BuildingFill;

            public static MinimapPalette From(MapDefinition map)
            {
                MapSurface surface = map.unitySurface;
                MapMinimap minimap = map.minimap;
                Color fallbackBase = surface != null
                    ? surface.groundColor.ToColor()
                    : new Color(0.28f, 0.34f, 0.22f);
                Color fallbackHard = surface != null
                    ? surface.hardColor.ToColor()
                    : new Color(0.41f, 0.38f, 0.31f);
                Color fallbackSoft = surface != null
                    ? surface.softColor.ToColor()
                    : new Color(0.19f, 0.27f, 0.21f);
                Color fallbackWater = surface != null
                    ? surface.waterColor.ToColor(0.7f)
                    : new Color(0.2f, 0.33f, 0.32f, 0.7f);
                Color fallbackRoadCasing = surface != null
                    ? surface.roadCasingColor.ToColor(0.9f)
                    : new Color(0.18f, 0.16f, 0.11f, 0.9f);
                Color fallbackRoadFill = surface != null
                    ? surface.roadColor.ToColor(0.95f)
                    : new Color(0.77f, 0.7f, 0.55f, 0.95f);
                Color building = map.unityStructures != null
                    ? Color.Lerp(map.unityStructures.buildingColor.ToColor(), Color.black, 0.68f)
                    : new Color(0.22f, 0.2f, 0.16f);

                return new MinimapPalette(
                    RgbTriplet(minimap?.@base, fallbackBase),
                    RgbTriplet(minimap?.hard, fallbackHard),
                    RgbTriplet(minimap?.soft, fallbackSoft),
                    CssColor(minimap?.water, fallbackWater),
                    CssColor(minimap?.waterStroke, new Color(0.11f, 0.19f, 0.19f, 0.8f)),
                    CssColor(minimap?.roadCasing, fallbackRoadCasing),
                    CssColor(minimap?.roadFill, fallbackRoadFill),
                    BuildingColor(minimap?.buildingFill, building));
            }

            private static Color RgbTriplet(int[] value, Color fallback)
            {
                if (value == null || value.Length < 3) return Opaque(fallback);
                return new Color(
                    Mathf.Clamp(value[0], 0, 255) / 255f,
                    Mathf.Clamp(value[1], 0, 255) / 255f,
                    Mathf.Clamp(value[2], 0, 255) / 255f,
                    1f);
            }

            private static Color BuildingColor(string value, Color fallback)
            {
                Color parsed = CssColor(value, fallback);
                if (!string.IsNullOrEmpty(value) &&
                    value.TrimStart().StartsWith("#", StringComparison.Ordinal))
                {
                    parsed.r *= 0.32f;
                    parsed.g *= 0.32f;
                    parsed.b *= 0.32f;
                    parsed.a = 0.9f;
                }
                return parsed;
            }

            private static Color CssColor(string value, Color fallback)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return fallback;
                string trimmed = value.Trim();
                if (ColorUtility.TryParseHtmlString(trimmed, out Color html))
                    return html;
                int start = trimmed.IndexOf('(');
                int end = trimmed.LastIndexOf(')');
                if (start < 0 || end <= start) return fallback;
                string function = trimmed.Substring(0, start).Trim().ToLowerInvariant();
                if (function != "rgb" && function != "rgba") return fallback;
                string[] parts = trimmed.Substring(start + 1, end - start - 1)
                    .Split(',');
                if (parts.Length < 3) return fallback;
                if (!TryParseComponent(parts[0], 255f, out float r) ||
                    !TryParseComponent(parts[1], 255f, out float g) ||
                    !TryParseComponent(parts[2], 255f, out float b))
                {
                    return fallback;
                }
                float alpha = 1f;
                if (parts.Length >= 4 &&
                    !TryParseComponent(parts[3], 1f, out alpha))
                {
                    return fallback;
                }
                return new Color(
                    Mathf.Clamp01(r),
                    Mathf.Clamp01(g),
                    Mathf.Clamp01(b),
                    Mathf.Clamp01(alpha));
            }

            private static bool TryParseComponent(
                string source,
                float divisor,
                out float value)
            {
                if (!float.TryParse(
                        source.Trim(),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float parsed))
                {
                    value = 0f;
                    return false;
                }
                value = divisor > 1f ? parsed / divisor : parsed;
                return true;
            }
        }

        private sealed class Marker
        {
            public Marker(RectTransform root, Text label)
            {
                Root = root;
                Label = label;
            }

            public RectTransform Root { get; }
            public Text Label { get; }
        }
    }
}
