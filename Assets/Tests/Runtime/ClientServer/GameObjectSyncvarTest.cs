using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.ClientServer
{
    public class SampleBehaviorWithGO : NetworkBehaviour
    {
        [SyncVar]
        public GameObject target;
    }

    public class GameObjectSyncvarTest : ClientServerSetup<SampleBehaviorWithGO>
    {
        [Test]
        public void IsNullByDefault()
        {
            // out of the box, target should be null in the client

            Assert.That(clientComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ChangeTarget() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.target = serverPlayerGO;

            await UniTask.WaitUntil(() => clientComponent.target != null);

            Assert.That(clientComponent.target, Is.SameAs(clientPlayerGO));
        });

        [Test]
        public void UpdateAfterSpawn()
        {
            // this situation can happen when the client does nto see an object
            // but the object is assigned in a syncvar.
            // this can easily happen during spawn if spawning in an unexpected order
            // or if there is AOI in play.
            // in this case we would have a valid net id, but we would not
            // find the object at spawn time

            var goSyncvar = new GameObjectSyncvar
            {
                objectLocator = clientObjectManager,
                netId = serverIdentity.NetId,
                gameObject = null,
            };

            Assert.That(goSyncvar.Value, Is.SameAs(clientPlayerGO));
        }

        [UnityTest]
        public IEnumerator SpawnWithTarget() => UniTask.ToCoroutine(async () =>
        {
            // create an object, set the target and spawn it
            GameObject newObject = Object.Instantiate(playerPrefab);
            SampleBehaviorWithGO newBehavior = newObject.GetComponent<SampleBehaviorWithGO>();
            newBehavior.target = serverPlayerGO;
            serverObjectManager.Spawn(newObject);

            // wait until the client spawns it
            uint newObjectId = newBehavior.NetId;
            await UniTask.WaitUntil(() => clientObjectManager.SpawnedObjects.ContainsKey(newObjectId));

            // check if the target was set correctly in the client
            NetworkIdentity newClientObject = clientObjectManager.SpawnedObjects[newObjectId];
            SampleBehaviorWithGO newClientBehavior = newClientObject.GetComponent<SampleBehaviorWithGO>();
            Assert.That(newClientBehavior.target, Is.SameAs(clientPlayerGO));

            // cleanup
            serverObjectManager.Destroy(newObject);
        });
    }
}
