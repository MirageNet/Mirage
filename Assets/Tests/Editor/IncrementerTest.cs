using System;
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
            uint next = incrementer.Next();
            Assert.That(next, Is.EqualTo(expected));
        }

        [Test]
        public void DefaultFirstShouldBeOne()
        {
            var incrementer = new Incrementer();
            uint next = incrementer.Next();
            Assert.That(next, Is.EqualTo(1u));
        }

        [Test]
        public void ValueShouldIncrement()
        {
            var incrementer = new Incrementer();

            for (uint i = 1u; i < 11u; i++)
            {
                uint next = incrementer.Next();
                Assert.That(next, Is.EqualTo(i));
            }
        }

        [Test]
        public void ResetShouldUseIntial()
        {
            var incrementer = new Incrementer();
            for (int i = 0; i < 10; i++)
            {
                _ = incrementer.Next();
            }

            uint expected = 45u;
            incrementer.Reset(expected);
            uint next = incrementer.Next();
            Assert.That(next, Is.EqualTo(expected));
        }
        [Test]
        public void ResetShouldDefaultToOne()
        {
            var incrementer = new Incrementer();
            for (int i = 0; i < 10; i++)
            {
                _ = incrementer.Next();
            }

            incrementer.Reset();
            uint next = incrementer.Next();
            Assert.That(next, Is.EqualTo(1u));
        }

        [Test]
        public void ShouldThrowOverFlow()
        {
            var incrementer = new Incrementer(uint.MaxValue);
            OverflowException exception = Assert.Throws<OverflowException>(() =>
            {
                _ = incrementer.Next();
            });
        }
    }
}
