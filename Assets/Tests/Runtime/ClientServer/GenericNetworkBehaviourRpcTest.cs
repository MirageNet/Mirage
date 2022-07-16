using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class GenericBehaviourWithRpc<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        public event Action<NetworkIdentity> onSendNetworkIdentityCalled;
        public event Action<GameObject> onSendGameObjectCalled;
        public event Action<NetworkBehaviour> onSendNetworkBehaviourCalled;
        public event Action<GenericBehaviourWithRpcImplement> onSendNetworkBehaviourDerivedCalled;

        [ClientRpc]
        public void SendNetworkIdentity(NetworkIdentity value)
        {
            onSendNetworkIdentityCalled?.Invoke(value);
        }

        [ClientRpc]
        public void SendGameObject(GameObject value)
        {
            onSendGameObjectCalled?.Invoke(value);
        }

        [ClientRpc]
        public void SendNetworkBehaviour(NetworkBehaviour value)
        {
            onSendNetworkBehaviourCalled?.Invoke(value);
        }

        [ClientRpc]
        public void SendNetworkBehaviourDerived(GenericBehaviourWithRpcImplement value)
        {
            onSendNetworkBehaviourDerivedCalled?.Invoke(value);
        }

        [ServerRpc]
        public void SendNetworkIdentityToServer(NetworkIdentity value)
        {
            onSendNetworkIdentityCalled?.Invoke(value);
        }

        [ServerRpc]
        public void SendGameObjectToServer(GameObject value)
        {
            onSendGameObjectCalled?.Invoke(value);
        }

        [ServerRpc]
        public void SendNetworkBehaviourToServer(NetworkBehaviour value)
        {
            onSendNetworkBehaviourCalled?.Invoke(value);
        }

        [ServerRpc]
        public void SendNetworkBehaviourDerivedToServer(GenericBehaviourWithRpcImplement value)
        {
            onSendNetworkBehaviourDerivedCalled?.Invoke(value);
        }
    }

    public class GenericBehaviourWithRpcImplement : GenericBehaviourWithRpc<GenericBehaviourWithRpcImplement> { }

    public class GenericNetworkBehaviourRpcTests : ClientServerSetup<GenericBehaviourWithRpcImplement>
    {
        [UnityTest]
        public IEnumerator SendNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<NetworkIdentity>>();
            clientComponent.onSendNetworkIdentityCalled += callback;

            serverComponent.SendNetworkIdentity(serverIdentity);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(clientIdentity);
        });

        [UnityTest]
        public IEnumerator SendNetworkBehavior() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<NetworkBehaviour>>();
            clientComponent.onSendNetworkBehaviourCalled += callback;

            serverComponent.SendNetworkBehaviour(serverComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(clientComponent);
        });

        [UnityTest]
        public IEnumerator SendNetworkBehaviorChild() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<GenericBehaviourWithRpcImplement>>();
            clientComponent.onSendNetworkBehaviourDerivedCalled += callback;

            serverComponent.SendNetworkBehaviourDerived(serverComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(clientComponent);
        });

        [UnityTest]
        public IEnumerator SendGameObject() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<GameObject>>();
            clientComponent.onSendGameObjectCalled += callback;

            serverComponent.SendGameObject(serverPlayerGO);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(clientPlayerGO);
        });

        [Test]
        public void SendInvalidGO()
        {
            var callback = Substitute.For<Action<GameObject>>();
            clientComponent.onSendGameObjectCalled += callback;

            // this object does not have a NI, so this should error out
            Assert.Throws<InvalidOperationException>(() =>
            {
                serverComponent.SendGameObject(serverGo);
            });
        }

        [UnityTest]
        public IEnumerator SendNullNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<NetworkIdentity>>();
            clientComponent.onSendNetworkIdentityCalled += callback;

            serverComponent.SendNetworkIdentity(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendNullNetworkBehavior() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<NetworkBehaviour>>();
            clientComponent.onSendNetworkBehaviourCalled += callback;

            serverComponent.SendNetworkBehaviour(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendNullNetworkBehaviorChild() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<GenericBehaviourWithRpcImplement>>();
            clientComponent.onSendNetworkBehaviourDerivedCalled += callback;

            serverComponent.SendNetworkBehaviourDerived(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendNullGameObject() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<GameObject>>();
            clientComponent.onSendGameObjectCalled += callback;

            serverComponent.SendGameObject(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendNetworkIdentityToServer() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<NetworkIdentity>>();
            serverComponent.onSendNetworkIdentityCalled += callback;

            clientComponent.SendNetworkIdentityToServer(clientIdentity);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(serverIdentity);
        });

        [UnityTest]
        public IEnumerator SendNetworkBehaviorToServer() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<NetworkBehaviour>>();
            serverComponent.onSendNetworkBehaviourCalled += callback;

            clientComponent.SendNetworkBehaviourToServer(clientComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(serverComponent);
        });

        [UnityTest]
        public IEnumerator SendNetworkBehaviorChildToServer() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<GenericBehaviourWithRpcImplement>>();
            serverComponent.onSendNetworkBehaviourDerivedCalled += callback;

            clientComponent.SendNetworkBehaviourDerivedToServer(clientComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(serverComponent);
        });

        [UnityTest]
        public IEnumerator SendGameObjectToServer() => UniTask.ToCoroutine(async () =>
        {
            var callback = Substitute.For<Action<GameObject>>();
            serverComponent.onSendGameObjectCalled += callback;

            clientComponent.SendGameObjectToServer(clientPlayerGO);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(serverPlayerGO);
        });

        [Test]
        public void SendInvalidGOToServer()
        {
            var callback = Substitute.For<Action<GameObject>>();
            serverComponent.onSendGameObjectCalled += callback;

            // this object does not have a NI, so this should error out
            Assert.Throws<InvalidOperationException>(() =>
            {
                clientComponent.SendGameObjectToServer(clientGo);
            });
        }
    }
}
