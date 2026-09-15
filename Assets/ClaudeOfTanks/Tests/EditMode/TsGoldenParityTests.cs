using System;
using System.IO;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class TsGoldenParityTests
    {
        private const string FixturePath =
            "Assets/ClaudeOfTanks/Tests/Fixtures/ts-golden-parity.json";

        [Test]
        public void SpottingConstantsMatchTypeScriptGoldenVectors()
        {
            TsGoldenParityFixture fixture = LoadFixture();
            Assert.That(fixture.version, Is.EqualTo(1));

            for (int i = 0; i < fixture.spotting.viewRange.Length; i++)
            {
                ViewRangeCase item =
                    fixture.spotting.viewRange[i];
                Assert.That(
                    SpottingSimulation.BaseViewRangeM(
                        item.id,
                        item.role),
                    Is.EqualTo(item.expected)
                        .Within(0.0001f),
                    item.id);
            }

            for (int i = 0; i < fixture.spotting.baseCamo.Length; i++)
            {
                BaseCamoCase item =
                    fixture.spotting.baseCamo[i];
                Assert.That(
                    SpottingSimulation.BaseCamouflage(
                        item.id,
                        item.role,
                        item.moving),
                    Is.EqualTo(item.expected)
                        .Within(0.0001f),
                    item.id);
            }

            for (int i = 0; i < fixture.spotting.checkInterval.Length; i++)
            {
                CheckIntervalCase item =
                    fixture.spotting.checkInterval[i];
                Assert.That(
                    SpottingSimulation.CheckIntervalSeconds(
                        item.distanceM),
                    Is.EqualTo(item.expected)
                        .Within(0.0001f),
                    item.distanceM.ToString());
            }

            for (int i = 0; i < fixture.spotting.spotRange.Length; i++)
            {
                SpotRangeCase item =
                    fixture.spotting.spotRange[i];
                Assert.That(
                    SpottingSimulation.SpotRangeM(
                        item.viewRangeM,
                        item.targetCamo),
                    Is.EqualTo(item.expected)
                        .Within(0.0001f),
                    item.viewRangeM + "/" + item.targetCamo);
            }
        }

        [Test]
        public void CamouflageFormulaMatchesTypeScriptGoldenVectors()
        {
            TsGoldenParityFixture fixture = LoadFixture();
            for (int i = 0; i < fixture.spotting.effectiveCamo.Length; i++)
            {
                EffectiveCamoCase item =
                    fixture.spotting.effectiveCamo[i];
                TankSpec spec = TankSpec.Medium();
                spec.CamouflageStill = item.@base;
                spec.CamouflageMoving = item.@base;
                TankState tank =
                    new TankState(
                        item.id,
                        Team.Alpha,
                        spec,
                        Float3.Zero,
                        0f);
                tank.CamouflagePaintBonus = item.paint;
                tank.FireCamouflageLoss = item.fireLoss;
                tank.LastFiredAtS = item.firedAtS;
                tank.Combat.Equipment.Camouflage = item.equip;

                Assert.That(
                    SpottingSimulation.FireBloomAt(
                        tank,
                        item.timeS),
                    Is.EqualTo(item.bloom)
                        .Within(0.0001f),
                    item.id);
                Assert.That(
                    SpottingSimulation.EffectiveCamouflage(
                        tank,
                        item.timeS,
                        item.bush),
                    Is.EqualTo(item.expected)
                        .Within(0.0001f),
                    item.id);
            }
        }

        [Test]
        public void SpecialActionKindMatchesTypeScriptGoldenVectors()
        {
            TsGoldenParityFixture fixture = LoadFixture();
            for (int i = 0; i < fixture.specialActions.Length; i++)
            {
                SpecialActionCase item = fixture.specialActions[i];
                Assert.That(
                    ToTsKind(
                        SpecialActionSimulation.KindFor(
                            SpecFor(item.id))),
                    Is.EqualTo(item.expected),
                    item.id);
            }
        }

        private static TankSpec SpecFor(string id)
        {
            TankSpec spec = TankSpec.Medium();
            switch (id)
            {
                case "hydropneumatic":
                    spec.HydropneumaticAim =
                        new HydropneumaticAimSpec();
                    break;
                case "guided_choice":
                    spec.Shells = new[]
                    {
                        new ShellSpec { Guided = false },
                        new ShellSpec { Guided = true }
                    };
                    break;
                case "guided_only":
                    spec.Shells = new[]
                    {
                        new ShellSpec { Guided = true }
                    };
                    break;
                case "magazine":
                    spec.MagazineSize = 3;
                    break;
            }
            return spec;
        }

        private static string ToTsKind(
            SpecialActionKind kind)
        {
            switch (kind)
            {
                case SpecialActionKind.GuidedMissile:
                    return "guided_missile";
                case SpecialActionKind.HydropneumaticAim:
                    return "hydropneumatic_aim";
                case SpecialActionKind.MagazineReload:
                    return "magazine_reload";
                default:
                    return "none";
            }
        }

        private static TsGoldenParityFixture LoadFixture()
        {
            string json = File.ReadAllText(FixturePath);
            return JsonUtility.FromJson<TsGoldenParityFixture>(
                json);
        }

        [Serializable]
        private sealed class TsGoldenParityFixture
        {
            public int version;
            public SpottingVectors spotting;
            public SpecialActionCase[] specialActions;
        }

        [Serializable]
        private sealed class SpottingVectors
        {
            public ViewRangeCase[] viewRange;
            public BaseCamoCase[] baseCamo;
            public CheckIntervalCase[] checkInterval;
            public SpotRangeCase[] spotRange;
            public EffectiveCamoCase[] effectiveCamo;
        }

        [Serializable]
        private sealed class ViewRangeCase
        {
            public string id;
            public string role;
            public float expected;
        }

        [Serializable]
        private sealed class BaseCamoCase
        {
            public string id;
            public string role;
            public bool moving;
            public float expected;
        }

        [Serializable]
        private sealed class CheckIntervalCase
        {
            public float distanceM;
            public float expected;
        }

        [Serializable]
        private sealed class SpotRangeCase
        {
            public float viewRangeM;
            public float targetCamo;
            public float expected;
        }

        [Serializable]
        private sealed class EffectiveCamoCase
        {
            public string id;
            public float @base;
            public float paint;
            public float equip;
            public float firedAtS;
            public float timeS;
            public float bush;
            public float fireLoss;
            public float bloom;
            public float expected;
        }

        [Serializable]
        private sealed class SpecialActionCase
        {
            public string id;
            public string expected;
        }
    }
}
