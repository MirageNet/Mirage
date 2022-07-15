using NUnit.Framework;

namespace Mirage.SocketLayer.Tests
{
    [Category("SocketLayer")]
    [TestFixture("hello")]
    [TestFixture("Mirage V123")]
    public class ConnectKeyValidatorTest
    {
        private ConnectKeyValidator validator;

        public ConnectKeyValidatorTest(string key)
        {
            validator = new ConnectKeyValidator(key);
        }

        [Test]
        public void KeyIsSameEachCall()
        {
            byte[] buffer1 = new byte[50];
            byte[] buffer2 = new byte[50];
            validator.CopyTo(buffer1);
            validator.CopyTo(buffer2);

            Assert.That(buffer1, Is.EquivalentTo(buffer2), "buffers should have same values");
        }

        [Test]
        public void ValidateReturnsTrueIfKeyIsCorrect()
        {
            byte[] buffer = new byte[50];
            validator.CopyTo(buffer);

            bool valid = validator.Validate(buffer);
            Assert.IsTrue(valid);
        }

        [Test]
        public void ValidateReturnsFalseIfKeyIsCorrect()
        {
            byte[] buffer = new byte[50];
            validator.CopyTo(buffer);
            // corrupt 1 byte
            buffer[4] = 0;

            bool valid = validator.Validate(buffer);
            Assert.IsFalse(valid);
        }

        [Test]
        [TestCase(1, 2)]
        [TestCase(10, 220)]
        public void DoesNotOverWriteFirst2Indexes(byte index1, byte index2)
        {
            // use tests cases so we check that it doesn't change atleast 2 sets of values
            byte[] buffer = new byte[50];
            buffer[0] = index1;
            buffer[1] = index2;
            validator.CopyTo(buffer);

            Assert.That(buffer[0], Is.EqualTo(index1));
            Assert.That(buffer[1], Is.EqualTo(index2));
        }
    }
}
