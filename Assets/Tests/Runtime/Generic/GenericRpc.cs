using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
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

        public int GetInt() => 0;
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
            // nothing
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
            // nothing
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

namespace Mirage.Tests.Runtime.ClientServer.SyncVars
{
    // note, this can't be a behaviour by itself, it must have a child class in order to be used
    public class GenericWithSyncVarBase_behaviour<T> : NetworkBehaviour
    {
        [SyncVar] public int baseValue;

    }
    public class GenericWithSyncVar_behaviour<T> : GenericWithSyncVarBase_behaviour<T>
    {
        [SyncVar] public int value;

        public int GetInt() => 0;
    }

    public class GenericWithSyncVar_behaviourInt : GenericWithSyncVar_behaviour<int>
    {
        [SyncVar] public int moreValue;
    }
    public class GenericWithSyncVar_behaviourObject : GenericWithSyncVar_behaviour<object>
    {
        [SyncVar] public int moreValue;
    }

    public class GenericWithSyncVarInt : ClientServerSetup<GenericWithSyncVar_behaviourInt>
    {
        [Test]
        public void DoesNotError()
        {
            // nothing
        }

        [UnityTest]
        public IEnumerator SyncToClient()
        {
            const int num1 = 11;
            const int num2 = 12;
            const int num3 = 13;
            Action<int> sub = Substitute.For<Action<int>>();
            serverComponent.baseValue = num1;
            serverComponent.value = num2;
            serverComponent.moreValue = num3;

            yield return null;
            yield return null;

            Assert.That(clientComponent.baseValue, Is.EqualTo(num1));
            Assert.That(clientComponent.value, Is.EqualTo(num2));
            Assert.That(clientComponent.moreValue, Is.EqualTo(num3));
        }
    }
    public class GenericWithSyncVarObject : ClientServerSetup<GenericWithSyncVar_behaviourObject>
    {
        [Test]
        public void DoesNotError()
        {
            // nothing
        }

        [UnityTest]
        public IEnumerator SyncToClient()
        {
            const int num1 = 11;
            const int num2 = 12;
            const int num3 = 13;
            Action<int> sub = Substitute.For<Action<int>>();
            serverComponent.baseValue = num1;
            serverComponent.value = num2;
            serverComponent.moreValue = num3;

            yield return null;
            yield return null;

            Assert.That(clientComponent.baseValue, Is.EqualTo(num1));
            Assert.That(clientComponent.value, Is.EqualTo(num2));
            Assert.That(clientComponent.moreValue, Is.EqualTo(num3));
        }
    }
}

