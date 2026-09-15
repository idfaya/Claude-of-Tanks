using System;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class ArmorSimulationTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void DirectionalArmorClassifiesFrontSideAndRear()
        {
            Float3 forward = new Float3(0f, 0f, 1f);

            Assert.That(
                ArmorSimulation.DirectionFromHit(new Float3(0f, 0f, -4f), forward),
                Is.EqualTo(ArmorDirection.Front));
            Assert.That(
                ArmorSimulation.DirectionFromHit(new Float3(3f, 0f, 0f), forward),
                Is.EqualTo(ArmorDirection.Side));
            Assert.That(
                ArmorSimulation.DirectionFromHit(new Float3(0f, 0f, 2f), forward),
                Is.EqualTo(ArmorDirection.Rear));
        }

        [Test]
        public void DirectionalArmorReturnsSelectedThickness()
        {
            Assert.That(
                ArmorSimulation.SelectDirectionalArmorMm(ArmorDirection.Front, 150f, 80f, 45f),
                Is.EqualTo(150f));
            Assert.That(
                ArmorSimulation.SelectDirectionalArmorMm(ArmorDirection.Side, 150f, 80f, 45f),
                Is.EqualTo(80f));
            Assert.That(
                ArmorSimulation.SelectDirectionalArmorMm(ArmorDirection.Rear, 150f, 80f, 45f),
                Is.EqualTo(45f));
        }

        [Test]
        public void ImpactAngleUsesIncomingDirectionAndOutwardNormal()
        {
            Float3 normal = new Float3(0f, 0f, 1f);
            Assert.That(
                ArmorSimulation.ImpactAngleDegrees(new Float3(0f, 0f, -5f), normal),
                Is.EqualTo(0f).Within(Tolerance));

            float radians = 60f * MathUtil.Deg2Rad;
            Float3 oblique = new Float3(MathF.Sin(radians), 0f, -MathF.Cos(radians));
            Assert.That(
                ArmorSimulation.ImpactAngleDegrees(oblique, normal),
                Is.EqualTo(60f).Within(Tolerance));
        }

        [Test]
        public void EffectiveThicknessAppliesShellNormalizationAndSlopeExponent()
        {
            ArmorPlateSpec plate = new ArmorPlateSpec(100f);
            float effectiveAngle;
            float thickness = ArmorSimulation.EffectiveThicknessMm(
                ArmorShellType.AP, 90f, plate, 60f, out effectiveAngle);

            float expected = 100f / MathF.Pow(MathF.Cos(55f * MathUtil.Deg2Rad), 1.4f);
            Assert.That(effectiveAngle, Is.EqualTo(55f).Within(Tolerance));
            Assert.That(thickness, Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void EffectiveThicknessUsesKeAndCeRatings()
        {
            ArmorPlateSpec composite = new ArmorPlateSpec(200f, 450f, 800f);
            float ignored;

            Assert.That(
                ArmorSimulation.EffectiveThicknessMm(
                    ArmorShellType.APFSDS, 120f, composite, 0f, out ignored),
                Is.EqualTo(450f).Within(Tolerance));
            Assert.That(
                ArmorSimulation.EffectiveThicknessMm(
                    ArmorShellType.HEAT, 120f, composite, 0f, out ignored),
                Is.EqualTo(800f).Within(Tolerance));
        }

        [Test]
        public void TwoCaliberOvermatchBoostsKineticNormalization()
        {
            float normalization = ArmorSimulation.NormalizationDegrees(
                ArmorShellType.AP, 122f, 40f);

            Assert.That(normalization, Is.EqualTo(21.35f).Within(Tolerance));
        }

        [Test]
        public void RicochetUsesRawAngleAndThreeCaliberOvermatch()
        {
            Assert.That(
                ArmorSimulation.WouldRicochet(ArmorShellType.AP, 90f, 40f, 70f),
                Is.False);
            Assert.That(
                ArmorSimulation.WouldRicochet(ArmorShellType.AP, 90f, 40f, 71f),
                Is.True);
            Assert.That(
                ArmorSimulation.WouldRicochet(ArmorShellType.AP, 122f, 40f, 80f),
                Is.False);
            Assert.That(
                ArmorSimulation.WouldRicochet(ArmorShellType.HE, 20f, 200f, 90f),
                Is.False);
        }

        [Test]
        public void ApfsdsUsesReducedEffectiveOvermatchCaliber()
        {
            Assert.That(
                ArmorSimulation.EffectiveOvermatchCaliberMm(
                    ArmorShellType.APFSDS, 120f),
                Is.EqualTo(72f).Within(Tolerance));
            Assert.That(
                ArmorSimulation.WouldRicochet(
                    ArmorShellType.APFSDS, 120f, 25f, 80f),
                Is.True);
            Assert.That(
                ArmorSimulation.WouldRicochet(
                    ArmorShellType.APFSDS, 120f, 25f, 80f, 80f),
                Is.False);
        }

        [Test]
        public void EraCutsKineticAndChemicalPenetrationUnlessTandem()
        {
            Assert.That(
                ArmorSimulation.ApplyEraPenetration(
                    ArmorShellType.APFSDS,
                    700f,
                    0.2f,
                    450f,
                    false),
                Is.EqualTo(560f).Within(Tolerance));
            Assert.That(
                ArmorSimulation.ApplyEraPenetration(
                    ArmorShellType.HEAT,
                    700f,
                    0.2f,
                    450f,
                    false),
                Is.EqualTo(250f).Within(Tolerance));
            Assert.That(
                ArmorSimulation.ApplyEraPenetration(
                    ArmorShellType.HEAT,
                    700f,
                    0.2f,
                    450f,
                    true),
                Is.EqualTo(700f).Within(Tolerance));
        }

        [Test]
        public void PenetrationRollIsRepeatableAndWithinTwentyFivePercent()
        {
            ShellSpec shell = FixedPenetrationShell(200f);
            DeterministicRandom first = new DeterministicRandom(77u);
            DeterministicRandom second = new DeterministicRandom(77u);

            float firstRoll = ArmorSimulation.RollPenetrationMm(shell, 500f, first);
            float secondRoll = ArmorSimulation.RollPenetrationMm(shell, 500f, second);

            Assert.That(firstRoll, Is.EqualTo(secondRoll));
            Assert.That(firstRoll, Is.InRange(150f, 250f));
        }

        [Test]
        public void ResolveHitRejectsRicochetBeforePenetration()
        {
            ArmorHitResult result = ArmorSimulation.ResolveHit(
                FixedPenetrationShell(1000f),
                ArmorShellType.AP,
                90f,
                new ArmorPlateSpec(40f),
                DirectionAtImpactAngle(71f),
                new Float3(0f, 0f, 1f),
                new Float3(0f, 0f, 1f),
                100f,
                new DeterministicRandom(1u));

            Assert.That(result.Direction, Is.EqualTo(ArmorDirection.Side));
            Assert.That(result.Ricocheted, Is.True);
            Assert.That(result.Penetrated, Is.False);
            Assert.That(result.EffectiveThicknessMm, Is.EqualTo(0f));
        }

        [Test]
        public void ResolveHitComparesRolledPenetrationToEffectiveThickness()
        {
            ArmorPlateSpec plate = new ArmorPlateSpec(100f);
            Float3 incoming = new Float3(0f, 0f, -1f);
            Float3 normal = new Float3(0f, 0f, 1f);
            Float3 targetForward = new Float3(0f, 0f, 1f);

            ArmorHitResult penetrating = ArmorSimulation.ResolveHit(
                FixedPenetrationShell(200f),
                ArmorShellType.AP,
                90f,
                plate,
                incoming,
                normal,
                targetForward,
                100f,
                new DeterministicRandom(5u));
            ArmorHitResult stopped = ArmorSimulation.ResolveHit(
                FixedPenetrationShell(50f),
                ArmorShellType.AP,
                90f,
                plate,
                incoming,
                normal,
                targetForward,
                100f,
                new DeterministicRandom(5u));

            Assert.That(penetrating.Direction, Is.EqualTo(ArmorDirection.Front));
            Assert.That(penetrating.Penetrated, Is.True);
            Assert.That(stopped.Penetrated, Is.False);
        }

        [Test]
        public void InvalidDirectionIsRejected()
        {
            Assert.Throws<ArgumentException>(
                () => ArmorSimulation.ImpactAngleDegrees(
                    Float3.Zero, new Float3(0f, 0f, 1f)));
        }

        private static ShellSpec FixedPenetrationShell(float penetrationMm)
        {
            return new ShellSpec
            {
                Pen100Mm = penetrationMm,
                Pen1000Mm = penetrationMm
            };
        }

        private static Float3 DirectionAtImpactAngle(float angleDeg)
        {
            float radians = angleDeg * MathUtil.Deg2Rad;
            return new Float3(MathF.Sin(radians), 0f, -MathF.Cos(radians));
        }
    }
}
