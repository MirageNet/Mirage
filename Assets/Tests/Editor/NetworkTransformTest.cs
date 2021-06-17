using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage
{
    [TestFixture]
    public class NetworkTransformTest
    {
        NetworkWriter writer = new NetworkWriter(1300);
        NetworkReader reader = new NetworkReader();

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
            var rotation = new Quaternion(0.1f, 0.2f, 0.3f, 0.4f);
            var scale = new Vector3(0.5f, 0.6f, 0.7f);

            NetworkTransformBase.SerializeIntoWriter(writer, position, rotation, scale);

            reader.Reset(writer.ToArraySegment());

            Assert.That(reader.ReadVector3(), Is.EqualTo(position));
            Assert.That(reader.ReadQuaternion(), Is.EqualTo(rotation));
            Assert.That(reader.ReadVector3(), Is.EqualTo(scale));
        }
    }
}
