using UnityEngine;
using UnityEngine.UI;

namespace Mirage.Examples.Basic
{
    public class BasicPlayer : NetworkBehaviour
    {
        [Header("BasicPlayer Components")]
        public RectTransform rectTransform;
        public Image image;

        [Header("Child Text Objects")]
        public Text playerNameText;
        public Text playerDataText;

        [SerializeField] private Vector2 offset = new Vector2(100, -170);
        [SerializeField] private Vector2 padding = new Vector2(10, 10);

        // These are set in OnStartServer and used in OnStartClient

        [SyncVar(initialOnly = true)] // playerNo is set on spawn so we can use initialOnly so it is only synced once
        public int playerNo;

        [SyncVar]
        private Color playerColor;

        // This is updated by UpdateData which is called from OnStartServer via InvokeRepeating
        [SyncVar(hook = nameof(OnPlayerDataChanged))]
        public int playerData;

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStartClient.AddListener(OnStartClient);
            Identity.OnStartLocalPlayer.AddListener(OnStartLocalPlayer);
        }

        // This is called by the hook of playerData SyncVar above
        private void OnPlayerDataChanged(int oldPlayerData, int newPlayerData)
        {
            // Show the data in the UI
            playerDataText.text = string.Format("Data: {0:000}", newPlayerData);
        }

        // This fires on server when this player object is network-ready
        public void OnStartServer()
        {
            // Set SyncVar values in OnStartServer so they will be sent with Spawn message
            playerColor = Random.ColorHSV(0f, 1f, 0.9f, 0.9f, 1f, 1f);

            // Start generating updates
            InvokeRepeating(nameof(UpdateData), 1, 1);
        }

        // This only runs on the server, called from OnStartServer via InvokeRepeating
        [Server(error = false)]
        private void UpdateData()
        {
            playerData = Random.Range(100, 1000);
        }

        // This fires on all clients when this player object is network-ready
        public void OnStartClient()
        {
            // Get spawner so we can set the parent under the canvas
            var spawner = Client.GetComponent<CanvasCharacterSpawner>();
            transform.SetParent(spawner.Parent);

            var size = rectTransform.sizeDelta + padding;

            // Calculate position in the layout panel
            var x = playerNo % 4 * size.x;
            var y = playerNo / 4 * size.y;
            rectTransform.anchoredPosition = offset + new Vector2(x, -y);

            // Apply SyncVar values
            playerNameText.color = playerColor;
            playerNameText.text = string.Format("BasicPlayer {0:00}", playerNo);
        }

        // This only fires on the local client when this player object is network-ready
        public void OnStartLocalPlayer()
        {
            // apply a shaded background to our player
            image.color = new Color(1f, 1f, 1f, 0.1f);
        }
    }
}
