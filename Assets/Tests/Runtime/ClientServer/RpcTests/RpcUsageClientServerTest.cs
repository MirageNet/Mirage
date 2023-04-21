using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    public interface IRpcUsageBehaviour
    {
        event Action<int> Called;
    }

    public class RpcUsageBehaviour_Player : NetworkBehaviour, IRpcUsageBehaviour
    {
        public event Action<int> Called;

        [ClientRpc(target = RpcTarget.Player)]
        public void RpcPlayer(INetworkPlayer _player, short arg1)
        {
            Called?.Invoke(arg1);
        }
    }
    public class RpcUsageBehaviour_Owner : NetworkBehaviour, IRpcUsageBehaviour
    {
        public event Action<int> Called;

        [ClientRpc(target = RpcTarget.Owner)]
        public void RpcOwner(short arg1)
        {
            Called?.Invoke(arg1);
        }
    }
    public class RpcUsageBehaviour_Observers : NetworkBehaviour, IRpcUsageBehaviour
    {
        public event Action<int> Called;
        [ClientRpc(target = RpcTarget.Observers)]
        public void RpcObservers(short arg1)
        {
            Called?.Invoke(arg1);
        }
    }
    public class RpcUsageBehaviour_RequireAuthority : NetworkBehaviour, IRpcUsageBehaviour
    {
        public event Action<int> Called;

        [ServerRpc(requireAuthority = true)]
        public void RpcRequireAuthority(short arg1)
        {
            Called?.Invoke(arg1);
        }
    }
    public class RpcUsageBehaviour_IgnoreAuthority : NetworkBehaviour, IRpcUsageBehaviour
    {
        public event Action<int> Called;

        [ServerRpc(requireAuthority = false)]
        public void RpcIgnoreAuthority(short arg1)
        {
            Called?.Invoke(arg1);
        }
    }

    public abstract class RpcUsageClientServerTestBase<T> : MultiRemoteClientSetup<T> where T : NetworkBehaviour, IRpcUsageBehaviour
    {
        protected override int RemoteClientCount => 2;
        protected const short NUM = 52;

        protected Action<int> _serverStub;
        protected Action<int>[] _clientStub;

        protected override async UniTask LateSetup()
        {
            await base.LateSetup();

            // 0 will be owned by 0th client
            var serverComp = ServerComponent(0);

            _serverStub = Substitute.For<Action<int>>();
            serverComp.Called += _serverStub;

            _clientStub = new Action<int>[RemoteClientCount];
            for (var i = 0; i < RemoteClientCount; i++)
            {
                _clientStub[i] = Substitute.For<Action<int>>();
                _remoteClients[i].Get(serverComp).Called += _clientStub[i];
            }
        }
    }

    public class RpcUsageClientServerTest_Player : RpcUsageClientServerTestBase<RpcUsageBehaviour_Player>
    {
        [UnityTest]
        public IEnumerator OnlyCalledOnTarget()
        {
            ServerComponent(0).RpcPlayer(ServerPlayer(0), NUM);

            yield return null;
            yield return null;

            _clientStub[0].Received(1).Invoke(NUM);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }

    public class RpcUsageClientServerTest_Owner : RpcUsageClientServerTestBase<RpcUsageBehaviour_Owner>
    {
        [UnityTest]
        public IEnumerator OnlyCalledOnOwner()
        {
            ServerComponent(0).RpcOwner(NUM);

            yield return null;
            yield return null;

            _clientStub[0].Received(1).Invoke(NUM);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }


        [UnityTest]
        public IEnumerator ThrowsIfNullOwner()
        {
            serverObjectManager.RemoveCharacter(ServerPlayer(0), false);

            yield return null;
            yield return null;

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                ServerComponent(0).RpcOwner(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Player target was null for Rpc"));

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _clientStub[0].DidNotReceiveWithAnyArgs().Invoke(default);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }

    public class RpcUsageClientServerTest_Observers : RpcUsageClientServerTestBase<RpcUsageBehaviour_Observers>
    {
        [UnityTest]
        public IEnumerator CalledOnAllObservers_AllObservering()
        {
            // ensure test is valid by checking players are in set
            var observers = ServerComponent(0).Identity.observers;
            Debug.Assert(observers.Contains(ServerPlayer(0)));
            Debug.Assert(observers.Contains(ServerPlayer(1)));

            ServerComponent(0).RpcObservers(NUM);

            yield return null;
            yield return null;

            _clientStub[0].Received(1).Invoke(NUM);
            _clientStub[1].Received(1).Invoke(NUM);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }


        [UnityTest]
        public IEnumerator CalledOnAllObservers_SomeObservering()
        {
            // ensure test is valid by checking players are in set
            var observers = ServerIdentity(0).observers;
            Debug.Assert(observers.Contains(ServerPlayer(0)));
            Debug.Assert(observers.Contains(ServerPlayer(1)));

            // remove player and check it doesn't receive it
            ServerIdentity(0).observers.Remove(ServerPlayer(1));

            ServerComponent(0).RpcObservers(NUM);

            yield return null;
            yield return null;

            _clientStub[0].Received(1).Invoke(NUM);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator CalledOnAllObservers_NoneObservering()
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

            ServerComponent(0).RpcObservers(NUM);

            yield return null;
            yield return null;

            _clientStub[0].DidNotReceiveWithAnyArgs().Invoke(default);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator ThrowsWhenCalledOnClient()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                ClientComponent(0).RpcObservers(NUM);
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
            var unspawned = CreateBehaviour<RpcUsageBehaviour_Observers>();
            unspawned.Called += _serverStub;

            yield return null;

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                unspawned.RpcObservers(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Client RPC can only be called when server is active"));

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }

    public class RpcUsageClientServerTest_RequireAuthority : RpcUsageClientServerTestBase<RpcUsageBehaviour_RequireAuthority>
    {
        [UnityTest]
        public IEnumerator CalledWhenCalledWithAuthority()
        {
            ClientComponent(0).RpcRequireAuthority(NUM);

            yield return null;
            yield return null;

            _clientStub[0].DidNotReceiveWithAnyArgs().Invoke(default);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator ThrowsWhenCalledWithoutAuthority()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                // call character[0] on client[1]
                _remoteClients[1].Get(ClientComponent(0)).RpcRequireAuthority(NUM);
            });

            // should be full message (see in client) because server is not active
            Assert.That(exception, Has.Message.EqualTo("Trying to send ServerRpc for object without authority. Mirage.Tests.Runtime.ClientServer.RpcTests.RpcUsageBehaviour_RequireAuthority.RpcRequireAuthority"));

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _clientStub[0].DidNotReceiveWithAnyArgs().Invoke(default);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator ThrowsWhenCalledOnServer()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                ServerComponent(0).RpcRequireAuthority(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Server RPC can only be called when client is active"));

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
            var unspawned = CreateBehaviour<RpcUsageBehaviour_RequireAuthority>();
            unspawned.Called += _serverStub;

            yield return null;

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                unspawned.RpcRequireAuthority(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Server RPC can only be called when client is active"));

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }

    public class RpcUsageClientServerTest_IgnoreAuthority : RpcUsageClientServerTestBase<RpcUsageBehaviour_IgnoreAuthority>
    {
        [UnityTest]
        public IEnumerator CalledWhenCalledWithAuthority()
        {
            ClientComponent(0).RpcIgnoreAuthority(NUM);

            yield return null;
            yield return null;

            _clientStub[0].DidNotReceiveWithAnyArgs().Invoke(default);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator CalledWhenCalledWithOutAuthority()
        {
            // call character 1 on client 2
            _remoteClients[1].Get(ClientComponent(0)).RpcIgnoreAuthority(NUM);

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _clientStub[0].DidNotReceiveWithAnyArgs().Invoke(default);
            _clientStub[1].DidNotReceiveWithAnyArgs().Invoke(default);
            _serverStub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator ThrowsWhenCalledOnServer()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                ServerComponent(0).RpcIgnoreAuthority(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Server RPC can only be called when client is active"));

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
            var unspawned = CreateBehaviour<RpcUsageBehaviour_RequireAuthority>();
            unspawned.Called += _serverStub;

            yield return null;

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                unspawned.RpcRequireAuthority(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Server RPC can only be called when client is active"));

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _serverStub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }
}
