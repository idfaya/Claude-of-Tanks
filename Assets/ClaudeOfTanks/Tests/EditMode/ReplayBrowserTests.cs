using System;
using System.Collections.Generic;
using System.IO;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Tests
{
    public sealed class ReplayBrowserTests
    {
        [Test]
        public void BrowserListsPlaysAndDeletesArchivedReplay()
        {
            string directory = TemporaryDirectory();
            GameObject canvasObject = new GameObject("ReplayBrowserTest", typeof(Canvas));
            try
            {
                ReplayArchive archive = new ReplayArchive(
                    directory,
                    () => 1700000000000,
                    () => "browser_replay");
                ReplayArchiveEntry saved = archive.Save(Recording(), "verdant", "player");
                string played = null;
                ReplayBrowserPanel browser =
                    ReplayBrowserPanel.Create(canvasObject.transform, archive, id => played = id);

                browser.Open();
                Assert.That(browser.IsVisible, Is.True);
                Assert.That(browser.EntryCount, Is.EqualTo(1));
                Assert.That(browser.SelectedReplayId, Is.EqualTo(saved.Id));
                Text details = browser.transform.Find("Shade/Surface/Details").GetComponent<Text>();
                Assert.That(details.text, Does.Contain("VERDANT"));
                Assert.That(details.text, Does.Contain("M1A1"));
                browser.PlaySelected();
                Assert.That(played, Is.EqualTo(saved.Id));
                browser.DeleteSelected();
                Assert.That(browser.EntryCount, Is.EqualTo(0));
                Assert.That(archive.List(), Is.Empty);
                browser.Close();
                Assert.That(browser.IsVisible, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvasObject);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void BattleLoadsArchivedReplayWithCatalogVisualMappings()
        {
            string directory = TemporaryDirectory();
            GameObject root = new GameObject("ArchivedReplayBattle");
            bool exited = false;
            try
            {
                ReplayArchive archive = new ReplayArchive(
                    directory,
                    () => 1700000000000,
                    () => "playback_replay");
                ReplayArchiveEntry entry = archive.Save(Recording(), "verdant", "player");
                ArchivedReplay replay = archive.Load(entry.Id);
                root.SetActive(false);
                BattleController battle = root.AddComponent<BattleController>();
                battle.Configure("m1a1", "verdant", GameModeId.Standard, () => exited = true, archive);
                root.SetActive(true);
                battle.Initialize();
                battle.LoadArchivedReplay(replay);

                Assert.That(battle.IsReplaying, Is.True);
                Assert.That(battle.Player.Id, Is.EqualTo("player"));
                Assert.That(battle.Player.Spec.Id, Is.EqualTo("m1a1"));
                Assert.That(
                    battle.Player.Equipment,
                    Is.EqualTo(new[] { "toolbox" }));
                Assert.That(
                    battle.PlayerCamouflageId,
                    Is.EqualTo("winter"));
                Assert.That(battle.State.Tanks, Has.Count.EqualTo(2));
                Renderer playerHull = GameObject
                    .Find("player/Hull")
                    .GetComponent<Renderer>();
                Renderer enemyHull = GameObject
                    .Find("enemy/Hull")
                    .GetComponent<Renderer>();
                Assert.That(
                    playerHull.sharedMaterial.mainTexture.name,
                    Does.Contain("-winter-"));
                Assert.That(
                    enemyHull.sharedMaterial.mainTexture.name,
                    Does.Contain("-desert-"));
                battle.ExitReplay();
                Assert.That(exited, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private static ReplayRecording Recording()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            BattleState state = new BattleState(new FlatHeightField(), 611u);
            state.Tanks.Add(new TankState(
                "player",
                Team.Alpha,
                catalog.GetVehicle("m1a1").ToTankSpec(),
                Float3.Zero,
                0f));
            state.Tanks.Add(new TankState(
                "enemy",
                Team.Bravo,
                catalog.GetVehicle("t90m").ToTankSpec(),
                new Float3(0f, 0f, 80f),
                MathUtil.Pi));
            LoadoutSimulation.ApplyEquipment(
                state.Tanks[0],
                new[] { "toolbox" });
            BattleReplayRecorder recorder = new BattleReplayRecorder(
                state,
                GameModeId.Standard,
                new Dictionary<string, string>
                {
                    ["player"] = "winter",
                    ["enemy"] = "desert"
                });
            Dictionary<string, TankInput> inputs = new Dictionary<string, TankInput>();
            for (int frame = 0; frame < 60; frame++)
            {
                inputs["player"] = new TankInput
                {
                    Throttle = 1f,
                    AimPoint = state.Tanks[1].Position
                };
                inputs["enemy"] = new TankInput { AimPoint = state.Tanks[0].Position };
                recorder.Record(inputs, BattleState.FixedDeltaTime);
            }
            return recorder.Recording;
        }

        private static string TemporaryDirectory()
        {
            return Path.Combine(
                Path.GetTempPath(),
                "cot-replay-browser-" + Guid.NewGuid().ToString("N"));
        }
    }
}
