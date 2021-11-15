using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    public abstract class BaseVisibilityInspector : MonoBehaviour
    {
        #region Fields

        protected ServerObjectManager ServerObjectManager;
        protected internal INetworkVisibility NetworkVisibility;

        #endregion

        #region Mirage Callbacks

        private void OnServerStarted()
        {
            // todo is this null check ok?
            NetworkVisibility?.Startup();
        }

        private void OnServerStopped()
        {
            NetworkVisibility?.ShutDown();
        }

        #endregion

        /// <summary>
        ///     Do initialization of data inside of here.
        /// </summary>
        protected abstract void Initialize();

        #region Unity Methods

        private void Awake()
        {
            // todo find better way to find ServerObjectManager
            ServerObjectManager = FindObjectOfType<ServerObjectManager>();

            Initialize();

            if (!ServerObjectManager.Server.Active)
                ServerObjectManager.Server.Started.AddListener(OnServerStarted);
            else
            {
                OnServerStarted();
            }

            ServerObjectManager.Server.Stopped.AddListener(OnServerStopped);
        }

        private void Destroy()
        {
            NetworkVisibility = null;
        }

        #endregion
    }
}
