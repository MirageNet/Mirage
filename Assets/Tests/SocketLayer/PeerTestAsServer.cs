using System;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("tests for Peer that only apply to server")]
    public class PeerTestAsServer : PeerTestBase
    {
        [Test]
        public void BindShoudlCallSocketBind()
        {
            IEndPoint endPoint = Substitute.For<IEndPoint>();
            peer.Bind(endPoint);

            socket.Received(1).Bind(Arg.Is(endPoint));
        }

        [Test]
        public void CloseSendsDisconnectMessageToAllConnections()
        {
            IEndPoint endPoint = Substitute.For<IEndPoint>();
            peer.Bind(endPoint);

            var endPoints = new IEndPoint[maxConnections];
            for (int i = 0; i < maxConnections; i++)
            {
                endPoints[i] = Substitute.For<IEndPoint>();

                socket.SetupReceiveCall(connectRequest, endPoints[i]);
                peer.Update();
            }

            for (int i = 0; i < maxConnections; i++)
            {
                socket.ClearReceivedCalls();
            }

            peer.Close();

            byte[] disconnectCommand = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.Disconnect,
                (byte)DisconnectReason.RequestedByRemotePeer,
            };
            for (int i = 0; i < maxConnections; i++)
            {
                socket.Received(1).Send(
                    Arg.Is(endPoints[i]),
                    Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(disconnectCommand)),
                    Arg.Is(disconnectCommand.Length)
                );
            }
        }

        [Test]
        public void AcceptsConnectionForValidMessage()
        {
            peer.Bind(Substitute.For<IEndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            IEndPoint endPoint = Substitute.For<IEndPoint>();
            socket.SetupReceiveCall(connectRequest, endPoint);
            peer.Update();

            // server sends accept and invokes event locally
            socket.Received(1).Send(endPoint, Arg.Is<byte[]>(x =>
                x.Length >= 2 &&
                x[0] == (byte)PacketType.Command &&
                x[1] == (byte)Commands.ConnectionAccepted
            ), 2);
            connectAction.ReceivedWithAnyArgs(1).Invoke(default);
        }

        [Test]
        public void AcceptsConnectionsUpToMax()
        {
            peer.Bind(Substitute.For<IEndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;


            var endPoints = new IEndPoint[maxConnections];
            for (int i = 0; i < maxConnections; i++)
            {
                endPoints[i] = Substitute.For<IEndPoint>();

                socket.SetupReceiveCall(connectRequest, endPoints[i]);
                peer.Update();
            }


            // server sends accept and invokes event locally
            connectAction.ReceivedWithAnyArgs(maxConnections).Invoke(default);
            for (int i = 0; i < maxConnections; i++)
            {
                socket.Received(1).Send(endPoints[i], Arg.Is<byte[]>(x =>
                    x.Length >= 2 &&
                    x[0] == (byte)PacketType.Command &&
                    x[1] == (byte)Commands.ConnectionAccepted
                ), 2);
            }
        }

        [Test]
        public void RejectsConnectionOverMax()
        {
            peer.Bind(Substitute.For<IEndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            for (int i = 0; i < maxConnections; i++)
            {
                socket.SetupReceiveCall(connectRequest);
                peer.Update();
            }

            // clear calls from valid connections
            socket.ClearReceivedCalls();
            connectAction.ClearReceivedCalls();

            IEndPoint overMaxEndpoint = Substitute.For<IEndPoint>();
            socket.SetupReceiveCall(connectRequest, overMaxEndpoint);


            byte[] received = null;
            socket.WhenForAnyArgs(x => x.Send(default, default, default)).Do(x =>
            {
                received = (byte[])x[1];
            });

            peer.Update();

            Debug.Log($"Length:{received.Length} [{received[0]},{received[1]},{received[2]}]");
            const int length = 3;
            socket.Received(1).Send(overMaxEndpoint, Arg.Is<byte[]>(x =>
                x.Length >= length &&
                x[0] == (byte)PacketType.Command &&
                x[1] == (byte)Commands.ConnectionRejected &&
                x[2] == (byte)RejectReason.ServerFull
            ), length);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test, Description("Should reject with no reason given")]
        public void IgnoresMessageThatIsInvalid()
        {
            peer.Bind(Substitute.For<IEndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            byte[] invalidRequest = new byte[2] {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest
            };

            socket.SetupReceiveCall(invalidRequest);
            peer.Update();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }
}
