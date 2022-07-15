using System;
using System.Collections;
using NSubstitute;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    // normal and rpc method in same class
    public class CallToNonRpcOverLoad_behaviour : NetworkBehaviour
    {
        public event Action<int> clientRpcCalled;
        public event Action<int> serverRpcCalled;
        public event Action<int> overloadCalled;

        [ClientRpc(target = RpcTarget.Player)]
        public void MyRpc(INetworkPlayer player, int arg1)
        {
            clientRpcCalled?.Invoke(arg1);

            // should call overload without any problem
            MyRpc(arg1);
        }

        [ServerRpc(requireAuthority = false)]
        public void MyRpc(int arg1, INetworkPlayer sender)
        {
            serverRpcCalled?.Invoke(arg1);

            // should call base user code, not generated rpc
            MyRpc(arg1);
        }

        public void MyRpc(int arg1)
        {
            overloadCalled?.Invoke(arg1);
        }
    }

    public class CallToNonRpcOverLoad : ClientServerSetup<CallToNonRpcOverLoad_behaviour>
    {
        [UnityTest]
        public IEnumerator CanCallServerRpc()
        {
            const int num = 32;
            var clientSub = Substitute.For<Action<int>>();
            var serverSub = Substitute.For<Action<int>>();
            var overloadSub = Substitute.For<Action<int>>();
            serverComponent.clientRpcCalled += clientSub;
            serverComponent.serverRpcCalled += serverSub;
            serverComponent.overloadCalled += overloadSub;
            clientComponent.MyRpc(num, default(INetworkPlayer));

            yield return null;
            yield return null;

            clientSub.DidNotReceive().Invoke(num);
            serverSub.Received(1).Invoke(num);
            overloadSub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CanCallClientRpc()
        {
            const int num = 32;
            var clientSub = Substitute.For<Action<int>>();
            var serverSub = Substitute.For<Action<int>>();
            var overloadSub = Substitute.For<Action<int>>();
            clientComponent.clientRpcCalled += clientSub;
            clientComponent.serverRpcCalled += serverSub;
            clientComponent.overloadCalled += overloadSub;
            serverComponent.MyRpc(serverPlayer, num);

            yield return null;
            yield return null;

            clientSub.Received(1).Invoke(num);
            serverSub.DidNotReceive().Invoke(num);
            overloadSub.Received(1).Invoke(num);
        }
    }
}
