using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using static Mirage.Tests.LocalConnections;

namespace Mirage.Tests.Runtime
{
    public class NetworkIdentityCallbackTests : ClientServerSetup<MockComponent>
    {
        #region test components
        private class RebuildEmptyObserversNetworkBehaviour : NetworkVisibility
        {
            public override bool OnCheckObserver(INetworkPlayer player) { return true; }
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize) { }
        }


        #endregion

        private GameObject gameObject;
        private NetworkIdentity identity;
        private INetworkPlayer player1;
        private INetworkPlayer player2;

        protected override async UniTask ExtraSetup()
        {
            await base.ExtraSetup();
            gameObject = CreateGameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;

            player1 = Substitute.For<INetworkPlayer>();
            player2 = Substitute.For<INetworkPlayer>();
        }

        [Test]
        public void AlwaysVisibleAddsAllReadyPlayers()
        {
            player1.SceneIsReady.Returns(true);
            player2.SceneIsReady.Returns(false);

            // add some server connections
            server.AddTestPlayer(player1);
            server.AddTestPlayer(player2);

            // add a host connection
            server.AddLocalConnection(client, Substitute.For<SocketLayer.IConnection>());
            server.Connected.Invoke(server.LocalPlayer);
            server.LocalPlayer.SceneIsReady = true;

            // call OnStartServer so that observers dict is created
            identity.StartServer();

            var alwaysVisible = new AlwaysVisible(serverObjectManager);

            // add all to observers. should have the two ready connections then.
            var newObservers = new HashSet<INetworkPlayer>();
            alwaysVisible.OnRebuildObservers(newObservers, true);

            Assert.That(newObservers, Is.EquivalentTo(new[] { player1, server.LocalPlayer, serverPlayer }));
        }

        [Test]
        public void RebuildObserversShouldAddOwner()
        {
            // add at least one observers component, otherwise it will just add
            // all server connections
            gameObject.AddComponent<RebuildEmptyObserversNetworkBehaviour>();

            // add own player connection
            (var serverPlayer, var _) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
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
