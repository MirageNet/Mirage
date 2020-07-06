using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests
{
    public class NetworkManagerTest
    {
        [UnityTest]
        public IEnumerator NetworkManagerSetupHasComponents()
        {
            NetworkHost networkHost = new GameObject("NetworkHost Test").AddComponent<NetworkHost>();
            yield return null;
            Assert.IsNotNull(networkHost.LocalClient);
            Object.DestroyImmediate(networkHost);
        }
    }
}
