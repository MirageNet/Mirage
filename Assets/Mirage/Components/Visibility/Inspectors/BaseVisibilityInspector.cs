using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    public abstract class BaseVisibilityInspector : MonoBehaviour
    {
        #region Fields

        protected ServerObjectManager ServerObjectManager;
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

        private void Awake()
        {
            ServerObjectManager ??= FindObjectOfType<ServerObjectManager>();
        }

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
