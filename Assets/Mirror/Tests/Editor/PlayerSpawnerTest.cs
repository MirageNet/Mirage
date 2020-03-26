using Mirror;
using NUnit.Framework;
using UnityEngine;


namespace Mirror.Tests
{

    public class PlayerSpawnerTest
    {
        private GameObject go;
        private NetworkClient client;
        private NetworkServer server;
        private PlayerSpawner spawner;
        private GameObject playerPrefab;

        private NetworkStartPosition pos1;
        private NetworkStartPosition pos2;

        [SetUp]
        public void Setup()
        {
            go = new GameObject();
            client = go.AddComponent<NetworkClient>();
            server = go.AddComponent<NetworkServer>();
            spawner = go.AddComponent<PlayerSpawner>();

            playerPrefab = new GameObject();
            NetworkIdentity playerId = playerPrefab.AddComponent<NetworkIdentity>();

            spawner.playerPrefab = playerId;

            pos1 = new GameObject().AddComponent<NetworkStartPosition>();
            pos2 = new GameObject().AddComponent<NetworkStartPosition>();

        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(playerPrefab);

            Object.DestroyImmediate(pos1.gameObject);
            Object.DestroyImmediate(pos2.gameObject);
        }

        [Test]
        public void AutoConfigureClient()
        {
            spawner.Start();
            Assert.That(spawner.client, Is.SameAs(client));
        }

        [Test]
        public void AutoConfigureServer()
        {
            spawner.Start();
            Assert.That(spawner.server, Is.SameAs(server));
        }

        [Test]
        public void AutoConfigureStartPoints()
        {
            spawner.Start();
            Assert.That(spawner.startPositions, Is.EquivalentTo(new[] { pos1.transform, pos2.transform }));
        }



        [Test]
        public void RegisterStartPositionTest()
        {
            spawner.Start();

            NetworkStartPosition pos3 = new GameObject().AddComponent<NetworkStartPosition>();

            spawner.RegisterStartPosition(pos3.transform);

            Assert.That(spawner.startPositions, Is.EquivalentTo(new[] { pos1.transform, pos2.transform, pos3.transform }));

            Object.DestroyImmediate(pos3.gameObject);
        }

        [Test]
        public void UnRegisterStartPositionTest()
        {
            spawner.Start();

            spawner.UnRegisterStartPosition(pos2.transform);

            Assert.That(spawner.startPositions, Is.EquivalentTo(new[] { pos1.transform }));

        }

        [Test]
        public void GetStartPositionRoundRobinTest()
        {
            spawner.Start();

            spawner.playerSpawnMethod = PlayerSpawner.PlayerSpawnMethod.RoundRobin;
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos1.transform));
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos2.transform));
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos1.transform));
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos2.transform));
        }

        [Test]
        public void GetStartPositionRandomTest()
        {
            spawner.Start();

            spawner.playerSpawnMethod = PlayerSpawner.PlayerSpawnMethod.Random;
            Assert.That(spawner.GetStartPosition(), Is.SameAs(pos1.transform) | Is.SameAs(pos2.transform));
        }
    }

}