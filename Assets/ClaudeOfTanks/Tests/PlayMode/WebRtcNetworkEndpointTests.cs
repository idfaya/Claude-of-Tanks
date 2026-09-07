using System;
using System.Collections;
using System.Collections.Generic;
using ClaudeOfTanks.Network;
using ClaudeOfTanks.Simulation;
using ClaudeOfTanks.WebRTC;
using NUnit.Framework;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.TestTools;

namespace ClaudeOfTanks.Tests
{
    public sealed class WebRtcNetworkEndpointTests
    {
        [UnityTest]
        public IEnumerator PeerSessionsExchangeStrictSignalsAndOpenTransport()
        {
            using (WebRtcPeerSession host =
                new WebRtcPeerSession(WebRtcPeerRole.Host))
            using (WebRtcPeerSession client =
                new WebRtcPeerSession(WebRtcPeerRole.Client))
            {
                Queue<WebRtcSignal> toHost = new Queue<WebRtcSignal>();
                Queue<WebRtcSignal> toClient = new Queue<WebRtcSignal>();
                List<string> failures = new List<string>();
                WebRtcSignal firstOffer = null;
                int clientDescriptions = 0;
                int clientRestarts = 0;
                host.SignalReady += signal =>
                {
                    if (signal.kind == "description" && firstOffer == null)
                        firstOffer = signal;
                    toClient.Enqueue(signal);
                };
                client.SignalReady += signal =>
                {
                    if (signal.kind == "description") clientDescriptions++;
                    if (signal.kind == "restart") clientRestarts++;
                    toHost.Enqueue(signal);
                };
                host.Failed += failures.Add;
                client.Failed += failures.Add;

                yield return host.Start();
                yield return client.Start();
                DateTime deadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < deadline &&
                    (!host.IsTransportReady || !client.IsTransportReady))
                {
                    while (toClient.Count > 0)
                    {
                        WebRtcSignal signal = toClient.Dequeue();
                        string json = JsonUtility.ToJson(signal);
                        Assert.That(json, Does.Contain("\"kind\""));
                        yield return client.HandleSignal(
                            JsonUtility.FromJson<WebRtcSignal>(json));
                    }
                    while (toHost.Count > 0)
                    {
                        WebRtcSignal signal = toHost.Dequeue();
                        yield return host.HandleSignal(
                            JsonUtility.FromJson<WebRtcSignal>(
                                JsonUtility.ToJson(signal)));
                    }
                    yield return null;
                }

                Assert.That(failures, Is.Empty);
                Assert.That(host.IsTransportReady, Is.True);
                Assert.That(client.IsTransportReady, Is.True);
                Assert.That(firstOffer, Is.Not.Null);
                int descriptionsBeforeReplay = clientDescriptions;
                yield return client.HandleSignal(
                    JsonUtility.FromJson<WebRtcSignal>(
                        JsonUtility.ToJson(firstOffer)));
                Assert.That(
                    clientDescriptions,
                    Is.EqualTo(descriptionsBeforeReplay + 1),
                    "duplicate offer must replay the existing answer");
                yield return client.Restart();
                Assert.That(clientRestarts, Is.EqualTo(1));
                byte received = 0;
                client.Transport.ControlReceived += packet => received = packet[0];
                Assert.That(host.Transport.SendControl(new byte[] { 77 }), Is.True);
                deadline = DateTime.UtcNow.AddSeconds(3);
                while (DateTime.UtcNow < deadline && received != 77)
                {
                    host.Transport.Pump();
                    client.Transport.Pump();
                    yield return null;
                }
                Assert.That(received, Is.EqualTo(77));
            }
        }

        [Test]
        public void RelayOnlySessionRequiresTurnAndSignalsAreBounded()
        {
            RTCConfiguration invalid = new RTCConfiguration
            {
                iceTransportPolicy = RTCIceTransportPolicy.Relay,
                iceServers = new[]
                {
                    new RTCIceServer { urls = new[] { "stun:localhost:3478" } }
                }
            };
            Assert.Throws<ArgumentException>(() =>
                new WebRtcPeerSession(WebRtcPeerRole.Host, invalid));
            using (WebRtcPeerSession client =
                new WebRtcPeerSession(WebRtcPeerRole.Client))
            {
                Assert.Throws<FormatException>(() =>
                {
                    IEnumerator routine = client.HandleSignal(new WebRtcSignal
                    {
                        kind = "ice",
                        candidate = new WebRtcCandidateSignal
                        {
                            candidate = new string('x',
                                WebRtcPeerSession.MaximumCandidateCharacters + 1),
                            sdpMid = "0"
                        }
                    });
                    routine.MoveNext();
                });
            }
        }

