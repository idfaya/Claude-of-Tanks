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
    }
}
