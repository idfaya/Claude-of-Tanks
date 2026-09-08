using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class LeopardFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "leopard2_proto",
            "leo2a4",
            "leo2a4_otco",
            "leo2a4m",
            "leo2a5",
            "leo2a5_a5nl",
            "leo2a6",
            "leo2a6m",
            "leo2_revolution",
            "leo2a7v",
            "leo2a6_ua"
        };

        [Test]
        public void ProductionFamilyUsesAuthoredArmorAndCommonFittings()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            for (int i = 0;
                i < ProductionIds.Length;
                i++)
            {
                string id = ProductionIds[i];
                VehicleDefinition definition =
                    catalog.GetVehicle(id);
                TankView view = Create(catalog, id);
                try
                {
                    Assert.That(
                        Count(view, "TurretWedge"),
                        Is.EqualTo(0),
                        id);
                    Assert.That(
                        Count(view, "SideArmor"),
                        Is.EqualTo(0),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Leopard-SmokeLauncher"),
                        Is.EqualTo(16),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Leopard-Hatch"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(view, "Leopard-Antenna"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Leopard-EngineGrille"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Leopard-BustleRackBar"),
                        Is.EqualTo(5),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Leopard-MG3-Receiver"),
                        Is.EqualTo(1),
                        id);
                    Assert.That(
                        CountPrefix(view, "Armor-"),
                        Is.EqualTo(
                            FleetVisualAssertions
                                .ValidPlateCount(
                                    definition)),
                        id);
                    Assert.That(
                        view.Root
                            .GetComponentsInChildren<
                                Collider>()
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
        public void BoxyAndWedgeLineagesStayDistinct()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            TankView prototype = Create(
                catalog,
                "leopard2_proto");
            TankView a4 = Create(catalog, "leo2a4");
            TankView a5 = Create(catalog, "leo2a5");
            TankView a6 = Create(catalog, "leo2a6");
            try
            {
                Assert.That(
                    CountPrefix(prototype, "Armor-"),
                    Is.EqualTo(19));
                Assert.That(
                    CountPrefix(a4, "Armor-"),
                    Is.EqualTo(19));
                Assert.That(
                    CountPrefix(a5, "Armor-"),
                    Is.EqualTo(30));
                Assert.That(
                    CountPrefix(a6, "Armor-"),
                    Is.EqualTo(24));
                Assert.That(
                    Count(
                        prototype,
                        "Painted-Leopard-Prototype-Rangefinder"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        prototype,
                        "Painted-Leopard-Prototype-Searchlight"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        prototype,
                        "Painted-Leopard-EMESHousing"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        a4,
                        "Painted-Leopard-EMESHousing"),
                    Is.EqualTo(1));
            }
            finally
            {
                prototype.Destroy();
                a4.Destroy();
                a5.Destroy();
                a6.Destroy();
            }
        }

        [Test]
        public void ModernizationPackagesRemainVariantSpecific()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            TankView otco = Create(
                catalog,
                "leo2a4_otco");
            TankView a4m = Create(
                catalog,
                "leo2a4m");
            TankView a5nl = Create(
                catalog,
                "leo2a5_a5nl");
            TankView a6m = Create(
                catalog,
                "leo2a6m");
            TankView ua = Create(
                catalog,
                "leo2a6_ua");
            TankView revolution = Create(
                catalog,
                "leo2_revolution");
            TankView a7v = Create(
                catalog,
                "leo2a7v");
            try
            {
                Assert.That(
                    Count(
                        otco,
                        "Painted-Leopard-OTCO-Shroud"),
                    Is.EqualTo(8));
                Assert.That(
                    Count(
                        otco,
                        "Painted-Leopard-OTCO-RoofWeapon-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a4m,
                        "Painted-Leopard-A4M-ArmorCassette"),
                    Is.EqualTo(14));
                Assert.That(
                    Count(
                        a4m,
                        "Leopard-A4M-HullSlat"),
                    Is.EqualTo(50));
                Assert.That(
                    Count(
                        a5nl,
                        "Painted-Leopard-A5NL-AwarenessPod"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        a5nl,
                        "Painted-Leopard-A5NL-Panorama"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a5nl,
                        "Painted-Leopard-A5NL-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a6m,
                        "Leopard-A6M-HullSlat"),
                    Is.EqualTo(60));
                Assert.That(
                    Count(
                        a6m,
                        "Leopard-A6M-TurretSlat"),
                    Is.EqualTo(30));
                Assert.That(
                    Count(
                        a6m,
                        "Painted-Leopard-A6M-Cooler"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a6m,
                        "Painted-Leopard-A6M-RoofRWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        ua,
                        "Leopard-UA-HullSlat"),
                    Is.EqualTo(70));
                Assert.That(
                    Count(
                        ua,
                        "Leopard-UA-TurretSlat"),
                    Is.EqualTo(60));
                Assert.That(
                    Count(
                        ua,
                        "Leopard-UA-RoofBasket"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        ua,
                        "Painted-Leopard-UA-HeavyRWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        ua,
                        "Painted-Leopard-UA-LightRWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        revolution,
                        "Painted-Leopard-Revolution-SEOSS"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        revolution,
                        "Painted-Leopard-Revolution-Electronics"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        revolution,
                        "Painted-Leopard-Revolution-RWS-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a7v,
                        "Painted-Leopard-A7V-APU"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        a7v,
                        "Painted-Leopard-A7V-ACUnit"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        a7v,
                        "Leopard-A7V-ADSSensor"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        a7v,
                        "Leopard-A7V-RearSlat"),
                    Is.EqualTo(4));
            }
            finally
            {
                otco.Destroy();
                a4m.Destroy();
                a5nl.Destroy();
                a6m.Destroy();
                ua.Destroy();
                revolution.Destroy();
                a7v.Destroy();
            }
        }

        [Test]
        public void FittingsUseCatalogSeatsAndCamouflageSemantics()
        {
            ContentCatalog catalog =
                ContentCatalog.Load();
            TankView a7v = Create(
                catalog,
                "leo2a7v",
                "summer");
            TankView a6m = Create(
                catalog,
                "leo2a6m",
                "summer");
            TankView ua = Create(
                catalog,
                "leo2a6_ua",
                "summer");
            try
            {
                VehicleDefinition definition =
                    catalog.GetVehicle("leo2a7v");
                Transform apu = Find(
                    a7v,
                    "Painted-Leopard-A7V-APU");
                Assert.That(
                    Mathf.Abs(apu.localPosition.x),
                    Is.EqualTo(
                        definition.dims.widthM *
                        0.33f)
                        .Within(0.0001f));
                Assert.That(
                    apu.localPosition.y,
                    Is.EqualTo(
                        HullRoof(definition) +
                        0.12f)
                        .Within(0.0001f));
                Assert.That(
                    apu.localPosition.z,
                    Is.EqualTo(
                        HullRear(definition) +
                        0.55f)
                        .Within(0.0001f));

                Renderer hull = a7v.Root
                    .Find("Hull")
                    .GetComponent<Renderer>();
                Renderer painted = apu
                    .GetComponent<Renderer>();
                Renderer sensor = Find(
                    a7v,
                    "Leopard-A7V-ADSSensor")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.SameAs(
                        hull.sharedMaterial.mainTexture));
                Assert.That(
                    sensor.sharedMaterial.mainTexture,
                    Is.Null);

                Assert.That(
                    Mathf.Abs(
                        Find(
                            a6m,
                            "Leopard-A6M-HullSlat")
                            .localPosition.x),
                    Is.EqualTo(1.99f)
                        .Within(0.0001f));
                Assert.That(
                    Mathf.Abs(
                        Find(
                            ua,
                            "Leopard-UA-HullSlat")
                            .localPosition.x),
                    Is.EqualTo(2.22f)
                        .Within(0.0001f));
            }
            finally
            {
                a7v.Destroy();
                a6m.Destroy();
                ua.Destroy();
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
                    "leopard-" + id,
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

        private static float HullRoof(
            VehicleDefinition definition)
        {
            return definition.armor.hullPlates
                .SelectMany(item => item.verts)
                .Max(item => item.y);
        }

        private static float HullRear(
            VehicleDefinition definition)
        {
            return definition.armor.hullPlates
                .SelectMany(item => item.verts)
                .Min(item => item.z);
        }
    }
}
