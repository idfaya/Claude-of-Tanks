using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClaudeOfTanks.WebRTC
{
    internal sealed class SignalingRoomRequester
    {
        private static readonly int[] StoreRetryDelaysMs = { 250, 750 };
        private static readonly int[] RoomRetryDelaysMs =
            { 250, 750, 1500, 3000 };
        private readonly RoomSignalingClientOptions _options;
        private readonly CancellationToken _cancellationToken;
        private readonly Func<Task> _connect;
        private readonly Func<SignalingRequestChannel> _channel;
        private readonly Action<string> _disconnect;
        private readonly Func<bool> _isClosed;

        public SignalingRoomRequester(
            RoomSignalingClientOptions options,
            CancellationToken cancellationToken,
            Func<Task> connect,
            Func<SignalingRequestChannel> channel,
            Action<string> disconnect,
            Func<bool> isClosed)
        {
            _options = options;
            _cancellationToken = cancellationToken;
            _connect = connect;
            _channel = channel;
            _disconnect = disconnect;
            _isClosed = isClosed;
        }

        public async Task<SignalingPayload> RequestRoomAsync(
            string type,
            SignalingPayload payload)
        {
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    await _connect().ConfigureAwait(false);
                    return await RequestWithStoreRetryAsync(type, payload)
                        .ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    if (!IsRetryableConnection(error) ||
                        attempt >= RoomRetryDelaysMs.Length ||
                        _isClosed())
                    {
                        throw;
                    }
                    _disconnect("room_request_retry");
                    await Task.Delay(
                        RoomRetryDelaysMs[attempt],
                        _cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<SignalingPayload> RequestWithStoreRetryAsync(
            string type,
            SignalingPayload payload,
            int timeoutMs = -1)
        {
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    return await _channel().RequestAsync(
                        type,
                        payload,
                        timeoutMs < 0 ? _options.RequestTimeoutMs : timeoutMs)
                        .ConfigureAwait(false);
                }
                catch (RoomSignalingException error)
                {
                    if (!IsRetryableStore(error.Code) ||
                        attempt >= StoreRetryDelaysMs.Length)
                    {
                        throw;
                    }
                    await Task.Delay(
                        StoreRetryDelaysMs[attempt],
                        _cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static bool IsRetryableConnection(Exception error)
        {
            RoomSignalingException signaling = error as RoomSignalingException;
            string code = signaling?.Code ?? string.Empty;
            return code == "signaling_closed" ||
                code == "signaling_connection_closed" ||
                code == "signaling_connection_failed" ||
                code == "signaling_connect_timeout" ||
                code == "signaling_request_timeout";
        }

        private static bool IsRetryableStore(string code)
        {
            return code == "signaling_store_unavailable" ||
                code == "redis_ready_timeout" ||
                code == "redis_connection_ended";
        }
    }
}
