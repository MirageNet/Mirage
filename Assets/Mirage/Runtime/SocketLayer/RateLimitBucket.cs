using System;

namespace Mirage.SocketLayer
{
    public class RateLimitBucket
    {
        public readonly RefillConfig Config;
        private float _tokens;
        private double _previousRefill;

        public float Tokens => _tokens;

        public RateLimitBucket(double now, RefillConfig config)
        {
            if (config.Interval <= 0)
                throw new ArgumentException("Interval can't be negative", "RefillConfig.Interval");
            if (config.MaxTokens < 0)
                throw new ArgumentException("MaxTokens can't be negative", "RefillConfig.MaxTokens");
            if (config.Refill < 0)
                throw new ArgumentException("Refill can't be negative", "RefillConfig.Refill");

            Config = config;
            _tokens = config.MaxTokens;
            _previousRefill = now;
        }

        /// <summary>
        /// Refills the bucket based on time passed and then consumes tokens.
        /// Useful for rarely used buckets that need to be up-to-date at the moment of a call.
        /// </summary>
        public bool UseTokens(double now, int amount)
        {
            CheckRefill(now);
            return UseTokens(amount);
        }

        /// <summary>
        /// Refills tokens based on config. If the bucket is full, it will reset the refill timer to 'now'.
        /// </summary>
        /// <param name="now">Current time in seconds</param>
        public void CheckRefill(double now)
        {
            var elapsed = (float)(now - _previousRefill);

            // this will cover case where elapsed is negative
            // (which shouldn't happen but if it does it will be fine)
            if (elapsed < Config.Interval)
                return;

            _previousRefill = now;
            var tokensPerSecond = Config.Refill / Config.Interval;
            var tokensToAdd = elapsed * tokensPerSecond;

            // Clamp to MaxTokens so that idle buckets don't accumulate infinite credit
            _tokens = Math.Min(Config.MaxTokens, _tokens + tokensToAdd);
        }

        /// <summary>
        /// Subtracts cost from token count. 
        /// Tokens can go negative to penalize burst spam.
        /// </summary>
        /// <returns>True if the bucket is empty (negative), indicating the rate limit was exceeded.</returns>
        public bool UseTokens(int cost)
        {
            _tokens -= cost;
            return _tokens < 0;
        }

        /// <summary>
        /// Returns true if tokens is negative (indicating we ran out and are currently in debt).
        /// </summary>
        public bool IsEmpty()
        {
            return _tokens < 0;
        }

        [System.Serializable]
        public struct RefillConfig
        {
            /// <summary>Seconds</summary>
            public float Interval;
            /// <summary>How many tokens refilled each interval</summary>
            public int Refill;
            /// <summary>Max number of tokens in bucket. Set this higher than 'Refill' to allow for bursts of usage.</summary>
            public int MaxTokens;
        }
    }
}
