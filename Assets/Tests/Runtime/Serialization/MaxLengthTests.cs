using System;
using System.Collections.Generic;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using Mirage.SocketLayer;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Serialization
{
    public struct CustomTypeWithLimit
    {
        public string Name;
        public int Value;
    }

    public struct TestLimitMessage
    {
        public string Content;
    }

    [NetworkMessage]
    public struct LimitMessage
    {
        [MaxLength(3)]
        public string Content;
    }

    public class RpcBehaviour : NetworkBehaviour
    {
        [ServerRpc]
        public void SendString([MaxLength(4)] string message)
        {
        }

        [ClientRpc]
        public void SendStringClient([MaxLength(5)] string message)
        {
        }

        [ServerRpc]
        public void SendString2([MaxLength(120)] string message)
        {
        }

        [ClientRpc]
        public void SendStringClient2([MaxLength(150)] string message)
        {
        }
    }

    public class SyncVarBehaviour : NetworkBehaviour
    {
        [SyncVar, MaxLength(6)]
        public string Content { get; set; }
    }

    public class DummyBehaviour : NetworkBehaviour
    {
        // Simple dummy behaviour for test network objects
    }

    [TestFixture]
    public class MaxLengthTests : TestBase
    {
        [Test]
        public void StringLimitWriteExceededThrowsSerializationLimitException()
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                Assert.Throws<SerializationLimitException>(() =>
                {
                    writer.WriteString("TooLongString", 5);
                });
            }
        }

        [Test]
        public void StringLimitWriteWithinLimitSucceeds()
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                Assert.DoesNotThrow(() =>
                {
                    writer.WriteString("abc", 5);
                });
            }
        }

        [Test]
        public void StringLimitReadExceededThrowsSerializationLimitException()
        {
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteString("abc", 5);
                bytes = writer.ToArray();
            }

            using (var reader = NetworkReaderPool.GetReader(bytes, null))
            {
                Assert.Throws<SerializationLimitException>(() =>
                {
                    reader.ReadString(2);
                });
            }
        }

        [Test]
        public void StringLimitReadWithinLimitSucceeds()
        {
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteString("abc", 5);
                bytes = writer.ToArray();
            }

            using (var reader = NetworkReaderPool.GetReader(bytes, null))
            {
                var result = reader.ReadString(5);
                Assert.That(result, Is.EqualTo("abc"));
            }
        }

        [Test]
        public void StringLimitReadEarlyFailThrowsSerializationLimitException()
        {
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                // Write a size prefix representing a size exceeding maxLength * 4 + 1
                // limit is 2, max bytes is UTF-8 max byte count for 2 chars: 3 * (2 + 1) = 9 bytes.
                // We write a size prefix of 11 (making realSize = 10)
                writer.WriteUInt16(11);
                writer.WriteBytes(new byte[10], 0, 10);
                bytes = writer.ToArray();
            }

            using (var reader = NetworkReaderPool.GetReader(bytes, null))
            {
                var ex = Assert.Throws<SerializationLimitException>(() =>
                {
                    reader.ReadString(2);
                });
                Assert.That(ex.Message, Contains.Substring("ReadString byte size"));
            }
        }

        [Test]
        public void ArrayLimitWriteExceededThrowsSerializationLimitException()
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                var array = new int[] { 1, 2, 3, 4, 5 };
                Assert.Throws<SerializationLimitException>(() =>
                {
                    writer.WriteArray(array, 3);
                });
            }
        }

        [Test]
        public void ArrayLimitReadExceededThrowsSerializationLimitException()
        {
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                var array = new int[] { 1, 2, 3 };
                writer.WriteArray(array, 5);
                bytes = writer.ToArray();
            }

            using (var reader = NetworkReaderPool.GetReader(bytes, null))
            {
                Assert.Throws<SerializationLimitException>(() =>
                {
                    reader.ReadArray<int>(2);
                });
            }
        }

        [Test]
        public void ListLimitWriteExceededThrowsSerializationLimitException()
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                var list = new List<float> { 1f, 2f, 3f, 4f, 5f };
                Assert.Throws<SerializationLimitException>(() =>
                {
                    writer.WriteList(list, 3);
                });
            }
        }

        [Test]
        public void ListLimitReadExceededThrowsSerializationLimitException()
        {
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                var list = new List<float> { 1f, 2f, 3f };
                writer.WriteList(list, 5);
                bytes = writer.ToArray();
            }

            using (var reader = NetworkReaderPool.GetReader(bytes, null))
            {
                Assert.Throws<SerializationLimitException>(() =>
                {
                    reader.ReadList<float>(2);
                });
            }
        }

        [Test]
        public void CustomTypeWriteAndReadWithLengthSucceeds()
        {
            Writer<CustomTypeWithLimit>.WriteWithLength = (writer, val, limit) =>
            {
                writer.WriteString(val.Name, limit);
                writer.WriteInt32(val.Value);
            };
            Reader<CustomTypeWithLimit>.ReadWithLength = (reader, limit) =>
            {
                return new CustomTypeWithLimit
                {
                    Name = reader.ReadString(limit),
                    Value = reader.ReadInt32()
                };
            };

            byte[] bytes;
            var original = new CustomTypeWithLimit { Name = "abc", Value = 42 };
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteWithLength(original, 5);
                bytes = writer.ToArray();
            }

            using (var reader = NetworkReaderPool.GetReader(bytes, null))
            {
                var read = reader.ReadWithLength<CustomTypeWithLimit>(5);
                Assert.That(read.Name, Is.EqualTo("abc"));
                Assert.That(read.Value, Is.EqualTo(42));
            }
        }

        [Test]
        public void CustomTypeWriteExceededThrowsSerializationLimitException()
        {
            Writer<CustomTypeWithLimit>.WriteWithLength = (writer, val, limit) =>
            {
                writer.WriteString(val.Name, limit);
                writer.WriteInt32(val.Value);
            };

            var val = new CustomTypeWithLimit { Name = "TooLongName", Value = 42 };
            using (var writer = NetworkWriterPool.GetWriter())
            {
                Assert.Throws<SerializationLimitException>(() =>
                {
                    writer.WriteWithLength(val, 5);
                });
            }
        }

        [Test]
        public void CustomTypeReadExceededThrowsSerializationLimitException()
        {
            Writer<CustomTypeWithLimit>.WriteWithLength = (writer, val, limit) =>
            {
                writer.WriteString(val.Name, limit);
                writer.WriteInt32(val.Value);
            };
            Reader<CustomTypeWithLimit>.ReadWithLength = (reader, limit) =>
            {
                return new CustomTypeWithLimit
                {
                    Name = reader.ReadString(limit),
                    Value = reader.ReadInt32()
                };
            };

            byte[] bytes;
            var val = new CustomTypeWithLimit { Name = "abc", Value = 42 };
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteWithLength(val, 5);
                bytes = writer.ToArray();
            }

            using (var reader = NetworkReaderPool.GetReader(bytes, null))
            {
                Assert.Throws<SerializationLimitException>(() =>
                {
                    reader.ReadWithLength<CustomTypeWithLimit>(2);
                });
            }
        }

        [Test]
        public void MessageHandlerDeserializationExceptionSetsPlayerErrorFlags()
        {
            var connection = Substitute.For<Mirage.SocketLayer.IConnection>();
            // A non-null error rate limit config is passed because SetError only tracks and updates
            // ErrorFlags when error rate limiting is enabled on the player connection.
            var server = CreateMonoBehaviour<NetworkServer>();
            var player = new NetworkPlayer(connection, false, server, new RateLimitBucket.RefillConfig { MaxTokens = 100, Refill = 10, Interval = 1 });

            var messageHandler = new MessageHandler(null, true);

            // Register message packer and dynamic reader/writer with limits
            MessagePacker.RegisterMessage<TestLimitMessage>();
            Writer<TestLimitMessage>.Write = (writer, val) =>
            {
                writer.WriteString(val.Content, 5);
            };
            Reader<TestLimitMessage>.Read = reader =>
            {
                // Reading with a low limit of 2 will cause a deserialization exception for "abc"
                return new TestLimitMessage
                {
                    Content = reader.ReadString(2)
                };
            };

            var invoked = 0;
            messageHandler.RegisterHandler<TestLimitMessage>((p, msg) => invoked++, allowUnauthenticated: true);

            // Pack a message containing a string that fits under write limit but fails read limit
            var packet = MessagePacker.Pack(new TestLimitMessage { Content = "abc" });

            LogAssert.ignoreFailingMessages = true;
            messageHandler.HandleMessage(player, new ArraySegment<byte>(packet));
            LogAssert.ignoreFailingMessages = false;

            // Verify message handler failed and marked player error flag
            Assert.That(invoked, Is.EqualTo(0));
            Assert.That((player.ErrorFlags & PlayerErrorFlags.SerializationLimit) != 0, Is.True);
            connection.Received(1).Disconnect();
        }

        [Test]
        public void RpcHandlerSerializationLimitExceptionSetsPlayerErrorFlags()
        {
            var connection = Substitute.For<Mirage.SocketLayer.IConnection>();
            // A non-null error rate limit config is passed because SetError only tracks and updates
            // ErrorFlags when error rate limiting is enabled on the player connection.
            var server = CreateMonoBehaviour<NetworkServer>();
            var player = new NetworkPlayer(connection, false, server, new RateLimitBucket.RefillConfig { MaxTokens = 100, Refill = 10, Interval = 1 });

            var go = new GameObject("DummyObj");
            var identity = go.AddComponent<NetworkIdentity>();
            var behaviour = go.AddComponent<DummyBehaviour>();

            // Register RPC delegate that throws SerializationLimitException when called
            var remoteCall = new RemoteCall(
                behaviour,
                0,
                RpcInvokeType.ServerRpc,
                (obj, reader, senderPlayer, replyId) => throw new SerializationLimitException("Simulated limit exceeded"),
                false,
                "TestRpc",
                RpcRateLimitConfig.Disabled()
            );

            // Inject the custom RPC into the identity's collection
            identity.RemoteCallCollection.RemoteCalls = new RemoteCall[] { remoteCall };
            identity.RemoteCallCollection.IndexOffset = new int[] { 0 };

            var objectLocator = Substitute.For<IObjectLocator>();
            objectLocator.TryGetIdentity(Arg.Any<uint>(), out Arg.Any<NetworkIdentity>())
                .Returns(x =>
                {
                    x[1] = identity;
                    return true;
                });

            var rpcHandler = new RpcHandler(objectLocator, RpcInvokeType.ServerRpc);

            var payload = new byte[] { 1, 2, 3 };
            var msg = new RpcMessage
            {
                NetId = 1,
                FunctionIndex = 0,
                Payload = new ArraySegment<byte>(payload)
            };

            LogAssert.ignoreFailingMessages = true;
            rpcHandler.OnRpcMessage(player, msg);
            LogAssert.ignoreFailingMessages = false;

            // Verify RPC execution caught SerializationLimitException and marked player
            Assert.That((player.ErrorFlags & PlayerErrorFlags.SerializationLimit) != 0, Is.True);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void StringStoreStringLimitWriteExceededThrowsSerializationLimitException()
        {
            var store = new StringStore();
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.StringStore = store;
                Assert.Throws<SerializationLimitException>(() =>
                {
                    writer.WriteString("TooLongString", 5);
                });
            }
        }

        [Test]
        public void StringStoreStringLimitReadExceededThrowsSerializationLimitException()
        {
            var store = new StringStore();

            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.StringStore = store;
                writer.WriteString("TooLongString", 20);
                bytes = writer.ToArray();
            }

            using (var reader = NetworkReaderPool.GetReader(bytes, null))
            {
                reader.StringStore = store;
                Assert.Throws<SerializationLimitException>(() =>
                {
                    reader.ReadString(5);
                });
            }
        }

        [Test]
        // Verify that a [NetworkMessage] with [MaxLength] correctly enforces constraints during serialization.
        public void MessageEnforcesLimits()
        {
            var msg = new LimitMessage { Content = "TooLongString" };
            using (var writer = NetworkWriterPool.GetWriter())
            {
                // Exceeding limit during write must throw SerializationLimitException
                Assert.Throws<SerializationLimitException>(() =>
                {
                    writer.Write(msg);
                });
            }

            // Exceeding limit during read must throw SerializationLimitException
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                // Bypass write check by manually writing string of size 8
                writer.WriteString("abcde123", 10);
                bytes = writer.ToArray();
            }

            using (var reader = NetworkReaderPool.GetReader(bytes, null))
            {
                Assert.Throws<SerializationLimitException>(() =>
                {
                    reader.Read<LimitMessage>();
                });
            }
        }

        [Test]
        // Verify that a [ServerRpc] skeleton method automatically enforces length constraints at runtime.
        public void RpcEnforcesLimits()
        {
            var connection = Substitute.For<Mirage.SocketLayer.IConnection>();
            var server = CreateMonoBehaviour<NetworkServer>();
            var player = new NetworkPlayer(connection, false, server, new RateLimitBucket.RefillConfig { MaxTokens = 100, Refill = 10, Interval = 1 });
            // We set player authentication and owner because ServerRpcs require validation checks
            // to succeed before deserializing parameters and executing on the server.
            player.SetAuthentication(new Mirage.Authentication.PlayerAuthentication(null, null), true);

            var go = new GameObject("DummyObj");
            var identity = go.AddComponent<NetworkIdentity>();
            var behaviour = go.AddComponent<RpcBehaviour>();
            identity.SetOwner(player);

            // The Weaver generates the remote call list. Find our ServerRpc.
            RemoteCall remoteCall = null;
            foreach (var rc in identity.RemoteCallCollection.RemoteCalls)
            {
                if (rc.Name.Contains("SendString") && !rc.Name.Contains("SendStringClient") && !rc.Name.Contains("SendString2") && rc.InvokeType == RpcInvokeType.ServerRpc)
                    remoteCall = rc;
            }
            Assert.That(remoteCall, Is.Not.Null);

            // Construct RpcHandler
            var objectLocator = Substitute.For<IObjectLocator>();
            objectLocator.TryGetIdentity(Arg.Any<uint>(), out Arg.Any<NetworkIdentity>())
                .Returns(x =>
                {
                    x[1] = identity;
                    return true;
                });

            var rpcHandler = new RpcHandler(objectLocator, RpcInvokeType.ServerRpc);

            // Construct payload with string exceeding limit (limit is 4, write 5)
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteString("abcde", 10);
                bytes = writer.ToArray();
            }

            var msg = new RpcMessage
            {
                NetId = identity.NetId,
                FunctionIndex = Array.IndexOf(identity.RemoteCallCollection.RemoteCalls, remoteCall),
                Payload = new ArraySegment<byte>(bytes)
            };

            LogAssert.ignoreFailingMessages = true;
            rpcHandler.OnRpcMessage(player, msg);
            LogAssert.ignoreFailingMessages = false;

            // Verify RPC execution caught limit violation and marked player error flag
            Assert.That((player.ErrorFlags & PlayerErrorFlags.SerializationLimit) != 0, Is.True);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        // Verify that a [ServerRpc] skeleton method automatically enforces length constraints at runtime.
        public void RpcEnforcesLimits2()
        {
            var connection = Substitute.For<Mirage.SocketLayer.IConnection>();
            var server = CreateMonoBehaviour<NetworkServer>();
            var player = new NetworkPlayer(connection, false, server, new RateLimitBucket.RefillConfig { MaxTokens = 100, Refill = 10, Interval = 1 });
            // We set player authentication and owner because ServerRpcs require validation checks
            // to succeed before deserializing parameters and executing on the server.
            player.SetAuthentication(new Mirage.Authentication.PlayerAuthentication(null, null), true);

            var go = new GameObject("DummyObj");
            var identity = go.AddComponent<NetworkIdentity>();
            var behaviour = go.AddComponent<RpcBehaviour>();
            identity.SetOwner(player);

            // The Weaver generates the remote call list. Find our ServerRpc.
            RemoteCall remoteCall = null;
            foreach (var rc in identity.RemoteCallCollection.RemoteCalls)
            {
                if (rc.Name.Contains("SendString2") && rc.InvokeType == RpcInvokeType.ServerRpc)
                    remoteCall = rc;
            }
            Assert.That(remoteCall, Is.Not.Null);

            // Construct RpcHandler
            var objectLocator = Substitute.For<IObjectLocator>();
            objectLocator.TryGetIdentity(Arg.Any<uint>(), out Arg.Any<NetworkIdentity>())
                .Returns(x =>
                {
                    x[1] = identity;
                    return true;
                });

            var rpcHandler = new RpcHandler(objectLocator, RpcInvokeType.ServerRpc);

            // Construct payload with string exceeding limit (limit is 120, write 130)
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteString(new string('a', 130), 140);
                bytes = writer.ToArray();
            }

            var msg = new RpcMessage
            {
                NetId = identity.NetId,
                FunctionIndex = Array.IndexOf(identity.RemoteCallCollection.RemoteCalls, remoteCall),
                Payload = new ArraySegment<byte>(bytes)
            };

            LogAssert.ignoreFailingMessages = true;
            rpcHandler.OnRpcMessage(player, msg);
            LogAssert.ignoreFailingMessages = false;

            // Verify RPC execution caught limit violation and marked player error flag
            Assert.That((player.ErrorFlags & PlayerErrorFlags.SerializationLimit) != 0, Is.True);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        // Verify that a [ClientRpc] skeleton method automatically enforces length constraints at runtime.
        public void ClientRpcEnforcesLimits()
        {
            var connection = Substitute.For<Mirage.SocketLayer.IConnection>();
            var server = CreateMonoBehaviour<NetworkServer>();
            var player = new NetworkPlayer(connection, false, server, new RateLimitBucket.RefillConfig { MaxTokens = 100, Refill = 10, Interval = 1 });
            // We set player authentication and owner because RPC messages require validation checks
            // to succeed before deserializing parameters.
            player.SetAuthentication(new Mirage.Authentication.PlayerAuthentication(null, null), true);

            var go = new GameObject("DummyObj");
            var identity = go.AddComponent<NetworkIdentity>();
            var behaviour = go.AddComponent<RpcBehaviour>();
            identity.SetOwner(player);

            // The Weaver generates the remote call list. Find our ClientRpc.
            RemoteCall remoteCall = null;
            foreach (var rc in identity.RemoteCallCollection.RemoteCalls)
            {
                if (rc.Name.Contains("SendStringClient") && !rc.Name.Contains("SendStringClient2") && rc.InvokeType == RpcInvokeType.ClientRpc)
                    remoteCall = rc;
            }
            Assert.That(remoteCall, Is.Not.Null);

            // Construct RpcHandler
            var objectLocator = Substitute.For<IObjectLocator>();
            objectLocator.TryGetIdentity(Arg.Any<uint>(), out Arg.Any<NetworkIdentity>())
                .Returns(x =>
                {
                    x[1] = identity;
                    return true;
                });

            var rpcHandler = new RpcHandler(objectLocator, RpcInvokeType.ClientRpc);

            // Construct payload with string exceeding limit (limit is 5, write 6)
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteString("abcde1", 10);
                bytes = writer.ToArray();
            }

            var msg = new RpcMessage
            {
                NetId = identity.NetId,
                FunctionIndex = Array.IndexOf(identity.RemoteCallCollection.RemoteCalls, remoteCall),
                Payload = new ArraySegment<byte>(bytes)
            };

            LogAssert.ignoreFailingMessages = true;
            rpcHandler.OnRpcMessage(player, msg);
            LogAssert.ignoreFailingMessages = false;

            // Verify RPC execution caught limit violation and marked player error flag
            Assert.That((player.ErrorFlags & PlayerErrorFlags.SerializationLimit) != 0, Is.True);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        // Verify that a [ClientRpc] skeleton method automatically enforces length constraints at runtime.
        public void ClientRpcEnforcesLimits2()
        {
            var connection = Substitute.For<Mirage.SocketLayer.IConnection>();
            var server = CreateMonoBehaviour<NetworkServer>();
            var player = new NetworkPlayer(connection, false, server, new RateLimitBucket.RefillConfig { MaxTokens = 100, Refill = 10, Interval = 1 });
            // We set player authentication and owner because RPC messages require validation checks
            // to succeed before deserializing parameters.
            player.SetAuthentication(new Mirage.Authentication.PlayerAuthentication(null, null), true);

            var go = new GameObject("DummyObj");
            var identity = go.AddComponent<NetworkIdentity>();
            var behaviour = go.AddComponent<RpcBehaviour>();
            identity.SetOwner(player);

            // The Weaver generates the remote call list. Find our ClientRpc.
            RemoteCall remoteCall = null;
            foreach (var rc in identity.RemoteCallCollection.RemoteCalls)
            {
                if (rc.Name.Contains("SendStringClient2") && rc.InvokeType == RpcInvokeType.ClientRpc)
                    remoteCall = rc;
            }
            Assert.That(remoteCall, Is.Not.Null);

            // Construct RpcHandler
            var objectLocator = Substitute.For<IObjectLocator>();
            objectLocator.TryGetIdentity(Arg.Any<uint>(), out Arg.Any<NetworkIdentity>())
                .Returns(x =>
                {
                    x[1] = identity;
                    return true;
                });

            var rpcHandler = new RpcHandler(objectLocator, RpcInvokeType.ClientRpc);

            // Construct payload with string exceeding limit (limit is 150, write 160)
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteString(new string('a', 160), 170);
                bytes = writer.ToArray();
            }

            var msg = new RpcMessage
            {
                NetId = identity.NetId,
                FunctionIndex = Array.IndexOf(identity.RemoteCallCollection.RemoteCalls, remoteCall),
                Payload = new ArraySegment<byte>(bytes)
            };

            LogAssert.ignoreFailingMessages = true;
            rpcHandler.OnRpcMessage(player, msg);
            LogAssert.ignoreFailingMessages = false;

            // Verify RPC execution caught limit violation and marked player error flag
            Assert.That((player.ErrorFlags & PlayerErrorFlags.SerializationLimit) != 0, Is.True);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        // Verify that a [SyncVar] with [MaxLength] correctly enforces constraints during serialization.
        public void SyncVarEnforcesLimits()
        {
            var go = new GameObject("DummyObj");
            var identity = go.AddComponent<NetworkIdentity>();
            var behaviour = go.AddComponent<SyncVarBehaviour>();

            // Within limits should succeed
            behaviour.Content = "abc";
            using (var writer = NetworkWriterPool.GetWriter())
            {
                Assert.DoesNotThrow(() =>
                {
                    behaviour.SerializeSyncVars(writer, true);
                });
            }

            // Exceeding limits should throw SerializationLimitException
            behaviour.Content = "TooLongString";
            using (var writer = NetworkWriterPool.GetWriter())
            {
                Assert.Throws<SerializationLimitException>(() =>
                {
                    behaviour.SerializeSyncVars(writer, true);
                });
            }

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        // Verify that when a hacker sends a custom payload for a [NetworkMessage] that violates length limits,
        // the MessageHandler catches it, logs it, and sets the PlayerErrorFlags.SerializationLimit flag.
        public void MessageHackerBypassSetsErrorFlags()
        {
            var connection = Substitute.For<Mirage.SocketLayer.IConnection>();
            var server = CreateMonoBehaviour<NetworkServer>();
            var player = new NetworkPlayer(connection, false, server, new RateLimitBucket.RefillConfig { MaxTokens = 100, Refill = 10, Interval = 1 });

            var messageHandler = new MessageHandler(null, true);
            MessagePacker.RegisterMessage<LimitMessage>();

            // Register handler for LimitMessage
            var invoked = 0;
            messageHandler.RegisterHandler<LimitMessage>((p, msg) => invoked++, allowUnauthenticated: true);

            // Bypassing write checks mimics a modified or malicious client that manually writes
            // payload data exceeding the allowed limit, testing that the receiver still enforces constraints.
            byte[] bytes;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                // Write standard message ID first
                writer.WriteUInt16((ushort)MessagePacker.GetId<LimitMessage>());
                writer.WriteString("abcde123", 10);
                bytes = writer.ToArray();
            }

            LogAssert.ignoreFailingMessages = true;
            messageHandler.HandleMessage(player, new ArraySegment<byte>(bytes));
            LogAssert.ignoreFailingMessages = false;

            // Verify message handler failed and marked player error flag
            Assert.That(invoked, Is.EqualTo(0));
            Assert.That((player.ErrorFlags & PlayerErrorFlags.SerializationLimit) != 0, Is.True);
        }

        [Test]
        // Verify that when a hacker sends a custom payload for a [SyncVar] that violates length limits,
        // the SyncVarReceiver / MessageHandler catches it, logs it, and sets the PlayerErrorFlags.SerializationLimit flag.
        public void SyncVarHackerBypassSetsErrorFlags()
        {
            var connection = Substitute.For<Mirage.SocketLayer.IConnection>();
            var server = CreateMonoBehaviour<NetworkServer>();
            var player = new NetworkPlayer(connection, false, server, new RateLimitBucket.RefillConfig { MaxTokens = 100, Refill = 10, Interval = 1 });
            // We set player authentication because SyncVar updates require the player connection
            // to be authenticated.
            player.SetAuthentication(new Mirage.Authentication.PlayerAuthentication(null, null), true);

            var go = new GameObject("DummyObj");
            var identity = go.AddComponent<NetworkIdentity>();
            var behaviour = go.AddComponent<SyncVarBehaviour>();

            // Set sync direction to To.Server and From.Owner so the server is allowed to deserialize it
            behaviour.SyncSettings = new SyncSettings
            {
                From = SyncFrom.Owner,
                To = SyncTo.Server,
                Interval = 0.1f
            };

            // Set up local behavior so that it is dirty/ready for deserialization
            var objectLocator = Substitute.For<IObjectLocator>();
            objectLocator.TryGetIdentity(Arg.Any<uint>(), out Arg.Any<NetworkIdentity>())
                .Returns(x =>
                {
                    x[1] = identity;
                    return true;
                });

            var messageHandler = new MessageHandler(null, true);
            var syncVarReceiver = new SyncVarReceiver(objectLocator);
            syncVarReceiver.ServerRegisterHandlers(messageHandler);

            // Bypassing write checks mimics a modified or malicious client that manually writes
            // payload data exceeding the allowed limit, testing that the receiver still enforces constraints.
            byte[] syncVarPayload;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                // Write component index
                writer.WriteByte(0);
                // SyncVar serialization writes the dirty mask first (1 for index 0 behaviour)
                writer.WritePackedUInt64(1);
                // Write string value that is longer than the MaxLength limit of 6 (e.g. length 8)
                writer.WriteString("abcde123", 10);
                // Write the serialization barrier byte (171)
                writer.WriteByte(171);
                syncVarPayload = writer.ToArray();
            }

            // Pack an UpdateVarsMessage
            var updateVars = new UpdateVarsMessage
            {
                NetId = identity.NetId,
                Payload = new ArraySegment<byte>(syncVarPayload)
            };

            // Register UpdateVarsMessage id
            MessagePacker.RegisterMessage<UpdateVarsMessage>();
            var packet = MessagePacker.Pack(updateVars);

            // We must mock identity to think it is server and has authority
            // We set owner so ValidateReceive passes
            identity.SetOwner(player);

            LogAssert.ignoreFailingMessages = true;
            messageHandler.HandleMessage(player, new ArraySegment<byte>(packet));
            LogAssert.ignoreFailingMessages = false;

            // Verify error was caught and player marked
            Assert.That((player.ErrorFlags & PlayerErrorFlags.SerializationLimit) != 0, Is.True);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }
    }
}
