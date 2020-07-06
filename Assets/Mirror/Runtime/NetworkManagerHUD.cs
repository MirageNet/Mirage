// vis2k: GUILayout instead of spacey += ...; removed Update hotkeys to avoid
// confusion if someone accidentally presses one.
using System.ComponentModel;
using UnityEngine;

namespace Mirror
{
    /// <summary>
    /// An extension for the NetworkManager that displays a default HUD for controlling the network state of the game.
    /// <para>This component also shows useful internal state for the networking system in the inspector window of the editor. It allows users to view connections, networked objects, message handlers, and packet statistics. This information can be helpful when debugging networked games.</para>
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkManagerHUD")]
    [RequireComponent(typeof(NetworkHost))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkManagerHUD.html")]
    public class NetworkManagerHUD : MonoBehaviour
    {
        NetworkHost host;

        /// <summary>
        /// Whether to show the default control HUD at runtime.
        /// </summary>
        public bool showGUI = true;

        /// <summary>
        /// The horizontal offset in pixels to draw the HUD runtime GUI at.
        /// </summary>
        public int offsetX;

        /// <summary>
        /// The vertical offset in pixels to draw the HUD runtime GUI at.
        /// </summary>
        public int offsetY;

        /// <summary>
        /// The IP address we're connecting to.
        /// </summary>
        public string serverIp = "localhost";

        void Awake()
        {
            host = GetComponent<NetworkHost>();
        }

        void OnGUI()
        {
            if (!showGUI)
                return;

            GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, 215, 9999));

            if (!host.LocalClient.IsConnected && !host.Active)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
                StopButtons();
            }

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (!host.LocalClient.Active)
            {
                // Server + Client
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    if (GUILayout.Button("Host (Server + Client)"))
                    {
                        _ = host.StartHost();
                    }
                }

                // Client + IP
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Client"))
                {
                    //TODO: How to handle client only with NetworkHost?
                    //host.LocalClient.StartClient(serverIp);
                }
                serverIp = GUILayout.TextField(serverIp);
                GUILayout.EndHorizontal();

                // Server Only
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    // cant be a server in webgl build
                    GUILayout.Box("(  WebGL cannot be server  )");
                }
                else
                {
                    if (GUILayout.Button("Server Only"))
                    {
                        _ = host.ListenAsync();
                    }
                }
            }
            else
            {
                // Connecting
                GUILayout.Label("Connecting to " + serverIp + "..");
                if (GUILayout.Button("Cancel Connection Attempt"))
                {
                    //TODO: How to handle client only with NetworkHost?
                    //host.StopClient();
                }
            }
        }

        void StatusLabels()
        {
            // server / client status message
            if (host.Active)
            {
                GUILayout.Label("Server: active. Transport: " + host.transport);
            }
            if (host.LocalClient.IsConnected)
            {
                GUILayout.Label("Client: address=" + serverIp);
            }
        }

        void StopButtons()
        {
            // stop host if host mode
            if (host.Active && host.LocalClient.IsConnected)
            {
                if (GUILayout.Button("Stop Host"))
                {
                    host.StopHost();
                }
            }
            // stop client if client-only
            else if (host.LocalClient.IsConnected)
            {
                if (GUILayout.Button("Stop Client"))
                {
                    host.LocalClient.Disconnect();
                }
            }
            // stop server if server-only
            else if (host.Active)
            {
                if (GUILayout.Button("Stop Server"))
                {
                    host.Disconnect();
                }
            }
        }
    }
}
