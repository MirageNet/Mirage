namespace Mirage.Components
{
    public class SceneVisibilitySettings : NetworkBehaviour
    {
        private NetworkSceneChecker _networkSceneChecker;
        private SceneSettings _sceneSettings;

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStopServer.AddListener(OnStopServer);
        }

        private void OnStartServer()
        {
            _sceneSettings = new SceneSettings { Scene = gameObject.scene, Identity = Identity };

            // todo find better way to get NetworkSceneChecker, FindObjectOfType wont work with multiple Servers
            //      maybe Server.GetComponent<NetworkSceneChecker>()
            _networkSceneChecker = FindObjectOfType<NetworkSceneChecker>();
            _networkSceneChecker.NetworkVisibility.RegisterObject(_sceneSettings);
        }

        private void OnStopServer()
        {
            _networkSceneChecker.NetworkVisibility.UnRegisterObject(_sceneSettings);
        }
    }
}
