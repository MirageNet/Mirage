using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class BitPackingCopyFromOtherTests
    {
        private NetworkWriter writer;
        private NetworkWriter otherWriter;
        private NetworkReader reader;

        [SetUp]
        public void Setup()
        {
            writer = new NetworkWriter(1300);
            otherWriter = new NetworkWriter(1300);
            reader = new NetworkReader();
        }

        [TearDown]
        public void TearDown()
        {
            writer.Reset();
            otherWriter.Reset();
            reader.Dispose();
        }

        [Test]
        public void CopyFromOtherWriterAligned()
        {
            otherWriter.Write(1, 8);
            otherWriter.Write(2, 8);
            otherWriter.Write(3, 8);
            otherWriter.Write(4, 8);
            otherWriter.Write(5, 8);


            writer.CopyFromWriter(otherWriter, 0, 5 * 8);

            var segment = writer.ToArraySegment();
            reader.Reset(segment);

            Assert.That(reader.Read(8), Is.EqualTo(1));
            Assert.That(reader.Read(8), Is.EqualTo(2));
            Assert.That(reader.Read(8), Is.EqualTo(3));
            Assert.That(reader.Read(8), Is.EqualTo(4));
            Assert.That(reader.Read(8), Is.EqualTo(5));
        }

        [Test]
        public void CopyFromOtherWriterUnAligned()
        {
            otherWriter.Write(1, 6);
            otherWriter.Write(2, 7);
            otherWriter.Write(3, 8);
            otherWriter.Write(4, 9);
            otherWriter.Write(5, 10);

            writer.Write(1, 3);

            writer.CopyFromWriter(otherWriter, 0, 40);

            var segment = writer.ToArraySegment();
            reader.Reset(segment);

            Assert.That(reader.Read(3), Is.EqualTo(1));
            Assert.That(reader.Read(6), Is.EqualTo(1));
            Assert.That(reader.Read(7), Is.EqualTo(2));
            Assert.That(reader.Read(8), Is.EqualTo(3));
            Assert.That(reader.Read(9), Is.EqualTo(4));
            Assert.That(reader.Read(10), Is.EqualTo(5));
        }

        [Test]
        [Repeat(100)]
        public void CopyFromOtherWriterUnAlignedBig()
        {
            ulong value1 = (ulong)UnityEngine.Random.Range(0, 20000);
            ulong value2 = (ulong)UnityEngine.Random.Range(0, 20000);
            ulong value3 = (ulong)UnityEngine.Random.Range(0, 20000);
            ulong value4 = (ulong)UnityEngine.Random.Range(0, 20000);
            ulong value5 = (ulong)UnityEngine.Random.Range(0, 20000);
            otherWriter.Write(value1, 46);
            otherWriter.Write(value2, 47);
            otherWriter.Write(value3, 48);
            otherWriter.Write(value4, 49);
            otherWriter.Write(value5, 50);

            writer.WriteUInt64(5);
            writer.Write(1, 3);
            writer.WriteByte(171);

            writer.CopyFromWriter(otherWriter, 0, 240);

            var segment = writer.ToArraySegment();
            reader.Reset(segment);

            Assert.That(reader.ReadUInt64(), Is.EqualTo(5ul));
            Assert.That(reader.Read(3), Is.EqualTo(1));
            Assert.That(reader.ReadByte(), Is.EqualTo(171));
            Assert.That(reader.Read(46), Is.EqualTo(value1), "Random value 1 not correct");
            Assert.That(reader.Read(47), Is.EqualTo(value2), "Random value 2 not correct");
            Assert.That(reader.Read(48), Is.EqualTo(value3), "Random value 3 not correct");
            Assert.That(reader.Read(49), Is.EqualTo(value4), "Random value 4 not correct");
            Assert.That(reader.Read(50), Is.EqualTo(value5), "Random value 5 not correct");
        }

        [Test]
        [Repeat(100)]
        public void CopyFromOtherWriterUnAlignedBigOtherUnaligned()
        {
            for (int i = 0; i < 10; i++)
            {
                otherWriter.Write(12, 20);
            }


            ulong value1 = (ulong)UnityEngine.Random.Range(0, 20000);
            ulong value2 = (ulong)UnityEngine.Random.Range(0, 20000);
            ulong value3 = (ulong)UnityEngine.Random.Range(0, 20000);
            ulong value4 = (ulong)UnityEngine.Random.Range(0, 20000);
            ulong value5 = (ulong)UnityEngine.Random.Range(0, 20000);
            otherWriter.Write(value1, 46);
            otherWriter.Write(value2, 47);
            otherWriter.Write(value3, 48);
            otherWriter.Write(value4, 49);
            otherWriter.Write(value5, 50);

            writer.WriteUInt64(5);
            writer.Write(1, 3);
            writer.WriteByte(171);

            writer.CopyFromWriter(otherWriter, 200, 240);

            var segment = writer.ToArraySegment();
            reader.Reset(segment);

            Assert.That(reader.ReadUInt64(), Is.EqualTo(5ul));
            Assert.That(reader.Read(3), Is.EqualTo(1));
            Assert.That(reader.ReadByte(), Is.EqualTo(171));
            Assert.That(reader.Read(46), Is.EqualTo(value1), "Random value 1 not correct");
            Assert.That(reader.Read(47), Is.EqualTo(value2), "Random value 2 not correct");
            Assert.That(reader.Read(48), Is.EqualTo(value3), "Random value 3 not correct");
            Assert.That(reader.Read(49), Is.EqualTo(value4), "Random value 4 not correct");
            Assert.That(reader.Read(50), Is.EqualTo(value5), "Random value 5 not correct");
        }
    }
}
