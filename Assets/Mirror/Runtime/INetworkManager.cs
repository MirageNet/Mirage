using System;

namespace Mirror
{
    public interface INetworkManager
    {
        void StartClient(Uri uri);

        void StopHost();

        void StopServer();

        void StopClient();

        void OnDestroy();

        void ServerChangeScene(string newSceneName);

        void OnServerReady(INetworkConnection conn);

        void OnServerRemovePlayer(INetworkConnection conn, NetworkIdentity player);

        void OnServerChangeScene(string newSceneName);

        void OnServerSceneChanged(string sceneName);

        void OnClientNotReady(INetworkConnection conn);

        void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling);

        void OnClientSceneChanged(INetworkConnection conn);
    }
}
