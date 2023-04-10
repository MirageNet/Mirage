using Mirage.Serialization;
using NUnit.Framework;
using Random = Mirage.Tests.BitPacking.TestRandom;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    [TestFixture(0.01f)]
    [TestFixture(0.02f)]
    [TestFixture(0.05f)]
    [TestFixture(0.1f)]
    [TestFixture(30)]
    public class AnglePackerTests : PackerTestBase
    {
        private readonly AnglePacker packer;
        private readonly float precsion;

        public AnglePackerTests(float precsion)
        {
            this.precsion = precsion;
            packer = new AnglePacker(precsion);
        }

        private float GetRandomAngle()
        {
            // dont use -180, that will wrap to 180
            return Random.Range(-179.9f, 180);
        }


        [Test]
        // takes about 1 seconds per 1000 values (including all fixtures)
        [Repeat(200)]
        public void UnpackedValueIsWithinPrecision()
        {
            var start = GetRandomAngle();
            var packed = packer.Pack(start);
            var unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }

        [Test]
        [TestCase(270, -90)]
        [TestCase(-560, 160)]
        public void AngleWillWrapToBeClosestToZero(float start, float expected)
        {
            var packed = packer.Pack(start);
            var unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(expected).Within(precsion));
        }

        [Test]
        // takes about 1 seconds per 1000 values (including all fixtures)
        [Repeat(200)]
        public void AngleWillWrapRepeat()
        {
            var start = GetRandomAngle();
            var packed = packer.Pack(start + 360);
            var unpacked = packer.Unpack(packed);

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
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
            var start = GetRandomAngle();
            packer.Pack(writer, start);
            var unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(start).Within(precsion));
        }


        [Test]
        [TestCase(270, -90)]
        [TestCase(-560, 160)]
        public void AngleWillWrapToBeClosestToZeroUsingWriter(float start, float expected)
        {
            packer.Pack(writer, start);
            var unpacked = packer.Unpack(GetReader());

            Assert.That(unpacked, Is.EqualTo(expected).Within(precsion));
        }
    }
}
