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
            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStopServer.AddListener(OnStopServer);
        }

        private void OnStartServer()
        {
            ProximitySettings.Identity = Identity;

            // todo find better way to get NetworkSceneChecker, FindObjectOfType wont work with multiple Servers
            //      maybe Server.GetComponent<NetworkSceneChecker>()
            _networkProximityChecker = FindObjectOfType<NetworkProximityChecker>();
            _networkProximityChecker.NetworkVisibility.RegisterObject(ProximitySettings);
        }

        private void OnStopServer()
        {
            _networkProximityChecker.NetworkVisibility.RegisterObject(ProximitySettings);
        }
    }
}
