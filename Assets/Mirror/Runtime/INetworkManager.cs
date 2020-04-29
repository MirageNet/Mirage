using System;

namespace Mirror
{
    public interface ServerSceneManager
    {
        void ServerChangeScene(string newSceneName);

        void OnServerChangeScene(string newSceneName);

        void OnServerSceneChanged(string sceneName);
    }

    public interface ClientSceneManager
    {
        void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling);

        void OnClientSceneChanged(INetworkConnection conn);
    }

    public interface INetworkManager : ServerSceneManager, ClientSceneManager
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
