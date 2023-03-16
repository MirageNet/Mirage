using System.Collections.Generic;
using Mirage.Serialization.HuffmanCoding;
using Mirage.Serialization.HuffmanCoding.Training;

namespace Mirage.Tests.Runtime.Serialization.HuffmanCoding
{
    public static class TestSource
    {
        public static IEnumerable<object[]> Source
        {
            get
            {
                for (var i = 0; i < 2; i++)
                {
                    var groupCounts = new int[] { 1, 2, 3, 4, 8 };
                    foreach (var count in groupCounts)
                    {
                        yield return new object[] { count, (DataType)i };
                    }
                }
            }
        }
    }

    public enum DataType
    {
        Raw = 0,
        DeltaZeroPacked = 1,
    }

    public abstract class HuffmanCodingTestBase
    {

        protected readonly int _groupSize;
        protected readonly DataType _dataType;
        protected Tree _tree;
        protected HuffmanCodingModel _model;

        public HuffmanCodingTestBase(int groupSize, DataType dataType)
        {
            _groupSize = groupSize;
            _dataType = dataType;
        }

        protected void Train(int? maxDepth)
        {
            var raw = _dataType == DataType.Raw ? Debugging.LoadRaw() : Debugging.RawZeroPacked();
            var data = TrainModel.CreateFrequencyData(_groupSize, raw);
            _tree = CreateTree.Create(data, maxDepth);
            _model = TrainModel.CreateModel(_groupSize, _tree);
        }
    }
}
