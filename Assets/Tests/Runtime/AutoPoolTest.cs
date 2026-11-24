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

        [Test]
        public void DisposingClearsListFromTakeList()
        {
            List<int> listRef;
            using (AutoPool.TakeList<int>(out var list))
            {
                listRef = list;
                list.Add(1);
                list.Add(2);
                Assert.That(list.Count, Is.EqualTo(2));
            }

            Assert.That(listRef.Count, Is.EqualTo(0));
        }

        [Test]
        public void DisposingClearsDictionary()
        {
            Dictionary<int, int> dictionaryRef;
            using (AutoPool.TakeDictionary<int, int>(out var dictionary))
            {
                dictionaryRef = dictionary;
                dictionary.Add(1, 10);
                dictionary.Add(2, 20);
                Assert.That(dictionary.Count, Is.EqualTo(2));
            }

            Assert.That(dictionaryRef.Count, Is.EqualTo(0));
        }

        [Test]
        public void DisposingClearsHashSet()
        {
            HashSet<int> setRef;
            using (AutoPool.TakeHashSet<int>(out var set))
            {
                setRef = set;
                set.Add(1);
                set.Add(2);
                Assert.That(set.Count, Is.EqualTo(2));
            }

            Assert.That(setRef.Count, Is.EqualTo(0));
        }

        [Test]
        public void DisposingClearsQueue()
        {
            Queue<int> queueRef;
            using (AutoPool.TakeQueue<int>(out var queue))
            {
                queueRef = queue;
                queue.Enqueue(1);
                queue.Enqueue(2);
                Assert.That(queue.Count, Is.EqualTo(2));
            }

            Assert.That(queueRef.Count, Is.EqualTo(0));
        }

        [Test]
        public void DisposingClearsStack()
        {
            Stack<int> stackRef;
            using (AutoPool.TakeStack<int>(out var stack))
            {
                stackRef = stack;
                stack.Push(1);
                stack.Push(2);
                Assert.That(stack.Count, Is.EqualTo(2));
            }

            Assert.That(stackRef.Count, Is.EqualTo(0));
        }
    }
}
