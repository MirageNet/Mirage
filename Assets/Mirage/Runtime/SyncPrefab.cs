using System.Collections.Generic;
using Mirage.Serialization;

namespace Mirage
{
    public struct SyncPrefab
    {
        public NetworkIdentity Prefab;
        public int PrefabHash;

        public SyncPrefab(NetworkIdentity prefab) : this()
        {
            Prefab = prefab;
        }

        public SyncPrefab(int hash) : this()
        {
            PrefabHash = hash;
        }

        /// <summary>
        /// Searches ClientObjectManager to find a prefab using its hash
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public NetworkIdentity FindPrefab(ClientObjectManager manager)
        {
            if (Prefab == null && PrefabHash != 0)
            {
                var handler = manager.GetSpawnHandler(PrefabHash);
                Prefab = handler.Prefab;
            }

            return Prefab;
        }

        /// <summary>
        /// Searches ClientObjectManager to find a prefab using its hash
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public NetworkIdentity FindPrefab(IEnumerable<NetworkIdentity> collection)
        {
            if (Prefab == null && PrefabHash != 0)
            {
                foreach (var item in collection)
                {
                    if (item.PrefabHash == PrefabHash)
                    {
                        Prefab = item;
                        break;
                    }
                }
            }

            return Prefab;
        }
    }

    public static class SyncPrefabSerialize
    {
        public static void WriteSyncPrefab(this NetworkWriter writer, SyncPrefab value)
        {
            // update hash to be the prefab's hash
            if (value.Prefab != null)
            {
                var hash = value.Prefab.PrefabHash;
                // unless it is zero
                if (hash != 0)
                    value.PrefabHash = hash;
            }

            writer.WriteInt32(value.PrefabHash);
        }

        public static SyncPrefab ReadSyncPrefab(this NetworkReader reader)
        {
            var hash = reader.ReadInt32();
            return new SyncPrefab(hash);
        }
    }
}
