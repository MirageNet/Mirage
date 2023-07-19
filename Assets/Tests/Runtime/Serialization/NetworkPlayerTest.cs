using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime
{
    public class NetworkPlayerTestBase : TestBase
    {
        protected NetworkPlayer player;
        protected SocketLayer.IConnection connection;

        [SetUp]
        public virtual void SetUp()
        {
            connection = Substitute.For<SocketLayer.IConnection>();
            player = new NetworkPlayer(connection, false);
        }

        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }
    }

    public class NetworkPlayerCharactorTest : NetworkPlayerTestBase
    {
        [Test]
        public void EventCalledWhenIdentityChanged()
        {
            var character = CreateNetworkIdentity();

            var action = Substitute.For<Action<NetworkIdentity>>();
            player.OnIdentityChanged += action;
            player.Identity = character;

            action.Received(1).Invoke(character);
            action.ClearReceivedCalls();

            player.Identity = null;
            action.Received(1).Invoke(null);
        }

        [Test]
        public void EventNotCalledWhenIdentityIsSame()
        {
            var character = CreateNetworkIdentity();

            var action = Substitute.For<Action<NetworkIdentity>>();
            player.OnIdentityChanged += action;
            player.Identity = character;
            action.ClearReceivedCalls();

            // set to same value
            player.Identity = character;
            action.DidNotReceive().Invoke(Arg.Any<NetworkIdentity>());
        }

        [Test]
        public void HasCharacterReturnsFalseIfIdentityIsSet()
        {
            Debug.Assert(player.Identity == null, "player had an identity, this test is invalid");
            Assert.That(player.HasCharacter, Is.False);
        }

        [Test]
        public void HasCharacterReturnsTrueIfIdentityIsSet()
        {
            var character = CreateNetworkIdentity();

            player.Identity = character;

            Debug.Assert(player.Identity != null, "player did not have identity, this test is invalid");
            Assert.That(player.HasCharacter, Is.True);
        }
    }

    public class NetworkPlayerMessageSendingTest : NetworkPlayerTestBase
    {
        [Test]
        [TestCase(Channel.Reliable)]
        [TestCase(Channel.Unreliable)]
        public void SendCallsSendOnConnection(Channel channel)
        {
            var messageBytes = new byte[] { 0, 1, 2, 3, 4 };
            const int offset = 1;
            const int length = 3;

            var message = new ArraySegment<byte>(messageBytes, offset, length);


            // set up DO assert
            if (channel == Channel.Reliable)
            {
                connection
                    .When(x => x.SendReliable(Arg.Any<byte[]>(), Arg.Any<int>(), length))
                    .Do(AssertIsSame(message));
            }
            else if (channel == Channel.Unreliable)
            {
                connection
                    .When(x => x.SendUnreliable(Arg.Any<byte[]>(), Arg.Any<int>(), length))
                    .Do(AssertIsSame(message));
            }

            // send the message via player
            player.Send(message, channel);

            // expect to see received
            if (channel == Channel.Reliable)
            {
                connection.Received(1).SendReliable(Arg.Any<byte[]>(), Arg.Any<int>(), length);
            }
            else if (channel == Channel.Unreliable)
            {
                connection.Received(1).SendUnreliable(Arg.Any<byte[]>(), Arg.Any<int>(), length);
            }

            static Action<NSubstitute.Core.CallInfo> AssertIsSame(ArraySegment<byte> message)
            {
                return (callInfo) =>
                {
                    var arrayArg = callInfo.ArgAt<byte[]>(0);
                    var offsetArg = callInfo.ArgAt<int>(1);
                    var lengthArg = callInfo.ArgAt<int>(2);

                    var seg = new ArraySegment<byte>(arrayArg, offsetArg, lengthArg);
                    Assert.That(seg.SequenceEqual(message));
                };
            }
        }

        [Test]
        public void DisconnectCallsDisconnectOnConnection()
        {
            player.Disconnect();
            connection.Received(1).Disconnect();
        }

        [Test]
        public void DisconnectStopsMessagesBeingSentToConnection()
        {
            player.Disconnect();
            player.Send(new ArraySegment<byte>(new byte[] { 0, 1, 2 }));
            connection.DidNotReceive().SendReliable(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
            connection.DidNotReceive().SendUnreliable(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
        }
        [Test]
        public void MarkAsDisconnectedStopsMessagesBeingSentToConnection()
        {
            player.MarkAsDisconnected();
            player.Send(new ArraySegment<byte>(new byte[] { 0, 1, 2 }));
            connection.DidNotReceive().SendReliable(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
            connection.DidNotReceive().SendUnreliable(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
        }
    }
}
