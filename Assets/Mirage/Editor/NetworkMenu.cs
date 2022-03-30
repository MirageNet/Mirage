using System;
using Mirage.Logging;
using Mirage.SocketLayer;
using Mirage.Sockets.Udp;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    public static class NetworkMenu
    {
        /// <summary>
        /// Creates a new game object with NetworkManager and other network components attached, Including UdpSocketFactory
        /// </summary>
        /// <returns></returns>
        [MenuItem("GameObject/Network/NetworkManager", priority = 7)]
        public static GameObject CreateNetworkManager()
        {
            return CreateNetworkManager<UdpSocketFactory>();
        }

        /// <summary>
        /// Creates a new game object with NetworkManager and other network components attached, Including Socket factory that is given as generic arg
        /// </summary>
        /// <remarks>
        /// This methods can be used by other socketfactories to create networkmanager with their setup
        /// </remarks>
        /// <returns></returns>
        public static GameObject CreateNetworkManager<T>() where T : SocketFactory
        {
            var components = new Type[]
            {
                typeof(NetworkManager),
                typeof(NetworkServer),
                typeof(NetworkClient),
                typeof(NetworkSceneManager),
                typeof(ServerObjectManager),
                typeof(ClientObjectManager),
                typeof(CharacterSpawner),
                typeof(T),
                typeof(LogSettings)
            };
            var go = new GameObject("NetworkManager", components);

            T socketFactory = go.GetComponent<T>();
            NetworkSceneManager nsm = go.GetComponent<NetworkSceneManager>();

            NetworkClient networkClient = go.GetComponent<NetworkClient>();
            networkClient.SocketFactory = socketFactory;

            NetworkServer networkServer = go.GetComponent<NetworkServer>();
            networkServer.SocketFactory = socketFactory;

            ServerObjectManager serverObjectManager = go.GetComponent<ServerObjectManager>();
            serverObjectManager.Server = networkServer;
            serverObjectManager.NetworkSceneManager = nsm;

            ClientObjectManager clientObjectManager = go.GetComponent<ClientObjectManager>();
            clientObjectManager.Client = networkClient;
            clientObjectManager.NetworkSceneManager = nsm;

            NetworkManager networkManager = go.GetComponent<NetworkManager>();
            networkManager.Client = networkClient;
            networkManager.Server = networkServer;
            networkManager.ServerObjectManager = serverObjectManager;
            networkManager.ClientObjectManager = clientObjectManager;
            networkManager.NetworkSceneManager = nsm;

            CharacterSpawner playerSpawner = go.GetComponent<CharacterSpawner>();
            playerSpawner.Client = networkClient;
            playerSpawner.Server = networkServer;
            playerSpawner.SceneManager = nsm;
            playerSpawner.ServerObjectManager = serverObjectManager;
            playerSpawner.ClientObjectManager = clientObjectManager;

            nsm.Client = networkClient;
            nsm.Server = networkServer;
            return go;
        }
    }
}
