namespace Mirror
{
    public interface IClientSceneManager
    {
        void SetClientReady();
    }

    public interface IServerSceneManager
    {
        void SetClientReady(INetworkConnection conn);

        void SetAllClientsNotReady();

        void SetClientNotReady(INetworkConnection conn);
    }

    public interface INetworkSceneManager : IClientSceneManager
    {
        void ChangeServerScene(string newSceneName, SceneOperation sceneOperation = SceneOperation.Normal);
    }
}
