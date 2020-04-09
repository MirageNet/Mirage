using NUnit.Framework;

namespace Mirror.Tests
{
    public class ExceptionTest : InvalidMessageException
    {
        public void ThrowsOne()
        {
            throw new InvalidMessageException();
        }

        public void ThrowsTwo()
        {
            throw new InvalidMessageException("Test Message");
        }

        public void ThrowsThree()
        {
            throw new InvalidMessageException("Test Message Too", new System.Exception());
        }
    }

    [TestFixture]
    public class InvalidMessageExceptionTest
    {
        [Test]
        public void InvalidMessageTest()
        {
            ExceptionTest exceptionTest = new ExceptionTest();

            InvalidMessageException ex = Assert.Throws<InvalidMessageException>(() =>
            {
                exceptionTest.ThrowsOne();
            });

            ex = Assert.Throws<InvalidMessageException>(() =>
            {
                exceptionTest.ThrowsTwo();
            });

            Assert.That(ex.Message, Is.EqualTo("Test Message"));

            ex = Assert.Throws<InvalidMessageException>(() =>
            {
                exceptionTest.ThrowsThree();
            });

            Assert.That(ex.Message, Is.EqualTo("Test Message Too"));
            Assert.That(ex.InnerException, Is.TypeOf(typeof(System.Exception)));
        }
    }
}
