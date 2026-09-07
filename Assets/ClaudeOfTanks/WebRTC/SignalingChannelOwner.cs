using System;
using System.Threading.Tasks;

namespace ClaudeOfTanks.WebRTC
{
    internal sealed class SignalingChannelOwner : IDisposable
    {
        private readonly object _gate = new object();
        private readonly Uri _endpoint;
        private readonly RoomSignalingClientOptions _options;
        private SignalingRequestChannel _channel;
        private Task _connectTask;
        private bool _disposed;

        public SignalingChannelOwner(
            Uri endpoint,
            RoomSignalingClientOptions options)
        {
            _endpoint = endpoint;
            _options = options;
        }

        public bool IsOpen
        {
            get
            {
                lock (_gate)
                    return _channel != null && _channel.IsOpen;
            }
        }

        public event Action<SignalingEnvelope> EventReceived;
        public event Action<string> Closed;

        public Task ConnectAsync()
        {
            lock (_gate)
            {
                RequireAlive();
                if (_channel != null && _channel.IsOpen)
                    return Task.CompletedTask;
                if (_connectTask != null) return _connectTask;
                ReplaceChannelLocked();
                _connectTask = ConnectCurrentAsync(_channel);
                return _connectTask;
            }
        }

        public SignalingRequestChannel Current()
        {
            lock (_gate)
            {
                if (_channel == null)
                    throw new RoomSignalingException(
                        "signaling_closed",
                        "Signaling socket is not open.");
                return _channel;
            }
        }

        public void Abort(string reason)
        {
            SignalingRequestChannel channel;
            lock (_gate)
            {
                channel = _channel;
                DetachLocked(channel);
                _channel = null;
                _connectTask = null;
            }
            channel?.Abort(reason);
            channel?.Dispose();
        }

        public async Task CloseAsync(string reason)
        {
            SignalingRequestChannel channel;
            lock (_gate)
            {
                channel = _channel;
                DetachLocked(channel);
                _channel = null;
                _connectTask = null;
            }
            if (channel != null)
            {
                await channel.CloseAsync(reason).ConfigureAwait(false);
                channel.Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Abort("disposed");
            _disposed = true;
            EventReceived = null;
            Closed = null;
        }

        private async Task ConnectCurrentAsync(
            SignalingRequestChannel channel)
        {
            try
            {
                await channel.ConnectAsync().ConfigureAwait(false);
                lock (_gate)
                {
                    if (_channel != channel)
                        throw new OperationCanceledException();
                    _connectTask = null;
                }
            }
            catch
            {
                lock (_gate)
                {
                    if (_channel == channel)
                        _connectTask = null;
                }
                throw;
            }
        }

        private void ReplaceChannelLocked()
        {
            SignalingRequestChannel previous = _channel;
            if (previous != null)
            {
                DetachLocked(previous);
                previous.Abort("replaced");
                previous.Dispose();
            }
            _channel = new SignalingRequestChannel(_endpoint, _options);
            _channel.EventReceived += ForwardEvent;
            _channel.Closed += ForwardClose;
        }

        private void DetachLocked(SignalingRequestChannel channel)
        {
            if (channel == null) return;
            channel.EventReceived -= ForwardEvent;
            channel.Closed -= ForwardClose;
        }

        private void ForwardEvent(SignalingEnvelope message)
        {
            EventReceived?.Invoke(message);
        }

        private void ForwardClose(string reason)
        {
            Closed?.Invoke(reason);
        }

        private void RequireAlive()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SignalingChannelOwner));
        }
    }
}
