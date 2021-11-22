using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    [DisallowMultipleComponent]
    public class SceneVisibilityFactory : VisibilitySystemFactory
    {
        protected override VisibilitySystem CreateSystem(ServerObjectManager serverObjectManager)
        {
            return new SceneVisibilitySystem(serverObjectManager);
        }
    }
}
