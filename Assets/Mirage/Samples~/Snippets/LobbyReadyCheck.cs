using System.Linq;
using Mirage.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Mirage.Snippets.LobbyReadyCheck
{
    // CodeEmbed-Start: send-to-ready
    [NetworkMessage]
    // Make sure to regieter message on client
    public struct MyMessage
    {
        public string message;
    }

    public class LobbyController : MonoBehaviour
    {
        public LobbyReady LobbyReady;

        public void SendToReady()
        {
            var myMessage = new MyMessage { message = "Hello, world!" };
            // Send message to ready players
            LobbyReady.SendToReady(myMessage);
        }
    }
    // CodeEmbed-End: send-to-ready

    public class LobbyController2 : MonoBehaviour
    {
        public LobbyReady LobbyReady;

        // CodeEmbed-Start: send-to-not-ready
        public void SendToNotReady()
        {
            var myMessage = new MyMessage { message = "Hello, world!" };
            // Send message to ready players
            LobbyReady.SendToReady(myMessage, sendToReady: false);
        }
        // CodeEmbed-End: send-to-not-ready

        // CodeEmbed-Start: set-all-not-ready
        public void ClearReady()
        {
            LobbyReady.SetAllClientsNotReady();
        }
        // CodeEmbed-End: set-all-not-ready

        public void SetReady()
        {
            var readyCheck = LobbyReady.Players.First().Value;
            // CodeEmbed-Start: set-ready
            readyCheck.SetReady(true);
            // CodeEmbed-End: set-ready
        }
    }

    // CodeEmbed-Start: ready-ui
    public class ReadyUI : MonoBehaviour
    {
        public ReadyCheck ReadyCheck;
        public Image Image;

        public void Start()
        {
            ReadyCheck.OnReadyChanged += OnReadyChanged;
            // invoke right away to set the current value
            OnReadyChanged(ReadyCheck.IsReady);
        }

        private void OnReadyChanged(bool ready)
        {
            Image.color = ready ? Color.green : Color.red;
        }
    }
    // CodeEmbed-End: ready-ui
}
