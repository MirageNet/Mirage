using System;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests
{
    [Category("SocketLayer")]
    public class RateLimitBucketTests
    {
        private double _time;

        [SetUp]
        public void SetUp()
        {
            // start at a large number to pretend like server has been running for over a week
            _time = 3600 * 24 * 7;
        }

        [Test]
        public void ConstructorThrowsOnInvalidInterval()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 0,
                MaxTokens = 10,
                Refill = 1
            };
            var ex = Assert.Throws<ArgumentException>(() => new RateLimitBucket(_time, config));
            Assert.That(ex.ParamName, Is.EqualTo("RefillConfig.Interval"));
        }

        [Test]
        public void ConstructorThrowsOnInvalidMaxTokens()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 1,
                MaxTokens = -1,
                Refill = 1
            };
            var ex = Assert.Throws<ArgumentException>(() => new RateLimitBucket(_time, config));
            Assert.That(ex.ParamName, Is.EqualTo("RefillConfig.MaxTokens"));
        }

        [Test]
        public void ConstructorThrowsOnInvalidRefill()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 1,
                MaxTokens = 10,
                Refill = -1
            };
            var ex = Assert.Throws<ArgumentException>(() => new RateLimitBucket(_time, config));
            Assert.That(ex.ParamName, Is.EqualTo("RefillConfig.Refill"));
        }

        [Test]
        public void InitialTokensAreMaxTokens()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 1,
                MaxTokens = 10,
                Refill = 1
            };
            var bucket = new RateLimitBucket(_time, config);
            // Internal _tokens field is private, but we can infer from UseTokens
            Assert.That(bucket.UseTokens(10), Is.False, "Should not be empty after using max tokens initially");
            Assert.That(bucket.UseTokens(1), Is.True, "Should be empty after using one more token");
        }

        [Test]
        public void CheckRefillAddsTokensOverTime()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 1,
                MaxTokens = 10,
                Refill = 1
            };
            var bucket = new RateLimitBucket(_time, config);
            
            // Consume some tokens
            bucket.UseTokens(5); // 5 tokens left

            // Advance time
            _time += 1.0; // 1 second passes
            bucket.CheckRefill(_time); // Should refill 1 token

            // Check if 1 token was refilled
            Assert.That(bucket.UseTokens(6), Is.False, "Should have 6 tokens after refill and using 6"); // 5 + 1 = 6 tokens now, using all 6 is not empty
            Assert.That(bucket.UseTokens(1), Is.True, "Should be empty after using one more token"); // 6 - 6 = 0, then 0-1 = -1 is empty
        }

        [Test]
        public void CheckRefillCapsAtMaxTokens()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 1,
                MaxTokens = 10,
                Refill = 1
            };
            var bucket = new RateLimitBucket(_time, config);

            // Tokens are already at max (10)

            // Advance time by a lot
            _time += 100.0;
            bucket.CheckRefill(_time);

            // Should still be at max tokens
            Assert.That(bucket.UseTokens(10), Is.False, "Should not be empty after using max tokens (after refill)");
            Assert.That(bucket.UseTokens(1), Is.True, "Should be empty after using one more token");
        }

        [Test]
        public void UseTokensReturnsTrueWhenBucketIsEmpty()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 1,
                MaxTokens = 5,
                Refill = 1
            };
            var bucket = new RateLimitBucket(_time, config);

            Assert.That(bucket.UseTokens(5), Is.False, "Bucket should not be empty after consuming exactly max tokens");
            Assert.That(bucket.UseTokens(1), Is.True, "Bucket should be empty after consuming one more token");
        }

        [Test]
        public void IsEmptyReturnsTrueWhenTokensAreNegative()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 1,
                MaxTokens = 5,
                Refill = 1
            };
            var bucket = new RateLimitBucket(_time, config);

            bucket.UseTokens(5);
            Assert.That(bucket.IsEmpty(), Is.False);

            bucket.UseTokens(1);
            Assert.That(bucket.IsEmpty(), Is.True);
        }

        [Test]
        public void MultipleRefillsWorkCorrectly()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 0.5f,
                MaxTokens = 10,
                Refill = 2
            };
            var bucket = new RateLimitBucket(_time, config); // 10 tokens

            bucket.UseTokens(5); // 5 tokens left

            _time += 0.5; // 0.5 seconds pass
            bucket.CheckRefill(_time); // refills 2 tokens. Now 7 tokens.

            _time += 0.5; // 0.5 seconds pass
            bucket.CheckRefill(_time); // refills 2 tokens. Now 9 tokens.

            Assert.That(bucket.UseTokens(9), Is.False); // Still 0 left, not empty
            Assert.That(bucket.UseTokens(1), Is.True); // Becomes empty
        }

        [Test]
        public void RefillIgnoresSmallTimeAdvances()
        {
            var config = new RateLimitBucket.RefillConfig
            {
                Interval = 1,
                MaxTokens = 10,
                Refill = 1
            };
            var bucket = new RateLimitBucket(_time, config); // 10 tokens

            bucket.UseTokens(5); // 5 tokens left

            _time += 0.4; // Less than interval
            bucket.CheckRefill(_time); // Should not refill

            Assert.That(bucket.UseTokens(5), Is.False); // Still 5 tokens left, not empty
            Assert.That(bucket.UseTokens(1), Is.True); // Becomes empty
        }
    }
}