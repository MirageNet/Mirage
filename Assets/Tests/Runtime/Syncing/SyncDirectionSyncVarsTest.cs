using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncDirectionTestBase<T> : ClientServerSetup<T> where T : NetworkBehaviour
    {
        protected static readonly MockPlayer.Guild guild = new MockPlayer.Guild("Fun");
        protected static readonly MockPlayer.Guild guild2 = new MockPlayer.Guild("Other");

        protected readonly NetworkWriter _ownerWriter = new NetworkWriter(1300);
        protected readonly NetworkWriter _observersWriter = new NetworkWriter(1300);
        protected readonly MirageNetworkReader _reader = new MirageNetworkReader();

        protected ClientInstance<T> _client2;

        /// <summary>
        /// Object that client1 Owns on client2
        /// </summary>
        protected T ObserverComponent { get; private set; }
        /// <summary>
        /// Object that client1 Owns on client1
        /// </summary>
        protected T OwnerComponent => clientComponent;

        protected NetworkIdentity ServerExtraIdentity { get; private set; }
        protected NetworkIdentity OwnerExtraIdentity { get; private set; }

        [TearDown]
        public void TearDown()
        {
            _ownerWriter.Reset();
            _observersWriter.Reset();
            _reader.Dispose();
        }

        public override async UniTask LateSetup()
        {
            _client2 = new ClientInstance<T>(ClientConfig, _server.socketFactory);
            _client2.client.Connect("localhost");

            await AsyncUtil.WaitUntilWithTimeout(() => server.Players.Count > 1);

            // get new player
            var serverPlayer2 = server.Players.First(x => x != serverPlayer);

            _client2.clientObjectManager.RegisterPrefab(playerPrefab.GetNetworkIdentity());

            // wait for client and server to initialize themselves
            await UniTask.Yield();

            var serverCharacter2 = InstantiateForTest(playerPrefab);
            serverObjectManager.AddCharacter(serverPlayer2, serverCharacter2);

            await AsyncUtil.WaitUntilWithTimeout(() => _client2.client.Player.HasCharacter);

            _client2.SetupCharacter();

            var found = _client2.client.World.TryGetIdentity(serverComponent.NetId, out var player1Character);
            if (!found)
                Debug.LogError("Could not find instance of player 1's character on client 2");
            ObserverComponent = player1Character.GetComponent<T>();
            Debug.Assert(ObserverComponent != null);

            ServerExtraIdentity = InstantiateForTest(playerPrefab).GetNetworkIdentity();
            Debug.Assert(ServerExtraIdentity != null);
            serverObjectManager.Spawn(ServerExtraIdentity);

            await UniTask.Yield();

            if (client.World.TryGetIdentity(ServerExtraIdentity.NetId, out var ownerExtra))
            {
                OwnerExtraIdentity = ownerExtra;
            }
        }

        protected void SetDirection(SyncFrom from, SyncTo to)
        {
            Debug.Assert(SyncSettings.IsValidDirection(from, to));

            serverComponent.SyncSettings.From = from;
            serverComponent.SyncSettings.To = to;
            serverComponent._nextSyncTime = Time.time;

            OwnerComponent.SyncSettings.From = from;
            OwnerComponent.SyncSettings.To = to;
            OwnerComponent._nextSyncTime = Time.time;

            ObserverComponent.SyncSettings.From = from;
            ObserverComponent.SyncSettings.To = to;
            ObserverComponent._nextSyncTime = Time.time;
        }
    }

    // differentCases

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
