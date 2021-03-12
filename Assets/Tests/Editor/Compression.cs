using NUnit.Framework;
using UnityEngine;

namespace Mirage
{
    public class CompressionTests
    {

        [Test]
        [Repeat(100)]
        public void QuaternionCompression()
        {
            Quaternion expected = Random.rotation;

            uint compressed = Compression.Compress(expected);

            Quaternion decompressed = Compression.Decompress(compressed);

            // decompressed should be almost the same
            Assert.That(Mathf.Abs(Quaternion.Dot(expected, decompressed)), Is.GreaterThan(1 - 0.001));
        }

        [Test]
        public void Compress90Degrees()
        {
            uint compressed = Compression.Compress(Quaternion.Euler(0,90,0));

            Quaternion decompressed = Compression.Decompress(compressed);

            Vector3 euler = decompressed.eulerAngles;
            Assert.That(euler.y, Is.EqualTo(90).Within(0.1));
        }
    }
}
