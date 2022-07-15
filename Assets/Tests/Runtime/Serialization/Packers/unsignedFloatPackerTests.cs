using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    public class UnsignedFloatPackerTests : PackerTestBase
    {
        private FloatPacker packer;
        private float max;
        private float precsion;

        [SetUp]
        public void Setup()
        {
            max = 100;
            precsion = 1 / 1000f;
            packer = new FloatPacker(max, precsion, false);
        }

        [Test]
        public void ClampsToZero()
        {
            packer.Pack(writer, -4.5f);
            var outValue = packer.Unpack(GetReader());

            Assert.That(outValue, Is.Zero);
        }

        [Test]
        public void CanWriteNearMax()
        {
            const float value = 99.5f;
            packer.Pack(writer, value);
            var outValue = packer.Unpack(GetReader());

            Assert.That(outValue, Is.EqualTo(value).Within(precsion));
        }
    }
}
