using System.Linq;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankFittingShapeFactoryTests
    {
        [Test]
        public void SmokeBankBuildsFannedTubesCapsAndBase()
        {
            GameObject owner = new GameObject("SmokeOwner");
            try
            {
                Transform bank = TankFittingShapeFactory.BuildSmokeBank(
                    "Proof",
                    owner.transform,
                    new Vector3(1f, 2f, 3f),
                    Quaternion.Euler(0f, 20f, 0f),
                    6,
                    0.04f,
                    0.27f,
                    -0.41f,
                    0.30f,
                    0.56f,
                    0.098f,
                    Color.gray,
                    Color.black);
                Assert.That(
                    Count(bank, "Proof-Detail-SmokeLauncher"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(bank, "Proof-SmokeLauncherCap"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(bank, "Proof-SmokeBankBase"),
                    Is.EqualTo(1));
                Transform first = bank
                    .GetComponentsInChildren<Transform>(true)
                    .First(item =>
                        item.name == "Proof-Detail-SmokeLauncher");
                const float offset = -2.5f;
                Assert.That(
                    first.localPosition.x,
                    Is.EqualTo(
                        Mathf.Cos(0.30f) * offset * 0.098f)
                        .Within(0.0001f));
                Assert.That(
                    first.localPosition.z,
                    Is.EqualTo(
                        -Mathf.Sin(0.30f) * offset * 0.098f)
                        .Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void AntennaWhipBuildsTsBaseAndRakedMember()
        {
            GameObject owner = new GameObject("AntennaOwner");
            try
            {
                Transform antenna =
                    TankFittingShapeFactory.BuildAntennaWhip(
                        "Proof",
                        owner.transform,
                        new Vector3(0f, 0.63f, -1.02f),
                        2.67f,
                        0.014f,
                        0.018f,
                        Color.gray,
                        Color.black);
                Assert.That(
                    Count(antenna, "Proof-AntennaBasePot"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(antenna, "Proof-AntennaCollar"),
                    Is.EqualTo(1));
                Transform whip = antenna
                    .GetComponentsInChildren<Transform>(true)
                    .First(item =>
                        item.name == "Proof-Detail-RadioWhip");
                Assert.That(
                    whip.GetComponent<MeshFilter>()
                        .sharedMesh.bounds.size.y,
                    Is.EqualTo(2.67f).Within(0.0001f));
                Assert.That(
                    whip.localPosition.y,
                    Is.EqualTo(
                        0.12f +
                        Mathf.Cos(0.018f) * 2.67f * 0.5f)
                        .Within(0.0001f));
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
