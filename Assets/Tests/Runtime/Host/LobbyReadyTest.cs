using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    [TestFixture]
    public class LobbyReadyTest : HostSetup<MockComponent>
    {
        GameObject readyPlayer;
        LobbyReady lobby;
        ObjectReady readyComp;

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
            readyComp = playerIdentity.gameObject.AddComponent<ObjectReady>();
            lobby.ObjectReadyList.Add(readyComp);
            readyComp.IsReady = true;

            lobby.SetAllClientsNotReady();

            Assert.That(readyComp.IsReady, Is.False);
        }

        [UnityTest]
        public IEnumerator SendToReadyTest() => UniTask.ToCoroutine(async () =>
        {
            readyComp = playerIdentity.gameObject.AddComponent<ObjectReady>();
            lobby.ObjectReadyList.Add(readyComp);
            readyComp.IsReady = true;

            bool invokeWovenTestMessage = false;
            ClientMessageHandler.RegisterHandler<SceneMessage>(msg => invokeWovenTestMessage = true);
            lobby.SendToReady(playerIdentity, new SceneMessage(), true, Channel.Reliable);

            await AsyncUtil.WaitUntilWithTimeout(() => invokeWovenTestMessage);
        });

        [Test]
        public void IsReadyStateTest()
        {
            readyComp = playerIdentity.gameObject.AddComponent<ObjectReady>();

            Assert.That(readyComp.IsReady, Is.False);
        }

        [Test]
        public void SetClientReadyTest()
        {
            readyComp = playerIdentity.gameObject.AddComponent<ObjectReady>();

            readyComp.SetClientReady();

            Assert.That(readyComp.IsReady, Is.True);
        }

        [Test]
        public void SetClientNotReadyTest()
        {
            readyComp = playerIdentity.gameObject.AddComponent<ObjectReady>();

            readyComp.SetClientNotReady();

            Assert.That(readyComp.IsReady, Is.False);
        }

        [UnityTest]
        public IEnumerator ClientReadyTest() => UniTask.ToCoroutine(async () =>
        {
            NetworkIdentity readyPlayer = CreateNetworkIdentity();
            readyComp = readyPlayer.gameObject.AddComponent<ObjectReady>();

            serverObjectManager.Spawn(readyPlayer, server.LocalPlayer);
            readyComp.Ready();

            await AsyncUtil.WaitUntilWithTimeout(() => readyComp.IsReady);
        });
    }
}
