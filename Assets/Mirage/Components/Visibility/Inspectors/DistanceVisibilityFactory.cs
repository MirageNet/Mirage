using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    [DisallowMultipleComponent]
    public class DistanceVisibilityFactory : VisibilitySystemFactory
    {
        /// <summary>
        /// How often (in seconds) that this object should update the list of observers that can see it.
        /// </summary>
        [Tooltip("How often (in seconds) that this object should update the list of observers that can see it.")]
        public float VisibilityUpdateInterval = 1;

        protected override VisibilitySystem CreateSystem(ServerObjectManager serverObjectManager)
        {
            return new DistanceVisibilitySystem(serverObjectManager, VisibilityUpdateInterval);
        }
    }
}
