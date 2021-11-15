using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    public abstract class VisibilitySystemFactory : MonoBehaviour
    {
        #region Fields

        public NetworkServer Server;
        public VisibilitySystem System { get; private set; }

        #endregion

        #region Mirage Callbacks

        private void OnServerStarted()
        {
            // todo is this null check ok?
            System = CreateSystem(Server.GetComponent<ServerObjectManager>());
            System.Startup();
        }

        private void ShutDown()
        {
            System?.ShutDown();
            System = null;
        }

        #endregion

        /// <summary>
        ///     Do initialization of data inside of here.
        /// </summary>
        protected abstract VisibilitySystem CreateSystem(ServerObjectManager serverObjectManager);

        #region Unity Methods

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Server.Stopped.AddListener(ShutDown);
        }
        private void OnDestroy()
        {
            ShutDown();
        }

        #endregion
    }
}
