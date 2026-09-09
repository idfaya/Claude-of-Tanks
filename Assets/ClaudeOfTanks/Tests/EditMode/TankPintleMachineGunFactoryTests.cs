using System.Linq;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankPintleMachineGunFactoryTests
    {
        [Test]
        public void NsvtBuildsCompleteTsLoadPath()
        {
            GameObject owner = new GameObject("NsvtOwner");
            try
            {
                Transform gun = TankPintleMachineGunFactory.Build(
                    "Proof-Nsvt",
                    owner.transform,
                    new Vector3(-0.46f, 0.87155f, -0.362f),
                    Quaternion.identity,
                    TankMachineGunClass.Nsvt,
                    0.58f,
                    -0.075f,
                    true,
                    TankMachineGunShield.Standard,
                    true,
                    Color.black,
                    Color.gray);
                Assert.That(Count(gun, "Proof-Nsvt-Bearing"), Is.EqualTo(1));
                Assert.That(Count(gun, "Proof-Nsvt-BearingRing"), Is.EqualTo(1));
                Assert.That(Count(gun, "Proof-Nsvt-Spindle"), Is.EqualTo(1));
                Assert.That(Count(gun, "Proof-Nsvt-CradleFork"), Is.EqualTo(2));
                Assert.That(Count(gun, "Proof-Nsvt-Trunnion"), Is.EqualTo(1));
                Assert.That(Count(gun, "Proof-Nsvt-Receiver"), Is.EqualTo(1));
                Assert.That(Count(gun, "Proof-Nsvt-SpadeGrip"), Is.EqualTo(2));
                Assert.That(Count(gun, "Proof-Nsvt-FeedLink"), Is.EqualTo(5));
                Assert.That(Count(gun, "Proof-Nsvt-BarrelBridge"), Is.EqualTo(1));
                Assert.That(Count(gun, "Proof-Nsvt-Barrel"), Is.EqualTo(1));
                Assert.That(Count(gun, "Proof-Nsvt-FlashHider"), Is.EqualTo(1));
                Assert.That(Count(gun, "Proof-Nsvt-Shield"), Is.EqualTo(2));
                Assert.That(
                    Count(gun, "Proof-Nsvt-ShieldFoldedEdge"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(gun, "Proof-Nsvt-ShieldVisionSlot"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(gun, "Proof-Nsvt-ShieldFastener"),
                    Is.EqualTo(4));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [TestCase((int)TankMachineGunClass.M2, 1, 0, 4)]
        [TestCase((int)TankMachineGunClass.Dshk, 0, 5, 0)]
        [TestCase((int)TankMachineGunClass.Kord, 1, 0, 4)]
        [TestCase((int)TankMachineGunClass.Mag, 0, 0, 0)]
        public void WeaponClassesSelectTsJacketGrammar(
            int weaponClass,
            int jackets,
            int fins,
            int rings)
        {
            GameObject owner =
                new GameObject("MachineGunClassOwner");
            try
            {
                Transform gun = TankPintleMachineGunFactory.Build(
                    "Proof-Class",
                    owner.transform,
                    Vector3.zero,
                    Quaternion.identity,
                    (TankMachineGunClass)weaponClass,
                    1f,
                    0.06f,
                    false,
                    TankMachineGunShield.None,
                    false,
                    Color.black,
                    Color.gray);
                Assert.That(
                    Count(gun, "Proof-Class-Jacket"),
                    Is.EqualTo(jackets));
                Assert.That(
                    Count(gun, "Proof-Class-JacketFin"),
                    Is.EqualTo(fins));
                Assert.That(
                    Count(gun, "Proof-Class-JacketRing"),
                    Is.EqualTo(rings));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        private static int Count(
            Transform root,
            string name)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .Count(item => item.name == name);
        }
    }
}
