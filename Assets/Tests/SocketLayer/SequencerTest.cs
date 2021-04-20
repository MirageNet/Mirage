using System;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests
{
    [Category("SocketLayer")]
    public class SequencerTest
    {
        [Test]
        public void StartsAt0([Range(1, 63)] int bits)
        {
            var sequencer = new Sequencer(bits);
            Assert.That(sequencer.Next(), Is.EqualTo(0));
        }

        [Test]
        public void ThrowsErrorIfBitsisOver63([Range(64, 70)] int bits)
        {
            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var sequencer = new Sequencer(bits);
            });

            var expected = new ArgumentOutOfRangeException("bits", bits, "Bits should be between 1 and 63");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }
        [Test]
        public void ThrowsErrorIfBitsisOver0([Range(-10, 0)] int bits)
        {
            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var sequencer = new Sequencer(bits);
            });

            var expected = new ArgumentOutOfRangeException("bits", bits, "Bits should be between 1 and 63");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void ItShouldRememberBitSize([Range(1, 63)] int bits)
        {
            var sequencer = new Sequencer(bits);
            Assert.That(sequencer.Bits, Is.EqualTo(bits));
        }

        [Test]
        public void ItShouldStartOverAfterLastSequenceNumber()
        {
            var sequencer = new Sequencer(2);
            Assert.That(sequencer.Next(), Is.EqualTo(0));
            Assert.That(sequencer.Next(), Is.EqualTo(1));
            Assert.That(sequencer.Next(), Is.EqualTo(2));
            Assert.That(sequencer.Next(), Is.EqualTo(3));
            Assert.That(sequencer.Next(), Is.EqualTo(0),
                "2 bit sequencer should wrap after 4 numbers");
        }

        [Test]
        public void ShouldReturnNegativeDistanceIfSecondIdComesAfter()
        {
            var sequencer = new Sequencer(8);
            Assert.That(sequencer.Distance(0, 8), Is.EqualTo(-8));
        }

        [Test]
        public void ShouldReturnPositiveDistanceIfSecondIdComesBefore()
        {
            var sequencer = new Sequencer(8);
            Assert.That(sequencer.Distance(8, 0), Is.EqualTo(8));
        }

        [Test]
        public void ValuesAfterWrappingConsideredToBeGreater()
        {
            var sequencer = new Sequencer(8);
            Assert.That(sequencer.Distance(254, 4), Is.EqualTo(-6));
        }
    }
}
