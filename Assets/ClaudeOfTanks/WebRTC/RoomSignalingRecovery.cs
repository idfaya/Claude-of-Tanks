using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClaudeOfTanks.WebRTC
{
    internal enum RoomResumeResult
    {
        Retry,
        Resumed,
        RoomClosed
    }

    internal sealed class RoomSignalingRecovery
    {
        private readonly object _gate = new object();
        private readonly RoomSignalingClientOptions _options;
        private readonly CancellationToken _cancellationToken;
        private bool _polling;
        private Task<bool> _reconnecting;
        private int _attempt;

        public RoomSignalingRecovery(
            RoomSignalingClientOptions options,
            CancellationToken cancellationToken)
        {
            _options = options;
            _cancellationToken = cancellationToken;
        }

        public void ResetAttempts()
        {
            lock (_gate) _attempt = 0;
        }

        public void StartPolling(
            Func<bool> canPoll,
            Func<Task> poll,
            Action<string> failed)
        {
            lock (_gate)
            {
                if (_polling || !canPoll()) return;
                _polling = true;
            }
            Task.Run(async () =>
            {
                try
                {
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(
                            _options.EventPollIntervalMs,
                            _cancellationToken).ConfigureAwait(false);
                        if (!canPoll()) return;
                        await poll().ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) { }
                catch { failed("signaling_poll_failed"); }
                finally
                {
                    bool restart;
                    lock (_gate)
                    {
                        _polling = false;
                        restart = canPoll();
                    }
                    if (restart) StartPolling(canPoll, poll, failed);
                }
            });
        }

        public Task<bool> EnsureReconnect(
            string reason,
            bool immediate,
            Func<bool> canReconnect,
            Func<Task<RoomResumeResult>> resume,
            Action<string, int, int> stateChanged)
        {
            lock (_gate)
            {
                if (!canReconnect()) return Task.FromResult(false);
                if (_reconnecting != null) return _reconnecting;
                int delay = immediate ? 0 : NextDelayLocked();
                stateChanged(reason, _attempt, delay);
                _reconnecting = ReconnectLoopAsync(
                    immediate,
                    delay,
                    canReconnect,
                    resume,
                    stateChanged);
                return _reconnecting;
            }
        }

        private async Task<bool> ReconnectLoopAsync(
            bool immediate,
            int delay,
            Func<bool> canReconnect,
            Func<Task<RoomResumeResult>> resume,
            Action<string, int, int> stateChanged)
        {
            await Task.Yield();
            try
            {
                while (canReconnect() &&
                    !_cancellationToken.IsCancellationRequested)
                {
                    if (!immediate && delay > 0)
                    {
                        await Task.Delay(delay, _cancellationToken)
                            .ConfigureAwait(false);
                    }
                    immediate = false;
                    RoomResumeResult result = await resume().ConfigureAwait(false);
                    if (result == RoomResumeResult.Resumed) return true;
                    if (result == RoomResumeResult.RoomClosed) return false;
                    lock (_gate)
                    {
                        delay = NextDelayLocked();
                        stateChanged("room_resume_retry", _attempt, delay);
                    }
                }
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            finally
            {
                lock (_gate) _reconnecting = null;
            }
        }

        private int NextDelayLocked()
        {
            int index = Math.Min(
                _attempt,
                _options.ReconnectDelaysMs.Length - 1);
            _attempt++;
            return _options.ReconnectDelaysMs[index];
        }
    }
}
