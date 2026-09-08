using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Marder1A3FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndExactBradleyDonorRunningGear()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("marder1a3");
            TankView view = Create(
                catalog,
                "marder1a3");
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(19));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions.ValidPlateCount(
                            definition)));
                Assert.That(
                    Count(view, "MissilePod"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(12));
                Assert.That(
                    view.Root
                        .GetComponentsInChildren<Collider>()
                        .Length,
                    Is.EqualTo(0));

                Transform[] wheels =
                    view.Root
                        .GetComponentsInChildren<Transform>()
                        .Where(item =>
                            item.name.StartsWith(
                                "RoadWheel-"))
                        .ToArray();
                Assert.That(
                    wheels.All(item =>
                        Mathf.Abs(
                            Mathf.Abs(
                                item.localPosition.x) -
                            1.1475f) <
                        0.0001f),
                    Is.True);
                Assert.That(
                    wheels.All(item =>
                        Mathf.Abs(
                            item.localPosition.y -
                            0.4f) <
                        0.0001f),
                    Is.True);
                Assert.That(
                    wheels.All(item =>
                        Mathf.Abs(
                            item.localScale.y -
                            0.2412f) <
                        0.0001f),
                    Is.True);
                float[] stations =
                    wheels
                        .Where(item =>
                            item.name ==
                            "RoadWheel-L")
                        .Select(item =>
                            item.localPosition.z)
                        .OrderBy(value => value)
                        .ToArray();
                Assert.That(
                    stations,
                    Is.EqualTo(
                        new[]
                        {
                            -1.87f,
                            -1.12f,
                            -0.37f,
                            0.38f,
                            1.13f,
                            1.88f
                        })
                        .Within(0.0001f));
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.1475f,
                            0.63f,
                            2.53f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.1475f,
                            0.81f,
                            -2.68f)));
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localScale.x,
                    Is.EqualTo(0.24f));
                Assert.That(
                    Find(view, "Idler-L")
                        .localScale.x,
                    Is.EqualTo(0.28f));
                AssertHidden(view, "Hull");
                AssertHidden(view, "UpperHull");
                AssertHidden(view, "Turret");
                AssertHidden(view, "Gun");
                AssertHidden(view, "Armor-track_L");
                AssertHidden(view, "Armor-track_R");
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void DonorHullKeepsClosureWithoutBradleySkirtPackage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "marder1a3");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-NarrowTub"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-BowCornerClosure"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-RearRamp"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-SkirtPanel"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        view,
                        "Painted-Marder-A3SidePanel"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        view,
                        "Marder-A3AppliqueRail"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-Marder-A3PanelTopRail"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Marder-RubberMudguard"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "Painted-Marder-LeftRearStowage"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Marder-RightExhaustHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Marder-RightExhaustLouvre"),
                    Is.EqualTo(4));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void LowCastTurretKeepsMilanPeriCrewKitAndRearClosure()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "marder1a3");
            try
            {
                Assert.That(
                    view.Root.Find("TurretRoot")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            0.18f,
                            1.895f,
                            -0.05f)));
                Assert.That(
                    Count(
                        view,
                        "Painted-Marder-LowCastBody"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Marder-TrunnionTower"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(view, "Marder-MilanTube"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Marder-PeriZ11Lens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Marder-CommanderPeriscope"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "Painted-Marder-ServiceBox"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Marder-RearEquipmentWall"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Marder-RearBasketRail"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "Painted-Marder-SmokeLauncher"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "Marder-RoofMgBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Marder-RadioAntenna"),
                    Is.EqualTo(2));
                Assert.That(
                    Find(view, "Marder-MilanTube")
                        .parent.name,
                    Is.EqualTo("TurretRoot"));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void Mk20FollowsGunAndPaintedPartsUseCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "marder1a3",
                "digital");
            try
            {
                AssertGunOwned(
                    view,
                    "Painted-Marder-Mk20Barrel");
                AssertGunOwned(
                    view,
                    "Marder-Mk20MuzzleBore");
                AssertGunOwned(
                    view,
                    "Marder-CoaxMg3Barrel");
                Renderer painted = Find(
                        view,
                        "Painted-Marder-A3SidePanel")
                    .GetComponent<Renderer>();
                Renderer optic = Find(
                        view,
                        "Marder-PeriZ11Lens")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.Not.Null);
                Assert.That(
                    optic.sharedMaterial.mainTexture,
                    Is.Null);
            }
            finally
            {
                view.Destroy();
            }
        }

        private static void AssertHidden(
            TankView view,
            string name)
        {
            Assert.That(
                Find(view, name)
                    .GetComponent<Renderer>()
                    .enabled,
                Is.False);
        }

        private static void AssertGunOwned(
            TankView view,
            string name)
        {
            Transform part = Find(view, name);
            Assert.That(
                part.parent.parent.name,
                Is.EqualTo("Gun"));
            Vector3 product = Vector3.Scale(
                part.parent.localScale,
                part.parent.parent.localScale);
            Assert.That(
                product.x,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                product.y,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                product.z,
                Is.EqualTo(1f).Within(0.0001f));
        }

        private static TankView Create(
            ContentCatalog catalog,
            string id,
            string camouflage = "factory")
        {
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            return TankView.Create(
                new TankState(
                    id + "-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                camouflage,
                "forest",
                catalog);
        }

        private static int Count(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item => item.name == name);
        }

        private static int CountPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item =>
                    item.name.StartsWith(prefix));
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .First(item => item.name == name);
        }
    }
}
