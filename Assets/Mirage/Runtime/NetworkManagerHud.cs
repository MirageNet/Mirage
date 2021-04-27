using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Mirage
{
    public class NetworkManagerHud : MonoBehaviour
    {
        public NetworkManager NetworkManager;
        public string NetworkAddress = "localhost";

        [Header("Prefab Canvas Elements")]
        public InputField NetworkAddressInput;
        public GameObject OfflineGO;
        public GameObject OnlineGO;
        public Text StatusLabel;

        private void Start()
        {
            DontDestroyOnLoad(transform.root.gameObject);
            Application.runInBackground = true;
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
            NetworkManager.Server.StartHost(NetworkManager.Client).Forget();
            OnlineSetActive();
        }

        public void StartServerOnlyButtonHandler()
        {
            SetLabel("Server Mode");
            NetworkManager.Server.ListenAsync().Forget();
            OnlineSetActive();
        }

        public void StartClientButtonHandler()
        {
            SetLabel("Client Mode");
            NetworkManager.Client.ConnectAsync(NetworkAddress).Forget();
            OnlineSetActive();
        }

        public void StopButtonHandler()
        {
            SetLabel(string.Empty);
            NetworkManager.Server.StopHost();
            NetworkManager.Client.Disconnect();
            OfflineSetActive();
        }

        public void OnNetworkAddressInputUpdate()
        {
            NetworkAddress = NetworkAddressInput.text;
        }
    }
}
