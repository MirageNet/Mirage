using UnityEngine;

namespace Mirage.Components
{
    [DisallowMultipleComponent]
    public class NetworkProximitySettings : NetworkBehaviour
    {
        public ProximitySettings ProximitySettings = new ProximitySettings();

        private NetworkProximityChecker _networkProximityChecker;

        private void Awake()
        {
            ProximitySettings.Identity = Identity;

            _networkProximityChecker = FindObjectOfType<NetworkProximityChecker>();
        }

        private void OnEnable()
        {
            ProximitySettings.Identity = Identity;

            _networkProximityChecker.NetworkVisibility.RegisterObject(ProximitySettings);
        }

        private void OnDisable()
        {
            _networkProximityChecker.NetworkVisibility.RegisterObject(ProximitySettings);
        }
    }
}
