using UnityEngine;

namespace Mirage.Components
{
    [DisallowMultipleComponent]
    public class NetworkProximitySettings : NetworkBehaviour
    {
        public ProximitySettings ProximitySettings = new ProximitySettings();

        private DistanceVisibilityFactory _networkProximityChecker;

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStopServer.AddListener(OnStopServer);
        }

        private void OnStartServer()
        {
            // todo find better way to get NetworkSceneChecker, FindObjectOfType wont work with multiple Servers
            //      maybe Server.GetComponent<NetworkSceneChecker>()
            _networkProximityChecker = FindObjectOfType<DistanceVisibilityFactory>();
            _networkProximityChecker.System.RegisterObject(Identity, ProximitySettings);
        }

        private void OnStopServer()
        {
            _networkProximityChecker.System.UnregisterObject(Identity);
        }
    }
}
