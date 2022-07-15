using Mirage.Serialization;
using NUnit.Framework;
using Random = UnityEngine.Random;

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
            float start = GetRandomFloat();
            uint packed = packer.Pack(start);
            float unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }

        [Test]
        public void ValueOverMaxWillBeUnpackedAsMax()
        {
            float start = max * 1.2f;
            uint packed = packer.Pack(start);
            float unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(max).Within(precsion));
        }

        [Test]
        public void ValueUnderNegativeMaxWillBeUnpackedAsNegativeMax()
        {
            float start = max * -1.2f;
            uint packed = packer.Pack(start);
            float unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(min).Within(precsion));
        }

        [Test]
        public void ZeroUnpackToExactlyZero()
        {
            const float zero = 0;
            uint packed = packer.Pack(zero);
            float unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(zero));
        }


        [Test]
        [Repeat(100)]
        public void UnpackedValueIsWithinPrecisionUsingWriter()
        {
            float start = GetRandomFloat();
            packer.Pack(writer, start);
            float unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }

        [Test]
        public void ValueOverMaxWillBeUnpackedUsingWriterAsMax()
        {
            float start = max * 1.2f;
            packer.Pack(writer, start);
            float unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(max).Within(precsion));
        }

        [Test]
        public void ValueUnderNegativeMaxWillBeUnpackedUsingWriterAsNegativeMax()
        {
            float start = max * -1.2f;
            packer.Pack(writer, start);
            float unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(min).Within(precsion));
        }


        [Test]
        [Repeat(100)]
        public void UnpackedValueIsWithinPrecisionNoClamp()
        {
            float start = GetRandomFloat();
            uint packed = packer.PackNoClamp(start);
            float unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }

        [Test]
        [Repeat(100)]
        public void UnpackedValueIsWithinPrecisionNoClampUsingWriter()
        {
            float start = GetRandomFloat();
            packer.PackNoClamp(writer, start);
            float unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }
    }
}
