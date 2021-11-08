using UnityEngine;

namespace Mirage.Components
{
    public class NetworkMatchChecker : BaseVisibilityInspector
    {
        #region Overrides of BaseVisibilityInspector

        protected override void Start()
        {
            NetworkVisibility = new NetworkMatchCheckerVisibility(ServerObjectManager, Identity);

            base.Start();
        }

        #endregion
    }
}
