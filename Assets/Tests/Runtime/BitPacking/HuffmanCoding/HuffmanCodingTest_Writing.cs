using System;
using Mirage.Serialization;
using Mirage.Serialization.HuffmanCoding;
using Mirage.Tests.BitPacking;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization.HuffmanCoding
{
    [TestFixtureSource(typeof(TestSource), nameof(TestSource.Source))]
    public class HuffmanCodingTest_Writing : HuffmanCodingTestBase
    {
        public HuffmanCodingTest_Writing(int groupSize, DataType dataType) : base(groupSize, dataType) { }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Train(6);
        }

        [Test]
        [TestCase(0)]
        [TestCase(20)]
        [TestCase(100)]
        [TestCase(10000)]
        public void CanWriteAndReadSameValues(int inValue)
        {
            var writer = new NetworkWriter(1200);
            var reader = new NetworkReader();

            _model.WriteSigned(writer, inValue);
            reader.Reset(writer.ToArraySegment());
            var outValue = _model.ReadSigned(reader);

            Assert.That(outValue, Is.EqualTo(inValue));
        }

        [Test]
        [Repeat(1000)]
        public void CanWriteAndReadSameValues_Random()
        {
            // we want to test a ranges of values and bit counts, so mask randon int by random bitcount
            var rawValue = TestRandom.Range(1, int.MaxValue);
            var rawBits = TestRandom.Range(1, 33);
            var sign = TestRandom.Range(-1, 1); // apply sign after mask (otherwise it will be excluded)
            var inValue = (rawValue & (int)BitMask.Mask(rawBits)) * sign;

            var writer = new NetworkWriter(1200);
            var reader = new NetworkReader();

            try
            {
                _model.WriteSigned(writer, inValue);
            }
            catch
            {
                Console.WriteLine($"Failed to Write Value {inValue}");
                throw; // rethrow
            }

            reader.Reset(writer.ToArraySegment());
            int outValue;
            try
            {
                outValue = _model.ReadSigned(reader);
            }
            catch
            {
                Console.WriteLine($"Failed to Read Value {inValue}");
                throw; // rethrow
            }

            Assert.That(outValue, Is.EqualTo(inValue));
        }
    }
}
