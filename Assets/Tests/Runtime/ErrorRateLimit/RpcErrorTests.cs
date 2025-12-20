using System;
using System.Collections;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ErrorRateLimit
{
    public class RpcErrorBehaviour : NetworkBehaviour
    {
        [ServerRpc]
        public void ServerRpcThrowsNullRef()
        {
            throw new NullReferenceException("some null object");
        }

        [ServerRpc]
        public void ServerRpcThrowsGeneric()
        {
            throw new Exception("some other problem");
        }

        [ServerRpc]
        public UniTask<int> ReturnRpcThrowsNullRef()
        {
            throw new NullReferenceException("some null object");
        }

        [ServerRpc]
        public UniTask<int> ReturnRpcThrowsGeneric()
        {
            throw new Exception("some other problem");
        }

        [ClientRpc]
        public void ClientRpcForServerRpcTest()
        {
            // used for invoke type test
        }
    }

    public class RpcErrorTests : ClientServerSetup<RpcErrorBehaviour>
    {
        private RpcMessage CreateRpcMessage(string funcName, NetworkWriter writer)
        {
            var remoteCalls = serverIdentity.RemoteCallCollection.RemoteCalls;
            var functionIndex = -1;

            for (var i = 0; i < remoteCalls.Length; i++)
            {
                var remoteCall = remoteCalls[i];
                // Ensure the RemoteCall belongs to the correct behaviour and has the matching name
                if (remoteCall != null && remoteCall.Behaviour == serverComponent && remoteCall.Name.Contains(funcName))
                {
                    functionIndex = i;
                    break;
                }
            }

            if (functionIndex == -1)
            {
                throw new InvalidOperationException($"Could not find RPC with name '{funcName}' on behaviour '{serverComponent.GetType().Name}'.");
            }

            var rpcMessage = new RpcMessage
            {
                NetId = serverComponent.NetId,
                FunctionIndex = functionIndex,
                Payload = writer.ToArraySegment()
            };
            return rpcMessage;
        }

        private void SendRpc(RpcMessage rpcMessage)
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                MessagePacker.Pack(rpcMessage, writer);
                server.MessageHandler.HandleMessage(serverPlayer, writer.ToArraySegment());
            }
        }

        [Test]
        public void InvalidRpcIndex()
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                var rpcMessage = new RpcMessage
                {
                    NetId = serverComponent.NetId,
                    FunctionIndex = 100, // out of bounds
                    Payload = writer.ToArraySegment()
                };

                LogAssert.Expect(LogType.Warning, new Regex(".*Invalid Rpc for index. Out of bounds.*"));
                SendRpc(rpcMessage);
            }

            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcSync));
        }

        [Test]
        public void InvalidInvokeType()
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                // we are sending a ServerRpc message, but using the function index of a ClientRpc
                var rpcMessage = CreateRpcMessage(nameof(RpcErrorBehaviour.ClientRpcForServerRpcTest), writer);

                LogAssert.Expect(LogType.Warning, new Regex(".*Invalid Rpc for index .* Expected ServerRpc but was ClientRpc.*"));
                SendRpc(rpcMessage);
            }

            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcSync));
        }

        [UnityTest]
        public IEnumerator NoAuthority() => UniTask.ToCoroutine(async () =>
        {
            // spawn a new object and don't give authority to client
            var newIdentity = InstantiateForTest(_characterPrefab);
            var newComponent = newIdentity.GetComponent<RpcErrorBehaviour>();
            serverObjectManager.Spawn(newIdentity);

            // wait for client to spawn it
            await UniTask.Delay(100);
            var clientInstance = _remoteClients[0].Get(newComponent);

            Assert.That(clientInstance.HasAuthority, Is.False);

            // expect a warning log on the client when trying to call a ServerRpc without authority

            // we are sending a ServerRpc message, but using the function index of a ClientRpc
            using (var writer = NetworkWriterPool.GetWriter())
            {
                var rpcMessage = CreateRpcMessage(nameof(RpcErrorBehaviour.ServerRpcThrowsGeneric), writer);
                rpcMessage.NetId = newIdentity.NetId;

                LogAssert.Expect(LogType.Warning, new Regex(".*ServerRpc for object without authority.*"));
                SendRpc(rpcMessage);
            }

            // The server should not have received the RPC, so no error flags should be set.
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.NoAuthority));
        });

        [UnityTest]
        public IEnumerator RpcThrowsNullRef() => UniTask.ToCoroutine(async () =>
        {
            clientComponent.ServerRpcThrowsNullRef();

            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcNullException));
        });

        [UnityTest]
        public IEnumerator RpcThrowsGeneric() => UniTask.ToCoroutine(async () =>
        {
            clientComponent.ServerRpcThrowsGeneric();

            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcException));
        });

        [UnityTest]
        public IEnumerator ReturnRpcThrowsNullRef() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Error, new Regex(".*Return RPC threw an Exception: System.NullReferenceException.*"));

            var task = WaitForReply(clientComponent.ReturnRpcThrowsNullRef());

            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcNullException));

            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            var didThrow = await task;
            Assert.IsTrue(didThrow, "ReturnRpc should throw if server throws");
        });

        [UnityTest]
        public IEnumerator ReturnRpcThrowsGeneric() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Error, new Regex(".*Return RPC threw an Exception: System.Exception:.*"));

            var task = WaitForReply(clientComponent.ReturnRpcThrowsGeneric());

            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcException));

            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            var didThrow = await task;
            Assert.IsTrue(didThrow, "ReturnRpc should throw if server throws");
        });

        private async UniTask<bool> WaitForReply(UniTask other)
        {
            try
            {
                await other;
                return false;
            }
            catch (ReturnRpcException)
            {
                return true;
            }
        }
    }
}
