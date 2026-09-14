using System;
using System.IO;
using ClaudeOfTanks.Simulation;
#if COT_STANDALONE_SERVER
using System.Text.Json;
#else
using UnityEngine;
#endif

namespace ClaudeOfTanks.Runtime
{
    public sealed class ContentCatalog
    {
        private const string ResourcePath = "Content/content-catalog";
        private const int SupportedSchemaVersion = 6;
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
#if COT_STANDALONE_SERVER
            throw new PlatformNotSupportedException(
                "Standalone servers must load the content catalog from a file.");
#else
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                throw new InvalidOperationException("Generated content catalog is missing.");
            }

            return LoadFromJson(asset.text);
#endif
        }

        public static ContentCatalog LoadFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Content catalog path is required.", nameof(path));
            return LoadFromJson(File.ReadAllText(Path.GetFullPath(path)));
        }

        public static ContentCatalog LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("Generated content catalog is empty.");
#if COT_STANDALONE_SERVER
            CatalogData data = JsonSerializer.Deserialize<CatalogData>(
                json,
                new JsonSerializerOptions { IncludeFields = true });
#else
            CatalogData data = JsonUtility.FromJson<CatalogData>(json);
#endif
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

        public CamouflageDefinition GetCamouflage(string id)
        {
            for (int i = 0; i < Camouflage.Length; i++)
            {
                if (Camouflage[i].id == id)
                    return Camouflage[i];
            }
            throw new ArgumentException(
                "Unknown camouflage id: " + id,
                nameof(id));
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
        public CamouflageRecipe recipe;
        public CamouflageNationVariant[] nationVariants;
        public bool usesVehicleScale;
        public bool usesAuthoredBasePatch;

        public CamouflageRecipe RecipeFor(string nation)
        {
            if (nationVariants != null)
            {
                for (int i = 0; i < nationVariants.Length; i++)
                {
                    if (nationVariants[i].nation == nation)
                        return nationVariants[i].recipe;
                }
            }
            return recipe;
        }
    }

    [Serializable]
    public sealed class CamouflageNationVariant
    {
        public string nation;
        public CamouflageRecipe recipe;
    }

    [Serializable]
    public sealed class CamouflageRecipe
    {
        public string scheme;
        public string baseColor;
        public string weatherColor;
        public string[] patchColors;
        public float camoScale = 0.34f;
        public float patchK = 1f;
        public float digitalCellK = 1f;
        public float solidWeatheringIntensity = 1f;
        public float bandAngle;
        public float blackK = 1f;
        public float rainK = 1f;
    }

    [Serializable]
    public sealed class VehicleDefinition
    {
        public string id;
        public string name;
        public string nation;
        public string era;
        public string role;
        public string factoryCamouflageId;
        public string signatureCamouflageId;
        public string defaultCamouflageId;
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
        public VehicleHydropneumaticAim hydropneumaticAim;

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
            return VehicleCombatSpecBuilder.Build(this);
        }
    }

    [Serializable] public sealed class VehicleHydropneumaticAim
    {
        public float noseDownDeg;
        public float noseUpDeg;
        public float rateDegS;
        public float compressionM;
        public float droopM;

        public bool IsValid =>
            noseDownDeg > 0f &&
            noseUpDeg > 0f &&
            rateDegS > 0f;

        public HydropneumaticAimSpec ToSpec()
        {
            return new HydropneumaticAimSpec
            {
                NoseDownRad =
                    MathF.Max(0f, noseDownDeg) *
                    MathUtil.Deg2Rad,
                NoseUpRad =
                    MathF.Max(0f, noseUpDeg) *
                    MathUtil.Deg2Rad,
                SpeedRadS =
                    MathF.Max(0f, rateDegS) *
                    MathUtil.Deg2Rad,
                CompressionM =
                    MathF.Max(0f, compressionM),
                DroopM =
                    MathF.Max(0f, droopM)
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
        public float reloadS;
        public int count;
        public bool guided;
    }

    [Serializable] public sealed class VehicleVisual
    {
        public string baseColor;
        public string @base;
        public string scheme;
        public string weather;
        public string[] patches;
        public float camoScale = 0.34f;
        public string marking;
        public string number;
        public float trackWidthM;

#if !COT_STANDALONE_SERVER
        public Color Color => ColorUtility.TryParseHtmlString(@base, out Color color)
            ? color : new Color(0.28f, 0.32f, 0.24f);
#endif
    }

    [Serializable] public sealed class VehicleArmor
    {
        public bool turretless;
        public CatalogPoint turretPivot;
        public CatalogPoint gunPivot;
        public VehicleGunBarrel gunBarrel;
        public ArmorPlateDefinition[] hullPlates;
        public ArmorPlateDefinition[] turretPlates;
        public ArmorModuleDefinition[] modules;
        public ArmorCrewDefinition[] crew;
        public float boundingRadiusM;
    }

    [Serializable] public sealed class VehicleGunBarrel
    {
        public float lengthM;
        public float radiusM;
    }

    [Serializable] public sealed class ArmorModuleDefinition
    {
        public string module;
        public bool turretLocal;
        public bool external;
        public string visualForm;
        public float[] min;
        public float[] max;
        public ArmorVolumeShapeDefinition[] shapes;
        public ArmorModulePartDefinition[] parts;
    }

    [Serializable] public sealed class ArmorCrewDefinition
    {
        public string crew;
        public bool turretLocal;
        public float[] min;
        public float[] max;
        public ArmorVolumeShapeDefinition[] shapes;
    }

    [Serializable] public sealed class ArmorVolumeShapeDefinition
    {
        public string kind;
        public float[] center;
        public float[] radii;
        public float[] a;
        public float[] b;
        public float radius;
        public int axis;
        public float halfLength;
    }

    [Serializable] public sealed class ArmorModulePartDefinition
    {
        public float[] min;
        public float[] max;
    }

    [Serializable] public sealed class CatalogPoint
    {
        public float x;
        public float y;
        public float z;
#if !COT_STANDALONE_SERVER
        public Vector3 ToVector3() { return new Vector3(x, y, z); }
#endif
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
#if !COT_STANDALONE_SERVER
        public Color ToColor(float alpha = 1f) { return new Color(r, g, b, alpha); }
#endif
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
