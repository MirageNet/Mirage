using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    // different Directions to test

    // server -> owner
    // server -> observer
    // server -> owner,observer

    // owner -> server
    // owner -> server,observer

    // owner,server -> owner,server
    // owner,server -> server,observer
    // owner,server -> owner,server,observer

    public class SyncDirectionFromServer : SyncDirectionTestBase<MockPlayer>
    {
        [UnityTest]
        public IEnumerator ToOwner()
        {
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

    public class SyncDirectionFromOwner : SyncDirectionTestBase<MockPlayer>
    {
        [UnityTest]
        public IEnumerator ToServer()
        {
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

    public class SyncDirectionFromServerAndOwner : SyncDirectionTestBase<MockPlayer>
    {
        [UnityTest]
        public IEnumerator ToServerAndOwner()
        {
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
