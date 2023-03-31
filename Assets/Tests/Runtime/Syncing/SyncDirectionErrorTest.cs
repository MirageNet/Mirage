using System.Collections;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncDirectionErrorTest : SyncDirectionTestBase<MockPlayer>
    {
        [UnityTest]
        public IEnumerator DoesErrorStuff()
        {
            Assert.Fail();
            yield return null;
        }

        [Test]
        public void ClientWithOutAuth()
        {
            // set all
            SetDirection(SyncFrom.Server, SyncTo.Owner);

            // set just owner (to fake bad client)
            OwnerExtraIdentity.HasAuthority = true;
            SetDirection(OwnerExtraComponent, SyncFrom.Owner, SyncTo.Server);

            // set value to update syncvar
            OwnerExtraComponent.guild = new MockPlayer.Guild("Bad");

            SendSyncVars(OwnerExtraIdentity);

            LogAssert.Expect(LogType.Warning, $"UpdateVarsMessage for object without authority [netId={ServerExtraComponent.NetId}]");
            // should not throw, but should give warning
            var msgType = MessagePacker.UnpackId(_reader);
            server.MessageHandler.InvokeHandler(serverPlayer, msgType, _reader);

            Assert.That(ServerExtraComponent.guild.name, Is.Null.Or.Empty, "Server should not have updated value");
        }

        private void SendSyncVars(NetworkIdentity target)
        {
            var (ownerWritten, observersWritten) = target.OnSerializeAll(false, _ownerWriter, _observersWriter);
            Assert.That(ownerWritten, Is.GreaterThanOrEqualTo(1));
            Assert.That(observersWritten, Is.EqualTo(0));
            var msg = new UpdateVarsMessage
            {
                netId = target.NetId,
                payload = _ownerWriter.ToArraySegment()
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
            prefab.gameObject.AddComponent<MockComponent>();
            prefab.gameObject.AddComponent<MockPlayer>();
            prefab.gameObject.AddComponent<MockPlayerWithList>();
            prefab.PrefabHash = 520;

            var serverIdentity = InstantiateForTest(prefab);
            serverIdentity.gameObject.SetActive(true);

            var serverComp1 = serverIdentity.GetComponent<MockComponent>();
            var serverComp2 = serverIdentity.GetComponent<MockPlayer>();
            var serverComp3 = serverIdentity.GetComponent<MockPlayerWithList>();

            // spawn with Authority
            clientObjectManager.RegisterPrefab(prefab);
            _client2.clientObjectManager.RegisterPrefab(prefab); // add with client2 as well, to stop error
            serverObjectManager.Spawn(serverIdentity, serverPlayer);

            yield return null;
            yield return null;

            MockComponent ownerComp1 = null;
            MockPlayer ownerComp2 = null;
            MockPlayerWithList ownerComp3 = null;

            if (client.World.TryGetIdentity(serverIdentity.NetId, out var ownerIdentity))
            {
                ownerIdentity.gameObject.SetActive(true);
                ownerComp1 = ownerIdentity.GetComponent<MockComponent>();
                ownerComp2 = ownerIdentity.GetComponent<MockPlayer>();
                ownerComp3 = ownerIdentity.GetComponent<MockPlayerWithList>();
            }
            else
            {
                Assert.Fail("Failed to create client object");
            }

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
                server.MessageHandler.InvokeHandler(serverPlayer, msgType, _reader);
            });

            // values not set
            Assert.That(serverComp2.guild.name, Is.Null.Or.Empty.Or.EqualTo("Good"), "this value is allowed to be set, but might noto have");
            Assert.That(serverComp3.guild.name, Is.Null.Or.Empty, "Server should not have updated value");

            Assert.That(exception.Message, Does.StartWith("Invalid sync settings on"));
        }
    }
}
