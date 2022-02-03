using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.RpcTests
{
    //// note, this can't be a behaviour by itself, it must have a child class in order to be used
    //public class GenericWithRpc_behaviour<T> : NetworkBehaviour
    //{
    //    public event Action<T> clientCalled;
    //    public event Action<T> serverCalled;

    //    [ClientRpc]
    //    public void MyRpc(T value)
    //    {
    //        clientCalled?.Invoke(value);
    //    }


    //    [ServerRpc(requireAuthority = false)]
    //    public void MyRpc(T value, INetworkPlayer sender)
    //    {
    //        serverCalled?.Invoke(value);
    //    }

    //    public void MyRpc_Weaver(T value)
    //    {
    //        if (base.IsClient)
    //        {
    //            MyRpc(default);
    //        }
    //        PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    //        writer.Write(value);
    //        ClientRpcSender.Send(this, -1903946991, writer, 0, false);
    //        writer.Release();
    //    }
    //}
    //public class GenericWithRpc_behaviourInt : GenericWithRpc_behaviour<int> { }

    //public class GenericWithRpc : ClientServerSetup<GenericWithRpc_behaviourInt>
    //{
    //    [UnityTest]
    //    public IEnumerator CanCallServerRpc()
    //    {
    //        const int num = 32;
    //        Action<int> sub = Substitute.For<Action<int>>();
    //        serverComponent.serverCalled += sub;
    //        clientComponent.MyRpc(num, default);

    //        yield return null;
    //        yield return null;

    //        sub.Received(1).Invoke(num);
    //    }

    //    [UnityTest]
    //    public IEnumerator CanCallClientRpc()
    //    {
    //        const int num = 32;
    //        Action<int> sub = Substitute.For<Action<int>>();
    //        clientComponent.clientCalled += sub;
    //        serverComponent.MyRpc(num);

    //        yield return null;
    //        yield return null;

    //        sub.Received(1).Invoke(num);
    //    }
    //}


    // note, this can't be a behaviour by itself, it must have a child class in order to be used
    public class NotGenericWithRpcBase_behaviour<T> : NetworkBehaviour
    {
        [SyncVar] int baseValue;

    }
    public class NotGenericWithRpc_behaviour<T> : NotGenericWithRpcBase_behaviour<T>
    {
        public event Action<int> clientCalled;
        public event Action<int> serverCalled;

        [SyncVar] int value;

        //[ServerRpc(requireAuthority = false)]
        //public void MyRpc2(int value, INetworkPlayer sender)
        //{
        //    serverCalled?.Invoke(value);
        //}
        //[ClientRpc]
        //public void MyRpc(int value)
        //{
        //    clientCalled?.Invoke(value);
        //}

        public int GetInt() => 0;
    }

    public class NotGenericWithRpc_behaviourInt : NotGenericWithRpc_behaviour<int>
    {
        [SyncVar] int moreValue;
    }
    public class NotGenericWithRpc_behaviourObject : NotGenericWithRpc_behaviour<object>
    {
        [SyncVar] int moreValue;
    }

    public class NotGenericWithRpcInt : ClientServerSetup<NotGenericWithRpc_behaviourInt>
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
            //clientComponent.MyRpc2(num, default);

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
            //serverComponent.MyRpc(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }
    public class NotGenericWithRpcObject : ClientServerSetup<NotGenericWithRpc_behaviourObject>
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
            //clientComponent.MyRpc2(num, default);

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
            //serverComponent.MyRpc(num);

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }
}