        [UnityTest]
        public IEnumerator RealPeerPairRunsAuthoritativeHostAndClientPumps()
        {
            using (RTCPeerConnection hostPeer = new RTCPeerConnection())
            using (RTCPeerConnection clientPeer = new RTCPeerConnection())
            {
                RTCDataChannel hostControl =
                    WebRtcNetworkEndpoint.CreateControlChannel(hostPeer);
                RTCDataChannel hostState =
                    WebRtcNetworkEndpoint.CreateStateChannel(hostPeer);
                RTCDataChannel clientControl = null;
                RTCDataChannel clientState = null;
                clientPeer.OnDataChannel = channel =>
                {
                    if (channel.Label == WebRtcNetworkEndpoint.ControlChannelLabel)
                        clientControl = channel;
                    else if (channel.Label == WebRtcNetworkEndpoint.StateChannelLabel)
                        clientState = channel;
                    else
                        channel.Close();
                };

                List<RTCIceCandidate> hostCandidates = new List<RTCIceCandidate>();
                List<RTCIceCandidate> clientCandidates = new List<RTCIceCandidate>();
                bool hostRemoteSet = false;
                bool clientRemoteSet = false;
                hostPeer.OnIceCandidate = candidate =>
                {
                    if (candidate == null) return;
                    if (clientRemoteSet) clientPeer.AddIceCandidate(candidate);
                    else hostCandidates.Add(candidate);
                };
                clientPeer.OnIceCandidate = candidate =>
                {
                    if (candidate == null) return;
                    if (hostRemoteSet) hostPeer.AddIceCandidate(candidate);
                    else clientCandidates.Add(candidate);
                };

                RTCSessionDescriptionAsyncOperation offer = hostPeer.CreateOffer();
                yield return offer;
                AssertOperation(offer.IsError, offer.Error);
                RTCSessionDescription offerDescription = offer.Desc;
                RTCSetSessionDescriptionAsyncOperation hostLocal =
                    hostPeer.SetLocalDescription(ref offerDescription);
                yield return hostLocal;
                AssertOperation(hostLocal.IsError, hostLocal.Error);
                RTCSetSessionDescriptionAsyncOperation clientRemote =
                    clientPeer.SetRemoteDescription(ref offerDescription);
                yield return clientRemote;
                AssertOperation(clientRemote.IsError, clientRemote.Error);
                clientRemoteSet = true;
                AddCandidates(clientPeer, hostCandidates);

                RTCSessionDescriptionAsyncOperation answer = clientPeer.CreateAnswer();
                yield return answer;
                AssertOperation(answer.IsError, answer.Error);
                RTCSessionDescription answerDescription = answer.Desc;
                RTCSetSessionDescriptionAsyncOperation clientLocal =
                    clientPeer.SetLocalDescription(ref answerDescription);
                yield return clientLocal;
                AssertOperation(clientLocal.IsError, clientLocal.Error);
                RTCSetSessionDescriptionAsyncOperation hostRemote =
                    hostPeer.SetRemoteDescription(ref answerDescription);
                yield return hostRemote;
                AssertOperation(hostRemote.IsError, hostRemote.Error);
                hostRemoteSet = true;
                AddCandidates(hostPeer, clientCandidates);

                DateTime deadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < deadline &&
                    (hostControl.ReadyState != RTCDataChannelState.Open ||
                     hostState.ReadyState != RTCDataChannelState.Open ||
                     clientControl == null ||
                     clientState == null ||
                     clientControl.ReadyState != RTCDataChannelState.Open ||
                     clientState.ReadyState != RTCDataChannelState.Open))
                {
                    yield return null;
                }

                Assert.That(hostControl.ReadyState, Is.EqualTo(RTCDataChannelState.Open));
                Assert.That(hostState.Ordered, Is.False);
                Assert.That(hostState.MaxRetransmits, Is.Zero);
                Assert.That(clientControl, Is.Not.Null);
                Assert.That(clientState, Is.Not.Null);

                using (WebRtcNetworkEndpoint hostEndpoint =
                    WebRtcNetworkEndpoint.Attach(hostControl, hostState))
                using (WebRtcNetworkEndpoint clientEndpoint =
                    WebRtcNetworkEndpoint.Attach(clientControl, clientState))
                {
                    BattleState state = new BattleState(
                        new FlatHeightField(),
                        1301u);
                    TankState tank = new TankState(
                        "rtc-entity",
                        Team.Alpha,
                        TankSpec.Medium(),
                        Float3.Zero,
                        0f);
                    state.Tanks.Add(tank);
                    AuthoritativeMatchHost host = new AuthoritativeMatchHost(
                        new BattleSimulation(state));
                    host.RegisterPlayer("rtc-player", tank.Id);
                    using (AuthoritativeHostPump hostPump =
                        new AuthoritativeHostPump(
                            host,
                            "rtc-player",
                            hostEndpoint))
                    using (NetworkClientPump clientPump =
                        new NetworkClientPump(
                            "rtc-player",
                            tank.Id,
                            clientEndpoint))
                    {
                        Assert.That(clientPump.SendInput(new NetworkInputCommand
                        {
                            PlayerId = "rtc-player",
                            Sequence = 1u,
                            ClientTick = 0,
                            SnapshotAckTick = -1,
                            Throttle = 1f,
                            AimDistanceM = 100f
                        }), Is.True);

                        deadline = DateTime.UtcNow.AddSeconds(5);
                        while (DateTime.UtcNow < deadline &&
                            clientPump.LatestSnapshot == null)
                        {
                            hostPump.Update(1);
                            clientPump.Pump();
                            yield return null;
                        }

                        Assert.That(clientPump.LatestSnapshot, Is.Not.Null);
                        Assert.That(
                            clientPump.LatestSnapshot.ViewerEntityId,
                            Is.EqualTo("rtc-entity"));
                        Assert.That(tank.Position.Z, Is.GreaterThan(0f));
                        Assert.That(
                            hostEndpoint.Stats.StateSent,
                            Is.GreaterThanOrEqualTo(1));
                        Assert.That(
                            clientEndpoint.Stats.ControlSent,
                            Is.GreaterThanOrEqualTo(1));
                    }
                }
            }
        }

        private static void AddCandidates(
            RTCPeerConnection peer,
            List<RTCIceCandidate> candidates)
        {
            for (int i = 0; i < candidates.Count; i++)
                Assert.That(peer.AddIceCandidate(candidates[i]), Is.True);
            candidates.Clear();
        }

        private static void AssertOperation(bool isError, RTCError error)
        {
            Assert.That(
                isError,
                Is.False,
                error.errorType + ": " + error.message);
        }
    }
}
