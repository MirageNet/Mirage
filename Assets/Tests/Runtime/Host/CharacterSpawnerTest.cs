using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    public class CharacterSpawnerTest : HostSetup<MockComponent>
    {
        private AssetBundle bundle;
        private GameObject player;
        private CharacterSpawner spawner;

        public override void ExtraSetup()
        {
            bundle = AssetBundle.LoadFromFile("Assets/Tests/Runtime/TestScene/testscene");

            spawner = networkManagerGo.AddComponent<CharacterSpawner>();

            spawner.Client = client;
            spawner.Server = server;
            spawner.SceneManager = sceneManager;
            spawner.ClientObjectManager = clientObjectManager;
            spawner.ServerObjectManager = serverObjectManager;

            player = new GameObject();
            var identity = player.AddComponent<NetworkIdentity>();
            spawner.PlayerPrefab = identity;

            spawner.AutoSpawn = false;
        }

        public override void ExtraTearDown()
        {
            bundle.Unload(true);
            Object.Destroy(player);
        }

        [UnityTest]
        public IEnumerator DontAutoSpawnTest() => UniTask.ToCoroutine(async () =>
        {
            var invokeAddPlayerMessage = false;
            ServerMessageHandler.RegisterHandler<AddCharacterMessage>(msg => invokeAddPlayerMessage = true);

            sceneManager.ServerLoadSceneNormal("Assets/Mirror/Tests/Runtime/testScene.unity");
            // wait for messages to be processed
            await UniTask.Yield();

            Assert.That(invokeAddPlayerMessage, Is.False);

        });

        [UnityTest]
        public IEnumerator ManualSpawnTest() => UniTask.ToCoroutine(async () =>
        {
            var invokeAddPlayerMessage = false;
            ServerMessageHandler.RegisterHandler<AddCharacterMessage>(msg => invokeAddPlayerMessage = true);

            spawner.RequestServerSpawnPlayer();

            // wait for messages to be processed
            await UniTask.Yield();

            Assert.That(invokeAddPlayerMessage, Is.True);
        });
    }
}
