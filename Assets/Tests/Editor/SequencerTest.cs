using NUnit.Framework;

namespace Mirage
{
    public class SequencerTest
    {
        [Test]
        public void FirstValueShouldBe1()
        {
            var sequencer = new Sequencer(3);
            Assert.That(sequencer.Next(), Is.EqualTo(1));
        }

        [Test]
        public void ItShouldRememberBitSize()
        {
            var sequencer = new Sequencer(3);
            Assert.That(sequencer.Bits, Is.EqualTo(3));
        }

        [Test]
        public void ItShouldStartOverAfterLastSequenceNumber()
        {
            var sequencer = new Sequencer(2);
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
