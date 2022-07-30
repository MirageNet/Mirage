using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Tests.Runtime.ClientServer;
using Mirage.Tests.Runtime.ClientServer.RpcTests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host.RpcTests
{
    public class RpcUsageHostTest : HostSetup<RpcUsageBehaviour>
    {
        private const short NUM = 52;

        private Action<int> _hostStub;
        private Action<int> _client2Stub;

        private ClientInstance<RpcUsageBehaviour> _client2;
        /// <summary>
        /// Player for client 2 on server
        /// </summary>
        private INetworkPlayer serverPlayer2;
        private INetworkPlayer hostPlayer => server.LocalPlayer;

        /// <summary>
        /// Component of player 1 character on client 2
        /// </summary>
        private RpcUsageBehaviour hostComponent_on2;
        private RpcUsageBehaviour hostComponent_onHost => playerComponent;

        private RpcUsageBehaviour client2Component_onHost;
        private RpcUsageBehaviour client2Component_on2;

        public override void ExtraSetup()
        {
            base.ExtraSetup();
            _hostStub = Substitute.For<Action<int>>();
            _client2Stub = Substitute.For<Action<int>>();
        }

        public override async UniTask LateSetup()
        {
            _client2 = new ClientInstance<RpcUsageBehaviour>(ClientConfig, networkManagerGo.GetComponent<TestSocketFactory>());
            _client2.client.Connect("localhost");

            await AsyncUtil.WaitUntilWithTimeout(() => server.Players.Count > 1);

            // get new player
            serverPlayer2 = server.Players.Where(x => x != server.LocalPlayer).First();

            var prefab = CreateBehaviour<RpcUsageBehaviour>();
            {
                // create and register a prefab
                // DontDestroyOnLoad so that "prefab" wont be destroyed by scene loading
                // also means that NetworkScenePostProcess will skip this unspawned object
                GameObject.DontDestroyOnLoad(prefab);

                var identity = prefab.GetComponent<NetworkIdentity>();
                identity.PrefabHash = Guid.NewGuid().GetHashCode();
                _client2.clientObjectManager.RegisterPrefab(playerIdentity);
                _client2.clientObjectManager.RegisterPrefab(identity);
            }

            // wait for client and server to initialize themselves
            await UniTask.Yield();

            {
                // create a player object in the server
                var go = GameObject.Instantiate(prefab);
                go.name = "player 2 (server)";
                var identity = go.GetComponent<NetworkIdentity>();
                client2Component_onHost = go.GetComponent<RpcUsageBehaviour>();
                serverObjectManager.AddCharacter(serverPlayer2, identity);
            }
            // wait for client to spawn it
            await AsyncUtil.WaitUntilWithTimeout(() => _client2.client.Player.HasCharacter);

            _client2.SetupCharacter();
            client2Component_on2 = _client2.component;


            var found = _client2.client.World.TryGetIdentity(playerComponent.NetId, out var player1Character);
            if (!found)
                Debug.LogError("Could not find instance of player 1's character on client 2");
            hostComponent_on2 = player1Character.GetComponent<RpcUsageBehaviour>();

            Debug.Assert(hostComponent_on2 != null);
            Debug.Assert(client2Component_onHost != null);
            Debug.Assert(client2Component_on2 != null);

            Debug.Assert(hostComponent_on2 != hostComponent_onHost);
            Debug.Assert(hostComponent_on2.NetId == hostComponent_onHost.NetId);

            Debug.Assert(client2Component_on2 != client2Component_onHost);
            Debug.Assert(client2Component_on2.NetId == client2Component_onHost.NetId);

            Debug.Assert(client2Component_onHost.Owner == serverPlayer2);
            Debug.Assert(hostComponent_onHost.Owner == server.LocalPlayer);
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

        [Test]
        [Description("Validate setup because it is kind of complex")]
        public void SetupDoesntError()
        {
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator Player_OnlyCalledOnTarget_HostTareget()
        {
            hostComponent_onHost.PlayerCalled += _hostStub;
            hostComponent_on2.PlayerCalled += _client2Stub;

            hostComponent_onHost.RpcPlayer(hostPlayer, NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator Player_OnlyCalledOnTarget_RemoteTareget()
        {
            hostComponent_onHost.PlayerCalled += _hostStub;
            hostComponent_on2.PlayerCalled += _client2Stub;

            hostComponent_onHost.RpcPlayer(serverPlayer2, NUM);

            yield return null;
            yield return null;

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator Owner_OnlyCalledOnTarget_HostOwner()
        {
            hostComponent_onHost.PlayerCalled += _hostStub;
            hostComponent_on2.PlayerCalled += _client2Stub;

            hostComponent_onHost.RpcOwner(NUM);

            yield return null;
            yield return null;

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator Owner_OnlyCalledOnOwner_RemoteOwner()
        {
            client2Component_onHost.PlayerCalled += _hostStub;
            client2Component_on2.PlayerCalled += _client2Stub;

            client2Component_onHost.RpcOwner(NUM);

            yield return null;
            yield return null;

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator Owner_ErrorIfNoOwner()
        {
            hostComponent_onHost.PlayerCalled += _hostStub;
            hostComponent_on2.PlayerCalled += _client2Stub;

            serverObjectManager.RemoveCharacter(hostPlayer);

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                hostComponent_onHost.RpcOwner(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Player target was null for Rpc"));

            yield return null;
            yield return null;

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }


        [UnityTest]
        public IEnumerator Observers_CalledOnAllObservers_AllObservering()
        {
            hostComponent_onHost.PlayerCalled += _hostStub;
            hostComponent_on2.PlayerCalled += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = hostComponent_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            hostComponent_onHost.RpcObservers(NUM);

            yield return new WaitForSeconds(1);

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator Observers_CalledOnAllObservers_HostObservering()
        {
            hostComponent_onHost.PlayerCalled += _hostStub;
            hostComponent_on2.PlayerCalled += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = hostComponent_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            // remove player and check it doesn't receive it
            hostComponent_onHost.Identity.observers.Remove(serverPlayer2);

            hostComponent_onHost.RpcObservers(NUM);

            yield return new WaitForSeconds(1);

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator Observers_CalledOnAllObservers_RemoteObservering()
        {
            client2Component_onHost.PlayerCalled += _hostStub;
            client2Component_on2.PlayerCalled += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = client2Component_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            // remove player and check it doesn't receive it
            client2Component_onHost.Identity.observers.Remove(hostPlayer);

            client2Component_onHost.RpcObservers(NUM);

            yield return new WaitForSeconds(1);

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator Observers_CalledOnAllObservers_NoneObservering()
        {
            hostComponent_onHost.PlayerCalled += _hostStub;
            hostComponent_on2.PlayerCalled += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = hostComponent_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            // remove player and check it doesn't receive it
            hostComponent_onHost.Identity.observers.Remove(serverPlayer2);

            // we also have to remove auth before we can remove observers
            hostComponent_onHost.Identity.RemoveClientAuthority();
            hostComponent_onHost.Identity.observers.Remove(hostPlayer);
            hostComponent_onHost.Identity.observers.Remove(serverPlayer2);

            hostComponent_onHost.RpcObservers(NUM);

            yield return new WaitForSeconds(1);

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }
}
