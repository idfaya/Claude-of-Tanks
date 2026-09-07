using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static ClaudeOfTanks.Runtime.RankedPanelUi;

namespace ClaudeOfTanks.Runtime
{
    public sealed class RankedPanel : MonoBehaviour
    {
        private const string CommanderNameKey =
            "cot.ranked.commanderName";
        private RankedCoordinator _coordinator;
        private Func<string> _vehicleId;
        private Func<string[]> _equipment;
        private Func<string> _camouflageId;
        private GameObject _surface;
        private InputField _endpoint;
        private InputField _displayName;
        private Dropdown _teamSize;
        private Text _status;
        private Text _profile;
        private Text _leaderboard;
        private Button _queue;
        private Button _cancel;
        private bool _subscribed;

        public bool IsVisible =>
            _surface != null && _surface.activeSelf;
        public string StatusText =>
            _status != null ? _status.text : string.Empty;
        public string ProfileText =>
            _profile != null ? _profile.text : string.Empty;

        public static RankedPanel Create(
            Transform parent,
            RankedCoordinator coordinator,
            Func<string> vehicleId,
            Func<string[]> equipment = null,
            Func<string> camouflageId = null)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            GameObject root = new GameObject(
                "Ranked",
                typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch(root.GetComponent<RectTransform>());
            RankedPanel panel = root.AddComponent<RankedPanel>();
            panel._coordinator = coordinator ??
                throw new ArgumentNullException(nameof(coordinator));
            panel._vehicleId = vehicleId ??
                throw new ArgumentNullException(nameof(vehicleId));
            panel._equipment = equipment ??
                (() => Array.Empty<string>());
            panel._camouflageId = camouflageId ??
                (() => "factory");
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
            _coordinator.RefreshProfile(_endpoint.text);
        }

        public void Close()
        {
            if (_surface != null) _surface.SetActive(false);
        }

        public bool FindMatch()
        {
            string name = _displayName.text.Trim();
            PlayerPrefs.SetString(CommanderNameKey, name);
            PlayerPrefs.Save();
            return _coordinator.BeginQueue(
                _endpoint.text,
                name,
                _vehicleId(),
                _equipment(),
                _camouflageId(),
                TeamSize());
        }

        public void Cancel()
        {
            _coordinator.CancelQueue();
        }

        public void SetConnectionFields(
            string endpoint,
            string displayName,
            int teamSize)
        {
            _endpoint.text = endpoint ?? string.Empty;
            _displayName.text = displayName ?? string.Empty;
            int index = Array.IndexOf(
                new[] { 1, 2, 3, 5, 7 },
                teamSize);
            _teamSize.SetValueWithoutNotify(
                index >= 0 ? index : 0);
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
            Button shadeButton =
                shade.gameObject.AddComponent<Button>();
            shadeButton.transition = Selectable.Transition.None;
            shadeButton.onClick.AddListener(Close);
            _surface = shade.gameObject;

            Image rail = Image(
                "Rail",
                shade.transform,
                new Color(0.045f, 0.055f, 0.052f, 0.98f));
            rail.gameObject.AddComponent<Button>().transition =
                Selectable.Transition.None;
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
            title.text = "RANKED";
            Place(title.rectTransform, 28f, 340f, -58f, -18f);
            Button close = Button(
                "Close",
                rail.transform,
                font,
                "CLOSE");
            Place(
                close.GetComponent<RectTransform>(),
                374f,
                472f,
                -58f,
                -18f);
            close.onClick.AddListener(Close);

            Text endpointLabel = Caption(
                "EndpointLabel",
                rail.transform,
                font,
                "MATCH SERVICE");
            Place(
                endpointLabel.rectTransform,
                28f,
                472f,
                -102f,
                -78f);
            _endpoint = Input(
                "Endpoint",
                rail.transform,
                font,
                RankedCoordinator.DefaultServiceEndpoint());
            Place(
                _endpoint.GetComponent<RectTransform>(),
                28f,
                472f,
                -142f,
                -104f);

            Text nameLabel = Caption(
                "NameLabel",
                rail.transform,
                font,
                "COMMANDER");
            Place(
                nameLabel.rectTransform,
                28f,
                330f,
                -184f,
                -160f);
            _displayName = Input(
                "DisplayName",
                rail.transform,
                font,
                PlayerPrefs.GetString(
                    CommanderNameKey,
                    "Commander"));
            _displayName.characterLimit = 24;
            Place(
                _displayName.GetComponent<RectTransform>(),
                28f,
                330f,
                -224f,
                -186f);

            Text formatLabel = Caption(
                "FormatLabel",
                rail.transform,
                font,
                "FORMAT");
            Place(
                formatLabel.rectTransform,
                346f,
                472f,
                -184f,
                -160f);
            _teamSize = Dropdown(
                "TeamSize",
                rail.transform,
                font);
            _teamSize.AddOptions(new List<string>
            {
                "1 v 1",
                "2 v 2",
                "3 v 3",
                "5 v 5",
                "7 v 7"
            });
            Place(
                _teamSize.GetComponent<RectTransform>(),
                346f,
                472f,
                -224f,
                -186f);

            _queue = Button(
                "FindMatch",
                rail.transform,
                font,
                "FIND MATCH");
            Place(
                _queue.GetComponent<RectTransform>(),
                28f,
                242f,
                -282f,
                -238f);
            _queue.onClick.AddListener(() => FindMatch());
            _cancel = Button(
                "Cancel",
                rail.transform,
                font,
                "CANCEL");
            Place(
                _cancel.GetComponent<RectTransform>(),
                258f,
                472f,
                -282f,
                -238f);
            _cancel.onClick.AddListener(Cancel);

            _status = Label(
                "Status",
                rail.transform,
                font,
                14,
                TextAnchor.UpperLeft);
            _status.color = new Color(0.52f, 0.78f, 0.66f);
            Place(
                _status.rectTransform,
                28f,
                472f,
                -348f,
                -306f);
            _profile = Label(
                "Profile",
                rail.transform,
                font,
                15,
                TextAnchor.UpperLeft);
            Place(
                _profile.rectTransform,
                28f,
                472f,
                -402f,
                -354f);
            _leaderboard = Label(
                "Leaderboard",
                rail.transform,
                font,
                14,
                TextAnchor.UpperLeft);
            _leaderboard.fontStyle = FontStyle.Normal;
            _leaderboard.verticalOverflow =
                VerticalWrapMode.Truncate;
            Place(
                _leaderboard.rectTransform,
                28f,
                472f,
                -680f,
                -414f);
            Refresh();
        }

