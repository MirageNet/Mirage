namespace Mirage.Components
{
    public class SceneVisibilitySettings : NetworkBehaviour
    {
        private SceneVisibilityFactory _networkSceneChecker;
        private SceneSettings _sceneSettings = new SceneSettings();

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStopServer.AddListener(OnStopServer);
        }

        private void OnStartServer()
        {
            _sceneSettings.Scene = gameObject.scene;

            // todo find better way to get NetworkSceneChecker, FindObjectOfType wont work with multiple Servers
            //      maybe Server.GetComponent<NetworkSceneChecker>()
            _networkSceneChecker = FindObjectOfType<SceneVisibilityFactory>();
            _networkSceneChecker.System.RegisterObject(Identity, _sceneSettings);
        }

        private void OnStopServer()
        {
            _networkSceneChecker.System.UnregisterObject(Identity);
        }
    }
}
