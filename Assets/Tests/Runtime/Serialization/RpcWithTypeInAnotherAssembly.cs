using System;
using System.Collections;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Serialization
{
    public class RpcWithTypeInAnotherAssemblyBehaviour : NetworkBehaviour
    {
        public event Action<MessageWithCustomWriter> onRpc_CustomWriter;
        public event Action<MessageWitAutoWriter> onRpc_AutoWriter;
        public event Action<MessageWithNoWriter> onRpc_NoWriter;

        [ServerRpc]
        public void SendValue_Custom(MessageWithCustomWriter value)
        {
            onRpc_CustomWriter?.Invoke(value);
        }

        [ServerRpc]
        public void SendValue_Auto(MessageWitAutoWriter value)
        {
            onRpc_AutoWriter?.Invoke(value);
        }

        [ServerRpc]
        public void SendValue_None(MessageWithNoWriter value)
        {
            onRpc_NoWriter?.Invoke(value);
        }
    }

    public class RpcWithTypeInAnotherAssembly : ClientServerSetup<RpcWithTypeInAnotherAssemblyBehaviour>
    {
        [UnityTest]
        [Description("This has custom writer in other Assembly, we need to make sure we use it")]
        public IEnumerator UsingWriterInAnotherAssembly()
        {
            // reset flags
            MessageWithCustomWriterExtesions.WriterCalled = 0;
            MessageWithCustomWriterExtesions.ReaderCalled = 0;

            var inValue = new MessageWithCustomWriter()
            {
                type = 3,
                value = 1.4f
            };

            var outValue = new MessageWithCustomWriter();
            serverComponent.onRpc_CustomWriter += (v) => outValue = v;
            clientComponent.SendValue_Custom(inValue);
            yield return null;
            yield return null;
            Assert.That(outValue.type, Is.EqualTo(inValue.type));
            Assert.That(outValue.value, Is.EqualTo(inValue.value).Within(0.01f));

            // methods should be called once
            Assert.That(MessageWithCustomWriterExtesions.WriterCalled, Is.EqualTo(1));
            Assert.That(MessageWithCustomWriterExtesions.ReaderCalled, Is.EqualTo(1));
        }

        [UnityTest]
        [Description("This struct has writer in its own Assembly, it is automatic, so doesn't really matter if we use that one of the one here")]
        public IEnumerator UsingAutoWriterInAnotherAsembly()
        {
            var inValue = new MessageWitAutoWriter()
            {
                type = 3,
                value = 1.4f
            };

            var outValue = new MessageWitAutoWriter();
            serverComponent.onRpc_AutoWriter += (v) => outValue = v;
            clientComponent.SendValue_Auto(inValue);
            yield return null;
            yield return null;
            Assert.That(outValue.type, Is.EqualTo(inValue.type));
            Assert.That(outValue.value, Is.EqualTo(inValue.value));
        }

        [UnityTest]
        [Description("This struct has no writer in its own Assembly, but this Assembly will generate one, and then use it for rpc")]
        public IEnumerator GeneratesAndUsesWriterInThisAssembly()
        {
            // reset flags
            MessageWithCustomWriterExtesions.WriterCalled = 0;
            MessageWithCustomWriterExtesions.ReaderCalled = 0;

            var inValue = new MessageWithNoWriter()
            {
                type = 3,
                value = 1.4f
            };

            var outValue = new MessageWithNoWriter();
            serverComponent.onRpc_NoWriter += (v) => outValue = v;
            clientComponent.SendValue_None(inValue);
            yield return null;
            yield return null;
            Assert.That(outValue.type, Is.EqualTo(inValue.type));
            Assert.That(outValue.value, Is.EqualTo(inValue.value));
        }
    }
}
