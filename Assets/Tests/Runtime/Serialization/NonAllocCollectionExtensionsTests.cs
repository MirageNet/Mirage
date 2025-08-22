using System.Collections.Generic;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class NonAllocCollectionExtensionsTests
    {
        private readonly NetworkWriter _writer = new NetworkWriter(1300);
        private readonly NetworkReader _reader = new NetworkReader();

        [TearDown]
        public void TearDown()
        {
            _writer.Reset();
            _reader.Dispose();
        }

        private NetworkReader GetReader()
        {
            _reader.Reset(_writer.ToArraySegment());
            return _reader;
        }

        #region List

        [Test]
        public void ReadListNonAlloc_NullValue()
        {
            _writer.WriteList<int>(null);
            var reader = GetReader();
            var list = new List<int>();
            reader.ReadListNonAlloc(list, out var wasNull);
            Assert.IsTrue(wasNull);
            Assert.IsEmpty(list);
        }

        [Test]
        public void ReadListNonAlloc_Empty()
        {
            _writer.WriteList(new List<int>());
            var reader = GetReader();
            var list = new List<int>();
            reader.ReadListNonAlloc(list, out var wasNull);
            Assert.IsFalse(wasNull);
            Assert.IsEmpty(list);
        }

        [Test]
        public void ReadListNonAlloc_WithData()
        {
            var data = new List<int> { 1, 2, 3 };
            _writer.WriteList(data);
            var reader = GetReader();
            var list = new List<int>();
            reader.ReadListNonAlloc(list, out var wasNull);
            Assert.IsFalse(wasNull);
            CollectionAssert.AreEqual(data, list);
        }

        [Test]
        public void ReadListNonAlloc_ClearsOldData()
        {
            var data = new List<int> { 1, 2, 3 };
            _writer.WriteList(data);
            var reader = GetReader();
            var list = new List<int> { 4, 5, 6, 7 };
            reader.ReadListNonAlloc(list, out var wasNull);
            Assert.IsFalse(wasNull);
            CollectionAssert.AreEqual(data, list);
        }

        #endregion

        #region Array

        [Test]
        public void ReadArrayNonAlloc_NullValue()
        {
            _writer.WriteArray<int>(null);
            var reader = GetReader();
            var array = new int[0];
            var count = reader.ReadArrayNonAlloc(array);
            Assert.IsFalse(count.HasValue);
        }

        [Test]
        public void ReadArrayNonAlloc_Empty()
        {
            _writer.WriteArray(new int[0]);
            var reader = GetReader();
            var array = new int[0];
            var count = reader.ReadArrayNonAlloc(array);
            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(0, count.Value);
        }

        [Test]
        public void ReadArrayNonAlloc_WithData()
        {
            var data = new int[] { 1, 2, 3 };
            _writer.WriteArray(data);
            var reader = GetReader();
            var array = new int[3];
            var count = reader.ReadArrayNonAlloc(array);
            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(3, count.Value);
            CollectionAssert.AreEqual(data, array);
        }

        [Test]
        public void ReadArrayNonAlloc_ThrowsIfTooSmall()
        {
            var data = new int[] { 1, 2, 3 };
            _writer.WriteArray(data);
            var reader = GetReader();
            var array = new int[2];
            Assert.Throws<System.ArgumentException>(() =>
            {
                reader.ReadArrayNonAlloc(array);
            });
        }

        #endregion

        #region Dictionary

        [Test]
        public void ReadDictionaryNonAlloc_NullValue()
        {
            _writer.WriteDictionary<int, string>(null);
            var reader = GetReader();
            var dict = new Dictionary<int, string>();
            reader.ReadDictionaryNonAlloc(dict, out var wasNull);
            Assert.IsTrue(wasNull);
            Assert.IsEmpty(dict);
        }

        [Test]
        public void ReadDictionaryNonAlloc_Empty()
        {
            _writer.WriteDictionary(new Dictionary<int, string>());
            var reader = GetReader();
            var dict = new Dictionary<int, string>();
            reader.ReadDictionaryNonAlloc(dict, out var wasNull);
            Assert.IsFalse(wasNull);
            Assert.IsEmpty(dict);
        }

        [Test]
        public void ReadDictionaryNonAlloc_WithData()
        {
            var data = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } };
            _writer.WriteDictionary(data);
            var reader = GetReader();
            var dict = new Dictionary<int, string>();
            reader.ReadDictionaryNonAlloc(dict, out var wasNull);
            Assert.IsFalse(wasNull);
            CollectionAssert.AreEqual(data, dict);
        }

        [Test]
        public void ReadDictionaryNonAlloc_ClearsOldData()
        {
            var data = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } };
            _writer.WriteDictionary(data);
            var reader = GetReader();
            var dict = new Dictionary<int, string> { { 3, "three" } };
            reader.ReadDictionaryNonAlloc(dict, out var wasNull);
            Assert.IsFalse(wasNull);
            CollectionAssert.AreEqual(data, dict);
        }

        #endregion

        #region Span

#if UNITY_2021_3_OR_NEWER
        [Test]
        public void ReadSpan_WithData()
        {
            var data = new int[] { 1, 2, 3 };
            _writer.WriteSpan<int>(data);
            var reader = GetReader();
            var span = new System.Span<int>(new int[3]);
            var count = reader.ReadSpanNonAlloc(span);
            Assert.AreEqual(3, count);
            CollectionAssert.AreEqual(data, span.ToArray());
        }

        [Test]
        public void ReadSpan_ThrowsIfTooSmall()
        {
            var data = new int[] { 1, 2, 3 };
            _writer.WriteSpan<int>(data);
            var reader = GetReader();
            Assert.Throws<System.ArgumentException>(() =>
            {
                var span = new System.Span<int>(new int[2]);
                reader.ReadSpanNonAlloc(span);
            });
        }
#endif

        #endregion
    }
}
