using UnityEngine;

namespace Mirage.Components
{
    [DisallowMultipleComponent]
    public class SceneVisibilityFactory : VisibilitySystemFactory
    {
        protected override void Initialize()
        {
            NetworkVisibility = new SceneVisibilitySystem(ServerObjectManager);
        }
    }
}
