using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using ClaudeOfTanks.WebRTC;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public enum PrivateRoomUiState
    {
        Idle,
        Connecting,
        Lobby,
        Starting,
        Error
    }

    public enum PrivateRoomRole
    {
        None,
        Host,
        Client
    }

    public sealed partial class PrivateRoomCoordinator : MonoBehaviour
    {
        private readonly HashSet<string> _vehicleIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _mapIds =
            new HashSet<string>(StringComparer.Ordinal);
        private RoomSignalingClient _signaling;
        private PrivateRoomHostRtcSession _hostRtc;
        private PrivateRoomClientRtcSession _clientRtc;
        private PrivateRoomHostLobbyRuntime _hostLobby;
        private PrivateRoomClientLobbyRuntime _clientLobby;
        private Task<SignalingRoomInfo> _acquireTask;
        private string _displayName, _desiredVehicleId;
        private string[] _desiredEquipment = Array.Empty<string>();
        private string _desiredCamoId = "factory";
        private string _playerId, _configuredPlayerId;

        public PrivateRoomUiState State { get; private set; } = PrivateRoomUiState.Idle;
        public PrivateRoomRole Role { get; private set; } = PrivateRoomRole.None;
        public RoomStateSnapshot LobbyState { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public string LocalPlayerId => _playerId;
        public bool IsHost => Role == PrivateRoomRole.Host;
        public bool IsBusy => State == PrivateRoomUiState.Connecting;
        public bool IsInLobby => State == PrivateRoomUiState.Lobby ||
            State == PrivateRoomUiState.Starting;

        public event Action Changed;
        public event Action MatchHandoffReady;

        public void ConfigurePlayerId(string playerId)
        {
            if (State != PrivateRoomUiState.Idle)
                throw new InvalidOperationException("Room session is active.");
            _configuredPlayerId = string.IsNullOrWhiteSpace(playerId)
                ? null
                : playerId.Trim();
        }

        public void ConfigureContent(
            IEnumerable<string> vehicleIds,
            IEnumerable<string> mapIds)
        {
            _vehicleIds.Clear();
            _mapIds.Clear();
            if (vehicleIds != null)
            {
                foreach (string id in vehicleIds)
                    if (!string.IsNullOrEmpty(id)) _vehicleIds.Add(id);
            }
            if (mapIds != null)
            {
                foreach (string id in mapIds)
                    if (!string.IsNullOrEmpty(id)) _mapIds.Add(id);
            }
        }

        public bool BeginCreate(
            string endpoint,
            string displayName,
            string vehicleId,
            string mapId,
            GameModeId gameMode,
            string playerId = null,
            string[] equipment = null,
            string camoId = null)
        {
            if (!BeginAcquire(
                endpoint,
                displayName,
                vehicleId,
                playerId,
                PrivateRoomRole.Host))
            {
                return false;
            }

            _acquireTask = _signaling.CreateRoomAsync(
                _playerId,
                _displayName,
                AuthoritativeRoom.MaximumPlayers);
            _pendingMapId = mapId;
            _pendingMode = gameMode;
            SetDesiredLoadout(equipment, camoId);
            return true;
        }

        public bool BeginJoin(
            string endpoint,
            string roomCode,
            string displayName,
            string vehicleId,
            string playerId = null,
            string[] equipment = null,
            string camoId = null)
        {
            if (!BeginAcquire(
                endpoint,
                displayName,
                vehicleId,
                playerId,
                PrivateRoomRole.Client))
            {
                return false;
            }

            try
            {
                _acquireTask = _signaling.JoinRoomAsync(
                    roomCode,
                    _playerId,
                    _displayName);
                SetDesiredLoadout(equipment, camoId);
                return true;
            }
            catch (Exception error)
            {
                Fail("join_failed", error);
                return false;
            }
        }

        public bool SelectVehicle(string vehicleId)
        {
            if (!IsInLobby || string.IsNullOrEmpty(vehicleId)) return false;
            _desiredVehicleId = vehicleId;
            return Submit(new LobbyCommand
            {
                Kind = LobbyCommandKind.SelectVehicle,
                Text = vehicleId
            });
        }

        public bool SelectMap(string mapId)
        {
            if (!IsHost || !IsInLobby || string.IsNullOrEmpty(mapId))
                return false;
            return Submit(new LobbyCommand
            {
                Kind = LobbyCommandKind.SetMap,
                Text = mapId
            });
        }

        public bool SelectEquipment(params string[] equipment)
        {
            _desiredEquipment = equipment == null
                ? Array.Empty<string>()
                : (string[])equipment.Clone();
            if (!IsInLobby) return false;
            return Submit(new LobbyCommand
            {
                Kind = LobbyCommandKind.SelectEquipment,
                Equipment = (string[])_desiredEquipment.Clone()
            });
        }

        public bool SelectCamo(string camoId)
        {
            _desiredCamoId = string.IsNullOrEmpty(camoId)
                ? "factory"
                : camoId;
            if (!IsInLobby) return false;
            return Submit(new LobbyCommand
            {
                Kind = LobbyCommandKind.SelectCamo,
                Text = _desiredCamoId
            });
        }

        public bool SelectMode(GameModeId mode)
        {
            if (!IsHost || !IsInLobby) return false;
            return Submit(new LobbyCommand
            {
                Kind = LobbyCommandKind.SetGameMode,
                GameMode = mode
            });
        }

        public bool SetReady(bool ready)
        {
            if (!IsInLobby) return false;
            return Submit(new LobbyCommand
            {
                Kind = LobbyCommandKind.SetReady,
                BoolValue = ready
            });
        }

        public bool StartMatch(uint seed)
        {
            if (!IsHost || State != PrivateRoomUiState.Lobby) return false;
            RoomMatchPlan plan = _hostLobby.SubmitHostCommand(
                new LobbyCommand
                {
                    Kind = LobbyCommandKind.Start,
                    MatchSeed = seed
                });
            return plan != null;
        }

        public void Leave()
        {
            _acquireTask = null;
            DisposeSession();
            Role = PrivateRoomRole.None;
            State = PrivateRoomUiState.Idle;
            LobbyState = null;
            ErrorCode = null;
            ErrorMessage = null;
            Changed?.Invoke();
        }

        private string _pendingMapId;
        private GameModeId _pendingMode;

        private void Update()
        {
            CompleteAcquire();
            if (_hostLobby != null)
            {
                try { _hostLobby.Pump(); }
                catch (Exception error) { Fail("lobby_pump_failed", error); }
            }
            else if (_clientLobby != null)
            {
                try { _clientLobby.Pump(); }
                catch (Exception error) { Fail("lobby_pump_failed", error); }
            }
        }

        private bool BeginAcquire(
            string endpoint,
            string displayName,
            string vehicleId,
            string playerId,
            PrivateRoomRole role)
        {
            if (State != PrivateRoomUiState.Idle &&
                State != PrivateRoomUiState.Error)
            {
                return false;
            }
            Uri uri;
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out uri) ||
                (uri.Scheme != "ws" && uri.Scheme != "wss"))
            {
                SetError("invalid_endpoint", "Use a ws:// or wss:// signaling address.");
                return false;
            }
            if (!_vehicleIds.Contains(vehicleId))
            {
                SetError("invalid_vehicle", "Select an available vehicle.");
                return false;
            }

            DisposeSession();
            _displayName = string.IsNullOrWhiteSpace(displayName)
                ? "Commander"
                : displayName.Trim();
            _desiredVehicleId = vehicleId;
            _playerId = string.IsNullOrEmpty(playerId)
                ? (_configuredPlayerId ??
                    PrivateRoomConnectionDefaults.LoadOrCreatePlayerId())
                : playerId;
            Role = role;
            State = PrivateRoomUiState.Connecting;
            LobbyState = null;
            ErrorCode = null;
            ErrorMessage = null;
            _signaling = new RoomSignalingClient(
                uri,
                new RoomSignalingClientOptions
                {
                    Origin = PrivateRoomConnectionDefaults.OriginFor(uri)
                });
            Changed?.Invoke();
            return true;
        }

        private void CompleteAcquire()
        {
            Task<SignalingRoomInfo> task = _acquireTask;
            if (task == null || !task.IsCompleted) return;
            _acquireTask = null;
            if (task.IsFaulted || task.IsCanceled)
            {
                Exception error = task.Exception?.GetBaseException() ??
                    new InvalidOperationException("Room request was canceled.");
                Fail("room_acquire_failed", error);
                return;
            }

            try
            {
                if (Role == PrivateRoomRole.Host)
                    CompleteHost(task.Result);
                else
                    CompleteClient(task.Result);
            }
            catch (Exception error)
            {
                Fail("room_setup_failed", error);
            }
        }

        private void CompleteHost(SignalingRoomInfo roomInfo)
        {
            _hostRtc = new PrivateRoomHostRtcSession(
                _signaling,
                roomInfo,
                this);
            AuthoritativeRoom room = new AuthoritativeRoom(
                roomInfo.RoomCode,
                _playerId,
                _displayName,
                _desiredVehicleId,
                teamSize: AuthoritativeRoom.MaximumTeamSize,
                maximumPlayers: AuthoritativeRoom.MaximumPlayers,
                gameMode: _pendingMode,
                mapId: _pendingMapId,
                vehicleAllowed: IsVehicleAllowed,
                mapAllowed: IsMapAllowed);
            room.SelectEquipment(_playerId, _desiredEquipment);
            room.SelectCamo(_playerId, _desiredCamoId);
            _hostLobby = new PrivateRoomHostLobbyRuntime(_hostRtc, room);
            _hostLobby.StateChanged += OnLobbyState;
            _hostLobby.MatchStarting += OnMatchStarting;
            _hostLobby.HandoffReady += OnHandoffReady;
            _hostLobby.PeerError += OnPeerError;
            State = PrivateRoomUiState.Lobby;
            OnLobbyState(_hostLobby.State);
        }

        private void CompleteClient(SignalingRoomInfo roomInfo)
        {
            _clientRtc = new PrivateRoomClientRtcSession(
                _signaling,
                roomInfo,
                this);
            _clientLobby = new PrivateRoomClientLobbyRuntime(_clientRtc);
            _clientLobby.StateChanged += OnLobbyState;
            _clientLobby.MatchStarting += OnMatchStarting;
            _clientLobby.ErrorReceived += OnPeerError;
            State = PrivateRoomUiState.Lobby;
            Changed?.Invoke();
        }

        private bool Submit(LobbyCommand command)
        {
            if (_hostLobby != null)
                return _hostLobby.SubmitHostCommand(command) != null ||
                    command.Kind != LobbyCommandKind.Start;
            return _clientLobby != null && _clientLobby.Submit(command);
        }

        private void OnLobbyState(RoomStateSnapshot state)
        {
            LobbyState = state;
            if (_clientLobby != null)
            {
                RoomPlayerSnapshot local = FindPlayer(state, _playerId);
                if (local != null &&
                    !local.Ready)
                {
                    if (local.VehicleSpecId != _desiredVehicleId)
                        SelectVehicle(_desiredVehicleId);
                    if (!SameEquipment(
                            local.Equipment,
                            _desiredEquipment))
                    {
                        SelectEquipment(_desiredEquipment);
                    }
                    if (local.CamoId != _desiredCamoId)
                        SelectCamo(_desiredCamoId);
                }
            }
            Changed?.Invoke();
        }

        private void OnMatchStarting(RoomMatchPlan plan)
        {
            State = PrivateRoomUiState.Starting;
            Changed?.Invoke();
            if (_clientLobby != null) MatchHandoffReady?.Invoke();
        }

        private void OnHandoffReady()
        {
            State = PrivateRoomUiState.Starting;
            Changed?.Invoke();
            MatchHandoffReady?.Invoke();
        }

        private void OnPeerError(string code, string message)
        {
            ErrorCode = code;
            ErrorMessage = message;
            Changed?.Invoke();
        }

        private void Fail(string fallbackCode, Exception error)
        {
            RoomSignalingException signaling = error as RoomSignalingException;
            SetError(
                signaling?.Code ?? fallbackCode,
                error?.Message ?? "Private room operation failed.");
            DisposeSession();
        }

        private void SetError(string code, string message)
        {
            State = PrivateRoomUiState.Error;
            ErrorCode = code;
            ErrorMessage = message;
            Changed?.Invoke();
        }

        private void DisposeSession()
        {
            if (_hostLobby != null)
            {
                UnsubscribeHost();
                _hostLobby.Dispose();
            }
            else
            {
                _hostRtc?.Dispose();
            }
            if (_clientLobby != null)
            {
                UnsubscribeClient();
                _clientLobby.Dispose();
            }
            else
            {
                _clientRtc?.Dispose();
            }
            if (_hostRtc == null && _clientRtc == null)
                _signaling?.Dispose();
            _hostLobby = null;
            _clientLobby = null;
            _hostRtc = null;
            _clientRtc = null;
            _signaling = null;
        }

        private void UnsubscribeHost()
        {
            if (_hostLobby == null) return;
            _hostLobby.StateChanged -= OnLobbyState;
            _hostLobby.MatchStarting -= OnMatchStarting;
            _hostLobby.HandoffReady -= OnHandoffReady;
            _hostLobby.PeerError -= OnPeerError;
        }

        private void UnsubscribeClient()
        {
            if (_clientLobby == null) return;
            _clientLobby.StateChanged -= OnLobbyState;
            _clientLobby.MatchStarting -= OnMatchStarting;
            _clientLobby.ErrorReceived -= OnPeerError;
        }

        private bool IsVehicleAllowed(string id)
        {
            return _vehicleIds.Contains(id);
        }

        private bool IsMapAllowed(string id)
        {
            return _mapIds.Contains(id);
        }

        private void SetDesiredLoadout(
            string[] equipment,
            string camoId)
        {
            _desiredEquipment = equipment == null
                ? Array.Empty<string>()
                : (string[])equipment.Clone();
            _desiredCamoId = string.IsNullOrEmpty(camoId)
                ? "factory"
                : camoId;
        }

        private static bool SameEquipment(
            string[] left,
            string[] right)
        {
            left = left ?? Array.Empty<string>();
            right = right ?? Array.Empty<string>();
            if (left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i]) return false;
            }
            return true;
        }

        private static RoomPlayerSnapshot FindPlayer(
            RoomStateSnapshot state,
            string playerId)
        {
            if (state?.Players == null) return null;
            for (int i = 0; i < state.Players.Length; i++)
                if (state.Players[i].PlayerId == playerId) return state.Players[i];
            return null;
        }

        private void OnDestroy()
        {
            DisposeSession();
            Changed = null;
            MatchHandoffReady = null;
        }
    }
}
