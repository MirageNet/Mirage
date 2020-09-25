using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

using static Mirror.Tests.AsyncUtil;

namespace Mirror.Tests
{
    [TestFixture]
    public class LobbyReadyTest : HostSetup<MockComponent>
    {
        LobbyReady lobby;
        ObjectReady playerReady;

        public override void ExtraSetup()
        {
            lobby = networkManagerGo.AddComponent<LobbyReady>();
        }

        public override void ExtraTearDown()
        {
            lobby = null;
        }

        [Test]
        public void SetAllClientsNotReadyTest()
        {
            playerReady = identity.gameObject.AddComponent<ObjectReady>();
            lobby.ObjectReadyList.Add(playerReady);
            playerReady.IsReady = true;

            lobby.SetAllClientsNotReady();

            Assert.That(playerReady.IsReady, Is.False);
        }

        //[UnityTest]
        //public IEnumerator SendToReadyTest() => RunAsync(async () =>
        //{
        //    playerReady = identity.gameObject.AddComponent<ObjectReady>();
        //    lobby.ObjectReadyList.Add(playerReady);
        //    playerReady.IsReady = true;

        //    bool invokeWovenTestMessage = false;
        //    client.Connection.RegisterHandler<SceneMessage>(msg => invokeWovenTestMessage = true);
        //    lobby.SendToReady(identity, new WovenTestMessage(), true, Channels.DefaultReliable);

        //    await WaitFor(() => invokeWovenTestMessage == true);
        //});

        [Test]
        public void IsReadyStateTest()
        {
            playerReady = identity.gameObject.AddComponent<ObjectReady>();

            Assert.That(playerReady.IsReady, Is.False);
        }

        [Test]
        public void SetClientReadyTest()
        {
            playerReady = identity.gameObject.AddComponent<ObjectReady>();

            playerReady.SetClientReady();

            Assert.That(playerReady.IsReady, Is.True);
        }

        [Test]
        public void SetClientNotReadyTest()
        {
            playerReady = identity.gameObject.AddComponent<ObjectReady>();

            playerReady.SetClientNotReady();

            Assert.That(playerReady.IsReady, Is.False);
        }

        //[Test]
        //public void ClientReadyTest()
        //{
        //    playerReady = identity.gameObject.AddComponent<ObjectReady>();

        //    playerReady.Ready();

        //    Assert.That(playerReady.IsReady, Is.False);
        //}
    }
}
