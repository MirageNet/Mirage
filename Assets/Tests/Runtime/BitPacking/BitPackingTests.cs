using System;
using System.Runtime.CompilerServices;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class BitPackingTests
    {
        private NetworkWriter writer;
        private NetworkReader reader;

        [SetUp]
        public void Setup()
        {
            // dont allow resizing for this test, because we test throw
            writer = new NetworkWriter(1300, false);
            reader = new NetworkReader();
        }

        [TearDown]
        public void TearDown()
        {
            writer.Reset();
            reader.Dispose();
        }

        [Test]
        [TestCase(0ul)]
        [TestCase(1ul)]
        [TestCase(0x_FFFF_FFFF_12Ul)]
        public void WritesCorrectUlongValue(ulong value)
        {
            writer.Write(value, 64);
            reader.Reset(writer.ToArray());

            var result = reader.Read(64);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0u)]
        [TestCase(1u)]
        [TestCase(0x_FFFF_FF12U)]
        public void WritesCorrectUIntValue(uint value)
        {
            writer.Write(value, 32);
            reader.Reset(writer.ToArray());

            var result = reader.Read(32);
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        [TestCase(0u, 10, 2u, 5)]
        [TestCase(10u, 10, 36u, 15)]
        [TestCase(1u, 1, 250u, 8)]
        public void WritesCorrectValues(uint value1, int bits1, uint value2, int bits2)
        {
            writer.Write(value1, bits1);
            writer.Write(value2, bits2);
            reader.Reset(writer.ToArray());

            var result1 = reader.Read(bits1);
            var result2 = reader.Read(bits2);
            Assert.That(result1, Is.EqualTo(value1));
            Assert.That(result2, Is.EqualTo(value2));
        }


        [Test]
        public void CanWriteToBufferLimit()
        {
            for (var i = 0; i < 208; i++)
            {
                writer.Write((ulong)i, 50);
            }

            var result = writer.ToArray();

            // written bits/8
            var expectedLength = (208 * 50) / 8;
            Assert.That(result, Has.Length.EqualTo(expectedLength));
        }

        [Test, Description("Real buffer size is 1304 because 1300 rounds up")]
        public void WriterThrowIfWritesTooMuch()
        {
            // write 1296 up to last word
            for (var i = 0; i < 162; i++)
            {
                writer.Write((ulong)i, 64);
            }

            writer.Write(0, 63);

            Assert.DoesNotThrow(() =>
            {
                writer.Write(0, 1);
            });

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                writer.Write(0, 1);
            });
            const int max = 162 * 64 + 64;
            Assert.That(exception, Has.Message.EqualTo($"Can not write over end of buffer, new length {max + 1}, capacity {max}"));
        }

        [Test]
        [Repeat(10)]
        public void WritesAllValueSizesCorrectly([Range(0, 63)] int startPosition, [Range(0, 64)] int valueBits)
        {
            var randomValue = ULongRandom.Next();
            writer.Write(0, startPosition);

            var maskedValue = randomValue & BitMask.Mask(valueBits);

            writer.Write(randomValue, valueBits);
            reader.Reset(writer.ToArray());

            _ = reader.Read(startPosition);
            var result = reader.Read(valueBits);
            Assert.That(result, Is.EqualTo(maskedValue));
        }

        [Test]
        public void WritesAllMasksCorrectly()
        {
            // we can't use [range] args because we have to skip cases where end is over 64
            var count = 0;
            for (var start = 0; start < 64; start++)
            {
                for (var bits = 0; bits < 64; bits++)
                {
                    var end = start + bits;
                    if (end > 64)
                    {
                        continue;
                    }

                    var expected = SlowMask(start, end);
                    var actual = BitMask.OuterMask(start, end);
                    count++;
                    if (expected != actual)
                    {
                        Assert.Fail($"Failed, start:{start} bits:{bits}");
                    }
                }
            }
            UnityEngine.Debug.Log($"{count} masks tested");
        }

        /// <summary>
        /// Slower but correct mask
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SlowMask(int start, int end)
        {
            // old mask, doesn't work when bitposition before/after is multiple of 64
            //           so we need to check if values == 0 before shifting masks
            var mask1 = start == 0 ? 0ul : (ulong.MaxValue >> (64 - start));
            // note: new position can not be 0, so no need to worry about 
            var mask2 = (end & 0b11_1111) == 0 ? 0ul : (ulong.MaxValue << end /*we can use full position here as c# will mask it to just 6 bits*/);
            // mask either side of value, eg writing 4 bits at position 3: 111...111_1000_0111
            var mask = mask1 | mask2;
            return mask;
        }
    }

    public static class ULongRandom
    {
        private static Random rand;
        private static byte[] bytes;

        public static unsafe ulong Next()
        {
            if (rand == null)
            {
                rand = new System.Random();
                bytes = new byte[8];
            }

            rand.NextBytes(bytes);
            fixed (byte* ptr = &bytes[0])
            {
                return *(ulong*)ptr;
            }
        }
    }
}
