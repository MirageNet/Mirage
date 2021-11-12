using UnityEngine;

namespace Mirage.Components
{
    [DisallowMultipleComponent]
    public class NetworkSceneChecker : BaseVisibilityInspector
    {
        protected override void Initialize()
        {
            NetworkVisibility = new SceneVisibilityChecker(ServerObjectManager);
        }
    }
}
