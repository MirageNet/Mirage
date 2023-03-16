using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    public class BitHelperTest
    {
        [Test]
        [TestCase(2, 0.05f, ExpectedResult = 7)]
        [TestCase(100, 0.1f, ExpectedResult = 11)]
        [TestCase(.707f, 0.002f, ExpectedResult = 10)]
        [TestCase(1000, 0.1f, ExpectedResult = 15)]
        [TestCase(2000, 0.01f, ExpectedResult = 19)]
        // 1023 is 10 bits, but max is -+ so 11 bits
        [TestCase(1023, 1, ExpectedResult = 11)]
        [TestCase(16, 0.5f, ExpectedResult = 7)]
        public int ReturnCorrectBitCountForMaxPrecision(float max, float precision)
        {
            return BitHelper.BitCount(max, precision);
        }

        [Test]
        [TestCase(0, ExpectedResult = 0)]
        [TestCase(0b1UL, ExpectedResult = 1)]
        [TestCase(0b10UL, ExpectedResult = 2)]
        [TestCase(0b11UL, ExpectedResult = 2)]
        [TestCase(0b100UL, ExpectedResult = 3)]
        [TestCase(0b101UL, ExpectedResult = 3)]
        [TestCase(0b110UL, ExpectedResult = 3)]
        [TestCase(0b111UL, ExpectedResult = 3)]
        [TestCase(8UL, ExpectedResult = 4)]
        [TestCase(15UL, ExpectedResult = 4)]
        [TestCase(16UL, ExpectedResult = 5)]
        [TestCase(31UL, ExpectedResult = 5)]
        [TestCase(32UL, ExpectedResult = 6)]
        [TestCase(63UL, ExpectedResult = 6)]
        [TestCase(255UL, ExpectedResult = 8)]
        [TestCase(256UL, ExpectedResult = 9)]
        public int ReturnCorrectBitCountForRange(ulong range)
        {
            return BitHelper.BitCount(range);
        }
    }
}
