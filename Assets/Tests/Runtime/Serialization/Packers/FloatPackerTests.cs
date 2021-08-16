using Mirage.Serialization;
using NUnit.Framework;
using Random = UnityEngine.Random;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    [TestFixture(100, 0.1f)]
    [TestFixture(500, 0.02f)]
    [TestFixture(2000, 0.05f)]
    [TestFixture(1.5f, 0.01f)]
    [TestFixture(100_000, 30)]
    public class FloatPackerTests : PackerTestBase
    {
        readonly FloatPacker packer;
        readonly float max;
        readonly float precsion;

        public FloatPackerTests(float max, float precsion)
        {
            this.max = max;
            this.precsion = precsion;
            packer = new FloatPacker(max, precsion);
        }


        float GetRandomFloat()
        {
            return Random.Range(-max, max);
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

            Assert.That(unpacked, Is.EqualTo(-max).Within(precsion));
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

            Assert.That(unpacked, Is.EqualTo(-max).Within(precsion));
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
