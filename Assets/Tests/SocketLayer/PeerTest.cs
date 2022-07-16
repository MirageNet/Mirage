using System;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer"), Description("tests for Peer that apply to both server and client")]
    public class PeerTest : PeerTestBase
    {
        [Test]
        public void ThrowIfSocketIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new Peer(null, 1000, Substitute.For<IDataHandler>(), new Config(), Substitute.For<ILogger>());
            });
            var expected = new ArgumentNullException("socket");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void ThrowIfDataHandlerIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new Peer(Substitute.For<ISocket>(), 1000, null, new Config(), Substitute.For<ILogger>());
            });
            var expected = new ArgumentNullException("dataHandler");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void DoesNotThrowIfConfigIsNull()
        {
            Assert.DoesNotThrow(() =>
            {
                _ = new Peer(Substitute.For<ISocket>(), 1000, Substitute.For<IDataHandler>(), null, Substitute.For<ILogger>());
            });
        }
        [Test]
        public void DoesNotThrowIfLoggerIsNull()
        {
            Assert.DoesNotThrow(() =>
            {
                _ = new Peer(Substitute.For<ISocket>(), 1000, Substitute.For<IDataHandler>(), new Config(), null);
            });
        }

        [Test]
        public void ThrowIfPacketSizeIsTooSmall()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _ = new Peer(Substitute.For<ISocket>(), 0, Substitute.For<IDataHandler>(), new Config(), Substitute.For<ILogger>());
            });
            var expected = new ArgumentException($"Max packet size too small for AckSystem header", "maxPacketSize");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void CloseShouldThrowIfNoActive()
        {
            LogAssert.Expect(LogType.Warning, "Peer is not active");
            peer.Close();
            LogAssert.NoUnexpectedReceived();
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
            peer.Bind(TestEndPoint.CreateSubstitute());

            var connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            socket.SetupReceiveCall(new byte[1] {
                (byte)UnityEngine.Random.Range(0, 255),
            });

            peer.UpdateTest();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void ThrowsIfSocketGivesLengthThatIsTooHigh()
        {
            peer.Bind(TestEndPoint.CreateSubstitute());

            var connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            const int aboveMTU = 5000;
            socket.SetupReceiveCall(new byte[1000], length: aboveMTU);

            var exception = Assert.Throws<IndexOutOfRangeException>(() =>
            {
                peer.UpdateTest();
            });

            Assert.That(exception, Has.Message.EqualTo($"Socket returned length above MTU. MaxPacketSize:{MAX_PACKET_SIZE} length:{aboveMTU}"));
        }

        [Test]
        [Repeat(10)]
        public void IgnoresRandomData()
        {
            peer.Bind(TestEndPoint.CreateSubstitute());

            var connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var endPoint = TestEndPoint.CreateSubstitute();

            // 2 is min length of a message
            var randomData = new byte[UnityEngine.Random.Range(2, 20)];
            for (var i = 0; i < randomData.Length; i++)
            {
                randomData[i] = (byte)UnityEngine.Random.Range(0, 255);
            }
            // make sure it is not Command, otherwise might be seen as valid packet
            // a valid packet starts with (1,1,...key...)
            // if key is wrong "invalid key" is sent back to client
            if (randomData[0] == (byte)PacketType.Command)
            {
                randomData[0] = 0;
            }

            socket.SetupReceiveCall(randomData);

            peer.UpdateTest();

            // server does nothing for invalid
            socket.DidNotReceiveWithAnyArgs().Send(default, default, default);
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        [Repeat(10)]
        public void SendsInvalidKeyForRandomKey()
        {
            peer.Bind(TestEndPoint.CreateSubstitute());

            var connectAction = Substitute.For<Action<IConnection>>();
            peer.OnConnected += connectAction;

            var endPoint = TestEndPoint.CreateSubstitute();

            // 2 is min length of a message
            var randomData = new byte[UnityEngine.Random.Range(2, 20)];
            for (var i = 0; i < randomData.Length; i++)
            {
                randomData[i] = (byte)UnityEngine.Random.Range(0, 255);
            }
            // a valid packet starts with (1,1,...key...)
            // if key is wrong "invalid key" is sent back to client
            randomData[0] = (byte)PacketType.Command;
            randomData[1] = (byte)Commands.ConnectRequest;
            if (randomData.Length >= 3)
                randomData[2] = 1; // set to 1 so tests desn't randomly pick valid key

            socket.SetupReceiveCall(randomData, endPoint);

            peer.UpdateTest();

            // server does nothing for invalid
            socket.Received(1).Send(
                endPoint: endPoint,
                length: 3,
                packet: Arg.Is<byte[]>(sent =>
                    sent[0] == (byte)PacketType.Command &&
                    sent[1] == (byte)Commands.ConnectionRejected &&
                    sent[2] == (byte)RejectReason.KeyInvalid
                ));
            connectAction.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [Test]
        public void StopsReceiveLoopIfClosedByMessageHandler()
        {
            // todo implement
            Assert.Ignore("Not Implemented");

            // if a receive handler calls close while in receive loop we should stop the loop before calling poll again
            // if we dont we will get object disposed errors
        }
    }
}
