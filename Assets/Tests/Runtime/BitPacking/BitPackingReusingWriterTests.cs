using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class BitPackingResizeTest
    {
        private NetworkWriter writer;
        private NetworkReader reader;

        [SetUp]
        public void SetUp()
        {
            writer = new NetworkWriter(1300, true);
            reader = new NetworkReader();
        }

        [TearDown]
        public void TearDown()
        {
            // we have to clear these each time so that capactity doesn't effect other tests
            writer.Reset();
            writer = null;
            reader.Dispose();
            reader = null;
        }

        [Test]
        public void ResizesIfWritingOverCapacity()
        {
            var overCapacity = (1300 / 8) + 10;
            Assert.That(writer.ByteCapacity, Is.EqualTo(1304), "is first multiple of 8 over 1300");
            for (var i = 0; i < overCapacity; i++)
            {
                writer.WriteUInt64((ulong)i);
            }

            Assert.That(writer.ByteCapacity, Is.EqualTo(1304 * 2), "should double in size");
        }


        [Test]
        public void WillResizeMultipleTimes()
        {
            var overCapacity = ((1300 / 8) + 10) * 10; // 1720 * 8 = 13760 bytes

            Assert.That(writer.ByteCapacity, Is.EqualTo(1304), "is first multiple of 8 over 1300");
            for (var i = 0; i < overCapacity; i++)
            {
                writer.WriteUInt64((ulong)i);
            }


            Assert.That(writer.ByteCapacity, Is.EqualTo(20_864), "should double each time it goes over capacity");
        }

        [Test]
        public void ResizedArrayContainsAllData()
        {
            var overCapacity = (1300 / 8) + 10;
            for (var i = 0; i < overCapacity; i++)
            {
                writer.WriteUInt64((ulong)i);
            }


            var segment = writer.ToArraySegment();
            reader.Reset(segment);
            for (var i = 0; i < overCapacity; i++)
            {
                Assert.That(reader.ReadUInt64(), Is.EqualTo((ulong)i));
            }
        }
    }
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
            var out1 = reader.ReadUInt16();
            Assert.That(out1, Is.EqualTo(value1));

            // reset and write 2nd value
            writer.Reset();

            writer.WriteUInt16(value2);

            reader.Reset(writer.ToArray());
            var out2 = reader.ReadUInt16();
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
            var out1 = reader.ReadUInt64();
            Assert.That(out1, Is.EqualTo(value1));

            // reset and write 2nd value
            writer.Reset();

            writer.WriteUInt64(value2);

            reader.Reset(writer.ToArray());
            var out2 = reader.ReadUInt64();
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
            var out1 = reader.Read(64);
            Assert.That(out1, Is.EqualTo(value1));

            // reset and write 2nd value
            writer.Reset();

            writer.Write(value2, 64);

            reader.Reset(writer.ToArray());
            var out2 = reader.Read(64);
            Assert.That(out2, Is.EqualTo(value2), "Value 2 was incorrect");
        }
    }
}
