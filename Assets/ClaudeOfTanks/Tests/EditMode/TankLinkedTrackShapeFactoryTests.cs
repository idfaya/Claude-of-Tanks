using System;
using System.Linq;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class TankLinkedTrackShapeFactoryTests
    {
        [Test]
        public void BuildsTwoUniformLinkedTrackLanes()
        {
            GameObject owner = new GameObject("TrackOwner");
            try
            {
                Transform course =
                    TankLinkedTrackShapeFactory.Build(
                        "Proof",
                        owner.transform,
                        1.405f,
                        0.44f,
                        0.09f,
                        0.165f,
                        0.83f,
                        0.05f,
                        2.20f,
                        -1.45f,
                        new TankTrackLoopEnd(
                            2.90f, 0.78f, 0.21f, 18),
                        new TankTrackLoopEnd(
                            -2.42f, 0.90f, 0.258f, 14),
                        new[]
                        {
                            new TankTrackSupport(
                                -1.40f, 0.80f, 0.086f),
                            new TankTrackSupport(
                                0f, 0.80f, 0.086f),
                            new TankTrackSupport(
                                1.44f, 0.80f, 0.086f)
                        },
                        true,
                        Color.gray);
                Transform[] pads = course
                    .GetComponentsInChildren<Transform>(true)
                    .Where(item => item.name == "Proof-TrackPad")
                    .ToArray();
                Assert.That(pads.Length, Is.GreaterThan(100));
                Assert.That(pads.Length % 2, Is.EqualTo(0));
                Assert.That(
                    pads.Count(item =>
                        item.localPosition.x < 0f),
                    Is.EqualTo(pads.Length / 2));
                Assert.That(
                    pads.Count(item =>
                        item.localPosition.x > 0f),
                    Is.EqualTo(pads.Length / 2));
                Assert.That(
                    pads.Min(item => item.localPosition.y),
                    Is.EqualTo(0.05f).Within(0.0001f));
                Assert.That(
                    pads.Max(item => item.localPosition.y),
                    Is.GreaterThan(1.16f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void RejectsReversedContactKnees()
        {
            GameObject owner = new GameObject("TrackOwner");
            try
            {
                Assert.Throws<ArgumentException>(() =>
                    TankLinkedTrackShapeFactory.Build(
                        "Proof",
                        owner.transform,
                        1.4f,
                        0.4f,
                        0.09f,
                        0.165f,
                        0.8f,
                        0.05f,
                        -1f,
                        1f,
                        new TankTrackLoopEnd(
                            2f, 0.8f, 0.2f, 8),
                        new TankTrackLoopEnd(
                            -2f, 0.8f, 0.2f, 8),
                        Array.Empty<TankTrackSupport>(),
                        false,
                        Color.gray));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }
    }
}
