using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;
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
    }
}
