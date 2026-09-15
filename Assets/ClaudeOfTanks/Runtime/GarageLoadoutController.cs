using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public interface IGarageLoadoutStore
    {
        string[] LoadEquipment(string vehicleId);
        string LoadCamouflage(string vehicleId);
        void SaveEquipment(string vehicleId, string[] equipment);
        void SaveCamouflage(string vehicleId, string camouflageId);
    }

    public sealed class GarageLoadoutController
    {
        private readonly ContentCatalog _catalog;
        private readonly IGarageLoadoutStore _store;
        private VehicleDefinition _vehicle;
        private string[] _equipment = Array.Empty<string>();
        private string _camouflageId = "factory";

        public GarageLoadoutController(
            ContentCatalog catalog,
            IGarageLoadoutStore store = null)
        {
            _catalog = catalog ??
                throw new ArgumentNullException(nameof(catalog));
            _store = store ?? new PlayerPrefsGarageLoadoutStore();
        }

        public string VehicleId => _vehicle?.id;
        public string[] Equipment => (string[])_equipment.Clone();
        public string CamouflageId => _camouflageId;
        public int EquipmentSlots => 3;

        public event Action Changed;

        public void SelectVehicle(string vehicleId)
        {
            VehicleDefinition vehicle = _catalog.GetVehicle(vehicleId);
            if (_vehicle != null && _vehicle.id == vehicle.id) return;
            _vehicle = vehicle;
            _equipment = Sanitize(_store.LoadEquipment(vehicle.id));
            _camouflageId = SanitizeCamouflage(
                _store.LoadCamouflage(vehicle.id));
            Changed?.Invoke();
        }

        public bool SetEquipment(string equipmentId, bool equipped)
        {
            RequireVehicle();
            if (!IsEligible(equipmentId)) return false;
            List<string> next = new List<string>(_equipment);
            bool contains = next.Contains(equipmentId);
            if (equipped == contains) return true;
            if (equipped)
            {
                if (next.Count >= EquipmentSlots) return false;
                next.Add(equipmentId);
            }
            else
            {
                next.Remove(equipmentId);
            }
            SetEquipment(next.ToArray(), true);
            return true;
        }

        public bool SetCamouflage(string camouflageId)
        {
            RequireVehicle();
            string clean = SanitizeCamouflage(camouflageId);
            if (!string.Equals(
                    clean,
                    camouflageId,
                    StringComparison.Ordinal))
            {
                return false;
            }
            if (_camouflageId == clean) return true;
            _camouflageId = clean;
            _store.SaveCamouflage(_vehicle.id, clean);
            Changed?.Invoke();
            return true;
        }

        public bool IsEquipped(string equipmentId)
        {
            return Array.IndexOf(_equipment, equipmentId) >= 0;
        }

        public bool IsEligible(string equipmentId)
        {
            if (_vehicle == null) return false;
            EquipmentDefinition item;
            try
            {
                item = _catalog.GetEquipment(equipmentId);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return _vehicle.AllowsEquipment(item.id);
        }

        public void ApplyAuthoritative(
            string vehicleId,
            string[] equipment,
            string camouflageId)
        {
            VehicleDefinition vehicle =
                _catalog.GetVehicle(vehicleId);
            bool vehicleChanged =
                _vehicle == null || _vehicle.id != vehicle.id;
            _vehicle = vehicle;
            string[] cleanEquipment = Sanitize(equipment);
            string cleanCamouflage =
                SanitizeCamouflage(camouflageId);
            bool changed = vehicleChanged ||
                !Same(_equipment, cleanEquipment) ||
                _camouflageId != cleanCamouflage;
            if (!changed) return;
            _equipment = cleanEquipment;
            _camouflageId = cleanCamouflage;
            _store.SaveEquipment(_vehicle.id, _equipment);
            _store.SaveCamouflage(_vehicle.id, _camouflageId);
            Changed?.Invoke();
        }

        public void Reset()
        {
            RequireVehicle();
            _equipment = Array.Empty<string>();
            _camouflageId = "factory";
            _store.SaveEquipment(_vehicle.id, _equipment);
            _store.SaveCamouflage(_vehicle.id, _camouflageId);
            Changed?.Invoke();
        }

        private void SetEquipment(string[] equipment, bool publish)
        {
            _equipment = Sanitize(equipment);
            _store.SaveEquipment(_vehicle.id, _equipment);
            if (publish) Changed?.Invoke();
        }

        private string[] Sanitize(IEnumerable<string> source)
        {
            List<string> result = new List<string>(EquipmentSlots);
            if (source == null) return result.ToArray();
            foreach (string id in source)
            {
                if (result.Contains(id) || !IsEligible(id)) continue;
                result.Add(id);
                if (result.Count == EquipmentSlots) break;
            }
            return result.ToArray();
        }

        private string SanitizeCamouflage(string value)
        {
            if (value == "custom")
                return value;
            return !string.IsNullOrEmpty(value) &&
                _catalog.ContainsCamouflage(value)
                    ? value
                    : "factory";
        }

        private void RequireVehicle()
        {
            if (_vehicle == null)
                throw new InvalidOperationException(
                    "Select a garage vehicle before editing its loadout.");
        }

        private static bool Same(
            string[] left,
            string[] right)
        {
            if (left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i]) return false;
            }
            return true;
        }
    }

    public sealed class PlayerPrefsGarageLoadoutStore :
        IGarageLoadoutStore
    {
        private const string EquipmentPrefix = "cot.equip.";
        private const string CamouflagePrefix = "cot.camo.";

        [Serializable]
        private sealed class EquipmentRecord
        {
            public string[] ids;
        }

        public string[] LoadEquipment(string vehicleId)
        {
            string json = PlayerPrefs.GetString(
                EquipmentPrefix + vehicleId,
                string.Empty);
            if (string.IsNullOrEmpty(json)) return Array.Empty<string>();
            try
            {
                EquipmentRecord record =
                    JsonUtility.FromJson<EquipmentRecord>(json);
                return record?.ids ?? Array.Empty<string>();
            }
            catch (ArgumentException)
            {
                return Array.Empty<string>();
            }
        }

        public string LoadCamouflage(string vehicleId)
        {
            return PlayerPrefs.GetString(
                CamouflagePrefix + vehicleId,
                "factory");
        }

        public void SaveEquipment(
            string vehicleId,
            string[] equipment)
        {
            string key = EquipmentPrefix + vehicleId;
            if (equipment == null || equipment.Length == 0)
                PlayerPrefs.DeleteKey(key);
            else
                PlayerPrefs.SetString(
                    key,
                    JsonUtility.ToJson(new EquipmentRecord
                    {
                        ids = (string[])equipment.Clone()
                    }));
            PlayerPrefs.Save();
        }

        public void SaveCamouflage(
            string vehicleId,
            string camouflageId)
        {
            PlayerPrefs.SetString(
                CamouflagePrefix + vehicleId,
                camouflageId);
            PlayerPrefs.Save();
        }
    }
}
