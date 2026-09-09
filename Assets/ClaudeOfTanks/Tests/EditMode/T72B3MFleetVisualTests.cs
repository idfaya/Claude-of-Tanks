using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class T72B3MFleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndExactSixWheelCourse()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            VehicleDefinition definition =
                catalog.GetVehicle("t72b3m");
            TankView view =
                Create(catalog);
            try
            {
                Assert.That(
                    CountPrefix(view, "Armor-"),
                    Is.EqualTo(223));
                Assert.That(
                    definition.armor.hullPlates
                        .Concat(definition.armor.turretPlates)
                        .Count(plate => plate.kind == "era"),
                    Is.EqualTo(204));
                Assert.That(
                    FindAll(view, "RoadWheel-L").Length,
                    Is.EqualTo(6));
                Assert.That(
                    FindAll(view, "RoadWheel-R").Length,
                    Is.EqualTo(6));
                Assert.That(
                    Count(view, "T72B3M-ReturnRoller"),
                    Is.EqualTo(6));

                Assert.That(
                    Find(view, "Sprocket-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.33f,
                        0.74f,
                        -3.46f)));
                Assert.That(
                    Find(view, "Idler-L")
                        .localPosition,
                    Is.EqualTo(new Vector3(
                        -1.33f,
                        0.8f,
                        1.38f)));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void ReplacesGenericShellAndKeepsReliktVisible()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                AssertHidden(view, "Hull");
                AssertHidden(view, "UpperHull");
                AssertHidden(view, "Turret");
                AssertHidden(view, "Gun");
                Assert.That(
                    Count(view, "SideArmor"),
                    Is.EqualTo(0));
                Assert.That(
                    VisibleArmor(view),
                    Is.EqualTo(204));
                Assert.That(
                    Count(view, "Painted-Soviet-FuelDrum"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(view, "Soviet-MachineGunReceiver"),
                    Is.EqualTo(0));
                Assert.That(
                    view.Root.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsReliktHullAndRearCageIdentity()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-T72B3M-UpperHull"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T72B3M-ReliktSoftBag"),
                    Is.EqualTo(18));
                Assert.That(
                    Count(view, "Painted-T72B3M-ReliktGlacisCassette"),
                    Is.EqualTo(13));
                Assert.That(
                    Count(view, "T72B3M-EngineLouvre"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(view, "T72B3M-RearSlat"),
                    Is.EqualTo(11));
                Assert.That(
                    Count(view, "Painted-T72B3M-RearFuelDrum"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void BuildsSosnaReliktTurretAndBasket()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                Assert.That(
                    Count(view, "Painted-T72B3M-CastDome"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "Painted-T72B3M-ReliktTurretCassette"),
                    Is.EqualTo(10));
                Assert.That(
                    Count(view, "Painted-T72B3M-TurretSoftBag"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(view, "Painted-T72B3M-SosnaUHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T72B3M-NsvtReceiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(view, "T72B3M-TurretBasketSlat"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(view, "Painted-T72B3M-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(
                    Count(view, "T72B3M-RadioWhip"),
                    Is.EqualTo(2));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void KeepsTwoA46M5OnAuthoritativeGun()
        {
            TankView view =
                Create(ContentCatalog.Load());
            try
            {
                foreach (string name in new[]
                    {
                        "Painted-T72B3M-2A46M5Saddle",
                        "Painted-T72B3M-2A46M5Root",
                        "Painted-T72B3M-2A46M5Evacuator",
                        "Painted-T72B3M-2A46M5ForwardTube",
                        "T72B3M-MuzzleBore"
                    })
                {
                    AssertGunOwned(view, name);
                }
                Assert.That(
                    Count(view, "T72B3M-2A46M5SleeveRing"),
                    Is.EqualTo(6));
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
                catalog.GetVehicle("t72b3m");
            return TankView.Create(
                new TankState(
                    "t72-b3m-test",
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
        }

        private static int VisibleArmor(
            TankView view)
        {
            return FindAllByPrefix(view, "Armor-")
                .Count(item =>
                    item.GetComponent<Renderer>().enabled);
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
            return FindAllByPrefix(view, prefix).Length;
        }

        private static Transform[] FindAll(
            TankView view,
            string name)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Where(item => item.name == name)
                .ToArray();
        }

        private static Transform[] FindAllByPrefix(
            TankView view,
            string prefix)
        {
            return view.Root
                .GetComponentsInChildren<Transform>(true)
                .Where(item => item.name.StartsWith(prefix))
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
