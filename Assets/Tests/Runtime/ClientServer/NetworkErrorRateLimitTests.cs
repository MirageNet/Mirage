using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using Mirage.Tests.Runtime.Host;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.ErrorRateLimit
{
    public class NetworkErrorRateLimitTests : ClientServerSetup
    {
        private const int MAX_TOKENS = 100;
        private const int REFILL_RATE = 20;
        private const float INTERVAL = 1.0f; // seconds

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
        }

        [Test]
        public void PlayerHasRateLimitBucketWhenEnabled()
        {
            Assert.That(serverPlayer.ErrorRateLimit, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator PlayerDisconnectsWhenErrorLimitReached() => UniTask.ToCoroutine(async () =>
        {
            var disconnected = false;
            server.Disconnected.AddListener(player =>
            {
                if (player == serverPlayer)
                {
                    disconnected = true;
                }
            });

            // Consume all tokens plus one
            serverPlayer.SetError(MAX_TOKENS + 1, PlayerErrorFlags.Critical);

            // Wait for a few frames for disconnect to process
            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(disconnected, Is.True);
            Assert.That(client.IsConnected, Is.False);
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.Critical));
        });

        [UnityTest]
        public IEnumerator SetErrorAndDisconnectDisconnectsPlayer() => UniTask.ToCoroutine(async () =>
        {
            var disconnected = false;
            server.Disconnected.AddListener(player =>
            {
                if (player == serverPlayer)
                {
                    disconnected = true;
                }
            });

            serverPlayer.SetErrorAndDisconnect(PlayerErrorFlags.Critical);

            // Wait for a few frames for disconnect to process
            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(disconnected, Is.True);
            Assert.That(client.IsConnected, Is.False);
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.Critical));
        });


        [UnityTest]
        public IEnumerator CustomCallbackInvokedWhenErrorLimitReached() => UniTask.ToCoroutine(async () =>
        {
            var callbackInvoked = false;
            INetworkPlayer receivedPlayer = null;

            server.SetErrorRateLimitReachedCallback((player) =>
            {
                callbackInvoked = true;
                receivedPlayer = player;
            });

            // Consume all tokens plus one
            serverPlayer.SetError(MAX_TOKENS + 1, PlayerErrorFlags.RpcException);

            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(callbackInvoked, Is.True);
            Assert.That(receivedPlayer, Is.EqualTo(serverPlayer));
            Assert.That(client.IsConnected, Is.True, "Client should not be disconnected if custom callback is used");
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcException));
        });

        [UnityTest]
        public IEnumerator SetErrorAndDisconnectCallsCustomCallback() => UniTask.ToCoroutine(async () =>
        {
            var callbackInvoked = false;
            INetworkPlayer receivedPlayer = null;

            server.SetErrorRateLimitReachedCallback((player) =>
            {
                callbackInvoked = true;
                receivedPlayer = player;
            });

            serverPlayer.SetErrorAndDisconnect(PlayerErrorFlags.RpcException);

            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(callbackInvoked, Is.True);
            Assert.That(receivedPlayer, Is.EqualTo(serverPlayer));
            Assert.That(client.IsConnected, Is.True, "Client should not be disconnected if custom callback is used");
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcException));
        });

        [Test]
        public void ErrorFlagsAreSetCorrectly()
        {
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.None));

            serverPlayer.SetError(1, PlayerErrorFlags.RpcNullException);
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcNullException));

            serverPlayer.SetError(1, PlayerErrorFlags.DeserializationException);
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.RpcNullException | PlayerErrorFlags.DeserializationException));

            serverPlayer.ResetErrorFlag();
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.None));
        }

        [UnityTest]
        public IEnumerator ErrorRateLimitRefillsTokensOverTime() => UniTask.ToCoroutine(async () =>
        {
            // Consume most tokens
            serverPlayer.SetError(MAX_TOKENS - (REFILL_RATE / 2), PlayerErrorFlags.None);

            // Simulate time passing (half of interval)
            server.UpdateReceive(); // Triggers player error limit update
            await UniTask.Delay((int)(INTERVAL * 1000 / 2), ignoreTimeScale: true);
            server.UpdateReceive(); // Triggers player error limit update

            // No refill yet as not enough time passed

            // Consume all remaining tokens, should not disconnect
            Assert.That(serverPlayer.ErrorRateLimit.UseTokens(REFILL_RATE / 2), Is.False);

            // Simulate time passing (full interval)
            await UniTask.Delay((int)(INTERVAL * 1000), ignoreTimeScale: true);
            server.UpdateReceive(); // Triggers player error limit update

            // Should have refilled REFILL_RATE tokens.
            Assert.That(serverPlayer.ErrorRateLimit.UseTokens(REFILL_RATE), Is.False, "Should have refilled enough to consume max tokens again");
            Assert.That(serverPlayer.ErrorRateLimit.UseTokens(1), Is.True, "Should be empty after consuming one more token");
        });



    }
    public class NetworkErrorRateLimitHost : HostSetup
    {
        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();
            server.ErrorRateLimitEnabled = true;
            server.ErrorRateLimitConfig = new RateLimitBucket.RefillConfig
            {
                MaxTokens = 100,
                Refill = 10,
                Interval = 1
            };
        }

        [Test]
        public void HostPlayerIsNotGivenErrorRateLimit()
        {
            Assert.That(server.LocalPlayer.ErrorRateLimit, Is.Null, "Host player should not be given a ErrorRateLimit");
        }

        [Test]
        public void NothingIsSetIfSetErrorIsCalledOnHost()
        {
            server.LocalPlayer.SetError(1, PlayerErrorFlags.RpcException);
            Assert.That(server.LocalPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.None));
        }
    }

    public class NetworkErrorRateLimitDisabledTests : ClientServerSetup
    {
        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();
            server.ErrorRateLimitEnabled = false;
        }

        [Test]
        public void PlayerDoesNotHaveRateLimitBucketWhenDisabled()
        {
            Assert.That(serverPlayer.ErrorRateLimit, Is.Null);
        }

        [Test]
        public void SetErrorWhenRateLimitNotEnabledHasNoEffect()
        {
            serverPlayer.SetError(1, PlayerErrorFlags.RpcException);
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.None));
        }

        [UnityTest]
        public IEnumerator SetErrorAndDisconnectDisconnectsWhenRateLimitDisabled() => UniTask.ToCoroutine(async () =>
        {
            var disconnected = false;
            server.Disconnected.AddListener(player =>
            {
                if (player == serverPlayer)
                {
                    disconnected = true;
                }
            });

            serverPlayer.SetErrorAndDisconnect(PlayerErrorFlags.Critical);

            // Wait for a few frames for disconnect to process
            await UniTask.Yield();
            await UniTask.Yield();

            Assert.That(disconnected, Is.True);
            Assert.That(client.IsConnected, Is.False);
            Assert.That(serverPlayer.ErrorFlags, Is.EqualTo(PlayerErrorFlags.None), "Error flags should not be set if rate limit is disabled");
        });
    }
}
