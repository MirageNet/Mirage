using System;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    public class AssertionMethodAttribute : Attribute { }

    [TestFixture]
    public class MessageTests
    {
        [Test]
        public void ServerRpcMessageTest()
        {
            // try setting value with constructor
            var message = new ServerRpcMessage
            {
                netId = 42,
                componentIndex = 4,
                functionIndex = 2,
                payload = new ArraySegment<byte>(new byte[] { 0x01, 0x02 })
            };
            var arr = MessagePacker.Pack(message);

            // deserialize the same data - do we get the same result?
            var fresh = MessagePacker.Unpack<ServerRpcMessage>(arr, null);
            Assert.That(fresh.netId, Is.EqualTo(message.netId));
            Assert.That(fresh.componentIndex, Is.EqualTo(message.componentIndex));
            Assert.That(fresh.functionIndex, Is.EqualTo(message.functionIndex));
            Assert.That(fresh.payload, Has.Count.EqualTo(message.payload.Count));
            for (var i = 0; i < fresh.payload.Count; ++i)
                Assert.That(fresh.payload.Array[fresh.payload.Offset + i],
                    Is.EqualTo(message.payload.Array[message.payload.Offset + i]));
        }

        [AssertionMethod]
        private void TestSerializeDeserialize<T>(T message)
        {
            // serialize
            var arr = MessagePacker.Pack(message);
            var fresh = MessagePacker.Unpack<T>(arr, null);
            Assert.That(fresh, Is.EqualTo(message));
        }

        [Test]
        public void NetworkPingMessageTest()
        {
            TestSerializeDeserialize(new NetworkPingMessage
            {
                clientTime = DateTime.Now.ToOADate()
            });
        }

        [Test]
        public void NetworkPongMessageTest()
        {
            TestSerializeDeserialize(new NetworkPongMessage
            {
                clientTime = DateTime.Now.ToOADate(),
                serverTime = DateTime.Now.ToOADate(),
            });
        }

        [Test]
        public void NotReadyMessageTest()
        {
            TestSerializeDeserialize(new SceneNotReadyMessage());
        }

        [Test]
        public void ObjectDestroyMessageTest()
        {
            TestSerializeDeserialize(new ObjectDestroyMessage
            {
                netId = 42,
            });
        }

        [Test]
        public void ObjectHideMessageTest()
        {
            TestSerializeDeserialize(new ObjectHideMessage
            {
                netId = 42,
            });
        }

        [Test]
        public void SceneReadyMessageTest()
        {
            TestSerializeDeserialize(new SceneReadyMessage());
        }

        [Test]
        public void AddPlayerMessageTest()
        {
            TestSerializeDeserialize(new AddCharacterMessage());
        }

        [Test]
        public void RpcMessageTest()
        {
            // try setting value with constructor
            var message = new RpcMessage
            {
                netId = 42,
                componentIndex = 4,
                functionIndex = 3,
                payload = new ArraySegment<byte>(new byte[] { 0x01, 0x02 })
            };
            var arr = MessagePacker.Pack(message);
            var fresh = MessagePacker.Unpack<RpcMessage>(arr, null);
            Assert.That(fresh.netId, Is.EqualTo(message.netId));
            Assert.That(fresh.componentIndex, Is.EqualTo(message.componentIndex));
            Assert.That(fresh.functionIndex, Is.EqualTo(message.functionIndex));
            Assert.That(fresh.payload.Count, Is.EqualTo(message.payload.Count));
            for (var i = 0; i < fresh.payload.Count; ++i)
                Assert.That(fresh.payload.Array[fresh.payload.Offset + i],
                    Is.EqualTo(message.payload.Array[message.payload.Offset + i]));
        }

        [Test]
        [TestCase(0ul)]
        [TestCase(42ul)]
        public void SpawnMessageTest(ulong testSceneId)
        {
            // try setting value with constructor
            var message = new SpawnMessage
            {
                netId = 42,
                isLocalPlayer = true,
                isOwner = true,
                sceneId = testSceneId,
                prefabHash = Guid.NewGuid().GetHashCode(),
                position = UnityEngine.Vector3.one,
                rotation = UnityEngine.Quaternion.identity,
                scale = UnityEngine.Vector3.one,
                payload = new ArraySegment<byte>(new byte[] { 0x01, 0x02 })
            };
            var arr = MessagePacker.Pack(message);
            var fresh = MessagePacker.Unpack<SpawnMessage>(arr, null);
            Assert.That(fresh.netId, Is.EqualTo(message.netId));
            Assert.That(fresh.isLocalPlayer, Is.EqualTo(message.isLocalPlayer));
            Assert.That(fresh.isOwner, Is.EqualTo(message.isOwner));
            Assert.That(fresh.sceneId, Is.EqualTo(message.sceneId));
            Assert.That(fresh.prefabHash, Is.EqualTo(message.prefabHash));
            Assert.That(fresh.position, Is.EqualTo(message.position));
            Assert.That(fresh.rotation, Is.EqualTo(message.rotation));
            Assert.That(fresh.scale, Is.EqualTo(message.scale));
            Assert.That(fresh.payload.Count, Is.EqualTo(message.payload.Count));

            for (var i = 0; i < fresh.payload.Count; ++i)
                Assert.That(fresh.payload.Array[fresh.payload.Offset + i],
                    Is.EqualTo(message.payload.Array[message.payload.Offset + i]));
        }

        [Test]
        public void UpdateVarsMessageTest()
        {
            // try setting value with constructor
            var message = new UpdateVarsMessage
            {
                netId = 42,
                payload = new ArraySegment<byte>(new byte[] { 0x01, 0x02 })
            };
            var arr = MessagePacker.Pack(message);
            var fresh = MessagePacker.Unpack<UpdateVarsMessage>(arr, null);
            Assert.That(fresh.netId, Is.EqualTo(message.netId));
            Assert.That(fresh.payload.Count, Is.EqualTo(message.payload.Count));
            for (var i = 0; i < fresh.payload.Count; ++i)
                Assert.That(fresh.payload.Array[fresh.payload.Offset + i],
                    Is.EqualTo(message.payload.Array[message.payload.Offset + i]));
        }

        [NetworkMessage]
        private struct NestedMessageWithAttr { }

        private struct NestedMessageWithoutAttr { }

        [Test]
        public void CreatesWriterForUnusedMessageWithAttribute()
        {
            Assert.That(Writer<NestedMessageWithAttr>.Write, Is.Not.Null);
            Assert.That(Reader<NestedMessageWithAttr>.Read, Is.Not.Null);

            // should not create for *unused* struct without attribute
            Assert.That(Writer<NestedMessageWithoutAttr>.Write, Is.Null);
            Assert.That(Reader<NestedMessageWithoutAttr>.Read, Is.Null);
        }
    }
}
