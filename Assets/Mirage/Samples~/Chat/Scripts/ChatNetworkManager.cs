using UnityEngine;

namespace Mirage.Examples.Chat
{
    [AddComponentMenu("")]
    public class ChatNetworkManager : NetworkManager
    {
        public string PlayerName { get; set; }

        [RequiredField]
        public ChatWindow chatWindow;

        [RequiredField]
        public Player playerPrefab;

        private void Awake()
        {
            Server.Started.AddListener(OnServerStarted);
            Client.Authenticated.AddListener(OnClientAuthenticated);
            Client.Disconnected.AddListener(OnClientDisconnected);
        }

        [NetworkMessage]
        public struct CreateCharacterMessage
        {
            public string name;
        }

        private void OnServerStarted()
        {
            Server.MessageHandler.RegisterHandler<CreateCharacterMessage>(OnCreatePlayer);
        }

        private void OnClientAuthenticated(INetworkPlayer player)
        {
            // Open the chat window on the client once authenticated
            chatWindow.gameObject.SetActive(true);

            // Request the server to create a player character with our chosen name
            player.Send(new CreateCharacterMessage { name = PlayerName });
        }

        private void OnClientDisconnected(ClientStoppedReason reason)
        {
            // Hide the chat window when client disconnects
            chatWindow.gameObject.SetActive(false);
        }

        private void OnCreatePlayer(INetworkPlayer player, CreateCharacterMessage createCharacterMessage)
        {
            // Create a gameobject using the name supplied by client
            var character = Instantiate(playerPrefab);
            character.playerName = createCharacterMessage.name;

            // Spawn the character and assign authority to the player connection
            ServerObjectManager.AddCharacter(player, character.gameObject);
        }
    }
}
