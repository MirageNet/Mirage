using System.ComponentModel;
using Mirage;
using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: create-mmo-character-message
    public struct CreateMMOCharacterMessage
    {
        public Race race;
        public string name;
        public Color hairColor;
        public Color eyeColor;
    }

    public enum Race
    {
        Human,
        Elvish,
        Dwarvish,
    }
    // CodeEmbed-End: create-mmo-character-message

    // CodeEmbed-Start: custom-character-spawner-class
    public class CustomCharacterSpawner : MonoBehaviour
    {
        [Header("References")]
        public NetworkClient Client;
        public NetworkServer Server;
        public ClientObjectManager ClientObjectManager;
        public ServerObjectManager ServerObjectManager;

        [Header("Prefabs")]
        // Different prefabs based on the Race the player picks
        public CustomCharacter HumanPrefab;
        public CustomCharacter ElvishPrefab;
        public CustomCharacter DwarvishPrefab;
    // CodeEmbed-End: custom-character-spawner-class

        // CodeEmbed-Start: custom-character-spawner-start
        public void Start()
        {
            Client.Started.AddListener(OnClientStarted);
            Client.Authenticated.AddListener(OnClientAuthenticated);
            Server.Started.AddListener(OnServerStarted);
        }
        // CodeEmbed-End: custom-character-spawner-start

        // CodeEmbed-Start: custom-character-spawner-client-started
        private void OnClientStarted()
        {
            // Make sure all prefabs are Register so mirage can spawn the character for this client and for other players
            ClientObjectManager.RegisterPrefab(HumanPrefab.Identity);
            ClientObjectManager.RegisterPrefab(ElvishPrefab.Identity);
            ClientObjectManager.RegisterPrefab(DwarvishPrefab.Identity);
        }
        // CodeEmbed-End: custom-character-spawner-client-started

        // CodeEmbed-Start: custom-character-spawner-client-authenticated
        // You can send the message here if you already know
        // everything about the character at the time of player
        // or at a later time when the user submits his preferences
        private void OnClientAuthenticated(INetworkPlayer player)
        {
            var mmoCharacter = new CreateMMOCharacterMessage
            {
                // populate the message with your data
                name = "player user name",
                race = Race.Human,
                eyeColor = Color.red,
                hairColor = Color.black,
            };
            player.Send(mmoCharacter);
        }
        // CodeEmbed-End: custom-character-spawner-client-authenticated

        // CodeEmbed-Start: custom-character-spawner-server-started
        private void OnServerStarted()
        {
            // Wait for client to send us an AddPlayerMessage
            Server.MessageHandler.RegisterHandler<CreateMMOCharacterMessage>(OnCreateCharacter);
        }

        private void OnCreateCharacter(INetworkPlayer player, CreateMMOCharacterMessage msg)
        {
            CustomCharacter prefab = GetPrefab(msg);

            // Create your character object
            // Use the data in msg to configure it
            CustomCharacter character = Instantiate(prefab);

            // Set syncVars before telling Mirage to spawn character
            // This will cause them to be sent to client in the spawn message
            character.PlayerName = msg.name;
            character.hairColor = msg.hairColor;
            character.eyeColor = msg.eyeColor;

            // Spawn it as the character object
            ServerObjectManager.AddCharacter(player, character.Identity);
        }

        private CustomCharacter GetPrefab(CreateMMOCharacterMessage msg)
        {
            // Get prefab based on race
            CustomCharacter prefab;
            switch (msg.race)
            {
                case Race.Human: prefab = HumanPrefab; break;
                case Race.Elvish: prefab = ElvishPrefab; break;
                case Race.Dwarvish: prefab = DwarvishPrefab; break;
                // Default case to check that client sent valid race.
                // The only reason it should be invalid is if the client's code was modified by an attacker
                // Throw will cause the client to be kicked
                default: throw new InvalidEnumArgumentException("Invalid race given");
            }

            return prefab;
        }
        // CodeEmbed-End: custom-character-spawner-server-started
    }

    public class CustomCharacter : NetworkBehaviour
    {
        public string PlayerName { get; set; }
        public Color hairColor { get; set; }
        public Color eyeColor { get; set; }
    }

    // CodeEmbed-Start: custom-character-spawner-respawn
    public class CustomCharacterSpawnerRespawn : MonoBehaviour
    {
        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;

        public void Respawn(NetworkPlayer player, GameObject newPrefab)
        {
            // Cache a reference to the current character object
            GameObject oldPlayer = player.Identity.gameObject;

            var newCharacter = Instantiate(newPrefab);

            // Instantiate the new character object and broadcast to clients
            // NOTE: here we can use `keepAuthority: true` because we are calling Destroy on the old prefab immediately after.
            ServerObjectManager.ReplaceCharacter(player, newCharacter, keepAuthority: true);

            // Remove the previous character object that's now been replaced
            ServerObjectManager.Destroy(oldPlayer);
        }
    }
    // CodeEmbed-End: custom-character-spawner-respawn

    public class PlayerDeathHandler : MonoBehaviour
    {
        public ServerObjectManager ServerObjectManager;

        // CodeEmbed-Start: custom-character-spawner-death
        public void OnPlayerDeath(INetworkPlayer player)
        {
            ServerObjectManager.DestroyCharacter(player);
        }
        // CodeEmbed-End: custom-character-spawner-death
    }
}
