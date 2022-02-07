using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.Generics
{
    public class WithGenericRpc_behaviour<T> : NetworkBehaviour
    {
        public event Action<T> clientCalled;
        public event Action<T, INetworkPlayer> serverCalled;


        [ServerRpc(requireAuthority = false)]
        public void MyRpc2(T value, INetworkPlayer sender)
        {
            serverCalled?.Invoke(value, sender);
        }
        [ClientRpc]
        public void MyRpc(T value)
        {
            clientCalled?.Invoke(value);
        }
    }

    public class WithGenericRpc_behaviourInt : WithGenericRpc_behaviour<int>
    {
    }
    public class WithGenericRpc_behaviourObject : WithGenericRpc_behaviour<MyClass>
    {
    }

    public class WithGenericRpcInt : ClientServerSetup<WithGenericRpc_behaviourInt>
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
            Action<int, INetworkPlayer> sub = Substitute.For<Action<int, INetworkPlayer>>();
            serverComponent.serverCalled += sub;
            clientComponent.MyRpc2(num, default);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num, serverPlayer);
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
    public class WithGenericRpcObject : ClientServerSetup<WithGenericRpc_behaviourObject>
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
            Action<MyClass, INetworkPlayer> sub = Substitute.For<Action<MyClass, INetworkPlayer>>();
            serverComponent.serverCalled += sub;
            clientComponent.MyRpc2(new MyClass { Value = num }, default);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(Arg.Is<MyClass>(x => x.Value == num), serverPlayer);
        }

        [UnityTest]
        public IEnumerator CanCallClientRpc()
        {
            const int num = 32;
            Action<MyClass> sub = Substitute.For<Action<MyClass>>();
            clientComponent.clientCalled += sub;
            serverComponent.MyRpc(new MyClass { Value = num });

            yield return null;
            yield return null;

            sub.Received(1).Invoke(Arg.Is<MyClass>(x => x.Value == num));
        }
    }
}
