using System.Linq;
using ClaudeOfTanks.Runtime;
using ClaudeOfTanks.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeOfTanks.Tests
{
    public sealed class MerkavaFleetVisualTests
    {
        private static readonly string[] ProductionIds =
        {
            "merkava1b",
            "merkava2b",
            "merkava2d",
            "merkava3c",
            "merkava3d",
            "merkava4b"
        };

        [Test]
        public void ProductionFamilyUsesAuthoredArmorAndCommonFittings()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            for (int index = 0;
                index < ProductionIds.Length;
                index++)
            {
                string id = ProductionIds[index];
                VehicleDefinition definition =
                    catalog.GetVehicle(id);
                TankView view = Create(catalog, id);
                try
                {
                    Assert.That(
                        Count(view, "SideArmor"),
                        Is.EqualTo(0),
                        id);
                    Assert.That(
                        CountPrefix(view, "Armor-"),
                        Is.EqualTo(
                            FleetVisualAssertions
                                .ValidPlateCount(
                                    definition)),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Merkava-RearServiceGrille"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Merkava-RearServiceSlat"),
                        Is.EqualTo(10),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Merkava-ClamshellDoor"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(view, "Merkava-DoorHinge"),
                        Is.EqualTo(4),
                        id);
                    Assert.That(
                        Count(view, "Merkava-RearMarker"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(view, "Merkava-RearTowEye"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Merkava-SmokeLauncher"),
                        Is.EqualTo(12),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Merkava-SmokeBankShoe"),
                        Is.EqualTo(2),
                        id);
                    Assert.That(
                        Count(view, "Merkava-Antenna"),
                        Is.EqualTo(
                            id == "merkava3d"
                                ? 1
                                : 2),
                        id);
                    Assert.That(
                        Count(view, "Merkava-BasketRail"),
                        Is.EqualTo(17),
                        id);
                    Assert.That(
                        Count(
                            view,
                            "Painted-Merkava-BasketStowage"),
                        Is.EqualTo(5),
                        id);
                    Assert.That(
                        Count(view, "Merkava-BallChain"),
                        Is.EqualTo(16),
                        id);
                    Assert.That(
                        Count(view, "Merkava-ChainBall"),
                        Is.EqualTo(16),
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
        public void GenerationsKeepDistinctRoofAndWeaponLayouts()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView one = Create(catalog, "merkava1b");
            TankView two = Create(catalog, "merkava2b");
            TankView twoD = Create(catalog, "merkava2d");
            TankView threeC = Create(catalog, "merkava3c");
            TankView threeD = Create(catalog, "merkava3d");
            TankView fourB = Create(catalog, "merkava4b");
            try
            {
                Assert.That(
                    MachineGunCount(one),
                    Is.EqualTo(4));
                Assert.That(
                    MachineGunCount(two),
                    Is.EqualTo(4));
                Assert.That(
                    MachineGunCount(twoD),
                    Is.EqualTo(4));
                Assert.That(
                    MachineGunCount(threeC),
                    Is.EqualTo(4));
                Assert.That(
                    MachineGunCount(threeD),
                    Is.EqualTo(4));
                Assert.That(
                    MachineGunCount(fourB),
                    Is.EqualTo(3));

                foreach (TankView early in new[]
                    {
                        one,
                        two,
                        twoD
                    })
                {
                    Assert.That(
                        Count(
                            early,
                            "Painted-Merkava-60mmMortarLid"),
                        Is.EqualTo(1));
                    Assert.That(
                        Count(
                            early,
                            "Painted-Merkava-PanoramicSight"),
                        Is.EqualTo(0));
                }
                foreach (TankView late in new[]
                    {
                        threeC,
                        threeD,
                        fourB
                    })
                {
                    Assert.That(
                        Count(
                            late,
                            "Painted-Merkava-60mmMortarLid"),
                        Is.EqualTo(0));
                    Assert.That(
                        Count(
                            late,
                            "Painted-Merkava-PanoramicSight"),
                        Is.EqualTo(1));
                }
                Assert.That(
                    Count(
                        threeD,
                        "Painted-Merkava3D-WideHatchCollar"),
                    Is.EqualTo(2));
                Assert.That(
                    CountPrefix(
                        threeC,
                        "Painted-Merkava3D-"),
                    Is.EqualTo(0));
            }
            finally
            {
                one.Destroy();
                two.Destroy();
                twoD.Destroy();
                threeC.Destroy();
                threeD.Destroy();
                fourB.Destroy();
            }
        }

        [Test]
        public void VariantStowageAndLatticesRemainSpecific()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView one = Create(catalog, "merkava1b");
            TankView two = Create(catalog, "merkava2b");
            TankView twoD = Create(catalog, "merkava2d");
            TankView threeC = Create(catalog, "merkava3c");
            TankView threeD = Create(catalog, "merkava3d");
            TankView fourB = Create(catalog, "merkava4b");
            try
            {
                Assert.That(
                    Count(
                        one,
                        "Painted-Merkava-Early-RoofPack"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        two,
                        "Painted-Merkava-Early-RoofPack"),
                    Is.EqualTo(4));
                Assert.That(
                    Count(
                        twoD,
                        "Painted-Merkava-Early-RoofPack"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(
                        twoD,
                        "Painted-Merkava2D-SightShoe"),
                    Is.EqualTo(1));
                Assert.That(
                    Count(
                        threeC,
                        "Painted-Merkava3C-RoofPack"),
                    Is.EqualTo(6));
                Assert.That(
                    Count(
                        threeD,
                        "Painted-Merkava3D-RoofPack"),
                    Is.EqualTo(7));
                Assert.That(
                    Count(
                        threeD,
                        "Merkava3D-RearLattice"),
                    Is.EqualTo(11));
                Assert.That(
                    Count(
                        fourB,
                        "Painted-Merkava4B-RoofCase"),
                    Is.EqualTo(3));
                Assert.That(
                    Count(
                        fourB,
                        "Painted-Merkava4B-BustlePack"),
                    Is.EqualTo(5));
                Assert.That(
                    Count(
                        fourB,
                        "Merkava4B-RearLattice"),
                    Is.EqualTo(13));
            }
            finally
            {
                one.Destroy();
                two.Destroy();
                twoD.Destroy();
                threeC.Destroy();
                threeD.Destroy();
                fourB.Destroy();
            }
        }

        [Test]
        public void FittingsUseCatalogSeatsOwnershipAndCamouflage()
        {
            ContentCatalog catalog = ContentCatalog.Load();
            TankView early = Create(
                catalog,
                "merkava2d",
                "summer");
            TankView late = Create(
                catalog,
                "merkava4b",
                "summer");
            try
            {
                VehicleDefinition definition =
                    catalog.GetVehicle("merkava4b");
                Transform door = Find(
                    late,
                    "Painted-Merkava-ClamshellDoor");
                Assert.That(
                    door.localPosition.z,
                    Is.EqualTo(
                        HullRear(definition) -
                        0.035f)
                        .Within(0.0001f));
                Transform smoke = Find(
                    late,
                    "Painted-Merkava-SmokeLauncher");
                Assert.That(
                    smoke.localPosition.y,
                    Is.EqualTo(
                        TurretRoof(definition) -
                        0.12f)
                        .Within(0.0001f));

                Transform earlyGun = Find(
                    early,
                    "Merkava-GunCradleM2-Receiver");
                Transform lateGun = Find(
                    late,
                    "Merkava4B-GunCradleM2-Receiver");
                Assert.That(
                    earlyGun.parent.name,
                    Is.EqualTo("Gun"));
                Assert.That(
                    lateGun.parent.name,
                    Is.EqualTo("Gun"));

                Renderer hull = late.Root
                    .Find("Hull")
                    .GetComponent<Renderer>();
                Renderer painted = Find(
                    late,
                    "Painted-Merkava4B-BustlePack")
                    .GetComponent<Renderer>();
                Renderer lens = Find(
                    late,
                    "Merkava-PanoramicLens")
                    .GetComponent<Renderer>();
                Assert.That(
                    painted.sharedMaterial.mainTexture,
                    Is.SameAs(
                        hull.sharedMaterial.mainTexture));
                Assert.That(
                    lens.sharedMaterial.mainTexture,
                    Is.Null);
            }
            finally
            {
                early.Destroy();
                late.Destroy();
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
                    "merkava-" + id,
                    Team.Alpha,
                    definition.ToTankSpec(),
                    Float3.Zero,
                    0f),
                definition,
                camouflage,
                "forest",
                catalog);
        }

        private static int MachineGunCount(
            TankView view)
        {
            return view.Root
                .GetComponentsInChildren<Transform>()
                .Count(item =>
                    (item.name.StartsWith("Merkava-") ||
                     item.name.StartsWith("Merkava4B-")) &&
                    item.name.EndsWith(
                        "-Receiver"));
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

        private static float HullRear(
            VehicleDefinition definition)
        {
            return definition.armor.hullPlates
                .SelectMany(item => item.verts)
                .Min(item => item.z);
        }

        private static float TurretRoof(
            VehicleDefinition definition)
        {
            return definition.armor.turretPlates
                .Where(item =>
                    item.name.StartsWith(
                        "turret_roof"))
                .SelectMany(item => item.verts)
                .Max(item => item.y);
        }
    }
}
