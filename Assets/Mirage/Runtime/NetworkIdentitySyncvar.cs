using Mirage.Serialization;

namespace Mirage
{

    /// <summary>
    /// backing struct for a NetworkIdentity when used as a syncvar
    /// the weaver will replace the syncvar with this struct.
    /// </summary>
    public struct NetworkIdentitySyncvar
    {
        /// <summary>
        /// The network client that spawned the parent object
        /// used to lookup the identity if it exists
        /// </summary>
        internal IObjectLocator objectLocator;
        internal uint netId;

        internal NetworkIdentity identity;

        internal uint NetId => identity != null ? identity.NetId : netId;

        public NetworkIdentity Value
        {
            get
            {
                if (identity != null)
                    return identity;

                if (objectLocator != null && objectLocator.TryGetIdentity(NetId, out NetworkIdentity result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                if (value == null)
                    netId = 0;
                identity = value;
            }
        }
    }


    public static class NetworkIdentitySerializers
    {
        public static void WriteNetworkIdentitySyncVar(this NetworkWriter writer, NetworkIdentitySyncvar id)
        {
            writer.WritePackedUInt32(id.NetId);
        }

        public static NetworkIdentitySyncvar ReadNetworkIdentitySyncVar(this NetworkReader reader)
        {
            uint netId = reader.ReadPackedUInt32();

            NetworkIdentity identity = null;
            reader.ObjectLocator?.TryGetIdentity(netId, out identity);

            return new NetworkIdentitySyncvar
            {
                objectLocator = reader.ObjectLocator,
                netId = netId,
                identity = identity
            };
        }
    }
}
