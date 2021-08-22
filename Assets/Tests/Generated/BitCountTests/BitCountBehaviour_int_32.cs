using System;
using System.Collections;
using Mirage.Serialization;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Generated.BitCountAttributeTests
{
    public class BitCountBehaviour_int_32 : NetworkBehaviour
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
    public class BitCountTest_int_32 : ClientServerSetup<BitCountBehaviour_int_32>
    {
        [Test]
        public void SyncVarIsBitPacked()
        {
            var behaviour = new BitCountBehaviour_int_32();

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                behaviour.SerializeSyncVars(writer, true);

                Assert.That(writer.BitPosition, Is.EqualTo(32));
            }
        }

        // [UnityTest]
        // [Ignore("Rpc not supported yet")]
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
            Assert.That(payloadSize, Is.EqualTo(4), $"32 bits is 4 bytes in payload");
        }
    }
}
