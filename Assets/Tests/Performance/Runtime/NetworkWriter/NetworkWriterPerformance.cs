using Mirage.Serialization;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Mirage.Tests.Performance
{
    [Category("Performance")]
    public class NetworkWriterPerformance
    {
        // A Test behaves as an ordinary method
        [Test]
        [Performance]
        public void WritePackedInt32()
        {
            Measure.Method(WPackedInt32)
                .IterationsPerMeasurement(1000)
                .WarmupCount(10)
                .MeasurementCount(100)
                .Run();
        }

        static void WPackedInt32()
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                for (int i = 0; i < 37; i++)
                {
                    writer.WritePackedInt32(i * 1000);
                }
            }
        }

        [Test]
        [Performance]
        public void WriteInt32()
        {
            Measure.Method(WInt32)
                .IterationsPerMeasurement(1000)
                .WarmupCount(10)
                .MeasurementCount(100)
                .Run();
        }
        static void WInt32()
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                for (int i = 0; i < 37; i++)
                {
                    writer.WriteInt32(i * 1000);
                }
            }
        }


        [Test]
        [Performance]
        public void WriteGenericInt32()
        {
            Measure.Method(WGenericInt32)
                .IterationsPerMeasurement(1000)
                .WarmupCount(10)
                .MeasurementCount(100)
                .Run();
        }
        static void WGenericInt32()
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                for (int i = 0; i < 37; i++)
                {
                    writer.Write(i * 1000);
                }
            }
        }
    }
}
