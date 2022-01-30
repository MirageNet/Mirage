using System;
using System.Collections;
using NSubstitute;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    public class CallToRpcBase_behaviour : CallToRpcBase_base
    {
        public event Action<int> clientRpcCalled;
        public event Action<int> serverRpcCalled;

        [ClientRpc]
        public override void MyRpc(int arg1)
        {
            clientRpcCalled?.Invoke(arg1);

            // should call base user code, not generated rpc
            base.MyRpc(arg1);
        }

        [ServerRpc(requireAuthority = false)]
        public override void MyRpc(int arg1, INetworkPlayer sender)
        {
            serverRpcCalled?.Invoke(arg1);

            // should call base user code, not generated rpc
            base.MyRpc(arg1, sender);
        }
    }

    public class CallToRpcBase_base : NetworkBehaviour
    {
        public event Action<int> baseClientRpcCalled;
        public event Action<int> baseServerRpcCalled;

        [ClientRpc]
        public virtual void MyRpc(int arg1)
        {
            baseClientRpcCalled?.Invoke(arg1);
        }

        [ServerRpc(requireAuthority = false)]
        public virtual void MyRpc(int arg1, INetworkPlayer sender)
        {
            baseServerRpcCalled?.Invoke(arg1);
        }
    }

    public class CallToRpcBase : ClientServerSetup<CallToRpcBase_behaviour>
    {
        [UnityTest]
        public IEnumerator CanCallServerRpc()
        {
            const int num = 32;
            Action<int> sub = Substitute.For<Action<int>>();
            Action<int> baseSub = Substitute.For<Action<int>>();
            serverComponent.serverRpcCalled += sub;
            serverComponent.baseServerRpcCalled += baseSub;
            clientComponent.MyRpc(num, default(INetworkPlayer));

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
            baseSub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CanCallClientRpc()
        {
            const int num = 32;
            Action<int> sub = Substitute.For<Action<int>>();
            Action<int> baseSub = Substitute.For<Action<int>>();
            clientComponent.clientRpcCalled += sub;
            clientComponent.baseClientRpcCalled += baseSub;
            serverComponent.MyRpc(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
            baseSub.Received(1).Invoke(num);
        }
    }
}
