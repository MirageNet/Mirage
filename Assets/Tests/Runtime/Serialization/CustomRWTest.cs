using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class MockQuest
    {
        public int Id { get; set; }

        public MockQuest(int id)
        {
            Id = id;
        }

        public MockQuest()
        {
            Id = 0;
        }
    }

    public static class MockQuestReaderWriter
    {
        public static void WriteQuest(this NetworkWriter writer, MockQuest quest)
        {
            writer.WritePackedInt32(quest.Id);
        }
        public static MockQuest ReadQuest(this NetworkReader reader)
        {
            return new MockQuest(reader.ReadPackedInt32());
        }
    }

    [TestFixture]
    public class CustomRWTest
    {
        [Test]
        public void TestCustomRW()
        {
            var quest = new MockQuest(100);

            var data = MessagePacker.Pack(quest);

            var unpacked = MessagePacker.Unpack<MockQuest>(data);
            Assert.That(unpacked.Id, Is.EqualTo(100));
        }
    }
}
