using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ErrorRateLimit
{
    [TestFixture]
    public class NetworkServerRateLimitSustainedRateTests : ClientServerSetup
    {
        private const int MAX_TOKENS = 10;
        private const int REFILL_RATE = 5;
        private const float INTERVAL = 0.2f; // seconds, 200ms

        private bool _disconnected;

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();
            server.ErrorRateLimitEnabled = true;
            server.ErrorRateLimitConfig = new RateLimitBucket.RefillConfig
            {
                MaxTokens = MAX_TOKENS,
                Refill = REFILL_RATE,
                Interval = INTERVAL
            };

            server.Disconnected.AddListener(player =>
            {
                if (player == serverPlayer)
                {
                    _disconnected = true;
                }
            });
        }

        [SetUp]
        public void TestSetup()
        {
            _disconnected = false;
        }

        [UnityTest]
        public IEnumerator BurstOfErrorsUpToMaxTokensDoesNotDisconnectThenDisconnects() => UniTask.ToCoroutine(async () =>
        {
            // use all tokens
            for (var i = 0; i < MAX_TOKENS; i++)
                serverPlayer.SetError(1, PlayerErrorFlags.Critical);
            await UniTask.Yield();
            Assert.That(_disconnected, Is.False, "Should not disconnect if error count is at max tokens");

            // use one more
            serverPlayer.SetError(1, PlayerErrorFlags.Critical);
            await UniTask.Yield();
            await UniTask.Yield();
            Assert.That(_disconnected, Is.True, "Should disconnect if error count is over max tokens");
        });

        [UnityTest]
        public IEnumerator SustainedErrorsBelowRateLimitDoesNotDisconnect() => UniTask.ToCoroutine(async () =>
        {
            // an error rate of REFILL_RATE should be sustainable
            const int errorRate = REFILL_RATE;
            const int duration = 5;

            for (var i = 0; i < duration; i++)
            {
                serverPlayer.SetError(errorRate, PlayerErrorFlags.Critical);
                Assert.That(_disconnected, Is.False, $"Should not disconnect at {i} seconds");

                // wait for tokens to refill
                await UniTask.Delay((int)(INTERVAL * 1000));
            }

            Assert.That(_disconnected, Is.False, "Should not disconnect if error rate is sustainable");
        });

        [UnityTest]
        public IEnumerator SustainedErrorsAboveRateLimitDisconnects() => UniTask.ToCoroutine(async () =>
        {
            // an error rate of REFILL_RATE + 1 should not be sustainable
            const int errorRate = REFILL_RATE + 1;
            // loop long enough to trigger disconnect for sure
            const int duration = MAX_TOKENS + 5;

            for (var i = 0; i < duration; i++)
            {
                serverPlayer.SetError(errorRate, PlayerErrorFlags.Critical);
                // wait for disconnect to process
                await UniTask.Yield();

                if (_disconnected)
                {
                    break;
                }

                // wait for tokens to refill
                await UniTask.Delay((int)(INTERVAL * 1000));
            }

            Assert.That(_disconnected, Is.True, $"Should disconnect after a few seconds with error rate of {errorRate}");
        });
    }
}
