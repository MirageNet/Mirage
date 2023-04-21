using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.Tests.Runtime.ClientServer.RpcTests;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host.RpcTests
{
    public abstract class RpcUsageHostTestBase<T> : HostSetup<T> where T : NetworkBehaviour
    {
        protected const short NUM = 52;

        protected Action<int> _hostStub;
        protected Action<int> _client2Stub;

        /// <summary>
        /// Player for client 2 on server
        /// </summary>
        protected INetworkPlayer serverPlayer2 => ServerPlayer(0);
        protected INetworkPlayer hostPlayer => server.LocalPlayer;

        protected T hostComponent_onHost => hostComponent;
        /// <summary>Component of player 1 character on client 2</summary>
        protected T hostComponent_on2 => _remoteClients[0].Get(hostComponent);

        protected T client2Component_onHost => ServerComponent(0);
        protected T client2Component_on2 => _remoteClients[0].Get(client2Component_onHost);

        protected override async UniTask ExtraSetup()
        {
            await base.ExtraSetup();
            _hostStub = Substitute.For<Action<int>>();
            _client2Stub = Substitute.For<Action<int>>();
        }

        protected override async UniTask LateSetup()
        {
            await base.LateSetup();
            await AddClient();

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
    }
    public class RpcUsageHostTest_Player : RpcUsageHostTestBase<RpcUsageBehaviour_Player>
    {
        [Test]
        [Description("Validate setup because it is kind of complex")]
        public void SetupDoesntError()
        {
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator OnlyCalledOnTarget_HostTareget()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

            hostComponent_onHost.RpcPlayer(hostPlayer, NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator OnlyCalledOnTarget_RemoteTareget()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

            hostComponent_onHost.RpcPlayer(serverPlayer2, NUM);

            yield return null;
            yield return null;

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.Received(1).Invoke(NUM);
        }
    }

    public class RpcUsageHostTest_Owner : RpcUsageHostTestBase<RpcUsageBehaviour_Owner>
    {
        [UnityTest]
        public IEnumerator OnlyCalledOnTarget_HostOwner()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

            hostComponent_onHost.RpcOwner(NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator OnlyCalledOnOwner_RemoteOwner()
        {
            client2Component_onHost.Called += _hostStub;
            client2Component_on2.Called += _client2Stub;

            client2Component_onHost.RpcOwner(NUM);

            yield return null;
            yield return null;

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator ErrorIfNoOwner()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

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
    }

    public class RpcUsageHostTest_Observers : RpcUsageHostTestBase<RpcUsageBehaviour_Observers>
    {
        [UnityTest]
        public IEnumerator CalledOnAllObservers_AllObservering()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = hostComponent_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            hostComponent_onHost.RpcObservers(NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator CalledOnAllObservers_HostObservering()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = hostComponent_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            // remove player and check it doesn't receive it
            hostComponent_onHost.Identity.observers.Remove(serverPlayer2);

            hostComponent_onHost.RpcObservers(NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator CalledOnAllObservers_RemoteObservering()
        {
            client2Component_onHost.Called += _hostStub;
            client2Component_on2.Called += _client2Stub;

            // ensure test is valid by checking players are in set
            var observers = client2Component_onHost.Identity.observers;
            Debug.Assert(observers.Contains(hostPlayer));
            Debug.Assert(observers.Contains(serverPlayer2));

            // remove player and check it doesn't receive it
            client2Component_onHost.Identity.observers.Remove(hostPlayer);

            client2Component_onHost.RpcObservers(NUM);

            yield return null;
            yield return null;

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.Received(1).Invoke(NUM);
        }

        [UnityTest]
        public IEnumerator CalledOnAllObservers_NoneObservering()
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

            hostComponent_onHost.RpcObservers(NUM);

            yield return null;
            yield return null;

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }

    public class RpcUsageClientServerTest_RequireAuthority : RpcUsageHostTestBase<RpcUsageBehaviour_RequireAuthority>
    {
        [UnityTest]
        public IEnumerator CalledWhenCalledWithAuthority()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

            yield return null;
            yield return null;

            hostComponent_onHost.RpcRequireAuthority(NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator ThrowsWhenCalledWithoutAuthority()
        {
            client2Component_onHost.Called += _hostStub;
            client2Component_on2.Called += _client2Stub;

            yield return null;
            yield return null;

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                // call character 2 on client host
                client2Component_onHost.RpcRequireAuthority(NUM);
            });

            Assert.That(exception, Has.Message.EqualTo("Trying to send ServerRpc for object without authority."));

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _hostStub.DidNotReceiveWithAnyArgs().Invoke(default);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }

    public class RpcUsageClientServerTest_IgnoreAuthority : RpcUsageHostTestBase<RpcUsageBehaviour_IgnoreAuthority>
    {
        [UnityTest]
        public IEnumerator CalledWhenCalledWithAuthority()
        {
            hostComponent_onHost.Called += _hostStub;
            hostComponent_on2.Called += _client2Stub;

            yield return null;
            yield return null;

            hostComponent_onHost.RpcIgnoreAuthority(NUM);

            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }

        [UnityTest]
        public IEnumerator ThrowsWhenCalledWithoutAuthority()
        {
            client2Component_onHost.Called += _hostStub;
            client2Component_on2.Called += _client2Stub;

            yield return null;
            yield return null;

            // call character 2 on client host
            client2Component_onHost.RpcIgnoreAuthority(NUM);

            // ensure that none were called, even if exception was throw
            yield return null;
            yield return null;

            _hostStub.Received(1).Invoke(NUM);
            _client2Stub.DidNotReceiveWithAnyArgs().Invoke(default);
        }
    }
}
