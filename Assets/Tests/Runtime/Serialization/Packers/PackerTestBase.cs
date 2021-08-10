using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization.Packers
{
    public class PackerTestBase
    {
        public readonly NetworkWriter writer = new NetworkWriter(1300);
        NetworkReader reader = new NetworkReader();

        [TearDown]
        public virtual void TearDown()
        {
            writer.Reset();
            reader.Dispose();
        }

        /// <summary>
        /// Gets Reader using the current data inside writer
        /// </summary>
        /// <returns></returns>
        public NetworkReader GetReader()
        {
            reader.Reset(writer.ToArraySegment());
            return reader;
        }
    }
}
