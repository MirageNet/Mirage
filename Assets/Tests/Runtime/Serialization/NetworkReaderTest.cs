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
                try
                {
                    reader.ReadBytes(bytes, bytes.Length + 1);
                    // BAD: IF WE GOT HERE, THEN NO EXCEPTION WAS THROWN
                    Assert.Fail();
                }
                catch (EndOfStreamException)
                {
                    // GOOD
                }
            }
        }
    }
}
