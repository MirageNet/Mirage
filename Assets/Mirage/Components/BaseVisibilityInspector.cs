using Mirage;
using Mirage.InterestManagement;
using UnityEngine;

namespace Assets.Mirage.Components
{
    public abstract class BaseVisibilityInspector : MonoBehaviour
    {
        #region Fields

        private protected ServerObjectManager ServerObjectManager;
        private protected INetworkVisibility NetworkVisibility;

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

        protected virtual void Awake()
        {
            ServerObjectManager ??= FindObjectOfType<ServerObjectManager>();

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
