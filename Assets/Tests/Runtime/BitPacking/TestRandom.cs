using System;

namespace Mirage.Tests.BitPacking
{
    public static class TestRandom
    {
        static Random random = new Random();
        public static float Range(float a, float b)
        {
            return (float)((random.NextDouble() * (b - a)) + a);
        }
        public static int Range(int a, int b)
        {
            return random.Next(a, b);
        }
    }
}
