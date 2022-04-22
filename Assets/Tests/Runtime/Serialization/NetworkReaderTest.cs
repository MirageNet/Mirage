using System;
using System.IO;
using Mirage.Serialization;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    // NetworkWriterTest already covers most cases for NetworkReader.
    // only a few are left
    [TestFixture]
    public class NetworkReaderTest
    {
        [Test]
        public void ReadBytesCountTooBigTest()
        {
            // calling ReadBytes with a count bigger than what is in Reader
            // should throw an exception
            byte[] bytes = { 0x00, 0x01 };

            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(bytes, null))
            {
                Assert.Throws<EndOfStreamException>(() =>
                {
                    reader.ReadBytes(bytes, 0, bytes.Length + 1);
                });
            }
        }

        [Test]
        public void GettingReaderSetObjectLocator_WithBytes()
        {
            IObjectLocator locator = Substitute.For<IObjectLocator>();
            byte[] bytes = { 0x00, 0x01 };
            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(bytes, locator))
            {
                Assert.That(reader.ObjectLocator, Is.EqualTo(locator));
            }
        }

        [Test]
        public void GettingReaderSetObjectLocator_WithSegment()
        {
            IObjectLocator locator = Substitute.For<IObjectLocator>();
            byte[] bytes = { 0x00, 0x01 };
            var segment = new ArraySegment<byte>(bytes);
            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(segment, locator))
            {
                Assert.That(reader.ObjectLocator, Is.EqualTo(locator));
            }
        }

        [Test]
        public void GettingReaderSetObjectLocator_WithBytesAndLength()
        {
            IObjectLocator locator = Substitute.For<IObjectLocator>();
            byte[] bytes = { 0x00, 0x01 };
            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(bytes, 0, 2, locator))
            {
                Assert.That(reader.ObjectLocator, Is.EqualTo(locator));
            }
        }

        [Test]
        public void ThrowsIfNotMirageReader()
        {
            var reader = new NetworkReader();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                _ = reader.ToMirageReader();
            });
            Assert.That(exception, Has.Message.EqualTo("Reader is not MirageNetworkReader"));
        }
    }
}
