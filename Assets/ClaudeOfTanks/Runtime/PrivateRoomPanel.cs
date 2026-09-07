using System;
using System.Text;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace ClaudeOfTanks.Runtime
{
    public sealed class PrivateRoomPanel : MonoBehaviour
    {
        private PrivateRoomCoordinator _coordinator;
        private Func<string> _vehicleId;
        private Func<string> _mapId;
        private Func<GameModeId> _gameMode;
        private GameObject _surface;
        private InputField _endpoint, _displayName, _roomCode;
        private Text _status, _roster;
        private Button _create, _join, _ready, _start, _leave;
        private bool _subscribed;

        public bool IsVisible => _surface != null && _surface.activeSelf;
        public string StatusText => _status != null ? _status.text : string.Empty;
        public string RosterText => _roster != null ? _roster.text : string.Empty;
        public string RoomCode => _coordinator?.LobbyState?.RoomCode;

        public static PrivateRoomPanel Create(
            Transform parent,
            PrivateRoomCoordinator coordinator,
            Func<string> vehicleId,
            Func<string> mapId,
            Func<GameModeId> gameMode)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            GameObject root = new GameObject(
                "PrivateRoom",
                typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            Stretch(rect);
            PrivateRoomPanel panel = root.AddComponent<PrivateRoomPanel>();
            panel._coordinator = coordinator ??
                throw new ArgumentNullException(nameof(coordinator));
            panel._vehicleId = vehicleId ??
                throw new ArgumentNullException(nameof(vehicleId));
            panel._mapId = mapId ??
                throw new ArgumentNullException(nameof(mapId));
            panel._gameMode = gameMode ??
                throw new ArgumentNullException(nameof(gameMode));
            panel.Build();
            panel.Subscribe();
            panel.Close();
            return panel;
        }

        public void Open()
        {
            _surface.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
        }

        public void Close()
        {
            if (_surface != null) _surface.SetActive(false);
        }

        public bool CreateRoom()
        {
            return _coordinator.BeginCreate(
                _endpoint.text,
                _displayName.text,
                _vehicleId(),
                _mapId(),
                _gameMode());
        }

        public bool JoinRoom()
        {
            return _coordinator.BeginJoin(
                _endpoint.text,
                _roomCode.text,
                _displayName.text,
                _vehicleId());
        }

        public bool ToggleReady()
        {
            RoomPlayerSnapshot local = LocalPlayer();
            return _coordinator.SetReady(local == null || !local.Ready);
        }

        public bool StartMatch()
        {
            return _coordinator.StartMatch(
                unchecked((uint)Environment.TickCount));
        }

        public void LeaveRoom()
        {
            _coordinator.Leave();
        }

        public void SetConnectionFields(
            string endpoint,
            string displayName,
            string roomCode)
        {
            _endpoint.text = endpoint ?? string.Empty;
            _displayName.text = displayName ?? string.Empty;
            _roomCode.text = roomCode ?? string.Empty;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_subscribed || _coordinator == null) return;
            _coordinator.Changed += Refresh;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || _coordinator == null) return;
            _coordinator.Changed -= Refresh;
            _subscribed = false;
        }

        private void Build()
        {
            Font font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");
            Image shade = Image(
                "Shade",
                transform,
                new Color(0.01f, 0.015f, 0.018f, 0.68f));
            Stretch(shade.rectTransform);
            Button shadeButton = shade.gameObject.AddComponent<Button>();
            shadeButton.transition = Selectable.Transition.None;
            shadeButton.onClick.AddListener(Close);
            _surface = shade.gameObject;

            Image rail = Image(
                "Rail",
                shade.transform,
                new Color(0.045f, 0.055f, 0.052f, 0.98f));
            rail.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;
            RectTransform railRect = rail.rectTransform;
            railRect.anchorMin = new Vector2(1f, 0f);
            railRect.anchorMax = new Vector2(1f, 1f);
            railRect.pivot = new Vector2(1f, 0.5f);
            railRect.sizeDelta = new Vector2(500f, 0f);
            railRect.anchoredPosition = Vector2.zero;

            Text title = Label(
                "Title",
                rail.transform,
                font,
                24,
                TextAnchor.MiddleLeft);
            title.text = "PRIVATE ROOM";
            Place(title.rectTransform, 28f, 340f, -58f, -18f);

            Button close = Button(
                "Close",
                rail.transform,
                font,
                "CLOSE");
            Place(close.GetComponent<RectTransform>(),
                374f, 472f, -58f, -18f);
            close.onClick.AddListener(Close);

            Text endpointLabel = Caption(
                "EndpointLabel",
                rail.transform,
                font,
                "SIGNAL SERVER");
            Place(endpointLabel.rectTransform, 28f, 472f, -102f, -78f);
            _endpoint = Input(
                "Endpoint",
                rail.transform,
                font,
                PrivateRoomConnectionDefaults.SignalingEndpoint());
            Place(_endpoint.GetComponent<RectTransform>(),
                28f, 472f, -142f, -104f);

            Text nameLabel = Caption(
                "NameLabel",
                rail.transform,
                font,
                "COMMANDER");
            Place(nameLabel.rectTransform, 28f, 472f, -184f, -160f);
            _displayName = Input(
                "DisplayName",
                rail.transform,
                font,
                "Commander");
            _displayName.characterLimit = 32;
            Place(_displayName.GetComponent<RectTransform>(),
                28f, 472f, -224f, -186f);

            _create = Button(
                "Create",
                rail.transform,
                font,
                "CREATE");
            Place(_create.GetComponent<RectTransform>(),
                28f, 214f, -282f, -238f);
            _create.onClick.AddListener(() => CreateRoom());

            _roomCode = Input(
                "RoomCode",
                rail.transform,
                font,
                string.Empty);
            _roomCode.characterLimit = 8;
            _roomCode.textComponent.alignment = TextAnchor.MiddleCenter;
            Place(_roomCode.GetComponent<RectTransform>(),
                230f, 330f, -282f, -238f);
            _join = Button(
                "Join",
                rail.transform,
                font,
                "JOIN");
            Place(_join.GetComponent<RectTransform>(),
                346f, 472f, -282f, -238f);
            _join.onClick.AddListener(() => JoinRoom());

            _status = Label(
                "Status",
                rail.transform,
                font,
                14,
                TextAnchor.UpperLeft);
            _status.color = new Color(0.52f, 0.78f, 0.66f);
            Place(_status.rectTransform, 28f, 472f, -350f, -304f);

            _roster = Label(
                "Roster",
                rail.transform,
                font,
                15,
                TextAnchor.UpperLeft);
            _roster.fontStyle = FontStyle.Normal;
            _roster.horizontalOverflow = HorizontalWrapMode.Wrap;
            _roster.verticalOverflow = VerticalWrapMode.Truncate;
            Place(_roster.rectTransform, 28f, 472f, -558f, -356f);

            _ready = Button(
                "Ready",
                rail.transform,
                font,
                "READY");
            Place(_ready.GetComponent<RectTransform>(),
                28f, 242f, 24f, 68f, Vector2.zero);
            _ready.onClick.AddListener(() => ToggleReady());
            _start = Button(
                "Start",
                rail.transform,
                font,
                "START");
            Place(_start.GetComponent<RectTransform>(),
                258f, 472f, 24f, 68f, Vector2.zero);
            _start.onClick.AddListener(() => StartMatch());
            _leave = Button(
                "Leave",
                rail.transform,
                font,
                "LEAVE");
            Place(_leave.GetComponent<RectTransform>(),
                28f, 472f, 82f, 126f, Vector2.zero);
            _leave.onClick.AddListener(LeaveRoom);
            Refresh();
        }

        private void Refresh()
        {
            if (_coordinator == null || _status == null) return;
            bool acquiring = _coordinator.IsBusy;
            bool lobby = _coordinator.IsInLobby;
            _endpoint.interactable = !acquiring && !lobby;
            _displayName.interactable = !acquiring && !lobby;
            _roomCode.interactable = !acquiring && !lobby;
            _create.interactable = !acquiring && !lobby;
            _join.interactable = !acquiring && !lobby;
            _ready.gameObject.SetActive(lobby);
            _start.gameObject.SetActive(
                lobby && _coordinator.IsHost);
            _leave.gameObject.SetActive(acquiring || lobby ||
                _coordinator.State == PrivateRoomUiState.Error);

            RoomStateSnapshot state = _coordinator.LobbyState;
            _status.text = Status(state);
            _roster.text = Roster(state);
            RoomPlayerSnapshot local = LocalPlayer();
            SetButtonText(
                _ready,
                local != null && local.Ready ? "UNREADY" : "READY");
            _ready.interactable =
                state != null && state.Phase == RoomPhase.Waiting;
            _start.interactable =
                state != null &&
                state.Phase == RoomPhase.Waiting &&
                AllPlayersReady(state);
        }

        private string Status(RoomStateSnapshot state)
        {
            if (_coordinator.State == PrivateRoomUiState.Connecting)
                return "CONNECTING";
            if (_coordinator.State == PrivateRoomUiState.Error)
                return "ERROR  " + _coordinator.ErrorMessage;
            if (_coordinator.State == PrivateRoomUiState.Starting)
                return "MATCH STARTING";
            if (state == null) return "CREATE OR JOIN A ROOM";
            return "ROOM " + state.RoomCode +
                "  /  " + state.GameMode.ToString().ToUpperInvariant() +
                "  /  " + state.MapId.ToUpperInvariant();
        }

        private static string Roster(RoomStateSnapshot state)
        {
            if (state?.Players == null) return string.Empty;
            StringBuilder text = new StringBuilder();
            text.AppendLine("ROSTER");
            for (int i = 0; i < state.Players.Length; i++)
            {
                RoomPlayerSnapshot player = state.Players[i];
                text.Append(player.Ready ? "READY  " : "WAIT   ");
                text.Append(player.Team.ToString().ToUpperInvariant());
                text.Append("  ");
                text.Append(player.DisplayName);
                if (player.IsHost) text.Append("  HOST");
                text.AppendLine();
                text.Append("       ");
                text.AppendLine(
                    string.IsNullOrEmpty(player.VehicleSpecId)
                        ? "NO VEHICLE"
                        : player.VehicleSpecId.ToUpperInvariant());
            }
            return text.ToString();
        }

        private RoomPlayerSnapshot LocalPlayer()
        {
            RoomStateSnapshot state = _coordinator?.LobbyState;
            if (state?.Players == null) return null;
            for (int i = 0; i < state.Players.Length; i++)
            {
                if (state.Players[i].PlayerId ==
                    _coordinator.LocalPlayerId)
                {
                    return state.Players[i];
                }
            }
            return null;
        }

        private static bool AllPlayersReady(RoomStateSnapshot state)
        {
            bool found = false;
            for (int i = 0; i < state.Players.Length; i++)
            {
                RoomPlayerSnapshot player = state.Players[i];
                if (player.Team == RoomTeam.Spectator) continue;
                found = true;
                if (!player.Connected || !player.Ready ||
                    string.IsNullOrEmpty(player.VehicleSpecId))
                {
                    return false;
                }
            }
            return found;
        }

        private static Text Caption(
            string name,
            Transform parent,
            Font font,
            string value)
        {
            Text text = Label(
                name,
                parent,
                font,
                12,
                TextAnchor.MiddleLeft);
            text.text = value;
            text.color = new Color(0.62f, 0.69f, 0.66f);
            return text;
        }

        private static InputField Input(
            string name,
            Transform parent,
            Font font,
            string value)
        {
            GameObject root = DefaultControls.CreateInputField(
                new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            InputField input = root.GetComponent<InputField>();
            input.textComponent.font = font;
            input.text = value;
            return input;
        }

        private static Button Button(
            string name,
            Transform parent,
            Font font,
            string value)
        {
            GameObject root = DefaultControls.CreateButton(
                new DefaultControls.Resources());
            root.name = name;
            root.transform.SetParent(parent, false);
            Text text = root.GetComponentInChildren<Text>();
            text.font = font;
            text.text = value;
            return root.GetComponent<Button>();
        }

        private static Text Label(
            string name,
            Transform parent,
            Font font,
            int size,
            TextAnchor alignment)
        {
            GameObject root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            root.transform.SetParent(parent, false);
            Text text = root.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = alignment;
            return text;
        }

        private static Image Image(
            string name,
            Transform parent,
            Color color)
        {
            GameObject root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            root.transform.SetParent(parent, false);
            Image image = root.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void SetButtonText(Button button, string value)
        {
            button.GetComponentInChildren<Text>().text = value;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(
            RectTransform rect,
            float left,
            float right,
            float top,
            float bottom,
            Vector2? anchor = null)
        {
            Vector2 value = anchor ?? new Vector2(0f, 1f);
            rect.anchorMin = value;
            rect.anchorMax = value;
            rect.offsetMin = new Vector2(left, top);
            rect.offsetMax = new Vector2(right, bottom);
        }
    }
}
