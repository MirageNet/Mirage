using UnityEngine;

namespace Mirage.Components
{
    [DisallowMultipleComponent]
    public class NetworkProximitySettings : NetworkBehaviour
    {
        public ProximitySettings ProximitySettings = new ProximitySettings();

        private void Start()
        {
            ProximitySettings.Identity = Identity;

            FindObjectOfType<NetworkProximityChecker>().NetworkVisibility.RegisterObject(ProximitySettings);
        }
    }
}
