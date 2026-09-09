using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T62Obr1975FleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndExactFiveWheelCourse()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t62mv1");
            TankView view = Create(catalog);
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
                    definition.armor.hullPlates
                        .Concat(
                            definition.armor.turretPlates)
                        .Count(plate =>
                            plate.kind == "era"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(view, "RoadWheel-"),
                    Is.EqualTo(10));
                Assert.That(
                    FindAll(view, "RoadWheel-L")
                        .Select(item =>
                            item.localPosition.z)
                        .ToArray(),
                    Is.EqualTo(
                        new[]
                        {
                            2.235f,
                            1.297f,
                            0.293f,
                            -0.791f,
                            -1.933f
                        })
                        .Within(0.0001f));
                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.397f,
                            0.79f,
                            -2.795f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(
                        new Vector3(
                            -1.397f,
                            0.83f,
                            3.01f)));
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
                    Is.EqualTo(-1.683f)
                        .Within(0.0002f));
                Assert.That(
                    track.max.x,
                    Is.EqualTo(-1.111f)
                        .Within(0.0002f));
                Assert.That(
                    track.min.y,
                    Is.EqualTo(-0.025f)
                        .Within(0.0002f));
                Assert.That(
                    track.max.y,
                    Is.EqualTo(1.2194f)
                        .Within(0.0002f));
                Assert.That(
                    track.min.z,
                    Is.EqualTo(-3.2012f)
                        .Within(0.0002f));
                Assert.That(
                    track.max.z,
                    Is.EqualTo(3.3967f)
                        .Within(0.0002f));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericAndArmorPresentationShells()
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
                Assert.That(
                    FindAllByPrefix(view, "Armor-")
                        .All(item =>
                            !item.GetComponent<Renderer>()
                                .enabled),
                    Is.True);
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(
                        view,
                        "Painted-T62-"),
                    Is.GreaterThan(100));
                Assert.That(
                    CountPrefix(
                        view,
                        "Painted-Soviet-"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(
                        view,
                        "Soviet-"),
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
        public void BuildsWidenedObr1975HullIdentity()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-FenderBin"),
                    Is.EqualTo(18));
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-RearFuelDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "T62-UnditchingLog"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-EngineLouvre"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-RoadWheelSpoke"),
                    Is.EqualTo(60));
                Assert.That(
                    Count(
                        view,
                        "T62-RoadWheelInset"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(
                        view,
                        "T62-FrontMudFlap"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsBareCastTurretAndSingleDshk()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-CastDome"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-CommanderCupola"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-LoaderCupola"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T62-PeriscopeLens"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        view,
                        "T62-TurretToolRoll"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "T62-DshkReceiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T62-DshkBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T62-RadioWhip"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(view, "ERA"),
                    Is.EqualTo(0));
                Assert.That(
                    CountPrefix(
                        view,
                        "Painted-T62-Smoke"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsKtd2LunaAndU5TsOnAuthoritativeGun()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-Ktd2Pod"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T62-Ktd2Lens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-LunaSearchlight"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "T62-LunaLens"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-T62-LunaYoke"),
                    Is.EqualTo(2));
                foreach (string name in new[]
                    {
                        "Painted-T62-Ktd2Pod",
                        "Painted-T62-LunaSearchlight",
                        "Painted-T62-U5TsRoot",
                        "Painted-T62-U5TsEvacuator",
                        "T62-U5TsMuzzleBore"
                    })
                {
                    AssertGunOwned(view, name);
                }
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
                catalog.GetVehicle("t62mv1");
            return TankView.Create(
                new TankState(
                    "t62-obr1975-test",
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
