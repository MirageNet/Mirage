using System.Collections;
using System.Text.RegularExpressions;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncDirectionErrorTest : SyncDirectionTestBase<MockPlayer>
    {
        [Test]
        public void ClientWithOutAuth()
        {
            // set all
            SetDirection(SyncFrom.Server, SyncTo.Owner);

            // extra objects on client0
            var clientExtraIdentity = _remoteClients[0].Get(ServerExtraIdentity);
            var clientExtraComponent = _remoteClients[0].Get(ServerExtraComponent);

            // set just owner (to fake bad client)
            clientExtraIdentity.HasAuthority = true;
            SetDirection(clientExtraComponent, SyncFrom.Owner, SyncTo.Server);

            // set value to update syncvar
            clientExtraComponent.guild = new MockPlayer.Guild("Bad");

            SendSyncVars(clientExtraIdentity);

            LogAssert.Expect(LogType.Warning, new Regex(@$"UpdateVarsMessage for object without authority \[netId={ServerExtraComponent.NetId}\]"));
            // should not throw, but should give warning
            var msgType = MessagePacker.UnpackId(_reader);
            server.MessageHandler.InvokeHandler(ServerPlayer(0), msgType, _reader);

            Assert.That(ServerExtraComponent.guild.name, Is.Null.Or.Empty, "Server should not have updated value");
        }

        private void SendSyncVars(NetworkIdentity target)
        {
            var (ownerWritten, observersWritten) = target.OnSerializeDelta(Time.unscaledTimeAsDouble, _ownerWriter, _observersWriter);
            Assert.That(ownerWritten, Is.GreaterThanOrEqualTo(1));
            Assert.That(observersWritten, Is.EqualTo(0));
            var msg = new UpdateVarsMessage
            {
                NetId = target.NetId,
                Payload = _ownerWriter.ToArraySegment()
            };

            _observersWriter.Reset();
            MessagePacker.Pack(msg, _observersWriter);
            _reader.Reset(_observersWriter.ToArraySegment());
        }

        [UnityTest]

        public IEnumerator MultipleBehaviours()
        {
            var prefab = CreateNetworkIdentity();
            prefab.gameObject.SetActive(false);
            prefab.gameObject.AddComponent<MockRpcComponent>();
            prefab.gameObject.AddComponent<MockPlayer>();
            prefab.gameObject.AddComponent<MockPlayerWithList>();
            prefab.PrefabHash = 520;

            var serverIdentity = InstantiateForTest(prefab);
            serverIdentity.gameObject.SetActive(true);

            var serverComp1 = serverIdentity.GetComponent<MockRpcComponent>();
            var serverComp2 = serverIdentity.GetComponent<MockPlayer>();
            var serverComp3 = serverIdentity.GetComponent<MockPlayerWithList>();

            // spawn with Authority
            ClientObjectManager(0).RegisterPrefab(prefab);
            // add with client2 as well, to stop error
            ClientObjectManager(1).RegisterPrefab(prefab);
            serverObjectManager.Spawn(serverIdentity, ServerPlayer(0));

            yield return null;
            yield return null;

            var ownerIdentity = _remoteClients[0].Get(serverIdentity);
            var ownerComp1 = _remoteClients[0].Get(serverComp1);
            var ownerComp2 = _remoteClients[0].Get(serverComp2);
            var ownerComp3 = _remoteClients[0].Get(serverComp3);

            SetDirection(serverComp1, SyncFrom.Server, SyncTo.Owner);
            SetDirection(serverComp2, SyncFrom.Owner, SyncTo.Server); // comp2 is client auth
            SetDirection(serverComp3, SyncFrom.Server, SyncTo.Owner);

            SetDirection(ownerComp1, SyncFrom.Server, SyncTo.Owner);
            SetDirection(ownerComp2, SyncFrom.Owner, SyncTo.Server); // comp2 is client auth
            SetDirection(ownerComp3, SyncFrom.Owner, SyncTo.Server); // also set this bad value

            // set good value
            ownerComp2.guild = new MockPlayer.Guild("Good");
            // set bad value
            ownerComp3.guild = new MockPlayer.Guild("Bad");

            SendSyncVars(ownerIdentity);

            // should not throw, but should give warning
            var msgType = MessagePacker.UnpackId(_reader);

            var exception = Assert.Throws<DeserializeFailedException>(() =>
            {
                server.MessageHandler.InvokeHandler(ServerPlayer(0), msgType, _reader);
            });

            // values not set
            Assert.That(serverComp2.guild.name, Is.Null.Or.Empty.Or.EqualTo("Good"), "this value is allowed to be set, but might noto have");
            Assert.That(serverComp3.guild.name, Is.Null.Or.Empty, "Server should not have updated value");

            Assert.That(exception.Message, Does.StartWith("Invalid sync settings on"));
        }
    }
}
