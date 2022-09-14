using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    public class CompressionExtensionTests : PackerTestBase
    {
        [Test]
        public void PackRotationUsesDefault9()
        {
            writer.WriteQuaternion(Quaternion.identity);
            Assert.That(writer.BitPosition, Is.EqualTo(29));
        }

        [Test]
        public void UnpackRotationUsesDefault9()
        {
            // manually pack identity
            writer.Write(0, 29);
            var reader = GetReader();
            Assert.That(reader.BitPosition, Is.EqualTo(0), "Check it starts at 0");
            var value = reader.ReadQuaternion();
            Assert.That(reader.BitPosition, Is.EqualTo(29));
            Assert.That(value, Is.EqualTo(Quaternion.identity));
        }
    }
}
