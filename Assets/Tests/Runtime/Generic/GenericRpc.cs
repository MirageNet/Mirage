using System;
using System.Collections;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.ManualRpcTests
{
    public class Manual_GenericWithRpc_behaviour<T> : NetworkBehaviour, __IRpcSkeleton
    {
        public event Action<int> serverCalled;


        public void MyRpc2(int value, INetworkPlayer sender)
        {
            if (base.IsServer)
            {
                UserCode_MyRpc2_4321(value, base.Server.LocalPlayer);
                return;
            }

            PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
            writer.WritePackedInt32(value);
            ServerRpcSender.Send(this, 4321, writer, 0, false);
            writer.Release();
        }
        public void UserCode_MyRpc2_4321(int value, INetworkPlayer sender)
        {
            serverCalled?.Invoke(value);
        }

        public void Skeleton_MyRpc2_4321(NetworkReader reader, INetworkPlayer senderConnection, int replyId)
        {
            UserCode_MyRpc2_4321(reader.ReadPackedInt32(), senderConnection);
        }



        public event Action<int> clientCalled;

        public void MyRpc(int value)
        {
            if (base.IsClient)
            {
                UserCode_MyRpc_1234(value);
            }
            PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
            writer.WritePackedInt32(value);
            ClientRpcSender.Send(this, 1234, writer, 0, false);
            writer.Release();
        }

        public void UserCode_MyRpc_1234(int value)
        {
            clientCalled?.Invoke(value);
        }

        public void Skeleton_MyRpc_1234(NetworkReader reader, INetworkPlayer senderConnection, int replyId)
        {
            UserCode_MyRpc_1234(reader.ReadPackedInt32());
        }

        static Manual_GenericWithRpc_behaviour()
        {
            __RPCGenericCaller.Register();
        }
    }
    interface __IRpcSkeleton
    {
        void Skeleton_MyRpc_1234(NetworkReader reader, INetworkPlayer senderConnection, int replyId);
        void Skeleton_MyRpc2_4321(NetworkReader reader, INetworkPlayer senderConnection, int replyId);
    }
    public class __RPCGenericCaller
    {
        protected static void Skeleton_MyRpc_1234(NetworkBehaviour behaviour, NetworkReader reader, INetworkPlayer senderConnection, int replyId)
        {
            ((__IRpcSkeleton)behaviour).Skeleton_MyRpc_1234(behaviour, reader, senderConnection, replyId);
        }
        protected static void Skeleton_MyRpc2_4321(NetworkBehaviour behaviour, NetworkReader reader, INetworkPlayer senderConnection, int replyId)
        {
            ((__IRpcSkeleton)behaviour).Skeleton_MyRpc2_4321(behaviour, reader, senderConnection, replyId);
        }

        public static void Register()
        {
            {
                var func = new RpcDelegate(Skeleton_MyRpc_1234);
                RemoteCallHelper.Register(null, "MyRpc", 1234, RpcInvokeType.ClientRpc, func, false);
            }

            {
                var func = new RpcDelegate(Skeleton_MyRpc2_4321);
                RemoteCallHelper.Register(null, "MyRpc2", 4321, RpcInvokeType.ServerRpc, func, false);
            }
        }
    }

    public class Manual_GenericWithRpc_behaviourInt : Manual_GenericWithRpc_behaviour<int>
    {
    }
    public class Manual_GenericWithRpc_behaviourObject : Manual_GenericWithRpc_behaviour<object>
    {
    }

    public class Manual_GenericWithRpcInt : ClientServerSetup<Manual_GenericWithRpc_behaviourInt>
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
    public class GenericWithRpcObject : ClientServerSetup<Manual_GenericWithRpc_behaviourObject>
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

