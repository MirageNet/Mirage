using System;
using System.Net;
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
            EndPoint endPoint = Substitute.For<EndPoint>();
            peer.Bind(endPoint);

            socket.Received(1).Bind(Arg.Is(endPoint));
        }

        [Test]
        public void CloseSendsDisconnectMessageToAllConnections()
        {
            EndPoint endPoint = Substitute.For<EndPoint>();
            peer.Bind(endPoint);

            Assert.Ignore("NotImplemented");
            // todo add connections

            var clientEndPoints = new EndPoint[] {
                Substitute.For<EndPoint>(),
                Substitute.For<EndPoint>() };

            peer.Close();
            byte[] expected = new byte[2]
            {
                (byte)PacketType.Command,
                (byte)Commands.Disconnect,
            };
            socket.Received(1).Send(
                Arg.Is(clientEndPoints[0]),
                Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                Arg.Is(expected.Length)
            );
            socket.Received(1).Send(
                Arg.Is(clientEndPoints[1]),
                Arg.Is<byte[]>(actual => actual.AreEquivalentIgnoringLength(expected)),
                Arg.Is(expected.Length)
            );
        }

        [Test]
        public void AcceptsForValidMessage()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var validator = new ConnectKeyValidator();
            byte[] valid = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest,
                0
            };
            validator.CopyTo(valid);
            EndPoint endPoint = Substitute.For<EndPoint>();
            socket.SetupRecieveCall(valid, endPoint);
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
        public void AcceptsUpToMaxConnections()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var validator = new ConnectKeyValidator();
            byte[] valid = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest,
                0
            };
            validator.CopyTo(valid);
            const int maxConnections = 5;
            var endPoints = new EndPoint[maxConnections];
            for (int i = 0; i < maxConnections; i++)
            {
                endPoints[i] = Substitute.For<EndPoint>();

                socket.SetupRecieveCall(valid, endPoints[i]);
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
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var validator = new ConnectKeyValidator();
            byte[] valid = new byte[3]
            {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest,
                validator.GetKey(),
            };


            const int maxConnections = 5;
            for (int i = 0; i < maxConnections; i++)
            {
                socket.SetupRecieveCall(valid);
                peer.Update();
            }

            // clear calls from valid connections
            socket.ClearReceivedCalls();
            connectAction.ClearReceivedCalls();

            EndPoint overMaxEndpoint = Substitute.For<EndPoint>();
            socket.SetupRecieveCall(valid, overMaxEndpoint);


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
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            socket.SetupRecieveCall(new byte[2] {
                (byte)PacketType.Command,
                (byte)Commands.ConnectRequest
            });
            peer.Update();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }
}
