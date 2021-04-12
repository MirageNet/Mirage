using System;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("tests for Peer that apply to both server and client")]
    public class PeerTest : PeerTestBase
    {
        [Test]
        public void ThrowIfSocketIsNull()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new Peer(null, Substitute.For<IDataHandler>(), new Config(), Substitute.For<ILogger>());
            });
            var expected = new ArgumentNullException("socket");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void ThrowIfDataHandlerIsNull()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new Peer(Substitute.For<ISocket>(), null, new Config(), Substitute.For<ILogger>());
            });
            var expected = new ArgumentNullException("dataHandler");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void DoesNotThrowIfConfigIsNull()
        {
            Assert.DoesNotThrow(() =>
            {
                new Peer(Substitute.For<ISocket>(), Substitute.For<IDataHandler>(), null, Substitute.For<ILogger>());
            });
        }
        [Test]
        public void DoesNotThrowIfLoggerIsNull()
        {
            Assert.DoesNotThrow(() =>
            {
                new Peer(Substitute.For<ISocket>(), Substitute.For<IDataHandler>(), new Config(), null);
            });
        }

        [Test]
        public void CloseShouldThrowIfNoActive()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                peer.Close();
            });
            Assert.That(exception, Has.Message.EqualTo("Peer is not active"));
        }

        [Test]
        public void CloseShouldCallSocketClose()
        {
            // activate peer
            peer.Bind(default);
            // close peer
            peer.Close();
            socket.Received(1).Close();
        }

        [Test]
        public void IgnoresMessageThatIsTooShort()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            socket.SetupRecieveCall(new byte[1] {
                (byte)UnityEngine.Random.Range(0, 255),
            });

            peer.Update();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void IgnoresMessageThatIsTooLong()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            const int aboveMTU = 5000;
            socket.SetupRecieveCall(new byte[aboveMTU]);

            peer.Update();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        [Repeat(10)]
        public void IgnoresRandomData()
        {
            peer.Bind(Substitute.For<EndPoint>());

            Action<IConnection> connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            EndPoint endPoint = Substitute.For<EndPoint>();

            // 2 is min length of a message
            byte[] randomData = new byte[UnityEngine.Random.Range(2, 20)];
            for (int i = 0; i < randomData.Length; i++)
            {
                randomData[i] = (byte)UnityEngine.Random.Range(0, 255);
            }
            socket.SetupRecieveCall(randomData);

            peer.Update();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }
}
