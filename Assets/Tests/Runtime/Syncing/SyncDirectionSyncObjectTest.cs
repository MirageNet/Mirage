using System.Collections;
using Mirage.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class MockPlayerWithList : MockPlayer
    {
        public SyncList<int> MySyncList = new SyncList<int>();
    }

    // different Directions to test

    // server -> owner
    // server -> observer
    // server -> owner,observer

    // owner -> server
    // owner -> server,observer

    // owner,server -> owner,server
    // owner,server -> server,observer
    // owner,server -> owner,server,observer

    public class SyncDirectionObjectFromServer : SyncDirectionTestBase<MockPlayerWithList>
    {
        [UnityTest]
        public IEnumerator ToOwner()
        {
            Assert.Fail();
            SetDirection(SyncFrom.Server, SyncTo.Owner);

            serverComponent.guild = guild;
            serverComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToObserver()
        {
            Assert.Fail();
            SetDirection(SyncFrom.Server, SyncTo.ObserversOnly);

            serverComponent.guild = guild;
            serverComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
        }

        [UnityTest]
        public IEnumerator ToOwnerAndObserver()
        {
            Assert.Fail();
            SetDirection(SyncFrom.Server, SyncTo.OwnerAndObservers);

            serverComponent.guild = guild;
            serverComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
        }
    }

    public class SyncDirectionObjectFromOwner : SyncDirectionTestBase<MockPlayerWithList>
    {
        [UnityTest]
        public IEnumerator ToServer()
        {
            Assert.Fail();
            SetDirection(SyncFrom.Owner, SyncTo.Server);

            OwnerComponent.guild = guild;
            OwnerComponent.target = OwnerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(serverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(serverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            Assert.Fail();
            SetDirection(SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            OwnerComponent.guild = guild;
            OwnerComponent.target = OwnerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(serverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(serverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
        }
    }

    public class SyncDirectionObjectFromServerAndOwner : SyncDirectionTestBase<MockPlayerWithList>
    {
        [UnityTest]
        public IEnumerator ToServerAndOwner()
        {
            Assert.Fail();
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner);

            serverComponent.guild = guild;
            serverComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(serverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(serverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);

            OwnerComponent.guild = guild2;
            OwnerComponent.target = null;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(serverComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(serverComponent.target, Is.Null);

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            Assert.Fail();
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            serverComponent.guild = guild;
            serverComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(serverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(serverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            // just update guild
            OwnerComponent.guild = guild2;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(serverComponent.guild.name, Is.EqualTo(guild2.name));
            // target should not be changed
            Assert.That(serverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
        }

        [UnityTest]
        public IEnumerator ToServerOwnerAndObservers()
        {
            Assert.Fail();
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner | SyncTo.ObserversOnly);

            serverComponent.guild = guild;
            serverComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(serverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(serverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            OwnerComponent.guild = guild2;
            OwnerComponent.target = null;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(serverComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(serverComponent.target, Is.Null);

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator CanSetDifferentVarsOnDifferentFrom()
        {
            Assert.Fail();
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner);

            OwnerComponent.guild = guild;
            serverComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(serverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(serverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }
    }
}
