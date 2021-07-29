using System;
using System.Collections;
using Mirage.Serialization;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Serialization.Attributes
{
    public class BitBehaviour10 : NetworkBehaviour
    {
        [BitCount(10)]
        [SyncVar] public int myIntValue;

        public event Action<int> onRpc;

        [ClientRpc]
        public void RpcSomeFunction([BitCount(10)] int myParam)
        {
            onRpc?.Invoke(myParam);
        }
    }
    public class BitCountTest10 : ClientServerSetup<BitBehaviour10>
    {
        [Test]
        public void SyncVarIsBitPacked()
        {
            var behaviour = new BitBehaviour10();

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                behaviour.SerializeSyncVars(writer, true);

                Assert.That(writer.BitPosition, Is.EqualTo(10));
            }
        }

        [UnityTest]
        public IEnumerator RpcIsBitPacked()
        {
            const int value = 20;

            int called = 0;
            clientComponent.onRpc += (v) => { called++; Assert.That(v, Is.EqualTo(value)); };

            client.MessageHandler.UnregisterHandler<RpcMessage>();
            int payloadSize = 0;
            client.MessageHandler.RegisterHandler<RpcMessage>((player, msg) =>
            {
                // store value in variable because assert will throw and be catch by message wrapper
                payloadSize = msg.payload.Count;
                clientObjectManager.OnRpcMessage(msg);
            });


            serverComponent.RpcSomeFunction(value);
            yield return null;
            Assert.That(called, Is.EqualTo(1));
            Assert.That(payloadSize, Is.EqualTo(2), "10 bits is 2 bytes in payload");
        }
    }

    public class BitBehaviour17 : NetworkBehaviour
    {
        [BitCount(17)]
        [SyncVar] public int myIntValue;

        public event Action<int> onRpc;

        [ClientRpc]
        public void RpcSomeFunction([BitCount(17)] int myParam)
        {
            onRpc?.Invoke(myParam);
        }
    }
    public class BitCountTest17 : ClientServerSetup<BitBehaviour17>
    {
        [Test]
        public void SyncVarIsBitPacked()
        {
            var behaviour = new BitBehaviour17();

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                behaviour.SerializeSyncVars(writer, true);

                Assert.That(writer.BitPosition, Is.EqualTo(17));
            }
        }

        [UnityTest]
        public IEnumerator RpcIsBitPacked()
        {
            const int value = 20;

            int called = 0;
            clientComponent.onRpc += (v) => { called++; Assert.That(v, Is.EqualTo(value)); };

            client.MessageHandler.UnregisterHandler<RpcMessage>();
            int payloadSize = 0;
            client.MessageHandler.RegisterHandler<RpcMessage>((player, msg) =>
            {
                // store value in variable because assert will throw and be catch by message wrapper
                payloadSize = msg.payload.Count;
                clientObjectManager.OnRpcMessage(msg);
            });


            serverComponent.RpcSomeFunction(value);
            yield return null;
            Assert.That(called, Is.EqualTo(1));
            Assert.That(payloadSize, Is.EqualTo(3), "17 bits is 3 bytes in payload");
        }
    }

    public class BitBehaviour32 : NetworkBehaviour
    {
        [BitCount(32)]
        [SyncVar] public int myIntValue;

        public event Action<int> onRpc;

        [ClientRpc]
        public void RpcSomeFunction([BitCount(32)] int myParam)
        {
            onRpc?.Invoke(myParam);
        }
    }
    public class BitCountTest32 : ClientServerSetup<BitBehaviour32>
    {
        [Test]
        public void SyncVarIsBitPacked()
        {
            var behaviour = new BitBehaviour32();

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                behaviour.SerializeSyncVars(writer, true);

                Assert.That(writer.BitPosition, Is.EqualTo(32));
            }
        }

        [UnityTest]
        public IEnumerator RpcIsBitPacked()
        {
            const int value = 20;

            int called = 0;
            clientComponent.onRpc += (v) => { called++; Assert.That(v, Is.EqualTo(value)); };

            client.MessageHandler.UnregisterHandler<RpcMessage>();
            int payloadSize = 0;
            client.MessageHandler.RegisterHandler<RpcMessage>((player, msg) =>
            {
                // store value in variable because assert will throw and be catch by message wrapper
                payloadSize = msg.payload.Count;
                clientObjectManager.OnRpcMessage(msg);
            });


            serverComponent.RpcSomeFunction(value);
            yield return null;
            Assert.That(called, Is.EqualTo(1));
            Assert.That(payloadSize, Is.EqualTo(4), "32 bits is 2 bytes in payload");
        }
    }
}
