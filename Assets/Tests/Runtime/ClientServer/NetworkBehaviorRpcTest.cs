using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Mirror.Tests
{
    public class SampleBehaviorWithRpc : NetworkBehaviour
    {
        public event Action<NetworkIdentity> onSendNetworkIdentityCalled;
        public event Action<GameObject> onSendGameObjectCalled;
        public event Action<NetworkBehaviour> onSendNetworkBehaviourCalled;
        public event Action<SampleBehaviorWithRpc> onSendNetworkBehaviourDerivedCalled;

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
        public void SendNetworkBehaviourDerived(SampleBehaviorWithRpc value)
        {
            onSendNetworkBehaviourDerivedCalled?.Invoke(value);
        }
    }

    public class NetworkBehaviorRPCTest : ClientServerSetup<SampleBehaviorWithRpc>
    {
        [UnityTest]
        public IEnumerator RpcCanSendNetworkIdentity() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkIdentity> callback = Substitute.For<Action<NetworkIdentity>>();
            clientComponent.onSendNetworkIdentityCalled += callback;

            serverComponent.SendNetworkIdentity(serverIdentity);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Count() > 0);
            callback.Received().Invoke(clientIdentity);
        });

        [UnityTest]
        public IEnumerator RpcCanSendNetworkBehavior() => UniTask.ToCoroutine(async () =>
        {
            Action<NetworkBehaviour> callback = Substitute.For<Action<NetworkBehaviour>>();
            clientComponent.onSendNetworkBehaviourCalled += callback;

            serverComponent.SendNetworkBehaviour(serverComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Count() > 0);
            callback.Received().Invoke(clientComponent);
        });

        [UnityTest]
        public IEnumerator RpcCanSendNetworkBehaviorChild() => UniTask.ToCoroutine(async () =>
        {
            Action<SampleBehaviorWithRpc> callback = Substitute.For<Action<SampleBehaviorWithRpc>>();
            clientComponent.onSendNetworkBehaviourDerivedCalled += callback;

            serverComponent.SendNetworkBehaviour(serverComponent);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Count() > 0);
            callback.Received().Invoke(clientComponent);
        });

        [UnityTest]
        public IEnumerator RpcCanSendGameObject() => UniTask.ToCoroutine(async () =>
        {
            Action<GameObject> callback = Substitute.For<Action<GameObject>>();
            clientComponent.onSendGameObjectCalled += callback;

            serverComponent.SendGameObject(serverGo);
            await UniTask.WaitUntil(() => callback.ReceivedCalls().Count() > 0);
            callback.Received().Invoke(clientGo);
        });
    }
}
