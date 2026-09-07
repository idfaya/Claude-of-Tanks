using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClaudeOfTanks.WebRTC
{
    internal sealed class SignalingSignalQueue
    {
        private readonly object _gate = new object();
        private readonly List<SignalingPayload> _queued =
            new List<SignalingPayload>();
        private bool _flushing;

        public int Count
        {
            get { lock (_gate) return _queued.Count; }
        }

        public void Enqueue(SignalingPayload signal)
        {
            lock (_gate)
            {
                if (_queued.Count >=
                    RoomSignalingProtocol.MaximumQueuedSignals)
                {
                    _queued.RemoveAt(0);
                }
                _queued.Add(signal);
            }
        }

        public void Clear()
        {
            lock (_gate) _queued.Clear();
        }

        public void StartFlush(
            Func<bool> canSend,
            Func<SignalingPayload, Task> send,
            Action<string> failed)
        {
            lock (_gate)
            {
                if (_flushing || _queued.Count == 0 || !canSend()) return;
                _flushing = true;
            }
            Task.Run(async () =>
            {
                try
                {
                    while (canSend())
                    {
                        SignalingPayload signal;
                        lock (_gate)
                        {
                            if (_queued.Count == 0) return;
                            signal = _queued[0];
                        }
                        await send(signal).ConfigureAwait(false);
                        lock (_gate)
                        {
                            if (_queued.Count > 0 &&
                                ReferenceEquals(_queued[0], signal))
                            {
                                _queued.RemoveAt(0);
                            }
                        }
                    }
                }
                catch
                {
                    failed("signal_flush_failed");
                }
                finally
                {
                    bool restart;
                    lock (_gate)
                    {
                        _flushing = false;
                        restart = _queued.Count > 0 && canSend();
                    }
                    if (restart) StartFlush(canSend, send, failed);
                }
            });
        }
    }
}
