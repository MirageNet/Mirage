using UnityEngine;
using UnityEngine.UI;

namespace Mirage
{
    /// <summary>
    /// Component that will display the clients ping in milliseconds
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkPingDisplay")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/components/network-ping-display")]
    public class NetworkPingDisplay : MonoBehaviour
    {
        public NetworkClient Client;
        public Text NetworkPingLabelText;

        internal void Update()
        {
            if (Client.Active)
                NetworkPingLabelText.text = string.Format("{0}ms", (int)(Client.World.Time.Rtt * 1000));
        }
    }
}
