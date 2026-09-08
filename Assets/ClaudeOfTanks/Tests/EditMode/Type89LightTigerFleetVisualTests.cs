using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class Type89LightTigerFleetVisualTests
    {
        [Test]
        public void UsesCatalogArmorAndScaledJapaneseCourse()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            VehicleDefinition definition = catalog.GetVehicle("type89_light_tiger");
            TankView view = Create(catalog);
            try
            {
                Assert.That(CountPrefix(view, "Armor-"), Is.EqualTo(24));
                Assert.That(CountPrefix(view, "Armor-"),
                    Is.EqualTo(FleetVisualAssertions.ValidPlateCount(definition)));
                Assert.That(Count(view, "SideArmor"), Is.EqualTo(0));
                Assert.That(Count(view, "MissilePod"), Is.EqualTo(0));
                Assert.That(CountPrefix(view, "RoadWheel-"), Is.EqualTo(12));
                Assert.That(view.Root.GetComponentsInChildren<Collider>().Length,
                    Is.EqualTo(0));
                Assert.That(Find(view, "Sprocket-L").localPosition,
                    Is.EqualTo(new Vector3(-1.278f, 0.756f, 2.7f)));
                Assert.That(Find(view, "Idler-L").localPosition,
                    Is.EqualTo(new Vector3(-1.278f, 0.693f, -2.664f)));
                Bounds track = Find(view, "TrackLinks-L")
                    .GetComponent<MeshFilter>().sharedMesh.bounds;
                Assert.That(track.min.x, Is.EqualTo(-1.485f).Within(0.002f));
                Assert.That(track.max.x, Is.EqualTo(-1.071f).Within(0.002f));
                Assert.That(track.min.y, Is.EqualTo(0.0063f).Within(0.02f));
                Assert.That(track.max.y, Is.EqualTo(1.1228f).Within(0.02f));
                Assert.That(track.min.z, Is.EqualTo(-3.0042f).Within(0.03f));
                Assert.That(track.max.z, Is.EqualTo(3.0637f).Within(0.03f));
                foreach (string name in new[]
                    { "Hull", "UpperHull", "Turret", "Gun",
                      "Armor-track_L", "Armor-track_R" })
                    Assert.That(Find(view, name).GetComponent<Renderer>().enabled,
                        Is.False);
            }
            finally { view.Destroy(); }
        }

        [Test]
        public void KeepsIndependentHullAndLayeredArmor()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(Count(view, "Painted-LightTiger-PlanarGlacis"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-LightTiger-SideCassette"),
                    Is.EqualTo(18));
                Assert.That(Count(view, "LightTiger-CassetteFace"),
                    Is.EqualTo(18));
                Assert.That(Count(view, "Painted-LightTiger-RearRamp"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "LightTiger-EngineLouvre"),
                    Is.EqualTo(9));
                Assert.That(Count(view, "Painted-Type89-LongGlacis"),
                    Is.EqualTo(0));
            }
            finally { view.Destroy(); }
        }

        [Test]
        public void KeepsFourTubeTurretSensorsRwsAndGunPlant()
        {
            TankView view = Create(ContentCatalog.Load());
            try
            {
                Assert.That(Count(view, "Painted-LightTiger-JyuMatPod"),
                    Is.EqualTo(2));
                Assert.That(Count(view, "LightTiger-JyuMatMouth"),
                    Is.EqualTo(4));
                Assert.That(Count(view, "Painted-LightTiger-RoofOptic"),
                    Is.EqualTo(2));
                Assert.That(Count(view, "LightTiger-RoofOpticLens"),
                    Is.EqualTo(2));
                Assert.That(Count(view, "Painted-LightTiger-PanoramaTower"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "LightTiger-RwsReceiver"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "Painted-LightTiger-SmokeLauncher"),
                    Is.EqualTo(12));
                Assert.That(Count(view, "Painted-LightTiger-KdeBarrel"),
                    Is.EqualTo(1));
                Assert.That(Count(view, "LightTiger-CoaxBarrel"),
                    Is.EqualTo(1));
                Transform barrel = Find(view, "Painted-LightTiger-KdeBarrel");
                Assert.That(barrel.parent.parent.name, Is.EqualTo("Gun"));
            }
            finally { view.Destroy(); }
        }

        private static TankView Create(ContentCatalog catalog)
        {
            VehicleDefinition definition = catalog.GetVehicle("type89_light_tiger");
            return TankView.Create(
                new TankState("light-tiger-test", Team.Alpha,
                    definition.ToTankSpec(), Float3.Zero, 0f),
                definition, "factory", "forest", catalog);
        }

        private static int Count(TankView view, string name)
        {
            return view.Root.GetComponentsInChildren<Transform>()
                .Count(item => item.name == name);
        }

        private static int CountPrefix(TankView view, string prefix)
        {
            return view.Root.GetComponentsInChildren<Transform>()
                .Count(item => item.name.StartsWith(prefix));
        }

        private static Transform Find(TankView view, string name)
        {
            return view.Root.GetComponentsInChildren<Transform>()
                .First(item => item.name == name);
        }
    }
}
