using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Examples
{
    public class NetworkManagerHUD : MonoBehaviour
    {
        public NetworkManager NetworkManager;

        [Header("Prefab Canvas Elements")]
        public InputField NetworkAddressInput;
        public GameObject OfflineGO;
        public GameObject OnlineGO;
        public Text StatusLabel;

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
            _ = NetworkManager.StartHost();
            OnlineSetActive();
        }

        public void StartServerOnlyButtonHandler()
        {
            _ = NetworkManager.StartServer();
            OnlineSetActive();
        }

        public void StartClientButtonHandler()
        {
            _ = NetworkManager.StartClient(NetworkAddressInput.text);
            OnlineSetActive();
        }

        public void StopButtonHandler()
        {
            NetworkManager.StopHost();
            OfflineSetActive();
        }
    }
}
