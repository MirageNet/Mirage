using Mirage.Serialization.HuffmanCoding.Training;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization.HuffmanCoding
{
    [TestFixtureSource(typeof(TestSource), nameof(TestSource.Source))]
    public class HuffmanCodingTest_SaveLoad : HuffmanCodingTestBase
    {
        public HuffmanCodingTest_SaveLoad(int groupSize, DataType dataType) : base(groupSize, dataType) { }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Train(6);
        }

        [Test]
        public void ModelCanConvertToJsonAndBack()
        {
            var json = LoadModel.ToJson(_model);
            var loaded = LoadModel.FromJson(json);

            Assert.That(loaded._maxPrefixLength, Is.EqualTo(_model._maxPrefixLength));
            Assert.That(loaded._groupSize, Is.EqualTo(_model._groupSize));
            CollectionAssert.AreEquivalent(loaded._prefixes, _model._prefixes);
            CollectionAssert.AreEquivalent(loaded._decodeTable, _model._decodeTable);
        }
    }
}
