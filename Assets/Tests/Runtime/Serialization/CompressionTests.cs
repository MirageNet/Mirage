namespace Mirage.Tests.Runtime.Serialization
{
    public class CompressionTests
    {
        // todo re-add with compression
        /*
        [Test]
        [Repeat(100)]
        public void QuaternionCompression()
        {
            Quaternion expected = Random.rotation;

            uint compressed = Compression.Compress(expected);

            Quaternion decompressed = Compression.Decompress(compressed);

            // decompressed should be almost the same,  dot product of 2 normalized quaternion is 1 if they are the same
            Assert.That(Mathf.Abs(Quaternion.Dot(expected, decompressed)), Is.EqualTo(1).Within(0.001));
        }

        [Test]
        public void Compress90Degrees()
        {
            uint compressed = Compression.Compress(Quaternion.Euler(0, 90, 0));

            Quaternion decompressed = Compression.Decompress(compressed);

            Vector3 euler = decompressed.eulerAngles;
            Assert.That(euler.x, Is.Zero);
            Assert.That(euler.y, Is.EqualTo(90).Within(0.1));
            Assert.That(euler.z, Is.Zero);
        }

        [Test]
        public void CompressCornerCases(
            [Range(0, 360, 45)] int x,
            [Range(0, 360, 45)] int y,
            [Range(0, 360, 45)] int z
            )
        {
            var expected = Quaternion.Euler(x, y, z);

            uint compressed = Compression.Compress(Quaternion.Euler(x, y, z));

            Quaternion decompressed = Compression.Decompress(compressed);

            // decompressed should be almost the same,  dot product of 2 normalized quaternion is 1 if they are the same
            Assert.That(Mathf.Abs(Quaternion.Dot(expected, decompressed)), Is.EqualTo(1).Within(0.001));

        }
        */
    }
}
