namespace Mirage.Components
{
    public class SceneVisibilitySettings : NetworkBehaviour
    {
        private NetworkSceneChecker _networkSceneChecker;
        private SceneSettings _sceneSettings = new SceneSettings();

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStopServer.AddListener(OnStopServer);
        }

        private void OnStartServer()
        {
            _sceneSettings.Scene = gameObject.scene;
            _sceneSettings.Identity = Identity;

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
