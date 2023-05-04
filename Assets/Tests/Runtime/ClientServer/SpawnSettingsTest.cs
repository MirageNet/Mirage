using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class SpawnSettingsTest : ClientServerSetup
    {
        [UnityTest]
        public IEnumerator SendsValues(
            [Values(true, false)] bool syncPosition,
            [Values(true, false)] bool syncRotation,
            [Values(true, false)] bool syncScale,
            [Values(true, false)] bool syncName,
            [Values(SyncActiveOption.DoNothing, SyncActiveOption.SyncWithServer, SyncActiveOption.ForceEnable)] SyncActiveOption syncActive,
            [Values(true, false)] bool serverActive
            )
        {
            var settings = new NetworkSpawnSettings
            {
                SendPosition = syncPosition,
                SendRotation = syncRotation,
                SendScale = syncScale,
                SendName = syncName,
                SendActive = syncActive,
            };

            var clone = InstantiateForTest(_characterPrefab);
            clone.SpawnSettings = settings;
            clone.transform.position = new Vector3(10, 0, 0);
            clone.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 45));
            clone.transform.localScale = new Vector3(2, 2, 2);
            clone.name = "server namme";
            clone.gameObject.SetActive(serverActive);

            serverObjectManager.Spawn(clone);

            yield return null;
            yield return null;

            var clientIdentity = _remoteClients[0].Get(clone);

            Assert.That(clientIdentity.transform.position, Is.EqualTo(syncPosition ? clone.transform.position : default));

            var expectedRotation = syncRotation ? clone.transform.rotation : Quaternion.identity;
            // check angle, because Quaternion can be compared directly
            var angle = Quaternion.Angle(clientIdentity.transform.rotation, expectedRotation);
            Assert.That(angle, Is.LessThan(0.1f));

            Assert.That(clientIdentity.transform.localScale, Is.EqualTo(syncScale ? clone.transform.localScale : Vector3.one));

            var expectedName = syncName ? clone.name : $"{_characterPrefab.name}(Clone)";
            Assert.That(clientIdentity.name, Is.EqualTo(expectedName));

            var clientIsActive = clientIdentity.gameObject.activeSelf;
            switch (syncActive)
            {
                case SyncActiveOption.DoNothing:
                    Assert.That(clientIsActive, Is.EqualTo(false), "prefab is not active, so spawned object should be not active");
                    break;
                case SyncActiveOption.SyncWithServer:
                    Assert.That(clientIsActive, Is.EqualTo(serverActive));
                    break;
                case SyncActiveOption.ForceEnable:
                    Assert.That(clientIsActive, Is.EqualTo(true));
                    break;
            }


        }
    }
}

