using Mirage;

namespace ServerRpcTests.RateLimitNegativeInterval
{
    class RateLimitNegativeInterval : NetworkBehaviour
    {
        [ServerRpc]
        [RateLimit(Interval = 0f)]
        void DoSomething() {}
    }
}
