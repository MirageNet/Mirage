using System;
using System.Collections;
using Mirage.Tests.Runtime.Host;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    public class RpcUsageBehaviours : NetworkBehaviour
    {
        public event Action<int> PlayerClientRpcCalled;
        public event Action<int> OwnerRpcCalled;
        public event Action<int> DefaultRpcCalled;

        [ClientRpc(target = RpcTarget.Player)]
        public void PlayerTest(INetworkPlayer player, short arg1)
        {
            PlayerClientRpcCalled?.Invoke(arg1);
        }

        [ClientRpc(target = RpcTarget.Owner)]
        public void OwnerTest(short arg1, int index)
        {
            OwnerRpcCalled?.Invoke(arg1);
        }

        [ClientRpc]
        public void DefaultTest(short arg1)
        {
            DefaultRpcCalled?.Invoke(arg1);
        }
    }

    public class HostRpcUsageTest : HostSetup<RpcUsageBehaviours>
    {
        [UnityTest]
        public IEnumerator SceneObjectCallPlayerOnlyClientRpc()
        {
            var test = new GameObject("Scene Object", typeof(NetworkIdentity));

            clientObjectManager.RegisterPrefab(test.GetNetworkIdentity());

            test.AddComponent<RpcUsageBehaviours>();

            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            test.PlayerClientRpcCalled += sub;

            test.PlayerTest(client.Player, num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CallPlayerOnlyClientRpc()
        {
            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            component.PlayerClientRpcCalled += sub;

            component.PlayerTest(client.Player, num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CallOwnerOnlyClientRpc()
        {
            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            component.OwnerRpcCalled += sub;

            component.OwnerTest(num, num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CallDefaultClientRpc()
        {
            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            component.DefaultRpcCalled += sub;

            component.DefaultTest(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }

    public class ClientServerRpcUsageTest : ClientServerSetup<RpcUsageBehaviours>
    {
        [UnityTest]
        public IEnumerator ServerCallPlayerOnlyClientRpc()
        {
            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            serverComponent.PlayerClientRpcCalled += sub;

            serverComponent.PlayerTest(serverComponent.Owner, num);

            yield return null;
            yield return null;

            sub.Received(0).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CallPlayerOnlyClientRpc()
        {
            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.PlayerClientRpcCalled += sub;

            serverComponent.PlayerTest(clientComponent.Owner, num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CallOwnerOnlyClientRpc()
        {
            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.OwnerRpcCalled += sub;

            serverComponent.OwnerTest(num, num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CallDefaultClientRpc()
        {
            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.DefaultRpcCalled += sub;

            serverComponent.DefaultTest(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }
}
