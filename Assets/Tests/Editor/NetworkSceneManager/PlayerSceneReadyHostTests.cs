using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Host
{
    public class PlayerSceneReadyHostTests : HostSetupWithSceneManager<EmptyBehaviour>
    {
        [Test]
        public void NetworkPlayerStartsReady()
        {
            var player = new NetworkPlayer(Substitute.For<SocketLayer.IConnection>());
            Assert.That(player.SceneIsReady, Is.True);
        }

        [Test]
        public void SetClientReadyAndNotReadyTest()
        {
            var player = Substitute.For<INetworkPlayer>();
            // set ready to true, then check if methods sets it back to falase
            player.SceneIsReady = true;

            sceneManager.SetClientNotReady(player);
            Assert.That(player.SceneIsReady, Is.False);
        }

        [Test]
        public void SetAllClientsNotReady_SetsAllPlayersInGivenListToNotReady()
        {
            var players = new List<INetworkPlayer>();
            // add first ready client
            var first = Substitute.For<INetworkPlayer>();
            first.SceneIsReady = true;
            players.Add(first);

            // add second ready client
            var second = Substitute.For<INetworkPlayer>();
            second.SceneIsReady = true;
            players.Add(second);

            // add second ready client
            var thrid = Substitute.For<INetworkPlayer>();
            thrid.SceneIsReady = true;

            // set all not ready
            sceneManager.SetAllClientsNotReady(players);
            Assert.That(first.SceneIsReady, Is.False);
            Assert.That(second.SceneIsReady, Is.False);
            Assert.That(thrid.SceneIsReady, Is.True, "should not be changed because it isn't in given list");
        }

        [Test]
        public void SetAllClientsNotReady_UsesServerPlayersByDefault()
        {
            // add first ready client
            var first = Substitute.For<INetworkPlayer>();
            first.SceneIsReady = true;
            server.AddTestPlayer(first);

            // add second ready client
            var second = Substitute.For<INetworkPlayer>();
            second.SceneIsReady = true;
            server.AddTestPlayer(second);

            // set all not ready
            sceneManager.SetAllClientsNotReady(null);
            Assert.That(first.SceneIsReady, Is.False);
            Assert.That(second.SceneIsReady, Is.False);
        }
    }
}
