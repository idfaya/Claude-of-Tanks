using System;
using System.Collections.Generic;
using System.IO;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class ReplayArchiveTests
    {
        [Test]
        public void ArchivePersistsLoadsAndPrunesOldestReplays()
        {
            string directory = TemporaryDirectory();
            long clock = 1000;
            int sequence = 0;
            try
            {
                ReplayArchive archive = new ReplayArchive(
                    directory,
                    () => clock++,
                    () => "replay_" + (++sequence).ToString("D3"));
                for (int i = 0; i < ReplayArchive.MaximumEntries + 2; i++)
                    archive.Save(Recording(i), "verdant", "alpha");

                ReplayArchiveEntry[] entries = archive.List();
                Assert.That(entries, Has.Length.EqualTo(ReplayArchive.MaximumEntries));
                Assert.That(entries[0].Id, Is.EqualTo("replay_014"));
                Assert.That(entries[entries.Length - 1].Id, Is.EqualTo("replay_003"));
                Assert.That(entries[0].PlayerVehicleId, Is.EqualTo("medium"));
                Assert.That(entries[0].FrameCount, Is.EqualTo(30));

                ArchivedReplay loaded = archive.Load(entries[0].Id);
                BattleSimulation replay = BattleReplayPlayer.Play(loaded.Recording);
                Assert.That(replay.State.Tanks[0].Position.Z, Is.GreaterThan(0f));
                Assert.That(loaded.Entry.MapId, Is.EqualTo("verdant"));
                Assert.That(archive.Delete(entries[0].Id), Is.True);
                Assert.That(archive.Delete(entries[0].Id), Is.False);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void ArchiveRejectsTraversalAndSkipsCorruptFiles()
        {
            string directory = TemporaryDirectory();
            try
            {
                ReplayArchive archive = new ReplayArchive(
                    directory,
                    () => 5000,
                    () => "valid_replay");
                ReplayArchiveEntry entry =
                    archive.Save(Recording(0), "verdant", "alpha");
                string path = Path.Combine(directory, entry.Id + ".cotreplay");
                byte[] bytes = File.ReadAllBytes(path);
                bytes[bytes.Length - 1] ^= 0x5a;
                File.WriteAllBytes(path, bytes);

                Assert.That(archive.List(), Is.Empty);
                Assert.Throws<FormatException>(() => archive.Load(entry.Id));
                Assert.Throws<ArgumentException>(() => archive.Load("../escape"));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private static ReplayRecording Recording(int seed)
        {
            BattleState state = new BattleState(new FlatHeightField(), (uint)(700 + seed));
            state.Tanks.Add(new TankState(
                "alpha", Team.Alpha, TankSpec.Medium(), Float3.Zero, 0f));
            state.Tanks.Add(new TankState(
                "bravo", Team.Bravo, TankSpec.Heavy(), new Float3(0f, 0f, 80f), MathUtil.Pi));
            BattleReplayRecorder recorder =
                new BattleReplayRecorder(state, GameModeId.Standard);
            Dictionary<string, TankInput> inputs = new Dictionary<string, TankInput>();
            for (int i = 0; i < 30; i++)
            {
                inputs["alpha"] = new TankInput
                {
                    Throttle = 1f,
                    AimPoint = state.Tanks[1].Position
                };
                inputs["bravo"] = new TankInput { AimPoint = state.Tanks[0].Position };
                recorder.Record(inputs, BattleState.FixedDeltaTime);
            }
            return recorder.Recording;
        }

        private static string TemporaryDirectory()
        {
            return Path.Combine(
                Path.GetTempPath(),
                "cot-replay-archive-" + Guid.NewGuid().ToString("N"));
        }
    }
}
