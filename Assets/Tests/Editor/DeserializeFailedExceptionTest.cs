using NUnit.Framework;

namespace Mirage
{
    [TestFixture]
    public class DeserializeFailedExceptionTest
    {
        [Test]
        public void InvalidMessageTest()
        {
            Assert.Throws<DeserializeFailedException>(() =>
            {
                throw new DeserializeFailedException();
            });
        }

        [Test]
        public void InvalidMessageWithTextTest()
        {
            DeserializeFailedException ex = Assert.Throws<DeserializeFailedException>(() =>
            {
                throw new DeserializeFailedException("Test Message");
            });

            Assert.That(ex.Message, Is.EqualTo("Test Message"));
        }

        [Test]
        public void InvalidMessageWithTextAndInnerTest()
        {
            DeserializeFailedException ex = Assert.Throws<DeserializeFailedException>(() =>
            {
                throw new DeserializeFailedException("Test Message Too", new System.Exception());
            });

            Assert.That(ex.Message, Is.EqualTo("Test Message Too"));
            Assert.That(ex.InnerException, Is.TypeOf(typeof(System.Exception)));
        }
    }
}
