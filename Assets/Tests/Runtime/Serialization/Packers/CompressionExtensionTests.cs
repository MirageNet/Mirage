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
    }
}
