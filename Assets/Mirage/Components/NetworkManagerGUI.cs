using System;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage
{
    [DisallowMultipleComponent]
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

        private void Reset()
        {
            // try to automatically add NetworkManager when this component is added
            NetworkManager = GetComponent<NetworkManager>();
        }

        private void Awake()
        {
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
            // Depending how the scene is set up, references can be broken. If the NM reference is null/missing, then
            // short-circuit here to prevent log spam.
            if (NetworkManager == null)
            {
                return;
            }

            // Draw the window on the screen.
            GUIUtility.ScaleAroundPivot(Vector2.one * Scale, GetPivotFromAnchor(GUIAnchor));
            GUI.Window(WINDOW_ID, GetRectFromAnchor(GUIAnchor, WINDOW_HEIGHT), DrawNetworkManagerWindow, "Mirage Networking");
        }


        /// <summary>
        /// Returns a Rect from the anchored position.
        /// </summary>
        /// <param name="anchor">Which part of the screen is this located?</param>
        /// <param name="height">How tall the Rect will be</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets a Pivot from the Anchored position.
        /// </summary>
        /// <param name="anchor">Which part of the screen is this located?</param>
        /// <returns>Vector2 with the pivot X and Y.</returns>
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

        /// <summary>
        /// Simply throws an exception if the NetworkManager reference was null.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        internal void CheckNetworkManagerReference()
        {
            if (NetworkManager == null)
            {
                throw new NullReferenceException("NetworkManager reference is NULL. Fix it, then try again.");
            }
        }

        /// <summary>
        /// Called when user clicks the Host button. It starts Host (Server + Client) mode.
        /// </summary>
        private void ClickHost()
        {
            CheckNetworkManagerReference();
            NetworkManager.Server.StartServer(NetworkManager.Client);
        }

        /// <summary>
        /// Called when user clicks Start Server button. Starts only the server, no server-mode client.
        /// </summary>
        private void ClickServerOnly()
        {
            CheckNetworkManagerReference();
            NetworkManager.Server.StartServer();
        }

        /// <summary>
        /// Called when the user clicks Connect. It connects to the server.
        /// </summary>
        private void ClickClient()
        {
            CheckNetworkManagerReference();
            NetworkManager.Client.Connect(NetworkAddress);
        }

        /// <summary>
        /// Draws the NetworkManager control window.
        /// </summary>
        /// <param name="id">Parameter reference passed from GUI.DrawWindow(int)</param>
        internal void DrawNetworkManagerWindow(int id)
        {
            // If the server is active...
            if (NetworkManager.Server.Active)
            {
                DrawServerControls();
            }
            else if (NetworkManager.Client.Active)
            {
                // The client is active.
                DrawClientControls();
            }
            else
            {
                // Nothing is active.
                DrawIdleControls();
            }
        }

        /// <summary>
        /// This draws the idle controls. Idle in the sense that
        /// neither the Server or Client is running.
        /// </summary>
        internal void DrawIdleControls()
        {
            // Begin the idle controls.
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            // Server mode controls (not available on WebGL).
#if !UNITY_WEBGL            
            GUILayout.Label("Server Mode");

            if (GUILayout.Button("Server Only", GUILayout.Height(WINDOW_BUTTON_HEIGHT)))
            {
                ClickServerOnly();
            }

            if (GUILayout.Button("Host Mode (Server + Client)", GUILayout.Height(WINDOW_BUTTON_HEIGHT)))
            {
                ClickHost();
            }
#endif
            // Client mode
            GUILayout.Label("Client Mode");
            NetworkAddress = GUILayout.TextField(NetworkAddress);

            if (GUILayout.Button("Connect", GUILayout.Height(WINDOW_BUTTON_HEIGHT)))
            {
                ClickClient();
            }

            // Spacer
            GUILayout.FlexibleSpace();

            // Versioning
            GUILayout.Label($"Mirage Networking v{Version.Current}\n" +
                $"Unity v{Application.unityVersion}");
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws controls and labels to give information about
        /// the server instance that's running.
        /// </summary>
        internal void DrawServerControls()
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            GUILayout.Label("Server is active.");

            if (NetworkManager.Client.IsConnected)
                GUILayout.Label("Host mode is active.");

            if (GUILayout.Button(NetworkManager.Client.IsConnected ? "Stop Host Mode" : "Stop Server", GUILayout.Height(WINDOW_BUTTON_HEIGHT)))
            {
                NetworkManager.Server.Stop();
            }

            // Additional texts.
            if (NetworkManager.Server.SocketFactory is IHasAddress hasAddress)
                GUILayout.Label($"Listen Address: {hasAddress.Address}");

            if (NetworkManager.Server.SocketFactory is IHasPort hasPort)
                GUILayout.Label($"Listen Port: {hasPort.Port}");

            GUILayout.Label($"Backend: {NetworkManager.Server.SocketFactory.GetType().Name}");

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws controls and labels to give information about the client instance
        /// that's running.
        /// </summary>
        internal void DrawClientControls()
        {
            double latency;

            GUILayout.BeginVertical();

            GUILayout.Label("Client mode is active.");

            if (NetworkManager.Client.IsConnected)
            {
                latency = NetworkManager.Client.World.Time.Rtt;

                if (GUILayout.Button("Stop Client", GUILayout.Height(WINDOW_BUTTON_HEIGHT)))
                {
                    NetworkManager.Client.Disconnect();
                }

                if (NetworkManager.Client.SocketFactory is IHasAddress hasAddress)
                    GUILayout.Label($"Server: {hasAddress.Address}");

                if (NetworkManager.Client.SocketFactory is IHasPort hasPort)
                    GUILayout.Label($"Port: {hasPort.Port}");

                // Round this to an integer as something like 7.0101025 ms looks incredibly odd.
                // We have to cast down to a float because Unity RoundToInt doesn't allow double -> int
                GUILayout.Label($"Latency: {Mathf.RoundToInt((float)(latency * 1000))} ms");
            }
            else
            {
                // No controls, just say we're connecting.
                GUILayout.Label($"Connecting to '{NetworkAddress}'.");
            }

            GUILayout.Label($"Backend: {NetworkManager.Client.SocketFactory.GetType().Name}");
            GUILayout.EndVertical();
        }

        // Constants.
        private const int WIDTH = 200;
        private const int PADDING_X = 10;
        private const int PADDING_Y = 10;

        private const int WINDOW_ID = 0;
        private const int WINDOW_HEIGHT = 240;
        private const int WINDOW_BUTTON_HEIGHT = 30;
    }
}
