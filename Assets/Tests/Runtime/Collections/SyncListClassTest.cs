using System.Linq;
using Mirage.Collections;
using NUnit.Framework;

namespace Mirage.Tests.Runtime
{
    internal class TestObjectBehaviour : NetworkBehaviour
    {
        // note synclists must be a property of a NetworkBehavior so that
        // the weaver generates the reader and writer for the object
        public readonly SyncList<TestObject> myList = new SyncList<TestObject>();
    }

    public class SyncListClassTest
    {
        [Test]
        public void RemoveShouldRemoveItem()
        {
            var serverList = new SyncList<TestObject>();
            var clientList = new SyncList<TestObject>();

            SyncObjectHelper.SerializeAllTo(serverList, clientList);

            // add some items
            var item1 = new TestObject { id = 1, text = "Lorem ipsum dolor sit, amet consectetur adipisicing elit. Nostrum ullam aliquid perferendis, aut nihil sunt quod ipsum corporis a. Cupiditate, alias. Commodi, molestiae distinctio repellendus dolor similique delectus inventore eum." };
            serverList.Add(item1);
            var item2 = new TestObject { id = 2, text = "Lorem ipsum dolor sit, amet consectetur adipisicing elit. Nostrum ullam aliquid perferendis, aut nihil sunt quod ipsum corporis a. Cupiditate, alias. Commodi, molestiae distinctio repellendus dolor similique delectus inventore eum." };
            serverList.Add(item2);

            // sync
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            // clear all items            
            serverList.Remove(item1);

            // sync
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            Assert.IsFalse(clientList.Any(x => x.id == item1.id));
            Assert.IsTrue(clientList.Any(x => x.id == item2.id));
        }

        [Test]
        public void ClearShouldClearAll()
        {
            var serverList = new SyncList<TestObject>();
            var clientList = new SyncList<TestObject>();

            SyncObjectHelper.SerializeAllTo(serverList, clientList);

            // add some items
            var item1 = new TestObject { id = 1, text = "Lorem ipsum dolor sit, amet consectetur adipisicing elit. Nostrum ullam aliquid perferendis, aut nihil sunt quod ipsum corporis a. Cupiditate, alias. Commodi, molestiae distinctio repellendus dolor similique delectus inventore eum." };
            serverList.Add(item1);
            var item2 = new TestObject { id = 2, text = "Lorem ipsum dolor sit, amet consectetur adipisicing elit. Nostrum ullam aliquid perferendis, aut nihil sunt quod ipsum corporis a. Cupiditate, alias. Commodi, molestiae distinctio repellendus dolor similique delectus inventore eum." };
            serverList.Add(item2);

            // sync
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            // clear all items            
            serverList.Clear();

            // sync
            SyncObjectHelper.SerializeDeltaTo(serverList, clientList);

            Assert.That(clientList.Count, Is.Zero);

            Assert.IsFalse(clientList.Any(x => x.id == item1.id));
            Assert.IsFalse(clientList.Any(x => x.id == item2.id));
        }
    }

    [System.Serializable]
    public class TestObject
    {
        public int id;
        public string text;
    }
}
