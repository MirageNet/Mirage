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

        [Test]
        public void WriteReadULongTest()
        {
            byte[] buffer = new byte[20];
            int offset = 3;
            // value large enough to use all 4 bytes
            ulong value = 0x12_34_56_78_90_24_68_02ul;

            ByteUtils.WriteULong(buffer, ref offset, value);
            Assert.That(offset, Is.EqualTo(11), "Should have moved offset by 8");
            Assert.That(buffer[3], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[4], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[5], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[6], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[7], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[8], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[9], Is.Not.Zero, "Should have written to buffer");
            Assert.That(buffer[10], Is.Not.Zero, "Should have written to buffer");

            offset = 3;
            ulong outValue = ByteUtils.ReadULong(buffer, ref offset);
            Assert.That(offset, Is.EqualTo(11), "Should have moved offset by 8");
            Assert.That(outValue, Is.EqualTo(value), "Out value should be equal to in value");
        }

        [Test]
        [Ignore("c# only lets you bit shift by max of 63, It only takes first 6 bits, so will drop any extra bits and try to shift anyway")]
        public void UlongShift()
        {
            ulong value = 0xfUL;
            ulong shifted = value << 56;
            ulong expected = 0x0F00_0000_0000_0000UL;

            Assert.That(shifted, Is.EqualTo(expected));
        }

        [Test]
        [Description("this test should fail")]
        [Ignore("c# only lets you bit shift by max of 63. It only takes first 6 bits, so will drop any extra bits and try to shift anyway")]
        public void UlongShift_ShouldFail()
        {
            ulong value = 0xfUL;
            ulong shifted = value << 64;
            ulong expected = 0;

            // expected to shift value 64 times and end up with 0
            // but instead it shifts 0 times (64 & 0xFFF === 0)
            // this leaves the final result same as value

            Assert.That(shifted, Is.EqualTo(expected));
        }

    }
}
