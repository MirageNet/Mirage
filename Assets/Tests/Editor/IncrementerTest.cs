using NUnit.Framework;

namespace Mirage.Tests
{
    public class IncrementerTest
    {
        [Test]
        public void FirstValueIsIntial()
        {
            uint expected = 10u;
            var incrementer = new Incrementer(expected);
            uint next = incrementer.GetNext();
            Assert.That(next, Is.EqualTo(expected));
        }

        [Test]
        public void DefaultFirstShouldBeOne()
        {
            var incrementer = new Incrementer();
            uint next = incrementer.GetNext();
            Assert.That(next, Is.EqualTo(1u));
        }

        [Test]
        public void ValueShouldIncrement()
        {
            var incrementer = new Incrementer();

            for (uint i = 1u; i < 11u; i++)
            {
                uint next = incrementer.GetNext();
                Assert.That(next, Is.EqualTo(i));
            }
        }

        [Test]
        public void ResetShouldUseIntial()
        {
            var incrementer = new Incrementer();
            for (int i = 0; i < 10; i++)
            {
                _ = incrementer.GetNext();
            }

            uint expected = 45u;
            incrementer.Reset(expected);
            uint next = incrementer.GetNext();
            Assert.That(next, Is.EqualTo(expected));
        }
        [Test]
        public void ResetShouldDefaultToOne()
        {
            var incrementer = new Incrementer();
            for (int i = 0; i < 10; i++)
            {
                _ = incrementer.GetNext();
            }

            incrementer.Reset();
            uint next = incrementer.GetNext();
            Assert.That(next, Is.EqualTo(1u));
        }
    }
}
