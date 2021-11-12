namespace Mirage.Components
{
    public class SceneVisibilitySettings : NetworkBehaviour
    {
        private NetworkSceneChecker _networkSceneChecker;
        private SceneSettings _sceneSettings;

        private void Awake()
        {
            _networkSceneChecker = FindObjectOfType<NetworkSceneChecker>();
        }

        private void OnEnable()
        {
            _sceneSettings = new SceneSettings { Scene = gameObject.scene, Identity = Identity };

            _networkSceneChecker.NetworkVisibility.RegisterObject(_sceneSettings);
        }

        private void OnDisable()
        {
            _networkSceneChecker.NetworkVisibility.UnRegisterObject(_sceneSettings);
        }
    }
}
