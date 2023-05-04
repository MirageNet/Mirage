using System.Collections;
using Mirage.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Components
{
    public class SyncObjectActiveTest : ClientServerSetup<SyncObjectActive>
    {
        [UnityTest]
        public IEnumerator SyncSelfActive()
        {
            Debug.Assert(serverPlayerGO.activeSelf);
            Debug.Assert(clientPlayerGO.activeSelf);

            serverPlayerGO.SetActive(false);
            serverComponent.SyncSettings.Timing = SyncTiming.NoInterval;

            yield return null;
            yield return null;

            Assert.That(clientPlayerGO.activeSelf, Is.False);
            serverPlayerGO.SetActive(true);

            yield return null;
            yield return null;

            Assert.That(clientPlayerGO.activeSelf, Is.True);
        }
    }
}
