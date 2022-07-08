using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    [Category("LoadsScene")]
    public class CharacterSpawnerTest : HostSetup<MockComponent>
    {
        CharacterSpawner spawner;

        public override void ExtraSetup()
        {
            // disable so awake isn't called till setup finished
            networkManagerGo.SetActive(false);
            spawner = networkManagerGo.AddComponent<CharacterSpawner>();

            spawner.Client = client;
            spawner.Server = server;
            spawner.SceneManager = sceneManager;
            spawner.ClientObjectManager = clientObjectManager;
            spawner.ServerObjectManager = serverObjectManager;

            spawner.PlayerPrefab = CreateNetworkIdentity();

            spawner.AutoSpawn = false;
            networkManagerGo.SetActive(true);
        }

        [UnityTest]
        public IEnumerator DontAutoSpawnTest() => UniTask.ToCoroutine(async () =>
        {
            bool invokeAddPlayerMessage = false;
            ServerMessageHandler.RegisterHandler<AddCharacterMessage>(msg => invokeAddPlayerMessage = true);

            sceneManager.ServerLoadSceneNormal(TestScene.Path);
            // wait for messages to be processed
            await UniTask.Yield();

            Assert.That(invokeAddPlayerMessage, Is.False);

        });

        [UnityTest]
        public IEnumerator ManualSpawnTest() => UniTask.ToCoroutine(async () =>
        {
            bool invokeAddPlayerMessage = false;
            ServerMessageHandler.RegisterHandler<AddCharacterMessage>(msg => invokeAddPlayerMessage = true);

            spawner.RequestServerSpawnPlayer();

            // wait for messages to be processed
            await UniTask.Yield();

            Assert.That(invokeAddPlayerMessage, Is.True);
        });
    }
}
