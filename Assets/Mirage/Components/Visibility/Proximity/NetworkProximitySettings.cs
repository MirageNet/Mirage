namespace Mirage.Components
{
    public class NetworkProximitySettings : NetworkBehaviour
    {
        public ProximitySettings ProximitySettings;

        private void Start()
        {
            ProximitySettings.Identity = Identity;

            FindObjectOfType<NetworkProximityChecker>().NetworkVisibility.RegisterObject(ProximitySettings);
        }
    }
}
