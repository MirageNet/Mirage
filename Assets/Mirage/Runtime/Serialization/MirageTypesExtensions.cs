using System;
using UnityEngine;

namespace Mirage.Serialization
{
    public static class MirageTypesExtensions
    {
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
            writer.WriteNetworkIdentity(value.NetIdentity);
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
    }
}
