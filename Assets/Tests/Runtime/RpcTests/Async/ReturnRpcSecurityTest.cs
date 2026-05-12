using System.Collections;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Mirage.RemoteCalls;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.RpcTests.Async
{
    public class ReturnRpcComponent_Delayed : NetworkBehaviour
    {
        public int rpcResult;

        [ClientRpc(target = RpcTarget.Player)]
        public async UniTask<int> GetResultTarget(INetworkPlayer target)
        {
            await UniTask.Delay(500);
            return rpcResult;
        }
    }

    public class ReturnRpcSecurityTest : MultiRemoteClientSetup<ReturnRpcComponent_Delayed>
    {
        protected override int RemoteClientCount => 2;

        [UnityTest]
        public IEnumerator SpoofedReplyIsRejected() => UniTask.ToCoroutine(async () =>
        {
            var serverPlayer0 = _serverInstance.Players[0].Player;
            var serverPlayer1 = _serverInstance.Players[1].Player;
            var client1 = Client(1);

            // 1. Server calls return-RPC on Client 0
            var rpcHandler = serverObjectManager._rpcHandler;
            var expectedId = rpcHandler._nextReplyId;

            ClientComponent(0).rpcResult = 42;
            var task = ServerComponent(0).GetResultTarget(serverPlayer0);

            // 2. Client 1 (attacker) immediately tries to spoof the reply
            LogAssert.Expect(LogType.Error, new Regex($".*Received RpcReply for id={expectedId} from .* but expected sender was .* Possible spoofing attempt.*"));
            client1.Send(new RpcReply
            {
                ReplyId = expectedId,
                Success = true,
                // Serialized int 10
                Payload = new System.ArraySegment<byte>(new byte[] { 10, 0, 0, 0 })
            });

            // Wait a few frames for server to process the spoof
            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            // Verify task still pending
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending));
            Assert.That(serverPlayer1.ErrorFlags.HasFlag(PlayerErrorFlags.LikelyCheater), Is.True);

            // 3. Wait for legitimate reply from Client 0 (after 0.5s delay)
            var result = await task;
            Assert.That(result, Is.EqualTo(42));
        });

        [UnityTest]
        public IEnumerator UnknownIdIsRejected() => UniTask.ToCoroutine(async () =>
        {
            var serverPlayer1 = _serverInstance.Players[1].Player;
            var client1 = Client(1);

            LogAssert.Expect(LogType.Error, new Regex($".*Received RpcReply from .* but no pending callbacks for id=999.*"));

            client1.Send(new RpcReply
            {
                ReplyId = 999,
                Success = true
            });

            // Wait for server to process
            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(serverPlayer1.ErrorFlags.HasFlag(PlayerErrorFlags.InvalidState), Is.True);
        });
    }
}
