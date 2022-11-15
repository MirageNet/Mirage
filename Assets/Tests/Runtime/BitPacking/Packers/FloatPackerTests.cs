using Mirage.Serialization;
using NUnit.Framework;
using Random = Mirage.Tests.BitPacking.TestRandom;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    [TestFixture(100, 0.1f, true)]
    [TestFixture(500, 0.02f, true)]
    [TestFixture(2000, 0.05f, true)]
    [TestFixture(1.5f, 0.01f, true)]
    [TestFixture(100_000, 30, true)]

    [TestFixture(100, 0.1f, false)]
    [TestFixture(500, 0.02f, false)]
    [TestFixture(2000, 0.05f, false)]
    [TestFixture(1.5f, 0.01f, false)]
    [TestFixture(100_000, 30, false)]
    public class FloatPackerTests : PackerTestBase
    {
        private readonly FloatPacker packer;
        private readonly float max;
        private readonly float min;
        private readonly float precsion;
        private readonly bool signed;

        public FloatPackerTests(float max, float precsion, bool signed)
        {
            this.max = max;
            min = signed ? -max : 0;
            this.precsion = precsion;
            this.signed = signed;
            packer = new FloatPacker(max, precsion, signed);
        }

        private float GetRandomFloat()
        {
            return Random.Range(min, max);
        }


        [Test]
        // takes about 1 seconds per 1000 values (including all fixtures)
        [Repeat(1000)]
        public void UnpackedValueIsWithinPrecision()
        {
            var start = GetRandomFloat();
            var packed = packer.Pack(start);
            var unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }

        [Test]
        public void ValueOverMaxWillBeUnpackedAsMax()
        {
            var start = max * 1.2f;
            var packed = packer.Pack(start);
            var unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(max).Within(precsion));
        }

        [Test]
        public void ValueUnderNegativeMaxWillBeUnpackedAsNegativeMax()
        {
            var start = max * -1.2f;
            var packed = packer.Pack(start);
            var unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(min).Within(precsion));
        }

        [Test]
        public void ZeroUnpackToExactlyZero()
        {
            const float zero = 0;
            var packed = packer.Pack(zero);
            var unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(zero));
        }


        [Test]
        [Repeat(100)]
        public void UnpackedValueIsWithinPrecisionUsingWriter()
        {
            var start = GetRandomFloat();
            packer.Pack(writer, start);
            var unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }

        [Test]
        public void ValueOverMaxWillBeUnpackedUsingWriterAsMax()
        {
            var start = max * 1.2f;
            packer.Pack(writer, start);
            var unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(max).Within(precsion));
        }

        [Test]
        public void ValueUnderNegativeMaxWillBeUnpackedUsingWriterAsNegativeMax()
        {
            var start = max * -1.2f;
            packer.Pack(writer, start);
            var unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(min).Within(precsion));
        }


        [Test]
        [Repeat(100)]
        public void UnpackedValueIsWithinPrecisionNoClamp()
        {
            var start = GetRandomFloat();
            var packed = packer.PackNoClamp(start);
            var unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }

        [Test]
        [Repeat(100)]
        public void UnpackedValueIsWithinPrecisionNoClampUsingWriter()
        {
            var start = GetRandomFloat();
            packer.PackNoClamp(writer, start);
            var unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }
    }
}
