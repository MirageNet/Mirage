using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class NetworkServerTests : ClientServerSetup<MockComponent>
    {
        WovenTestMessage message;

        public override void ExtraSetup()
        {
            message = new WovenTestMessage
            {
                IntValue = 1,
                DoubleValue = 1.0,
                StringValue = "hello"
            };

        }

        [Test]
        public void InitializeTest()
        {
            Assert.That(server.Players, Has.Count.EqualTo(1));
            Assert.That(server.Active);
            Assert.That(server.LocalClientActive, Is.False);
        }

        [Test]
        public void ThrowsIfListenIsCalledWhileAlreadyActive()
        {
            InvalidOperationException expection = Assert.Throws<InvalidOperationException>(() =>
            {
                server.StartServer();
            });
            Assert.That(expection, Has.Message.EqualTo("Server is already active"));
        }

        [UnityTest]
        public IEnumerator ReadyMessageSetsClientReadyTest() => UniTask.ToCoroutine(async () =>
        {
            clientPlayer.Send(new SceneReadyMessage());

            await AsyncUtil.WaitUntilWithTimeout(() => serverPlayer.SceneIsReady);

            // ready?
            Assert.That(serverPlayer.SceneIsReady, Is.True);
        });

        [UnityTest]
        public IEnumerator SendToAll() => UniTask.ToCoroutine(async () =>
        {
            bool invoked = false;

            ClientMessageHandler.RegisterHandler<WovenTestMessage>(msg => invoked = true);

            server.SendToAll(message);

            // todo assert correct message was sent using Substitute for socket or player
            // connectionToServer.ProcessMessagesAsync().Forget();

            await AsyncUtil.WaitUntilWithTimeout(() => invoked);
        });

        [UnityTest]
        public IEnumerator SendToClientOfPlayer() => UniTask.ToCoroutine(async () =>
        {
            bool invoked = false;

            ClientMessageHandler.RegisterHandler<WovenTestMessage>(msg => invoked = true);

            serverIdentity.Owner.Send(message);

            // todo assert correct message was sent using Substitute for socket or player
            // connectionToServer.ProcessMessagesAsync().Forget();

            await AsyncUtil.WaitUntilWithTimeout(() => invoked);
        });

        [UnityTest]
        public IEnumerator RegisterMessage1() => UniTask.ToCoroutine(async () =>
        {
            bool invoked = false;

            ServerMessageHandler.RegisterHandler<WovenTestMessage>(msg => invoked = true);
            clientPlayer.Send(message);

            await AsyncUtil.WaitUntilWithTimeout(() => invoked);

        });

        [UnityTest]
        public IEnumerator RegisterMessage2() => UniTask.ToCoroutine(async () =>
        {
            bool invoked = false;

            ServerMessageHandler.RegisterHandler<WovenTestMessage>((conn, msg) => invoked = true);

            clientPlayer.Send(message);

            await AsyncUtil.WaitUntilWithTimeout(() => invoked);
        });

        [UnityTest]
        public IEnumerator UnRegisterMessage1() => UniTask.ToCoroutine(async () =>
        {
            MessageDelegate<WovenTestMessage> func = Substitute.For<MessageDelegate<WovenTestMessage>>();

            ServerMessageHandler.RegisterHandler(func);
            ServerMessageHandler.UnregisterHandler<WovenTestMessage>();

            clientPlayer.Send(message);

            await UniTask.Delay(1);

            func.Received(0).Invoke(
                Arg.Any<WovenTestMessage>());
        });

        [Test]
        public void NumPlayersTest()
        {
            Assert.That(server.NumberOfPlayers, Is.EqualTo(1));
        }

        [Test]
        public void VariableTest()
        {
            Assert.That(server.MaxConnections, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator StopStateTest() => UniTask.ToCoroutine(async () =>
        {
            server.Stop();

            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);
        });

        [UnityTest]
        public IEnumerator StoppedInvokeTest() => UniTask.ToCoroutine(async () =>

        {
            UnityAction func1 = Substitute.For<UnityAction>();
            server.Stopped.AddListener(func1);

            server.Stop();

            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);

            func1.Received(1).Invoke();
        });

        public IEnumerator ApplicationQuitTest() => UniTask.ToCoroutine(async () =>
        {
            UnityAction func1 = Substitute.For<UnityAction>();
            server.Stopped.AddListener(func1);

            await UniTask.Delay(1);

            Application.Quit();

            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);

            func1.Received(1).Invoke();
        });

        [UnityTest]
        public IEnumerator DisconnectCalledBeforePlayerIsDestroyed()
        {
            INetworkPlayer serverPlayer = base.serverPlayer;
            int disconnectCalled = 0;
            server.Disconnected.AddListener(player =>
            {
                disconnectCalled++;
                Assert.That(player, Is.EqualTo(serverPlayer));
                // use unity null check
                Assert.That(player.Identity != null);
            });


            client.Disconnect();
            // wait a tick for messages to be processed
            yield return null;

            Assert.That(disconnectCalled, Is.EqualTo(1));
            // use unity null check
            Assert.That(serverPlayer.Identity == null);

        }
    }
}
