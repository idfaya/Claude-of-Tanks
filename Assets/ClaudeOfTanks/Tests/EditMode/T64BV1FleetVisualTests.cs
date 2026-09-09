using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T64BV1FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndExactSixWheelCourse()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t64bv1");
            TankView view = Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(117));
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(
                        FleetVisualAssertions.ValidPlateCount(
                            definition)));
                Assert.That(
                    definition.armor.hullPlates
                        .Concat(
                            definition.armor.turretPlates)
                        .Count(plate =>
                            plate.kind == "era"),
                    Is.EqualTo(98));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(12));
                Assert.That(
                    FindAll(view, "RoadWheel-L")
                        .Select(item =>
                            item.localPosition.z)
                        .ToArray(),
                    Is.EqualTo(
                        new[]
                        {
                            1.875f,
                            1.125f,
                            0.4f,
                            -0.325f,
                            -1.075f,
                            -1.775f
                        })
                        .Within(0.0001f));
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.28f,
                            0.868f,
                            -2.555f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.28f,
                            0.785f,
                            2.55f)));
                Assert.That(
                    Count(
                        view,
                        "T64-ReturnRoller"),
                    Is.EqualTo(8));
                Assert.That(
                    FindAllByPrefix(
                            view,
                            "ReturnRoller-")
                        .All(item =>
                            !item.GetComponent<Renderer>()
                                .enabled),
                    Is.True);

                Bounds track =
                    Find(view, "TrackLinks-L")
                        .GetComponent<MeshFilter>()
                        .sharedMesh.bounds;
                Assert.That(
                    track.min.x,
                    Is.EqualTo(-1.569f)
                        .Within(0.0002f));
                Assert.That(
                    track.max.x,
                    Is.EqualTo(-0.991f)
                        .Within(0.0002f));
                Assert.That(
                    track.min.y,
                    Is.EqualTo(0.085f)
                        .Within(0.0002f));
                Assert.That(
                    track.max.y,
                    Is.EqualTo(1.2729f)
                        .Within(0.0002f));
                Assert.That(
                    track.min.z,
                    Is.EqualTo(-2.9584f)
                        .Within(0.0002f));
                Assert.That(
                    track.max.z,
                    Is.EqualTo(2.8995f)
                        .Within(0.0002f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericShellAndKeepsEraVisible()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    {
                        "Hull",
                        "UpperHull",
                        "Turret",
                        "Gun"
                    })
                {
                    AssertHidden(view, name);
                }

                Transform[] armor =
                    FindAllByPrefix(view, "Armor-");
                Assert.That(
                    armor.Count(item =>
                        item.GetComponent<Renderer>()
                            .enabled),
                    Is.EqualTo(98));
                Assert.That(
                    armor
                        .Where(item =>
                            item.GetComponent<Renderer>()
                                .enabled)
                        .All(item =>
                            item.name.Contains("era")),
                    Is.True);
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(
                        view,
                        "Painted-T64-"),
                    Is.GreaterThan(150));
                Assert.That(
                    CountPrefix(
                        view,
                        "Painted-Soviet-"),
                    Is.EqualTo(0));
                Assert.That(
                    view.Root
                        .GetComponentsInChildren<Collider>(
                            true)
                        .Length,
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsLowHullAndT64DeckIdentity()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-FenderBin"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-SkirtPanel"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-EngineLouvre"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-SpareTrackLink"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-LeftExhaust"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T64-UnditchingLog"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-RoadWheelSpoke"),
                    Is.EqualTo(96));
                Assert.That(
                    Count(
                        view,
                        "T64-FrontMudFlap"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "T64-RearMudFlap"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsCastTurretNsvtAndRearRack()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-CastDome"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-CommanderGallery"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T64-PeriscopeLens") +
                    Count(
                        view,
                        "T64-RoofPeriscopeLens"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        view,
                        "T64-NsvtReceiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T64-NsvtBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T64-902ALauncher"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "T64-BustleRackRail"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-RackFuelDrum"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-T64-OpvtTube"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "T64-RadioWhip"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsTwoA46OnAuthoritativeGun()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    {
                        "Painted-T64-2A46Saddle",
                        "Painted-T64-2A46Root",
                        "Painted-T64-2A46Evacuator",
                        "Painted-T64-2A46MuzzleTube",
                        "T64-2A46MuzzleBore"
                    })
                {
                    AssertGunOwned(view, name);
                }
                Assert.That(
                    Count(
                        view,
                        "T64-2A46SleeveRing"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "T64-2A46MuzzleBore"),
                    Is.EqualTo(1));
            }
            finally
            {
                view.Destroy();
            }
        }

        private static TankView Create(
            ContentCatalog catalog)
        {
            VehicleDefinition definition =
                catalog.GetVehicle("t64bv1");
            return TankView.Create(
                new TankState(
                    "t64-bv1-test",
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                "factory",
                "forest",
                catalog);
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
            Vector3 scale = Vector3.Scale(
                part.parent.localScale,
                part.parent.parent.localScale);
            Assert.That(
                scale.x,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                scale.y,
                Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                scale.z,
                Is.EqualTo(1f).Within(0.0001f));
        }

        private static int Count(
            TankView view,
            string name)
        {
            return FindAll(view, name).Length;
        }

        private static int CountPrefix(
            TankView view,
            string prefix)
        {
            return FindAllByPrefix(
                view,
                prefix).Length;
        }

        private static Transform[] FindAll(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(
                    true)
                .Where(item =>
                    item.name == name)
                .ToArray();
        }

        private static Transform[] FindAllByPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(
                    true)
                .Where(item =>
                    item.name.StartsWith(prefix))
                .ToArray();
        }

        private static Transform Find(
            TankView view,
            string name)
        {
            return FindAll(view, name).First();
        }
    }
}
