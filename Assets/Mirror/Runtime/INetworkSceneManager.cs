namespace Mirror
{
    public interface INetworkSceneManager
    {
        void ChangeServerScene(string newSceneName, SceneOperation sceneOperation);

        void SetClientReady(INetworkConnection conn);
    }
}
