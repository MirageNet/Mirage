using System.Collections.Generic;
using Mirage.Tests.Runtime.ClientServer;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using static Mirage.Tests.LocalConnections;

namespace Mirage.Tests.Runtime
{
    public class NetworkIdentityCallbackTests : ClientServerSetup<MockComponent>
    {
        #region test components
        class RebuildEmptyObserversNetworkBehaviour : NetworkVisibility
        {
            public override bool OnCheckObserver(INetworkPlayer player) { return true; }
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize) { }
        }


        #endregion

        GameObject gameObject;
        NetworkIdentity identity;

        INetworkPlayer player1;
        INetworkPlayer player2;

        [SetUp]
        public override void ExtraSetup()
        {
            gameObject = CreateGameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;

            player1 = Substitute.For<INetworkPlayer>();
            player2 = Substitute.For<INetworkPlayer>();
        }


        [Test]
        public void AddAllReadyServerConnectionsToObservers()
        {
            player1.SceneIsReady.Returns(true);
            player2.SceneIsReady.Returns(false);

            // add some server connections
            server.AddTestPlayer(player1);
            server.AddTestPlayer(player2);

            // add a host connection
            server.AddLocalConnection(client, Substitute.For<SocketLayer.IConnection>());
            server.InvokeLocalConnected();
            server.LocalPlayer.SceneIsReady = true;

            // call OnStartServer so that observers dict is created
            identity.StartServer();

            // add all to observers. should have the two ready connections then.
            identity.AddAllReadyServerConnectionsToObservers();
            Assert.That(identity.observers, Is.EquivalentTo(new[] { player1, server.LocalPlayer, serverPlayer }));

            // clean up
            server.Stop();
        }

        // RebuildObservers should always add the own ready connection
        // (if any). fixes https://github.com/vis2k/Mirror/issues/692
        [Test]
        public void RebuildObserversAddsOwnReadyPlayer()
        {
            // add at least one observers component, otherwise it will just add
            // all server connections
            gameObject.AddComponent<RebuildEmptyObserversNetworkBehaviour>();

            // add own player connection
            (NetworkPlayer serverPlayer, NetworkPlayer _) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
            serverPlayer.SceneIsReady = true;
            identity.Owner = serverPlayer;

            // call OnStartServer so that observers dict is created
            identity.StartServer();

            // rebuild should at least add own ready player
            identity.RebuildObservers(true);
            Assert.That(identity.observers, Does.Contain(identity.Owner));
        }
    }
}
