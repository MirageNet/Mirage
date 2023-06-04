using System;
using System.Linq;
using Mirage.Collections;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests.Runtime
{
    [TestFixture]
    public class SyncStackTestString
    {
        private SyncStack<string> serverSyncStack;
        private SyncStack<string> clientSyncStack;

        [SetUp]
        public void SetUp()
        {
            serverSyncStack = new SyncStack<string>();
            clientSyncStack = new SyncStack<string>();

            // add some data to the stack
            serverSyncStack.Push("Hello");
            serverSyncStack.Push("World");
            serverSyncStack.Push("!");
            SyncObjectHelper.SerializeAllTo(serverSyncStack, clientSyncStack);
        }

        [Test]
        public void TestStackOrder()
        {
            var callback = Substitute.For<Action<string>>();
            clientSyncStack.OnPop += callback;

            serverSyncStack.Pop();
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            callback.Received(1).Invoke("!");
            callback.ClearReceivedCalls();

            serverSyncStack.Pop();
            serverSyncStack.Pop();
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            Received.InOrder(() =>
            {
                callback.Invoke("World");
                callback.Invoke("Hello");
            });
        }


        [Test]
        public void TestInit()
        {
            string[] expectedValues = { "!", "World", "Hello" };
            Assert.That(clientSyncStack, Is.EquivalentTo(expectedValues));
        }


        [Test]
        public void ClearEventOnSyncAll()
        {
            var callback = Substitute.For<Action>();
            clientSyncStack.OnClear += callback;
            SyncObjectHelper.SerializeAllTo(serverSyncStack, clientSyncStack);
            callback.Received().Invoke();
        }

        [Test]
        public void PushEventOnSyncAll()
        {
            var callback = Substitute.For<Action<string>>();
            clientSyncStack.OnPush += callback;
            SyncObjectHelper.SerializeAllTo(serverSyncStack, clientSyncStack);

            Received.InOrder(() =>
            {
                callback.Invoke("Hello");
                callback.Invoke("World");
                callback.Invoke("!");
            });
        }

        [Test]
        public void ChangeEventOnSyncAll()
        {
            var callback = Substitute.For<Action>();
            clientSyncStack.OnChange += callback;
            SyncObjectHelper.SerializeAllTo(serverSyncStack, clientSyncStack);
            callback.Received().Invoke();
        }

        [Test]
        public void TestPush()
        {
            serverSyncStack.Push("yay");
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            Assert.That(clientSyncStack, Is.EquivalentTo(new[] { "Hello", "World", "!", "yay" }));
        }

        [Test]
        public void TestAddRange()
        {
            serverSyncStack.AddRange(new[] { "One", "Two", "Three" });
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            // Reverse because we need to check from top of stack
            Assert.That(clientSyncStack, Is.EqualTo(new[] { "Hello", "World", "!", "One", "Two", "Three" }.Reverse()));
        }

        [Test]
        public void TestClear()
        {
            serverSyncStack.Clear();
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            Assert.That(clientSyncStack, Is.EquivalentTo(new string[] { }));
        }

        [Test]
        public void TestMultSync()
        {
            serverSyncStack.Push("1");
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            // add some delta and see if it applies
            serverSyncStack.Push("2");
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            Assert.That(clientSyncStack, Is.EquivalentTo(new[] { "Hello", "World", "!", "1", "2" }));
        }

        [Test]
        public void AddClientCallbackTest()
        {
            var callback = Substitute.For<Action<string>>();
            clientSyncStack.OnPush += callback;
            serverSyncStack.Push("yay");
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            callback.Received().Invoke("yay");
        }


        [Test]
        public void RemoveClientCallbackTest()
        {
            var callback = Substitute.For<Action<string>>();
            clientSyncStack.OnPop += callback;
            serverSyncStack.Pop();
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            callback.Received().Invoke("!");
        }

        [Test]
        public void ClearClientCallbackTest()
        {
            var callback = Substitute.For<Action>();
            clientSyncStack.OnClear += callback;
            serverSyncStack.Clear();
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            callback.Received().Invoke();
        }

        [Test]
        public void ChangeClientCallbackTest()
        {
            var callback = Substitute.For<Action>();
            clientSyncStack.OnChange += callback;
            serverSyncStack.Push("1");
            serverSyncStack.Push("2");
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);
            callback.Received(1).Invoke();
        }

        [Test]
        public void PishServerCallbackTest()
        {
            var callback = Substitute.For<Action<string>>();
            serverSyncStack.OnPush += callback;
            serverSyncStack.Push("yay");
            callback.Received().Invoke("yay");
        }

        [Test]
        public void PopServerCallbackTest()
        {
            var callback = Substitute.For<Action<string>>();
            serverSyncStack.OnPop += callback;
            serverSyncStack.Pop();
            callback.Received().Invoke("!");
        }

        [Test]
        public void ClearServerCallbackTest()
        {
            var callback = Substitute.For<Action>();
            serverSyncStack.OnClear += callback;
            serverSyncStack.Clear();
            callback.Received().Invoke();
        }

        [Test]
        public void ChangeServerCallbackTest()
        {
            var callback = Substitute.For<Action>();
            serverSyncStack.OnChange += callback;
            serverSyncStack.Push("1");
            serverSyncStack.Push("2");
            // note that on the server we would receive 2 calls
            // because we are adding 2 operations separately
            // there is no way to batch operations in the server
            callback.Received().Invoke();
        }

        [Test]
        public void CountTest()
        {
            Assert.That(serverSyncStack.Count, Is.EqualTo(3));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ReadOnlyTest(bool shouldSync)
        {
            var asObject = (ISyncObject)serverSyncStack;
            asObject.SetShouldSyncFrom(shouldSync);
            Assert.That(serverSyncStack.IsReadOnly, Is.EqualTo(!shouldSync));

            serverSyncStack.Reset();
            Assert.That(serverSyncStack.IsReadOnly, Is.EqualTo(false));
        }

        [Test]
        public void WritingToReadOnlyThrows()
        {
            var asObject = (ISyncObject)serverSyncStack;
            asObject.SetShouldSyncFrom(false);
            Assert.Throws<InvalidOperationException>(() =>
            {
                serverSyncStack.Push("fail");
            });
        }

        [Test]
        public void DirtyTest()
        {
            // Sync Delta to clear dirty
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);

            // nothing to send
            Assert.That(serverSyncStack.IsDirty, Is.False);

            // something has changed
            serverSyncStack.Push("1");
            Assert.That(serverSyncStack.IsDirty, Is.True);
            SyncObjectHelper.SerializeDeltaTo(serverSyncStack, clientSyncStack);

            // data has been flushed,  should go back to clear
            Assert.That(serverSyncStack.IsDirty, Is.False);
        }

        [Test]
        public void ObjectCanBeReusedAfterReset()
        {
            clientSyncStack.Reset();

            // make old client the host
            var hostList = clientSyncStack;
            var clientList2 = new SyncStack<string>();

            Assert.That(hostList.IsReadOnly, Is.False);

            // Check Add and Sync without errors
            hostList.Push("hello");
            hostList.Push("world");
            SyncObjectHelper.SerializeDeltaTo(hostList, clientList2);
        }

        [Test]
        public void ResetShouldSetReadOnlyToFalse()
        {
            clientSyncStack.Reset();

            Assert.That(clientSyncStack.IsReadOnly, Is.False);
        }

        [Test]
        public void ResetShouldClearChanges()
        {
            serverSyncStack.Reset();

            Assert.That(serverSyncStack.ChangeCount, Is.Zero);
        }

        [Test]
        public void ResetShouldClearItems()
        {
            serverSyncStack.Reset();

            Assert.That(serverSyncStack, Is.Empty);
        }
    }

    [TestFixture]
    public class SyncStackTestOtherTypes
    {
        [Test]
        public void SyncStackIntTest()
        {
            var serverList = new SyncStack<int>();
            var clientList = new SyncStack<int>();

            serverList.Push(1);
            serverList.Push(2);
            serverList.Push(3);
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            Assert.That(clientList, Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void SyncStackBoolTest()
        {
            var serverList = new SyncStack<bool>();
            var clientList = new SyncStack<bool>();

            serverList.Push(true);
            serverList.Push(false);
            serverList.Push(true);
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            Assert.That(clientList, Is.EquivalentTo(new[] { true, false, true }));
        }

        [Test]
        public void SyncStackUIntTest()
        {
            var serverList = new SyncStack<uint>();
            var clientList = new SyncStack<uint>();

            serverList.Push(1U);
            serverList.Push(2U);
            serverList.Push(3U);
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            Assert.That(clientList, Is.EquivalentTo(new[] { 1U, 2U, 3U }));
        }

        [Test]
        public void SyncStackFloatTest()
        {
            var serverList = new SyncStack<float>();
            var clientList = new SyncStack<float>();

            serverList.Push(1.0F);
            serverList.Push(2.0F);
            serverList.Push(3.0F);
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            Assert.That(clientList, Is.EquivalentTo(new[] { 1.0F, 2.0F, 3.0F }));
        }

        [Test]
        public void TestStackOrder()
        {
            var serverList = new SyncStack<int>();
            var clientList = new SyncStack<int>();

            serverList.Push(1);
            serverList.Push(2);
            serverList.Push(3);
            serverList.Push(4);
            serverList.Push(5);
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            serverList.Pop();
            serverList.Pop();
            serverList.Pop();
            serverList.Pop();
            serverList.Pop();
            var callback = Substitute.For<Action<int>>();
            clientList.OnPop += callback;
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            Received.InOrder(() =>
            {
                callback.Invoke(5);
                callback.Invoke(4);
                callback.Invoke(3);
                callback.Invoke(2);
                callback.Invoke(1);
            });
        }

    }
}
