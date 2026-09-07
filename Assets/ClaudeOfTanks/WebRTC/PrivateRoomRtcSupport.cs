using System;
using System.Collections;
using Unity.WebRTC;
using UnityEngine;

namespace ClaudeOfTanks.WebRTC
{
    public sealed class PrivateRoomPeerTransport
    {
        internal PrivateRoomPeerTransport(
            string peerId,
            string sessionId,
            SignalingPlayer player,
            WebRtcNetworkEndpoint transport)
        {
            PeerId = peerId;
            SessionId = sessionId;
            Player = player;
            Transport = transport;
        }

        public string PeerId { get; }
        public string SessionId { get; }
        public SignalingPlayer Player { get; }
        public WebRtcNetworkEndpoint Transport { get; }
    }

    public sealed class PrivateRoomRtcSessionOptions
    {
        public RTCConfiguration? Configuration;
        public int FailedRebuildDelayMs = 2000;
        public int DisconnectedRebuildDelayMs = 8000;
    }

    internal sealed class WebRtcCoroutineDispatcher
    {
        private readonly MonoBehaviour _owner;

        public WebRtcCoroutineDispatcher(MonoBehaviour owner)
        {
            _owner = owner != null
                ? owner
                : throw new ArgumentNullException(nameof(owner));
        }

        public void Run(
            IEnumerator routine,
            Func<bool> isCurrent,
            Action<Exception> failed = null,
            Action completed = null)
        {
            if (routine == null) throw new ArgumentNullException(nameof(routine));
            if (isCurrent == null) throw new ArgumentNullException(nameof(isCurrent));
            _owner.StartCoroutine(
                Execute(routine, isCurrent, failed, completed));
        }

        private static IEnumerator Execute(
            IEnumerator routine,
            Func<bool> isCurrent,
            Action<Exception> failed,
            Action completed)
        {
            while (isCurrent())
            {
                bool moved;
                object current;
                try
                {
                    moved = routine.MoveNext();
                    current = moved ? routine.Current : null;
                }
                catch (Exception error)
                {
                    failed?.Invoke(error);
                    yield break;
                }
                if (!moved)
                {
                    completed?.Invoke();
                    yield break;
                }
                yield return current;
            }
        }
    }
}
