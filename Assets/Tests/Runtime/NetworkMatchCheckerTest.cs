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

        static Dictionary<Guid, HashSet<NetworkIdentity>> GetMatchPlayersDictionary()
        {
            Type type = typeof(NetworkMatchChecker);
            FieldInfo fieldInfo = type.GetField("matchPlayers", BindingFlags.Static | BindingFlags.NonPublic);
            return (Dictionary<Guid, HashSet<NetworkIdentity>>)fieldInfo.GetValue(null);
        }

        static NetworkPlayer CreatePlayer(GameObject character)
        {
            var player = new NetworkPlayer(Substitute.For<SocketLayer.IConnection>())
            {
                Identity = character.GetComponent<NetworkIdentity>()
            };
            player.Identity.ConnectionToClient = player;
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

        static void SetMatchId(NetworkMatchChecker target, Guid guid)
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

            player1MatchChecker.CheckForObservers(player1Connection.Identity, Vector3.zero, out var players1);
            Assert.IsTrue(players1 != null);

            player1MatchChecker.CheckForObservers(player2Connection.Identity, Vector3.zero, out var players2);
            Assert.IsTrue(players2 != null);
        }

        [Test]
        public void OnCheckObserverShouldBeFalseForDifferentMatchId()
        {
            string guid1 = Guid.NewGuid().ToString();
            string guid2 = Guid.NewGuid().ToString();

            SetMatchId(player1MatchChecker, new Guid(guid1));
            SetMatchId(player2MatchChecker, new Guid(guid2));

            player1MatchChecker.CheckForObservers(player1Connection.Identity, Vector3.zero,  out var players1);
            Assert.IsTrue(players1 != null);

            player1MatchChecker.CheckForObservers(player2Connection.Identity, Vector3.zero, out var players2);
            Assert.IsFalse(players2 == null);


            player2MatchChecker.CheckForObservers(player1Connection.Identity, Vector3.zero, out var players3);
            Assert.IsFalse(players3 == null);

            player2MatchChecker.CheckForObservers(player2Connection.Identity, Vector3.zero, out var players4);
            Assert.IsTrue(players4 != null);
        }

        [Test]
        public void OnCheckObserverShouldBeFalseIfObjectDoesNotHaveNetworkMatchChecker()
        {
            string guid = Guid.NewGuid().ToString();

            SetMatchId(player1MatchChecker, new Guid(guid));

            player1MatchChecker.CheckForObservers(player3Connection.Identity, Vector3.zero, out var players);
            Assert.IsFalse(players == null);
        }

        [Test]
        public void OnCheckObserverShouldBeFalseForEmptyGuid()
        {
            string guid = Guid.Empty.ToString();

            SetMatchId(player1MatchChecker, new Guid(guid));
            SetMatchId(player2MatchChecker, new Guid(guid));

            player1MatchChecker.CheckForObservers(player1Connection.Identity, Vector3.zero, out var players1);
            Assert.IsFalse(players1 == null);

            player1MatchChecker.CheckForObservers(player2Connection.Identity, Vector3.zero, out var players2);
            Assert.IsFalse(players2 == null);
        }
    }
}
