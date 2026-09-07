using System;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed class ContentCatalog
    {
        private const string ResourcePath = "Generated/content-catalog";
        private readonly CatalogData _data;

        private ContentCatalog(CatalogData data)
        {
            _data = data;
        }

        public int SavedVehicleCount => _data.counts.savedVehicles;
        public int ReleaseVehicleCount => _data.counts.releaseVehicles;
        public int ProductionVehicleCount => _data.counts.productionVehicles;
        public int MapCount => _data.counts.maps;

        public static ContentCatalog Load()
        {
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                throw new InvalidOperationException("Generated content catalog is missing.");
            }

            CatalogData data = JsonUtility.FromJson<CatalogData>(asset.text);
            if (data == null || data.schemaVersion != 1)
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

        [Serializable]
        private sealed class CatalogData
        {
            public int schemaVersion;
            public CatalogCounts counts;
            public VehicleRecord[] vehicles;
            public MapRecord[] maps;
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
        private sealed class VehicleRecord
        {
            public string id;
        }

        [Serializable]
        private sealed class MapRecord
        {
            public string id;
        }
    }
}
