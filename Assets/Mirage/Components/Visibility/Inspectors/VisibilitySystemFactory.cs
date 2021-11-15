using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    public abstract class VisibilitySystemFactory : MonoBehaviour
    {
        #region Fields

        protected ServerObjectManager ServerObjectManager;
        protected NetworkServer Server;
        protected internal VisibilitySystem System;

        #endregion

        #region Mirage Callbacks

        private void OnServerStarted()
        {
            // todo is this null check ok?
            System?.Startup();
        }

        private void OnServerStopped()
        {
            System?.ShutDown();
        }

        #endregion

        /// <summary>
        ///     Do initialization of data inside of here.
        /// </summary>
        protected abstract void CreateSystem();

        #region Unity Methods

        private void Awake()
        {
            // todo find better way to find ServerObjectManager
            ServerObjectManager = FindObjectOfType<ServerObjectManager>();
            Server = FindObjectOfType<NetworkServer>();

            CreateSystem();

            Server.Started.AddListener(OnServerStarted);
            Server.Stopped.AddListener(OnServerStarted);
        }

        private void Destroy()
        {
            System = null;
        }

        #endregion
    }
}
