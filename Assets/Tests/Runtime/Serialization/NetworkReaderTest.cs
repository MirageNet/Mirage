using System.IO;
using Mirage.Serialization;
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

            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(bytes))
            {
                Assert.Throws<EndOfStreamException>(() =>
                {
                    reader.ReadBytes(bytes, 0, bytes.Length + 1);
                });
            }
        }
    }
}
