using System;
using Mirage;
using Mirage.Serialization;

namespace Mirage.Components
{
    /// <summary>
    /// Simple component to track if a player is ready in a lobby
    /// <para>
    /// To best use this component Set Sync Direction from owner to server
    /// </para>
    /// </summary>
    public class ReadyCheck : NetworkBehaviour
    {
        public event Action<bool> OnReadyChanged;

        [SyncVar(hook = nameof(OnReadyChanged), invokeHookOnServer = true, invokeHookOnOwner = true)]
        private bool _isReady;

        public bool IsReady => _isReady;

        // note need a methods to set syncvar, otherwise scripts in another asmdef will not set if via weaver
        public void SetReady(bool ready)
        {
            _isReady = ready;
        }
        
        [ClientRpc(target = Player)]
        public void TargetRpc(INetworkPlayer target, INetworkPlayer badTarget, int arg1)
        {
            //
        }
    }
}
namespace A
{


    [NetworkMessage]
    public struct ValidMessage
    {
        [MaxLength(100)]
        public CustomType customValue;
    }

    public struct CustomType
    {
        // type with invalid field but custom writers
        public int[,] myArray;
    }

    public static class CustomLengthSerialization
    {
        public static void WriteCustomType(this NetworkWriter writer, CustomType value, int maxLength) { }
        public static CustomType ReadCustomType(this NetworkReader reader, int maxLength) => default;
    }

}
namespace B
{
    [NetworkMessage]
    public struct ValidMessage
    {
        public CustomType customValue;
    }

    public struct CustomType
    {
        // type with invalid field but custom writers
        public int[,] myArray;
    }

    public static class CustomSerialization
    {
        public static void WriteCustomType(this NetworkWriter writer, CustomType value) { }
        public static CustomType ReadCustomType(this NetworkReader reader) => default;
    }


}
