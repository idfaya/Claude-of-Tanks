using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClaudeOfTanks.Tests
{
    public sealed class BattleHudTests
    {
        [Test]
        public void DamageStateAndBattleSummaryUseAuthoritativeValues()
        {
            BattleHud hud = BattleHud.Create(() => { }, () => { });
            TankState player = new TankState(
                "player", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
            player.Combat.Modules["engine"].Condition = DamageModuleCondition.Red;
            player.Combat.Crew["gunner"] = false;
            player.Combat.Fire.Burning = true;
            MatchModeState mode = new MatchModeState(GameModeId.Standard)
            {
                Winner = Team.Alpha
            };
            BattleHudStats stats = new BattleHudStats
            {
                ShotsFired = 5,
                Hits = 3,
                Penetrations = 2,
                Kills = 1,
                DamageDealt = 780f,
                DamageReceived = 240f,
                TimeS = 125f
            };

            try
            {
                hud.SetState(player, mode, "VICTORY", true, stats);

                Assert.That(hud.DamageSummary, Does.Contain("FIRE"));
                Assert.That(hud.DamageSummary, Does.Contain("ENGINE RED"));
                Assert.That(hud.DamageSummary, Does.Contain("GUNNER OUT"));
                Assert.That(hud.ResultVisible, Is.True);
                Assert.That(hud.ResultSummary, Does.Contain("DAMAGE 780"));
                Assert.That(hud.ResultSummary, Does.Contain("HITS 3/5 (60%)"));
                Assert.That(hud.ResultSummary, Does.Contain("TIME 02:05"));

                hud.SetState(player, mode, string.Empty, false, default);
                Assert.That(hud.ResultVisible, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(hud.gameObject);
            }
        }

        [Test]
        public void MinimapUsesMapSurfaceAndHidesUnspottedEnemies()
        {
            BattleHud hud = BattleHud.Create(() => { }, () => { });
            TankSpec spec = TankSpec.Medium();
            TankState player = new TankState(
                "player", Team.Alpha, spec, Float3.Zero, 0f);
            TankState ally = new TankState(
                "ally", Team.Alpha, spec, new Float3(20f, 0f, 20f), 0.4f);
            TankState hiddenEnemy = new TankState(
                "hidden", Team.Bravo, spec, new Float3(0f, 0f, -300f), 0f);
            TankState visibleEnemy = new TankState(
                "visible", Team.Bravo, spec, new Float3(0f, 0f, 100f), 0f);
            List<TankState> tanks = new List<TankState>
                { player, ally, hiddenEnemy, visibleEnemy };
            MatchModeState mode = new MatchModeState(GameModeId.ZoneControl);
            mode.Zones[0] = new Float3(-100f, 0f, 0f);
            mode.Zones[1] = Float3.Zero;
            mode.Zones[2] = new Float3(100f, 0f, 0f);

            try
            {
                hud.SetMap(ContentCatalog.Load().GetMap("coastal"));
                RawImage map = hud.transform.Find("Minimap/Map").GetComponent<RawImage>();
                Assert.That(map.texture, Is.Not.Null);
                Assert.That(map.texture.width, Is.EqualTo(BattleMinimap.TextureSize));
                Assert.That(map.texture.height, Is.EqualTo(BattleMinimap.TextureSize));

                hud.SetMinimap(player, tanks, mode, new SpottingSimulation());
                Assert.That(hud.MinimapTankMarkers, Is.EqualTo(3));
                Assert.That(hud.MinimapEnemyMarkers, Is.EqualTo(1));
                Assert.That(hud.MinimapObjectiveMarkers, Is.EqualTo(3));
                Assert.That(
                    hud.transform.Find("Minimap/Map/Tank-3").gameObject.activeSelf,
                    Is.False);

                RectTransform root = hud.transform.Find("Minimap").GetComponent<RectTransform>();
                hud.SetTouchVisible(true);
                Assert.That(root.anchorMin, Is.EqualTo(new Vector2(0f, 1f)));
                Assert.That(root.rect.width, Is.EqualTo(150f).Within(0.1f));
                hud.SetTouchVisible(false);
                Assert.That(root.anchorMin, Is.EqualTo(new Vector2(1f, 0f)));
                Assert.That(root.rect.width, Is.EqualTo(220f).Within(0.1f));
            }
            finally
            {
                Object.DestroyImmediate(hud.gameObject);
            }
        }

        [Test]
        public void MinimapUsesMapPaletteForBackgroundAndFeatureLayers()
        {
            BattleHud hud = BattleHud.Create(() => { }, () => { });
            MapDefinition map = new MapDefinition
            {
                id = "palette-test",
                minimap = new MapMinimap
                {
                    @base = new[] { 10, 20, 30 },
                    hard = new[] { 90, 100, 110 },
                    soft = new[] { 5, 15, 25 },
                    water = "rgba(110,130,150,0.5)",
                    waterStroke = "rgba(210,220,230,1)",
                    roadCasing = "rgba(200,10,20,1)",
                    roadFill = "rgba(20,220,40,1)",
                    buildingFill = "#f0c060"
                },
                terrain = new MapTerrain
                {
                    landforms = new LandformDefinition[0]
                },
                unitySurface = new MapSurface
                {
                    roads = new[]
                    {
                        new MapPolyline
                        {
                            points = new[]
                            {
                                new MapPoint { x = -200f, z = 150f },
                                new MapPoint { x = 200f, z = 150f }
                            }
                        }
                    },
                    lakes = new[]
                    {
                        new MapDisc { x = -250f, z = -150f, r = 60f }
                    },
                    marshes = new MapDisc[0],
                    groundColor = ColorDef(0.8f, 0.1f, 0.1f),
                    hardColor = ColorDef(0.1f, 0.8f, 0.1f),
                    softColor = ColorDef(0.1f, 0.1f, 0.8f),
                    roadColor = ColorDef(0.8f, 0.8f, 0.8f),
                    roadCasingColor = ColorDef(0.2f, 0.2f, 0.2f),
                    waterColor = ColorDef(0.1f, 0.2f, 0.9f)
                },
                unityStructures = new MapStructures
                {
                    buildingColor = ColorDef(0.8f, 0.8f, 0.8f),
                    buildings = new[]
                    {
                        new MapBuilding
                        {
                            x = 250f,
                            z = -150f,
                            w = 48f,
                            d = 48f,
                            h = 8f
                        }
                    },
                    walls = new MapWall[0]
                }
            };

            try
            {
                hud.SetMap(map);
                Texture2D texture = (Texture2D)hud.transform
                    .Find("Minimap/Map")
                    .GetComponent<RawImage>()
                    .texture;

                AssertColor32Near(
                    texture.GetPixel(5, 5),
                    new Color32(10, 20, 30, 255),
                    1,
                    "base palette");
                AssertColor32Near(
                    texture.GetPixel(
                        PixelX(-250f),
                        PixelY(-150f)),
                    new Color32(60, 75, 90, 255),
                    2,
                    "water palette alpha-composited over base");
                AssertColor32Near(
                    texture.GetPixel(
                        PixelX(0f),
                        PixelY(150f)),
                    new Color32(20, 220, 40, 255),
                    1,
                    "road fill palette");
                AssertColor32Near(
                    texture.GetPixel(
                        PixelX(0f),
                        PixelY(150f) + 3),
                    new Color32(200, 10, 20, 255),
                    1,
                    "road casing palette");
                AssertColor32Near(
                    texture.GetPixel(
                        PixelX(250f),
                        PixelY(-150f)),
                    new Color32(70, 57, 31, 255),
                    3,
                    "building palette darkened like TS minimap");
            }
            finally
            {
                Object.DestroyImmediate(hud.gameObject);
            }
        }

        [Test]
        public void EveryMapBuildsDistinctMinimapTexture()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            BattleHud hud = BattleHud.Create(() => { }, () => { });
            HashSet<Color32> centerColors = new HashSet<Color32>();
            try
            {
                for (int i = 0; i < catalog.Maps.Length; i++)
                {
                    hud.SetMap(catalog.Maps[i]);
                    Texture2D texture = (Texture2D)hud.transform
                        .Find("Minimap/Map")
                        .GetComponent<RawImage>()
                        .texture;
                    Assert.That(texture.name, Is.EqualTo("Minimap-" + catalog.Maps[i].id));
                    centerColors.Add(texture.GetPixel(
                        BattleMinimap.TextureSize / 2,
                        BattleMinimap.TextureSize / 2));
                }
                Assert.That(centerColors.Count, Is.GreaterThan(8));
            }
            finally
            {
                Object.DestroyImmediate(hud.gameObject);
            }
        }

        private static MapColor ColorDef(float r, float g, float b)
        {
            return new MapColor { r = r, g = g, b = b };
        }

        private static int PixelX(float worldX)
        {
            return Mathf.RoundToInt(
                (worldX / BattleMinimap.WorldSizeM + 0.5f) *
                (BattleMinimap.TextureSize - 1));
        }

        private static int PixelY(float worldZ)
        {
            return Mathf.RoundToInt(
                (worldZ / BattleMinimap.WorldSizeM + 0.5f) *
                (BattleMinimap.TextureSize - 1));
        }

        private static void AssertColor32Near(
            Color color,
            Color32 expected,
            int tolerance,
            string label)
        {
            Color32 actual = color;
            Assert.That(Mathf.Abs(actual.r - expected.r), Is.LessThanOrEqualTo(tolerance), label + " r");
            Assert.That(Mathf.Abs(actual.g - expected.g), Is.LessThanOrEqualTo(tolerance), label + " g");
            Assert.That(Mathf.Abs(actual.b - expected.b), Is.LessThanOrEqualTo(tolerance), label + " b");
            Assert.That(Mathf.Abs(actual.a - expected.a), Is.LessThanOrEqualTo(tolerance), label + " a");
        }

        [Test]
        public void TouchControlsUseSafeAnchorsAndIndependentHeldState()
        {
            BattleHud hud = BattleHud.Create(() => { }, () => { });
            try
            {
                hud.SetTouchVisible(true);
                RectTransform root = hud.transform.Find("TouchControls")
                    .GetComponent<RectTransform>();
                Assert.That(root.anchorMin, Is.EqualTo(Vector2.zero));
                Assert.That(root.anchorMax, Is.EqualTo(Vector2.one));

                RectTransform drive = root.Find("DrivePad").GetComponent<RectTransform>();
                RectTransform fire = root.Find("Fire").GetComponent<RectTransform>();
                RectTransform brake = root.Find("Brake").GetComponent<RectTransform>();
                RectTransform scope = root.Find("Sniper").GetComponent<RectTransform>();
                RectTransform aim = root.Find("AimSurface").GetComponent<RectTransform>();
                Assert.That(drive.anchorMin, Is.EqualTo(Vector2.zero));
                Assert.That(fire.anchorMin, Is.EqualTo(new Vector2(1f, 0f)));
                Assert.That(brake.anchorMin, Is.EqualTo(new Vector2(1f, 0f)));
                Assert.That(scope.anchorMin, Is.EqualTo(new Vector2(1f, 0f)));
                Assert.That(aim.anchorMin.x, Is.GreaterThanOrEqualTo(0.35f));
                Assert.That(aim.anchorMin.y, Is.GreaterThanOrEqualTo(0.18f));
                RectTransform repair = hud.transform.Find("Repair").GetComponent<RectTransform>();
                hud.SetTouchLayoutForViewport(720, 1280);
                Assert.That(repair.offsetMin.y, Is.EqualTo(250f));
                hud.SetTouchLayoutForViewport(1280, 720);
                Assert.That(repair.offsetMin.y, Is.EqualTo(20f));

                HoldControl forward = drive.Find("Forward").GetComponent<HoldControl>();
                HoldControl left = drive.Find("Left").GetComponent<HoldControl>();
                forward.OnPointerDown(null);
                left.OnPointerDown(null);
                Assert.That(hud.TouchDrive, Is.EqualTo(new Vector2(-1f, 1f)));
                forward.OnPointerUp(null);
                Assert.That(hud.TouchDrive, Is.EqualTo(new Vector2(-1f, 0f)));
                left.OnPointerUp(null);
                Assert.That(hud.TouchDrive, Is.EqualTo(Vector2.zero));

                HoldControl brakeHold = brake.GetComponent<HoldControl>();
                brakeHold.OnPointerDown(null);
                Assert.That(hud.BrakeHeld, Is.True);
                brakeHold.OnPointerUp(null);
                Assert.That(hud.BrakeHeld, Is.False);

                scope.GetComponent<Button>().onClick.Invoke();
                Assert.That(hud.ConsumeSniperToggle(), Is.True);
                Assert.That(hud.ConsumeSniperToggle(), Is.False);
                root.Find("Shell2")
                    .GetComponent<Button>()
                    .onClick.Invoke();
                Assert.That(
                    hud.ConsumeShellSlot(0, 3),
                    Is.EqualTo(1));
                Assert.That(
                    hud.ConsumeShellSlot(0, 3),
                    Is.EqualTo(0));

                TouchAimControl aimControl = aim.GetComponent<TouchAimControl>();
                PointerEventData pointer = new PointerEventData(EventSystem.current)
                {
                    position = new Vector2(900f, 420f)
                };
                aimControl.OnPointerDown(pointer);
                Vector2 aimPosition;
                Assert.That(hud.TryGetTouchAimPosition(out aimPosition), Is.True);
                Assert.That(aimPosition, Is.EqualTo(pointer.position));
                aimControl.OnPointerUp(pointer);
                Assert.That(hud.TryGetTouchAimPosition(out aimPosition), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(hud.gameObject);
            }
        }

        [Test]
        public void ResultScreenOffersKillcamAndFullReplayControls()
        {
            int killcam = 0;
            int fullReplay = 0;
            int exit = 0;
            BattleHud hud = BattleHud.Create(() => { }, () => { });
            try
            {
                hud.ConfigureReplayActions(
                    () => killcam++,
                    () => fullReplay++,
                    () => exit++);
                TankState tank = new TankState(
                    "player", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f);
                hud.SetState(
                    tank,
                    new MatchModeState(GameModeId.Standard),
                    "VICTORY",
                    true,
                    new BattleHudStats());
                Assert.That(hud.ResultVisible, Is.True);
                hud.transform.Find("BattleResult/Killcam")
                    .GetComponent<Button>().onClick.Invoke();
                hud.transform.Find("BattleResult/FullReplay")
                    .GetComponent<Button>().onClick.Invoke();
                Assert.That(killcam, Is.EqualTo(1));
                Assert.That(fullReplay, Is.EqualTo(1));

                hud.SetReplayState(true, true, 3.2f, 8f, false);
                Assert.That(hud.ResultVisible, Is.False);
                Assert.That(hud.ReplayVisible, Is.True);
                Text status = hud.transform.Find("ReplayOverlay/Band/Status")
                    .GetComponent<Text>();
                Assert.That(status.text, Does.Contain("KILLCAM"));
                Assert.That(status.text, Does.Contain("00:03 / 00:08"));
                hud.transform.Find("ReplayOverlay/Band/ExitReplay")
                    .GetComponent<Button>().onClick.Invoke();
                Assert.That(exit, Is.EqualTo(1));
                hud.SetReplayState(false, false, 0f, 0f, false);
                Assert.That(hud.ReplayVisible, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(hud.gameObject);
            }
        }

        [Test]
        public void AccessibilityAndInputPromptsFollowSettingsAndActiveDevice()
        {
            MemorySettingsStore store = new MemorySettingsStore();
            GameSettings settings = new GameSettings(
                store,
                new SettingsTarget());
            BattleHud hud = BattleHud.Create(
                () => { },
                () => { },
                settings);
            try
            {
                Transform prompts = hud.transform.Find("InputPrompts");
                Assert.That(prompts, Is.Not.Null);
                Assert.That(
                    prompts.Find("Fire").GetComponent<Text>().text,
                    Does.Contain("SPACE"));
                settings.SetBinding(GameInputAction.Fire, KeyCode.F);
                Assert.That(
                    prompts.Find("Fire").GetComponent<Text>().text,
                    Is.EqualTo("FIRE  F"));

                hud.SetInputDevice(BattleInputDevice.Gamepad);
                Assert.That(
                    prompts.Find("Fire").GetComponent<Text>().text,
                    Is.EqualTo("FIRE  RT"));
                Assert.That(
                    hud.transform.Find("Repair/Label")
                        .GetComponent<Text>().text,
                    Is.EqualTo("X"));
                Assert.That(
                    hud.transform.Find("FirstAid/Label")
                        .GetComponent<Text>().text,
                    Is.EqualTo("Y"));
                Assert.That(
                    hud.transform.Find("Extinguish/Label")
                        .GetComponent<Text>().text,
                    Is.EqualTo("B"));

                settings.SetHudScale(1.25f);
                settings.SetHighContrast(true);
                Image panel = hud.transform.Find("VehiclePanel")
                    .GetComponent<Image>();
                Assert.That(
                    prompts.Find("Fire").GetComponent<Text>().fontSize,
                    Is.EqualTo(15));
                Assert.That(panel.color.a, Is.GreaterThanOrEqualTo(0.98f));
            }
            finally
            {
                Object.DestroyImmediate(hud.gameObject);
            }
        }

        [Test]
        public void SettingsOverlayOwnsBattlePauseAndUiFeedback()
        {
            GameSettings settings = new GameSettings(
                new MemorySettingsStore(),
                new SettingsTarget());
            BattleHud hud = BattleHud.Create(
                () => { },
                () => { },
                settings);
            try
            {
                Button[] buttons =
                    hud.GetComponentsInChildren<Button>(true);
                Assert.That(buttons.Length, Is.GreaterThan(6));
                for (int i = 0; i < buttons.Length; i++)
                {
                    Assert.That(
                        buttons[i].GetComponent<UiAudioButtonFeedback>(),
                        Is.Not.Null,
                        buttons[i].name);
                }

                hud.SetPaused(true);

                Assert.That(hud.IsPaused, Is.True);
                Assert.That(
                    hud.transform.Find(
                        "Settings/Shade/Surface/PauseStatus")
                        .gameObject.activeSelf,
                    Is.True);

                hud.SetPaused(false);

                Assert.That(hud.IsPaused, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(hud.gameObject);
            }
        }

        private sealed class MemorySettingsStore : ISettingsStore
        {
            private readonly Dictionary<string, int> _ints =
                new Dictionary<string, int>();
            private readonly Dictionary<string, float> _floats =
                new Dictionary<string, float>();

            public int GetInt(string key, int fallback)
            {
                int value;
                return _ints.TryGetValue(key, out value) ? value : fallback;
            }

            public float GetFloat(string key, float fallback)
            {
                float value;
                return _floats.TryGetValue(key, out value) ? value : fallback;
            }

            public void SetInt(string key, int value) { _ints[key] = value; }
            public void SetFloat(string key, float value) { _floats[key] = value; }
            public void Save() { }
        }

        private sealed class SettingsTarget : ISettingsTarget
        {
            public int QualityLevelCount => 3;
            public void ApplyVolume(float value) { }
            public void ApplyQuality(int value) { }
            public void ApplyFullscreen(bool value) { }
        }
    }
}
