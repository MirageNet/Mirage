using System;
using System.Collections;
using NSubstitute;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    public class CallToNonRpcBase_behaviour : CallToNonRpcBase_base
    {
        public event Action<int> clientRpcCalled;
        public event Action<int> serverRpcCalled;

        [ClientRpc]
        public override void MyRpc(int arg1)
        {
            clientRpcCalled?.Invoke(arg1);

            // should call normal base method, no swapping to rpc (that doesn't exist)
            base.MyRpc(arg1);
        }

        [ServerRpc(requireAuthority = false)]
        public void MyRpc(int arg1, INetworkPlayer sender)
        {
            serverRpcCalled?.Invoke(arg1);

            // should call normal base method, no swapping to rpc (that doesn't exist)
            base.MyRpc(arg1);
        }
    }

    public class CallToNonRpcBase_base : NetworkBehaviour
    {
        public event Action<int> baseCalled;

        // not an rpc, override is, so it should just be called normally on receiver
        public virtual void MyRpc(int arg1)
        {
            baseCalled?.Invoke(arg1);
        }
    }

    public class CallToNonRpcBase : ClientServerSetup<CallToNonRpcBase_behaviour>
    {

        [UnityTest]
        public IEnumerator CanCallServerRpc()
        {
            const int num = 32;
            var sub = Substitute.For<Action<int>>();
            var baseSub = Substitute.For<Action<int>>();
            serverComponent.serverRpcCalled += sub;
            serverComponent.baseCalled += baseSub;
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
            var sub = Substitute.For<Action<int>>();
            var baseSub = Substitute.For<Action<int>>();
            clientComponent.clientRpcCalled += sub;
            clientComponent.baseCalled += baseSub;
            serverComponent.MyRpc(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
            baseSub.Received(1).Invoke(num);
        }
    }
}
