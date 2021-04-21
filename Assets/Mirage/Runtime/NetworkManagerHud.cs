using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Mirage
{
    public class NetworkManagerHud : MonoBehaviour
    { 
        public NetworkManager NetworkManager;
        public string NetworkAddress = "localhost";

        [Header("IMGUI Settings")]
        public bool UseIMGUI = false;
        [Range(0.01f, 10f)]
        public float Scale = 1f;
        public TextAnchor GUIAnchor = TextAnchor.UpperLeft;

        [Header("Prefab Canvas Elements")]
        public InputField NetworkAddressInput;
        public GameObject OfflineGO;
        public GameObject OnlineGO;
        public Text StatusLabel;
        string labelText;

        private void Start()
        { 
            DontDestroyOnLoad(transform.root.gameObject);
            Application.runInBackground = true;
        }

        private void Update()
        {
            if (StatusLabel) StatusLabel.text = labelText;
        }

        internal void OnlineSetActive()
        {
            OfflineGO.SetActive(false);
            OnlineGO.SetActive(true);
        }

        internal void OfflineSetActive()
        {
            OfflineGO.SetActive(true);
            OnlineGO.SetActive(false);
        }

        public void StartHostButtonHandler()
        {
            labelText = "Host Mode";
            NetworkManager.Server.StartHost(NetworkManager.Client).Forget();
            OnlineSetActive();
        }

        public void StartServerOnlyButtonHandler()
        {
            labelText = "Server Mode";
            NetworkManager.Server.ListenAsync().Forget();
            OnlineSetActive();
        }

        public void StartClientButtonHandler()
        {
            labelText = "Client Mode";
            NetworkManager.Client.ConnectAsync(NetworkAddress).Forget();
            OnlineSetActive();
        }

        public void StopButtonHandler()
        {
            labelText = string.Empty;
            NetworkManager.Server.StopHost();
            NetworkManager.Client.Disconnect();
            OfflineSetActive();
        }

        public void OnNetworkAddressInputUpdate()
        {
            NetworkAddress = NetworkAddressInput.text;
        }

        private void OnGUI()
        {
            if (!UseIMGUI)
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
                GUILayout.Label($"Transport: {NetworkManager.Server.Transport.GetType().Name}");

                if (NetworkManager.Client.IsConnected)
                {
                    if (GUILayout.Button("Stop Host"))
                    {
                        NetworkManager.Server.StopHost();
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Server"))
                    {
                        NetworkManager.Server.Disconnect();
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
            else if(NetworkManager.Client.Active)
            {
                GUILayout.Label($"Connecting to {NetworkAddress}...");
                //TODO: Implement cancel button when it's possible.
            }
           
            GUILayout.EndArea();
        }

        private const int WIDTH = 200;
        private const int PADDING_X = 10;
        private const int PADDING_Y = 10;

        private Rect GetRectFromAnchor(TextAnchor anchor, int height)
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

        private Vector2 GetPivotFromAnchor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return Vector2.zero;
                case TextAnchor.UpperCenter:
                    return new Vector2(Screen.width / 2, 0f);
                case TextAnchor.UpperRight:
                    return new Vector2(Screen.width, 0f);
                case TextAnchor.MiddleLeft:
                    return new Vector2(0f, Screen.height / 2);
                case TextAnchor.MiddleCenter:
                    return new Vector2(Screen.width / 2, Screen.height / 2);
                case TextAnchor.MiddleRight:
                    return new Vector2(Screen.width, Screen.height / 2);
                case TextAnchor.LowerLeft:
                    return new Vector2(0f, Screen.height);
                case TextAnchor.LowerCenter:
                    return new Vector2(Screen.width / 2, Screen.height);
                default: // Lower right
                    return new Vector2(Screen.width, Screen.height);
            }
        }

        private void ClickHost()
        {
            NetworkManager.Server.StartHost(NetworkManager.Client).Forget();
        }

        private void ClickServerOnly()
        {
            NetworkManager.Server.ListenAsync().Forget();
        }

        private void ClickClient()
        {
            NetworkManager.Client.ConnectAsync(NetworkAddress);
        }
    }
}
