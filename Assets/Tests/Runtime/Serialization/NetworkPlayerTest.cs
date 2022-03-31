using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime
{
    public class NetworkPlayerTestBase
    {
        protected NetworkPlayer player;
        protected SocketLayer.IConnection connection;

        [SetUp]
        public virtual void SetUp()
        {
            connection = Substitute.For<SocketLayer.IConnection>();
            player = new NetworkPlayer(connection);
        }
    }

    public class NetworkPlayerCharactorTest : NetworkPlayerTestBase
    {
        [Test]
        public void EventCalledWhenIdentityChanged()
        {
            NetworkIdentity character = new GameObject("EventCalledWhenIdentityChanged").AddComponent<NetworkIdentity>();

            Action<NetworkIdentity> action = Substitute.For<Action<NetworkIdentity>>();
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
            NetworkIdentity character = new GameObject("EventNotCalledWhenIdentityIsSame").AddComponent<NetworkIdentity>();

            Action<NetworkIdentity> action = Substitute.For<Action<NetworkIdentity>>();
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
            NetworkIdentity character = new GameObject("HasCharacterReturnsTrueIfIdentityIsSet").AddComponent<NetworkIdentity>();

            player.Identity = character;

            Debug.Assert(player.Identity != null, "player did not have identity, this test is invalid");
            Assert.That(player.HasCharacter, Is.True);

            GameObject.Destroy(character.gameObject);
        }
    }

    public class NetworkPlayerMessageSendingTest : NetworkPlayerTestBase
    {
        [Test]
        [TestCase(Channel.Reliable)]
        [TestCase(Channel.Unreliable)]
        public void SendCallsSendOnConnection(int channel)
        {
            byte[] message = new byte[] { 0, 1, 2 };
            player.Send(new ArraySegment<byte>(message), channel);
            if (channel == Channel.Reliable)
            {
                connection.Received(1).SendReliable(Arg.Is<ArraySegment<byte>>(arg => arg.SequenceEqual(message)));
            }
            else if (channel == Channel.Unreliable)
            {
                connection.Received(1).SendUnreliable(Arg.Is<ArraySegment<byte>>(arg => arg.SequenceEqual(message)));
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
            connection.DidNotReceive().SendReliable(Arg.Any<byte[]>());
            connection.DidNotReceive().SendUnreliable(Arg.Any<byte[]>());
        }
        [Test]
        public void MarkAsDisconnectedStopsMessagesBeingSentToConnection()
        {
            player.MarkAsDisconnected();
            player.Send(new ArraySegment<byte>(new byte[] { 0, 1, 2 }));
            connection.DidNotReceive().SendReliable(Arg.Any<byte[]>());
            connection.DidNotReceive().SendUnreliable(Arg.Any<byte[]>());
        }
    }
}
