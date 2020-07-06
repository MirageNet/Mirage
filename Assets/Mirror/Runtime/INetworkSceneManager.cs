namespace Mirror
{
    public interface INetworkSceneManager
    {
        void ClientSceneMessage(INetworkConnection conn, SceneMessage msg);

        void ChangeServerScene(string newSceneName);

        void Ready(INetworkConnection conn);
    }
}
