using System;
using UnityEngine;

namespace Mirage
{
    [DisallowMultipleComponents]
    public class NetworkManagerGUI : MonoBehaviour
    {
        [Header("Defaults")]
        [Tooltip("This is the default string displayed in the client address text box.")]
        public string NetworkAddress = "localhost";

        [Header("References")]
        [Tooltip("If enabled, we'll automatically reference NetworkManager in script's initialization.\nIf you've already set the NetworkManager field, auto configuring will not be performed.")]
        public bool AutoConfigureNetworkManager = true;

        [Tooltip("The value in this property field (if set) will not be overwritten if Auto Configuration is enabled.")]
        public NetworkManager NetworkManager;

        [Header("Cosmetics")]
        [Range(0.01f, 10f), Tooltip("Adjusts scale of the GUI elements. 1x is normal size. 2x might be sane for big screens. <= 0.5x or > 2x is silly.")]
        public float Scale = 1f;

        [Tooltip("Select where you want the NetworkManagerGUI elements to be located on the screen.")]
        public TextAnchor GUIAnchor = TextAnchor.UpperLeft;

        private void Awake()
        {
            // Coburn, 2023-02-02: 
            // If automatic configuration of NetworkManager is enabled, then attempt to grab the
            // NetworkManager that this script is attached to.
            if (AutoConfigureNetworkManager)
            {
                // Ensure this is null before doing anything. If not, return early.
                if (NetworkManager != null)
                {
                    return;
                }

                // Attempt to get the NetworkManager component.
                NetworkManager = GetComponent<NetworkManager>();

                // Is this STILL null? Then we throw in the towel and go home.
                if (NetworkManager == null)
                {
                    throw new InvalidOperationException($"You requested automatic configuration for the NetworkManagerGUI component on '{gameObject.name}'" +
                        $" but one could not be found. Either disable automatic configuration or ensure this script is on a GameObject with a Mirage NetworkManager.");
                }
            }
        }

        private void OnGUI()
        {
            // Coburn, 2023-02-02: Apparently NMGUI can/may lose reference to NetworkManager for reasons unknown
            // (maybe due to being in and out of DDOL and scene changes?). To prevent a NRE being spewed every OnGUI
            // update, short-circuit here to prevent log spam.
            if (NetworkManager == null)
            {
                return;
            }

            GUIUtility.ScaleAroundPivot(Vector2.one * Scale, GetPivotFromAnchor(GUIAnchor));

            if (!NetworkManager.Server.Active && !NetworkManager.Client.Active)
            {
                StartButtons(GetRectFromAnchor(GUIAnchor, 71));
            }
            else
            {
                StatusLabels(GetRectFromAnchor(GUIAnchor, 100));
            }
        }

        private void StartButtons(Rect position)
        {
            GUILayout.BeginArea(position);

            // Coburn, 2023-02-02, possible TODO: Remove the server buttons for WebGL since it can't be used as server
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                GUILayout.Box("WebGL cannot host");
            }
            else
            {
                if (GUILayout.Button("Host (Server + Client)"))
                {
                    ClickHost();
                }
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Client"))
            {
                ClickClient();
            }
            NetworkAddress = GUILayout.TextField(NetworkAddress);

            GUILayout.EndHorizontal();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                GUILayout.Box("WebGL cannot be a server");
            }
            else
            {
                if (GUILayout.Button("Server Only"))
                {
                    ClickServerOnly();
                }
            }

            GUILayout.EndArea();
        }

        private void StatusLabels(Rect position)
        {
            GUILayout.BeginArea(position);

            if (NetworkManager.Server.Active)
            {
                GUILayout.Label("Server: active");
                GUILayout.Label($"Socket: {NetworkManager.Server.SocketFactory.GetType().Name}");

                if (NetworkManager.Client.IsConnected)
                {
                    if (GUILayout.Button("Stop Host"))
                    {
                        NetworkManager.Server.Stop();
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Server"))
                    {
                        NetworkManager.Server.Stop();
                    }
                }
            }

            if (NetworkManager.Client.IsConnected)
            {
                GUILayout.Label($"Client: address={NetworkAddress}");

                if (GUILayout.Button("Stop Client"))
                {
                    NetworkManager.Client.Disconnect();
                }
            }
            else if (NetworkManager.Client.Active)
            {
                GUILayout.Label($"Connecting to {NetworkAddress}...");
                //TODO: Implement cancel button when it's possible.
            }

            GUILayout.EndArea();
        }

        private static Rect GetRectFromAnchor(TextAnchor anchor, int height)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return new Rect(PADDING_X, PADDING_Y, WIDTH, height);
                case TextAnchor.UpperCenter:
                    return new Rect((Screen.width / 2) - (WIDTH / 2), PADDING_Y, WIDTH, height);
                case TextAnchor.UpperRight:
                    return new Rect(Screen.width - (WIDTH + PADDING_X), PADDING_Y, WIDTH, height);
                case TextAnchor.MiddleLeft:
                    return new Rect(PADDING_X, (Screen.height / 2) - (height / 2), WIDTH, height);
                case TextAnchor.MiddleCenter:
                    return new Rect((Screen.width / 2) - (WIDTH / 2), (Screen.height / 2) - (height / 2), WIDTH, height);
                case TextAnchor.MiddleRight:
                    return new Rect(Screen.width - (WIDTH + PADDING_X), (Screen.height / 2) - (height / 2), WIDTH, height);
                case TextAnchor.LowerLeft:
                    return new Rect(PADDING_X, Screen.height - (height + PADDING_Y), WIDTH, height);
                case TextAnchor.LowerCenter:
                    return new Rect((Screen.width / 2) - (WIDTH / 2), Screen.height - (height + PADDING_Y), WIDTH, height);
                default: // Lower right
                    return new Rect(Screen.width - (WIDTH + PADDING_X), Screen.height - (height + PADDING_Y), WIDTH, height);
            }
        }

        private static Vector2 GetPivotFromAnchor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return Vector2.zero;
                case TextAnchor.UpperCenter:
                    return new Vector2(Screen.width / 2f, 0f);
                case TextAnchor.UpperRight:
                    return new Vector2(Screen.width, 0f);
                case TextAnchor.MiddleLeft:
                    return new Vector2(0f, Screen.height / 2f);
                case TextAnchor.MiddleCenter:
                    return new Vector2(Screen.width / 2f, Screen.height / 2f);
                case TextAnchor.MiddleRight:
                    return new Vector2(Screen.width, Screen.height / 2f);
                case TextAnchor.LowerLeft:
                    return new Vector2(0f, Screen.height);
                case TextAnchor.LowerCenter:
                    return new Vector2(Screen.width / 2f, Screen.height);
                default: // Lower right
                    return new Vector2(Screen.width, Screen.height);
            }
        }

        private void ClickHost()
        {
            NetworkManager.Server.StartServer(NetworkManager.Client);
        }

        private void ClickServerOnly()
        {
            NetworkManager.Server.StartServer();
        }

        private void ClickClient()
        {
            NetworkManager.Client.Connect(NetworkAddress);
        }

        private const int WIDTH = 200;
        private const int PADDING_X = 10;
        private const int PADDING_Y = 10;
    }
}
