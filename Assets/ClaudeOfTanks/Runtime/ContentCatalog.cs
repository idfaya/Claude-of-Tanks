using System;
using ClaudeOfTanks.Simulation;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class ContentCatalog
    {
        private const string ResourcePath = "Generated/content-catalog";
        private const int SupportedSchemaVersion = 5;
        private readonly CatalogData _data;

        private ContentCatalog(CatalogData data)
        {
            _data = data;
        }

        public int SavedVehicleCount => _data.counts.savedVehicles;
        public int ReleaseVehicleCount => _data.counts.releaseVehicles;
        public int ProductionVehicleCount => _data.counts.productionVehicles;
        public int MapCount => _data.counts.maps;
        public string[] ProductionVehicleIds => _data.catalogs.production;
        public VehicleDefinition[] Vehicles => _data.vehicles;
        public MapDefinition[] Maps => _data.maps;
        public EquipmentDefinition[] Equipment => _data.loadout.equipment;
        public CamouflageDefinition[] Camouflage =>
            _data.loadout.camouflage;

        public static ContentCatalog Load()
        {
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                throw new InvalidOperationException("Generated content catalog is missing.");
            }

            CatalogData data = JsonUtility.FromJson<CatalogData>(asset.text);
            if (data == null || data.schemaVersion != SupportedSchemaVersion)
            {
                throw new InvalidOperationException("Unsupported content catalog schema.");
            }

            return new ContentCatalog(data);
        }

        public bool ContainsVehicle(string id)
        {
            for (int i = 0; i < _data.vehicles.Length; i++)
            {
                if (_data.vehicles[i].id == id) return true;
            }
            return false;
        }

        public bool ContainsMap(string id)
        {
            for (int i = 0; i < _data.maps.Length; i++)
            {
                if (_data.maps[i].id == id) return true;
            }
            return false;
        }

        public VehicleDefinition GetVehicle(string id)
        {
            for (int i = 0; i < _data.vehicles.Length; i++)
            {
                if (_data.vehicles[i].id == id) return _data.vehicles[i];
            }
            throw new ArgumentException("Unknown vehicle id: " + id, nameof(id));
        }

        public MapDefinition GetMap(string id)
        {
            for (int i = 0; i < _data.maps.Length; i++)
            {
                if (_data.maps[i].id == id) return _data.maps[i];
            }
            throw new ArgumentException("Unknown map id: " + id, nameof(id));
        }

        public EquipmentDefinition GetEquipment(string id)
        {
            for (int i = 0; i < Equipment.Length; i++)
            {
                if (Equipment[i].id == id) return Equipment[i];
            }
            throw new ArgumentException(
                "Unknown equipment id: " + id,
                nameof(id));
        }

        public bool ContainsCamouflage(string id)
        {
            for (int i = 0; i < Camouflage.Length; i++)
            {
                if (Camouflage[i].id == id) return true;
            }
            return false;
        }

        [Serializable]
        private sealed class CatalogData
        {
            public int schemaVersion;
            public CatalogCounts counts;
            public CatalogLists catalogs;
            public LoadoutCatalogData loadout;
            public VehicleDefinition[] vehicles;
            public MapDefinition[] maps;
        }

        [Serializable]
        private sealed class CatalogCounts
        {
            public int savedVehicles;
            public int releaseVehicles;
            public int productionVehicles;
            public int maps;
        }

        [Serializable]
        private sealed class CatalogLists
        {
            public string[] saved;
            public string[] release;
            public string[] production;
        }

        [Serializable]
        private sealed class LoadoutCatalogData
        {
            public EquipmentDefinition[] equipment;
            public CamouflageDefinition[] camouflage;
        }
    }

    [Serializable]
    public sealed class EquipmentDefinition
    {
        public string id;
        public string name;
        public string shortName;
        public string category;
        public string era;
        public string description;
    }

    [Serializable]
    public sealed class CamouflageDefinition
    {
        public string id;
        public string name;
    }

    [Serializable]
    public sealed class VehicleDefinition
    {
        public string id;
        public string name;
        public string nation;
        public string era;
        public string role;
        public float hp;
        public float enginePowerHp;
        public float weightTons;
        public float topSpeedKmh;
        public float reverseSpeedKmh;
        public float hullTraverseDegS;
        public float turretTraverseDegS;
        public VehicleDimensions dims;
        public VehicleGun gun;
        public VehicleVisual visual;
        public VehicleArmor armor;

        public bool IsModern =>
            era == "cold-war" ||
            era == "modern" ||
            era == "next-generation";

        public bool AllowsEquipment(string equipmentId)
        {
            return LoadoutSimulation.IsEquipmentAllowed(
                equipmentId,
                IsModern,
                gun != null &&
                gun.autoloader != null &&
                gun.autoloader.magazineSize > 1);
        }

        public TankSpec ToTankSpec()
        {
            VehicleShell source = gun != null && gun.shells != null && gun.shells.Length > 0
                ? gun.shells[0] : new VehicleShell();
            float width = dims != null && dims.widthM > 0f ? dims.widthM : 3.4f;
            VehicleGunBloom bloom =
                gun != null && gun.bloom != null
                    ? gun.bloom
                    : new VehicleGunBloom();
            return new TankSpec
            {
                Id = id,
                DisplayName = name,
                Role = string.IsNullOrEmpty(role) ? "medium" : role,
                IsModern = IsModern,
                MaxHealth = hp,
                EnginePowerHp = enginePowerHp,
                WeightTons = weightTons,
                TopSpeedKmh = topSpeedKmh,
                ReverseSpeedKmh = reverseSpeedKmh,
                HullTraverseDegS = hullTraverseDegS,
                TurretTraverseDegS = turretTraverseDegS,
                CollisionRadiusM = width * 0.62f,
                AimTimeS = gun != null && gun.aimTimeS > 0f
                    ? gun.aimTimeS
                    : 2f,
                BaseAccuracyMAt100 =
                    gun != null && gun.baseAccuracy > 0f
                        ? gun.baseAccuracy
                        : 0.36f,
                AimBloomMove = bloom.move,
                AimBloomHullRotation = bloom.hullRot,
                AimBloomTurretRotation = bloom.turret,
                AimBloomAfterShot = bloom.afterShot,
                ViewRangeM =
                    SpottingSimulation.BaseViewRangeM(id, role),
                CamouflageStill =
                    SpottingSimulation.BaseCamouflage(
                        id,
                        role,
                        false),
                CamouflageMoving =
                    SpottingSimulation.BaseCamouflage(
                        id,
                        role,
                        true),
                MagazineSize =
                    gun != null &&
                    gun.autoloader != null &&
                    gun.autoloader.magazineSize > 1
                        ? gun.autoloader.magazineSize
                        : 1,
                MagazineReloadS =
                    gun != null && gun.autoloader != null
                        ? gun.autoloader.fullReloadS
                        : 0f,
                IntraClipS =
                    gun != null && gun.autoloader != null
                        ? gun.autoloader.intraClipS
                        : 0f,
                Shell = new ShellSpec
                {
                    Name = source.name,
                    Type = source.type,
                    CaliberMm = source.caliberMm,
                    VelocityMps = source.velocityMps,
                    Damage = source.dmg,
                    Pen100Mm = source.pen100Mm,
                    Pen1000Mm = source.pen1000Mm,
                    Pen2000Mm = source.pen2000Mm,
                    ReloadS = gun != null ? gun.reloadS : 5.5f,
                    Guided = source.guided
                }
            };
        }
    }

    [Serializable] public sealed class VehicleDimensions
    {
        public float widthM;
        public float heightM;
        public float hullLengthM;
        public float overallLengthM;
    }

    [Serializable] public sealed class VehicleGun
    {
        public float caliberMm;
        public float reloadS;
        public float aimTimeS;
        public float baseAccuracy;
        public VehicleGunBloom bloom;
        public VehicleAutoloader autoloader;
        public VehicleShell[] shells;
    }

    [Serializable] public sealed class VehicleGunBloom
    {
        public float afterShot = 2.8f;
        public float hullRot = 0.2f;
        public float move = 0.2f;
        public float turret = 0.12f;
    }

    [Serializable] public sealed class VehicleAutoloader
    {
        public int magazineSize;
        public float fullReloadS;
        public float intraClipS;
    }

    [Serializable] public sealed class VehicleShell
    {
        public string name = "AP";
        public string type = "AP";
        public float caliberMm = 120f;
        public float velocityMps = 900f;
        public float dmg = 240f;
        public float pen100Mm = 180f;
        public float pen1000Mm = 145f;
        public float pen2000Mm;
        public bool guided;
    }

    [Serializable] public sealed class VehicleVisual
    {
        public string baseColor;
        public string @base;
        public string scheme;
        public string marking;
        public string number;
        public float trackWidthM;

        public Color Color => ColorUtility.TryParseHtmlString(@base, out Color color)
            ? color : new Color(0.28f, 0.32f, 0.24f);
    }

    [Serializable] public sealed class VehicleArmor
    {
        public CatalogPoint turretPivot;
        public CatalogPoint gunPivot;
        public ArmorPlateDefinition[] hullPlates;
        public ArmorPlateDefinition[] turretPlates;
    }

    [Serializable] public sealed class CatalogPoint
    {
        public float x;
        public float y;
        public float z;
        public Vector3 ToVector3() { return new Vector3(x, y, z); }
    }

    [Serializable] public sealed class ArmorPlateDefinition
    {
        public string name;
        public string kind;
        public float physicalMm;
        public float keMm;
        public float ceMm;
        public CatalogPoint[] verts;
    }

    [Serializable] public sealed class MapDefinition
    {
        public string id;
        public string name;
        public string blurb;
        public MapSky sky;
        public MapSpawns spawns;
        public MapTerrain terrain;
        public MapProps props;
        public MapVegetation vegetation;
        public MapSurface unitySurface;
        public MapStructures unityStructures;
        public MapVegetationLayout unityVegetation;
    }

    [Serializable] public sealed class MapSky
    {
        public int fogTintHex;
        public float fogDensity;
        public int sunColorHex;
        public float sunIntensity;
        public float sunElevationDeg;
        public float sunAzimuthDeg;
    }

    [Serializable] public sealed class MapPoint { public float x; public float z; }
    [Serializable] public sealed class MapSpawns { public MapPoint player; public MapPoint[] enemies; }
    [Serializable] public sealed class MapTerrain { public LandformDefinition[] landforms; }
    [Serializable] public sealed class MapSurface
    {
        public MapPolyline[] roads;
        public MapDisc[] lakes;
        public MapDisc[] marshes;
        public bool frozenWater;
        public MapColor groundColor;
        public MapColor hardColor;
        public MapColor softColor;
        public MapColor roadColor;
        public MapColor roadCasingColor;
        public MapColor waterColor;
    }
    [Serializable] public sealed class MapPolyline { public MapPoint[] points; }
    [Serializable] public sealed class MapDisc
    {
        public float x;
        public float z;
        public float r;
        public float depth;
        public float level;
    }
    [Serializable] public sealed class MapColor
    {
        public float r;
        public float g;
        public float b;
        public Color ToColor(float alpha = 1f) { return new Color(r, g, b, alpha); }
    }
    [Serializable] public sealed class MapStructures
    {
        public MapBuilding[] buildings;
        public MapWall[] walls;
        public int rubblePiles;
        public int sandbagLines;
        public int hedgehogs;
        public MapColor buildingColor;
    }
    [Serializable] public sealed class MapBuilding
    {
        public string kind;
        public string profile;
        public float x;
        public float z;
        public float w;
        public float d;
        public float h;
        public float yawDeg;
        public bool tactical;
        public bool destructible;
    }
    [Serializable] public sealed class MapWall
    {
        public float x1;
        public float z1;
        public float x2;
        public float z2;
        public int variant;
    }
    [Serializable] public sealed class LandformDefinition
    {
        public string kind;
        public float x;
        public float z;
        public float height;
        public float length;
        public float width;
        public float rx;
        public float rz;
        public float yawDeg;
    }
    [Serializable] public sealed class MapProps { public int rocks; public int craters; }
    [Serializable] public sealed class MapVegetation
    {
        public int loneCount;
        public int rimCount;
        public int clusterCount;
        public string bushSpecies;
    }
    [Serializable] public sealed class MapVegetationLayout
    {
        public MapVegetationStand[] stands;
        public int treeCount;
    }
    [Serializable] public sealed class MapVegetationStand
    {
        public string zone;
        public float x;
        public float z;
        public float radius;
        public int count;
        public string species;
        public uint seed;
    }
}
