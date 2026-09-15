using ClaudeOfTanks.Network;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class GameFlowController
    {
        private GarageLoadoutController _loadout;
        private GarageLoadoutPanel _loadoutPanel;
        private bool _applyingRoomLoadout;

        public GarageLoadoutController Loadout => _loadout;
        public GarageLoadoutPanel LoadoutPanel => _loadoutPanel;
        public string[] SelectedEquipment =>
            _loadout?.Equipment ?? System.Array.Empty<string>();
        public string SelectedCamouflageId =>
            _loadout?.CamouflageId ?? "factory";
        private string SelectedNetworkCamouflageId =>
            SelectedCamouflageId == "custom"
                ? "factory"
                : SelectedCamouflageId;

        private void InitializeLoadout()
        {
            _loadout = new GarageLoadoutController(_catalog);
            _loadout.Changed += OnLoadoutChanged;
            _loadout.SelectVehicle(
                _catalog.ProductionVehicleIds[0]);
        }

        private void BuildLoadoutUi(Transform parent)
        {
            Button loadout =
                _garagePresentation.CreateActionButton(
                "Loadout",
                "LOADOUT");
            _loadoutPanel = GarageLoadoutPanel.Create(
                parent,
                _catalog,
                _loadout);
            loadout.onClick.AddListener(_loadoutPanel.Open);
        }

        private void BuildGarageModeActions(
            Transform parent)
        {
            Button privateRoom =
                _garagePresentation.CreateActionButton(
                    "PrivateRoom",
                    "PRIVATE ROOM");
            _privateRoomPanel = PrivateRoomPanel.Create(
                parent,
                _privateRoom,
                () => SelectedVehicleId,
                () => SelectedMapId,
                () => SelectedMode,
                () => SelectedEquipment,
                () => SelectedNetworkCamouflageId);
            privateRoom.onClick.AddListener(
                _privateRoomPanel.Open);

            Button ranked =
                _garagePresentation.CreateActionButton(
                    "Ranked",
                    "RANKED");
            _rankedPanel = RankedPanel.Create(
                parent,
                _ranked,
                () => SelectedVehicleId,
                () => SelectedEquipment,
                () => SelectedNetworkCamouflageId);
            ranked.onClick.AddListener(
                _rankedPanel.Open);
            BuildLoadoutUi(parent);
        }

        private void OnGarageVehicleChanged(int index)
        {
            _loadout.SelectVehicle(
                _catalog.ProductionVehicleIds[index]);
            _privateRoom.SelectVehicle(SelectedVehicleId);
        }

        private void OnGarageMapChanged()
        {
            RefreshPreview(_vehicle.value);
            _privateRoom.SelectMap(SelectedMapId);
        }

        private void OnGarageModeChanged()
        {
            RefreshGarageStatus();
            _privateRoom.SelectMode(SelectedMode);
        }

        private void OnLoadoutChanged()
        {
            if (_garage != null && _vehicle != null)
                RefreshPreview(_vehicle.value);
            if (_applyingRoomLoadout ||
                _privateRoom == null ||
                !_privateRoom.IsInLobby)
            {
                return;
            }
            _privateRoom.SelectEquipment(
                _loadout.Equipment);
            _privateRoom.SelectCamo(
                SelectedNetworkCamouflageId);
        }

        private void ApplyRoomLoadout(RoomPlayerSnapshot local)
        {
            if (local == null ||
                string.IsNullOrEmpty(local.VehicleSpecId))
            {
                return;
            }
            _applyingRoomLoadout = true;
            try
            {
                _loadout.ApplyAuthoritative(
                    local.VehicleSpecId,
                    local.Equipment,
                    local.CamoId);
            }
            finally
            {
                _applyingRoomLoadout = false;
            }
        }

        private void RefreshDefaultGarageSelection()
        {
            string firstVehicle =
                _catalog.ProductionVehicleIds[0];
            bool changed =
                _loadout.VehicleId != firstVehicle;
            _loadout.SelectVehicle(firstVehicle);
            if (!changed) RefreshPreview(0);
        }

        private void SyncLoadoutFromRoom(
            RoomPlayerSnapshot local,
            int vehicleIndex)
        {
            _vehicle.SetValueWithoutNotify(vehicleIndex);
            bool changed =
                _loadout.VehicleId != local.VehicleSpecId ||
                !SameSelection(
                    _loadout.Equipment,
                    local.Equipment) ||
                _loadout.CamouflageId != local.CamoId;
            ApplyRoomLoadout(local);
            if (!changed) RefreshPreview(vehicleIndex);
        }

        private void DisposeLoadout()
        {
            if (_loadout != null)
                _loadout.Changed -= OnLoadoutChanged;
        }

        private static bool SameSelection(
            string[] left,
            string[] right)
        {
            left = left ?? System.Array.Empty<string>();
            right = right ?? System.Array.Empty<string>();
            if (left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i]) return false;
            }
            return true;
        }
    }
}
