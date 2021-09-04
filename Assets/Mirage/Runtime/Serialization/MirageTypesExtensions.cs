using System;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Serialization
{
    public static class MirageTypesExtensions
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(MirageTypesExtensions));

        public static void WriteNetworkIdentity(this NetworkWriter writer, NetworkIdentity value)
        {
            if (value == null)
            {
                writer.WritePackedUInt32(0);
                return;
            }
            writer.WritePackedUInt32(value.NetId);
        }
        public static void WriteNetworkBehaviour(this NetworkWriter writer, NetworkBehaviour value)
        {
            if (value == null)
            {
                writer.WriteNetworkIdentity(null);
                return;
            }
            writer.WriteNetworkIdentity(value.Identity);
            writer.WriteByte((byte)value.ComponentIndex);
        }
        public static void WriteGameObject(this NetworkWriter writer, GameObject value)
        {
            if (value == null)
            {
                writer.WriteNetworkIdentity(null);
                return;
            }
            NetworkIdentity identity = value.GetComponent<NetworkIdentity>();
            if (identity == null)
                throw new InvalidOperationException($"Cannot send GameObject without a NetworkIdentity {value.name}");
            writer.WriteNetworkIdentity(identity);
        }



        public static NetworkIdentity ReadNetworkIdentity(this NetworkReader reader)
        {
            uint netId = reader.ReadPackedUInt32();
            if (netId == 0)
                return null;

            if (reader.ObjectLocator != null)
            {
                // if not found return c# null
                return reader.ObjectLocator.TryGetIdentity(netId, out NetworkIdentity identity)
                    ? identity
                    : null;
            }

            if (logger.WarnEnabled()) logger.LogFormat(LogType.Warning, "ReadNetworkIdentity netId:{0} not found in spawned", netId);
            return null;
        }

        public static NetworkBehaviour ReadNetworkBehaviour(this NetworkReader reader)
        {
            NetworkIdentity identity = reader.ReadNetworkIdentity();
            if (identity == null)
            {
                return null;
            }

            byte componentIndex = reader.ReadByte();
            return identity.NetworkBehaviours[componentIndex];
        }

        public static T ReadNetworkBehaviour<T>(this NetworkReader reader) where T : NetworkBehaviour
        {
            return reader.ReadNetworkBehaviour() as T;
        }

        public static GameObject ReadGameObject(this NetworkReader reader)
        {
            NetworkIdentity identity = reader.ReadNetworkIdentity();
            if (identity == null)
            {
                return null;
            }
            return identity.gameObject;
        }
    }
}
