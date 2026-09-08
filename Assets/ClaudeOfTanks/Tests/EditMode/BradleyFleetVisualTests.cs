using System.Collections.Generic;
using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class BradleyFleetVisualTests
    {
        [TestCase("m2a2_bradley", 19)]
        [TestCase("ua_m2a3_bradley", 24)]
        [TestCase("m3a3_bradley", 117)]
        public void UsesCatalogArmorAndExactBradleyRunningGear(
            string id,
            int armorCount)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle(id);
            TankView view = Create(
                catalog,
                id);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(armorCount));
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
                float[] actualStations =
                    wheels
                        .Where(item =>
                            item.name ==
                            "RoadWheel-L")
                        .Select(item =>
                            item.localPosition.z)
                        .OrderBy(value => value)
                        .ToArray();
                Assert.That(
                    actualStations,
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

        [TestCase("m2a2_bradley")]
        [TestCase("ua_m2a3_bradley")]
        [TestCase("m3a3_bradley")]
        public void SharedHullKeepsClosureSkirtMountsAndTroopRamp(
            string id)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                id);
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
                        "Painted-Bradley-SkirtPanel"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-SkirtHanger"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "Bradley-SkirtHangerBolt"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-SkirtApron"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Bradley-DriverPeriscopeLens"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-RearRamp"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-RearCornerPost"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("m2a2_bradley", false)]
        [TestCase("ua_m2a3_bradley", true)]
        public void A2VariantsKeepRoofRisersPitchingTowAndVariantKit(
            string id,
            bool ukrainian)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                id);
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-A2RoofRiser"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Bradley-TowTubeMouth"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-Bradley-SmokeLauncher"),
                    Is.EqualTo(8));
                AssertGunOwned(
                    view,
                    "Painted-Bradley-A2TowPod");
                Assert.That(
                    Count(
                        view,
                        "Painted-BradleyUA-HeavySideModule"),
                    Is.EqualTo(
                        ukrainian ? 16 : 0));
                Assert.That(
                    Count(
                        view,
                        "Painted-BradleyUA-GlacisTile"),
                    Is.EqualTo(
                        ukrainian ? 16 : 0));
                Assert.That(
                    Count(
                        view,
                        "Painted-BradleyUA-TurretSideTile"),
                    Is.EqualTo(
                        ukrainian ? 8 : 0));
                Assert.That(
                    Count(
                        view,
                        "BradleyUA-IsuHead"),
                    Is.EqualTo(
                        ukrainian ? 1 : 0));
                Assert.That(
                    Count(
                        view,
                        "BradleyUA-MgBarrel"),
                    Is.EqualTo(
                        ukrainian ? 1 : 0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void M3A3KeepsCfvTurretEraCarriersAndEquipment()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle(
                    "m3a3_bradley");
            TankView view = Create(
                catalog,
                "m3a3_bradley");
            try
            {
                Assert.That(
                    CountArmorKind(
                        view,
                        definition,
                        "era"),
                    Is.EqualTo(88));
                Assert.That(
                    Count(
                        view,
                        "Painted-BradleyM3-GlacisEraCarrier"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-BradleyM3-SideEraCarrier"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-BradleyM3-FacetedCheek"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "BradleyM3-CivDrum"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-BradleyM3-RoofServiceBin"),
                    Is.EqualTo(4));
                Assert.That(
                    CountPrefix(
                        view,
                        "BradleyM3-M2-MgBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(
                        view,
                        "BradleyM3-M240-MgBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Bradley-TowTubeMouth"),
                    Is.EqualTo(2));
                AssertGunOwned(
                    view,
                    "Painted-BradleyM3-TowPod");
            }
            finally
            {
                view.Destroy();
            }
        }

        [TestCase("m2a2_bradley")]
        [TestCase("ua_m2a3_bradley")]
        [TestCase("m3a3_bradley")]
        public void M242FollowsGunAndPaintedPartsUseCamouflage(
            string id)
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                id,
                "digital");
            try
            {
                AssertGunOwned(
                    view,
                    "Painted-Bradley-M242-Barrel");
                AssertGunOwned(
                    view,
                    "Bradley-M242-MuzzleBore");
                Renderer painted = Find(
                        view,
                        "Painted-Bradley-UpperGlacis")
                    .GetComponent<Renderer>();
                Renderer optic = Find(
                        view,
                        id == "m3a3_bradley"
                            ? "BradleyM3-IsuLens"
                            : "Bradley-A2IsuLens")
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
