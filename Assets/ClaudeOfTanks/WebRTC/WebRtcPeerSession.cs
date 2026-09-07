using System;
using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;

namespace ClaudeOfTanks.WebRTC
{
    public enum WebRtcPeerRole
    {
        Host,
        Client
    }

    [Serializable]
    public sealed class WebRtcDescriptionSignal
    {
        public string type;
        public string sdp;
    }

    [Serializable]
    public sealed class WebRtcCandidateSignal
    {
        public string candidate;
        public string sdpMid;
        public int sdpMLineIndex = -1;
    }

    [Serializable]
    public sealed class WebRtcSignal
    {
        public string kind;
        public WebRtcDescriptionSignal description;
        public WebRtcCandidateSignal candidate;

        public static WebRtcSignal Description(
            RTCSessionDescription description)
        {
            return new WebRtcSignal
            {
                kind = "description",
                description = new WebRtcDescriptionSignal
                {
                    type = description.type == RTCSdpType.Offer
                        ? "offer"
                        : "answer",
                    sdp = description.sdp
                }
            };
        }

        public static WebRtcSignal Candidate(RTCIceCandidate candidate)
        {
            return new WebRtcSignal
            {
                kind = "ice",
                candidate = new WebRtcCandidateSignal
                {
                    candidate = candidate.Candidate,
                    sdpMid = candidate.SdpMid,
                    sdpMLineIndex = candidate.SdpMLineIndex ?? -1
                }
            };
        }

        public static WebRtcSignal Restart()
        {
            return new WebRtcSignal { kind = "restart" };
        }
    }

    public sealed class WebRtcPeerSession : IDisposable
    {
        public const int MaximumSdpCharacters = 96 * 1024;
        public const int MaximumCandidateCharacters = 8 * 1024;
        public const int MaximumPendingCandidates = 256;

        private readonly RTCPeerConnection _peer;
        private readonly List<WebRtcCandidateSignal> _pendingCandidates =
            new List<WebRtcCandidateSignal>();
        private readonly HashSet<string> _seenCandidates =
            new HashSet<string>(StringComparer.Ordinal);
        private RTCDataChannel _control;
        private RTCDataChannel _state;
        private WebRtcNetworkEndpoint _transport;
        private bool _remoteDescriptionSet;
        private string _remoteDescriptionType;
        private string _remoteDescriptionSdp;
        private WebRtcSignal _localDescriptionSignal;
        private bool _started;
        private bool _negotiating;
        private bool _disposed;

        public WebRtcPeerSession(
            WebRtcPeerRole role,
            RTCConfiguration? configuration = null)
        {
            Role = role;
            if (configuration.HasValue)
            {
                RTCConfiguration value = configuration.Value;
                ValidateConfiguration(value);
                _peer = new RTCPeerConnection(ref value);
            }
            else
            {
                _peer = new RTCPeerConnection();
            }
            _peer.OnIceCandidate = candidate =>
            {
                if (!_disposed && candidate != null)
                    SignalReady?.Invoke(WebRtcSignal.Candidate(candidate));
            };
            _peer.OnDataChannel = channel =>
            {
                if (Role != WebRtcPeerRole.Client)
                {
                    channel.Close();
                    return;
                }
                AttachChannel(channel);
            };
            _peer.OnConnectionStateChange = state =>
            {
                ConnectionStateChanged?.Invoke(state);
                if (state == RTCPeerConnectionState.Failed)
                    Failed?.Invoke("rtc_connection_failed");
                else if (state == RTCPeerConnectionState.Closed)
                    Failed?.Invoke("rtc_connection_closed");
            };
        }

        public WebRtcPeerRole Role { get; }
        public RTCPeerConnection Peer => _peer;
        public WebRtcNetworkEndpoint Transport => _transport;
        public bool IsTransportReady => _transport != null && _transport.IsOpen;
        public event Action<WebRtcSignal> SignalReady;
        public event Action<WebRtcNetworkEndpoint> TransportReady;
        public event Action<RTCPeerConnectionState> ConnectionStateChanged;
        public event Action<string> Failed;

        public IEnumerator Start()
        {
            RequireAlive();
            if (_started) throw new InvalidOperationException("WebRTC session already started.");
            _started = true;
            if (Role == WebRtcPeerRole.Client) yield break;
            _control = WebRtcNetworkEndpoint.CreateControlChannel(_peer);
            _state = WebRtcNetworkEndpoint.CreateStateChannel(_peer);
            WatchChannel(_control);
            WatchChannel(_state);
            yield return CreateAndPublishOffer(false);
        }

