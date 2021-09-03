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
            writer.PackRotation(Quaternion.identity);
            Assert.That(writer.BitPosition, Is.EqualTo(29));
        }

        [Test]
        public void UnpackRotationUsesDefault9()
        {
            writer.Write(0, 27);
            writer.Write(3, 2);
            NetworkReader reader = GetReader();
            Assert.That(reader.BitPosition, Is.EqualTo(0), "Check it starts at 0");
            Quaternion value = reader.UnpackRotation();
            Assert.That(reader.BitPosition, Is.EqualTo(29));
            Assert.That(value, Is.EqualTo(Quaternion.identity));
        }
    }
}