        private void Refresh()
        {
            if (_coordinator == null || _status == null) return;
            bool active =
                _coordinator.State ==
                    RankedUiState.Connecting ||
                _coordinator.State ==
                    RankedUiState.Queued;
            _endpoint.interactable = !active;
            _displayName.interactable = !active;
            _teamSize.interactable = !active;
            _queue.interactable =
                _coordinator.State == RankedUiState.Idle ||
                _coordinator.State == RankedUiState.Error;
            _cancel.interactable = active ||
                _coordinator.State == RankedUiState.Error;
            _status.text = Status();
            RankedProfile profile = _coordinator.Profile;
            _profile.text = profile == null
                ? "UNRANKED  /  1000 ELO"
                : profile.rank.ToUpperInvariant() +
                    "  /  " + profile.rating +
                    " ELO  /  " + profile.matches +
                    " MATCHES";
            _leaderboard.text = LeaderboardText(
                _coordinator.Leaderboard);
        }

        private string Status()
        {
            switch (_coordinator.State)
            {
                case RankedUiState.Connecting:
                    return "CONNECTING TO MATCH SERVICE";
                case RankedUiState.Queued:
                    return "SEARCHING NEAR " +
                        _coordinator.Rating + " ELO";
                case RankedUiState.Starting:
                    return "MATCH FOUND";
                case RankedUiState.Battle:
                    return "MATCH IN PROGRESS";
                case RankedUiState.Finishing:
                    return "UPDATING RATING";
                case RankedUiState.Error:
                    return "ERROR  " +
                        _coordinator.ErrorMessage;
                default:
                    return "SERVER-AUTHORITATIVE MATCHMAKING";
            }
        }

        private static string LeaderboardText(
            RankedLeaderboard leaderboard)
        {
            StringBuilder text = new StringBuilder("LEADERBOARD\n");
            RankedProfile[] players =
                leaderboard?.players ?? Array.Empty<RankedProfile>();
            for (int i = 0; i < players.Length; i++)
            {
                RankedProfile player = players[i];
                text.Append('#');
                text.Append(player.place);
                text.Append("  ");
                text.Append(player.name);
                text.Append("  ");
                text.Append(player.rank);
                text.Append("  ");
                text.Append(player.rating);
                text.AppendLine();
            }
            if (players.Length == 0)
                text.Append("NO RATED MATCHES");
            return text.ToString();
        }

        private int TeamSize()
        {
            int[] values = { 1, 2, 3, 5, 7 };
            return values[Mathf.Clamp(
                _teamSize.value,
                0,
                values.Length - 1)];
        }

    }
}
