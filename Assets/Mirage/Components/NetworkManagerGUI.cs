using System;
using UnityEngine;

namespace Mirage
{
    public class NetworkManagerGUI : MonoBehaviour
    {
        [Tooltip("If enabled, we'll automatically reference NetworkManager in script's initialization.\nIf you've already set the NetworkManager field, auto configuring will not be performed.")]
        public bool AutoConfigureNetworkManager = false;
        public NetworkManager NetworkManager;

        public string NetworkAddress = "localhost";

        [Range(0.01f, 10f)]
        public float Scale = 1f;
        public TextAnchor GUIAnchor = TextAnchor.UpperLeft;

        private void Awake()
        {
            // Coburn, 2023-01-29: 
            // If automatic configuration of NetworkManager is enabled, then attempt to grab the
            // NetworkManager that this script is attached to.
            if (AutoConfigureNetworkManager && NetworkManager == null)
            {
                NetworkManager = GetComponent<NetworkManager>();

                // Is this STILL null? Then we throw in the towel and go home.
                if (NetworkManager == null)
                {
                    throw new ArgumentNullException("NetworkManager", $"You requested automatic configuration for the NetworkManagerGUI component on '{gameObject.name}'" +
                        $" but one could not be found. Either disable automatic configuration or ensure this script is on a GameObject with a Mirage NetworkManager.");
                }
            }
        }

        private void OnGUI()
        {
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

        private const int WIDTH = 200;
        private const int PADDING_X = 10;
        private const int PADDING_Y = 10;

        private static Rect GetRectFromAnchor(TextAnchor anchor, int height)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return new Rect(PADDING_X, PADDING_Y, WIDTH, height);
                case TextAnchor.UpperCenter:
                    return new Rect(Screen.width / 2 - (WIDTH / 2), PADDING_Y, WIDTH, height);
                case TextAnchor.UpperRight:
                    return new Rect(Screen.width - (WIDTH + PADDING_X), PADDING_Y, WIDTH, height);
                case TextAnchor.MiddleLeft:
                    return new Rect(PADDING_X, Screen.height / 2 - (height / 2), WIDTH, height);
                case TextAnchor.MiddleCenter:
                    return new Rect(Screen.width / 2 - (WIDTH / 2), Screen.height / 2 - (height / 2), WIDTH, height);
                case TextAnchor.MiddleRight:
                    return new Rect(Screen.width - (WIDTH + PADDING_X), Screen.height / 2 - (height / 2), WIDTH, height);
                case TextAnchor.LowerLeft:
                    return new Rect(PADDING_X, Screen.height - (height + PADDING_Y), WIDTH, height);
                case TextAnchor.LowerCenter:
                    return new Rect(Screen.width / 2 - (WIDTH / 2), Screen.height - (height + PADDING_Y), WIDTH, height);
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
    }
}
