using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.Generics
{
    public class GenericWithRpc_behaviour<T> : NetworkBehaviour
    {
        public event Action<int> clientCalled;
        public event Action<int> serverCalled;

        [ServerRpc(requireAuthority = false)]
        public void MyRpc2(int value, INetworkPlayer sender)
        {
            serverCalled?.Invoke(value);
        }
        [ClientRpc]
        public void MyRpc(int value)
        {
            clientCalled?.Invoke(value);
        }
    }

    public class GenericWithRpc_behaviourInt : GenericWithRpc_behaviour<int>
    {
    }
    public class GenericWithRpc_behaviourObject : GenericWithRpc_behaviour<object>
    {
    }

    public class GenericWithRpcInt : ClientServerSetup<GenericWithRpc_behaviourInt>
    {
        [Test]
        public void DoesNotError()
        {
            // passes setup without errors
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator CanCallServerRpc()
        {
            const int num = 32;
            Action<int> sub = Substitute.For<Action<int>>();
            serverComponent.serverCalled += sub;
            clientComponent.MyRpc2(num, default);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CanCallClientRpc()
        {
            const int num = 32;
            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.clientCalled += sub;
            serverComponent.MyRpc(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }

    public class GenericWithRpcObject : ClientServerSetup<GenericWithRpc_behaviourObject>
    {
        [Test]
        public void DoesNotError()
        {
            // passes setup without errors
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator CanCallServerRpc()
        {
            const int num = 32;
            Action<int> sub = Substitute.For<Action<int>>();
            serverComponent.serverCalled += sub;
            clientComponent.MyRpc2(num, default);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CanCallClientRpc()
        {
            const int num = 32;
            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.clientCalled += sub;
            serverComponent.MyRpc(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }
}
