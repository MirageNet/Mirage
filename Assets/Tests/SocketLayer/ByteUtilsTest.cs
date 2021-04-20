using NUnit.Framework;

namespace Mirage.SocketLayer.Tests
{
    [Category("SocketLayer")]
    public class ByteUtilsTest
    {
        [Test]
        public void WriteReadByteTest()
        {
            byte[] buffer = new byte[10];
            int offset = 3;
            byte value = 100;

            ByteUtils.WriteByte(buffer, ref offset, value);
            Assert.That(offset, Is.EqualTo(4), "Should have moved offset by 1");
            Assert.That(buffer[3], Is.Not.Zero, "Should have written to buffer");

            offset = 3;
            byte outValue = ByteUtils.ReadByte(buffer, ref offset);
            Assert.That(offset, Is.EqualTo(4), "Should have moved offset by 1");
            Assert.That(outValue, Is.EqualTo(value), "Out value should be equal to in value");
        }


        [Test]
        public void WriteReadUShortTest()
        {
            byte[] buffer = new byte[10];
            int offset = 3;
            ushort value = 1000;

            ByteUtils.WriteUShort(buffer, ref offset, value);
            Assert.That(offset, Is.EqualTo(5), "Should have moved offset by 2");
            Assert.That(buffer[3], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[4], Is.Not.Zero, "Should have written to buffer");

            offset = 3;
            ushort outValue = ByteUtils.ReadUShort(buffer, ref offset);
            Assert.That(offset, Is.EqualTo(5), "Should have moved offset by 2");
            Assert.That(outValue, Is.EqualTo(value), "Out value should be equal to in value");
        }

        [Test]
        public void WriteReadUIntTest()
        {
            byte[] buffer = new byte[10];
            int offset = 3;
            // value large enough to use all 4 bytes
            uint value = 0x12_34_56_78;

            ByteUtils.WriteUInt(buffer, ref offset, value);
            Assert.That(offset, Is.EqualTo(7), "Should have moved offset by 4");
            Assert.That(buffer[3], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[4], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[5], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[6], Is.Not.Zero, "Should have written to buffer");

            offset = 3;
            uint outValue = ByteUtils.ReadUInt(buffer, ref offset);
            Assert.That(offset, Is.EqualTo(7), "Should have moved offset by 4");
            Assert.That(outValue, Is.EqualTo(value), "Out value should be equal to in value");
        }
    }
}
