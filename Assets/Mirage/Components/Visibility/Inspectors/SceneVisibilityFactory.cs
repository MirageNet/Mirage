using UnityEngine;

namespace Mirage.Components
{
    [DisallowMultipleComponent]
    public class SceneVisibilityFactory : VisibilitySystemFactory
    {
        protected override void CreateSystem()
        {
            System = new SceneVisibilitySystem(ServerObjectManager);
        }
    }
}
