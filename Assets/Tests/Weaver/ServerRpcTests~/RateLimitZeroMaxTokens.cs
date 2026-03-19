using Mirage;

namespace ServerRpcTests.RateLimitZeroMaxTokens
{
    class RateLimitZeroMaxTokens : NetworkBehaviour
    {
        [ServerRpc]
        [RateLimit(MaxTokens = 0)]
        void DoSomething() {}
    }
}
