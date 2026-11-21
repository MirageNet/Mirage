using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.RpcTests
{
    public class RpcUsageBehaviour_IncludeServer : NetworkBehaviour, IRpcUsageBehaviour
    {
        public event Action<int> Called;
        public event Action<INetworkPlayer, int> CalledWithPlayer;

        [ClientRpc(includeServer = true)]
        public void RpcIncludeServer(short arg1)
        {
            Called?.Invoke(arg1);
        }

        [ClientRpc(includeServer = true)]
        public void RpcIncludeServerWithPlayer(INetworkPlayer player, short arg1)
        {
            CalledWithPlayer?.Invoke(player, arg1);
        }
    }

    // note: server only NOT invoking without includeServer is covered by RpcUsageClientServerTest_Observers
    public class RpcUsageClientServerTest_IncludeServer : RpcUsageClientServerTestBase<RpcUsageBehaviour_IncludeServer>
    {
        [UnityTest]
        public IEnumerator CalledOnServerAndAllObservers()
        {
            // ensure test is valid by checking players are in set
            var observers = ServerIdentity(0).observers;
            Debug.Assert(observers.Contains(ServerPlayer(0)));
            Debug.Assert(observers.Contains(ServerPlayer(1)));

            ServerComponent(0).RpcIncludeServer(NUM);
            _serverStub.Received(1).Invoke(NUM); // should have invoked right away

            yield return null;
            yield return null;

            // check again after messages are processed, so we know server was only invoked once
            _serverStub.Received(1).Invoke(NUM);
            _clientStub[0].Received(1).Invoke(NUM);
            _clientStub[1].Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator CalledOnServer_NoneObservering()
        {
            // ensure test is valid by checking players are in set
            var observers = ServerIdentity(0).observers;
            Debug.Assert(observers.Contains(ServerPlayer(0)));
            Debug.Assert(observers.Contains(ServerPlayer(1)));

            // remove player and check it doesn't receive it
            ServerIdentity(0).observers.Remove(ServerPlayer(1));

            // we also have to remove auth before we can remove observers
            serverObjectManager.RemoveCharacter(ServerPlayer(0), false);
            ServerIdentity(0).observers.Remove(ServerPlayer(0));

            ServerComponent(0).RpcIncludeServer(NUM);

            yield return null;
            yield return null;

            // server invoke does not depend on observers
            _serverStub.Received(1).Invoke(NUM);
            _clientStub[0].DidNotReceiveWithAnyArgs().Invoke(default);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator NoOwnerCalledOnServer()
        {
            _noOwnerBehaviourServer.RpcIncludeServer(NUM);
            _noOwnerStub_OnServer.Received(1).Invoke(NUM); // should have invoked right away

            yield return null;
            yield return null;

            _noOwnerStub_OnServer.Received(1).Invoke(NUM);
            _noOwnerStub_OnClient.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator ServerGetsNullPlayer()
        {
            var serverPlayerStub = Substitute.For<Action<INetworkPlayer, int>>();
            ServerComponent(0).CalledWithPlayer += serverPlayerStub;

            var clientPlayerStub = new Action<INetworkPlayer, int>[RemoteClientCount];
            for (var i = 0; i < RemoteClientCount; i++)
            {
                clientPlayerStub[i] = Substitute.For<Action<INetworkPlayer, int>>();
                _remoteClients[i].Get(ServerComponent(0)).CalledWithPlayer += clientPlayerStub[i];
            }

            // pass in a player to check the local invoke replaces it, server only has no local client so it should be null
            ServerComponent(0).RpcIncludeServerWithPlayer(ServerPlayer(1), NUM);
            serverPlayerStub.Received(1).Invoke(null, NUM);

            yield return null;
            yield return null;

            serverPlayerStub.Received(1).Invoke(null, NUM);
            for (var i = 0; i < RemoteClientCount; i++)
                clientPlayerStub[i].Received(1).Invoke(Client(i).Player, NUM);
        }

        [UnityTest]
        public IEnumerator ThrowsWhenCalledOnClient()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                ClientComponent(0).RpcIncludeServer(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Client RPC can only be called when server is active"));

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _clientStub[0].DidNotReceiveWithAnyArgs().Invoke(default);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator ThrowsWhenCalledUnSpawnedObject()
        {
            var unspawned = CreateBehaviour<RpcUsageBehaviour_IncludeServer>();
            unspawned.Called += _serverStub;

            yield return null;

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                unspawned.RpcIncludeServer(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Client RPC can only be called when server is active"));

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }

    public class RpcUsageHostTest_IncludeServer : RpcUsageHostTestBase<RpcUsageBehaviour_IncludeServer>
    {
        [UnityTest]
        public IEnumerator CalledOnceOnHost_AllObservering()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = hostComponent_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            hostComponent_onHost.RpcIncludeServer(NUM);
            _hostStub.Received(1).Invoke(NUM); // should have invoked right away

            yield return null;
            yield return null;

            // check again after messages are processed, so we know host was not also sent the rpc
            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator CalledOnceOnHost_RemoteOwner()
        {
            client2Component_onHost.Called += _hostStub;
            client2Component_on2.Called += _client2Stub;

            client2Component_onHost.RpcIncludeServer(NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator CalledOnceOnHost_HostNotObservering()
        {
            client2Component_onHost.Called += _hostStub;
            client2Component_on2.Called += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = client2Component_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            // remove host, server should still invoke because includeServer ignores host observer checks
            client2Component_onHost.Identity.observers.Remove(hostPlayer);

            client2Component_onHost.RpcIncludeServer(NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator CalledOnceOnHost_NoneObservering()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = hostComponent_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            // remove player and check it doesn't receive it
            // we also have to remove auth before we can remove observers
            serverObjectManager.RemoveCharacter(hostPlayer);
            hostComponent_onHost.Identity.observers.Remove(hostPlayer);
            hostComponent_onHost.Identity.observers.Remove(serverPlayer2);

            hostComponent_onHost.RpcIncludeServer(NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator HostGetsClientPlayer()
        {
            var hostPlayerStub = Substitute.For<Action<INetworkPlayer, int>>();
            var client2PlayerStub = Substitute.For<Action<INetworkPlayer, int>>();
            hostComponent_onHost.CalledWithPlayer += hostPlayerStub;
            hostComponent_on2.CalledWithPlayer += client2PlayerStub;

            // pass in a player to check the local invoke replaces it with the host's connection to the server
            hostComponent_onHost.RpcIncludeServerWithPlayer(serverPlayer2, NUM);

            yield return null;
            yield return null;

            hostPlayerStub.Received(1).Invoke(hostClientPlayer, NUM);
            client2PlayerStub.Received(1).Invoke(_remoteClients[0].Client.Player, NUM);
        }
    }
}
