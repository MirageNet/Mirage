using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization.StructMessages
{
    public struct SomeStructMessage
    {
        public int someValue;
    }

    [TestFixture]
    public class StructMessagesTests
    {
        private readonly NetworkWriter writer = new NetworkWriter(1300);
        private readonly NetworkReader reader = new NetworkReader();

        [TearDown]
        public void TearDown()
        {
            writer.Reset();
            reader.Dispose();
        }

        [Test]
        public void SerializeAreAddedWhenEmptyInStruct()
        {
            writer.Reset();

            const int someValue = 3;
            writer.Write(new SomeStructMessage
            {
                someValue = someValue,
            });

            reader.Reset(writer.ToArraySegment());
            var received = reader.Read<SomeStructMessage>();

            Assert.AreEqual(someValue, received.someValue);

            var writeLength = writer.ByteLength;
            var readLength = reader.BytePosition;
            Assert.That(writeLength == readLength, $"OnSerializeAll and OnDeserializeAll calls write the same amount of data\n    writeLength={writeLength}\n    readLength={readLength}");
        }
    }
}
