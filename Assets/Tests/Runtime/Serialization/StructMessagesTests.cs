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
        NetworkWriter writer = new NetworkWriter(1300);
        NetworkReader reader = new NetworkReader();

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
            SomeStructMessage received = reader.Read<SomeStructMessage>();

            Assert.AreEqual(someValue, received.someValue);

            int writeLength = writer.ByteLength;
            int readLength = reader.BytePosition;
            Assert.That(writeLength == readLength, $"OnSerializeAll and OnDeserializeAll calls write the same amount of data\n    writeLength={writeLength}\n    readLength={readLength}");
        }
    }
}
