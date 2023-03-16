using System;
using System.Collections.Generic;
using Mirage.Serialization.HuffmanCoding.Training;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization.HuffmanCoding
{
    [TestFixtureSource(typeof(TestSource), nameof(TestSource.Source))]
    public class HuffmanCodingTest_Training : HuffmanCodingTestBase
    {
        public HuffmanCodingTest_Training(int groupSize, DataType dataType) : base(groupSize, dataType) { }

        [Test]
        [TestCase(false, 0)]
        [TestCase(true, 6)]
        [Description("Logs tree")]
        public void DebugWalk(bool useLimit, int limit)
        {
            Assert.DoesNotThrow(() =>
            {
                Train(useLimit ? limit : default(int?));
                Debugging.Walk(_tree);
            });
        }

        [Test]
        public void DebugDecodeTable()
        {
            Assert.DoesNotThrow(() =>
            {
                Train(6);
                TrainModel.PrintDecodeTable(_model);
            });
        }


        [Test]
        public void CreatesWithoutError()
        {
            Train(null);

            Assert.That(_tree, Is.Not.Null);
            Assert.That(_tree.Root, Is.Not.Null);
        }

        [Test]
        public void NodesHaveCorrectDepth()
        {
            Train(null);

            _tree.Walk((node, _, depth) =>
            {
                if (node.Depth != depth)
                    Assert.Fail($"Node had incorrect depth, node.Depth={node.Depth}, walk Depth={depth}");
            });
        }

        [Test]
        [TestCase(false, 0)]
        [TestCase(true, 6)]
        public void MakeSureHighFrequenciesHaveLessDepth(bool useLimit, int limit)
        {
            Train(useLimit ? limit : default(int?));

            var minMax = new List<MinMax>();
            _tree.Walk((node, _, depth) =>
            {
                if (depth >= minMax.Count)
                    minMax.Add(MinMax.Default);

                // only use leafs, we dont care if branches have total higher
                if (node.IsLeaf)
                {
                    var count = node.Data.Frequency;
                    minMax[depth] = MinMax.Compare(minMax[depth], count);
                }
            });

            for (var d1 = 0; d1 < minMax.Count; d1++)
            {
                var d1Min = minMax[d1].Min;

                for (var d2 = 0; d2 < minMax.Count; d2++)
                {
                    // only check when d1 is less than d2
                    // this reducdes the number of check, and makes checking simplier becuase are just checking 1 way
                    if (d1 >= d2)
                        continue;

                    // check that the MIN count in d1 is higher (or equal?) than MAX count in d2
                    var d2Max = minMax[d2].Max;
                    // fail is d1 is smaller than d2
                    if (d1Min < d2Max)
                    {
                        // if is a lot faster than assert.that, so use Assert.Fail 
                        Assert.Fail($"MAX count at depth:{d2} (max = {d2Max}) was higher than MIN count at depth:{d1} (min = {d1Min})");
                    }
                }
            }
        }

        [Test]
        [TestCase(6)]
        [TestCase(8)]
        public void LimitDepth(int limit)
        {
            Train(limit);
            Assert.That(_tree.GetMaxDepth(), Is.LessThanOrEqualTo(limit));
        }

        [Test]
        [TestCase(40)] // make sure limit is over 32, because we test with group of 1 bits
        public void LimitDepthDoesNotChangeIfAbove(int limit)
        {
            Train(null);
            var startingDepth = _tree.GetMaxDepth();

            Train(limit);

            var newDepth = _tree.GetMaxDepth();
            Assert.That(newDepth, Is.EqualTo(startingDepth));
        }


        private struct MinMax
        {
            public readonly int Min;
            public readonly int Max;

            public MinMax(int min, int max)
            {
                Min = min;
                Max = max;
            }

            public static MinMax Default => new MinMax(int.MaxValue, int.MinValue);

            public static MinMax Compare(MinMax minMax, int count)
            {
                var min = Math.Min(minMax.Min, count);
                var max = Math.Max(minMax.Max, count);

                return new MinMax(min, max);
            }
        }
    }
}
