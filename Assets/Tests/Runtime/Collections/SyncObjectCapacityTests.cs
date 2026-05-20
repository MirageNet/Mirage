using System;
using Mirage.Collections;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Collections.SyncObjectCapacityTests
{
    public class SyncListCapacityTests
    {


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
            var serverList = new SyncList<int>(100);
            serverList.Add(1);
            serverList.Add(2);
            serverList.Add(3);

            var clientList = new SyncList<int>(2);

            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeAllTo(serverList, clientList));
        }

        [Test]
        public void SyncList_ReaderSide_DeltaAddPastLimitThrows()
        {
            var serverList = new SyncList<int>(100);
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
            var serverDict = new SyncDictionary<int, string>(100);
            serverDict.Add(1, "one");
            serverDict.Add(2, "two");
            serverDict.Add(3, "three");

            var clientDict = new SyncDictionary<int, string>(2);

            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeAllTo(serverDict, clientDict));
        }

        [Test]
        public void SyncDictionary_ReaderSide_DeltaAddPastLimitThrows()
        {
            var serverDict = new SyncDictionary<int, string>(100);
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
            var serverSet = new SyncHashSet<int>(100);
            serverSet.Add(1);
            serverSet.Add(2);
            serverSet.Add(3);

            var clientSet = new SyncHashSet<int>(2);

            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeAllTo(serverSet, clientSet));
        }

        [Test]
        public void SyncSet_ReaderSide_DeltaAddPastLimitThrows()
        {
            var serverSet = new SyncHashSet<int>(100);
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
            var serverStack = new SyncStack<int>(100);
            serverStack.Push(1);
            serverStack.Push(2);
            serverStack.Push(3);

            var clientStack = new SyncStack<int>(2);

            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeAllTo(serverStack, clientStack));
        }

        [Test]
        public void SyncStack_ReaderSide_DeltaPushPastLimitThrows()
        {
            var serverStack = new SyncStack<int>(100);
            var clientStack = new SyncStack<int>(2);

            SyncObjectHelper.SerializeAllTo(serverStack, clientStack);

            serverStack.Push(1);
            serverStack.Push(2);
            SyncObjectHelper.SerializeDeltaTo(serverStack, clientStack);

            serverStack.Push(3);
            Assert.Throws<InvalidOperationException>(() => SyncObjectHelper.SerializeDeltaTo(serverStack, clientStack));
        }
    }

    public class SyncObjectFullSyncTests
    {
        [Test]
        public void SyncList_DeltaCompressesLargeChangeCountToFullSync()
        {
            var serverList = new SyncList<int>();
            var clientList = new SyncList<int>();

            SyncObjectHelper.SerializeAllTo(serverList, clientList);

            // Generate 150 changes, which is > 100 threshold
            for (var i = 0; i < 150; i++)
            {
                serverList.Add(i);
            }

            var clearCalled = false;
            var insertCount = 0;
            clientList.OnClear += () => clearCalled = true;
            clientList.OnInsert += (idx, val) => insertCount++;

            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            // Verify that the changes were compressed and applied correctly
            Assert.That(clearCalled, Is.True);
            Assert.That(insertCount, Is.EqualTo(150));
            Assert.That(clientList.Count, Is.EqualTo(150));
            for (var i = 0; i < 150; i++)
            {
                Assert.That(clientList[i], Is.EqualTo(i));
            }
        }

        [Test]
        public void SyncDictionary_DeltaCompressesLargeChangeCountToFullSync()
        {
            var serverDict = new SyncDictionary<int, string>();
            var clientDict = new SyncDictionary<int, string>();

            SyncObjectHelper.SerializeAllTo(serverDict, clientDict);

            // Generate 150 changes, which is > 100 threshold
            for (var i = 0; i < 150; i++)
            {
                serverDict.Add(i, $"value-{i}");
            }

            var clearCalled = false;
            var insertCount = 0;
            clientDict.OnClear += () => clearCalled = true;
            clientDict.OnInsert += (key, val) => insertCount++;

            SyncObjectHelper.SerializeDeltaTo(serverDict, clientDict);

            Assert.That(clearCalled, Is.True);
            Assert.That(insertCount, Is.EqualTo(150));
            Assert.That(clientDict.Count, Is.EqualTo(150));
            for (var i = 0; i < 150; i++)
            {
                Assert.That(clientDict[i], Is.EqualTo($"value-{i}"));
            }
        }

        [Test]
        public void SyncSet_DeltaCompressesLargeChangeCountToFullSync()
        {
            var serverSet = new SyncHashSet<int>();
            var clientSet = new SyncHashSet<int>();

            SyncObjectHelper.SerializeAllTo(serverSet, clientSet);

            // Generate 150 changes, which is > 100 threshold
            for (var i = 0; i < 150; i++)
            {
                serverSet.Add(i);
            }

            var clearCalled = false;
            var addCount = 0;
            clientSet.OnClear += () => clearCalled = true;
            clientSet.OnAdd += (val) => addCount++;

            SyncObjectHelper.SerializeDeltaTo(serverSet, clientSet);

            Assert.That(clearCalled, Is.True);
            Assert.That(addCount, Is.EqualTo(150));
            Assert.That(clientSet.Count, Is.EqualTo(150));
            for (var i = 0; i < 150; i++)
            {
                Assert.That(clientSet.Contains(i), Is.True);
            }
        }

        [Test]
        public void SyncStack_DeltaCompressesLargeChangeCountToFullSync()
        {
            var serverStack = new SyncStack<int>();
            var clientStack = new SyncStack<int>();

            SyncObjectHelper.SerializeAllTo(serverStack, clientStack);

            // Generate 150 changes, which is > 100 threshold
            for (var i = 0; i < 150; i++)
            {
                serverStack.Push(i);
            }

            var clearCalled = false;
            var pushCount = 0;
            clientStack.OnClear += () => clearCalled = true;
            clientStack.OnPush += (val) => pushCount++;

            SyncObjectHelper.SerializeDeltaTo(serverStack, clientStack);

            Assert.That(clearCalled, Is.True);
            Assert.That(pushCount, Is.EqualTo(150));
            Assert.That(clientStack.Count, Is.EqualTo(150));
            // Stack is LIFO, so elements on popping: 149, 148, ..., 0
            for (var i = 149; i >= 0; i--)
            {
                Assert.That(clientStack.Pop(), Is.EqualTo(i));
            }
        }

        [Test]
        public void SyncList_FullSyncResetsChangesAhead()
        {
            var serverList = new SyncList<int>();
            var clientList = new SyncList<int>();

            for (var i = 0; i < 150; i++)
            {
                serverList.Add(i);
            }

            for (var i = 0; i < 100; i++)
            {
                serverList.RemoveAt(0);
            }

            SyncObjectHelper.SerializeAllTo(serverList, clientList);

            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            serverList.Add(999);

            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            Assert.That(clientList.Contains(999), Is.True);
        }
    }
}
