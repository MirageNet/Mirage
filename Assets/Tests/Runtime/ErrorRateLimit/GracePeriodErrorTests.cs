using System;
using System.Collections;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ErrorRateLimit
{
    public class GracePeriodBehaviour : NetworkBehaviour
    {
        [ServerRpc]
        public void TestServerRpc()
        {
            // used in tests
        }
    }

    public class GracePeriodErrorTests : ClientServerSetup<GracePeriodBehaviour>
    {
        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();
            server.ErrorRateLimitEnabled = true;
            server.ErrorRateLimitConfig = new RateLimitBucket.RefillConfig
            {
                MaxTokens = 100,
                Refill = 20,
                Interval = 1.0f
            };
        }

        private RpcMessage CreateRpcMessage(NetworkIdentity identity, string funcName, NetworkWriter writer)
        {
            var component = identity.GetComponent<GracePeriodBehaviour>();
            var remoteCalls = identity.RemoteCallCollection.RemoteCalls;
            var functionIndex = -1;

            for (var i = 0; i < remoteCalls.Length; i++)
            {
                var remoteCall = remoteCalls[i];
                if (remoteCall != null && remoteCall.Behaviour == component && remoteCall.Name.Contains(funcName))
                {
                    functionIndex = i;
                    break;
                }
            }
            if (functionIndex == -1)
            {
                throw new InvalidOperationException($"Could not find RPC with name '{funcName}' on behaviour '{component.GetType().Name}'.");
            }

            var rpcMessage = new RpcMessage
            {
                NetId = identity.NetId,
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

        [UnityTest]
        public IEnumerator AuthorityGracePeriod() => UniTask.ToCoroutine(async () =>
        {
            // spawn a new object and give authority to client
            var newIdentity = InstantiateForTest(_characterPrefab);
            serverObjectManager.Spawn(newIdentity, serverPlayer);

            // wait for client to spawn it
            await UniTask.Delay(100);
            var clientInstance = _remoteClients[0].Get(newIdentity.GetComponent<GracePeriodBehaviour>());

            Assert.That(clientInstance.HasAuthority, Is.True);
            var initialTokens = serverPlayer.ErrorRateLimit.Tokens;

            RpcMessage rpcMessage;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                rpcMessage = CreateRpcMessage(newIdentity, nameof(GracePeriodBehaviour.TestServerRpc), writer);
            }

            // Remove authority
            newIdentity.RemoveClientAuthority();

            // Immediately send RPC for destroyed object
            LogAssert.Expect(LogType.Warning, new Regex(".*ServerRpc for object without authority.*"));
            SendRpc(rpcMessage);

            await UniTask.Delay(100);
            Assert.That(clientInstance.HasAuthority, Is.False);

            // Should be in grace period, no error
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.None));
            Assert.That(serverPlayer.ErrorRateLimit.Tokens, Is.EqualTo(initialTokens));

            // Wait for grace period to end. Grace period is 1.5s in RpcHandler.
            await UniTask.Delay(2000);

            // Immediately send RPC for destroyed object
            LogAssert.Expect(LogType.Error, new Regex(".*ServerRpc for object without authority.*"));
            SendRpc(rpcMessage);
            await UniTask.Delay(100);

            // Should be outside grace period, now there is an error
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.NoAuthority));
            Assert.That(serverPlayer.ErrorRateLimit.Tokens, Is.LessThan(initialTokens));

        });

        [UnityTest]
        public IEnumerator DestroyedGracePeriod() => UniTask.ToCoroutine(async () =>
        {
            var newIdentity = InstantiateForTest(_characterPrefab);
            serverObjectManager.Spawn(newIdentity);
            var netId = newIdentity.NetId;

            await UniTask.Delay(100);
            var initialTokens = serverPlayer.ErrorRateLimit.Tokens;

            RpcMessage rpcMessage;
            using (var writer = NetworkWriterPool.GetWriter())
            {
                rpcMessage = CreateRpcMessage(newIdentity, nameof(GracePeriodBehaviour.TestServerRpc), writer);
            }

            // Destroy object
            serverObjectManager.Destroy(newIdentity);

            // Immediately send RPC for destroyed object
            LogAssert.Expect(LogType.Warning, new Regex(".*Spawned object not found, but was recently destroyed.*"));
            SendRpc(rpcMessage);
            await UniTask.Delay(100);

            // Should be in grace period, no error
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.None));
            Assert.That(serverPlayer.ErrorRateLimit.Tokens, Is.EqualTo(initialTokens));

            // Wait for grace period to end. Grace period is 1.5s in RpcHandler.
            await UniTask.Delay(2000);

            // Send RPC again
            LogAssert.Expect(LogType.Error, new Regex(".*Spawned object not found when handling ServerRpc message.*"));
            SendRpc(rpcMessage);
            await UniTask.Delay(100);

            // Should be outside grace period, now there is an error
            // object not found error doesn't set a flag, but reduces tokens
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.None));
            Assert.That(serverPlayer.ErrorRateLimit.Tokens, Is.LessThan(initialTokens));
        });
    }
}
