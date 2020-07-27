namespace Mirror
{
    public interface IClientSceneManager
    {
        void SetClientReady();
    }


    //These need to be moved out of NS. Problem with setting ready in: AddPlayerForConnection
    public interface IServerSceneManager
    {
        void SetClientReady(INetworkConnection conn);

        void SetAllClientsNotReady();

        void SetClientNotReady(INetworkConnection conn);
    }
    
    public interface INetworkSceneManager : IClientSceneManager, IServerSceneManager
    {
        void ChangeServerScene(string newSceneName, SceneOperation sceneOperation = SceneOperation.Normal);
    }
}
