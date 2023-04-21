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
        private GameObject readyPlayer;
        private LobbyReady lobby;
        private ObjectReady readyComp;

        protected async override UniTask ExtraSetup() 
        {
            await base.ExtraSetup();
            lobby = networkManagerGo.AddComponent<LobbyReady>();
        }

        public override void ExtraTearDown()
        {
            lobby = null;
        }

        [Test]
        public void SetAllClientsNotReadyTest()
        {
            readyComp = hostIdentity.gameObject.AddComponent<ObjectReady>();
            lobby.ObjectReadyList.Add(readyComp);
            readyComp.IsReady = true;

            lobby.SetAllClientsNotReady();

            Assert.That(readyComp.IsReady, Is.False);
        }

        [UnityTest]
        public IEnumerator SendToReadyTest() => UniTask.ToCoroutine((System.Func<UniTask>)(async () =>
        {
            readyComp = base.hostIdentity.gameObject.AddComponent<ObjectReady>();
            lobby.ObjectReadyList.Add(readyComp);
            readyComp.IsReady = true;

            var invokeWovenTestMessage = false;
            ClientMessageHandler.RegisterHandler<SceneMessage>(msg => invokeWovenTestMessage = true);
            lobby.SendToReady((NetworkIdentity)base.hostIdentity, new SceneMessage(), true, Channel.Reliable);

            await AsyncUtil.WaitUntilWithTimeout(() => invokeWovenTestMessage);
        }));

        [Test]
        public void IsReadyStateTest()
        {
            readyComp = hostIdentity.gameObject.AddComponent<ObjectReady>();

            Assert.That(readyComp.IsReady, Is.False);
        }

        [Test]
        public void SetClientReadyTest()
        {
            readyComp = hostIdentity.gameObject.AddComponent<ObjectReady>();

            readyComp.SetClientReady();

            Assert.That(readyComp.IsReady, Is.True);
        }

        [Test]
        public void SetClientNotReadyTest()
        {
            readyComp = hostIdentity.gameObject.AddComponent<ObjectReady>();

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
