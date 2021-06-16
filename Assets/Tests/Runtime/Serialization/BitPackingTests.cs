using System;
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

    public class BitPackingTests
    {
        private NetworkWriter writer;
        private NetworkReader reader;

        [SetUp]
        public void Setup()
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
        [TestCase(0ul)]
        [TestCase(1ul)]
        [TestCase(0x_FFFF_FFFF_12Ul)]
        public void WritesCorrectUlongValue(ulong value)
        {
            writer.Write(value, 64);
            reader.Reset(writer.ToArray());

            ulong result = reader.Read(64);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0u)]
        [TestCase(1u)]
        [TestCase(0x_FFFF_FF12U)]
        public void WritesCorrectUIntValue(uint value)
        {
            writer.Write(value, 32);
            reader.Reset(writer.ToArray());

            ulong result = reader.Read(32);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0u, 10, 2u, 5)]
        [TestCase(10u, 10, 36u, 15)]
        [TestCase(1u, 1, 250u, 8)]
        public void WritesCorrectValues(uint value1, int bits1, uint value2, int bits2)
        {
            writer.Write(value1, bits1);
            writer.Write(value2, bits2);
            reader.Reset(writer.ToArray());

            ulong result1 = reader.Read(bits1);
            ulong result2 = reader.Read(bits2);
            Assert.That(result1, Is.EqualTo(value1));
            Assert.That(result2, Is.EqualTo(value2));
        }


        [Test]
        public void CanWriteToBufferLimit()
        {
            for (int i = 0; i < 208; i++)
            {
                writer.Write((ulong)i, 50);
            }

            byte[] result = writer.ToArray();

            // written bits/8
            int expectedLength = (208 * 50) / 8;
            Assert.That(result, Has.Length.EqualTo(expectedLength));
        }

        [Test, Description("Real buffer size is 1304 because 1300 rounds up")]
        public void WriterThrowIfWritesTooMuch()
        {
            // write 1296 up to last word
            for (int i = 0; i < 162; i++)
            {
                writer.Write((ulong)i, 64);
            }

            writer.Write(0, 63);

            Assert.DoesNotThrow(() =>
            {
                writer.Write(0, 1);
            });

            IndexOutOfRangeException exception = Assert.Throws<IndexOutOfRangeException>(() =>
            {
                writer.Write(0, 1);
            });
            Assert.That(exception, Has.Message.EqualTo("Index was outside the bounds of the array."));
        }
    }
}
