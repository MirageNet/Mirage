using System;

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
    }
}