        public IEnumerator HandleSignal(WebRtcSignal signal)
        {
            RequireAlive();
            ValidateSignal(signal);
            if (signal.kind == "restart")
            {
                if (Role == WebRtcPeerRole.Host)
                {
                    _peer.RestartIce();
                    yield return CreateAndPublishOffer(true);
                }
                else
                {
                    Fail("rtc_unexpected_restart");
                }
                yield break;
            }
            if (signal.kind == "ice")
            {
                string candidateKey = CandidateKey(signal.candidate);
                if (_seenCandidates.Contains(candidateKey)) yield break;
                if (_seenCandidates.Count >= MaximumPendingCandidates * 2)
                {
                    Fail("rtc_candidate_history_overflow");
                    yield break;
                }
                _seenCandidates.Add(candidateKey);
                if (_remoteDescriptionSet)
                    AddCandidate(signal.candidate);
                else if (_pendingCandidates.Count < MaximumPendingCandidates)
                    _pendingCandidates.Add(Clone(signal.candidate));
                else
                    Fail("rtc_candidate_overflow");
                yield break;
            }

            RTCSdpType type = signal.description.type == "offer"
                ? RTCSdpType.Offer
                : RTCSdpType.Answer;
            if ((Role == WebRtcPeerRole.Host && type != RTCSdpType.Answer) ||
                (Role == WebRtcPeerRole.Client && type != RTCSdpType.Offer))
            {
                Fail("rtc_unexpected_description");
                yield break;
            }
            if (_remoteDescriptionSet &&
                _remoteDescriptionType == signal.description.type &&
                _remoteDescriptionSdp == signal.description.sdp)
            {
                if (Role == WebRtcPeerRole.Client &&
                    _localDescriptionSignal != null)
                {
                    SignalReady?.Invoke(_localDescriptionSignal);
                }
                yield break;
            }
            if (_negotiating)
            {
                Fail("rtc_overlapping_negotiation");
                yield break;
            }
            _negotiating = true;
            RTCSessionDescription description = new RTCSessionDescription
            {
                type = type,
                sdp = signal.description.sdp
            };
            RTCSetSessionDescriptionAsyncOperation remote =
                _peer.SetRemoteDescription(ref description);
            yield return remote;
            if (remote.IsError)
            {
                _negotiating = false;
                Fail("rtc_remote_description:" + remote.Error.errorType);
                yield break;
            }
            _remoteDescriptionSet = true;
            _remoteDescriptionType = signal.description.type;
            _remoteDescriptionSdp = signal.description.sdp;
            DrainCandidates();
            if (Role == WebRtcPeerRole.Client)
            {
                RTCSessionDescriptionAsyncOperation answer = _peer.CreateAnswer();
                yield return answer;
                if (answer.IsError)
                {
                    _negotiating = false;
                    Fail("rtc_answer:" + answer.Error.errorType);
                    yield break;
                }
                RTCSessionDescription localDescription = answer.Desc;
                RTCSetSessionDescriptionAsyncOperation local =
                    _peer.SetLocalDescription(ref localDescription);
                yield return local;
                if (local.IsError)
                {
                    _negotiating = false;
                    Fail("rtc_local_description:" + local.Error.errorType);
                    yield break;
                }
                _localDescriptionSignal =
                    WebRtcSignal.Description(localDescription);
                SignalReady?.Invoke(_localDescriptionSignal);
            }
            _negotiating = false;
        }

