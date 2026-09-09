using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T90SMTurretEquipmentTests
    {
        [Test]
        public void BuildsRoofSightsAndIntegratedRemoteNsvt()
        {
            TankView view = Create();
            try
            {
                Assert.That(
                    Count(view, "Painted-T90SM-LeftPlateauBin"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-RightRoofBin"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-SosnaServiceCassette"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-SosnaAperture"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-PanoramaHead"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-PanoramaWindow"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-BackupSightPedestal"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-BackupSightAperture"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-NsvtRace"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-NsvtHeadSide"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-NsvtOpticLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-NsvtWorkLightLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-RemoteNsvt"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-Nsvt-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-Nsvt-Barrel"),
                    Is.EqualTo(1));

                Transform weapon =
                    Find(view, "T90SM-RemoteNsvt");
                Assert.That(
                    weapon.localPosition.x,
                    Is.EqualTo(0.40f).Within(0.000001f));
                Assert.That(
                    weapon.localPosition.y,
                    Is.EqualTo(0.728072f).Within(0.000001f));
                Assert.That(
                    weapon.localPosition.z,
                    Is.EqualTo(-0.882f).Within(0.000001f));

                Renderer cap = Find(
                        view,
                        "T90SM-PanoramaCap")
                    .GetComponent<Renderer>();
                Assert.That(
                    cap.bounds.max.y,
                    Is.EqualTo(2.245f).Within(0.0001f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsSquaredBustleAndContinuousSideCage()
        {
            TankView view = Create();
            try
            {
                Mesh forward = Find(
                        view,
                        "Painted-T90SM-BustleForward")
                    .GetComponent<MeshFilter>()
                    .sharedMesh;
                AssertBounds(
                    forward.bounds,
                    new Vector3(-0.91f, 0.125f, -1.95f),
                    new Vector3(0.91f, 0.525f, -1.26f));
                Assert.That(
                    Count(view, "Painted-T90SM-BustleMiddle"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T90SM-BustleRearStep"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-BustleRearSlat"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(view, "T90SM-BustleRearStile"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T90SM-BustleRearStrut"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-BustleSideBacking"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Painted-T90SM-BustleSideFoot"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "T90SM-BustleSideRail"),
                    Is.EqualTo(64));
                Assert.That(
                    Count(view, "T90SM-BustleSidePost"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(view, "T90SM-BustleCornerPost"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-BustleDeckPad"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-BustleBasketCrossRail"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-BustleBasketSideRail"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "T90SM-BustleBasketPost"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T90SM-OPVTSnorkel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T90SM-OPVTStrap"),
                    Is.EqualTo(2));

                float[] backingZ = view.Root
                    .GetComponentsInChildren<Transform>(true)
                    .Where(item =>
                        item.name == "T90SM-BustleSideBacking")
                    .Select(item => item.localPosition.z)
                    .OrderBy(value => value)
                    .ToArray();
                Assert.That(
                    backingZ,
                    Is.EqualTo(new[] { -1.63f, -1.57f }));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t90sm");
            return TankView.Create(
                new TankState(
                    "t90sm-equipment-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .First(item => item.name == name);
        }

        private static int Count(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Count(item => item.name == name);
        }

        private static void AssertBounds(
            Bounds bounds,
            Vector3 minimum,
            Vector3 maximum)
        {
            Assert.That(bounds.min.x,
                Is.EqualTo(minimum.x).Within(0.0001f));
            Assert.That(bounds.min.y,
                Is.EqualTo(minimum.y).Within(0.0001f));
            Assert.That(bounds.min.z,
                Is.EqualTo(minimum.z).Within(0.0001f));
            Assert.That(bounds.max.x,
                Is.EqualTo(maximum.x).Within(0.0001f));
            Assert.That(bounds.max.y,
                Is.EqualTo(maximum.y).Within(0.0001f));
            Assert.That(bounds.max.z,
                Is.EqualTo(maximum.z).Within(0.0001f));
        }
    }
}
