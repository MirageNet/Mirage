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
            var managerGo = new GameObject("NetworkManager", typeof(NetworkManager), typeof(LogSettings));
            var serverGo = AddChild(managerGo, "Server", typeof(NetworkServer), typeof(ServerObjectManager));
            var clientGo = AddChild(managerGo, "Client", typeof(NetworkClient), typeof(ClientObjectManager));
            var authGo = AddChild(managerGo, "Authentication");
            var sceneGo = AddChild(managerGo, "Scene", typeof(NetworkSceneManager));
            var characterGo = AddChild(managerGo, "Character", typeof(CharacterSpawner));
            var socketGo = AddChild(managerGo, "Socket", typeof(T));

            var nsm = sceneGo.GetComponent<NetworkSceneManager>();

            var networkClient = clientGo.GetComponent<NetworkClient>();
            networkClient.SocketFactory = socketGo.GetComponent<T>();

            var networkServer = serverGo.GetComponent<NetworkServer>();
            networkServer.SocketFactory = socketGo.GetComponent<T>();

            var serverObjectManager = serverGo.GetComponent<ServerObjectManager>();
            serverObjectManager.Server = networkServer;
            nsm.ServerObjectManager = serverObjectManager;

            var clientObjectManager = serverGo.GetComponent<ClientObjectManager>();
            clientObjectManager.Client = networkClient;
            clientObjectManager.NetworkSceneManager = nsm;

            var networkManager = managerGo.GetComponent<NetworkManager>();
            networkManager.Client = networkClient;
            networkManager.Server = networkServer;
            networkManager.ServerObjectManager = serverObjectManager;
            networkManager.ClientObjectManager = clientObjectManager;
            networkManager.NetworkSceneManager = nsm;

            var playerSpawner = characterGo.GetComponent<CharacterSpawner>();
            playerSpawner.Client = networkClient;
            playerSpawner.Server = networkServer;
            playerSpawner.SceneManager = nsm;
            playerSpawner.ServerObjectManager = serverObjectManager;
            playerSpawner.ClientObjectManager = clientObjectManager;

            nsm.Client = networkClient;
            nsm.Server = networkServer;
            return managerGo;
        }

        private static GameObject AddChild(GameObject parent, string name, params Type[] types)
        {
            var serverGo = new GameObject(name, types);
            serverGo.transform.parent = parent.transform;
            return serverGo;
        }

        /// <summary>
        /// Creates a new game object with NetworkManager and other network components attached, Including UdpSocketFactory
        /// </summary>
        /// <returns></returns>
        [MenuItem("GameObject/Network/NetworkIdentity", priority = 7)]
        public static GameObject CreateNetworkIdentity()
        {
            var components = new Type[]
            {
                typeof(NetworkIdentity),
            };
            var go = new GameObject("NetworkIdentity", components);
            return go;
        }
    }
}
