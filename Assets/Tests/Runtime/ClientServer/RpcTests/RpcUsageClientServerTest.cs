using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    public class RpcUsageBehaviour_Player : NetworkBehaviour
    {
        public event Action<int> PlayerCalled;
        [ClientRpc(target = RpcTarget.Player)]
        public void RpcPlayer(INetworkPlayer _player, short arg1)
        {
            PlayerCalled?.Invoke(arg1);
        }
    }
    public class RpcUsageBehaviour_Owner : NetworkBehaviour
    {

        public event Action<int> OwnerCalled;
        [ClientRpc(target = RpcTarget.Owner)]
        public void RpcOwner(short arg1)
        {
            OwnerCalled?.Invoke(arg1);
        }
    }
    public class RpcUsageBehaviour_Observers : NetworkBehaviour
    {
        public event Action<int> ObserversCalled;
        [ClientRpc(target = RpcTarget.Observers)]
        public void RpcObservers(short arg1)
        {
            ObserversCalled?.Invoke(arg1);
        }
    }

    public abstract class RpcUsageClientServerTestBase<T> : ClientServerSetup<T> where T : NetworkBehaviour
    {
        protected const short NUM = 52;

        protected Action<int> _client1Stub;
        protected Action<int> _client2Stub;
        protected Action<int> _serverStub;

        protected ClientInstance<T> _client2;
        /// <summary>
        /// Player for client 2 on server
        /// </summary>
        protected INetworkPlayer serverPlayer2;

        /// <summary>
        /// Component of player 1 character on client 2
        /// </summary>
        protected T clientComponent_on2;

        public override void ExtraSetup()
        {
            base.ExtraSetup();
            _client1Stub = Substitute.For<Action<int>>();
            _client2Stub = Substitute.For<Action<int>>();
            _serverStub = Substitute.For<Action<int>>();
        }

        public override async UniTask LateSetup()
        {
            _client2 = new ClientInstance<T>(ClientConfig, _server.socketFactory);
            _client2.clientObjectManager.RegisterPrefab(playerPrefab.GetNetworkIdentity());
            _client2.client.Connect("localhost");

            await AsyncUtil.WaitUntilWithTimeout(() => server.Players.Count > 1);

            // get new player
            serverPlayer2 = server.Players.Where(x => x != serverPlayer).First();

            // create a player object in the server
            var go = InstantiateForTest(playerPrefab);
            go.name = "player 2 (server)";
            var identity = go.GetComponent<NetworkIdentity>();
            var component = go.GetComponent<T>();
            serverObjectManager.AddCharacter(serverPlayer2, identity);


            // wait for client to spawn it
            await AsyncUtil.WaitUntilWithTimeout(() => _client2.client.Player.HasCharacter);

            _client2.SetupCharacter();

            var found = _client2.client.World.TryGetIdentity(serverComponent.NetId, out var player1Character);
            if (!found)
                Debug.LogError("Could not find instance of player 1's character on client 2");
            clientComponent_on2 = player1Character.GetComponent<T>();
        }

        public override void ExtraTearDown()
        {
            var toDestroy = _client2.client.World.SpawnedIdentities.ToArray();

            if (_client2.client.Active) _client2.client.Disconnect();
            GameObject.Destroy(_client2.go);

            foreach (var obj in toDestroy)
            {
                GameObject.Destroy(obj);
            }
        }
    }

    public class RpcUsageClientServerTest_Player : RpcUsageClientServerTestBase<RpcUsageBehaviour_Player>
    {
        [UnityTest]
        public IEnumerator OnlyCalledOnTarget()
        {
            clientComponent.PlayerCalled += _client1Stub;
            clientComponent_on2.PlayerCalled += _client2Stub;
            serverComponent.PlayerCalled += _serverStub;

            serverComponent.RpcPlayer(serverPlayer, NUM);

            yield return null;
            yield return null;

            _client1Stub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }

    public class RpcUsageClientServerTest_Owner : RpcUsageClientServerTestBase<RpcUsageBehaviour_Owner>
    {
        [UnityTest]
        public IEnumerator OnlyCalledOnOwner()
        {
            clientComponent.OwnerCalled += _client1Stub;
            clientComponent_on2.OwnerCalled += _client2Stub;
            serverComponent.OwnerCalled += _serverStub;

            serverComponent.RpcOwner(NUM);

            yield return null;
            yield return null;

            _client1Stub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }


        [UnityTest]
        public IEnumerator ThrowsIfNullOwner()
        {
            clientComponent.OwnerCalled += _client1Stub;
            clientComponent_on2.OwnerCalled += _client2Stub;
            serverComponent.OwnerCalled += _serverStub;

            serverObjectManager.RemoveCharacter(serverPlayer, false);

            yield return null;
            yield return null;

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                serverComponent.RpcOwner(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Player target was null for Rpc"));

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _client1Stub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }

    public class RpcUsageClientServerTest_Observers : RpcUsageClientServerTestBase<RpcUsageBehaviour_Observers>
    {
        [UnityTest]
        public IEnumerator CalledOnAllObservers_AllObservering()
        {
            clientComponent.ObserversCalled += _client1Stub;
            clientComponent_on2.ObserversCalled += _client2Stub;
            serverComponent.ObserversCalled += _serverStub;

            // ensure test is valid by checking players are in set
            var observers = serverComponent.Identity.observers;
            Debug.Assert(observers.Contains(serverPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            serverComponent.RpcObservers(NUM);

            yield return null;
            yield return null;

            _client1Stub.Received(1).Invoke(NUM);
            _client2Stub.Received(1).Invoke(NUM);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }


        [UnityTest]
        public IEnumerator CalledOnAllObservers_SomeObservering()
        {
            clientComponent.ObserversCalled += _client1Stub;
            clientComponent_on2.ObserversCalled += _client2Stub;
            serverComponent.ObserversCalled += _serverStub;

            // ensure test is valid by checking players are in set
            var observers = serverComponent.Identity.observers;
            Debug.Assert(observers.Contains(serverPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            // remove player and check it doesn't receive it
            serverComponent.Identity.observers.Remove(serverPlayer2);

            serverComponent.RpcObservers(NUM);

            yield return null;
            yield return null;

            _client1Stub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator CalledOnAllObservers_NoneObservering()
        {
            clientComponent.ObserversCalled += _client1Stub;
            clientComponent_on2.ObserversCalled += _client2Stub;
            serverComponent.ObserversCalled += _serverStub;

            // ensure test is valid by checking players are in set
            var observers = serverComponent.Identity.observers;
            Debug.Assert(observers.Contains(serverPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            // remove player and check it doesn't receive it
            serverComponent.Identity.observers.Remove(serverPlayer2);

            // we also have to remove auth before we can remove observers
            serverObjectManager.RemoveCharacter(serverPlayer, false);
            serverComponent.Identity.observers.Remove(serverPlayer);

            serverComponent.RpcObservers(NUM);

            yield return null;
            yield return null;

            _client1Stub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }
}
