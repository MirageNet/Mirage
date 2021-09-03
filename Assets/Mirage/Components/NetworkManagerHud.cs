using UnityEngine;
using UnityEngine.UI;

namespace Mirage
{
    public class NetworkManagerHud : MonoBehaviour
    {
        public NetworkManager NetworkManager;
        public string NetworkAddress = "localhost";
        public bool DontDestroy = true;

        [Header("Prefab Canvas Elements")]
        public InputField NetworkAddressInput;
        public GameObject OfflineGO;
        public GameObject OnlineGO;
        public Text StatusLabel;

        private void Start()
        {
            if (DontDestroy)
                DontDestroyOnLoad(transform.root.gameObject);

            Application.runInBackground = true;

            // return to offset menu when server or client is stopped
            NetworkManager.Server?.Stopped.AddListener(OfflineSetActive);
            NetworkManager.Client?.Disconnected.AddListener(_ => OfflineSetActive());
        }

        void SetLabel(string value)
        {
            if (StatusLabel) StatusLabel.text = value;
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
            SetLabel("Host Mode");
            NetworkManager.Server.StartServer(NetworkManager.Client);
            OnlineSetActive();
        }

        public void StartServerOnlyButtonHandler()
        {
            SetLabel("Server Mode");
            NetworkManager.Server.StartServer();
            OnlineSetActive();
        }

        public void StartClientButtonHandler()
        {
            SetLabel("Client Mode");
            NetworkManager.Client.Connect(NetworkAddress);
            OnlineSetActive();
        }

        public void StopButtonHandler()
        {
            SetLabel(string.Empty);

            if (NetworkManager.Server.Active)
                NetworkManager.Server.Stop();
            if (NetworkManager.Client.Active)
                NetworkManager.Client.Disconnect();
            OfflineSetActive();
        }

        public void OnNetworkAddressInputUpdate()
        {
            NetworkAddress = NetworkAddressInput.text;
        }
    }
}
