using System;

namespace Mirror
{
    public interface IServerSceneManager
    {
        void ServerChangeScene(string newSceneName);

        void OnServerChangeScene(string newSceneName);

        void OnServerSceneChanged(string sceneName);
    }

    public interface IClientSceneManager
    {
        void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling);

        void OnClientSceneChanged(INetworkConnection conn);
    }

    public interface INetworkManager : IServerSceneManager, IClientSceneManager
    {
        void StartClient(Uri uri);

        void StopHost();

        void StopServer();

        void StopClient();

        void OnDestroy();

        void OnServerReady(INetworkConnection conn);

        void OnServerRemovePlayer(INetworkConnection conn, NetworkIdentity player);

        void OnClientNotReady(INetworkConnection conn);
    }
}
