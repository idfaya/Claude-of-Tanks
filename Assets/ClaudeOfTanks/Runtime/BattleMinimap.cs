using System;
using System.Collections.Generic;
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
            SpottingSimulation spotting)
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
                    if (enemy && (spotting == null || !spotting.CanSpot(player, tank))) continue;
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
            Color ground = surface != null
                ? surface.groundColor.ToColor()
                : new Color(0.28f, 0.34f, 0.22f);
            Color[] pixels = new Color[TextureSize * TextureSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = ground;
            if (surface != null)
            {
                DrawDiscs(pixels, surface.marshes, surface.softColor.ToColor());
                DrawDiscs(
                    pixels,
                    surface.lakes,
                    surface.frozenWater
                        ? Color.Lerp(surface.waterColor.ToColor(), Color.white, 0.48f)
                        : surface.waterColor.ToColor());
                DrawRoads(pixels, surface.roads, surface.roadCasingColor.ToColor(), 4);
                DrawRoads(pixels, surface.roads, surface.roadColor.ToColor(), 2);
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

        private static void DrawDiscs(Color[] pixels, MapDisc[] discs, Color color)
        {
            if (discs == null) return;
            for (int i = 0; i < discs.Length; i++)
            {
                Vector2 center = WorldToPixel(discs[i].x, discs[i].z);
                int radius = Mathf.Max(1, Mathf.RoundToInt(
                    discs[i].r / WorldSizeM * TextureSize));
                int minX = Mathf.Max(0, Mathf.FloorToInt(center.x) - radius);
                int maxX = Mathf.Min(TextureSize - 1, Mathf.CeilToInt(center.x) + radius);
                int minY = Mathf.Max(0, Mathf.FloorToInt(center.y) - radius);
                int maxY = Mathf.Min(TextureSize - 1, Mathf.CeilToInt(center.y) + radius);
                float radiusSq = radius * radius;
                for (int y = minY; y <= maxY; y++)
                    for (int x = minX; x <= maxX; x++)
                        if ((new Vector2(x, y) - center).sqrMagnitude <= radiusSq)
                            pixels[y * TextureSize + x] = color;
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

        private static void PaintSquare(Color[] pixels, int centerX, int centerY, int radius, Color color)
        {
            for (int y = Mathf.Max(0, centerY - radius);
                y <= Mathf.Min(TextureSize - 1, centerY + radius);
                y++)
                for (int x = Mathf.Max(0, centerX - radius);
                    x <= Mathf.Min(TextureSize - 1, centerX + radius);
                    x++)
                    pixels[y * TextureSize + x] = color;
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
