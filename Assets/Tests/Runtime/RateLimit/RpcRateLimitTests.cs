using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.Tests.Runtime.Host;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.RateLimit
{
    public class RpcRateLimitBehaviour : NetworkBehaviour
    {
        public int Count;

        [ServerRpc(requireAuthority = false)]
        [RateLimit(Interval = 0.5f, Refill = 1, MaxTokens = 2, Penalty = 10)]
        public void ServerRpcWithRateLimit()
        {
            Count++;
        }

        [ServerRpc(requireAuthority = false)]
        [RateLimit(Interval = 1f, Refill = 1, MaxTokens = 1)]
        public void AnotherRateLimitedRpc()
        {
            Count++;
        }

        public int AsyncCount;

        [ServerRpc(requireAuthority = false)]
        [RateLimit(Interval = 0.5f, Refill = 1, MaxTokens = 2, Penalty = 10)]
        public UniTask<int> AsyncServerRpcWithRateLimit()
        {
            AsyncCount++;
            return UniTask.FromResult(AsyncCount);
        }

        public int UnattributedCount;

        [ServerRpc(requireAuthority = false)]
        public void UnattributedRpc()
        {
            UnattributedCount++;
        }
    }

    public class RpcRateLimitTests : ClientServerSetup<RpcRateLimitBehaviour>
    {
        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();
            server.ErrorRateLimitEnabled = true;
            server.ErrorRateLimitConfig = new Mirage.SocketLayer.RateLimitBucket.RefillConfig
            {
                MaxTokens = 100,
                Refill = 10,
                // Keep interval small so tests don't take too long
                Interval = 1
            };
        }

        [UnityTest]
        public IEnumerator RpcIsThrottledWhenCalledTooFast() => UniTask.ToCoroutine(async () =>
        {
            // First 2 calls should succeed (MaxTokens = 2)
            clientComponent.ServerRpcWithRateLimit();
            clientComponent.ServerRpcWithRateLimit();

            await UniTask.Delay(100);

            Assert.That(serverComponent.Count, Is.EqualTo(2));

            // Third call should be dropped
            clientComponent.ServerRpcWithRateLimit();

            await UniTask.Delay(100);

            Assert.That(serverComponent.Count, Is.EqualTo(2), "Third call should have been dropped");
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RateLimit));
        });

        [UnityTest]
        public IEnumerator AsyncRpcIsThrottledWhenCalledTooFast() => UniTask.ToCoroutine(async () =>
        {
            // First 2 calls should succeed (MaxTokens = 2)
            // Fire and forget since we expect the third to drop and not respond, meaning awaiting it would just timeout
            clientComponent.AsyncServerRpcWithRateLimit().Forget();
            clientComponent.AsyncServerRpcWithRateLimit().Forget();

            await UniTask.Delay(100);

            Assert.That(serverComponent.AsyncCount, Is.EqualTo(2));

            // Third call should be dropped
            clientComponent.AsyncServerRpcWithRateLimit().Forget();

            await UniTask.Delay(100);

            Assert.That(serverComponent.AsyncCount, Is.EqualTo(2), "Third call should have been dropped");
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RateLimit));
        });

        [UnityTest]
        public IEnumerator PenaltyIsAppliedToErrorRateLimit() => UniTask.ToCoroutine(async () =>
        {
            // Initial tokens in ErrorRateLimit should be full
            var initialTokens = serverPlayer.ErrorRateLimit.Tokens;

            // Exhaust the RPC limit
            clientComponent.ServerRpcWithRateLimit();
            clientComponent.ServerRpcWithRateLimit();
            await UniTask.Delay(100);

            // This call is dropped and applies penalty
            clientComponent.ServerRpcWithRateLimit();
            await UniTask.Delay(100);

            // Penalty is 10
            Assert.That(serverPlayer.ErrorRateLimit.Tokens, Is.EqualTo(initialTokens - 10));
        });

        [UnityTest]
        public IEnumerator DifferentRpcsHaveSeparateBuckets() => UniTask.ToCoroutine(async () =>
        {
            // Exhaust first RPC limit
            clientComponent.ServerRpcWithRateLimit();
            clientComponent.ServerRpcWithRateLimit();
            clientComponent.ServerRpcWithRateLimit(); // Dropped

            await UniTask.Delay(100);

            Assert.That(serverComponent.Count, Is.EqualTo(2));

            // Call another RPC, should still work
            clientComponent.AnotherRateLimitedRpc();

            await UniTask.Delay(100);

            Assert.That(serverComponent.Count, Is.EqualTo(3));
        });

        [UnityTest]
        public IEnumerator RpcRefillsOverTime() => UniTask.ToCoroutine(async () =>
        {
            // Exhaust limit
            clientComponent.ServerRpcWithRateLimit();
            clientComponent.ServerRpcWithRateLimit();

            await UniTask.Delay(100);

            Assert.That(serverComponent.Count, Is.EqualTo(2));

            // Wait for refill (Interval = 0.5s)
            await UniTask.Delay(600);

            // Should be able to call again
            clientComponent.ServerRpcWithRateLimit();

            await UniTask.Delay(100);

            Assert.That(serverComponent.Count, Is.EqualTo(3));
        });

        [UnityTest]
        public IEnumerator RpcDoesNotRefillBeforeInterval() => UniTask.ToCoroutine(async () =>
        {
            // Exhaust limit
            clientComponent.ServerRpcWithRateLimit();
            clientComponent.ServerRpcWithRateLimit();

            await UniTask.Delay(100);

            Assert.That(serverComponent.Count, Is.EqualTo(2));

            // Wait less than the refill interval (Interval = 0.5s)
            await UniTask.Delay(200);

            // Should still be unable to call because it hasn't refilled
            clientComponent.ServerRpcWithRateLimit();

            await UniTask.Delay(100);

            Assert.That(serverComponent.Count, Is.EqualTo(2), "Call should be dropped because interval hasn't accumulated enough time for a refill");
        });

        [UnityTest]
        public IEnumerator RpcWithoutAttributeIsNotRateLimited() => UniTask.ToCoroutine(async () =>
        {
            for (int i = 0; i < 1000; i++)
            {
                clientComponent.UnattributedRpc();
            }

            await UniTask.Delay(100); // adjust delay if needed

            Assert.That(serverComponent.UnattributedCount, Is.EqualTo(1000), "RPC without attribute should not be rate limited");
            Assert.That(serverPlayer.ErrorFlags, Is.Not.EqualTo(PlayerErrorFlags.RateLimit));
        });
    }

    public class RpcRateLimitHostTests : HostSetup<RpcRateLimitBehaviour>
    {
        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();
            server.ErrorRateLimitEnabled = true;
            server.ErrorRateLimitConfig = new Mirage.SocketLayer.RateLimitBucket.RefillConfig
            {
                MaxTokens = 100,
                Refill = 10,
                Interval = 1
            };
        }

        [UnityTest]
        public IEnumerator RpcIsNotThrottledForHost() => UniTask.ToCoroutine(async () =>
        {
            // MaxTokens is 2 for the attribute, but as a host, there should be no rate limiting.
            for (int i = 0; i < 5; i++)
            {
                hostComponent.ServerRpcWithRateLimit();
            }

            // Wait to ensure processing
            await UniTask.Delay(100);

            Assert.That(hostComponent.Count, Is.EqualTo(5), "Host should not be rate limited");
            Assert.That(server.LocalPlayer.ErrorFlags, Is.Not.EqualTo(PlayerErrorFlags.RateLimit));
        });
    }
}
