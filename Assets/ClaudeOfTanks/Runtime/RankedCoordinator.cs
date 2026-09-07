using System;
using System.Threading;
using System.Threading.Tasks;
using ClaudeOfTanks.Network;
using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public enum RankedUiState
    {
        Idle,
        Connecting,
        Queued,
        Starting,
        Battle,
        Finishing,
        Error
    }

    public sealed class RankedCoordinator : MonoBehaviour
    {
        private static readonly int[] TeamSizes = { 1, 2, 3, 5, 7 };
        private IRankedIdentityStore _identityStore =
            new PlayerPrefsRankedIdentityStore();
        private RankedServiceClient _client;
        private CancellationTokenSource _lifetime;
        private RankedMatchHandoff _handoff;
        private Task _operation = Task.CompletedTask;
        private int _generation;
        private string _ticketId;
        private string _ticketToken;

        public RankedUiState State { get; private set; } =
            RankedUiState.Idle;
        public string ErrorMessage { get; private set; }
        public Exception LastException { get; private set; }
        public int Rating { get; private set; } =
            RankedRatingStore.StartingRating;
        public RankedProfile Profile { get; private set; }
        public RankedLeaderboard Leaderboard { get; private set; }
        public Task CurrentOperation => _operation;
        public bool IsBusy =>
            State == RankedUiState.Connecting ||
            State == RankedUiState.Queued ||
            State == RankedUiState.Starting ||
            State == RankedUiState.Finishing;

        public event Action Changed;
        public event Action MatchHandoffReady;

        public static string DefaultServiceEndpoint()
        {
            string environment =
                Environment.GetEnvironmentVariable("COT_MATCH_URL");
            return string.IsNullOrWhiteSpace(environment)
                ? "http://127.0.0.1:18791/"
                : environment.Trim();
        }

        public void ConfigureIdentityStore(
            IRankedIdentityStore identityStore)
        {
            if (State != RankedUiState.Idle)
                throw new InvalidOperationException(
                    "Ranked identity store can only change while idle.");
            _identityStore = identityStore ??
                throw new ArgumentNullException(nameof(identityStore));
        }

        public bool BeginQueue(
            string serviceUrl,
            string displayName,
            string vehicleSpecId,
            string[] equipment,
            string camoId,
            int teamSize)
        {
            if (State != RankedUiState.Idle &&
                State != RankedUiState.Error)
            {
                return false;
            }
            if (Array.IndexOf(TeamSizes, teamSize) < 0)
            {
                SetError("Ranked team size is invalid.");
                return false;
            }
            RankedServiceClient client;
            try
            {
                client = new RankedServiceClient(
                    serviceUrl,
                    _identityStore);
            }
            catch (Exception error)
            {
                SetError(error.Message);
                return false;
            }
            ReleaseOperation();
            _client = client;
            _lifetime = new CancellationTokenSource();
            int generation = ++_generation;
            State = RankedUiState.Connecting;
            ErrorMessage = null;
            LastException = null;
            Publish();
            _operation = QueueAsync(
                generation,
                displayName,
                vehicleSpecId,
                equipment,
                camoId,
                teamSize,
                _lifetime.Token);
            return true;
        }

        public void CancelQueue()
        {
            if (State != RankedUiState.Connecting &&
                State != RankedUiState.Queued &&
                State != RankedUiState.Error)
            {
                return;
            }
            string ticketId = _ticketId;
            string ticketToken = _ticketToken;
            RankedServiceClient client = _client;
            ++_generation;
            _lifetime?.Cancel();
            _lifetime?.Dispose();
            _lifetime = null;
            _client = null;
            _ticketId = null;
            _ticketToken = null;
            _handoff?.Dispose();
            _handoff = null;
            State = RankedUiState.Idle;
            ErrorMessage = null;
            Publish();
            _operation = CancelAndDisposeAsync(
                client,
                ticketId,
                ticketToken);
        }

        public RankedMatchHandoff TakeHandoff()
        {
            if (State != RankedUiState.Starting ||
                _handoff == null)
            {
                throw new InvalidOperationException(
                    "Ranked match handoff is unavailable.");
            }
            RankedMatchHandoff handoff = _handoff;
            _handoff = null;
            State = RankedUiState.Battle;
            Publish();
            return handoff;
        }

        public void ReturnFromBattle()
        {
            if (State != RankedUiState.Battle) return;
            State = RankedUiState.Finishing;
            Publish();
            _lifetime?.Dispose();
            _lifetime = new CancellationTokenSource();
            int generation = ++_generation;
            _operation = ReadResultAsync(
                generation,
                _lifetime.Token);
        }

        public bool RefreshProfile(string serviceUrl)
        {
            if (IsBusy || State == RankedUiState.Battle)
                return false;
            RankedServiceClient client;
            try
            {
                client = new RankedServiceClient(
                    serviceUrl,
                    _identityStore);
            }
            catch (Exception error)
            {
                SetError(error.Message);
                return false;
            }
            ReleaseOperation();
            _client = client;
            _lifetime = new CancellationTokenSource();
            int generation = ++_generation;
            _operation = RefreshProfileAsync(
                generation,
                _lifetime.Token);
            return true;
        }

        private async Task QueueAsync(
            int generation,
            string displayName,
            string vehicleSpecId,
            string[] equipment,
            string camoId,
            int teamSize,
            CancellationToken cancellationToken)
        {
            try
            {
                RankedQueueState queue = await _client.JoinAsync(
                    displayName,
                    vehicleSpecId,
                    equipment,
                    camoId,
                    teamSize,
                    cancellationToken);
                if (!Current(generation)) return;
                RequireQueueTicket(queue);
                _ticketId = queue.ticketId;
                _ticketToken = queue.ticketToken;
                Rating = queue.rating;
                State = RankedUiState.Queued;
                Publish();
                while (string.Equals(
                    queue.status,
                    "queued",
                    StringComparison.OrdinalIgnoreCase))
                {
                    await Task.Delay(500, cancellationToken);
                    queue = await _client.PollAsync(
                        _ticketId,
                        _ticketToken,
                        cancellationToken);
                    if (!Current(generation)) return;
                    Rating = queue.rating;
                    Publish();
                }
                if (!string.Equals(
                        queue.status,
                        "matched",
                        StringComparison.OrdinalIgnoreCase) ||
                    queue.match == null)
                {
                    throw new InvalidOperationException(
                        "Ranked queue ended with status " +
                        (queue.status ?? "unknown") + ".");
                }
                State = RankedUiState.Connecting;
                Publish();
                Uri socketUri = _client.MatchSocketUri;
                DedicatedSocketConnection connection =
                    await WebSocketNetworkEndpoint.ConnectDedicatedAsync(
                        socketUri,
                        new DedicatedSocketAuthRequest
                        {
                            Kind = DedicatedSocketAuthKind.Ticket,
                            MatchId = queue.match.matchId,
                            PlayerId = queue.match.playerId,
                            Token = queue.match.token
                        },
                        cancellationToken: cancellationToken);
                if (!Current(generation))
                {
                    connection.Dispose();
                    return;
                }
                _handoff = new RankedMatchHandoff(
                    socketUri,
                    queue.match,
                    connection);
                State = RankedUiState.Starting;
                Publish();
                MatchHandoffReady?.Invoke();
            }
            catch (OperationCanceledException)
            {
                if (Current(generation))
                {
                    State = RankedUiState.Idle;
                    Publish();
                }
            }
            catch (Exception error)
            {
                if (Current(generation)) SetError(error);
            }
        }

        private async Task ReadResultAsync(
            int generation,
            CancellationToken cancellationToken)
        {
            try
            {
                for (int i = 0; i < 20; i++)
                {
                    RankedQueueState queue =
                        await _client.PollAsync(
                            _ticketId,
                            _ticketToken,
                            cancellationToken);
                    if (!Current(generation)) return;
                    if (string.Equals(
                            queue.status,
                            "finished",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        Profile = queue.profile;
                        if (Profile != null) Rating = Profile.rating;
                        break;
                    }
                    await Task.Delay(500, cancellationToken);
                }
                if (!Current(generation)) return;
                State = RankedUiState.Idle;
                ReleaseClient();
                Publish();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                if (Current(generation)) SetError(error);
            }
        }

        private async Task RefreshProfileAsync(
            int generation,
            CancellationToken cancellationToken)
        {
            try
            {
                RankedIdentity identity = _client.Identity;
                if (identity != null)
                {
                    try
                    {
                        Profile = await _client.ProfileAsync(
                            identity.playerId,
                            cancellationToken);
                        Rating = Profile.rating;
                    }
                    catch (RankedServiceException error)
                        when (error.StatusCode == 404)
                    {
                        _client.ClearIdentity();
                        Profile = null;
                        Rating = RankedRatingStore.StartingRating;
                    }
                }
                Leaderboard = await _client.LeaderboardAsync(
                    8,
                    cancellationToken);
                if (!Current(generation)) return;
                State = RankedUiState.Idle;
                ReleaseClient();
                Publish();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception error)
            {
                if (Current(generation)) SetError(error);
            }
        }

        private static async Task CancelAndDisposeAsync(
            RankedServiceClient client,
            string ticketId,
            string ticketToken)
        {
            if (client == null) return;
            try
            {
                if (!string.IsNullOrEmpty(ticketId) &&
                    !string.IsNullOrEmpty(ticketToken))
                {
                    await client.CancelAsync(
                        ticketId,
                        ticketToken,
                        CancellationToken.None);
                }
            }
            catch
            {
            }
            finally
            {
                client.Dispose();
            }
        }

        private static void RequireQueueTicket(
            RankedQueueState queue)
        {
            if (queue == null ||
                string.IsNullOrEmpty(queue.ticketId) ||
                string.IsNullOrEmpty(queue.ticketToken) ||
                string.IsNullOrEmpty(queue.status))
            {
                throw new FormatException(
                    "Ranked queue ticket is incomplete.");
            }
        }

        private bool Current(int generation)
        {
            return generation == _generation &&
                _lifetime != null &&
                !_lifetime.IsCancellationRequested;
        }

        private void SetError(string message)
        {
            State = RankedUiState.Error;
            LastException = null;
            ErrorMessage = string.IsNullOrEmpty(message)
                ? "Ranked service failed."
                : message;
            Publish();
        }

        private void SetError(Exception error)
        {
            LastException = error;
            State = RankedUiState.Error;
            ErrorMessage = string.IsNullOrEmpty(error?.Message)
                ? "Ranked service failed."
                : error.Message;
            Publish();
        }

        private void ReleaseOperation()
        {
            ++_generation;
            _lifetime?.Cancel();
            _lifetime?.Dispose();
            _lifetime = null;
            _handoff?.Dispose();
            _handoff = null;
            ReleaseClient();
        }

        private void ReleaseClient()
        {
            _client?.Dispose();
            _client = null;
        }

        private void Publish()
        {
            Changed?.Invoke();
        }

        private void OnDestroy()
        {
            ++_generation;
            _lifetime?.Cancel();
            _lifetime?.Dispose();
            _handoff?.Dispose();
            ReleaseClient();
            Changed = null;
            MatchHandoffReady = null;
        }
    }
}
