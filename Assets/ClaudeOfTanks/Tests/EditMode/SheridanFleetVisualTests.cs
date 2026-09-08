using System.Collections.Generic;
using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class SheridanFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "m551_sheridan",
            "m551a1_tts"
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
                        Is.EqualTo(
                            id == "m551_sheridan"
                                ? 38
                                : 118),
                        id);
                    Assert.That(
                        Count(view, "SideArmor"),
                        Is.EqualTo(0),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Sheridan-EngineGrille"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Sheridan-EngineLouvre"),
                        Is.EqualTo(
                            id == "m551_sheridan"
                                ? 12
                                : 14),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Sheridan-CommanderVisionBlock"),
                        Is.EqualTo(8),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Sheridan-SmokeLauncher"),
                        Is.EqualTo(8),
                        id);
                    Assert.That(
                        Count(view, "Sheridan-Antenna"),
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
        public void BaseSheridanKeepsRoofWeaponsAndFuelDrums()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "m551_sheridan");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Sheridan-CommanderM2-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Sheridan-LoaderMAG-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Sheridan-RearFuelDrum"),
                    Is.EqualTo(2));
                Assert.That(
                    Count(
                        view,
                        "Sheridan-FuelDrumSupportRail"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        view,
                        "Painted-Sheridan-RearStowage"),
                    Is.EqualTo(1));
                Assert.That(
                    CountPrefix(
                        view,
                        "Painted-Sheridan-TTS-") +
                    CountPrefix(
                        view,
                        "Sheridan-TTS-"),
                    Is.EqualTo(0));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void TtsKeepsModernizationAndOpenBustle()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "m551a1_tts");
            try
            {
                Assert.That(
                    Count(
                        view,
                        "Sheridan-CommanderM2-Receiver"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        view,
                        "Sheridan-LoaderMAG-Receiver"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Sheridan-RearFuelDrum"),
                    Is.EqualTo(0));
                Assert.That(
                    Count(
                        view,
                        "Painted-Sheridan-TTS-RearDeckExtension"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Sheridan-TTS-APU"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Sheridan-TTS-SkirtCagePost"),
                    Is.EqualTo(16));
                Assert.That(
                    Count(
                        view,
                        "Painted-Sheridan-TTS-AutocannonStation"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Sheridan-TTS-AutocannonBarrel"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Painted-Sheridan-TTS-SearchlightHousing"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        view,
                        "Sheridan-TTS-BustleSideRail"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        view,
                        "Sheridan-TTS-BustlePost"),
                    Is.EqualTo(5));
            }
            finally
            {
                view.Destroy();
            }
        }

        [Test]
        public void M81FittingsFollowGunAndPaintedPartsUseCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView view = Create(
                catalog,
                "m551a1_tts",
                "digital");
            try
            {
                AssertGunOwned(
                    view,
                    "Painted-Sheridan-M81-Mantlet");
                AssertGunOwned(
                    view,
                    "Painted-Sheridan-M81-MuzzleRing");
                AssertGunOwned(
                    view,
                    "Sheridan-M81-CoaxBarrel");

                Renderer painted = Find(
                        view,
                        "Painted-Sheridan-TTS-AutocannonStation")
                    .GetComponent<Renderer>();
                Renderer weapon = Find(
                        view,
                        "Sheridan-TTS-AutocannonMechanism")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.Not.Null);
                Assert.That(
                    weapon.sharedMaterial.mainTexture,
                    Is.Null);
            }
            finally
            {
                view.Destroy();
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
                    "sheridan-" + id,
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
