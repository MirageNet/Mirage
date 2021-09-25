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

        #region Unity Methods

        protected abstract void Start();

        private void OnEnable()
        {
            NetworkVisibility?.Startup();
        }

        private void OnDisable()
        {
            NetworkVisibility?.ShutDown();
        }

        private void Destroy()
        {
            NetworkVisibility = null;
        }

        #endregion
    }
}
