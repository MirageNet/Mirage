using Mirage.Collections;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests
{
    public static class SyncObjectHelper
    {
        private static readonly NetworkWriter writer = new NetworkWriter(1300);
        private static readonly NetworkReader reader = new NetworkReader();

        public static void SerializeAllTo<T>(T fromList, T toList) where T : ISyncObject
        {
            writer.Reset();
            fromList.OnSerializeAll(writer);

            reader.Reset(writer.ToArray());
            toList.OnDeserializeAll(reader);

            var writeLength = writer.ByteLength;
            var readLength = reader.BytePosition;
            Assert.That(writeLength == readLength, $"OnSerializeAll and OnDeserializeAll calls write the same amount of data\n    writeLength={writeLength}\n    readLength={readLength}");
        }

        public static void SerializeDeltaTo<T>(T fromList, T toList) where T : ISyncObject
        {
            writer.Reset();

            fromList.OnSerializeDelta(writer);
            reader.Reset(writer.ToArray());

            toList.OnDeserializeDelta(reader);
            fromList.Flush();

            var writeLength = writer.ByteLength;
            var readLength = reader.BytePosition;
            Assert.That(writeLength == readLength, $"OnSerializeDelta and OnDeserializeDelta calls write the same amount of data\n    writeLength={writeLength}\n    readLength={readLength}");
        }
    }
}
