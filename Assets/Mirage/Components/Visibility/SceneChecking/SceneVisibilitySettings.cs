using UnityEngine;

namespace Mirage.Components
{
    // todo find better name
    public class SceneVisibilitySettings : NetworkBehaviour
    {
        private SceneVisibilityFactory _networkSceneChecker;

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStopServer.AddListener(OnStopServer);
        }

        private void OnStartServer()
        {
            // todo find better way to get NetworkSceneChecker, FindObjectOfType wont work with multiple Servers
            //      maybe Server.GetComponent<NetworkSceneChecker>()
            _networkSceneChecker = FindObjectOfType<SceneVisibilityFactory>();
            Debug.Assert(_networkSceneChecker != null, "Could not found SceneVisibilityFactory");
            _networkSceneChecker.System.RegisterObject(Identity, gameObject.scene);
        }

        private void OnStopServer()
        {
            _networkSceneChecker.System.UnregisterObject(Identity);
        }
    }
}
