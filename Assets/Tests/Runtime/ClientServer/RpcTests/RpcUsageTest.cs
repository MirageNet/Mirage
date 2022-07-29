using System;
using System.Collections;
using NSubstitute;
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

    public class RpcUsageTest : ClientServerSetup<RpcUsageBehaviours>
    {
        [UnityTest]
        public IEnumerator CallPlayerOnlyClientRpc()
        {
            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.PlayerClientRpcCalled += sub;

            serverComponent.PlayerTest(clientPlayer, num);

            yield return null;

        }

        [UnityTest]
        public IEnumerator CallOwnerOnlyClientRpc()
        {
            const short num = short.MaxValue;

            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.OwnerRpcCalled += sub;

            serverComponent.OwnerTest(num, num);

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

            sub.Received(1).Invoke(num);

        }
    }
}
