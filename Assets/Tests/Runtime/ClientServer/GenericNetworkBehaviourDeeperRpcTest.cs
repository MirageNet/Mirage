using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.ClientServer
{
    public class GenericBehaviourWithRpcDeeperBase<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        public event Action<NetworkIdentity> onSendNetworkIdentityCalled;
        public event Action<GameObject> onSendGameObjectCalled;
        public event Action<NetworkBehaviour> onSendNetworkBehaviourCalled;
        public event Action<GenericBehaviourWithRpcDeeperImplement> onSendNetworkBehaviourDerivedCalled;

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
        public void SendNetworkBehaviourDerived(GenericBehaviourWithRpcDeeperImplement value)
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
        public void SendNetworkBehaviourDerivedToServer(GenericBehaviourWithRpcDeeperImplement value)
        {
            onSendNetworkBehaviourDerivedCalled?.Invoke(value);
        }
    }

    public class GenericBehaviourWithRpcDeeperMiddle<T> : GenericBehaviourWithRpcDeeperBase<T> where T : NetworkBehaviour
    {
        public event Action<NetworkIdentity> onSendDeeperNetworkIdentityCalled;
        public event Action<GameObject> onSendDeeperGameObjectCalled;
        public event Action<NetworkBehaviour> onSendDeeperNetworkBehaviourCalled;
        public event Action<GenericBehaviourWithRpcDeeperImplement> onSendDeeperNetworkBehaviourDerivedCalled;

        [ClientRpc]
        public void SendDeeperNetworkIdentity(NetworkIdentity value)
        {
            onSendDeeperNetworkIdentityCalled?.Invoke(value);
        }

        [ClientRpc]
        public void SendDeeperGameObject(GameObject value)
        {
            onSendDeeperGameObjectCalled?.Invoke(value);
        }

        [ClientRpc]
        public void SendDeeperNetworkBehaviour(NetworkBehaviour value)
        {
            onSendDeeperNetworkBehaviourCalled?.Invoke(value);
        }

        [ClientRpc]
        public void SendDeeperNetworkBehaviourDerived(GenericBehaviourWithRpcDeeperImplement value)
        {
            onSendDeeperNetworkBehaviourDerivedCalled?.Invoke(value);
        }

        [ServerRpc]
        public void SendDeeperNetworkIdentityToServer(NetworkIdentity value)
        {
            onSendDeeperNetworkIdentityCalled?.Invoke(value);
        }

        [ServerRpc]
        public void SendDeeperGameObjectToServer(GameObject value)
        {
            onSendDeeperGameObjectCalled?.Invoke(value);
        }

        [ServerRpc]
        public void SendDeeperNetworkBehaviourToServer(NetworkBehaviour value)
        {
            onSendDeeperNetworkBehaviourCalled?.Invoke(value);
        }

        [ServerRpc]
        public void SendDeeperNetworkBehaviourDerivedToServer(GenericBehaviourWithRpcDeeperImplement value)
        {
            onSendDeeperNetworkBehaviourDerivedCalled?.Invoke(value);
        }
    }

    public class GenericBehaviourWithRpcDeeperImplement : GenericBehaviourWithRpcDeeperMiddle<GenericBehaviourWithRpcDeeperImplement> { }

    public class GenericNetworkBehaviourDeeperRpcTests : ClientServerSetup<GenericBehaviourWithRpcDeeperImplement>
    {
        [UnityTest]
        public IEnumerator SendNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> callback = Substitute.For<Action<NetworkIdentity>>();
            clientComponent.onSendNetworkIdentityCalled += callback;

            serverComponent.SendNetworkIdentity(serverIdentity);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(clientIdentity);
        });

        [UnityTest]
        public IEnumerator SendNetworkBehavior() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkBehaviour> callback = Substitute.For<Action<NetworkBehaviour>>();
            clientComponent.onSendNetworkBehaviourCalled += callback;

            serverComponent.SendNetworkBehaviour(serverComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(clientComponent);
        });

        [UnityTest]
        public IEnumerator SendNetworkBehaviorChild() => UniTask.ToCoroutine(async () =>
        {
            Action<GenericBehaviourWithRpcDeeperImplement> callback = Substitute.For<Action<GenericBehaviourWithRpcDeeperImplement>>();
            clientComponent.onSendNetworkBehaviourDerivedCalled += callback;

            serverComponent.SendNetworkBehaviourDerived(serverComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(clientComponent);
        });

        [UnityTest]
        public IEnumerator SendGameObject() => UniTask.ToCoroutine(async () =>
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            clientComponent.onSendGameObjectCalled += callback;

            serverComponent.SendGameObject(serverPlayerGO);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(clientPlayerGO);
        });

        [Test]
        public void SendInvalidGO()
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
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
            Action<NetworkIdentity> callback = Substitute.For<Action<NetworkIdentity>>();
            clientComponent.onSendNetworkIdentityCalled += callback;

            serverComponent.SendNetworkIdentity(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendNullNetworkBehavior() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkBehaviour> callback = Substitute.For<Action<NetworkBehaviour>>();
            clientComponent.onSendNetworkBehaviourCalled += callback;

            serverComponent.SendNetworkBehaviour(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendNullNetworkBehaviorChild() => UniTask.ToCoroutine(async () =>
        {
            Action<GenericBehaviourWithRpcDeeperImplement> callback = Substitute.For<Action<GenericBehaviourWithRpcDeeperImplement>>();
            clientComponent.onSendNetworkBehaviourDerivedCalled += callback;

            serverComponent.SendNetworkBehaviourDerived(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendNullGameObject() => UniTask.ToCoroutine(async () =>
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            clientComponent.onSendGameObjectCalled += callback;

            serverComponent.SendGameObject(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendNetworkIdentityToServer() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> callback = Substitute.For<Action<NetworkIdentity>>();
            serverComponent.onSendNetworkIdentityCalled += callback;

            clientComponent.SendNetworkIdentityToServer(clientIdentity);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(serverIdentity);
        });

        [UnityTest]
        public IEnumerator SendNetworkBehaviorToServer() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkBehaviour> callback = Substitute.For<Action<NetworkBehaviour>>();
            serverComponent.onSendNetworkBehaviourCalled += callback;

            clientComponent.SendNetworkBehaviourToServer(clientComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(serverComponent);
        });

        [UnityTest]
        public IEnumerator SendNetworkBehaviorChildToServer() => UniTask.ToCoroutine(async () =>
        {
            Action<GenericBehaviourWithRpcDeeperImplement> callback = Substitute.For<Action<GenericBehaviourWithRpcDeeperImplement>>();
            serverComponent.onSendNetworkBehaviourDerivedCalled += callback;

            clientComponent.SendNetworkBehaviourDerivedToServer(clientComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(serverComponent);
        });

        [UnityTest]
        public IEnumerator SendGameObjectToServer() => UniTask.ToCoroutine(async () =>
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            serverComponent.onSendGameObjectCalled += callback;

            clientComponent.SendGameObjectToServer(clientPlayerGO);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(serverPlayerGO);
        });

        [Test]
        public void SendInvalidGOToServer()
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            serverComponent.onSendGameObjectCalled += callback;

            // this object does not have a NI, so this should error out
            Assert.Throws<InvalidOperationException>(() =>
            {
                clientComponent.SendGameObjectToServer(clientGo);
            });
        }

        [UnityTest]
        public IEnumerator SendDeeperNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> callback = Substitute.For<Action<NetworkIdentity>>();
            clientComponent.onSendDeeperNetworkIdentityCalled += callback;

            serverComponent.SendDeeperNetworkIdentity(serverIdentity);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(clientIdentity);
        });

        [UnityTest]
        public IEnumerator SendDeeperNetworkBehavior() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkBehaviour> callback = Substitute.For<Action<NetworkBehaviour>>();
            clientComponent.onSendDeeperNetworkBehaviourCalled += callback;

            serverComponent.SendDeeperNetworkBehaviour(serverComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(clientComponent);
        });

        [UnityTest]
        public IEnumerator SendDeeperNetworkBehaviorChild() => UniTask.ToCoroutine(async () =>
        {
            Action<GenericBehaviourWithRpcDeeperImplement> callback = Substitute.For<Action<GenericBehaviourWithRpcDeeperImplement>>();
            clientComponent.onSendDeeperNetworkBehaviourDerivedCalled += callback;

            serverComponent.SendDeeperNetworkBehaviourDerived(serverComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(clientComponent);
        });

        [UnityTest]
        public IEnumerator SendDeeperGameObject() => UniTask.ToCoroutine(async () =>
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            clientComponent.onSendDeeperGameObjectCalled += callback;

            serverComponent.SendDeeperGameObject(serverPlayerGO);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(clientPlayerGO);
        });

        [Test]
        public void SendDeeperInvalidGO()
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            clientComponent.onSendDeeperGameObjectCalled += callback;

            // this object does not have a NI, so this should error out
            Assert.Throws<InvalidOperationException>(() =>
            {
                serverComponent.SendDeeperGameObject(serverGo);
            });
        }

        [UnityTest]
        public IEnumerator SendDeeperNullNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> callback = Substitute.For<Action<NetworkIdentity>>();
            clientComponent.onSendDeeperNetworkIdentityCalled += callback;

            serverComponent.SendDeeperNetworkIdentity(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendDeeperNullNetworkBehavior() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkBehaviour> callback = Substitute.For<Action<NetworkBehaviour>>();
            clientComponent.onSendDeeperNetworkBehaviourCalled += callback;

            serverComponent.SendDeeperNetworkBehaviour(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendDeeperNullNetworkBehaviorChild() => UniTask.ToCoroutine(async () =>
        {
            Action<GenericBehaviourWithRpcDeeperImplement> callback = Substitute.For<Action<GenericBehaviourWithRpcDeeperImplement>>();
            clientComponent.onSendDeeperNetworkBehaviourDerivedCalled += callback;

            serverComponent.SendDeeperNetworkBehaviourDerived(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendDeeperNullGameObject() => UniTask.ToCoroutine(async () =>
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            clientComponent.onSendDeeperGameObjectCalled += callback;

            serverComponent.SendDeeperGameObject(null);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(null);
        });

        [UnityTest]
        public IEnumerator SendDeeperNetworkIdentityToServer() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> callback = Substitute.For<Action<NetworkIdentity>>();
            serverComponent.onSendDeeperNetworkIdentityCalled += callback;

            clientComponent.SendDeeperNetworkIdentityToServer(clientIdentity);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any());
            callback.Received().Invoke(serverIdentity);
        });

        [UnityTest]
        public IEnumerator SendDeeperNetworkBehaviorToServer() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkBehaviour> callback = Substitute.For<Action<NetworkBehaviour>>();
            serverComponent.onSendDeeperNetworkBehaviourCalled += callback;

            clientComponent.SendDeeperNetworkBehaviourToServer(clientComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2));
            callback.Received().Invoke(serverComponent);
        });

        [UnityTest]
        public IEnumerator SendDeeperNetworkBehaviorChildToServer() => UniTask.ToCoroutine(async () =>
        {
            Action<GenericBehaviourWithRpcDeeperImplement> callback = Substitute.For<Action<GenericBehaviourWithRpcDeeperImplement>>();
            serverComponent.onSendDeeperNetworkBehaviourDerivedCalled += callback;

            clientComponent.SendDeeperNetworkBehaviourDerivedToServer(clientComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(serverComponent);
        });

        [UnityTest]
        public IEnumerator SendDeeperGameObjectToServer() => UniTask.ToCoroutine(async () =>
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            serverComponent.onSendDeeperGameObjectCalled += callback;

            clientComponent.SendDeeperGameObjectToServer(clientPlayerGO);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2)); ;
            callback.Received().Invoke(serverPlayerGO);
        });

        [Test]
        public void SendDeeperInvalidGOToServer()
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            serverComponent.onSendDeeperGameObjectCalled += callback;

            // this object does not have a NI, so this should error out
            Assert.Throws<InvalidOperationException>(() =>
            {
                clientComponent.SendDeeperGameObjectToServer(clientGo);
            });
        }
    }
}
