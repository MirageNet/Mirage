using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests
{
    [TestFixture]
    public class NetworkManagerTest
    {
        GameObject gameObject;
        NetworkManager manager;


        [SetUp]
        public void SetupNetworkManager()
        {
            gameObject = new GameObject();
            manager = gameObject.AddComponent<NetworkManager>();
            manager.Client = gameObject.GetComponent<NetworkClient>();
            manager.Server = gameObject.GetComponent<NetworkServer>();
        }

        [TearDown]
        public void TearDownNetworkManager()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void VariableTest()
        {
            Assert.That(manager.DontDestroyOnLoad, Is.True);
            Assert.That(manager.RunInBackground, Is.True);
            Assert.That(manager.StartOnHeadless, Is.True);
            Assert.That(manager.ShowDebugMessages, Is.False);
            Assert.That(manager.ServerTickRate, Is.EqualTo(30));
            Assert.That(manager.OfflineScene, Is.Empty);
            Assert.That(manager.MaxConnections, Is.EqualTo(4));
            Assert.That(manager.AutoCreatePlayer, Is.True);
            Assert.That(manager.SpawnPrefabs, Is.Empty);
            Assert.That(manager.NumberOfActivePlayers, Is.Zero);
            Assert.That(manager.IsNetworkActive, Is.False);

            Assert.That(manager.NetworkSceneName, Is.Empty);
            Assert.That(manager.StartPositionIndex, Is.Zero);
            Assert.That(manager.StartPositions, Is.Empty);
        }

        [Test]
        public void StartServerTest()
        {
            Assert.That(manager.Server.active , Is.False);

            manager.StartServer();

            Assert.That(manager.IsNetworkActive , Is.True);
            Assert.That(manager.Mode, Is.EqualTo(NetworkManagerMode.ServerOnly));
            Assert.That(manager.Server.active , Is.True);

            manager.StopServer();
        }

        [Test]
        public void StopServerTest()
        {
            manager.StartServer();
            manager.StopServer();

            Assert.That(manager.IsNetworkActive , Is.False);
            Assert.That(manager.Mode, Is.EqualTo(NetworkManagerMode.Offline));
        }

        [Test]
        public void StartClientTest()
        {
            manager.StartClient("localhost");

            Assert.That(manager.IsNetworkActive , Is.True);
            Assert.That(manager.Mode, Is.EqualTo(NetworkManagerMode.ClientOnly));

            manager.StopClient();
        }

        [Test]
        public void StopClientTest()
        {
            manager.StartClient("localhost");
            manager.StopClient();

            Assert.That(manager.IsNetworkActive , Is.False);
            Assert.That(manager.Mode, Is.EqualTo(NetworkManagerMode.Offline));
        }

        [Test]
        public void ShutdownTest()
        {
            manager.StartClient("localhost");
            manager.StopClient();

            Assert.That(manager.StartPositions , Is.Empty);
            Assert.That(manager.StartPositionIndex , Is.Zero);
        }

        [Test]
        public void RegisterStartPositionTest()
        {
            Assert.That(manager.StartPositions , Is.Empty);

            manager.RegisterStartPosition(gameObject.transform);
            Assert.That(manager.StartPositions.Count , Is.EqualTo(1));
            Assert.That(manager.StartPositions, Has.Member(gameObject.transform));

            manager.UnRegisterStartPosition(gameObject.transform);
        }

        [Test]
        public void UnRegisterStartPositionTest()
        {
            Assert.That(manager.StartPositions , Is.Empty);
            
            manager.RegisterStartPosition(gameObject.transform);
            Assert.That(manager.StartPositions.Count , Is.EqualTo(1));
            Assert.That(manager.StartPositions, Has.Member(gameObject.transform));

            manager.UnRegisterStartPosition(gameObject.transform);
            Assert.That(manager.StartPositions , Is.Empty);
        }

        [Test]
        public void GetStartPositionTest()
        {
            Assert.That(manager.StartPositions , Is.Empty);
            
            manager.RegisterStartPosition(gameObject.transform);
            Assert.That(manager.StartPositions.Count , Is.EqualTo(1));
            Assert.That(manager.StartPositions, Has.Member(gameObject.transform));

            Assert.That(manager.GetStartPosition(), Is.SameAs(gameObject.transform));

            manager.UnRegisterStartPosition(gameObject.transform);
        }
    }
}
