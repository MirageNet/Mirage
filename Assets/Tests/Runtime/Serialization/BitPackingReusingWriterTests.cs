using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class BitPackingReusingWriterTests
    {
        private NetworkWriter writer;
        private NetworkReader reader;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            writer = new NetworkWriter(1300);
            reader = new NetworkReader();
        }

        [TearDown]
        public void TearDown()
        {
            writer.Reset();
            reader.Dispose();
        }


        [Test]
        public void WriteUShortAfterReset()
        {
            ushort value1 = 0b0101;
            ushort value2 = 0x1000;

            // write first value
            writer.WriteUInt16(value1);

            reader.Reset(writer.ToArray());
            ushort out1 = reader.ReadUInt16();
            Assert.That(out1, Is.EqualTo(value1));

            // reset and write 2nd value
            writer.Reset();

            writer.WriteUInt16(value2);

            reader.Reset(writer.ToArray());
            ushort out2 = reader.ReadUInt16();
            Assert.That(out2, Is.EqualTo(value2), "Value 2 was incorrect");
        }

        [Test]
        [TestCase(0b0101ul, 0x1000ul)]
        [TestCase(0xffff_0000_ffff_fffful, 0x0000_ffff_1111_0000ul)]
        public void WriteULongAfterReset(ulong value1, ulong value2)
        {
            // write first value
            writer.WriteUInt64(value1);

            reader.Reset(writer.ToArray());
            ulong out1 = reader.ReadUInt64();
            Assert.That(out1, Is.EqualTo(value1));

            // reset and write 2nd value
            writer.Reset();

            writer.WriteUInt64(value2);

            reader.Reset(writer.ToArray());
            ulong out2 = reader.ReadUInt64();
            Assert.That(out2, Is.EqualTo(value2), "Value 2 was incorrect");
        }

        [Test]
        [TestCase(0b0101ul, 0x1000ul)]
        [TestCase(0xffff_0000_ffff_fffful, 0x0000_ffff_1111_0000ul)]
        public void WriteULongWriteBitsAfterReset(ulong value1, ulong value2)
        {
            // write first value
            writer.Write(value1, 64);

            reader.Reset(writer.ToArray());
            ulong out1 = reader.Read(64);
            Assert.That(out1, Is.EqualTo(value1));

            // reset and write 2nd value
            writer.Reset();

            writer.Write(value2, 64);

            reader.Reset(writer.ToArray());
            ulong out2 = reader.Read(64);
            Assert.That(out2, Is.EqualTo(value2), "Value 2 was incorrect");
        }
    }
}
