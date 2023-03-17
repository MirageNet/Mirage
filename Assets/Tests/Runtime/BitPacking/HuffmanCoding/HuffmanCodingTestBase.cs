using System;
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
                var groupCounts = new int[] { 1, 2, 3, 4, 8 };
                foreach (var count in groupCounts)
                {
                    yield return new object[] { count, DataType.GeneratedRandom };
                }
            }
        }
    }

    public enum DataType
    {
        Raw = 0,
        DeltaZeroPacked = 1,
        GeneratedRandom = 2,
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
            List<byte[]> raw = null;
            switch (_dataType)
            {
                case DataType.Raw:
                    throw new NotSupportedException("needs data set");
                    //raw = Debugging.LoadRaw();
                    break;
                case DataType.DeltaZeroPacked:
                    throw new NotSupportedException("needs data set");
                    //raw = Debugging.RawZeroPacked();
                    break;
                case DataType.GeneratedRandom:
                    raw = Debugging.CreateRaw();
                    break;
            }
            var data = TrainModel.CreateFrequencyData(_groupSize, raw);
            _tree = CreateTree.Create(data, maxDepth);
            _model = TrainModel.CreateModel(_groupSize, _tree);
        }
    }
}
