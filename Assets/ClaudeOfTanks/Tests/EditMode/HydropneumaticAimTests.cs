using System;
using System.Collections.Generic;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Tests
{
    public sealed class HydropneumaticAimTests
    {
        private static readonly string[] VehicleIds =
        {
            "udes03",
            "strv103",
            "strv103a",
            "stb1",
            "type74",
            "mbt70"
        };

        [Test]
        public void CatalogMapsAllHydropneumaticProfilesIntoSimulation()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            foreach (string id in VehicleIds)
            {
                HydropneumaticAimSpec spec =
                    catalog.GetVehicle(id)
                        .ToTankSpec()
                        .HydropneumaticAim;
                Assert.That(spec, Is.Not.Null, id);
                Assert.That(spec.IsValid, Is.True, id);
                Assert.That(spec.NoseDownRad, Is.GreaterThan(0f), id);
                Assert.That(spec.NoseUpRad, Is.GreaterThan(0f), id);
                Assert.That(spec.SpeedRadS, Is.GreaterThan(0f), id);
                Assert.That(spec.CompressionM, Is.GreaterThan(0f), id);
                Assert.That(spec.DroopM, Is.GreaterThan(0f), id);
                bool fixedGun =
                    id == "udes03" ||
                    id == "strv103" ||
                    id == "strv103a";
                Assert.That(
                    catalog.GetVehicle(id)
                        .ToTankSpec()
                        .FixedHydraulicGun,
                    Is.EqualTo(fixedGun),
                    id);
            }
            Assert.That(
                catalog.GetVehicle("m1a2")
                    .ToTankSpec()
                    .HydropneumaticAim,
                Is.Null);
        }

        [Test]
        public void FixedHydraulicGunPinsTurretAndAutoTraversesHull()
        {
            TankSpec spec = TankSpec.Medium();
            spec.FixedHydraulicGun = true;
            spec.HydropneumaticAim =
                new HydropneumaticAimSpec
                {
                    NoseDownRad = 0.2f,
                    NoseUpRad = 0.2f,
                    SpeedRadS = 0.2f
                };
            TankState tank = new TankState(
                "siege",
                Team.Alpha,
                spec,
                Float3.Zero,
                0f)
            {
                TurretYaw = 0.4f
            };

            TankMovement.Step(
                tank,
                new TankInput
                {
                    AimPoint =
                        new Float3(100f, 0f, 100f)
                },
                new FlatHeightField(),
                BattleState.FixedDeltaTime);

            Assert.That(tank.TurretYaw, Is.Zero);
            Assert.That(tank.Yaw, Is.GreaterThan(0f));
        }

        [Test]
        public void FixedStepToggleClampsAndRecentersDeterministically()
        {
            TankSpec spec = TankSpec.Medium();
            spec.HydropneumaticAim =
                new HydropneumaticAimSpec
                {
                    NoseDownRad = 10f * MathUtil.Deg2Rad,
                    NoseUpRad = 6f * MathUtil.Deg2Rad,
                    SpeedRadS = 12f * MathUtil.Deg2Rad
                };
            TankState first = new TankState(
                "first",
                Team.Alpha,
                spec,
                Float3.Zero,
                0f);
            TankState second = new TankState(
                "second",
                Team.Alpha,
                spec,
                Float3.Zero,
                0f);
            TankInput input = new TankInput
            {
                ToggleHydropneumaticAim = true,
                AimPoint = new Float3(0f, -100f, 100f)
            };
            StepBoth(first, second, input);
            input.ToggleHydropneumaticAim = false;
            for (int tick = 0; tick < 60; tick++)
                StepBoth(first, second, input);

            Assert.That(first.HydropneumaticAimActive, Is.True);
            Assert.That(
                first.HullPitchRad,
                Is.EqualTo(-10f * MathUtil.Deg2Rad)
                    .Within(0.00001f));
            Assert.That(
                second.HullPitchRad,
                Is.EqualTo(first.HullPitchRad));

            input.ToggleHydropneumaticAim = true;
            StepBoth(first, second, input);
            input.ToggleHydropneumaticAim = false;
            for (int tick = 0; tick < 60; tick++)
                StepBoth(first, second, input);
            Assert.That(first.HydropneumaticAimActive, Is.False);
            Assert.That(first.HullPitchRad, Is.EqualTo(0f).Within(0.00001f));
        }

        [Test]
        public void NetworkInputSnapshotAndBufferCarryAuthoritativePitch()
        {
            NetworkInputCommand command = new NetworkInputCommand
            {
                PlayerId = "peer",
                Sequence = 1u,
                ActionSequence = 1u,
                ClientTick = 1,
                SnapshotAckTick = -1,
                AimDistanceM = 100f,
                Actions = NetworkActionBits.HydropneumaticAim
            };
            NetworkInputCommand decodedCommand =
                InputWireCodec.Decode(
                    InputWireCodec.Encode(command));
            TankInput input = NetworkProtocol.ToTankInput(
                decodedCommand,
                new TankState(
                    "tank",
                    Team.Alpha,
                    TankSpec.Medium(),
                    Float3.Zero,
                    0f));
            Assert.That(input.ToggleHydropneumaticAim, Is.True);

            NetworkWorldSnapshot first = Snapshot(
                1,
                10.0,
                0.02f,
                true);
            NetworkWorldSnapshot second = Snapshot(
                2,
                20.0,
                0.1f,
                true);
            NetworkWorldSnapshot decoded =
                SnapshotWireCodec.Decode(
                    SnapshotWireCodec.Encode(second));
            Assert.That(
                decoded.Entities[0].HydropneumaticAimActive,
                Is.True);
            Assert.That(
                decoded.Entities[0].HullPitchRad,
                Is.EqualTo(0.1f));

            SnapshotBuffer buffer = new SnapshotBuffer();
            buffer.Push(first);
            buffer.Push(second);
            SampledEntityState sample;
            Assert.That(
                buffer.TrySampleEntity(
                    "tank",
                    15.0,
                    out sample),
                Is.True);
            Assert.That(
                sample.HullPitchRad,
                Is.EqualTo(0.06f).Within(0.00001f));
            Assert.That(
                sample.HydropneumaticAimActive,
                Is.True);
        }

        [Test]
        public void ReplayVersionSixPreservesProfileAndToggle()
        {
            TankSpec spec = TankSpec.Medium();
            spec.HydropneumaticAim =
                new HydropneumaticAimSpec
                {
                    NoseDownRad = 0.18f,
                    NoseUpRad = 0.1f,
                    SpeedRadS = 0.22f,
                    CompressionM = 0.5f,
                    DroopM = 0.4f
                };
            spec.FixedHydraulicGun = true;
            BattleState state = new BattleState(
                new FlatHeightField(),
                89u);
            state.Tanks.Add(
                new TankState(
                    "tank",
                    Team.Alpha,
                    spec,
                    Float3.Zero,
                    0f));
            BattleReplayRecorder recorder =
                new BattleReplayRecorder(
                    state,
                    GameModeId.Standard);
            recorder.Record(
                new Dictionary<string, TankInput>
                {
                    ["tank"] = new TankInput
                    {
                        ToggleHydropneumaticAim = true,
                        AimPoint = new Float3(0f, 20f, 100f)
                    }
                },
                BattleState.FixedDeltaTime);

            byte[] packet = ReplayWireCodec.Encode(
                recorder.Recording);
            ReplayRecording decoded =
                ReplayWireCodec.Decode(packet);
            BattleSimulation replay =
                BattleReplayPlayer.Play(decoded);

            Assert.That(
                replay.State.Tanks[0]
                    .Spec.HydropneumaticAim.NoseDownRad,
                Is.EqualTo(0.18f));
            Assert.That(
                replay.State.Tanks[0]
                    .Spec.HydropneumaticAim.CompressionM,
                Is.EqualTo(0.5f));
            Assert.That(
                replay.State.Tanks[0]
                    .Spec.HydropneumaticAim.DroopM,
                Is.EqualTo(0.4f));
            Assert.That(
                replay.State.Tanks[0]
                    .HydropneumaticAimActive,
                Is.True);
            Assert.That(
                replay.State.Tanks[0]
                    .Spec.FixedHydraulicGun,
                Is.True);
            Assert.That(
                replay.State.Tanks[0].HullPitchRad,
                Is.GreaterThan(0f));
        }

        [Test]
        public void ReplayVersionFiveDefaultsHydropneumaticMetadata()
        {
            BattleState state = new BattleState(
                new FlatHeightField(),
                90u);
            state.Tanks.Add(
                new TankState(
                    "tank",
                    Team.Alpha,
                    TankSpec.Medium(),
                    Float3.Zero,
                    0f));
            BattleReplayRecorder recorder =
                new BattleReplayRecorder(
                    state,
                    GameModeId.Standard);
            byte[] packet = ReplayWireCodec.Encode(
                recorder.Recording);
            int combatMetadata =
                FindMagic(packet, 0x434d4f43u);
            Array.Resize(
                ref packet,
                combatMetadata - 2);
            packet[4] = 5;
            packet[5] = 0;

            ReplayRecording decoded =
                ReplayWireCodec.Decode(packet);

            Assert.That(
                BattleReplayPlayer.Play(decoded)
                    .State.Tanks[0]
                    .Spec.HydropneumaticAim,
                Is.Null);
        }

        private static int FindMagic(
            byte[] packet,
            uint magic)
        {
            for (int i = packet.Length - 4;
                i >= 0;
                i--)
            {
                if (BitConverter.ToUInt32(
                        packet,
                        i) == magic)
                {
                    return i;
                }
            }
            Assert.Fail(
                "Replay metadata magic was not found.");
            return -1;
        }

        [Test]
        public void ViewAndHudPresentTheAuthoritativeState()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("mbt70");
            TankState tank = new TankState(
                "tank",
                Team.Alpha,
                definition.ToTankSpec(),
                Float3.Zero,
                0f)
            {
                HydropneumaticAimActive = true,
                HullPitchRad = 0.08f
            };
            TankView view = TankView.Create(
                tank,
                definition,
                "factory",
                "forest",
                catalog);
            BattleHud hud = BattleHud.Create(
                () => { },
                () => { });
            try
            {
                view.Sync(tank);
                Quaternion expected =
                    Quaternion.Euler(
                        -0.08f * Mathf.Rad2Deg,
                        0f,
                        0f);
                Assert.That(
                    Quaternion.Angle(
                        view.Root.transform.rotation,
                        expected),
                    Is.LessThan(0.001f));

                hud.SetState(
                    tank,
                    new MatchModeState(GameModeId.Standard),
                    string.Empty,
                    false,
                    default);
                Button button = hud.transform
                    .Find("HydropneumaticAim")
                    .GetComponent<Button>();
                Assert.That(button.gameObject.activeSelf, Is.True);
                Assert.That(
                    button.GetComponentInChildren<Text>().text,
                    Is.EqualTo("E ON"));
                hud.SetTouchLayoutForViewport(1280, 720);
                RectTransform rect =
                    button.GetComponent<RectTransform>();
                Assert.That(
                    rect.offsetMin,
                    Is.EqualTo(new Vector2(430f, 76f)));
                Assert.That(
                    rect.rect.size,
                    Is.EqualTo(new Vector2(64f, 40f)));
                button.onClick.Invoke();
                Assert.That(
                    hud.ConsumeHydropneumaticToggle(),
                    Is.True);
                Assert.That(
                    hud.ConsumeHydropneumaticToggle(),
                    Is.False);
            }
            finally
            {
                view.Destroy();
                UnityEngine.Object.DestroyImmediate(
                    hud.gameObject);
            }
        }

        private static void StepBoth(
            TankState first,
            TankState second,
            TankInput input)
        {
            TankMovement.Step(
                first,
                input,
                new FlatHeightField(),
                BattleState.FixedDeltaTime);
            TankMovement.Step(
                second,
                input,
                new FlatHeightField(),
                BattleState.FixedDeltaTime);
        }

        private static NetworkWorldSnapshot Snapshot(
            long tick,
            double time,
            float pitch,
            bool active)
        {
            return new NetworkWorldSnapshot
            {
                Tick = tick,
                ServerTimeMs = time,
                GameMode = GameModeId.Standard,
                Entities = new[]
                {
                    new NetworkEntitySnapshot
                    {
                        EntityId = "tank",
                        VehicleSpecId = "mbt70",
                        Team = Team.Alpha,
                        Health = 1000f,
                        MaxHealth = 1000f,
                        HydropneumaticAimActive = active,
                        HullPitchRad = pitch
                    }
                },
                Shells =
                    Array.Empty<NetworkShellSnapshot>(),
                Events = Array.Empty<BattleEvent>()
            };
        }
    }
}
