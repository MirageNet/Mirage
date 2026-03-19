using Mirage;

namespace ServerRpcTests.RateLimitZeroRefill
{
    class RateLimitZeroRefill : NetworkBehaviour
    {
        [ServerRpc]
        [RateLimit(Refill = 0)]
        void DoSomething() {}
    }
}
