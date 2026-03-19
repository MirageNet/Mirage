using Mirage;

namespace ServerRpcTests.RateLimitNegativePenalty
{
    class RateLimitNegativePenalty : NetworkBehaviour
    {
        [ServerRpc]
        [RateLimit(Penalty = -1)]
        void DoSomething() {}
    }
}
