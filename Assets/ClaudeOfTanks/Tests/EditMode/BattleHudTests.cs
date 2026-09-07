using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

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
    }
}
