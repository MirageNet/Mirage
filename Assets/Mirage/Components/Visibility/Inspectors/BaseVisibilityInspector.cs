using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    [DisallowMultipleComponent]
    public abstract class BaseVisibilityInspector : NetworkBehaviour
    {
        #region Fields

        protected INetworkVisibility NetworkVisibility;

        #endregion

        #region Mirage Callbacks

        private void OnServerStarted()
        {
            NetworkVisibility?.Startup();
        }

        private void OnServerStopped()
        {
            NetworkVisibility?.ShutDown();
        }

        #endregion

        #region Unity Methods

        protected virtual void Start()
        {
            ServerObjectManager.Server.Started.AddListener(OnServerStarted);
            ServerObjectManager.Server.Stopped.AddListener(OnServerStopped);
        }

        private void Destroy()
        {
            NetworkVisibility = null;
        }

        #endregion
    }
}