        public IEnumerator Restart()
        {
            RequireAlive();
            if (Role == WebRtcPeerRole.Host)
            {
                _peer.RestartIce();
                yield return CreateAndPublishOffer(true);
            }
            else
            {
                SignalReady?.Invoke(WebRtcSignal.Restart());
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            SignalReady = null;
            TransportReady = null;
            ConnectionStateChanged = null;
            Failed = null;
            _peer.OnIceCandidate = null;
            _peer.OnDataChannel = null;
            _peer.OnConnectionStateChange = null;
            if (_transport != null)
            {
                _transport.Dispose();
                _transport = null;
                _control = null;
                _state = null;
            }
            else
            {
                DisposeChannel(_control);
                DisposeChannel(_state);
            }
            _pendingCandidates.Clear();
            _seenCandidates.Clear();
            _peer.Dispose();
        }

        private IEnumerator CreateAndPublishOffer(bool iceRestart)
        {
            if (_negotiating)
            {
                Fail("rtc_overlapping_negotiation");
                yield break;
            }
            _negotiating = true;
            RTCOfferAnswerOptions options = RTCOfferAnswerOptions.Default;
            options.iceRestart = iceRestart;
            RTCSessionDescriptionAsyncOperation offer =
                _peer.CreateOffer(ref options);
            yield return offer;
            if (offer.IsError)
            {
                _negotiating = false;
                Fail("rtc_offer:" + offer.Error.errorType);
                yield break;
            }
            RTCSessionDescription description = offer.Desc;
            RTCSetSessionDescriptionAsyncOperation local =
                _peer.SetLocalDescription(ref description);
            yield return local;
            _negotiating = false;
            if (local.IsError)
            {
                Fail("rtc_local_description:" + local.Error.errorType);
                yield break;
            }
            _localDescriptionSignal = WebRtcSignal.Description(description);
            SignalReady?.Invoke(_localDescriptionSignal);
        }

        private void AttachChannel(RTCDataChannel channel)
        {
            if (channel.Label == WebRtcNetworkEndpoint.ControlChannelLabel)
            {
                DisposeChannel(_control);
                _control = channel;
            }
            else if (channel.Label == WebRtcNetworkEndpoint.StateChannelLabel)
            {
                DisposeChannel(_state);
                _state = channel;
            }
            else
            {
                channel.Close();
                channel.Dispose();
                Fail("rtc_unexpected_channel");
                return;
            }
            WatchChannel(channel);
            TryAttachTransport();
        }

        private void WatchChannel(RTCDataChannel channel)
        {
            channel.OnOpen = TryAttachTransport;
            channel.OnClose = () =>
            {
                if (!_disposed) Failed?.Invoke("rtc_channel_closed");
            };
        }

        private void TryAttachTransport()
        {
            if (_disposed || _transport != null ||
                _control == null || _state == null ||
                _control.ReadyState != RTCDataChannelState.Open ||
                _state.ReadyState != RTCDataChannelState.Open)
            {
                return;
            }
            _transport = WebRtcNetworkEndpoint.Attach(_control, _state);
            TransportReady?.Invoke(_transport);
        }

        private void DrainCandidates()
        {
            for (int i = 0; i < _pendingCandidates.Count; i++)
                AddCandidate(_pendingCandidates[i]);
            _pendingCandidates.Clear();
        }

        private void AddCandidate(WebRtcCandidateSignal signal)
        {
            RTCIceCandidateInit value = new RTCIceCandidateInit
            {
                candidate = signal.candidate,
                sdpMid = string.IsNullOrEmpty(signal.sdpMid) ? null : signal.sdpMid,
                sdpMLineIndex = signal.sdpMLineIndex < 0
                    ? (int?)null
                    : signal.sdpMLineIndex
            };
            using (RTCIceCandidate candidate = new RTCIceCandidate(value))
            {
                if (!_peer.AddIceCandidate(candidate))
                    Fail("rtc_candidate_rejected");
            }
        }

        private void Fail(string reason)
        {
            Failed?.Invoke(reason);
        }

        private void RequireAlive()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WebRtcPeerSession));
        }

        private static void ValidateSignal(WebRtcSignal signal)
        {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (signal.kind == "restart") return;
            if (signal.kind == "description")
            {
                if (signal.description == null ||
                    (signal.description.type != "offer" &&
                     signal.description.type != "answer") ||
                    string.IsNullOrEmpty(signal.description.sdp) ||
                    signal.description.sdp.Length > MaximumSdpCharacters)
                {
                    throw new FormatException("WebRTC description signal is invalid.");
                }
                return;
            }
            if (signal.kind == "ice")
            {
                if (signal.candidate == null ||
                    string.IsNullOrEmpty(signal.candidate.candidate) ||
                    signal.candidate.candidate.Length > MaximumCandidateCharacters ||
                    (string.IsNullOrEmpty(signal.candidate.sdpMid) &&
                     signal.candidate.sdpMLineIndex < 0))
                {
                    throw new FormatException("WebRTC candidate signal is invalid.");
                }
                return;
            }
            throw new FormatException("WebRTC signal kind is invalid.");
        }

        private static WebRtcCandidateSignal Clone(WebRtcCandidateSignal value)
        {
            return new WebRtcCandidateSignal
            {
                candidate = value.candidate,
                sdpMid = value.sdpMid,
                sdpMLineIndex = value.sdpMLineIndex
            };
        }

        private static string CandidateKey(WebRtcCandidateSignal value)
        {
            return value.candidate + "|" + value.sdpMid + "|" +
                value.sdpMLineIndex;
        }

        private static void ValidateConfiguration(RTCConfiguration value)
        {
            RTCIceServer[] servers = value.iceServers ?? Array.Empty<RTCIceServer>();
            if (servers.Length > 16)
                throw new ArgumentException("Too many ICE servers.", nameof(value));
            if (value.iceTransportPolicy == RTCIceTransportPolicy.Relay)
            {
                bool hasTurn = false;
                for (int i = 0; i < servers.Length; i++)
                {
                    string[] urls = servers[i].urls ?? Array.Empty<string>();
                    for (int j = 0; j < urls.Length; j++)
                        hasTurn |= urls[j].StartsWith(
                            "turn:",
                            StringComparison.OrdinalIgnoreCase) ||
                            urls[j].StartsWith(
                                "turns:",
                                StringComparison.OrdinalIgnoreCase);
                }
                if (!hasTurn)
                    throw new ArgumentException(
                        "Relay-only WebRTC requires a TURN server.",
                        nameof(value));
            }
        }

        private static void DisposeChannel(RTCDataChannel channel)
        {
            if (channel == null) return;
            channel.Close();
            channel.Dispose();
        }
    }
}
