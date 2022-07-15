using Mirage.Serialization;
using UnityEngine;

namespace Mirage
{

    /// <summary>
    /// backing struct for a NetworkIdentity when used as a syncvar
    /// the weaver will replace the syncvar with this struct.
    /// </summary>
    public struct GameObjectSyncvar
    {
        /// <summary>
        /// The network client that spawned the parent object
        /// used to lookup the identity if it exists
        /// </summary>
        internal IObjectLocator _objectLocator;
        internal uint _netId;

        internal GameObject _gameObject;

        internal uint NetId => _gameObject != null ? _gameObject.GetComponent<NetworkIdentity>().NetId : _netId;

        public GameObject Value
        {
            get
            {
                if (_gameObject != null)
                    return _gameObject;

                if (_objectLocator != null && _objectLocator.TryGetIdentity(NetId, out var result))
                {
                    return result.gameObject;
                }

                return null;
            }

            set
            {
                if (value == null)
                    _netId = 0;
                _gameObject = value;
            }
        }
    }

    public static class GameObjectSerializers
    {
        public static void WriteGameObjectSyncVar(this NetworkWriter writer, GameObjectSyncvar id)
        {
            writer.WritePackedUInt32(id.NetId);
        }

        public static GameObjectSyncvar ReadGameObjectSyncVar(this NetworkReader reader)
        {
            var mirageReader = reader.ToMirageReader();

            var netId = reader.ReadPackedUInt32();

            NetworkIdentity identity = null;
            var hasValue = mirageReader.ObjectLocator?.TryGetIdentity(netId, out identity) ?? false;

            return new GameObjectSyncvar
            {
                _objectLocator = mirageReader.ObjectLocator,
                _netId = netId,
                _gameObject = hasValue ? identity.gameObject : null
            };
        }
    }
}
