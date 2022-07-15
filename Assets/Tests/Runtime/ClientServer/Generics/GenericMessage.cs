using System.Collections;
using Mirage.Serialization;
using NUnit.Framework;
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
            var result = reader.Read<MyMessage<int>>();

            Assert.That(result, Is.EqualTo(msg));
        }

        [UnityTest]
        public IEnumerator CanSendMessage()
        {
            const int num = 32;
            var called = 0;

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
}
