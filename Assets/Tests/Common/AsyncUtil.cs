using System;
using Cysharp.Threading.Tasks;

namespace Mirage.Tests
{
    public class AssertionMethodAttribute : Attribute { }

    public static class AsyncUtil
    {
        [AssertionMethod]
        public static async UniTask<NetworkIdentity> WaitUntilSpawn(NetworkWorld world, uint netId, double timeoutSeconds = 2)
        {
            NetworkIdentity identity = null;
            await WaitUntilWithTimeout(() => world.TryGetIdentity(netId, out identity), timeoutSeconds);

            return identity;
        }

        [AssertionMethod]
        public static UniTask WaitUntilWithTimeout(Func<bool> predicate, double seconds = 2)
        {
            return UniTask.WaitUntil(predicate).Timeout(TimeSpan.FromSeconds(seconds));
        }
    }
}
