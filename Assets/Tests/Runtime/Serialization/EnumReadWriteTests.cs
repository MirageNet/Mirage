using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public static class MyCustomEnumReadWrite
    {
        public static void WriteMyCustomEnum(this NetworkWriter networkWriter, EnumReadWriteTests.MyCustom customEnum)
        {
            // if O write N
            if (customEnum == EnumReadWriteTests.MyCustom.O)
            {
                networkWriter.WriteInt32((int)EnumReadWriteTests.MyCustom.N);
            }
            else
            {
                networkWriter.WriteInt32((int)customEnum);
            }
        }
        public static EnumReadWriteTests.MyCustom ReadMyCustomEnum(this NetworkReader networkReader)
        {
            return (EnumReadWriteTests.MyCustom)networkReader.ReadInt32();
        }
    }
    public class EnumReadWriteTests
    {
        public enum MyByte : byte
        {
            A, B, C, D
        }

        public enum MyShort : short
        {
            E, F, G, H
        }

        public enum MyCustom
        {
            M, N, O, P
        }

        private readonly NetworkWriter writer = new NetworkWriter(1300);
        private readonly NetworkReader reader = new NetworkReader();

        [TearDown]
        public void TearDown()
        {
            writer.Reset();
            reader.Dispose();
        }

        [Test]
        public void ByteIsSentForByteEnum()
        {
            var byteEnum = MyByte.B;

            writer.Write(byteEnum);

            // should only be 1 byte
            Assert.That(writer.ByteLength, Is.EqualTo(1));
        }

        [Test]
        public void ShortIsSentForShortEnum()
        {
            var shortEnum = MyShort.G;

            writer.Write(shortEnum);

            // should only be 1 byte
            Assert.That(writer.ByteLength, Is.EqualTo(2));
        }

        [Test]
        public void CustomWriterIsUsedForEnum()
        {
            var customEnum = MyCustom.O;
            var clientMsg = SerializeAndDeserializeMessage(customEnum);

            // custom writer should write N if it sees O
            Assert.That(clientMsg, Is.EqualTo(MyCustom.N));
        }

        private T SerializeAndDeserializeMessage<T>(T msg)
        {
            writer.Write(msg);

            reader.Reset(writer.ToArraySegment());
            return reader.Read<T>();
        }
    }
}
