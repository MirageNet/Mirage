using System;
using Mirage.Collections;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Collections.SyncObjectCapacityTests
{
    public class SyncListCapacityTests
    {
        [Test]
        public void SyncList_DefaultMaxElementsIsNull()
        {
            var list = new SyncList<int>();
            Assert.That(list.MaxElements, Is.Null);
        }

        [Test]
        public void SyncList_ConstructorSetsMaxElements()
        {
            var list = new SyncList<int>(5);
            Assert.That(list.MaxElements, Is.EqualTo(5));
        }

        [Test]
        public void SyncList_WriterSide_AddPastLimitThrows()
        {
            var list = new SyncList<int>(2);
            list.Add(1);
            list.Add(2);

            Assert.Throws<InvalidOperationException>(() => list.Add(3));
        }

        [Test]
        public void SyncList_WriterSide_InsertPastLimitThrows()
        {
            var list = new SyncList<int>(2);
            list.Add(1);
            list.Add(2);

            Assert.Throws<InvalidOperationException>(() => list.Insert(0, 3));
        }

        [Test]
        public void SyncList_ReaderSide_AllAddPastLimitThrows()
        {
            var serverList = new SyncList<int>();
            serverList.Add(1);
            serverList.Add(2);
            serverList.Add(3);

            var clientList = new SyncList<int>(2);

            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeAllTo(serverList, clientList));
        }

        [Test]
        public void SyncList_ReaderSide_DeltaAddPastLimitThrows()
        {
            var serverList = new SyncList<int>();
            var clientList = new SyncList<int>(2);

            SyncObjectHelper.SerializeAllTo(serverList, clientList);

            serverList.Add(1);
            serverList.Add(2);
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            serverList.Add(3);
            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeDeltaTo(serverList, clientList));
        }
    }

    public class SyncDictionaryCapacityTests
    {
        [Test]
        public void SyncDictionary_DefaultMaxElementsIsNull()
        {
            var dict = new SyncDictionary<int, string>();
            Assert.That(dict.MaxElements, Is.Null);
        }

        [Test]
        public void SyncDictionary_ConstructorSetsMaxElements()
        {
            var dict = new SyncDictionary<int, string>(5);
            Assert.That(dict.MaxElements, Is.EqualTo(5));
        }

        [Test]
        public void SyncDictionary_WriterSide_AddPastLimitThrows()
        {
            var dict = new SyncDictionary<int, string>(2);
            dict.Add(1, "one");
            dict.Add(2, "two");

            Assert.Throws<InvalidOperationException>(() => dict.Add(3, "three"));
        }

        [Test]
        public void SyncDictionary_WriterSide_SetterNewKeyPastLimitThrows()
        {
            var dict = new SyncDictionary<int, string>(2);
            dict.Add(1, "one");
            dict.Add(2, "two");

            Assert.Throws<InvalidOperationException>(() => dict[3] = "three");
        }

        [Test]
        public void SyncDictionary_WriterSide_SetterExistingKeyPastLimitDoesNotThrow()
        {
            var dict = new SyncDictionary<int, string>(2);
            dict.Add(1, "one");
            dict.Add(2, "two");

            Assert.DoesNotThrow(() => dict[2] = "two-updated");
        }

        [Test]
        public void SyncDictionary_ReaderSide_AllAddPastLimitThrows()
        {
            var serverDict = new SyncDictionary<int, string>();
            serverDict.Add(1, "one");
            serverDict.Add(2, "two");
            serverDict.Add(3, "three");

            var clientDict = new SyncDictionary<int, string>(2);

            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeAllTo(serverDict, clientDict));
        }

        [Test]
        public void SyncDictionary_ReaderSide_DeltaAddPastLimitThrows()
        {
            var serverDict = new SyncDictionary<int, string>();
            var clientDict = new SyncDictionary<int, string>(2);

            SyncObjectHelper.SerializeAllTo(serverDict, clientDict);

            serverDict.Add(1, "one");
            serverDict.Add(2, "two");
            SyncObjectHelper.SerializeDeltaTo(serverDict, clientDict);

            serverDict.Add(3, "three");
            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeDeltaTo(serverDict, clientDict));
        }
    }

    public class SyncSetCapacityTests
    {
        [Test]
        public void SyncSet_DefaultMaxElementsIsNull()
        {
            var set = new SyncHashSet<int>();
            Assert.That(set.MaxElements, Is.Null);
        }

        [Test]
        public void SyncSet_ConstructorSetsMaxElements()
        {
            var set = new SyncHashSet<int>(5);
            Assert.That(set.MaxElements, Is.EqualTo(5));
        }

        [Test]
        public void SyncSet_WriterSide_AddNewPastLimitThrows()
        {
            var set = new SyncHashSet<int>(2);
            set.Add(1);
            set.Add(2);

            Assert.Throws<InvalidOperationException>(() => set.Add(3));
        }

        [Test]
        public void SyncSet_WriterSide_AddExistingPastLimitDoesNotThrow()
        {
            var set = new SyncHashSet<int>(2);
            set.Add(1);
            set.Add(2);

            Assert.DoesNotThrow(() => set.Add(2));
        }

        [Test]
        public void SyncSet_ReaderSide_AllAddPastLimitThrows()
        {
            var serverSet = new SyncHashSet<int>();
            serverSet.Add(1);
            serverSet.Add(2);
            serverSet.Add(3);

            var clientSet = new SyncHashSet<int>(2);

            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeAllTo(serverSet, clientSet));
        }

        [Test]
        public void SyncSet_ReaderSide_DeltaAddPastLimitThrows()
        {
            var serverSet = new SyncHashSet<int>();
            var clientSet = new SyncHashSet<int>(2);

            SyncObjectHelper.SerializeAllTo(serverSet, clientSet);

            serverSet.Add(1);
            serverSet.Add(2);
            SyncObjectHelper.SerializeDeltaTo(serverSet, clientSet);

            serverSet.Add(3);
            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeDeltaTo(serverSet, clientSet));
        }
    }

    public class SyncStackCapacityTests
    {
        [Test]
        public void SyncStack_DefaultMaxElementsIsNull()
        {
            var stack = new SyncStack<int>();
            Assert.That(stack.MaxElements, Is.Null);
        }

        [Test]
        public void SyncStack_ConstructorSetsMaxElements()
        {
            var stack = new SyncStack<int>(5);
            Assert.That(stack.MaxElements, Is.EqualTo(5));
        }

        [Test]
        public void SyncStack_WriterSide_PushPastLimitThrows()
        {
            var stack = new SyncStack<int>(2);
            stack.Push(1);
            stack.Push(2);

            Assert.Throws<InvalidOperationException>(() => stack.Push(3));
        }

        [Test]
        public void SyncStack_ReaderSide_AllAddPastLimitThrows()
        {
            var serverStack = new SyncStack<int>();
            serverStack.Push(1);
            serverStack.Push(2);
            serverStack.Push(3);

            var clientStack = new SyncStack<int>(2);

            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeAllTo(serverStack, clientStack));
        }

        [Test]
        public void SyncStack_ReaderSide_DeltaPushPastLimitThrows()
        {
            var serverStack = new SyncStack<int>();
            var clientStack = new SyncStack<int>(2);

            SyncObjectHelper.SerializeAllTo(serverStack, clientStack);

            serverStack.Push(1);
            serverStack.Push(2);
            SyncObjectHelper.SerializeDeltaTo(serverStack, clientStack);

            serverStack.Push(3);
            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeDeltaTo(serverStack, clientStack));
        }
    }
}
