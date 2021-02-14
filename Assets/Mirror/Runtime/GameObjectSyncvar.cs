using UnityEngine;

namespace Mirror
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
        internal IObjectLocator objectLocator;
        internal uint netId;

        internal GameObject gameObject;

        internal uint NetId => gameObject != null ? gameObject.GetComponent<NetworkIdentity>().NetId : netId;

        public GameObject Value
        {
            get
            {
                if (gameObject != null)
                    return gameObject;

                if (objectLocator != null)
                {
                    NetworkIdentity result = objectLocator[netId];
                    if (result != null)
                        return result.gameObject;
                }

                return null;
            }

            set
            {
                if (value == null)
                    netId = 0;
                gameObject = value;
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
            uint netId = reader.ReadPackedUInt32();

            NetworkIdentity identity = null;
            if (!(reader.ObjectLocator is null))
                identity = reader.ObjectLocator[netId];

            return new GameObjectSyncvar
            {
                objectLocator = reader.ObjectLocator,
                netId = netId,
                gameObject = identity != null ? identity.gameObject : null
            };
        }
    }
}