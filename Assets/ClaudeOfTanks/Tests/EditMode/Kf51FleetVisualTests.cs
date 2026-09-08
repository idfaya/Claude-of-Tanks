using System.Collections.Generic;
using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Kf51FleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "kf51",
            "kf51b"
        };

        [Test]
        public void ProductionFamilyUsesOnlyCatalogArmor()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            foreach (string id in ProductionIds)
            {
                VehicleDefinition definition =
                    catalog.GetVehicle(id);
                TankView view = Create(catalog, id);
                try
                {
                    Assert.That(
                        CountPrefix(view, "Armor-"),
                        Is.EqualTo(
                            FleetVisualAssertions
                                .ValidPlateCount(
                                    definition)),
                        id);
                    Assert.That(
                        CountArmorKind(
                            view,
                            definition,
                            "era"),
                        Is.EqualTo(44),
                        id);
                    Assert.That(
                        Count(view, "SideArmor"),
                        Is.EqualTo(0),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "KF51-SkirtCarrierFoot"),
                        Is.EqualTo(14),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "KF51-SkirtStationJoint"),
                        Is.EqualTo(12),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-KF51-EngineGrille"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-KF51-ClosedMudguardShoulder"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-KF51-ClosedMudguardWeb"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-KF51-SmokeLauncher"),
                        Is.EqualTo(8),
                        id);
                    Assert.That(
                        Count(view, "KF51-Antenna"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        view.Root
                            .GetComponentsInChildren<Collider>()
                            .Length,
                        Is.EqualTo(0),
                        id);
                }
                finally
                {
                    view.Destroy();
                }
            }
        }

        [Test]
        public void DemonstratorKeepsChevronCageAndFiveLightRws()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog, "kf51");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-KF51-ChevronUpperPanel"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-KF51-ChevronLowerPanel"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-KF51-CheekCassette"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "KF51-DemonstratorCageRail"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "KF51-DemonstratorCagePost"),
                    Is.EqualTo(18));
                Assert.That(
                    Count(
                        view,
                        "KF51-DemonstratorCageBrace"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        view,
                        "KF51-OpenYokeRWS-YokeArm"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "KF51-OpenYokeRWS-FeedBelt"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "KF51-OpenYokeRWS-WorkLight"),
                    Is.EqualTo(5));
                Assert.That(
                    CountPrefix(view, "KF51B-"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void OwnerExactKeepsConvexTurretServiceBandAndBustleCage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(catalog, "kf51b");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-KF51B-ConvexCrownCourse"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(
                        view,
                        "Painted-KF51B-FlankPanel"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "KF51B-ServiceGrilleRail"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "KF51B-ServiceGrillePost"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(
                        view,
                        "Painted-KF51B-MultispectralSight"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "KF51B-OpenYokeRWS-YokeArm"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "KF51B-OpenYokeRWS-FeedBelt"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "KF51B-BustleCageSideRail"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(
                        view,
                        "KF51B-BustleCageRearRail"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "Painted-KF51-ChevronUpperPanel"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void Rh130FittingsFollowGunAndPaintedPartsUseCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView baseView = Create(
                catalog,
                "kf51",
                "digital");
            TankView ownerView = Create(
                catalog,
                "kf51b",
                "digital");
            try
            {
                AssertGunOwned(
                    baseView,
                    "Painted-KF51-Rh130-Housing");
                AssertGunOwned(
                    baseView,
                    "KF51-Rh130-HexClamp");
                AssertGunOwned(
                    ownerView,
                    "Painted-KF51B-Rh130-Housing");
                AssertGunOwned(
                    ownerView,
                    "KF51B-Rh130-HexClamp");
                Assert.That(
                    Count(
                        baseView,
                        "Painted-KF51-Rh130-ThermalShroud"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        ownerView,
                        "Painted-KF51B-Rh130-ThermalShroud"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        ownerView,
                        "KF51B-Rh130-Cinch"),
                    Is.EqualTo(3));

                Renderer painted = Find(
                        ownerView,
                        "Painted-KF51B-FlankPanel")
                    .GetComponent<Renderer>();
                Renderer mechanism = Find(
                        ownerView,
                        "KF51B-OpenYokeRWS-Mechanism")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.Not.Null);
                Assert.That(
                    mechanism.sharedMaterial.mainTexture,
                    Is.Null);
            }
            finally
            {
                baseView.Destroy();
                ownerView.Destroy();
            }
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

        private static int CountArmorKind(
            TankView view,
            VehicleDefinition definition,
            string kind)
        {
            HashSet<string> names =
                new HashSet<string>();
            AddPlateNames(
                names,
                definition.armor?.hullPlates,
                kind);
            AddPlateNames(
                names,
                definition.armor?.turretPlates,
                kind);
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item =>
                    item.name.StartsWith("Armor-") &&
                    names.Contains(
                        item.name.Substring(
                            "Armor-".Length)));
        }

        private static void AddPlateNames(
            HashSet<string> names,
            ArmorPlateDefinition[] plates,
            string kind)
        {
            if (plates == null) return;
            for (int index = 0;
                index < plates.Length;
                index++)
            {
                ArmorPlateDefinition plate =
                    plates[index];
                if (plate?.kind == kind)
                    names.Add(plate.name);
            }
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
                    "kf51-" + id,
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
