using System;
using System.Collections;
using NSubstitute;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    public class RpcOverload_behaviour : NetworkBehaviour
    {
        public event Action<int> serverRpcCalled;
        public event Action<int> serverRpcOptionCalled;
        public event Action<int> clientRpcCalled;
        public event Action<int> clientRpcOptionCalled;

        [ServerRpc(requireAuthority = false)]
        public void MyRpc(int arg1, INetworkPlayer sender)
        {
            serverRpcCalled?.Invoke(arg1);
        }
        [ServerRpc(requireAuthority = false)]
        public void MyRpc(int arg1, bool option1, INetworkPlayer sender)
        {
            serverRpcOptionCalled?.Invoke(arg1);
        }

        [ClientRpc]
        public void MyRpc(int arg1)
        {
            clientRpcCalled?.Invoke(arg1);
        }
        [ClientRpc]
        public void MyRpc(int arg1, bool option1)
        {
            clientRpcOptionCalled?.Invoke(arg1);
        }
    }

    public class RpcOverload : ClientServerSetup<RpcOverload_behaviour>
    {
        [UnityTest]
        public IEnumerator CanCallServerRpc()
        {
            const int num = 32;
            var sub = Substitute.For<Action<int>>();
            serverComponent.serverRpcCalled += sub;
            clientComponent.MyRpc(num, default(INetworkPlayer));

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CanCallServerRpcOption()
        {
            const int num = 32;
            var sub = Substitute.For<Action<int>>();
            serverComponent.serverRpcOptionCalled += sub;
            clientComponent.MyRpc(num, true, default(INetworkPlayer));

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CanCallClientRpc()
        {
            const int num = 32;
            var sub = Substitute.For<Action<int>>();
            clientComponent.clientRpcCalled += sub;
            serverComponent.MyRpc(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CanCallClientRpcOption()
        {
            const int num = 32;
            var sub = Substitute.For<Action<int>>();
            clientComponent.clientRpcOptionCalled += sub;
            serverComponent.MyRpc(num, true);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }
}
