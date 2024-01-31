using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime
{
    public class NetworkIdentityCallbackTests : ClientServerSetup
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
        private INetworkPlayer player3;

        protected override async UniTask ExtraSetup()
        {
            await base.ExtraSetup();
            gameObject = CreateGameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;

            player1 = Substitute.For<INetworkPlayer>();
            player2 = Substitute.For<INetworkPlayer>();
            player3 = Substitute.For<INetworkPlayer>();
        }

        [Test]
        public void AlwaysVisibleAddsAllReadyPlayers()
        {
            player1.SceneIsReady.Returns(true);
            player2.SceneIsReady.Returns(false);
            player3.SceneIsReady.Returns(true);

            // add some server connections
            server.AddTestPlayer(player1);
            server.AddTestPlayer(player2);
            server.AddTestPlayer(player3, false);

            // add a host connection
            server.AddLocalConnection(client, Substitute.For<SocketLayer.IConnection>());
            server.Connected.Invoke(server.LocalPlayer);
            server.LocalPlayer.SceneIsReady = true;
            server.LocalPlayer.SetAuthentication(new Mirage.Authentication.PlayerAuthentication(null, null));

            // call OnStartServer so that observers dict is created
            identity.StartServer();

            var alwaysVisible = new AlwaysVisible(server);

            // add all to observers. should have the two ready connections then.
            var newObservers = new HashSet<INetworkPlayer>();
            alwaysVisible.OnRebuildObservers(newObservers, true);

            // player2 should be missing, because not ready
            // player3 should be missing, because not authenticated
            Assert.That(newObservers, Is.EquivalentTo(new[] { player1, server.LocalPlayer, serverPlayer }));
        }

        [Test]
        public void RebuildObserversShouldAddOwner()
        {
            // add at least one observers component, otherwise it will just add
            // all server connections
            gameObject.AddComponent<RebuildEmptyObserversNetworkBehaviour>();

            // add own player connection
            var serverPlayer = Substitute.For<INetworkPlayer>();
            serverPlayer.SceneIsReady = true;
            identity.SetOwner(serverPlayer);

            // call OnStartServer so that observers dict is created
            identity.StartServer();

            // rebuild should at least add own ready player
            identity.RebuildObservers(true);
            Assert.That(identity.observers, Does.Contain(identity.Owner));
        }
    }
}
