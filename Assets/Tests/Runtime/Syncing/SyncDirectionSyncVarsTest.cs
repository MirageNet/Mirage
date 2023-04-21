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

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;

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

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
        }

        [UnityTest]
        public IEnumerator ToOwnerAndObserver()
        {
            SetDirection(SyncFrom.Server, SyncTo.OwnerAndObservers);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
        }
    }

    public class SyncDirectionFromOwner : SyncDirectionTestBase<MockPlayer>
    {
        [UnityTest]
        public IEnumerator ToServer()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server);

            OwnerComponent.guild = guild;
            OwnerComponent.target = _remoteClients[0].Get(ServerExtraIdentity);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            OwnerComponent.guild = guild;
            OwnerComponent.target = _remoteClients[0].Get(ServerExtraIdentity);

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
        }
    }

    public class SyncDirectionFromServerAndOwner : SyncDirectionTestBase<MockPlayer>
    {
        [UnityTest]
        public IEnumerator ToServerAndOwner()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);

            OwnerComponent.guild = guild2;
            OwnerComponent.target = null;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ServerComponent.target, Is.Null);

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));

            // just update guild
            OwnerComponent.guild = guild2;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild2.name));
            // target should not be changed
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));
        }

        [UnityTest]
        public IEnumerator ToServerOwnerAndObservers()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner | SyncTo.ObserversOnly);

            ServerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[1].Get(ServerExtraIdentity)));

            OwnerComponent.guild = guild2;
            OwnerComponent.target = null;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(OwnerComponent.target, Is.Null);

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ServerComponent.target, Is.Null);

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild2.name));
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator CanSetDifferentVarsOnDifferentFrom()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner);

            OwnerComponent.guild = guild;
            ServerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(OwnerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(OwnerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ServerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ServerComponent.target, Is.EqualTo(ServerExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }
    }
}
