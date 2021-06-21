using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class BitPackingProperties
    {
        private readonly NetworkWriter writer = new NetworkWriter(1300);
        private readonly NetworkReader reader = new NetworkReader();

        readonly byte[] sampleData = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        const int BITS_PER_BYTE = 8;

        [TearDown]
        public void TearDown()
        {
            writer.Reset();
            reader.Dispose();
        }


        [Test]
        public void WriterBitPositionStartsAtZero()
        {
            Assert.That(writer.BitPosition, Is.EqualTo(0));
        }

        [Test]
        public void WriterByteLengthStartsAtZero()
        {
            Assert.That(writer.ByteLength, Is.EqualTo(0));
        }

        [Test]
        public void ReaderBitPositionStartsStartsAtZero()
        {
            reader.Reset(sampleData);
            Assert.That(reader.BitPosition, Is.EqualTo(0));
        }

        [Test]
        public void ReaderBytePositionStartsStartsAtZero()
        {
            reader.Reset(sampleData);
            Assert.That(reader.BytePosition, Is.EqualTo(0));
        }

        [Test]
        public void ReaderBitLengthStartsStartsAtArrayLength()
        {
            reader.Reset(sampleData);
            Assert.That(reader.BitLength, Is.EqualTo(sampleData.Length * BITS_PER_BYTE));
        }



        [Test]
        public void WriterBitPositionIncreasesAfterWriting()
        {
            writer.Write(0, 15);
            Assert.That(writer.BitPosition, Is.EqualTo(15));

            writer.Write(0, 50);
            Assert.That(writer.BitPosition, Is.EqualTo(65));
        }

        [Test]
        public void WriterByteLengthIncreasesAfterWriting_ShouldRoundUp()
        {
            writer.Write(0, 15);
            Assert.That(writer.ByteLength, Is.EqualTo(2));

            writer.Write(0, 50);
            Assert.That(writer.ByteLength, Is.EqualTo(9));
        }

        [Test]
        public void ReaderBitPositionIncreasesAfterReading()
        {
            reader.Reset(sampleData);
            _ = reader.Read(15);
            Assert.That(reader.BitPosition, Is.EqualTo(15));

            _ = reader.Read(50);
            Assert.That(reader.BitPosition, Is.EqualTo(65));
        }

        [Test]
        public void ReaderBytePositionIncreasesAfterReading_ShouldRoundUp()
        {
            reader.Reset(sampleData);
            _ = reader.Read(15);
            Assert.That(reader.BytePosition, Is.EqualTo(2));

            _ = reader.Read(50);
            Assert.That(reader.BytePosition, Is.EqualTo(9));
        }

        [Test]
        public void ReaderBitLengthDoesnotIncreasesAfterReading()
        {
            reader.Reset(sampleData);
            _ = reader.Read(15);
            Assert.That(reader.BitLength, Is.EqualTo(sampleData.Length * BITS_PER_BYTE));

            _ = reader.Read(50);
            Assert.That(reader.BitLength, Is.EqualTo(sampleData.Length * BITS_PER_BYTE));
        }
    }
}
