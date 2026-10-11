using System.Collections.Generic;
using NUnit.Framework;

namespace Mirage.Tests.Runtime
{
    [TestFixture]
    public class AutoPoolTest
    {
        private class TestItem
        {
            public int Value;
        }

        [Test]
        public void TakeReturnsNonNullWrapperAndItem()
        {
            using var wrapper = AutoPool<TestItem>.Take();

            Assert.That(wrapper, Is.Not.Null);
            Assert.That(wrapper.Item, Is.Not.Null);
        }

        [Test]
        public void TakeWithOutParameterOutputsSameItem()
        {
            using var wrapper = AutoPool<TestItem>.Take(out var item);

            Assert.That(wrapper, Is.Not.Null);
            Assert.That(item, Is.Not.Null);
            Assert.That(item, Is.SameAs(wrapper.Item));
        }

        [Test]
        public void ImplicitConversionReturnsItem()
        {
            using var wrapper = AutoPool<TestItem>.Take();
            TestItem item = wrapper;

            Assert.That(item, Is.SameAs(wrapper.Item));
        }

        [Test]
        public void DisposingReturnsToPoolForReuse()
        {
            TestItem firstItem;
            using (var wrapper = AutoPool<TestItem>.Take(out var item))
            {
                firstItem = item;
                firstItem.Value = 42;
            }

            // taking again from pool should reuse the returned instance
            using (var wrapper = AutoPool<TestItem>.Take(out var reusedItem))
            {
                Assert.That(reusedItem, Is.SameAs(firstItem));
            }
        }

        [Test]
        public void DisposingClearsList()
        {
            List<int> listRef;
            using (AutoPool<List<int>>.Take(out var list))
            {
                listRef = list;
                list.Add(1);
                list.Add(2);
                list.Add(3);
                Assert.That(list.Count, Is.EqualTo(3));
            }

            // dispose clears lists so the next consumer receives an empty collection
            Assert.That(listRef.Count, Is.EqualTo(0));
        }
    }
}
