using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirror.Discovery
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkDiscoveryHUD")]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkDiscovery.html")]
    [RequireComponent(typeof(NetworkDiscovery))]
    public class NetworkDiscoveryHUD : MonoBehaviour
    {
        private readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        private Vector2 scrollViewPos = Vector2.zero;

        [FormerlySerializedAs("networkDiscovery")]
        public NetworkDiscovery NetworkDiscovery;

        [FormerlySerializedAs("networkManager")]
        public NetworkManager NetworkManager;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (NetworkDiscovery == null)
            {
                NetworkDiscovery = GetComponent<NetworkDiscovery>();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(NetworkDiscovery.OnServerFound, OnDiscoveredServer);
                UnityEditor.Undo.RecordObjects(new Object[] { this, NetworkDiscovery }, "Set NetworkDiscovery");
            }

            if (NetworkManager != null)
                return;

            NetworkManager = GetComponent<NetworkManager>();
            UnityEditor.Undo.RecordObjects(new Object[] { this }, "Set NetworkManager");
        }
#endif

        private void OnGUI()
        {
            if (NetworkManager.server.active || NetworkManager.client.active)
                return;

            if (!NetworkManager.client.isConnected && !NetworkManager.server.active && !NetworkManager.client.active)
                DrawGUI();
        }

        private void DrawGUI()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Find Servers"))
            {
                discoveredServers.Clear();
                NetworkDiscovery.StartDiscovery();
            }

            // LAN Host
            if (GUILayout.Button("Start Host"))
            {
                discoveredServers.Clear();
                NetworkManager.StartHost();
                NetworkDiscovery.AdvertiseServer();
            }

            // Dedicated server
            if (GUILayout.Button("Start Server"))
            {
                discoveredServers.Clear();
                NetworkManager.StartServer();

                NetworkDiscovery.AdvertiseServer();
            }

            GUILayout.EndHorizontal();

            // show list of found server

            GUILayout.Label($"Discovered Servers [{discoveredServers.Count}]:");

            // servers
            scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);

            foreach (ServerResponse info in discoveredServers.Values)
                if (GUILayout.Button(info.EndPoint.Address.ToString()))
                    Connect(info);

            GUILayout.EndScrollView();
        }

        private void Connect(ServerResponse info)
        {
            NetworkManager.StartClient(info.uri);
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            // Note that you can check the versioning to decide if you can connect to the server or not using this
            // method
            discoveredServers[info.serverId] = info;
        }
    }
}
