using UnityEngine;

namespace Mirage.Components
{
    public class NetworkProximityChecker : BaseVisibilityInspector
    {

        /// <summary>
        /// The maximum range that objects will be visible at.
        /// </summary>
        [Tooltip("The maximum range that objects will be visible at.")]
        public int VisibilityRange = 10;

        /// <summary>
        /// How often (in seconds) that this object should update the list of observers that can see it.
        /// </summary>
        [Tooltip("How often (in seconds) that this object should update the list of observers that can see it.")]
        public float VisibilityUpdateInterval = 1;

        protected override void Start()
        {
            NetworkVisibility = new NetworkProximityCheckerVisibility(ServerObjectManager, VisibilityRange, VisibilityUpdateInterval);

            base.Start();
        }
    }
}
