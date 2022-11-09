using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    public class CompressionExtensionTests
    {
        private readonly NetworkWriter _writer = new NetworkWriter(1300);
        private readonly NetworkReader _reader = new NetworkReader();

        [TearDown]
        public virtual void TearDown()
        {
            _writer.Reset();
            _reader.Dispose();
        }

        /// <summary>
        /// Gets Reader using the current data inside writer
        /// </summary>
        /// <returns></returns>
        private NetworkReader GetReader()
        {
            _reader.Reset(_writer.ToArraySegment());
            return _reader;
        }

        [Test]
        public void PackRotationUsesDefault9()
        {
            _writer.WriteQuaternion(Quaternion.identity);
            Assert.That(_writer.BitPosition, Is.EqualTo(29));
        }

        [Test]
        public void UnpackRotationUsesDefault9()
        {
            // manually pack identity
            _writer.Write(0, 29);
            var reader = GetReader();
            Assert.That(reader.BitPosition, Is.EqualTo(0), "Check it starts at 0");
            var value = reader.ReadQuaternion();
            Assert.That(reader.BitPosition, Is.EqualTo(29));
            Assert.That(value, Is.EqualTo(Quaternion.identity));
        }
    }
}
