using Cysharp.Threading.Tasks;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncDirectionTestBase<T> : MultiRemoteClientSetup<T> where T : NetworkBehaviour
    {
        protected override int RemoteClientCount => 2;

        protected static readonly MockPlayer.Guild guild = new MockPlayer.Guild("Fun");
        protected static readonly MockPlayer.Guild guild2 = new MockPlayer.Guild("Other");

        protected readonly NetworkWriter _ownerWriter = new NetworkWriter(1300);
        protected readonly NetworkWriter _observersWriter = new NetworkWriter(1300);
        protected readonly MirageNetworkReader _reader = new MirageNetworkReader();

        /// <summary>Object on server that client0 owns</summary>
        protected new T ServerComponent => ServerComponent(0);
        /// <summary>Object on client0 that client0 owns</summary>
        protected T OwnerComponent => ClientComponent(0);
        /// <summary>Object on client1 that client0 owns</summary>
        protected T ObserverComponent => _remoteClients[1].Get(OwnerComponent);


        protected NetworkIdentity ServerExtraIdentity { get; private set; }
        protected T ServerExtraComponent { get; private set; }

        [TearDown]
        public void TearDown()
        {
            _ownerWriter.Reset();
            _observersWriter.Reset();
            _reader.Dispose();
        }

        protected override async UniTask LateSetup()
        {
            await base.LateSetup();

            ServerExtraIdentity = InstantiateForTest(_characterPrefab);
            ServerExtraComponent = ServerExtraIdentity.GetComponent<T>();
            Debug.Assert(ServerExtraIdentity != null);
            serverObjectManager.Spawn(ServerExtraIdentity);

            await UniTask.Yield();
        }

        protected static void SetDirection(NetworkBehaviour behaviour, SyncFrom from, SyncTo to)
        {
            Debug.Assert(SyncSettings.IsValidDirection(from, to));

            behaviour.SyncSettings.From = from;
            behaviour.SyncSettings.To = to;
            behaviour._nextSyncTime = Time.time;
            behaviour.SyncSettings.Timing = SyncTiming.NoInterval;
            behaviour.UpdateSyncObjectShouldSync();
        }

        protected void SetDirection(SyncFrom from, SyncTo to)
        {
            RunOnAll(ServerComponent(0), comp =>
            {
                SetDirection(comp, from, to);
            });
        }
    }
}
