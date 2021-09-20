using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests
{
    [Description("Test first few cases to ensure logic doesn't have typo")]
    class FromBitCountTest
    {
        [Test]
        public void b1Is1()
        {
            Assert.That(FromBitCount.b1, Is.EqualTo(1));
        }

        [Test]
        public void b2Is3()
        {
            Assert.That(FromBitCount.b2, Is.EqualTo(3));
        }

        [Test]
        public void b3Is7()
        {
            Assert.That(FromBitCount.b3, Is.EqualTo(7));
        }

        [Test]
        public void b4Is15()
        {
            Assert.That(FromBitCount.b4, Is.EqualTo(15));
        }

        [Test]
        public void b8Is255()
        {
            Assert.That(FromBitCount.b8, Is.EqualTo(255));
        }

        [Test]
        public void b32IsIntMax()
        {
            Assert.That(FromBitCount.b32, Is.EqualTo(uint.MaxValue));
        }

        [Test]
        public void b64IsMax()
        {
            Assert.That(FromBitCount.b64, Is.EqualTo(ulong.MaxValue));
        }
    }
}
