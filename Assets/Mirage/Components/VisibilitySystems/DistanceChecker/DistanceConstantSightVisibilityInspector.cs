using UnityEngine;

namespace Mirage.Components
{
    public class DistanceConstantSightVisibilityInspector : BaseVisibilityInspector
    {
        [SerializeField] private float _sightDistnace = 10;
        [SerializeField] private float _updateInterval = 0;

        protected override void Awake()
        {
            NetworkVisibility = new DistanceConstantSightVisibility(ServerObjectManager, _sightDistnace, _updateInterval);
        }
    }
}
