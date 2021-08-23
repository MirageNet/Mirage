using System.Collections.Generic;
using Mirage.Tests.Runtime.ClientServer;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using static Mirage.Tests.LocalConnections;
using Object = UnityEngine.Object;

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
            gameObject = new GameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;

            player1 = Substitute.For<INetworkPlayer>();
            player2 = Substitute.For<INetworkPlayer>();
        }

        [TearDown]
        public override void ExtraTearDown()
        {
            // set isServer is false. otherwise Destroy instead of
            // DestroyImmediate is called internally, giving an error in Editor
            Object.DestroyImmediate(gameObject);
        }


        [Test]
        public void AddAllReadyServerConnectionsToObservers()
        {
            player1.IsReady.Returns(true);
            player2.IsReady.Returns(false);

            // add some server connections
            server.Players.Add(player1);
            server.Players.Add(player2);

            // add a host connection
            server.AddLocalConnection(client, Substitute.For<SocketLayer.IConnection>());
            server.InvokeLocalConnected();
            server.World.LocalPlayer.IsReady = true;

            // call OnStartServer so that observers dict is created
            identity.StartServer();

            // add all to observers. should have the two ready connections then.
            identity.AddAllReadyServerConnectionsToObservers();
            Assert.That(identity.observers, Is.EquivalentTo(new[] { player1, server.World.LocalPlayer, serverPlayer }));

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
            serverPlayer.IsReady = true;
            identity.ConnectionToClient = serverPlayer;

            // call OnStartServer so that observers dict is created
            identity.StartServer();

            // rebuild should at least add own ready player
            identity.RebuildObservers(true);
            Assert.That(identity.observers, Does.Contain(identity.ConnectionToClient));
        }
    }
}
