using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncDirectionObjectFromServer_Host : SyncDirectionTestBase_Host<MockPlayerWithList>
    {
        private const int listValue = 5;

        [UnityTest]
        public IEnumerator ToOwner()
        {
            SetDirection(SyncFrom.Server, SyncTo.Owner);

            hostComponent.guild = guild;
            hostComponent.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(hostComponent.MySyncList[0], Is.EqualTo(listValue));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.MySyncList.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ToObserver()
        {
            SetDirection(SyncFrom.Server, SyncTo.ObserversOnly);

            hostComponent.guild = guild;
            hostComponent.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            // host mode, so should still be set
            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(hostComponent.MySyncList[0], Is.EqualTo(listValue));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ObserverComponent.MySyncList[0], Is.EqualTo(listValue));
        }

        [UnityTest]
        public IEnumerator ToOwnerAndObserver()
        {
            SetDirection(SyncFrom.Server, SyncTo.OwnerAndObservers);

            hostComponent.guild = guild;
            hostComponent.MySyncList.Add(listValue);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(hostComponent.MySyncList[0], Is.EqualTo(listValue));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.MySyncList.Count, Is.EqualTo(1));
            Assert.That(ObserverComponent.MySyncList[0], Is.EqualTo(listValue));
        }

        [UnityTest]
        public IEnumerator CanAddInStartServer()
        {
            var clone = InstantiateForTest(_characterPrefab);
            var component = clone.GetComponent<MockPlayerWithList>();
            clone.OnStartServer.AddListener(() =>
            {
                component.MySyncList.Add(listValue);
            });

            serverObjectManager.Spawn(clone);

            // wait for sync
            yield return null;
            yield return null;

            var clientObj = _remoteClients[0].Get(component);

            Assert.That(clientObj.MySyncList.Count, Is.EqualTo(1));
            Assert.That(clientObj.MySyncList[0], Is.EqualTo(listValue));
        }
    }
}
