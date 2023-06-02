using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    [Category("LoadsScene")]
    public class CharacterSpawnerTest : HostSetupWithSceneManager<MockComponent>
    {
        private CharacterSpawner spawner;

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            // disable so awake isn't called till setup finished
            serverGo.SetActive(false);
            spawner = serverGo.AddComponent<CharacterSpawner>();

            spawner.Client = client;
            spawner.Server = server;
            spawner.SceneManager = sceneManager;
            spawner.ClientObjectManager = clientObjectManager;
            spawner.ServerObjectManager = serverObjectManager;

            spawner.PlayerPrefab = CreateNetworkIdentity();

            spawner.AutoSpawn = false;
            serverGo.SetActive(true);
        }

        [UnityTest]
        public IEnumerator DontAutoSpawnTest() => UniTask.ToCoroutine(async () =>
        {
            Assert.Fail();
            var invokeAddPlayerMessage = false;
            //ServerMessageHandler.RegisterHandler<AddCharacterMessage>(msg => invokeAddPlayerMessage = true);

            sceneManager.ServerLoadSceneNormal(TestScenes.Path);
            // wait for messages to be processed
            await UniTask.Yield();

            Assert.That(invokeAddPlayerMessage, Is.False);

        });

        [UnityTest]
        public IEnumerator ManualSpawnTest() => UniTask.ToCoroutine(async () =>
        {
            Assert.Fail();
            //var invokeAddPlayerMessage = false;
            //ServerMessageHandler.RegisterHandler<AddCharacterMessage>(msg => invokeAddPlayerMessage = true);

            //spawner.RequestServerSpawnPlayer();

            // wait for messages to be processed
            await UniTask.Yield();

            //Assert.That(invokeAddPlayerMessage, Is.True);
        });
    }
}
