using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;
using UnityEngine;

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
        protected T ServerExtraComponent { get; private set; }
        /// <summary>
        /// Object on the owner's instance, but is not owned by them
        /// </summary>
        protected NetworkIdentity OwnerExtraIdentity { get; private set; }
        protected T OwnerExtraComponent { get; private set; }

        [TearDown]
        public void TearDown()
        {
            _ownerWriter.Reset();
            _observersWriter.Reset();
            _reader.Dispose();

            GameObject.Destroy(_client2.go);
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
            ServerExtraComponent = ServerExtraIdentity.GetComponent<T>();
            Debug.Assert(ServerExtraIdentity != null);
            serverObjectManager.Spawn(ServerExtraIdentity);

            await UniTask.Yield();

            if (client.World.TryGetIdentity(ServerExtraIdentity.NetId, out var ownerExtra))
            {
                OwnerExtraIdentity = ownerExtra;
                OwnerExtraComponent = ownerExtra.GetComponent<T>();
            }
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
            SetDirection(serverComponent, from, to);
            SetDirection(OwnerComponent, from, to);
            SetDirection(ObserverComponent, from, to);
        }
    }
}
