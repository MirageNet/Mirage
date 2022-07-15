using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests
{
    [TestFixture]
    public class NetworkTransformTest
    {
        private readonly NetworkWriter writer = new NetworkWriter(1300);
        private readonly NetworkReader reader = new NetworkReader();

        [TearDown]
        public void TearDown()
        {
            writer.Reset();
            reader.Dispose();
        }

        [Test]
        public void SerializeIntoWriterTest()
        {
            var position = new Vector3(1, 2, 3);
            Quaternion rotation = new Quaternion(0.1f, 0.2f, 0.3f, 0.4f).normalized;
            var scale = new Vector3(0.5f, 0.6f, 0.7f);

            NetworkTransformBase.SerializeIntoWriter(writer, position, rotation, scale);

            reader.Reset(writer.ToArraySegment());

            Assert.That(reader.ReadVector3(), Is.EqualTo(position));
            Quaternion actual = reader.ReadQuaternion();
            Assert.That(actual.x, Is.EqualTo(rotation.x).Within(0.01f));
            Assert.That(actual.y, Is.EqualTo(rotation.y).Within(0.01f));
            Assert.That(actual.z, Is.EqualTo(rotation.z).Within(0.01f));
            Assert.That(actual.w, Is.EqualTo(rotation.w).Within(0.01f));
            Assert.That(reader.ReadVector3(), Is.EqualTo(scale));
        }
    }
}
