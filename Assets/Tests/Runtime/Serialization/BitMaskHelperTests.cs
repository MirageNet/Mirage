using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class BitMaskHelperTests
    {
        /// <summary>
        /// slow way of creating correct mask
        /// </summary>
        static ulong slowMask(int bits)
        {
            ulong mask = 0;
            for (int i = 0; i < bits; i++)
            {
                mask |= 1ul << i;
            }

            return mask;
        }

        [Test]
        [Description("manually checking edge cases to be sure")]
        public void MaskValueIsCorrect0()
        {
            ulong mask = BitMask.Mask(0);
            Assert.That(mask, Is.EqualTo(0x0));
        }

        [Test]
        [Description("manually checking edge cases to be sure")]
        public void MaskValueIsCorrect63()
        {
            ulong mask = BitMask.Mask(63);
            Assert.That(mask, Is.EqualTo(0x7FFF_FFFF_FFFF_FFFF));
        }

        [Test]
        [Description("manually checking edge cases to be sure")]
        public void MaskValueIsCorrect64()
        {
            ulong mask = BitMask.Mask(64);
            Assert.That(mask, Is.EqualTo(0xFFFF_FFFF_FFFF_FFFF));
        }

        [Test]
        public void MaskValueIsCorrect([Range(0, 64)] int bits)
        {
            ulong mask = BitMask.Mask(bits);
            ulong expected = slowMask(bits);
            Assert.That(mask, Is.EqualTo(expected), $"    mask:{mask:X}\nexpected:{expected:X}");
        }
    }
}
