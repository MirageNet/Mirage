using System;

namespace Mirror
{
    public interface IServerSceneManager
    {
        void ServerChangeScene(string newSceneName);

        void OnServerChangeScene(string newSceneName);

        void OnServerSceneChanged(string sceneName);
    }

    public interface INetworkManager : IServerSceneManager
    {
        void StartClient(Uri uri);

        void StopHost();

        void StopServer();

        void StopClient();

        void OnDestroy();

        void OnServerReady(INetworkConnection conn);

        void OnServerRemovePlayer(INetworkConnection conn, NetworkIdentity player);
    }
}
