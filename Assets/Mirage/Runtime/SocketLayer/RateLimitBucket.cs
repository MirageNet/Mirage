using System;

namespace Mirage.SocketLayer
{
    public class RateLimitBucket
    {
        private readonly RefillConfig _config;
        private float _tokens;
        private double _previousRefill;

        public RateLimitBucket(double now, RefillConfig config)
        {
            if (config.Interval <= 0)
                throw new ArgumentException("Interval can't be negative", "RefillConfig.Interval");
            if (config.MaxTokens < 0)
                throw new ArgumentException("MaxTokens can't be negative", "RefillConfig.MaxTokens");
            if (config.Refill < 0)
                throw new ArgumentException("Refill can't be negative", "RefillConfig.Refill");

            _config = config;
            _tokens = config.MaxTokens;
            _previousRefill = now;
        }

        /// <summary>
        /// refills tokens based on config
        /// </summary>
        /// <param name="now">seconds</param>
        public void CheckRefill(double now)
        {
            var elapsed = (float)(now - _previousRefill);

            // this will cover case where elapsed is negative
            // (which shouldn't happen but if it does it will be fine)
            if (elapsed < _config.Interval)
                return;

            _previousRefill = now;
            var tokensPerSecond = _config.Refill / _config.Interval;
            var tokensToAdd = elapsed * tokensPerSecond;
            _tokens = Math.Min(_config.MaxTokens, _tokens + tokensToAdd);
        }

        /// <summary>
        /// subtracts cost from token count, returns true if tokens is negative (we ran out)
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        public bool UseTokens(int cost)
        {
            _tokens -= cost;
            return _tokens < 0;
        }

        /// <summary>
        /// returns true if tokens is negative (we ran out)
        /// </summary>
        /// <returns></returns>
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
            /// <summary>Max number of tokens in bucket. set this number higher than per seconds value to allow bursts of usage</summary>
            public int MaxTokens;
        }
    }
}
