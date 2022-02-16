using System;
using System.Collections;
using Mirage.Collections;
using Mirage.Serialization;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.Generics
{
    public struct MyMessage<T>
    {
        public T Value;
    }

    public class GenericMessages : ClientServerSetup<MockComponent>
    {
        [Test]
        public void CanReadWrite()
        {
            const int num = 32;
            var msg = new MyMessage<int> { Value = num };

            var writer = new NetworkWriter(100);
            writer.Write(msg);

            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            MyMessage<int> result = reader.Read<MyMessage<int>>();

            Assert.That(result, Is.EqualTo(msg));
        }

        [UnityTest]
        public IEnumerator CanSendMessage()
        {
            const int num = 32;
            int called = 0;

            var msg = new MyMessage<int> { Value = num };
            server.MessageHandler.RegisterHandler<MyMessage<int>>((result) =>
            {
                called++;
                Assert.That(result, Is.EqualTo(msg));
            });

            client.Send(msg);
            yield return null;
            yield return null;

            Assert.That(called, Is.EqualTo(1));
        }
    }
    public class GenericSyncList_Behaviour : NetworkBehaviour
    {
        public readonly SyncList<MyStruct<int>> myList = new SyncList<MyStruct<int>>();

        public struct MyStruct<T>
        {
            public T value;
        }
    }
    public class GenericSyncList : ClientServerSetup<GenericSyncList_Behaviour>
    {
        [UnityTest]
        public IEnumerator SyncsValues()
        {
            const int num1 = 32;
            const int num2 = 48;
            serverComponent.myList.Add(new GenericSyncList_Behaviour.MyStruct<int> { value = num1 });
            serverComponent.myList.Add(new GenericSyncList_Behaviour.MyStruct<int> { value = num2 });

            yield return new WaitForSeconds(0.4f);

            Assert.That(clientComponent.myList.Count, Is.EqualTo(2));
            Assert.That(clientComponent.myList[0].value, Is.EqualTo(num1));
            Assert.That(clientComponent.myList[1].value, Is.EqualTo(num2));

            serverComponent.myList.Remove(new GenericSyncList_Behaviour.MyStruct<int> { value = num1 });

            yield return new WaitForSeconds(0.4f);

            Assert.That(clientComponent.myList.Count, Is.EqualTo(1));
            Assert.That(clientComponent.myList[0].value, Is.EqualTo(num2));
        }
    }

    public class GenericStructAsArg_Behaviour : NetworkBehaviour
    {
        public struct MyStruct<T>
        {
            public T value;
        }
        public struct MyHolder
        {
            public MyStruct<int> inner;
        }

        public event Action<int> structParam;
        public event Action<int> holderParam;

        [ClientRpc]
        public void MyRpc(MyStruct<int> value)
        {
            structParam.Invoke(value.value);
        }

        [ClientRpc]
        public void MyRpcHolder(MyHolder value)
        {
            holderParam.Invoke(value.inner.value);
        }
    }
    public class GenericStructAsArg : ClientServerSetup<GenericStructAsArg_Behaviour>
    {
        [UnityTest]
        public IEnumerator CanCallServerRpc()
        {
            const int num = 32;
            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.structParam += sub;
            serverComponent.MyRpc(new GenericStructAsArg_Behaviour.MyStruct<int> { value = num });

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }

        [UnityTest]
        public IEnumerator CanCallServerRpcWithHolder()
        {
            const int num = 32;
            Action<int> sub = Substitute.For<Action<int>>();
            clientComponent.holderParam += sub;
            serverComponent.MyRpcHolder(new GenericStructAsArg_Behaviour.MyHolder
            {
                inner = new GenericStructAsArg_Behaviour.MyStruct<int>
                {
                    value = num
                }
            });

            yield return null;
            yield return null;

            sub.Received(1).Invoke(num);
        }
    }
}
