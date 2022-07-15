using System;
using System.Collections.Generic;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime
{
    public class NetworkMatchCheckerTest
    {
        private GameObject serverGO;
        private NetworkServer server;
        private ServerObjectManager serverObjectManager;
        private GameObject character1;
        private GameObject character2;
        private GameObject character3;
        private NetworkMatchChecker player1MatchChecker;
        private NetworkMatchChecker player2MatchChecker;
        private NetworkPlayer player1Connection;
        private NetworkPlayer player2Connection;
        private NetworkPlayer player3Connection;
        private Dictionary<Guid, HashSet<NetworkIdentity>> matchPlayers;

        [SetUp]
        public void Setup()
        {
            // todo use Substitute for interfaces instead of gameobjeccts for this test

            serverGO = new GameObject("Network Server", typeof(TestSocketFactory), typeof(NetworkServer), typeof(ServerObjectManager));

            server = serverGO.GetComponent<NetworkServer>();
            serverObjectManager = serverGO.GetComponent<ServerObjectManager>();
            serverObjectManager.Server = server;

            character1 = new GameObject("TestCharacter1", typeof(NetworkIdentity), typeof(NetworkMatchChecker));
            character2 = new GameObject("TestCharacter2", typeof(NetworkIdentity), typeof(NetworkMatchChecker));
            character3 = new GameObject("TestCharacter3", typeof(NetworkIdentity));


            character1.GetComponent<NetworkIdentity>().Server = server;
            character1.GetComponent<NetworkIdentity>().ServerObjectManager = serverObjectManager;
            character2.GetComponent<NetworkIdentity>().Server = server;
            character2.GetComponent<NetworkIdentity>().ServerObjectManager = serverObjectManager;
            character3.GetComponent<NetworkIdentity>().Server = server;
            character3.GetComponent<NetworkIdentity>().ServerObjectManager = serverObjectManager;

            player1MatchChecker = character1.GetComponent<NetworkMatchChecker>();
            player2MatchChecker = character2.GetComponent<NetworkMatchChecker>();


            player1Connection = CreatePlayer(character1);
            player2Connection = CreatePlayer(character2);
            player3Connection = CreatePlayer(character3);
            Dictionary<Guid, HashSet<NetworkIdentity>> g = GetMatchPlayersDictionary();
            matchPlayers = g;
        }

        private static Dictionary<Guid, HashSet<NetworkIdentity>> GetMatchPlayersDictionary()
        {
            Type type = typeof(NetworkMatchChecker);
            FieldInfo fieldInfo = type.GetField("matchPlayers", BindingFlags.Static | BindingFlags.NonPublic);
            return (Dictionary<Guid, HashSet<NetworkIdentity>>)fieldInfo.GetValue(null);
        }

        private static NetworkPlayer CreatePlayer(GameObject character)
        {
            var player = new NetworkPlayer(Substitute.For<SocketLayer.IConnection>())
            {
                Identity = character.GetComponent<NetworkIdentity>()
            };
            player.Identity.Owner = player;
            player.SceneIsReady = true;
            return player;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(character1);
            Object.DestroyImmediate(character2);
            Object.DestroyImmediate(character3);

            Object.DestroyImmediate(serverGO);
            matchPlayers.Clear();
            matchPlayers = null;
        }

        private static void SetMatchId(NetworkMatchChecker target, Guid guid)
        {
            // set using reflection so bypass property
            FieldInfo field = typeof(NetworkMatchChecker).GetField("currentMatch", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, guid);
        }

        [Test]
        public void OnCheckObserverShouldBeTrueForSameMatchId()
        {
            string guid = Guid.NewGuid().ToString();

            SetMatchId(player1MatchChecker, new Guid(guid));
            SetMatchId(player2MatchChecker, new Guid(guid));

            bool player1Visable = player1MatchChecker.OnCheckObserver(player1Connection);
            Assert.IsTrue(player1Visable);

            bool player2Visable = player1MatchChecker.OnCheckObserver(player2Connection);
            Assert.IsTrue(player2Visable);
        }

        [Test]
        public void OnCheckObserverShouldBeFalseForDifferentMatchId()
        {
            string guid1 = Guid.NewGuid().ToString();
            string guid2 = Guid.NewGuid().ToString();

            SetMatchId(player1MatchChecker, new Guid(guid1));
            SetMatchId(player2MatchChecker, new Guid(guid2));

            bool player1VisableToPlayer1 = player1MatchChecker.OnCheckObserver(player1Connection);
            Assert.IsTrue(player1VisableToPlayer1);

            bool player2VisableToPlayer1 = player1MatchChecker.OnCheckObserver(player2Connection);
            Assert.IsFalse(player2VisableToPlayer1);


            bool player1VisableToPlayer2 = player2MatchChecker.OnCheckObserver(player1Connection);
            Assert.IsFalse(player1VisableToPlayer2);

            bool player2VisableToPlayer2 = player2MatchChecker.OnCheckObserver(player2Connection);
            Assert.IsTrue(player2VisableToPlayer2);
        }

        [Test]
        public void OnCheckObserverShouldBeFalseIfObjectDoesNotHaveNetworkMatchChecker()
        {
            string guid = Guid.NewGuid().ToString();

            SetMatchId(player1MatchChecker, new Guid(guid));

            bool player3Visable = player1MatchChecker.OnCheckObserver(player3Connection);
            Assert.IsFalse(player3Visable);
        }

        [Test]
        public void OnCheckObserverShouldBeFalseForEmptyGuid()
        {
            string guid = Guid.Empty.ToString();

            SetMatchId(player1MatchChecker, new Guid(guid));
            SetMatchId(player2MatchChecker, new Guid(guid));

            bool player1Visable = player1MatchChecker.OnCheckObserver(player1Connection);
            Assert.IsFalse(player1Visable);

            bool player2Visable = player1MatchChecker.OnCheckObserver(player2Connection);
            Assert.IsFalse(player2Visable);
        }

        [Test]
        public void SettingMatchIdShouldRebuildObservers()
        {
            string guidMatch1 = Guid.NewGuid().ToString();

            // make players join same match
            player1MatchChecker.MatchId = new Guid(guidMatch1);
            player2MatchChecker.MatchId = new Guid(guidMatch1);

            // check player1's observers contains player 2
            Assert.That(player1MatchChecker.Identity.observers, Contains.Item(player2MatchChecker.Owner));
            // check player2's observers contains player 1
            Assert.That(player2MatchChecker.Identity.observers, Contains.Item(player1MatchChecker.Owner));
        }

        [Test]
        public void ChangingMatchIdShouldRebuildObservers()
        {
            string guidMatch1 = Guid.NewGuid().ToString();
            string guidMatch2 = Guid.NewGuid().ToString();

            // make players join same match
            player1MatchChecker.MatchId = new Guid(guidMatch1);
            player2MatchChecker.MatchId = new Guid(guidMatch1);

            // make player2 join different match
            player2MatchChecker.MatchId = new Guid(guidMatch2);

            // check player1's observers does NOT contain player 2
            Assert.That(player1MatchChecker.Identity.observers, !Contains.Item(player2MatchChecker.Owner));
            // check player2's observers does NOT contain player 1
            Assert.That(player2MatchChecker.Identity.observers, !Contains.Item(player1MatchChecker.Owner));
        }

        [Test]
        public void ClearingMatchIdShouldRebuildObservers()
        {
            string guidMatch1 = Guid.NewGuid().ToString();

            // make players join same match
            player1MatchChecker.MatchId = new Guid(guidMatch1);
            player2MatchChecker.MatchId = new Guid(guidMatch1);

            // make player 2 leave match
            player2MatchChecker.MatchId = Guid.Empty;

            // check player1's observers does NOT contain player 2
            Assert.That(player1MatchChecker.Identity.observers, !Contains.Item(player2MatchChecker.Owner));
            // check player2's observers does NOT contain player 1
            Assert.That(player2MatchChecker.Identity.observers, !Contains.Item(player1MatchChecker.Owner));
        }
    }
}
