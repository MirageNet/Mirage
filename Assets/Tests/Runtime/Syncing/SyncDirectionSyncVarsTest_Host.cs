using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;
using Mirage.Tests.Runtime.Host;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    // todo find way to avoid having a full copy of this class
    public class SyncDirectionTestBase_Host : HostSetup<MockPlayer>
    {
        protected static readonly MockPlayer.Guild guild = new MockPlayer.Guild("Fun");
        protected static readonly MockPlayer.Guild guild2 = new MockPlayer.Guild("Other");

        protected readonly NetworkWriter _ownerWriter = new NetworkWriter(1300);
        protected readonly NetworkWriter _observersWriter = new NetworkWriter(1300);
        protected readonly MirageNetworkReader _reader = new MirageNetworkReader();

        protected int _onSerializeCalled;
        protected int _onDeserializeCalled;
        protected INetworkPlayer _serverPlayer2;


        /// <summary>Object on client0 that hostOwns</summary>
        protected MockPlayer ObserverComponent => _remoteClients[0].Get(hostComponent);

        /// <summary>Objcet that server controls</summary>
        protected NetworkIdentity HostExtraIdentity { get; private set; }
        protected MockPlayer HostExtraComponent { get; private set; }

        protected void ResetCounters()
        {
            _onSerializeCalled = 0;
            _onDeserializeCalled = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _ownerWriter.Reset();
            _observersWriter.Reset();
            _reader.Dispose();
        }

        protected override async UniTask LateSetup()
        {
            await AddClient();

            HostExtraIdentity = InstantiateForTest(_characterPrefab);
            HostExtraComponent = HostExtraIdentity.GetComponent<MockPlayer>();
            serverObjectManager.Spawn(HostExtraIdentity);

            await UniTask.Yield();

            hostComponent.OnSerializeCalled += () => _onSerializeCalled++;
            hostComponent.OnDeserializeCalled += () => _onDeserializeCalled++;
        }

        protected static void SetDirection(NetworkBehaviour behaviour, SyncFrom from, SyncTo to)
        {
            Debug.Assert(SyncSettings.IsValidDirection(from, to));

            behaviour.SyncSettings.From = from;
            behaviour.SyncSettings.To = to;
            behaviour._nextSyncTime = Time.time;
        }
        protected void SetDirection(SyncFrom from, SyncTo to)
        {
            RunOnAll(hostComponent, comp =>
            {
                SetDirection(comp, from, to);
            });
        }
    }

    public class SyncDirectionFromServer_Host : SyncDirectionTestBase_Host
    {
        [UnityTest]
        public IEnumerator ToOwner()
        {
            SetDirection(SyncFrom.Server, SyncTo.Owner);

            ResetCounters();
            hostComponent.guild = guild;
            hostComponent.target = HostExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            // Should not have serialized anything
            Assert.That(_onSerializeCalled, Is.Zero);
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.target, Is.EqualTo(HostExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToObserver()
        {
            SetDirection(SyncFrom.Server, SyncTo.ObserversOnly);

            ResetCounters();
            hostComponent.guild = guild;
            hostComponent.target = HostExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.target, Is.EqualTo(HostExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[0].Get(HostExtraIdentity)));
        }

        [UnityTest]
        public IEnumerator ToOwnerAndObserver()
        {
            SetDirection(SyncFrom.Server, SyncTo.OwnerAndObservers);

            ResetCounters();
            hostComponent.guild = guild;
            hostComponent.target = HostExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            // Should not have serialized anything
            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.target, Is.EqualTo(HostExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[0].Get(HostExtraIdentity)));
        }
    }

    public class SyncDirectionFromOwner_Host : SyncDirectionTestBase_Host
    {
        [UnityTest]
        public IEnumerator ToServer()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server);

            ResetCounters();
            hostComponent.guild = guild;
            hostComponent.target = HostExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.Zero);
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.target, Is.EqualTo(HostExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            ResetCounters();
            hostComponent.guild = guild;
            hostComponent.target = HostExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.target, Is.EqualTo(HostExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            var onObserver = _remoteClients[0].Get(HostExtraIdentity);
            Assert.That(ObserverComponent.target, Is.EqualTo(onObserver));
        }
    }

    public class SyncDirectionFromServerAndOwner_Host : SyncDirectionTestBase_Host
    {
        [UnityTest]
        public IEnumerator ToServerAndOwner()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner);

            ResetCounters();
            hostComponent.guild = guild;
            hostComponent.target = HostExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.Zero);
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.target, Is.EqualTo(HostExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            ResetCounters();
            hostComponent.guild = guild;
            hostComponent.target = HostExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;


            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.target, Is.EqualTo(HostExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[0].Get(HostExtraIdentity)));
        }

        [UnityTest]
        public IEnumerator ToServerOwnerAndObservers()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner | SyncTo.ObserversOnly);

            ResetCounters();
            hostComponent.guild = guild;
            hostComponent.target = HostExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(hostComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(hostComponent.target, Is.EqualTo(HostExtraIdentity));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target, Is.EqualTo(_remoteClients[0].Get(HostExtraIdentity)));
        }
    }
}
