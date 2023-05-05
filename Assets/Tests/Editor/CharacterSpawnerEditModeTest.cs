using System;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests
{
    public class CharacterSpawnerEditModeTest : TestBase
    {
        private GameObject go;
        private NetworkClient client;
        private NetworkServer server;
        private CharacterSpawner spawner;
        private NetworkSceneManager sceneManager;
        private ServerObjectManager serverObjectManager;
        private ClientObjectManager clientObjectManager;
        private GameObject playerPrefab;

        private Transform pos1;
        private Transform pos2;

        [SetUp]
        public void Setup()
        {
            go = CreateGameObject();
            client = go.AddComponent<NetworkClient>();
            server = go.AddComponent<NetworkServer>();
            spawner = go.AddComponent<CharacterSpawner>();
            sceneManager = go.AddComponent<NetworkSceneManager>();
            serverObjectManager = go.AddComponent<ServerObjectManager>();
            clientObjectManager = go.AddComponent<ClientObjectManager>();
            spawner.SceneManager = sceneManager;
            sceneManager.Client = client;
            sceneManager.Server = server;
            server.ObjectManager = serverObjectManager;
            client.ObjectManager = clientObjectManager;
            sceneManager.ClientObjectManager = clientObjectManager;
            spawner.Client = client;
            spawner.Server = server;
            spawner.ServerObjectManager = serverObjectManager;
            spawner.ClientObjectManager = clientObjectManager;

            var identity = CreateNetworkIdentity();
            playerPrefab = identity.gameObject;

            spawner.PlayerPrefab = identity;

            pos1 = CreateGameObject().transform;
            pos2 = CreateGameObject().transform;
            spawner.startPositions.Add(pos1);
            spawner.startPositions.Add(pos2);
        }

        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }

        [Test]
        public void StartExceptionTest()
        {
            spawner.PlayerPrefab = null;
            Assert.Throws<InvalidOperationException>(() =>
            {
                spawner.Awake();
            });
        }

        [Test]
        public void StartExceptionMissingServerObjectManagerTest()
        {
            spawner.ServerObjectManager = null;
            Assert.Throws<InvalidOperationException>(() =>
            {
                spawner.Awake();
            });
        }

        [Test]
        public void AutoConfigureClient()
        {
            spawner.Awake();
            Assert.That(spawner.Client, Is.SameAs(client));
        }

        [Test]
        public void AutoConfigureServer()
        {
            spawner.Awake();
            Assert.That(spawner.Server, Is.SameAs(server));
        }

        [Test]
        public void GetStartPositionRoundRobinTest()
        {
            spawner.Awake();

            spawner.playerSpawnMethod = CharacterSpawner.PlayerSpawnMethod.RoundRobin;
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos1.transform));
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos2.transform));
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos1.transform));
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos2.transform));
        }

        [Test]
        public void GetStartPositionRandomTest()
        {
            spawner.Awake();

            spawner.playerSpawnMethod = CharacterSpawner.PlayerSpawnMethod.Random;
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos1.transform) | Is.SameAs(pos2.transform));
        }

        [Test]
        public void GetStartPositionNullTest()
        {
            spawner.Awake();

            spawner.startPositions.Clear();
            Assert.That(spawner.GetStartPosition(), Is.SameAs(null));
        }

        [Test]
        public void MissingClientObjectSpawnerExceptionTest()
        {
            spawner.ClientObjectManager = null;

            Assert.Throws<InvalidOperationException>(() =>
            {
                spawner.OnClientConnected(null);
            });
        }
    }
}
