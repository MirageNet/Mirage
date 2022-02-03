using System;
using System.Collections;
using NSubstitute;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    public class WithGenericRpc_behaviour : NetworkBehaviour
    {
        public Action<object> called;

        [ClientRpc]
        public void MyRpc<T>(T value)
        {
            called?.Invoke(value);
        }
    }

    // note, this can't be a behaviour by itself, it must have a child class in order to be used
    public class GenericWithRpc_behaviour<T> : NetworkBehaviour
    {
        public Action<T> called;

        [ClientRpc]
        public void MyRpc(T value)
        {
            called?.Invoke(value);
        }
    }
    public class GenericWithRpc_behaviourInt : GenericWithRpc_behaviour<int> { }

    public class WithGenericRpc : ClientServerSetup<WithGenericRpc_behaviour>
    {
        [UnityTest]
        public IEnumerator CanCallServerRpc()
        {
            const int num = 32;
            Action<object> sub = Substitute.For<Action<object>>();
            serverComponent.called += sub;
            clientComponent.MyRpc(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }
    public class GenericWithRpc : ClientServerSetup<GenericWithRpc_behaviourInt>
    {
        [UnityTest]
        public IEnumerator CanCallServerRpc()
        {
            const int num = 32;
            Action<int> sub = Substitute.For<Action<int>>();
            serverComponent.called += sub;
            clientComponent.MyRpc(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }
}
