using Mirage.Collections;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime
{
    public class SyncListStructTest
    {
        [SetUp]
        public void Setup()
        {
            // let the weaver know to generate a reader and writer for TestPlayer
            var writer = new NetworkWriter(1300);
            writer.Write<TestPlayer>(default);
        }

        [Test]
        public void ListIsDirtyWhenModifingAndSettingStruct()
        {
            var serverList = new SyncList<TestPlayer>();
            var clientList = new SyncList<TestPlayer>();
            SerializeHelper.SerializeAllTo(serverList, clientList);
            serverList.Add(new TestPlayer { item = new TestItem { price = 10 } });
            SerializeHelper.SerializeDeltaTo(serverList, clientList);
            Assert.That(serverList.IsDirty, Is.False);

            TestPlayer player = serverList[0];
            player.item.price = 15;
            serverList[0] = player;

            Assert.That(serverList.IsDirty, Is.True);
        }
    }

    public struct TestPlayer
    {
        public TestItem item;
    }
    public struct TestItem
    {
        public float price;
    }
}
