using System.Collections;
using Mirage.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Components
{
    public class LobbyReadyTest : MultiRemoteClientSetup<ReadyCheck>
    {
        private LobbyReady _lobby;

        protected override int RemoteClientCount => 4;

        protected override void ExtraPrefabSetup(NetworkIdentity prefab)
        {
            base.ExtraPrefabSetup(prefab);

            // get dont add, base setup will add it
            var readyCheck = prefab.gameObject.GetComponent<ReadyCheck>();

            // sync to/from everyone
            readyCheck.SyncSettings.From = SyncFrom.Owner | SyncFrom.Server;
            readyCheck.SyncSettings.To = SyncTo.Server | SyncTo.Owner | SyncTo.ObserversOnly;
            readyCheck.SyncSettings.Timing = SyncTiming.NoInterval;
        }

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();
            _lobby = serverGo.AddComponent<LobbyReady>();
            _lobby.Server = server;
        }

        [Test]
        public void LobbyAutoFindsReadyCheck()
        {
            Assert.That(_lobby.Players.Count, Is.EqualTo(2));

            Assert.That(_lobby.Players.ContainsKey(ServerIdentity(0)));
            Assert.That(_lobby.Players.ContainsKey(ServerIdentity(1)));

            Assert.That(_lobby.Players[ServerIdentity(0)], Is.EqualTo(ServerComponent(0)));
            Assert.That(_lobby.Players[ServerIdentity(1)], Is.EqualTo(ServerComponent(1)));
        }

        [UnityTest]
        public IEnumerator SendsReadyToServer()
        {
            ClientComponent(0).SetReady(true);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(ServerComponent(0).IsReady, Is.True);
        }


        [UnityTest]
        public IEnumerator SetAllClientsNotReadyTest()
        {
            // start ready
            ClientComponent(0).SetReady(true);
            ClientComponent(1).SetReady(true);

            // wait for sync
            yield return null;
            yield return null;


            _lobby.SetAllClientsNotReady();

            // wait for sync
            yield return null;
            yield return null;

            // check all instances are not ready
            foreach (var serverComp in _lobby.Players.Values)
            {
                RunOnAll(serverComp, (readyCheck) =>
                {
                    Assert.That(readyCheck.IsReady, Is.False);
                });
            }
        }


        [UnityTest]
        public IEnumerator SendsToAll()
        {
            ClientComponent(0).SetReady(true);

            // wait for sync
            yield return null;
            yield return null;
            yield return null;

            RunOnAll(ClientComponent(0), readyCheck =>
            {
                Assert.That(readyCheck.IsReady, Is.True, $"was not ready on {((Object)readyCheck.Client ?? readyCheck.Server)?.name}");
            });
        }

        [UnityTest]
        public IEnumerator SendToReadyTest()
        {
            ClientComponent(0).SetReady(true);
            ClientComponent(1).SetReady(false);
            ClientComponent(2).SetReady(true);
            ClientComponent(3).SetReady(true);

            yield return null;
            yield return null;

            var receivedMessage = new int[RemoteClientCount];
            RunOnAllClients((instance, index) =>
            {
                instance.Client.MessageHandler.RegisterHandler<SceneMessage>(msg => receivedMessage[index]++);
            });

            _lobby.SendToReady<SceneMessage>(new SceneMessage());

            yield return null;
            yield return null;

            Assert.That(receivedMessage[0], Is.EqualTo(1));
            Assert.That(receivedMessage[1], Is.EqualTo(0));
            Assert.That(receivedMessage[2], Is.EqualTo(1));
            Assert.That(receivedMessage[3], Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SendToNotReadyTest()
        {
            ClientComponent(0).SetReady(true);
            ClientComponent(1).SetReady(false);
            ClientComponent(2).SetReady(false);
            ClientComponent(3).SetReady(true);
            yield return null;
            yield return null;

            var receivedMessage = new int[RemoteClientCount];
            RunOnAllClients((instance, index) =>
            {
                instance.Client.MessageHandler.RegisterHandler<SceneMessage>(msg => receivedMessage[index]++);
            });

            _lobby.SendToReady<SceneMessage>(new SceneMessage(), sendToReady: false);

            yield return null;
            yield return null;

            Assert.That(receivedMessage[0], Is.EqualTo(0));
            Assert.That(receivedMessage[1], Is.EqualTo(1));
            Assert.That(receivedMessage[2], Is.EqualTo(1));
            Assert.That(receivedMessage[3], Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator SendExcludeOwner()
        {
            ClientComponent(0).SetReady(true);
            ClientComponent(1).SetReady(false);
            ClientComponent(2).SetReady(true);
            ClientComponent(3).SetReady(true);
            yield return null;
            yield return null;

            var receivedMessage = new int[RemoteClientCount];
            RunOnAllClients((instance, index) =>
            {
                instance.Client.MessageHandler.RegisterHandler<SceneMessage>(msg => receivedMessage[index]++);
            });

            _lobby.SendToReady<SceneMessage>(new SceneMessage(), exclude: ServerIdentity(0));

            yield return null;
            yield return null;

            Assert.That(receivedMessage[0], Is.EqualTo(0), "should have been exlcuded");
            Assert.That(receivedMessage[1], Is.EqualTo(0));
            Assert.That(receivedMessage[2], Is.EqualTo(1));
            Assert.That(receivedMessage[3], Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator EventUInvokesOnOwner()
        {
            var ownerComp = ClientComponent(0);

            var invoked = 0;
            ownerComp.OnReadyChanged += _ => invoked++;

            ownerComp.SetReady(true);

            Assert.That(invoked, Is.EqualTo(1), "Should be invoked locally when setting");

            yield return null;
            yield return null;

            // change on server
            _serverInstance.Get(ownerComp).SetReady(false);

            yield return null;
            yield return null;

            Assert.That(invoked, Is.EqualTo(2), "should be invoked when receving from server");
        }
    }
}
