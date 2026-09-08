using System.Collections.Generic;
using ClaudeOfTanks.Runtime;
using NUnit.Framework;

namespace ClaudeOfTanks.Tests
{
    public sealed class GarageLoadoutTests
    {
        [Test]
        public void SelectionPersistsPerVehicleAndEnforcesEligibility()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            MemoryStore store = new MemoryStore();
            GarageLoadoutController loadout =
                new GarageLoadoutController(catalog, store);

            loadout.SelectVehicle("m1a2");
            Assert.That(
                loadout.SetEquipment("rammer", true),
                Is.True);
            Assert.That(
                loadout.SetEquipment("vstab", true),
                Is.True);
            Assert.That(
                loadout.SetEquipment("optics", true),
                Is.True);
            Assert.That(
                loadout.SetEquipment("toolbox", true),
                Is.False);
            Assert.That(
                loadout.SetCamouflage("winter"),
                Is.True);

            loadout.SelectVehicle("tiger1");
            Assert.That(
                loadout.SetEquipment("vstab", true),
                Is.False);
            Assert.That(loadout.Equipment, Is.Empty);

            loadout.SelectVehicle("m1a3");
            Assert.That(
                loadout.SetEquipment("rammer", true),
                Is.False);

            loadout.SelectVehicle("m1a2");
            Assert.That(
                loadout.Equipment,
                Is.EqualTo(new[]
                {
                    "rammer",
                    "vstab",
                    "optics"
                }));
            Assert.That(
                loadout.CamouflageId,
                Is.EqualTo("winter"));
        }

        [Test]
        public void GeneratedCatalogCarriesCanonicalLoadoutData()
        {
            ContentCatalog catalog = ContentCatalog.Load();

            Assert.That(catalog.Equipment, Has.Length.EqualTo(14));
            Assert.That(
                catalog.GetEquipment("spall_liner").description,
                Does.Contain("splash"));
            Assert.That(
                catalog.Camouflage,
                Has.Length.GreaterThanOrEqualTo(100));
            Assert.That(
                catalog.ContainsCamouflage("service_t90m"),
                Is.True);
            Assert.That(
                catalog.ContainsCamouflage("custom"),
                Is.False);
            Assert.That(catalog.Camouflage, Has.Length.EqualTo(112));
            for (int i = 0; i < catalog.Camouflage.Length; i++)
            {
                CamouflageDefinition camouflage =
                    catalog.Camouflage[i];
                Assert.That(
                    camouflage.recipe,
                    Is.Not.Null,
                    camouflage.id);
                Assert.That(
                    camouflage.recipe.scheme,
                    Is.Not.Empty,
                    camouflage.id);
                Assert.That(
                    camouflage.recipe.baseColor,
                    Does.StartWith("#"),
                    camouflage.id);
            }

            CamouflageDefinition service =
                catalog.GetCamouflage("service_t90m");
            Assert.That(service.recipe.scheme, Is.EqualTo("digital"));
            Assert.That(service.recipe.patchColors, Has.Length.EqualTo(3));
            Assert.That(service.recipe.camoScale, Is.EqualTo(0.42f));

            Assert.That(
                TankCamouflage.ResolveId(
                    catalog.GetVehicle("m1a2"),
                    "factory",
                    "desert"),
                Is.EqualTo("service_usa_desert"));
            Assert.That(
                TankCamouflage.ResolveId(
                    catalog.GetVehicle("t90m"),
                    "signature",
                    "desert"),
                Is.EqualTo("sig_t90m"));
        }

        private sealed class MemoryStore :
            IGarageLoadoutStore
        {
            private readonly Dictionary<string, string[]> _equipment =
                new Dictionary<string, string[]>();
            private readonly Dictionary<string, string> _camouflage =
                new Dictionary<string, string>();

            public string[] LoadEquipment(string vehicleId)
            {
                string[] value;
                return _equipment.TryGetValue(vehicleId, out value)
                    ? (string[])value.Clone()
                    : System.Array.Empty<string>();
            }

            public string LoadCamouflage(string vehicleId)
            {
                string value;
                return _camouflage.TryGetValue(vehicleId, out value)
                    ? value
                    : "factory";
            }

            public void SaveEquipment(
                string vehicleId,
                string[] equipment)
            {
                _equipment[vehicleId] =
                    (string[])equipment.Clone();
            }

            public void SaveCamouflage(
                string vehicleId,
                string camouflageId)
            {
                _camouflage[vehicleId] = camouflageId;
            }
        }
    }
}
