using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    public abstract class VisibilitySystemFactory : MonoBehaviour
    {
        public NetworkServer Server;
        public VisibilitySystem System { get; private set; }

        private void Awake()
        {
            System = CreateSystem(Server.GetComponent<ServerObjectManager>());
        }
        private void OnDestroy()
        {
            System?.ShutDown();
            System = null;
        }

        /// <summary>
        /// Do initialization of data inside of here.
        /// </summary>
        protected abstract VisibilitySystem CreateSystem(ServerObjectManager serverObjectManager);
    }
}
